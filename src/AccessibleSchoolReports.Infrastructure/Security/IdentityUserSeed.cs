using AccessibleSchoolReports.Application.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.Infrastructure.Security;

public static class IdentityUserSeed
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
        if (string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Identity:SeedUserName and Identity:SeedPassword must both be configured, or both left empty.");
        }

        var role = configuration[RoleKey];
        role = string.IsNullOrWhiteSpace(role) ? AppRoles.Admin : role.Trim();
        if (!AppRoles.IsDefined(role))
        {
            throw new InvalidOperationException(
                "Identity:SeedRole must be Admin, ReportUser, or Viewer.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        var user = await users.FindByNameAsync(userName.Trim());
        if (user is null)
        {
            user = new IdentityUser { UserName = userName.Trim() };
            var created = await users.CreateAsync(user, password);
            EnsureSucceeded(created, "Identity seed user could not be created.");
        }
        else
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var reset = await users.ResetPasswordAsync(user, token, password);
            EnsureSucceeded(reset, "Identity seed user's password could not be reset.");
        }

        var accessFailedCountReset = await users.ResetAccessFailedCountAsync(user);
        EnsureSucceeded(accessFailedCountReset, "Identity seed user's failed sign-in count could not be reset.");
        var lockoutReset = await users.SetLockoutEndDateAsync(user, null);
        EnsureSucceeded(lockoutReset, "Identity seed user could not be unlocked.");

        if (!await users.IsInRoleAsync(user, role))
        {
            var assigned = await users.AddToRoleAsync(user, role);
            EnsureSucceeded(assigned, "Identity seed user could not be assigned a role.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
            throw new InvalidOperationException($"{message} {errors}");
        }
    }
}
