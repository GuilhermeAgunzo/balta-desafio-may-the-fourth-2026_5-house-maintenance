using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Core.Contracts;

public interface IMaintenancePlanningAgent
{
    Task<MaintenancePlanDraft> GeneratePlanAsync(MaintenancePlanningContext context, CancellationToken cancellationToken);
}
