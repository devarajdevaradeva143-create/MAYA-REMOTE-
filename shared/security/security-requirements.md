# MAYA Remote Protocol - Security Requirements

## Scope
These requirements apply to the MAYA Remote Protocol as defined in `shared/protocol/protocol-spec.json`. The transport is `direct-encrypted-tcp`: a single persistent TCP connection between a client (controller) and a device (target), encrypted and authenticated end-to-end before any application message is framed.

## Transport security
1. **Mandatory encryption.** All protocol traffic MUST be encrypted in transit. Plaintext TCP is prohibited. The framing layer in `protocol-spec.json` carries no security fields; security is provided entirely by the transport.
2. **TLS / mutual authentication.** The connection MUST use TLS 1.2 or later with mutual (two-way) certificate authentication. Both client and device MUST present a certificate trusted by the peer. Certificate validation failure MUST terminate the connection with error `TLS_VALIDATION_FAILED` (code 4001).
3. **Certificate pinning.** Devices and clients SHOULD pin the peer's certificate or CA fingerprint. A pin mismatch MUST fail the connection.
4. **Perfect forward secrecy.** Cipher suites MUST provide forward secrecy (ECDHE key exchange). Static RSA key exchange is prohibited.
5. **Strong ciphers only.** Only AEAD ciphers (AES-GCM, ChaCha20-Poly1305) are permitted. CBC, RC4, EXPORT, and null cipher suites are prohibited.
6. **Certificate validity.** Certificates MUST be valid for the current time, issued by a trusted CA, and match the expected identity (hostname or device ID as pinned). Expired or revoked certificates MUST fail the connection.

## Identity and pairing
7. **Pairing required.** No application message (PING, PONG, DEVICE_INFO, CONNECTION_STATUS, AUTH_*, etc.) MAY be processed before pairing completes. Only pairing-related messages are accepted in the PAIRING connection state.
8. **Pairing code.** Pairing is initiated with a short-lived, user-visible pairing code (`PAIR_REQUEST`, message type 10). The code MUST be high-entropy (at least 40 bits), displayed out of band, and single-use.
9. **Pairing expiry.** Pairing codes MUST expire after a bounded window. Expiry MUST result in error `PAIRING_EXPIRED` (code 1002).
10. **Rejection.** A user MAY reject pairing at any time, resulting in `PAIRING_REJECTED` (code 1003) and closure of the pairing attempt.
11. **Invalid code.** An invalid pairing code MUST result in `INVALID_PAIRING_CODE` (code 1001) without revealing whether any part of the code was correct.
12. **Device authorization.** After successful pairing, the device is enrolled and authorized. Subsequent connections require the enrolled device identity; unauthorized devices receive `DEVICE_NOT_AUTHORIZED` (code 2001).

## Authentication and sessions
13. **Authenticated session.** Every connection MUST perform an authenticated handshake (`AUTH_REQUEST` / `AUTH_RESPONSE`, message types 20/21) after pairing before application traffic. The handshake MUST bind the session to the paired device identity and the transport channel.
14. **Session binding.** Message framing (`messageId`) MUST be scoped to a single authenticated session. MessageIds MUST NOT be reused across sessions or connections.
15. **Replay protection.** Receivers MUST reject stale or duplicate `messageId` values within a session and nonces within the transport handshake.
16. **Connection states.** Connections MUST progress through the states in `protocol-spec.json` (`DISCONNECTED` -> ... -> `PAIRING` -> `AUTHENTICATING` -> `CONNECTED`) and MUST NOT process application messages outside the `CONNECTED` state.

## Access control and revocation
17. **Revocation.** A revoked device MUST be denied all access and receive `DEVICE_REVOKED` (code 6001). Revocation lists MUST be checked on every connection and enforced immediately.
18. **Remote control disable.** When remote control is disabled by the user or policy, the device MUST refuse remote-control sessions with `REMOTE_CONTROL_DISABLED` (code 7001).
19. **Least privilege.** Authenticated sessions are scoped to remote-control operations defined by the protocol; no administrative or filesystem access is implied by protocol messages.

## Errors and failure handling
20. **Generic error messages.** Protocol errors (message type 99, ERROR) MUST not leak implementation details, stack traces, or certificate internals. Use the error codes and messages defined in `protocol-spec.json`.
21. **Connection timeout.** Idle or stalled connections MUST time out with `CONNECTION_TIMEOUT` (code 3002).
22. **Unreachable network.** Network failures MUST surface as `NETWORK_UNREACHABLE` (code 3001) without exposing internal topology.
23. **Protocol version mismatch.** An unsupported `protocolVersion` MUST fail with `PROTOCOL_VERSION_UNSUPPORTED` (code 5001) and the connection MUST be closed.
24. **Disconnect.** Either peer MAY terminate a session with `DISCONNECT` (message type 50); the transport MUST then be closed and resources released.

## Framing integrity
25. **Payload limits.** `payloadLength` MUST NOT exceed `maxPayloadBytes` (1048576 / 1 MiB) from the framing spec. Oversized frames MUST be rejected and the connection closed.
26. **Frame validation.** Receivers MUST validate `protocolVersion`, `messageType` (against the enum in protocol-spec.json), and `payloadLength` before processing the payload. Invalid frames MUST be rejected without processing payload content.

## Non-requirements (out of scope)
- End-user application-layer authorization beyond remote control.
- Storage encryption of pairing records (platform responsibility).

## Compliance
An implementation is compliant only if every requirement marked MUST is satisfied, and every SHOULD is satisfied or has a documented exception accepted by the device owner.
