# Security Model

MAYA Remote's security model is built around a single premise: **trust is established locally, once, and then used to authenticate every session between the user's own devices.** There is no cloud identity, no account, and no third-party trust anchor.

## Trust model

- **Trusted endpoints:** the user's own Android device and Windows machine.
- **Trust anchor:** a local pairing performed on the LAN, resulting in a shared long-term credential stored on both devices.
- **Untrusted network:** the local network itself is treated as potentially hostile (other devices, ARP/DNS spoofing, sniffing). Everything crossing the network is authenticated and encrypted.
- **No remote trust:** the internet, relays, and cloud services are explicitly out of scope and not part of the trust model.

## Identities and keys

- Each device has a long-term key pair/credential generated on-device and stored locally (Android Keystore on Android; DPAPI-protected storage on Windows).
- The **long-term credential** is the pairing secret. It is exchanged only during pairing and never sent in cleartext, never logged, and never included in cloud backups.
- A **session key** is derived per connection for the transport. Session keys are ephemeral and discarded when the session ends.

Key handling rules:

- Keys never leave the device except during the pairing handshake (as protected key material, not as recoverable secrets).
- Keys are never written to logs, diagnostics, or crash reports.
- `local.properties`, keystores, `.pfx`, `.pem`, and `.jks` are git-ignored.

## Pairing (one-time, local)

Pairing establishes the long-term credential. High-level:

1. User initiates pairing on one device; a short-lived **pairing code** or QR is displayed.
2. The other device scans/enters the code.
3. The two devices complete an authenticated key exchange, each verifying possession of the code.
4. Both persist the resulting long-term credential and device record.

Pairing properties:

- **Short-lived code:** the pairing code expires (see `PAIRING_EXPIRED`, 1002). It is single-use.
- **Local-only:** the code is only valid between devices on the same local network session; it is not usable remotely.
- **Explicit user action:** pairing requires a user to display/scan the code; it cannot complete silently.
- **Revocable:** either side can revoke a paired device (`REVOKE_DEVICE`, 60), which invalidates the pairing on both ends.

Full sequence: [Pairing Flow](pairing-flow.md). Wire details: [Authentication](../protocol/authentication.md).

## Session authentication

Every connection after pairing re-authenticates using the stored long-term credential:

- A fresh session key is derived and used to protect the transport.
- The peer must prove possession of the long-term credential before any application message is accepted.
- Failure to authenticate yields `DEVICE_NOT_AUTHORIZED` (2001).

## Transport protection

- The TCP transport is protected with authenticated encryption derived from the session key.
- Payloads are confidential and integrity-protected; a tampered frame is rejected.
- Frame validation (version, length ≤ 1 MiB, message type) is applied before decryption/processing.

## Authorization and revocation

- Only paired, non-revoked devices may connect (`REVOKED`, 6001 when a revoked device attempts to connect).
- The Windows host and Android client each maintain their own paired-device list; revocation on either side blocks the peer.
- Remote control can be disabled (`REMOTE_CONTROL_DISABLED`, 7001), which stops input/screen features without ending the session.

## Data minimization

- The app collects no telemetry and sends nothing to third parties.
- Device metadata shared during discovery and pairing is limited to what is needed to identify the device.
- Pairing records are stored locally and are not synced or backed up to any cloud service.

## Logging and diagnostics

- Logs must not contain keys, pairing codes, session keys, or payloads.
- Error `technicalDetail` fields are for developers and must be scrubbed before display.

## Residual risks

- A compromised endpoint (rooted device, malware on Windows) can observe its own plaintext.
- An attacker physically on the LAN who can also trick the user into pairing can gain control — mitigated by short-lived, explicit, local pairing and easy revocation.
- OS/keystore vulnerabilities are outside the app's control.

See the [Threat Model](threat-model.md) for enumerated threats and mitigations.