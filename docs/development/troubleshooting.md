# Troubleshooting

Common issues when building, running, or connecting MAYA Remote, and how to resolve them. No cloud or relay is involved, so problems are almost always local: environment, build config, or network reachability.

## Android

### Gradle wrapper not found

**Symptom:** `.\gradlew.bat` is not recognized, or Android Studio prompts to create the wrapper.

**Fix:** generate it (see [setup-android.md](setup-android.md)):

```powershell
cd android
gradle wrapper --gradle-version 8.7
```

Or open the `android/` folder in Android Studio and let it sync.

### `MainActivity` not found / app won't launch

**Symptom:** build succeeds but the app crashes or the launcher activity is unresolved. `AndroidManifest.xml` references `.MainActivity`.

**Fix:** add `MainActivity.kt`:

```kotlin
package com.maya.remote

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import dagger.hilt.android.AndroidEntryPoint

@AndroidEntryPoint
class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent { /* Compose UI */ }
    }
}
```

### JDK / Gradle JVM mismatch

**Symptom:** Gradle fails with a Java version error.

**Fix:** ensure `JAVA_HOME` points to JDK 17:

```powershell
$env:JAVA_HOME = "C:\Program Files\Java\jdk-17"
.\gradlew.bat :app:assembleDebug
```

### SDK not found / licenses not accepted

**Fix:**

```powershell
sdkmanager --licenses
sdkmanager "platforms;android-34" "build-tools;34.0.0"
```

### Dependency resolution fails (offline)

The app makes no cloud calls at runtime, but Gradle needs network access **once** to download dependencies. If behind a proxy, configure `gradle.properties` proxy settings. For fully offline builds, pre-populate the Gradle cache.

### `allowBackup` concern

`android:allowBackup="true"` in the manifest can include pairing records in cloud backups. For a security-sensitive app, set it to `false` before release.

## Windows

### No solution or project files

**Symptom:** `dotnet build` fails with "MSB1009: Project file does not exist" or no `.sln`.

**Fix:** create/attach the solution and projects (see [setup-windows.md](setup-windows.md)):

```powershell
cd windows
dotnet new sln -n MayaRemote
dotnet sln add (Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { $_.FullName })
```

### WPF workload missing

**Symptom:** build errors about WindowsDesktop SDK or WPF targets.

**Fix:** install the ".NET desktop development" workload in the Visual Studio Installer, or install the Windows Desktop targeting pack for the SDK version.

### NuGet restore fails

**Fix:**

```powershell
cd windows
dotnet restore --force
```

Check `nuget.config` and network/proxy settings.

### Input/screen features don't work

The `MayaRemote.WindowsServices` capabilities (input injection, screen capture) require an interactive desktop and appropriate permissions. Run in a normal user session; if running as a service/agent, ensure it has access to the interactive desktop. See [setup-windows.md](setup-windows.md) Permissions.

## Connectivity (both platforms)

### Devices can't see each other (discovery)

- Confirm both are on the **same subnet/VLAN**; multicast may not cross subnets.
- Disable **AP isolation** on the Wi-Fi.
- Allow the app through the firewall on the private network.
- Fallback: enter the host's IP address and pairing code manually (discovery is a convenience, not a requirement).

### "Network is unreachable" (3001) / "Connection timed out" (3002)

- Verify the Windows host is listening on the configured port.
- Check the Windows firewall inbound rule for the app/port.
- Confirm no VPN is routing local traffic away from the LAN.
- Verify the address/port the client is dialing matches the host config (`Infrastructure/Configuration`).

### Pairing fails

| Error | Likely cause | Fix |
|-------|--------------|-----|
| `INVALID_PAIRING_CODE` (1001) | Typo or wrong device | Re-enter; scan QR instead of typing. |
| `PAIRING_EXPIRED` (1002) | Code timed out | Regenerate the code and retry quickly. |
| `PAIRING_REJECTED` (1003) | User declined | Confirm the prompt and retry. |

### "Device is not authorized" (2001)

The session authentication failed — the long-term credential is missing or doesn't match. Re-pair the devices.

### "Device access has been revoked" (6001)

The peer was revoked. Re-pair from a trusted device to restore access.

### "Protocol version is not supported" (5001)

The two apps are on different protocol versions. Update both to a compatible version (see [compatibility.md](../protocol/compatibility.md)).

### "TLS validation failed" (4001)

Transport validation failed. Ensure both devices have synchronized local time (certificates/keys are time-sensitive) and that no TLS-intercepting proxy is on the path.

### "Remote control is disabled" (7001)

Remote control was turned off in Security Settings. Re-enable it to use input/screen/audio.

## Build scripts

- **`scripts/build-android.ps1`** requires the Gradle wrapper; generate it first.
- **`scripts/build-windows.ps1`** requires `.sln`/`.csproj`.
- **`scripts/test-all.ps1`** requires both toolchains; each sub-run reports independently.
- **`scripts/clean.ps1`** deletes Android `build/` and Windows `bin/`/`obj/`; re-run a build afterward.

## Getting more help

- Check logs (with secrets redacted) — never share keys, pairing codes, or payloads.
- Search existing issues; open a new issue with platform, version, and reproduction steps.
- For security issues, follow [SECURITY.md](../../SECURITY.md) instead of a public issue.

## Related

- [Build Instructions](build-instructions.md)
- [Testing Guide](testing-guide.md)
- [Networking](../architecture/networking.md)
- [Error Codes](../protocol/error-codes.md)