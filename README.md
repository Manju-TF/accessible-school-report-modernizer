# Accessible School Report Modernizer

A .NET 8 capstone that imports graduate Excel data, applies characterized SAS report rules, and generates school summary PDFs. Files under `/legacy` are the immutable characterization baseline.

## Legacy integrity hook

`scripts/pre-commit.ps1` protects characterization integrity. It refuses a commit if staged changes touch:

- `legacy/sas/*`
- `legacy/samples/*`
- `legacy/baseline/*`

When those paths are staged, the script fails the commit and prints:

```text
Legacy baseline files are immutable.
```

It then runs `scripts/verify-legacy-integrity.ps1`, which checks every file under `/legacy` against the SHA-256 records in `docs/capstone/legacy-baseline.md`.

### Install

From the repository root, once per clone:

```powershell
$root = git rev-parse --show-toplevel
$hook = Join-Path $root ".git\hooks\pre-commit"
@'
#!/bin/sh
repo_root="$(git rev-parse --show-toplevel)"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$repo_root/scripts/pre-commit.ps1"
exit $?
'@ | Set-Content -Path $hook -Encoding ascii
```

Git for Windows runs `.git/hooks/pre-commit` through `sh`, which then calls the PowerShell script.

### Usage

After installation, `git commit` runs the hook automatically.

- A commit that stages any file under the protected `legacy` folders fails.
- A commit that leaves `/legacy` different from the baseline manifest also fails.
- Unstage or restore the legacy files, then commit again.

To run the same checks without committing:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/pre-commit.ps1
```

To check hashes only:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-legacy-integrity.ps1
```
