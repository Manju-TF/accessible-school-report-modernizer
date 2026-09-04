using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace AccessibleSchoolReports.Web.Downloads;

public static class ReportDownloadEndpoints
{
    public static void MapReportDownloads(this WebApplication app)
    {
        app.MapGet("/downloads/reports/{itemId:int}/{fileName}", DownloadAsync);
        app.MapGet("/downloads/reports/{itemId:int}", DownloadAsync);
    }

    private static async Task<IResult> DownloadAsync(
        int itemId,
        SchoolReportsDbContext db,
        IOptions<ReportGenerationOptions> options,
        CancellationToken cancellationToken)
    {
        var item = await db.ReportRunItems
            .AsNoTracking()
            .Include(row => row.School)
            .FirstOrDefaultAsync(row => row.Id == itemId, cancellationToken);

        if (item is null
            || !ReportFileAccess.TryResolveDownloadPath(item.OutputPath, options.Value.OutputRoot, out var path))
        {
            return Results.NotFound();
        }

        var fileName = $"{SanitizeFileName(item.School.Code)}-summary-report.pdf";
        return new PdfFileDownloadResult(path, fileName);
    }

    private static string SanitizeFileName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "school";
        }

        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray();
        return new string(chars);
    }

    private sealed class PdfFileDownloadResult : IResult
    {
        private readonly string _path;
        private readonly string _fileName;

        public PdfFileDownloadResult(string path, string fileName)
        {
            _path = path;
            _fileName = fileName;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            var disposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = _fileName
            };

            httpContext.Response.ContentType = "application/pdf";
            httpContext.Response.Headers.XContentTypeOptions = "nosniff";
            httpContext.Response.Headers.ContentDisposition = disposition.ToString();
            await httpContext.Response.SendFileAsync(_path, httpContext.RequestAborted);
        }
    }
}
