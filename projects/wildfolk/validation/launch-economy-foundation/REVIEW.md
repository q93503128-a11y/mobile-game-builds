# Launch Economy Foundation2 · 조사와 구현 기록

시작 main `f1ae334ffa81c8dad103cb7c3b1d854df554d342`. remote fetch/local 일치·clean 확인 후 시작했다. 아래 외부 조사를 먼저 수행했으며 구현/최종 검증 결과는 완료 순서에 맞춰 추가한다. Foundation1/1.5의 과거 검증을 이번 실행으로 재분류하지 않는다.

## 외부 조사 비교 (2026-10-26 열람)

| 게임 | 실제 공개 사례 | 장점 | Lowfire와 차이 | 적용/배제와 이유 |
|---|---|---|---|---|
| Clash of Clans | [공식 Builder 설명](https://support.supercell.com/clash-of-clans/en/articles/builders-4.html): 추가 Builder250/500/1000/2000Gem. [2026-06 공식 패치](https://supercell.com/en/games/clashofclans/blog/release-notes/the-anime-fury-update-is-here/)는 누적 Hero Level1당 track1점과 Ranked 업적 Gem 재조정을 명시. [2025 공지](https://supercell.com/en/games/clashofclans/blog/news/battle-and-ranked-modes-are-coming-to-clash-/)는 구 League All-Star 교체를 예고했다. | 영구 효용 구매와 단계별 누적 목표를 구분하고 운영 중 보상을 조정할 수 있다. | Lowfire Engineer는 필수 생산 슬롯이 아닌 지원 직업이며 세계 Gold와 계정 Gem이 분리돼 있다. | 영구 해금과 반복 revive 비용을 분리한다. 오래된 All-Star450/1000/2000Gem 수치를 현재 경제로 복사하지 않는다. 업적 ID/이미 수령한 receipt는 보상표 개정에도 보존한다. |
| Brawl Stars | [2025 공식 회고](https://supercell.com/en/games/brawlstars/blog/news/rip-masteries/): Title 보유11.8%, 반복 farming 중심 Masteries를 없애고 Records·Trophy Road로 재배치. [2026-01 공식 이벤트](https://supercell.com/en/games/brawlstars/blog/community/randombs-event-breakdown/): UTC 약08시 갱신, 공동 takedown 목표와 서로 다른 일별 보상, Gem111/11/1 선택 사례. [공식 해금 설명](https://ingame.support.supercell.com/brawl-stars/en/articles/unlocking-brawlers.html): Credit 진행 또는 Gem 해금. | prestige와 경제 지급처를 구분하고 기간/갱신을 명시한다. | PvP 로스터·공동 목표·랜덤 보상은 Lowfire의 소규모 생존 루프와 다르다. | 프로필 Point와 Gem을 구분하고 읽기 쉬운 보상/받기 badge를 둔다. 대량 반복 노동·가챠·커뮤니티 목표는 넣지 않는다. 예전 Mastery 수치를 현행 시스템으로 서술하지 않는다. |
| Genshin Impact | [HoYoWiki 공개 업적 표](https://wiki.hoyolab.com/pc/genshin/entry/5832): Opening Night Act3/6/8, Toy Medal3/6/9에 각각5/10/20Primogem, 단일 지원 캐릭터 조건 업적도 있다. [공식 출석 진입을 연결한 팬 자료](https://traveler.gg/hoyolab-community-daily-check-in/)는 2023년 기준 첫28회60Primogem·월1일 초기화·나머지날 Mora를 설명한다. | 행동 빈도에 따라 다른 threshold와 보상 간격을 쓰고, 누적·특수 조건을 함께 제공한다. | 대형 가챠 게임이며 Mora/아이템은 같은 계정 세계로 배송된다. | metric별 threshold를 따로 정하고 family 카드·상세 tier를 사용한다. 팬 자료의2023 수익률은 현재 검증값으로 복사하지 않는다. Lowfire 여러 Host world에 Gold/아이템을 배달하는 거래는 이번에 만들지 않는다. |
| Honkai: Star Rail | [공식 HoYoWiki Gift of Odyssey](https://wiki.hoyolab.com/pc/hsr/entry/1394?lang=en-us): 기간 중 비연속7회 로그인으로10Special Pass, 누적 일수 순차 보상, server04시 reset. 공개 페이지의2.0 기간은2024년 사례다. | 결석으로 streak를 잃지 않고 종료 기간과 다음 단계를 알 수 있다. | Pass는 가챠 티켓이고 지역 서버별 timezone이 있다. | 신규 누적7회와 기간형 Event board를 채택한다. 티켓·가챠와 사례 기간/현재 일정은 채택하지 않는다. Lowfire는 단일 UTC00시 day/month를 사용한다. |
| Terraria | [개발사가 등록한 Steam 업적](https://steamcommunity.com/stats/105600/achievements): 첫 나무, 첫 완전한 밤, Workbench 제작, Eye of Cthulhu 처치처럼 서로 다른 진행 사건을 함께 노출한다. | 평소 진행과 기억할 만한 사건을 결합한다. | 플랫폼 prestige이며 Lowfire처럼 업적 Gem을 서버 지급하지 않는다. | Workbench·동굴·보스의 실제 확정 이벤트를 사용한다. 비슷한 초반 재료마다 업적을 쪼개지 않는다. local 플랫폼 업적의 신뢰를 premium wallet에 적용하지 않는다. |
| Core Keeper | [개발사가 등록한 Steam 업적](https://steamcommunity.com/stats/1621690/achievements): 낚시/채광/제작 등 skill100, cookbook100 recipe, 첫 pet 부화·waypoint·특정 장비 조합 등이 병존한다. | 공통 생활 목표와 특별 발견을 분리한다. | skill 상한·recipe 발견·pet/waypoint 기능이 Lowfire와 다르다. | 채집/제작/낚시의 장기 목표를 둔다. Lowfire에 없는 pet/waypoint와 가짜100 recipe 발견 지표는 넣지 않는다. 이미 있는111 recipe 중 실제 제작 성공만 집계한다. |

본문 조회가 JS shell인 HoYoWiki는 검색 색인에 노출된 해당 공개 표/규칙까지 확인했다. 현재 재화 공급률을 검증하지 못한 부분은 위에 날짜·한계를 표시했다. Claim All의 현재 외부 게임 지원 여부는 확인 자료가 부족해 그 게임의 기능이라고 주장하지 않는다. Lowfire에서는 사용자 요구와 한 번의 서버 batch CAS로 보상을 확정할 수 있다는 이유로 제공한다. 외부 asset/code/layout은 가져오지 않는다.

## Backend 선택

[Unity Economy 현행 공식 안내](https://docs.unity.com/en-us/economy/index)는2026-09-08 이후 신규 프로젝트 가입을 받지 않고, 신규 경제에 Cloud Code/Cloud Save/Remote Config를 권한다. Lowfire의 기존 Economy 활성 config는 확인된 적이 없어 그것에 의존하지 않는다. [Economy virtual purchase](https://docs.unity.com/en-us/Economy/add-virtual-purchase)는 자체 자원 교환에 적합하지만 별도 Cloud Save 업적 claim과 하나의 cross-service 원자 거래라는 근거는 없다.

실제 배포된 Cloud Save Protected/Cloud Code를 재사용하고 CloudProfile1과 별도 `lf_economy` Schema1을 **유일한 wallet 정본**으로 선택한다. balance·ledger·claim receipt·Engineer ownership을 같은 key의 writeLock CAS로 확정한다. [Protected 권한/5MiB](https://docs.unity.com/en-us/cloud-save/concepts/player-data), [write lock/첫 생성의 lock 생략](https://docs.unity.com/en-us/cloud-save/concepts/write-locks)을 확인했다. 첫 생성은 기존 Private creation gate를 재사용한다. Economy Currency와 balance를 이중 기록하는 대안은 배제한다.

모든 ledger 행을 보존하며 저장 상한에 다다르면 mutation을 원자적으로 거절한다. 자동으로 오래된 거래를 지워 감사/중복 경계를 잃지 않는다. 초기 root 한계와 향후 archive/Ads·IAP 확대 조건은 구현·검증 결과에 수치로 기록한다. world Gold/Crystal, Profile2 및 Host world save에 wallet을 넣지 않는다.

## 출석·소유권 결정

HSR의 비연속 누적과 Unity의 [Daily Rewards 사례](https://docs.unity.com/en-us/services/solutions/daily-rewards)는 결석 후 같은 순번을 이어 받는 구조를 뒷받침한다. [server time](https://docs.unity.com/en-us/cloud-code/scripts/use-cases/server-time-anti-cheat), [기간 보상 재검사](https://docs.unity.com/en-us/services/solutions/seasonal-events)를 따른다. newcomer7회는 계정당 한 번, monthly는 UTC 달마다 최대28회, event는 UTC 시작 포함/종료 제외다.

Genshin의 다른 날 Mora/아이템을 Lowfire에 그대로 넣으면 어느 Host world에 지급했는지 분산 거래가 필요하다. 사용자가 허용한 소유권 단순화에 따라 월간 일반 날은 **해당 board의 출석 기록과 다음 milestone 진행**만 남기고 Gem milestone을 분명히 표시한다. 출석 표식은 별도 재화·아이템·구매권이 아니다. 매일 Gem을 채우거나 가짜 미래 물품을 주는 대안, account Gold 신설은 배제한다. 화면은 Gem이 없는 날을 Gem 지급처럼 표현하지 않는다.

신규7일의 비교적 빠른 무료 공급, 장기 월간 공급, 일회성 업적, Engineer 영구 해금과 revive 반복 소비를 함께 산출한 뒤 값을 확정한다. 실측 session length가 없으므로 completion 시나리오는 가정이며 실제 플레이 측정으로 포장하지 않는다.

## 실제 출시 데이터·threshold 원칙

가격/보상의 작성 원본은 Resources/EconomyCatalog.json이며 backend mirror는 sync-catalog 도구로 생성한다. 상용 사례의 초반 빠른 보상/증가 간격/특별 사건 구조를 채택하되, 아래 값은 Lowfire1.0의 채집·제작·밤·class·단일 보스 범위와 명시적인 진행 모델로 선택한 초기 수치다. 실제 retention/플레이 속도 측정값이 아니다. Tier reward를 모두 동일5Gem으로 주거나 모든 metric에 같은 threshold, 보스500회, 재료마다 초반 업적을 쪼개는 대안은 배제했다. 숨김 조건은 사용하지 않는다.

| Family / 실제 이름 | Category | Threshold | Gem | Point | 집계/선택 이유 |
|---|---|---|---|---|---|
| hostiles / 캠프의 수호자 | Combat | 5/30/120/360 | 1/2/3/5 | 5/10/15/20 | 밤/이동 전투 빈도에 맞춘5→30→120→360, 무해 동물 제외 |
| undead / 밤을 걷는 자들 | Combat | 3/20/80/240 | 1/2/3/5 | 5/10/15/20 | Night 반복3→20→80→240; faction 중심 |
| goblins / 길 위의 위협 | Combat | 3/15/60/180 | 1/2/3/5 | 5/10/15/20 | 낮 무리/Camp3→15→60→180; 독립 variant 농사 금지 |
| wildlife / 야생의 경계 | Combat | 2/12/45/120 | 1/2/3/5 | 5/10/15/20 | 식량 사냥2→12→45→120; 적 처치와 분리 |
| harvesting / 대지의 선물 | Gathering | 12/80/300/900 | 1/2/3/5 | 5/10/15/20 | 명확한1.0 진행 사건 한 번: 자원 지점을 채집해 고갈시키세요. 산출물 수가 아닌 지점 수입니다. |
| crafting / 만드는 손 | Gathering | 6/30/100/240 | 1/2/3/5 | 5/10/15/20 | 명확한1.0 진행 사건 한 번: 즉시 제작을 완료하거나 가공 결과를 수령하세요. |
| fishing / 물가의 기록 | Life | 3/15/50/120 | 1/2/3/5 | 5/10/15/20 | 명확한1.0 진행 사건 한 번: 실제로 물고기를 낚아 가방에 받으세요. |
| building / 작은 정착지 | Progression | 3/15/40/100 | 1/2/3/5 | 5/10/15/20 | 명확한1.0 진행 사건 한 번: 직접 구조물을 설치하세요. 복원된 건물은 세지 않습니다. |
| nights / 아침을 맞으며 | Survival | 1/4/12/30 | 1/2/3/5 | 5/10/15/20 | 중간 밤 Load 제외, full night1→4→12→30 |
| rescue / 서로의 등불 | Coop | 1/4/12/30 | 1/2/3/5 | 5/10/15/20 | 명확한1.0 진행 사건 한 번: 동료 구조를 끝까지 완료하세요. |
| king / 왕의 침묵 | Combat | 1/2/3 | 6/8/10 | 15/20/25 | 명확한1.0 진행 사건 한 번: Undead King 보상 확정에 참여하세요. |
| classes / 서로 다른 길 | Progression | 1/2/4 | 3/4/7 | 10/15/20 | 명확한1.0 진행 사건 한 번: Gold 직업을 새로 해금하세요. 이전 Profile을 보상 증거로 쓰지 않습니다. |
| learning / 직업의 길 | Progression | 1/3/6 | 2/4/6 | 10/15/20 | 명확한1.0 진행 사건 한 번: 서로 다른 직업 노드를 새로 배우세요. 재분배 중복은 세지 않습니다. |
| workbench_ii / 더 단단한 도구 | Progression | 1 | 5 | 10 | 명확한1.0 진행 사건 한 번: 제작대를 II로 업그레이드하세요. |
| workbench_iii / 깊은 곳을 향해 | Progression | 1 | 8 | 20 | 명확한1.0 진행 사건 한 번: 제작대를 III로 업그레이드하세요. |
| diamond / 결정의 장인 | Progression | 1 | 8 | 20 | 명확한1.0 진행 사건 한 번: Diamond 장비를 직접 제작하세요. |
| cavern / 땅 아래의 길 | Exploration | 1 | 4 | 10 | 명확한1.0 진행 사건 한 번: Caverns에 들어가세요. |
| deep / 깊은 동굴 | Exploration | 1 | 8 | 20 | 명확한1.0 진행 사건 한 번: Deep Caverns에 도달하세요. |
| biomes / 네 가지 풍경 | Exploration | 4 | 6 | 20 | 명확한1.0 진행 사건 한 번: 네 Surface Biome을 모두 방문하세요. |
| crown / 쓰러진 왕의 표식 | Combat | 1 | 6 | 15 | 명확한1.0 진행 사건 한 번: Undead Crown을 장착하세요. |
| summoning / 왕을 깨우다 | Combat | 1 | 4 | 10 | 명확한1.0 진행 사건 한 번: 왕묘에 제물을 바쳐 소환을 확정하세요. |
| camp / 확보한 보급품 | Exploration | 1 | 4 | 10 | 명확한1.0 진행 사건 한 번: 전투가 끝난 Goblin Camp의 보급품을 받으세요. |

총22family/58tier/특수9/7category, 전체213Gem/785Point. Point는 완료의 비소비 prestige이며 Claim 전에 표시한다. Gem은 별도 서버 수령이다. Recipe/item/스탯/생물/보스/Workbench progression은 추가하거나 바꾸지 않았다.

## 보상표와 경제 계산

신규7회: 3/4/5/6/7/8/12 =45Gem. 월간 일반24slot은 출석 기록만 남기며,7/14/21/28회에6/8/10/16 =40Gem이다. 일반 slot을 Gem/아이템 지급처럼 표시하지 않는다. 의미 없는 계정 Gold를 만들어 다른 world로 중복 배송하는 구조를 피했다. Event의 실제 config는 빈 배열이며 fixture 예시7일은 공개 이벤트가 아니다.

초기0Gem, Engineer120Gem, revive8Gem. 영구 지원 직업은30일 월간40Gem의3배, 부활15회의 기회비용이다. 업적을 진행하면서 신규7회45Gem을 받으면5h/4일 예시124Gem에서 선택 가능하며, 반복 revive를 쓰면 늦어진다. 기존 Gold class60은 세계 통화로 교환율을 만들지 않는다. 초기Gem100 일괄지급/Engineer100·revive10 예시를 그대로 복사하는 대안은 배제했다. 월간40은 장기적으로 부활5회/월이며 일회 업적 고갈 후 무료 공급이 무한 부활을 보장하지 않는다.

| 모델 시점 | 가정 달력일/누적 시간 | 업적Gem | 신규Gem | 월간Gem | 합계 | Engineer 보류 시 부활횟수 | Engineer 구매 후 잔액/부활 |
|---|---|---|---|---|---|---|---|
| 30m | 1일 / 0.5h | 7 | 3 | 0 | 10 | 1 | 0 / 0 |
| 1h | 1일 / 1h | 27 | 3 | 0 | 30 | 3 | 0 / 0 |
| 3h | 2일 / 3h | 54 | 7 | 0 | 61 | 7 | 0 / 0 |
| 5h | 4일 / 5h | 106 | 18 | 0 | 124 | 15 | 4 / 0 |
| 10h | 7일 / 10h | 141 | 45 | 6 | 192 | 24 | 72 / 9 |
| 20h | 14일 / 20h | 202 | 45 | 14 | 261 | 32 | 141 / 17 |
| 30d-month-aligned | 30일 / 30h | 202 | 45 | 40 | 287 | 35 | 167 / 20 |
| 90d-month-aligned | 90일 / 90h | 207 | 45 | 120 | 372 | 46 | 252 / 31 |
| 30d-start-Oct26 | 30일 / 30h | 202 | 45 | 24 | 271 | 33 | 151 / 18 |
| 90d-start-Oct26 | 90일 / 90h | 207 | 45 | 104 | 356 | 44 | 236 / 29 |

1h 모델은 적25/자원80node/제작25/낚시8/건축8/밤4/Gold직업1/학습1/WBII/Camp1을 가정한다.3h는 동굴·첫 구조,5h는WBIII/첫Diamond장비/Deep·소환,10h는첫보스·Gold4직업·node6,20h는보스3회 등으로 넓어진다. 모든 행의 행동 벡터는 tests/economy_model.py에 공개돼 재현 가능하다. Crown20% RNG 보상은 모델에서 제외하며 co-op 구조는 친구와 실제 플레이한 가정이다. solo는 구조 관련 최대11Gem이 줄지만 Engineer가 co-op 필수로 잠기지 않는다. 월초 정렬/2026-10-26 시작을 분리해 실제 달력30/31·2월 영향을 숨기지 않는다.20회 월간 출석만 한 보수 시나리오도 JSON에 기록했다. 소비0을 가정한 공급 모델이며 Ads/IAP 공급/수익/과금 전환율을 가정하지 않는다.

## 구현 경계·보안·운영

- 유일 wallet은 Protected lf_economy Schema1. 원장/수령/Engineer/debit를 한 CAS로 확정한다. 일반 client Grant/Spend/SetBalance/Admin/Ads/IAP endpoint는 없다. 모든 transaction 필수11필드와 immutable source receipt, 음수 잔액 거절, bounded retry, operation payload mismatch flag, reverse history pagination을 제공한다.
- GetEconomy/TransactionHistory, SubmitProgress, ClaimAchievement/ClaimAll, ClaimAttendance, UnlockEngineer, Begin/FinishRevive를 실제 LowfireEconomy에 배포했다. 기존 LowfireAccount는 ledger premium receipt를 검증해 Engineer 소유를 투영한다. Profile2 import는 premium 증거가 아니며 offline/원격 class claim을 새 유료 소유권으로 승인하지 않는다.
- 진행은 정본 성공 이벤트와 server→owner NGO RPC, owner-local 탐험/밤 관찰에서만 기록한다. UI는 progress를 생성하지 않는다. 최근 로컬 source와 permanent boss/night/node/class source dedupe, device stream high-water, aggregate outbox·서버 claim receipt를 함께 쓴다. import/로드/reHost/respec 이벤트를 다시 보상하지 않는다.
- gameplay source는 player-host/offline unverified다. Play Integrity는 Alpha/Production의 approved build/실제 provider proof에 필요하지만 modified Host가 천천히 그럴듯한 행동을 보내는 것을 증명하지 못한다. 명백한100000kill/50boss burst 거절·Suspicious/audit까지만 하며 자동 영구 Ban은 없다. 실제 Google/Integrity 설정·기기 검증은 아직 없으므로 증명했다고 보고하지 않는다.
- 부활은 서버8Gem reserve→기존 world atomic save→committed 또는 단1회+8 compensation이다. durable local intent가 Prepared/CommitStarted/Committed/Failed를 기록하고 Save14의 run/death/Dead를 대조한다. unknown 파일/다른 run/응답 유실은 추정 환급하지 않고 pending으로 복구한다. 다른 기기/백업으로 local intent가 유실된 reservation은 지원 복구가 필요하다. 서버/디스크의 하나의 분산 원자 거래라는 주장은 하지 않는다. offline spending은 항상 불가하며 Home은 무료로 남긴다.
- Admin economy-lookup/economy-adjust는 서버 역할·scope·operator/case/reason/operation을 필수로 하고 기본 dry-run이다. 실제 개발 QA 보정은 canonical AdminAdjust 로직+Hub CLI writeLock으로 제한된 새 QA account에만 적용했다. 공개 player endpoint나 비밀값을 넣은 config는 만들지 않았다. 동의한 feedback만 마지막 공개 receipt ID를 첨부한다.
- ledger는 영구 append-only8192행/root3MiB에서 fail-closed, Protected 총5MiB보다 낮다. audit64/op256/devices64/critical4096·local outbox32/recent2048·mutations500/day를 제한한다. 오래된 금융 receipt를 자동 삭제하는 대안은 배제했다. 향후 Ads/IAP 대량 공급 전에 archive/idempotency migration·capacity 알림/비용 검증이 필요하다.
- 삭제 inventory는 모든 환경 Protected keys와 lf_economy, 현재 기기 economic cache/intent/bound guest economic journal을 포함한다. 실제 world/원본 Profile2는 QA나 계정 삭제에서 보상 테스트 목적으로 변형하지 않는다.

## Offline·UI·요청 비용

Offline은 last-known 잔액/진행과 unsynced 상태만 표시한다. claim/Engineer/revive는 불가하다. Profile/MainMenu의 수령 badge,7category+family scroll+실제 tier detail/Claim·ClaimAll, 신규/월간/event7×4 board와 completed seal·다음 순번 강조, 신뢰 시각 기반 reset 안내, 명시 Sync, 기존 Class 확인 modal/Gem 잔액·SP DeathHud를 연결했다. EN/KO 정본 키를 사용하며 가짜 상점 + 버튼은 없다. 씬은 기존 Camp Journal authoring으로 직렬화했고 runtime UI를 매번 새로 만들지 않는다.

UI는 상태 변경 때 갱신하며 탐험/밤은0.5초 관찰, network 제출은5분 간격·화면 열기/수령·명시 sync다.30분6/1h12 progress call+menu/claim6회 가정이다. 일반 경제 요청은 CloudCode1+Authentication검증1+Private policy/config 조회1(같은 call에서2key)+account 조회1+economy 조회(현재 Get는 생성 확인+실제 읽기2, mutation도 CAS읽기 포함2)+필요시CAS1이다. 초기 생성은 gate 추가요청, Alpha/Production progress는 IntegrityVerify/Google 호출이 더 든다. CAS/전송 retry는 각3회 상한이며 실제 latency/DAU/과금 비용은 측정하지 않았다. JSON의 초기 requestEstimate는 개념적 분류이고 실제 정확한 호출 수는 이 코드 경로 설명을 따른다. 매 프레임 cloud/매 처치HTTP/전체 creature scan/무제한 로컬 큐/금융 원장 prune은 없다.

## 검증 분류와 남은 완료 기록

서버 Node/model, Unity Edit/Play, 실제 development Cloud Code, 실제 Unity SDK, offscreen graphical viewport, 실제2프로세스 LAN을 서로 구분해 결과 파일로 기록한다. Unity MCP는 이 세션의 도구 목록에 없어 호출하지 못했으며 hidden isolated Editor batch를 사용했다. Windows 입력/창 Focus·Show는 사용하지 않았다. viewport는 실제 Unity Canvas를 RenderTexture에 렌더한 검사이며 사용자 기기 터치/Android 성능 검사를 대신하지 않는다. production QA player/data를 만들지 않았다. Google/PGS/OAuth/Privacy/삭제 URL은 SETUP_REQUIRED/RELEASE_BLOCKERS에 남아 있고 제품 backend Enabled=false다. Foundation2 구현은 Ads/IAP를 활성화하거나 다음 패스를 시작하지 않는다.

## 2026-10-26 종료 체크포인트 · 완료 선언 보류

사용자 노트북 종료 요청으로 여기서 멈춘다. [HANDOFF](HANDOFF.md)를 다음 재개 정본으로 사용한다. 서버64/64, Edit74/74, 최종 Play 고유17개, 실제 서비스33개, 실제 Unity SDK 핵심 거래, 실제2프로세스 LAN은 통과했다. QA계정7개/Protected data/임시함수 정리 완료. 최종 viewport는 해상도 전환 뒤 Selected Achievement content 높이 검사에서 실패했으며 아직 수정·재검증이 남아 있다. 마지막 UI source와 동일한 Android compile/전체 diff 최종 감사·live Editor 동기화도 남겼다. 최신 검사 상태는 checkpoint-validation.json이다. 실제 사용자 저장2개는 hash/mtime/bytes 일치, credential pattern scan3541 text file0건이다. 이 문서가 포함된 main checkpoint commit은 중간 보존이며 Foundation2 최종 SHA가 아니다.

초기 회귀 실패의 일부는 Save11/12/13·가격0·Protocol21 기대값과 Survivor의 기존1회 lethal 방어를 고려하지 않은 QA fixture였다. 현재 Save14/고정가격/Protocol22와 명시적 lethal fixture로 정정 후 관련 검사가 통과했다. SDK actual roundtrip에서 엄격한 응답 역직렬화의 claimedDay/event hash/admin ledger metadata 누락을 찾아 정본 DTO를 수정했다. ordinary ClassXP mirror dirty가 Auth offline과 혼동되던 경계를 별도 CloudAvailable로 분리했고 offline 선택은 local write로 다시 열리지 않음을 검사했다. UI 목록 anchor/slot 줄바꿈/footer 및 상세 scroll을 수정했으나 최종 content sizing 검사는 미통과다.
