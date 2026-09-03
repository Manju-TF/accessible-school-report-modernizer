# Legacy Baseline Manifest

This is the integrity record for every file under `/legacy`.

These artifacts are **IMMUTABLE**. Do not modify, reformat, rename, or overwrite them.
If a working copy is needed, copy into `src/`, `data/`, or `evidence/`.

Verification: `scripts/verify-legacy-integrity.ps1`

Algorithm: SHA-256

---

## File records

### `legacy/sas/createschrptfiles2025.sas`

- relative path: `legacy/sas/createschrptfiles2025.sas`
- file purpose: Original SAS program that builds school-level counts, salary information, and combined 2025 school report files from ERSS Class of 2025 submission data.
- SHA-256: `f17bbce1d067818645b8d995487357745370dca72a6e26ea9cbf85c74657d839`
- status: IMMUTABLE

### `legacy/sas/schreptsummary_2025.sas`

- relative path: `legacy/sas/schreptsummary_2025.sas`
- file purpose: Original SAS program that formats and produces the 2025 school report summary, including employment categories, demographics, job location, region, and firm size.
- SHA-256: `33799aa471f227e5d4960d8f2d3c7cb8c1ef7c6303e94907922d1e1f830e5645`
- status: IMMUTABLE

### `legacy/samples/sample-export.xlsx`

- relative path: `legacy/samples/sample-export.xlsx`
- file purpose: Original Excel sample export used as the baseline spreadsheet artifact for the school reporting workflow.
- SHA-256: `e6d116fb1127be05995857f55d8fa7bb26bd31ffebda7f61ce03bf9956877426`
- status: IMMUTABLE

### `legacy/baseline/test-school-report.pdf`

- relative path: `legacy/baseline/test-school-report.pdf`
- file purpose: Baseline PDF school report used to compare modernized output. This file is a preserved legacy artifact, not a newly generated accessible PDF.
- SHA-256: `f27b3b52258aa4b23e2216b8ec4077ce979677897553b3b142a468d9e21cb15b`
- status: IMMUTABLE

### `legacy/sas/.gitkeep`

- relative path: `legacy/sas/.gitkeep`
- file purpose: Empty placeholder that keeps the `legacy/sas` directory in version control.
- SHA-256: `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`
- status: IMMUTABLE

### `legacy/baseline/.gitkeep`

- relative path: `legacy/baseline/.gitkeep`
- file purpose: Empty placeholder that keeps the `legacy/baseline` directory in version control.
- SHA-256: `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`
- status: IMMUTABLE
