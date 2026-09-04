namespace AccessibleSchoolReports.Domain.Entities;

/// <summary>
/// One text chunk of a knowledge document. Vectors are stored after a separate embedding index pass.
/// </summary>
public sealed class KnowledgeChunk
{
    public int Id { get; set; }

    public int KnowledgeDocumentId { get; set; }

    public KnowledgeDocument KnowledgeDocument { get; set; } = null!;

    public int ChunkNumber { get; set; }

    public required string Content { get; set; }

    public string? RuleId { get; set; }

    public required string Category { get; set; }

    public required string SourceLocation { get; set; }

    public byte[]? Embedding { get; set; }

    public string? EmbeddingModel { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
