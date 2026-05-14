using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Web.Client.Services;

public interface IMaintenancePlannerClient
{
    Task<MaintenancePlanResponse> GeneratePlanAsync(IReadOnlyList<string> tasks, CancellationToken cancellationToken = default);

    Task<MaintenancePlanResponse> GetMissionAsync(Guid missionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentMissionResponse>> GetRecentMissionsAsync(int take = 6, CancellationToken cancellationToken = default);
}
