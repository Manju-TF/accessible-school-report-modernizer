using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Web.Ui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Web.Api;

public static class ReportsApi
{
    public static void MapReportsApi(this WebApplication app)
    {
        app.MapGet("/api/reports/all", async (
                SchoolReportsDbContext db,
                IOptions<ReportGenerationOptions> options,
                CancellationToken cancellationToken) =>
            {
                var eligibleIds = await db.Schools
                    .AsNoTracking()
                    .Where(school => db.GraduateRecords.Any(row => row.SchoolId == school.Id))
                    .Select(school => school.Id)
                    .ToListAsync(cancellationToken);
                var years = await SchoolClassYears.LoadAsync(
                    db,
                    eligibleIds,
                    options.Value.ClassYear,
                    cancellationToken);
                return Results.Json(
                    new
                    {
                        eligibleCount = eligibleIds.Count,
                        classYear = years.DefaultYear,
                        classYears = years.Years,
                        minParallelism = ReportGenerationOptions.MinMaxParallelism,
                        maxParallelism = ReportGenerationOptions.MaxMaxParallelism,
                        defaultParallelism = ReportGenerationOptions.DefaultMaxParallelism,
                    },
                    ApiConventions.Json);
            })
            .RequireAuthorization(AppPolicies.RequireAdmin);

        app.MapPost("/api/reports", GenerateOneAsync)
            .RequireAuthorization(AppPolicies.RequireReportGeneration);

        app.MapPost("/api/reports/all", GenerateAllAsync)
            .RequireAuthorization(AppPolicies.RequireAdmin);

        app.MapGet("/api/reports/{id:int}", GetReportAsync)
            .RequireAuthorization(AppPolicies.RequireReportAccess);
    }

    private static async Task<IResult> GenerateOneAsync(
        GenerateOneRequest body,
        IReportGenerationService generation,
        CancellationToken cancellationToken)
    {
        if (body.SchoolId <= 0)
        {
            return Results.Json(new { error = "Select a school before generating a report." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }

        if (!ApiConventions.IsFourDigitYear(body.ClassYear))
        {
            return Results.Json(new { error = "Report year must be a four-digit year." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await generation.GenerateSchoolReportAsync(body.SchoolId, cancellationToken, body.ClassYear.Trim());
        return Results.Json(ToSchoolResult(result), ApiConventions.Json);
    }

    private static async Task<IResult> GenerateAllAsync(
        GenerateAllRequest body,
        IReportGenerationService generation,
        CancellationToken cancellationToken)
    {
        if (!ApiConventions.IsFourDigitYear(body.ClassYear))
        {
            return Results.Json(new { error = "Report year must be a four-digit year." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }

        var parallel = string.Equals(body.Mode, "parallel", StringComparison.OrdinalIgnoreCase);
        if (parallel
            && (body.MaxParallelism < ReportGenerationOptions.MinMaxParallelism
                || body.MaxParallelism > ReportGenerationOptions.MaxMaxParallelism))
        {
            return Results.Json(
                new { error = $"Maximum parallelism must be between {ReportGenerationOptions.MinMaxParallelism} and {ReportGenerationOptions.MaxMaxParallelism}." },
                ApiConventions.Json,
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = parallel
            ? await generation.GenerateAllParallelAsync(body.MaxParallelism, cancellationToken, body.ClassYear.Trim())
            : await generation.GenerateAllSequentialAsync(cancellationToken, body.ClassYear.Trim());

        return Results.Json(
            new
            {
                runId = result.ReportRunId,
                status = UiFormat.Status(result.Status),
                statusTone = UiFormat.StatusTone(result.Status),
                total = result.Total,
                successful = result.Successful,
                failed = result.Failed,
                duration = UiFormat.Duration(result.Duration),
                message = result.Message,
                items = result.Items.Select(ToSchoolResult),
            },
            ApiConventions.Json);
    }

    private static async Task<IResult> GetReportAsync(
        int id,
        HttpContext http,
        SchoolReportsDbContext db,
        IReportAuthorizationService authorization,
        CancellationToken cancellationToken)
    {
        if (!await authorization.CanViewReportAsync(http.User, id, cancellationToken))
        {
            return Results.Json(
                new { error = "That report is not available." },
                ApiConventions.Json,
                statusCode: StatusCodes.Status404NotFound);
        }

        var item = await db.ReportRunItems
            .AsNoTracking()
            .Include(row => row.School)
            .FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item?.School is null)
        {
            return Results.Json(
                new { error = "That report is not available." },
                ApiConventions.Json,
                statusCode: StatusCodes.Status404NotFound);
        }

        var year = await db.KnowledgeDocuments
            .AsNoTracking()
            .Where(document => document.ReportId == id)
            .Select(document => document.ReportYear)
            .FirstOrDefaultAsync(cancellationToken);

        var completed = item.Status == Domain.Persistence.RunStatus.Completed && !string.IsNullOrWhiteSpace(item.OutputPath);
        return Results.Json(
            new
            {
                id = item.Id,
                schoolCode = item.School.Code,
                schoolName = item.School.Name,
                schoolLabel = UiFormat.SchoolLabel(item.School.Code, item.School.Name),
                status = UiFormat.Status(item.Status),
                statusTone = UiFormat.StatusTone(item.Status),
                reportYear = year,
                startedUtc = UiFormat.Utc(item.StartedUtc),
                completedUtc = UiFormat.Utc(item.CompletedUtc),
                downloadUrl = completed ? $"/downloads/reports/{item.Id}/{item.School.Code}-summary-report.pdf" : null,
                downloadName = completed ? $"{item.School.Code}-summary-report.pdf" : null,
            },
            ApiConventions.Json);
    }

    private static object ToSchoolResult(SchoolReportGenerationResult result)
    {
        var completed = result.Status == Domain.Persistence.RunStatus.Completed && result.ReportRunItemId is int;
        return new
        {
            runId = result.ReportRunId,
            reportRunItemId = result.ReportRunItemId,
            schoolId = result.SchoolId,
            schoolCode = result.SchoolCode,
            schoolName = result.SchoolName,
            schoolLabel = UiFormat.SchoolLabel(result.SchoolCode ?? "unknown", result.SchoolName),
            status = UiFormat.Status(result.Status),
            statusTone = UiFormat.StatusTone(result.Status),
            graduateCount = result.GraduateCount,
            duration = UiFormat.Duration(result.Duration),
            message = result.Message,
            downloadUrl = completed
                ? $"/downloads/reports/{result.ReportRunItemId}/{result.SchoolCode ?? "school"}-summary-report.pdf"
                : null,
        };
    }

    public sealed record GenerateOneRequest(int SchoolId, string ClassYear);

    public sealed record GenerateAllRequest(string Mode, int MaxParallelism, string ClassYear);
}
