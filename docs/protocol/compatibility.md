# Compatibility

MAYA Remote has two independently implemented endpoints — Android (Kotlin) and Windows (C#) — that interoperate only through the protocol. Compatibility rules keep them in sync without a shared runtime.

## The compatibility contract

- **Schema is the source of truth.** `shared/protocol/protocol-spec.json` defines framing, message types, connection states, and error codes.
- **Two implementations must match byte-for-byte.** `ProtocolMessage.kt` and `ProtocolMessage.cs` must encode identical bytes and enforce identical decode rules for the same input.
- **Version is explicit.** The `protocolVersion` byte gates compatibility; v1 rejects any non-1 version with `PROTOCOL_VERSION_UNSUPPORTED` (5001).

## What "compatible" means

Two builds are compatible if:

1. They speak the same `protocolVersion` (currently `1`).
2. Every message type one sends is understood by the other.
3. Payload schemas for exchanged messages are compatible (same required fields, compatible optional fields).
4. Shared error codes and connection states are interpreted the same way.

## Backward-compatible changes (no version bump)

- **Adding a new message type** in a reserved category. Unknown types are ignored by the peer (or answered with `ERROR` if the peer cannot proceed).
- **Adding optional payload fields.** Parsers must tolerate unknown trailing fields and missing optional fields.
- **Adding a new error code** in an unused value.
- **Tightening validation** that rejects only malformed input (never valid peers).

## Breaking changes (require a new version)

- Removing or renumbering a message type.
- Changing frame layout, header size, byte order, or the max payload size.
- Changing the meaning of an existing field or error code.
- Making a previously optional field required.
- Changing version-check behavior.

A breaking change increments `protocolVersion` and requires a migration plan: both apps must be updated together, and an old build must fail cleanly against a new one (it will, via the version check).

## Version negotiation

- There is no negotiation handshake in v1. A peer that receives a non-1 version rejects the frame and reports `PROTOCOL_VERSION_UNSUPPORTED`.
- Future versions may add a negotiation exchange; until then, both endpoints must be on the same major protocol version.

## Interoperability checklist for contributors

Before changing protocol behavior:

1. Update `shared/protocol/protocol-spec.json`.
2. Update both `ProtocolMessage` implementations identically.
3. Update `shared/protocol/test-vectors/` and confirm both implementations pass the same vectors.
4. Update `docs/protocol/protocol-v1.md` and, if applicable, this document.
5. Confirm whether the change is compatible (no bump) or breaking (bump).

## Testing compatibility

- Cross-implementation tests run both codecs over shared test vectors (see [testing guide](../development/testing-guide.md)).
- Golden-byte tests assert `Encode()` output matches the vector bytes exactly.
- Decode tests assert identical accept/reject behavior on malformed input.

## Related

- [Protocol v1](protocol-v1.md)
- [Message Types](message-types.md)
- [Error Codes](error-codes.md)
- [System Design](../architecture/system-design.md)