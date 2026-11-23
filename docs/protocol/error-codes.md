# Error Codes

Error codes are stable uint32 values carried in an `ERROR` (message type 99) payload, and referenced throughout the protocol and connection state handling. The canonical list is in [`shared/protocol/protocol-spec.json`](../../shared/protocol/protocol-spec.json).

## Registry

| Code | Name | Message |
|------|------|---------|
| 1001 | `INVALID_PAIRING_CODE` | The pairing code is invalid. |
| 1002 | `PAIRING_EXPIRED` | The pairing code has expired. |
| 1003 | `PAIRING_REJECTED` | Pairing was rejected by the user. |
| 2001 | `DEVICE_NOT_AUTHORIZED` | Device is not authorized. |
| 3001 | `NETWORK_UNREACHABLE` | Network is unreachable. |
| 3002 | `CONNECTION_TIMEOUT` | Connection timed out. |
| 4001 | `TLS_VALIDATION_FAILED` | TLS validation failed. |
| 5001 | `PROTOCOL_VERSION_UNSUPPORTED` | Protocol version is not supported. |
| 6001 | `DEVICE_REVOKED` | Device access has been revoked. |
| 7001 | `REMOTE_CONTROL_DISABLED` | Remote control is disabled. |

## Ranges

| Range | Category |
|-------|----------|
| 1000–1999 | Pairing |
| 2000–2999 | Authentication / authorization |
| 3000–3999 | Network / connection |
| 4000–4999 | Transport / TLS |
| 5000–5999 | Protocol |
| 6000–6999 | Trust / revocation |
| 7000–7999 | Feature policy |

## Usage

- `ERROR` payload: error code (uint32), human-readable message, optional developer-facing `technicalDetail` (must be scrubbed before display).
- Codes are **stable**: do not repurpose or renumber an existing code.
- New codes are added with a new value in the appropriate range and documented here and in the schema.
- Clients map codes to user-facing text centrally; do not hardcode messages at call sites.

## Handling by code

| Code | Typical handling |
|------|------------------|
| `INVALID_PAIRING_CODE` | Prompt user to re-enter code. |
| `PAIRING_EXPIRED` | Regenerate code and retry. |
| `PAIRING_REJECTED` | Abort pairing; return to `DISCONNECTED`. |
| `DEVICE_NOT_AUTHORIZED` | Fail session; require re-auth or re-pair. |
| `NETWORK_UNREACHABLE` | Retry with backoff (`RECONNECTING`). |
| `CONNECTION_TIMEOUT` | Retry; escalate to `FAILED` after attempts. |
| `TLS_VALIDATION_FAILED` | Fail session; do not downgrade. |
| `PROTOCOL_VERSION_UNSUPPORTED` | Fail; inform user of version mismatch. |
| `DEVICE_REVOKED` | Move to `REVOKED`; require re-pair. |
| `REMOTE_CONTROL_DISABLED` | Keep session; disable input/screen/audio features. |

## State transitions

See [connection-flow.md](../architecture/connection-flow.md) for how each code affects the connection state machine.

## Related

- [Protocol v1](protocol-v1.md)
- [Message Types](message-types.md)
- [Authentication](authentication.md)