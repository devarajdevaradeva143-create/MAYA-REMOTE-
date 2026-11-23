# MAYA Remote Protocol v1

**Status:** draft v1
**Transport:** direct, authenticated, encrypted TCP over the local network
**Byte order:** big-endian

The protocol is defined once in [`shared/protocol/protocol-spec.json`](../../shared/protocol/protocol-spec.json) and implemented independently on Android (`ProtocolMessage.kt`) and Windows (`ProtocolMessage.cs`). This document describes the wire format and the rules both implementations must satisfy.

## Design goals

- **Local-only.** Direct TCP between paired devices. No relay, no cloud, no rendezvous.
- **Simple framing.** Fixed-size header, length-prefixed payload, explicit version.
- **Strict validation.** Reject bad versions, oversized/negative lengths, and malformed frames.
- **Symmetric.** Either side may send any message; the protocol does not assume a client/server split beyond who listens.

## Frame format

Every message is exactly one frame:

| Offset | Field | Size | Type | Notes |
|--------|-------|------|------|-------|
| 0 | `protocolVersion` | 1 | uint8 | Must equal `1`. |
| 1 | `messageType` | 2 | uint16 | See message types. |
| 3 | `messageId` | 8 | uint64 | Per-message identifier. |
| 11 | `payloadLength` | 4 | uint32 | `0 … 1,048,576`. |
| 15 | `payload` | variable | bytes | Up to 1 MiB. |

Header size: **15 bytes**. Max frame size: **15 + 1,048,576 bytes**.

### Field rules

- **protocolVersion:** `1`. Any other value → reject frame, surface `PROTOCOL_VERSION_UNSUPPORTED` (5001).
- **messageType:** one of the constants in [message-types.md](message-types.md).
- **messageId:** assigned by the sender; echoed in replies where applicable.
- **payloadLength:** `>= 0` and `<= 1048576`; otherwise reject.
- **payload:** exactly `payloadLength` bytes. Incomplete frames wait for more bytes.

## Decoding rules

Both implementations follow the same decode contract:

- If fewer than 15 bytes are available → not enough data (return "need more").
- If `protocolVersion != 1` → reject (return `null`).
- If `payloadLength < 0 || payloadLength > 1048576` → reject (return `null`).
- If fewer than `15 + payloadLength` bytes are available → need more data.
- Otherwise → return the decoded message with payload copied out.

Kotlin: `ProtocolMessage.decode(buffer): ProtocolMessage?` returns `null` on any rejection.
C#: `ProtocolMessage.Decode(ReadOnlySpan<byte>): ProtocolMessage?` returns `null` on any rejection.

## Encoding rules

- Write version, message type (big-endian uint16), message id (big-endian uint64), payload length (big-endian uint32), then payload.
- Kotlin: `ProtocolMessage.encode(msg): ByteArray`.
- C#: `ProtocolMessage.Encode(): byte[]`.

The two implementations produce identical bytes for identical input.

## Message types

| Name | Value |
|------|-------|
| `PING` | 1 |
| `PONG` | 2 |
| `PAIR_REQUEST` | 10 |
| `PAIR_RESPONSE` | 11 |
| `AUTH_REQUEST` | 20 |
| `AUTH_RESPONSE` | 21 |
| `DEVICE_INFO` | 30 |
| `CONNECTION_STATUS` | 40 |
| `DISCONNECT` | 50 |
| `REVOKE_DEVICE` | 60 |
| `ERROR` | 99 |

See [message-types.md](message-types.md) for payload semantics.

## Connection states

`DISCONNECTED, DISCOVERING, PAIRING, AUTHENTICATING, CONNECTING, CONNECTED, RECONNECTING, FAILED, REVOKED`

See [connection-flow.md](../architecture/connection-flow.md) for transitions.

## Error codes

| Name | Code |
|------|------|
| `INVALID_PAIRING_CODE` | 1001 |
| `PAIRING_EXPIRED` | 1002 |
| `PAIRING_REJECTED` | 1003 |
| `DEVICE_NOT_AUTHORIZED` | 2001 |
| `NETWORK_UNREACHABLE` | 3001 |
| `CONNECTION_TIMEOUT` | 3002 |
| `TLS_VALIDATION_FAILED` | 4001 |
| `PROTOCOL_VERSION_UNSUPPORTED` | 5001 |
| `DEVICE_REVOKED` | 6001 |
| `REMOTE_CONTROL_DISABLED` | 7001 |

See [error-codes.md](error-codes.md).

## Example

A `PING` (type 1) with id `0x0102030405060708` and empty payload:

```
01                      protocolVersion = 1
00 01                   messageType = 1 (PING)
01 02 03 04 05 06 07 08 messageId
00 00 00 00             payloadLength = 0
```

Total: 15 bytes.

## Versioning and compatibility

- The `protocolVersion` byte is the compatibility gate. v1 rejects any non-1 version.
- Backward-compatible additions use new message types or optional payload fields without bumping the version; see [compatibility.md](compatibility.md).
- Breaking changes require a new protocol version and a migration plan.

## Reference implementations

- Kotlin: `android/app/src/main/java/com/maya/remote/core/networking/ProtocolMessage.kt`
- C#: `windows/src/MayaRemote.Core/Protocol/ProtocolMessage.cs`
- Schema: `shared/protocol/protocol-spec.json`

Keep these three in sync; validate changes against `shared/protocol/test-vectors/`.