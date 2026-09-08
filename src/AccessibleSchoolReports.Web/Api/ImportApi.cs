using AccessibleSchoolReports.Application.Imports;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Web.Ui;

namespace AccessibleSchoolReports.Web.Api;

public static class ImportApi
{
    private const long MaxFileBytes = 10 * 1024 * 1024;

    public static void MapImportApi(this WebApplication app)
    {
        app.MapGet("/api/imports", () => Results.Json(
                new
                {
                    accept = ".xlsx",
                    maxBytes = MaxFileBytes,
                    hint = "Accepted format: .xlsx. Maximum size: 10 MB.",
                },
                ApiConventions.Json))
            .RequireAuthorization(AppPolicies.RequireAdmin);

        app.MapPost("/api/imports", ImportAsync)
            .RequireAuthorization(AppPolicies.RequireAdmin);
    }

    private static async Task<IResult> ImportAsync(
        HttpRequest request,
        IGraduateImportService import,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Results.Json(new { error = "Choose an Excel workbook before importing." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }

        var file = request.Form.Files.GetFile("file") ?? request.Form.Files.FirstOrDefault();
        if (file is null)
        {
            return Results.Json(new { error = "Choose an Excel workbook before importing." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Json(new { error = "The selected file must be an .xlsx workbook." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }

        if (file.Length > MaxFileBytes)
        {
            return Results.Json(new { error = "The file is larger than 10 MB or could not be read." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }

        await using var stream = file.OpenReadStream();
        var result = await import.ImportAsync(stream, file.FileName, cancellationToken);
        return Results.Json(
            new
            {
                importRunId = result.ImportRunId,
                status = UiFormat.Status(result.Status),
                statusTone = result.WasDuplicate ? "warning" : UiFormat.StatusTone(result.Status),
                importedRowCount = result.ImportedRowCount,
                invalidRowCount = result.InvalidRowCount,
                blankRowCount = result.BlankRowCount,
                wasDuplicate = result.WasDuplicate,
                message = result.WasDuplicate
                    ? result.Message ?? "This file was already imported. Duplicate rows were not stored."
                    : $"Import {UiFormat.Status(result.Status).ToLowerInvariant()}. Imported {result.ImportedRowCount} rows.",
                issues = result.Issues.Take(100).Select(issue => new { rowNumber = issue.RowNumber, reason = issue.Reason }),
                issueCount = result.Issues.Count,
            },
            ApiConventions.Json);
    }
}
