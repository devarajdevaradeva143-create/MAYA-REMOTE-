# Contributing to MAYA Remote

MAYA Remote is a personal, local-first remote control application. Contributions are welcome, but the project has a small, deliberate scope: **no cloud, no relay, no third-party service**. Please keep changes consistent with that scope.

## Ground rules

- **Local-only.** Do not introduce cloud sync, telemetry, analytics, crash-reporting uploads, remote relays, or any third-party network dependency. Pairing, authentication, and transport happen entirely on the local network.
- **Security first.** Every change touching `shared/security/`, authentication, pairing, or transport requires a design note before a pull request. See `docs/architecture/security-model.md` and `docs/architecture/threat-model.md`.
- **No secrets.** Never commit keys, keystores, `.jks`, `.pfx`, `.pem`, tokens, or pairing codes. `.gitignore` blocks common cases; verify with `git status` before committing.
- **Match existing style.** Kotlin uses the official Kotlin code style; C# follows the conventions in the surrounding `.cs` files. Do not add comments unless they explain non-obvious intent.

## Repository layout

| Path | Contents |
|------|----------|
| `android/` | Android client (Kotlin, Jetpack Compose) |
| `windows/` | Windows host/agent (C#, WPF, .NET) |
| `shared/` | Cross-platform protocol schemas, examples, test vectors, security requirements |
| `docs/` | Architecture, protocol, and development documentation |
| `scripts/` | Build, test, and clean PowerShell scripts |

## Getting started

1. Read `docs/architecture/overview.md` and `docs/protocol/protocol-v1.md`.
2. Set up a build environment following `docs/development/setup-android.md` and `docs/development/setup-windows.md`.
3. Build both components with `scripts/build-android.ps1` and `scripts/build-windows.ps1`.
4. Run tests with `scripts/test-all.ps1`.

## Making a change

1. Create a branch: `feature/<short-description>` or `fix/<short-description>`.
2. Make focused commits with clear messages. Reference the milestone or issue where relevant.
3. Run the relevant build and tests locally:
   - Android: `scripts/build-android.ps1`
   - Windows: `scripts/build-windows.ps1`
   - Full test suite: `scripts/test-all.ps1`
4. Open a pull request describing **what changed, why, and how it was tested**. Include screenshots for UI changes on either platform.

## Protocol changes

The protocol is defined once in `shared/protocol/protocol-spec.json` and implemented independently in:

- `android/app/src/main/java/com/maya/remote/core/networking/ProtocolMessage.kt`
- `windows/src/MayaRemote.Core/Protocol/ProtocolMessage.cs`

Any change to framing, message types, error codes, or version rules must update the schema, both implementations, and `docs/protocol/protocol-v1.md` together. Never bump the protocol version for a backward-compatible addition; use the compatibility rules in `docs/protocol/compatibility.md`.

## Commit and PR hygiene

- One logical change per commit.
- Do not mix formatting churn with behavioral changes.
- Ensure `scripts/test-all.ps1` passes before requesting review.

## Reporting issues

Open an issue with platform, version, reproduction steps, and logs (with secrets redacted). For security issues, do **not** open a public issue — follow `SECURITY.md`.