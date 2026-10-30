# Android Internal Test build

RC1 configuration is `com.gltam.lowfire`, launcher **Lowfire**, `1.0.0-rc1` / versionCode **2**, Unity **6000.3.19f1**, IL2CPP / ARM64, min API25 / target API36. Earlier local development bundles used code1; the user confirmed no Play uploads and no pre-existing dedicated upload key. Increment versionCode for subsequent uploads.

The Android define is `LOWFIRE_ALPHA`. The verified environment is `alpha` / `a9eee8ca-e2a8-4fa7-9530-6df9b230e943`. Missing/conflicting environment mappings always fail. Offline Alpha may package with `BackendSettings.Enabled=false`; enabling accounts still requires real Google/Privacy configuration. Production retains all strict external setup gates. Ads, live ads and real purchases remain OFF; this is not a Production release.

Use an isolated ASCII project/output path and mirror canonical Assets/Packages/ProjectSettings, preserving only safe Library caches. Never copy user UserData or a generated fake PGS setup. `InternalTestBuild.CheapAndroid` validates product data, exact configuration, audio101 and non-development Android Alpha/Production assemblies. Run focused account/environment/save/ads/store EditMode and PlayMode checks before packaging. `InternalTestBuild.Export` exports a non-development Gradle project, with all profiling/debugging options off. It requires batch mode and explicit `LOWFIRE_RC1_PROJECT`, `LOWFIRE_RC1_RESULTS`, `LOWFIRE_RC1_EXPORT` absolute ASCII paths. The cheap gate also requires `LOWFIRE_BACKEND_RESULTS`.

## Stable upload signing

`python tools/android_upload_signing.py init` explicitly creates/reuses a dedicated stable upload key **outside Git**. Windows CurrentUser DPAPI encrypts its password; NTFS ACLs restrict the directory to that user and SYSTEM. An existing partial/mismatched key state fails; there is no disposable/debug fallback. `public` prints only public certificate metadata.

Unity exports an **unsigned Gradle project**, not a debug-signed AAB. The canonical launcher template signs release tasks with the stable upload key using process environment credentials; it rejects missing release credentials. Unity PlayerSettings never receives the password. `python tools/android_upload_signing.py run --project <Gradle-export> --log <outside-Git-log> :launcher:processReleaseMainManifest` performs the merged-manifest gate. After inspecting dependencies/configuration/manifest, use the same command with `:launcher:bundleRelease` **once**. Configuration cache and persistent Gradle daemon are disabled. No secrets belong in command arguments, generated source, metadata or logs.

Securely back up the private keystore and encrypted DPAPI material, including the original Windows profile/recovery keys needed to decrypt it. Copying a DPAPI file alone to a new Windows user is insufficient. Never send credentials in chat. Public upload certificate fingerprints differ from Google's app signing certificate after Play App Signing; configure PGS/OAuth against the appropriate installed build certificate.

Keep the AAB, native symbol archive, exact build-source SHA, cert fingerprints, SHA256, manifest/ABI/page-size audit and attempt timing outside Git. Final validation goes in [RC1 review](../validation/internal-test-rc1/REVIEW.md). A documentation-only closing commit does not require a second AAB; record its SHA separately from the frozen build-source SHA.

## Current Google requirements checked 2026-10-30

| Requirement | RC1 policy | Official source |
|---|---|---|
| New app/update target API36 since 2026-08-31 | Explicit target36, preserve min25 | [Target API](https://developer.android.com/google/play/requirements/target-sdk) |
| Android App Bundle / Play App Signing | Release AAB signed by stable upload key; user enrolls/selects app signing during first upload | [App bundles](https://developer.android.com/guide/app-bundle), [App signing](https://developer.android.com/studio/publish/app-signing) |
| 64-bit native code | ARM64 only; inspect actual bundle libraries | [64-bit](https://developer.android.com/google/play/requirements/64-bit) |
| 16KB native/page alignment | Verify actual ELF PT_LOAD and bundle alignment, then device test separately | [16KB guidance](https://developer.android.com/guide/practices/page-sizes) |
| Supported Billing version | Existing Billing9.1.0 retained; purchasing OFF | [Billing deadlines](https://developer.android.com/google/play/billing/deprecation-faq) |

PGS/OAuth, Integrity, public Privacy/Delete URLs, actual SSV/purchases, Android device/16KB device, physical four-player and Alpha internet Relay remain separate functional/release gates. Upload readiness describes the bundle, not verified Play Console acceptance or completed external setup. No automatic Console upload, release, tester management or Production deployment.
