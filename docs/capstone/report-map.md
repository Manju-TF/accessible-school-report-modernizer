# Report Map — `legacy/baseline/test-school-report.pdf`

Source PDF: `legacy/baseline/test-school-report.pdf` (IMMUTABLE). This file was not modified.

This document maps the baseline PDF into logical sections and compares that layout to the SAS report program characterized in `docs/capstone/schreptsummary-analysis.md`. Count and salary **values** on this PDF come from the builder characterized in `docs/capstone/createschrptfiles-analysis.md`. Rule IDs below are from `docs/capstone/business-rules.md`.

This is **not** a redesign. No layout, label, or calculation is proposed. Missing categories are recorded as absent, not as defects to “fix.”

Classification used below:

- **CONFIRMED** — visible on this PDF and/or required by the SAS report statements.
- **INFERRED** — suggested by a note, bookmark, or title, not proven as extra SAS logic.
- **AMBIGUOUS** — this PDF and the 2025 SAS disagree, or the PDF shows a value the SAS alone does not explain.

---

## Artifact identity

| Item | Observed on this PDF |
|---|---|
| Path | `legacy/baseline/test-school-report.pdf` |
| SHA-256 | `f27b3b52258aa4b23e2216b8ec4077ce979677897553b3b142a468d9e21cb15b` (see `legacy-baseline.md`) |
| Pages | 7, letter (612 × 792 pt), one table procedure per page |
| Producer | SAS Institute Inc. |
| Creator | `9.04.01M7P08052020` (SAS 9.4) |
| Creation date | 2026-06-01 |
| School title | `Test University School of Law` |
| Report title | `Class of 2024 Summary Report` (page 1); pages 2–7 append `- Page N` |
| Footer date line | `Table prepared by NALP, July 2025` |
| Bookmarks | `Page 1` … `Page 7`, each with SAS defaults `Detailed and/or summarized report` and `Table 1` |

This PDF is a **test-school** artifact, not one of the live `%SCHRPTS` school files named in `schreptsummary_2025.sas`.

Year strings on this PDF do **not** match the 2025 SAS source:

| Element | This PDF | `schreptsummary_2025.sas` |
|---|---|---|
| TITLE2 | Class of **2024** | Class of **2025** (`SS-HDR-02`, `SS-HDR-03`) |
| NALP table line | July **2025** | July **2026** (`SS-FTR-01`) |

Treat those as class-year / issuance-string differences between this baseline file and the 2025 program. Do not invent a new title rule from the PDF, and do not change the SAS characterization to 2024.

---

## Shared chrome (every page)

### Page frame

| Field | Value |
|---|---|
| Section name | Page chrome |
| Order | Applies to all 7 pages |
| Displayed fields | TITLE1 school name; TITLE2 report title; column headers; section headers from `$recode`; note block; NALP table line; ABA/NALP disclaimer; `www.nalp.org/erssinfo` |
| Grouping | Not a data group |
| Count / percentage / salary / subtotal | None at chrome level |
| Suppression | None |
| Notes | Page-specific note under each table (see sections) |
| Footer | Same three-part footer on every page (below) |
| Page relationship | One `PROC REPORT` → one PDF page (`SS-PAGE-02`) |

### Column sets

**Pages 1–5** (`SS-RPT-01`–`05`, `SS-SAL-06`):

| Column | SAS field | Header on PDF |
|---|---|---|
| Row label | `Newvar` via `$newvar` | unlabeled category column |
| Count | `Count` SUM | Number Reported |
| Percent | `Percent` SUM | % of Reported |
| Salary n | `N` DISPLAY | # with Salary |
| Q1 | `Pct25` DISPLAY | 25th Percentile |
| Median | `Median` DISPLAY | Median |
| Q3 | `Pct75` DISPLAY | 75th Percentile |
| Mean | `Mean` DISPLAY | Mean |
| Spanning header | — | Full-time Long-term Salaries (over the five salary columns) |

**Page 6** (`SS-RPT-06`): Number Reported, % of Reported only. No salary block.

**Page 7** (`SS-RPT-07`): spanning header `Number of Jobs Reported as:` over `Long-term (1+ years)` and `Short-term (Less than 1 year)`. No percent. No salary block.

### Missing-value mark

Empty numeric cells print as a period (`.`). That is the SAS missing-value display. It appears for unmerged salary columns and for unused page-7 duration cells.

### Footer (all pages)

| Field | Value |
|---|---|
| Section name | Page footer |
| Order | After the page note, every page |
| Displayed fields | (1) `Table prepared by NALP, July 2025`; (2) ABA/NALP disclaimer paragraph; (3) `www.nalp.org/erssinfo.` |
| SAS rule | `SS-FTR-01` (wording). Date year on this PDF is 2025, not the 2026 string in the 2025 SAS. |
| Grouping / count / percentage / salary / suppression / subtotal | None |
| Notes | Disclaimer is footer text, not a data note |
| Page relationship | Repeated after each of the seven tables |

Disclaimer text (observed):

> NALP Summary Report data may vary slightly from the school-specific data published by the ABA because of definitional differences between the two organizations and because NALP's quality control process can result in changes which may not be reflected in ABA data. For more on this, see www.nalp.org/erssinfo.

---

## Logical sections

Order is reading order through the PDF. A section that the 2025 SAS can print, but that is **absent** here because the input had no rows, is still listed (`SS-FIL-08` / `SS-SUP-02`).

Row order inside a section is SAS **unformatted** `newvar` order (`SS-GRP-02`), then `$newvar` labels.

### 1 — Total Reported banner

| Field | Value |
|---|---|
| Section name | Total Reported |
| Order | 1 (page 1 only, above Gender) |
| Displayed fields | `Total Reported = 100` |
| Grouping | Not grouped. Value is the `ANALVAR='A'` count stored in `Ct_<code>` (`SS-PREP-02`, `SS-HDR-07`, `SS-CNT-02`, `CF-C-01`) |
| Count | 100 |
| Percentage | Not shown |
| Salary fields | None |
| Suppression | None |
| Subtotal | None |
| Notes | None on this line |
| Footer | Page 1 footer |
| Page relationship | Page 1 only. `ANALVAR='A'` is **not** a printed table section (`SS-FMT-02` maps `A` to a blank title and page 1 does not select `A`) |

### 2 — Gender Reported

| Field | Value |
|---|---|
| Section name | Gender Reported |
| Order | 2 |
| Displayed fields | Women; Men |
| Grouping | `analvar='B'` (`SS-GRP-01`, `CF-C-02`, `CF-S-01`) |
| Count | Women 46; Men 54 |
| Percentage | 46.0; 54.0 |
| Salary fields | Women: n 40, 70,000 / 85,500 / 110,000 / 85,802. Men: n 30, 25th printed without a comma in the text layer (`850000` pieces), 90,500 / 120,000 / 103,401 |
| Suppression | Non-binary row not shown (no graduates). Salary cells would be `.` if `n < 5` (`CF-S-00`) |
| Subtotal | 100 / 100.0. Salary columns not subtotaled (`SS-SUB-01`) |
| Notes | Page 1 note (section 6) |
| Footer | Page 1 footer |
| Page relationship | Page 1, first `$recode` group (`SS-FIL-01`) |

`$newvar` label that did **not** appear: `Non-binary or Chose to Self-identify` (`N`).

### 3 — Race Reported

| Field | Value |
|---|---|
| Section name | Race Reported |
| Order | 3 |
| Displayed fields | People of Color; White |
| Grouping | `analvar='C'` (`CF-C-03`, `CF-S-02`). Internal order `MINOR` then `NONMIN` |
| Count | 24; 46 |
| Percentage | 34.3; 65.7 (of the 70 graduates with minority status known, not of Total Reported 100) |
| Salary fields | People of Color: n 8, 60,000 / 85,000 / 89,000 / 82,604. White: n 34, 80,000 / 86,000 / 95000 (comma missing in text layer) / 90,559 |
| Suppression | No other race rows exist in `$newvar` |
| Subtotal | 70 / 100.0 |
| Notes | Page 1 note |
| Footer | Page 1 footer |
| Page relationship | Page 1 |

### 4 — Gender & Race Reported

| Field | Value |
|---|---|
| Section name | Gender & Race Reported |
| Order | 4 |
| Displayed fields | Women of Color; Men of Color; White Women; White Men |
| Grouping | `analvar='C1'` (`CF-C-04`, `CF-S-03`) |
| Count | 10; 4; 30; 20 |
| Percentage | 15.6; 6.3; 46.9; 31.3 |
| Salary fields | Women of Color: n 6, 65,000 / 88,500 / 90000 / 84,673. Men of Color: all `.`. White Women: n 25, 75,000 / 80,000 / 101,000 / 95,983. White Men: n 15, 85,000 / 88,000 / 105,000 / 107,471 |
| Suppression | Men of Color count is 4 → salary omitted (`CF-S-00`, `SS-SUP-01`). Non-binary cross-tab rows not shown |
| Subtotal | 64 / 100.0. Text layer has a control character after `64` |
| Notes | Page 1 note |
| Footer | Page 1 footer |
| Page relationship | Page 1 |

On **this** PDF the cross-tab labels printed as Women/Men of Color and White Women/Men. That is evidence that, for this baseline file, `$newvar` matched the stored keys. The 2025 SAS mismatch (`MINOR F` vs builder `MINORF`, `SS-FMT-04`) remains **AMBIGUOUS** for the 2025 program; do not rewrite the format from this 2024 test PDF.

### 5 — Employment Status Known

| Field | Value |
|---|---|
| Section name | Employment Status Known |
| Order | 5 |
| Displayed fields | Bar Admission Required/ Anticipated (wrapped after `/`); JD Advantage; Other Professional; Other Position; Not Employed-Seeking; Not Employed-Not Seeking |
| Grouping | `analvar='D'` (`CF-C-05`, `CF-S-04`). Internal order of `1-LJD` … `9-UNWK` |
| Count | 78; 12; 1; 2; 3; 4 |
| Percentage | 78.0; 12.0; 1.0; 2.0; 3.0; 4.0 |
| Salary fields | LJD: n 69, 25th text layer `765000`, 88,000 / 110,000 / 90,397. JD Advantage: n 10, 72,250 / 93,500 / 104,200 / 100,500. Remaining rows: all `.` |
| Suppression | No salary on rows with count &lt; 5 or no merged salary. Absent `$newvar` rows: Job Type Unknown; Enrolled in Graduate Studies; Employed-Start Date after March 16, 2026 |
| Subtotal | Count prints as 100 / 100.0. Text layer has extra control characters around `100` |
| Notes | Page 1 note |
| Footer | Page 1 footer |
| Page relationship | Page 1, last data group |

### 6 — Page 1 note

| Field | Value |
|---|---|
| Section name | Page 1 note |
| Order | 6 |
| Displayed fields | Note paragraph (`SS-NOTE-01`) |
| Grouping | After all page 1 groups (`COMPUTE AFTER`) |
| Count / percentage / salary / subtotal | None |
| Suppression | States the five-salary rule and hidden empty categories |
| Notes | Observed wording matches `SS-NOTE-01`. PDF line-breaks glue some words (`non-binaryor`, `self-identifycategory`, `multiplegender`) |
| Footer | Immediately above the page footer |
| Page relationship | Page 1 only |

Note text (logical, spaces restored):

> Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. The non-binary or chose to self-identify category also includes graduates who selected multiple gender identities. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.

### 7 — Total Employed or Enrolled in Graduate Studies

| Field | Value |
|---|---|
| Section name | Total Employed or Enrolled in Graduate Studies |
| Order | 7 |
| Displayed fields | Employed |
| Grouping | `analvar='D1'` (`CF-C-06`, `CF-S-05`) |
| Count | 93 |
| Percentage | 93.0 (of Total Reported, not of this section alone) |
| Salary fields | n 83, 70,000 / 89,000 / 105,000 / 95,236 |
| Suppression | `Enrolled in Graduate Studies` (`6-ADVD`) not shown |
| Subtotal | 93 / 93.0 — same as the single detail row. Not forced to 100.0 |
| Notes | Page 2 note (section 11) |
| Footer | Page 2 footer |
| Page relationship | Page 2 first group (`SS-FIL-02`) |

### 8 — Employment by Sector

| Field | Value |
|---|---|
| Section name | Employment by Sector |
| Order | 8 |
| Displayed fields | Private Sector; Public Sector |
| Grouping | `analvar='D2'` (`CF-C-08`, `CF-S-06`, `CF-S-07`). Internal order `PRIVATE` then `PUBLIC` |
| Count | 78; 15 |
| Percentage | 83.9; 16.1 |
| Salary fields | Private: n 58, 84,000 / 95,500 / 106,000 / 101,425. Public: n 10, 66,000 / 73,950 / 92,000 / 78,532 |
| Suppression | No unknown-employer row (`EMPUNK` deleted in the builder) |
| Subtotal | 93 / 100.0 |
| Notes | Page 2 note defines private = law firms and business; all other jobs public; excludes unreported employer type |
| Footer | Page 2 footer |
| Page relationship | Page 2 |

### 9 — Full-time or Part-time Job Status

| Field | Value |
|---|---|
| Section name | Full-time or Part-time Job Status |
| Order | 9 |
| Displayed fields | Bar Admission Required/ Anticipated: Full-time; JD Advantage: Full-time; JD Advantage: Part-time; Other Professional: Full-time; Other Position: Full-time; Other Position: Part-time |
| Grouping | `analvar='D3'` (`CF-C-07`, `CF-S-19`) |
| Count | 79; 10; 2; 1; 1; 1 |
| Percentage | 84.9; 10.8; 2.2; 1.1; 1.1; 1.1 |
| Salary fields | LJD FT: n 69, 74,000 / 88,000 / 104,000 / 88,297. JD Advantage FT: n **12**, 70,250 / 93,500 / 104,200 / 99,325. Other rows: `.` |
| Suppression | Unknown-type FT/PT rows not shown. Part-time LJD not shown |
| Subtotal | **93 / 100.0** |
| Notes | Page 2 note |
| Footer | Page 2 footer |
| Page relationship | Page 2 |

**AMBIGUOUS on this baseline (do not invent a reconciliation):**

- Displayed D3 detail counts sum to **94**; printed subtotal is **93**.
- Page 2 LJD Full-time count is **79**; page 1 LJD count is **78**.
- JD Advantage Full-time `# with Salary` is **12** while Number Reported is **10**.

These are observed PDF values. They are consistent with “display stored columns” (`SS-CALC-01`, `SS-SAL-01`) plus whatever this test file stored. They are not a new business rule.

### 10 — Employment Categories

| Field | Value |
|---|---|
| Section name | Employment Categories |
| Order | 10 |
| Displayed fields | Business; Judicial Clerkships; Private Practice; Government; Public Interest |
| Grouping | `analvar='E1'` (`CF-C-09`, `CF-S-08`). Internal order `BUS`, `CLERK`, `FIRM`, `GOVT`, `PUBINT` |
| Count | 14; 5; 50; 16; 8 |
| Percentage | 15.1; 5.4; 53.8; 17.2; 8.6 |
| Salary fields | Business: n 13, 95,000 / 108,000 / 110,000 / 120,500. Clerkships: `.`. Private Practice: n 45, 82,000 / 93,000 / 105,000 / 96,901. Government: n 15, 65,000 / 84,000 / 93,000 / 82,010. Public Interest: n 6, 64,000 / 73,495 / 92,000 / 76,801 |
| Suppression | Education (`ACAD`) and Unknown Type (`ZEMPUN`) not shown. Clerkships headcount is 5 but salaries are `.` (salary `n < 5`) |
| Subtotal | 93 / 100.0 |
| Notes | Page 2 note |
| Footer | Page 2 footer |
| Page relationship | Page 2 last group. `ANALVAR='E'` is in `$recode` but is not selected by page 2’s character filter as a separate printed header on this PDF |

### 11 — Page 2 note

| Field | Value |
|---|---|
| Section name | Page 2 note |
| Order | 11 |
| Displayed fields | Note paragraph (`SS-NOTE-02`) |
| Grouping | After all page 2 groups |
| Count / percentage / salary / subtotal | None |
| Suppression | Repeats five-salary and hidden-category sentences |
| Notes | Adds public/private definition and “does not include graduates for whom employer type was not reported” |
| Footer | Page 2 footer |
| Page relationship | Page 2 |

### 12 — Education Jobs (absent)

| Field | Value |
|---|---|
| Section name | Education Jobs |
| Order | 12 (SAS page 3 first group; **not printed**) |
| Displayed fields | None |
| Grouping | Would be `analvar='E2'` (`SS-FIL-03`, `CF-C-10`, `CF-S-10`) |
| Count / percentage / salary / subtotal | None |
| Suppression | Entire section omitted because no `E2` rows (`SS-FIL-08`) |
| Notes | Page 3 note still prints |
| Footer | Page 3 footer |
| Page relationship | Expected on page 3; absent on this PDF |

### 13 — Business Jobs

| Field | Value |
|---|---|
| Section name | Business Jobs |
| Order | 13 |
| Displayed fields | Bar Admission Required/ Anticipated; JD Advantage; Other Professional; Other Position |
| Grouping | `analvar='E3'` (`CF-C-11`, `CF-S-09`) |
| Count | 2; 10; 1; 1 |
| Percentage | 14.3; 71.3; 7.1; 7.1 |
| Salary fields | Only JD Advantage: n 8, 95,000 / 108,000 / 109,200 / 109,185. Others `.` |
| Suppression | LJD count 2 &lt; 5 → no salary |
| Subtotal | 14 / 100.0 |
| Notes | Page 3 note (section 17) |
| Footer | Page 3 footer |
| Page relationship | Page 3 |

### 14 — Private Practice Jobs

| Field | Value |
|---|---|
| Section name | Private Practice Jobs |
| Order | 14 |
| Displayed fields | Bar Admission Required/ Anticipated; JD Advantage |
| Grouping | `analvar='E4'` (`CF-C-12`, `CF-S-11`) |
| Count | 46; 4 |
| Percentage | 92.0; 8.0 |
| Salary fields | LJD: n 40, 82,000 / 95,000 / 102,000 / 96,608. JD Advantage: `.` (count 4) |
| Suppression | Other job-type rows not shown |
| Subtotal | 50 / 100.0 |
| Notes | Page 3 note |
| Footer | Page 3 footer |
| Page relationship | Page 3 |

### 15 — Government Jobs

| Field | Value |
|---|---|
| Section name | Government Jobs |
| Order | 15 |
| Displayed fields | Bar Admission Required/ Anticipated; JD Advantage |
| Grouping | `analvar='E5'` (`CF-C-13`, `CF-S-12`) |
| Count | 13; 3 |
| Percentage | 81.3; 18.8 |
| Salary fields | LJD: n 11, 78,000 / 88,000 / 90,000 / 83,957. JD Advantage: `.` |
| Suppression | Other job-type rows not shown |
| Subtotal | 16 / 100.0 |
| Notes | Page 3 note |
| Footer | Page 3 footer |
| Page relationship | Page 3 |

### 16 — Judicial Clerkships

| Field | Value |
|---|---|
| Section name | Judicial Clerkships |
| Order | 16 |
| Displayed fields | State; Local |
| Grouping | `analvar='E55'` (`CF-C-14`, `CF-S-13`). Internal `emptype1` order |
| Count | 4; 1 |
| Percentage | 80.0; 20.0 |
| Salary fields | All `.` |
| Suppression | Federal, Tribal, Unknown, International not shown. Headcount 5 but no salary row (`n < 5`) |
| Subtotal | 5 / 100.0 |
| Notes | Page 3 note |
| Footer | Page 3 footer |
| Page relationship | Page 3 last group |

### 17 — Page 3 note

| Field | Value |
|---|---|
| Section name | Page 3 note |
| Order | 17 |
| Displayed fields | Note paragraph (`SS-NOTE-03`) |
| Grouping | After page 3 groups |
| Count / percentage / salary / subtotal | None |
| Suppression | Five-salary + hidden-category + FT long-term + solo sentences |
| Notes | No public/private extra sentences |
| Footer | Page 3 footer |
| Page relationship | Page 3 |

### 18 — Public Interest Jobs

| Field | Value |
|---|---|
| Section name | Public Interest Jobs |
| Order | 18 |
| Displayed fields | Bar Admission Required/ Anticipated; JD Advantage; Other Position |
| Grouping | `analvar='E6'` (`CF-C-15`, `CF-S-14`) |
| Count | 6; 1; 1 |
| Percentage | 75.0; 12.5; 12.5 |
| Salary fields | LJD: n 5, 66,000 / 71,405 / 88,000 / 73,821 (threshold met). Others `.` |
| Suppression | Other job-type rows not shown |
| Subtotal | 8 / 100.0 |
| Notes | Page 4 note (section 21) |
| Footer | Page 4 footer |
| Page relationship | Page 4 first group (`SS-FIL-04`) |

### 19 — Size of Law Firm (by # of Attorneys)

| Field | Value |
|---|---|
| Section name | Size of Law Firm (by # of Attorneys) |
| Order | 19 |
| Displayed fields | 1-10; 11-25; 26-50; 51-100; 101-250; 251-500; 501+ |
| Grouping | `analvar='FIRM'` (`CF-C-16`, `CF-S-15`). Internal `LF1`…`LF7` |
| Count | 30; 10; 3; 4; 1; 1; 1 |
| Percentage | 60.0; 20.0; 6.0; 8.0; 2.0; 2.0; 2.0 |
| Salary fields | 1-10: n 18, 78,000 / 84,000 / 90,000 / 83,710. 11-25: n 10, 81,000 / 87,000 / 101,000 / 89,300. Sizes with count &lt; 5: `.` |
| Suppression | Solo Practitioner and Unknown Size not shown. Note still says solo salaries were excluded (`SS-SUP-03` / `CF-AMB-05`) |
| Subtotal | 50 / 100.0 |
| Notes | Page 4 note |
| Footer | Page 4 footer |
| Page relationship | Page 4 |

### 20 — Type of Law Firm Job

| Field | Value |
|---|---|
| Section name | Type of Law Firm Job |
| Order | 20 |
| Displayed fields | Associate/Entry-level Attorney; Law Clerk |
| Grouping | `analvar='FIRM2'` (`CF-C-17`, `CF-S-16`). Labels `ATTY`, `LCLERK` |
| Count | 42; 8 |
| Percentage | 84.0; 16.0 |
| Salary fields | Associate: n 37, 88,000 / 92,000 / 101,000 / 98,074. Law Clerk: n 5, 76,000 / 80,500 / 92,000 / 81,803 |
| Suppression | Paralegal, Patent Agent, Manager/administrator, Staff Attorney, Other Non-attorney Position not shown |
| Subtotal | 50 / 100.0 |
| Notes | Page 4 note |
| Footer | Page 4 footer |
| Page relationship | Page 4 last group |

### 21 — Page 4 note

| Field | Value |
|---|---|
| Section name | Page 4 note |
| Order | 21 |
| Displayed fields | Same sentence set as page 3 (`SS-NOTE-03`) |
| Grouping | After page 4 groups |
| Count / percentage / salary / subtotal | None |
| Suppression | Same as page 3 note |
| Notes | — |
| Footer | Page 4 footer |
| Page relationship | Page 4 |

### 22 — Jobs Taken by Region

| Field | Value |
|---|---|
| Section name | Jobs Taken by Region |
| Order | 22 |
| Displayed fields | New England; Mid-Atlantic; South Atlantic; E South Central; Mountain |
| Grouping | `analvar='JOBREG1'` (`CF-C-18`, `CF-S-17`). Codes `1`, `2`, `5`, `6`, `8` |
| Count | 69; 16; 2; 1; 5 |
| Percentage | 74.2; 17.2; 2.2; 1.1; 5.4 |
| Salary fields | New England: n 57, 84,000 / 93,000 / 105,000 / 92,800. Mid-Atlantic: n 11, 57,950 / 84,000 / 101,000 / 92,727. South Atlantic / E South Central / Mountain: `.` |
| Suppression | E North Central, W North Central, W South Central, Pacific, US Territories, Non-US locations not shown. Mountain **count is 5** but salaries are `.` (salary `n`, not headcount, is the threshold) |
| Subtotal | 93 / 100.0 |
| Notes | Page 5 note (section 25) |
| Footer | Page 5 footer |
| Page relationship | Page 5 first group (`SS-FIL-05`) |

### 23 — Location of Jobs

| Field | Value |
|---|---|
| Section name | Location of Jobs |
| Order | 23 |
| Displayed fields | In-State; Out of State |
| Grouping | `analvar='JOBREG2'` (`CF-C-19`, `CF-S-18`) |
| Count | 63; 30 |
| Percentage | 67.8; 32.2 |
| Salary fields | In-State: n 51, 81,000 / 92,000 / 101,000 / 90,470. Out of State: n 20, 66,000 / 83,000 / 107,000 / 99,226 |
| Suppression | Foreign not shown |
| Subtotal | 93 / 100.0 |
| Notes | Page 5 note |
| Footer | Page 5 footer |
| Page relationship | Page 5 |

### 24 — # States and Territories with Employed Grads

| Field | Value |
|---|---|
| Section name | # States and Territories with Employed Grads |
| Order | 24 |
| Displayed fields | One detail row with a **blank** label (`$newvar` `JOBREG3` → spaces) showing count 14; percent and all salary cells `.` |
| Grouping | `analvar='JOBREG3'` (`CF-C-20`). No salary step in the builder for this key |
| Count | 14 |
| Percentage | `.` on the detail row |
| Salary fields | All `.` |
| Suppression | No extra state list; the figure is the stored count of states/territories |
| Subtotal | Label **Total #** (`SS-SUB-02`, `SS-FMT-03`). Count 14. Percent `.` |
| Notes | Page 5 note |
| Footer | Page 5 footer |
| Page relationship | Page 5 last group |

### 25 — Page 5 note

| Field | Value |
|---|---|
| Section name | Page 5 note |
| Order | 25 |
| Displayed fields | Same sentence set as pages 3–4 (`SS-NOTE-03`) |
| Grouping | After page 5 groups |
| Count / percentage / salary / subtotal | None |
| Suppression | Same as pages 3–4 |
| Notes | — |
| Footer | Page 5 footer |
| Page relationship | Page 5 |

### 26 — Source of Job

| Field | Value |
|---|---|
| Section name | Source of Job |
| Order | 26 |
| Displayed fields | Career office recruitment program (e.g., OCI); Job fair or career conference; Career office job posting; Non-career office job posting; Clerkship application process or OSCAR; Returned to or continued with pre-law school employer; Referral; Self-initiated contact/networking; Other |
| Grouping | `analvar='SOURCE'` on part2 (`CF-P2-03`, `SS-FIL-06`). Internal source-code order |
| Count | 4; 1; 10; 10; 2; 2; 10; 12; 39 |
| Percentage | 4.4; 1.1; 11.1; 11.1; 2.2; 2.2; 11.1; 13.3; 43.3 |
| Salary fields | None (page 6) |
| Suppression | Started own practice or business; Temp agency; Before Bar Results not shown |
| Subtotal | 90 / 100.0 |
| Notes | Page 6 note (section 29) |
| Footer | Page 6 footer |
| Page relationship | Page 6 first group |

### 27 — Timing of Job Offer

| Field | Value |
|---|---|
| Section name | Timing of Job Offer |
| Order | 27 |
| Displayed fields | Before graduation; After graduation |
| Grouping | `analvar='TIME'` (`CF-P2-04`) |
| Count | 49; 31 |
| Percentage | 61.3; 38.8 |
| Salary fields | None |
| Suppression | Other timing codes not shown |
| Subtotal | 80 / 100.0. Detail percents sum to 100.1 at one decimal — stored percents, then `SUM` (`SS-PCT-01`, `SS-PCT-02`) |
| Notes | Page 6 note says own-practice starts are excluded from timing. This PDF has no `SELFPR` source row. The report file has no `WHERE` on `SELFPR` (`SS-SUP-04`) |
| Footer | Page 6 footer |
| Page relationship | Page 6 |

On **this** PDF “After graduation” printed. The 2025 builder stores `ZAFTGRD` while `$newvar` lists `ZAFTGR` (`SS-FMT-04`). That key mismatch stays **AMBIGUOUS** for 2025; this baseline does not authorize changing either key.

### 28 — Search Status of Employed Grads

| Field | Value |
|---|---|
| Section name | Search Status of Employed Grads |
| Order | 28 |
| Displayed fields | Seeking a different job; Not seeking a different job |
| Grouping | `analvar='ZSTATUS'` (`CF-P2-05`). Internal `NOTSET` then `SET` |
| Count | 4; 86 |
| Percentage | 4.4; 95.6 |
| Salary fields | None |
| Suppression | No other status rows |
| Subtotal | 90 / 100.0 |
| Notes | Page 6 note |
| Footer | Page 6 footer |
| Page relationship | Page 6 last group |

### 29 — Page 6 note

| Field | Value |
|---|---|
| Section name | Page 6 note |
| Order | 29 |
| Displayed fields | Note paragraph (`SS-NOTE-06`) |
| Grouping | After page 6 groups |
| Count / percentage / salary / subtotal | None |
| Suppression | Explains item-reported denominators (`SS-PCT-04`) |
| Notes | Observed: “Figures are based on jobs for which the item was reported, and thus may not add to the total number of jobs. Timing of job offer figures exclude any graduates starting their own practice.” |
| Footer | Page 6 footer |
| Page relationship | Page 6 |

### 30 — Duration of Jobs by Employer Type

| Field | Value |
|---|---|
| Section name | Duration of Jobs by Employer Type |
| Order | 30 |
| Displayed fields | Business; Judicial Clerkships; Private Practice; Government; Public Interest |
| Grouping | `analvar='DURATION'` on part2 (`CF-P2-01`, `SS-FIL-07`). `newvar=empgen` |
| Count | Long-term / short-term: Business 10 / 1; Judicial Clerkships 5 / `.`; Private Practice 50 / `.`; Government 12 / 1; Public Interest 8 / `.` |
| Percentage | Not a column (`SS-PCT-03`) |
| Salary fields | None |
| Suppression | Education not shown. `.` means no stored short-term count, not a printed zero |
| Subtotal | Label **Total Reported** (`SS-FMT-03`). 85 / 2 |
| Notes | Page 7 note (section 32) |
| Footer | Page 7 footer |
| Page relationship | Page 7 |

### 31 — Law-school-funded jobs (absent)

| Field | Value |
|---|---|
| Section name | Total Number of Jobs Reported as Funded by Law School |
| Order | 31 (SAS page 7 second group; **not printed**) |
| Displayed fields | None |
| Grouping | Would be `analvar='LAW SCHOOL FUNDED'` (`CF-P2-02`, `SS-CNT-05`). Row label for `YES` is blank (`SS-FMT-04`) |
| Count / percentage / salary / subtotal | None. Subtotal label would be `Total Reported` |
| Suppression | Entire section omitted; no funded rows in this test file |
| Notes | Page 7 note still mentions funded jobs |
| Footer | Page 7 footer |
| Page relationship | Expected on page 7; absent on this PDF |

### 32 — Page 7 note

| Field | Value |
|---|---|
| Section name | Page 7 note |
| Order | 32 |
| Displayed fields | Note paragraph (`SS-NOTE-07`) |
| Grouping | After page 7 groups |
| Count / percentage / salary / subtotal | None |
| Suppression | Duration based on jobs with the item reported |
| Notes | “The count of jobs funded by the law school is a total, regardless of duration.” Still printed even though the funded section is absent |
| Footer | Page 7 footer |
| Page relationship | Page 7 |

---

## Comparison to SAS report logic

The PDF is the same **seven-page** `PROC REPORT` skeleton as `%SCHRPTS` (`SS-ORD-01`, `SS-RPT-01`–`07`).

| Topic | SAS 2025 report logic | This baseline PDF |
|---|---|---|
| Page count and sequence | 7 procedures, new page each | 7 pages in that order |
| Page 1 groups | `B`, `C`, `C1`, `D` plus Total Reported banner | Same four groups + banner |
| Page 2 groups | Character `GE 'D1' AND LT 'E2'` → `D1`, `D2`, `D3`, (`E` if present), `E1` | `D1`, `D2`, `D3`, `E1`. No `E` header |
| Page 3 groups | `E2`, `E3`, `E4`, `E5`, `E55` | `E3`, `E4`, `E5`, `E55`. `E2` omitted |
| Page 4 groups | `E6`, `FIRM`, `FIRM2` | All three present |
| Page 5 groups | `JOBREG1`, `JOBREG2`, `JOBREG3` | All three present; `JOBREG3` blank label + `Total #` |
| Page 6 groups | `SOURCE`, `TIME`, `ZSTATUS` on part2 | All three; no salary columns |
| Page 7 groups | `DURATION`, `LAW SCHOOL FUNDED` | Duration only |
| Empty categories | Not dropped in REPORT; missing if absent from input | Matches `SS-FIL-08` |
| Salary columns | Display upstream; no `IF N<5` here | `.` where no salary merge; note states five-salary rule |
| Subtotals | `Count.sum` / `Percent.sum`; salaries not summed | Matches, including D1 subtotal 93.0 and JOBREG3 `Total #` |
| Notes | `SS-NOTE-01`–`03`, `06`, `07` | Same sentences; PDF wrapping glues some words |
| Bookmarks | `proclabel='Page N'`, `pdftoc=1` | `Page N` plus SAS default child nodes |
| School / class year | Live school `NAME`; Class of 2025; July 2026 | Test University; Class of 2024; July 2025 |
| Cross-tab / timing labels | 2025 `$newvar` keys may not match builder | This PDF **did** print Women/Men of Color and After graduation |
| Accessibility | `accessible` only on the first live `ods pdf` | Not validated here. Do not claim PDF-UA (`SS-OUT-03`) |

### What the PDF does not add

The PDF does not show extra sections, extra columns, or a second suppression threshold. It does not recompute percents of Total Reported except where the stored `Percent` already is that (page 1 gender/employment; D1). Race, sector, and employer percents are of the section’s own denominator, as stored by `PROC FREQ`.

### Observed number anomalies (characterization only)

Recorded so a later implementation is not “corrected” to match a guessed identity:

1. D3 detail counts sum to 94; D3 subtotal is 93.
2. Page 2 LJD Full-time = 79 vs page 1 LJD = 78.
3. JD Advantage Full-time `# with Salary` (12) &gt; Number Reported (10).
4. Some `COMMA8.0` salaries lack a thousands separator in the text layer (`850000`, `765000`, `95000`, `90000`).
5. Control characters sit next to the page 1 C1 and D subtotal counts.

Do not turn (1)–(5) into new calculation rules.

---

## Traceability matrix

`SAS Rule` is from `business-rules.md`. `Report Section` uses the section names above. `Expected Result` is what the modernized report must match **if that rule is in scope** — still no redesign.

| SAS Rule | Report Section | Expected Result |
|---|---|---|
| CF-C-01 / SS-PREP-02 / SS-CNT-02 / SS-HDR-07 | Total Reported | Banner `Total Reported =` the `ANALVAR='A'` count. This PDF: 100. Not a table row |
| CF-C-02 / CF-S-01 / SS-FIL-01 / SS-RPT-01 | Gender Reported | Women/Men/(Non-binary if present) with count, percent, salary stats. This PDF: Women 46, Men 54; no Non-binary |
| CF-C-03 / CF-S-02 | Race Reported | People of Color then White (internal `MINOR`/`NONMIN`). This PDF: 24 / 46; subtotal 70 |
| CF-C-04 / CF-S-03 / SS-FMT-04 | Gender & Race Reported | Cross-tab labels from `$newvar`. This PDF prints Women/Men of Color and White Women/Men. Keep 2025 key mismatch documented |
| CF-C-05 / CF-S-04 | Employment Status Known | `$jobcat` labels for known status. This PDF omits unknown / advanced-degree / deferred-start rows |
| CF-C-06 / CF-S-05 / CF-AMB-02 / CF-AMB-03 | Total Employed or Enrolled… | `EMPL` and `6-ADVD` if present. This PDF: Employed 93 / 93.0 only |
| CF-C-08 / CF-S-06 / CF-S-07 | Employment by Sector | Private then Public. This PDF: 78 / 15. No unknown-employer row |
| CF-C-07 / CF-S-19 / CF-AMB-01 / CF-AMB-04 | Full-time or Part-time Job Status | `jobcat\|\|jobftpt` labels. Display stored count/N even if they disagree (this PDF: 79 vs 78; N 12 vs count 10) |
| CF-C-09 / CF-S-08 | Employment Categories | Employer types except those with no rows. This PDF: no Education, no Unknown Type |
| CF-C-10 / CF-S-10 | Education Jobs | Print only if `E2` rows exist. This PDF: section absent |
| CF-C-11 / CF-S-09 | Business Jobs | Job-category rows for `empgen='BUS'`. This PDF: 2 / 10 / 1 / 1 |
| CF-C-12 / CF-S-11 | Private Practice Jobs | Job-category rows for firms. This PDF: 46 / 4 |
| CF-C-13 / CF-S-12 | Government Jobs | Job-category rows for government. This PDF: 13 / 3 |
| CF-C-14 / CF-S-13 | Judicial Clerkships | Court/subtype labels. This PDF: State 4, Local 1; salaries `.` |
| CF-C-15 / CF-S-14 | Public Interest Jobs | Job-category rows. This PDF: 6 / 1 / 1; LJD salaries shown at n=5 |
| CF-C-16 / CF-S-15 / CF-AMB-05 | Size of Law Firm | `SOLO`/`LF1`–`LF8` if present. This PDF: LF1–LF7 only; no Solo row |
| CF-C-17 / CF-S-16 | Type of Law Firm Job | `lfjob` labels. This PDF: Associate 42, Law Clerk 8 |
| CF-C-18 / CF-S-17 / CF-PREP-03 / CF-AMB-06 | Jobs Taken by Region | Region labels `1`–`9`, `T`, `X`. This PDF: 1, 2, 5, 6, 8 only |
| CF-C-19 / CF-S-18 | Location of Jobs | In-State / Out of State / Foreign if present. This PDF: no Foreign |
| CF-C-20 | # States and Territories… | One blank-labeled count; subtotal `Total #`. This PDF: 14 |
| CF-S-00 / SS-SUP-01 | All salary cells, pages 1–5 | Omit salary stats unless `n ge 5`. Print `.` when omitted. Threshold is salary n, not headcount (Mountain count 5, salaries `.`; Men of Color count 4, salaries `.`) |
| CF-S-20 / CF-M-01 | Pages 1–5 salary columns | Display merged `N`, `Pct25`, `Median`, `Pct75`, `Mean`. Do not recompute in the report |
| CF-P2-03 / SS-FIL-06 / SS-RPT-06 | Source of Job | Source labels; count and percent only. This PDF subtotal 90 |
| CF-P2-04 / SS-FMT-04 | Timing of Job Offer | Before / After graduation if keys match `$newvar`. This PDF: 49 / 31, subtotal 80 |
| CF-P2-05 | Search Status of Employed Grads | Seeking / not seeking. This PDF: 4 / 86, subtotal 90 |
| CF-P2-01 / SS-CALC-03 / SS-CNT-04 / SS-RPT-07 | Duration of Jobs by Employer Type | Long-term / short-term columns; no percent. This PDF: Total Reported 85 / 2 |
| CF-P2-02 / SS-CNT-05 | Law-school-funded section | Print if funded rows exist; `YES` label is blank; subtotal `Total Reported`. This PDF: section absent |
| CF-P2-06 | Pages 6–7 | Those pages read part2, not the main summary file |
| CF-FMT-01 / CF-FMT-02 / CF-FMT-03 | Row labels on 6 / status / FT-PT | Labels come from report `$newvar`, not from reprinting builder formats |
| CF-PREP-02 | Type of Law Firm Job | Recodes `ADMIN`/`OTHNL`/`STATTY` affect which FIRM2 labels can appear |
| CF-PREP-04 | Source of Job | `OTHER`→`ZOTHER`, `OCI`→`AOCI` so OCI prints as Career office recruitment program |
| CF-PREP-05 | Judicial Clerkships | Collapsed `emptype1` keys (`JCTLOG`, `JCUGOV`, …) |
| CF-PREP-06 | Gender / Gender & Race | `W`→`F`, `X`→`N`, `ND` blank before counts |
| CF-DEAD-01 / CF-DEAD-02 / CF-DEAD-03 | (no section) | Must **not** appear as extra recodes |
| CF-AMB-07 / SS-SAL-07 | Salary notes, pages 1–5 | Note language only; do not add FT/solo filters in the renderer |
| SS-PREP-01 / SS-FTR-02 / SS-FTR-04 | Page chrome | Session/footnote leftovers; no extra printed footnote date |
| SS-PREP-03 | Entire PDF | Seven reports for one school `CODE`/`NAME`. `JOBST`/`ST` unused |
| SS-FMT-01 | (no section) | `$jobcat` unused in REPORT |
| SS-FMT-02 / SS-HDR-06 / SS-GRP-01 | Every data section header | Printed `$recode` title; raw `analvar` hidden |
| SS-FMT-03 / SS-SUB-01 / SS-SUB-02 | Every data section | `Subtotal`, except JOBREG3 `Total #`, duration/funded `Total Reported`. No salary subtotals |
| SS-FMT-04 / SS-GRP-02 | Every detail row | Internal-code order; formatted label |
| SS-FIL-01–07 | Pages 1–7 | Same `ANALVAR` slices as SAS |
| SS-FIL-08 / SS-SUP-02 | Any missing `$recode` group | Do not invent placeholder rows |
| SS-ORD-01 / SS-PAGE-01 / SS-PAGE-02 | Page relationship | One school, pages 1→7, one procedure per page |
| SS-ORD-02 / SS-OUT-01 / SS-OUT-02 | (filename / school list) | Not testable from this single Test University file |
| SS-CALC-01 / SS-CNT-01 | Count columns, pages 1–6 | Display/sum stored `Count` (`COMMA6.0`) |
| SS-CALC-02 / SS-PCT-01 / SS-PCT-02 | Percent columns, pages 1–6 | Display/sum stored `Percent` (6.1), including D1 93.0 and timing 100.1-at-detail |
| SS-CALC-04 / SS-FTR-03 | (no section) | Commented page-7 columns and reprint footnote must **not** appear |
| SS-PCT-03 | Duration / funded | No percent column |
| SS-PCT-04 / SS-NOTE-06 | Page 6 note | Print the item-reported / own-practice sentences; do not add a `SELFPR` filter |
| SS-SAL-01–05 | Salary columns, pages 1–5 | Headers `# with Salary`, 25th Percentile, Median, 75th Percentile, Mean |
| SS-SAL-06 | Pages 1–5 | Spanning header `Full-time Long-term Salaries` |
| SS-SUP-03 | Firm size + pages 1–5 notes | Solo exclusion is note (and builder salary mapping); this PDF has no Solo count row |
| SS-SUP-04 | Timing of Job Offer + page 6 note | Note only in the renderer |
| SS-HDR-01 | All pages | School name as TITLE1. This PDF: Test University School of Law |
| SS-HDR-02 | Page 1 | Title without `- Page 1`. This PDF says Class of 2024; 2025 SAS says 2025 |
| SS-HDR-03 | Pages 2–7 | Title with `- Page N` |
| SS-HDR-04 / SS-PAGE-03 | Bookmarks | At least `Page 1`–`Page 7` |
| SS-HDR-05 | Column headers | Split headers as on the PDF |
| SS-FTR-01 | Page footer | NALP table line + ABA disclaimer + URL. This PDF: July 2025; 2025 SAS: July 2026 |
| SS-PAGE-04 | Visual style | GrayscalePrinter / Thorndale-like fonts; not a calculation |
| SS-PAGE-05 / SS-OUT-03 | (document properties) | Do not claim accessibility from this file |
| SS-PAGE-06 / SS-PAGE-07 | (other schools) | Not represented by this PDF |
| SS-NOTE-01 | Page 1 note | Print the five sentences listed in section 6 |
| SS-NOTE-02 | Page 2 note | Include public/private and missing-employer-type sentences |
| SS-NOTE-03 | Pages 3–5 notes | No-grad, five-salary, FT long-term, solo |
| SS-NOTE-07 | Page 7 note | Duration item-reported + funded-is-total sentence, even if funded section is absent |

---

## Testers

- Compare **structure and labels** to this map; compare **formulas** to the builder rules, not to a redesigned table.
- Salary suppression to implement is **`n ge 5` (`CF-S-00`)**. The PDF note is the same rule stated in words.
- Empty categories stay empty. Do not add Education, Solo, Foreign, Non-binary, or Law School Funded rows to “complete” this test PDF.
- Do not normalize D3 94-vs-93, LJD 79-vs-78, or N 12-vs-10 unless a later approved data file shows those were test-file errors.

No application code was added. The baseline PDF was not modified.
