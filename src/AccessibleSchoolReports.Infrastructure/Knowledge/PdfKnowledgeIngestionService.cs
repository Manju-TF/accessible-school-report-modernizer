using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Infrastructure.Knowledge;

public sealed class PdfKnowledgeIngestionService : IPdfKnowledgeIngestionService
{
    private readonly IDbContextFactory<SchoolReportsDbContext> _dbFactory;
    private readonly IPdfTextExtractor _extractor;
    private readonly ReportGenerationOptions _options;

    public PdfKnowledgeIngestionService(
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        IPdfTextExtractor extractor,
        IOptions<ReportGenerationOptions> options)
    {
        _dbFactory = dbFactory;
        _extractor = extractor;
        _options = options.Value;
    }

    public async Task<PdfKnowledgeIngestionResult> IndexGeneratedReportAsync(
        GeneratedPdfKnowledgeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ReportRunItemId <= 0 || request.SchoolId <= 0)
        {
            return PdfKnowledgeIngestionResult.From(PdfKnowledgeIngestionStatus.Rejected, message: "Report metadata is incomplete.");
        }

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var item = await db.ReportRunItems
            .AsNoTracking()
            .Include(row => row.School)
            .FirstOrDefaultAsync(row => row.Id == request.ReportRunItemId, cancellationToken);
        if (item is null
            || item.Status != RunStatus.Completed
            || item.SchoolId != request.SchoolId
            || string.IsNullOrWhiteSpace(item.OutputPath))
        {
            return PdfKnowledgeIngestionResult.From(PdfKnowledgeIngestionStatus.Rejected, message: "Generated report metadata was not found.");
        }

        if (!ReportFileAccess.TryResolveDownloadPath(item.OutputPath, _options.OutputRoot, out var path))
        {
            return PdfKnowledgeIngestionResult.From(PdfKnowledgeIngestionStatus.MissingPdf, message: "The generated PDF was not found.");
        }

        byte[] hashBytes;
        await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            hashBytes = await ReadAllBytesAsync(stream, cancellationToken);
        }

        var hash = KnowledgeContentHash.Sha256Hex(hashBytes);
        var existing = await db.KnowledgeDocuments
            .Include(document => document.Chunks)
            .FirstOrDefaultAsync(document => document.ReportId == item.Id, cancellationToken);
        if (existing is not null && existing.ContentHash == hash)
        {
            return PdfKnowledgeIngestionResult.From(
                PdfKnowledgeIngestionStatus.SkippedDuplicate,
                existing.Id,
                "This generated PDF is already indexed.");
        }

        PdfTextExtractionResult extracted;
        await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            extracted = _extractor.Extract(stream);
        }

        if (extracted.Status == PdfTextExtractionStatus.InvalidPdf)
        {
            return PdfKnowledgeIngestionResult.From(PdfKnowledgeIngestionStatus.InvalidPdf, message: extracted.Message);
        }

        if (extracted.Status != PdfTextExtractionStatus.Succeeded)
        {
            return PdfKnowledgeIngestionResult.From(PdfKnowledgeIngestionStatus.ExtractionFailed, message: extracted.Message);
        }

        var chunks = KnowledgeTextChunker.ChunkPages(extracted.Pages);
        if (chunks.Count == 0)
        {
            return PdfKnowledgeIngestionResult.From(
                PdfKnowledgeIngestionStatus.ExtractionFailed,
                message: "No indexable text was produced from the generated PDF.");
        }

        var now = DateTimeOffset.UtcNow;
        var schoolCode = item.School.Code;
        var reportYear = request.ReportYear
            ?? (int.TryParse(_options.ClassYear, out var year) ? year : 2025);
        var reportType = string.IsNullOrWhiteSpace(request.ReportType)
            ? GeneratedPdfKnowledgeRequest.DefaultReportType
            : request.ReportType.Trim();

        if (existing is null)
        {
            var document = new KnowledgeDocument
            {
                FileName = $"{schoolCode}-summary-report.pdf",
                DocumentType = KnowledgeDocumentType.GeneratedReport,
                ContentHash = hash,
                SourceIdentifier = item.OutputPath,
                IndexedAt = now,
                SchoolId = item.SchoolId,
                SchoolCode = schoolCode,
                ReportId = item.Id,
                ReportRunId = item.ReportRunId,
                ReportYear = reportYear,
                ReportType = reportType,
                AuthorizationScope = KnowledgeAuthorizationScope.Report,
                CreatedAt = now,
            };
            AddChunks(document, chunks, now);
            db.KnowledgeDocuments.Add(document);
            await db.SaveChangesAsync(cancellationToken);
            return PdfKnowledgeIngestionResult.From(PdfKnowledgeIngestionStatus.Indexed, document.Id);
        }

        db.KnowledgeChunks.RemoveRange(existing.Chunks);
        existing.Chunks.Clear();
        existing.FileName = $"{schoolCode}-summary-report.pdf";
        existing.DocumentType = KnowledgeDocumentType.GeneratedReport;
        existing.ContentHash = hash;
        existing.SourceIdentifier = item.OutputPath;
        existing.IndexedAt = now;
        existing.SchoolId = item.SchoolId;
        existing.SchoolCode = schoolCode;
        existing.ReportId = item.Id;
        existing.ReportRunId = item.ReportRunId;
        existing.ReportYear = reportYear;
        existing.ReportType = reportType;
        existing.AuthorizationScope = KnowledgeAuthorizationScope.Report;
        AddChunks(existing, chunks, now);
        await db.SaveChangesAsync(cancellationToken);
        return PdfKnowledgeIngestionResult.From(PdfKnowledgeIngestionStatus.Reindexed, existing.Id);
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

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }
}
