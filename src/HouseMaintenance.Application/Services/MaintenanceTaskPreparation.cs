using System.Globalization;
using System.Text.RegularExpressions;
using HouseMaintenance.Application.Exceptions;
using HouseMaintenance.Core.Models;

namespace HouseMaintenance.Application.Services;

internal static partial class MaintenanceTaskPreparation
{
    private const int MaxTasksPerMission = 12;
    private const int MaxTaskLength = 120;

    private static readonly CultureInfo PortugueseCulture = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly ToolkitRule[] ToolkitRules =
    [
        new(
            "Kit elétrico",
            "Prioriza itens de energia para concentrar escada, alicate, lâmpadas e chaves de teste na mesma janela.",
            ["lâmpada", "lampada", "tomada", "interruptor", "fiação", "fiacao", "fusível", "fusivel", "luminária", "luminaria", "chuveiro"]),
        new(
            "Kit de fixação",
            "Agrupa furadeira, buchas, parafusos e nível para evitar medir a parede mais de uma vez.",
            ["quadro", "prateleira", "fixar", "furar", "broca", "parafuso", "suporte", "armário", "armario", "trilho", "gancho"]),
        new(
            "Kit hidráulico",
            "Agrupa itens com água, vedação e vazão para reaproveitar luvas, veda-rosca e balde.",
            ["ralo", "pia", "torneira", "cano", "vazamento", "registro", "descarga", "sifão", "sifao", "vedação", "vedacao"]),
        new(
            "Kit de limpeza",
            "Concentra limpeza, panos, escovas e produtos para limitar espalhamento de sujeira pela casa.",
            ["limpar", "lavar", "desengordurar", "poeira", "mancha", "varrer", "aspirar", "esfregar"]),
        new(
            "Kit de acabamento",
            "Agrupa retoques finos com lixa, massa, selante ou rejunte para um fechamento limpo da missão.",
            ["pintar", "lixa", "massa", "selante", "rejunte", "calafetar", "verniz"])
    ];

    public static IReadOnlyList<PreparedMaintenanceTask> Prepare(IReadOnlyList<string> rawTasks)
    {
        ArgumentNullException.ThrowIfNull(rawTasks);

        var preparedTasks = new List<PreparedMaintenanceTask>();
        var uniqueDescriptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (rawTask, index) in rawTasks.Select((task, index) => (task, index)))
        {
            var normalizedDescription = Normalize(rawTask);

            if (string.IsNullOrWhiteSpace(normalizedDescription))
            {
                continue;
            }

            if (!uniqueDescriptions.Add(normalizedDescription))
            {
                continue;
            }

            if (normalizedDescription.Length > MaxTaskLength)
            {
                throw new RequestValidationException($"O reparo '{normalizedDescription}' excede o limite de {MaxTaskLength} caracteres.");
            }

            var toolkit = ResolveToolkit(normalizedDescription);

            preparedTasks.Add(
                new PreparedMaintenanceTask
                {
                    OriginalDescription = rawTask.Trim(),
                    NormalizedDescription = normalizedDescription,
                    ToolkitHint = toolkit.Name,
                    ToolkitRationale = toolkit.Rationale,
                    OriginalOrder = index + 1
                });
        }

        if (preparedTasks.Count == 0)
        {
            throw new RequestValidationException("Informe pelo menos um reparo para montar o plano.");
        }

        if (preparedTasks.Count > MaxTasksPerMission)
        {
            throw new RequestValidationException($"Envie no máximo {MaxTasksPerMission} reparos por missão.");
        }

        return preparedTasks;
    }

    private static string Normalize(string? rawTask)
    {
        if (string.IsNullOrWhiteSpace(rawTask))
        {
            return string.Empty;
        }

        var trimmed = SpaceNormalizerRegex().Replace(rawTask.Trim(), " ");
        trimmed = trimmed.Trim().TrimEnd('.', ';', ',', ':');

        if (trimmed.Length < 3)
        {
            throw new RequestValidationException($"O reparo '{trimmed}' é curto demais para gerar um plano confiável.");
        }

        return char.ToUpper(trimmed[0], PortugueseCulture) + trimmed[1..];
    }

    private static ToolkitRule ResolveToolkit(string normalizedDescription)
    {
        var lowerDescription = normalizedDescription.ToLowerInvariant();

        foreach (var rule in ToolkitRules)
        {
            if (rule.Keywords.Any(lowerDescription.Contains))
            {
                return rule;
            }
        }

        return new ToolkitRule(
            "Kit geral de manutenção",
            "Mantém itens híbridos juntos com ferramentas universais como luvas, panos, chave multiuso e organizadores.",
            []);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceNormalizerRegex();

    private sealed record ToolkitRule(string Name, string Rationale, IReadOnlyList<string> Keywords);
}
