namespace AccessibleSchoolReports.Domain.Persistence;

public enum RunStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    CompletedWithErrors = 3,
    Failed = 4,
    Cancelled = 5,
}
