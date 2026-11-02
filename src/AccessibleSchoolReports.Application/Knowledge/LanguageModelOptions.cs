namespace AccessibleSchoolReports.Application.Knowledge;

public sealed class LanguageModelOptions
{
    public const string SectionName = "LanguageModel";

    public string Provider { get; set; } = "OpenAICompatible";

    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    public string Model { get; set; } = "gpt-4o-mini";

    public int TimeoutSeconds { get; set; } = 60;

    public int MaxRetries { get; set; } = 3;

    public double Temperature { get; set; }

    public int MaxTokens { get; set; } = 800;

    public string ApiKey { get; set; } = "";

    public override string ToString() =>
        $"Provider={Provider}; Model={Model}; TimeoutSeconds={TimeoutSeconds}; Temperature={Temperature}";
}
