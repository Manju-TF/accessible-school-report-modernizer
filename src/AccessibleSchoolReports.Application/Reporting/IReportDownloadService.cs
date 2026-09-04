using System.Security.Claims;

namespace AccessibleSchoolReports.Application.Reporting;

public interface IReportDownloadService
{
    /// <summary>
    /// Authenticate, load report metadata, authorize the school, then open the PDF.
    /// Failures (missing, unauthorized, missing file) are indistinguishable.
    /// </summary>
    Task<ReportDownloadResult> TryDownloadAsync(
        ClaimsPrincipal user,
        int reportRunItemId,
        CancellationToken cancellationToken = default);
}

public sealed class ReportDownloadResult
{
    private ReportDownloadResult(bool succeeded, Stream? content, string fileName)
    {
        Succeeded = succeeded;
        Content = content;
        FileName = fileName;
    }

    public bool Succeeded { get; }

    public Stream? Content { get; }

    public string FileName { get; }

    public static ReportDownloadResult Denied { get; } = new(false, null, string.Empty);

    public static ReportDownloadResult Ok(Stream content, string fileName)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        return new ReportDownloadResult(true, content, fileName);
    }
}
