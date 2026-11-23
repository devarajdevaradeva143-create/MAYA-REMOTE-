# Windows Setup

This guide covers setting up a development environment for the Windows component (`windows/`).

## Prerequisites

- **Windows 10 (1809+) or Windows 11.**
- **.NET SDK** (the projects target a modern .NET; install the latest 8.0/9.0 SDK, or the version specified once `.csproj` files are added).
- **Visual Studio 2022** (Community/Professional) with the **.NET desktop development (WPF)** workload, or the `dotnet` CLI plus Build Tools.
- Internet access **once** for NuGet restore (the app itself makes no cloud calls at runtime).

Verify:

```powershell
dotnet --info
```

Confirm the SDK version is listed and the WPF targeting packs are available (installed with the VS workload).

## Project layout

```
windows/
├─ src/
│  ├─ MayaRemote.App/              # WPF UI
│  ├─ MayaRemote.Core/             # contracts, Protocol, Security
│  ├─ MayaRemote.Networking/       # Discovery, Transport, Pairing, Authentication, Connection
│  ├─ MayaRemote.Infrastructure/   # Configuration, Logging, Storage
│  ├─ MayaRemote.WindowsServices/  # ScreenCapture, Input, Audio, ApplicationControl, Permissions
│  └─ MayaRemote.Agent/            # headless agent
└─ tests/
   ├─ MayaRemote.Core.Tests/
   ├─ MayaRemote.Networking.Tests/
   ├─ MayaRemote.Security.Tests/
   └─ MayaRemote.IntegrationTests/
```

> Current state: the directory skeleton and `src/MayaRemote.Core/Protocol/ProtocolMessage.cs` exist. `.csproj` files, a `.sln`, and the remaining sources must be added before the solution will build. The instructions below assume they are present.

## Create/attach the solution (when `.csproj` files exist)

```powershell
cd windows
dotnet new sln -n MayaRemote
dotnet sln add (Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { $_.FullName })
```

## Restore and build

```powershell
# from the repository root
.\scripts\build-windows.ps1
```

Or directly:

```powershell
cd windows
dotnet restore
dotnet build -c Debug
```

## Run

```powershell
cd windows
dotnet run --project src/MayaRemote.App
```

## Tests

```powershell
cd windows
dotnet test
```

See [testing guide](testing-guide.md) and `scripts/test-all.ps1`.

## Permissions

Input injection and screen capture may require:

- Running the app/agent with appropriate desktop permissions.
- Windows may prompt for accessibility/input permissions on first use; the `MayaRemote.WindowsServices/Permissions` layer checks and reports capability state.

## Firewall

The host listens on a local TCP port. Allow the app through Windows Defender Firewall for the local/private network so the Android device can connect. See [networking.md](../architecture/networking.md).

## Known setup issues

- **No `.sln`/`.csproj` yet:** create or add them (see above).
- **WPF workload missing:** install ".NET desktop development" in the VS Installer.
- **SDK mismatch:** align `TargetFramework` with the installed SDK.
- **Binding redirects / package restore:** run `dotnet restore` before build if NuGet errors appear.

## Related

- [Android Setup](setup-android.md)
- [Build Instructions](build-instructions.md)
- [Testing Guide](testing-guide.md)
- [Troubleshooting](troubleshooting.md)