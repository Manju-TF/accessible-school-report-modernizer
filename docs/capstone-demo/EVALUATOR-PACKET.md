# Capstone evaluator packet

Honest status of required outcomes. Paths are relative to the repository root.

**Product name in the UI:** Meridian Test Client  
**Repository:** https://github.com/Manju-TF/accessible-school-report-modernizer

---

## Required outcomes

| Requirement | Status | Where to look |
|---|---|---|
| AI-assisted build with human review | Done | `.cursor/rules/`, Git history, `docs/decisions/rejected-ai-proposals.md` |
| Saved specification | Done | `docs/`, this folder, `README.md` |
| Corrected AI plan (not the first over-built plan) | Done | `docs/architecture/corrected-plan.md` (approved); `docs/architecture/implementation-plan.md` (historical) |
| Legacy SAS characterized, not invented | Done | `docs/capstone/createschrptfiles-analysis.md`, `schreptsummary-analysis.md`, `business-rules.md` |
| Characterization tests | Done | `tests/AccessibleSchoolReports.CharacterizationTests` (18 skipped where SAS is ambiguous) |
| .NET 8 modernization | Done | `src/` Domain, Application, Infrastructure, Web |
| SQLite only | Done | `SchoolReportsDbContext`; `data/schoolreports.db` gitignored |
| Excel import + validation | Done | `ExcelGraduateImportService`; Admin `/import` |
| Characterized calculations | Done | `SchoolReportCalculator`, `LegacyRecodes`, `SasUnivariate` |
| Salary suppression `n ge 5` (`CF-S-00`) | Done | Calculator tests; rejected AI proposal to emit salaries when `n < 5` |
| Single-school PDF | Done | `/generate` |
| Batch generate (sequential + bounded parallel) | Done | `/generate-all`, max parallelism 1–8 |
| Run history | Done | `/runs` |
| Tagged PDF / PDF-UA **target** | Done as a target | `QuestPdfAccessiblePdfGenerator` |
| Adobe Acrobat accessibility checker (reading order) | Observed pass | `evidence/test-results/adobe-acrobat-accessibility-check.md` |
| Visual parity with SAS / GrayscalePrinter | Targeted | Seven letter pages; do not restyle. Baseline file is local `/legacy` only |
| Accessible-enough UI (WCAG 2.2 AA principles) | Reviewed, not certified | React SPA in `src/web`; `docs/accessibility/ui-accessibility-review.md` |
| Authentication | Done | ASP.NET Core Identity cookie `.asr.auth` |
| Authorization + school grants | Done | Admin / ReportUser / Viewer; `UserSchoolAccess` |
| Protected PDF download | Done | `/downloads/reports/{id}` → 404 if unauthorized |
| Knowledge Assistant / RAG | Done | Catalog + generated PDFs; not the calculator |
| Authorization-aware RAG | Done | `KnowledgeAccess` before scoring / LLM |
| RAG evaluation (observed) | Done | `evidence/test-results/rag-evaluation.md` |
| Security tests | Done | `evidence/test-results/security-test-results.md` |
| CI quality gate | Done | `.github/workflows/quality.yml` (excludes known-failing baseline parity test) |
| Human-rejected AI change on record | Done | `docs/decisions/rejected-ai-proposals.md` |
| Baseline PDF vs sample Excel parity | **Not a pass** | Different populations; `evidence/test-results/parity-results.md` |
| Playwright test project / CI job | **Not implemented** | Playwright MCP used in Cursor sessions only |
| veraPDF / PAC / NVDA pack | **Not in this repository** | Adobe Acrobat checker is the recorded validation |
| Formal performance benchmark | **Not recorded** | Run History stores duration only |

---

## Traceability (SAS → rule → code → test)

| Rule ID | Meaning | Code | Test |
|---|---|---|---|
| CF-S-00 | Keep salary univariate only when `n ge 5` on non-missing `salftperm` | `SasUnivariate`, `SchoolReportCalculator` | `Salary_IsSuppressedWhenNIsBelow5` |
| SS-SUP-01 | Printed note: at least five salaries | Same threshold as CF-S-00 | `ReportNote_StatesTheSameFiveSalaryRule_NotASecondThreshold` |
| CF-PREP-06 | `sex3`: `W`→`F`, `X`→`N`, `ND`→ blank | `LegacyRecodes` | Characterization + `LegacyRecodesTests` |
| CF-C-01 | Total reported = frequency of school `code` | `SchoolReportCalculator` | Calculator + gender/count characterization |
| SS-FIL-01…07 | Seven page filters | `SchoolReportLayout` | `SchoolReportLayoutTests` |

Full catalogue: `docs/capstone/business-rules.md`.

---

## Architecture (what shipped)

```text
React SPA (src/web)  →  ASP.NET Core APIs + Identity
        ↓
Application (calculator, import, knowledge contracts)
        ↓
Domain (entities, recodes, univariate)
        ↑
Infrastructure (EF/SQLite, ClosedXML, QuestPDF, RAG)
        ↓
SQLite  +  output PDFs  +  optional OpenAI-compatible chat
```

`/legacy` is immutable, gitignored, and local-only. Never edit it.

---

## What evaluators should not hear

- “The PDF is PDF/UA-certified by veraPDF / PAC / NVDA” (those packs are not in the repo). Adobe Acrobat checker + reading order is the recorded check.
- “The assistant calculated the employment totals” (calculator is C#; assistant quotes printed PDF text or documented rules).
- “We matched the Test University baseline headcount” (known subject mismatch).
- Professional-association names in UI or generated PDFs.

---

## Suggested questions for the student

1. Why is salary suppression `n ge 5` on `salftperm`, not on headcount?
2. Why are eighteen characterization tests skipped?
3. Why does the baseline parity test fail, and why was the calculator not changed?
4. How does a Viewer fail to download another school’s PDF?
5. What is indexed for RAG, and what is never indexed (Excel rows)?
6. What did a human reject from the AI, and why?
