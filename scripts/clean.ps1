<#
.SYNOPSIS
    Cleans MAYA Remote build outputs.
.DESCRIPTION
    Removes Android Gradle build directories and Windows bin/obj folders.
.PARAMETER All
    Also remove the Gradle wrapper caches under android/.gradle and any
    leftover test result folders.
.EXAMPLE
    .\scripts\clean.ps1
    .\scripts\clean.ps1 -All
#>
[CmdletBinding()]
param(
    [switch]$All
)

$ErrorActionPreference = 'Continue'

$root = Split-Path -Parent $PSScriptRoot

function Remove-Tree {
    param([string]$Path)
    if (Test-Path -LiteralPath $Path) {
        Write-Host "Removing $Path" -ForegroundColor Yellow
        Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host '== MAYA Remote clean ==' -ForegroundColor Cyan

# Android build outputs
$androidBuild = Join-Path $root 'android\app\build'
Remove-Tree $androidBuild
$androidBuildRoot = Join-Path $root 'android\build'
Remove-Tree $androidBuildRoot

# Windows bin/obj
Get-ChildItem -LiteralPath (Join-Path $root 'windows') -Recurse -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in @('bin', 'obj') } |
    ForEach-Object { Remove-Tree $_.FullName }

# Test results
Get-ChildItem -LiteralPath $root -Recurse -Directory -Filter 'TestResults' -ErrorAction SilentlyContinue |
    ForEach-Object { Remove-Tree $_.FullName }

if ($All) {
    Remove-Tree (Join-Path $root 'android\.gradle')
    Write-Host 'Removed Gradle project cache (android/.gradle).' -ForegroundColor Yellow
}

Write-Host 'Clean complete.' -ForegroundColor Green