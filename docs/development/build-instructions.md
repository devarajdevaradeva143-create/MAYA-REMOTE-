# Build Instructions

This document describes how to build both components of MAYA Remote from a clean checkout. It assumes the prerequisites in the setup guides are met.

## Prerequisites (summary)

- **Android:** JDK 17, Android Studio with SDK Platform 34 + Build-Tools, Gradle wrapper (generated — see [setup-android.md](setup-android.md)).
- **Windows:** Windows 10/11, .NET SDK, WPF workload (Visual Studio 2022) or `dotnet` CLI.
- **PowerShell** (Windows) for the scripts under `scripts/`.

No cloud, relay, or third-party service is used at build time beyond normal package/dependency resolution (Maven Central, Google Maven, NuGet). The application itself performs no cloud communication.

## Build Android

From the repository root:

```powershell
.\scripts\build-android.ps1
```

What the script does:

1. Ensures a Gradle wrapper exists (`gradlew.bat`); if missing, it directs you to generate one.
2. Runs `gradlew :app:assembleDebug` (and `assembleRelease` when configured).
3. Places the APK under `android/app/build/outputs/apk/`.

Manual equivalent:

```powershell
cd android
.\gradlew.bat :app:assembleDebug
```

Output: `android/app/build/outputs/apk/debug/app-debug.apk`.

## Build Windows

From the repository root:

```powershell
.\scripts\build-windows.ps1
```

What the script does:

1. Runs `dotnet restore`.
2. Runs `dotnet build -c Debug` (and `-c Release` when configured).
3. Reports output paths under `windows/src/**/bin/`.

Manual equivalent:

```powershell
cd windows
dotnet restore
dotnet build -c Debug
```

> Requires `.sln`/`.csproj` files. See [setup-windows.md](setup-windows.md) if they are not yet present.

## Build everything

```powershell
.\scripts\build-android.ps1
if ($?) { .\scripts\build-windows.ps1 }
```

## Release builds

- Android release: `.\scripts\build-android.ps1 -Release` (signing config required; see setup-android).
- Windows release: `.\scripts\build-windows.ps1 -Release`.

## Run tests

```powershell
.\scripts\test-all.ps1
```

See [testing guide](testing-guide.md).

## Clean

```powershell
.\scripts\clean.ps1
```

Removes Android `build/` outputs and Windows `bin/`/`obj/`.

## Troubleshooting

See [troubleshooting.md](troubleshooting.md) for common build errors (missing wrapper, missing `.sln`, JDK/SDK mismatches, NuGet restore failures).

## Script reference

| Script | Purpose |
|--------|---------|
| `scripts/build-android.ps1` | Build the Android app (Gradle). |
| `scripts/build-windows.ps1` | Build the Windows solution (`dotnet`). |
| `scripts/test-all.ps1` | Run Android + Windows tests. |
| `scripts/clean.ps1` | Delete build outputs. |