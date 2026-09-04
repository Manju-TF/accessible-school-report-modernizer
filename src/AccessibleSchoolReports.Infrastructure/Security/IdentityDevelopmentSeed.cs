using AccessibleSchoolReports.Application.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.Infrastructure.Security;

/// <summary>
/// Development-only Identity user. Created only when both seed settings are present.
/// Passwords are hashed by ASP.NET Core Identity. This type never logs credentials.
/// </summary>
public static class IdentityDevelopmentSeed
{
    public const string UserNameKey = "Identity:SeedUserName";
    public const string PasswordKey = "Identity:SeedPassword";
    public const string RoleKey = "Identity:SeedRole";

    public static async Task TryApplyAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var userName = configuration[UserNameKey];
        var password = configuration[PasswordKey];
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var role = configuration[RoleKey];
        role = string.IsNullOrWhiteSpace(role) ? AppRoles.Admin : role.Trim();
        if (!AppRoles.IsDefined(role))
        {
            throw new InvalidOperationException(
                "Identity:SeedRole must be Admin, ReportUser, or Viewer.");
        }

        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        var user = await users.FindByNameAsync(userName.Trim());
        if (user is null)
        {
            user = new IdentityUser { UserName = userName.Trim() };
            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    "Identity seed user could not be created. Set Identity:SeedPassword to a value that meets Identity password rules.");
            }
        }

        if (!await users.IsInRoleAsync(user, role))
        {
            var assigned = await users.AddToRoleAsync(user, role);
            if (!assigned.Succeeded)
            {
                throw new InvalidOperationException("Identity seed user could not be assigned a role.");
            }
        }
    }
}
