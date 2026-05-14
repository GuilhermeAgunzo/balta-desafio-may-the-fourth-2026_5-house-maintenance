using System.Net;
using System.Net.Http.Json;
using HouseMaintenance.Core.Contracts;
using HouseMaintenance.Core.Models;
using HouseMaintenance.Infra.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HouseMaintenance.API.Tests;

public sealed class MaintenancePlansApiTests
{
    [Fact]
    public async Task Post_ReturnsGeneratedMissionPlan()
    {
        using var factory = new HouseMaintenanceApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/maintenance-plans",
            new GenerateMaintenancePlanRequest
            {
                Tasks =
                [
                    "Trocar lâmpada do corredor",
                    "Limpar ralo do banheiro"
                ]
            });

        response.EnsureSuccessStatusCode();

        var plan = await response.Content.ReadFromJsonAsync<MaintenancePlanResponse>();

        Assert.NotNull(plan);
        Assert.Equal("Operação Hangar Doméstico", plan.MissionTitle);
        Assert.Equal(2, plan.TaskCount);
        Assert.Single(plan.ToolGroups);
    }

    [Fact]
    public async Task GetHistory_ReturnsPersistedMission()
    {
        using var factory = new HouseMaintenanceApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync(
            "/api/maintenance-plans",
            new GenerateMaintenancePlanRequest
            {
                Tasks =
                [
                    "Fixar quadro da sala",
                    "Limpar ralo do banheiro"
                ]
            });

        var history = await client.GetFromJsonAsync<RecentMissionResponse[]>("/api/maintenance-plans/history");

        Assert.NotNull(history);
        Assert.Single(history);
        Assert.Equal("Operação Hangar Doméstico", history[0].MissionTitle);
        Assert.Equal(2, history[0].TaskCount);
    }

    [Fact]
    public async Task Post_WithEmptyTasks_ReturnsProblemDetails()
    {
        using var factory = new HouseMaintenanceApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/maintenance-plans",
            new GenerateMaintenancePlanRequest { Tasks = [] });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains("Informe pelo menos um reparo", problem.Detail);
    }

    private sealed class HouseMaintenanceApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"housemaintenance-tests-{Guid.NewGuid():N}.db");
        private readonly IMaintenancePlanningAgent _planningAgent = new StubPlanningAgent();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration(
                (_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:housemaintenance"] = $"Data Source={_databasePath}",
                            ["OpenAI:Model"] = "gpt-4o-mini"
                        });
                });

            builder.ConfigureServices(
                services =>
                {
                    services.RemoveAll<IMaintenancePlanningAgent>();
                    services.RemoveAll<DbContextOptions<HouseMaintenanceDbContext>>();
                    services.RemoveAll<HouseMaintenanceDbContext>();
                    services.AddDbContext<HouseMaintenanceDbContext>(
                        options => options.UseSqlite($"Data Source={_databasePath}"));
                    services.AddSingleton<IMaintenancePlanningAgent>(_planningAgent);
                });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            try
            {
                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                }
            }
            catch (IOException)
            {
            }
        }
    }

    private sealed class StubPlanningAgent : IMaintenancePlanningAgent
    {
        public Task<MaintenancePlanDraft> GeneratePlanAsync(MaintenancePlanningContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new MaintenancePlanDraft
                {
                    MissionTitle = "Operação Hangar Doméstico",
                    Summary = "Agrupa reparos para reduzir trocas de kit durante a execução.",
                    StrategicRationale = "Mantém a missão enxuta ao consolidar tarefas parecidas no mesmo bloco.",
                    ToolGroups =
                    [
                        new MaintenanceToolGroupDraft
                        {
                            Toolkit = "Kit geral de manutenção",
                            ToolkitReason = "Executa todos os passos em uma passagem enxuta.",
                            Steps = context.Tasks
                                .Select(
                                    task => new MaintenancePlanStepDraft
                                    {
                                        TaskName = task.NormalizedDescription,
                                        WhyNow = "A mesma bancada e as mesmas ferramentas já estão abertas.",
                                        EstimatedMinutes = 15
                                    })
                                .ToArray()
                        }
                    ],
                    SafetyNotes =
                    [
                        "Use luvas e desligue a energia sempre que a tarefa tocar em iluminação."
                    ]
                });
        }
    }
}
