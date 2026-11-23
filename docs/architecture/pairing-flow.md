# Pairing Flow

Pairing is a **one-time, local** operation that establishes the long-term credential shared by the user's two devices. It happens on the local network, requires explicit user action, and produces a revocable device record on both ends.

## Overview

```
Android device (initiator)                Windows host (responder)
        │                                          │
        │  (1) user taps "Pair"                    │
        │─────────────────────────────────────────▶│  discovery / address
        │  (2) host shows short-lived code or QR   │
        │◀─────────────────────────────────────────│
        │  (3) user scans QR / enters code         │
        │  (4) PAIR_REQUEST (10) ─────────────────▶│
        │  (5) verify code, derive exchange        │
        │  (6) ◀───────────────────── PAIR_RESPONSE (11)
        │  (7) both persist long-term credential   │
        │  (8) optional CONNECTION_STATUS (40)     │
        ▼                                          ▼
   paired record                            paired record
```

## Preconditions

- Both devices are on the same local network.
- The Windows host is running and listening (or the Android device's host is, if roles are reversed).
- The user physically has both devices.

## Steps

1. **Discover.** The Android client discovers the Windows host on the LAN, or the user enters its address. See [Networking](networking.md).
2. **Show code.** The responder (typically Windows) generates a **short-lived, single-use pairing code** and/or QR encoding the code and connection info. The code expires (see `PAIRING_EXPIRED`, 1002).
3. **Scan / enter.** The user scans the QR or types the code on the initiator. This is the explicit user action that authorizes pairing.
4. **Pair request.** The initiator sends `PAIR_REQUEST` (10) containing its identity and proof derived from the code. The responder validates the code is unexpired and unused.
5. **Exchange.** The two sides complete an authenticated key exchange, each proving possession of the code, and derive the long-term credential. No cleartext secrets cross the network.
6. **Pair response.** The responder replies with `PAIR_RESPONSE` (11), including its identity and acceptance/rejection.
7. **Persist.** Both devices store the long-term credential and a device record locally (Android Keystore / DPAPI on Windows). The code is marked used.
8. **Confirm.** An optional `CONNECTION_STATUS` (40) may be exchanged to reflect the new trusted relationship.

## Failure modes

| Code | Constant | When |
|------|----------|------|
| 1001 | `INVALID_PAIRING_CODE` | Code is malformed or unknown. |
| 1002 | `PAIRING_EXPIRED` | Code past its validity window. |
| 1003 | `PAIRING_REJECTED` | User declined on the responder. |

On failure the session moves to `FAILED`/`DISCONNECTED`; no credential is stored.

## Security properties

- **Local-only:** the code is meaningless off the local network and short-lived.
- **Explicit:** pairing cannot complete without a user showing/scanning a code.
- **Single-use:** a code is consumed on first successful pairing.
- **Revocable:** pairing can be undone via `REVOKE_DEVICE` (60) from either side.

## Implementation references

- Android: `android/.../feature/pairing/`, `android/.../service/connection/`, codec in `core/networking/ProtocolMessage.kt`.
- Windows: `windows/src/MayaRemote.Networking/Pairing/`, codec in `MayaRemote.Core/Protocol/ProtocolMessage.cs`.
- Schema: `shared/protocol/protocol-spec.json`.

## Related

- [Connection Flow](connection-flow.md)
- [Authentication](../protocol/authentication.md)
- [Security Model](security-model.md)
- [Threat Model](threat-model.md)