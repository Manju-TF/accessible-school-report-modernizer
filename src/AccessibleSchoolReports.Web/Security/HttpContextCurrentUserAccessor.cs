using System.Security.Claims;
using AccessibleSchoolReports.Application.Security;

namespace AccessibleSchoolReports.Web.Security;

public sealed class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _http;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor http)
    {
        _http = http;
    }

    public ClaimsPrincipal User => _http.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
}
