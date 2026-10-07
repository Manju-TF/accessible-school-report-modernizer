namespace AccessibleSchoolReports.Application.Knowledge;

public sealed class EmbeddingOptions
{
    public const string SectionName = "Embeddings";

    public string Provider { get; set; } = "Gemini";

    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    public string Model { get; set; } = "gemini-embedding-001";

    public int Dimensions { get; set; } = 3072;

    public int TimeoutSeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 3;

    public int MaxBatchSize { get; set; } = 16;

    public string ApiKey { get; set; } = "";

    public bool UsesLocalLexical =>
        string.Equals(Provider, "Lexical", StringComparison.OrdinalIgnoreCase);

    public bool UsesGemini =>
        string.Equals(Provider, "Gemini", StringComparison.OrdinalIgnoreCase);

    public override string ToString() =>
        $"Provider={Provider}; Model={Model}; Dimensions={Dimensions}; TimeoutSeconds={TimeoutSeconds}";
}
