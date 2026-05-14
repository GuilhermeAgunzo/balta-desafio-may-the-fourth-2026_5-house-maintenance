using HouseMaintenance.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HouseMaintenance.Infra.Persistence;

public sealed class HouseMaintenanceDbContext(DbContextOptions<HouseMaintenanceDbContext> options) : DbContext(options)
{
    public DbSet<MaintenanceMission> Missions => Set<MaintenanceMission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateTimeOffsetConverter = new DateTimeOffsetToBinaryConverter();

        modelBuilder.Entity<MaintenanceMission>(
            entity =>
            {
                entity.ToTable("MaintenanceMissions");
                entity.HasKey(mission => mission.Id);
                entity.Property(mission => mission.CreatedAtUtc)
                    .HasConversion(dateTimeOffsetConverter)
                    .IsRequired();

                entity.HasMany(mission => mission.Tasks)
                    .WithOne()
                    .HasForeignKey(task => task.MaintenanceMissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mission => mission.Plan)
                    .WithOne()
                    .HasForeignKey<MaintenanceExecutionPlan>(plan => plan.MaintenanceMissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<MaintenanceMissionTask>(
            entity =>
            {
                entity.ToTable("MaintenanceMissionTasks");
                entity.HasKey(task => task.Id);
                entity.Property(task => task.OriginalDescription).HasMaxLength(120).IsRequired();
                entity.Property(task => task.NormalizedDescription).HasMaxLength(120).IsRequired();
                entity.Property(task => task.ToolkitHint).HasMaxLength(80).IsRequired();
                entity.Property(task => task.ToolkitRationale).HasMaxLength(240).IsRequired();
                entity.Property(task => task.OriginalOrder).IsRequired();
            });

        modelBuilder.Entity<MaintenanceExecutionPlan>(
            entity =>
            {
                entity.ToTable("MaintenanceExecutionPlans");
                entity.HasKey(plan => plan.Id);
                entity.Property(plan => plan.MissionTitle).HasMaxLength(140).IsRequired();
                entity.Property(plan => plan.Summary).HasMaxLength(800).IsRequired();
                entity.Property(plan => plan.StrategicRationale).HasMaxLength(800).IsRequired();

                entity.HasMany(plan => plan.ToolGroups)
                    .WithOne()
                    .HasForeignKey(group => group.MaintenanceExecutionPlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(plan => plan.SafetyNotes)
                    .WithOne()
                    .HasForeignKey(note => note.MaintenanceExecutionPlanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<MaintenanceToolGroup>(
            entity =>
            {
                entity.ToTable("MaintenanceToolGroups");
                entity.HasKey(group => group.Id);
                entity.Property(group => group.Order).IsRequired();
                entity.Property(group => group.Toolkit).HasMaxLength(80).IsRequired();
                entity.Property(group => group.ToolkitReason).HasMaxLength(400).IsRequired();

                entity.HasMany(group => group.Steps)
                    .WithOne()
                    .HasForeignKey(step => step.MaintenanceToolGroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<MaintenancePlanStep>(
            entity =>
            {
                entity.ToTable("MaintenancePlanSteps");
                entity.HasKey(step => step.Id);
                entity.Property(step => step.Order).IsRequired();
                entity.Property(step => step.TaskName).HasMaxLength(120).IsRequired();
                entity.Property(step => step.WhyNow).HasMaxLength(400).IsRequired();
                entity.Property(step => step.EstimatedMinutes).IsRequired();
            });

        modelBuilder.Entity<MaintenanceSafetyNote>(
            entity =>
            {
                entity.ToTable("MaintenanceSafetyNotes");
                entity.HasKey(note => note.Id);
                entity.Property(note => note.Order).IsRequired();
                entity.Property(note => note.Content).HasMaxLength(240).IsRequired();
            });
    }
}
