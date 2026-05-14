namespace HouseMaintenance.Core.Models;

public sealed class RecentMissionResponse
{
    public required Guid MissionId { get; init; }

    public required string MissionTitle { get; init; }

    public required string Summary { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }

    public required int TaskCount { get; init; }

    public required int ToolkitSwitches { get; init; }
}
