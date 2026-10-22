# Character Progression + Class Foundation

## 기준과 진행 상태
시작 main cbe3c14115ee5529eed7babd062df801e7981b99. 2026-10-22 remote fetch 후 local/remote 일치·clean 확인. 아래는 구현 전 감사/결정이다. 검증 결과는 수행 후 별도 기록한다. 재미·손맛·주관적 밸런스를 판정하지 않는다.

## 공개 자료 비교
2026-10-22 직접 조회. 버전이 다른 수치를 합치지 않는다. 일부 wiki 원문은403/402로 차단되어 검색에 노출된 구체적 설명과 개발사 자료를 대조했다. 외부 asset/code/layout을 복사하지 않는다.

|게임|시스템|확인한 실제 사례/자료|장점|Lowfire와 차이|적용|이유|
|---|---|---|---|---|---|---|
|DST|캐릭터별 선택 성장|[배급사 안내](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/827-how-to-unlock-skill-points-insight-points/): 68일 생존으로15 Insight; [Wilson 공개 refresh](https://kleiforums.com/forums/topic/146186-wilson-refresh-in-beta/page/2/): Beard/Torch/Alchemy 세 계열|캐릭터 행동을 강화하며 제한된 선택|월드 Skill과 별도인 Class XP 필요|9 node/현재6 milestone,클래스별 profile|모든 node 동시 획득을 피하고 데이터로 확장|
|Grounded|Mutation/loadout|[개발사1.2](https://grounded.obsidian.net/news/grounded/teaser-update-1-2):4 loadout,기본2 active→최대5; [개발사 패치](https://www.obsidian.net/news/grounded/september-content-update-notes):Reliable Friend revive speed 강화|도구/전투/구조를 선택|Lowfire는 행동 중 Class 교체 금지|구조 specialization은 선택,기본4초 구조는 모두 가능|필수 healer와 전투 중 loadout swap은 채택 안 함|
|DRG|Engineer/team identity|[공식 연결 wiki Engineer](https://deeprockgalactic.wiki.gg/wiki/Engineer):Platform Gun+Sentry; [LMG](https://deeprockgalactic.wiki.gg/wiki/Sentry):탄약을 가진 고정 지원 화기|이동/준비/방어 역할로 구분|기존 Lowfire turret는 모든 Class 소유 공용 건물|Engineer 수리·buffer·reload/rotation 지원|프리미엄 raw damage/포탑 독점/무료 탄약 배제|
|Core Keeper|Skill XP/talents|[Skills](https://corekeeper.atma.gg/en/Skills):5 level마다 talent point; Endurance runner 최대25% food drain감소,Healing potency 최대30%,Blacksmith repair재료5~25%절감|작은 생존 효율과 독립 행동 성장|Lowfire 전투 kill XP만 기존 제공|Metabolism25%,Field Care30%,기존XP1:ClassXP1|별도 반복 행동 XP farming/새 재화 배제|
|Wild Rift|Landscape mobile 선택 UI|[Riot Champ Select](https://support.riotgames.com/en-us/wild-rift/gameplay/champ-select/):선택 중 역할/능력 미리보기,hover와lock 구분,loadout 별도|구매 전 역할 판단과 최종 선택 상태 분리|6종의 작은 PvE roster|카드 목록+역할/능력 상세+명시적 선택 CTA|초기 roster에 검색/팀draft/큰radial은 불필요|
|Brawl Stars|Landscape mobile 선택/잠금|[Supercell Battle Cards](https://support.supercell.com/brawl-stars/en/articles/battle-cards-6.html):Brawler 상세 화면의Select버튼 위 보조정보; [선택 즐겨찾기 패치](https://supercell.com/en/games/brawlstars/blog/release-notes/release-notes-ranked-rework-lumi-finx-2/):선택 목록 정렬; [공식 재화 안내](https://support.supercell.com/brawl-stars/en/articles/currencies-2.html):구매/업그레이드 용도 구분|선택/보조정보/가격 구분|Starr Road 관련 옛 도움말과2026 replacement 혼재|Owned/Locked/Selected를 문구+색,구매 확인단계 분리|랜덤해금/새Credit 재화/오래된구매경제는 채택 안 함|
|Diablo Immortal|Mobile Class 선택/정보|[Blizzard 변신 안내](https://news.blizzard.com/en-us/article/23785115/transforming-yourself-in-diablo-immortal):Class identity와외형분리; [Class Change](https://news.blizzard.com/en-gb/article/23826547/class-change-and-new-features-debut-in-diablo-immortal):Lv35/7일변경,첫변경tree reset,장비보존|역할선택과기존성장보존|Lowfire는Lobby에서만선택하며무료respec|6카드+간단한능력+독립tree영역|전투중도시Class변경/장비복사/주기과금은 배제|

UI 조사는 세 게임의 공개 선택 흐름·정보 구조에 근거한다. 실제 기기 UI를 실행해 본 기록이나 외부 화면의 픽셀 단위 복제라는 주장은 하지 않는다. Lowfire1920×1080 실제 화면은 구현 후 자체 검수한다.

## 현재 source 감사
- CharacterGrowthDefinition:RequiredXP=35+(L-1)*20,MaximumLevel20;PlayerProgression int Level/Point,long total/current XP;고정 배열 runtime이나save는ID→Rank다.
- Vitality/Endurance root,Recovery/Stride는Endurance1.4종모두Rank3.기존rank>0이면선행조건이없어도추가투자허용. Vitality+10HP,Endurance+15Stamina,Recovery+.15,Stride sprint cost=1/(1+.12*rank). HP최대만증가.
- CharacterStats는영구성장/Meal/Armor 집계. HP100,Stamina100,회복16/s·delay1.2,sprint14/s,dash24. Meal150초단일. Medicine category 이미존재:붕대20/8s,연고30즉시;공용4s. Self만구현,application cast없음.
- Armor flat감산최소1. 기존ApplyDamage에source위치가없어정면Guard판정용metadata가필요. Melee ToolKind.Spear와RangedWeaponDefinition으로generic분류가능.
- Save12/Protocol19. profile/localGuid는인증계정아님. Gold는world character잔액이며기존소비처없음. IRewardedReviveService/IPremiumCurrencyWallet의production은Unavailable.
- 죽음/구조는PlayerDeath가소유하고IReviveModifiers확장경계가있음. Home초기Spawn과compass WorkbenchHome은별개. 새클래스코드가Deathstate를별도로소유하지않는다.
- NetworkSession admission은Guid,Host simulation20Hz,NetworkPlayer/Inventory owner요청검증. class 선택/entitlement/progression없음. reconnect는Host player snapshot사용.
- Telescope는2.5*64=160unit scout point·청크presentation만요청. Survey18/1.2s/6s/12s,Signal60/20s/30s;boundedquery. Marker없음.
- Placeable Health/SaveId/Creator/queue/cargo/Turret snapshot존재.범용Repair/Dismantle없음. BoneTrap단발소모,Spike100HP지속형. Ballista62/2.3s/45deg/s/180HP,Repeater16/.55s/105deg/s/160HP;탄약/5module독립.

## 경제와 후보값 근거
Loot asset 실제 XP/Gold:Zombie5/1,Skeleton7/2,SwordSkeleton8/2,Goblin8/3,Slingshot8/3. Bat3/0,Leech5/0,StoneCrawler7/0,Wolf8/0,CrystalCrawler12/0,GiantBoar15/0. 온순동물0/0. POI/탐험/레벨업직접Gold0,별도Elite보너스0.

시나리오는실제수율에가정한kill cadence를곱한산술이며실측플레이수입이아니다. 중간시나리오/분:Zombie.5,Skeleton.25,Goblin.25,Gold0위험생물.5(XP평균8)→Gold1.75/분,XP10.25/분. 느린/빠른은절반/두배.
|시간|느림 Gold / XP|중간 Gold / XP|빠름 Gold / XP|
|---|---|---|---|
|30분|26.25 /153.75|52.5 /307.5|105 /615|
|60분|52.5 /307.5|105 /615|210 /1230|
|120분|105 /615|210 /1230|420 /2460|
4개Gold Class는각60Gold Alpha. 예상17.1~68.6분(중간34.3분). 새world시작Gold0/기본item재판매없음/시작보급Gold0이라새world만들기자체의수입0. 과거save수동rollback/로컬profile조작은인증backend없는Alpha의보안한계다.
Class milestone누적XP:50,180,400,750,1200,1800. 중간첫4.88분/여섯175.61분,빠른여섯87.8분/느린351.2분. current6entry→6point이며code상한6아님. Engineer는PremiumGem종류만확정/가격미정/production구매불가.

## 구현 결정 (외부 사례 → 현재 문제 → 원칙 → 배제 대안)
1. DST/Core Keeper → worldgrowth와Class혼합위험 → 일반Point는world,ClassXP/ownership/allocation은durableprofile →새MetaCoin/일반Point전용소비배제.
2. CoreKeeper/DST → 기존Levelcap/옛save누적XP → 선형식무제한·closed-form구간합/binarysearch·IDrank보존 →exponential/반복stat sink배제.
3. Grounded/DRG → 지원Class없어도solo/co-op가능해야함 →공용구조/수리/철거+genericMedicine·선택특화 →필수Medic/Engineer전용건물배제.
4. DRG/CoreKeeper → damage증가만으로정체성부족 →Guard/Quarry/치료/도구/정비실제행동경계 →전역ClassID분기/새Shield배제.
5. WildRift/Brawl/Diablo →mobile정보밀도/구매오조작 →Lobby CLASS에서6카드·분리상세/tree·가격확인·선택상태 →게임중Class교체/무의미active버튼배제.
6. DST/Diablo →해금과월드Gold수명불일치 →기존savedworldGold를차감하는idempotentreceipt+localprofileentitlement →가짜계정잔액/가짜Premium성공배제.
7. Grounded/CoreKeeper →치료받는보너스/주는보너스충돌 →Medicine outgoing*received pipeline,instant그대로/HoT총량보존 →itemID/새강제cast배제. 현재두Medicine의application time은0으로유지한다.A2는non-zero application data에만작용하고A3는HoTdelivery만담당.새비영점의약품을추가하지않으므로현재A2효과는generic QA fixture로검증한다.
8. DRG →turretmodule와중복 →Overclock12s/30s,reload+25%/rotation+35%,ammo그대로 →damage/range/AutoAim무료배제.
9. CoreKeeper/DRG →잘못배치회수와생산복제위험 →canonicalconstruction비용과upgrade누적을각1번만반환,HP90%이상100%/그미만HP비율floor,contents차단 →새loot/무료charge/임의high-tiercost수리배제. batch제작의분수비용은무손실portableitem회수로처리.
10. Grounded/DRG →큰탐사범위가simulation/wallhack위험 →Optic/Survey/Signalcategory,고정simulation·관측점범위/LOS검증·60s/team최대4임시marker →미생성청크scan/영구save/정확광물좌표배제.
11. SDK미연결명확 →adclose/storevisit보상위험 →provider의공식RewardEarned완료만success;fail-closed/QA분리 →앱timer/close/skip기반보상배제.

수치·테스트·실제채택결과는이어지는구현/검증절에서확정한다. 외부사례의수치를Lowfire최종밸런스로주장하지않는다.

## 2026-10-22 중간 검증 상태

사용자 요청으로 현재 지점을 보존하고 중단했다. 구현/검증 완료 판정 전이다. [재개 지점·실제 통과/미실행 검사](HANDOFF.md)를 따른다. EditMode20과 PlayMode고유9개 관련 실행 합집합이 통과했고, 실제LAN은R1과R2대부분이 통과했다. reHost의client복원/R3/4actor simulation/최종 성능·화면 검증과관련회귀는 남았다. 마지막QA파일공유race는수정·컴파일완료/재실행전이다. SHA는시작cbe3c14115ee5529eed7babd062df801e7981b99그대로이며미커밋변경을보존했다.

## 최종 구현 / 검증 결과 (2026-10-22 재개 후)

중간 중단 기록은 당시 상태다. 재개 시 remote fetch 후에도 main은 시작 SHA와 같았고 미커밋 구현을 보존했다. 아래 결과가 현재 검증 상태다. 다음 Boss/Undead King 작업은 시작하지 않았다.

### 구현과 수치 정본

- 공용 Level 상한 없음, XP35+(L−1)×20 유지, +1 일반Point/Level. decimal 구간합/이진 탐색으로 큰XP를 처리하고 정수 저장 범위를 검사한다. 여섯 공용Skill 최대Rank10/3/3/3/5/3, 새2종rank0·기존ID/rank/point 보존. 공용tree root는 Class Survivor와 혼동하지 않게 '공용 성장'이다.
- Class 소유/선택/XP/노드는 profile Schema1, 월드 XP/Gold/Rank와 분리했다. Survivor무료, Warrior/Hunter/Medic/Explorer 각60 기존 Gold, Engineer PremiumGem 가격/provider 미결정으로 production 해금불가. QA에서는 명시 주입한 지갑/승인 fixture가격7만 검증했다.
- Class XP는 실제 gameplay XP1당1. class별 milestone50/180/400/750/1200/1800→현재6Point. node54개의 이름/branch/효과/실제값은 [성장 정본](../../docs/CHARACTER_PROGRESSION.md). 한 class는9node 중6개 배분, 선행1→2→3, Lobby 무료respec, admission/world 진입 뒤 lock이다.
- Save13/Profile1/Protocol20, Item134/Recipe111/generation3. 새 saved 필드는 SessionClass/SurvivalCooldown/ClassPurchases/PlayerBuilt다. 메인/backup과 기존 save migration/생물·recipe 경제를 보존한다.
- Medic A2는 non-zero application time만 줄인다. 기존 붕대/연고는 application0을 유지해 현재 두 아이템에 효과가 없고 generic timed fixture로 검증했다. A3는 HoT 총량 보존·기간25% 단축이다.
- 일반 full repair는 공통 재료의 약25%, 부족HP비율ceil·최소1개. Wood Wall처럼 원가2개면1개가50%여서 작은 정수 비용은25%를 넘는다. 신규 재화/희귀부품 수리비를 만들지 않았다. [전 설치물 실제 비용](building-economy.tsv).
- Engineer 무료HP수리와 별도buffer(base20%, A2+10%p, 시설특화+10%p), transient Overclock12초/30초·장전+25%/회전+35%(B:16초/24초·+35%/+45%)다. damage/range/ammo/modules를 지급하지 않는다.
- 회수90%이상 원가100%, 미만 floor(원가×HP비율), WBIII는 base+upgrade 각1회. batch 제작은90%이상 portable1개/미만0개. content/queue/output/ammo/Loaded/module은 먼저 비워야 하며 full bag이면 원자 거절한다. BoneTrap의 소비를 수리로 되돌리지 않는다.

### 자동 검사와 실제 Play — 구분한 결과

|검사|결과|증거와 범위|
|---|---|---|
|Class EditMode|20/20, 관련 회귀53에 포함|models-corrected.xml 및 final-related.xml|
|관련 EditMode 고유53개|최종 모두 통과|final-related.xml의52 + final-poi-protocol.xml의1. 남은 실패는 테스트의旧Protocol19 예상값→20 갱신 후 해당1개만 재검. 한 번에53을 통과했다고 쓰지 않음|
|Class PlayMode 고유9개|관련 실행 합집합 통과|flows-service-corrected.xml의5 + flows-final-corrections.xml의4. flows-transactions-expiry.xml의2는 중복 포함|
|실제 MCP visible sequence|8개 flow 완료|mcp-live.tsv의09:51~09:52 SharedGrowth/Survivor/Warrior/Hunter/Medicine/Buildings/TurretsAndExploration/Lobby. root문구변경 후SharedGrowth추가실행|
|1920×1080 실제 화면|직접 PNG 열어 확인|01-shared-tree,04-hunter-quarry,06-engineer-repair,08-explorer-optic-marker,11-lobby-owned-selected-tree,13-overclock-operation. 한국어54상세 높이검사,1001표시,잠금/보유/선택/6node/9node 확인|
|실제 UI 입력/가방 용량|통과|ui-input-capacity.txt/.cs.txt: Guard button top raycast, pointerDown→Guard/pointerUp→release, full bag 회수 거절·구조/가방 보존|
|2 process LAN|전체 완료|아래 최종 run의 host/client TSV 및 complete.txt. 실제 NGO owner RPC 사용. file은 단계/ACK 조정용|
|4 actor simulation|완료|같은run에 server fixture3명+Host, 네 다른class 유지/Medic치료·구조/Engineer수리/세션lock. 실제4클라이언트라고 주장하지 않음|
|Source/data validator|통과|Python1406 GUID/참조/visible text/Landscape, MCP ProjectDataValidator0, 최종Console error0|
|기존 사용자 save|전후 hash/mtime 동일|user-save-before.json/user-save-final.json. 실제저장1개를 건드리지 않았고 QA는 Captures 격리|

초기 flows-compile-corrected 성공은 Phase 조기종료 race 때문에 무효다. 중간 실패 XML/TSV는 삭제하거나 성공으로 바꾸지 않았다.

### 실제 LAN에서 확인한 동작

- Warrior Host/Medic Client: 서로 다른 class/Guard 복제, self20→54·ally20→65(critical15%×FieldCare30%)·아이템한개, Downed의약품거절, GoldenTime한번/유효구조중timer감속/취소재시도중복없음,3.2초구조50%HP·3초보호.
- Explorer Host/Engineer Client:관측점표식복제,무료수리,buffer30→12피해흡수후18·HP100,두포탑1.35/1.45속도복제·탄약0/module없음,Wood2회수,저장,reconnect에서다른Survivorclaim에도Engineer유지,reHost에서Explorer/Engineer복원·임시표식미저장.
- Hunter Host/Survivor Client: 실제 network GiantBoar 적중Quarry복제, client 일반XP35→Level2·공통투자ownerRPC·추가투자거절·Character/6node UI,첫치명상HP10/cooldown270·공격시보호취소·두번째치명상Downed·저장정규화Dead·Home에도cooldown보존.
- clock regression 수정: admission lock/snapshot restore는 Spawn 전에 실행되므로 이때도 활성 session ServerTime을 사용한다. 이전의Time.time→ServerTime전환이 만들던 가짜cooldown을 제거했고 최종R1에서Host/Client cooldown0을 확인했다.
- 그 외 발견/교정: network prefab component등록, hold release완료latch, 파괴된target의Unity null검사, scene-unload StaminaHud null검사, pool재사용Quarry generation검사, transient포탑복제. 최종LAN은이교정들을포함한다.

### 성능 / 검증 한계

4 actor Editor sample3.004662초/277frame, 평균10.84715ms. managed memory delta−80,293,888byte는 GC회수를 포함한 사용량 차이이며 할당량/zero GC/새시스템단독비용이 아니다. 짧은 격리 Editor sample로 장시간·Android performance를 입증하지 않는다. ClassHud context는5Hz fixed64 overlap, player목록최대4이며 매frame전체creature/placeable/POI scan을 추가하지 않았다. landmark읽기는표식생성때현재관측화면의기존표시만확인한다.

실제 Android, Relay 원격망, 실제4인, 광고SDK/스토어IAP/계정backend는 미검증·미연결이다. 광고true는 SDK의공식reward-earned completion만 의미하며Started/Closed/Skipped/StoreOpened/timer는보상아님. production ad/wallet은unavailable이다. local profile claim/checksum은 인증·치팅방지가 아니며 수동save복원/로컬GUID교체를 막는 서버계정이 없다.

객관적 Foundation 판정 **A**: 요청한 행동/계산/저장/권위 경계를 검증했다. 실제 재미·손맛·장기경제·체감밸런스는 사용자가 플레이한 뒤 판정한다.

최종 LAN run: [`654feae46b104f1a9916c7ed040a5948`](actual-lan/654feae46b104f1a9916c7ed040a5948/host.tsv), [Client](actual-lan/654feae46b104f1a9916c7ed040a5948/client.tsv), [완료](actual-lan/654feae46b104f1a9916c7ed040a5948/complete.txt).

## 후속 교정 안내 · 2026-10-22

이 문서는 Foundation 완료 당시의 조사/검수 기록이다. 이후 remote admission 신뢰, 영구 XP receipts, Medic A2 무효 효과와 UI 문제를 별도 패스에서 교정했다. 당시 기록을 현재 보안/시각 품질의 근거로 사용하지 않는다. 최신 동작·비교·검수는 [Class correction / Camp Journal](../ui-redesign/REVIEW.md)을 따른다. 중간 HANDOFF/실패 실행/중복 XML 정리는 최신 REVIEW의 제거 목록을 따른다.
