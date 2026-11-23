# System Design

This document describes how the MAYA Remote components are structured, what each layer does, and how work is scheduled. It complements the protocol and flow documents.

## Component model

There are two deployable components and one shared specification.

### Android client (`android/`)

Layered by feature and core capability, following a clean architecture split:

- **`ui/`** — Jetpack Compose screens (`ui/screens/`), theme (`ui/theme/`, `ui/typography/`), icons (`ui/icons/`), and reusable components (`ui/components/`).
- **`feature/`** — one package per feature: `onboarding`, `pairing`, `devices`, `connection`, `remote`, `screenviewer`, `security`, `settings`, `profile`. Each feature owns its screen(s), ViewModel, and UI state.
- **`domain/`** — framework-free models (`domain/model/`), repository interfaces (`domain/repository/`), and use cases (`domain/usecase/`).
- **`data/`** — repository implementations (`data/repository/`), local persistence (`data/local/`, `data/preferences/` via DataStore), and device records (`data/device/`).
- **`service/`** — long-running work: `service/discovery/` (LAN discovery), `service/connection/` (transport/session), `service/foreground/` (foreground service that keeps a session alive).
- **`core/`** — cross-cutting primitives: `core/common/` (`ConnectionState`), `core/networking/` (`ProtocolMessage`), `core/result/` (`Result`, `MayaError`), `core/security/`, `core/permissions/`, `core/dispatchers/`, `core/logging/`.
- **`di/`** — Hilt modules wiring the graph. **`navigation/`** — route definitions (`Routes.kt`).

### Windows host (`windows/`)

A .NET solution split into projects:

- **`MayaRemote.App`** — WPF application: `Views/`, `ViewModels/`, `Themes/`, `Resources/`. Presentation layer only.
- **`MayaRemote.Core`** — contracts and domain: `Interfaces/`, `Models/`, `Services/`, `Protocol/` (`ProtocolMessage.cs`), `Security/`.
- **`MayaRemote.Networking`** — `Discovery/`, `Transport/`, `Pairing/`, `Authentication/`, `Connection/`.
- **`MayaRemote.Infrastructure`** — `Configuration/`, `Logging/`, `Storage/`.
- **`MayaRemote.WindowsServices`** — OS-facing capabilities: `ScreenCapture/`, `Input/`, `Audio/`, `ApplicationControl/`, `Permissions/`.
- **`MayaRemote.Agent`** — headless/host-side agent used when the app is not in the foreground.

### Shared specification (`shared/`)

- **`protocol/protocol-spec.json`** — the single source of truth for framing, message types, connection states, and error codes.
- **`protocol/schemas/`, `protocol/examples/`, `protocol/test-vectors/`** — schema artifacts and cross-implementation vectors.
- **`security/`** — security requirements and key/pairing parameters.

## Layering and dependency rules

- `ui` → `feature` → `domain` ← `data`.
- `domain` depends on nothing Android- or WPF-specific.
- `data` implements `domain` interfaces.
- `service`/`networking` call into `domain` abstractions; concrete transports live in `data`/`networking`.
- `Core` never references `App`, `Networking`, `Infrastructure`, or `WindowsServices`. The dependency arrow points from outer layers to `Core`.
- The protocol codec is duplicated per platform but validated against the same schema and test vectors. Do not share binaries between platforms.

## Threading and lifecycle

### Android

- UI runs on the main thread via Compose; ViewModel exposes immutable UI state.
- Network I/O and pairing run on `Dispatchers.IO`; CPU work on `Dispatchers.Default`.
- Long-lived sessions run in a foreground service (`service/foreground/`) with a user-visible notification, so connection survives app backgrounding.
- Discovery and connection are suspend-based; cancellation propagates through structured concurrency.
- No blocking calls on the main thread.

### Windows

- WPF UI thread owns all view state; background services run on the thread pool.
- Long-running connection/session work is hosted off the UI thread and marshals results back to the UI via the dispatcher.
- Screen capture and input injection are performed on dedicated workers to avoid stalling the UI.
- The agent can run without a visible window.

## Error and result handling

- Android models outcomes with `Result<T>` (`Success`/`Failure`) and structured `MayaError(code, message, technicalDetail)`. User-facing messages are mapped from protocol error codes.
- Windows uses explicit result/error types surfaced through `Core` services and rendered in the UI.
- Both platforms map wire error codes to stable user-facing messages (see `../protocol/error-codes.md`).

## Configuration and storage

- Android: DataStore Preferences for settings; device records persisted locally; no cloud backup of records.
- Windows: `Infrastructure/Configuration` for settings; `Infrastructure/Storage` for paired-device records and keys.
- Both store the long-term pairing credential and paired-device list locally and treat them as secrets.

## Security boundary

The trust boundary is the pair of paired devices on the local network. All crossing of that boundary (discovery announcements, pairing, session setup, and each message) is defined in [Security Model](security-model.md) and constrained by the [Threat Model](threat-model.md).

## Build and test

- Android builds via Gradle (`scripts/build-android.ps1`).
- Windows builds via `dotnet` (`scripts/build-windows.ps1`).
- Combined test run via `scripts/test-all.ps1`.
- Clean via `scripts/clean.ps1`.

See [Development docs](../development/build-instructions.md) for prerequisites.