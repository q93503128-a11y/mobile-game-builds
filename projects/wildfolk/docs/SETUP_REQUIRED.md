# Launch Backend · 외부 설정 체크리스트

현재 상태: 외부 설정/배포 **미확인**, BackendSettings.Enabled=false. 이 문서는 실제 서비스 설정이 완료되었다는 기록이 아니다. 신규 secret/가짜 project/environment/OAuth ID를 만들지 않았다.

## 1. 기존 UGS 프로젝트/환경 보호

- 현재 ProjectSettings cloudProjectId: `70ba2490-7029-4528-b55a-9c2231fde007`, organization `q93503128`. UnityConnectSettings m_Enabled=0. 이 값은 secret이 아니다.
- Core Editor API 읽기 조회로 production(ID `9da27a5f-4c79-44bc-b96b-1e490f83619d`) 한 개를 확인해 공개 config에 반영했다. development/alpha는 아직 없다. Editor 활성 selector는 빈 값/Guid.Empty다. Dashboard에서 기존 서비스·Relay 사용 환경을 추가 확인한다. 기존 환경의 삭제/rename/이관 금지. 현재 SDK는 명시 환경 override가 없는 legacy default 경로다.
- development / alpha / production을 실제 확인·생성하고 각 name/ID를 Resources/BackendSettings.json Environments에 넣는다. 이름을 추정해서 ID를 채우지 않는다.
- Authentication은 project identity, Cloud Save/Cloud Code 정책·데이터는 environment별임을 확인한다. 기존 cached SDK profile `default`를 재사용한다.
- 테스트는 격리 QA identity와 development/alpha에서만 한다. 기존 production 사용자 데이터를 QA import하지 않는다.

## 2. Google Play Games / Android

- Play Console에서 실제 Android application ID·서명 인증서 SHA를 확인한다. 게임 PGS v2 설정, Android OAuth credential, Web/Game server OAuth credential, tester/track를 연결한다.
- 공식 PGS plugin2.3.0을 import했다. 번들 EDM4U1.2.182 하나를 사용한다. Plugin이 요구하는 Games v2 Android dependency22.1.0과 Integrity1.6.0의 Gradle resolution을 확인한다.
- Google Play Games Android Setup에 **공개 Web client ID**와 실제 PGS resource 설정을 입력해 AndroidManifest를 생성한다. 현재 빈 값을 가짜 ID로 대체하지 않았다.
- Unity Authentication Dashboard ID Provider를 Google Play Games로 추가한다. Web client secret은 Dashboard에만 입력하며 Assets/Git/로그에 넣지 않는다.
- BackendSettings의 GoogleWebClientId, GoogleCloudProjectNumber는 공개 config다. Google OAuth secret/private key/access/refresh token은 여기에 넣지 않는다.
- Android 빌드·PGS 실제 로그인·cached guest link·AccountAlreadyLinked·기기 변경·실패/offline·closed/alpha track UX는 사용자 실기기 검증 대상이다.

## 3. Cloud Save / Cloud Code

- Cloud Save와 Cloud Code를 각 환경에서 활성화하고 서비스 역할을 확인한다.
- Protected player key lf_support의 equality query index를 만든다. index 생성 전 자료는 자동 검색 가능하다고 가정하지 않는다. Support lookup 결과0/2개 이상은 거절한다.
- backend/config/policy.example.json을 실제 환경별 local 설정 파일로 복사해 environmentId/channel/window/approved app versions/certificates/Android versionCodes를 채운다. Production allowLegacyImport=false, 모든 realMoneyPurchase=false.
- 아래 Admin setup은 먼저 dry-run, 검토 후 --apply. credentials는 환경 변수 LOWFIRE_UGS_KEY_ID / LOWFIRE_UGS_SECRET_KEY만 사용한다. 실제 값은 명령문/스크린샷/Git에 남기지 않는다.
- `node backend/tools/admin.cjs setup --project $env:LOWFIRE_UGS_PROJECT_ID --environment $env:LOWFIRE_UGS_ENVIRONMENT_ID --policy <검토한-local-policy-file> --dry-run`
- 검토 후 같은 명령의 --dry-run을 --apply로 바꾼다. Private Game Data lowfire_backend의 creation_gate를 최초 provisioning하고 policy를 저장한다. 기존 gate owner를 setup이 덮어쓰지 않는다.
- 생성 gate가 잠긴 채 crash/unknown commit이면 자동 강제 해제하지 않는다. Cloud Code 실행과 마지막 Cloud Save 요청이 모두 종료된 뒤 profile 생성 결과를 확인하고 운영자가 복구한다.
- `node backend/tools/sync-catalog.cjs`, `node backend/tools/bundle.cjs` 후 backend/deploy/LowfireAccount.js를 **LowfireAccount**라는 JS script로 배포한다. operation/channel/buildVersion(String required), input(JSON) parameter 설정을 확인하고 Publish한다.
- backend/access/lowfire.ac를 환경별로 배포한다. Cloud Save player Write만 막고 query는 허용한다. Authentication/Relay API는 차단 대상에 넣지 않았다. 실제 direct-write rejection과 Cloud Code write, 기존 Relay Host/Join을 별도로 검증한다.

## 4. Alpha 빌드 증명 / secrets

- Google Play Integrity API를 실제 Play 앱/Cloud project에 연결한다. package/versionCode/certificate allowlist와 enrollment UTC start/end를 server policy에 넣는다. alphaOpen은 운영자가 창을 열 때만 true.
- Google Integrity API 호출 역할을 가진 최소 service account credential JSON은 **UGS Secret Manager** `LOWFIRE_GOOGLE_INTEGRITY_SERVICE_ACCOUNT`에만 넣고 Cloud Code 접근을 허용한다. credential을 repo에 저장하지 않는다.
- 서버는 short-lived OAuth token을 메모리에서 얻어 Google decodeIntegrityToken을 호출한다. token/credential/무결성 원문을 로그·Cloud profile에 저장하지 않는다.
- 실기기에서 package/certificate/version/license/requestHash/freshness 승인·거절 및 retry를 확인한다. Editor fixture/model pass는 이 실검증의 대체가 아니다.

## 5. 운영 / 이관 / 삭제

- Support ID로 Player Management/CLI lookup, ticket/audit/의심 flag 조회 담당자를 정한다. 현재 문의 자동 이메일/Slack 전송은 없다.
- ticket20개 상한과30일 삭제 대상·로그 retention을 Privacy에 명시하고 inactive account 정리 작업을 운영에 배치한다.
- Alpha 종료 후 `export-alpha --out <보호된-output>` → `migrate-alpha --source <alpha-ID> --target <production-ID> --players <export-file> --dry-run`을 검토한다. target count와 production_profile_required를 확인한 뒤 승인된 목록에만 --apply한다. 실제 bulk migration은 이번에 실행하지 않는다.
- export의 --limit(기본1000)은 페이지 종료 기준이다. 현재100명 page를 끝까지 처리해 cursor 재개 시 후보를 누락하지 않으며 결과는 최대1099명이다. next cursor가 있으면 별도 보호된 파일로 이어서 export한다.
- 삭제 요청은 `delete-account --player <PlayerId> --environments <실제-모든-ID의-쉼표목록> --journal <보호된-journal> --dry-run`으로 확인한다. 최종 실행에는 --apply --confirm DELETE가 필요하다. 기본 project/environment 인수도 지정한다.
- 삭제 도구는 Remote Config Admin 환경 목록 조회와 전달된 전체 ID 목록의 정확한 일치를 검사한다(추가 read 역할 필요). credential 역할·전체 환경 목록을 재확인하고 journal을 먼저 보존한다. disable Auth→각 환경 player data→Auth delete 순서다. 부분 실패를 완료로 처리하지 않는다. journal/보호된 export의 보관·삭제 정책도 정한다.
- 공개 Privacy Policy와 앱 재설치 없이 요청 가능한 웹 삭제 경로를 실제로 운영한다. URL은 BackendSettings에 넣고 Play Console Data safety와 일치시킨다. 가짜 URL은 불가하다.
- 서버 요청 접수 후 현재 SDK profile의 token/session 및 해당 account cache 정리를 구현했다. 이 동작과 원격 삭제 완료 통지를 실기기에서 검증한다. 로컬 world 보존 정책을 명확히 설명한다.

## 6. Release gate

- backend 준비 후에만 Enabled=true. 환경을 바꾼 뒤 앱을 재시작해 shared UGS bootstrap을 검증한다.
- Alpha는 LOWFIRE_ALPHA, Production은 LOWFIRE_PRODUCTION. Development build flag/개발 adapter를 release에 넣지 않는다.
- BackendBuildValidator의 blocker를 모두 해소하고 실제 Android/Relay/Cloud 호출·지원/삭제를 확인한다. 현금 결제는 이번 패스에서 계속 false다.
