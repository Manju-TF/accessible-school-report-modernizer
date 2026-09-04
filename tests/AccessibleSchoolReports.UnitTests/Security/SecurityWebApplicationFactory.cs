using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Security;
using AccessibleSchoolReports.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AccessibleSchoolReports.UnitTests.Security;

public sealed class SecurityWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestUserName = "test.user";
    public const string TestPassword = "Test-Password-1!";
    public const string AdminUserName = "admin.user";
    public const string ReportUserName = "report.user";
    public const string ViewerUserName = "viewer.user";

    private static readonly object HostGate = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var directory = Path.Combine(Path.GetTempPath(), "asr-auth-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var database = Path.Combine(directory, "schoolreports.db");

        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:SchoolReports", $"Data Source={database}");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SchoolReports"] = $"Data Source={database}",
                ["Identity:SeedUserName"] = "",
                ["Identity:SeedPassword"] = "",
                ["Identity:SeedRole"] = "",
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        lock (HostGate)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            IdentityRoleSeed.EnsureRolesAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            EnsureUser(users, TestUserName, AppRoles.Viewer);
            EnsureUser(users, ViewerUserName, AppRoles.Viewer);
            EnsureUser(users, ReportUserName, AppRoles.ReportUser);
            EnsureUser(users, AdminUserName, AppRoles.Admin);
            return host;
        }
    }

    private static void EnsureUser(UserManager<IdentityUser> users, string userName, string role)
    {
        var user = users.FindByNameAsync(userName).GetAwaiter().GetResult()
            ?? CreateUser(users, userName);

        if (!users.IsInRoleAsync(user, role).GetAwaiter().GetResult())
        {
            var assigned = users.AddToRoleAsync(user, role).GetAwaiter().GetResult();
            if (!assigned.Succeeded && !users.IsInRoleAsync(user, role).GetAwaiter().GetResult())
            {
                throw new InvalidOperationException($"Could not assign role '{role}'.");
            }
        }
    }

    private static IdentityUser CreateUser(UserManager<IdentityUser> users, string userName)
    {
        var user = new IdentityUser { UserName = userName };
        var created = users.CreateAsync(user, TestPassword).GetAwaiter().GetResult();
        if (created.Succeeded)
        {
            return user;
        }

        var existing = users.FindByNameAsync(userName).GetAwaiter().GetResult();
        if (existing is not null)
        {
            return existing;
        }

        throw new InvalidOperationException($"Could not create test user '{userName}'.");
    }
}
