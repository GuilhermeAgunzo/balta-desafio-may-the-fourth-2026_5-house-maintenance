using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Core.Entities;

public sealed class MaintenancePlanStep
{
    private MaintenancePlanStep()
    {
    }

    internal MaintenancePlanStep(Guid maintenanceToolGroupId, int order, MaintenancePlanStepDraft draft)
    {
        Id = Guid.NewGuid();
        MaintenanceToolGroupId = maintenanceToolGroupId;
        Order = order;
        TaskName = draft.TaskName;
        WhyNow = draft.WhyNow;
        EstimatedMinutes = draft.EstimatedMinutes;
    }

    public Guid Id { get; private set; }

    public Guid MaintenanceToolGroupId { get; private set; }

    public int Order { get; private set; }

    public string TaskName { get; private set; } = string.Empty;

    public string WhyNow { get; private set; } = string.Empty;

    public int EstimatedMinutes { get; private set; }
}
