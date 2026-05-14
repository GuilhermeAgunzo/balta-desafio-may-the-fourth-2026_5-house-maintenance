using System.Net.Http.Json;
using System.Text.Json;
using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Web.Client.Services;

public sealed class MaintenancePlannerClient(HttpClient httpClient) : IMaintenancePlannerClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<MaintenancePlanResponse> GeneratePlanAsync(
        IReadOnlyList<string> tasks,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "frontend-api/maintenance-plans",
            new GenerateMaintenancePlanRequest { Tasks = tasks.ToArray() },
            cancellationToken);

        return await ReadAsync<MaintenancePlanResponse>(response, cancellationToken);
    }

    public async Task<MaintenancePlanResponse> GetMissionAsync(Guid missionId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"frontend-api/maintenance-plans/{missionId}", cancellationToken);
        return await ReadAsync<MaintenancePlanResponse>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<RecentMissionResponse>> GetRecentMissionsAsync(
        int take = 6,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"frontend-api/maintenance-plans/history?take={take}", cancellationToken);
        return await ReadAsync<RecentMissionResponse[]>(response, cancellationToken);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new MaintenancePlannerClientException(await ReadProblemMessageAsync(response, cancellationToken));
        }

        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);

        return payload
            ?? throw new MaintenancePlannerClientException("O backend retornou uma resposta vazia.");
    }

    private static async Task<string> ReadProblemMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return $"A solicitação falhou com status {(int)response.StatusCode}.";
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(raw);
            var root = jsonDocument.RootElement;

            if (root.TryGetProperty("detail", out var detailElement) &&
                detailElement.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(detailElement.GetString()))
            {
                return detailElement.GetString()!;
            }

            if (root.TryGetProperty("title", out var titleElement) &&
                titleElement.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(titleElement.GetString()))
            {
                return titleElement.GetString()!;
            }
        }
        catch (JsonException)
        {
        }

        return raw;
    }
}
