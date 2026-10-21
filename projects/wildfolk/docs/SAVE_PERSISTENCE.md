# Save / Load11 · v1~v10 이관

2026-10-21 현재 Save11 / Protocol18, 신규 generation3. 정본은 SaveGameRules/SaveValidation/SaveMigration/WorldSnapshotCodec/PlayerSnapshotCodec/SaveRepository다. 기존 generation1/2 지형·시설·자원 ID를 보존한다.

## 파일과 검증

SP 파일명 `world-v1.json`, Host 파일명 `multiplayer-world-v5.json`을 유지한다. Editor는 프로젝트 UserData, 빌드는 persistentDataPath다. checksum을 포함한 JSON envelope, temporary 쓰기/flush→재읽기/데이터 검증→atomic replace와 정상 backup 정책을 유지한다. 손상된 primary는 정상 backup을 시도하고, 손상된 primary 때문에 정상 backup을 덮지 않는다. Load만으로 원래 파일을 덮어쓰지 않는다.

Home 검사와 실제 Continue/Save는 같은 SaveGameRules.Validate를 쓴다. 버전/체크섬/ID/슬롯·수량·Charges/Quality/위치/HP/Recipe·queue·시간/성장/장비/참가자·Camp 참조를 적용 전에 검사한다. 로드 실패는 안내 후 현재 화면에 남고 임의의 새 월드로 대체하지 않는다. 기기 Background/Quit의 살아 있는 세션 자동 저장과 Pause Save·Save & return home을 제공한다. 플레이어용 Load 버튼은 없고 Home Continue와 Editor Load를 유지한다.

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

Host는 저장 성공 후 세션을 종료하고 실패하면 유지한다. reHost는 저장을 먼저 읽어 새 연결을 만들며 참가자 상태를 spawn 전에 복원한다. 멀티 HP0은 Defeated를 유지한다. Cloud/Account/Host Migration/crash 보장은 없다. 현재 Protocol18 peer끼리만 호환한다.

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
