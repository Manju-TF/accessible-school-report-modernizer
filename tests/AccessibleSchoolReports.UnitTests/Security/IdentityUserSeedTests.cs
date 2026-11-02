using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.UnitTests.Security;

public sealed class IdentityUserSeedTests
{
    [Fact]
    public async Task TryApplyAsync_CreatesAndResetsConfiguredUserPassword()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var settings = new Dictionary<string, string?>
        {
            [IdentityUserSeed.UserNameKey] = "iis-admin",
            [IdentityUserSeed.PasswordKey] = "First-Password-1!",
            [IdentityUserSeed.RoleKey] = "Admin",
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddDbContext<SchoolReportsDbContext>(options => options.UseSqlite(connection));
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<SchoolReportsDbContext>()
            .AddDefaultTokenProviders();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<SchoolReportsDbContext>();
        await database.Database.EnsureCreatedAsync();
        await IdentityRoleSeed.EnsureRolesAsync(scope.ServiceProvider);

        await IdentityUserSeed.TryApplyAsync(scope.ServiceProvider);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByNameAsync("iis-admin");
        Assert.NotNull(user);
        Assert.True(await userManager.CheckPasswordAsync(user, "First-Password-1!"));
        Assert.True(await userManager.IsInRoleAsync(user, "Admin"));

        configuration[IdentityUserSeed.PasswordKey] = "Second-Password-2!";
        await IdentityUserSeed.TryApplyAsync(scope.ServiceProvider);

        Assert.False(await userManager.CheckPasswordAsync(user, "First-Password-1!"));
        Assert.True(await userManager.CheckPasswordAsync(user, "Second-Password-2!"));
    }
}
