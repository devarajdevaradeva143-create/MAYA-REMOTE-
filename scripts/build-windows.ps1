<#
.SYNOPSIS
    Builds the MAYA Remote Windows solution.
.DESCRIPTION
    Restores NuGet packages and builds the Windows solution with dotnet.
    Requires a .sln (and .csproj files) under windows/.
.PARAMETER Release
    Build the Release configuration in addition to Debug.
.EXAMPLE
    .\scripts\build-windows.ps1
    .\scripts\build-windows.ps1 -Release
#>
[CmdletBinding()]
param(
    [switch]$Release
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$windows = Join-Path $root 'windows'

Write-Host '== MAYA Remote Windows build ==' -ForegroundColor Cyan

if (-not (Test-Path -LiteralPath $windows)) {
    Write-Error "Windows directory not found: $windows"
}

$sln = Get-ChildItem -LiteralPath $windows -Filter *.sln -File | Select-Object -First 1
if (-not $sln) {
    Write-Warning 'No .sln found under windows/.'
    Write-Warning 'Create/attach projects first, for example:'
    Write-Warning '    cd windows'
    Write-Warning '    dotnet new sln -n MayaRemote'
    Write-Warning '    dotnet sln add (Get-ChildItem -Recurse -Filter *.csproj | %% { $_.FullName })'
    throw "No solution file found under: $windows"
}

Push-Location $windows
try {
    Write-Host '-- restore --' -ForegroundColor Yellow
    & dotnet restore $sln.FullName
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }

    Write-Host '-- build (Debug) --' -ForegroundColor Yellow
    & dotnet build $sln.FullName -c Debug --no-restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet build (Debug) failed with exit code $LASTEXITCODE" }

    if ($Release) {
        Write-Host '-- build (Release) --' -ForegroundColor Yellow
        & dotnet build $sln.FullName -c Release --no-restore
        if ($LASTEXITCODE -ne 0) { throw "dotnet build (Release) failed with exit code $LASTEXITCODE" }
    }

    Write-Host 'Windows build complete.' -ForegroundColor Green
}
finally {
    Pop-Location
}