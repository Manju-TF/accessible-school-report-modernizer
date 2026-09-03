# Analysis: `legacy/sas/createschrptfiles2025.sas`

Source: `legacy/sas/createschrptfiles2025.sas` (IMMUTABLE). This file was not modified.

Header comment (lines 3–7) states the program creates counts of various items by school, salary info by school, and puts it all together. The header also names the program `createschreptfile2.sas`; the repository filename is `createschrptfiles2025.sas`. That naming mismatch is recorded only as an observation.

## Method

- Only behavior present in this SAS file is documented.
- Labels:
  - **CONFIRMED** — the SAS statement does this.
  - **INFERRED** — suggested by a name or comment, not proven by this file alone.
  - **AMBIGUOUS** — conflicting, commented-out, undefined, or incomplete in this file.
- Missing labels, codebook meanings, and source-system definitions are **not invented**.
- `PROC PRINT` `WHERE CODE LT/LE/GT '…'` filters are **display-only**. They do not change saved datasets unless noted.
- Dataset names are reused later in the same program. A `SET` copies the dataset as it exists at that moment. Later overwrites do not change already-built stacks.

## Inputs and outputs

| Item | Observed in this file |
|---|---|
| Library | `erss2025` → path on another workstation (`C:\Users\DanielleTaylor\…\Class of 2025 ERSS Submission Files`) |
| Input dataset | `erss2025.erss2025` |
| Working extract | `fornalprepts` |
| Count stack | `schoolcounts2025` |
| Salary stack | `schoolsalaries2025` |
| Output 1 | `erss2025.schreptsummary2025` |
| Output 2 | `erss2025.schreptsummary2025_part2` |

The physical contents of `erss2025.erss2025` are not in this repository. Field meanings beyond how this program uses them are **INFERRED** only where marked.

---

## Formats

### CF-FMT-01 — Offer-timing labels

| Field | Value |
|---|---|
| Classification | CONFIRMED (mapping); INFERRED (business labels) |
| Confidence | High for the mapping; medium for what `BGRAD` / `AFTGRD` mean in ERSS |
| SAS code/location | Lines 12–16, `proc format value $time` |
| Input fields | `BGRAD`, `AFTGRD` |
| Output fields | Formatted labels `Before Graduation`, `After Graduation` |
| Transformation | Value label only |
| Business meaning | Used later when `time1` is formatted with `$time.` (CF-P2-03). Labels are not written into `newvar`. |
| Dependencies | CF-P2-03 |
| Potential edge cases | Other `time1` values have no label in this format. |

### CF-FMT-02 — Job-source labels

| Field | Value |
|---|---|
| Classification | CONFIRMED (mapping); INFERRED (wording of labels) |
| Confidence | High for the mapping |
| SAS code/location | Lines 17–30, `proc format value $source` |
| Input fields | `OCI`, `JOBFRC`, `JOBPST`, `OTHER`, `PRNSMJ`, `RFFRND`, `SELFPR`, `SLFINI`, `TEMPAG`, `ONLINE`, `OSCAR` |
| Output fields | Display labels (for example `Career office recruitment program (e.g., OCI)`) |
| Transformation | Value label only |
| Business meaning | Applied when counting `source` (CF-P2-02). After recode, stored `source` values include `AOCI` and `ZOTHER`, which are **not** in this format. |
| Dependencies | CF-PREP-04, CF-P2-02 |
| Potential edge cases | `SOCI` appears only in a commented statement (CF-DEAD-01). `AOCI` / `ZOTHER` will not match `$source`. |

### CF-FMT-03 — Job-category and FT/PT labels

| Field | Value |
|---|---|
| Classification | CONFIRMED (mapping present); AMBIGUOUS (same format mixes job category and FT/PT) |
| Confidence | High that these mappings exist; medium that every code is still used in 2025 data |
| SAS code/location | Lines 33–47, `proc format value $jobcat` |
| Input fields | `LJD`, `NLJD`, `NLP`, `NLO`, `WUNK`, `ADVD`, `UDEF`, `USKW`, `UNWK`, `UNKN`, `FULL`, `PART` |
| Output fields | `1-LJD`, `2-NLJD`, `3-NLP`, `4-NLO`, `5-WUNK`, `6-ADVD`, `7-UDEF`, `8-USKW`, `9-UNWK`, `UNKN`, `Full-time`, `Part-time` |
| Transformation | `PUT(jobcat1, $jobcat.)` creates `JOBCAT` (CF-PREP-01). Numeric prefixes look like sort keys. |
| Business meaning | This file does **not** define LJD/NLJD/NLP/NLO/ADVD/etc. in prose. Only the codes and formatted values are known. |
| Dependencies | CF-PREP-01 and later `JOBCAT` tables |
| Potential edge cases | `FULL`/`PART` are in `$jobcat` but `jobftpt` is later formatted with **`$jobcat1`**, which is not defined in this file (CF-AMB-01). |

---

## Preparation (`DATA fornalprepts`)

### CF-PREP-00 — Load and drop columns

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 50–51 |
| Input fields | All columns on `erss2025.erss2025` |
| Output fields | `fornalprepts` without `office_size`, `othersource`, `Field35b`, `field9b`, `Field36`, `jobdesc` |
| Transformation | `SET` + `DROP` |
| Business meaning | Those columns are not used in later operations in this file. Why they are dropped is not stated. |
| Dependencies | All later steps use `fornalprepts` |
| Potential edge cases | If a dropped column was needed later, this file never uses it. |

### CF-PREP-01 — Format `jobcat1` into `JOBCAT`

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Line 54 |
| Input fields | `jobcat1` |
| Output fields | `JOBCAT` |
| Transformation | `jobcat = put(jobcat1, $jobcat.);` |
| Business meaning | Working job-category value is the formatted `$jobcat` string, not the raw code. |
| Dependencies | CF-FMT-03 |
| Potential edge cases | Unknown `jobcat1` values become the raw value (SAS `PUT` behavior), not a documented fallback. |

### CF-PREP-02 — Law-firm job-type recodes

| Field | Value |
|---|---|
| Classification | CONFIRMED (recode); INFERRED (done for sort/display order) |
| Confidence | High for the assignments; low for the business reason |
| SAS code/location | Lines 55–57 |
| Input fields | `lfjob` |
| Output fields | `lfjob` |
| Transformation | `ADMIN` → `YADMIN`; `OTHNL` → `ZOTHNL`; `STATTY` → `ATTYST` |
| Business meaning | Values are rewritten. This file does not say what ADMIN/OTHNL/STATTY mean. Prefixes `Y`/`Z` and `ATTYST` look like sort-order keys, but that is **INFERRED**. |
| Dependencies | CF-C-16, CF-S-16 |
| Potential edge cases | Other `lfjob` values are unchanged. |

### CF-PREP-03 — Unknown region `0` → `X`

| Field | Value |
|---|---|
| Classification | CONFIRMED (assignment); AMBIGUOUS (meaning of `0` / `X`) |
| Confidence | High that `0` becomes `X`; low for what those codes mean |
| SAS code/location | Line 59 |
| Input fields | `jobreg` |
| Output fields | `jobreg` |
| Transformation | `if jobreg = '0' then jobreg = 'X';` |
| Business meaning | Later, counts of states exclude `X` and comment “exclude forreign locations” (CF-C-19). This file never defines `0` or `X`. |
| Dependencies | CF-C-17, CF-C-19, CF-S-17 |
| Potential edge cases | Character comparison is exact `'0'`, not numeric zero. |

### CF-PREP-04 — Source recodes for sort-like prefixes

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 60–61 |
| Input fields | `source` |
| Output fields | `source` |
| Transformation | `OTHER` → `ZOTHER`; `OCI` → `AOCI` |
| Business meaning | Stored codes change before source counts. `$source` no longer matches these stored values. |
| Dependencies | CF-FMT-02, CF-P2-02 |
| Potential edge cases | Other source codes are unchanged. |

### CF-PREP-05 — Clerkship / government employer-type recodes

| Field | Value |
|---|---|
| Classification | CONFIRMED (assignments); INFERRED (these are clerkship/government collapse codes) |
| Confidence | High for the assignments; medium that `emptype1` is court/employer subtype |
| SAS code/location | Lines 62–64 |
| Input fields | `emptype1` |
| Output fields | `emptype1` |
| Transformation | `JCLOGV` → `JCTLOG`; `JCINGV` → `JCXIOG`; `JCOTGV`, `JCUNGV`, `JC` → `JCUGOV` |
| Business meaning | Several raw codes are collapsed. This file does not define the raw codes. |
| Dependencies | CF-C-13, CF-S-13 |
| Potential edge cases | `JC` is grouped with `JCOTGV` and `JCUNGV`. Whether that is intentional is not stated. |

### CF-PREP-06 — Gender (`sex3`) recodes

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 67–69 |
| Input fields | `sex3` |
| Output fields | `sex3` |
| Transformation | `W` → `F`; `X` → `N`; `ND` → blank |
| Business meaning | Later gender tables keep only `M`, `F`, `N`. `ND` becomes blank and is excluded from those tables. This file does not define `W`, `X`, `N`, or `ND`. |
| Dependencies | CF-C-02, CF-C-04, CF-S-01, CF-S-03 |
| Potential edge cases | Blank `sex3` is dropped from gender counts. `sex` (not `sex3`) recodes are commented out (CF-DEAD-02). |

---

## Count operations (part 1)

Unless noted, each count is `PROC FREQ` `BY code` with `OUT=` containing `code`, the table variable(s), `count`, and `percent`. Then `analvar` / `newvar` are attached.

### CF-C-01 — Total graduates (`schrept1`, `analvar=A`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 74–96 |
| Input fields | `code` |
| Output fields | `schrept1`: `code`, `count`, `percent`, `newvar='A'`, `analvar='A'` |
| Transformation | Frequency of `code` with no `WHERE` |
| Business meaning | One row per school: headcount of all rows in `fornalprepts`. Title calls this “COUNT OF total grads”. |
| Dependencies | CF-PREP-00 |
| Potential edge cases | Every input row is a “grad” only if the source file is already one row per graduate. That is **INFERRED**, not proven here. |

### CF-C-02 — Counts by gender (`schrept2`, `analvar=B`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 105–127 |
| Input fields | `code`, `sex3` |
| Output fields | `newvar = sex3`, `analvar = 'B'`, `count` |
| Transformation | `WHERE sex3 in ('M','F','N')` then `TABLE sex3` |
| Business meaning | School counts by recoded gender. Other / blank `sex3` omitted. |
| Dependencies | CF-PREP-06 |
| Potential edge cases | Schools with only blank/`ND` gender contribute no `schrept2` rows. |

### CF-C-03 — Counts by minority status (`schrept3`, `analvar=C`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (filter/codes); INFERRED (`MINOR` / `NONMIN` meaning) |
| Confidence | High for the filter; medium for the demographic meaning |
| SAS code/location | Lines 137–155 |
| Input fields | `code`, `minstat` |
| Output fields | `newvar = minstat`, `analvar = 'C'` |
| Transformation | `WHERE minstat in ('NONMIN','MINOR')` then `TABLE minstat` |
| Business meaning | Title: “COUNT by MINOR status”. Other `minstat` values are omitted. |
| Dependencies | CF-PREP-00 |
| Potential edge cases | Unknown or blank `minstat` is excluded. |

### CF-C-04 — Counts by minority status × gender (`schrept4`, `analvar=C1`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 163–182 |
| Input fields | `code`, `minstat`, `sex3` |
| Output fields | `newvar = minstat\|\|sex3`, `analvar = 'C1'` |
| Transformation | `WHERE minstat in ('NONMIN','MINOR') and sex3 in ('F','M','N')` then `TABLE minstat*sex3` |
| Business meaning | Cross-tab of the two filters. `newvar` is concatenated (for example `MINORF`). |
| Dependencies | CF-PREP-06 |
| Potential edge cases | SAS `\|\|` does not trim; unexpected spaces would appear in `newvar`. |

### CF-C-05 — Employment-status counts (`schrept5`, `analvar=D`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 190–209 |
| Input fields | `code`, `jobcat1`, `JOBCAT` |
| Output fields | `newvar = JOBCAT`, `analvar = 'D'` |
| Transformation | `WHERE jobcat1 ne 'UNKN'` then `TABLE JOBCAT` |
| Business meaning | Title: “COUNT of employment status”. `UNKN` raw codes are excluded before tabulating formatted `JOBCAT`. |
| Dependencies | CF-PREP-01 |
| Potential edge cases | Rows with missing `jobcat1` are included if they are not the string `UNKN`. |

### CF-C-06 — Employed vs advanced-degree rollup (`schrept6`, `analvar=D1`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (what the statements do); AMBIGUOUS (intended exclusions vs written codes) |
| Confidence | High for the assignments that match; high that the `NOT IN` list does not match `$jobcat` outputs |
| SAS code/location | Lines 218–255 |
| Input fields | `code`, `jobcat1`, `JOBCAT`, `count`, `percent` |
| Output fields | `newvar` in (`EMPL`, `6-ADVD`, or missing), `analvar = 'D1'`, summed `count` and `percent` |
| Transformation | 1. Same freq as CF-C-05. 2. `WHERE JOBCAT not in ('7-USKW','8-UNWK')`. 3. `6-ADVD` stays `6-ADVD`. 4. `1-LJD`,`2-NLJD`,`3-NLP`,`4-NLO`,`5-WUNK` → `EMPL`. 5. `PROC MEANS SUM` by `code newvar`. |
| Business meaning | Title: “COUNT of degree and employed”. Comment implies some statuses are dropped before employed/advanced-degree rollup. |
| Dependencies | CF-PREP-01, CF-FMT-03 |
| Potential edge cases | `$jobcat` produces `8-USKW` and `9-UNWK`, not `'7-USKW'` or `'8-UNWK'`. Those `NOT IN` values therefore do not exclude the formatted USKW/UNWK rows. `7-UDEF` is not assigned to `EMPL` or `6-ADVD`, so `newvar` can be missing and then summed as its own group. Summing `percent` is what the code does; it is not a standard percent-of-total. |

### CF-C-07 — Full-time / part-time × job category (`schrept6a` counts, `analvar=D3`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (structure); AMBIGUOUS (`$jobcat1` and later overwrite) |
| Confidence | High that this count is built here and copied into `schoolcounts2025`; low that `jobftpt` labels are correct |
| SAS code/location | Lines 264–286 |
| Input fields | `code`, `jobftpt`, `JOBCAT` |
| Output fields | `newvar = compress(jobcat\|\|jobftpt)`, `analvar = 'D3'` |
| Transformation | `WHERE jobftpt ne ' '` then `TABLE jobftpt*jobcat` with `FORMAT jobftpt $jobcat1.` |
| Business meaning | Title: “COUNT of ft/pt jobs”. Added “for 2002 and then modified in 2003”. |
| Dependencies | CF-PREP-01, CF-AMB-01; stacked in CF-C-20 **before** the name `schrept6a` is reused for salaries (CF-S-05) |
| Potential edge cases | `$jobcat1` is not defined in this file. Formatted vs raw `jobftpt` is unknown. Blank `jobftpt` excluded. |

### CF-C-08 — Public vs private employment (`schrept7`, `analvar=D2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 295–335 |
| Input fields | `code`, `empgen` |
| Output fields | `newvar` in (`PUBLIC`, `PRIVATE`), `analvar = 'D2'`, summed `count`/`percent` |
| Transformation | `WHERE empgen ne ' '`. `ACAD`,`GOVT`,`CLERK`,`PUBINT` → `PUBLIC`. `BUS`,`FIRM` → `PRIVATE`. `EMPUNK` rows `DELETE`. Then `PROC MEANS SUM` by `code newvar`. |
| Business meaning | Title: “COUNT public/private emp”. Sector grouping is explicit. |
| Dependencies | CF-PREP-00 |
| Potential edge cases | Any other non-blank `empgen` is kept with missing `newvar` and can form its own `MEANS` group. `EMPUNK` is removed from this count only. |

### CF-C-09 — Counts by employer type (`schrept8`, `analvar=E1`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 343–362 |
| Input fields | `code`, `empgen` |
| Output fields | `newvar = empgen` (after recode), `analvar = 'E1'` |
| Transformation | `WHERE empgen ne ' '`. If `empgen = 'EMPUNK'` then `empgen = 'ZEMPUN'`. |
| Business meaning | Title: “COUNT by employer type”. `ZEMPUN` looks like a sort-to-end key (**INFERRED**). |
| Dependencies | CF-PREP-00 |
| Potential edge cases | Unknown `empgen` values are counted as-is. |

### CF-C-10 — Academic jobs by `JOBCAT` (`schrept9a` counts, `analvar=E2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 369–388 |
| Input fields | `code`, `empgen`, `JOBCAT` |
| Output fields | `newvar = JOBCAT`, `analvar = 'E2'` |
| Transformation | `WHERE empgen = 'ACAD'` then `TABLE JOBCAT` |
| Business meaning | Title: “legal/nonlegal academic jobs”. Comment: new for 2011. |
| Dependencies | CF-PREP-01; stacked before later overwrite of `schrept9a` (CF-S-10) |
| Potential edge cases | Academic rows with any `JOBCAT`, including statuses not in the salary `1-LJD`…`4-NLO` list. |

### CF-C-11 — Business jobs by `JOBCAT` (`schrept9`, `analvar=E3`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 394–413 |
| Input fields | `code`, `empgen`, `JOBCAT` |
| Output fields | `newvar = JOBCAT`, `analvar = 'E3'` |
| Transformation | `WHERE empgen = 'BUS'` then `TABLE JOBCAT` |
| Business meaning | Title: “legal/nonlegal biz jobs”. |
| Dependencies | CF-PREP-01 |
| Potential edge cases | Same as CF-C-10 for unlisted `JOBCAT` values. |

### CF-C-12 — Firm jobs by `JOBCAT` (`schrept9b` counts, `analvar=E4`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 419–438 |
| Input fields | `code`, `empgen`, `JOBCAT` |
| Output fields | `newvar = JOBCAT`, `analvar = 'E4'` |
| Transformation | `WHERE empgen = 'FIRM'` then `TABLE JOBCAT` |
| Business meaning | Title text still says `schrept9` but the dataset is `schrept9b`: “legal/nonlegal firm jobs”. |
| Dependencies | CF-PREP-01 |
| Potential edge cases | Title/dataset name mismatch is documentation-only. |

### CF-C-13 — Government jobs by `JOBCAT` (`schrept9c` counts, `analvar=E5`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 444–463 |
| Input fields | `code`, `empgen`, `JOBCAT` |
| Output fields | `newvar = JOBCAT`, `analvar = 'E5'` |
| Transformation | `WHERE empgen = 'GOVT'` then `TABLE JOBCAT` |
| Business meaning | Title: “legal/nonlegal govt jobs”. |
| Dependencies | CF-PREP-01 |
| Potential edge cases | None stated beyond missing `GOVT` rows. |

### CF-C-14 — Clerkships by court/employer subtype (`schrept9cc`, `analvar=E55`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (table); INFERRED (court breakdown) |
| Confidence | High for the table; medium that `emptype1` is court type |
| SAS code/location | Lines 468–488 |
| Input fields | `code`, `empgen`, `emptype1` |
| Output fields | `newvar = emptype1`, `analvar = 'E55'` |
| Transformation | `WHERE empgen = 'CLERK'` then `TABLE emptype1` |
| Business meaning | Comment: “judicial clerkships by court”, added 2013. |
| Dependencies | CF-PREP-05 |
| Potential edge cases | Clerk rows with missing `emptype1` still appear as a missing-level row if `PROC FREQ` emits one. |

### CF-C-15 — Public-interest jobs by `JOBCAT` (`schrept9d` counts, `analvar=E6`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 493–512 |
| Input fields | `code`, `empgen`, `JOBCAT` |
| Output fields | `newvar = JOBCAT`, `analvar = 'E6'` |
| Transformation | `WHERE empgen = 'PUBINT'` then `TABLE JOBCAT` |
| Business meaning | Title: “legal/nonlegal pi jobs”. |
| Dependencies | CF-PREP-01 |
| Potential edge cases | None stated. |

### CF-C-16 — Law firm jobs by size (`schrept10`, `analvar=FIRM`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (mapping of listed codes); AMBIGUOUS (unlisted `firm1`) |
| Confidence | High for `S` and `1`–`8` |
| SAS code/location | Lines 519–545 |
| Input fields | `code`, `empgen`, `firm1` |
| Output fields | `newvar` in (`SOLO`,`LF1`…`LF8`) or missing, `analvar = 'FIRM'` |
| Transformation | `WHERE empgen = 'FIRM'` then `TABLE firm1`. `S`→`SOLO`; `1`–`8`→`LF1`–`LF8`. |
| Business meaning | Title: “law firm jobs by size”. This file does not define how many attorneys each `firm1` code represents. |
| Dependencies | CF-PREP-00 |
| Potential edge cases | Other `firm1` values keep missing `newvar`. |

### CF-C-17 — Types of law-firm jobs (`schrept10a` counts, `analvar=FIRM2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 551–570 |
| Input fields | `code`, `empgen`, `lfjob` |
| Output fields | `newvar = lfjob`, `analvar = 'FIRM2'` |
| Transformation | `WHERE empgen = 'FIRM' and lfjob ne ' '` then `TABLE lfjob` |
| Business meaning | Title: “types of law firm jobs”. Uses recoded `lfjob` (CF-PREP-02). |
| Dependencies | CF-PREP-02; stacked before `schrept10a` is reused for firm-size salaries (CF-S-15) |
| Potential edge cases | Blank `lfjob` excluded. |

### CF-C-18 — Jobs by region (`schrept11`, `analvar=JOBREG1`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (filter); AMBIGUOUS (character compare and meaning of region codes) |
| Confidence | High that `WHERE jobreg ge '0'` is applied; low for which codes that includes |
| SAS code/location | Lines 576–594 |
| Input fields | `code`, `jobreg` |
| Output fields | `newvar = jobreg`, `analvar = 'JOBREG1'` |
| Transformation | `WHERE jobreg ge '0'` then `TABLE jobreg`. `noprint` is commented, so this freq may print. |
| Business meaning | Title: “jobs by region”. After CF-PREP-03, former `'0'` is `'X'`. In ASCII, `'X' ge '0'` is true, so `X` is included here. |
| Dependencies | CF-PREP-03 |
| Potential edge cases | Character `ge '0'` is not a numeric region test. Missing `jobreg` is typically excluded. |

### CF-C-19 — In-state / out-of-state (`schrept12`, `analvar=JOBREG2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 601–621 |
| Input fields | `code`, `jobreg`, `locationflag` |
| Output fields | `newvar = locationflag`, `analvar = 'JOBREG2'` |
| Transformation | Same `WHERE jobreg ge '0'` then `TABLE locationflag` |
| Business meaning | Title: “jobs instate/out”. `locationflag` values are not listed in this file except a commented `OUTOFSTATE` patch (CF-DEAD-03). |
| Dependencies | CF-PREP-03 |
| Potential edge cases | Blank `locationflag` can be counted. Commented school `90503` override is **not** applied. |

### CF-C-20 — Number of states where grads are employed (`schrept13`, `analvar=JOBREG3`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (steps); AMBIGUOUS (whether the second `PROC FREQ` is the intended state-count) |
| Confidence | Medium |
| SAS code/location | Lines 628–656 |
| Input fields | `code`, `jobreg`, `jobst` |
| Output fields | `newvar = 'JOBREG3'`, `analvar = 'JOBREG3'`, `count` = sum of the second freq’s `count` |
| Transformation | 1. `WHERE jobreg gt '0' and jobreg ne 'X'` then `TABLE jobst`. Comment: exclude foreign locations. 2. Another `PROC FREQ` of `jobst` by `code` writing `schrept13` again. 3. `PROC MEANS SUM` of `count` by `code`. |
| Business meaning | Title: “# states where grads employed”. After the second freq, each remaining state is typically count 1, so the sum is the number of distinct `jobst` values. |
| Dependencies | CF-PREP-03 |
| Potential edge cases | `'X'` excluded. `jobreg gt '0'` is character. `jobst` coding for foreign vs US is not in this file. If the second freq were skipped, `count` would be graduate counts, not state counts. |

### CF-C-21 — Stack all part-1 counts

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 664–674 |
| Input fields | `schrept1`–`schrept13` including `schrept6a` and `schrept9*` **as they exist at this line** |
| Output fields | `schoolcounts2025`, sorted by `code analvar newvar` |
| Transformation | `SET` concatenation |
| Business meaning | All part-1 count rows live in one table keyed by school + analysis bucket + detail code. |
| Dependencies | CF-C-01 through CF-C-20 |
| Potential edge cases | Later reuse of `schrept6a`, `schrept9a`–`schrept9d`, `schrept10a` does **not** change this stack. |

---

## Salary operations

Shared pattern: `PROC UNIVARIATE` on `salftperm` → `n`, `mean`, `pct25` (Q1), `median`, `pct75` (Q3). Then keep rows with **`n ge 5`**.

SAS `PROC UNIVARIATE` omits missing `salftperm` from those statistics. That default is **CONFIRMED** SAS behavior, not an extra `WHERE` in this file.

### CF-S-00 — Salary suppression (`n ge 5`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Every salary `DATA` step: lines 702, 736, 771, 806, 840, 874, 906, 942, 976, 1009, 1042, 1075, 1109, 1143, 1176, 1220, 1255, 1288, 1319 |
| Input fields | Univariate `n` |
| Output fields | Salary row kept only if `n ge 5` |
| Transformation | `WHERE n ge 5` |
| Business meaning | Salary statistics are suppressed when fewer than 5 salaries are in that by-group. Counts in part 1 are **not** suppressed by this rule. |
| Dependencies | All CF-S-01…CF-S-19 |
| Potential edge cases | `n` is the univariate n of non-missing `salftperm`, not the employment count. Groups with 5 jobs but 4 salaries are suppressed. There is no other threshold in this file. |

### CF-S-01 — Salaries by gender (`schrept2a`, `analvar=B`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 681–709 |
| Input fields | `code`, `sex3`, `salftperm` |
| Output fields | `analvar='B'`, `newvar=sex3`, `n`, `mean`, `pct25`, `median`, `pct75` |
| Transformation | `BY code sex3` and `WHERE sex3 in ('F','M','N')` |
| Business meaning | Title: “salaries by gender”. |
| Dependencies | CF-PREP-06, CF-S-00 |
| Potential edge cases | Same gender filter as CF-C-02. |

### CF-S-02 — Salaries by minority status (`schrept3a`, `analvar=C`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 718–743 |
| Input fields | `code`, `minstat`, `salftperm` |
| Output fields | `analvar='C'`, `newvar=minstat`, salary stats |
| Transformation | `WHERE minstat in ('NONMIN','MINOR')` |
| Business meaning | Title: “salaries by MINORority”. |
| Dependencies | CF-S-00 |
| Potential edge cases | Same `minstat` filter as CF-C-03. |

### CF-S-03 — Salaries by minority status × gender (`schrept4a`, `analvar=C1`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 752–778 |
| Input fields | `code`, `minstat`, `sex3`, `salftperm` |
| Output fields | `analvar='C1'`, `newvar=minstat\|\|sex3` |
| Transformation | Same filters as CF-C-04 |
| Business meaning | Title: “salaries by MINORority and gender”. |
| Dependencies | CF-PREP-06, CF-S-00 |
| Potential edge cases | Concatenation same as CF-C-04. |

### CF-S-04 — Salaries by job category (`schrept5a`, `analvar=D`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 787–813 |
| Input fields | `code`, `JOBCAT`, `salftperm` |
| Output fields | `analvar='D'`, `newvar=JOBCAT` |
| Transformation | `WHERE JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO')` |
| Business meaning | Title: “salaries by job type”. Only those four formatted categories. |
| Dependencies | CF-PREP-01, CF-S-00 |
| Potential edge cases | `5-WUNK`, `6-ADVD`, and other `JOBCAT` values get no salary row here. |

### CF-S-05 — Overall salaries labeled employed (`schrept6a` salaries, `analvar=D1`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (code); AMBIGUOUS (title vs filter) |
| Confidence | High that there is no `empgen`/`jobftpt`/`JOBCAT` where; low that this is “all ft jobs” |
| SAS code/location | Lines 822–847 |
| Input fields | `code`, `salftperm` |
| Output fields | `analvar='D1'`, `newvar='EMPL'` |
| Transformation | `BY code` only, then `n ge 5` |
| Business meaning | Title: “salaries for all ft jobs”. The step does **not** filter `jobftpt` or employment status. It uses every non-missing `salftperm` at the school. |
| Dependencies | CF-S-00; overwrites count dataset name `schrept6a` after CF-C-21 |
| Potential edge cases | Title and `newvar='EMPL'` do not match the unrestricted `WHERE`. |

### CF-S-06 — Private-sector salaries (`schrept7a`, `analvar=D2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 856–881 |
| Input fields | `code`, `empgen`, `salftperm` |
| Output fields | `analvar='D2'`, `newvar='PRIVATE'` |
| Transformation | `WHERE empgen in ('BUS','FIRM')` |
| Business meaning | Title: “salaries for private jobs”. Matches CF-C-08 private list. |
| Dependencies | CF-S-00, CF-C-08 |
| Potential edge cases | None beyond missing `empgen`. |

### CF-S-07 — Public-sector salaries (`schrept7b`, `analvar=D2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 890–913 |
| Input fields | `code`, `empgen`, `salftperm` |
| Output fields | `analvar='D2'`, `newvar='PUBLIC'` |
| Transformation | `WHERE empgen in ('ACAD','GOVT','CLERK','PUBINT')` |
| Business meaning | Title: “salaries for public jobs”. Matches CF-C-08 public list. |
| Dependencies | CF-S-00, CF-C-08 |
| Potential edge cases | None beyond missing `empgen`. |

### CF-S-08 — Salaries by employer type (`schrept8a`, `analvar=E1`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 923–949 |
| Input fields | `code`, `empgen`, `salftperm` |
| Output fields | `analvar='E1'`, `newvar=empgen` (`EMPUNK` → `ZEMPUN`) |
| Transformation | `BY code empgen` with no `empgen` where; then recode `EMPUNK` |
| Business meaning | Title: “salaries by employer type”. |
| Dependencies | CF-S-00, CF-C-09 |
| Potential edge cases | Blank `empgen` can form a by-group. Unlike CF-C-08, `EMPUNK` is recoded, not deleted. |

### CF-S-09 — Business salaries by legal/non-legal (`schrept9a` salaries, `analvar=E3`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 958–983 |
| Input fields | `code`, `empgen`, `JOBCAT`, `salftperm` |
| Output fields | `analvar='E3'`, `newvar=JOBCAT` |
| Transformation | `WHERE empgen = 'BUS' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO')` |
| Business meaning | Title: “salaries biz jobs/legal non-legal”. |
| Dependencies | CF-S-00; overwrites count `schrept9a` after CF-C-21 |
| Potential edge cases | Salary `JOBCAT` list is narrower than count CF-C-10/CF-C-11. |

### CF-S-10 — Academic salaries by legal/non-legal (`schrept9b` salaries, `analvar=E2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 991–1016 |
| Input fields | `code`, `empgen`, `JOBCAT`, `salftperm` |
| Output fields | `analvar='E2'`, `newvar=JOBCAT` |
| Transformation | `WHERE empgen = 'ACAD' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO')` |
| Business meaning | Title: “salaries academic jobs/legal non-legal”. |
| Dependencies | CF-S-00 |
| Potential edge cases | Overwrites count dataset name `schrept9b`. |

### CF-S-11 — Firm salaries by legal/non-legal (`schrept9c` salaries, `analvar=E4`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1024–1049 |
| Input fields | `code`, `empgen`, `JOBCAT`, `salftperm` |
| Output fields | `analvar='E4'`, `newvar=JOBCAT` |
| Transformation | `WHERE empgen = 'FIRM' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO')` |
| Business meaning | Title: “salaries firm jobs/legal non-legal”. |
| Dependencies | CF-S-00 |
| Potential edge cases | Overwrites count name `schrept9c`. |

### CF-S-12 — Government salaries by legal/non-legal (`schrept9d` salaries, `analvar=E5`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1057–1082 |
| Input fields | `code`, `empgen`, `JOBCAT`, `salftperm` |
| Output fields | `analvar='E5'`, `newvar=JOBCAT` |
| Transformation | `WHERE empgen = 'GOVT' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO')` |
| Business meaning | Title: “salaries govt jobs/legal non-legal”. |
| Dependencies | CF-S-00 |
| Potential edge cases | Overwrites count name `schrept9d`. |

### CF-S-13 — Clerkship salaries by `emptype1` (`schrept9dd`, `analvar=E55`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1090–1116 |
| Input fields | `code`, `empgen`, `emptype1`, `salftperm` |
| Output fields | `analvar='E55'`, `newvar=emptype1` |
| Transformation | `WHERE empgen = 'CLERK'` (no `JOBCAT` restriction) |
| Business meaning | Title: “salaries clerkships”. Comment: added 2013. |
| Dependencies | CF-PREP-05, CF-S-00 |
| Potential edge cases | Title line 1105 still says “govt jobs”. |

### CF-S-14 — Public-interest salaries by legal/non-legal (`schrept9e`, `analvar=E6`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1124–1150 |
| Input fields | `code`, `empgen`, `JOBCAT`, `salftperm` |
| Output fields | `analvar='E6'`, `newvar=JOBCAT` |
| Transformation | `WHERE empgen = 'PUBINT' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO')` |
| Business meaning | Title: “salaries pi jobs/legal non-legal”. |
| Dependencies | CF-S-00 |
| Potential edge cases | None beyond the four-category filter. |

### CF-S-15 — Salaries by firm size (`schrept10a` salaries, `analvar=FIRM`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (codes `1`–`8`); AMBIGUOUS (`firm1='S'` / SOLO) |
| Confidence | High that `1`–`8` map to `LF1`–`LF8`; high that `S` is **not** mapped here |
| SAS code/location | Lines 1157–1191 |
| Input fields | `code`, `empgen`, `firm1`, `salftperm` |
| Output fields | `analvar='FIRM'`, `newvar` = `LF1`–`LF8` or missing |
| Transformation | `WHERE empgen = 'FIRM'`. Maps `1`–`8` only. No `S` → `SOLO`. |
| Business meaning | Title: “salaries by firm size”. |
| Dependencies | CF-S-00; overwrites count `schrept10a` |
| Potential edge cases | Solo (`S`) salaries can have missing `newvar` and will not merge to count `SOLO` rows. |

### CF-S-16 — Salaries by type of firm job (`schrept10b`, `analvar=FIRM2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1199–1227 |
| Input fields | `code`, `empgen`, `lfjob`, `salftperm` |
| Output fields | `analvar='FIRM2'`, `newvar=lfjob` |
| Transformation | `WHERE empgen = 'FIRM' and lfjob ne ' '` |
| Business meaning | Title: “salaries by type of firm job”. |
| Dependencies | CF-PREP-02, CF-S-00 |
| Potential edge cases | Same recoded `lfjob` values as CF-C-17. |

### CF-S-17 — Salaries by region (`schrept11a`, `analvar=JOBREG1`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1237–1262 |
| Input fields | `code`, `jobreg`, `salftperm` |
| Output fields | `analvar='JOBREG1'`, `newvar=jobreg` |
| Transformation | `WHERE jobreg ge '0'` |
| Business meaning | Title: “salaries by region”. Same character filter as CF-C-18, so `X` is included. |
| Dependencies | CF-PREP-03, CF-S-00 |
| Potential edge cases | Same `ge '0'` issues as CF-C-18. |

### CF-S-18 — Salaries by in-state / out-of-state (`schrept12a`, `analvar=JOBREG2`)

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1271–1295 |
| Input fields | `code`, `locationflag`, `salftperm` |
| Output fields | `analvar='JOBREG2'`, `newvar=locationflag` |
| Transformation | `WHERE locationflag ne ' '` |
| Business meaning | Title: “salaries by instate/outofstate”. |
| Dependencies | CF-S-00 |
| Potential edge cases | Unlike CF-C-19, blank `locationflag` is excluded, and there is no `jobreg ge '0'` filter. Count and salary universes can differ. |

### CF-S-19 — Salaries labeled full-time by job category (`schrept14`, `analvar=D3`)

| Field | Value |
|---|---|
| Classification | CONFIRMED (code); AMBIGUOUS (hard-coded `FULL` vs actual `jobftpt`) |
| Confidence | High that `jobftpt` is set to `'FULL'` after the fact; high that `jobftpt` is not filtered |
| SAS code/location | Lines 1304–1326 |
| Input fields | `code`, `JOBCAT`, `salftperm` |
| Output fields | `analvar='D3'`, `jobftpt='FULL'`, `newvar = compress(jobcat\|\|'FULL')` |
| Transformation | `WHERE JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO')` only |
| Business meaning | These rows are stored as D3 full-time-looking keys. The statistic is **not** limited to `jobftpt` full-time in this step. |
| Dependencies | CF-S-00, CF-C-07 |
| Potential edge cases | Merge to CF-C-07 succeeds only if count `newvar` is `1-LJDFULL` / formatted equivalent. `$jobcat1` on counts may prevent a match. |

### CF-S-20 — Stack all salary tables

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1332–1342 |
| Input fields | `schrept2a`…`schrept14` as they exist after the salary section |
| Output fields | `schoolsalaries2025`, sorted by `code analvar newvar` |
| Transformation | `SET` concatenation |
| Business meaning | Salary facts share the same key as counts for the merge. |
| Dependencies | CF-S-01 through CF-S-19 |
| Potential edge cases | No `schrept1` / `analvar=A` salary table exists. |

---

## Merge (part 1 output)

### CF-M-01 — Merge counts and salaries

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1347–1354 |
| Input fields | `schoolcounts2025`, `schoolsalaries2025` |
| Output fields | `erss2025.schreptsummary2025` |
| Transformation | `MERGE` by `code analvar newvar`. `DROP empgen firm1 JOBCAT jobreg locationflag minstat sex3`. |
| Business meaning | One summary row per school + analysis + detail, with count fields and salary fields when both exist. |
| Dependencies | CF-C-21, CF-S-20 |
| Potential edge cases | Unmatched keys produce count-only or salary-only rows. `PROC FREQ` of `analvar`/`newvar` (lines 1361–1363) is diagnostic only. |

---

## Part 2 — duration, funding, source, timing, search status

### CF-P2-01 — Job duration overall and by employer type

| Field | Value |
|---|---|
| Classification | CONFIRMED (mechanics); AMBIGUOUS (`duration` code meanings) |
| Confidence | High for the transpose; low for long-term/short-term labels |
| SAS code/location | Lines 1370–1426 |
| Input fields | `code`, `empgen`, `duration` |
| Output fields | `duration_final`: duration codes as columns from `ID duration`, `analvar='DURATION'`, `newvar=empgen` |
| Transformation | `WHERE empgen ne ' ' and duration ne ''`. Freq of `duration`, transpose counts to duration-named columns. Repeat `duration*empgen`, transpose by `code empgen`. `SET` both together. |
| Business meaning | Comment: long-term/short-term. Actual `duration` values are whatever is in the data; they are not listed here. Overall rows have missing `empgen`, so `newvar` is missing on those rows. |
| Dependencies | CF-PREP-00 |
| Potential edge cases | Unusual `duration` values become extra column names. Mixing overall and by-`empgen` rows in one table leaves sparse columns. |

### CF-P2-02 — Law-school-funded jobs

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1431–1449 |
| Input fields | `code`, `schoolfund` |
| Output fields | `fund`: `perm` (renamed `count`), `newvar=schoolfund`, `analvar='LAW SCHOOL FUNDED'` |
| Transformation | Freq where `schoolfund ne ' '`, then keep `schoolfund in ('YES','Y')`, drop `percent`, `RENAME count=perm`. |
| Business meaning | Comment: “law school funded count”. Both `YES` and `Y` are treated as funded. |
| Dependencies | CF-PREP-00 |
| Potential edge cases | `NO` / other values are counted in the first freq and then dropped. `perm` is not defined elsewhere in this file. |

### CF-P2-03 — Source of jobs

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1455–1476 |
| Input fields | `code`, `source` |
| Output fields | `newvar=source`, `analvar='SOURCE'` |
| Transformation | `WHERE source ne ' '`, `TABLE source`, `FORMAT source $source.` |
| Business meaning | Title: “source of jobs”. Stored codes are post-recode (`AOCI`, `ZOTHER`, …). |
| Dependencies | CF-PREP-04, CF-FMT-02 |
| Potential edge cases | Format does not include `AOCI`/`ZOTHER`. Formatted labels are display on this freq; `newvar` is the stored `source` value after `SET sourcetable`. |

### CF-P2-04 — Timing of job offer

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1486–1508 |
| Input fields | `code`, `time1` |
| Output fields | `newvar=time1`, `analvar='TIME'` |
| Transformation | `WHERE time1 ne ' '`, `FORMAT time1 $time.`, then `if time1 = 'AFTGRD' then time1 = 'ZAFTGRD'` |
| Business meaning | Title: “time of job offers”. `ZAFTGRD` looks like a sort-to-end recode (**INFERRED**). |
| Dependencies | CF-FMT-01 |
| Potential edge cases | `BGRAD` is unchanged. Other `time1` values pass through. |

### CF-P2-05 — Search status of employed grads

| Field | Value |
|---|---|
| Classification | CONFIRMED (table); INFERRED (only employed grads — from the title, not a `WHERE` on employment) |
| Confidence | High that `status ne ' '` is the only filter; low that the population is employed-only |
| SAS code/location | Lines 1517–1537 |
| Input fields | `code`, `status` |
| Output fields | `newvar=status`, `analvar='ZSTATUS'` |
| Transformation | `WHERE status ne ' '` then `TABLE status` |
| Business meaning | Title: “SEARCH STATUS OF EMPLYED GRADS”. There is no `jobcat` / `empgen` filter in this step. |
| Dependencies | CF-PREP-00 |
| Potential edge cases | If unemployed rows have `status`, they are counted. `status` codes are not listed in this file. |

### CF-P2-06 — Stack part 2

| Field | Value |
|---|---|
| Classification | CONFIRMED |
| Confidence | High |
| SAS code/location | Lines 1544–1550 |
| Input fields | `sourcetable`, `timetable`, `statustable`, `duration_final`, `fund` |
| Output fields | `erss2025.schreptsummary2025_part2` |
| Transformation | `SET` concatenation. Diagnostic `PROC FREQ` / `PROC PRINT`. |
| Business meaning | Second summary piece described in the comment at lines 1366–1367. |
| Dependencies | CF-P2-01 through CF-P2-05 |
| Potential edge cases | Column sets differ (`perm`, duration columns, `count`). SAS aligns by name. |

---

## Commented-out / inactive statements

These are **not** applied.

### CF-DEAD-01 — Collapse some sources to OTHER

| Field | Value |
|---|---|
| Classification | AMBIGUOUS (inactive) |
| Confidence | High that it is commented; no confidence it is a current rule |
| SAS code/location | Line 58 |
| Input fields | `source` |
| Output fields | Would have set `ONLINE`,`SOCI`,`TEMPAG` to `OTHER` |
| Transformation | Commented `if` |
| Business meaning | Not executed. |
| Dependencies | None while commented |
| Potential edge cases | Do not treat this as a live business rule. |

### CF-DEAD-02 — Recode `sex` (not `sex3`)

| Field | Value |
|---|---|
| Classification | AMBIGUOUS (inactive) |
| Confidence | High that it is commented |
| SAS code/location | Lines 65–66 |
| Input fields | `sex` |
| Output fields | Would map `TW`/`W` → `F`, `TM` → `M` |
| Transformation | Commented |
| Business meaning | Live gender logic is `sex3` only (CF-PREP-06). |
| Dependencies | None while commented |
| Potential edge cases | Do not apply these mappings unless another artifact shows they still happen upstream. |

### CF-DEAD-03 — Hard-coded school `90503` location counts

| Field | Value |
|---|---|
| Classification | AMBIGUOUS (inactive) |
| Confidence | High that it is commented |
| SAS code/location | Lines 610–611 |
| Input fields | `code`, `locationflag`, `count` |
| Output fields | Would force `OUTOFSTATE` count to 77 and delete blank location for `90503` |
| Transformation | Commented |
| Business meaning | Not executed. |
| Dependencies | None while commented |
| Potential edge cases | Do not invent a current exception for school `90503`. |

---

## Ambiguity register (no invented resolution)

| ID | What was observed | What is unclear | Conservative reading |
|---|---|---|---|
| CF-AMB-01 | Line 269 uses `FORMAT jobftpt $jobcat1.` | `$jobcat1` is never defined in this program. `$jobcat` (not `$jobcat1`) contains `FULL`/`PART`. | Do not assume `jobftpt` is labeled Full-time/Part-time unless another program defines `$jobcat1`. |
| CF-AMB-02 | CF-C-06 `NOT IN ('7-USKW','8-UNWK')` | Those strings are not `$jobcat` outputs (`8-USKW`, `9-UNWK`, `7-UDEF` are). | Document the written `NOT IN` list; do not “fix” it to USKW/UNWK. |
| CF-AMB-03 | CF-S-05 title says “all ft jobs” | No FT filter on `salftperm`. | Treat as all non-missing `salftperm` by school, labeled `EMPL`. |
| CF-AMB-04 | CF-S-19 forces `jobftpt='FULL'` | Salaries are not filtered to full-time. | Store the D3 key as written; do not claim only FT salaries. |
| CF-AMB-05 | CF-S-15 has no `SOLO` map | Count side maps `S`→`SOLO`. | Solo salaries may not align to count `SOLO`. |
| CF-AMB-06 | `jobreg` `'0'`→`'X'`, then `ge '0'` / `gt '0'` | Region codebook and foreign-location rule are not in this file. | Keep the character comparisons as written. |
| CF-AMB-07 | `salftperm` name | Not defined in this file. | Use the variable as the salary measure; do not add extra FT/permanent filters beyond what each step writes. |
| CF-AMB-08 | `PROC MEANS` sums `percent` | Sum of percents is not a defined business percent. | Preserve summed `percent` if the output column is kept; do not reinterpret it. |
| CF-AMB-09 | Dataset names reused | Easy to think later salary tables replace earlier counts. | Counts are already stacked in `schoolcounts2025` before overwrites. |
| CF-AMB-10 | Print `WHERE CODE LT '23200'` / `'23000'` / `LE '30000'` / `GT '90000'` | Looks like school-code ranges. | Display-only unless a later program filters the saved SAS datasets. |

---

## Classification summary

| Class | Rule IDs |
|---|---|
| CONFIRMED | CF-FMT-01/02/03 (mappings), CF-PREP-00/01/03/04/05/06, CF-C-01–05, CF-C-08–19, CF-C-21, CF-S-00–04, CF-S-06–14, CF-S-16–18, CF-S-20, CF-M-01, CF-P2-02/03/04/06 |
| INFERRED (plus a CONFIRMED mechanic) | CF-PREP-02 (sort-order reason), CF-C-14 (court), CF-P2-01 (long/short labels), CF-P2-05 (employed-only population), parts of format label wording |
| AMBIGUOUS | CF-C-06 (`NOT IN` codes), CF-C-07 (`$jobcat1`), CF-C-20 (state-count method), CF-S-05 (title vs filter), CF-S-15 (solo), CF-S-19 (hard-coded FULL), CF-AMB-01–10, CF-DEAD-01–03 |

No C# design is proposed here. No missing employment-status or salary-suppression rule was invented. Salary suppression observed in this file is **`n ge 5`** on salary by-groups only.
