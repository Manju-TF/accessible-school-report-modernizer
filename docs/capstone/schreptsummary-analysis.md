# Analysis: `legacy/sas/schreptsummary_2025.sas`

Source: `legacy/sas/schreptsummary_2025.sas` (IMMUTABLE). This file was not modified. Nothing was implemented.

This program does **not** recompute school counts or salary statistics from graduate-level rows. It formats and prints `erss2025.schreptsummary2025` and `erss2025.schreptsummary2025_part2` (built by `createschrptfiles2025.sas`) into one PDF per school.

Classification used below:

- **CONFIRMED** — the SAS statement does this.
- **INFERRED** — suggested by a name, note, or comment, not proven by this file alone.
- **AMBIGUOUS** — conflicting, unused, commented-out, or incomplete in this file.

Missing codebook meanings are **not invented**.

---

## Inputs, outputs, and setup

| Item | Observed |
|---|---|
| Library | `erss2025` → another workstation path (`C:\Users\OksanaPoulis\…\Class of 2025 ERSS Submission Files`) |
| Input 1 | `erss2025.schreptsummary2025` |
| Input 2 | `erss2025.schreptsummary2025_part2` |
| Output | One ODS PDF per school under `…\ERSS School Reports\2025\` |
| Options | `pagesize=63`, `linesize=110` then later `100`; `nodate`; `nonumber`; `source source2 mprint mlogic symbolgen` |

### SS-PREP-01 — Session options and empty footnote

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | data preparation, footers |
| SAS location | Lines 1–5 |
| Rule | Set listing options; `footnote ' '` clears a leftover footnote. |
| Notes | Does not transform data. |

### SS-PREP-02 — Total-reported macro variables

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | data preparation, calculated fields, headers |
| SAS location | Lines 201–205 |
| Rule | `DATA _NULL_` reads `schreptsummary2025` where `ANALVAR EQ "A"` and `CALL SYMPUT(COMPRESS("Ct_" \|\| Code), TRIM(LEFT(PUT(Count, 4.))))`. |
| Input fields | `ANALVAR`, `Code`, `Count` |
| Output fields | Macro variables `Ct_<schoolcode>` |
| Business meaning | Page 1 prints `Total Reported = &&Ct_&Code.` (SS-RPT-01). |
| Edge cases | `PUT(Count, 4.)` can overflow or truncate if count > 9999. Schools with no `ANALVAR='A'` row get no macro variable. |

### SS-PREP-03 — School-report macro

| Field | Value |
|---|---|
| Classification | CONFIRMED (parameters); AMBIGUOUS (`JOBST`, `ST` unused) |
| Topic | data preparation |
| SAS location | Lines 217–597 |
| Rule | `%MACRO SCHRPTS(CODE, NAME, JOBST, ST)` runs seven `PROC REPORT` steps for one school. |
| Input fields | `CODE` used in `WHERE`; `NAME` used in titles. |
| Output fields | Seven report pages in the open ODS PDF. |
| Edge cases | `JOBST` and `ST` are never referenced in the macro body. Buffalo is called with `'249'` while other NY schools use `'233'` (line 752); that difference has no effect unless a later change uses `JOBST`. |

---

## Formats (labels only)

These formats do not change stored `analvar` / `newvar` values. They label report text.

### SS-FMT-01 — `$jobcat`

| Field | Value |
|---|---|
| Classification | CONFIRMED (defined); AMBIGUOUS (unused in this file) |
| Topic | headers |
| SAS location | Lines 8–23 |
| Mapping | `LJD`→`1-LJD`, `NLJD`→`2-NLJD`, `NLP`→`3-NLP`, `NLO`→`4-NLO`, `WUNK`→`5-WUNK`, `ADVD`→`6-ADVD`, `UDEF`→`7-UDEF`, `USKW`→`8-USKW`, `UNWK`→`9-UNWK`, `UNKN`→`UNKN` |
| Notes | No `PROC REPORT` applies `$jobcat`. Row labels use `$newvar` on already-formatted codes such as `1-LJD`. |

### SS-FMT-02 — `$recode` (section headers)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | headers, grouping |
| SAS location | Lines 25–54 |
| Rule | Maps `analvar` to section titles printed in `COMPUTE BEFORE ANALVAR`. |

| `analvar` | Header text |
|---|---|
| `A` | three spaces (blank title) |
| `B` | Gender Reported: |
| `C` | Race Reported: |
| `C1` | Gender & Race Reported: |
| `D` | Employment Status Known: |
| `D1` | Total Employed or Enrolled in Graduate Studies: |
| `D2` | Employment by Sector: |
| `D3` | Full-time or Part-time Job Status: |
| `E` | Employment by Sector: |
| `E1` | Employment Categories: |
| `E2` | Education Jobs: |
| `E3` | Business Jobs: |
| `E4` | Private Practice Jobs: |
| `E5` | Government Jobs: |
| `E55` | Judicial Clerkships: |
| `E6` | Public Interest Jobs: |
| `FIRM` | Size of Law Firm (by # of Attorneys): |
| `FIRM2` | Type of Law Firm Job: |
| `JOBREG1` | Jobs Taken by Region: |
| `JOBREG2` | Location of Jobs: |
| `JOBREG3` | # States and Territories with Employed Grads: |
| `SOURCE` | Source of Job: |
| `TIME` | Timing of Job Offer: |
| `ZSTATUS` | Search Status of Employed Grads: |
| `DURATION` | Duration of Jobs by Employer Type: |
| `LAW SCHOOL FUNDED` | Total Number of Jobs Reported as Funded by Law School: |

`A` is formatted but not selected on any report page. `E` is formatted; this file never selects `ANALVAR='E'`.

### SS-FMT-03 — `$subtotal` (after-group labels)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | subtotals |
| SAS location | Lines 56–83 |
| Rule | Most groups print `   Subtotal`. Exceptions: `JOBREG3` → `   Total #`; `DURATION` → `     Total Reported`; `LAW SCHOOL FUNDED` → `   Total Reported`. |

### SS-FMT-04 — `$newvar` (row labels)

| Field | Value |
|---|---|
| Classification | CONFIRMED (mappings present); AMBIGUOUS (several keys may not match stored values) |
| Topic | headers, report ordering |
| SAS location | Lines 85–191 |

Observed label groups:

- Gender: `F` Women; `M` Men; `N` Non-binary or Chose to Self-identify
- Race/ethnicity as labeled here: `NONMIN` White; `MINOR` People of Color
- Cross: `NONMINF` White Women; `NONMINM` White Men; `NONMINN` White Non-binary…; `MINOR F` Women of Color; `MINOR M` Men of Color; `MINOR N` Non-binary… People of Color
- Job category: `1-LJD` Bar Admission Required/ Anticipated; `2-NLJD` JD Advantage; `3-NLP` Other Professional; `4-NLO` Other Position; `5-WUNK` Job Type Unknown; `6-ADVD` Enrolled in Graduate Studies; `7-UDEF` Employed-Start Date after March 16, 2026; `8-USKW` Not Employed-Seeking; `9-UNWK` Not Employed-Not Seeking; `EMPL` Employed
- Employer: `ACAD` Education; `BUS` Business; `CLERK` Judicial Clerkships; `GOVT` Government; `FIRM` Private Practice; `PUBINT` Public Interest; `ZEMPUN`/`EMPUNK` Unknown Type
- Firm size: `SOLO` Solo Practitioner; `LF1` 1-10; `LF2` 11-25; `LF3` 26-50; `LF4` 51-100; `LF5` 101-250; `LF6` 251-500; `LF7` 501+; `LF8` Unknown Size
- Clerkship court: `JCFDGV` Federal; `JCSTGV` State; `JCTLOG` Local; `JCTRGV` Tribal; `JCUGOV` Unknown; `JCINGV`/`JCXIOG` International
- Region: `1`–`9` census-style names; `T` US Territories; `X` Non-US locations
- Location: `INSTATE` In-State; `OUTOFSTATE` Out of State; `FOREIGN` Foreign
- Sector: `PRIVATE` Private Sector; `PUBLIC` Public Sector
- Source / timing / status / FT-PT / firm job type as listed in lines 153–188
- `A` → `TOTAL `; `JOBREG3` → blank; `YES` → blank; `Z-Total Reporte` → Total Number Reported

**AMBIGUOUS vs `createschrptfiles2025.sas` (do not “fix” here):**

- Cross-tab keys in the builder are `minstat\|\|sex3` (example `MINORF`). This format lists `MINOR F` / `MINOR M` with a space.
- Builder recodes `AFTGRD` → `ZAFTGRD`. This format lists `ZAFTGR`, not `ZAFTGRD`.
- `YES` maps to a blank label (used for law-school-funded rows).

---

## Filtering

No graduate-level filters exist in this file. Filters are school + `analvar` page slices.

| Rule ID | Classification | Page | Filter |
|---|---|---|---|
| SS-FIL-01 | CONFIRMED | 1 | `CODE = "&CODE" AND ANALVAR in ('B','C','C1','D')` |
| SS-FIL-02 | CONFIRMED | 2 | `CODE = "&CODE" AND ANALVAR GE 'D1' AND ANALVAR LT 'E2'` |
| SS-FIL-03 | CONFIRMED | 3 | `CODE = "&CODE" AND ANALVAR in ('E2','E3','E4','E5','E55')` |
| SS-FIL-04 | CONFIRMED | 4 | `CODE = "&CODE" AND ANALVAR IN ('E6','FIRM','FIRM2')` |
| SS-FIL-05 | CONFIRMED | 5 | `CODE = "&CODE" AND ANALVAR IN ('JOBREG1','JOBREG2','JOBREG3')` |
| SS-FIL-06 | CONFIRMED | 6 | `CODE = "&CODE" AND ANALVAR in ('SOURCE','TIME','ZSTATUS')` on **part2** |
| SS-FIL-07 | CONFIRMED | 7 | `CODE = "&CODE" AND ANALVAR in ('DURATION','LAW SCHOOL FUNDED')` on **part2** |

### SS-FIL-02 notes (AMBIGUOUS)

Page 2 uses **character** `GE 'D1' AND LT 'E2'`, not an explicit list.

Included under SAS character order (typical): `D1`, `D2`, `D3`, `E`, `E1`.

Excluded: `E2` and later (`E3`…`E55`, `FIRM`, `JOBREG*`). Those appear on later pages.

`E` is in `$recode` but is not produced by the builder analysis. Whether any `E` rows exist is not shown here.

### SS-FIL-08 — Categories with no graduates

| Field | Value |
|---|---|
| Classification | CONFIRMED (effect); INFERRED (intent from the note) |
| Topic | filtering, notes |
| Rule | There is no code that drops zero-count rows. Empty categories are absent because the input datasets only contain `PROC FREQ` rows that occurred. |
| Note text | “Categories with no graduates reported are not shown.” (pages 1–5) |

---

## Grouping, sorting, and report ordering

### SS-GRP-01 — Group by `analvar` (hidden)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | grouping |
| SAS location | Every `DEFINE Analvar / GROUP NOPRINT` |
| Rule | Rows are grouped by `analvar`. The raw code is not printed; `$recode` prints the section header. |

### SS-GRP-02 — Group / order `newvar`

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | grouping, sorting, report ordering |
| SAS location | `DEFINE Newvar / ORDER = INTERNAL GROUP` |
| Rule | Detail rows are grouped and ordered by the **unformatted internal** `newvar` value, then displayed with `$newvar`. |
| Edge cases | Internal order is SAS unformatted order (typically character sort of codes such as `1-LJD`, `F`, `LF1`), not the prose label order. |

### SS-ORD-01 — Page sequence

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | report ordering, page breaks |
| Rule | Macro always runs pages 1→7 in source order. |

### SS-ORD-02 — School sequence

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | report ordering |
| SAS location | Lines 599–1845 |
| Rule | Schools are invoked in the order written (roughly by `CODE`). About 193 live `%SCHRPTS` calls. This is a school list, not a calculation. |

---

## Calculated fields in this file

This file does **not** compute `pct25`, `median`, `pct75`, `mean`, or `n` from salaries. Those columns are **displayed** from the input.

### SS-CALC-01 — Report count = `SUM` of `Count`

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | counts, calculated fields |
| Rule | `DEFINE Count / SUM 'Number*Reported' FORMAT=COMMA6.0` |

### SS-CALC-02 — Report percent = `SUM` of `Percent`

| Field | Value |
|---|---|
| Classification | CONFIRMED (what REPORT does); AMBIGUOUS (whether summed percents are a meaningful percent) |
| Topic | percentages, calculated fields |
| Rule | `DEFINE Percent / SUM '% of*Reported' FORMAT=6.1` |
| Notes | Input `Percent` values come from upstream `PROC FREQ`. Summing them for a subtotal is what this report does. This file does not recalculate percent of total graduates. |

### SS-CALC-03 — Duration / funded sums

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | counts, calculated fields |
| SAS location | Lines 570–573 |
| Rule | Page 7: `perm SUM` labeled `Long-term (1+ years)`; `temp SUM` labeled `Short-term (Less than 1 year)`. |
| Notes | Column names `perm`/`temp` must already exist on part2. The builder renamed funded `count` to `perm`. Duration transpose uses whatever `duration` values exist as IDs. If those IDs are not `perm`/`temp`, page 7 columns can be empty. That match is **AMBIGUOUS** without the part2 dataset. |

### SS-CALC-04 — Unused / commented page-7 columns

| Field | Value |
|---|---|
| Classification | AMBIGUOUS (inactive) |
| SAS location | Lines 571–572 |
| Rule | Commented `DEFINE count` (“Number of School Funded”) and `DEFINE fixed` (“Number of Fixed Duration”). Not applied. |

---

## PROC REPORT sections

Shared style (pages 1–6 unless noted):

- `NOWD HEADSKIP HEADLINE SPLIT='*'`
- Column font 8pt, width 0.75 in; header 9pt
- `Newvar` cell width 1.75 in (2.0 in on page 7)
- `ODS ESCAPECHAR = '^'`

| Rule ID | Page | Dataset | Columns | Salary block |
|---|---|---|---|---|
| SS-RPT-01 | 1 | `schreptsummary2025` | AnalVar, NewVar, Count, Percent, N, Pct25, Median, Pct75, Mean | Yes, spanning header `Full-time Long-term Salaries` |
| SS-RPT-02 | 2 | same | same | Yes |
| SS-RPT-03 | 3 | same | same | Yes |
| SS-RPT-04 | 4 | same | same | Yes |
| SS-RPT-05 | 5 | same | same | Yes |
| SS-RPT-06 | 6 | `schreptsummary2025_part2` | AnalVar, NewVar, Count, Percent | **No** salary columns |
| SS-RPT-07 | 7 | `schreptsummary2025_part2` | AnalVar, NewVar, perm, temp | **No** salary columns |

Page 1 only: `COMPUTE BEFORE` prints a blank line and `Total Reported = &&Ct_&Code.` (SS-PREP-02).

Each page: `COMPUTE BEFORE ANALVAR` prints `$recode` header; `COMPUTE AFTER ANALVAR` prints `$subtotal` plus summed measures.

---

## Counts

| Rule ID | What is counted | How |
|---|---|---|
| SS-CNT-01 | Number reported | Display/sum of input `Count` (pages 1–6) |
| SS-CNT-02 | Total reported (page 1 banner) | Macro `Ct_<code>` from `ANALVAR='A'` |
| SS-CNT-03 | # with salary | Display of input `N` (not summed) |
| SS-CNT-04 | Long-term / short-term jobs | Sum of `perm` / `temp` (page 7) |
| SS-CNT-05 | Law-school-funded jobs | Same page-7 structure; note says the funded figure is a total regardless of duration (SS-NOTE-07) |

---

## Percentages

| Rule ID | Classification | Rule |
|---|---|---|
| SS-PCT-01 | CONFIRMED | Detail `% of Reported` is the input `Percent` column, format 6.1. |
| SS-PCT-02 | CONFIRMED | Section subtotal prints `Percent.sum`. |
| SS-PCT-03 | CONFIRMED | Page 7 has no percent column. |
| SS-PCT-04 | INFERRED from note | Page 6 note: figures are based on jobs for which the item was reported and may not add to total jobs. This file does not enforce that; it prints stored percents. |

---

## Subtotals

### SS-SUB-01 — After each `analvar`

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Topic | subtotals |
| Rule | `COMPUTE AFTER ANALVAR` prints `$subtotal` label, `Count.sum` (or `perm.sum`/`temp.sum` on page 7), and `Percent.sum` on pages 1–6. |
| Positions | Pages 1–5: `+47` count, `+17` percent. Page 6: `+45` count, `+17` percent. Page 7: `+42` perm, `+13` temp. |
| Notes | Salary columns are **not** subtotaled. |

### SS-SUB-02 — JOBREG3 label

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Rule | `$subtotal` for `JOBREG3` is `   Total #` instead of `Subtotal`. |

---

## Salary, percentiles, median, mean

This file **displays** upstream salary columns. It does not run `PROC UNIVARIATE`.

| Rule ID | Column | DEFINE | Format | Header |
|---|---|---|---|---|
| SS-SAL-01 | `N` | DISPLAY `# with*Salary` | COMMA6.0 | count of salaries in the by-group |
| SS-SAL-02 | `Pct25` | DISPLAY `25th*Percentile` | COMMA8.0 | 25th percentile |
| SS-SAL-03 | `Median` | DISPLAY `Median` | COMMA8.0 | median |
| SS-SAL-04 | `Pct75` | DISPLAY `75th*Percentile` | COMMA8.0 | 75th percentile |
| SS-SAL-05 | `Mean` | DISPLAY `Mean` | COMMA8.0 | mean |
| SS-SAL-06 | spanning header | `('Full-time Long-term Salaries' …)` | — | pages 1–5 only |

All SS-SAL-01–05 are **CONFIRMED** as display mappings. The numeric calculation of those fields is in `createschrptfiles2025.sas`, not here.

### SS-SAL-07 — Salary universe stated on the report

| Field | Value |
|---|---|
| Classification | INFERRED from printed notes (not computed in this file) |
| Topic | salary calculations, notes |
| Note text (pages 1–5) | “Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.” |
| Conservative reading | Treat this as **report language**. Upstream salary steps use `salftperm` and `n ge 5`. Whether every displayed salary is actually FT long-term, and whether solo is excluded, must stay tied to the builder analysis — do not invent extra filters here. |

---

## Suppression logic

### SS-SUP-01 — At least five salaries

| Field | Value |
|---|---|
| Classification | CONFIRMED as printed note; INFERRED as this file’s enforcement |
| Topic | suppression logic |
| Note | “At least five salaries are required for each salary analysis.” |
| What this file does | Displays `N`, `Pct25`, `Median`, `Pct75`, `Mean` as stored. Empty salary cells appear when the builder omitted the salary row (`n ge 5`). This file has **no** `IF N < 5 THEN …` logic. |

### SS-SUP-02 — Hide unused categories

| Field | Value |
|---|---|
| Classification | CONFIRMED effect (see SS-FIL-08) |
| Rule | No row ⇒ category not shown. |

### SS-SUP-03 — Solo salaries

| Field | Value |
|---|---|
| Classification | AMBIGUOUS |
| Note | Pages 1–5 say solo-practice salaries were excluded. |
| This file | No `WHERE` on `SOLO` / `firm1='S'`. Any exclusion already happened upstream or by failed merge. |

### SS-SUP-04 — Own-practice timing

| Field | Value |
|---|---|
| Classification | INFERRED from note only |
| Note (page 6) | “Timing of job offer figures exclude any graduates starting their own practice.” |
| This file | No `WHERE` on `SELFPR`. |

---

## Headers

| Rule ID | Location | Text |
|---|---|---|
| SS-HDR-01 | TITLE1 all pages | `&NAME` (school name argument) |
| SS-HDR-02 | TITLE2 page 1 | `Class of 2025 Summary Report` |
| SS-HDR-03 | TITLE2 pages 2–7 | `Class of 2025 Summary Report - Page N` |
| SS-HDR-04 | ODS proclabel | `Page 1` … `Page 7` (PDF bookmarks when `pdftoc=1`) |
| SS-HDR-05 | Column headers | `Number*Reported`, `% of*Reported`, salary headers in SS-SAL-* |
| SS-HDR-06 | Section headers | `$recode` via `COMPUTE BEFORE ANALVAR` |
| SS-HDR-07 | Page 1 extra | `Total Reported = &&Ct_&Code.` |

`SPLIT='*'` breaks header text at `*`.

---

## Footers

| Rule ID | Classification | What prints |
|---|---|---|
| SS-FTR-01 | CONFIRMED | `STYLE=[posttext=…]` after each report: `Table prepared by NALP, July 2026` plus ABA/NALP disclaimer and `www.nalp.org/erssinfo`. |
| SS-FTR-02 | CONFIRMED | `FOOTNOTE1` and `FOOTNOTE2` are blank `' '`. |
| SS-FTR-03 | AMBIGUOUS (inactive) | Commented reprint footnote using `today()` / weekdate. Not applied. |
| SS-FTR-04 | CONFIRMED | Line 3 `footnote ' '` at startup. |

---

## Page breaks

| Rule ID | Classification | Rule |
|---|---|---|
| SS-PAGE-01 | CONFIRMED | Each school: `ods pdf file=…` then `%SCHRPTS` then `ods pdf close` → one file per school. |
| SS-PAGE-02 | CONFIRMED | Seven `PROC REPORT` steps inside one PDF. ODS PDF default is a new page per procedure unless `startpage=never`. This file never sets `STARTPAGE=NEVER`. |
| SS-PAGE-03 | CONFIRMED | `pdftoc=1` on the ODS PDF style statement (bookmark depth). |
| SS-PAGE-04 | CONFIRMED | Style `GrayscalePrinter`. |
| SS-PAGE-05 | AMBIGUOUS | Only the **first** school PDF (Quinnipiac, line 599) includes the ODS option `accessible`. Other `ods pdf file=` statements omit it. |
| SS-PAGE-06 | AMBIGUOUS | Line 1792 calls `%SCHRPTS` for 90520 (LaVerne) without an `ods pdf file` immediately above it in the same pattern as others. Destination is whatever PDF is open. |
| SS-PAGE-07 | AMBIGUOUS | Line 1846 is a stray `*/`. Lines 1848–1851 then open another PDF for American University (`ed50901_2017.pdf`). Whether 1846 closes an earlier `/*` is not clearly paired. |

---

## Notes (`COMPUTE AFTER`)

| Rule ID | Pages | Note text (observed) |
|---|---|---|
| SS-NOTE-01 | 1 | Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. The non-binary or chose to self-identify category also includes graduates who selected multiple gender identities. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis. |
| SS-NOTE-02 | 2 | Same opening sentences, plus: Private sector includes jobs in law firms and business. All other jobs are considered public sector. Employment by sector does not include graduates for whom employer type was not reported. Then the FT long-term and solo sentences. |
| SS-NOTE-03 | 3–5 | No-graduates, five-salary, FT long-term, and solo sentences only. |
| SS-NOTE-06 | 6 | Figures are based on jobs for which the item was reported, and thus may not add to the total number of jobs. Timing of job offer figures exclude any graduates starting their own practice. |
| SS-NOTE-07 | 7 | Figures for job duration are based on jobs for which the item was reported, and thus may not add to the total number of jobs. The count of jobs funded by the law school is a total, regardless of duration. |

SS-NOTE-01’s non-binary sentence is **INFERRED** as report policy. This file does not recode multiple gender identities; that would have to be in `sex3` upstream.

---

## PDF generation loop (not business calculations)

### SS-OUT-01 — Per-school PDF naming

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Pattern | `{CODE}_{slug}_summary2025.pdf` on a local NALP path. |
| Exceptions | `31405` filename contains `summary2024` and a space (`summary2024 .pdf`). |

### SS-OUT-02 — Commented / skipped schools

| Field | Value |
|---|---|
| Classification | CONFIRMED as inactive |
| Examples | Rutgers-Camden `23101`; Penn State Law `23909` (“merged with dickinson, no longer reporting separately”); Valparaiso `31504`; Saint Louis `42603`; several 2017 reprint blocks at the end. |
| Rule | Do not treat commented `%SCHRPTS` as current output. |

### SS-OUT-03 — Accessibility claim

| Field | Value |
|---|---|
| Classification | AMBIGUOUS |
| Observation | `accessible` appears on one `ods pdf` statement only. Project rules require tagged PDF/PDF-UA and forbid claiming accessibility without validation. This file is not validation. |

---

## Topic index

| Requested topic | Rule IDs |
|---|---|
| Data preparation | SS-PREP-01, SS-PREP-02, SS-PREP-03, SS-FMT-01–04 |
| Filtering | SS-FIL-01–08 |
| Grouping | SS-GRP-01, SS-GRP-02 |
| Sorting | SS-GRP-02, SS-ORD-01, SS-ORD-02 |
| Calculated fields | SS-PREP-02, SS-CALC-01–04 |
| PROC REPORT sections | SS-RPT-01–07 |
| Counts | SS-CNT-01–05 |
| Percentages | SS-PCT-01–04 |
| Subtotals | SS-SUB-01, SS-SUB-02, SS-FMT-03 |
| Salary calculations | SS-SAL-01–07 (display only) |
| Percentile calculations | SS-SAL-02, SS-SAL-04 (display of `Pct25`/`Pct75`) |
| Median | SS-SAL-03 |
| Mean | SS-SAL-05 |
| Suppression logic | SS-SUP-01–04 |
| Report ordering | SS-ORD-01, SS-ORD-02, SS-GRP-02 |
| Headers | SS-HDR-01–07, SS-FMT-02 |
| Footers | SS-FTR-01–04 |
| Page breaks | SS-PAGE-01–07 |
| Notes | SS-NOTE-01–03, SS-NOTE-06, SS-NOTE-07 |

---

## What this file does not do

- Does not read `erss2025.erss2025` graduate rows.
- Does not apply `n ge 5` in SAS logic (only prints the note; suppression is in the builder).
- Does not recalculate mean, median, or percentiles.
- Does not implement page-6 “exclude own practice” or page-note solo exclusion as `WHERE` clauses.
- Does not invent school-code geography; `JOBST`/`ST` are unused.

No application code was added. Review this analysis against the SAS and the builder analysis before any implementation.
