# Message Types

Message types are identified by the `messageType` uint16 field in the protocol frame. The canonical list is in [`shared/protocol/protocol-spec.json`](../../shared/protocol/protocol-spec.json) and mirrored as constants in both `ProtocolMessage` implementations.

| Name | Value | Category | Direction (typical) |
|------|-------|----------|---------------------|
| `PING` | 1 | Heartbeat | either |
| `PONG` | 2 | Heartbeat | either |
| `PAIR_REQUEST` | 10 | Pairing | initiator → responder |
| `PAIR_RESPONSE` | 11 | Pairing | responder → initiator |
| `AUTH_REQUEST` | 20 | Session | initiator → responder |
| `AUTH_RESPONSE` | 21 | Session | responder → initiator |
| `DEVICE_INFO` | 30 | Info | either |
| `CONNECTION_STATUS` | 40 | Session | either |
| `DISCONNECT` | 50 | Session | either |
| `REVOKE_DEVICE` | 60 | Trust | either |
| `ERROR` | 99 | Error | either |

Payload schemas below describe the **minimum** contract. Implementations may extend payloads only per the [compatibility rules](compatibility.md).

## Heartbeat

### PING (1)
- Payload: empty or small opaque nonce.
- Purpose: liveness check; prompts a `PONG`.

### PONG (2)
- Payload: echoes the `PING` nonce if one was sent; otherwise empty.
- Purpose: proves the peer is alive and reachable.

## Pairing

### PAIR_REQUEST (10)
- Sent by the device initiating pairing.
- Payload: initiator identity, protocol version, and proof derived from the short-lived pairing code/QR.
- The responder must reject if the code is unknown (`INVALID_PAIRING_CODE`, 1001) or expired (`PAIRING_EXPIRED`, 1002).

### PAIR_RESPONSE (11)
- Sent by the responder.
- Payload: responder identity, acceptance flag, and exchange material.
- Acceptance=false with a reason mapped to `PAIRING_REJECTED` (1003) when the user declines.

See [pairing-flow.md](../architecture/pairing-flow.md).

## Session authentication

### AUTH_REQUEST (20)
- Sent by the connecting device to prove possession of the long-term credential.
- Payload: authentication material (proof, never the raw credential).
- Failure → `DEVICE_NOT_AUTHORIZED` (2001) or `DEVICE_REVOKED` (6001).

### AUTH_RESPONSE (21)
- Sent in reply. Payload: acceptance and session parameters.

## Device information

### DEVICE_INFO (30)
- Payload: non-secret metadata (platform, app version, protocol version, display name).
- Used by both sides to render the peer. No credentials or secrets.

## Connection status

### CONNECTION_STATUS (40)
- Payload: current connection state and optional reason.
- Used to reflect the relationship state to the user (e.g., after pairing).

## Teardown and revocation

### DISCONNECT (50)
- Payload: optional reason.
- Closes the session cleanly; both sides return to `DISCONNECTED`.

### REVOKE_DEVICE (60)
- Payload: target device identifier.
- Revokes trust; the revoked peer moves to `REVOKED` and cannot reconnect until re-paired (`DEVICE_REVOKED`, 6001).

## Errors

### ERROR (99)
- Payload: error code (uint32), human-readable message, optional technical detail (developer-facing, must be scrubbed before display).
- Code values are listed in [error-codes.md](error-codes.md).
- Any protocol violation (bad version, oversized length, malformed frame) may result in an `ERROR` and/or immediate session failure.

## Reserved ranges

- Values are grouped by category (1–9 heartbeat, 10–19 pairing, 20–29 auth, 30–39 info, 40–49 session, 50–59 teardown, 60–69 trust, 99 error).
- New message types must be allocated within the appropriate category and documented here and in the schema.

## Implementation references

- Kotlin constants: `android/.../core/networking/ProtocolMessage.kt` → `MessageTypes`.
- C# constants: `windows/src/MayaRemote.Core/Protocol/ProtocolMessage.cs` → `MessageTypes`.
- Schema: `shared/protocol/protocol-spec.json`.

## Related

- [Protocol v1](protocol-v1.md)
- [Authentication](authentication.md)
- [Error Codes](error-codes.md)
- [Compatibility](compatibility.md)