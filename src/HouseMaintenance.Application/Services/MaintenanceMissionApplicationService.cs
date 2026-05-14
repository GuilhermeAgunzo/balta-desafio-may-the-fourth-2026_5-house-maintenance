using HouseMaintenance.Application.Exceptions;
using HouseMaintenance.Core.Contracts;
using HouseMaintenance.Core.Entities;
using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Application.Services;

public sealed class MaintenanceMissionApplicationService(
    IMaintenanceMissionRepository repository,
    IMaintenancePlanningAgent planningAgent)
{
    public async Task<MaintenancePlanResponse> GeneratePlanAsync(
        GenerateMaintenancePlanRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var preparedTasks = MaintenanceTaskPreparation.Prepare(request.Tasks);

        MaintenancePlanDraft draft = await planningAgent.GeneratePlanAsync(
            new MaintenancePlanningContext { Tasks = preparedTasks },
            cancellationToken);

        MaintenancePlanContractValidator.Validate(draft, preparedTasks);

        var mission = new MaintenanceMission(preparedTasks, draft);
        await repository.AddAsync(mission, cancellationToken);

        return mission.ToResponse();
    }

    public async Task<MaintenancePlanResponse> GetPlanAsync(Guid missionId, CancellationToken cancellationToken)
    {
        if (missionId == Guid.Empty)
        {
            throw new RequestValidationException("O identificador da missão é obrigatório.");
        }

        MaintenanceMission mission = await repository.GetByIdAsync(missionId, cancellationToken)
            ?? throw new ResourceNotFoundException("Missão não encontrada.");

        return mission.ToResponse();
    }

    public async Task<IReadOnlyList<RecentMissionResponse>> ListRecentAsync(int take, CancellationToken cancellationToken)
    {
        if (take is < 1 or > 20)
        {
            throw new RequestValidationException("O histórico deve solicitar entre 1 e 20 missões.");
        }

        var missions = await repository.ListRecentAsync(take, cancellationToken);
        return missions.Select(mission => mission.ToRecentResponse()).ToArray();
    }
}
