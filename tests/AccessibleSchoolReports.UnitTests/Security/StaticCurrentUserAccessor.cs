using System.Security.Claims;
using AccessibleSchoolReports.Application.Security;

namespace AccessibleSchoolReports.UnitTests.Security;

internal sealed class StaticCurrentUserAccessor : ICurrentUserAccessor
{
    public StaticCurrentUserAccessor(ClaimsPrincipal user)
    {
        User = user;
    }

    public ClaimsPrincipal User { get; }
}
