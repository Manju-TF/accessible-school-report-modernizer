[CmdletBinding()]
param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\artifacts\iis-deployment.zip')
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$projectPath = Join-Path $repositoryRoot 'src\AccessibleSchoolReports.Web\AccessibleSchoolReports.Web.csproj'
$runbookPath = Join-Path $repositoryRoot 'docs\deployment\iis.md'
if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    $resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
}
else {
    $resolvedOutputPath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputPath))
}
$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) (
    'AccessibleSchoolReports-IIS-' + [guid]::NewGuid().ToString('N'))
$publishPath = Join-Path $temporaryRoot 'publish'

try {
    New-Item -ItemType Directory -Path $publishPath -Force | Out-Null

    & dotnet publish $projectPath -c Release -r win-x64 --self-contained false -o $publishPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }

    Get-ChildItem -LiteralPath $publishPath -Filter 'appsettings.*.json' -File |
        Remove-Item -Force

    $settingsPath = Join-Path $publishPath 'appsettings.json'
    $settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
    $settings.ConnectionStrings.SchoolReports =
        'Data Source=C:\ProgramData\AccessibleSchoolReports\data\schoolreports.db'
    $settings.ReportGeneration.OutputRoot =
        'C:\ProgramData\AccessibleSchoolReports\output'
    [System.IO.File]::WriteAllText(
        $settingsPath,
        ($settings | ConvertTo-Json -Depth 30),
        [System.Text.UTF8Encoding]::new($false))

    Copy-Item -LiteralPath $runbookPath -Destination (Join-Path $publishPath 'IIS-DEPLOYMENT.md')
    $outputDirectory = Split-Path -Parent $resolvedOutputPath
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    Compress-Archive -Path (Join-Path $publishPath '*') `
        -DestinationPath $resolvedOutputPath `
        -CompressionLevel Optimal `
        -Force

    Write-Output "IIS deployment package created: $resolvedOutputPath"
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force
    }
}
