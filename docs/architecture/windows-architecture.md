# Windows Architecture

The Windows component is a .NET solution written in C# with a WPF presentation layer. It acts as both the host that a phone connects to and the agent that performs screen capture, input injection, and audio.

## Projects

```
windows/
├─ src/
│  ├─ MayaRemote.App/              # WPF application (Views, ViewModels, Themes, Resources)
│  ├─ MayaRemote.Core/             # Contracts, models, services, Protocol, Security
│  ├─ MayaRemote.Networking/       # Discovery, Transport, Pairing, Authentication, Connection
│  ├─ MayaRemote.Infrastructure/   # Configuration, Logging, Storage
│  ├─ MayaRemote.WindowsServices/  # ScreenCapture, Input, Audio, ApplicationControl, Permissions
│  └─ MayaRemote.Agent/            # Headless agent
└─ tests/
   ├─ MayaRemote.Core.Tests/
   ├─ MayaRemote.Networking.Tests/
   ├─ MayaRemote.Security.Tests/
   └─ MayaRemote.IntegrationTests/
```

### MayaRemote.Core

Contracts and domain types. Contains:

- `Protocol/ProtocolMessage.cs` — the wire codec and `MessageTypes` constants.
- `Interfaces/` — service interfaces (networking, pairing, security, storage abstractions).
- `Models/` — domain models (device records, session state, messages).
- `Services/` — orchestration services.
- `Security/` — key handling, credential protection, verification primitives.

`Core` must not reference any other project. All other projects depend on it.

### MayaRemote.Networking

Transport and connection machinery:

- `Discovery/` — LAN discovery of Android peers.
- `Transport/` — TCP transport, framing, and message send/receive.
- `Pairing/` — pairing request/response handling.
- `Authentication/` — per-session authentication.
- `Connection/` — connection state machine and session lifetime.

### MayaRemote.Infrastructure

- `Configuration/` — application settings.
- `Logging/` — structured logging (never logs secrets).
- `Storage/` — paired-device records and key persistence.

### MayaRemote.WindowsServices

OS-facing capabilities, isolated behind interfaces in `Core`:

- `ScreenCapture/` — desktop frame capture.
- `Input/` — mouse and keyboard injection.
- `Audio/` — audio capture/playback.
- `ApplicationControl/` — foreground app targeting.
- `Permissions/` — Windows permission and capability checks.

### MayaRemote.App

WPF UI. `Views/` are XAML windows/pages; `ViewModels/` hold presentation state; `Themes/` and `Resources/` hold styles and assets. The UI thread owns view state; background work marshals results back via the dispatcher.

### MayaRemote.Agent

Headless host used when no window is required (e.g., session active without foreground UI). Shares `Core`, `Networking`, `Infrastructure`, and `WindowsServices`.

## Protocol codec

`MayaRemote.Core/Protocol/ProtocolMessage.cs`:

- `Encode()` writes a 15-byte header (1 version, 2 message type, 8 message id, 4 payload length) in big-endian, then payload.
- `Decode(ReadOnlySpan<byte>)` returns `null` on insufficient length, unsupported version, or oversized/negative length (max 1 MiB).
- `MessageTypes` matches `shared/protocol/protocol-spec.json`.

This mirrors `android/.../core/networking/ProtocolMessage.kt` byte-for-byte. Keep them in sync and validate against shared test vectors.

> Note: the `windows/` tree currently contains only `MayaRemote.Core/Protocol/ProtocolMessage.cs` plus the directory skeleton. `.csproj` files, solution file, and remaining sources are **not yet present**; build instructions assume they are added. See [setup-windows.md](../development/setup-windows.md).

## Threading

- UI thread: WPF rendering and view state only.
- Thread pool / dedicated workers: transport, discovery, screen capture, input injection.
- Results marshaled to the UI thread via `Dispatcher`.
- No blocking calls on the UI thread.

## Configuration and storage

- `Infrastructure/Configuration` loads settings (listen address/port, discovery options).
- `Infrastructure/Storage` persists paired-device records and the long-term credential.
- Secrets are stored locally and never written to logs.

## Security

- Keys and pairing records are protected using Windows DPAPI or equivalent platform protection in `Core/Security`.
- See [Security Model](security-model.md) and [Security (protocol)](../protocol/authentication.md).

## Build and test

- Build: `scripts/build-windows.ps1` (`dotnet build`).
- Tests: `scripts/test-all.ps1` runs the four test projects.
- Prerequisites: .NET SDK. See [build instructions](../development/build-instructions.md).