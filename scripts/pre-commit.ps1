#Requires -Version 5.1
<#
.SYNOPSIS
  Blocks commits that change immutable legacy baseline files.

.DESCRIPTION
  Fails if staged paths fall under legacy/sas, legacy/samples, or
  legacy/baseline, then runs scripts/verify-legacy-integrity.ps1 so the
  working tree still matches the characterization manifest.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $repoRoot

$protectedPrefixes = @(
    'legacy/sas/',
    'legacy/samples/',
    'legacy/baseline/'
)

function Test-ProtectedLegacyPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $normalized = ($Path -replace '\\', '/').TrimStart('./')
    foreach ($prefix in $protectedPrefixes) {
        $directory = $prefix.TrimEnd('/')
        if ($normalized.Equals($directory, [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }

        if ($normalized.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

$stagedPaths = New-Object System.Collections.Generic.List[string]
$rawStatus = @(git diff --cached --name-status --diff-filter=ACDMR)
foreach ($line in $rawStatus) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    $parts = $line -split '\t'
    for ($i = 1; $i -lt $parts.Count; $i++) {
        if (-not [string]::IsNullOrWhiteSpace($parts[$i])) {
            $stagedPaths.Add($parts[$i].Trim())
        }
    }
}

$blocked = @(
    $stagedPaths |
        Where-Object { Test-ProtectedLegacyPath -Path $_ } |
        Select-Object -Unique
)

$failed = $false

if ($blocked.Count -gt 0) {
    Write-Host 'Legacy baseline files are immutable.'
    $failed = $true
}

$verifyScript = Join-Path $repoRoot 'scripts\verify-legacy-integrity.ps1'
if (-not (Test-Path -LiteralPath $verifyScript)) {
    Write-Error "Legacy integrity script not found: $verifyScript"
    exit 1
}

$verify = Start-Process -FilePath (Join-Path $PSHOME 'powershell.exe') `
    -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $verifyScript) `
    -WorkingDirectory $repoRoot `
    -Wait `
    -PassThru `
    -NoNewWindow

if ($verify.ExitCode -ne 0) {
    $failed = $true
}

if ($failed) {
    exit 1
}

exit 0
