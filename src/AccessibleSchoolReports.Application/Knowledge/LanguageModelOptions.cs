namespace AccessibleSchoolReports.Application.Knowledge;

public sealed class LanguageModelOptions
{
    public const string SectionName = "LanguageModel";

    public string Provider { get; set; } = "Gemini";

    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions";

    public string Model { get; set; } = "gemini-3.8-flash";

    public int TimeoutSeconds { get; set; } = 60;

    public int MaxRetries { get; set; } = 3;

    public double Temperature { get; set; }

    public int MaxTokens { get; set; } = 800;

    public string ApiKey { get; set; } = "";

    public override string ToString() =>
        $"Provider={Provider}; Model={Model}; TimeoutSeconds={TimeoutSeconds}; Temperature={Temperature}";
}
