# Internal Test RC1 · Signed Android AAB

**READY — AAB internal upload preparation only. Android actual device and Play Console upload are UNVERIFIED.**

Read/fetched dev-playbook main e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7 and project main before changes. Main direct; no branch/PR. Canonical runtime/content unchanged; Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3/IAP Schema1 retained.

| # | Item | Actual result / evidence |
|---|---|---|
| 1 | 시작 SHA | bd97d25b48b537eaa412ce45cd67f0c0e1d9513c, fetched clean main |
| 2 | 종료/빌드 SHA | aebe188458b8d5b12b08757847e8b779a164b169 (frozen build source). Closing documentation commit/remote/clean exact SHA is recorded after push in external FINAL_GIT.json; the tracked report cannot embed its own commit hash. |
| 3 | Build purpose | First signed internal-test RC; no gameplay/content/balance/Audio/Production changes. |
| 4 | Build channel | LOWFIRE_ALPHA; generated IL2CPP BackendPolicy.get_Channel returns Alpha=1. |
| 5 | Backend environment | alpha / a9eee8ca-e2a8-4fa7-9530-6df9b230e943; no fallback. Seven published Alpha scripts match local bundle hashes; no deployments. |
| 6 | Package | com.gltam.lowfire; actual bundle manifest and authorities verified. |
| 7 | Display name | Lowfire; actual bundletool string/app_name=Lowfire. |
| 8 | Unity | 6000.3.19f1 LTS / Universal2D |
| 9 | Target/min SDK | 36 / 25, explicit in source, merged manifest and actual bundle. |
| 10 | Architecture | ARM64 only; no unexpected ABI. |
| 11 | Scripting backend | IL2CPP |
| 12 | Development state | Development / Script Debugging / Autoconnect Profiler / Deep Profiling OFF. debuggable/testOnly absent=false. |
| 13 | versionName | 1.0.0-rc1 |
| 14 | versionCode | 2; prior local development history code1. User confirms no prior Play upload or dedicated key. |
| 15 | Signing | Stable RSA3072/SHA256withRSA upload key; unsigned Unity Gradle export → environment-only release Gradle signing; no debug-signed AAB or disposable key. |
| 16 | Upload certificate | SHA1 BE12EFDE782B5E7A6BEF03A781FB35DA9BA990AD; SHA256 0FFAC69CEC133491E20E3AC51A716AB1864144AD589CE714AC1DEB4453522FCB. Google app signing certificate is separate. |
| 17 | Secrets | CurrentUser DPAPI + restricted NTFS ACL; key outside Git; no credential argv/source/metadata/log output. Source2214 text files scanned; final staging5493 files + logs + AAB exact password matches0. |
| 18 | Manifest | Release merged and actual protobuf-decoded manifest audited: launcher/SDK activities, services, receivers, providers, queries; explicit exported where intent filters. No foreign product authorities. cleartext defaults false at target36; no custom network policy. allowBackup is unspecified, so Android platform backup default is retained (device restore unverified). |
| 19 | Permissions | INTERNET/ACCESS_NETWORK_STATE; Ads AD_ID/AD_SERVICES; BILLING; WAKE_LOCK; FOREGROUND_SERVICE; signature receiver permission. No location/contact/storage/mic/camera/phone permissions. WAKE_LOCK comes from measurement/WorkManager and FOREGROUND_SERVICE from AndroidX WorkManager; retained SDK requirements. Exported jobs/diagnostics/profile installer are permission-protected; PGS shortcut activity is vendor-owned, not a Lowfire cheat. |
| 20 | Ads | Client Enabled/PrivacyReviewed/ProductionLive OFF; official sample App ID only; Alpha server Enabled/SSV/live OFF. SDK included, no ad request/reward claimed. |
| 21 | IAP | Client Purchase/ProductionLive/ExternalReviewed OFF; Alpha always disallows real money; remote Alpha Purchase/Settlement/Live OFF and TestOnly=true. Billing9.1.0 present. |
| 22 | PGS | Actual OAuth/app setup absent; no invented IDs. Connect/Sync/Alpha registration disabled offline. PGS build warning retained; actual Android startup/login UNVERIFIED. |
| 23 | Integrity | No actual project number/config. Unavailable path fails closed; no fake verdict or device success. |
| 24 | QA/debug contamination | Release Alpha/Production59 assemblies each exclude development identity/ads/billing types. Generated player C++ excludes them too. AAB no managed test DLL/C#/private-key/DoNotShip payload; boot native debugger wait0. SDK protected diagnostic/profile receiver is distinct from gameplay QA. |
| 25 | Cheap gates | Data0errors; Edit47/47 unique and Play28/28 unique final passes; native59/59; Alpha UI/New/Continue Save14/Profile2 smoke. First cheap result-path guard failed before AAB, fixed ASCII orchestration. Initial test harness failures and focused retries retained in tests.json. |
| 26 | AAB attempts | 1 successful final attempt, no retries and no second Production/confirmation AAB. |
| 27 | AAB timing | 2026-10-30T12:07:35.909168+00:00 → 2026-10-30T12:31:09.773684+00:00; total 1413.865s; export process 361.563s, Gradle package 784.746s. Remaining interval is manifest/dependency/inspection gating, not extra packaging. |
| 28 | AAB path | C:\Users\User\Documents\LowfireBuilds\InternalTest\RC1\Lowfire-Internal-RC1-v1.0.0-rc1-2.aab |
| 29 | AAB bytes | 66,119,741 bytes / 66.119741MB / 63.057MiB |
| 30 | AAB SHA256 | a626cd3ac0cc2582658aaa3f9a47d74b9ae3d428e6a5904ea822e5416644e2d0 |
| 31 | Bundle inspection | Jarsigner verified and upload certificate matched; ZIP CRC and all822 payload SHA256 manifest digests/SF manifest digest pass. Self-signed upload chain/no timestamp/JarInputStream ordering warnings retained; JarFile and independent payload validation pass. No actual Console acceptance claimed. |
| 32 | Native libraries | 7 arm64-v8a libraries: burst, libc++, game, il2cpp, main, swappywrapper, unity. Every PT_LOAD alignment16384; bundle PAGE_ALIGNMENT_16K. Actual16KB-device behavior UNVERIFIED. |
| 33 | Audio / size delta | Actual AAB serialized assets: Audio101, streams7,709,440 bytes, MixerController1 and groups/snapshot, settings3 exact canonical JSON. Foundation4 actual65,241,833 bytes → RC1+877,908 (+1.346%). Older baseline was Development and had different stripping/content: net delta is not audio alone. Compressed native36,652,594 / UnityData21,703,824 / dex7,106,714 bytes; no unexplained hundreds-of-MB growth. |
| 34 | Symbols | C:\Users\User\Documents\LowfireBuilds\InternalTest\RC1\Lowfire-Internal-RC1-native-symbols-full.zip; 259,139,063 bytes; SHA256 d8bd6f38dcd1f101b1ff516a1101e1ce2b5737aeeabc5810aa9978cff7bc5599. il2cpp/main/unity ELF Build IDs match actual bundle. FULL IL2CPP debug + available engine symbols preserved separately, not in AAB. Other vendor/Burst full symbols unavailable; Burst text kept in export. Java mapping N/A (minify OFF). |
| 35 | User save protection | Existing world primary/backup2 files exactly unchanged SHA256/bytes/mtime; no existing Profile2 file created/modified. QA-only Profile2 verified. Isolated selection cleared; QA files archived as CompletedEvidence; real saves never loaded as test input. |
| 36 | Actual Android device | UNVERIFIED: no install, touchscreen, thermal, mobile audio focus, Play-distributed cold launch, backup restore or physical multiplayer claimed. |
| 37 | Feature matrix | See table below; Editor/previous source evidence is separated from Android device verification. |
| 38 | Upload readiness | READY for signed-AAB internal upload preparation; no known bundle signing/package/manifest/ABI blocker. Console app/account setup and actual acceptance are user-side UNVERIFIED. |
| 39 | Remaining blockers | PGS/OAuth, Integrity, public Privacy/Delete URLs, owned AdMob/real SSV, Play products/merchant/Developer API and actual purchase/refund, Alpha Relay internet/physical4-player, device performance/audio/restore, Production activation. No fake settings or deploys. |
| 40 | Next Console steps | Use UPLOAD_CHECKLIST.md; user configures app signing, uploads AAB/symbols, notes/testers/internal rollout. No automated upload/release/tester change. |
| 41 | Modified files | 13 source/build/test/docs files in build-source commit; completion adds review/evidence/checklist and README/SETUP_REQUIRED/RELEASE_BLOCKERS/Android doc status. See changes.txt. |
| 42 | Local HEAD | Frozen build source above; final documentation HEAD is the commit containing this report. Exact value in external FINAL_GIT.json after completion. |
| 43 | Remote main | Source commit pushed; final documentation push/equality verified in external FINAL_GIT.json. |
| 44 | Git status | Final clean canonical worktree required; external FINAL_GIT.json records actual closing state. Build/signing/artifacts remain outside Git. |

## Feature matrix

| Capability | RC1 state | Verification boundary |
|---|---|---|
| Offline/core play, Save14/Profile2 | EXPECTED / Editor smoke passed | New → save → home → same-seed Continue; full1.0 device progression unverified |
| Audio/UI | Included, Editor smoke passed | Actual bundle101clips/mixer; PC playback2sources; no Android listening claim |
| LAN | EXPECTED, unchanged | Prior pass LAN evidence only; not repeated or physical-device verified here |
| Real money / live ads | DISABLED BY POLICY | Client plus read-only Alpha server flags OFF |
| PGS / cloud account / Alpha enrollment | BLOCKED BY EXTERNAL SETUP | Offline UI unavailable; no Google success claimed |
| Integrity / real SSV / real purchase | BLOCKED BY EXTERNAL SETUP | No fake config/verdict/payment |
| Alpha Relay internet / physical4-player | UNVERIFIED | Development Relay history does not verify this Alpha build |
| Public/Production release | NOT READY | Existing RELEASE_BLOCKERS retained |

## Corrections within packaging scope

Launcher Wildfolk→Lowfire, app version/code and explicit target36, canonical Android Alpha define, env-only stable upload signing/export entry, offline Alpha build gate while preserving active-account/Production strictness. Channel compile helper now removes competing channel symbols before each variant. Added4 negative/positive policy tests. Existing account UI test now expects current Meta Hub back route; runtime UI was not changed. No gameplay/balance/Audio refactor.

## Signing and recovery

Keystore: `C:\Users\User\AppData\Local\Lowfire\Signing\lowfire-upload.p12`; alias `lowfire-upload`; created 2026-10-30T11:51:46.141670+00:00. Password exists only as Windows CurrentUser DPAPI encrypted material and short-lived process environment. Backup required: keystore plus DPAPI material AND recoverable original Windows user/profile keys; ciphertext alone is not portable recovery. Public certificate beside AAB. No credential value is in this report, Git or BUILD_METADATA.

## Current official requirements / limitations

[Target API36](https://developer.android.com/google/play/requirements/target-sdk), [AAB](https://developer.android.com/guide/app-bundle), [signing](https://developer.android.com/studio/publish/app-signing), [64-bit](https://developer.android.com/google/play/requirements/64-bit), [16KB](https://developer.android.com/guide/practices/page-sizes), [Billing deadlines](https://developer.android.com/google/play/billing/deprecation-faq), [native symbols](https://developer.android.com/build/include-native-symbols) checked2026-10-30. Androids current guidance allows1.6GB symbols; preserved raw symbols933,529,960 bytes are below it. SDK deprecations, known Editor QA warnings and PGS unavailable warning are retained, not represented as zero warnings (Unity export summary74 warnings/0errors). Ads OFF does not establish zero SDK data collection; device behavior and disclosure must be reviewed before wider release. Platform backup default remains and must be tested with isolated restore data.

## Timing / usage

See timing.json. Each primary wall phase is one row; parallel tests/import/native work are not summed twice. Native export process361.563s (Unity report309.487s), Gradle package784.746s, single full attempt1413.865s. Per-operation token/quota usage is unavailable and recorded null, never estimated. Final push completion is stamped outside Git in FINAL_GIT.json.

STOP after signed artifact/validation/Git delivery. No Play upload, tester mutation, Production activation or next gameplay/polish pass.
