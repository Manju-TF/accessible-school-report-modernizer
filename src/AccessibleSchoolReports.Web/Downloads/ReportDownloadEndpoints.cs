using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Application.Security;
using Microsoft.Net.Http.Headers;

namespace AccessibleSchoolReports.Web.Downloads;

public static class ReportDownloadEndpoints
{
    public static void MapReportDownloads(this WebApplication app)
    {
        app.MapGet("/downloads/reports/{itemId:int}/{fileName}", DownloadAsync)
            .RequireAuthorization(AppPolicies.RequireReportAccess);
        app.MapGet("/downloads/reports/{itemId:int}", DownloadAsync)
            .RequireAuthorization(AppPolicies.RequireReportAccess);
    }

    private static async Task<IResult> DownloadAsync(
        int itemId,
        HttpContext http,
        IReportDownloadService downloads,
        CancellationToken cancellationToken)
    {
        var result = await downloads.TryDownloadAsync(http.User, itemId, cancellationToken);
        if (!result.Succeeded || result.Content is null)
        {
            return Results.NotFound();
        }

        return new PdfStreamDownloadResult(result.Content, result.FileName);
    }

    private sealed class PdfStreamDownloadResult : IResult
    {
        private readonly Stream _content;
        private readonly string _fileName;

        public PdfStreamDownloadResult(Stream content, string fileName)
        {
            _content = content;
            _fileName = fileName;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            await using (_content)
            {
                var disposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = _fileName
                };

                httpContext.Response.ContentType = "application/pdf";
                httpContext.Response.Headers.XContentTypeOptions = "nosniff";
                httpContext.Response.Headers.ContentDisposition = disposition.ToString();
                await _content.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted);
            }
        }
    }
}
