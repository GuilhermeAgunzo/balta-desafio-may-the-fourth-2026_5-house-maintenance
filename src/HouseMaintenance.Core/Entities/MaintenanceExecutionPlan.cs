using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Core.Entities;

public sealed class MaintenanceExecutionPlan
{
    private MaintenanceExecutionPlan()
    {
    }

    internal MaintenanceExecutionPlan(Guid maintenanceMissionId, MaintenancePlanDraft draft)
    {
        Id = Guid.NewGuid();
        MaintenanceMissionId = maintenanceMissionId;
        MissionTitle = draft.MissionTitle;
        Summary = draft.Summary;
        StrategicRationale = draft.StrategicRationale;
        ToolGroups = draft.ToolGroups
            .Select((group, index) => new MaintenanceToolGroup(Id, index + 1, group))
            .ToList();
        SafetyNotes = draft.SafetyNotes
            .Select((note, index) => new MaintenanceSafetyNote(Id, index + 1, note))
            .ToList();
    }

    public Guid Id { get; private set; }

    public Guid MaintenanceMissionId { get; private set; }

    public string MissionTitle { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string StrategicRationale { get; private set; } = string.Empty;

    public List<MaintenanceToolGroup> ToolGroups { get; private set; } = [];

    public List<MaintenanceSafetyNote> SafetyNotes { get; private set; } = [];
}
