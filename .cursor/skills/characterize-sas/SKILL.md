---
name: characterize-sas
description: >-
  Characterize legacy SAS before changing recodes, counts, salaries, suppression,
  or printed report numbers. Use when implementing or editing a CF-* or SS-* rule,
  SchoolReportCalculator, SasUnivariate, LegacyRecodes, characterization tests,
  or when the user mentions SAS, salary suppression, n ge 5, or CF-S-00.
---

# Characterize SAS Before Implementing

Do not invent business rules. `/legacy` is read-only. Copy working files into `src/`, `data/`, or `evidence/` if needed.

## Workflow

1. **Name the Rule ID** (`CF-*` or `SS-*`) from `docs/capstone/business-rules.md`.
2. **Read the source analysis**, not only the C# method:
   - Builder: `docs/capstone/createschrptfiles-analysis.md` → `legacy/sas/createschrptfiles2025.sas`
   - Printer: `docs/capstone/schreptsummary-analysis.md` → `legacy/sas/schreptsummary_2025.sas`
   - Printed cells: `docs/capstone/report-map.md`
3. **Open the SAS** and quote the observed statements. If `/legacy` is missing on this machine, use the analysis docs and say so. Never recreate SAS from memory.
4. **Record ambiguity** in the same three parts: what was observed, what is unclear, conservative interpretation.
5. **State the change plan** (files, rules, tests, risks) before editing `src/`.
6. **Implement in C#** to match the characterized rule. Do not “clean up” SAS behavior.
7. **Add or update a characterization test** that locks the observed output.

## Hard rules

- Salary suppression is **`CF-S-00`**: keep salary stats only when univariate `n >= 5` on non-missing `salftperm`. Code: `LegacyRecodes.SalarySuppressionMinimumN` and `SasUnivariate.MeetsSalarySuppression`.
- The PDF note “at least five salaries” (`SS-SUP-01`) is the same threshold, not a second one.
- Missing printed values stay `.` (screen-reader name: Not displayed).
- Do not emit salaries when `n < 5`. That change was rejected (`docs/decisions/rejected-ai-proposals.md`).
- Inactive commented SAS stays inactive. Test that it is **not** applied.

## Where the C# lives

| Concern | Path |
|---|---|
| Recodes / `n ge 5` constant | `src/AccessibleSchoolReports.Domain/Recodes/LegacyRecodes.cs` |
| Univariate + suppression gate | `src/AccessibleSchoolReports.Domain/Reporting/SasUnivariate.cs` |
| School report assembly | `src/AccessibleSchoolReports.Application/Reporting/SchoolReportCalculator.cs` |
| Characterization tests | `tests/AccessibleSchoolReports.CharacterizationTests/` |

## Tests to run

```powershell
dotnet test tests/AccessibleSchoolReports.CharacterizationTests
dotnet test tests/AccessibleSchoolReports.UnitTests --filter "FullyQualifiedName~SasUnivariate|FullyQualifiedName~LegacyRecodes"
```

If the change can affect printed cells, also run the matching calculator/unit tests. Do not run `LegacyModernParityTests` unless the user asks and `/legacy` plus the sample workbook are present.

## Stop and ask

Human approval is required before changing a CONFIRMED or AMBIGUOUS rule, the suppression threshold, percentile algorithm, or any filter not written in SAS.
