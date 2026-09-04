namespace AccessibleSchoolReports.Application.Knowledge;

public sealed class EmbeddingOptions
{
    public const string SectionName = "Embeddings";

    public string Provider { get; set; } = "OpenAICompatible";

    public string Endpoint { get; set; } = "https://api.openai.com/v1/embeddings";

    public string Model { get; set; } = "text-embedding-3-small";

    public int Dimensions { get; set; } = 1536;

    public int TimeoutSeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 3;

    public int MaxBatchSize { get; set; } = 16;

    public string ApiKey { get; set; } = "";

    public bool UsesLocalLexical =>
        string.Equals(Provider, "Lexical", StringComparison.OrdinalIgnoreCase);

    public override string ToString() =>
        $"Provider={Provider}; Model={Model}; Dimensions={Dimensions}; TimeoutSeconds={TimeoutSeconds}";
}
