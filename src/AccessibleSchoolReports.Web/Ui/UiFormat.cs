using AccessibleSchoolReports.Domain.Persistence;

namespace AccessibleSchoolReports.Web.Ui;

public static class UiFormat
{
    public static string StatusAccessibleName(RunStatus status) => $"Status: {Status(status)}";

    public static string Status(RunStatus status) => status switch
    {
        RunStatus.Pending => "Pending",
        RunStatus.Running => "Running",
        RunStatus.Completed => "Completed",
        RunStatus.CompletedWithErrors => "Completed with errors",
        RunStatus.Failed => "Failed",
        RunStatus.Cancelled => "Cancelled",
        _ => status.ToString(),
    };

    public static string StatusTone(RunStatus status) => status switch
    {
        RunStatus.Completed => "success",
        RunStatus.CompletedWithErrors => "warning",
        RunStatus.Failed => "error",
        RunStatus.Cancelled => "warning",
        RunStatus.Running => "info",
        _ => "info",
    };

    public static string Mode(ReportGenerationMode mode) => mode switch
    {
        ReportGenerationMode.Single => "Single school",
        ReportGenerationMode.Sequential => "Sequential",
        ReportGenerationMode.BoundedParallel => "Parallel",
        _ => mode.ToString(),
    };

    public static string Utc(DateTimeOffset? value) =>
        value is null ? "None" : value.Value.ToString("yyyy-MM-dd HH:mm 'UTC'");

    public static string Duration(long milliseconds)
    {
        if (milliseconds < 1000)
        {
            return $"{milliseconds} ms";
        }

        var seconds = milliseconds / 1000d;
        if (seconds < 60)
        {
            return $"{seconds:0.0} s";
        }

        var minutes = (int)(seconds / 60);
        return $"{minutes} min {seconds - (minutes * 60):0} s";
    }

    public static string Duration(TimeSpan duration) =>
        Duration((long)Math.Max(0, duration.TotalMilliseconds));

    public static string SchoolLabel(string code, string? name, int? graduateCount = null)
    {
        var label = string.IsNullOrWhiteSpace(name) ? code : $"{code} — {name}";
        if (graduateCount is null)
        {
            return label;
        }

        var noun = graduateCount == 1 ? "graduate" : "graduates";
        return $"{label} ({graduateCount} {noun})";
    }
}
