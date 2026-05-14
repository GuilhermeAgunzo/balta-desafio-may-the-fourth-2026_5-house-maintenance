using System.Text.Json.Serialization;

namespace HouseMaintenance.Core.Models;

public sealed class MaintenancePlanDraft
{
    [JsonPropertyName("missionTitle")]
    public string MissionTitle { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("strategicRationale")]
    public string StrategicRationale { get; init; } = string.Empty;

    [JsonPropertyName("toolGroups")]
    public IReadOnlyList<MaintenanceToolGroupDraft> ToolGroups { get; init; } = [];

    [JsonPropertyName("safetyNotes")]
    public IReadOnlyList<string> SafetyNotes { get; init; } = [];
}

public sealed class MaintenanceToolGroupDraft
{
    [JsonPropertyName("toolkit")]
    public string Toolkit { get; init; } = string.Empty;

    [JsonPropertyName("toolkitReason")]
    public string ToolkitReason { get; init; } = string.Empty;

    [JsonPropertyName("steps")]
    public IReadOnlyList<MaintenancePlanStepDraft> Steps { get; init; } = [];
}

public sealed class MaintenancePlanStepDraft
{
    [JsonPropertyName("taskName")]
    public string TaskName { get; init; } = string.Empty;

    [JsonPropertyName("whyNow")]
    public string WhyNow { get; init; } = string.Empty;

    [JsonPropertyName("estimatedMinutes")]
    public int EstimatedMinutes { get; init; }
}
