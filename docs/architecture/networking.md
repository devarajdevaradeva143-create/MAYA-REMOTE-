# Networking

MAYA Remote networking is **local-only**: direct TCP between two devices on the same local network, with no relay, no rendezvous, and no cloud.

## Principles

1. **Direct connection.** A TCP socket between the Android device and the Windows machine. Nothing is proxied.
2. **Local discovery.** Devices find each other on the LAN by multicast announcement; a manual address/code path always exists.
3. **Authenticated before trusted.** Discovery and pairing are unauthenticated by design; every session and every message after pairing is authenticated.
4. **Bounded and validated.** All reads are length-bounded (max payload 1 MiB), version-checked, and rejected on protocol violation.

## Topology

```
Android device  ── TCP (local network) ──  Windows machine
   (client)                                   (host / listener)
```

Either side may initiate the TCP connection depending on which is listening; the Windows host typically listens and the Android device dials, but the protocol is symmetric and does not depend on direction.

## Discovery

- **Mechanism:** LAN multicast/DNS-SD-style announcement. Peers advertise a service name and endpoint.
- **Payload:** minimal device metadata (platform, app/protocol version, display name). No secrets are broadcast.
- **Privacy:** discovery never includes the long-term credential or pairing code.
- **Fallback:** the user can enter an address and pairing code manually when discovery is unavailable (e.g., AP isolation).

## Transport and framing

The wire format is defined once in `shared/protocol/protocol-spec.json` and implemented in:

- `android/.../core/networking/ProtocolMessage.kt`
- `windows/src/MayaRemote.Core/Protocol/ProtocolMessage.cs`

Header (15 bytes, big-endian):

| Field | Size | Type | Notes |
|-------|------|------|-------|
| protocolVersion | 1 | uint8 | must equal 1 |
| messageType | 2 | uint16 | see message types |
| messageId | 8 | uint64 | per-message identifier |
| payloadLength | 4 | uint32 | 0 … 1,048,576 |
| payload | variable | bytes | up to 1 MiB |

Rules:

- **Byte order:** big-endian.
- **Max payload:** 1,048,576 bytes (1 MiB). Larger frames are rejected.
- **Version check:** a frame whose `protocolVersion` is not 1 is rejected (decode returns `null` / `Decode` returns `null`), and the session should surface `PROTOCOL_VERSION_UNSUPPORTED` (5001).
- **Length check:** negative or oversized lengths are rejected; incomplete frames wait for more bytes.

## Connection lifecycle

The transport maps to the connection state machine (`ConnectionState` / `Core` connection):

```
DISCONNECTED → DISCOVERING → PAIRING → AUTHENTICATING → CONNECTING → CONNECTED
                                                          ↘ RECONNECTING ↗
                                     (any) → FAILED / REVOKED → DISCONNECTED
```

See [Connection Flow](connection-flow.md) for the full sequence and message exchange.

## Heartbeat

- `PING` (1) / `PONG` (2) keep the session alive and detect dead peers.
- The transport tracks last-received time; missed heartbeats trigger `RECONNECTING` then `FAILED`/`DISCONNECTED`.

## Ports and firewall

- The Windows host listens on a local TCP port (configurable in `Infrastructure/Configuration`).
- Only the local network needs reachability; inbound rules on Windows must permit the chosen port. No outbound internet access is required.

## Failure handling

- Unreachable host → `NETWORK_UNREACHABLE` (3001).
- Timeout → `CONNECTION_TIMEOUT` (3002).
- TLS/transport validation failure → `TLS_VALIDATION_FAILED` (4001).
- Unsupported version → `PROTOCOL_VERSION_UNSUPPORTED` (5001).

See [error codes](../protocol/error-codes.md).

## Non-goals

- NAT traversal, hole punching, TURN/STUN, or any internet relay.
- Cross-subnet discovery guarantees (depends on the local network).
- Persistence of connection across device sleep beyond what the foreground service / reconnect logic allows.