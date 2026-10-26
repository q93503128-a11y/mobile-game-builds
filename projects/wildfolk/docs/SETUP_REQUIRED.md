# Launch Backend · 외부 설정 체크리스트

Foundation1.5 현재 상태: **PARTIALLY ACTIVE**. development/alpha 실제 환경, Cloud Code2개, Private policy/gate, Protected Support index와 Access Control을 배포했다. development 실제 서비스76개와 Unity SDK 왕복·인터넷 Relay를 확인했다. 제품 `BackendSettings.Enabled=false`는 Google/Privacy 준비까지 유지한다. [검증 정본](../validation/launch-backend-activation/REVIEW.md).

## 완료된 공개 설정

| 항목 | 현재 값/상태 |
|---|---|
| UGS Project | 70ba2490-7029-4528-b55a-9c2231fde007 |
| development | 9aa02b53-8272-4975-a42a-f6a0b75a360f |
| alpha | a9eee8ca-e2a8-4fa7-9530-6df9b230e943 |
| production | 9da27a5f-4c79-44bc-b96b-1e490f83619d, 기존 default 유지 |
| 최종 Android package | **com.gltam.lowfire**, 사용자 확정값 |
| non-production scripts | LowfireAccount / LowfireIntegrityVerify |
| Protected index | lf_support_1, READY |
| Alpha window | 닫힘, 날짜·승인 build/cert 목록 미설정 |
| Production mutation | 이번 패스0. 정책/스크립트도 배포하지 않음 |

## 사용자/운영자 외부 작업

| 작업 | 현재 상태 | 담당/위치 | 필요한 값 | 완료 확인 |
|---|---|---|---|---|
| Play 앱 생성·PGS v2 연결 | 사용자 확인: 미설정 | 사용자, Play Console→게임 앱/Play Games Services | com.gltam.lowfire, 실제 Google Cloud 프로젝트 | PGS 설정과 앱 package 일치 |
| Android OAuth | 미설정 | 사용자, PGS/Google Cloud Credentials | 실제 package와 서명 SHA-1 | debug/closed track 기기 인증 |
| Web/Game server OAuth | 미설정 | 사용자, Google Cloud Credentials | 공개 Web client ID와 비공개 secret | UGS Google Play Games provider link/sign-in 성공 |
| Plugin resources/manifest | PlayGamesSettings 및 생성 manifest 없음 | 사용자, Unity Google Play Games Android Setup | 실제 PGS resources/app ID, 같은 Web client ID | 생성 resource/manifest와 BackendSettings 일치, release validator 통과 |
| UGS Google Play Games provider | remote 상태 UNVERIFIED, OAuth 미생성 | 사용자, UGS Dashboard Authentication→Identity Providers | 위 Web client ID/secret | 익명 Link, AccountAlreadyLinked, 복구를 실기기에서 검증 |
| Google project number | 사용자 확인: 없음 | 사용자, Google Cloud project/Play Integrity | 실제 숫자 project number | BackendSettings와 Play 연결 project 일치 |
| Play Integrity | 실제 Google 호출 미검증 | 사용자, Play Console App integrity/Google Cloud, UGS Secret Manager | API 활성화·연결, cert digest/versionCode, service credential | 실제 alpha 앱 requestHash/package/cert/version/license/time 승인·거절 |
| Privacy Policy | 사용자 확인: URL 없음 | 사용자/정책 담당, 실제 공개 HTTPS 사이트와 Play Console Data safety | 실제 수집·보관·삭제 정책 URL | 앱 링크와 Data safety 고지 일치 |
| Account Deletion web | 사용자 확인: URL 없음 | 사용자/운영 담당, 실제 공개 요청 페이지/endpoint | 앱 재설치 없이 요청 가능한 HTTPS URL | 본인 확인→요청→모든 환경 cleanup→Auth 삭제→완료 통지 |
| Android signing | production keystore 미지정 | 사용자, Play App Signing/Unity Publishing | 실제 upload key·Play signing cert | 설치 track별 실제 cert와 OAuth/Integrity allowlist 일치 |
| Alpha 개시 | 닫힘 | 운영자, alpha Private policy | 실제 시작/끝 UTC, 승인 app version/versionCodes/cert | linked 실제 기기 한 계정 등록·중복 안전성 |
| 운영 retention/삭제 | 담당·주기 미확정 | 운영자, Player Management/특권 도구 | 최소 역할 service account와 보호된 journal | 비활성 ticket30일 처리, 부분 실패 resume, 로그 retention |
| Production 준비 | 기존 환경 보존, 미배포 | 별도 출시 작업, 운영자 | 검토된 policy/index/scripts/Access Control | production 전용 검수 후 활성화. QA 대체 환경으로 사용 금지 |

비밀값은 채팅/Git/Assets/명령 인수/로그에 넣지 않는다. OAuth secret은 UGS provider Dashboard, Google credential은 Secret Manager `LOWFIRE_GOOGLE_INTEGRITY_SERVICE_ACCOUNT`, 특권 CLI key는 환경 변수 또는 승인 local secret store를 사용자가 직접 설정한다. 공개 client ID/project number와 secret을 혼동하지 않는다. 임의 ID·URL·keystore를 만들지 않았다.

Debug keystore, upload key, Play App Signing key의 SHA-1/SHA-256은 서로 다를 수 있다. 실제 설치 APK를 서명한 인증서로 Android OAuth와 Integrity digest를 등록한다. 현재 fingerprint 값을 추정하지 않았다. PGS resources의 app ID와 Google Cloud project number도 별도 항목으로 확인한다.

## 재현 가능한 운영 명령

작업 디렉터리는 projects/wildfolk. 공식 UGS CLI2.0.0을 사용한다. 이번에는 공식 GitHub 실행 파일을 저장소 밖에 설치하고 Unity Hub 인증으로 실행했다. 아래 인증은 게임 사용자 Authentication과 별개다.

```powershell
$env:UGS_CLI_USE_HUB_AUTH = 'true'
ugs login --unity-hub
ugs env list -p 70ba2490-7029-4528-b55a-9c2231fde007 --json
npm.cmd --prefix backend ci --ignore-scripts
node backend/tools/sync-catalog.cjs
node backend/tools/bundle.cjs
ugs deploy backend/deploy/LowfireAccount.js backend/deploy/LowfireIntegrityVerify.js backend/access/lowfire.ac -p 70ba2490-7029-4528-b55a-9c2231fde007 -e development --dry-run --json
ugs deploy backend/deploy/LowfireAccount.js backend/deploy/LowfireIntegrityVerify.js backend/access/lowfire.ac -p 70ba2490-7029-4528-b55a-9c2231fde007 -e development --json
ugs cloud-code scripts get LowfireAccount -p 70ba2490-7029-4528-b55a-9c2231fde007 -e development --versions --json
ugs access project-policy list -p 70ba2490-7029-4528-b55a-9c2231fde007 -e development --json
ugs cloud-save data index list -p 70ba2490-7029-4528-b55a-9c2231fde007 -e development --json
```

환경 이름을 반드시 명시하고 --reconcile이나 production 교체를 QA에 쓰지 않는다. 기존 development/alpha/gate/index는 생성 완료했으므로 재생성하지 않는다. Private policy 변경은 현재 값과 writeLock을 조회하고 검토한 차이만 CAS로 쓴다. 기존 gate owner의 무조건 덮어쓰기/자동 expiry는 금지한다. 공개 policy 정본은 backend/config/development.policy.json / alpha.policy.json이다.

기존 backend/tools/admin.cjs는 service account 환경 변수 LOWFIRE_UGS_KEY_ID / LOWFIRE_UGS_SECRET_KEY를 사용한다. lookup/export-alpha/migrate-alpha/delete-account 명령은 --project/--environment 등 범위를 명시한다. migration은 source를 다시 읽는 dry-run→candidate/duplicate/already-granted/grant plan 확인→승인된 운영 apply 순서이며 이번에 production 지급은 실행하지 않았다. 삭제는 모든 실제 환경 ID의 exact-set 검사, durable journal, --apply --confirm DELETE를 요구한다. Remote Config 환경 목록 read 역할도 필요하다. 상세는 [운영 정본](BACKEND_OPERATIONS.md).

## 활성화 전 최종 확인

PGS 설정과 실제 URL을 완료한 뒤에만 BackendSettings.Enabled=true. LOWFIRE_ALPHA 또는 LOWFIRE_PRODUCTION 중 의도한 한 channel을 지정하고 앱을 재시작한다. 환경 초기화/인증 ID mismatch는 fail-closed이며 production fallback은 없다. 실제 Android/Google 복구·Integrity·삭제 전체 흐름, AAB/IL2CPP/Gradle·실기기·4기기 검증을 진행한다. 현금 결제는 계속 false다. [출시 차단 항목](RELEASE_BLOCKERS.md)을 함께 확인한다.

## PGS 설정 전 Development Android 빌드 확인

Google/URL 값이 없을 때도 기존 Development 채널의 빌드 도구 체인은 별도로 검사할 수 있다. `AccountCompileQa.DevelopmentBundle`은 `-batchmode -buildTarget Android`로 실행하며, `LOWFIRE_QA_PROJECT`가 현재 프로젝트의 절대 경로와 같고 `/tests/artifacts/unity`로 끝나는 격리 복사본에서만 동작한다. 프로젝트와 결과 폴더는 모두 영문/ASCII 경로를 쓴다(예: `C:/UnityProjects/LowfireAndroidQa/tests/artifacts/unity`, `C:/UnityProjects/LowfireAndroidQa/results`). Android 사전검사는 현재 저장소의 한글 경로를 거절하므로 저장소 자체를 옮기지 않고 복사본을 사용한다. Assets/Packages/ProjectSettings를 복사하되 사용자 UserData나 credential은 복사하지 않는다. `LOWFIRE_BACKEND_RESULTS`에는 Assets 밖의 결과 폴더를 지정한다.

이 경로는 Enabled=false·AllowDevelopmentIdentity=false·custom keystore 미사용·release define 미지정·IL2CPP/ARM64를 요구한다. 실제 제품 씬을 `BuildOptions.Development`와 표준 debug 서명으로 AAB 빌드하고 결과 JSON을 기록한다. Alpha/Production validator나 Google 설정 검사를 우회하지 않는다. 이 개발 AAB의 성공은 실제 PGS 로그인·Alpha/Production AAB·출시 서명·기기 실행 성공을 뜻하지 않는다. 없는 Google resource/manifest는 설정 완료로 표시하지 않는다.

2026-10-26: 빈 미설정 PGS `.androidlib` skeleton 제거 후 실제 Development AAB가 생성됐다. PGS Setup은 실제 값 입력 시 폴더/manifest를 다시 만든다. [실제 후속 검증](../validation/launch-backend-activation/android-development/REVIEW.md). 공개 Google 값/URL 부재와 release 차단은 그대로다.
