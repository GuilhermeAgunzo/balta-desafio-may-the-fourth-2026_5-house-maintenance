namespace HouseMaintenance.Core.Models;

public sealed class GenerateMaintenancePlanRequest
{
    public IReadOnlyList<string> Tasks { get; init; } = [];
}
