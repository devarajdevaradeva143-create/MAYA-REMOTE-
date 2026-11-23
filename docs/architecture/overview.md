# MAYA Remote — Architecture Overview

## Purpose

MAYA Remote is a personal, local-first remote control application that lets a user control their own Android phone from their own Windows laptop, or vice versa, over the same local network. It is intentionally scoped:

- **No cloud.** There is no backend, account system, or hosted service.
- **No relay.** Devices talk directly to each other; nothing is proxied through a third party.
- **No third-party service.** No analytics, telemetry, crash reporting, or remote configuration.
- **Local pairing.** Devices are paired once, on the local network, via a short-lived code or QR; trust is established locally and persisted on-device.

## Two components

| Component | Path | Stack | Role |
|-----------|------|-------|------|
| Android client | `android/` | Kotlin, Jetpack Compose, Hilt, DataStore, Navigation, Kotlin Coroutines | Mobile endpoint. Discovers, pairs, connects, and provides remote input/screen surfaces. |
| Windows host | `windows/` | C#, WPF, .NET | Desktop endpoint and agent. Hosts the service, performs screen capture and input injection, enforces pairing and connection. |

Both components implement the same wire protocol (`shared/protocol/protocol-spec.json`) independently. There is no shared runtime; the protocol specification and test vectors are the contract.

## High-level design

```
┌────────────────────┐        local network (LAN / Wi-Fi Direct)        ┌────────────────────┐
│   Android device   │  <────── direct, authenticated, encrypted ─────>  │   Windows machine   │
│  (Kotlin / Compose)│                 TCP, no relay                     │   (C# / WPF / .NET) │
└─────────┬──────────┘                                                    └──────────┬─────────┘
          │                                                                            │
          │ pairing (one-time, local)                                                 │ pairing store
          ▼                                                                            ▼
   paired-device record                                                        paired-device record
   (keys, metadata, revocation)                                               (keys, metadata, revocation)
```

Key architectural decisions:

1. **Direct transport only.** A TCP connection between the two devices on the local network. No relay, no NAT traversal service, no cloud rendezvous.
2. **Local discovery.** Devices find each other on the LAN (multicast/DNS-SD style). Discovery is a convenience; a user can always enter an address or code manually.
3. **One-time local pairing.** A short-lived pairing code or QR establishes initial trust. The long-term credential is exchanged during pairing and stored on each device.
4. **Per-session authentication.** Each connection re-authenticates using the stored long-term credential; pairing is not re-run per session.
5. **Versioned protocol.** Protocol v1 is defined in one schema. Implementations validate the version and reject unsupported versions with a defined error.
6. **Explicit connection state machine.** Both platforms model connection lifecycle as a finite state set (`DISCONNECTED`, `DISCOVERING`, `PAIRING`, `AUTHENTICATING`, `CONNECTING`, `CONNECTED`, `RECONNECTING`, `FAILED`, `REVOKED`).

## Documents

- [System Design](system-design.md) — components, layers, responsibilities, threading model.
- [Android Architecture](android-architecture.md)
- [Windows Architecture](windows-architecture.md)
- [Networking](networking.md) — discovery, transport, framing.
- [Security Model](security-model.md) — trust, keys, pairing, session auth.
- [Threat Model](threat-model.md)
- [Pairing Flow](pairing-flow.md)
- [Connection Flow](connection-flow.md)
- [Data Flow](data-flow.md)
- Protocol docs under [`../protocol/`](../protocol/protocol-v1.md)
- Development docs under [`../development/`](../development/setup-android.md)

## Non-goals

- Remote access over the internet or across NAT without user-managed VPN.
- Multi-user accounts or team sharing.
- Cloud storage, backups, or sync of device records.
- Any telemetry or usage analytics.