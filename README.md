# Accessible School Report Modernizer

A capstone that replaces a read-only SAS school-employment reporting pipeline with a local **.NET 8** application. The app imports a graduate Excel workbook, applies **characterized** SAS calculation rules, stores data in **SQLite**, and writes seven-page letter PDFs that **target** tagged PDF / PDF-UA structure while keeping the GrayscalePrinter layout.

The running UI is branded **Meridian Test Client**. Generated PDFs and chrome do not print NALP or ABA names.

`/legacy` is the immutable characterization baseline. It is never edited.

This repository is an **AI-assisted modernization**. Cursor agents generated plans and code. Humans reviewed, corrected, and rejected output. Report totals come from deterministic C#, not from the Knowledge Assistant.

---

## Table of contents

1. [Project goals](#project-goals)
2. [Problem statement](#problem-statement)
3. [Legacy system](#legacy-system)
4. [Modernized architecture](#modernized-architecture)
5. [Technology stack](#technology-stack)
6. [Solution structure](#solution-structure)
7. [Business-rule modernization](#business-rule-modernization)
8. [Characterization testing and parity](#characterization-testing-and-parity)
9. [Data flow](#data-flow)
10. [SQLite](#sqlite)
11. [Excel import](#excel-import)
12. [Report generation](#report-generation)
13. [Accessible PDF generation](#accessible-pdf-generation)
14. [Authentication and authorization](#authentication-and-authorization)
15. [Secure PDF access](#secure-pdf-access)
16. [Legacy Knowledge Assistant / RAG](#legacy-knowledge-assistant--rag)
17. [RAG security](#rag-security)
18. [Knowledge Assistant UI](#knowledge-assistant-ui)
19. [Report-specific assistant](#report-specific-assistant)
20. [AI-assisted development](#ai-assisted-development)
21. [Cursor configuration](#cursor-configuration)
22. [MCP usage](#mcp-usage)
23. [Testing strategy](#testing-strategy)
24. [Security test coverage](#security-test-coverage)
25. [Accessibility testing](#accessibility-testing)
26. [CI/CD and quality gates](#cicd-and-quality-gates)
27. [Git workflow](#git-workflow)
28. [Human review](#human-review--reviewed-not-abdicated)
29. [Setup and local run](#setup-and-local-run)
30. [Configuration](#configuration)
31. [Demo walkthrough](#demo--capstone-walkthrough)
32. [Traceability matrix](#traceability-matrix)
33. [Definition of done](#capstone-definition-of-done)
34. [Performance](#performance)
35. [Security considerations](#security-considerations)
36. [Limitations, assumptions, and risks](#limitations-assumptions-and-risks)
37. [Future enhancements](#future-enhancements)
38. [Documentation index](#documentation-index)

---

## Architecture at a glance

```text
Legacy SAS
    ↓
Business-rule extraction (docs/capstone)
    ↓
Characterization tests
    ↓
.NET Domain / Application logic
    ↓
SQLite (data/schoolreports.db)
    ↓
SchoolReportCalculator
    ↓
QuestPDF (tagged-PDF target)
    ↓
Blazor Server UI + authorized download
```

Knowledge Assistant (supporting feature, not the calculator):

```text
Legacy SAS / project docs / generated reports
                ↓
        Document ingestion
                ↓
             Chunking
                ↓
     Lexical embeddings (default)
                ↓
        Authorization-aware retrieval
                ↓
        External LLM API (chat)
                ↓
       Cited answer or insufficient evidence
```

---

## Project goals

| Goal | Status |
|---|---|
| Replace SAS reporting logic with testable C# | Implemented |
| Preserve characterized SAS behavior, including ugly rules | Implemented |
| SQLite as the only database | Implemented |
| Excel import with validation | Implemented |
| Single-school and all-school PDF generation | Implemented |
| Sequential and bounded-parallel generate-all | Implemented |
| Blazor Server UI | Implemented |
| ASP.NET Core Identity + school-level authorization | Implemented |
| Protected PDF downloads | Implemented |
| Authorization-aware Knowledge Assistant | Implemented |
| Traceability SAS → Rule ID → C# → test | Implemented |
| AI-assisted build with human review | Implemented |
| Automated quality gate on `main` and pull requests | Implemented |
| Formal PDF/UA or WCAG certification | **Not implemented** — targeting and reviews only |
| Automated Playwright test project / CI job | **Not implemented** — Playwright MCP used in Cursor sessions |

---

## Problem statement

The legacy workflow is two SAS 9.4 programs plus an Excel extract. A reporting shop preprocesses graduate rows, computes counts and salary statistics, and emits one PDF per school.

That pipeline is hard to run, hard to test, and hard to use with assistive technology:

- SAS must be installed and pointed at local paths.
- Business rules live in `DATA` / `PROC` steps, not in automated tests.
- The ODS PDF (`STYLE=GrayscalePrinter`) is a print layout. Characterization notes that only the first school’s `ods pdf` statement used the `accessible` option (`SS-PAGE-05`); the rest omit it.
- The preserved baseline PDF is a **Test University** artifact. It is useful for layout and value mapping. It is not an accessibility certificate, and it is not the same population as the sample Excel workbook.

This project modernizes the **workflow** without inventing rules. Salary suppression stays `n ge 5` on non-missing full-time long-term salaries (`CF-S-00`), not on headcount.

---

## Legacy system

```text
Excel extract
    → SAS preprocessing (createschrptfiles2025.sas)
    → SAS summary / PROC REPORT (schreptsummary_2025.sas)
    → ODS PDF (GrayscalePrinter) — not a tagged-PDF product
```

The files in this repository (not parenthetical copies) are:

| Artifact | Role |
|---|---|
| [`legacy/samples/sample-export.xlsx`](legacy/samples/sample-export.xlsx) | Original graduate extract used as the import sample |
| [`legacy/sas/createschrptfiles2025.sas`](legacy/sas/createschrptfiles2025.sas) | Builds school counts, salaries, and combined report files |
| [`legacy/sas/schreptsummary_2025.sas`](legacy/sas/schreptsummary_2025.sas) | Formats seven `PROC REPORT` pages and writes the PDF |
| [`legacy/baseline/test-school-report.pdf`](legacy/baseline/test-school-report.pdf) | Immutable visual/value baseline (Class of 2024 / July 2025 chrome on that file) |

Characterization: [`docs/capstone/createschrptfiles-analysis.md`](docs/capstone/createschrptfiles-analysis.md), [`docs/capstone/schreptsummary-analysis.md`](docs/capstone/schreptsummary-analysis.md), [`docs/capstone/business-rules.md`](docs/capstone/business-rules.md), [`docs/capstone/report-map.md`](docs/capstone/report-map.md). Ambiguous SAS is recorded as skipped tests, not guessed.

| Legacy component | Purpose | Modern equivalent |
|---|---|---|
| `createschrptfiles2025.sas` | Recodes, frequencies, salary univariate, merge | `LegacyRecodes`, `SchoolReportCalculator`, `SasUnivariate` |
| `schreptsummary_2025.sas` | Seven-page `PROC REPORT`, labels, note, footer | `SchoolReportLayout`, `SchoolReportPresentation`, `QuestPdfAccessiblePdfGenerator` |
| `sample-export.xlsx` | Graduate extract | ClosedXML import → `GraduateRecord` / `School` |
| `test-school-report.pdf` | Layout and printed-value baseline | Generated `{schoolCode}-summary-report.pdf` (layout target; different population) |

---

## Modernized architecture

The corrected plan in [`docs/architecture/corrected-plan.md`](docs/architecture/corrected-plan.md) originally rejected a four-project split. The repository **did** grow into four class libraries plus three test projects. The layers below describe what is in source today.

```text
AccessibleSchoolReports.Web
        ↓
AccessibleSchoolReports.Application
        ↓
AccessibleSchoolReports.Domain
        ↑
AccessibleSchoolReports.Infrastructure
        ↓
SQLite  +  output PDFs  +  optional OpenAI-compatible HTTP
```

### Domain (`src/AccessibleSchoolReports.Domain`)

Entities: `School`, `GraduateRecord`, `ImportRun`, `ImportRowIssue`, `ReportRun`, `ReportRunItem`, `UserSchoolAccess`, `KnowledgeDocument`, `KnowledgeChunk`.

Supporting types: `LegacyRecodes`, `LegacySchoolNames`, `SasUnivariate`, `SalaryStatistics`, `SchoolAccessLevel`, `KnowledgeAuthorizationScope`, `RunStatus`, `ReportGenerationMode`.

### Application (`src/AccessibleSchoolReports.Application`)

Use-case contracts and deterministic report logic:

- Import: `IGraduateImportService`, header/column helpers
- Reporting: `ISchoolReportCalculator`, `SchoolReportCalculator`, `SchoolReportLayout`, `SchoolReportPresentation`, `IReportGenerationService`, `IAccessiblePdfGenerator`, `ReportFileAccess`
- Security: `AppRoles`, `AppPolicies`, `AppAuthorizationPolicies`, `IReportAuthorizationService`, `ICurrentUserAccessor`
- Knowledge: retrieval/ingestion/embedding/LLM interfaces, `KnowledgeAccess`, `KnowledgeSourceCatalog`, `KnowledgeGroundedPrompt`, `HashedLexicalVector`

The PDF layer does not recompute business statistics. Presentation tests use a hand-built `SchoolReport` fixture so a calculator change cannot silently rewrite labels.

### Infrastructure (`src/AccessibleSchoolReports.Infrastructure`)

- EF Core + SQLite (`SchoolReportsDbContext`, migrations)
- ClosedXML (`ExcelGraduateImportService`, `ExcelGraduateWorkbookParser`)
- QuestPDF (`QuestPdfAccessiblePdfGenerator`)
- Report orchestration (`ReportGenerationService`)
- Knowledge ingest, retrieval, assistant, lexical/OpenAI embeddings, OpenAI-compatible chat
- Identity seed helpers and `ReportAuthorizationService`

### Web (`src/AccessibleSchoolReports.Web`)

Blazor Server composition root. Pages, cookie Identity, download endpoints, `KnowledgeStartup` on boot.

| Route | Policy | Purpose |
|---|---|---|
| `/signin` | Anonymous | Local Identity sign-in |
| `/` | `RequireReportAccess` | Dashboard |
| `/import` | `RequireAdmin` | Excel import |
| `/generate` | `RequireReportGeneration` | One school PDF |
| `/generate-all` | `RequireAdmin` | Sequential or bounded-parallel all-school |
| `/runs` | `RequireReportAccess` | Run history |
| `/reports/{id}` | `RequireReportAccess` | Report details + “Ask about this report” |
| `/knowledge-assistant` | `RequireRagAccess` | Knowledge Assistant |
| `/denied` | Authenticated | Access denied |
| `/downloads/reports/{id}` | `RequireReportAccess` + resource check | PDF download |

---

## Technology stack

| Technology | Purpose in this repo |
|---|---|
| .NET 8 / C# | All source and test projects |
| ASP.NET Core + Blazor Server | UI and download endpoints |
| Entity Framework Core | SQLite persistence and migrations |
| SQLite | Required MVP database |
| ASP.NET Core Identity | Local accounts, password hashing, cookie `.asr.auth` |
| ClosedXML | Excel `.xlsx` import |
| QuestPDF | Tagged-PDF / PDF-UA-oriented generation |
| PdfPig | Text extraction for generated-PDF knowledge ingest |
| xUnit | Characterization, unit, and integration tests |
| Git / GitHub | Source control and pull requests |
| GitHub Actions | `.github/workflows/quality.yml` |
| Cursor | AI-assisted development against `.cursor/rules/` |
| GitHub MCP | PRs and repository operations (token not committed) |
| Playwright MCP | Cursor-session browser review only — **no** Playwright test project |
| SQLite MCP | Read-only inspect script for `data/schoolreports.db` |
| OpenAI-compatible HTTP | Optional chat completions; optional remote embeddings |
| Hashed lexical vectors | Default RAG embeddings (`Lexical` / `hashed-bow`) |

Node.js is used only for the optional SQLite MCP script, not by the web app.

---

## Solution structure

```text
AccessibleSchoolReportModernizer/
├── .cursor/                 Project rules + mcp.json
├── .github/workflows/       quality.yml
├── docs/                    Design and characterization
├── evidence/                Observed test and review output
├── legacy/                  Immutable SAS, sample Excel, baseline PDF
├── scripts/                 Legacy integrity + read-only SQLite MCP
├── src/
│   ├── AccessibleSchoolReports.Domain/
│   ├── AccessibleSchoolReports.Application/
│   ├── AccessibleSchoolReports.Infrastructure/
│   └── AccessibleSchoolReports.Web/
├── tests/
│   ├── AccessibleSchoolReports.CharacterizationTests/
│   ├── AccessibleSchoolReports.UnitTests/
│   └── AccessibleSchoolReports.IntegrationTests/
├── data/                    Working SQLite (gitignored) + sample-import.xlsx
├── AccessibleSchoolReports.sln
└── README.md
```

Generated PDFs are written under `src/AccessibleSchoolReports.Web/output/{year}/{schoolCode}/summary-report.pdf`, not under a repository-root `output/` folder.

---

## Business-rule modernization

```text
Legacy SAS
    ↓
Identified Rule ID (CF-* / SS-*)
    ↓
docs/capstone/business-rules.md
    ↓
C# (LegacyRecodes, SchoolReportCalculator, layout/presentation)
    ↓
Characterization + unit test
    ↓
Generated report
```

The catalogue is [`docs/capstone/business-rules.md`](docs/capstone/business-rules.md). Rule IDs were extracted from the two SAS programs. They were not invented for cleaner code. There is no `SALARY-004` or `EMP-001` identifier in this repository.

| Rule ID | Legacy source | Business rule | Modern implementation | Test |
|---|---|---|---|---|
| CF-S-00 | `createschrptfiles2025.sas` | Keep salary univariate only when `n ge 5` on non-missing `salftperm` | `SasUnivariate.SuppressUnlessEligible`, `SchoolReportCalculator` | `Salary_IsSuppressedWhenNIsBelow5`, `SalarySuppressionTests.SalaryRow_KeptOnlyWhenNGe5` |
| SS-SUP-01 | `schreptsummary_2025.sas` | Printed note: at least five salaries | Same threshold as CF-S-00, not a second cutoff | `ReportNote_StatesTheSameFiveSalaryRule_NotASecondThreshold` |
| CF-PREP-06 | `createschrptfiles2025.sas` | `sex3`: `W`→`F`, `X`→`N`, `ND`→ blank | `LegacyRecodes` | Characterization + `LegacyRecodesTests` |
| CF-C-01 | `createschrptfiles2025.sas` | Total reported = frequency of school `code` | `SchoolReportCalculator` | Calculator + gender/count characterization |
| SS-FIL-01…07 | `schreptsummary_2025.sas` | Seven page filters on `ANALVAR` | `SchoolReportLayout` | `SchoolReportLayoutTests` |

Eighteen characterization cases stay **skipped** because SAS is ambiguous (filters, formats, title-year differences). Schools `53404` and `54703` have no `%SCHRPTS` name; the app does not invent names.

---

## Characterization testing and parity

Characterization tests lock **observed** SAS maps in `LegacyRules` and printed baseline values. That project references **Domain only**. It does **not** run `SchoolReportCalculator`. Live calculator coverage is in unit tests.

Parity against the Test University baseline PDF is **not** a pass. `LegacyModernParityTests` compares that PDF’s totals (100 graduates) to a school from the sample workbook (for example `23306`, 31 graduates). That is a subject mismatch. Do not change the calculator to chase those numbers. Recorded write-up: [`evidence/test-results/parity-results.md`](evidence/test-results/parity-results.md).

```powershell
dotnet test tests/AccessibleSchoolReports.CharacterizationTests
dotnet test tests/AccessibleSchoolReports.UnitTests
dotnet test tests/AccessibleSchoolReports.IntegrationTests --filter "FullyQualifiedName!~LegacyModernParityTests"
```

A solution run recorded on 4 September 2026 in [`evidence/test-results/final-quality-report.md`](evidence/test-results/final-quality-report.md): **322 passed**, **1 failed** (parity), **19 skipped**. Later commits added more unit tests; treat those totals as that report’s snapshot.

---

## Data flow

```text
Excel .xlsx
    ↓
Header / row validation (ClosedXML)
    ↓
ImportRun + GraduateRecord + School
    ↓
SQLite
    ↓
Authorized school selection
    ↓
SchoolReportCalculator
    ↓
SchoolReport + layout slices
    ↓
QuestPDF
    ↓
output/{year}/{schoolCode}/summary-report.pdf
    ↓
Authorized /downloads/reports/{id}
```

After a completed PDF, `IPdfKnowledgeIngestionService` extracts text. Embedding is a separate pass and must not fail generation.

---

## SQLite

SQLite is the **required** MVP database (`.cursor/rules/30-mvp-constraints.mdc`). The capstone does not introduce SQL Server or another licensed server database.

| Item | Value |
|---|---|
| Connection | `ConnectionStrings:SchoolReports` = `Data Source=data/schoolreports.db` |
| Resolved path | Repository-root `data/schoolreports.db` via `SqliteConnectionString.FindRepositoryRoot` |
| Context | `SchoolReportsDbContext` : `IdentityDbContext<IdentityUser>` |
| Startup | `MigrateAsync` then Identity role seed |
| Migrations | Under `src/AccessibleSchoolReports.Infrastructure/Persistence/Migrations/` |

The working database file is gitignored. Inspect it with any SQLite viewer, or with the read-only MCP script `scripts/mcp-sqlite-readonly.mjs`. Do not commit a database that contains secrets.

---

## Excel import

Implemented by `ExcelGraduateImportService` / `ExcelGraduateWorkbookParser` (`IGraduateImportService`).

- Accepts `.xlsx` only (UI maximum 10 MB).
- Validates required columns (`GraduateImportColumns` / `ExcelHeaderNormalizer`).
- Records invalid and blank rows on the `ImportRun` without inventing values.
- Rejects a duplicate file content hash (`WasDuplicate`).
- Persists `School` and `GraduateRecord` rows.
- Admin-only page: `/import`.

A working copy may live at `data/sample-import.xlsx`. Do not save edits back into `/legacy`.

---

## Report generation

`ReportGenerationService` implements `IReportGenerationService`.

### Single report (`/generate`)

One authorized school, class year folder (default `2025`), one `ReportRun` with `ReportGenerationMode.Single`, one seven-page PDF.

### Generate all (`/generate-all`, Admin)

Every eligible school. Mode is sequential or bounded parallel.

### Sequential

`ReportGenerationMode.Sequential` — one school at a time.

### Bounded parallel

`ReportGenerationMode.BoundedParallel`. `ReportGeneration:DefaultMaxParallelism` defaults to **4** and is clamped **1–8**. Each school is isolated: one failure does not delete other PDFs. Integration tests compare sequential and parallel **extracted PDF text**, not bytes, and persist `DurationMilliseconds`.

Unauthorized generate attempts return a failed run (“Not authorized”) and do not write a PDF.

---

## Accessible PDF generation

`QuestPdfAccessiblePdfGenerator` **targets** tagged PDF / PDF-UA-oriented structure behind the SAS GrayscalePrinter design. Accessibility work is tags, language, reading order, and alternative text — not a new look.

Preserved layout (see `.cursor/rules/60-pdf-visual-parity.mdc`):

- Seven letter pages, one table per page
- Centered Bold Italic school name and report title
- Gray header cells, black grid, 8 pt body / 9 pt headers
- `Total Reported` as a table row under the headers on page 1
- Note, then test-client footer, in content flow
- Missing / suppressed values as `.` (screen-reader name: Not displayed)
- Year chrome from the 2025 SAS program: **Class of 2025**, July **2026** (not the baseline artifact’s 2024 / July 2025)

Unit tests assert structure markers (`/StructTreeRoot`, `/Lang`, `pdfuaid`). That is **not** a veraPDF, PAC, or screen-reader pass.

> The application is designed to produce accessibility-oriented / tagged PDF output and includes automated structure checks where implemented. Formal compliance remains subject to validation with PDF accessibility tools. Do not claim “the PDF is accessible.”

Strategy: [`docs/accessibility/pdf-accessibility-strategy.md`](docs/accessibility/pdf-accessibility-strategy.md).

---

## Authentication and authorization

There is **no** Microsoft Entra dependency, **no** OAuth, and **no** application JWT login. The language-model API key is not an identity token.

```text
ASP.NET Core Identity
        ↓
Cookie .asr.auth
        ↓
Roles (Admin / ReportUser / Viewer)
        ↓
Policies (AppPolicies)
        ↓
UserSchoolAccess
        ↓
IReportAuthorizationService
```

### Authentication

- `IdentityUser` in the same SQLite database
- Passwords hashed by Identity (`PasswordHasher<IdentityUser>`)
- Cookie `.asr.auth`: HttpOnly, SameSite Lax, Secure on HTTPS, sliding 8 hours, not persistent
- POST `/account/signin` and `/account/signout` with antiforgery
- Local `returnUrl` only
- Details: [`docs/capstone/authentication.md`](docs/capstone/authentication.md)

### Roles and policies

| Policy | Roles | Surfaces |
|---|---|---|
| `RequireAdmin` | Admin | Import, Generate All |
| `RequireReportGeneration` | Admin, ReportUser | Generate Report |
| `RequireReportAccess` | Admin, ReportUser, Viewer | Dashboard, Run History, downloads, report details |
| `RequireRagAccess` | Admin, ReportUser, Viewer | Knowledge Assistant |

Default fallback policy requires an authenticated user. Anonymous pages are explicit.

### School-level authorization

```text
User
 ↓
UserSchoolAccess (View or Generate)
 ↓
School
 ↓
ReportRunItem / PDF
```

Admin does not need a grant row. UI hiding is not sufficient; `IReportAuthorizationService` is checked in generation, downloads, report details, and RAG.

---

## Secure PDF access

PDFs live under configured `OutputRoot` (Web `output/`), not in SQLite and not in `wwwroot`. `UseStaticFiles` serves `wwwroot` only.

```text
Authenticate
 → load ReportRunItem by integer id
 → CanViewReportAsync
 → resolve stored OutputPath under OutputRoot only
 → return PDF or 404
```

Implemented protections:

- Unauthorized or unknown ids → **404** (no metadata, no physical path)
- Decorative `{fileName}` in the URL is ignored
- Stored paths with `..` or outside `OutputRoot` are rejected
- Path-traversal query/route variants are tested and denied
- Missing or deleted files → 404

---

## Legacy Knowledge Assistant / RAG

RAG is a **supporting knowledge** feature. It is **not** the source of truth for report totals. The assistant must not invent rules or calculate statistics.

```text
Catalog + generated PDFs
      ↓
Ingestion (text + SHA-256)
      ↓
Chunking (Rule IDs when present)
      ↓
Embeddings (default: lexical hashed-bow)
      ↓
Authorization-aware retrieval
      ↓
Grounded prompt (untrusted document data)
      ↓
ILanguageModelService
      ↓
Answer + sources, or insufficient evidence
```

Indexed catalog (`KnowledgeSourceCatalog`):

- `legacy/sas/*.sas`
- `docs/capstone/business-rules.md`
- `docs/capstone/createschrptfiles-analysis.md`
- `docs/capstone/schreptsummary-analysis.md`
- `docs/capstone/report-map.md`
- `docs/accessibility/pdf-accessibility-strategy.md` (fallback path)
- `docs/architecture/corrected-plan.md` (fallback path)
- `README.md`

Generated school PDFs are indexed after a successful generate (`AuthorizationScope=Report`). Excel files, `data/`, and `evidence/` are not ingested.

Default embeddings are **local lexical** so Ask does not require hundreds of remote embedding calls. `LanguageModel:ApiKey` is used for chat. See [`docs/capstone/external-rag-api.md`](docs/capstone/external-rag-api.md) and [`docs/capstone/generated-pdf-rag.md`](docs/capstone/generated-pdf-rag.md).

On startup (except `Testing`), `KnowledgeStartup` ingests the catalog and indexes pending embeddings. Restart the web app after changing secrets.

---

## RAG security

```text
User
 ↓
Authenticate + RequireRagAccess
 ↓
KnowledgeAccess filter
 ↓
Authorized chunks only
 ↓
Similarity / top-K
 ↓
Prompt (BEGIN/END UNTRUSTED PROJECT DATA)
 ↓
External LLM
```

Implemented controls:

- Unauthorized school or report chunks are not candidates and are not sent to the LLM
- Query-string `report` and session report id are re-checked with `CanViewReportAsync`
- Failed report lookup: “That report is not available.” — no school metadata
- API keys stay in user secrets / environment; empty in committed `appsettings.json`
- Prompt injection in SAS/markdown/PDF is treated as untrusted data (`KnowledgeGroundedPromptTests`)
- Empty retrieval shows **Insufficient evidence** (the UI does not present an ungrounded answer)
- Observed 11-case evaluation (lexical embeddings, fake LLM, School B leak check): [`evidence/test-results/rag-evaluation.md`](evidence/test-results/rag-evaluation.md)

---

## Knowledge Assistant UI

- Route: `/knowledge-assistant`
- Policy: `RequireRagAccess`
- Labeled question field, suggested questions, Ask / Cancel
- `aria-live` status (`LiveStatus`)
- Answer + sources (document name, kind, Rule ID, source location; school code / year when present)
- Insufficient-evidence heading when retrieval is empty
- Keyboard focus moves to the result heading
- Does not display API keys, embeddings, chunk ids, or filesystem paths

UI accessibility findings (not a WCAG pass): [`docs/accessibility/ui-accessibility-review.md`](docs/accessibility/ui-accessibility-review.md).

---

## Report-specific assistant

```text
/reports/{id}
     ↓
Ask about this report
     ↓
Server session after CanViewReportAsync
     ↓
/knowledge-assistant?report={id} (re-checked)
     ↓
Retrieval limited to that report’s chunks
```

The browser-supplied id is not trusted. Changing the query string cannot load another school’s report.

---

## AI-assisted development

Work was done in **Cursor** against project rules. AI produced plans and diffs. Humans approved behavior changes and rejected incorrect suggestions.

```text
Requirement
    ↓
AI-generated plan
    ↓
Human review
    ↓
Corrected plan (docs/architecture/corrected-plan.md)
    ↓
Implementation
    ↓
Tests
    ↓
Human review
    ↓
Accepted or rejected (docs/decisions/rejected-ai-proposals.md)
```

The original four-project overlay in [`docs/architecture/implementation-plan.md`](docs/architecture/implementation-plan.md) is historical. The approved architecture is [`docs/architecture/corrected-plan.md`](docs/architecture/corrected-plan.md). The current tree later added Domain / Application / Infrastructure projects and a Knowledge Assistant; those exist in source even where the corrected plan preferred fewer assemblies.

---

## Cursor configuration

| Path | Role |
|---|---|
| `.cursor/rules/00-change-protocol.mdc` | List files, rules, tests, and risks before behavior changes |
| `.cursor/rules/10-legacy-immutable.mdc` | Never modify `/legacy` |
| `.cursor/rules/20-characterize-sas.mdc` | Do not invent SAS rules |
| `.cursor/rules/30-mvp-constraints.mdc` | SQLite only; keep the MVP small |
| `.cursor/rules/40-accessibility.mdc` | WCAG-oriented UI; do not claim PDF accessibility without validation |
| `.cursor/rules/50-secrets.mdc` | Do not commit secrets |
| `.cursor/rules/60-pdf-visual-parity.mdc` | Keep GrayscalePrinter layout |
| `.cursor/mcp.json` | GitHub MCP + read-only SQLite MCP |

---

## MCP usage

| Server | Configured in `.cursor/mcp.json` | Role |
|---|---|---|
| GitHub MCP | Yes | Issues, PRs, review. Auth via `${env:GITHUB_PAT}` — no token in git |
| SQLite MCP | Yes | `scripts/mcp-sqlite-readonly.mjs` opens `data/schoolreports.db` read-only |
| Playwright MCP | **No** | Used in Cursor sessions for UI/security review snapshots |

MCP is a development aid. It is not a production API and not a substitute for `dotnet test`.

---

## Testing strategy

```text
                 Playwright MCP reviews (manual)
                       ▲
                 Integration tests
                       ▲
               Characterization tests
                       ▲
                   Unit tests
```

| Level | Project | What it covers |
|---|---|---|
| Unit | `tests/AccessibleSchoolReports.UnitTests` | Calculator, recodes, layout, PDF markers, import parser, authz, downloads, RAG, embeddings, LLM fakes |
| Characterization | `tests/AccessibleSchoolReports.CharacterizationTests` | Observed SAS maps and baseline printed values |
| Integration | `tests/AccessibleSchoolReports.IntegrationTests` | SQLite migrate, Excel import, generate one/all, knowledge ingest |
| Playwright | *None* | No `*Playwright*` test project and no CI browser job |

---

## Security test coverage

Implemented in unit tests (`AuthenticationFoundationTests`, `RoleAuthorizationTests`, `PolicyAuthorizationTests`, `ReportAuthorizationServiceTests`, `ReportDownloadEndpointTests`, `KnowledgeAssistantPageTests`, `KnowledgeAccessTests`, `EmbeddingAccessTests`, `ApplicationSecuritySuiteTests`) and recorded in [`evidence/test-results/security-test-results.md`](evidence/test-results/security-test-results.md).

Covered:

- Anonymous pages redirect to `/signin`
- Invalid credentials
- Logout ends the cookie
- Viewer / ReportUser cannot use Admin pages
- Unauthorized school and report denied
- Unauthorized PDF → 404; traversal variants denied
- Direct `wwwroot` is not the PDF store
- School A caller does not receive School B knowledge or LLM context
- Report id tampering does not load another report
- `ApiKey` / `Bearer` not rendered on the assistant page

---

## Accessibility testing

| Check | Status |
|---|---|
| PDF structure markers in unit tests | Implemented (not certification) |
| UI labels, headings, skip link, focus, `aria-live` | Implemented in the Blazor UI |
| Playwright MCP UI review | Observed notes in `docs/accessibility/ui-accessibility-review.md` and `evidence/test-results/security-ui-review.md` |
| veraPDF / PAC / NVDA / JAWS | **Not in this repository** |
| Automated axe/Playwright a11y CI | **Not implemented** |

---

## CI/CD and quality gates

[`.github/workflows/quality.yml`](.github/workflows/quality.yml) runs on **pushes to `main`** and on **pull requests**:

```text
Restore → Build Release → dotnet test (exclude LegacyModernParityTests)
```

There is no Playwright, veraPDF, or performance job. A red quality gate is a failed restore, build, or included test.

Local integrity:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/pre-commit.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-legacy-integrity.ps1
```

`scripts/pre-commit.ps1` refuses commits that stage `legacy/sas`, `legacy/samples`, or `legacy/baseline`, then checks hashes in [`docs/capstone/legacy-baseline.md`](docs/capstone/legacy-baseline.md).

---

## Git workflow

```text
feature branch
     ↓
implementation + tests
     ↓
self-review
     ↓
commit (legacy hook)
     ↓
pull request
     ↓
quality.yml
     ↓
review / merge to main
```

Remote: [https://github.com/Manju-TF/accessible-school-report-modernizer](https://github.com/Manju-TF/accessible-school-report-modernizer). Example human review: [PR #2](https://github.com/Manju-TF/accessible-school-report-modernizer/pull/2).

---

## Human review / “Reviewed, not abdicated”

AI-generated code was not treated as accepted.

**Rejected change** ([`docs/decisions/rejected-ai-proposals.md`](docs/decisions/rejected-ai-proposals.md)): emit salary statistics when `n < 5`. That would violate `CF-S-00`. The calculator still suppresses those cells.

**Accepted with conditions** (PR #2): calculator, layout, and tagged-PDF targeting for the capstone merge; downloads require `RequireReportAccess` and school grants; do not describe PDFs as accessible without validation tooling.

---

## Setup and local run

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git
- A current browser
- Optional: Node.js only if you run the SQLite MCP script

SAS is not required to run the modern app.

### Clone and build

```powershell
git clone https://github.com/Manju-TF/accessible-school-report-modernizer.git
cd accessible-school-report-modernizer
dotnet restore
dotnet build AccessibleSchoolReports.sln
```

### Development user

Committed `appsettings.json` keeps seed credentials empty. Set user secrets locally:

```powershell
dotnet user-secrets set Identity:SeedUserName "dev.user" --project src/AccessibleSchoolReports.Web
dotnet user-secrets set Identity:SeedPassword "Replace-This-1!" --project src/AccessibleSchoolReports.Web
dotnet user-secrets set Identity:SeedRole "Admin" --project src/AccessibleSchoolReports.Web
```

Do not commit the password. Identity requires 8+ characters with upper, lower, digit, and non-alphanumeric.

### Database

Startup runs `MigrateAsync` and ensures roles. If a local database predates Identity tables, delete gitignored `data/schoolreports.db` and restart.

### Run

```powershell
dotnet run --project src/AccessibleSchoolReports.Web --launch-profile https
```

Open **https://localhost:7117** (also http://localhost:5017, which redirects).

1. Sign in
2. Import `legacy/samples/sample-export.xlsx` (do not write back to `/legacy`)
3. Generate one school (year `2025`) and download the PDF
4. Generate all (sequential or parallel, max 1–8)
5. Confirm Run History duration and downloads
6. Open Knowledge Assistant and ask a characterized-rule question

### Tests

```powershell
dotnet test tests/AccessibleSchoolReports.CharacterizationTests
dotnet test tests/AccessibleSchoolReports.UnitTests
dotnet test tests/AccessibleSchoolReports.IntegrationTests --filter "FullyQualifiedName!~LegacyModernParityTests"
dotnet test tests/AccessibleSchoolReports.UnitTests --filter "FullyQualifiedName~RagEvaluation"
```

Full solution (includes the known-failing parity test):

```powershell
dotnet test AccessibleSchoolReports.sln
```

### Legacy hook (once per clone)

```powershell
$root = git rev-parse --show-toplevel
$hook = Join-Path $root ".git\hooks\pre-commit"
@'
#!/bin/sh
repo_root="$(git rev-parse --show-toplevel)"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$repo_root/scripts/pre-commit.ps1"
exit $?
'@ | Set-Content -Path $hook -Encoding ascii
```

---

## Configuration

| Key | Role |
|---|---|
| `ConnectionStrings:SchoolReports` | SQLite path (`Data Source=data/schoolreports.db`) |
| `Identity:SeedUserName` / `SeedPassword` / `SeedRole` | Optional development user |
| `ReportGeneration:OutputRoot` | PDF root (`output` under the Web content root) |
| `ReportGeneration:ClassYear` | Default `2025` |
| `ReportGeneration:DefaultMaxParallelism` | Default `4` |
| `Embeddings:Provider` | `Lexical` (default) or `OpenAICompatible` |
| `Embeddings:ApiKey` | Required only for remote embeddings |
| `LanguageModel:ApiKey` | Required for live chat completions |
| `LanguageModel:Endpoint` / `Model` | Default OpenAI-compatible chat |

Placeholders (user secrets or environment; never commit real values):

```text
Identity:SeedPassword=<your-secret>
LanguageModel:ApiKey=<your-secret>
Embeddings:ApiKey=<your-secret>
```

Equivalent environment variables use double underscores, for example `LanguageModel__ApiKey`. Restart the web app after changing secrets.

---

## Demo / capstone walkthrough

1. Sign in as a seeded Admin
2. Dashboard: school and graduate counts after import
3. Import the sample workbook
4. Generate one school PDF; download via `/downloads/reports/{id}`
5. Generate all sequential, then bounded parallel
6. Run History: status, counts, duration
7. Report details → Ask about this report
8. Knowledge Assistant: characterized-rule question; show sources or insufficient evidence
9. Sign in as Viewer: Import / Generate All denied; unauthorized school PDF is 404
10. `dotnet test` with the CI filter
11. Show `.github/workflows/quality.yml` and a merged PR
12. Show `docs/decisions/rejected-ai-proposals.md`

---

## Traceability matrix

| Capstone requirement | Implementation | Evidence |
|---|---|---|
| AI-assisted build | Cursor + `.cursor/rules/` | This README, Git history |
| Corrected plan | Approved MVP plan | [`docs/architecture/corrected-plan.md`](docs/architecture/corrected-plan.md) |
| Business rules | `CF-*` / `SS-*` catalogue | [`docs/capstone/business-rules.md`](docs/capstone/business-rules.md) |
| Characterization tests | Domain-only lock of SAS maps | `tests/AccessibleSchoolReports.CharacterizationTests` |
| Modern .NET build | Four projects + Web | `src/` |
| SQLite | EF Core + migrations | `SchoolReportsDbContext` |
| Accessible PDF *target* | QuestPDF | `QuestPdfAccessiblePdfGenerator` + PDF unit tests |
| Authentication | Identity cookie | [`docs/capstone/authentication.md`](docs/capstone/authentication.md) |
| Authorization | Policies + `UserSchoolAccess` | [`docs/capstone/authorization-model.md`](docs/capstone/authorization-model.md) |
| RAG | Knowledge Assistant | [`docs/capstone/external-rag-api.md`](docs/capstone/external-rag-api.md) |
| RAG evaluation | 11 observed cases | [`evidence/test-results/rag-evaluation.md`](evidence/test-results/rag-evaluation.md) |
| Security tests | Application security suite | [`evidence/test-results/security-test-results.md`](evidence/test-results/security-test-results.md) |
| UI review | Playwright MCP | [`docs/accessibility/ui-accessibility-review.md`](docs/accessibility/ui-accessibility-review.md) |
| CI quality gate | GitHub Actions | [`.github/workflows/quality.yml`](.github/workflows/quality.yml) |
| Human review | Rejected salary-below-5 change | [`docs/decisions/rejected-ai-proposals.md`](docs/decisions/rejected-ai-proposals.md) |
| Visual PDF compare | Page images | [`evidence/screenshots/pdf-compare/`](evidence/screenshots/pdf-compare/) |

---

## Capstone definition of done

- [x] AI-assisted build evidenced
- [x] Saved specification (`docs/`)
- [x] Corrected AI plan
- [x] Legacy behavior characterized
- [x] Business rules documented
- [x] Characterization tests implemented
- [x] .NET modernization completed
- [x] SQLite implemented
- [x] Excel import implemented
- [x] Report calculations implemented
- [ ] Parity validated against the Test University baseline PDF (known subject mismatch; calculator tests still lock `CF-S-00`)
- [x] Accessible PDF *targeted* (not certified)
- [x] Single report generation
- [x] Batch report generation
- [x] Sequential processing
- [x] Parallel processing
- [x] Authentication
- [x] Authorization
- [x] School-level access control
- [x] Protected PDF access
- [x] RAG / Knowledge Assistant
- [x] Authorization-aware RAG
- [x] RAG evaluation (observed, lexical + fake LLM)
- [x] Security tests
- [ ] Playwright test project / CI job
- [x] Accessibility *review* (not a WCAG/PDF-UA certificate)
- [x] GitHub Actions quality gate
- [x] Human review evidence
- [x] Rejected AI output documented
- [x] README completed

---

## Performance

`evidence/test-results/performance-results.md` is **not in this repository**. No formal 189-school benchmark was recorded.

What exists:

- Each `ReportRun` stores `DurationMilliseconds` (Run History)
- Integration tests assert sequential and parallel generate-all complete with a non-negative duration
- Sequential vs parallel is checked for equivalent PDF **text**, not a required speedup
- Default RAG embeddings are local lexical vectors so Ask does not depend on embedding-API quota

If a timed run is needed, generate all schools in the UI and copy the duration from Run History. Do not invent timings.

---

## Security considerations

| Control | Status |
|---|---|
| Identity + hashed passwords + cookie | Implemented |
| Role policies + school grants | Implemented |
| Server-side report/PDF checks | Implemented |
| Path traversal / output-root confinement | Implemented |
| Excel validation | Implemented |
| Secrets not in git | Implemented (user secrets / env) |
| RAG filter before LLM | Implemented |
| Prompt injection treated as data | Implemented |
| External API timeouts / retries | Implemented |
| Formal threat model / SIEM / production IdP | **Not implemented** |

---

## Limitations, assumptions, and risks

### Limitations

- PDF accessibility is **not** validated with veraPDF, PAC, or a screen reader
- UI accessibility is reviewed, not certified
- Local MVP: no multi-node host, no second database, no cloud deploy
- SQLite writers can lock under concurrent load
- Eighteen characterization tests remain skipped
- Baseline PDF and sample Excel are different populations
- No Playwright E2E suite
- Pixel identity with SAS ThorndaleAMT is not claimed (Times/Thorndale stack)
- Live LLM answers depend on provider quota (HTTP 429 has been observed)
- RAG must not be used as the calculator

### Assumptions

- Input workbooks follow the characterized column set
- Report year `2025` is the MVP chrome year
- School codes are the SAS `%SCHRPTS` identifiers
- External AI APIs are optional at deploy time (lexical embeddings still retrieve)
- Development seed users exist only when secrets are set

### Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Undocumented SAS behavior | High | Characterization + skipped tests instead of guesses |
| Business-rule mismatch | High | Rule IDs + calculator tests (`CF-S-00`) |
| PDF accessibility gaps | High | Target tags; report validation separately |
| Unauthorized report access | Critical | `IReportAuthorizationService` + 404 downloads |
| RAG leakage | Critical | `KnowledgeAccess` before scoring / LLM |
| External API failure | Medium | Timeouts, retries, insufficient-evidence / user-facing errors |
| Parallel generate issues | Medium | Bounded 1–8, per-school isolation, text compare |
| AI-generated incorrect code | High | Change protocol, human review, rejected-proposal log |

---

## Future enhancements

These are **not** required for the current capstone:

- PostgreSQL or SQL Server for multi-writer hosting
- A dedicated vector database
- veraPDF / PAC / screen-reader evidence packs
- Production identity provider
- Object storage for PDFs
- Distributed generate-all
- Automated Playwright CI
- Recorded performance benchmarks
- Richer RAG evaluation against a live model
- Audit logging and observability

---

## Documentation index

Indexes: [`docs/README.md`](docs/README.md) (design), [`evidence/README.md`](evidence/README.md) (observed runs).

| Path | Contents |
|---|---|
| [`docs/capstone/business-rules.md`](docs/capstone/business-rules.md) | Combined Rule IDs |
| [`docs/capstone/createschrptfiles-analysis.md`](docs/capstone/createschrptfiles-analysis.md) | Builder SAS |
| [`docs/capstone/schreptsummary-analysis.md`](docs/capstone/schreptsummary-analysis.md) | Report SAS |
| [`docs/capstone/report-map.md`](docs/capstone/report-map.md) | Baseline section map |
| [`docs/capstone/legacy-baseline.md`](docs/capstone/legacy-baseline.md) | SHA-256 manifest for `/legacy` |
| [`docs/capstone/authentication.md`](docs/capstone/authentication.md) | Identity cookie sign-in |
| [`docs/capstone/authorization-model.md`](docs/capstone/authorization-model.md) | Roles and school grants |
| [`docs/capstone/security-architecture.md`](docs/capstone/security-architecture.md) | Policies and download/RAG boundaries |
| [`docs/capstone/external-rag-api.md`](docs/capstone/external-rag-api.md) | Embeddings and LLM |
| [`docs/capstone/generated-pdf-rag.md`](docs/capstone/generated-pdf-rag.md) | PDF → knowledge chunks |
| [`docs/architecture/corrected-plan.md`](docs/architecture/corrected-plan.md) | Approved architecture plan |
| [`docs/architecture/implementation-plan.md`](docs/architecture/implementation-plan.md) | Rejected original plan |
| [`docs/accessibility/pdf-accessibility-strategy.md`](docs/accessibility/pdf-accessibility-strategy.md) | Tagged-PDF target |
| [`docs/accessibility/ui-accessibility-review.md`](docs/accessibility/ui-accessibility-review.md) | UI review findings |
| [`docs/decisions/rejected-ai-proposals.md`](docs/decisions/rejected-ai-proposals.md) | Human-rejected AI changes |
| [`evidence/test-results/`](evidence/test-results/) | Quality, parity, security, RAG, UI review |
| [`evidence/screenshots/pdf-compare/`](evidence/screenshots/pdf-compare/) | Visual page compare |

---

## Important notes for evaluators

- Do not claim the generated PDF is accessible without separate validation.
- Do not treat the Knowledge Assistant as authoritative for counts or salaries.
- Do not edit `/legacy`.
- Do not invent school names or suppression thresholds.
- Do not commit API keys, seed passwords, or `.env` files.
