using HouseMaintenance.Application.Exceptions;
using HouseMaintenance.Application.Services;
using HouseMaintenance.Core.Contracts;
using HouseMaintenance.Core.Entities;
using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Application.Tests;

public sealed class MaintenanceMissionApplicationServiceTests
{
    [Fact]
    public async Task GeneratePlanAsync_NormalizesAndDeduplicatesTasksBeforePersisting()
    {
        var repository = new InMemoryMaintenanceMissionRepository();
        var agent = new CapturingPlanningAgent(context => CreateDraft(context.Tasks.Select(task => task.NormalizedDescription).ToArray()));
        var service = new MaintenanceMissionApplicationService(repository, agent);

        var response = await service.GeneratePlanAsync(
            new GenerateMaintenancePlanRequest
            {
                Tasks =
                [
                    " trocar lâmpada da cozinha  ",
                    "Trocar lâmpada da cozinha.",
                    "limpar ralo do banheiro "
                ]
            },
            CancellationToken.None);

        Assert.Equal(2, response.TaskCount);
        Assert.Single(repository.StoredMissions);
        Assert.Equal("Kit elétrico", agent.LastContext!.Tasks[0].ToolkitHint);
        Assert.Equal("Kit hidráulico", agent.LastContext.Tasks[1].ToolkitHint);
        Assert.Equal(
            ["Trocar lâmpada da cozinha", "Limpar ralo do banheiro"],
            agent.LastContext.Tasks.Select(task => task.NormalizedDescription).ToArray());
    }

    [Fact]
    public async Task GeneratePlanAsync_ThrowsWhenAgentOmitsOneTaskFromResponse()
    {
        var repository = new InMemoryMaintenanceMissionRepository();
        var agent = new CapturingPlanningAgent(
            context => CreateDraft(context.Tasks.Take(1).Select(task => task.NormalizedDescription).ToArray()));
        var service = new MaintenanceMissionApplicationService(repository, agent);

        await Assert.ThrowsAsync<MaintenancePlanContractException>(
            () => service.GeneratePlanAsync(
                new GenerateMaintenancePlanRequest
                {
                    Tasks =
                    [
                        "Trocar lâmpada do corredor",
                        "Fixar quadro da sala"
                    ]
                },
                CancellationToken.None));

        Assert.Empty(repository.StoredMissions);
    }

    [Fact]
    public async Task ListRecentAsync_RejectsTakeOutsideAllowedRange()
    {
        var repository = new InMemoryMaintenanceMissionRepository();
        var agent = new CapturingPlanningAgent(context => CreateDraft(context.Tasks.Select(task => task.NormalizedDescription).ToArray()));
        var service = new MaintenanceMissionApplicationService(repository, agent);

        var exception = await Assert.ThrowsAsync<RequestValidationException>(
            () => service.ListRecentAsync(30, CancellationToken.None));

        Assert.Equal("O histórico deve solicitar entre 1 e 20 missões.", exception.Message);
    }

    private static MaintenancePlanDraft CreateDraft(params string[] normalizedTasks)
    {
        return new MaintenancePlanDraft
        {
            MissionTitle = "Operação Hangar Doméstico",
            Summary = "Agrupa reparos para reduzir trocas de kit durante a execução.",
            StrategicRationale = "Consolidar ferramentas semelhantes primeiro evita deslocamentos e reduz bagunça acumulada.",
            ToolGroups =
            [
                new MaintenanceToolGroupDraft
                {
                    Toolkit = "Kit geral de manutenção",
                    ToolkitReason = "Todos os passos podem ser executados em sequência sem trocar de estação.",
                    Steps = normalizedTasks
                        .Select(
                            (task, index) => new MaintenancePlanStepDraft
                            {
                                TaskName = task,
                                WhyNow = "Mantém o mesmo conjunto de ferramentas aberto.",
                                EstimatedMinutes = 12 + index
                            })
                        .ToArray()
                }
            ],
            SafetyNotes =
            [
                "Separe luvas e desligue a energia antes de itens elétricos."
            ]
        };
    }

    private sealed class InMemoryMaintenanceMissionRepository : IMaintenanceMissionRepository
    {
        public List<MaintenanceMission> StoredMissions { get; } = [];

        public Task AddAsync(MaintenanceMission mission, CancellationToken cancellationToken)
        {
            StoredMissions.Add(mission);
            return Task.CompletedTask;
        }

        public Task<MaintenanceMission?> GetByIdAsync(Guid missionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredMissions.SingleOrDefault(mission => mission.Id == missionId));
        }

        public Task<IReadOnlyList<MaintenanceMission>> ListRecentAsync(int take, CancellationToken cancellationToken)
        {
            IReadOnlyList<MaintenanceMission> result = StoredMissions
                .OrderByDescending(mission => mission.CreatedAtUtc)
                .Take(take)
                .ToArray();

            return Task.FromResult(result);
        }
    }

    private sealed class CapturingPlanningAgent(Func<MaintenancePlanningContext, MaintenancePlanDraft> factory) : IMaintenancePlanningAgent
    {
        public MaintenancePlanningContext? LastContext { get; private set; }

        public Task<MaintenancePlanDraft> GeneratePlanAsync(MaintenancePlanningContext context, CancellationToken cancellationToken)
        {
            LastContext = context;
            return Task.FromResult(factory(context));
        }
    }
}
