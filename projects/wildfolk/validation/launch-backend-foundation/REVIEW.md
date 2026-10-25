# Launch Backend Foundation1 · Review

## 기준과 완료 경계

시작 SHA `23ae4341edb904923946f3bdcc0c72a2909dc8cd`의 remote main을 먼저 fetch하고 clean main에서 시작했다. 종료 SHA는 이 문서를 포함하는 commit이며 `git log -1 --format=%H -- projects/wildfolk/validation/launch-backend-foundation/REVIEW.md`로 확인한다. 자기 commit SHA를 문서에 순환 삽입하지 않는다. 최종 응답에서 push 후 remote/local SHA와 clean 상태를 별도 보고한다.

구현 대상은 Account/Cloud Profile/Alpha entitlement/Support/Operations의 기반이다. 실제 운영 서비스를 개통한 패스가 아니다. `BackendSettings.Enabled=false`. 외부 설정 미완료를 fixture 성공으로 대체하지 않았다. Save14/Profile2/Protocol21/generation3, Item136/Recipe111 및 보스·드랍·제작·성장 수치는 유지한다. 다음 콘텐츠나 Gem/Achievement/Attendance/Ads/IAP/Audio를 시작하지 않는다.

## 공식 조사와 선택

2026-10-25 공식 문서·패키지 registry·release를 확인하고 구현했다. 아래는 실제 API/버전 선택의 근거다.

| 자료 | 확인한 구체적 사실 | Lowfire 결정 / 제외한 대안 |
|---|---|---|
| [Unity Authentication PGS](https://docs.unity.com/en-us/authentication/platform-signin/google-play-games) | 기존 익명 계정 Link와 기존 PGS 계정 SignIn은 다른 동작이다 | cached 익명에 우선 Link. 충돌 시 명시적 계정 선택. 자동 SignIn/합치기 제외 |
| [PGS Unity 시작](https://developer.android.com/games/pgs/unity/unity-start), [v2 migration](https://developer.android.com/games/pgs/unity/migrate-to-v2), [release2.3.0](https://github.com/playgameservices/play-games-plugin-for-unity/releases/tag/v2.3.0) | PGS v2, server-side access code, 2026-09-29 release2.3.0 | 공식 plugin2.3.0과 Games-v2 22.1.0. 오래된 v1 로그인/이메일 계정 구현 제외 |
| [Cloud Save player data](https://docs.unity.com/en-us/cloud-save/concepts/player-data), [game data](https://docs.unity.com/en-us/cloud-save/concepts/game-data) | Protected player data는 서버 쓰기, Private custom data는 서버 전용 | 계정 root는 Protected, policy/gate는 Private. client 직접 쓰기 제외 |
| [write locks](https://docs.unity.com/en-us/cloud-save/concepts/write-locks), [Cloud Code 예](https://docs.unity.com/en-us/cloud-save/tutorials/cloud-code) | 기존 writeLock CAS, 새 항목 lock 생략은 무조건 저장 | 첫 생성만 사전 provisioning gate로 직렬화. 단순 read→upsert 경쟁과 자동 lease expiry 제외 |
| [Cloud Code access control](https://docs.unity.com/en-us/cloud-code/3.0.0/server-access-control), [runtime libraries](https://docs.unity.com/en-us/cloud-code/3.0.0/scripts/reference/available-libraries) | Player Cloud Save write 차단 정책과 versioned runtime SDK | 실제 SDK registry상3.0.0은Unity6000.7 요구. 현재6000.3 호환2.10.4 고정, server CloudSave1.4/axios1.6 |
| [Secret Manager integration](https://docs.unity.com/en-us/services/secret-manager/tutorials/integrations/cloud-code/scripts) | Cloud Code 주입 secretManager로 secret 접근 | Google service credential은 서버 secret. Assets/문서/config 복사 제외 |
| [service environments](https://docs.unity.com/en-us/services/service-environments), [Editor API](https://docs.unity.com/en-us/services/environments-api) | 환경별 서비스 데이터와 Editor selector/default 우선순위 | 기존 implicit Relay 경로 보존. 환경 목록 읽기 확인 후 공개 ID만 기록 |
| [Player Management](https://docs.unity.com/en-us/services/player-management/get-started), [Auth Client REST](https://docs.unity.com/en-us/oas-player-auth-client/1.0.0), [Auth Admin REST](https://docs.unity.com/en-us/oas-player-auth-admin/1.0.0) | project identity와 provider 확인/운영 API | Unity PlayerId 정본. 클라이언트 provider bool/Support ID를 인증 수단으로 쓰지 않음 |
| [Cloud Save Admin REST](https://docs.unity.com/en-us/oas-cloud-save-data-admin/1.0.0), [Remote Config Admin REST](https://docs.unity.com/en-us/oas-remote-config-admin/1.0.0) | 환경별 데이터 및 실제 환경 목록 읽기 API | 삭제 전 전체 환경 exact-set 검사. 환경3개를 하드코딩해 누락하는 삭제 제외 |
| [Play Integrity standard](https://developer.android.com/google/play/integrity/standard), [setup](https://developer.android.com/google/play/integrity/setup), [verdicts](https://developer.android.com/google/play/integrity/verdicts), [service account OAuth](https://developers.google.com/identity/protocols/oauth2/service-account) | server decode, requestHash, package/cert/version/license/time | Alpha 등록에 실제 승인 앱 증명 필요. client channel 문자열만으로 자격 부여 제외 |
| [UGS account deletion](https://docs.unity.com/en-us/authentication/delete-accounts), [Play deletion policy](https://support.google.com/googleplay/android-developer/answer/13327111) | identity 삭제가 다른 서비스 데이터 삭제를 대신하지 않음; 외부 웹 경로 필요 | 환경별 cleanup 후 Auth 삭제, UI는 pending. 가짜 Privacy URL/완료 표시 제외 |

## 기존 UGS/Relay 감사 · 환경

기존 multiplayer2.4.0, core1.18.0, Authentication3.8.0의 cached anonymous/SDK profile `default`를 유지한다. account와 Relay가 `UgsBootstrap` 하나를 공유하고 SemaphoreSlim으로 초기화/로그인을 직렬화한다. 이미 다른 환경으로 초기화되었으면 오류로 끝내며 실행 중 몰래 환경을 바꾸지 않는다. LAN은 UGS 인증을 요구하지 않는다. world participant GUID/Host ownership은 기존 LocalPlayerIdentity이며 Unity PlayerId로 바꾸지 않았다.

실제 Core Editor `EnvironmentsApi.RefreshAsync` 읽기 조회: project `70ba2490-7029-4528-b55a-9c2231fde007`, 환경은 **production 한 개**, ID `9da27a5f-4c79-44bc-b96b-1e490f83619d`. Editor selector 이름은 빈 값/ID Guid.Empty. 이 결과와 서비스 실제 활성 환경 검증을 구분한다. production을 생성/rename/delete하지 않았다. development/alpha는 아직 없고 서비스 계정 환경 변수는 미설정이다. Provider/secrets/index/Access Control/Cloud Code 배포는 확인하지 못했다. [환경 조회 증거](ugs-environment-audit.json).

| Channel | 환경 | 개발 adapter | Alpha enrollment | 현금 결제 |
|---|---|---|---|---|
| Development | development, ID 미설정 | 명시적 격리 Editor/Development만 | 불가 | 불가 |
| Alpha | alpha, ID 미설정 | 불가 | 서버 승인 build/window/linked/Integrity 필요 | 불가 |
| Production | 확인된 production | 불가 | 불가 | 현재 불가 |

Release build validator는 disabled backend, 환경/PGS/Integrity/Privacy/삭제URL 누락 및 개발 설정 혼입을 막는다. 따라서 현재 바로 출시용 AAB가 생성된다고 주장하지 않는다.

## Authentication / Android

PGS2.3.0, Authentication3.8.0, CloudCode2.10.4, 번들 EDM4U1.2.182, Integrity1.6.0. 중복 UPM EDM1.2.190은 제거하고 공식 PGS 번들 하나만 유지했다. 공식 license와 SDK source를 포함한다. vendor 빈 폴더의 orphan meta2개만 정리했다.

PGS auth code→UGS Link 우선, AccountAlreadyLinked는 선택 화면. 기존 계정 선택 시 새 PGS code로 `lowfire-recover` SDK profile에서 SignIn한다. 원래 guest profile token/files를 자동 삭제하지 않는다. 실패 시 원래 SDK profile로 돌아간다. 자동 max/union/unlink/guest 덮어쓰기는 없다. Backend가 disabled/네트워크 실패/PGS 미지원이어도 SP/LAN과 로컬 Save를 사용할 수 있다. 강제 가입은 없다.

Android JNI Play Integrity standard bridge는60초 제한과 보존 설정을 갖는다. 실제 OAuth·manifest·tester track·서명이 없어 Google/Integrity 서비스 성공은 미검증이다.

## CloudProfile1 / Profile2 import

Protected `lf_account`: Schema1/PlayerId/SupportId/server CreatedAt/ProviderLinked/Revision/ClassRevision/ClassProfile/Import/Alpha/Entitlements/Security/Tickets/Audit/Operations/Daily/Deletion. `lf_support`는 indexed 운영 검색용이다. client projection은 support 본문/audit/receipt 전체를 노출하지 않는다. 월드 Save14와 공용Level/Gold/시설/Boss는 계속 로컬 Host 정본이다.

ClassProfile은 기존 Profile2 중 선택/6Class 소유/XP/54node만 담고 pending 거래/월드 경로를 전송하지 않는다. 신규 cloud가 없을 때 development/alpha policy가 허용한 최초 import만 가능하고 영구 receipt/Closed로 재시도를 봉쇄한다. Production import는 닫혀 있다. 기존 Cloud는 유지한다. local import는 `prelaunch_local_unverified`이며 premium 획득 증명이 아니다.

계정별 별도 CloudAccounts cache로 materialize하여 원본 guest Profile2와 world를 보존한다. 로컬 Class 변경은 먼저 dirty marker를 저장한다. 이후 exact ClassRevision으로 `CommitClassProgress`를 요청하고 충돌은 사용자 선택이다. Use cloud는 기존 cache를 archive한다. 무조건 높은 XP/소유권 합치기가 없다. Meta/audit 변경은 ClassRevision을 올리지 않는다. 기존 Host-local 성장 snapshot은 검증된 형식의 **unverified durability mirror**이며 완전한 anti-cheat는 아니다. PremiumGem 소유권 신규 추가는 이 경로로 거절하고 Entitlements와 분리한다.

첫 생성은 Private `lowfire_backend/creation_gate` 기존 항목 CAS를 요구한다. timeout/unknown write outcome이면 gate를 유지하고 운영자 확인 전 강제 expiry하지 않는다. 기존 계정은 gate 없이 read/CAS한다. 과도한 자동 재시도 대신 생성 가용성 저하를 받아들여 늦은 initial upsert의 덮어쓰기를 방지했다.

## Alpha / 영구 entitlement

linked PGS, Alpha 환경/channel, server window, 승인 app version, Play Integrity package/certificate/version/license/freshness/requestHash를 모두 확인한다. hash는 player/environment/op/build에 묶는다. 첫/마지막 server timestamp와 source environment를 기록한다. Alpha 참여가 곧 Production 지급은 아니다.

운영 `export-alpha`→source를 다시 읽는 `migrate-alpha` dry-run→승인된 apply 경로를 제공한다. `alpha_tester_2026`은 additive/idempotent 영구 receipt/audit grant다. 외부 manifest만으로 지급하지 않는다. Production profile이 없는 대상은 별도 provisioning이 필요하다. 대량 이관/실제 지급은 실행하지 않았다. Alpha Class profile의 Production 이전은 자동 merge하지 않으며 별도 출시 경계다.

## Support / audit / suspicious / feedback

Support ID는 project+PlayerId SHA256의80bit, LF-4hex×5 형식으로 안정적이다. 10,000 sample uniqueness는 수학적 충돌 불가 보장이 아니다. 운영 index lookup이0개/2개 이상이면 fail-safe로 거절하며 인증키로 사용하지 않는다. UI copy는 이전 clipboard를 복구하는 QA로 확인했다.

audit는 eventId/playerId/supportId/server timestamp/correlation/build/channel/type/op/result/reason이다. 최근64개, Security reason16, 임시 operations512와2UTC날짜, account mutation200/일로 제한한다. 동일 op/payload는 같은 결과, 다른 payload는 의심 flag를 남긴다. client report는 unverified로 구분한다. invalid import/불가능한 entitlement/문의 abuse를 Suspicious로 표시하며 자동 ban하지 않는다.

문의6종,5~1500자,60초 간격·5회/UTC일·최근20개, 서버 시간과 인증 context를 쓴다. 선택적 diagnostics는 whitelist/redaction하며 저장 원문·경로·token을 로그로 보내지 않는다. 본문은 protected ticket이며 raw text 로그가 없다.30일 경과 ticket은 다음 접수 때 정리한다. 비활성 계정 retention 작업·운영 담당/로그 보관 설정은 아직 배치되지 않아 release blocker다.

운영 흐름: Support ID unique lookup→Player Management→환경 profile→audit correlation→ticket→entitlement/import/security 조사. 기존 Admin lookup/export는 특권 operator 도구이며 실제 service credential이 없어 실행하지 않았다.

## Access Control / Cloud Code / secrets

Cloud Code operations: EnsureAccountProfile/GetAccountProfile/EnrollAlphaParticipation/GetEntitlements/CommitClassProgress/SubmitFeedback/RecordOperationalEvent/RequestAccountDeletionPreparation. context PlayerId/token을 Authentication Get Player로 확인하고 linked provider도 서버 응답에서 판단한다. 정책 environment/channel 검사를 먼저 한다. 클라이언트 GrantEntitlement/RealMoneyPurchase는 거절한다.

Access Control 소스는 Player Cloud Save writes deny + query exception이며 Auth/Relay에는 적용하지 않는다. Protected/Private 쓰기는 service context, CAS 최대3회, client 동일 op retry3회/250·500ms다. access/secret 정책은 배포되어야 효력이 생기며 실제 direct-write rejection은 미검증이다. runtime은 공식 versioned SDK를 사용한다. secret은 Admin 환경 변수/UGS Secret Manager이고 Git에는 실제 key/code/token/credential을 넣지 않았다.

## 삭제 / Privacy

UI 경고→재확인→linked 인증·온라인→서버 삭제 요청 ID/처리 대기. 첫 확인만으로 서버 요청하지 않는다. Admin은 durable journal→Auth disable→실제 전체 환경 Default/Public/Protected player data cleanup→Auth delete 순서다. Remote Config Admin 목록과 환경 set가 다르면 파괴 작업 전에 거절한다. 실패 후 journal resume, Auth 오류에도 완료로 표시하지 않는다. 실제 사용자 삭제는 실행하지 않았다. 요청 접수 후 현재 account cache/선택 pointer 및 현재 SDK profile token/session을 정리한다. 원본 guest/world 보존을 격리 테스트했다. filesystem 실패는 marker로 재선택을 차단하고 pending/local_cleanup_failed로 남긴다.

로컬 world를 지우지 않는다. Privacy/외부 웹 삭제 URL은 비어 있고 가짜 주소가 없다. 접수 후 현재 기기 account cache/session 정리는 구현·격리 테스트했다. 다른 기기/OS 백업 처리·원격 완료 통지, 비활성 ticket retention, 웹 요청 처리·Privacy 고지와 Play Data safety 검증은 출시 차단 항목이다. 사용 서비스가 늘면 cleanup inventory도 늘려야 한다. 이 기반을 운영 삭제 end-to-end 완료로 간주하지 않는다.

## 검증과 사용자 저장 보호

최종 결과·실행 구분은 아래 증거와 `verification.json`에 기록한다. fixture/model은 실제 Google/UGS 호출 성공의 증거가 아니다.

- 서버: [40개 TAP](server-tests.tap), [실행 결과](server-tests.json). 생성/재import/충돌/지급/Alpha/동일 요청/문의·redaction/제한/CAS/unknown outcome/deletion 실패복구·환경누락을 검사했다.
- Unity: EditMode 관련 모델·Scene contract, PlayMode 계정 UI/offline→실제 SP Save14 및 기존 Class 회귀. 초기 Edit fixture가 씬을 복구하지 않아 GlobalLight 중복1건이 났고 teardown을 바로잡아 재실행했다. 삭제 보존 검사를 추가한 뒤 테스트 Repository 경로가 다음 case에 남는 격리 실패1건도 수정했다([당시 결과](deletion-edit-initial.json)). 관련 회귀38/38와 최종 account14/14의 합집합은 고유40개이며, 최종 PlayMode4/4다. [초기 실패](model-initial.xml)를 숨기지 않는다.
- Android: Alpha/Production release C# script compile, native provider assembly 포함·DevelopmentIdentityProvider 제외. Gradle/IL2CPP/AAB/실기기 검증과 다르다.
- 실제 Editor MCP: isolated QA save,1920×1080 Account/문의/삭제 UI, clipboard 복구, dropdown6개, 동의 기본false,1500자 제한, 삭제 pending. 연결 성공 표시는 명시적 개발 fixture다.
- 실제2프로세스 localhost LAN: Host/Join·자원 집합/채집·chunk 왕복·재접속/reHost·Boss/Class 보상 회귀. 실제 Relay allocation/Room Code/4기기는 실행하지 않았다. shared bootstrap compile과 LAN 통과를 실제 Relay 통과라고 표현하지 않는다.
- 사용자 저장: canonical/Live UserData의 모든 기존 파일 SHA256/크기/mtime을 시작/끝 비교한다. QA는 Captures/Temp/격리 batch project만 사용한다. 씬/Play/QA session/Console 상태를 정리한다.

공식 SDK vendor source와 기존 MainMenu UI 변경은 수동 diff 감사 대상이다. Scene object audit: 기존1384개 보존,445개 추가, 기존 의미 변경6개. 씬 object reorder/trailing serialization noise를 정본에서 제거했다. Gameplay 씬과 game content data는 변경하지 않았다.

## 외부 설정과 미검증

[SETUP_REQUIRED](../../docs/SETUP_REQUIRED.md), [RELEASE_BLOCKERS](../../docs/RELEASE_BLOCKERS.md)가 정본이다. development/alpha 환경 생성·이름/ID mapping, Provider/OAuth/서명/manifest, Play Integrity service secret, 정책/gate/index/Cloud Code/Access Control 배포, 실제 Privacy/웹 삭제 URL·운영 retention, Android 실제 로그인·Relay·삭제 e2e가 남아 있다. 현재 설정은 안전하게 disabled다. 외부 설정 없이 실제 서비스 성공/완전한 anti-cheat/공개 출시 준비 완료를 주장하지 않는다.

작업별 시간은 [timing](timing.json)과 각 process JSON에 UTC 시작/끝/벽시계 시간을 기록한다. 겹치는 작업 시간을 합산하지 않는다. 도구/모델이 작업별 token 소비를 제공하지 않아 `perOperationTokens=null`이며 추정 token을 실제 사용량으로 기록하지 않았다.

최종 수동 감사: [source validation](source-validation.json), [secret scan](secret-scan.json), [실제 UI 상태](mcp-ui-final.json), [종합 결과](verification.json). 자체 소스 diff whitespace 검사는 통과했다. 공식 vendor 및 원본 TSV 증거의 기존 공백은 보존했다. 사용자 기존 save 두 파일의 SHA256/크기/mtime은 모두 동일하다.
