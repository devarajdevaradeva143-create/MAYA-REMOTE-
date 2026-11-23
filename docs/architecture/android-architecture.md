# Android Architecture

The Android client is a Kotlin application using Jetpack Compose, Hilt for dependency injection, DataStore for preferences, and Navigation-Compose for routing. It targets `minSdk 26` and `compileSdk 34`, with Java 17 source/target compatibility.

## Module

Single Gradle module: `:app` (namespace and `applicationId` `com.maya.remote`).

Key build facts (from `android/app/build.gradle.kts`):

- Plugins: `com.android.application`, `org.jetbrains.kotlin.android`, `org.jetbrains.kotlin.plugin.compose`, `com.google.dagger.hilt.android`, `kotlin-kapt`.
- Compose BOM `2024.06.00`, Material 3, Navigation-Compose `2.7.7`.
- Hilt `2.51`, `androidx.hilt:hilt-navigation-compose`.
- Coroutines `1.8.1`, DataStore Preferences `1.1.1`.
- Testing: JUnit 4 and `kotlinx-coroutines-test`.

> Note: `android/build.gradle.kts`, `gradle.properties`, and `settings.gradle.kts` exist at the `android/` root. The Gradle wrapper (`gradlew`, `gradle/wrapper/`) is **not** yet present; see [build instructions](../development/build-instructions.md) for how to generate it.

## Package layout

```
com.maya.remote
├─ MayaRemoteApplication.kt      # @HiltAndroidApp
├─ navigation/                   # Routes.kt
├─ ui/                           # Compose: screens/, components/, icons/, theme/, typography/
├─ feature/                      # onboarding, pairing, devices, connection,
│                                # remote, screenviewer, security, settings, profile
├─ domain/                       # model/, repository/ (interfaces), usecase/
├─ data/                         # repository/, local/, preferences/, device/
├─ service/                      # discovery/, connection/, foreground/
├─ di/                           # Hilt modules
└─ core/
   ├─ common/                    # ConnectionState
   ├─ networking/                # ProtocolMessage
   ├─ result/                    # Result, MayaError
   ├─ security/
   ├─ permissions/
   ├─ dispatchers/
   └─ logging/
```

## Application entry

`MayaRemoteApplication` is annotated with `@HiltAndroidApp`, which generates the Hilt component for the app. `AndroidManifest.xml` declares:

- `INTERNET` and `ACCESS_NETWORK_STATE` permissions (local network transport and connectivity checks only — no cloud calls).
- A single `MainActivity` with `MAIN`/`LAUNCHER` intent filter.
- `android:allowBackup="true"` is set in the manifest; **review before release**, because device pairing records should not be silently included in cloud backups.

> Manifest note: the current manifest declares `.MainActivity` but no `MainActivity.kt` exists yet; it must be added for the app to build. See troubleshooting in [setup-android.md](../development/setup-android.md).

## Dependency injection (Hilt)

- Hilt is applied via the `com.google.dagger.hilt.android` plugin and `kapt` for the compiler.
- Modules under `di/` provide: dispatchers, repositories, data sources, security primitives, and network/transport dependencies.
- ViewModels are `@HiltViewModel` and receive use cases/repositories via constructor injection.
- `hilt-navigation-compose` supplies `hiltViewModel()` for Compose screens.

## State management

- Each feature exposes a single immutable UI state through a ViewModel.
- `ConnectionState` (`core/common/ConnectionState.kt`) is the canonical connection lifecycle enum:

  `DISCONNECTED, DISCOVERING, PAIRING, AUTHENTICATING, CONNECTING, CONNECTED, RECONNECTING, FAILED, REVOKED`

  with an explicit `canTransitionTo(next)` guard. Use this guard in the connection service; never set state directly from UI.

- Outcomes use `Result<T>` with `MayaError` for failures; map to user-facing text centrally.

## Networking and protocol

- `ProtocolMessage.kt` is the wire codec: big-endian header (1 byte version, 2 bytes message type, 8 bytes message id, 4 bytes payload length) followed by payload. Max payload 1 MiB. Version must equal `PROTOCOL_VERSION` (1) or decode returns `null`.
- `MessageTypes` constants match `shared/protocol/protocol-spec.json`.
- Transport and framing run on `Dispatchers.IO` inside the foreground service.

## Navigation

- `navigation/Routes.kt` defines routes: `splash`, `welcome`, `profile_setup`, `home`, `pair`, `pair_qr`, `enter_code`, `device_approval`, `connected_devices`, `connection_status`, `remote_mouse`, `remote_keyboard`, `screen_viewer`, `security_settings`, `app_settings`.
- Screens are declared in `ui/screens/Screens.kt` (currently placeholder composables; wire them to `Routes` and ViewModels).

## Services and foreground execution

- `service/discovery/` — LAN discovery of Windows peers.
- `service/connection/` — transport session management and message dispatch.
- `service/foreground/` — foreground service that holds an active session and posts a notification.

## Security

- Keys and pairing records live in `core/security/` and `data/`, protected by Android Keystore where available. Never log keys, pairing codes, or payloads.
- See [Security Model](security-model.md) and [Security (protocol)](../protocol/authentication.md).

## Testing

- Unit tests: `android/app/src/test/`.
- Instrumented tests: `android/app/src/androidTest/`.
- Run with `scripts/build-android.ps1` (and the connected test task); see [testing guide](../development/testing-guide.md).