param(
    [string]$Configuration = "Release"
)

<# 
    Simple local smoke check:
    1) Builds Halcyon.sln with the given configuration.
    2) Exits non-zero on build failure.
    Run from anywhere: powershell -ExecutionPolicy Bypass -File scripts/local-smoke.ps1
    Note: Does not start the simulator; use docs/inventory-smoke.md for manual viewer checks.
#>

$repoRoot = Split-Path -Path $PSScriptRoot -Parent
Set-Location -Path $repoRoot

$msbuild = Get-Command msbuild.exe -ErrorAction SilentlyContinue
if (-not $msbuild) {
    Write-Error "msbuild.exe not found in PATH. Install Visual Studio Build Tools or add msbuild to PATH."
    exit 1
}

Write-Host "Running msbuild on Halcyon.sln (Configuration=$Configuration)..." -ForegroundColor Cyan
& $msbuild.Path "Halcyon.sln" "/m" "/p:Configuration=$Configuration" "/p:Platform=Any CPU"
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Build succeeded." -ForegroundColor Green
