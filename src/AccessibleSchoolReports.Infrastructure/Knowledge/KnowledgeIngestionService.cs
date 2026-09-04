using System.Text;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Knowledge;

public sealed class KnowledgeIngestionService : IKnowledgeIngestionService
{
    private readonly SchoolReportsDbContext _db;

    public KnowledgeIngestionService(SchoolReportsDbContext db)
    {
        _db = db;
    }

    public async Task<KnowledgeIngestionResult> IngestLegacyAndProjectDocumentsAsync(
        string repositoryRoot,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var root = Path.GetFullPath(repositoryRoot);
        var resolved = ResolveSources(root);
        var indexed = new List<string>();
        var reindexed = new List<string>();
        var skipped = new List<string>();
        var now = DateTimeOffset.UtcNow;

        foreach (var source in resolved.Sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = await ReadAllBytesAsync(source.FullPath, cancellationToken);
            var hash = KnowledgeContentHash.Sha256Hex(bytes);
            var existing = await _db.KnowledgeDocuments
                .Include(document => document.Chunks)
                .FirstOrDefaultAsync(
                    document => document.SourceIdentifier == source.RelativePath,
                    cancellationToken);

            if (existing is not null && existing.ContentHash == hash)
            {
                skipped.Add(source.RelativePath);
                continue;
            }

            var text = Encoding.UTF8.GetString(bytes);
            var chunks = KnowledgeTextChunker.Chunk(text, source.Kind);
            if (existing is null)
            {
                _db.KnowledgeDocuments.Add(CreateDocument(source, hash, chunks, now));
                indexed.Add(source.RelativePath);
            }
            else
            {
                _db.KnowledgeChunks.RemoveRange(existing.Chunks);
                existing.Chunks.Clear();
                ReplaceDocument(existing, source, hash, chunks, now);
                reindexed.Add(source.RelativePath);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new KnowledgeIngestionResult
        {
            Indexed = indexed,
            Reindexed = reindexed,
            SkippedUnchanged = skipped,
            Missing = resolved.Missing,
        };
    }

    private static KnowledgeDocument CreateDocument(
        ResolvedSource source,
        string hash,
        IReadOnlyList<KnowledgeTextChunk> chunks,
        DateTimeOffset now)
    {
        var document = new KnowledgeDocument
        {
            FileName = Path.GetFileName(source.RelativePath),
            DocumentType = source.DocumentType,
            ContentHash = hash,
            SourceIdentifier = source.RelativePath,
            IndexedAt = now,
            AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
            CreatedAt = now,
        };
        AddChunks(document, chunks, now);
        return document;
    }

    private static void ReplaceDocument(
        KnowledgeDocument existing,
        ResolvedSource source,
        string hash,
        IReadOnlyList<KnowledgeTextChunk> chunks,
        DateTimeOffset now)
    {
        existing.Chunks.Clear();
        existing.FileName = Path.GetFileName(source.RelativePath);
        existing.DocumentType = source.DocumentType;
        existing.ContentHash = hash;
        existing.IndexedAt = now;
        existing.AuthorizationScope = KnowledgeAuthorizationScope.Authenticated;
        existing.SchoolId = null;
        existing.ReportId = null;
        existing.ReportYear = null;
        existing.ReportType = null;
        AddChunks(existing, chunks, now);
    }

    private static void AddChunks(
        KnowledgeDocument document,
        IReadOnlyList<KnowledgeTextChunk> chunks,
        DateTimeOffset now)
    {
        foreach (var chunk in chunks)
        {
            document.Chunks.Add(new KnowledgeChunk
            {
                ChunkNumber = chunk.ChunkNumber,
                Content = chunk.Content,
                RuleId = chunk.RuleId,
                Category = chunk.Category,
                SourceLocation = chunk.SourceLocation,
                Embedding = null,
                EmbeddingModel = null,
                CreatedAt = now,
            });
        }
    }

    private static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }

    private static ResolvedBatch ResolveSources(string root)
    {
        var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var sources = new List<ResolvedSource>();
        var missing = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var sasDirectory = Path.Combine(root, "legacy", "sas");
        if (Directory.Exists(sasDirectory))
        {
            foreach (var fullPath in Directory.GetFiles(sasDirectory, "*.sas")
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                TryAdd(fullPath, KnowledgeDocumentType.Legacy, KnowledgeSourceKind.Sas);
            }
        }

        foreach (var project in KnowledgeSourceCatalog.ProjectDocuments)
        {
            var preferred = CombineUnderRoot(root, project.RelativePath);
            if (preferred is not null && File.Exists(preferred))
            {
                TryAdd(preferred, KnowledgeDocumentType.Project, KnowledgeSourceKind.Markdown);
                continue;
            }

            var fallback = project.FallbackRelativePath is null
                ? null
                : CombineUnderRoot(root, project.FallbackRelativePath);
            if (fallback is not null && File.Exists(fallback))
            {
                TryAdd(fallback, KnowledgeDocumentType.Project, KnowledgeSourceKind.Markdown);
                continue;
            }

            missing.Add(KnowledgeSourceCatalog.Normalize(project.RelativePath));
        }

        return new ResolvedBatch(sources, missing);

        void TryAdd(string fullPath, KnowledgeDocumentType documentType, KnowledgeSourceKind kind)
        {
            var full = Path.GetFullPath(fullPath);
            if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)
                || !File.Exists(full))
            {
                return;
            }

            var relative = KnowledgeSourceCatalog.Normalize(Path.GetRelativePath(root, full));
            if (!KnowledgeSourceCatalog.IsAllowedRelativePath(relative)
                || !seen.Add(relative))
            {
                return;
            }

            sources.Add(new ResolvedSource(relative, full, documentType, kind));
        }
    }

    private static string? CombineUnderRoot(string root, string relativePath)
    {
        var normalized = KnowledgeSourceCatalog.Normalize(relativePath);
        if (!KnowledgeSourceCatalog.IsAllowedRelativePath(normalized)
            || normalized.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        return Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
    }

    private sealed record ResolvedSource(
        string RelativePath,
        string FullPath,
        KnowledgeDocumentType DocumentType,
        KnowledgeSourceKind Kind);

    private sealed record ResolvedBatch(
        IReadOnlyList<ResolvedSource> Sources,
        IReadOnlyList<string> Missing);
}
