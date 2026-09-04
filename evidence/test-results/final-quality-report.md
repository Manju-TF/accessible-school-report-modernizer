# Final quality report

**Date:** 4 September 2026  
**Branch:** `feature/report-modernization`  
**Method:** `dotnet build`, `dotnet test`, and `scripts/verify-legacy-integrity.ps1`. No source code was modified.

This report records what was run and what passed or failed. It does not claim the UI or generated PDFs are accessible.

---

## Verdict

**Not clean.**

The solution builds with no warnings. Characterization tests and unit tests pass. Legacy files match the immutable baseline. Database creation, Excel import, single-school generation, sequential generate-all, and parallel generate-all are covered by passing integration tests.

`dotnet test` on the full solution **fails** because of one integration test: `LegacyModernParityTests.CharacterizedMetrics_MatchBetweenLegacyPdfAndModernCalculator`. That test compares the baseline PDF (100 graduates) to the sample workbook school `23306` (31 graduates). It is a population mismatch, not a build defect and not a salary-suppression regression.

---

## Commands run

| Command | Configuration | Result |
|---|---|---|
| `dotnet build AccessibleSchoolReports.sln` | Debug (default) | Succeeded. 0 warnings, 0 errors. 18.6 s |
| `dotnet build AccessibleSchoolReports.sln --configuration Release` | Release | Succeeded. 0 warnings, 0 errors. 28.9 s |
| `dotnet test AccessibleSchoolReports.sln --configuration Release --no-build` | Release | Failed. 1 failed, 322 passed, 19 skipped |
| `dotnet test` (integration project, filter `!~LegacyModernParityTests`) | Debug | Passed. 30/30 |
| `scripts/verify-legacy-integrity.ps1` | — | OK. 6/6 files match |

No compiler warnings were emitted. None indicated defects.

---

## Test totals

| Suite | Failed | Passed | Skipped | Total |
|---|---:|---:|---:|---:|
| Characterization | 0 | 172 | 18 | 190 |
| Unit | 0 | 120 | 1 | 121 |
| Integration (full) | 1 | 30 | 0 | 31 |
| **Solution** | **1** | **322** | **19** | **342** |

### Failed test

`AccessibleSchoolReports.IntegrationTests.Parity.LegacyModernParityTests.CharacterizedMetrics_MatchBetweenLegacyPdfAndModernCalculator`

- 320 of 560 metrics mismatched; 239 matched; 1 unresolved.
- School selection: no PDF-identity match. Used sample school `23306` (most matching characterized counts: 21).
- Example: `total.count` CF-C-01 legacy=100, modern=31.

Documented in `evidence/test-results/parity-results.md`. Do not change the calculator to chase these numbers. Salary suppression remains `n ge 5` (`CF-S-00`).

### Skipped tests

Skipped items are parked characterization TODOs (ambiguous SAS) plus one unit skip for an undefined `$jobcat1` format. They are not failures.

Characterization (18): D3 salary-n vs count; D3 subtotal; jobftpt format; own-practice timing; ZAFTGRD vs ZAFTGR; MINORF vs `MINOR F`; solo salary mechanism; extra FT/long-term salary filter; employed FT filter; raw-salary mean/percentiles; PDF 2024 vs SAS 2025 title; baseline PDF school code; D1 intended vs written exclusions; region `ge 0` after zero→X; exact `jobst` membership.

Unit (1): `LegacyRecodesTests.NormalizeJobFtPt_UndefinedJobcat1Format`.

---

## Verification

### No test failures (required)

**Not met** for the full solution. One integration parity test failed, as above.

Characterization: no failures. Unit: no failures. All other integration tests: no failures.

### No build warnings that indicate defects

**Met.** Debug and Release builds: 0 warnings, 0 errors.

### Legacy integrity

**Met.**

```text
Legacy integrity check
Expected files: 6
Actual files:   6
OK: all 6 legacy files match the IMMUTABLE baseline.
```

Manifest: `docs/capstone/legacy-baseline.md`. Files under `legacy/sas`, `legacy/samples`, and `legacy/baseline` were not modified.

### Database creation

**Met** (integration tests).

| Test | Result |
|---|---|
| `SchoolReportsDbContextTests.Migrate_CreatesDatabaseFile` | Passed |
| `SchoolReportsDbContextTests.ConnectionString_IsReadFromConfiguration` | Passed |
| `SchoolReportsDbContextTests.InsertsSchoolImportAndGraduate_WithForeignKeys` | Passed |
| `SchoolReportsDbContextTests.Graduate_WithUnknownSchool_FailsForeignKey` | Passed |
| `SchoolReportsDbContextTests.SchoolCode_IsUnique` | Passed |
| `SchoolReportsDbContextTests.ReportRunItem_StoresOutputPath_NotPdfBytes` | Passed |
| `SchoolReportsDbContextTests.SaveChangesAsync_HonorsCancellation` | Passed |
| `SchoolReportsDbContextTests.MigrateAsync_HonorsCancellation` | Passed |

### Excel import

**Met** (integration tests).

| Test | Result |
|---|---|
| `ExcelGraduateImportServiceTests.Import_PersistsValidRowsAndImportRun` | Passed |
| `ExcelGraduateImportServiceTests.Import_CapturesInvalidRows_AndPersistsValidOnes` | Passed |
| `ExcelGraduateImportServiceTests.Import_IgnoresBlankRows` | Passed |
| `ExcelGraduateImportServiceTests.Import_MissingRequiredColumns_RecordsFailedRun` | Passed |
| `ExcelGraduateImportServiceTests.Import_SameFile_IsRejectedAsDuplicate` | Passed |
| `ExcelGraduateImportServiceTests.Import_FailedRun_DoesNotBlockRetryAfterFix` | Passed |
| `ExcelGraduateImportServiceTests.Import_SampleExport_PersistsAllRows` | Passed |

Parser unit tests (headers, blank rows, invalid salary, missing school code) also passed as part of the unit suite.

### Single report

**Met** (integration tests).

| Test | Result |
|---|---|
| `ReportGenerationServiceTests.GenerateSchoolReport_WritesPdfAndPersistsRunMetadata` | Passed |
| `ReportGenerationServiceTests.GenerateSchoolReport_UnknownSchool_DoesNotThrow_AndPersistsFailedRun` | Passed |
| `ReportGenerationServiceTests.GenerateSchoolReport_NoGraduates_DoesNotThrow_AndPersistsFailedItem` | Passed |
| `ReportGenerationServiceTests.GenerateSchoolReport_PdfFailure_DoesNotThrow_AndLeavesNoPartialFile` | Passed |
| `ReportGenerationServiceTests.GenerateSchoolReport_Cancelled_DoesNotThrow_AndPersistsCancelled` | Passed |

Calculator unit tests include `CF-S-00` suppression (`n < 5` omitted; `n >= 5` emitted; salary n vs headcount). PDF unit tests write a seven-page PDF and do **not** certify PDF/UA.

### Sequential reports

**Met** (integration tests).

| Test | Result |
|---|---|
| `ReportGenerationServiceTests.GenerateAllSequential_WritesEachEligibleSchool_AndPersistsTotals` | Passed |
| `ReportGenerationServiceTests.GenerateAllSequential_ContinuesAfterIndividualFailure` | Passed |
| `ReportGenerationServiceTests.GenerateAllSequential_NoEligibleSchools_PersistsZeroTotals` | Passed |
| `ReportGenerationServiceTests.GenerateAllSequential_ReportsProgressAfterEachSchool` | Passed |
| `ReportGenerationServiceTests.GenerateAllSequential_CancelledAfterFirstSchool_StopsAndPersistsCounts` | Passed |

### Parallel reports

**Met** (integration tests).

| Test | Result |
|---|---|
| `ReportGenerationServiceTests.GenerateAllParallel_WritesEachEligibleSchool_AndPersistsTotals` | Passed |
| `ReportGenerationServiceTests.GenerateAllParallel_ContinuesAfterIndividualFailure` | Passed |
| `ReportGenerationServiceTests.GenerateAllParallel_ClampsMaxDegreeOfParallelism` | Passed |
| `ReportGenerationServiceTests.GenerateAllParallel_Cancelled_DoesNotThrow_AndPersistsCancelled` | Passed |
| `ReportGenerationServiceTests.GenerateAllSequential_AndParallel_ProduceEquivalentResults` | Passed |

Sequential and parallel results were compared on PDF **text**, not bytes.

---

## Out of scope for this run

- No source changes.
- No Playwright UI run.
- No veraPDF / PAC / screen-reader PDF validation.
- No claim that generated PDFs are accessible.
- The failing parity test was not “fixed.” Changing calculator rules to match a different school’s PDF would break characterization.

---

## Summary

| Check | Status |
|---|---|
| Debug build, 0 warnings | Pass |
| Release build, 0 warnings | Pass |
| Characterization tests | Pass (172 / 18 skipped) |
| Unit tests | Pass (120 / 1 skipped) |
| Integration tests except parity | Pass (30) |
| Full `dotnet test` | **Fail** (1 parity test) |
| Legacy integrity | Pass |
| Database creation | Pass |
| Excel import | Pass |
| Single report | Pass |
| Sequential reports | Pass |
| Parallel reports | Pass |
