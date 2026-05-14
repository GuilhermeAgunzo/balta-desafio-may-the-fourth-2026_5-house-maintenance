using HouseMaintenance.Application.Exceptions;
using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Application.Services;

internal static class MaintenancePlanContractValidator
{
    public static void Validate(MaintenancePlanDraft draft, IReadOnlyList<PreparedMaintenanceTask> preparedTasks)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(preparedTasks);

        if (string.IsNullOrWhiteSpace(draft.MissionTitle))
        {
            throw new MaintenancePlanContractException("A IA retornou um plano sem título.");
        }

        if (string.IsNullOrWhiteSpace(draft.Summary))
        {
            throw new MaintenancePlanContractException("A IA retornou um plano sem resumo.");
        }

        if (string.IsNullOrWhiteSpace(draft.StrategicRationale))
        {
            throw new MaintenancePlanContractException("A IA retornou um plano sem justificativa estratégica.");
        }

        if (draft.ToolGroups.Count == 0)
        {
            throw new MaintenancePlanContractException("A IA não agrupou os reparos por kit de ferramentas.");
        }

        if (draft.SafetyNotes.Count == 0)
        {
            throw new MaintenancePlanContractException("A IA deve retornar ao menos uma nota de segurança.");
        }

        var expectedTasks = preparedTasks
            .Select(task => task.NormalizedDescription)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var actualTasks = draft.ToolGroups
            .SelectMany(group => group.Steps)
            .Select(step =>
            {
                if (string.IsNullOrWhiteSpace(step.TaskName))
                {
                    throw new MaintenancePlanContractException("A IA retornou um passo sem nome de reparo.");
                }

                if (string.IsNullOrWhiteSpace(step.WhyNow))
                {
                    throw new MaintenancePlanContractException($"O passo '{step.TaskName}' veio sem justificativa.");
                }

                if (step.EstimatedMinutes is < 5 or > 180)
                {
                    throw new MaintenancePlanContractException($"O passo '{step.TaskName}' veio com duração inválida.");
                }

                return step.TaskName.Trim();
            })
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (expectedTasks.Length != actualTasks.Length)
        {
            throw new MaintenancePlanContractException("A IA não retornou a mesma quantidade de reparos enviada.");
        }

        for (var index = 0; index < expectedTasks.Length; index++)
        {
            if (!string.Equals(expectedTasks[index], actualTasks[index], StringComparison.OrdinalIgnoreCase))
            {
                throw new MaintenancePlanContractException(
                    "A IA deve reutilizar exatamente os reparos normalizados enviados pela camada de aplicação.");
            }
        }

        foreach (var group in draft.ToolGroups)
        {
            if (string.IsNullOrWhiteSpace(group.Toolkit))
            {
                throw new MaintenancePlanContractException("A IA retornou um grupo sem nome de kit.");
            }

            if (string.IsNullOrWhiteSpace(group.ToolkitReason))
            {
                throw new MaintenancePlanContractException($"O grupo '{group.Toolkit}' veio sem explicação.");
            }

            if (group.Steps.Count == 0)
            {
                throw new MaintenancePlanContractException($"O grupo '{group.Toolkit}' não contém reparos.");
            }
        }
    }
}
