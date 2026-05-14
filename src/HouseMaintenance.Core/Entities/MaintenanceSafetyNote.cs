namespace HouseMaintenance.Core.Entities;

public sealed class MaintenanceSafetyNote
{
    private MaintenanceSafetyNote()
    {
    }

    internal MaintenanceSafetyNote(Guid maintenanceExecutionPlanId, int order, string content)
    {
        Id = Guid.NewGuid();
        MaintenanceExecutionPlanId = maintenanceExecutionPlanId;
        Order = order;
        Content = content;
    }

    public Guid Id { get; private set; }

    public Guid MaintenanceExecutionPlanId { get; private set; }

    public int Order { get; private set; }

    public string Content { get; private set; } = string.Empty;
}
