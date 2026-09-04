# Implementation Plan — Rejected

**Status:** Rejected. Do not implement from this file.

**Superseded by:** `docs/architecture/corrected-plan.md` (approved architecture).

This file is the original AI four-project plan. It is kept only as a record of what was proposed and why it was too large for a solo capstone. Required *features* from this plan still apply; they are delivered with the **corrected** layering (one Web project, two tables, no ports).

Do not scaffold `Domain` / `Application` / `Infrastructure`, ports, summary tables, migrations, or five Blazor pages.

---

## Decision: original vs simplified

| Topic | Original AI plan (this file, **rejected**) | Corrected plan (`corrected-plan.md`, **approved**) |
|---|---|---|
| Solution | `Domain`, `Application`, `Infrastructure`, `Web` | One Web project |
| Abstractions | Three ports plus a thin `IReportDb` | No ports; call EF / ClosedXML / QuestPDF from the app |
| Calculations | Domain recodes + count/salary builders + merger | One calculator class in Web |
| Persistence | Graduates + summaries + runs + run-schools; migrations | Graduates + runs only; `EnsureCreated` |
| When rules run | `BuildSchoolSummaries` use case, then PDF reads the table | Compute at PDF time |
| UI | Import, Schools, Generate, GenerateAll, Runs | Import, Reports |
| Tests | Characterization + Domain.Tests + Application.Tests + Playwright E2E | Characterization + calculator tests; one Playwright path |
| Evidence | Capstone evidence per step | Evidence at the end |
| Excel | Formal map gate before EF schema | Short header list; do not invent columns |
| CI | Step 14 thin workflow | Skip unless time left |

**Why original is rejected.** The stack and features fit the capstone. The four-project overlay, ports, summary warehouse, and per-step evidence do not fit a solo-capstone scope. Implement `corrected-plan.md`.

---

## Constraints that stay in force

- `/legacy` is immutable (SAS, baseline PDF, Excel sample).
- SQLite only. No extra databases, queues, Redis, Kafka, cloud, microservices, MediatR, or CQRS.
- No per-entity repositories. EF Core `DbContext` plus **three** ports: Excel read, PDF write, file store. Plus thin `IReportDb` (DbSets / `SaveChanges` only).
- Salary suppression is **`n ge 5` (`CF-S-00`)**. The PDF note is the same rule in words (`SS-SUP-01`), not a second threshold.
- Do not add FT / solo / `SELFPR` filters that exist only in notes.
- Do not rewrite `NOT IN ('7-USKW','8-UNWK')`, `$jobcat1`, or D3 `FULL` labeling.
- Tagged PDF is a **target**. Accessibility is reported from validation, not assumed.
- Existing characterization tests stay. New code must satisfy them or keep the same `TODO` skips.
- Class-year strings: implement **2025 SAS** titles/footer (`Class of 2025`, `July 2026`). The baseline PDF’s 2024 / July 2025 strings stay a documented difference.

Sources: `createschrptfiles-analysis.md`, `schreptsummary-analysis.md`, `business-rules.md`, `report-map.md`, `legacy-baseline.md`.

---

## Stack

| Item | Choice |
|---|---|
| Runtime | .NET 8 |
| UI | ASP.NET Core Blazor |
| ORM / DB | EF Core + SQLite |
| Excel | ClosedXML |
| PDF | QuestPDF (tagged / PDF-UA **target**) |
| Unit / characterization | xUnit |
| E2E | Playwright |

**Do not introduce:** microservices, Redis, Kafka, message queues, cloud infrastructure, unnecessary CQRS, unnecessary MediatR, unnecessary repository abstractions.

---

## Target solution

```text
AccessibleSchoolReportModernizer.sln
src/AccessibleSchoolReports.Domain
src/AccessibleSchoolReports.Application
src/AccessibleSchoolReports.Infrastructure
src/AccessibleSchoolReports.Web
tests/AccessibleSchoolReports.CharacterizationTests   (already present)
tests/AccessibleSchoolReports.Domain.Tests
tests/AccessibleSchoolReports.Application.Tests
tests/AccessibleSchoolReports.Web.E2ETests
```

| Project | Allowed contents |
|---|---|
| **Domain** | Graduate row, recodes, count/salary/section results, `n ge 5`. No EF, no PDF, no HTTP. |
| **Application** | Use cases: import, validate, persist, build summary, generate one/all, list runs. Ports only. |
| **Infrastructure** | EF Core + SQLite, ClosedXML, QuestPDF, file storage. |
| **Web** | Blazor UI: upload, validation, generate, history, PDF download. |

**Composition:** Web registers Infrastructure implementations. Application depends on Domain + ports. Domain has no project dependencies.

**Failed school in a batch (approved default):** continue, mark that school failed, run ends as `CompletedWithErrors`.

**Percentiles:** `PROC UNIVARIATE` `q1` / `median` / `q3` / `mean` / `n` with no `PCTLDEF` in SAS. Document the conservative SAS default used. Keep a TODO until raw salaries exist in-repo.

---

## What already exists

| Artifact | Role |
|---|---|
| `docs/capstone/createschrptfiles-analysis.md` | Builder rules |
| `docs/capstone/schreptsummary-analysis.md` | Renderer rules |
| `docs/capstone/business-rules.md` | Rule IDs |
| `docs/capstone/report-map.md` | PDF section map |
| `tests/AccessibleSchoolReports.CharacterizationTests/` | 172 passing locks + 18 TODOs |
| `legacy/samples/sample-export.xlsx` | Import sample — **not yet column-characterized** |
| `legacy/baseline/test-school-report.pdf` | Visual/numeric comparison only |

**Gap before schema work:** Excel columns are not characterized. Step 1 documents them. Do not invent a codebook.

---

## Step 1 — Characterize the Excel sample (docs only)

**Purpose.** List actual `sample-export.xlsx` sheets, headers, types, and how they map to SAS inputs (`code`, `sex3`, `minstat`, `jobcat1`, `jobftpt`, `empgen`, `firm1`, `lfjob`, `jobreg`, `locationflag`, `jobst`, `source`, `time1`, `status`, `duration`, `schoolfund`, `salftperm`, `emptype1`). No invented columns.

**Files.** `docs/capstone/excel-import-map.md` only. Do not change `/legacy`.

**Dependencies.** `legacy/samples/sample-export.xlsx`; builder field list from `createschrptfiles-analysis.md`.

**Tests.** None yet. If a column cannot be mapped, mark **TODO** in the doc.

**Risks.** Schema invented from names instead of the workbook. Extra Excel columns treated as required.

**Capstone evidence.** `evidence/excel-import-map.md` copy or link; note unmapped columns.

**Approval gate.** Do not create EF entities until this map is accepted.

---

## Step 2 — Solution scaffold (no business behavior)

**Purpose.** Create the four projects, solution, net8.0, project references, empty Blazor host, SQLite path from user secrets or untracked local config (not a committed connection string with credentials).

**Files.**
- `AccessibleSchoolReportModernizer.sln`
- `src/AccessibleSchoolReports.{Domain,Application,Infrastructure,Web}/*.csproj`
- `src/AccessibleSchoolReports.Web/Program.cs`, `App.razor`, layout shell
- `tests/AccessibleSchoolReports.Domain.Tests/*.csproj`
- `tests/AccessibleSchoolReports.Application.Tests/*.csproj`
- `tests/AccessibleSchoolReports.Web.E2ETests/*.csproj` (Playwright stub)
- `.gitignore` for `bin/`, `obj/`, `*.db`, user secrets

**Dependencies.** Step 1 accepted. Existing characterization project added to the solution as-is.

**Tests.** `dotnet test` still runs characterization (172 pass / 18 skip). New projects compile.

**Risks.** Premature folders “for later.” Secrets committed.

**Capstone evidence.** `evidence/scaffold.md` — solution tree and `dotnet build` output.

---

## Step 3 — Domain recodes and filters (port characterization locks)

**Purpose.** Move **confirmed** mappings from test `LegacyRules` into Domain. Characterization tests call Domain. No “cleanup” of ambiguous lists.

**Files.**
- `src/AccessibleSchoolReports.Domain/Recoding/Sex3Recoder.cs` (`CF-PREP-06`)
- `…/JobcatFormatter.cs` (`CF-FMT-03`, `CF-PREP-01`)
- `…/SourceRecoder.cs` (`CF-PREP-04`)
- `…/JobregRecoder.cs` (`CF-PREP-03`)
- `…/LfjobRecoder.cs` (`CF-PREP-02`)
- `…/EmptypeRecoder.cs` (`CF-PREP-05`)
- `…/FirmSizeMapper.cs` (counts include `S`→`SOLO`; salaries do **not** — `CF-C-16`, `CF-S-15`)
- `…/SectorMapper.cs` (`CF-C-08`)
- `…/D1EmploymentRollup.cs` (written `NOT IN ('7-USKW','8-UNWK')` unchanged)
- `src/AccessibleSchoolReports.Domain/Reporting/AnalvarPageFilter.cs` (`SS-FIL-01`–`07`)
- `src/AccessibleSchoolReports.Domain/Reporting/RowLabels.cs` (`SS-FMT-02`–`04`)
- Characterization tests updated to use Domain (delete duplicated test-only maps)

**Dependencies.** Step 2.

**Tests.** Existing characterization facts for mappings/filters must keep passing. Domain.Tests only for small extras that still cite Rule IDs.

**Risks.** “Fixing” `$jobcat1`, `MINOR F` vs `MINORF`, or D1 exclusions. Implementing commented `CF-DEAD-*` recodes.

**Capstone evidence.** `evidence/domain-recodes.md` — Rule ID → type; note that 18 TODOs remain skipped.

---

## Step 4 — Domain counts (builder reproduction)

**Purpose.** Pure functions that take graduate rows and emit `analvar` / `newvar` / count / percent rows the way the builder’s `PROC FREQ` / `MEANS SUM` steps do. Percents are **stored frequency percents**, then summed for subtotals (`SS-CALC-02`). Do not recompute “percent of Total Reported” unless that is what SAS stored.

**Files.**
- `src/AccessibleSchoolReports.Domain/Records/GraduateRecord.cs`
- `src/AccessibleSchoolReports.Domain/Summaries/SchoolCountBuilder.cs` (`CF-C-01`–`CF-C-20`, `CF-P2-01`–`05`)
- `…/CountRow.cs`, `…/Part2Row.cs`
- `tests/AccessibleSchoolReports.Domain.Tests/SchoolCountBuilderTests.cs`

**Dependencies.** Step 3. Fixture rows **constructed in tests**, not guessed from Excel until Step 1 is done.

**Tests.**
- Gender / status / sector / employer / region / source / timing / FT-PT with known fixtures.
- D1 rollup uses the **written** exclusion list.
- Empty categories produce **no row** (`SS-FIL-08`).
- Characterization count observations stay; do not force D3 94 to equal subtotal 93.

**Risks.** Inventing percent bases. Dropping zero rows in the builder instead of omitting missing freq keys. Treating Test University PDF anomalies as formulas.

**Capstone evidence.** `evidence/count-rules.md` — fixture → `analvar` / `newvar` / count; Rule IDs.

---

## Step 5 — Domain salaries (population, n, percentiles, mean, suppression)

**Purpose.** For each salary slice (`CF-S-01`–`19`), compute n, mean, Q1, median, Q3 on `salftperm`. Keep row only if **`n >= 5`**. Display blank / missing when omitted. Do not implement extra FT/solo filters.

**Files.**
- `src/AccessibleSchoolReports.Domain/Salaries/SalaryStatisticCalculator.cs`
- `…/SalarySuppression.cs` (`CF-S-00`)
- `…/SalaryRow.cs`
- `src/AccessibleSchoolReports.Domain/Summaries/SchoolSalaryBuilder.cs`
- `src/AccessibleSchoolReports.Domain/Summaries/SchoolSummaryMerger.cs` (`CF-M-01`)

**Dependencies.** Step 4. Conservative SAS reading: `PROC UNIVARIATE` outputs with **no `PCTLDEF` in source**. Document the default used; keep TODO if unproven against raw salaries (none are in the repo).

**Tests.**
- Suppression: n=4 omitted, n=5 kept; headcount 5 with salary n &lt; 5 omitted.
- D3 salary keys forced to `…FULL` without an FT filter (`CF-S-19`).
- Firm-size salaries do not map `S`→`SOLO`.
- Do **not** invent raw salaries that “should” produce PDF 85,802. Lock calculator tests on **constructed** lists. PDF displayed stats stay characterization-only until a real graduate file exists.

**Risks.** Wrong percentile definition presented as fact. Using headcount instead of salary n. Adding note-only filters.

**Capstone evidence.** `evidence/salary-suppression.md` — `n ge 5` cases; percentile default stated as conservative, not claimed proven.

---

## Step 6 — SQLite persistence (EF Core, no repositories)

**Purpose.** Store imported graduates, built summary rows, and report-run history. One `ReportDbContext`. SQLite file under a local untracked path (e.g. `data/app.db` gitignored).

**Files.**
- `src/AccessibleSchoolReports.Infrastructure/Data/ReportDbContext.cs`
- `…/Entities/ImportedGraduate.cs`, `SchoolSummaryRow.cs`, `ReportRun.cs`, `ReportRunSchool.cs`
- `…/Migrations/*` after first approved schema
- `src/AccessibleSchoolReports.Web/Program.cs` — `AddDbContext`, migrate on startup

**Suggested tables (adjust only after Step 1):**
- `ImportedGraduates` — one row per Excel graduate; import batch id
- `SchoolSummaries` — `code`, `analvar`, `newvar`, count, percent, n, pct25, median, pct75, mean, part2 columns
- `ReportRuns` — id, started/ended, mode (`Single` / `Sequential` / `BoundedParallel`), status, max parallelism, error
- `ReportRunSchools` — run id, school code, name, pdf path, status

**Dependencies.** Steps 1 and 2. Domain types mapped in Infrastructure, not the other way around.

**Tests.** Application.Tests with EF Core SQLite in-memory or temp file: insert batch, query by `code`, write a `ReportRun`.

**Risks.** Extra aggregate tables. Committed `.db` or secrets. Repository layer creeping in.

**Capstone evidence.** `evidence/sqlite-schema.md` — tables and “no second database.”

---

## Step 7 — Excel import + validation

**Purpose.** ClosedXML reads the mapped sheet(s). Validation is structural and type-based from Step 1, plus “required for builder” fields. Invalid rows are reported; do not silently recode unknown values beyond confirmed SAS recodes (unknown `jobcat1` stays raw, like `PUT`).

**Files.**
- `src/AccessibleSchoolReports.Application/Imports/IExcelImportReader.cs`
- `…/ImportExcel.cs`, `…/ValidateImport.cs`, `…/ImportResult.cs`, `…/ValidationMessage.cs`
- `src/AccessibleSchoolReports.Infrastructure/Excel/ClosedXmlImportReader.cs`
- Working copy of the sample under `data/` if needed — **never write into `/legacy`**

**Dependencies.** Steps 1 and 6.

**Tests.**
- Application.Tests: missing sheet, missing header, empty file, valid fixture row persists.
- Do not treat `sample-export.xlsx` as containing Test University unless Step 1 shows that.

**Risks.** Validation inventing NALP codebook rules. Mutating the sample workbook.

**Capstone evidence.** `evidence/import-validation.md` — sample run: rows read, errors, persisted count.

---

## Step 8 — Application: build school summary (use case)

**Purpose.** `BuildSchoolSummaries` loads imported rows, runs Domain builders, writes `SchoolSummaries`. This is the SAS builder, not the PDF.

**Files.**
- `src/AccessibleSchoolReports.Application/Summaries/BuildSchoolSummaries.cs`
- `…/IReportDb.cs` — **one** abstraction: DbSets / `SaveChanges`, not a repository per table.

**Dependencies.** Steps 4–7.

**Tests.** Application.Tests: one fixture school → expected `analvar` rows. Characterization Rule IDs on asserts.

**Risks.** Recomputing in the PDF step. Hiding D1/D3 ambiguities behind “cleaner” SQL.

**Capstone evidence.** `evidence/summary-build.md` — row dump for one school vs characterization keys.

---

## Step 9 — QuestPDF single-school report (tagged target)

**Purpose.** One PDF per school, seven pages, `report-map.md` sections. Display stored summary columns. Notes/headers/footers from `SS-NOTE-*` / `SS-HDR-*` / `SS-FTR-01` using **2025 SAS** wording. Empty categories omitted. Salary cells blank when n suppressed. No salary subtotals.

**Files.**
- `src/AccessibleSchoolReports.Application/Reporting/ISchoolReportRenderer.cs`
- `…/IReportFileStore.cs`
- `…/GenerateSchoolReport.cs`
- `src/AccessibleSchoolReports.Infrastructure/Pdf/QuestSchoolReportRenderer.cs`
- `src/AccessibleSchoolReports.Infrastructure/Storage/LocalReportFileStore.cs` → gitignored `data/reports/`

**Dependencies.** Step 8. `docs/capstone/report-map.md`.

**Tests.**
- Domain/Application: page membership, subtotal labels (`Subtotal` / `Total #` / `Total Reported`).
- Golden-structure test: section order and labels for a fixture school.
- Do not assert “PDF is accessible.” Optionally extract text and compare structure to report-map.
- Visual pixel-match to the baseline PDF is **out of scope** (2024 vs 2025 strings; Test University is not a live `%SCHRPTS` school).

**Risks.** Redesigning layout. Claiming PDF-UA. Implementing note-only filters. Over-fitting Test University anomalies.

**Capstone evidence.** `evidence/single-school-pdf/` — generated PDF + short structural checklist. Accessibility results in Step 12, not here.

---

## Step 10 — Blazor UI: import, validate, single-school generate

**Purpose.** WCAG 2.2 AA-oriented UI: upload Excel, show validation, pick a school, generate, download PDF. Semantic headings, labels, focus, keyboard, live regions for status.

**Files.**
- `src/AccessibleSchoolReports.Web/Components/Pages/Import.razor`
- `…/Schools.razor`, `…/Generate.razor`, `…/Runs.razor` (history stub ok until Step 11)
- `…/Layout/MainLayout.razor`, `NavMenu.razor`
- `wwwroot` styles — simple, high contrast, no inaccessible icon-only controls

**Dependencies.** Steps 7–9.

**Tests.** Manual keyboard pass + Step 13 Playwright. A single render screenshot is not verification.

**Risks.** Color-only errors. Inaccessible file input. Generating on GET.

**Capstone evidence.** `evidence/ui-single-school.md` — screenshots **plus** keyboard notes (tab order, errors announced).

---

## Step 11 — All-school sequential and bounded parallel + run history

**Purpose.**
- Sequential: one school at a time (`SS-ORD-01` page order per school; school order = imported `code` sort, not a new ranking rule).
- Bounded parallel: `Parallel.ForEachAsync` with a **user-capped** degree (e.g. 1–4). No Hangfire/queues.
- Each attempt writes `ReportRuns` / `ReportRunSchools`. Conservative default: **continue, mark school failed, run completes with `CompletedWithErrors`**.
- Use a DbContext **per school** (or equivalent) so parallel workers do not share one context.

**Files.**
- `src/AccessibleSchoolReports.Application/Reporting/GenerateAllSchoolReports.cs` (`GenerationMode`)
- `src/AccessibleSchoolReports.Web/Components/Pages/GenerateAll.razor`
- `…/Runs.razor` completed: list runs, status, links to PDFs

**Dependencies.** Steps 6, 9, 10.

**Tests.**
- Application.Tests: 3 fake schools; sequential order; parallel max=2; one renderer failure → run status + two successes.
- No test that requires Redis/queues.

**Risks.** Unbounded `Parallel.ForEach`. Shared DbContext across threads. Silent overwrite of PDFs.

**Capstone evidence.** `evidence/all-school-runs.md` — timings, max parallelism, history rows.

---

## Step 12 — Accessibility validation (separate from generation)

**Purpose.** Validate UI and PDF. Do not update copy to say “the PDF is accessible.”

**Files.**
- `docs/capstone/accessibility-validation.md`
- `evidence/pdf-validation/` — tool output actually run
- `evidence/ui-wcag.md` — keyboard, contrast, names/roles; stated honestly

**Dependencies.** Steps 9–11.

**Tests.** Playwright accessibility assertions that **fail on missing names/roles** for the flows we own. PDF tool results are evidence files, not a green “PDF-UA certified” unit test unless a real validator is in CI.

**Risks.** Treating QuestPDF helpers as PDF-UA. Hiding failed checks.

**Capstone evidence.** Validation reports **separate** from the generate button.

---

## Step 13 — Playwright end-to-end

**Purpose.** Browser tests for the real user path.

**Files.**
- `tests/AccessibleSchoolReports.Web.E2ETests/ImportAndGenerateTests.cs`
- `…/AllSchoolsRunTests.cs`
- `…/AccessibilitySmokeTests.cs`

**Flows.**
1. Upload fixture workbook (working copy of sample or a tiny generated xlsx from Step 1 columns).
2. See validation summary.
3. Generate one school; download PDF; assert HTTP success and non-empty file.
4. Start sequential all-school; history shows a completed run.
5. Start bounded parallel (max 2); history records mode + max degree.
6. Keyboard: file input and generate button reachable and named.

**Dependencies.** Steps 10–12. Local SQLite temp database per test.

**Risks.** Tests hitting `/legacy` write paths. Flaky absolute timeouts. Only screenshot assertions.

**Capstone evidence.** `evidence/e2e.md` — command, pass/fail, traces if failed.

---

## Step 14 — CI (thin)

**Purpose.** `dotnet test` on characterization + Domain + Application. Playwright in CI only if the runner can install browsers; otherwise document “E2E local / optional job.”

**Files.** `.github/workflows/ci.yml` (replace `.gitkeep`).

**Dependencies.** Steps 2 and 13.

**Tests.** The workflow is the test.

**Risks.** Secrets in YAML. Claiming E2E passed when the job is skipped.

**Capstone evidence.** Green workflow URL or local `dotnet test` log in `evidence/ci.md`.

---

## Feature → step map

| Required feature | Step |
|---|---|
| Excel import | 1, 7 |
| Validation | 7 |
| SQLite persistence | 6 |
| SAS business-rule reproduction | 3–5, 8 |
| Single-school report | 9–10 |
| Sequential all-school | 11 |
| Bounded parallel all-school | 11 |
| Report run history | 6, 11 |
| Accessible tagged PDF | 9 generate, 12 validate |
| Accessible web UI | 10, 12 |
| Automated tests | 3–8, existing characterization |
| Playwright E2E | 13 |

---

## Explicitly out of scope

- Redesigning sections, labels, or suppression.
- Resolving the 18 characterization TODOs by guessing.
- Matching Test University 2024 PDF year-strings.
- Pixel-identical SAS ODS styling.
- Auth, multi-tenant, cloud deploy, Hangfire, Redis, Kafka, microservices, MediatR, CQRS.
- Implementing this four-project file after it was rejected. Use `corrected-plan.md`.

---

## Suggested budget (rejected plan)

| Relative effort | Steps |
|---|---|
| Low | 1 Excel map + approval |
| Low | 2 scaffold |
| Medium | 3–4 recodes + counts |
| Medium | 5 salaries + suppression |
| Medium | 6–7 SQLite + import |
| Medium | 8 summary use case |
| High | 9 PDF |
| Medium | 10 UI |
| Low | 11 batch + history |
| Low | 12–14 a11y evidence, E2E, CI |

This budget belongs to the rejected plan. Use the implementation order in `corrected-plan.md`.

---

## Do not start from here

This file is historical. The approved next work is in `docs/architecture/corrected-plan.md`: calculator and `n ge 5` tests first, after the human says to start.

No application code was added in this documentation-only update.
