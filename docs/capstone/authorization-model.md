# Authorization model

This document describes role policies and school-level report access. It does not claim that generated PDFs are accessible. Report calculations stay in deterministic C#.

## Roles and policies

Policy names live in `AppPolicies`. Role mapping lives only in `AppAuthorizationPolicies`.

| Policy | Roles | Surfaces |
|---|---|---|
| `RequireAdmin` | Admin | Import, Generate All |
| `RequireReportGeneration` | Admin, ReportUser | Generate Report |
| `RequireReportAccess` | Admin, ReportUser, Viewer | Dashboard, Run History, downloads |
| `RequireRagAccess` | Admin, ReportUser, Viewer | Knowledge Assistant (`/knowledge-assistant`) |

Authentication is required by default. Anonymous pages are explicit.

## Resource access: `UserSchoolAccess`

Non-admin users may use a school only when a row exists.

| Field | Meaning |
|---|---|
| `UserId` | ASP.NET Core Identity user id |
| `SchoolId` | Assigned school |
| `AccessLevel` | `View` or `Generate` (`Generate` includes view) |
| `CreatedAt` | UTC timestamp |

Admin does **not** need a row. Admin may access every school and every stored report.

| Caller | School A assigned | School B not assigned |
|---|---|---|
| Admin | view + generate | view + generate |
| ReportUser with `Generate` on A | view + generate A | denied |
| Viewer with `View` on B | denied | view B only |

## Service checks

`IReportAuthorizationService` is the application-layer gate. UI hiding is not sufficient.

- `CanAccessSchoolAsync(user, school)`
- `CanViewReportAsync(user, report)` / `CanViewReportAsync(user, reportId)`
- `CanGenerateReportAsync(user, school)`

Unknown or unauthorized report ids return **false**. Downloads respond **404**, not 200. Generation returns a failed run with “Not authorized” and does not write a PDF.

## PDF downloads

Generated PDFs are stored under the configured `OutputRoot` (the Web `output/` folder), not in SQLite and not in `wwwroot`. `UseStaticFiles` serves `wwwroot` only.

`IReportDownloadService` is the only read path. Before the file is opened:

1. Authenticate the caller.
2. Load `ReportRunItem` metadata by integer id.
3. Resolve the associated `School`.
4. Call `IReportAuthorizationService.CanViewReportAsync`.
5. Deny if unauthorized.
6. Resolve the file from the stored `OutputPath` and configured `OutputRoot` only.
7. Return the PDF.

The request may include a decorative file name. That value is ignored. Stored paths that contain `..` or fall outside `OutputRoot` are rejected. Missing, deleted, unknown, and unauthorized reports all return **404** with no report metadata and no physical directory path.

## Knowledge persistence

`KnowledgeDocument` and `KnowledgeChunk` store indexed text and a source reference (`SourceIdentifier`). SQLite does **not** store PDF binaries.

| `AuthorizationScope` | Meaning |
|---|---|
| `Authenticated` | Legacy/global knowledge. Callers with `RequireRagAccess` may retrieve it. |
| `School` | Restricted to callers who can access `SchoolId`. |
| `Report` | Restricted to callers who can access the associated school/report. |
| `Admin` | Restricted to Admin. Viewers and report users cannot retrieve it. |

`KnowledgeAccess.WhereAccessible` applies that filter **before** similarity ranking or any LLM call. `IKnowledgeAssistantService` retrieves authorized top-K hits, builds a grounded prompt that treats retrieved text as untrusted data, then calls `ILanguageModelService`. `IEmbeddingService` uses the same rule (`EmbeddingAccess`) before any chunk text is sent to an external embedding provider.

Legacy SAS and listed project markdown files are indexed by `IKnowledgeIngestionService`. Generated school PDFs are indexed by `IPdfKnowledgeIngestionService` with `AuthorizationScope=Report`. See `docs/capstone/generated-pdf-rag.md`. Ingestion is read-only, SHA-256 incremental, and does not store embeddings or graduate records.

## Where it is enforced

- `ReportGenerationService` before calculating or writing a PDF
- `IReportDownloadService` / `/downloads/reports/{id}` before opening file bytes
- Generate Report school list and Run History items (filter only)
- Knowledge Assistant (`/knowledge-assistant`) before retrieval and the language-model call
- Report details (`/reports/{id}`) and report-scoped assistant context via `CanViewReportAsync` before any report chunks are searched

There is no grant-management UI in this slice. Rows can be inserted in SQLite for local testing.

## Out of scope

- Chat features beyond the Knowledge Assistant page
- Calculator or PDF layout changes
- Per-report sharing that is not school-based
