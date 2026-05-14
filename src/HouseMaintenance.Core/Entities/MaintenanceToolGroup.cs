using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Core.Entities;

public sealed class MaintenanceToolGroup
{
    private MaintenanceToolGroup()
    {
    }

    internal MaintenanceToolGroup(Guid maintenanceExecutionPlanId, int order, MaintenanceToolGroupDraft draft)
    {
        Id = Guid.NewGuid();
        MaintenanceExecutionPlanId = maintenanceExecutionPlanId;
        Order = order;
        Toolkit = draft.Toolkit;
        ToolkitReason = draft.ToolkitReason;
        Steps = draft.Steps
            .Select((step, index) => new MaintenancePlanStep(Id, index + 1, step))
            .ToList();
    }

    public Guid Id { get; private set; }

    public Guid MaintenanceExecutionPlanId { get; private set; }

    public int Order { get; private set; }

    public string Toolkit { get; private set; } = string.Empty;

    public string ToolkitReason { get; private set; } = string.Empty;

    public List<MaintenancePlanStep> Steps { get; private set; } = [];
}
