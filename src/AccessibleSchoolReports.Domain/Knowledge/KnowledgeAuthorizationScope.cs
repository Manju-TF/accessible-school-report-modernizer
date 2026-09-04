namespace AccessibleSchoolReports.Domain.Knowledge;

public enum KnowledgeAuthorizationScope
{
    /// <summary>
    /// Legacy or other global knowledge. Visible to any authenticated caller.
    /// </summary>
    Authenticated = 0,

    /// <summary>
    /// Restricted to callers who can access the associated school.
    /// </summary>
    School = 1,

    /// <summary>
    /// Restricted to callers who can access the associated school and report.
    /// </summary>
    Report = 2,

    /// <summary>
    /// Restricted to Admin. Viewers and report users cannot retrieve this knowledge.
    /// </summary>
    Admin = 3,
}
