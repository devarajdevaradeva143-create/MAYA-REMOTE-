# Security Policy

## Scope

MAYA Remote is a **local-only** remote control application. There is no cloud backend, no relay, and no third-party service. The entire trust model rests on direct, authenticated communication between a user's own Android device and Windows machine on the same local network.

## Reporting a vulnerability

Please do **not** open a public issue for security vulnerabilities.

Report privately to the maintainer using the repository's security advisory feature (GitHub: **Security → Advisories → Report a vulnerability**) or by contacting the maintainer directly via the contact listed on the repository profile.

When reporting, include:

- A description of the issue and its impact.
- Affected platform(s) and version(s).
- Steps to reproduce, including network topology.
- Any relevant logs, redacted of keys, pairing codes, and personal data.
- Whether the issue requires local network access, physical access, or a paired device.

Please allow reasonable time for a response before any public disclosure.

## What to expect

- **Acknowledgement** within a few business days.
- **Assessment** and a decision on whether it is a vulnerability, a hardening opportunity, or out of scope.
- **Fix or mitigation guidance** once confirmed. Security fixes are prioritized over feature work.

## Out of scope

The following are considered expected behavior for a local-only app and are generally not vulnerabilities unless they enable remote compromise:

- Denial of service by a device the user has already paired.
- Attacks that require the attacker to already control one endpoint or be on the trusted LAN with the ability to intercept traffic.
- Social engineering that tricks a user into pairing with an attacker-controlled device.
- Operating system, driver, or firmware vulnerabilities outside the application.

## Security best practices for users

- Pair only with devices you physically control.
- Review and revoke paired devices in **Security Settings** when they are no longer needed.
- Keep both the Android and Windows apps on supported versions.
- Treat the pairing code as a secret; never share it outside the pairing flow.
- Do not expose the Windows app beyond your trusted local network.

## Supported versions

Security fixes target the latest released minor version of each platform. Older versions may not receive backports.