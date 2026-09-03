# Combined Business Rules — Traceability

Sources (read-only; no rules added beyond these):

- `docs/capstone/createschrptfiles-analysis.md` → `legacy/sas/createschrptfiles2025.sas`
- `docs/capstone/schreptsummary-analysis.md` → `legacy/sas/schreptsummary_2025.sas`
- Printed notes on the baseline report, only where that analysis already quoted them

Rules that look similar (for example builder salary `n ge 5` and the report note “at least five salaries”) are **both kept**. Inactive commented SAS is kept and marked inactive.

Confidence is the classification from the source analysis: **CONFIRMED**, **INFERRED**, **AMBIGUOUS**. Mixed labels are preserved.

**Test Required**

- **Yes** — numbers, membership, keys, labels, suppression, or visible report content
- **Negative** — inactive SAS; test that the rule is **not** applied
- **No** — session/style only, or unused leftover that does not change stored or printed results

SAS Source abbreviations:

- `createschrptfiles2025.sas`
- `schreptsummary_2025.sas`

---

## Traceability table

| Rule ID | SAS Source | Input | Transformation | Calculation | Expected Output | Confidence | Test Required |
|---|---|---|---|---|---|---|---|
| CF-FMT-01 | createschrptfiles2025.sas | `BGRAD`, `AFTGRD` | `$time` value labels | none | `Before Graduation`, `After Graduation` (display) | CONFIRMED mapping; INFERRED meaning | Yes |
| CF-FMT-02 | createschrptfiles2025.sas | source codes (`OCI`, `JOBFRC`, …) | `$source` value labels | none | Display labels for listed codes | CONFIRMED mapping; INFERRED wording | Yes |
| CF-FMT-03 | createschrptfiles2025.sas | `jobcat1` / `FULL` / `PART` | `$jobcat` maps to `1-LJD`…`9-UNWK`, `UNKN`, `Full-time`, `Part-time` | none | Formatted `JOBCAT` strings | CONFIRMED mapping; AMBIGUOUS mixed use | Yes |
| CF-PREP-00 | createschrptfiles2025.sas | `erss2025.erss2025` | `SET`; drop `office_size`, `othersource`, `Field35b`, `field9b`, `Field36`, `jobdesc` | none | `fornalprepts` without dropped columns | CONFIRMED | Yes |
| CF-PREP-01 | createschrptfiles2025.sas | `jobcat1` | `PUT(jobcat1, $jobcat.)` | none | `JOBCAT` formatted value | CONFIRMED | Yes |
| CF-PREP-02 | createschrptfiles2025.sas | `lfjob` | `ADMIN`→`YADMIN`; `OTHNL`→`ZOTHNL`; `STATTY`→`ATTYST` | none | Recoded `lfjob` | CONFIRMED recode; INFERRED sort reason | Yes |
| CF-PREP-03 | createschrptfiles2025.sas | `jobreg` | `if jobreg='0' then jobreg='X'` | none | `X` in place of `0` | CONFIRMED; AMBIGUOUS meaning | Yes |
| CF-PREP-04 | createschrptfiles2025.sas | `source` | `OTHER`→`ZOTHER`; `OCI`→`AOCI` | none | Recoded `source` | CONFIRMED | Yes |
| CF-PREP-05 | createschrptfiles2025.sas | `emptype1` | `JCLOGV`→`JCTLOG`; `JCINGV`→`JCXIOG`; `JCOTGV`/`JCUNGV`/`JC`→`JCUGOV` | none | Collapsed `emptype1` | CONFIRMED; INFERRED clerkship meaning | Yes |
| CF-PREP-06 | createschrptfiles2025.sas | `sex3` | `W`→`F`; `X`→`N`; `ND`→ blank | none | Recoded `sex3` | CONFIRMED | Yes |
| CF-C-01 | createschrptfiles2025.sas | `code` | `PROC FREQ` of `code`; `newvar='A'`; `analvar='A'` | frequency count | `schrept1` total rows per school | CONFIRMED | Yes |
| CF-C-02 | createschrptfiles2025.sas | `code`, `sex3` | `WHERE sex3 in ('M','F','N')`; table `sex3`; `analvar='B'` | frequency | Gender counts; `newvar=sex3` | CONFIRMED | Yes |
| CF-C-03 | createschrptfiles2025.sas | `code`, `minstat` | `WHERE minstat in ('NONMIN','MINOR')`; `analvar='C'` | frequency | Minority-status counts | CONFIRMED filter; INFERRED meaning | Yes |
| CF-C-04 | createschrptfiles2025.sas | `code`, `minstat`, `sex3` | Cross-tab with both filters; `newvar=minstat\|\|sex3`; `analvar='C1'` | frequency | Concatenated cross-tab counts | CONFIRMED | Yes |
| CF-C-05 | createschrptfiles2025.sas | `jobcat1`, `JOBCAT` | `WHERE jobcat1 ne 'UNKN'`; table `JOBCAT`; `analvar='D'` | frequency | Employment-status counts | CONFIRMED | Yes |
| CF-C-06 | createschrptfiles2025.sas | `JOBCAT`, `count`, `percent` | `NOT IN ('7-USKW','8-UNWK')`; `6-ADVD` stays; listed cats → `EMPL`; `MEANS SUM` | sum `count` and `percent` by `code newvar` | `analvar='D1'` rollup | CONFIRMED statements; AMBIGUOUS `NOT IN` codes | Yes |
| CF-C-07 | createschrptfiles2025.sas | `jobftpt`, `JOBCAT` | `WHERE jobftpt ne ' '`; `FORMAT jobftpt $jobcat1.`; `newvar=compress(jobcat\|\|jobftpt)`; `analvar='D3'` | frequency | FT/PT × job-category counts | CONFIRMED structure; AMBIGUOUS `$jobcat1` | Yes |
| CF-C-08 | createschrptfiles2025.sas | `empgen` | `ACAD/GOVT/CLERK/PUBINT`→`PUBLIC`; `BUS/FIRM`→`PRIVATE`; delete `EMPUNK`; `MEANS SUM` | sum `count`/`percent` | `analvar='D2'` public/private | CONFIRMED | Yes |
| CF-C-09 | createschrptfiles2025.sas | `empgen` | `EMPUNK`→`ZEMPUN`; `newvar=empgen`; `analvar='E1'` | frequency | Employer-type counts | CONFIRMED | Yes |
| CF-C-10 | createschrptfiles2025.sas | `empgen='ACAD'`, `JOBCAT` | table `JOBCAT`; `analvar='E2'` | frequency | Academic job-category counts | CONFIRMED | Yes |
| CF-C-11 | createschrptfiles2025.sas | `empgen='BUS'`, `JOBCAT` | table `JOBCAT`; `analvar='E3'` | frequency | Business job-category counts | CONFIRMED | Yes |
| CF-C-12 | createschrptfiles2025.sas | `empgen='FIRM'`, `JOBCAT` | table `JOBCAT`; `analvar='E4'` | frequency | Firm job-category counts | CONFIRMED | Yes |
| CF-C-13 | createschrptfiles2025.sas | `empgen='GOVT'`, `JOBCAT` | table `JOBCAT`; `analvar='E5'` | frequency | Government job-category counts | CONFIRMED | Yes |
| CF-C-14 | createschrptfiles2025.sas | `empgen='CLERK'`, `emptype1` | table `emptype1`; `analvar='E55'` | frequency | Clerkship subtype counts | CONFIRMED table; INFERRED court | Yes |
| CF-C-15 | createschrptfiles2025.sas | `empgen='PUBINT'`, `JOBCAT` | table `JOBCAT`; `analvar='E6'` | frequency | Public-interest job-category counts | CONFIRMED | Yes |
| CF-C-16 | createschrptfiles2025.sas | `empgen='FIRM'`, `firm1` | `S`→`SOLO`; `1`–`8`→`LF1`–`LF8`; `analvar='FIRM'` | frequency | Firm-size counts | CONFIRMED listed maps; AMBIGUOUS other `firm1` | Yes |
| CF-C-17 | createschrptfiles2025.sas | `empgen='FIRM'`, `lfjob` | `WHERE lfjob ne ' '`; `newvar=lfjob`; `analvar='FIRM2'` | frequency | Law-firm job-type counts | CONFIRMED | Yes |
| CF-C-18 | createschrptfiles2025.sas | `jobreg` | `WHERE jobreg ge '0'`; `analvar='JOBREG1'` | frequency | Region counts (includes `X` after recode) | CONFIRMED filter; AMBIGUOUS compare | Yes |
| CF-C-19 | createschrptfiles2025.sas | `jobreg`, `locationflag` | same `ge '0'`; table `locationflag`; `analvar='JOBREG2'` | frequency | In/out/foreign location counts | CONFIRMED | Yes |
| CF-C-20 | createschrptfiles2025.sas | `jobreg`, `jobst` | `jobreg gt '0' and ne 'X'`; freq `jobst`; second freq; `MEANS SUM` | sum of second-freq `count` | `analvar='JOBREG3'` state count | CONFIRMED steps; AMBIGUOUS intent | Yes |
| CF-C-21 | createschrptfiles2025.sas | `schrept1`–`schrept13` at stack time | `SET` concatenate; sort `code analvar newvar` | none | `schoolcounts2025` | CONFIRMED | Yes |
| CF-S-00 | createschrptfiles2025.sas | univariate `n` | `WHERE n ge 5` on every salary `DATA` step | keep if `n >= 5` | Salary row omitted when fewer than 5 salaries | CONFIRMED | Yes |
| CF-S-01 | createschrptfiles2025.sas | `code`, `sex3`, `salftperm` | `WHERE sex3 in ('F','M','N')`; univariate | n, mean, Q1, median, Q3 | `schrept2a` `analvar='B'` | CONFIRMED | Yes |
| CF-S-02 | createschrptfiles2025.sas | `code`, `minstat`, `salftperm` | `WHERE minstat in ('NONMIN','MINOR')` | same stats | `schrept3a` `analvar='C'` | CONFIRMED | Yes |
| CF-S-03 | createschrptfiles2025.sas | `code`, `minstat`, `sex3`, `salftperm` | same filters as CF-C-04 | same stats | `schrept4a` `analvar='C1'` | CONFIRMED | Yes |
| CF-S-04 | createschrptfiles2025.sas | `JOBCAT`, `salftperm` | `WHERE JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO')` | same stats | `schrept5a` `analvar='D'` | CONFIRMED | Yes |
| CF-S-05 | createschrptfiles2025.sas | `code`, `salftperm` | `BY code` only; `newvar='EMPL'`; `analvar='D1'` | same stats | School-level salaries labeled employed | CONFIRMED code; AMBIGUOUS vs “ft jobs” title | Yes |
| CF-S-06 | createschrptfiles2025.sas | `empgen`, `salftperm` | `WHERE empgen in ('BUS','FIRM')`; `newvar='PRIVATE'` | same stats | `schrept7a` `analvar='D2'` | CONFIRMED | Yes |
| CF-S-07 | createschrptfiles2025.sas | `empgen`, `salftperm` | `WHERE empgen in ('ACAD','GOVT','CLERK','PUBINT')`; `newvar='PUBLIC'` | same stats | `schrept7b` `analvar='D2'` | CONFIRMED | Yes |
| CF-S-08 | createschrptfiles2025.sas | `empgen`, `salftperm` | `BY code empgen`; `EMPUNK`→`ZEMPUN`; `analvar='E1'` | same stats | Employer-type salaries | CONFIRMED | Yes |
| CF-S-09 | createschrptfiles2025.sas | `empgen='BUS'`, listed `JOBCAT`, `salftperm` | filter then univariate | same stats | `analvar='E3'` salaries | CONFIRMED | Yes |
| CF-S-10 | createschrptfiles2025.sas | `empgen='ACAD'`, listed `JOBCAT`, `salftperm` | filter then univariate | same stats | `analvar='E2'` salaries | CONFIRMED | Yes |
| CF-S-11 | createschrptfiles2025.sas | `empgen='FIRM'`, listed `JOBCAT`, `salftperm` | filter then univariate | same stats | `analvar='E4'` salaries | CONFIRMED | Yes |
| CF-S-12 | createschrptfiles2025.sas | `empgen='GOVT'`, listed `JOBCAT`, `salftperm` | filter then univariate | same stats | `analvar='E5'` salaries | CONFIRMED | Yes |
| CF-S-13 | createschrptfiles2025.sas | `empgen='CLERK'`, `emptype1`, `salftperm` | no `JOBCAT` filter | same stats | `analvar='E55'` clerkship salaries | CONFIRMED | Yes |
| CF-S-14 | createschrptfiles2025.sas | `empgen='PUBINT'`, listed `JOBCAT`, `salftperm` | filter then univariate | same stats | `analvar='E6'` salaries | CONFIRMED | Yes |
| CF-S-15 | createschrptfiles2025.sas | `empgen='FIRM'`, `firm1`, `salftperm` | map `1`–`8`→`LF1`–`LF8` only (no `S`→`SOLO`) | same stats | Firm-size salaries | CONFIRMED `1`–`8`; AMBIGUOUS solo | Yes |
| CF-S-16 | createschrptfiles2025.sas | `empgen='FIRM'`, `lfjob`, `salftperm` | `lfjob ne ' '`; `analvar='FIRM2'` | same stats | Firm job-type salaries | CONFIRMED | Yes |
| CF-S-17 | createschrptfiles2025.sas | `jobreg`, `salftperm` | `WHERE jobreg ge '0'` | same stats | Region salaries | CONFIRMED | Yes |
| CF-S-18 | createschrptfiles2025.sas | `locationflag`, `salftperm` | `WHERE locationflag ne ' '` (no `jobreg` filter) | same stats | In/out-of-state salaries | CONFIRMED | Yes |
| CF-S-19 | createschrptfiles2025.sas | listed `JOBCAT`, `salftperm` | force `jobftpt='FULL'`; `newvar=jobcat\|\|'FULL'`; `analvar='D3'` | same stats | D3 keys labeled FULL without FT filter | CONFIRMED code; AMBIGUOUS vs FT | Yes |
| CF-S-20 | createschrptfiles2025.sas | salary tables after salary section | `SET` concatenate; sort key | none | `schoolsalaries2025` | CONFIRMED | Yes |
| CF-M-01 | createschrptfiles2025.sas | `schoolcounts2025`, `schoolsalaries2025` | `MERGE` by `code analvar newvar`; drop listed by-vars | none | `erss2025.schreptsummary2025` | CONFIRMED | Yes |
| CF-P2-01 | createschrptfiles2025.sas | `empgen`, `duration` | freq + transpose overall and by `empgen`; `analvar='DURATION'` | counts into duration-named columns | `duration_final` | CONFIRMED mechanics; AMBIGUOUS codes | Yes |
| CF-P2-02 | createschrptfiles2025.sas | `schoolfund` | keep `YES`/`Y`; `count`→`perm`; `analvar='LAW SCHOOL FUNDED'` | frequency | Funded-job rows | CONFIRMED | Yes |
| CF-P2-03 | createschrptfiles2025.sas | `source` | `WHERE source ne ' '`; `$source` format; `analvar='SOURCE'` | frequency | Source counts (`AOCI`/`ZOTHER` stored) | CONFIRMED | Yes |
| CF-P2-04 | createschrptfiles2025.sas | `time1` | `AFTGRD`→`ZAFTGRD`; `analvar='TIME'` | frequency | Timing counts | CONFIRMED | Yes |
| CF-P2-05 | createschrptfiles2025.sas | `status` | `WHERE status ne ' '` only; `analvar='ZSTATUS'` | frequency | Search-status counts | CONFIRMED table; INFERRED employed-only | Yes |
| CF-P2-06 | createschrptfiles2025.sas | source/time/status/duration/fund | `SET` concatenate | none | `erss2025.schreptsummary2025_part2` | CONFIRMED | Yes |
| CF-DEAD-01 | createschrptfiles2025.sas | `source` | commented `ONLINE`/`SOCI`/`TEMPAG`→`OTHER` | none | **Not applied** | AMBIGUOUS inactive | Negative |
| CF-DEAD-02 | createschrptfiles2025.sas | `sex` | commented `TW`/`W`→`F`, `TM`→`M` | none | **Not applied** (live gender is `sex3`) | AMBIGUOUS inactive | Negative |
| CF-DEAD-03 | createschrptfiles2025.sas | school `90503`, `locationflag` | commented count=77 / delete blank | none | **Not applied** | AMBIGUOUS inactive | Negative |
| CF-AMB-01 | createschrptfiles2025.sas | `jobftpt` | `$jobcat1` used, never defined in this program | unknown format | Do not assume Full-time/Part-time labels | AMBIGUOUS | Yes |
| CF-AMB-02 | createschrptfiles2025.sas | `JOBCAT` | written `NOT IN ('7-USKW','8-UNWK')` | may exclude nothing | Keep written list; do not rewrite to `8-USKW`/`9-UNWK` | AMBIGUOUS | Yes |
| CF-AMB-03 | createschrptfiles2025.sas | `salftperm` | CF-S-05 has no FT filter | all non-missing `salftperm` | Labeled `EMPL` anyway | AMBIGUOUS | Yes |
| CF-AMB-04 | createschrptfiles2025.sas | CF-S-19 | hard-coded `FULL` | not filtered to FT | D3 key looks FT | AMBIGUOUS | Yes |
| CF-AMB-05 | createschrptfiles2025.sas | `firm1='S'` | count maps `SOLO`; salary step does not | possible unmatched salary | Solo salary may not merge to `SOLO` | AMBIGUOUS | Yes |
| CF-AMB-06 | createschrptfiles2025.sas | `jobreg` | `'0'`→`'X'` then `ge '0'` / `gt '0'` | character compares | Keep as written | AMBIGUOUS | Yes |
| CF-AMB-07 | createschrptfiles2025.sas | `salftperm` | name only; no extra FT/perm `WHERE` | univariate on this var | Do not add filters not in SAS | AMBIGUOUS | Yes |
| CF-AMB-08 | createschrptfiles2025.sas | `percent` | `PROC MEANS SUM` | sum of percents | Preserve if column kept | AMBIGUOUS | Yes |
| CF-AMB-09 | createschrptfiles2025.sas | reused dataset names | counts stacked before salary overwrites | none | `schoolcounts2025` keeps first `schrept6a`/`9a`/`10a` | AMBIGUOUS | Yes |
| CF-AMB-10 | createschrptfiles2025.sas | `code` | `PROC PRINT` `WHERE CODE LT/LE/GT …` | none | Display-only; saved datasets unfiltered | AMBIGUOUS | Negative |
| SS-PREP-01 | schreptsummary_2025.sas | session | `pagesize`/`linesize`/`nodate`/`nonumber`; empty footnote | none | Listing options only | CONFIRMED | No |
| SS-PREP-02 | schreptsummary_2025.sas | `ANALVAR='A'`, `Code`, `Count` | `SYMPUT Ct_<code>` | `PUT(Count,4.)` | Macro total-reported per school | CONFIRMED | Yes |
| SS-PREP-03 | schreptsummary_2025.sas | `CODE`, `NAME`, `JOBST`, `ST` | `%SCHRPTS` runs 7 reports | none | One school’s PDF pages; `JOBST`/`ST` unused | CONFIRMED; AMBIGUOUS unused args | Yes |
| SS-FMT-01 | schreptsummary_2025.sas | raw jobcat codes | `$jobcat` defined | none | Unused in `PROC REPORT` | CONFIRMED defined; AMBIGUOUS unused | No |
| SS-FMT-02 | schreptsummary_2025.sas | `analvar` | `$recode` section titles | none | Printed section headers | CONFIRMED | Yes |
| SS-FMT-03 | schreptsummary_2025.sas | `analvar` | `$subtotal` | none | `Subtotal` / `Total #` / `Total Reported` | CONFIRMED | Yes |
| SS-FMT-04 | schreptsummary_2025.sas | `newvar` | `$newvar` row labels (Women, White, 1-10, …) | none | Printed row text; some keys may not match builder (`MINOR F` vs `MINORF`, `ZAFTGR` vs `ZAFTGRD`) | CONFIRMED maps; AMBIGUOUS mismatches | Yes |
| SS-FIL-01 | schreptsummary_2025.sas | `CODE`, `ANALVAR` | page 1 `in ('B','C','C1','D')` | none | Page 1 rows | CONFIRMED | Yes |
| SS-FIL-02 | schreptsummary_2025.sas | `CODE`, `ANALVAR` | `GE 'D1' AND LT 'E2'` (character) | none | Page 2 typically `D1`–`D3`, `E`, `E1` | CONFIRMED; AMBIGUOUS char order | Yes |
| SS-FIL-03 | schreptsummary_2025.sas | `CODE`, `ANALVAR` | `in ('E2','E3','E4','E5','E55')` | none | Page 3 rows | CONFIRMED | Yes |
| SS-FIL-04 | schreptsummary_2025.sas | `CODE`, `ANALVAR` | `IN ('E6','FIRM','FIRM2')` | none | Page 4 rows | CONFIRMED | Yes |
| SS-FIL-05 | schreptsummary_2025.sas | `CODE`, `ANALVAR` | `IN ('JOBREG1','JOBREG2','JOBREG3')` | none | Page 5 rows | CONFIRMED | Yes |
| SS-FIL-06 | schreptsummary_2025.sas | part2 `ANALVAR` | `in ('SOURCE','TIME','ZSTATUS')` | none | Page 6 rows | CONFIRMED | Yes |
| SS-FIL-07 | schreptsummary_2025.sas | part2 `ANALVAR` | `in ('DURATION','LAW SCHOOL FUNDED')` | none | Page 7 rows | CONFIRMED | Yes |
| SS-FIL-08 | schreptsummary_2025.sas | input rows | no extra drop of zeros | none | Missing categories stay missing | CONFIRMED effect; INFERRED from note | Yes |
| SS-GRP-01 | schreptsummary_2025.sas | `Analvar` | `GROUP NOPRINT` | none | Hidden group; header from `$recode` | CONFIRMED | Yes |
| SS-GRP-02 | schreptsummary_2025.sas | `Newvar` | `ORDER=INTERNAL GROUP` + `$newvar` | none | Internal-code order, formatted label | CONFIRMED | Yes |
| SS-ORD-01 | schreptsummary_2025.sas | pages 1–7 | source order of `PROC REPORT` | none | Page 1 then 2…7 | CONFIRMED | Yes |
| SS-ORD-02 | schreptsummary_2025.sas | school list | `%SCHRPTS` call order | none | PDFs in listed school order | CONFIRMED | No |
| SS-CALC-01 | schreptsummary_2025.sas | `Count` | `DEFINE Count / SUM` | `Count.sum` | Number Reported | CONFIRMED | Yes |
| SS-CALC-02 | schreptsummary_2025.sas | `Percent` | `DEFINE Percent / SUM` | `Percent.sum` | % of Reported (sum of stored percents) | CONFIRMED; AMBIGUOUS meaning | Yes |
| SS-CALC-03 | schreptsummary_2025.sas | `perm`, `temp` | `SUM` on page 7 | `perm.sum`, `temp.sum` | Long-term / short-term columns | CONFIRMED; AMBIGUOUS column IDs | Yes |
| SS-CALC-04 | schreptsummary_2025.sas | `count`, `fixed` | commented `DEFINE`s | none | **Not applied** | AMBIGUOUS inactive | Negative |
| SS-RPT-01 | schreptsummary_2025.sas | `schreptsummary2025` | page 1 `PROC REPORT` + salary columns + total banner | display/sum | Page 1 table | CONFIRMED | Yes |
| SS-RPT-02 | schreptsummary_2025.sas | `schreptsummary2025` | page 2 report + salary columns | display/sum | Page 2 table | CONFIRMED | Yes |
| SS-RPT-03 | schreptsummary_2025.sas | `schreptsummary2025` | page 3 report + salary columns | display/sum | Page 3 table | CONFIRMED | Yes |
| SS-RPT-04 | schreptsummary_2025.sas | `schreptsummary2025` | page 4 report + salary columns | display/sum | Page 4 table | CONFIRMED | Yes |
| SS-RPT-05 | schreptsummary_2025.sas | `schreptsummary2025` | page 5 report + salary columns | display/sum | Page 5 table | CONFIRMED | Yes |
| SS-RPT-06 | schreptsummary_2025.sas | `schreptsummary2025_part2` | page 6; no salary columns | display/sum | Page 6 table | CONFIRMED | Yes |
| SS-RPT-07 | schreptsummary_2025.sas | `schreptsummary2025_part2` | page 7 `perm`/`temp` | sum | Page 7 table | CONFIRMED | Yes |
| SS-CNT-01 | schreptsummary_2025.sas | `Count` | SUM/display | `Count` / `Count.sum` | Number Reported | CONFIRMED | Yes |
| SS-CNT-02 | schreptsummary_2025.sas | `Ct_<code>` | print on page 1 | none | `Total Reported = …` | CONFIRMED | Yes |
| SS-CNT-03 | schreptsummary_2025.sas | `N` | DISPLAY (not SUM) | none | # with Salary | CONFIRMED | Yes |
| SS-CNT-04 | schreptsummary_2025.sas | `perm`, `temp` | SUM | sums | Long-term / short-term job counts | CONFIRMED | Yes |
| SS-CNT-05 | schreptsummary_2025.sas | funded `perm` | same page-7 columns | display/sum | Funded total (note: regardless of duration) | CONFIRMED structure; INFERRED from note | Yes |
| SS-PCT-01 | schreptsummary_2025.sas | `Percent` | format 6.1 | none | Detail % of Reported | CONFIRMED | Yes |
| SS-PCT-02 | schreptsummary_2025.sas | `Percent` | `Percent.sum` after group | sum | Section percent subtotal | CONFIRMED | Yes |
| SS-PCT-03 | schreptsummary_2025.sas | page 7 | no percent column | none | No % on duration/funded page | CONFIRMED | Yes |
| SS-PCT-04 | schreptsummary_2025.sas / page 6 note | reported items | note only; no extra `WHERE` | none | Percents may not add to all jobs | INFERRED from note | Yes |
| SS-SUB-01 | schreptsummary_2025.sas | group totals | `COMPUTE AFTER ANALVAR` | `Count.sum` / `Percent.sum` or `perm`/`temp` | Subtotal line; salaries not subtotaled | CONFIRMED | Yes |
| SS-SUB-02 | schreptsummary_2025.sas | `JOBREG3` | `$subtotal` = `Total #` | none | Different subtotal label | CONFIRMED | Yes |
| SS-SAL-01 | schreptsummary_2025.sas | `N` | DISPLAY | none (upstream calc) | # with Salary | CONFIRMED display | Yes |
| SS-SAL-02 | schreptsummary_2025.sas | `Pct25` | DISPLAY COMMA8.0 | none (upstream Q1) | 25th percentile | CONFIRMED display | Yes |
| SS-SAL-03 | schreptsummary_2025.sas | `Median` | DISPLAY COMMA8.0 | none (upstream median) | Median | CONFIRMED display | Yes |
| SS-SAL-04 | schreptsummary_2025.sas | `Pct75` | DISPLAY COMMA8.0 | none (upstream Q3) | 75th percentile | CONFIRMED display | Yes |
| SS-SAL-05 | schreptsummary_2025.sas | `Mean` | DISPLAY COMMA8.0 | none (upstream mean) | Mean | CONFIRMED display | Yes |
| SS-SAL-06 | schreptsummary_2025.sas | pages 1–5 | spanning header | none | `Full-time Long-term Salaries` | CONFIRMED | Yes |
| SS-SAL-07 | schreptsummary_2025.sas notes | printed note | FT long-term / solo language | none in this file | Report language only; do not add filters here | INFERRED from note | Yes |
| SS-SUP-01 | schreptsummary_2025.sas note + CF-S-00 | `n` | note “at least five salaries”; no `IF N<5` here | suppression is upstream | Salary cells blank when builder omitted row | CONFIRMED note; INFERRED enforcement here | Yes |
| SS-SUP-02 | schreptsummary_2025.sas | missing categories | no row printed | none | Hidden unused categories | CONFIRMED effect | Yes |
| SS-SUP-03 | schreptsummary_2025.sas note | solo salaries | note says excluded; no `WHERE` here | none in this file | Do not invent a new solo filter | AMBIGUOUS | Yes |
| SS-SUP-04 | schreptsummary_2025.sas page 6 note | own practice / timing | note says exclude; no `SELFPR` `WHERE` | none in this file | Do not invent a timing exclusion | INFERRED from note | Yes |
| SS-HDR-01 | schreptsummary_2025.sas | `NAME` | TITLE1 | none | School name | CONFIRMED | Yes |
| SS-HDR-02 | schreptsummary_2025.sas | page 1 | TITLE2 | none | `Class of 2025 Summary Report` | CONFIRMED | Yes |
| SS-HDR-03 | schreptsummary_2025.sas | pages 2–7 | TITLE2 | none | `… - Page N` | CONFIRMED | Yes |
| SS-HDR-04 | schreptsummary_2025.sas | ODS | `proclabel='Page N'` | none | PDF bookmarks | CONFIRMED | Yes |
| SS-HDR-05 | schreptsummary_2025.sas | columns | `SPLIT='*'` headers | none | Number Reported / % / salary headers | CONFIRMED | Yes |
| SS-HDR-06 | schreptsummary_2025.sas | `analvar` | `$recode` before group | none | Section title line | CONFIRMED | Yes |
| SS-HDR-07 | schreptsummary_2025.sas | `Ct_<code>` | page 1 `COMPUTE BEFORE` | none | Total Reported banner | CONFIRMED | Yes |
| SS-FTR-01 | schreptsummary_2025.sas | posttext | July 2026 + ABA/NALP disclaimer | none | Footer after each table | CONFIRMED | Yes |
| SS-FTR-02 | schreptsummary_2025.sas | footnotes | `FOOTNOTE1/2` blank | none | Empty footnotes | CONFIRMED | No |
| SS-FTR-03 | schreptsummary_2025.sas | reprint date | commented `today()` footnote | none | **Not applied** | AMBIGUOUS inactive | Negative |
| SS-FTR-04 | schreptsummary_2025.sas | startup | `footnote ' '` | none | Clears leftover footnote | CONFIRMED | No |
| SS-PAGE-01 | schreptsummary_2025.sas | school | `ods pdf` / `%SCHRPTS` / `close` | none | One PDF per school | CONFIRMED | Yes |
| SS-PAGE-02 | schreptsummary_2025.sas | 7 procedures | default ODS startpage | none | New page per `PROC REPORT` | CONFIRMED | Yes |
| SS-PAGE-03 | schreptsummary_2025.sas | ODS | `pdftoc=1` | none | Bookmark depth 1 | CONFIRMED | No |
| SS-PAGE-04 | schreptsummary_2025.sas | ODS | `STYLE=GrayscalePrinter` | none | Grayscale style | CONFIRMED | No |
| SS-PAGE-05 | schreptsummary_2025.sas | first `ods pdf` only | `accessible` on Quinnipiac only | none | Other schools omit `accessible` | AMBIGUOUS | Yes |
| SS-PAGE-06 | schreptsummary_2025.sas | 90520 | `%SCHRPTS` without matching `ods pdf file` pattern | none | Destination unclear | AMBIGUOUS | Yes |
| SS-PAGE-07 | schreptsummary_2025.sas | line 1846+ | stray `*/` then extra American U PDF | none | Extra/reprint destination | AMBIGUOUS | No |
| SS-NOTE-01 | schreptsummary_2025.sas page 1 | `COMPUTE AFTER` | print note | none | No-grad / 5-salary / non-binary / FT-LT / solo note | CONFIRMED text | Yes |
| SS-NOTE-02 | schreptsummary_2025.sas page 2 | `COMPUTE AFTER` | print note | none | Page 1 themes plus public/private / missing employer-type | CONFIRMED text | Yes |
| SS-NOTE-03 | schreptsummary_2025.sas pages 3–5 | `COMPUTE AFTER` | print note | none | No-grad / 5-salary / FT-LT / solo | CONFIRMED text | Yes |
| SS-NOTE-06 | schreptsummary_2025.sas page 6 | `COMPUTE AFTER` | print note | none | Item-reported / exclude own-practice timing | CONFIRMED text | Yes |
| SS-NOTE-07 | schreptsummary_2025.sas page 7 | `COMPUTE AFTER` | print note | none | Duration item-reported; funded is total regardless of duration | CONFIRMED text | Yes |
| SS-OUT-01 | schreptsummary_2025.sas | `CODE`, slug | `{CODE}_{slug}_summary2025.pdf` | none | Per-school filename (`31405` uses `summary2024` + space) | CONFIRMED | Yes |
| SS-OUT-02 | schreptsummary_2025.sas | commented schools | `*` / comments skip some `%SCHRPTS` | none | No PDF for skipped codes (e.g. 23101, 23909, 31504) | CONFIRMED inactive | Negative |
| SS-OUT-03 | schreptsummary_2025.sas | `accessible` option | one file only | none | Not a validated PDF-UA claim | AMBIGUOUS | Yes |

---

## Notes for testers

- **Salary suppression to implement/test is CF-S-00** (`n ge 5`). SS-SUP-01 is the same requirement stated on the PDF, not a second threshold.
- **Do not “clean up” CF-C-06, CF-S-05, CF-S-19, or `$jobcat1`** to match comments or titles.
- **SS-FMT-04 mismatches** (`MINOR F` vs `MINORF`, `ZAFTGR` vs `ZAFTGRD`) stay documented until a baseline PDF row proves which label prints.
- Baseline PDF notes (SS-NOTE-*, SS-SAL-07, SS-SUP-03/04) are report text. They are not extra calculation steps unless the builder SAS already does that work.

No application code was added. No characterized rule was dropped. No unsupported rule was introduced.
