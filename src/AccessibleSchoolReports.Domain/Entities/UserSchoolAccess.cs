using AccessibleSchoolReports.Domain.Security;

namespace AccessibleSchoolReports.Domain.Entities;

/// <summary>
/// Explicit school assignment for a non-admin Identity user.
/// Admin does not need a row; Admin may access every school.
/// </summary>
public sealed class UserSchoolAccess
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public int SchoolId { get; set; }

    public School School { get; set; } = null!;

    public SchoolAccessLevel AccessLevel { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
