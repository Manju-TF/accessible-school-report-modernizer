# Legacy vs modern parity results

Sources (read-only):

- `legacy/baseline/test-school-report.pdf` — expected business values from `docs/capstone/report-map.md`
- `legacy/samples/sample-export.xlsx` — modern calculator input

This is value-level parity. No pixel comparison was done.

**Result: FAIL.** 320 numerical mismatches. `LegacyModernParityTests` asserts zero mismatches.

## Subject

- Legacy school name: `Test University School of Law`
- Legacy class year on PDF: `Class of 2024`
- Legacy school CODE: **unknown** (not on the PDF; not in the 2025 `%SCHRPTS` list)
- Modern school CODE used for comparison: `23306`
- Modern graduate count (CF-C-01): 31
- Sample schools imported: 189
- Largest sample school graduate count: 73
- Selection: No PDF-identity match. Used sample school 23306 with the most matching characterized counts (21).

Compared 560 characterized metrics. Match 239. Mismatch 320. Unresolved 1.

## Matched rules

- **CF-C-07** — 1 metric(s). Legacy 1 vs modern 1
- **CF-C-11** — 1 metric(s). Legacy 1 vs modern 1
- **CF-C-16** — 3 metric(s). Legacy 3 vs modern 3
- **CF-P2-01** — 4 metric(s). Legacy 1 vs modern 1; Compared to DurationCounts[TEMP]. Missing PDF '.' is expected null.
- **CF-P2-02** — 1 metric(s). Legacy . vs modern .; Absent on the baseline PDF.
- **CF-P2-03** — 2 metric(s). Legacy 4 vs modern 4
- **CF-S-00** — 40 metric(s). Legacy . vs modern .
- **SS-CALC-01** — 1 metric(s). Legacy 100.0 vs modern 100.0; Displayed details sum to 94; printed subtotal is 93.
- **SS-FIL-08** — 9 metric(s). Legacy . vs modern .; Absent on the baseline PDF.
- **SS-SAL-02** — 40 metric(s). Legacy . vs modern .
- **SS-SAL-03** — 41 metric(s). Legacy . vs modern .
- **SS-SAL-04** — 40 metric(s). Legacy . vs modern .
- **SS-SAL-05** — 40 metric(s). Legacy . vs modern .
- **SS-SUB-01** — 16 metric(s). Legacy 100.0 vs modern 100.0

## Mismatched rules

- **CF-C-01** — 1 metric(s). Legacy 100 vs modern 31
- **CF-C-02** — 4 metric(s). Legacy 46 vs modern 9
- **CF-C-03** — 4 metric(s). Legacy 24 vs modern 10
- **CF-C-04** — 8 metric(s). Legacy 10 vs modern 4
- **CF-C-05** — 12 metric(s). Legacy 78 vs modern 27; PDF text layer 25th is 765000 (comma missing).
- **CF-C-06** — 2 metric(s). Legacy 93 vs modern 29
- **CF-C-07** — 11 metric(s). Legacy 79 vs modern 26; Page-2 LJD FT 79 vs page-1 LJD 78.
- **CF-C-08** — 4 metric(s). Legacy 78 vs modern 22
- **CF-C-09** — 10 metric(s). Legacy 14 vs modern 2
- **CF-C-11** — 7 metric(s). Legacy 2 vs modern .
- **CF-C-12** — 4 metric(s). Legacy 46 vs modern 20
- **CF-C-13** — 4 metric(s). Legacy 13 vs modern 1
- **CF-C-14** — 4 metric(s). Legacy 4 vs modern .; PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column.
- **CF-C-15** — 6 metric(s). Legacy 6 vs modern 2
- **CF-C-16** — 11 metric(s). Legacy 30 vs modern 3
- **CF-C-17** — 4 metric(s). Legacy 42 vs modern 19
- **CF-C-18** — 10 metric(s). Legacy 69 vs modern .
- **CF-C-19** — 4 metric(s). Legacy 63 vs modern 25
- **CF-C-20** — 2 metric(s). Legacy 14 vs modern 4
- **CF-P2-01** — 8 metric(s). Legacy 85 vs modern 28; Compared to DurationCounts[PERM]. Long/short labels are not a characterized codebook.
- **CF-P2-03** — 16 metric(s). Legacy 4.4 vs modern 16
- **CF-P2-04** — 4 metric(s). Legacy 49 vs modern 18
- **CF-P2-05** — 4 metric(s). Legacy 4 vs modern 2
- **CF-S-00** — 30 metric(s). Legacy 40 vs modern 5
- **SS-CALC-01** — 1 metric(s). Legacy 93 vs modern 29; Displayed details sum to 94; printed subtotal is 93.
- **SS-CNT-02** — 1 metric(s). Legacy 100 vs modern 31
- **SS-FIL-08** — 1 metric(s). Legacy . vs modern 6; Absent on the baseline PDF.
- **SS-SAL-02** — 30 metric(s). Legacy 70000 vs modern 80500
- **SS-SAL-03** — 29 metric(s). Legacy 85500 vs modern 95500
- **SS-SAL-04** — 30 metric(s). Legacy 110000 vs modern 130500
- **SS-SAL-05** — 30 metric(s). Legacy 85802 vs modern 103500
- **SS-SUB-01** — 22 metric(s). Legacy 100 vs modern 31
- **SS-SUB-02** — 2 metric(s). Legacy 14 vs modern 4

## Unresolved rules

- **SS-PREP-03** — 1 metric(s). Baseline PDF school CODE is unknown and is not in the 2025 %SCHRPTS list. Sample identity cannot be proven.

## Metric table

| Status | Id | Rule | Analvar | Newvar | Field | Legacy | Modern | Explanation |
|---|---|---|---|---|---|---|---|---|
| Unresolved | `identity.school-code` | SS-PREP-03 | SCHOOL |  | Count | . | . | Baseline PDF school CODE is unknown and is not in the 2025 %SCHRPTS list. Sample identity cannot be proven. |
| Mismatch | `total.count` | CF-C-01 | A | A | Count | 100 | 31 | Legacy 100 vs modern 31 |
| Mismatch | `total.subtotal-count` | SS-CNT-02 | A | A | SubtotalCount | 100 | 31 | Legacy 100 vs modern 31 |
| Mismatch | `gender-women.count` | CF-C-02 | B | F | Count | 46 | 9 | Legacy 46 vs modern 9 |
| Mismatch | `gender-women.percent` | CF-C-02 | B | F | Percent | 46.0 | 29.0 | Legacy 46.0 vs modern 29.0 |
| Mismatch | `gender-women.n` | CF-S-00 | B | F | SalaryN | 40 | 5 | Legacy 40 vs modern 5 |
| Mismatch | `gender-women.p25` | SS-SAL-02 | B | F | Pct25 | 70000 | 80500 | Legacy 70000 vs modern 80500 |
| Mismatch | `gender-women.median` | SS-SAL-03 | B | F | Median | 85500 | 95500 | Legacy 85500 vs modern 95500 |
| Mismatch | `gender-women.p75` | SS-SAL-04 | B | F | Pct75 | 110000 | 130500 | Legacy 110000 vs modern 130500 |
| Mismatch | `gender-women.mean` | SS-SAL-05 | B | F | Mean | 85802 | 103500 | Legacy 85802 vs modern 103500 |
| Mismatch | `gender-men.count` | CF-C-02 | B | M | Count | 54 | 16 | Legacy 54 vs modern 16; PDF text layer 25th is 850000 (comma missing). |
| Mismatch | `gender-men.percent` | CF-C-02 | B | M | Percent | 54.0 | 51.6 | Legacy 54.0 vs modern 51.6; PDF text layer 25th is 850000 (comma missing). |
| Mismatch | `gender-men.n` | CF-S-00 | B | M | SalaryN | 30 | 13 | Legacy 30 vs modern 13; PDF text layer 25th is 850000 (comma missing). |
| Mismatch | `gender-men.p25` | SS-SAL-02 | B | M | Pct25 | 850000 | 80500 | Legacy 850000 vs modern 80500; PDF text layer 25th is 850000 (comma missing). |
| Mismatch | `gender-men.median` | SS-SAL-03 | B | M | Median | 90500 | 90000 | Legacy 90500 vs modern 90000; PDF text layer 25th is 850000 (comma missing). |
| Mismatch | `gender-men.p75` | SS-SAL-04 | B | M | Pct75 | 120000 | 100500 | Legacy 120000 vs modern 100500; PDF text layer 25th is 850000 (comma missing). |
| Mismatch | `gender-men.mean` | SS-SAL-05 | B | M | Mean | 103401 | 99923 | Legacy 103401 vs modern 99923; PDF text layer 25th is 850000 (comma missing). |
| Mismatch | `gender.subtotal-count` | SS-SUB-01 | B |  | SubtotalCount | 100 | 31 | Legacy 100 vs modern 31 |
| Match | `gender.subtotal-percent` | SS-SUB-01 | B |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Mismatch | `gender-nonbinary.absent` | SS-FIL-08 | B | N | Count | . | 6 | Legacy . vs modern 6; Absent on the baseline PDF. |
| Mismatch | `race-poc.count` | CF-C-03 | C | MINOR | Count | 24 | 10 | Legacy 24 vs modern 10 |
| Mismatch | `race-poc.percent` | CF-C-03 | C | MINOR | Percent | 34.3 | 33.3 | Legacy 34.3 vs modern 33.3 |
| Mismatch | `race-poc.n` | CF-S-00 | C | MINOR | SalaryN | 8 | 5 | Legacy 8 vs modern 5 |
| Mismatch | `race-poc.p25` | SS-SAL-02 | C | MINOR | Pct25 | 60000 | 90000 | Legacy 60000 vs modern 90000 |
| Mismatch | `race-poc.median` | SS-SAL-03 | C | MINOR | Median | 85000 | 90500 | Legacy 85000 vs modern 90500 |
| Mismatch | `race-poc.p75` | SS-SAL-04 | C | MINOR | Pct75 | 89000 | 130500 | Legacy 89000 vs modern 130500 |
| Mismatch | `race-poc.mean` | SS-SAL-05 | C | MINOR | Mean | 82604 | 103400 | Legacy 82604 vs modern 103400 |
| Mismatch | `race-white.count` | CF-C-03 | C | NONMIN | Count | 46 | 20 | Legacy 46 vs modern 20 |
| Mismatch | `race-white.percent` | CF-C-03 | C | NONMIN | Percent | 65.7 | 66.7 | Legacy 65.7 vs modern 66.7 |
| Mismatch | `race-white.n` | CF-S-00 | C | NONMIN | SalaryN | 34 | 18 | Legacy 34 vs modern 18 |
| Mismatch | `race-white.p25` | SS-SAL-02 | C | NONMIN | Pct25 | 80000 | 80500 | Legacy 80000 vs modern 80500 |
| Mismatch | `race-white.median` | SS-SAL-03 | C | NONMIN | Median | 86000 | 90280 | Legacy 86000 vs modern 90280 |
| Mismatch | `race-white.p75` | SS-SAL-04 | C | NONMIN | Pct75 | 95000 | 100500 | Legacy 95000 vs modern 100500 |
| Mismatch | `race-white.mean` | SS-SAL-05 | C | NONMIN | Mean | 90559 | 98142 | Legacy 90559 vs modern 98142 |
| Mismatch | `race.subtotal-count` | SS-SUB-01 | C |  | SubtotalCount | 70 | 30 | Legacy 70 vs modern 30 |
| Match | `race.subtotal-percent` | SS-SUB-01 | C |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Mismatch | `cross-woc.count` | CF-C-04 | C1 | MINORF | Count | 10 | 4 | Legacy 10 vs modern 4 |
| Mismatch | `cross-woc.percent` | CF-C-04 | C1 | MINORF | Percent | 15.6 | 13.3 | Legacy 15.6 vs modern 13.3 |
| Mismatch | `cross-woc.n` | CF-S-00 | C1 | MINORF | SalaryN | 6 | . | Legacy 6 vs modern . |
| Mismatch | `cross-woc.p25` | SS-SAL-02 | C1 | MINORF | Pct25 | 65000 | . | Legacy 65000 vs modern . |
| Mismatch | `cross-woc.median` | SS-SAL-03 | C1 | MINORF | Median | 88500 | . | Legacy 88500 vs modern . |
| Mismatch | `cross-woc.p75` | SS-SAL-04 | C1 | MINORF | Pct75 | 90000 | . | Legacy 90000 vs modern . |
| Mismatch | `cross-woc.mean` | SS-SAL-05 | C1 | MINORF | Mean | 84673 | . | Legacy 84673 vs modern . |
| Mismatch | `cross-moc.count` | CF-C-04 | C1 | MINORM | Count | 4 | 3 | Legacy 4 vs modern 3 |
| Mismatch | `cross-moc.percent` | CF-C-04 | C1 | MINORM | Percent | 6.3 | 10 | Legacy 6.3 vs modern 10 |
| Match | `cross-moc.n` | CF-S-00 | C1 | MINORM | SalaryN | . | . | Legacy . vs modern . |
| Match | `cross-moc.p25` | SS-SAL-02 | C1 | MINORM | Pct25 | . | . | Legacy . vs modern . |
| Match | `cross-moc.median` | SS-SAL-03 | C1 | MINORM | Median | . | . | Legacy . vs modern . |
| Match | `cross-moc.p75` | SS-SAL-04 | C1 | MINORM | Pct75 | . | . | Legacy . vs modern . |
| Match | `cross-moc.mean` | SS-SAL-05 | C1 | MINORM | Mean | . | . | Legacy . vs modern . |
| Mismatch | `cross-ww.count` | CF-C-04 | C1 | NONMINF | Count | 30 | 4 | Legacy 30 vs modern 4 |
| Mismatch | `cross-ww.percent` | CF-C-04 | C1 | NONMINF | Percent | 46.9 | 13.3 | Legacy 46.9 vs modern 13.3 |
| Mismatch | `cross-ww.n` | CF-S-00 | C1 | NONMINF | SalaryN | 25 | . | Legacy 25 vs modern . |
| Mismatch | `cross-ww.p25` | SS-SAL-02 | C1 | NONMINF | Pct25 | 75000 | . | Legacy 75000 vs modern . |
| Mismatch | `cross-ww.median` | SS-SAL-03 | C1 | NONMINF | Median | 80000 | . | Legacy 80000 vs modern . |
| Mismatch | `cross-ww.p75` | SS-SAL-04 | C1 | NONMINF | Pct75 | 101000 | . | Legacy 101000 vs modern . |
| Mismatch | `cross-ww.mean` | SS-SAL-05 | C1 | NONMINF | Mean | 95983 | . | Legacy 95983 vs modern . |
| Mismatch | `cross-wm.count` | CF-C-04 | C1 | NONMINM | Count | 20 | 13 | Legacy 20 vs modern 13 |
| Mismatch | `cross-wm.percent` | CF-C-04 | C1 | NONMINM | Percent | 31.3 | 43.3 | Legacy 31.3 vs modern 43.3 |
| Mismatch | `cross-wm.n` | CF-S-00 | C1 | NONMINM | SalaryN | 15 | 12 | Legacy 15 vs modern 12 |
| Mismatch | `cross-wm.p25` | SS-SAL-02 | C1 | NONMINM | Pct25 | 85000 | 80500 | Legacy 85000 vs modern 80500 |
| Match | `cross-wm.median` | SS-SAL-03 | C1 | NONMINM | Median | 88000 | 88000 | Legacy 88000 vs modern 88000 |
| Mismatch | `cross-wm.p75` | SS-SAL-04 | C1 | NONMINM | Pct75 | 105000 | 108000 | Legacy 105000 vs modern 108000 |
| Mismatch | `cross-wm.mean` | SS-SAL-05 | C1 | NONMINM | Mean | 107471 | 100750 | Legacy 107471 vs modern 100750 |
| Mismatch | `cross.subtotal-count` | SS-SUB-01 | C1 |  | SubtotalCount | 64 | 30 | Legacy 64 vs modern 30 |
| Match | `cross.subtotal-percent` | SS-SUB-01 | C1 |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Mismatch | `emp-ljd.count` | CF-C-05 | D | 1-LJD | Count | 78 | 27 | Legacy 78 vs modern 27; PDF text layer 25th is 765000 (comma missing). |
| Mismatch | `emp-ljd.percent` | CF-C-05 | D | 1-LJD | Percent | 78.0 | 87.1 | Legacy 78.0 vs modern 87.1; PDF text layer 25th is 765000 (comma missing). |
| Mismatch | `emp-ljd.n` | CF-S-00 | D | 1-LJD | SalaryN | 69 | 22 | Legacy 69 vs modern 22; PDF text layer 25th is 765000 (comma missing). |
| Mismatch | `emp-ljd.p25` | SS-SAL-02 | D | 1-LJD | Pct25 | 765000 | 80500 | Legacy 765000 vs modern 80500; PDF text layer 25th is 765000 (comma missing). |
| Mismatch | `emp-ljd.median` | SS-SAL-03 | D | 1-LJD | Median | 88000 | 90280 | Legacy 88000 vs modern 90280; PDF text layer 25th is 765000 (comma missing). |
| Mismatch | `emp-ljd.p75` | SS-SAL-04 | D | 1-LJD | Pct75 | 110000 | 100500 | Legacy 110000 vs modern 100500; PDF text layer 25th is 765000 (comma missing). |
| Mismatch | `emp-ljd.mean` | SS-SAL-05 | D | 1-LJD | Mean | 90397 | 97866 | Legacy 90397 vs modern 97866; PDF text layer 25th is 765000 (comma missing). |
| Mismatch | `emp-nljd.count` | CF-C-05 | D | 2-NLJD | Count | 12 | 1 | Legacy 12 vs modern 1 |
| Mismatch | `emp-nljd.percent` | CF-C-05 | D | 2-NLJD | Percent | 12.0 | 3.2 | Legacy 12.0 vs modern 3.2 |
| Mismatch | `emp-nljd.n` | CF-S-00 | D | 2-NLJD | SalaryN | 10 | . | Legacy 10 vs modern . |
| Mismatch | `emp-nljd.p25` | SS-SAL-02 | D | 2-NLJD | Pct25 | 72250 | . | Legacy 72250 vs modern . |
| Mismatch | `emp-nljd.median` | SS-SAL-03 | D | 2-NLJD | Median | 93500 | . | Legacy 93500 vs modern . |
| Mismatch | `emp-nljd.p75` | SS-SAL-04 | D | 2-NLJD | Pct75 | 104200 | . | Legacy 104200 vs modern . |
| Mismatch | `emp-nljd.mean` | SS-SAL-05 | D | 2-NLJD | Mean | 100500 | . | Legacy 100500 vs modern . |
| Mismatch | `emp-nlp.count` | CF-C-05 | D | 3-NLP | Count | 1 | . | Legacy 1 vs modern . |
| Mismatch | `emp-nlp.percent` | CF-C-05 | D | 3-NLP | Percent | 1.0 | . | Legacy 1.0 vs modern . |
| Match | `emp-nlp.n` | CF-S-00 | D | 3-NLP | SalaryN | . | . | Legacy . vs modern . |
| Match | `emp-nlp.p25` | SS-SAL-02 | D | 3-NLP | Pct25 | . | . | Legacy . vs modern . |
| Match | `emp-nlp.median` | SS-SAL-03 | D | 3-NLP | Median | . | . | Legacy . vs modern . |
| Match | `emp-nlp.p75` | SS-SAL-04 | D | 3-NLP | Pct75 | . | . | Legacy . vs modern . |
| Match | `emp-nlp.mean` | SS-SAL-05 | D | 3-NLP | Mean | . | . | Legacy . vs modern . |
| Mismatch | `emp-nlo.count` | CF-C-05 | D | 4-NLO | Count | 2 | 1 | Legacy 2 vs modern 1 |
| Mismatch | `emp-nlo.percent` | CF-C-05 | D | 4-NLO | Percent | 2.0 | 3.2 | Legacy 2.0 vs modern 3.2 |
| Match | `emp-nlo.n` | CF-S-00 | D | 4-NLO | SalaryN | . | . | Legacy . vs modern . |
| Match | `emp-nlo.p25` | SS-SAL-02 | D | 4-NLO | Pct25 | . | . | Legacy . vs modern . |
| Match | `emp-nlo.median` | SS-SAL-03 | D | 4-NLO | Median | . | . | Legacy . vs modern . |
| Match | `emp-nlo.p75` | SS-SAL-04 | D | 4-NLO | Pct75 | . | . | Legacy . vs modern . |
| Match | `emp-nlo.mean` | SS-SAL-05 | D | 4-NLO | Mean | . | . | Legacy . vs modern . |
| Mismatch | `emp-uskw.count` | CF-C-05 | D | 8-USKW | Count | 3 | 1 | Legacy 3 vs modern 1 |
| Mismatch | `emp-uskw.percent` | CF-C-05 | D | 8-USKW | Percent | 3.0 | 3.2 | Legacy 3.0 vs modern 3.2 |
| Match | `emp-uskw.n` | CF-S-00 | D | 8-USKW | SalaryN | . | . | Legacy . vs modern . |
| Match | `emp-uskw.p25` | SS-SAL-02 | D | 8-USKW | Pct25 | . | . | Legacy . vs modern . |
| Match | `emp-uskw.median` | SS-SAL-03 | D | 8-USKW | Median | . | . | Legacy . vs modern . |
| Match | `emp-uskw.p75` | SS-SAL-04 | D | 8-USKW | Pct75 | . | . | Legacy . vs modern . |
| Match | `emp-uskw.mean` | SS-SAL-05 | D | 8-USKW | Mean | . | . | Legacy . vs modern . |
| Mismatch | `emp-unwk.count` | CF-C-05 | D | 9-UNWK | Count | 4 | . | Legacy 4 vs modern . |
| Mismatch | `emp-unwk.percent` | CF-C-05 | D | 9-UNWK | Percent | 4.0 | . | Legacy 4.0 vs modern . |
| Match | `emp-unwk.n` | CF-S-00 | D | 9-UNWK | SalaryN | . | . | Legacy . vs modern . |
| Match | `emp-unwk.p25` | SS-SAL-02 | D | 9-UNWK | Pct25 | . | . | Legacy . vs modern . |
| Match | `emp-unwk.median` | SS-SAL-03 | D | 9-UNWK | Median | . | . | Legacy . vs modern . |
| Match | `emp-unwk.p75` | SS-SAL-04 | D | 9-UNWK | Pct75 | . | . | Legacy . vs modern . |
| Match | `emp-unwk.mean` | SS-SAL-05 | D | 9-UNWK | Mean | . | . | Legacy . vs modern . |
| Mismatch | `emp.subtotal-count` | SS-SUB-01 | D |  | SubtotalCount | 100 | 31 | Legacy 100 vs modern 31 |
| Match | `emp.subtotal-percent` | SS-SUB-01 | D |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Match | `emp-advd.absent` | SS-FIL-08 | D | 6-ADVD | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Mismatch | `d1-empl.count` | CF-C-06 | D1 | EMPL | Count | 93 | 29 | Legacy 93 vs modern 29 |
| Mismatch | `d1-empl.percent` | CF-C-06 | D1 | EMPL | Percent | 93.0 | 93.5 | Legacy 93.0 vs modern 93.5 |
| Mismatch | `d1-empl.n` | CF-S-00 | D1 | EMPL | SalaryN | 83 | 23 | Legacy 83 vs modern 23 |
| Mismatch | `d1-empl.p25` | SS-SAL-02 | D1 | EMPL | Pct25 | 70000 | 80500 | Legacy 70000 vs modern 80500 |
| Mismatch | `d1-empl.median` | SS-SAL-03 | D1 | EMPL | Median | 89000 | 90500 | Legacy 89000 vs modern 90500 |
| Mismatch | `d1-empl.p75` | SS-SAL-04 | D1 | EMPL | Pct75 | 105000 | 115500 | Legacy 105000 vs modern 115500 |
| Mismatch | `d1-empl.mean` | SS-SAL-05 | D1 | EMPL | Mean | 95236 | 99285 | Legacy 95236 vs modern 99285 |
| Mismatch | `d1.subtotal-count` | SS-SUB-01 | D1 |  | SubtotalCount | 93 | 29 | Legacy 93 vs modern 29 |
| Mismatch | `d1.subtotal-percent` | SS-SUB-01 | D1 |  | SubtotalPercent | 93.0 | 93.5 | Legacy 93.0 vs modern 93.5 |
| Match | `d1-advd.absent` | SS-FIL-08 | D1 | 6-ADVD | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Mismatch | `d2-private.count` | CF-C-08 | D2 | PRIVATE | Count | 78 | 22 | Legacy 78 vs modern 22 |
| Mismatch | `d2-private.percent` | CF-C-08 | D2 | PRIVATE | Percent | 83.9 | 75.9 | Legacy 83.9 vs modern 75.9 |
| Mismatch | `d2-private.n` | CF-S-00 | D2 | PRIVATE | SalaryN | 58 | 16 | Legacy 58 vs modern 16 |
| Mismatch | `d2-private.p25` | SS-SAL-02 | D2 | PRIVATE | Pct25 | 84000 | 83000 | Legacy 84000 vs modern 83000 |
| Mismatch | `d2-private.median` | SS-SAL-03 | D2 | PRIVATE | Median | 95500 | 93000 | Legacy 95500 vs modern 93000 |
| Mismatch | `d2-private.p75` | SS-SAL-04 | D2 | PRIVATE | Pct75 | 106000 | 123000 | Legacy 106000 vs modern 123000 |
| Mismatch | `d2-private.mean` | SS-SAL-05 | D2 | PRIVATE | Mean | 101425 | 106750 | Legacy 101425 vs modern 106750 |
| Mismatch | `d2-public.count` | CF-C-08 | D2 | PUBLIC | Count | 15 | 7 | Legacy 15 vs modern 7 |
| Mismatch | `d2-public.percent` | CF-C-08 | D2 | PUBLIC | Percent | 16.1 | 24.1 | Legacy 16.1 vs modern 24.1 |
| Mismatch | `d2-public.n` | CF-S-00 | D2 | PUBLIC | SalaryN | 10 | 7 | Legacy 10 vs modern 7 |
| Mismatch | `d2-public.p25` | SS-SAL-02 | D2 | PUBLIC | Pct25 | 66000 | 80500 | Legacy 66000 vs modern 80500 |
| Mismatch | `d2-public.median` | SS-SAL-03 | D2 | PUBLIC | Median | 73950 | 85500 | Legacy 73950 vs modern 85500 |
| Mismatch | `d2-public.p75` | SS-SAL-04 | D2 | PUBLIC | Pct75 | 92000 | 90059 | Legacy 92000 vs modern 90059 |
| Mismatch | `d2-public.mean` | SS-SAL-05 | D2 | PUBLIC | Mean | 78532 | 82223 | Legacy 78532 vs modern 82223 |
| Mismatch | `d2.subtotal-count` | SS-SUB-01 | D2 |  | SubtotalCount | 93 | 29 | Legacy 93 vs modern 29 |
| Match | `d2.subtotal-percent` | SS-SUB-01 | D2 |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Mismatch | `d3-ljd-ft.count` | CF-C-07 | D3 | 1-LJDFULL | Count | 79 | 26 | Legacy 79 vs modern 26; Page-2 LJD FT 79 vs page-1 LJD 78. |
| Mismatch | `d3-ljd-ft.percent` | CF-C-07 | D3 | 1-LJDFULL | Percent | 84.9 | 89.7 | Legacy 84.9 vs modern 89.7; Page-2 LJD FT 79 vs page-1 LJD 78. |
| Mismatch | `d3-ljd-ft.n` | CF-S-00 | D3 | 1-LJDFULL | SalaryN | 69 | 22 | Legacy 69 vs modern 22; Page-2 LJD FT 79 vs page-1 LJD 78. |
| Mismatch | `d3-ljd-ft.p25` | SS-SAL-02 | D3 | 1-LJDFULL | Pct25 | 74000 | 80500 | Legacy 74000 vs modern 80500; Page-2 LJD FT 79 vs page-1 LJD 78. |
| Mismatch | `d3-ljd-ft.median` | SS-SAL-03 | D3 | 1-LJDFULL | Median | 88000 | 90280 | Legacy 88000 vs modern 90280; Page-2 LJD FT 79 vs page-1 LJD 78. |
| Mismatch | `d3-ljd-ft.p75` | SS-SAL-04 | D3 | 1-LJDFULL | Pct75 | 104000 | 100500 | Legacy 104000 vs modern 100500; Page-2 LJD FT 79 vs page-1 LJD 78. |
| Mismatch | `d3-ljd-ft.mean` | SS-SAL-05 | D3 | 1-LJDFULL | Mean | 88297 | 97866 | Legacy 88297 vs modern 97866; Page-2 LJD FT 79 vs page-1 LJD 78. |
| Mismatch | `d3-nljd-ft.count` | CF-C-07 | D3 | 2-NLJDFULL | Count | 10 | 1 | Legacy 10 vs modern 1; Salary n 12 > count 10. |
| Mismatch | `d3-nljd-ft.percent` | CF-C-07 | D3 | 2-NLJDFULL | Percent | 10.8 | 3.4 | Legacy 10.8 vs modern 3.4; Salary n 12 > count 10. |
| Mismatch | `d3-nljd-ft.n` | CF-S-00 | D3 | 2-NLJDFULL | SalaryN | 12 | . | Legacy 12 vs modern .; Salary n 12 > count 10. |
| Mismatch | `d3-nljd-ft.p25` | SS-SAL-02 | D3 | 2-NLJDFULL | Pct25 | 70250 | . | Legacy 70250 vs modern .; Salary n 12 > count 10. |
| Mismatch | `d3-nljd-ft.median` | SS-SAL-03 | D3 | 2-NLJDFULL | Median | 93500 | . | Legacy 93500 vs modern .; Salary n 12 > count 10. |
| Mismatch | `d3-nljd-ft.p75` | SS-SAL-04 | D3 | 2-NLJDFULL | Pct75 | 104200 | . | Legacy 104200 vs modern .; Salary n 12 > count 10. |
| Mismatch | `d3-nljd-ft.mean` | SS-SAL-05 | D3 | 2-NLJDFULL | Mean | 99325 | . | Legacy 99325 vs modern .; Salary n 12 > count 10. |
| Mismatch | `d3-nljd-pt.count` | CF-C-07 | D3 | 2-NLJDPART | Count | 2 | . | Legacy 2 vs modern . |
| Mismatch | `d3-nljd-pt.percent` | CF-C-07 | D3 | 2-NLJDPART | Percent | 2.2 | . | Legacy 2.2 vs modern . |
| Match | `d3-nljd-pt.n` | CF-S-00 | D3 | 2-NLJDPART | SalaryN | . | . | Legacy . vs modern . |
| Match | `d3-nljd-pt.p25` | SS-SAL-02 | D3 | 2-NLJDPART | Pct25 | . | . | Legacy . vs modern . |
| Match | `d3-nljd-pt.median` | SS-SAL-03 | D3 | 2-NLJDPART | Median | . | . | Legacy . vs modern . |
| Match | `d3-nljd-pt.p75` | SS-SAL-04 | D3 | 2-NLJDPART | Pct75 | . | . | Legacy . vs modern . |
| Match | `d3-nljd-pt.mean` | SS-SAL-05 | D3 | 2-NLJDPART | Mean | . | . | Legacy . vs modern . |
| Mismatch | `d3-nlp-ft.count` | CF-C-07 | D3 | 3-NLPFULL | Count | 1 | . | Legacy 1 vs modern . |
| Mismatch | `d3-nlp-ft.percent` | CF-C-07 | D3 | 3-NLPFULL | Percent | 1.1 | . | Legacy 1.1 vs modern . |
| Match | `d3-nlp-ft.n` | CF-S-00 | D3 | 3-NLPFULL | SalaryN | . | . | Legacy . vs modern . |
| Match | `d3-nlp-ft.p25` | SS-SAL-02 | D3 | 3-NLPFULL | Pct25 | . | . | Legacy . vs modern . |
| Match | `d3-nlp-ft.median` | SS-SAL-03 | D3 | 3-NLPFULL | Median | . | . | Legacy . vs modern . |
| Match | `d3-nlp-ft.p75` | SS-SAL-04 | D3 | 3-NLPFULL | Pct75 | . | . | Legacy . vs modern . |
| Match | `d3-nlp-ft.mean` | SS-SAL-05 | D3 | 3-NLPFULL | Mean | . | . | Legacy . vs modern . |
| Mismatch | `d3-nlo-ft.count` | CF-C-07 | D3 | 4-NLOFULL | Count | 1 | . | Legacy 1 vs modern . |
| Mismatch | `d3-nlo-ft.percent` | CF-C-07 | D3 | 4-NLOFULL | Percent | 1.1 | . | Legacy 1.1 vs modern . |
| Match | `d3-nlo-ft.n` | CF-S-00 | D3 | 4-NLOFULL | SalaryN | . | . | Legacy . vs modern . |
| Match | `d3-nlo-ft.p25` | SS-SAL-02 | D3 | 4-NLOFULL | Pct25 | . | . | Legacy . vs modern . |
| Match | `d3-nlo-ft.median` | SS-SAL-03 | D3 | 4-NLOFULL | Median | . | . | Legacy . vs modern . |
| Match | `d3-nlo-ft.p75` | SS-SAL-04 | D3 | 4-NLOFULL | Pct75 | . | . | Legacy . vs modern . |
| Match | `d3-nlo-ft.mean` | SS-SAL-05 | D3 | 4-NLOFULL | Mean | . | . | Legacy . vs modern . |
| Match | `d3-nlo-pt.count` | CF-C-07 | D3 | 4-NLOPART | Count | 1 | 1 | Legacy 1 vs modern 1 |
| Mismatch | `d3-nlo-pt.percent` | CF-C-07 | D3 | 4-NLOPART | Percent | 1.1 | 3.4 | Legacy 1.1 vs modern 3.4 |
| Match | `d3-nlo-pt.n` | CF-S-00 | D3 | 4-NLOPART | SalaryN | . | . | Legacy . vs modern . |
| Match | `d3-nlo-pt.p25` | SS-SAL-02 | D3 | 4-NLOPART | Pct25 | . | . | Legacy . vs modern . |
| Match | `d3-nlo-pt.median` | SS-SAL-03 | D3 | 4-NLOPART | Median | . | . | Legacy . vs modern . |
| Match | `d3-nlo-pt.p75` | SS-SAL-04 | D3 | 4-NLOPART | Pct75 | . | . | Legacy . vs modern . |
| Match | `d3-nlo-pt.mean` | SS-SAL-05 | D3 | 4-NLOPART | Mean | . | . | Legacy . vs modern . |
| Mismatch | `d3.subtotal-count` | SS-CALC-01 | D3 |  | SubtotalCount | 93 | 29 | Legacy 93 vs modern 29; Displayed details sum to 94; printed subtotal is 93. |
| Match | `d3.subtotal-percent` | SS-CALC-01 | D3 |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0; Displayed details sum to 94; printed subtotal is 93. |
| Mismatch | `e1-bus.count` | CF-C-09 | E1 | BUS | Count | 14 | 2 | Legacy 14 vs modern 2 |
| Mismatch | `e1-bus.percent` | CF-C-09 | E1 | BUS | Percent | 15.1 | 6.9 | Legacy 15.1 vs modern 6.9 |
| Mismatch | `e1-bus.n` | CF-S-00 | E1 | BUS | SalaryN | 13 | . | Legacy 13 vs modern . |
| Mismatch | `e1-bus.p25` | SS-SAL-02 | E1 | BUS | Pct25 | 95000 | . | Legacy 95000 vs modern . |
| Mismatch | `e1-bus.median` | SS-SAL-03 | E1 | BUS | Median | 108000 | . | Legacy 108000 vs modern . |
| Mismatch | `e1-bus.p75` | SS-SAL-04 | E1 | BUS | Pct75 | 110000 | . | Legacy 110000 vs modern . |
| Mismatch | `e1-bus.mean` | SS-SAL-05 | E1 | BUS | Mean | 120500 | . | Legacy 120500 vs modern . |
| Mismatch | `e1-clerk.count` | CF-C-09 | E1 | CLERK | Count | 5 | 4 | Legacy 5 vs modern 4 |
| Mismatch | `e1-clerk.percent` | CF-C-09 | E1 | CLERK | Percent | 5.4 | 13.8 | Legacy 5.4 vs modern 13.8 |
| Match | `e1-clerk.n` | CF-S-00 | E1 | CLERK | SalaryN | . | . | Legacy . vs modern . |
| Match | `e1-clerk.p25` | SS-SAL-02 | E1 | CLERK | Pct25 | . | . | Legacy . vs modern . |
| Match | `e1-clerk.median` | SS-SAL-03 | E1 | CLERK | Median | . | . | Legacy . vs modern . |
| Match | `e1-clerk.p75` | SS-SAL-04 | E1 | CLERK | Pct75 | . | . | Legacy . vs modern . |
| Match | `e1-clerk.mean` | SS-SAL-05 | E1 | CLERK | Mean | . | . | Legacy . vs modern . |
| Mismatch | `e1-firm.count` | CF-C-09 | E1 | FIRM | Count | 50 | 20 | Legacy 50 vs modern 20 |
| Mismatch | `e1-firm.percent` | CF-C-09 | E1 | FIRM | Percent | 53.8 | 69.0 | Legacy 53.8 vs modern 69.0 |
| Mismatch | `e1-firm.n` | CF-S-00 | E1 | FIRM | SalaryN | 45 | 15 | Legacy 45 vs modern 15 |
| Mismatch | `e1-firm.p25` | SS-SAL-02 | E1 | FIRM | Pct25 | 82000 | 80500 | Legacy 82000 vs modern 80500 |
| Mismatch | `e1-firm.median` | SS-SAL-03 | E1 | FIRM | Median | 93000 | 90500 | Legacy 93000 vs modern 90500 |
| Mismatch | `e1-firm.p75` | SS-SAL-04 | E1 | FIRM | Pct75 | 105000 | 120500 | Legacy 105000 vs modern 120500 |
| Mismatch | `e1-firm.mean` | SS-SAL-05 | E1 | FIRM | Mean | 96901 | 105167 | Legacy 96901 vs modern 105167 |
| Mismatch | `e1-govt.count` | CF-C-09 | E1 | GOVT | Count | 16 | 1 | Legacy 16 vs modern 1 |
| Mismatch | `e1-govt.percent` | CF-C-09 | E1 | GOVT | Percent | 17.2 | 3.4 | Legacy 17.2 vs modern 3.4 |
| Mismatch | `e1-govt.n` | CF-S-00 | E1 | GOVT | SalaryN | 15 | . | Legacy 15 vs modern . |
| Mismatch | `e1-govt.p25` | SS-SAL-02 | E1 | GOVT | Pct25 | 65000 | . | Legacy 65000 vs modern . |
| Mismatch | `e1-govt.median` | SS-SAL-03 | E1 | GOVT | Median | 84000 | . | Legacy 84000 vs modern . |
| Mismatch | `e1-govt.p75` | SS-SAL-04 | E1 | GOVT | Pct75 | 93000 | . | Legacy 93000 vs modern . |
| Mismatch | `e1-govt.mean` | SS-SAL-05 | E1 | GOVT | Mean | 82010 | . | Legacy 82010 vs modern . |
| Mismatch | `e1-pi.count` | CF-C-09 | E1 | PUBINT | Count | 8 | 2 | Legacy 8 vs modern 2 |
| Mismatch | `e1-pi.percent` | CF-C-09 | E1 | PUBINT | Percent | 8.6 | 6.9 | Legacy 8.6 vs modern 6.9 |
| Mismatch | `e1-pi.n` | CF-S-00 | E1 | PUBINT | SalaryN | 6 | . | Legacy 6 vs modern . |
| Mismatch | `e1-pi.p25` | SS-SAL-02 | E1 | PUBINT | Pct25 | 64000 | . | Legacy 64000 vs modern . |
| Mismatch | `e1-pi.median` | SS-SAL-03 | E1 | PUBINT | Median | 73495 | . | Legacy 73495 vs modern . |
| Mismatch | `e1-pi.p75` | SS-SAL-04 | E1 | PUBINT | Pct75 | 92000 | . | Legacy 92000 vs modern . |
| Mismatch | `e1-pi.mean` | SS-SAL-05 | E1 | PUBINT | Mean | 76801 | . | Legacy 76801 vs modern . |
| Mismatch | `e1.subtotal-count` | SS-SUB-01 | E1 |  | SubtotalCount | 93 | 29 | Legacy 93 vs modern 29 |
| Match | `e1.subtotal-percent` | SS-SUB-01 | E1 |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Match | `e1-acad.absent` | SS-FIL-08 | E1 | ACAD | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Match | `e2.absent` | SS-FIL-08 | E2 |  | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Mismatch | `e3-ljd.count` | CF-C-11 | E3 | 1-LJD | Count | 2 | . | Legacy 2 vs modern . |
| Mismatch | `e3-ljd.percent` | CF-C-11 | E3 | 1-LJD | Percent | 14.3 | . | Legacy 14.3 vs modern . |
| Match | `e3-ljd.n` | CF-S-00 | E3 | 1-LJD | SalaryN | . | . | Legacy . vs modern . |
| Match | `e3-ljd.p25` | SS-SAL-02 | E3 | 1-LJD | Pct25 | . | . | Legacy . vs modern . |
| Match | `e3-ljd.median` | SS-SAL-03 | E3 | 1-LJD | Median | . | . | Legacy . vs modern . |
| Match | `e3-ljd.p75` | SS-SAL-04 | E3 | 1-LJD | Pct75 | . | . | Legacy . vs modern . |
| Match | `e3-ljd.mean` | SS-SAL-05 | E3 | 1-LJD | Mean | . | . | Legacy . vs modern . |
| Mismatch | `e3-nljd.count` | CF-C-11 | E3 | 2-NLJD | Count | 10 | 1 | Legacy 10 vs modern 1 |
| Mismatch | `e3-nljd.percent` | CF-C-11 | E3 | 2-NLJD | Percent | 71.3 | 50 | Legacy 71.3 vs modern 50 |
| Mismatch | `e3-nljd.n` | CF-S-00 | E3 | 2-NLJD | SalaryN | 8 | . | Legacy 8 vs modern . |
| Mismatch | `e3-nljd.p25` | SS-SAL-02 | E3 | 2-NLJD | Pct25 | 95000 | . | Legacy 95000 vs modern . |
| Mismatch | `e3-nljd.median` | SS-SAL-03 | E3 | 2-NLJD | Median | 108000 | . | Legacy 108000 vs modern . |
| Mismatch | `e3-nljd.p75` | SS-SAL-04 | E3 | 2-NLJD | Pct75 | 109200 | . | Legacy 109200 vs modern . |
| Mismatch | `e3-nljd.mean` | SS-SAL-05 | E3 | 2-NLJD | Mean | 109185 | . | Legacy 109185 vs modern . |
| Mismatch | `e3-nlp.count` | CF-C-11 | E3 | 3-NLP | Count | 1 | . | Legacy 1 vs modern . |
| Mismatch | `e3-nlp.percent` | CF-C-11 | E3 | 3-NLP | Percent | 7.1 | . | Legacy 7.1 vs modern . |
| Match | `e3-nlp.n` | CF-S-00 | E3 | 3-NLP | SalaryN | . | . | Legacy . vs modern . |
| Match | `e3-nlp.p25` | SS-SAL-02 | E3 | 3-NLP | Pct25 | . | . | Legacy . vs modern . |
| Match | `e3-nlp.median` | SS-SAL-03 | E3 | 3-NLP | Median | . | . | Legacy . vs modern . |
| Match | `e3-nlp.p75` | SS-SAL-04 | E3 | 3-NLP | Pct75 | . | . | Legacy . vs modern . |
| Match | `e3-nlp.mean` | SS-SAL-05 | E3 | 3-NLP | Mean | . | . | Legacy . vs modern . |
| Match | `e3-nlo.count` | CF-C-11 | E3 | 4-NLO | Count | 1 | 1 | Legacy 1 vs modern 1 |
| Mismatch | `e3-nlo.percent` | CF-C-11 | E3 | 4-NLO | Percent | 7.1 | 50 | Legacy 7.1 vs modern 50 |
| Match | `e3-nlo.n` | CF-S-00 | E3 | 4-NLO | SalaryN | . | . | Legacy . vs modern . |
| Match | `e3-nlo.p25` | SS-SAL-02 | E3 | 4-NLO | Pct25 | . | . | Legacy . vs modern . |
| Match | `e3-nlo.median` | SS-SAL-03 | E3 | 4-NLO | Median | . | . | Legacy . vs modern . |
| Match | `e3-nlo.p75` | SS-SAL-04 | E3 | 4-NLO | Pct75 | . | . | Legacy . vs modern . |
| Match | `e3-nlo.mean` | SS-SAL-05 | E3 | 4-NLO | Mean | . | . | Legacy . vs modern . |
| Mismatch | `e3.subtotal-count` | SS-SUB-01 | E3 |  | SubtotalCount | 14 | 2 | Legacy 14 vs modern 2 |
| Match | `e3.subtotal-percent` | SS-SUB-01 | E3 |  | SubtotalPercent | 100.0 | 100 | Legacy 100.0 vs modern 100 |
| Mismatch | `e4-ljd.count` | CF-C-12 | E4 | 1-LJD | Count | 46 | 20 | Legacy 46 vs modern 20 |
| Mismatch | `e4-ljd.percent` | CF-C-12 | E4 | 1-LJD | Percent | 92.0 | 100 | Legacy 92.0 vs modern 100 |
| Mismatch | `e4-ljd.n` | CF-S-00 | E4 | 1-LJD | SalaryN | 40 | 15 | Legacy 40 vs modern 15 |
| Mismatch | `e4-ljd.p25` | SS-SAL-02 | E4 | 1-LJD | Pct25 | 82000 | 80500 | Legacy 82000 vs modern 80500 |
| Mismatch | `e4-ljd.median` | SS-SAL-03 | E4 | 1-LJD | Median | 95000 | 90500 | Legacy 95000 vs modern 90500 |
| Mismatch | `e4-ljd.p75` | SS-SAL-04 | E4 | 1-LJD | Pct75 | 102000 | 120500 | Legacy 102000 vs modern 120500 |
| Mismatch | `e4-ljd.mean` | SS-SAL-05 | E4 | 1-LJD | Mean | 96608 | 105167 | Legacy 96608 vs modern 105167 |
| Mismatch | `e4-nljd.count` | CF-C-12 | E4 | 2-NLJD | Count | 4 | . | Legacy 4 vs modern . |
| Mismatch | `e4-nljd.percent` | CF-C-12 | E4 | 2-NLJD | Percent | 8.0 | . | Legacy 8.0 vs modern . |
| Match | `e4-nljd.n` | CF-S-00 | E4 | 2-NLJD | SalaryN | . | . | Legacy . vs modern . |
| Match | `e4-nljd.p25` | SS-SAL-02 | E4 | 2-NLJD | Pct25 | . | . | Legacy . vs modern . |
| Match | `e4-nljd.median` | SS-SAL-03 | E4 | 2-NLJD | Median | . | . | Legacy . vs modern . |
| Match | `e4-nljd.p75` | SS-SAL-04 | E4 | 2-NLJD | Pct75 | . | . | Legacy . vs modern . |
| Match | `e4-nljd.mean` | SS-SAL-05 | E4 | 2-NLJD | Mean | . | . | Legacy . vs modern . |
| Mismatch | `e4.subtotal-count` | SS-SUB-01 | E4 |  | SubtotalCount | 50 | 20 | Legacy 50 vs modern 20 |
| Match | `e4.subtotal-percent` | SS-SUB-01 | E4 |  | SubtotalPercent | 100.0 | 100 | Legacy 100.0 vs modern 100 |
| Mismatch | `e5-ljd.count` | CF-C-13 | E5 | 1-LJD | Count | 13 | 1 | Legacy 13 vs modern 1 |
| Mismatch | `e5-ljd.percent` | CF-C-13 | E5 | 1-LJD | Percent | 81.3 | 100 | Legacy 81.3 vs modern 100 |
| Mismatch | `e5-ljd.n` | CF-S-00 | E5 | 1-LJD | SalaryN | 11 | . | Legacy 11 vs modern . |
| Mismatch | `e5-ljd.p25` | SS-SAL-02 | E5 | 1-LJD | Pct25 | 78000 | . | Legacy 78000 vs modern . |
| Mismatch | `e5-ljd.median` | SS-SAL-03 | E5 | 1-LJD | Median | 88000 | . | Legacy 88000 vs modern . |
| Mismatch | `e5-ljd.p75` | SS-SAL-04 | E5 | 1-LJD | Pct75 | 90000 | . | Legacy 90000 vs modern . |
| Mismatch | `e5-ljd.mean` | SS-SAL-05 | E5 | 1-LJD | Mean | 83957 | . | Legacy 83957 vs modern . |
| Mismatch | `e5-nljd.count` | CF-C-13 | E5 | 2-NLJD | Count | 3 | . | Legacy 3 vs modern . |
| Mismatch | `e5-nljd.percent` | CF-C-13 | E5 | 2-NLJD | Percent | 18.8 | . | Legacy 18.8 vs modern . |
| Match | `e5-nljd.n` | CF-S-00 | E5 | 2-NLJD | SalaryN | . | . | Legacy . vs modern . |
| Match | `e5-nljd.p25` | SS-SAL-02 | E5 | 2-NLJD | Pct25 | . | . | Legacy . vs modern . |
| Match | `e5-nljd.median` | SS-SAL-03 | E5 | 2-NLJD | Median | . | . | Legacy . vs modern . |
| Match | `e5-nljd.p75` | SS-SAL-04 | E5 | 2-NLJD | Pct75 | . | . | Legacy . vs modern . |
| Match | `e5-nljd.mean` | SS-SAL-05 | E5 | 2-NLJD | Mean | . | . | Legacy . vs modern . |
| Mismatch | `e5.subtotal-count` | SS-SUB-01 | E5 |  | SubtotalCount | 16 | 1 | Legacy 16 vs modern 1 |
| Match | `e5.subtotal-percent` | SS-SUB-01 | E5 |  | SubtotalPercent | 100.0 | 100 | Legacy 100.0 vs modern 100 |
| Mismatch | `e55-state.count` | CF-C-14 | E55 | JCSTGV | Count | 4 | . | Legacy 4 vs modern .; PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column. |
| Mismatch | `e55-state.percent` | CF-C-14 | E55 | JCSTGV | Percent | 80.0 | . | Legacy 80.0 vs modern .; PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-state.n` | CF-S-00 | E55 | JCSTGV | SalaryN | . | . | Legacy . vs modern .; PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-state.p25` | SS-SAL-02 | E55 | JCSTGV | Pct25 | . | . | Legacy . vs modern .; PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-state.median` | SS-SAL-03 | E55 | JCSTGV | Median | . | . | Legacy . vs modern .; PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-state.p75` | SS-SAL-04 | E55 | JCSTGV | Pct75 | . | . | Legacy . vs modern .; PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-state.mean` | SS-SAL-05 | E55 | JCSTGV | Mean | . | . | Legacy . vs modern .; PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column. |
| Mismatch | `e55-local.count` | CF-C-14 | E55 | JCTLOG | Count | 1 | . | Legacy 1 vs modern .; PDF label Local maps to $newvar JCTLOG (SS-FMT-04). Sample export has no emptype1 column. |
| Mismatch | `e55-local.percent` | CF-C-14 | E55 | JCTLOG | Percent | 20.0 | . | Legacy 20.0 vs modern .; PDF label Local maps to $newvar JCTLOG (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-local.n` | CF-S-00 | E55 | JCTLOG | SalaryN | . | . | Legacy . vs modern .; PDF label Local maps to $newvar JCTLOG (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-local.p25` | SS-SAL-02 | E55 | JCTLOG | Pct25 | . | . | Legacy . vs modern .; PDF label Local maps to $newvar JCTLOG (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-local.median` | SS-SAL-03 | E55 | JCTLOG | Median | . | . | Legacy . vs modern .; PDF label Local maps to $newvar JCTLOG (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-local.p75` | SS-SAL-04 | E55 | JCTLOG | Pct75 | . | . | Legacy . vs modern .; PDF label Local maps to $newvar JCTLOG (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-local.mean` | SS-SAL-05 | E55 | JCTLOG | Mean | . | . | Legacy . vs modern .; PDF label Local maps to $newvar JCTLOG (SS-FMT-04). Sample export has no emptype1 column. |
| Match | `e55-federal.absent` | SS-FIL-08 | E55 | JCFDGV | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Match | `e55-tribal.absent` | SS-FIL-08 | E55 | JCTRGV | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Match | `e55-unknown.absent` | SS-FIL-08 | E55 | JCUGOV | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Mismatch | `e55.subtotal-count` | SS-SUB-01 | E55 |  | SubtotalCount | 5 | . | Legacy 5 vs modern . |
| Mismatch | `e55.subtotal-percent` | SS-SUB-01 | E55 |  | SubtotalPercent | 100.0 | . | Legacy 100.0 vs modern . |
| Mismatch | `e6-ljd.count` | CF-C-15 | E6 | 1-LJD | Count | 6 | 2 | Legacy 6 vs modern 2 |
| Mismatch | `e6-ljd.percent` | CF-C-15 | E6 | 1-LJD | Percent | 75.0 | 100 | Legacy 75.0 vs modern 100 |
| Mismatch | `e6-ljd.n` | CF-S-00 | E6 | 1-LJD | SalaryN | 5 | . | Legacy 5 vs modern . |
| Mismatch | `e6-ljd.p25` | SS-SAL-02 | E6 | 1-LJD | Pct25 | 66000 | . | Legacy 66000 vs modern . |
| Mismatch | `e6-ljd.median` | SS-SAL-03 | E6 | 1-LJD | Median | 71405 | . | Legacy 71405 vs modern . |
| Mismatch | `e6-ljd.p75` | SS-SAL-04 | E6 | 1-LJD | Pct75 | 88000 | . | Legacy 88000 vs modern . |
| Mismatch | `e6-ljd.mean` | SS-SAL-05 | E6 | 1-LJD | Mean | 73821 | . | Legacy 73821 vs modern . |
| Mismatch | `e6-nljd.count` | CF-C-15 | E6 | 2-NLJD | Count | 1 | . | Legacy 1 vs modern . |
| Mismatch | `e6-nljd.percent` | CF-C-15 | E6 | 2-NLJD | Percent | 12.5 | . | Legacy 12.5 vs modern . |
| Match | `e6-nljd.n` | CF-S-00 | E6 | 2-NLJD | SalaryN | . | . | Legacy . vs modern . |
| Match | `e6-nljd.p25` | SS-SAL-02 | E6 | 2-NLJD | Pct25 | . | . | Legacy . vs modern . |
| Match | `e6-nljd.median` | SS-SAL-03 | E6 | 2-NLJD | Median | . | . | Legacy . vs modern . |
| Match | `e6-nljd.p75` | SS-SAL-04 | E6 | 2-NLJD | Pct75 | . | . | Legacy . vs modern . |
| Match | `e6-nljd.mean` | SS-SAL-05 | E6 | 2-NLJD | Mean | . | . | Legacy . vs modern . |
| Mismatch | `e6-nlo.count` | CF-C-15 | E6 | 4-NLO | Count | 1 | . | Legacy 1 vs modern . |
| Mismatch | `e6-nlo.percent` | CF-C-15 | E6 | 4-NLO | Percent | 12.5 | . | Legacy 12.5 vs modern . |
| Match | `e6-nlo.n` | CF-S-00 | E6 | 4-NLO | SalaryN | . | . | Legacy . vs modern . |
| Match | `e6-nlo.p25` | SS-SAL-02 | E6 | 4-NLO | Pct25 | . | . | Legacy . vs modern . |
| Match | `e6-nlo.median` | SS-SAL-03 | E6 | 4-NLO | Median | . | . | Legacy . vs modern . |
| Match | `e6-nlo.p75` | SS-SAL-04 | E6 | 4-NLO | Pct75 | . | . | Legacy . vs modern . |
| Match | `e6-nlo.mean` | SS-SAL-05 | E6 | 4-NLO | Mean | . | . | Legacy . vs modern . |
| Mismatch | `e6.subtotal-count` | SS-SUB-01 | E6 |  | SubtotalCount | 8 | 2 | Legacy 8 vs modern 2 |
| Match | `e6.subtotal-percent` | SS-SUB-01 | E6 |  | SubtotalPercent | 100.0 | 100 | Legacy 100.0 vs modern 100 |
| Mismatch | `firm-lf1.count` | CF-C-16 | FIRM | LF1 | Count | 30 | 3 | Legacy 30 vs modern 3 |
| Mismatch | `firm-lf1.percent` | CF-C-16 | FIRM | LF1 | Percent | 60.0 | 15 | Legacy 60.0 vs modern 15 |
| Mismatch | `firm-lf1.n` | CF-S-00 | FIRM | LF1 | SalaryN | 18 | . | Legacy 18 vs modern . |
| Mismatch | `firm-lf1.p25` | SS-SAL-02 | FIRM | LF1 | Pct25 | 78000 | . | Legacy 78000 vs modern . |
| Mismatch | `firm-lf1.median` | SS-SAL-03 | FIRM | LF1 | Median | 84000 | . | Legacy 84000 vs modern . |
| Mismatch | `firm-lf1.p75` | SS-SAL-04 | FIRM | LF1 | Pct75 | 90000 | . | Legacy 90000 vs modern . |
| Mismatch | `firm-lf1.mean` | SS-SAL-05 | FIRM | LF1 | Mean | 83710 | . | Legacy 83710 vs modern . |
| Mismatch | `firm-lf2.count` | CF-C-16 | FIRM | LF2 | Count | 10 | 3 | Legacy 10 vs modern 3 |
| Mismatch | `firm-lf2.percent` | CF-C-16 | FIRM | LF2 | Percent | 20.0 | 15 | Legacy 20.0 vs modern 15 |
| Mismatch | `firm-lf2.n` | CF-S-00 | FIRM | LF2 | SalaryN | 10 | . | Legacy 10 vs modern . |
| Mismatch | `firm-lf2.p25` | SS-SAL-02 | FIRM | LF2 | Pct25 | 81000 | . | Legacy 81000 vs modern . |
| Mismatch | `firm-lf2.median` | SS-SAL-03 | FIRM | LF2 | Median | 87000 | . | Legacy 87000 vs modern . |
| Mismatch | `firm-lf2.p75` | SS-SAL-04 | FIRM | LF2 | Pct75 | 101000 | . | Legacy 101000 vs modern . |
| Mismatch | `firm-lf2.mean` | SS-SAL-05 | FIRM | LF2 | Mean | 89300 | . | Legacy 89300 vs modern . |
| Match | `firm-lf3.count` | CF-C-16 | FIRM | LF3 | Count | 3 | 3 | Legacy 3 vs modern 3 |
| Mismatch | `firm-lf3.percent` | CF-C-16 | FIRM | LF3 | Percent | 6.0 | 15 | Legacy 6.0 vs modern 15 |
| Match | `firm-lf3.n` | CF-S-00 | FIRM | LF3 | SalaryN | . | . | Legacy . vs modern . |
| Match | `firm-lf3.p25` | SS-SAL-02 | FIRM | LF3 | Pct25 | . | . | Legacy . vs modern . |
| Match | `firm-lf3.median` | SS-SAL-03 | FIRM | LF3 | Median | . | . | Legacy . vs modern . |
| Match | `firm-lf3.p75` | SS-SAL-04 | FIRM | LF3 | Pct75 | . | . | Legacy . vs modern . |
| Match | `firm-lf3.mean` | SS-SAL-05 | FIRM | LF3 | Mean | . | . | Legacy . vs modern . |
| Match | `firm-lf4.count` | CF-C-16 | FIRM | LF4 | Count | 4 | 4 | Legacy 4 vs modern 4 |
| Mismatch | `firm-lf4.percent` | CF-C-16 | FIRM | LF4 | Percent | 8.0 | 20 | Legacy 8.0 vs modern 20 |
| Match | `firm-lf4.n` | CF-S-00 | FIRM | LF4 | SalaryN | . | . | Legacy . vs modern . |
| Match | `firm-lf4.p25` | SS-SAL-02 | FIRM | LF4 | Pct25 | . | . | Legacy . vs modern . |
| Match | `firm-lf4.median` | SS-SAL-03 | FIRM | LF4 | Median | . | . | Legacy . vs modern . |
| Match | `firm-lf4.p75` | SS-SAL-04 | FIRM | LF4 | Pct75 | . | . | Legacy . vs modern . |
| Match | `firm-lf4.mean` | SS-SAL-05 | FIRM | LF4 | Mean | . | . | Legacy . vs modern . |
| Mismatch | `firm-lf5.count` | CF-C-16 | FIRM | LF5 | Count | 1 | 2 | Legacy 1 vs modern 2 |
| Mismatch | `firm-lf5.percent` | CF-C-16 | FIRM | LF5 | Percent | 2.0 | 10 | Legacy 2.0 vs modern 10 |
| Match | `firm-lf5.n` | CF-S-00 | FIRM | LF5 | SalaryN | . | . | Legacy . vs modern . |
| Match | `firm-lf5.p25` | SS-SAL-02 | FIRM | LF5 | Pct25 | . | . | Legacy . vs modern . |
| Match | `firm-lf5.median` | SS-SAL-03 | FIRM | LF5 | Median | . | . | Legacy . vs modern . |
| Match | `firm-lf5.p75` | SS-SAL-04 | FIRM | LF5 | Pct75 | . | . | Legacy . vs modern . |
| Match | `firm-lf5.mean` | SS-SAL-05 | FIRM | LF5 | Mean | . | . | Legacy . vs modern . |
| Match | `firm-lf6.count` | CF-C-16 | FIRM | LF6 | Count | 1 | 1 | Legacy 1 vs modern 1 |
| Mismatch | `firm-lf6.percent` | CF-C-16 | FIRM | LF6 | Percent | 2.0 | 5 | Legacy 2.0 vs modern 5 |
| Match | `firm-lf6.n` | CF-S-00 | FIRM | LF6 | SalaryN | . | . | Legacy . vs modern . |
| Match | `firm-lf6.p25` | SS-SAL-02 | FIRM | LF6 | Pct25 | . | . | Legacy . vs modern . |
| Match | `firm-lf6.median` | SS-SAL-03 | FIRM | LF6 | Median | . | . | Legacy . vs modern . |
| Match | `firm-lf6.p75` | SS-SAL-04 | FIRM | LF6 | Pct75 | . | . | Legacy . vs modern . |
| Match | `firm-lf6.mean` | SS-SAL-05 | FIRM | LF6 | Mean | . | . | Legacy . vs modern . |
| Mismatch | `firm-lf7.count` | CF-C-16 | FIRM | LF7 | Count | 1 | 2 | Legacy 1 vs modern 2 |
| Mismatch | `firm-lf7.percent` | CF-C-16 | FIRM | LF7 | Percent | 2.0 | 10 | Legacy 2.0 vs modern 10 |
| Match | `firm-lf7.n` | CF-S-00 | FIRM | LF7 | SalaryN | . | . | Legacy . vs modern . |
| Match | `firm-lf7.p25` | SS-SAL-02 | FIRM | LF7 | Pct25 | . | . | Legacy . vs modern . |
| Match | `firm-lf7.median` | SS-SAL-03 | FIRM | LF7 | Median | . | . | Legacy . vs modern . |
| Match | `firm-lf7.p75` | SS-SAL-04 | FIRM | LF7 | Pct75 | . | . | Legacy . vs modern . |
| Match | `firm-lf7.mean` | SS-SAL-05 | FIRM | LF7 | Mean | . | . | Legacy . vs modern . |
| Mismatch | `firm.subtotal-count` | SS-SUB-01 | FIRM |  | SubtotalCount | 50 | 20 | Legacy 50 vs modern 20 |
| Match | `firm.subtotal-percent` | SS-SUB-01 | FIRM |  | SubtotalPercent | 100.0 | 100 | Legacy 100.0 vs modern 100 |
| Match | `firm-solo.absent` | SS-FIL-08 | FIRM | SOLO | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Mismatch | `firm2-atty.count` | CF-C-17 | FIRM2 | ATTY | Count | 42 | 19 | Legacy 42 vs modern 19 |
| Mismatch | `firm2-atty.percent` | CF-C-17 | FIRM2 | ATTY | Percent | 84.0 | 95 | Legacy 84.0 vs modern 95 |
| Mismatch | `firm2-atty.n` | CF-S-00 | FIRM2 | ATTY | SalaryN | 37 | 15 | Legacy 37 vs modern 15 |
| Mismatch | `firm2-atty.p25` | SS-SAL-02 | FIRM2 | ATTY | Pct25 | 88000 | 80500 | Legacy 88000 vs modern 80500 |
| Mismatch | `firm2-atty.median` | SS-SAL-03 | FIRM2 | ATTY | Median | 92000 | 90500 | Legacy 92000 vs modern 90500 |
| Mismatch | `firm2-atty.p75` | SS-SAL-04 | FIRM2 | ATTY | Pct75 | 101000 | 120500 | Legacy 101000 vs modern 120500 |
| Mismatch | `firm2-atty.mean` | SS-SAL-05 | FIRM2 | ATTY | Mean | 98074 | 105167 | Legacy 98074 vs modern 105167 |
| Mismatch | `firm2-lclerk.count` | CF-C-17 | FIRM2 | LCLERK | Count | 8 | 1 | Legacy 8 vs modern 1 |
| Mismatch | `firm2-lclerk.percent` | CF-C-17 | FIRM2 | LCLERK | Percent | 16.0 | 5 | Legacy 16.0 vs modern 5 |
| Mismatch | `firm2-lclerk.n` | CF-S-00 | FIRM2 | LCLERK | SalaryN | 5 | . | Legacy 5 vs modern . |
| Mismatch | `firm2-lclerk.p25` | SS-SAL-02 | FIRM2 | LCLERK | Pct25 | 76000 | . | Legacy 76000 vs modern . |
| Mismatch | `firm2-lclerk.median` | SS-SAL-03 | FIRM2 | LCLERK | Median | 80500 | . | Legacy 80500 vs modern . |
| Mismatch | `firm2-lclerk.p75` | SS-SAL-04 | FIRM2 | LCLERK | Pct75 | 92000 | . | Legacy 92000 vs modern . |
| Mismatch | `firm2-lclerk.mean` | SS-SAL-05 | FIRM2 | LCLERK | Mean | 81803 | . | Legacy 81803 vs modern . |
| Mismatch | `firm2.subtotal-count` | SS-SUB-01 | FIRM2 |  | SubtotalCount | 50 | 20 | Legacy 50 vs modern 20 |
| Match | `firm2.subtotal-percent` | SS-SUB-01 | FIRM2 |  | SubtotalPercent | 100.0 | 100 | Legacy 100.0 vs modern 100 |
| Mismatch | `reg-1.count` | CF-C-18 | JOBREG1 | 1 | Count | 69 | . | Legacy 69 vs modern . |
| Mismatch | `reg-1.percent` | CF-C-18 | JOBREG1 | 1 | Percent | 74.2 | . | Legacy 74.2 vs modern . |
| Mismatch | `reg-1.n` | CF-S-00 | JOBREG1 | 1 | SalaryN | 57 | . | Legacy 57 vs modern . |
| Mismatch | `reg-1.p25` | SS-SAL-02 | JOBREG1 | 1 | Pct25 | 84000 | . | Legacy 84000 vs modern . |
| Mismatch | `reg-1.median` | SS-SAL-03 | JOBREG1 | 1 | Median | 93000 | . | Legacy 93000 vs modern . |
| Mismatch | `reg-1.p75` | SS-SAL-04 | JOBREG1 | 1 | Pct75 | 105000 | . | Legacy 105000 vs modern . |
| Mismatch | `reg-1.mean` | SS-SAL-05 | JOBREG1 | 1 | Mean | 92800 | . | Legacy 92800 vs modern . |
| Mismatch | `reg-2.count` | CF-C-18 | JOBREG1 | 2 | Count | 16 | 28 | Legacy 16 vs modern 28 |
| Mismatch | `reg-2.percent` | CF-C-18 | JOBREG1 | 2 | Percent | 17.2 | 96.6 | Legacy 17.2 vs modern 96.6 |
| Mismatch | `reg-2.n` | CF-S-00 | JOBREG1 | 2 | SalaryN | 11 | 22 | Legacy 11 vs modern 22 |
| Mismatch | `reg-2.p25` | SS-SAL-02 | JOBREG1 | 2 | Pct25 | 57950 | 80500 | Legacy 57950 vs modern 80500 |
| Mismatch | `reg-2.median` | SS-SAL-03 | JOBREG1 | 2 | Median | 84000 | 90500 | Legacy 84000 vs modern 90500 |
| Mismatch | `reg-2.p75` | SS-SAL-04 | JOBREG1 | 2 | Pct75 | 101000 | 115500 | Legacy 101000 vs modern 115500 |
| Mismatch | `reg-2.mean` | SS-SAL-05 | JOBREG1 | 2 | Mean | 92727 | 100139 | Legacy 92727 vs modern 100139 |
| Mismatch | `reg-5.count` | CF-C-18 | JOBREG1 | 5 | Count | 2 | 1 | Legacy 2 vs modern 1 |
| Mismatch | `reg-5.percent` | CF-C-18 | JOBREG1 | 5 | Percent | 2.2 | 3.4 | Legacy 2.2 vs modern 3.4 |
| Match | `reg-5.n` | CF-S-00 | JOBREG1 | 5 | SalaryN | . | . | Legacy . vs modern . |
| Match | `reg-5.p25` | SS-SAL-02 | JOBREG1 | 5 | Pct25 | . | . | Legacy . vs modern . |
| Match | `reg-5.median` | SS-SAL-03 | JOBREG1 | 5 | Median | . | . | Legacy . vs modern . |
| Match | `reg-5.p75` | SS-SAL-04 | JOBREG1 | 5 | Pct75 | . | . | Legacy . vs modern . |
| Match | `reg-5.mean` | SS-SAL-05 | JOBREG1 | 5 | Mean | . | . | Legacy . vs modern . |
| Mismatch | `reg-6.count` | CF-C-18 | JOBREG1 | 6 | Count | 1 | . | Legacy 1 vs modern . |
| Mismatch | `reg-6.percent` | CF-C-18 | JOBREG1 | 6 | Percent | 1.1 | . | Legacy 1.1 vs modern . |
| Match | `reg-6.n` | CF-S-00 | JOBREG1 | 6 | SalaryN | . | . | Legacy . vs modern . |
| Match | `reg-6.p25` | SS-SAL-02 | JOBREG1 | 6 | Pct25 | . | . | Legacy . vs modern . |
| Match | `reg-6.median` | SS-SAL-03 | JOBREG1 | 6 | Median | . | . | Legacy . vs modern . |
| Match | `reg-6.p75` | SS-SAL-04 | JOBREG1 | 6 | Pct75 | . | . | Legacy . vs modern . |
| Match | `reg-6.mean` | SS-SAL-05 | JOBREG1 | 6 | Mean | . | . | Legacy . vs modern . |
| Mismatch | `reg-8.count` | CF-C-18 | JOBREG1 | 8 | Count | 5 | . | Legacy 5 vs modern . |
| Mismatch | `reg-8.percent` | CF-C-18 | JOBREG1 | 8 | Percent | 5.4 | . | Legacy 5.4 vs modern . |
| Match | `reg-8.n` | CF-S-00 | JOBREG1 | 8 | SalaryN | . | . | Legacy . vs modern . |
| Match | `reg-8.p25` | SS-SAL-02 | JOBREG1 | 8 | Pct25 | . | . | Legacy . vs modern . |
| Match | `reg-8.median` | SS-SAL-03 | JOBREG1 | 8 | Median | . | . | Legacy . vs modern . |
| Match | `reg-8.p75` | SS-SAL-04 | JOBREG1 | 8 | Pct75 | . | . | Legacy . vs modern . |
| Match | `reg-8.mean` | SS-SAL-05 | JOBREG1 | 8 | Mean | . | . | Legacy . vs modern . |
| Mismatch | `reg.subtotal-count` | SS-SUB-01 | JOBREG1 |  | SubtotalCount | 93 | 29 | Legacy 93 vs modern 29 |
| Match | `reg.subtotal-percent` | SS-SUB-01 | JOBREG1 |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Mismatch | `loc-in.count` | CF-C-19 | JOBREG2 | INSTATE | Count | 63 | 25 | Legacy 63 vs modern 25 |
| Mismatch | `loc-in.percent` | CF-C-19 | JOBREG2 | INSTATE | Percent | 67.8 | 86.2 | Legacy 67.8 vs modern 86.2 |
| Mismatch | `loc-in.n` | CF-S-00 | JOBREG2 | INSTATE | SalaryN | 51 | 21 | Legacy 51 vs modern 21 |
| Mismatch | `loc-in.p25` | SS-SAL-02 | JOBREG2 | INSTATE | Pct25 | 81000 | 85500 | Legacy 81000 vs modern 85500 |
| Mismatch | `loc-in.median` | SS-SAL-03 | JOBREG2 | INSTATE | Median | 92000 | 90500 | Legacy 92000 vs modern 90500 |
| Mismatch | `loc-in.p75` | SS-SAL-04 | JOBREG2 | INSTATE | Pct75 | 101000 | 115500 | Legacy 101000 vs modern 115500 |
| Mismatch | `loc-in.mean` | SS-SAL-05 | JOBREG2 | INSTATE | Mean | 90470 | 102122 | Legacy 90470 vs modern 102122 |
| Mismatch | `loc-out.count` | CF-C-19 | JOBREG2 | OUTOFSTATE | Count | 30 | 4 | Legacy 30 vs modern 4 |
| Mismatch | `loc-out.percent` | CF-C-19 | JOBREG2 | OUTOFSTATE | Percent | 32.2 | 13.8 | Legacy 32.2 vs modern 13.8 |
| Mismatch | `loc-out.n` | CF-S-00 | JOBREG2 | OUTOFSTATE | SalaryN | 20 | . | Legacy 20 vs modern . |
| Mismatch | `loc-out.p25` | SS-SAL-02 | JOBREG2 | OUTOFSTATE | Pct25 | 66000 | . | Legacy 66000 vs modern . |
| Mismatch | `loc-out.median` | SS-SAL-03 | JOBREG2 | OUTOFSTATE | Median | 83000 | . | Legacy 83000 vs modern . |
| Mismatch | `loc-out.p75` | SS-SAL-04 | JOBREG2 | OUTOFSTATE | Pct75 | 107000 | . | Legacy 107000 vs modern . |
| Mismatch | `loc-out.mean` | SS-SAL-05 | JOBREG2 | OUTOFSTATE | Mean | 99226 | . | Legacy 99226 vs modern . |
| Mismatch | `loc.subtotal-count` | SS-SUB-01 | JOBREG2 |  | SubtotalCount | 93 | 29 | Legacy 93 vs modern 29 |
| Match | `loc.subtotal-percent` | SS-SUB-01 | JOBREG2 |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Match | `loc-foreign.absent` | SS-FIL-08 | JOBREG2 | FOREIGN | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |
| Mismatch | `states` | CF-C-20 | JOBREG3 | JOBREG3 | Count | 14 | 4 | Legacy 14 vs modern 4 |
| Mismatch | `states.percent` | CF-C-20 | JOBREG3 | JOBREG3 | Percent | . | 100 | Legacy . vs modern 100 |
| Mismatch | `states.subtotal-count` | SS-SUB-02 | JOBREG3 |  | SubtotalCount | 14 | 4 | Legacy 14 vs modern 4 |
| Mismatch | `states.subtotal-percent` | SS-SUB-02 | JOBREG3 |  | SubtotalPercent | . | 100 | Legacy . vs modern 100 |
| Match | `src-aoci.count` | CF-P2-03 | SOURCE | AOCI | Count | 4 | 4 | Legacy 4 vs modern 4 |
| Mismatch | `src-aoci.percent` | CF-P2-03 | SOURCE | AOCI | Percent | 4.4 | 16 | Legacy 4.4 vs modern 16 |
| Match | `src-aoci.n` | CF-S-00 | SOURCE | AOCI | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-aoci.p25` | SS-SAL-02 | SOURCE | AOCI | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-aoci.median` | SS-SAL-03 | SOURCE | AOCI | Median | . | . | Legacy . vs modern . |
| Match | `src-aoci.p75` | SS-SAL-04 | SOURCE | AOCI | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-aoci.mean` | SS-SAL-05 | SOURCE | AOCI | Mean | . | . | Legacy . vs modern . |
| Match | `src-jobfrc.count` | CF-P2-03 | SOURCE | JOBFRC | Count | 1 | 1 | Legacy 1 vs modern 1 |
| Mismatch | `src-jobfrc.percent` | CF-P2-03 | SOURCE | JOBFRC | Percent | 1.1 | 4 | Legacy 1.1 vs modern 4 |
| Match | `src-jobfrc.n` | CF-S-00 | SOURCE | JOBFRC | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-jobfrc.p25` | SS-SAL-02 | SOURCE | JOBFRC | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-jobfrc.median` | SS-SAL-03 | SOURCE | JOBFRC | Median | . | . | Legacy . vs modern . |
| Match | `src-jobfrc.p75` | SS-SAL-04 | SOURCE | JOBFRC | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-jobfrc.mean` | SS-SAL-05 | SOURCE | JOBFRC | Mean | . | . | Legacy . vs modern . |
| Mismatch | `src-jobpst.count` | CF-P2-03 | SOURCE | JOBPST | Count | 10 | 3 | Legacy 10 vs modern 3 |
| Mismatch | `src-jobpst.percent` | CF-P2-03 | SOURCE | JOBPST | Percent | 11.1 | 12 | Legacy 11.1 vs modern 12 |
| Match | `src-jobpst.n` | CF-S-00 | SOURCE | JOBPST | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-jobpst.p25` | SS-SAL-02 | SOURCE | JOBPST | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-jobpst.median` | SS-SAL-03 | SOURCE | JOBPST | Median | . | . | Legacy . vs modern . |
| Match | `src-jobpst.p75` | SS-SAL-04 | SOURCE | JOBPST | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-jobpst.mean` | SS-SAL-05 | SOURCE | JOBPST | Mean | . | . | Legacy . vs modern . |
| Mismatch | `src-online.count` | CF-P2-03 | SOURCE | ONLINE | Count | 10 | 4 | Legacy 10 vs modern 4 |
| Mismatch | `src-online.percent` | CF-P2-03 | SOURCE | ONLINE | Percent | 11.1 | 16 | Legacy 11.1 vs modern 16 |
| Match | `src-online.n` | CF-S-00 | SOURCE | ONLINE | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-online.p25` | SS-SAL-02 | SOURCE | ONLINE | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-online.median` | SS-SAL-03 | SOURCE | ONLINE | Median | . | . | Legacy . vs modern . |
| Match | `src-online.p75` | SS-SAL-04 | SOURCE | ONLINE | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-online.mean` | SS-SAL-05 | SOURCE | ONLINE | Mean | . | . | Legacy . vs modern . |
| Mismatch | `src-oscar.count` | CF-P2-03 | SOURCE | OSCAR | Count | 2 | . | Legacy 2 vs modern . |
| Mismatch | `src-oscar.percent` | CF-P2-03 | SOURCE | OSCAR | Percent | 2.2 | . | Legacy 2.2 vs modern . |
| Match | `src-oscar.n` | CF-S-00 | SOURCE | OSCAR | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-oscar.p25` | SS-SAL-02 | SOURCE | OSCAR | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-oscar.median` | SS-SAL-03 | SOURCE | OSCAR | Median | . | . | Legacy . vs modern . |
| Match | `src-oscar.p75` | SS-SAL-04 | SOURCE | OSCAR | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-oscar.mean` | SS-SAL-05 | SOURCE | OSCAR | Mean | . | . | Legacy . vs modern . |
| Mismatch | `src-prnsmj.count` | CF-P2-03 | SOURCE | PRNSMJ | Count | 2 | 1 | Legacy 2 vs modern 1 |
| Mismatch | `src-prnsmj.percent` | CF-P2-03 | SOURCE | PRNSMJ | Percent | 2.2 | 4 | Legacy 2.2 vs modern 4 |
| Match | `src-prnsmj.n` | CF-S-00 | SOURCE | PRNSMJ | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-prnsmj.p25` | SS-SAL-02 | SOURCE | PRNSMJ | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-prnsmj.median` | SS-SAL-03 | SOURCE | PRNSMJ | Median | . | . | Legacy . vs modern . |
| Match | `src-prnsmj.p75` | SS-SAL-04 | SOURCE | PRNSMJ | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-prnsmj.mean` | SS-SAL-05 | SOURCE | PRNSMJ | Mean | . | . | Legacy . vs modern . |
| Mismatch | `src-rffrnd.count` | CF-P2-03 | SOURCE | RFFRND | Count | 10 | 4 | Legacy 10 vs modern 4 |
| Mismatch | `src-rffrnd.percent` | CF-P2-03 | SOURCE | RFFRND | Percent | 11.1 | 16 | Legacy 11.1 vs modern 16 |
| Match | `src-rffrnd.n` | CF-S-00 | SOURCE | RFFRND | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-rffrnd.p25` | SS-SAL-02 | SOURCE | RFFRND | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-rffrnd.median` | SS-SAL-03 | SOURCE | RFFRND | Median | . | . | Legacy . vs modern . |
| Match | `src-rffrnd.p75` | SS-SAL-04 | SOURCE | RFFRND | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-rffrnd.mean` | SS-SAL-05 | SOURCE | RFFRND | Mean | . | . | Legacy . vs modern . |
| Mismatch | `src-slfini.count` | CF-P2-03 | SOURCE | SLFINI | Count | 12 | 6 | Legacy 12 vs modern 6 |
| Mismatch | `src-slfini.percent` | CF-P2-03 | SOURCE | SLFINI | Percent | 13.3 | 24 | Legacy 13.3 vs modern 24 |
| Match | `src-slfini.n` | CF-S-00 | SOURCE | SLFINI | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-slfini.p25` | SS-SAL-02 | SOURCE | SLFINI | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-slfini.median` | SS-SAL-03 | SOURCE | SLFINI | Median | . | . | Legacy . vs modern . |
| Match | `src-slfini.p75` | SS-SAL-04 | SOURCE | SLFINI | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-slfini.mean` | SS-SAL-05 | SOURCE | SLFINI | Mean | . | . | Legacy . vs modern . |
| Mismatch | `src-zother.count` | CF-P2-03 | SOURCE | ZOTHER | Count | 39 | 2 | Legacy 39 vs modern 2 |
| Mismatch | `src-zother.percent` | CF-P2-03 | SOURCE | ZOTHER | Percent | 43.3 | 8 | Legacy 43.3 vs modern 8 |
| Match | `src-zother.n` | CF-S-00 | SOURCE | ZOTHER | SalaryN | . | . | Legacy . vs modern . |
| Match | `src-zother.p25` | SS-SAL-02 | SOURCE | ZOTHER | Pct25 | . | . | Legacy . vs modern . |
| Match | `src-zother.median` | SS-SAL-03 | SOURCE | ZOTHER | Median | . | . | Legacy . vs modern . |
| Match | `src-zother.p75` | SS-SAL-04 | SOURCE | ZOTHER | Pct75 | . | . | Legacy . vs modern . |
| Match | `src-zother.mean` | SS-SAL-05 | SOURCE | ZOTHER | Mean | . | . | Legacy . vs modern . |
| Mismatch | `source.subtotal-count` | SS-SUB-01 | SOURCE |  | SubtotalCount | 90 | 25 | Legacy 90 vs modern 25 |
| Match | `source.subtotal-percent` | SS-SUB-01 | SOURCE |  | SubtotalPercent | 100.0 | 100 | Legacy 100.0 vs modern 100 |
| Mismatch | `time-bgrad.count` | CF-P2-04 | TIME | BGRAD | Count | 49 | 18 | Legacy 49 vs modern 18 |
| Mismatch | `time-bgrad.percent` | CF-P2-04 | TIME | BGRAD | Percent | 61.3 | 62.1 | Legacy 61.3 vs modern 62.1 |
| Match | `time-bgrad.n` | CF-S-00 | TIME | BGRAD | SalaryN | . | . | Legacy . vs modern . |
| Match | `time-bgrad.p25` | SS-SAL-02 | TIME | BGRAD | Pct25 | . | . | Legacy . vs modern . |
| Match | `time-bgrad.median` | SS-SAL-03 | TIME | BGRAD | Median | . | . | Legacy . vs modern . |
| Match | `time-bgrad.p75` | SS-SAL-04 | TIME | BGRAD | Pct75 | . | . | Legacy . vs modern . |
| Match | `time-bgrad.mean` | SS-SAL-05 | TIME | BGRAD | Mean | . | . | Legacy . vs modern . |
| Mismatch | `time-zaftgrd.count` | CF-P2-04 | TIME | ZAFTGRD | Count | 31 | 11 | Legacy 31 vs modern 11; Builder stores ZAFTGRD; report $newvar lists ZAFTGR. |
| Mismatch | `time-zaftgrd.percent` | CF-P2-04 | TIME | ZAFTGRD | Percent | 38.8 | 37.9 | Legacy 38.8 vs modern 37.9; Builder stores ZAFTGRD; report $newvar lists ZAFTGR. |
| Match | `time-zaftgrd.n` | CF-S-00 | TIME | ZAFTGRD | SalaryN | . | . | Legacy . vs modern .; Builder stores ZAFTGRD; report $newvar lists ZAFTGR. |
| Match | `time-zaftgrd.p25` | SS-SAL-02 | TIME | ZAFTGRD | Pct25 | . | . | Legacy . vs modern .; Builder stores ZAFTGRD; report $newvar lists ZAFTGR. |
| Match | `time-zaftgrd.median` | SS-SAL-03 | TIME | ZAFTGRD | Median | . | . | Legacy . vs modern .; Builder stores ZAFTGRD; report $newvar lists ZAFTGR. |
| Match | `time-zaftgrd.p75` | SS-SAL-04 | TIME | ZAFTGRD | Pct75 | . | . | Legacy . vs modern .; Builder stores ZAFTGRD; report $newvar lists ZAFTGR. |
| Match | `time-zaftgrd.mean` | SS-SAL-05 | TIME | ZAFTGRD | Mean | . | . | Legacy . vs modern .; Builder stores ZAFTGRD; report $newvar lists ZAFTGR. |
| Mismatch | `time.subtotal-count` | SS-SUB-01 | TIME |  | SubtotalCount | 80 | 29 | Legacy 80 vs modern 29; Stored percents 61.3+38.8=100.1. |
| Mismatch | `time.subtotal-percent` | SS-SUB-01 | TIME |  | SubtotalPercent | 100.1 | 100.0 | Legacy 100.1 vs modern 100.0; Stored percents 61.3+38.8=100.1. |
| Mismatch | `status-notset.count` | CF-P2-05 | ZSTATUS | NOTSET | Count | 4 | 2 | Legacy 4 vs modern 2 |
| Mismatch | `status-notset.percent` | CF-P2-05 | ZSTATUS | NOTSET | Percent | 4.4 | 7.1 | Legacy 4.4 vs modern 7.1 |
| Match | `status-notset.n` | CF-S-00 | ZSTATUS | NOTSET | SalaryN | . | . | Legacy . vs modern . |
| Match | `status-notset.p25` | SS-SAL-02 | ZSTATUS | NOTSET | Pct25 | . | . | Legacy . vs modern . |
| Match | `status-notset.median` | SS-SAL-03 | ZSTATUS | NOTSET | Median | . | . | Legacy . vs modern . |
| Match | `status-notset.p75` | SS-SAL-04 | ZSTATUS | NOTSET | Pct75 | . | . | Legacy . vs modern . |
| Match | `status-notset.mean` | SS-SAL-05 | ZSTATUS | NOTSET | Mean | . | . | Legacy . vs modern . |
| Mismatch | `status-set.count` | CF-P2-05 | ZSTATUS | SET | Count | 86 | 26 | Legacy 86 vs modern 26 |
| Mismatch | `status-set.percent` | CF-P2-05 | ZSTATUS | SET | Percent | 95.6 | 92.9 | Legacy 95.6 vs modern 92.9 |
| Match | `status-set.n` | CF-S-00 | ZSTATUS | SET | SalaryN | . | . | Legacy . vs modern . |
| Match | `status-set.p25` | SS-SAL-02 | ZSTATUS | SET | Pct25 | . | . | Legacy . vs modern . |
| Match | `status-set.median` | SS-SAL-03 | ZSTATUS | SET | Median | . | . | Legacy . vs modern . |
| Match | `status-set.p75` | SS-SAL-04 | ZSTATUS | SET | Pct75 | . | . | Legacy . vs modern . |
| Match | `status-set.mean` | SS-SAL-05 | ZSTATUS | SET | Mean | . | . | Legacy . vs modern . |
| Mismatch | `status.subtotal-count` | SS-SUB-01 | ZSTATUS |  | SubtotalCount | 90 | 28 | Legacy 90 vs modern 28 |
| Match | `status.subtotal-percent` | SS-SUB-01 | ZSTATUS |  | SubtotalPercent | 100.0 | 100.0 | Legacy 100.0 vs modern 100.0 |
| Mismatch | `dur-total.perm` | CF-P2-01 | DURATION |  | Count | 85 | 28 | Legacy 85 vs modern 28; Compared to DurationCounts[PERM]. Long/short labels are not a characterized codebook. |
| Mismatch | `dur-total.temp` | CF-P2-01 | DURATION |  | Count | 2 | 1 | Legacy 2 vs modern 1; Compared to DurationCounts[TEMP]. Missing PDF '.' is expected null. |
| Mismatch | `dur-bus.perm` | CF-P2-01 | DURATION | BUS | Count | 10 | 1 | Legacy 10 vs modern 1; Compared to DurationCounts[PERM]. Long/short labels are not a characterized codebook. |
| Match | `dur-bus.temp` | CF-P2-01 | DURATION | BUS | Count | 1 | 1 | Legacy 1 vs modern 1; Compared to DurationCounts[TEMP]. Missing PDF '.' is expected null. |
| Mismatch | `dur-clerk.perm` | CF-P2-01 | DURATION | CLERK | Count | 5 | 4 | Legacy 5 vs modern 4; Compared to DurationCounts[PERM]. Long/short labels are not a characterized codebook. |
| Match | `dur-clerk.temp` | CF-P2-01 | DURATION | CLERK | Count | . | . | Legacy . vs modern .; Compared to DurationCounts[TEMP]. Missing PDF '.' is expected null. |
| Mismatch | `dur-firm.perm` | CF-P2-01 | DURATION | FIRM | Count | 50 | 20 | Legacy 50 vs modern 20; Compared to DurationCounts[PERM]. Long/short labels are not a characterized codebook. |
| Match | `dur-firm.temp` | CF-P2-01 | DURATION | FIRM | Count | . | . | Legacy . vs modern .; Compared to DurationCounts[TEMP]. Missing PDF '.' is expected null. |
| Mismatch | `dur-govt.perm` | CF-P2-01 | DURATION | GOVT | Count | 12 | 1 | Legacy 12 vs modern 1; Compared to DurationCounts[PERM]. Long/short labels are not a characterized codebook. |
| Mismatch | `dur-govt.temp` | CF-P2-01 | DURATION | GOVT | Count | 1 | . | Legacy 1 vs modern .; Compared to DurationCounts[TEMP]. Missing PDF '.' is expected null. |
| Mismatch | `dur-pi.perm` | CF-P2-01 | DURATION | PUBINT | Count | 8 | 2 | Legacy 8 vs modern 2; Compared to DurationCounts[PERM]. Long/short labels are not a characterized codebook. |
| Match | `dur-pi.temp` | CF-P2-01 | DURATION | PUBINT | Count | . | . | Legacy . vs modern .; Compared to DurationCounts[TEMP]. Missing PDF '.' is expected null. |
| Match | `funded.absent` | CF-P2-02 | LAW SCHOOL FUNDED | YES | Count | . | . | Legacy . vs modern .; Absent on the baseline PDF. |

## Explanations

1. The baseline PDF is a Test University Class of **2024** artifact. The sample export is a multi-school 2025-style file. No sample school has 100 graduates (largest observed is 73). School identity is unresolved, so numerical mismatches are expected until a matching graduate file exists.
2. Men and LJD 25th-percentile PDF text-layer values `850000` and `765000` are recorded as observed, not corrected to 85,000 / 76,500.
3. D3 printed subtotal 93 vs detail sum 94, LJD 79 vs 78, and JD Advantage salary n 12 vs count 10 stay documented PDF/SAS tensions. The calculator is not changed to hide them.
4. Clerkship printed labels State/Local are compared as `$newvar` `JCSTGV`/`JCTLOG` (`SS-FMT-04`). The sample export has no `emptype1` column, so modern E55 rows are expected to be missing.
5. Duration long-term/short-term is compared to `DurationCounts` keys `perm`/`PERM` and `temp`/`TEMP`. CF-P2-01 treats duration codes as data-driven column IDs; that mapping is a conservative reading of the report `perm`/`temp` columns, not a proven ERSS codebook.
6. Percents are compared at one decimal (SAS `6.1`). Salary money is compared at zero decimals (SAS `COMMA7.0`).
7. Of the 239 matches, most are missing salary cells or absent categories (both sides `.`). Those are not evidence that school `23306` is Test University.
8. Mismatches are listed in full above. None were dropped to make the test look green.
