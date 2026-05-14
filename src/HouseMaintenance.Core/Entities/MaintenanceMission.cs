using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Core.Entities;

public sealed class MaintenanceMission
{
    private MaintenanceMission()
    {
    }

    public MaintenanceMission(IReadOnlyList<PreparedMaintenanceTask> tasks, MaintenancePlanDraft draft)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        ArgumentNullException.ThrowIfNull(draft);

        if (tasks.Count == 0)
        {
            throw new ArgumentException("A maintenance mission must include at least one task.", nameof(tasks));
        }

        Id = Guid.NewGuid();
        CreatedAtUtc = DateTimeOffset.UtcNow;
        Tasks = tasks
            .Select(task => new MaintenanceMissionTask(Id, task))
            .ToList();
        Plan = new MaintenanceExecutionPlan(Id, draft);
    }

    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public List<MaintenanceMissionTask> Tasks { get; private set; } = [];

    public MaintenanceExecutionPlan Plan { get; private set; } = null!;
}
