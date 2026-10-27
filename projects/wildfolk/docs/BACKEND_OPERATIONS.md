# Account / Cloud / Operations 정본

## 현재 구현과 활성화 상태

Foundation1.5는 development/alpha 실제 환경에 서버 기반을 배포했다. development 실제 서비스76개, Unity SDK account 왕복, 실제2프로세스 인터넷 Relay를 검증했다. 실제 Google 로그인/Integrity/URL/Android release 설정은 미완료이며 Resources/BackendSettings.json Enabled=false를 유지한다. [활성화 검증](../validation/launch-backend-activation/REVIEW.md), [외부 설정](SETUP_REQUIRED.md).

정본 서버는 `backend/src/`, Admin 도구는 `backend/tools/admin.cjs`, 접근 정책은 `backend/access/lowfire.ac`다. `node backend/tools/sync-catalog.cjs`는 기존 Classes.json에서 최소 검증 표를 생성한다. npm ci 후 `node backend/tools/bundle.cjs`가 Cloud Code scripts `LowfireAccount`/`LowfireIntegrityVerify`/`LowfireEconomy`를 `backend/deploy/`에 만든다. 생성된 배포물은 Git에 중복 저장하지 않는다.

## Identity와 기존 Relay

Android Google Play Games v2의 일회용 server auth code를 Unity Authentication에 넘긴다. canonical account key는 Unity PlayerId다. Google 외부 ID/email을 게임 key나 Cloud 필드로 복사하지 않는다. Lowfire 이메일/비밀번호는 없다.

RelayRoomConnection과 계정 기능은 UgsBootstrap을 공유한다. 기존 AuthenticationProfile=`default`, 익명/cached sign-in을 유지하고 초기화·로그인을 직렬화한다. 이미 초기화된 다른 환경을 몰래 재초기화하지 않는다. Foundation1.5에서는 Enabled와 무관하게 채널에 등록된 환경을 명시한다. development=9aa02b53-8272-4975-a42a-f6a0b75a360f, alpha=a9eee8ca-e2a8-4fa7-9530-6df9b230e943, production=9da27a5f-4c79-44bc-b96b-1e490f83619d다. Core1.18의 공개 초기화 API SetEnvironmentName을 사용하고, Authentication이 실제 반환한 IEnvironmentId component를 등록 ID와 대조한 뒤에만 Relay/Cloud 요청한다. 이 낮은 수준의 공개 component API 사용은 pinned Core/Authentication 업그레이드 때 다시 검증한다. 빈 ID/중복 mapping/이미 다른 환경 초기화/ID mismatch는 fail-closed이며 production fallback은 없다. production의 기본 환경 지정은 유지했고 provider/다른 remote 서비스 상태는 조회하지 않은 부분을 UNVERIFIED로 남긴다.

cached anonymous→PGS LinkWithGooglePlayGamesAsync로 기존 Unity PlayerId를 보존한다. AccountAlreadyLinked는 사용자 선택으로 처리한다. 기존 계정 로그인 선택 때만 별도 SDK profile `lowfire-recover`에서 SignInWithGooglePlayGamesAsync한다. 원래 guest SDK profile의 cached token은 이 전환에서 지우지 않는다. 자동 merge/union/max, 자동 unlink 또는 저장 삭제는 없다. 오프라인/오류는 로컬 플레이를 막지 않는다.

월드 participant GUID/Host ownership은 LocalPlayerIdentity 그대로다. Unity PlayerId를 participant GUID에 대입하지 않는다. 현재 remote Class admission은 기존 Survivor/session-only 검증 경계를 유지하며, Cloud 소유권을 Client claim만으로 Host에 승격하지 않는다.

## 환경과 Build Channel

| Channel | 목표 UGS 환경 | QA identity | Alpha 등록 | 실제 현금 결제 |
|---|---|---|---|---|
| Development | development | 명시적 격리 QA만 | 아니오 | 항상 불가 |
| Alpha | alpha | 불가 | 서버 조건 충족 때만 | 항상 불가 |
| Production | production | 불가 | 불가 | 이번 패스는 항상 불가 |

LOWFIRE_ALPHA / LOWFIRE_PRODUCTION define으로 release channel을 정한다. 일반 Editor/Development build는 Development, 그 외 기본은 Production이다. Alpha/Production에 Development build 옵션을 섞으면 build validator가 거절한다. Development identity는 Editor/Development 조건부 컴파일과 channel+isolated-directory 검사로 제한한다.

환경 이름/ID는 실제 Dashboard 값을 확인해 채운다. 기존 환경을 삭제/이름변경/이관하지 않는다. 기존 cached SDK profile default는 보존하며 환경 mapping을 바꾼 뒤 앱을 재시작한다. 계정과 Relay는 같은 mapping을 쓴다. 서로 다른 환경을 같은 실행 중 섞지 않는다. Authentication identity는 project 범위, 서비스 데이터는 environment 범위이므로 Alpha 데이터가 Production에 자동 복사된다고 가정하지 않는다.

## CloudProfile1 / local Profile2 소유권

Protected player key `lf_account`: Schema1, PlayerId, server CreatedAt, SupportId, provider-linked 최소 bool, Revision, ClassRevision, ClassProfile, 영구 Import receipt, Alpha participation, Entitlements, bounded Audit/Operations/Tickets/Security, Deletion request. `lf_support`는 운영 검색용 indexed protected key다. 외부 auth ID/token/이메일/world/path는 저장하지 않는다.

월드 Save14, 지형·자원·시설·시간·Boss·공용Level/Gold는 로컬 Host 정본이다. 전체 world Cloud backup은 없다. Account 화면 Level은 로컬 월드 정보이며 ClassXP와 합치지 않는다.

새 Cloud account의 첫 Ensure에 한해서 development/alpha 서버 정책이 허용한 Profile2를 일회성 import할 수 있다. Version/선택/6Class/XP/노드/선행 조건을 검증하고 pending 거래·월드 경로를 전송하지 않는다. XP 입력 상한10억은 backend 입력 제한이며 게임 성장 밸런스 변경이 아니다. 영구 receipt와 Imported/Closed 상태로 다시 import하지 못한다. Production legacy import는 거절한다.

이전 로컬 데이터는 **prelaunch_local_unverified**다. import가 암호학적으로 신뢰되는 획득 증명이라는 뜻은 아니다. 이후 future Gem/IAP/프리미엄 지급의 증거로 사용할 수 없다.

기존 Cloud가 있으면 그 snapshot이 account durable 정본이다. 게스트 원본 Profile2/backup은 건드리지 않고 별도 `CloudAccounts/<environment+PlayerId hash>/` 캐시에 materialize한다. 계정 cache를 현재 Class store에 연결해 오프라인 플레이를 유지한다. 로컬 변경은 dirty로 표시하고 명시적인 Sync에서 Cloud Code `CommitClassProgress`를 호출한다. 정확한 ClassRevision 충돌이면 자동 max/union/덮어쓰기를 하지 않고 선택을 요구한다. Use cloud는 현재 캐시를 archive하고 canonical snapshot을 받는다. guest 파일/월드 소유권은 유지한다.

Class snapshot sync는 기존 player-host 로컬 성장의 **unverified durability mirror**다. canonical 규칙·revision·XP 감소 거절·소유권 보존을 검사한다. Gold 기반 기존 Class 진행은 기존 로컬 경제 신뢰 범위이며 dedicated server 획득 증명이 아니다. PremiumGem Class의 새 ownership은 이 경로로 추가할 수 없다. Account Entitlements/미래 premium economy는 이 Class mirror와 별개 서버 권위다. Meta/audit/문의 변경은 ClassRevision을 올리지 않아 클래스 충돌을 만들지 않는다.

## Cloud Code 작업 / 트랜잭션

| Operation | 책임 |
|---|---|
| EnsureAccountProfile | 첫 생성, 조건부 일회 import, 기존 account 보존 |
| GetAccountProfile | account projection 읽기 |
| EnrollAlphaParticipation | linked account + 실제 빌드 증명 + 환경/window 검사 |
| GetEntitlements | 서버 지급 자격 읽기 |
| CommitClassProgress | canonical ClassRevision 기준 검증된 snapshot 교체 |
| SubmitFeedback | category/text/consent 검사, 제한·durable ticket |
| RecordOperationalEvent | 허용된 client-report event만 unverified로 기록 |
| RequestAccountDeletionPreparation | 재확인된 삭제 요청·durable ID |

Cloud Code context PlayerId/accessToken을 Authentication Get Player로 확인한다. linked provider는 Authentication 응답에서 검사한다. 요청 JSON의 provider/account/자격 주장은 증거가 아니다. 요청 op allowlist, 안전한 error/correlation ID를 사용한다.

동일 account state의 writeLock CAS로 profile/receipt/audit/ticket을 함께 기록한다. CAS 재시도 최대3회; 클라이언트 전송도 동일 operationId로 최대3회다. 날짜:GUID key는 현재/이전 UTC 날짜만 유효하다. Generic receipt는 두 날짜·최대512, account write 최대200/UTC일, Audit64, Security reason16, Tickets20, Entitlements256이다. 영구 import/entitlement/참여 사실은 이 임시 receipt 창에 의존하지 않는다. 같은 key의 다른 payload는 의심 flag, 동일 payload는 같은 결과를 반환한다.

Cloud Save의 신규 항목 생성에는 기존 writeLock이 없고 무조건 upsert하면 최초 import가 경쟁할 수 있다. 미리 provisioning한 Private Game Data `lowfire_backend/creation_gate`의 CAS로 **첫 생성만** 직렬화한다. 생성 도중 crash/unknown commit이면 gate를 자동 만료하지 않는다. 이는 availability를 낮춰 데이터 덮어쓰기를 막는 선택이다. 운영자는 실행 종료·요청 결과 확인 후에만 gate를 복구한다. 정상 기존 account read/write는 이 gate를 쓰지 않는다. 모든 mutation을 거대한 lease나 무한 retry로 숨기지 않는다.

## Alpha / Entitlement

Alpha 환경 정책, 서버 승인 app version, enrollment window, Authentication의 google-play-games 연결, Play Integrity standard 결과를 모두 검사한다. requestHash는 PlayerId/environment/operationId/build에 묶는다. package/certificate/versionCode/PLAY_RECOGNIZED/LICENSED와 freshness도 검사한다. 클라이언트 문자열만으로 Alpha 자격을 주지 않는다. 실제 기기·Google API 동작은 별도 외부 설정/검증 대상이다.

Alpha 참여 기록은 program alpha_2026, first/last server time, build, provider linked, source environment다. 현재 Alpha에서 Production 보상이 지급된 것처럼 표시하지 않는다. 이후 service-account CLI가 source 참여를 다시 읽고 Production `alpha_tester_2026`을 additive/idempotent grant한다. 원격 export 파일을 자격 증거로 단독 신뢰하지 않는다. 기존 Production profile이 없는 대상은 production_profile_required로 분리하며 먼저 정상 provisioning한 후 dry-run을 반복한다. Alpha/Production Class profile 이전은 별도 release migration 결정이며 이 도구가 임의 union하지 않는다. 실제 대량 이전은 실행하지 않았다.

Entitlement는 Id/GrantedAt/Source/ReceiptId/MetadataVersion. 클라이언트 grant/회수 endpoint는 없다. 현재 privileged grant는 Alpha 자격만 허용한다. 추가 유료 자격·회수는 별도 명시적 운영 작업으로 설계한다.

## Support / audit / suspicious

Support ID는 project+PlayerId SHA256의80bit를 표시용 LF-XXXX-XXXX-XXXX-XXXX-XXXX로 인코딩한다. account당 안정적이며 복사 가능하다. 비밀·인증 수단이 아니다. 운영 query에서 일치 결과가 정확히1개가 아니면 lookup을 거절하여 충돌/불완전 index를 다른 계정으로 해석하지 않는다. 확률적 충돌 저항과 ambiguous lookup fail-closed 전략이며 수학적 무충돌을 주장하지 않는다.

문의는6category,5~1500자,60초 간격·하루5개, 최신20개다. 본문은 protected ticket에 저장하고 로그에 출력하지 않는다. 진단 동의가 있을 때만 whitelist의 버전/channel/platform/device/OS/locale/schema/scene/session/error/cloud 상태를 첨부한다. token/secret/email/path/opaque credential 형태를 제거한다. player/support 식별자는 서버 context로 연결한다. 현재 whole save/log 첨부는 없다.

Ticket은30일 뒤 삭제 대상이며 새 접수 시 정리된다. 비활성 계정까지30일 내 정리하려면 출시 운영에서 별도 retention job/담당자 절차를 반드시 배치해야 한다. 자동 TTL이 이미 설정되었다고 주장하지 않는다. Audit은 최근64개, 운영 로그 retention은 Dashboard에서 정해야 한다. 사용자 자유 텍스트는 별도 개인정보 처리 대상이다.

Audit: eventId/playerId/supportId/server timestamp/correlationId/buildVersion/channel/eventType/operationId/result/reason. 안전한 metadata만 저장한다. client report는 서버 확인 사실과 구분한다. invalid migration, impossible entitlement, support spam, payload mismatch를 Suspicious로 표시하며 자동 영구 Ban은 하지 않는다. Normal/Suspicious와 향후 UnderReview/Restricted 경계를 유지한다. Host memory/world cheat 완전 방지는 목표가 아니다.

조사 흐름: Support ID → unique lookup → UGS Player Management의 identity/환경별 profile → audit correlation → ticket → entitlement/import/의심 flag 확인. Admin은 일반 player credential이 아니라 최소 권한 service account를 쓴다. key/secret은 환경 변수, Google Integrity credential은 UGS Secret Manager다. 토큰·키·raw HTTP 오류를 출력하지 않는다.

## 삭제 / Privacy

Account UI 경고→재확인→인증/온라인→서버 요청 ID. UI는 요청 접수/처리 대기로 표시하며 완료라고 하지 않는다. 운영자는 durable local deletion journal을 먼저 기록하고 Authentication 비활성화로 새 cloud write를 막는다. development/alpha/production **모든 실제 환경**의 Default/Public/Protected player data를 삭제한 뒤 Authentication을 삭제한다. 실패 시 journal에서 완료 단계 다음으로 재시도한다. Auth만 삭제하고 Cloud를 남기는 경로는 없다. 사용 중 서비스가 늘어나면 cleanup inventory를 확장해야 한다.

로컬 world는 삭제하지 않는다. Sign Out은 현재 계정 연결/선택을 해제하며 원본 guest와 world를 보존한다. 요청 접수 후 현재 기기의 계정 캐시/선택 pointer를 정리하고 현재 SDK profile token/session을 지운다. 원래 별도 guest와 local world는 보존한다. filesystem 실패는 deletion marker로 재선택을 막고 pending/local_cleanup_failed로 남긴다. 다른 오프라인 기기/OS 백업을 원격에서 지울 수 없으므로 웹 완료 통지·실기기 검증은 출시 전 확정해야 한다. UGS Authentication 삭제는 project 범위이므로 환경 하나만 정리한 후 실행하면 안 된다.

현재 Privacy/웹 삭제 URL은 비어 있으며 가짜 링크를 넣지 않았다. URL·Dashboard·실기기 end-to-end 검증이 끝나기 전 공개 출시할 수 없다. 수집 inventory: UGS identity(UGS가 관리), account snapshot/entitlement/import, 선택적 support text/diagnostics, bounded audit. Foundation2는 economic ledger/achievement/attendance 데이터를 추가한다. Analytics/Ads/IAP SDK 데이터는 수집하지 않는다. [설정](SETUP_REQUIRED.md), [출시 차단 항목](RELEASE_BLOCKERS.md).

## Foundation1.5 실제 배포 경계

공식 UGS CLI2.0.0의 Unity Hub 로그인으로 환경/정책/스크립트/index를 관리했다. 게임 Authentication과 운영 CLI 인증은 별개다. Production은 read-only metadata 감사만 수행했다. 실제 서비스 검증과 fixture 결과는 [REVIEW](../validation/launch-backend-activation/REVIEW.md)에서 구분한다.

Cloud Code JS에는 Node crypto가 없어 초기 배포가 실패했다. 정본은 node-forge1.4.0의 필요한 browser 모듈을 esbuild로 번들하며 Node built-in/TextEncoder/WebCrypto에 의존하지 않는다. SHA256은 독립 Node 구현과, RS256 서명은 메모리에서 생성한 일회성 key의 독립 native verify와 비교했다. 실제 Google 검증 성공을 뜻하지 않는다. 서버 context.correlationId와 operation 기반의 비밀이 아닌 식별자를 사용하며 자체 credential/random key를 만들지 않는다.

128KB script limit 때문에 Account와 IntegrityVerify를 분리했다. bundle 도구가 각128000byte 미만을 강제한다. Account는 같은 실제 player token으로 같은 project/environment의 IntegrityVerify를 호출하고, 서버 응답 verified를 확인한 뒤 enrollment 조건·idempotency를 다시 검사한다. child는 별도 linked provider/window/policy/hash/Google 검증을 수행하며 자격을 직접 지급하지 않는다. Alpha가 닫혔으면 Google credential을 읽거나 외부 검증을 호출하지 않는다.

서버 audit는 account.created, cloud.profile_created/imported, support.submitted, cloud.conflict, cloud.profile_updated, account.deletion_requested, alpha.enrolled 및 security.suspicious를 공통 schema에 기록한다. account.linked/cloud.write_failed의 client report는 서버 확인 사실과 구분한다. migration plan은 candidate/duplicate/already-granted/target count와 결정적 operationId를 제공한다.

새 dependency의 RSA verification advisory는 현재 호출하지 않는 API의 문제이며 npm audit 결과를 숨기지 않는다. [Release blockers](RELEASE_BLOCKERS.md), [third-party notices](../backend/THIRD_PARTY_NOTICES.md). user save/Profile2와 Host-owned Continue/후발 Join/Host migration 없음 정책은 그대로다.

## Launch Economy Foundation2 · 현재 경계

LowfireEconomy는 lf_economy의 지급/차감/Claim/출석/Engineer/부활 reservation·compensation을 같은 CAS로 확정한다. Private economy_config Events=[]가 실제 non-production에 배포된다. admin.cjs economy-lookup/economy-adjust(reason/operator/case/operation 필수, default dry-run) 추가. ledger는 append-only8192행/root3MiB에서 fail-closed, audit64/op256/devices64/critical4096. 삭제 inventory에 lf_economy와 계정 economic cache/intent를 포함한다. Ads/IAP 이전 archive/용량/비용 검증 필요. provider/URL/실기기 미설정으로 제품 Enabled=false 유지. [경제 전체 규칙](ECONOMY.md) · [실행·검증](../validation/launch-economy-foundation/REVIEW.md).

## Foundation2.1 운영 경계 · 2026-10-27 checkpoint

Development/Alpha의 LowfireAccount/LowfireEconomy/LowfirePremiumAdmission 정본을 배포했다. Integrity/policy/creation gate는 변경하지 않았고 Production은 배포·활성화하지 않았다. 실제 서비스 검증은 신규 anonymous QA이며 Google 성공을 뜻하지 않는다.

`economy-lookup`은 capacity/Engineer receipt 검증/last admission을 제공한다. `economy-history --before <absolute sequence> --limit <1..50>`, `economy-source --source <id>`, `economy-transaction --transaction <id>`, `economy-capacity`, `economy-reconcile [--cursor <JSON file>] [--limit <segments>]`, `economy-maintain [--apply]`를 추가했다. 기존 project/environment/player 또는 support scope와 operator credential을 사용한다. maintain 기본은 dry-run이며 reconcile은 bounded cursor로 이어간다. 경제 조정은 기존 reason/operator/case/operationId와 CAS를 유지한다.

삭제는 environment별 archive Count+1(orphan 가능)와256 source/resolution bucket을 먼저 drain하고 player data, 마지막 Auth를 지운다. journal 완료 checkpoint 전에 실패하면 같은 environment를 재시도한다. 긴 이력의 삭제/지원 조회 요청 수는 이력 크기에 비례하며 작업 시간·비용을 운영에서 관찰해야 한다. raw token/secret은 저장하지 않는다. 미래 Ads/IAP global receipt registry의 법적 retention/restore/deletion 정책은 실제 provider 도입 전에 확정한다.

audit64/day·활성 proof16/90초·mutation budget500/day, rejected proof는 bounded suspicious와 reason으로 남기며 자동 ban은 없다. malformed/replay/cross-player/session/revision을 거절하고 성공 ownership은 재조회한다. 수정된 Host gameplay와 능동 LAN MITM 전체를 방어하는 anti-cheat가 아니다. 최종 QA/정리 결과와 남은 항목은 [RESUME](../validation/economy-premium-mp-ledger/RESUME.md)를 따른다.
