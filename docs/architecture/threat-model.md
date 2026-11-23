# Threat Model

This document enumerates threats to MAYA Remote and the mitigations designed into the system. It uses a STRIDE-style structure and assumes the local network is hostile.

## Assumptions

- The user's Android device and Windows machine are individually trusted (not compromised).
- The local network is untrusted: other hosts may sniff, spoof, or attempt to join.
- There is no cloud, no relay, and no third-party service to attack.
- Physical access to a device is out of scope as an initial-access vector but in scope for impact.

## Assets

- Long-term pairing credential (the core secret).
- Session keys and live traffic (screen frames, input events, audio).
- Paired-device records and revocation state.
- Control of the remote machine (input injection, application control).

## Trust boundaries

1. Internet ↔ app (no channel; documented as out of scope).
2. LAN ↔ app (discovery, pairing, transport).
3. UI ↔ service (Android: UI/ViewModel ↔ service; Windows: WPF ↔ services).
4. App ↔ OS capabilities (input, screen capture, keystore/DPAPI).

## Threats and mitigations

### Spoofing

- **T:** An attacker impersonates a paired device or the Windows host.
- **M:** Mutual authentication using the long-term credential on every session. Impersonation fails without the credential.

### Tampering

- **T:** An on-path attacker modifies frames (input, screen, pairing).
- **M:** Authenticated encryption and integrity checks on every frame; version/length validation; tampered frames are dropped and the session fails.

### Repudiation

- **T:** A party denies having performed an action.
- **M:** Pairing and revocation are explicit user actions with local records; no anonymous actions are possible.

### Information disclosure

- **T:** Sniffing pairing codes, credentials, or traffic on the LAN.
- **M:** Pairing codes are short-lived and single-use; long-term credentials are never sent in cleartext; traffic is encrypted with session keys; payloads are never logged.

### Denial of service

- **T:** Flooding the host with connections, oversized frames, or malformed data.
- **M:** Max payload 1 MiB enforced on both platforms; strict decode returns `null`/rejects; connection rate and state guards; unreachable/invalid peers time out (`CONNECTION_TIMEOUT`, 3002).

### Elevation of privilege

- **T:** A paired but malicious or compromised device abuses input injection, screen capture, or application control.
- **M:** Remote control can be disabled (`REMOTE_CONTROL_DISABLED`, 7001); revocation is immediate (`REVOKED`, 6001); Windows capabilities are gated behind `WindowsServices/Permissions`.

## STRIDE summary

| Category | Threat | Mitigation |
|----------|--------|------------|
| Spoofing | Fake peer/host | Mutual auth with long-term credential |
| Tampering | Modified frames | AEAD + integrity + validation |
| Repudiation | Denial of actions | Explicit, recorded pairing/revocation |
| Info disclosure | Sniffed codes/traffic | Short-lived codes, encrypted transport, no logging |
| DoS | Floods/oversized frames | 1 MiB cap, rate/state guards, timeouts |
| Elevation | Malicious paired device | Disable remote control, revocation, permission gates |

## Specific attack scenarios

### Malicious LAN device tries to pair

- It cannot complete pairing without the short-lived code the user displays.
- Even if it obtains the code, pairing requires explicit user action; an unexpected pairing prompt is visible.
- Mitigation: single-use, expiring codes; user-initiated pairing only.

### Replay of an old pairing or session

- Session keys are derived fresh per connection; old captured frames do not decrypt under new keys.
- Pairing code is single-use and expired.

### Compromised keystore/OS

- Out of scope for the app; documented residual risk. Users should keep OS updated.

### Revoked device reconnects

- Rejected with `DEVICE_REVOKED` (6001); revocation is checked before any capability is granted.

## Non-goals

- Protection against a fully compromised endpoint.
- Protection when the user is socially engineered into pairing with an attacker.
- Internet exposure or NAT traversal.
- Multi-user or enterprise threat scenarios.

## Review cadence

Revisit this model whenever the protocol version, pairing method, or capability set changes. Any new network message or capability must be added here with its mitigation.