using System.Net.Http.Json;
using HouseMaintenance.Core.Models;
using HouseMaintenance.Web.Components;
using HouseMaintenance.Web.Client.Services;
using HouseMaintenance.Web.Services;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var apiTimeoutSeconds = NormalizeProxyTimeout(builder.Configuration.GetValue<int?>("Api:TimeoutSeconds") ?? 180);

builder.Services.AddProblemDetails();
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddScoped<IMaintenancePlannerClient, ServerMaintenancePlannerClient>();
#pragma warning disable EXTEXP0001
builder.Services.AddHttpClient(
    "housemaintenance-api",
    (serviceProvider, client) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var configuredBaseUrl = configuration["Api:BaseUrl"];
        client.BaseAddress = new Uri(
            string.IsNullOrWhiteSpace(configuredBaseUrl)
                ? "https+http://housemaintenance-api"
                : configuredBaseUrl);
    })
    .RemoveAllResilienceHandlers()
    .AddStandardResilienceHandler(
        options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(apiTimeoutSeconds);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(Math.Max(30, apiTimeoutSeconds - 10));
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(apiTimeoutSeconds * 2);
        });
#pragma warning restore EXTEXP0001

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(HouseMaintenance.Web.Client._Imports).Assembly);

var frontendApi = app.MapGroup("/frontend-api/maintenance-plans");

frontendApi.MapPost(
    "/",
    async (GenerateMaintenancePlanRequest request, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
        await ForwardAsync(
            httpClientFactory.CreateClient("housemaintenance-api")
                .PostAsJsonAsync("/api/maintenance-plans", request, cancellationToken),
            cancellationToken));

frontendApi.MapGet(
    "/history",
    async (int? take, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
        await ForwardAsync(
            httpClientFactory.CreateClient("housemaintenance-api")
                .GetAsync($"/api/maintenance-plans/history?take={take ?? 6}", cancellationToken),
            cancellationToken));

frontendApi.MapGet(
    "/{missionId:guid}",
    async (Guid missionId, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
        await ForwardAsync(
            httpClientFactory.CreateClient("housemaintenance-api")
                .GetAsync($"/api/maintenance-plans/{missionId}", cancellationToken),
            cancellationToken));

app.Run();

static async Task<IResult> ForwardAsync(Task<HttpResponseMessage> responseTask, CancellationToken cancellationToken)
{
    using var response = await responseTask;
    var payload = await response.Content.ReadAsStringAsync(cancellationToken);
    var mediaType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
    return Results.Content(payload, mediaType, statusCode: (int)response.StatusCode);
}

static int NormalizeProxyTimeout(int timeoutSeconds) => Math.Clamp(timeoutSeconds, 60, 600);

public partial class Program;
