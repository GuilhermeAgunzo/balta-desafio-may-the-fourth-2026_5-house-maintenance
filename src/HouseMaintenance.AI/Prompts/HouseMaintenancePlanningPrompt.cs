using System.Text;
using HouseMaintenance.Core.Models;

namespace HouseMaintenance.AI.Prompts;

internal static class HouseMaintenancePlanningPrompt
{
    public const string Instructions =
        """
        Você é o Quartermaster da manutenção doméstica em um hangar Rebelde.
        Sua missão é montar uma sequência de execução que reduza trocas de ferramentas, deslocamentos e sujeira, sem sacrificar segurança.

        Regras obrigatórias:
        - Responda sempre em português do Brasil.
        - Use exatamente os valores de `normalizedDescription` enviados pela aplicação em `toolGroups[].steps[].taskName`.
        - Não invente, renomeie, combine ou omita tarefas.
        - Agrupe tarefas pelo mesmo kit sempre que isso fizer sentido operacionalmente.
        - Priorize elétrica antes de tarefas com água ou umidade quando houver conflito.
        - Crie justificativas curtas, úteis e concretas.
        - Gere entre 1 e 5 notas de segurança.
        - Pense na interface como um painel de missão Star Wars: técnico, claro e com tom épico, mas sem exagero.
        """;

    public static string Build(MaintenancePlanningContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var prompt = new StringBuilder();
        prompt.AppendLine("Monte um plano ordenado para a seguinte missão doméstica:");
        prompt.AppendLine();
        prompt.AppendLine("Tarefas da missão:");

        foreach (var task in context.Tasks.OrderBy(task => task.OriginalOrder))
        {
            prompt.AppendLine($"- originalDescription: \"{task.OriginalDescription}\"");
            prompt.AppendLine($"  normalizedDescription: \"{task.NormalizedDescription}\"");
            prompt.AppendLine($"  toolkitHint: \"{task.ToolkitHint}\"");
            prompt.AppendLine($"  toolkitRationale: \"{task.ToolkitRationale}\"");
        }

        prompt.AppendLine();
        prompt.AppendLine("Campos esperados na resposta estruturada:");
        prompt.AppendLine("- missionTitle: título curto da missão");
        prompt.AppendLine("- summary: resumo operacional com foco em menos bagunça");
        prompt.AppendLine("- strategicRationale: explicação de por que a sequência reduz trocas de kit");
        prompt.AppendLine("- toolGroups: grupos ordenados por kit de ferramentas");
        prompt.AppendLine("  - toolkit: nome do kit");
        prompt.AppendLine("  - toolkitReason: por que o grupo ficou nessa posição");
        prompt.AppendLine("  - steps: passos dentro do grupo");
        prompt.AppendLine("    - taskName: deve ser exatamente um normalizedDescription da lista");
        prompt.AppendLine("    - whyNow: justificativa curta do passo");
        prompt.AppendLine("    - estimatedMinutes: inteiro entre 5 e 180");
        prompt.AppendLine("- safetyNotes: notas curtas e práticas");
        prompt.AppendLine();
        prompt.AppendLine("Evite repetir kits em grupos separados quando puder consolidar.");

        return prompt.ToString();
    }
}
