namespace AccessibleSchoolReports.Application.Knowledge;

public interface ILanguageModelService
{
    LanguageModelInfo Model { get; }

    Task<LanguageModelCompletion> CompleteAsync(
        LanguageModelRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class LanguageModelInfo
{
    public required string Provider { get; init; }

    public required string Model { get; init; }

    public string Key => $"{Provider}/{Model}";
}

public sealed class LanguageModelRequest
{
    public required string SystemInstructions { get; init; }

    public required string UserQuestion { get; init; }

    public required IReadOnlyList<LanguageModelContextDocument> ContextDocuments { get; init; }

    /// <summary>
    /// Application-computed sums and differences of printed PDF values.
    /// Not SAS calculator output. Omitted when the question is not arithmetic.
    /// </summary>
    public string? PrintedArithmetic { get; init; }
}

public sealed class LanguageModelContextDocument
{
    public required string FileName { get; init; }

    public required string SourceLocation { get; init; }

    public required string SourceIdentifier { get; init; }

    public string? RuleId { get; init; }

    public string? SchoolCode { get; init; }

    public int? ReportYear { get; init; }

    public required string Content { get; init; }
}

public sealed class LanguageModelCompletion
{
    public required string Text { get; init; }

    public required string Provider { get; init; }

    public required string Model { get; init; }
}
