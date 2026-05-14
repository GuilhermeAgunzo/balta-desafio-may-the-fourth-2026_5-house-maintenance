using HouseMaintenance.Core.Entities;

namespace HouseMaintenance.Core.Contracts;

public interface IMaintenanceMissionRepository
{
    Task AddAsync(MaintenanceMission mission, CancellationToken cancellationToken);

    Task<MaintenanceMission?> GetByIdAsync(Guid missionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<MaintenanceMission>> ListRecentAsync(int take, CancellationToken cancellationToken);
}
