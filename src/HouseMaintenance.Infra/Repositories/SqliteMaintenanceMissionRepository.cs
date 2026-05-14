using HouseMaintenance.Core.Contracts;
using HouseMaintenance.Core.Entities;
using HouseMaintenance.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseMaintenance.Infra.Repositories;

public sealed class SqliteMaintenanceMissionRepository(HouseMaintenanceDbContext dbContext) : IMaintenanceMissionRepository
{
    public async Task AddAsync(MaintenanceMission mission, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mission);

        dbContext.Missions.Add(mission);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MaintenanceMission?> GetByIdAsync(Guid missionId, CancellationToken cancellationToken)
    {
        return await BuildGraphQuery()
            .FirstOrDefaultAsync(mission => mission.Id == missionId, cancellationToken);
    }

    public async Task<IReadOnlyList<MaintenanceMission>> ListRecentAsync(int take, CancellationToken cancellationToken)
    {
        return await BuildGraphQuery()
            .OrderByDescending(mission => mission.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<MaintenanceMission> BuildGraphQuery()
    {
        return dbContext.Missions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(mission => mission.Tasks)
            .Include(mission => mission.Plan)
                .ThenInclude(plan => plan.ToolGroups)
                    .ThenInclude(group => group.Steps)
            .Include(mission => mission.Plan)
                .ThenInclude(plan => plan.SafetyNotes);
    }
}
