# Corrected Plan — Approved

**Status:** Approved. This is the plan to implement.

The original four-project AI plan in `docs/architecture/implementation-plan.md` is **rejected**. Do not implement Domain / Application / Infrastructure projects, ports, summary tables, migrations, or five Blazor pages.

**Do not start coding until the human says to start.** No application code was added when this status was set.

This file is the 30-hour MVP architecture. Required features stay; extra structure from the original plan does not.

Required outcomes stay: import, validate, SQLite, characterized SAS rules, single-school PDF, sequential all-school, bounded parallel all-school, run history, tagged-PDF *target*, accessible UI, xUnit, Playwright.

`/legacy` stays immutable. Do not invent business rules. Salary suppression is **`n ge 5` (`CF-S-00`)**.

---

## Review against the 30-hour solo constraint

A solo capstone can ship:

Excel → validate → SQLite → apply characterized SAS rules → school PDFs → run history → UI/PDF aimed at accessibility → tests on the calculator → one Playwright path.

It cannot also finish clean-architecture ports, four test assemblies, migrations, a summary warehouse, five Blazor pages, per-step evidence packs, and a full a11y/CI toolchain without cutting the rules or the PDF.

The original AI plan’s stack (.NET 8, Blazor, EF Core, SQLite, ClosedXML, QuestPDF, xUnit, Playwright) is right. The **assembly and process overlay** is what exceeds 30 hours.

---

## Every component that is not necessary for the MVP

Cutting these does **not** drop a required feature. It drops structure around the feature.

### Project / layering

| Not necessary | Why |
|---|---|
| `AccessibleSchoolReports.Domain` as its own project | Calculations can live in a folder (or one class library). A second assembly does not add behavior. |
| `AccessibleSchoolReports.Application` as its own project | Use cases are a few methods. A project boundary is ceremony. |
| `AccessibleSchoolReports.Infrastructure` as its own project | ClosedXML, EF, and QuestPDF can be called from the web project. |
| Ports `IExcelImportReader`, `ISchoolReportRenderer`, `IReportFileStore` | One implementation each. Interfaces “for testing” cost more than they save. |
| Thin `IReportDb` port | `ReportDbContext` used directly is enough. Not a per-entity repository; just no extra interface. |
| Application → Infrastructure inversion / composition-root theater | `Program.cs` registers `DbContext` and concrete classes. |

### Types / tables / files

| Not necessary | Why |
|---|---|
| One class per recode (`Sex3Recoder`, `JobcatFormatter`, `SourceRecoder`, …) | One `LegacyRecodes` type (ported from characterization maps) is enough. |
| Separate `SchoolCountBuilder` + `SchoolSalaryBuilder` + `SchoolSummaryMerger` as a pipeline | One `SchoolReportCalculator` that returns count rows and salary rows. |
| Persisted `SchoolSummaries` table | Compute at generate time from imported graduates. A warehouse is an extra schema and sync problem. |
| `ReportRunSchools` child table | One `ReportRuns` row with mode, status, output folder, and a short note (or JSON of school outcomes). |
| EF Core migrations | `EnsureCreated()` on a gitignored SQLite file. |
| `IReportFileStore` + storage abstraction | `File.WriteAllBytes` under `data/reports/`. |
| Separate Blazor pages for Import / Schools / Generate / GenerateAll / Runs | Two pages: **Import** and **Reports** (pick school, one/all, history). |
| Formal Excel map *plus* a duplicate evidence copy as a hard gate | One short column list. Do not spend a day on a second doc set. Still do not invent columns. |
| Evidence write-up after every one of 14 steps | Evidence at the end: test run, one PDF, one a11y note. |

### Process / extra quality systems

| Not necessary | Why |
|---|---|
| `Domain.Tests` + `Application.Tests` + `Web.E2ETests` as three new projects | Keep existing characterization tests; add calculator tests there or in **one** `AccessibleSchoolReports.Tests`. Playwright as a folder or one optional project. |
| Golden PDF / text-extract structural suite | Characterization + calculator tests lock rules. Spot-check PDF against `report-map.md`. |
| CI Playwright job + browser install | Optional. Local `dotnet test` + one Playwright run is the bar. |
| PAC / veraPDF / Adobe as a required *product* step | Target tagged PDF; file whatever was actually run. Do not build a validation system. |
| Step 14 CI workflow as a delivery requirement | Nice if time remains. Not required to prove the workflow. |
| Bounded-parallel *architecture* (per-school context policies, generation subsystem) | The *feature* is `MaxDegreeOfParallelism` on the same generate loop. |

### Still required (do not cut)

- Characterized SAS rules, including ugly ones (`n ge 5`, written D1 `NOT IN`, no note-only filters).
- Automated tests on counts, percents, salaries, suppression.
- SQLite for imported rows and run history.
- Single-school PDF and all-school generation (sequential + bounded parallel option).
- Accessible-enough UI (labels, keyboard, headings) and a tagged-PDF attempt, validation reported separately.
- `/legacy` untouched. Eighteen characterization TODOs stay skipped; do not guess values.

---

## Simplified architecture (human-directed)

```text
AccessibleSchoolReportModernizer.sln
src/AccessibleSchoolReports.Web
tests/AccessibleSchoolReports.CharacterizationTests   // existing; add calculator tests here
tests/AccessibleSchoolReports.Web.E2ETests            // one Playwright happy path, after the app works
data/app.db                                           // gitignored, EnsureCreated
data/reports/*.pdf                                    // gitignored
```

One extra test project maximum besides characterization. No Domain / Application / Infrastructure assemblies.

### Folders, not solution layers

```text
Web/
  Data/ReportDbContext.cs
  Data/ImportedGraduate.cs
  Data/ReportRun.cs
  Import/ExcelImporter.cs              // ClosedXML + validation
  Reporting/LegacyRecodes.cs           // confirmed maps only
  Reporting/SchoolReportCalculator.cs
  Reporting/SchoolReportPdf.cs         // QuestPDF
  Reporting/ReportGenerator.cs         // one / all sequential / all parallel
  Components/Pages/Import.razor
  Components/Pages/Reports.razor
  Program.cs
```

### Data stored

| Table | Purpose |
|---|---|
| `ImportedGraduates` | Excel rows (the working file). |
| `ReportRuns` | When, mode (`Single` / `AllSequential` / `AllParallel`), max degree, status, output folder, note. |

No summary warehouse. No run-school child table unless history is unusable without it.

### Generate path

1. Read graduates for `code` (or all codes).
2. `SchoolReportCalculator` → in-memory rows.
3. `SchoolReportPdf` → `data/reports/`.
4. Insert `ReportRun`.

### All-school

- Sequential: `foreach` school codes (sort by imported `code`, not a new ranking rule).
- Parallel: same method, `Parallel.ForEachAsync` with a cap (1–4). Do not share one `DbContext` across threads (short-lived context per school, or compute in memory and write files, then one save for the run). That is a coding caution, not a new layer.

### DI

Register `ReportDbContext`. Concrete services are fine. No ports.

### Excel columns

Skim `sample-export.xlsx` and list headers against the SAS fields already in `createschrptfiles-analysis.md`. Mark unmapped columns **TODO**. Do not invent a codebook. Do not block a week on a second characterization book.

### Titles / footer

Use **2025 SAS** strings (`Class of 2025`, `July 2026`). Baseline PDF 2024 / July 2025 remains a documented difference.

### Batch failure

Continue on school failure; run status `CompletedWithErrors`. Same product rule as the original plan; no extra policy engine.

### Accessibility

QuestPDF structure tags. Do not claim PDF-UA. Write down what was validated (UI keyboard + whatever PDF check was run).

---

## Implementation order (corrected)

1. Excel header list (short, in this repo as a note or a small section in this file when done — not a second architecture).
2. Web project + SQLite `EnsureCreated` + gitignore for `*.db`.
3. `LegacyRecodes` + `SchoolReportCalculator` with tests (`n ge 5` first). Point characterization mapping tests at the same recodes.
4. ClosedXML import + validation on the Import page.
5. One school PDF from the calculator.
6. All-school sequential + bounded parallel + `ReportRuns` on the Reports page.
7. UI keyboard/labels; tagged-PDF target; evidence note.
8. One Playwright path: import → generate → download.
9. CI only if time remains.

If the clock runs out, drop CI, extra E2E flows, and parallel *tuning*. Do not drop suppression tests or invent rules to make D3 94/93 “nice.”

---

## Compare: original AI plan vs simplified human-directed plan

| Topic | Original AI plan (`implementation-plan.md`) | Simplified human-directed plan (this file) |
|---|---|---|
| Solution | 4 src projects + 3 new test projects + characterization | 1 web project + existing characterization + optional 1 Playwright project |
| Style | Ports-and-adapters modular monolith | Single app, folders by feature |
| Abstractions | 3 ports + `IReportDb`; no per-entity repositories | No ports. `DbContext`, ClosedXML, QuestPDF, `File` |
| Domain model | Many recode types + count/salary/merger pipeline | `LegacyRecodes` + `SchoolReportCalculator` |
| Persistence | Graduates + summaries + runs + run-schools; migrations | Graduates + runs; `EnsureCreated` |
| When rules run | Separate `BuildSchoolSummaries`, PDF reads the table | Calculate at generate time |
| UI | Import, Schools, Generate, GenerateAll, Runs | Import, Reports |
| All-school | First-class sequential + parallel subsystem | One generator; parallel is a parameter |
| Tests | Characterization + Domain + Application + E2E + golden PDF | Characterization + calculator tests; one E2E path after the app works |
| Docs / evidence | Excel map gate + evidence after each of 14 steps | Short Excel column note; evidence at the end |
| A11y | Renderer project + later validation product | Tagged QuestPDF target; write down what you validated |
| CI | Dedicated Step 14 | Skip unless time left |
| Hours at risk | Layering and paper eat the PDF and rules | Hours go to calculator, PDF, import |

### Features: same outcomes, less machinery

| Required feature | Original AI plan | Simplified human-directed plan |
|---|---|---|
| Excel import | ClosedXML behind a port | ClosedXML class in Web |
| Validation | Application service + message types | Same importer; errors on the Import page |
| SQLite | 4 tables, migrations | 2 tables, `EnsureCreated` |
| SAS rules | Domain project + merger | `SchoolReportCalculator` tested against Rule IDs |
| Single-school PDF | Renderer port + file store | QuestPDF → `data/reports` |
| Sequential all | `GenerateAllSchoolReports` | Loop in `ReportGenerator` |
| Bounded parallel | Mode + policy + per-school context *design* | `Parallel.ForEachAsync(max)` |
| Run history | Two tables + Runs page | One table on the Reports page |
| Tagged PDF | Separate renderer + validation step | Structure tags; validation note in evidence |
| Accessible UI | Five pages, full WCAG pass as a phase | Two pages; labels/keyboard/headings first |
| Automated tests | Four test assemblies | Characterization + calculator tests |
| Playwright | Multi-flow E2E on the critical path | One import → generate → download test |

---

## Explicitly out of scope (both plans)

- Redesigning sections, labels, or suppression.
- Resolving the 18 characterization TODOs by guessing.
- Matching Test University 2024 PDF year-strings.
- Pixel-identical SAS ODS styling.
- Auth, multi-tenant, cloud, Hangfire, Redis, Kafka, microservices, MediatR, CQRS, per-entity repositories.

---

## How to read the two plan files

| File | Meaning |
|---|---|
| `docs/architecture/implementation-plan.md` | Original AI plan. **Rejected.** Historical record only. |
| `docs/architecture/corrected-plan.md` | **Approved.** Human-directed MVP. **Use this to implement.** |

No projects, entities, or UI were created from this status change. Start only when the human says to begin (calculator + `n ge 5` tests first).
