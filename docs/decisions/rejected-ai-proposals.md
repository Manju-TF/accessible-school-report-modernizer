# Rejected AI proposals

Human-reviewed AI suggestions that must not be implemented. These stay on record so the same change is not reintroduced.

---

## Salary statistics below five observations

**Date:** 4 September 2026

### Proposed change

Calculate and display salary statistics (n, 25th percentile, median, 75th percentile, mean) when a by-group has fewer than five non-missing `salftperm` values.

### Affected files

If accepted, the change would have touched:

- `src/AccessibleSchoolReports.Domain/Reporting/SasUnivariate.cs` (`SuppressUnlessEligible`)
- `src/AccessibleSchoolReports.Application/Reporting/SchoolReportCalculator.cs` (`MergeSalary`)
- `src/AccessibleSchoolReports.Application/Reporting/SchoolReportPresentation.cs` (display of suppressed cells)
- `src/AccessibleSchoolReports.Infrastructure/Pdf/QuestPdfAccessiblePdfGenerator.cs` (printed salary columns)

No files were changed. The current code still omits salary statistics when `n < 5`.

### Affected SAS rule

**CF-S-00** — `createschrptfiles2025.sas`

Every salary `DATA` step keeps the univariate row only with `WHERE n ge 5`. `n` is the count of non-missing `salftperm` in that by-group, not the employment headcount.

The printed note **SS-SUP-01** (“at least five salaries are required”) is the same rule in words, not a second threshold.

### Test that would fail

`AccessibleSchoolReports.UnitTests.Reporting.SchoolReportCalculatorTests.Salary_IsSuppressedWhenNIsBelow5`

Related tests that would also fail:

- `SasUnivariateTests.SuppressUnlessEligible_RequiresNGe5`
- `SchoolReportCalculatorTests.SalarySuppression_UsesSalaryN_NotHeadcount`
- `SalarySuppressionTests.SalaryRow_KeptOnlyWhenNGe5`
- `SalarySuppressionTests.BaselinePdf_MenOfColor_Count4_HasNoSalaryCells`
- `SchoolReportLayoutTests.Compose_UsesNotDisplayed_ForSuppressedSalaries`

### Human decision

**REJECTED.**

### Reason

The legacy SAS behavior suppresses salary statistics below the required minimum population.

Changing this behavior would break characterization parity.
