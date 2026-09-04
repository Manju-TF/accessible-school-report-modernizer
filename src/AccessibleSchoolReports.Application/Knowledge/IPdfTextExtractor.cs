namespace AccessibleSchoolReports.Application.Knowledge;

public interface IPdfTextExtractor
{
    PdfTextExtractionResult Extract(Stream pdf);
}

public enum PdfTextExtractionStatus
{
    Succeeded = 0,
    InvalidPdf = 1,
    Failed = 2,
}

public sealed record PdfExtractedPage(int PageNumber, string Text);

public sealed class PdfTextExtractionResult
{
    public required PdfTextExtractionStatus Status { get; init; }

    public IReadOnlyList<PdfExtractedPage> Pages { get; init; } = [];

    public string? Message { get; init; }

    public static PdfTextExtractionResult Invalid(string? message = null) =>
        new() { Status = PdfTextExtractionStatus.InvalidPdf, Message = message };

    public static PdfTextExtractionResult Failed(string? message = null) =>
        new() { Status = PdfTextExtractionStatus.Failed, Message = message };

    public static PdfTextExtractionResult Ok(IReadOnlyList<PdfExtractedPage> pages) =>
        new() { Status = PdfTextExtractionStatus.Succeeded, Pages = pages };
}
