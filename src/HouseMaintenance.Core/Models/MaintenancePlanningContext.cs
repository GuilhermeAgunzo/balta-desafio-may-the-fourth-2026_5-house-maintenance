namespace HouseMaintenance.Core.Models;

public sealed class MaintenancePlanningContext
{
    public required IReadOnlyList<PreparedMaintenanceTask> Tasks { get; init; }
}

public sealed class PreparedMaintenanceTask
{
    public required string OriginalDescription { get; init; }

    public required string NormalizedDescription { get; init; }

    public required string ToolkitHint { get; init; }

    public required string ToolkitRationale { get; init; }

    public required int OriginalOrder { get; init; }
}
