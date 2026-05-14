namespace HouseMaintenance.AI.Configuration;

public sealed class OpenAiAgentOptions
{
    public const string SectionName = "OpenAI";

    public string? ApiKey { get; init; }

    public string Model { get; init; } = "gpt-5.4-mini";

    public int TimeoutSeconds { get; init; } = 90;
}
