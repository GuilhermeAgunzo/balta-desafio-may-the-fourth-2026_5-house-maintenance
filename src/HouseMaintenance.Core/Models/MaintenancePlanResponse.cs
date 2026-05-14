namespace HouseMaintenance.Core.Models;

public sealed class MaintenancePlanResponse
{
    public required Guid MissionId { get; init; }

    public required string MissionTitle { get; init; }

    public required string Summary { get; init; }

    public required string StrategicRationale { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }

    public required int TaskCount { get; init; }

    public required int EstimatedMinutes { get; init; }

    public IReadOnlyList<MaintenanceToolkitGroupResponse> ToolGroups { get; init; } = [];

    public IReadOnlyList<string> SafetyNotes { get; init; } = [];
}

public sealed class MaintenanceToolkitGroupResponse
{
    public required string Toolkit { get; init; }

    public required string ToolkitReason { get; init; }

    public required int Order { get; init; }

    public required int EstimatedMinutes { get; init; }

    public IReadOnlyList<MaintenancePlanStepResponse> Steps { get; init; } = [];
}

public sealed class MaintenancePlanStepResponse
{
    public required int Order { get; init; }

    public required string TaskName { get; init; }

    public required string WhyNow { get; init; }

    public required int EstimatedMinutes { get; init; }
}
