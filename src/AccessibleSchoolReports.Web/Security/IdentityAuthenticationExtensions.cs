using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace AccessibleSchoolReports.Web.Security;

public static class IdentityAuthenticationExtensions
{
    public const string AuthCookieName = ".asr.auth";

    public static IServiceCollection AddSchoolReportsIdentity(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        services.AddCascadingAuthenticationState();
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<SchoolReportsDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = AuthCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.LoginPath = "/signin";
            options.LogoutPath = "/account/signout";
            options.AccessDeniedPath = "/denied";
            options.ReturnUrlParameter = "returnUrl";
        });

        services.AddAuthorization(AppAuthorizationPolicies.Add);

        return services;
    }
}
