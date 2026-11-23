# Android Setup

This guide covers setting up a development environment for the Android component (`android/`).

## Prerequisites

- **JDK 17** (required by the project's Java/Kotlin target).
- **Android Studio** (Giraffe/Koala or newer) with the Android SDK installed.
- **Android SDK Platform 34** (`compileSdk`/`targetSdk = 34`).
- **Android SDK Build-Tools** compatible with AGP 8.5.
- A device or emulator running **Android 8.0 (API 26)** or newer (`minSdk 26`).
- Internet access **once** for Gradle/Maven dependency resolution (the app itself makes no cloud calls at runtime).

Verify:

```powershell
java -version
# open Android Studio → SDK Manager to confirm Platform 34 + Build-Tools
```

## Project layout

```
android/
├─ build.gradle.kts
├─ settings.gradle.kts
├─ gradle.properties
└─ app/
   ├─ build.gradle.kts
   └─ src/
      ├─ main/        # code + AndroidManifest.xml
      ├─ test/        # unit tests
      └─ androidTest/ # instrumented tests
```

Key facts from `app/build.gradle.kts`:

- namespace / applicationId: `com.maya.remote`
- `minSdk 26`, `compileSdk 34`, `targetSdk 34`
- Java/Kotlin target: 17
- Compose BOM `2024.06.00`, Hilt `2.51`, Coroutines `1.8.1`

## Generate the Gradle wrapper (one time)

The repository does **not** include the Gradle wrapper (`gradlew`, `gradle/wrapper/`). Generate it with a locally installed Gradle or via Android Studio:

**Option A — Android Studio:** open the `android/` folder as a project; Studio creates the wrapper and syncs.

**Option B — command line** (requires a local Gradle install):

```powershell
cd android
gradle wrapper --gradle-version 8.7
```

Then use `.\gradlew` for all subsequent commands (see `scripts/build-android.ps1`).

## Build

```powershell
# from the repository root
.\scripts\build-android.ps1
```

Or directly:

```powershell
cd android
.\gradlew.bat :app:assembleDebug
```

## Run

- **Emulator:** create an AVD (API 26+) in Android Studio and click Run.
- **Physical device:** enable USB debugging, connect, then:

```powershell
cd android
.\gradlew.bat :app:installDebug
```

## Tests

```powershell
cd android
.\gradlew.bat :app:testDebugUnitTest        # JVM unit tests
.\gradlew.bat :app:connectedDebugAndroidTest # needs a device/emulator
```

See [testing guide](testing-guide.md).

## Known setup issues

- **Missing `MainActivity`:** `AndroidManifest.xml` references `.MainActivity`, but no `MainActivity.kt` exists yet. Add one before the app will launch:
  - a class extending `ComponentActivity`, annotated `@AndroidEntryPoint`, setting Compose content.
- **Missing wrapper:** see above.
- **SDK licenses:** accept licenses via `sdkmanager --licenses` if Gradle complains.
- **JDK mismatch:** Gradle fails if the active JDK is not 17; set `JAVA_HOME` accordingly.

## Manifest review before release

- `android:allowBackup="true"` is currently set. For a security-sensitive local app, consider `allowBackup="false"` or narrowly scoped backup rules so pairing records are not exported.

## Related

- [Windows Setup](setup-windows.md)
- [Build Instructions](build-instructions.md)
- [Testing Guide](testing-guide.md)
- [Troubleshooting](troubleshooting.md)