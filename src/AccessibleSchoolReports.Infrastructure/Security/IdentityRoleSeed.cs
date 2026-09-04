using AccessibleSchoolReports.Application.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.Infrastructure.Security;

/// <summary>
/// Ensures the three application roles exist. Does not create users or passwords.
/// Safe if two startups try to create the same role.
/// </summary>
public static class IdentityRoleSeed
{
    public static async Task EnsureRolesAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in AppRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roles.RoleExistsAsync(name))
            {
                continue;
            }

            try
            {
                var result = await roles.CreateAsync(new IdentityRole(name));
                if (result.Succeeded || await roles.RoleExistsAsync(name))
                {
                    continue;
                }

                throw new InvalidOperationException($"Identity role '{name}' could not be created.");
            }
            catch (DbUpdateException)
            {
                if (!await roles.RoleExistsAsync(name))
                {
                    throw;
                }
            }
        }
    }
}
