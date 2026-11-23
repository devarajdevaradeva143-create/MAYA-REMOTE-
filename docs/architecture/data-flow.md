# Data Flow

This document describes what data moves between the Android and Windows components, how it is framed, and how each platform handles it. It focuses on the data plane after a session is authenticated.

## Data categories

| Category | Direction | Examples | Sensitivity |
|----------|-----------|----------|-------------|
| Discovery metadata | both | service name, endpoint, protocol version | low |
| Pairing data | both | ephemeral code, exchange material, identity | high (credential) |
| Session control | both | AUTH, CONNECTED status, PING/PONG, DISCONNECT, REVOKE | medium |
| Device info | both | platform, app version, display name | low |
| Remote input | phone → PC | pointer moves, clicks, key events | high (controls PC) |
| Screen frames | PC → phone | captured desktop frames | high (reveals screen) |
| Audio | both | captured/played audio | high |
| Errors | both | structured error code + message | medium (no secrets) |

## Framing

Every message is a single protocol frame:

```
+-----------+-------------+------------+---------------+-------------------+
| version   | messageType | messageId  | payloadLength | payload           |
| 1 byte    | 2 bytes     | 8 bytes    | 4 bytes       | up to 1 MiB       |
+-----------+-------------+------------+---------------+-------------------+
   big-endian, header = 15 bytes
```

- Version must be `1`; otherwise the frame is rejected.
- `payloadLength` must be `0 … 1,048,576`; larger or negative is rejected.
- The payload is opaque to the framing layer; each message type defines its own schema.

## Message dispatch

1. **Receive.** Transport reads the 15-byte header, validates version and length, then reads the payload.
2. **Decode.** `ProtocolMessage` (Kotlin) / `ProtocolMessage.Decode` (C#) produce a typed message.
3. **Authenticate.** Only messages passing session authentication are processed (see [Authentication](../protocol/authentication.md)).
4. **Dispatch.** The message type routes to the appropriate handler:
   - Control: heartbeat, status, disconnect, revoke.
   - Device info: update peer metadata.
   - Input/screen/audio: feature handlers.
5. **Acknowledge / respond.** Replies reuse the same framing with a new `messageId`.

## Android data flow

```
Compose UI  →  ViewModel  →  UseCase  →  Repository (domain interface)
                                            │
                          ┌─────────────────┴─────────────────┐
                          ▼                                   ▼
                 DataStore (prefs)                     Connection service
                 Room/local (device records)          (transport, Dispatchers.IO)
                          │                                   │
                          └──────────► foreground service ◄───┘
```

- UI state is immutable and driven by the ViewModel.
- Networking runs off the main thread inside the foreground service.
- Device records and preferences are persisted locally; no cloud sync.

## Windows data flow

```
WPF View  →  ViewModel  →  Core service  →  Networking (transport)
                                            │
                              ┌─────────────┼──────────────┐
                              ▼             ▼              ▼
                       WindowsServices  Infrastructure  Protocol codec
                       (screen/input/   (config/log/    (encode/decode)
                        audio/app)       storage)
```

- UI thread owns view state; workers handle capture, input, transport.
- Results marshal back to the UI dispatcher.
- Paired-device records and keys persist via `Infrastructure/Storage`.

## Screen and input paths

- **Screen:** Windows `ScreenCapture` produces frames → framed and sent → Android `ScreenViewer` renders. Frames are large; respect the 1 MiB payload cap (chunk if needed).
- **Input:** Android `RemoteMouse`/`RemoteKeyboard` capture user gestures → framed and sent → Windows `Input` injects locally.
- **Audio:** `WindowsServices/Audio` and the Android audio path exchange framed audio chunks.

## Size and performance rules

- Never exceed 1 MiB per frame; split larger payloads at the application layer.
- Stream large media rather than buffering whole streams in memory.
- Keep encoding/decoding off the UI thread on both platforms.
- Heartbeats must not be starved by large transfers.

## Error propagation

- A handler failure produces an `ERROR` (99) message carrying a stable code from [error codes](../protocol/error-codes.md).
- The session maps the code to UI state and, where appropriate, transitions the connection state.

## Data minimization

- Only metadata needed to operate is exchanged.
- No telemetry, no analytics, no cloud.
- Secrets (credentials, pairing codes, session keys) never appear in payloads or logs.

## Related

- [Connection Flow](connection-flow.md)
- [Protocol v1](../protocol/protocol-v1.md)
- [Message Types](../protocol/message-types.md)
- [Security Model](security-model.md)