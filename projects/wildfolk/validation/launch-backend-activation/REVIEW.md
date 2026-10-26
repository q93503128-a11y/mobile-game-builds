# Launch Backend Foundation1.5 · Activation Review

## 결론과 기준

**PARTIALLY ACTIVE**. 실제 non-production backend core는 배포·왕복·접근 제어·Relay까지 검증했다. 제품 Google 계정 연결·Integrity·Privacy/웹 삭제·Android release는 외부 설정 대기이며 BackendSettings.Enabled=false다. 코드 준비만으로 ACTIVE라고 하지 않는다.

시작 remote/local clean main은 `0ce14989f5819dc1d1c46e4a128b3ee4f93e15f7`. 최초 fetch 후 AGENTS/README/scope/운영/setup/blockers/이전 REVIEW와 Foundation1 source/data/SDK integration/ProjectSettings를 다시 감사했다. 종료 SHA는 이 문서를 포함하는 commit이며 `git log -1 --format=%H -- projects/wildfolk/validation/launch-backend-activation/REVIEW.md`로 재현한다. 최종 응답과 저장소 밖 publication receipt에서 실제 push SHA를 별도 기록한다.

main 직접 작업, PR/임시 branch0. production mutation0. Save14/Profile2/Protocol21/generation3, Item136/Recipe111, world authority와 인게임 범위 유지. Foundation2/Gem/Achievement/Attendance/Ads/IAP/Audio를 시작하지 않는다.

## 공식 최신 근거 · 2026-10-25

| 공식 자료 | 직접 확인한 사실 | 적용/선택 |
|---|---|---|
| [CLI2 login](https://docs.unity.com/en-us/services-cli/2.0.0/manual/general/base-commands/login), [install](https://docs.unity.com/en-us/services-cli/2.0.0/manual/general/get-started/install-the-cli) | Hub 인증과 service account는 운영 인증이며 게임 인증과 별개 | 공식2.0 실행 파일을 repo 밖 설치, Hub auth로 실제 배포. credential 추출/출력 없음 |
| [환경 add](https://docs.unity.com/en-us/services-cli/2.0.0/manual/general/base-commands/environment/add), [Editor 환경 API](https://docs.unity.com/en-us/services/environments-api) | 공식 환경 생성 명령과 SDK 환경 선택 | development/alpha 실제 생성. 기존 production ID/default/이름 보존 |
| [PGS Auth](https://docs.unity.com/en-us/authentication/platform-signin/google-play-games) | Link는 cached player 보존, SignIn은 cached player 교체 | 기존 Link 우선·명시적 conflict 선택 보존. 자동 merge/unlink 없음 |
| [PGS Unity 설정](https://developer.android.com/games/pgs/unity/unity-start), [plugin2.3.0](https://github.com/playgameservices/play-games-plugin-for-unity/releases/tag/v2.3.0) | PGS 설정·서명·Web client/resource가 필요 | 사용자 확정 com.gltam.lowfire 사용. 실제 OAuth/PGS 값은 미생성, 임의 값 없음 |
| [Cloud Save Player Data](https://docs.unity.com/en-us/cloud-save/concepts/player-data) | Protected는 본인 read/서버 write | 실제 owner read200, Default/Protected 직접 write403 검증 |
| [Cloud Code libraries](https://docs.unity.com/en-us/cloud-code/3.0.0/scripts/reference/available-libraries), [runtime](https://docs.unity.com/en-us/oas-cloud-code-client/1.0.0) | 제공 library/HTTP 호출 경계 | Node crypto 미제공을 실제 publish에서 발견. SDK aliases만 external, crypto browser bundle |
| [Access Control](https://docs.unity.com/en-us/cloud-code/3.0.0/server-access-control) | trusted server write와 client read 구분 | development/alpha 정책 배포. Authentication/Relay는 차단하지 않음 |
| [Cloud Save SDK1.4 DataApi](https://cloud-code-sdk-documentation.cloud.unity3d.com/cloud-save/v1.4/dataapi) | 메서드별 인수 순서가 다름 | 초기 임시 cleanup의 삭제 인수 오류를 바로잡고 실제 cleanup 완료. canonical account Get/Set/CAS도 서비스 검증 |
| [Player Management](https://docs.unity.com/en-us/services/player-management/get-started) | identity는 project, 서비스 데이터는 환경 범위 | QA identity도 project-wide임을 구분하고 새 QA identity만 cleanup 후 삭제 |
| [Integrity standard](https://developer.android.com/google/play/integrity/standard), [setup](https://developer.android.com/google/play/integrity/setup) | 서버 decode/hash와 실제 Google project 연결 | Lowfire Alpha 등록에는 필수 정책. Google 비밀값이 없어 실제 proof 성공은 미검증 |
| [Multiplayer Services](https://docs.unity.com/en-us/mps-sdk) | 통합 SDK가 Relay 포함 | 기존 multiplayer2.4.0/DTLS 흐름 유지. 실제2프로세스 Relay Host/Join·재접속/reHost 통과 |
| [Auth deletion](https://docs.unity.com/en-us/authentication/delete-accounts), [Play deletion](https://support.google.com/googleplay/android-developer/answer/13327111) | Auth 삭제는 관련 서비스 cleanup을 대신하지 않음; 외부 웹 경로 필요 | QA Cloud cleanup→Auth delete 실제 수행. 제품 전체 흐름/웹 URL은 blocker |
| [Forge](https://github.com/digitalbazaar/forge), [advisory](https://github.com/advisories/GHSA-86w9-cpqp-85rv) | 현재1.4.0, RSA verification advisory에 patched version 없음 | SHA256/RS256 signing만 사용; 검증 API 노출/호출 없음. 위험 범위·출시 전 재검토를 숨기지 않음 |

패키지 registry도 다시 확인했다([package audit](package-audit.json)). PGS2.3.0 유지, Auth3.8.0 유지, CloudCode2.10.4 유지. Auth3.8.1은 Targeting attribute의 Low 이상 stripping 수정이며 현재 Minimal/no Targeting 사용이라 활성화에 필요한 변경이 아니다. CloudCode3.0은 Unity6000.7 이상이므로6000.3에 올리지 않았다. 새 서버 dependencies는 node-forge1.4.0/esbuild0.28.2 고정과 lockfile이다. [공식 license](../../backend/THIRD_PARTY_NOTICES.md)를 포함한다.

## 실제 배포와 hash

| 환경 | Script | Published version | 실제 code SHA256 |
|---|---|---|---|
| development | LowfireAccount | 3 | `5f8c610aeccaed0a8f461cc669df68b59b59531b594b565ad7e6e60264550d8b` |
| development | LowfireIntegrityVerify | 2 | `e75819c1e01c08cd864bf3bd48dee2d4d3d4de9fbbdce323da7c60b2c1421898` |
| alpha | LowfireAccount | 2 | `5f8c610aeccaed0a8f461cc669df68b59b59531b594b565ad7e6e60264550d8b` |
| alpha | LowfireIntegrityVerify | 2 | `e75819c1e01c08cd864bf3bd48dee2d4d3d4de9fbbdce323da7c60b2c1421898` |

각 실제 code가 local generated bundle과 일치한다. Account 약44KB, IntegrityVerify 약120KB이고 각128000byte 제한을 build에서 검사한다. 배포물은 ignored deploy에만 생성하고 중복 정본으로 commit하지 않는다. [실제 metadata/config/index/policy](service-inventory.json).

초기 실제 publish는 `Cannot find module crypto`로 실패했다. 지원되지 않는 require를 우회 override로 숨기지 않고 정본 의존성을 교체했다. SDK runtime probe에서 Buffer 제공, WebCrypto/TextEncoder/Node crypto 미제공을 확인했다. RSA를 포함한 한 bundle은128KB를 넘어서 역할을 두 script로 분리했다. 지원 종료된 jsrsasign은 채택하지 않았다. Account가 같은 player token으로 같은 environment의 IntegrityVerify를 호출하고 검증 결과와 ledger 조건을 재검사한다. child가 자격을 직접 지급하는 API는 없다.

Creation gate는 실제 Private item CAS다. write 결과를 모르는 경우 자동 expiry하지 않는 기존 안전 정책은 유지한다. 첫 create 경쟁/unknown outcome은 MODEL 검증이며 실제 장애 주입이라고 하지 않는다. Meta와 ClassRevision은 분리해 문의/audit가 가짜 Class 충돌을 만들지 않는다.

## 필수 감사 항목

| 번호 | 항목 | 현재 확인/한계 |
|---|---|---|
| 1 | 시작 SHA | 0ce1498 fetch·local 동일·clean main |
| 2 | 종료 SHA | 이 REVIEW 포함 commit, 최종 push 결과로 확인 |
| 3 | Foundation1 재감사 | 기존 code/문서/SDK와 실제 미배포 상태를 기준으로 시작. 과거 REVIEW 수정0 |
| 4 | 버전 | PGS2.3.0/Auth3.8.0/CloudCode2.10.4, core1.18.0/multiplayer2.4.0 유지 |
| 5 | 최신 공식 자료 | 위 표, 실제 CLI help/registry/SDK signatures 교차 확인 |
| 6 | UGS Project | 70ba2490-7029-4528-b55a-9c2231fde007. Editor binding name Wildfolk 2026-10-13_09-04-47; 별도 remote project-name API는 미조회 |
| 7 | 환경 | development9aa02b53-8272-4975-a42a-f6a0b75a360f / alphaa9eee8ca-e2a8-4fa7-9530-6df9b230e943 / production9da27a5f-4c79-44bc-b96b-1e490f83619d |
| 8 | 생성·변경 | development/alpha 신규. production 기본 지정/ID/이름/정책·스크립트 보존 |
| 9 | Channel mapping | 모두 명시 name 선택, Auth 실제 resolved ID 검증 후 Cloud/Relay. 빈 값/중복/ID mismatch fail-closed, disabled 상태도 production fallback 없음 |
| 10 | Cloud Code | non-production 각2개 publish/get/hash 일치. 임시 QA script는 제거 |
| 11 | Cloud Save | Private policy/gate, Protected profile/Support index READY, 실제 create/get/CAS/lookup |
| 12 | Access Control | development/alpha 실제 배포. dev owner read 허용·직접 write 거절, Auth/Relay 정상. production 정책은 변경0 |
| 13 | Authentication | 실제 fresh/cached anonymous·동일 PlayerId, Unity SDK roundtrip. 실제 PGS는 미검증 |
| 14 | PGS config | 사용자: 아직 없음. PlayGamesSettings/manifest 없음, Web client ID/secret 미생성. release validator 교차 일치 검사 추가 |
| 15 | Integrity | Alpha closed, Google project/secret/cert 미설정. 실제 Google decode 미검증. model verdict와 signer 상호 검증 |
| 16 | Profile2 import | 실제 dev malformed/no-root/최초 import/repeat 거절/기존 profile 유지. pending/local file 보호는 기존 격리 MODEL 테스트 보존 |
| 17 | Alpha enrollment | actual dev 거절, Alpha policy closed. linked·승인 build/window/실제 proof 모두 필요. 성공은 MODEL |
| 18 | entitlement 이전 | dry-run count/duplicate/already-granted/결정적 operation plan. source 재조회·additive CAS 보존. production 지급0, 모델 검증 |
| 19 | Support ID | 실제 create/re-auth/get에서 안정, protected index unique lookup. credential 아님 |
| 20 | Feedback | 실제6종 submit/중복·즉시 cooldown·invalid input·동의/redaction·admin read. 하루5개는 MODEL, 실제4분 기다려 cap 채우기 미실행 |
| 21 | Audit/suspicious | 실제 account/profile/support/cloud conflict/security flag. linked/alpha/entitlement 성공은 MODEL 또는 외부 검증 대기. 자동 ban0 |
| 22 | Admin lookup | 실제 Hub CLI protected equality query. 기존 특권 service-account CLI도 유지, 그 credential은 미설정 |
| 23 | 삭제 | 실제 dev request pending·중복 안정, QA associated data cleanup 후 identity 삭제. 제품 linked Google/부분 실패/타 기기 완료 흐름은 미검증 |
| 24 | Privacy | 사용자 확인 URL0. 가짜 URL0. public release blocker 유지 |
| 25 | LAN | 실제2프로세스 resource/harvest/chunk/reconnect/save/reHost/Boss/Class 회귀 통과 |
| 26 | Relay | 실제 development Auth·Relay allocation/room-code join/gameplay/rejoin/reHost 통과. 같은 컴퓨터2프로세스, 실제4기기 아님 |
| 27 | Android | com.gltam.lowfire/IL2CPP/ARM64/min25/targetAuto/Minimal/INTERNET 강제. Alpha·Production C#57 assemblies씩 통과, dev identity 제외. AAB/Gradle/기기 미실행 |
| 28 | UI | 실제 Game View20 capture,1920×1080/1280×720/844×390, 한국어·offline/unavailable/conflict/alpha/help/delete. 가상44/20 inset, 물리 notch 미검증 |
| 29 | 자동검사 | Edit17/Play4/backend43, source validator, 두 채널 script compile. 증거 JSON/TAP 참조 |
| 30 | 등급 | 아래3등급 엄격 구분, fixture connected/Alpha 표시를 Google 성공으로 사용하지 않음 |
| 31 | secret | 전체 tracked/추가 source 전수 패턴+구조화 설정/로그 수동 감사. 실제 token/key/credential을 기록하지 않음. npm advisory는 별도 공개 |
| 32 | save | 접근 가능한 기존 UserData2파일 SHA256/mtime/크기 보존. 모든 QA는 Captures/Temp/별도 env/SDK profile. 실제 Profile2 import0 |
| 33 | 외부 잔여 | SETUP_REQUIRED의 담당/위치/값/완료 기준. service remote 미조회는 UNVERIFIED |
| 34 | Foundation2 | development 실제 서버 권위 기반 개발 가능. Google 사용자/공개 Alpha·production 준비 완료는 아님. 다음 사용자 지시 전 STOP |

## 검증 등급과 증거

| 등급 | 실행한 것 | 증거 |
|---|---|---|
| ACTUAL SERVICE | 공식 CLI Hub auth/환경 생성/2환경 배포·정책·index, development HTTP76 assertions, actual Unity anonymous roundtrip,2프로세스 인터넷 Relay, QA data→Auth cleanup | [76 service cases](actual-service-tests.json), [Unity account](unity-actual-cloud.json), [Relay](relay-result.json), [cleanup](qa-cleanup.json) |
| ACTUAL UNITY LOCAL | main Editor·별도 batch Unity, LAN,20 viewport capture, Android release C# compile | [LAN](lan-result.json), [viewports](viewport-result.json), [Android](android-scripts.json), [preflight](android-preflight.json) |
| MODEL / FIXTURE | Editor fake identity/PGS conflict/Alpha 표시, Node memory/CAS/race/실패 복구/Google verdict/native signer comparison | [Edit](edit-final.json), [Play](play-final.json), [backend](server-tests.tap) |

실제 Google 계정 로그인/링크/충돌/기기 복구, 성공한 Integrity/Alpha 등록, 실제 자격 production grant, Android APK/AAB·merged manifest·IL2CPP stripping·실제4기기·모바일 터치/장시간은 미검증이다. UI geometry 통과를 실기기 가독성/손맛 검증으로 표현하지 않는다.

현재 remote PGS provider/Economy/Analytics/Diagnostics 상태는 UNVERIFIED다. 로컬 legacy Analytics/Ads/Purchasing/Diagnostics 설정은 꺼져 있고 신규 SDK/콘텐츠를 추가하지 않았다. UnityConnectSettings의 legacy m_Enabled=0을 현재 UGS 서비스 전체가 꺼졌다는 근거로 쓰지 않는다. 실제 Anonymous/Cloud/Relay 호출이 성공했다.

## 보안·운영 한계와 정리

npm audit는 node-forge RSA verification high1건/수정 release 없음으로 끝났다. 현재 Account bundle은 SHA256만, Integrity는 trusted private key의 RS256 signing만 사용한다. 해당 외부 signature verification API를 호출하거나 request에 노출하지 않는다. 이 제한이 모든 향후 사용의 안전을 보장하는 것은 아니다. 출시/새 검증 기능 전에 dependency 상태를 재검토한다. [audit](dependency-audit.json), [공식 advisory](https://github.com/advisories/GHSA-86w9-cpqp-85rv).

QA Auth identity도 project-wide이므로 새로 생성한 명단만 추적했다. development root/support16keys를 지우고 alpha/production의 해당 QA Protected data가 비어 있음을 확인한 뒤 identity를 삭제했다. Relay QA2개는 모든 환경에 Protected data가 없음을 확인하고 삭제했다. 임시 capabilities/cleanup 함수는 삭제했고 제품 함수2개만 남겼다. 기존 사용자 identity/data를 수정하지 않았다. 운영자 cleanup 검증은 실제 사용자 Google 삭제 전체 흐름의 대체가 아니다.

Protected root/receipt/자격과 client input의 경계, max3 CAS/전송 retry와 exponential250/500ms, bounded audit/receipt/rate/storage는 유지한다. 클라이언트 매frame request/serialize/polling은 추가하지 않았다. Dedicated Host anti-cheat 완성을 주장하지 않는다. 전체 world Cloud backup/Host migration/독립 client Continue도 추가하지 않았다.

[외부 작업](../../docs/SETUP_REQUIRED.md)과 [출시 차단 항목](../../docs/RELEASE_BLOCKERS.md)이 현재 정본이다. 과거 Foundation1 REVIEW는 당시 기록으로 유지한다. 단계 UTC와 process wall time은 timing/process JSON에 기록하고 겹친 시간을 더하지 않는다. 작업별 정확한 token 소비는 제공되지 않아 null이며 추정 수치를 실제 사용량으로 쓰지 않았다.

## 다음에 이어갈 checkpoint

사용자 요청으로 현재 완성분을 정리하고 여기서 멈춘다. 실제 UGS development/alpha 배포·서비스76·Unity 왕복·LAN/Relay·UI20·Android C#까지 완료했으며 제품 Google/URL/AAB는 미완료다. 테스트 상세 객체는 Editor domain reload 이후 MCP에서 소실되어, 앞서 직접 관측한 Passed summary와 영속 succeeded/progress를 함께 기록했다.

다음 작업은 SETUP_REQUIRED의 실제 PGS/OAuth/서명·Google Cloud number·Secret Manager·Privacy/웹 삭제 URL 설정과 실제 Android 검증이다. package는 com.gltam.lowfire로 확정되어 다시 묻지 않는다. 서비스/사용자에게 없는 값이나 secret을 생성·추정하지 않는다. node-forge 검증 advisory 범위도 공개 출시 전에 재검토한다. Foundation2/Gem/Achievement/Attendance는 별도 사용자 지시 전 시작하지 않는다.

## 2026-10-26 후속

사용자 재개 지시 후 실제 Development Android AAB 빌드·검사와 빈 PGS library 정리를 수행했다. [현재 후속 검증](android-development/REVIEW.md). 위 2026-10-25 기록은 당시 결과이며, Google/Alpha/Production·실기기 미검증 경계는 계속 유지한다.
