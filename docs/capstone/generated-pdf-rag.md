# Generated PDF RAG indexing

This document describes how a successfully generated school PDF becomes eligible for later RAG retrieval. It does not claim that generated PDFs are accessible.

## When indexing runs

`ReportGenerationService` writes the PDF to `OutputRoot` and stores `ReportRunItem.OutputPath`. After a **completed** item is saved, it calls `IPdfKnowledgeIngestionService`.

On non-`Testing` startup, `KnowledgeStartup` also backfills completed `ReportRunItem` PDFs that are missing or still using the older chunk format. It does **not** scan files that have no completed run item.

Indexing failure does not delete the PDF and does not fail generation.

## What is stored

SQLite stores metadata and extracted **text chunks** only.

| Field | Source |
|---|---|
| `DocumentType` | `GeneratedReport` |
| `AuthorizationScope` | `Report` |
| `SchoolId` / `SchoolCode` | The generated item’s school |
| `ReportId` | `ReportRunItem.Id` |
| `ReportRunId` | The parent run, when present |
| `ReportYear` | Class year from the output path (`{year}/{schoolCode}/summary-report.pdf`) |
| `ReportType` | `Summary` |
| `SourceIdentifier` | Stored output path (reference only) |
| `ContentHash` | SHA-256 of the PDF file bytes |
| `KnowledgeChunk.SourceLocation` | `page N` (and line range when a page is split) |
| `KnowledgeChunk.Content` | Extracted page text with a `[School {code}, Class of {year}, page N]` prefix for school/year context during retrieval |

There is no PDF blob column. Vectors are produced later by `IKnowledgeEmbeddingIndexService` (see `docs/capstone/external-rag-api.md`), only for chunks the caller may send to the configured provider. Report generation does not wait on embeddings.

Pages exceeding the chunk limits are split with a two-line overlap. Markdown tables retain their column header in every row chunk, so a retrieved cell remains labeled with the field it belongs to. `ChunkFormatVersion` triggers existing source and PDF chunks to be rebuilt when the chunking format changes.

## File handling

- The service opens the PDF **read-only**.
- Path resolution uses stored `OutputPath` plus configured `OutputRoot` (`ReportFileAccess`). Request paths are not trusted.
- The PDF is never rewritten, moved, or deleted by indexing.

## Incremental behavior

| Situation | Result |
|---|---|
| Same `ReportId` and same SHA-256 | Skip (`SkippedDuplicate`) |
| Same `ReportId`, different SHA-256 | Replace chunks (`Reindexed`) |
| Missing file | `MissingPdf` |
| Not a PDF | `InvalidPdf` |
| Opened but text extraction failed | `ExtractionFailed` |

## Authorization

Generated PDF chunks inherit the report’s school boundary.

- Scope is `Report`, not `Authenticated`.
- Retrieval uses `KnowledgeAccess` / `IReportAuthorizationService` before scoring or calling an LLM.
- A caller who may view School A must not receive School B chunks. Generated PDF chunks are `Report` scope and also require `CanViewReportAsync`.

Legacy/project documents remain globally `Authenticated`. Those rows are a different document type.

## Out of scope

- Retrieval / chat UI
- Changing calculator or PDF layout
- Indexing graduate worksheets or raw student rows
