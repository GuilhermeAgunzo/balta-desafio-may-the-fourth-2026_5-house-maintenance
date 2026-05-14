using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Core.Entities;

public sealed class MaintenanceMissionTask
{
    private MaintenanceMissionTask()
    {
    }

    internal MaintenanceMissionTask(Guid maintenanceMissionId, PreparedMaintenanceTask task)
    {
        Id = Guid.NewGuid();
        MaintenanceMissionId = maintenanceMissionId;
        OriginalDescription = task.OriginalDescription;
        NormalizedDescription = task.NormalizedDescription;
        ToolkitHint = task.ToolkitHint;
        ToolkitRationale = task.ToolkitRationale;
        OriginalOrder = task.OriginalOrder;
    }

    public Guid Id { get; private set; }

    public Guid MaintenanceMissionId { get; private set; }

    public string OriginalDescription { get; private set; } = string.Empty;

    public string NormalizedDescription { get; private set; } = string.Empty;

    public string ToolkitHint { get; private set; } = string.Empty;

    public string ToolkitRationale { get; private set; } = string.Empty;

    public int OriginalOrder { get; private set; }
}
