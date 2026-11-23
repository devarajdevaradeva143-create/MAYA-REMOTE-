# MAYA Remote

MAYA Remote is a personal, secure, two-way remote control application allowing a user to connect and control their own Android mobile device and Windows laptop.

## No Cloud. No Relay. No Third-Party Service.

All pairing and authentication happens locally between devices.

## Structure

- `android/` - Android application (Kotlin, Jetpack Compose)
- `windows/` - Windows application (C#, WPF, .NET 10)
- `docs/` - Architecture, protocol, development docs
- `shared/` - Shared protocol schemas and security requirements
- `scripts/` - Build scripts

## First Milestone

Local network discovery, pairing via temporary code/QR, authenticated encrypted connection, PING/PONG, CONNECTED status, safe disconnect.

## Build

See `docs/development/build-instructions.md`.
