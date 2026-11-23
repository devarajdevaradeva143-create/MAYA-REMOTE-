# Testing Guide

MAYA Remote is verified at three levels: **unit tests** (per platform), **protocol conformance** (cross-implementation codecs against shared vectors), and **integration tests** (the two endpoints talking to each other).

## Test layers

### Android

- **Unit tests** (`android/app/src/test/`): JUnit 4 + `kotlinx-coroutines-test`. Cover use cases, repositories, state machine transitions, and the protocol codec against shared vectors.
- **Instrumented tests** (`android/app/src/androidTest/`): run on a device/emulator; cover UI, persistence (DataStore), and end-to-end flows.

Run:

```powershell
cd android
.\gradlew.bat :app:testDebugUnitTest          # JVM unit tests
.\gradlew.bat :app:connectedDebugAndroidTest  # device/emulator tests
```

### Windows

Test projects under `windows/tests/`:

- `MayaRemote.Core.Tests` — models, protocol codec, state logic.
- `MayaRemote.Networking.Tests` — discovery, transport, pairing, auth.
- `MayaRemote.Security.Tests` — key handling, credential protection.
- `MayaRemote.IntegrationTests` — end-to-end flows.

Run:

```powershell
cd windows
dotnet test
```

## Protocol conformance

The protocol is implemented twice; conformance tests ensure both sides agree.

- **Golden-byte tests:** `Encode()` output for a fixed message must equal the exact bytes in `shared/protocol/test-vectors/`.
- **Decode parity:** both `ProtocolMessage.kt` and `ProtocolMessage.cs` must accept/reject the same inputs (valid frames, bad version, oversized length, truncated frames).
- **Schema validation:** new/changed message types and error codes must exist in `shared/protocol/protocol-spec.json`.

When changing the protocol, update vectors and both codecs together (see [compatibility.md](../protocol/compatibility.md)).

## Integration tests

- Start the Windows host (or agent) on the test machine.
- Run the Android client against it on the same network (emulator on the host network, or a physical device on the same Wi-Fi).
- Verify: discovery, pairing, session auth, `CONNECTED`, `PING/PONG`, and clean `DISCONNECT`.
- Revocation: revoke from one side and confirm the other is blocked (`DEVICE_REVOKED`, 6001).

## Running everything

From the repository root:

```powershell
.\scripts\test-all.ps1
```

The script runs the Android unit tests and the Windows `dotnet test` suite, and reports pass/fail per project.

## Writing tests

- **No network in unit tests.** Mock transports; test framing and logic in isolation.
- **No secrets in fixtures.** Use synthetic keys/codes; never commit real credentials or pairing codes.
- **Deterministic.** Avoid timing-dependent assertions except for heartbeat/timeout logic, which should use injected clocks.
- **Cover failure paths.** Assert behavior on `INVALID_PAIRING_CODE`, `PAIRING_EXPIRED`, `DEVICE_NOT_AUTHORIZED`, `PROTOCOL_VERSION_UNSUPPORTED`, oversized payloads, and truncated frames.
- **State machine.** Test `canTransitionTo` for every allowed and disallowed transition.

## What not to test here

- Cloud/relay behavior (none exists).
- OS-level input/screen injection on CI (requires an interactive desktop; test manually or in a dedicated agent environment).

## Related

- [Build Instructions](build-instructions.md)
- [Troubleshooting](troubleshooting.md)
- [Protocol v1](../protocol/protocol-v1.md)
- [Compatibility](../protocol/compatibility.md)