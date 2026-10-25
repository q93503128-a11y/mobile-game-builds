# Save / Load13 · v1~v12 이관

2026-10-23 현재 Save14 / Protocol21, 신규 generation3. 정본은 SaveGameRules/SaveValidation/SaveMigration/WorldSnapshotCodec/PlayerSnapshotCodec/SaveRepository다. 기존 generation1/2 지형·시설·자원 ID를 보존한다.

## 파일과 검증

SP 파일명 `world-v1.json`, Host 파일명 `multiplayer-world-v5.json`을 유지한다. Editor는 프로젝트 UserData, 빌드는 persistentDataPath다. checksum을 포함한 JSON envelope, temporary 쓰기/flush→재읽기/데이터 검증→atomic replace와 정상 backup 정책을 유지한다. 손상된 primary는 정상 backup을 시도하고, 손상된 primary 때문에 정상 backup을 덮지 않는다. Load만으로 원래 파일을 덮어쓰지 않는다.

Home 검사와 실제 Continue/Save는 같은 SaveGameRules.Validate를 쓴다. 버전/체크섬/ID/슬롯·수량·Charges/Quality/위치/HP/Recipe·queue·시간/성장/장비/참가자·Camp 참조를 적용 전에 검사한다. 로드 실패는 안내 후 현재 화면에 남고 임의의 새 월드로 대체하지 않는다. 기기 Background/Quit의 플레이 세션 자동 저장과 Pause Save·Save & return home을 제공한다. 플레이어용 Load 버튼은 없고 Home Continue와 Editor Load를 유지한다.

## 현재 저장 범위

|경계|영속 상태|
|---|---|
|Player|위치/HP·전투 회복 대기/Inventory36·선택/성장·Skill·XP/Gold/Head·Body·Utility/Stamina/Home·Beacon·Navigation/SeenRecipe|
|생존|Hunger/Thirst/생존 피해 clock/Raw Water 불이익|
|World|Seed/BaseGenerationVersion/누적 게임 시간/feature state|
|Resource|기본 source delta·tombstone, 회복 source/region deficit·ticket·harvest exclusion·serial/cursor|
|Animal|살아 있는 ID/species/rule/group/원래 지역/위치/Home/HP, pending/serial/cursor/initialized·expanded regions|
|Goblin/Camp|Camp ID/위치/거주 보충 시간, 생존 actor ID/역할/group/home/HP/위치와 휴면 상태, 보급품 최초 생성/개봉/잔량/revision|
|Night|살아 있는 Zombie/Skeleton/검 Variant/위치/HP/feature|
|Facility|stable ID/Item/Creator/위치/회전/HP·문 상태/Chest contents/queue·예약·완료 결과/SimulatedAt/Turret 탄약·모듈/Beacon 순번|
|Ground|Item/수량/Charges/Quality/위치|
|Host|WorldId/HostId/식별자별 PlayerSnapshot|

전체 생성 월드/지형 cache/AI 순간 target·pose/투사체/UI drag ghost·Sprint hold·Dash 진행/연결 code·NetworkObjectId는 저장하지 않는다. 소비품4초 입력 cooldown도 순간 상태다. HP/Stamina·소비된 stack은 보존한다. 처리·재생은 저장한 게임 시간에서 재개하고 오프라인 시간을 더하지 않는다.

## v10→v11 생물·보급품 이관

검증된 기존1~10은 순차 이관 뒤 SaveMigration.CreatureLootFromEarlier로11이 된다. 기존 Animal의 ID/rule index0~4/group/HP/위치/Home과 initialized regions를 보존하고 ExpandedRegions를 비운다. 기존 방문 청크에 새8규칙을 한 번만 초기화하며 구 개체를 대체하지 않는다. 지질 generation을 바꾸지 않으므로 generation1/2에 Diamond/Crystal source를 소급 생성하지 않는다.

기존 Camp의 ID/거주 적/보충 시간을 보존하고 보급품은 uninitialized/empty/unopened/revision0으로 시작한다. 최초 활성화 때 Seed+Camp ID+:supplies로 고정된6칸을 생성한다. 이후 개봉과 실제 잔량만 저장하며 재방문/거주 적 보충/Load/reHost가 상자를 채우지 않는다. 이미11인 initialized Camp는6칸과 Item/stack 상태를 검증한다. 빈 슬롯은 공용 SavedStack의 Count0/빈 Item/Charges0/Empty quality이며 null 슬롯을 생성하지 않는다. active·dormant Camp capture는 분리된 snapshot이다.

## 이전 이관 경계

- v1→2: 누적XP를 Level/남은XP/Point로 변환, 기존 inventory·월드 보존, 당시 없던 Armor는 비움, Stamina Full/SeenRecipe 빈 상태.
- v2→3: 구 개별 resource timer를 지역 deficit·harvest exclusion·회복 ticket으로 변환.
- v3→4: 살아 있는 동물의 ID/HP/위치 보존, species/group/region과 회복 deadline 구성.
- v4→5: SP/Host mode와 참가자·creator metadata. 식별자는 기기 로컬 Guid이며 인증 계정이 아님.
- v5→6: Reinforced Chest32→36 보존, 사라진 recipe Seen ID 정리, 구 Furnace Steel 입력은 원재료/완료 결과는 Steel로 보존. Steel은 Blast Furnace로 분리.
- v6 이하: generation1 고정. v7부터 BaseGenerationVersion/청크 logical state·시설 timestamp·동굴 feature를 사용.
- v7→8: 개인 Home ID. 빈 참조는 임의의 제작대로 추정하지 않음.
- v9: 공용 ItemStack/SavedStack/NetworkStack의 물 용기 Charges/Quality 및 콘텐츠 정리.
- v10: Beacon·Turret cargo/module·Navigation과 generation3 기반. 기존 generation1/2를 유지.

## Host와 QA

Host는 저장 성공 후 세션을 종료하고 실패하면 유지한다. reHost는 저장을 먼저 읽어 새 연결을 만들며 참가자 상태를 spawn 전에 복원한다. 멀티 HP0은 저장 경계에서 Dead로 남기며 무료 Home으로 복귀한다. Cloud/Account/Host Migration/crash 보장은 없다. 현재 Protocol21 peer끼리만 호환한다.

실제 사용자 파일은 QA에서 수정·삭제하지 않는다. Editor 전용 IsolatedQaProfile은 별도 Captures 디렉터리와 격리 identity/preferences를 쓴다. 실제 Unity SP/Host와 자동 회귀, 원격·단말 미검수 범위는 [이번 REVIEW](../validation/creature-loot-expansion/REVIEW.md)를 따른다. Android 강제 종료 파일 시스템은 별도 실기 검증이다.

## Medicine2-A · transient 치료

Save11을 유지한다. 신규 item ID는 기존 Catalog codec, Herb 고갈/지역 ticket은 기존 resource snapshot을 사용한다. 이미 소비된 붕대와 실제 적용된 HP만 저장하며, 남은8초 치료와 공용 사용 간격은 Load/reHost/사망에서 초기화한다. 저장·재접속으로 새 치료를 다시 지급하지 않는다. 별도 migration 필드가 필요 없다. Protocol14는 PlayerStatusState에 치료 종료 서버 시각을 추가하고 Primary RPC에 단조 증가 요청 번호를 추가했기 때문이며13 peer와 혼용하지 않는다. 사용자 원본 저장의 hash/mtime 비교는 [이번 REVIEW](../validation/medicine-expansion/REVIEW.md).

## Food2-B · Meal 저장/네트워크

Save11유지.Meal150초의남은효과는기존Medicine정책처럼transient로취급해Load/reHost/죽음에삭제한다.PlayerSnapshotCodec은현재HP와영구최대HP(base+Vitality)중작은값만기록한다.Hunter로HP108이되었을때liveHP는그대로두고snapshot은100(Vitality1이면110까지)이다.임시최대치를초과한invalidSave·무료재지급·무한연장을피하며새migration필드없음.

Protocol15는PlayerStatusState에MealItem/MealUntil을추가해클라이언트가정확한아이콘/시간을표시하기때문이다.기존PrimaryownerRPC/단조증가sequence와공용소비검증을재사용한다.실제원본UserData는격리QA와분리했다. [호환·Host/reHost 검수](../validation/food-expansion/REVIEW.md).


## Fishing2-D · Save11 / Protocol16

새물고기4/음식1과Recipe5는기존item/recipe ID codec을사용한다.기존raw_fishstack과음식queue는유효하며Saveversion/migration필드추가없다.낚시Casting/Waiting/Bite/Caught와PendingCatch는저장하지않는다.실제로지급된Inventory만영속이다.같은객체Load도PlayerSnapshotCodec.Apply에서낚시를초기화해이전pending을다시수령하지않는다.reHost/despawn도초기화한다.미끼가없어미끼복제경로도없다.

Protocol16은ActivityState에CatchIndex/CatchCount가추가되고owner수역/거절안내RPC가추가되기때문이다.15peer와혼용하지않는다.서버Primary의단조sequence검사·Host원자inventory권위를유지한다.검수는격리저장만사용하며 [2-D 원본hash/mtime및Save/reHost](../validation/fishing-aquatic-expansion/REVIEW.md)를따른다.

## Exploration Utility2-E · Save11 / Protocol17

신규 두 item은 기존 item ID/stack codec으로 저장하고 migration 필드가 없다. Survey channel/result/쿨다운과 signal 위치/만료/쿨다운은 transient다. PlayerSnapshotCodec.Apply와 NetworkExplorationUtility.OnNetworkDespawn에서 모두 지워 Load/reHost/reconnect에 남기지 않는다. 소비/보상을 주는 도구가 아니므로 transient 초기화로 아이템을 복제하지 않는다. 기존 persistent Beacon/Home/장비 저장은 유지한다. 새 NetworkBehaviour/복제 struct 때문에 Protocol17이고16 peer와 혼용하지 않는다. [격리 save 검증](../validation/exploration-utility-expansion/REVIEW.md).

## POI2-F · Save11 / Protocol18

Save11과 generation3를 유지한다. WorldFeatureSnapshot.Site의 선택 Schema1 state는 Legacy/Opened/Revision/Loot6칸/Water/RefillAt/Quarry Nodes를 저장한다. Site null뿐 아니라 Schema0도 과거 누락 필드로 취급한다. 기존 feature는 빈 Legacy state, Well0/480초 대기로 이관하고 기존 visited를 미방문으로 바꾸지 않는다. capture는 중첩 상태를 복제한다.

유한 보급품은 Host가 초기화/원자 transfer하고 site와 bag의 독립 revision·owner sequence·거리·생존·scouting·guard 상태를 검사한다. 일회 보급과 node 고갈은 unload/Load/reHost로 재충전하지 않는다. 우물은 오프라인 시간이 아닌 저장한 게임 시간만 쓴다. Graveyard의 feature actor는 낮/휴면/Save/reHost에 HP와 clear를 보존하며 home은 stable anchor에서 복원한다. NetworkCreature가 이 수호자의 야간 lifecycle을 다시 켜지 않는다.

PoiSaveValidation은 anchor 종류/Schema/고정 seed 보급 상한·잔량/빈 슬롯/물 범위·유한 시각/node ID·rule·HP·위치/중복 guard와 clear 일관성을 검사한다. 신규 NetworkPoiSite/NetworkList와 owner POI RPC 추가로 Protocol18이 필요하며17 peer와 혼용하지 않는다. 기존 저장 파일명·원본 파일은 유지한다. [격리 저장·원본 hash/mtime](../validation/base-defense-poi-expansion/REVIEW.md).

## v11→v12 · lifecycle/Home

기존1~11을 검증·기존 이관한 뒤 DeathSnapshot(Home vector,Run Guid,Death number,Dead,AdUsed)을 추가한다. Home은 해당 seed/generation의 WorldMap.Spawn, 구 HP0 참가자는 Dead, 광고 미사용으로 시작한다. 기존 Workbench HomeId/위치/가방/장비/성장/월드/POI는 보존한다. 새12에서 Death record 누락/HP불일치/잘못된 Home은 거절한다. 파일명/checksum/backup/atomic replace와 generation은 유지한다.

죽음 직후와 부활/Home commit은 동기 영속 저장한다. Downed/Reviving/대기 광고는 HP0+Dead로 정규화하며 재접속/reHost/Load는 Dead다. 구조자/NetworkObjectId/hold progress/bleedout deadline/보호는 저장하지 않는다. 성공 광고1회는 AdUsed로 영속, Gem은 이를 소모하지 않는다. Home만 새 Run/광고 기회를 시작한다. 유효하지 않은 죽음 좌표는 HP0에서만 유한 범위로 저장을 허용하여 Home 경로를 잃지 않게 한다.

광고 완료는 현재 death/revision과 연결해 중복·취소·늦은 callback을 버린다. Gem은 지갑의 원자 commit 계약 안에서 world 저장이 성공한 뒤에만 완료하고 실패하면 Dead/HP0/이전 원정 상태로 되돌린다. 실제 결제 공급자가 없으므로 production은 disabled다. 개발 provider는 Editor 전용 메모리 fixture이며 실제 계정 잔액/영속 결제를 검증했다고 주장하지 않는다. 손상 backup fallback/수동 파일 복원·위조/인증 없는 local GUID 교체까지 막는 서버계정 시스템은 없다.

[Save·거래·LAN·실제 Play 증거](../validation/death-revive-respawn/REVIEW.md).

## v12→v13 · Class / player-built metadata

ClassSaveValidation은 SessionClass=survivor, SurvivalCooldown=0, ClassPurchases=[]를 기본값으로 추가한다. 공용 Growth는 ID별 기존4Skill Rank/Point를 보존하고 Metabolism/FieldCare를0으로 추가한다. 구 Level20 이후 누적XP는 새 Level과 Point로 변환한다. Class 소유/XP/선택/node는 world와 분리된 checksum profile Schema1이며 실제 인증 backend가 아니다.

Placeable.PlayerBuilt를 추가한다. 구 SP 설치물은 cargo 외 플레이어 건설로, 구 MP는 유효 Creator GUID가 있는 설치물만 플레이어 건설로 이관한다. 무료 starter/자연시설은 회수 불가다. SessionClass/치명상 방지 cooldown 잔여초/구매receipt를 player snapshot에 보존한다. 보호·Guard·Quarry·표식·Overrepair·Overclock은 transient다.

Gold 구매는 profile intent 저장→world Gold debit+receipt atomic commit→profile entitlement 확정 순서다. 중단 후 world receipt가 있으면 grant, receipt가 없으면 intent만 취소해 재시도한다. read/validation 실패는 해금하지 않는다. 로컬 save 수동 rollback/인증 없는 GUID·파일 교체는 막지 못한다.

Protocol20은 NetworkClassActions/ClassActionState와 owner request, NetworkCommonGrowth, NetworkBuilding PlayerBuilt/buffer 및 임시 포탑 상태를 추가하기 때문이다.19 peer와 혼용하지 않는다.

## Class receipt compaction / admission correction · 2026-10-22

Save13/Profile1/Protocol20 유지. Profile1의 과거 Receipts 배열은 JsonUtility가 무시한다. checksum/atomic/backup 검증 뒤 소유·총XP·선택·노드·구매 intent는 보존하며 다음 정상 저장은 obsolete kill receipt를 쓰지 않는다. backup은 기존 원자 교체 정책대로 교체되므로 직전 legacy payload가 다음 저장까지 backup에 남을 수 있다. 신규 kill 기록은 영속하지 않는다. 월드의 구매 ClassPurchases는 제거하지 않는다.

중복 처치는 death commit과 세션 recent512 window로 차단한다. 원격 미인증 계정 claim을 Host 영구 profile로 승격하지 않는다. 기본 원격 Class 진행은 새 Survivor 세션이며 해당 Host의 reconnect까지만 XP/lock을 보관한다. 명시 개발 verifier의 원격 상위 직업도 ephemeral이다. Host 로컬/SP의 profile 진행은 기존 영구 store를 쓴다. [실측·이관·LAN 검증](../validation/ui-redesign/REVIEW.md).

## Boss / Accessory · Save14 / Protocol21

Save13→14는 기존 player/월드/참가자 payload를 유지하고 Accessory=null/Bosses=[]로 정상 이관한다. 이전1~12 reader도13을 거쳐14로 간다. schema만 변경하며 generation1/2/3과 파일명/checksum/atomic replace/backup을 유지한다.

BossSnapshot은 boss ID/Tomb ID/LastNight/Home/Position/Active/HP/PendingReward/Eligible/이미 추첨한 RolledLoot를 보존한다. 기존 guardian-cleared feature가 unlock이다. 알려진 boss·anchor·guard·HP·유한 좌표·night/중복 ID·고정 보상 min/max/확정 Core·중복 stack을 검증한다. 성공한 offering 순간 Bone10과 그 밤 identity를 한 저장에 확정하고 실패하면 provisional boss/가방/밤을 rollback한다.

Active load는 저장 HP의 Returning에서 재개한다. target/공격 포즈/임시 adds/mark는 transient이며 저장해 중복 생성하지 않는다. 죽는 중 load는 이미 뽑은 보상과 eligible ID를 유지해 짧은 Dying 이후 해결한다. 물리 pickups+공용XP/Gold+resolved flag는 한 world snapshot commit이며 실패하면 모두 rollback하고 재추첨 없이 재시도한다. resolved reload/reHost는 다시 지급하지 않는다. Boss 도입 당시에는 별도 로컬 Class profile XP를 world commit 뒤 전달해 두 저장소 사이 강제 종료 유실 공백이 있었다. 아래 Post-Boss Stabilization의 Profile2 복구 거래가 이 경계를 보완한다. account/cloud/crash-proof 보장을 추가하지 않았다.

Protocol21은 NetworkBoss의 HP/state/phase/attack/server timestamp/mark, NetworkBossWorld offering, POI unlock 및 player Accessory appearance를 추가한다.20 peer와 혼용하지 않는다. Host가 simulation/roll/보상·저장을 결정하며 Client는 owner RPC 요청과 replica만 수행한다.

## Post-Boss Stabilization · Profile2 / Save14 유지

Profile1→2는 checksum 검증 뒤 기존 소유·선택·ClassXP·노드·구매 Pending을 보존하고 PendingXP=null로 정상 이관한다. 로드만으로 일반 프로필을 다시 쓰지 않으며 다음 정상 Write에서2로 저장한다. 옛 영속 처치 Receipts는 계속 무시한다. Save14/Protocol21/generation3의 payload와 파일명은 바꾸지 않는다.

Boss 보상은 로컬 영속 profile에 ClassId/XP/WorldPath/seed/generation/boss/tomb/night/receipt를 가진 PendingXP 한 개를 먼저 atomic commit한다(XP는 아직 그대로). 그 다음 물리 loot·공용 XP/Gold·resolved boss를 한 world snapshot에 commit하고, profile의 XP 가산+PendingXP 제거를 한 atomic Write로 마감한다. 진행 중인 동일 프로세스의 profile Read는 미확정 거래를 회수하지 않도록 in-flight guard를 사용한다.

프로세스 재시작 시 LocalAccountProgressionStore.Read는 미완료 의도가 있을 때만 해당 world를 checksum/정본 validator로 읽는다. 같은 seed/generation/boss/tomb/night의 HP0·inactive·보상 해결 상태이면 원래 Class에 XP를 한 번 더하고 marker를 함께 제거한다. world가 아직 미확정이면 XP 없이 marker를 제거한다. world를 읽을 수 없거나 profile Write에 실패하면 marker를 남겨 다음 읽기/정상 save에서 재시도한다. 새 밤 소환은 미해결 로컬 profile 보상을 복구하기 전 기존 resolved receipt를 덮지 않는다. 정상 상태에서 매 프레임 파일 검사나 영속 처치 이력 누적을 하지 않는다.

Background/Quit/Save & home/Host Save는 기존 world Save 경계를 유지하고, 미완료 로컬 ClassXP가 있으면 복구도 시도한다. 원격 development verifier의 진행은 session-only이며 Host가 원격 profile을 영속 승격하지 않는다. A~E 단계 중단/쓰기 실패는 controlled fault test이며 실제 OS crash/Android 파일 시스템 내구 시험은 아니다. 독립 파일의 원자성·미디어 손상·과거 world backup의 수동 rollback까지 account 수준의 exactly-once를 주장하지 않는다.
