# Changelog

All notable changes to MAYA Remote are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Repository documentation and root files: `LICENSE` (MIT), `.gitignore`, `CONTRIBUTING.md`, `SECURITY.md`, `CHANGELOG.md`.
- Architecture documentation under `docs/architecture/` (overview, system design, Android and Windows architecture, networking, security model, threat model, pairing/connection/data flows).
- Protocol documentation under `docs/protocol/` (protocol v1, message types, authentication, error codes, compatibility).
- Development documentation under `docs/development/` (Android setup, Windows setup, build instructions, testing guide, troubleshooting).
- Build and maintenance PowerShell scripts under `scripts/` (`build-android.ps1`, `build-windows.ps1`, `test-all.ps1`, `clean.ps1`).

### Notes

- No runtime behavior changes in this release. Documentation and tooling only.

## [0.1.0] - Unreleased

### Added

- Project scaffold: Android app module (`android/app`) with Kotlin, Jetpack Compose, Hilt, DataStore, and Navigation.
- Windows solution skeleton (`windows/src`) with `MayaRemote.Core`, `.Networking`, `.Infrastructure`, `.WindowsServices`, `.Agent`, and `.App` (WPF) projects.
- Protocol definition v1 in `shared/protocol/protocol-spec.json`.
- Cross-platform protocol message codec:
  - Kotlin: `android/.../core/networking/ProtocolMessage.kt`
  - C#: `windows/src/MayaRemote.Core/Protocol/ProtocolMessage.cs`
- Connection state machine: `android/.../core/common/ConnectionState.kt`.
- Result/error type: `android/.../core/result/Result.kt`.

### Known limitations

- Windows solution projects, `.csproj` files, and the Android Gradle wrapper are not yet present; build instructions assume they are added. See `docs/development/build-instructions.md`.
- Only PING/PONG framing and basic message types are implemented end-to-end. Pairing, authentication, and screen/input features are planned.
- No cloud, no relay, no third-party service. All pairing and transport are local-network only.