using HouseMaintenance.Core.Entities;
using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Application.Services;

internal static class MaintenanceMissionMappings
{
    public static MaintenancePlanResponse ToResponse(this MaintenanceMission mission)
    {
        var orderedGroups = mission.Plan.ToolGroups
            .OrderBy(group => group.Order)
            .ToArray();

        return new MaintenancePlanResponse
        {
            MissionId = mission.Id,
            MissionTitle = mission.Plan.MissionTitle,
            Summary = mission.Plan.Summary,
            StrategicRationale = mission.Plan.StrategicRationale,
            CreatedAtUtc = mission.CreatedAtUtc,
            TaskCount = mission.Tasks.Count,
            EstimatedMinutes = orderedGroups.SelectMany(group => group.Steps).Sum(step => step.EstimatedMinutes),
            ToolGroups = orderedGroups
                .Select(
                    group => new MaintenanceToolkitGroupResponse
                    {
                        Order = group.Order,
                        Toolkit = group.Toolkit,
                        ToolkitReason = group.ToolkitReason,
                        EstimatedMinutes = group.Steps.Sum(step => step.EstimatedMinutes),
                        Steps = group.Steps
                            .OrderBy(step => step.Order)
                            .Select(
                                step => new MaintenancePlanStepResponse
                                {
                                    Order = step.Order,
                                    TaskName = step.TaskName,
                                    WhyNow = step.WhyNow,
                                    EstimatedMinutes = step.EstimatedMinutes
                                })
                            .ToArray()
                    })
                .ToArray(),
            SafetyNotes = mission.Plan.SafetyNotes
                .OrderBy(note => note.Order)
                .Select(note => note.Content)
                .ToArray()
        };
    }

    public static RecentMissionResponse ToRecentResponse(this MaintenanceMission mission)
    {
        return new RecentMissionResponse
        {
            MissionId = mission.Id,
            MissionTitle = mission.Plan.MissionTitle,
            Summary = mission.Plan.Summary,
            CreatedAtUtc = mission.CreatedAtUtc,
            TaskCount = mission.Tasks.Count,
            ToolkitSwitches = Math.Max(0, mission.Plan.ToolGroups.Count - 1)
        };
    }
}
