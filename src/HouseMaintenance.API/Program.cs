using HouseMaintenance.AI.Agents;
using HouseMaintenance.AI.Configuration;
using HouseMaintenance.API.Exceptions;
using HouseMaintenance.Application.Services;
using HouseMaintenance.Core.Contracts;
using HouseMaintenance.Core.Models;
using HouseMaintenance.Infra;
using HouseMaintenance.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MaintenanceExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.Configure<OpenAiAgentOptions>(builder.Configuration.GetSection(OpenAiAgentOptions.SectionName));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<MaintenanceMissionApplicationService>();
builder.Services.AddScoped<IMaintenancePlanningAgent, HouseMaintenancePlanningAgent>();

var app = builder.Build();

await EnsureDatabaseAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapDefaultEndpoints();

var maintenancePlans = app.MapGroup("/api/maintenance-plans")
    .WithTags("Maintenance Plans");

maintenancePlans.MapPost(
        "/",
        async (GenerateMaintenancePlanRequest request, MaintenanceMissionApplicationService service, CancellationToken cancellationToken) =>
            TypedResults.Ok(await service.GeneratePlanAsync(request, cancellationToken)))
    .WithName("GenerateMaintenancePlan");

maintenancePlans.MapGet(
        "/history",
        async (int? take, MaintenanceMissionApplicationService service, CancellationToken cancellationToken) =>
            TypedResults.Ok(await service.ListRecentAsync(take ?? 6, cancellationToken)))
    .WithName("GetMaintenancePlanHistory");

maintenancePlans.MapGet(
        "/{missionId:guid}",
        async (Guid missionId, MaintenanceMissionApplicationService service, CancellationToken cancellationToken) =>
            TypedResults.Ok(await service.GetPlanAsync(missionId, cancellationToken)))
    .WithName("GetMaintenancePlanById");

app.Run();

static async Task EnsureDatabaseAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HouseMaintenanceDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

public partial class Program;
