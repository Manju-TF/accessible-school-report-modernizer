#Requires -Version 5.1
<#
.SYNOPSIS
  Verifies that every file under /legacy still matches the baseline manifest.

.DESCRIPTION
  Reads SHA-256 hashes from docs/capstone/legacy-baseline.md, hashes the
  current /legacy tree, and fails if any file is missing, added, or changed.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $repoRoot 'docs\capstone\legacy-baseline.md'
$legacyRoot = Join-Path $repoRoot 'legacy'

if (-not (Test-Path -LiteralPath $manifestPath)) {
    Write-Error "Baseline manifest not found: $manifestPath"
    exit 1
}

if (-not (Test-Path -LiteralPath $legacyRoot)) {
    Write-Error "Legacy directory not found: $legacyRoot"
    exit 1
}

function ConvertTo-RelativeUnixPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FullPath,
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    $rootPrefix = $Root.TrimEnd('\', '/')
    $relative = $FullPath.Substring($rootPrefix.Length).TrimStart('\', '/')
    return ($relative -replace '\\', '/')
}

$manifestText = Get-Content -LiteralPath $manifestPath -Raw
$expected = @{}

$pathMatches = [regex]::Matches($manifestText, '(?im)^-\s*relative path:\s*`([^`]+)`\s*$')
$hashMatches = [regex]::Matches($manifestText, '(?im)^-\s*SHA-256:\s*`([0-9a-f]{64})`\s*$')

if ($pathMatches.Count -eq 0 -or $pathMatches.Count -ne $hashMatches.Count) {
    Write-Error "Manifest parse failed. Found $($pathMatches.Count) paths and $($hashMatches.Count) hashes."
    exit 1
}

for ($i = 0; $i -lt $pathMatches.Count; $i++) {
    $rel = $pathMatches[$i].Groups[1].Value.Trim() -replace '\\', '/'
    $hash = $hashMatches[$i].Groups[1].Value.ToLowerInvariant()
    if ($expected.ContainsKey($rel)) {
        Write-Error "Duplicate path in manifest: $rel"
        exit 1
    }
    $expected[$rel] = $hash
}

$actual = @{}
Get-ChildItem -LiteralPath $legacyRoot -Recurse -File -Force | ForEach-Object {
    $rel = ConvertTo-RelativeUnixPath -FullPath $_.FullName -Root $repoRoot
    $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    $actual[$rel] = $hash
}

$failures = New-Object System.Collections.Generic.List[string]

foreach ($path in ($expected.Keys | Sort-Object)) {
    if (-not $actual.ContainsKey($path)) {
        $failures.Add("MISSING  $path")
        continue
    }

    if ($actual[$path] -ne $expected[$path]) {
        $failures.Add("CHANGED  $path")
        $failures.Add("         expected $($expected[$path])")
        $failures.Add("         actual   $($actual[$path])")
    }
}

foreach ($path in ($actual.Keys | Sort-Object)) {
    if (-not $expected.ContainsKey($path)) {
        $failures.Add("ADDED    $path")
    }
}

Write-Host "Legacy integrity check"
Write-Host "Manifest: $manifestPath"
Write-Host "Expected files: $($expected.Count)"
Write-Host "Actual files:   $($actual.Count)"

if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "FAILED: /legacy is immutable and has changed."
    $failures | ForEach-Object { Write-Host $_ }
    exit 1
}

Write-Host "OK: all $($expected.Count) legacy files match the IMMUTABLE baseline."
exit 0
