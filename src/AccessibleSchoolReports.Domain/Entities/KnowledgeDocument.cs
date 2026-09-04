using AccessibleSchoolReports.Domain.Knowledge;

namespace AccessibleSchoolReports.Domain.Entities;

/// <summary>
/// Indexed knowledge metadata. Stores a PDF path or other source reference, never PDF bytes.
/// </summary>
public sealed class KnowledgeDocument
{
    public int Id { get; set; }

    public required string FileName { get; set; }

    public KnowledgeDocumentType DocumentType { get; set; }

    public required string ContentHash { get; set; }

    /// <summary>
    /// Physical or logical source reference (for example a stored report output path).
    /// </summary>
    public required string SourceIdentifier { get; set; }

    public DateTimeOffset IndexedAt { get; set; }

    public int? SchoolId { get; set; }

    public School? School { get; set; }

    public string? SchoolCode { get; set; }

    public int? ReportId { get; set; }

    public ReportRunItem? Report { get; set; }

    public int? ReportRunId { get; set; }

    public ReportRun? ReportRun { get; set; }

    public int? ReportYear { get; set; }

    public string? ReportType { get; set; }

    public KnowledgeAuthorizationScope AuthorizationScope { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<KnowledgeChunk> Chunks { get; } = new List<KnowledgeChunk>();
}
