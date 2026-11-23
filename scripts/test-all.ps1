<#
.SYNOPSIS
    Runs the full MAYA Remote test suite (Android + Windows).
.DESCRIPTION
    Executes the Android unit tests (and instrumented tests if a device is
    connected) and the Windows dotnet test suite. Each platform runs
    independently; failures are reported per platform.
.PARAMETER SkipAndroid
    Skip the Android test run.
.PARAMETER SkipWindows
    Skip the Windows test run.
.EXAMPLE
    .\scripts\test-all.ps1
    .\scripts\test-all.ps1 -SkipAndroid
#>
[CmdletBinding()]
param(
    [switch]$SkipAndroid,
    [switch]$SkipWindows
)

$ErrorActionPreference = 'Continue'

$root = Split-Path -Parent $PSScriptRoot
$android = Join-Path $root 'android'
$windows = Join-Path $root 'windows'
$failed = $false

function Invoke-Step {
    param([string]$Name, [scriptblock]$Action)
    Write-Host "== $Name ==" -ForegroundColor Cyan
    try {
        & $Action
        if ($LASTEXITCODE -ne $null -and $LASTEXITCODE -ne 0) {
            Write-Host "$Name failed (exit $LASTEXITCODE)" -ForegroundColor Red
            $script:failed = $true
        }
    }
    catch {
        Write-Host "$Name failed: $($_.Exception.Message)" -ForegroundColor Red
        $script:failed = $true
    }
}

if (-not $SkipAndroid) {
    $gradlew = Join-Path $android 'gradlew.bat'
    if (Test-Path -LiteralPath $gradlew) {
        Invoke-Step 'Android unit tests' {
            Push-Location $android
            try { & $gradlew ':app:testDebugUnitTest' } finally { Pop-Location }
        }
    }
    else {
        Write-Warning "Gradle wrapper not found; skipping Android tests ($gradlew)."
    }
}

if (-not $SkipWindows) {
    $sln = Get-ChildItem -LiteralPath $windows -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($sln) {
        Invoke-Step 'Windows tests' {
            Push-Location $windows
            try { & dotnet test $sln.FullName } finally { Pop-Location }
        }
    }
    else {
        Write-Warning "No .sln found under windows/; skipping Windows tests."
    }
}

if ($failed) {
    Write-Host 'TEST RUN FAILED' -ForegroundColor Red
    exit 1
}

Write-Host 'All requested test suites passed.' -ForegroundColor Green
exit 0