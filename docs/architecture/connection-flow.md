# Connection Flow

The connection flow describes how a paired Android device and Windows machine establish, authenticate, use, and tear down a session. It is governed by the connection state machine and the protocol messages defined in `shared/protocol/protocol-spec.json`.

## States

```
DISCONNECTED
  → DISCOVERING
  → PAIRING            (one-time, see Pairing Flow)
  → AUTHENTICATING
  → CONNECTING
  → CONNECTED
      ↘ RECONNECTING ↗
(any) → FAILED / REVOKED → DISCONNECTED
```

Transitions are guarded by `ConnectionState.canTransitionTo` on Android and the equivalent connection service on Windows.

## Successful sequence (already paired)

```
Android device                              Windows host
     │  discover / address                      │
     │──────────── TCP connect ───────────────▶│
     │              (transport up)              │
     │  AUTH_REQUEST (20) ────────────────────▶│
     │  ◀──────────────────────── AUTH_RESPONSE (21)
     │            (session authenticated)       │
     │  DEVICE_INFO (30) ─────────────────────▶│
     │  ◀──────────────────────── DEVICE_INFO (30)
     │  CONNECTED                               │
     │  PING (1) ─────────────────────────────▶│
     │  ◀──────────────────────── PONG (2)      │
     │            ... session ...               │
     │  DISCONNECT (50) ──────────────────────▶│
     │  (transport closed)                      │
     ▼                                          ▼
 DISCONNECTED                              DISCONNECTED
```

### Step detail

1. **Discover / connect.** The client resolves the host and opens a TCP connection (transport up). State: `CONNECTING`.
2. **Authenticate.** `AUTH_REQUEST` (20) / `AUTH_RESPONSE` (21) using the stored long-term credential; a fresh session key is derived. State: `AUTHENTICATING`. Failure → `DEVICE_NOT_AUTHORIZED` (2001) or `DEVICE_REVOKED` (6001).
3. **Exchange device info.** `DEVICE_INFO` (30) carries non-secret metadata (platform, app/protocol version, display name) so each side can render the peer.
4. **Connected.** State: `CONNECTED`. Heartbeats via `PING` (1) / `PONG` (2) run for the session lifetime.
5. **Tear down.** `DISCONNECT` (50) closes cleanly; both sides return to `DISCONNECTED`.

## Reconnection

- On heartbeat timeout or transient transport failure, the session enters `RECONNECTING` and attempts to re-establish without re-pairing.
- Repeated failures escalate to `FAILED` and then `DISCONNECTED`.
- `CONNECTION_TIMEOUT` (3002) and `NETWORK_UNREACHABLE` (3001) are the typical errors.

## Revocation

- Either side may send `REVOKE_DEVICE` (60) to terminate trust. The revoked peer is moved to `REVOKED` and cannot reconnect until re-paired (`DEVICE_REVOKED`, 6001).

## Version handling

- The first byte of every frame is the protocol version. If it is not 1, the peer rejects the frame and reports `PROTOCOL_VERSION_UNSUPPORTED` (5001). See [Compatibility](../protocol/compatibility.md).

## Heartbeat and timeouts

- Heartbeat interval and timeout are transport configuration. A missed heartbeat triggers reconnect; a bounded number of attempts precedes `FAILED`.

## Error mapping

| Error | Code | Effect on state |
|-------|------|-----------------|
| `DEVICE_NOT_AUTHORIZED` | 2001 | `FAILED` → `DISCONNECTED` |
| `NETWORK_UNREACHABLE` | 3001 | `RECONNECTING` → `FAILED` |
| `CONNECTION_TIMEOUT` | 3002 | `RECONNECTING` → `FAILED` |
| `TLS_VALIDATION_FAILED` | 4001 | `FAILED` |
| `PROTOCOL_VERSION_UNSUPPORTED` | 5001 | `FAILED` |
| `DEVICE_REVOKED` | 6001 | `REVOKED` |
| `REMOTE_CONTROL_DISABLED` | 7001 | stays `CONNECTED`, features disabled |

See [error codes](../protocol/error-codes.md).

## Implementation references

- Android: `android/.../core/common/ConnectionState.kt`, `android/.../service/connection/`.
- Windows: `windows/src/MayaRemote.Networking/Connection/` and `Transport/`.

## Related

- [Pairing Flow](pairing-flow.md)
- [Data Flow](data-flow.md)
- [Networking](networking.md)
- [Protocol v1](../protocol/protocol-v1.md)