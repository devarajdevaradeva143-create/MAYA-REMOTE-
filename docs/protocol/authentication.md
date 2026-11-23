# Authentication

Authentication in MAYA Remote has two distinct phases: **pairing** (one-time, establishes trust) and **session authentication** (per connection, proves the peer still holds the trusted credential). There is no cloud identity provider, no account, and no third-party service involved.

## Phase 1 — Pairing

Pairing is covered in detail in [pairing-flow.md](../architecture/pairing-flow.md). In authentication terms:

- A **short-lived, single-use pairing code** (displayed as a code or QR) is the initial secret.
- The two devices perform an authenticated key exchange, each proving possession of the code.
- The result is a **long-term credential** stored locally on both devices (Android Keystore; Windows DPAPI-protected storage).
- The code is never reused and expires quickly (`PAIRING_EXPIRED`, 1002).

## Phase 2 — Session authentication

Every connection re-authenticates using the stored long-term credential. Pairing is **not** repeated per session.

```
Android device                         Windows host
     │  TCP connect                         │
     │  AUTH_REQUEST (20) ────────────────▶│
     │    - proof of long-term credential   │
     │    - fresh nonce                     │
     │  ◀──────────────────── AUTH_RESPONSE (21)
     │    - acceptance + session params     │
     │  (session key derived)               │
     │  CONNECTED                           │
```

Requirements:

- **Mutual proof.** The initiator proves possession of the long-term credential; the responder verifies it. Both sides derive a fresh **session key** for this connection.
- **Freshness.** A nonce per session prevents replay of old authentication.
- **Revocation check.** A revoked peer is rejected with `DEVICE_REVOKED` (6001) before any capability is granted.
- **Failure.** Missing/invalid proof yields `DEVICE_NOT_AUTHORIZED` (2001); the session ends.

## Key lifecycle

| Key | Lifetime | Storage | Notes |
|-----|----------|---------|-------|
| Pairing code | seconds–minutes | in-memory | single-use, never persisted |
| Long-term credential | until revoked | Keystore / DPAPI | the pairing secret; never logged or backed up to cloud |
| Session key | one session | in-memory | derived per connection, discarded on close |

## Authorization after authentication

Once authenticated and `CONNECTED`, capabilities (input, screen, audio) are available unless:

- The user disabled remote control → `REMOTE_CONTROL_DISABLED` (7001): session stays up, features off.
- The device is revoked mid-session → `DEVICE_REVOKED` (6001): session ends.

## What is never sent

- The raw long-term credential is never transmitted in cleartext.
- Pairing codes are never included in logs or discovery broadcasts.
- Session keys are never exposed to the application or UI.

## Security references

- [Security Model](../architecture/security-model.md)
- [Threat Model](../architecture/threat-model.md)
- [Pairing Flow](../architecture/pairing-flow.md)
- [Connection Flow](../architecture/connection-flow.md)
- [Message Types](message-types.md)

## Implementation references

- Android: `android/.../core/security/`, `android/.../feature/pairing/`, `android/.../feature/security/`.
- Windows: `windows/src/MayaRemote.Core/Security/`, `windows/src/MayaRemote.Networking/Authentication/`, `MayaRemote.Networking/Pairing/`.