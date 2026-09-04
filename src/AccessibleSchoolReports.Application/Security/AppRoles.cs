namespace AccessibleSchoolReports.Application.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string ReportUser = "ReportUser";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlyList<string> All = [Admin, ReportUser, Viewer];

    public static bool IsDefined(string? role) =>
        !string.IsNullOrWhiteSpace(role)
        && All.Contains(role.Trim(), StringComparer.Ordinal);
}
