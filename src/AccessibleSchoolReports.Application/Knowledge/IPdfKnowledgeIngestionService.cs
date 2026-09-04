namespace AccessibleSchoolReports.Application.Knowledge;

public interface IPdfKnowledgeIngestionService
{
    Task<PdfKnowledgeIngestionResult> IndexGeneratedReportAsync(
        GeneratedPdfKnowledgeRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class GeneratedPdfKnowledgeRequest
{
    public const string DefaultReportType = "Summary";

    public int ReportRunItemId { get; init; }

    public int? ReportRunId { get; init; }

    public int SchoolId { get; init; }

    public string? SchoolCode { get; init; }

    public string? OutputPath { get; init; }

    public int? ReportYear { get; init; }

    public string? ReportType { get; init; }
}

public enum PdfKnowledgeIngestionStatus
{
    Indexed = 0,
    SkippedDuplicate = 1,
    Reindexed = 2,
    MissingPdf = 3,
    InvalidPdf = 4,
    ExtractionFailed = 5,
    Rejected = 6,
}

public sealed class PdfKnowledgeIngestionResult
{
    public required PdfKnowledgeIngestionStatus Status { get; init; }

    public int? KnowledgeDocumentId { get; init; }

    public string? Message { get; init; }

    public static PdfKnowledgeIngestionResult From(
        PdfKnowledgeIngestionStatus status,
        int? knowledgeDocumentId = null,
        string? message = null) =>
        new()
        {
            Status = status,
            KnowledgeDocumentId = knowledgeDocumentId,
            Message = message,
        };
}
