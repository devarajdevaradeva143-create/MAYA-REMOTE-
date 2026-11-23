<#
.SYNOPSIS
    Builds the MAYA Remote Android application.
.DESCRIPTION
    Runs the Gradle build for the :app module. Ensures a Gradle wrapper is
    present (generating a pointer if missing), then assembles debug and,
    optionally, release APKs.
.PARAMETER Release
    Build a release APK in addition to the debug APK.
.EXAMPLE
    .\scripts\build-android.ps1
    .\scripts\build-android.ps1 -Release
#>
[CmdletBinding()]
param(
    [switch]$Release
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$android = Join-Path $root 'android'
$gradlew = Join-Path $android 'gradlew.bat'

Write-Host '== MAYA Remote Android build ==' -ForegroundColor Cyan

if (-not (Test-Path -LiteralPath $android)) {
    Write-Error "Android directory not found: $android"
}

if (-not (Test-Path -LiteralPath $gradlew)) {
    Write-Warning 'Gradle wrapper (gradlew.bat) not found.'
    Write-Warning 'Generate it once, for example:'
    Write-Warning '    cd android'
    Write-Warning '    gradle wrapper --gradle-version 8.7'
    Write-Warning 'or open the android/ folder in Android Studio and let it sync.'
    throw "Gradle wrapper missing: $gradlew"
}

Push-Location $android
try {
    Write-Host '-- assembleDebug --' -ForegroundColor Yellow
    & $gradlew ':app:assembleDebug'
    if ($LASTEXITCODE -ne 0) { throw "assembleDebug failed with exit code $LASTEXITCODE" }

    if ($Release) {
        Write-Host '-- assembleRelease --' -ForegroundColor Yellow
        & $gradlew ':app:assembleRelease'
        if ($LASTEXITCODE -ne 0) { throw "assembleRelease failed with exit code $LASTEXITCODE" }
        Write-Host "Release APK: $(Join-Path $android 'app\build\outputs\apk\release\app-release.apk')"
    }

    Write-Host "Debug APK:   $(Join-Path $android 'app\build\outputs\apk\debug\app-debug.apk')" -ForegroundColor Green
    Write-Host 'Android build complete.' -ForegroundColor Green
}
finally {
    Pop-Location
}