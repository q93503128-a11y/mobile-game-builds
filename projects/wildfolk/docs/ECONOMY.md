# 계정 경제 정본 · Foundation2

표시 Gem은 세계 Gold/Crystal과 별개 계정 통화다. 가격/업적/출석 표의 단일 작성 원본은 `client/Assets/Wildfolk/Resources/EconomyCatalog.json`; backend mirror는 `node backend/tools/sync-catalog.cjs`로 만든다. 전체 목록·행동 가정·검증은 [REVIEW](../validation/launch-economy-foundation/REVIEW.md)와 [경제 모델](../validation/launch-economy-foundation/economy-model.json)이다. Ads/IAP/상점/신규 progression은 없다.

## 통화와 영구 소유

초기0Gem, Engineer120Gem, SP 현장 부활8Gem이다. Engineer는 기존 utility/support 직업으로 필수가 아니다. 기존4개 Gold 직업 각60·Survivor 기본 소유·6Class54node·성장/공격 수치는 유지한다. CloudProfile의 Engineer flag나 Profile2 import는 프리미엄 획득 증거가 아니다. Account 응답은 `lf_economy`의 유효한 class_unlock debit receipt로 Engineer 소유를 투영한다. 실제 구매 후 Account Sync로 캐시를 받으며 offline cache는 플레이 편의용이다. 원격 Engineer는 아래 Foundation2.1의 인증된 서버 admission을 통과해야 한다. 원격 XP/node는 계속 session-only이며 Gold 직업의 기존 입장 정책은 유지한다.

## 서버와 거래

Protected player `lf_economy` Schema2에 integer Balance, 최근 Ledger, archive checkpoint, metric/claim/attendance receipt, Engineer의 원본 해금 증거, revive reservation, devices/critical source, bounded security/audit/operation을 저장한다. 전체 금융 이력은 Private Game Data archive와 영구 source index를 포함한 정본이다. 기존 Schema1은 거래/잔액/claim을 보존해 서버에서 이관한다. Client wire snapshot은 Schema1을 유지한다. 기존 `lf_account` CloudProfile1과 world Save14/local Profile2는 별개다. `LowfireEconomy`가 인증 context의 PlayerId/환경/channel/provider/삭제 상태·Private economy_config를 검증하고 같은 key의 writeLock CAS로 지급/차감/수령을 원자 확정한다. 첫 생성은 기존 Private creation_gate를 재사용한다. Economy Currency를 따로 복제하지 않는다. Player의 Protected/default data 직접 쓰기는 Access Control이 차단한다.

각 Gem 거래는 TransactionId/PlayerId/Timestamp/Delta/BalanceBefore/BalanceAfter/Reason/SourceId/OperationId/BuildChannel/MetadataVersion을 보존한다. 숫자는 정수이고 잔액0 미만은 거절한다. 공개 op는 GetEconomy, SubmitProgress, ClaimAchievement, ClaimAll, ClaimAttendance, UnlockEngineer, BeginRevive, FinishRevive, TransactionHistory뿐이다. 내부 transact의 grant/spend를 클라이언트 일반 API로 노출하지 않는다. achievement:family:tier, attendance:board:period:slot, class:engineer, revive:attempt, admin:operation source는 영구 중복 방지 receipt다. 과거 tier ID/순서를 재사용/재정렬하지 않는다.

CloudProfile1/local Profile2/Save14는 유지하고 기존 NetworkCommonGrowth의 서버→owner 진행 RPC를 추가해 Protocol23을 사용한다. 구 Protocol21/22 peer와 접속을 허용하지 않는다.

## 업적과 진행 기록

22family58tier(특수9), 7category, 전체213Gem/785비소비 Point다. 완료는 metric에서 계산하고 Point는 완료만으로 표시한다. Gem은 온라인 별도 Claim 또는 ClaimAll이 확정한다. ClaimAll 전체는 같은 CAS 한 번으로 확정하므로 일부만 중복 지급하지 않는다. 숨김 업적은 사용하지 않는다.

정본 성공 이벤트: 실제 creature death once commit/공유 reward 대상, 자원 파괴, instant craft 또는 processing output 수령1행동, 설치, fishing catch 성공, full night alive, 동료 구조 확정, 보스 durable world reward, Gold class 실제 소유 확정, 처음 배운 서로 다른 node, Workbench 업그레이드, Diamond 장비 제작, 동굴/Deep 진입, surface biome4, Crown 장착, offering world commit, Camp 물품 수령이다. 단순 replica 표시/로드/import/UI 버튼에서는 진행을 만들지 않는다. wildlife는 동물 전체를 셀 수 있으나 hostiles에는 Undead/Goblin/공격적인 wildlife만 포함한다.

호스트 권위 이벤트는 기존 플레이어 NetworkCommonGrowth의 server-only owner RPC로 수신 owner의 journal에 전달한다. Client replica는 같은 사건을 직접 다시 세지 않는다. owner-local 탐험/밤 관찰은0.5초 간격이며 loading 중간 밤은 완료로 보상하지 않는다. 보스는 seed/generation/tomb/night, full night는 seed/generation/night, class/node는 고정 source 해시를 보존해 reHost/respec 재보상을 막는다. 세계 seed 복제는 같은 source로 본다.

오프라인은 account/environment별 checksum+atomic replace+backup aggregate journal에만 진행을 저장한다. 첫 로그인 전 guest journal은 첫 계정 하나에만 두 파일 binding 후 채택한다. 기존 Profile2/공용 Level/과거 보스를 일괄 환산하지 않는다. 로그아웃 후 별도 guest로 얻은 새 진행을 계속 여러 계정에 재수입하지 않는다. 같은 계정의 offline cache 상태에서는 account journal을 그대로 사용한다.

per-device 증가 sequence/high-water와 고정 operation의 frozen outbox를 서버가 확인한 뒤에만 제거한다. 한 요청에서 여러 metric을 합치고 최대5분 간격·화면 열기·명시 Sync 때 전송한다. 한 번의 Sync는 시작할 때 고정한 outbox만 처리한다. 요청 중 생긴 새 진행은 다음 Sync로 남겨 두고, 마지막 sequence보다 작은 ack는 outbox를 지우지 않고 중단한다. 매 kill HTTP는 없다. 한 계정64device, permanent critical4096, local recent2048/outbox32, mutation500/day, 서버/클라이언트 각 최대3retry다. metric delta/daily cap은 유한 전체 목표까지이며100000kill/50boss 같은 불가능한 burst를 거절하고 Suspicious/audit만 남긴다. 실제 행동 발생은 player-host/offline unverified다. 느리게 그럴듯한 가짜 이벤트를 만드는 modified Host까지 방어하지 않으며 자동 영구 Ban은 없다. read spam은 UGS project quota/throttle와 운영 모니터링이 필요하다.

## 출석과 시간

서버 UTC00시가 day 경계이고 monthly period는 UTC YYYY-MM이다. 장치 날짜/금액/index는 수령 권위가 아니다. 화면 countdown은 마지막 서버 시각+monotonic elapsed의 안내이며 실제 수령 때 서버가 재판정한다.

신규7회는 계정당1회, 비연속 누적이며 보상3/4/5/6/7/8/12 =45Gem. Monthly는 달마다 최대28회,7/14/21/28회에6/8/10/16 =40Gem. 일반 날은 해당 board 출석 기록/다음 milestone 진행만 남긴다. 이 표식은 인벤토리 item/통화가 아니고 다른 world에 Gold를 배송하지 않는다. 월말 남은 미수령 slot은 새 달에 이어 붙이지 않으며 신규 출석은 달이 바뀌어도 유지한다.

Private `lowfire_backend/economy_config`의 Schema1 Enabled/Events[]가 기간 이벤트를 설정한다. 각 event는 EventId/NameKo/NameEn/StartAt(포함)/EndAt(제외)/ClaimCount1..28/Rewards/Eligibility(Channels,MinimumAccountAgeDays,RequireLinked)를 가진다. 동시에 신규·월간·event를 각각 받을 수 있다. 첫 수령 때 definition hash를 고정하며 같은 ID의 보상표를 중간 교체하면 fail-closed다. 최대8개 활성 config/128개 보존 ID. 새 기간은 새 immutable ID를 사용한다. 실제 배포 Events=[]; 공개 중인 가짜 이벤트는 없다. QA 화면의 예시 이벤트는 isolated Editor fixture뿐이다.

## 부활의 두 저장소 경계

실제 지갑은 비동기다. 로컬 account journal에 world path/run/death/임의 attempt/operation을 먼저 저장하고 server BeginRevive가8Gem 예약 차감한 뒤, 같은 죽음 revision에 한해 기존 atomic world save로 부활한다. 로컬 확정 후 FinishRevive(committed), 명백한 실패는 failed의 단1회 +8 compensation이다. 잔액/현장 행동이 같은 분산 원자 저장이라는 주장은 하지 않는다.

Prepared/CommitStarted/Committed/Failed intent를 재시작 후 확인한다. CommitStarted는 primary Save14의 run/death/Dead를 읽어 성공 또는 실패를 대조하며, 파일 미확인/다른 run이면 오래된 backup으로 환급을 추정하지 않고 pending으로 유지한다. 네트워크 unknown outcome은 같은 attempt/operation으로 복구한다. OS backup/다른 기기에서 local intent가 유실된 원격 Reserved는 지원 복구가 필요하다. pending 동안 추가 cloud spending은 차단하고 무료 Home은 유지한다. 늦게 온 콜백은 Home 후 새 원정을 다시 살리지 못한다. offline cache balance로 소비하지 않는다. 수정된 로컬 Host의 거짓 실패/로컬 HP 조작까지 서버가 증명하지는 못한다.

## 운영·삭제·용량

Admin `economy-lookup`은 실제 account/env scope를 읽는다. `economy-adjust --request <JSON> --operator <id> --case <ticket>`는 delta/reason/operationId를 필수로 받고 기본 dry-run, 명시 --apply만 writeLock CAS한다. credentials는 기존 Dashboard/운영 환경변수로 직접 설정하며 채팅/Git에 넣지 않는다. player client로 호출할 public admin endpoint는 없다. class_unlock/refund/support case 사유가 ledger/audit에 남는다. 피드백 진단 동의가 있을 때만 마지막 공개 경제 receipt ID와 version을 첨부하며 token/world path/inventory는 전송하지 않는다.

일반 audit64개·operation cache256개는 bounded다. 금융행과 source receipt는 삭제하지 않고 아래 Private archive로 옮긴다. root3MiB와 hot768행, index bucket2000key/4MiB에서 fail-closed한다. 기존8192행은 과거 Schema1 reader의 상한이며 새 금융 이력 전체 상한이 아니다. provider 확인/Ads SDK/Billing 상품/가짜 충전 버튼은 없다.

계정 삭제 운영은 각 환경에서 Private archive와 source/resolution index를 먼저 지운 후 Default/Public/Protected player data와 마지막 Authentication을 삭제한다. root manifest를 먼저 지우지 않는다. operator journal로 중간 실패를 재개한다. 현재 기기는 wallet/cache/revive intent와 해당 계정에 bound된 guest economic journal을 정리하며 실제 world/guest Profile2는 보존한다. 수집 inventory에 economic ledger, progress/device random ID/source hashes, attendance/Engineer/revive receipts를 추가한다. Privacy/삭제 URL, Google/PGS/Integrity/실기기 설정은 여전히 RELEASE_BLOCKERS다. Alpha 통화의 Production 자동 이전은 제공하지 않으며 Alpha Tester entitlement와 분리한다.

UI 상세 tier는 ContentSizeFitter의 preferred height를 사용하고 Canvas pixel scale이 실제로 바뀔 때만 텍스트/layout을 다시 계산한다. 같은 scale의 frame에서는 rebuild하지 않는다. 직업 수첩의 기존 Gold/Gem 칸을 각각 실제 값에 연결한다.

## Foundation2.1 · 서버 admission / 원장 수명

인증된 소유자가 `LowfirePremiumAdmission`에서 Engineer 증명을 발급받고, 인증된 Host가 같은 서버에서 다시 확인한다. 증명은 실제 PlayerId/HostPlayerId/session/admission/profile/selected class/operation과 account·economy revision에 묶이며 서버 시간90초 유효, 동시16개다. 소유권은 실제 class_unlock debit 및 보호된 Engineer.Proof로 검증한다. NGO에는 bearer credential을 보내지 않는다. 발급/검증은 입장 시에만 하며 매 frame 호출하지 않는다. reconnect마다 admission nonce를 바꾸고 reHost마다 session을 바꾼다. Host 자신의 Engineer도 같은 경계다. backend unavailable/미소유/timeout은 Survivor로 입장하고 번역된 상태를 표시하며 영구 선택/XP를 바꾸지 않는다.

hot512행부터 확정된 앞256행을 한 번에 archive한다. segment는 Private Custom Item 하나의 immutable JSON으로 scope/절대 sequence/opening·closing balance/previous hash를 포함한다. HashVersion1은 재귀 정렬 JSON의 SHA256이며 Cloud Save object key 순서 변화에 안전하다. source SHA256 앞2hex의256 Custom Item에 개별 immutable key를 둔다. item ID/key는 전체 SHA256의 base64url을 사용해45자이며, 예전 SDK50자 제한도 만족한다. source 조회는 index key1개와 segment1개, UI는 최근 hot최대50행만 읽는다.

순서는 archive 쓰기·재조회 검증 → source/resolution index 쓰기(concurrency16) → root writeLock CAS다. speculative 거래를 archive하지 않는다. 성공 응답 유실 시 같은 확정 prefix를 재시도하며 root checkpoint/count가 권위다. root CAS 전 orphan 한 segment/index set이 남을 수 있으며 같은 prefix 재시도로 회복한다. 한 invocation에서 한 segment, CAS최대3회. legacy8192행은 요청당256행씩 점진 compact하며 금융행을 버리지 않는다. Engineer.Proof는 이관 때 보존하고 terminal revive는 최근32개만 root에, 이전 terminal 결과는 영구 resolution key에 둔다. Reserved는 prune하지 않는다.

achievement/attendance는 immutable claim identity를 보존한다. future Ads/IAP는 검증된 server receipt만 받는 내부 binding 경계와 메모리 fixture가 있으며 public grant API/SDK/provider verifier는 없다. provider+환경+receipt hash 전역 owner binding이 다른 계정의 재사용을 차단한다. raw purchase token은 저장하지 않는다. 실제 provider 검증·restore/refund·개인정보 보존/삭제 정책은 다음 단계에서 확정해야 한다.

capacity는 실제 serialized root/index bytes와 hot/archive 개수를 지원 도구에 노출한다. root3MiB, bucket2000key/4MiB의75%/90%에서 warning/critical이며 플레이어 popup은 없다. 100k 모델은 root193217B/hot416/archive389/Custom Item645/최대bucket450key, archive+index+root59006349B다. 이는 서비스 성능·비용 보장이 아니다. 실제512행 이관은 archive256/hot256 및 서버1745ms를 확인했다. 정상 Get도 maintenance 확인을 위한 root read 비용이 추가되며 archive는 threshold/recovery 때만 쓴다. 비용·대량 운영 경고 전달 job·실기기 성능은 별도 운영 검증이다.

현재 구현·실제 검증·남은 검증은 [Foundation2.1 REVIEW](../validation/economy-premium-mp-ledger/REVIEW.md)와 [재개 목록](../validation/economy-premium-mp-ledger/RESUME.md)에서 구분한다.
