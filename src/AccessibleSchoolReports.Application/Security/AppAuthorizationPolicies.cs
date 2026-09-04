using Microsoft.AspNetCore.Authorization;

namespace AccessibleSchoolReports.Application.Security;

/// <summary>
/// The only place that maps Identity roles onto authorization policies.
/// Pages and endpoints reference <see cref="AppPolicies"/> names, not role strings.
/// </summary>
public static class AppAuthorizationPolicies
{
    public static void Add(AuthorizationOptions options)
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.AddPolicy(
            AppPolicies.RequireAdmin,
            policy => policy.RequireRole(AppRoles.Admin));

        options.AddPolicy(
            AppPolicies.RequireReportAccess,
            policy => policy.RequireRole(AppRoles.Admin, AppRoles.ReportUser, AppRoles.Viewer));

        options.AddPolicy(
            AppPolicies.RequireRagAccess,
            policy => policy.RequireRole(AppRoles.Admin, AppRoles.ReportUser, AppRoles.Viewer));

        options.AddPolicy(
            AppPolicies.RequireReportGeneration,
            policy => policy.RequireRole(AppRoles.Admin, AppRoles.ReportUser));
    }
}
