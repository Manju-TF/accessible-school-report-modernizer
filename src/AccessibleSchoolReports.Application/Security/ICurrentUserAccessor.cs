using System.Security.Claims;

namespace AccessibleSchoolReports.Application.Security;

public interface ICurrentUserAccessor
{
    ClaimsPrincipal User { get; }
}
