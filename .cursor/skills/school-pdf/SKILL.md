---
name: school-pdf
description: >-
  Generate and change school summary PDFs while keeping SAS/GrayscalePrinter
  visual parity and PDF/UA tags. Use when editing QuestPdfAccessiblePdfGenerator,
  SchoolReportLayout, report year chrome, tagged PDF, PDF/UA, visual compare,
  or when the user mentions school PDF, baseline PDF, or page layout.
---

# School PDF Visual Parity

Generated PDFs must look like the SAS GrayscalePrinter report. Accessibility is tags, language, reading order, and alternative text — not a new look.

Do not modify `/legacy`. Do not change calculator or suppression rules to “fit” the page.

## Preserve

- Seven letter pages, one table per page
- Centered Bold Italic school name and report title
- Gray header cells, black grid, 8pt body / 9pt headers
- `Total Reported` as a table row under the headers on page 1
- Note, then test-client footer, in content flow (not duplicated or bottom-pinned)
- Year chrome from the 2025 SAS program: **Class of 2025**, July **2026**
- Missing values as `.` (screen-reader name: Not displayed)
- Branding: **Meridian Test Client**. Do not print professional-association names in the PDF or UI chrome

The file `legacy/baseline/test-school-report.pdf` is a **layout** reference. Do not copy its 2024 / July 2025 artifact years.

## Code map

| Concern | Path |
|---|---|
| Tagged PDF renderer | `src/AccessibleSchoolReports.Infrastructure/Pdf/QuestPdfAccessiblePdfGenerator.cs` |
| Page/section layout | `src/AccessibleSchoolReports.Application/Reporting/SchoolReportLayout.cs` |
| Calculator (do not restyle here) | `src/AccessibleSchoolReports.Application/Reporting/SchoolReportCalculator.cs` |
| Output path | `src/AccessibleSchoolReports.Web/output/{year}/{schoolCode}/summary-report.pdf` |
| Strategy (target, not a certificate) | `docs/accessibility/pdf-accessibility-strategy.md` |
| Section map | `docs/capstone/report-map.md` |

## Workflow

1. State the change plan (files, rules, tests, risks). Wait if layout or printed values change.
2. Edit renderer/layout only. Keep numbers coming from the calculator.
3. After generate, compare against the seven-page baseline layout — not a redesigned template.
4. Add or update `SchoolReportLayoutTests` / `QuestPdfAccessiblePdfGeneratorTests`.
5. Report PDF/UA or tag-validation results **separately**. Never claim the PDF is accessible without that evidence.

## Tests

```powershell
dotnet test tests/AccessibleSchoolReports.UnitTests --filter "FullyQualifiedName~SchoolReportLayout|FullyQualifiedName~QuestPdfAccessiblePdfGenerator"
```

Visual compare screenshots belong in `evidence/screenshots/pdf-compare/`. Do not write run logs into `docs/`.

## Stop and ask

Do not restyle fonts, colors, links, or footer placement. Do not add extra pages or drop a table to make content fit.
