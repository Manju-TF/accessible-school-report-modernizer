# PDF accessibility strategy

This document describes how the modernizer *targets* a tagged, PDF/UA-oriented school report. It does **not** certify that generated files are accessible or PDF/UA conformant.

Generation and validation are separate. Passing a generate test is not an accessibility claim.

## Target

`IAccessiblePdfGenerator` / `QuestPdfAccessiblePdfGenerator` renders a calculated `SchoolReport` with QuestPDF semantic tags and PDF/UA-1 *conformance settings*. The calculator is not used inside the renderer.

Business content follows `docs/capstone/report-map.md` and 2025 SAS chrome (`SS-HDR-*`, `SS-NOTE-*`, `SS-FTR-01`). Layout is not a pixel match of `legacy/baseline/test-school-report.pdf`.

## What generation does

| Requirement | How it is targeted | What that is not |
|---|---|---|
| Tagged PDF | QuestPDF `SemanticSection`, headings, paragraphs, `SemanticTable`, captions, links | A veraPDF or PAC pass |
| PDF/UA-oriented structure | `DocumentSettings.PDFUA_Conformance = PDFUA_1` and `PDFA_3A` | A certified PDF/UA-1 file |
| Document title | `DocumentMetadata.Title` = school name + Class of 2025 Summary Report | Proof a reader will announce it |
| Document language | `DocumentMetadata.Language` = `en-US` | Proof a screen reader uses the voice |
| Logical headings | H1 school name, H2 page title, H3 section titles | A checked outline in Acrobat |
| Correct reading order | One column: header → sections → note → footer | A PAC reading-order check |
| Semantic tables | `SemanticTable` plus `table.Header` column titles | Proof every TH/TD association is correct |
| Table headers | Column header row; first-column category cells tagged `SemanticHorizontalHeader` | Manual header-scope review |
| No color-only information | Missing/suppressed values print as **Not displayed**; headers also use bold + text | A contrast-audit pass |
| Meaningful text | SAS `.` and blank `$newvar` labels are replaced with words (see below) | A language/clarity review |
| Page structure | Always seven pages; SAS `ANALVAR` slices; empty groups omitted (`SS-FIL-08`) | Pixel layout of the SAS ODS PDF |

### Accessible wording that differs from SAS print

These are presentation substitutions. They do not change stored counts or salaries.

- SAS missing numeric `.` → `Not displayed`
- Blank JOBREG3 row label → `# States and Territories with Employed Grads`
- Blank funded `YES` label → `Funded by law school`
- 2025 SAS title/footer years (Class of **2025**, July **2026**), not the 2024 baseline artifact years

Salary suppression remains the stored `n ge 5` result from the calculator (`CF-S-00`). The renderer does not apply a second threshold.

## Automated validation

Automated tests **must** fail when generation or business content is wrong. They **must not** be treated as PDF-UA certification.

Covered today (`QuestPdfAccessiblePdfGeneratorTests`, `SchoolReportLayoutTests`):

- Bytes are a PDF (`%PDF`)
- Seven pages
- Document title is set
- Extracted text includes school name, Total Reported, section headings, characterized notes/footer, table column names
- Empty Education Jobs section is omitted
- Suppressed salaries and unused duration cells appear as `Not displayed`
- Renderer consumes a `SchoolReport` fixture (and separately a calculator result) without recalculating

Not covered by automated tests:

- Tag tree completeness
- PDF/UA-1 or PDF/A-3a machine conformance
- Screen-reader announcement of headers, tables, and lists
- Reading order artifacts
- Contrast ratios
- Bookmark/outline quality beyond heading tags existing in code
- veraPDF, PAC, Adobe Accessibility Checker, or NVDA/JAWS output

Do not add a green test named “PDF is accessible.”

## Manual accessibility validation

Manual checks are required before anyone can say a file is usable with assistive technology. Record the tool, date, file hash, and findings. Failed checks stay failed.

Suggested (not claimed as done here):

1. **veraPDF** — PDF/UA-1 and/or PDF/A-3a machine conformance. Save the report under `evidence/` if a run is performed.
2. **PAC (PDF Accessibility Checker)** — reading order, heading levels, table structure, language, title display.
3. **Screen reader** (NVDA or JAWS) — open one generated school PDF; navigate headings; read one salary table including a `Not displayed` cell; confirm the note is read after the tables.
4. **Keyboard / viewer** — bookmarks jump to pages; text can be selected in order; link to `www.nalp.org/erssinfo` works.
5. **Visual (not pixel match)** — grayscale/high-contrast text; no meaning that exists only as color.

Until those results exist, the correct statement is:

> The generator **targets** tagged PDF and PDF/UA-oriented structure. Accessibility has **not** been validated on the output.

## Independence from calculation

```
Graduate rows → SchoolReportCalculator → SchoolReport
SchoolReport → SchoolReportLayout (labels/pages) → QuestPdfAccessiblePdfGenerator → PDF
```

The PDF layer must not import graduate rows, apply recodes, or recompute univariate statistics. Layout tests use a hand-built `SchoolReport` so a calculator change cannot silently rewrite presentation rules.

## Out of scope for this MVP

- Pixel-perfect SAS fonts, rules, and wrapping
- Claiming PDF-UA because QuestPDF settings were set
- Embedding a veraPDF CI gate (optional later; evidence only)
- Changing characterized salary suppression or inventing missing `$newvar` keys
