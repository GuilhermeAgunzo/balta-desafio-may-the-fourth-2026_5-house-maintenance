using Bunit;
using HouseMaintenance.Core.Models;
using HouseMaintenance.Web.Client.Pages;
using HouseMaintenance.Web.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HouseMaintenance.Web.Tests;

public sealed class HomePageTests
{
    [Fact]
    public void Home_RendersHistoryFromClientService()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IMaintenancePlannerClient>(
            new FakeMaintenancePlannerClient
            {
                RecentMissions =
                [
                    new RecentMissionResponse
                    {
                        MissionId = Guid.NewGuid(),
                        MissionTitle = "Operação Quadro Reto",
                        Summary = "Agrupa furadeira e nível em uma única visita.",
                        CreatedAtUtc = DateTimeOffset.UtcNow,
                        TaskCount = 2,
                        ToolkitSwitches = 0
                    }
                ]
            });

        var component = context.Render<Home>();

        component.WaitForAssertion(() => Assert.Contains("Operação Quadro Reto", component.Markup));
    }

    [Fact]
    public void Home_ShowsGeneratedPlanAfterPrimaryAction()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IMaintenancePlannerClient>(
            new FakeMaintenancePlannerClient
            {
                GeneratedPlan = new MaintenancePlanResponse
                {
                    MissionId = Guid.NewGuid(),
                    MissionTitle = "Operação Hangar Doméstico",
                    Summary = "Agrupa reparos por kit de forma contínua.",
                    StrategicRationale = "Abrir menos kits reduz deslocamento e sujeira acumulada.",
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    TaskCount = 3,
                    EstimatedMinutes = 42,
                    ToolGroups =
                    [
                        new MaintenanceToolkitGroupResponse
                        {
                            Order = 1,
                            Toolkit = "Kit elétrico",
                            ToolkitReason = "Resolve primeiro o bloco com energia e iluminação.",
                            EstimatedMinutes = 22,
                            Steps =
                            [
                                new MaintenancePlanStepResponse
                                {
                                    Order = 1,
                                    TaskName = "Trocar lâmpada do corredor",
                                    WhyNow = "A escada e os itens elétricos já estão posicionados.",
                                    EstimatedMinutes = 10
                                }
                            ]
                        }
                    ],
                    SafetyNotes =
                    [
                        "Desligue o circuito antes de mexer na luminária."
                    ]
                }
            });

        var component = context.Render<Home>();
        component.Find(".mission-button").Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Operação Hangar Doméstico", component.Markup);
            Assert.Contains("Kit elétrico", component.Markup);
        });
    }

    private sealed class FakeMaintenancePlannerClient : IMaintenancePlannerClient
    {
        public IReadOnlyList<RecentMissionResponse> RecentMissions { get; init; } = [];

        public MaintenancePlanResponse GeneratedPlan { get; init; } = new()
        {
            MissionId = Guid.NewGuid(),
            MissionTitle = "Missão padrão",
            Summary = "Resumo padrão",
            StrategicRationale = "Racional padrão",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            TaskCount = 1,
            EstimatedMinutes = 10,
            ToolGroups = [],
            SafetyNotes = []
        };

        public Task<MaintenancePlanResponse> GeneratePlanAsync(IReadOnlyList<string> tasks, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GeneratedPlan);
        }

        public Task<MaintenancePlanResponse> GetMissionAsync(Guid missionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GeneratedPlan);
        }

        public Task<IReadOnlyList<RecentMissionResponse>> GetRecentMissionsAsync(int take = 6, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RecentMissions);
        }
    }
}
