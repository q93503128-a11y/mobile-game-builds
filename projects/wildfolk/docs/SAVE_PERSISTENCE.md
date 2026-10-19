# Save / Load v9, v1~v8 호환

2026-10-18 현재 자원 템포: **Save9 / Protocol11, Item86 / Recipe71**. [현재 정본](CONTENT_EXPANSION.md). 아래 이전 버전 설명은 해당 패스의 기록이다.


2026-10-18 현재: generation2 지형 유지 / **Save8 / Protocol9**, Item50 / Recipe38. Torch·Compass·제작대 Home·동굴 Exit·제한 지형 cache/분할 생성을 제공한다. [탐험 정본](EXPLORATION_FOUNDATION.md)과 [실제 검수/성능](EXPLORATION_REVIEW.md)를 따른다. 아래 이전 날짜/Version 설명은 각 단계의 기록이다.

2026-10-17 현재: 신규 generation2/Save7/Protocol8. BaseGenerationVersion과 청크 logical state·시설 timestamp·동굴 state를 저장한다. v1~v6 저장은 generation1 지형/시설 위치를 유지한다. 아래 v5/v6/Protocol7 내용은 해당 단계의 구조/이관 기록이며 현재 월드 정책은 [WORLD_GENERATION](WORLD_GENERATION.md)을 따른다.
정본은 WorldSaveService / SavedGame / SaveRepository / SaveValidation이다. 수동 Save game과 Save & return home은 Pause 메뉴에 있다. 플레이어용 Load game 버튼은 제거했고 WorldSaveService.Load 및 Editor Developer Load는 유지한다. 기기 Pause/Background/Quit에는 살아 있는 세션을 자동 저장한다. Home의 Single Player는 무작위 Seed로 시작하며, 저장 게임은 Home Continue로 불러온다. Gameplay 씬 직접 실행의 기본 Seed는 유지한다. 사망/재시작 규칙과 사망 보상 정책은 변경하지 않았다.

## 저장 범위

- Player: 위치, HP, Hunger/Thirst, 생존 피해 타이머, Raw Water 불이익, 전투 후 회복 대기, Inventory 슬롯/Item ID/수량, 선택 슬롯, 누적 XP/Gold, Level/CurrentXP/Skill Points/Skill Rank, Head/Body Armor, Stamina 현재량/회복 대기, 확인한 Recipe IDs.
- World: Seed, 누적 게임 시간. Day/Phase는 같은 DayNightDefinition으로 복원한다.
- Resource: 기본 r<index> 손상/파괴 Delta, 살아 있는 p<serial> 회복 개체. Region 대기/Harvest 제외/Serial/Candidate Cursor를 별도 Recovery Snapshot으로 저장한다.
- Animal: 살아 있는 종/ID/Group/Region/Position/HP/Home Anchor와 지역 부족 보충/Serial/Group Serial/Cursor.
- Facility: 고유 Save ID, Item ID, 위치/회전/HP, Door 열림, Chest24칸, Campfire Batch와 Furnace 현재 Recipe/남은 시간/예약 대기 순서/완료 결과.
- WorldPickup: Item ID/수량/위치. 로드 후 즉시 자동 획득하지 않도록 기존 지연 흐름을 사용한다.

Seed 기반 지형·물·길·전체 자원 생성 결과를 통째로 저장하지 않는다. 로드할 때 Base World를 만든 뒤 Delta와 시설을 적용한다. 소분 중인 UI의 들고 있는 표시와 적 AI 순간 상태는 저장하지 않는다. 소분은 배치 전까지 원본 모델을 바꾸지 않으므로 중단해도 아이템은 남는다. Goblin Camp/거주·로밍 Group/Ranged 역할과 살아 있는 Zombie/Skeleton/검 Variant를 복원한다. 순간 Aggro/Attack Pose/투사체는 복원하지 않는다.

## 파일 안정성

Version5 + JSON Payload + SHA256 검증값을 사용한다. Temporary 쓰기/Flush → 파일 재읽기 및 데이터 검증 → atomic replace 순서이며 이전 정상 파일은 backup으로 보존한다. 최초 저장은 같은 디렉터리의 rename이다. 손상된 primary를 덮어쓸 때 정상 backup을 덮지 않는다. 불완전한 temporary는 로드 대상으로 사용하지 않는다.

버전/체크섬/Item ID/수량/슬롯 수/위치/HP/Resource ID/시설 ID/Recipe/처리 수량/시간을 실제 적용 전에 검증한다. Primary가 손상되면 유효한 backup을 시도한다. v1~v4를 지원하며 미래 버전은 명시적인 Migration을 추가해야 한다.

Editor는 각 프로젝트의 UserData/world-v1.json, 빌드는 Application.persistentDataPath에 저장한다. 테스트 복사본과 열린 프로젝트의 Editor 저장 위치는 분리된다. Developer Harness의 변경은 자동 저장을 끄며 명시적인 Save만 테스트 상태를 보존한다.

현재 오프라인 경과 시간을 재생/제련에 더하지 않는다. 저장한 게임 시간부터 계속한다. Host 권위 멀티 저장은 아래 v5 경계를 사용한다. Cloud Save/Account Save는 없다. 데이터·Seed 생성 알고리즘의 호환성이 변경될 때 Version/Migration을 함께 변경해야 한다. Android의 실제 강제 종료/파일 시스템 동작은 실기 검증 대상이다.

## v1→v2 Migration

체크섬 검증 후 Seed/시간/Inventory/자원/동물/시설/처리/Pickup/HP/생존/Gold를 유지한다. 누적 XP를 CharacterGrowth 곡선으로 Level/남은 XP로 나누고 해당 Level의 미사용 Point를 지급한다. v1에는 실제 Armor가 없었으므로 두 슬롯을 비우며 Stamina Full/회복 대기0/SeenRecipe 빈 상태로 시작한다. 로드만으로 파일을 덮어쓰지 않고 v1→v2→v3→v4→v5로 이관하고 다음 정상 Save가 v5를 atomic write한다.

기존 접근을 보존하려고 파일 경로 world-v1.json을 유지한다. 현재 콘텐츠 Version은5다. Workbench II는 실제 Item/Placement ID로 저장하고 시설 ID/Cargo 정책을 유지한다. XP총합/Point 사용합/Skill ID·Rank/Armor Slot·ID/Stamina 범위/Recipe IDs를 적용 전에 검사한다. Dash 진행/Sprint Hold/UI 선택은 저장하지 않고 보행 상태로 재개한다. Skill Modifier를 HP보다 먼저 복원한다.

Pause Save game/Load game과 Background/Quit AutoSave를 유지한다. Home Continue/Single Player/Settings와 Pause Save & return home을 제공한다. 다중 World Select는 없다.


## v2→v3 Migration

기존 Seed/Inventory/Armor/XP·Point·Rank/시간/시설/동물/Pickup을 유지한다. 기존 Resource 파괴 ID는 Tombstone으로 남기고 현재 Region 부족량/새 Delay 범위의 대기/최근 Harvest 제외로 변환한다. 회복한 기존 r ID의 실제 위치도 보존한다. Runtime 처리기를 복원할 때 과거 Furnace Batch의 예약 수량과 전체 진행 비율을 완료 결과/현재 한 단위/나머지 대기로 나눈다. 현재2초 단위와 과거6초 정의를 데이터로 구분하며 재료를 재소비하지 않는다. 적용 후 Capture는 Sequential Snapshot을 기록한다. v2에서 선행 없이 배운 Skill Rank를 삭제하거나 Point를 회수하지 않는다.

Language는 Control Size/Master/BGM/SFX와 같은 Local User Preference다. 월드에 저장하지 않는다. 새 World나 Load가 언어를 바꾸지 않는다. 미래 버전은 거절하고 v3 Recovery/Queue의 ID·범위·수량·시간·중복 결과도 적용 전에 검증한다.


## v3→v4 Migration

기존 Seed/게임 시간/지형/자원 Delta/Recovery/Player Inventory·Growth·Armor/시설·Chest·처리·DoorOpen/Pickup을 유지한다. 살아 있는 기존 a0/a1 동물의 ID/Position/HP/Home을 보존하고 종/Group/Region을 보충한다. Missing Slot은 남은 시간을 새 종 Delay 안으로 제한해 지역 보충 대기로 바꾼다. 신규 종 부족량은 Population Restore가 예약한다. 기존 Save의 개별 Home Slot 부활은 사용하지 않는다.

v3에는 Camp와 저장된 적 개체가 없었으므로 기존 시설을 적용한 다음 작은 Camp를 안전 후보로 추가하고 Night 개체 목록은 비운다. v4부터 Camp 위치/RefillAt와 거주·로밍 Goblin ID/Group/Camp/Ranged/HP/Position/Home, Night 종/검 Variant/HP/Position을 저장한다. Load는 순간 전투 상태를 초기화하고 살아 있는 Night 개체를 복원한다. Armor Drag 중 Ghost는 정본이 아니며 성공한 Equip/Unequip 결과만 저장한다. 언어는 기존 Local Preference다.

동물 Hard Cap/Species/Group/Region/대기 중복,적 종/Variant/HP/상한/좌표/Camp 참조를 적용 전에 검사한다. 기존 world-v1.json 경로·체크섬·atomic replace·backup은 유지한다. 이 v4 단계에는 오프라인 재생/멀티플레이/클라우드 저장이 없었다. 현재 Host 저장은 아래 v5를 따른다.

Home의 저장 검사와 게임의 Save/Load는 SaveGameRules.Validate를 공유한다. 기존 검증/이관을 이동했으며 Save Version/파일명/체크섬/atomic replace/backup 정책은 바꾸지 않았다. Home 진입 및 Continue 클릭에서 Repository.Read로 유효성/backup을 확인한다. 파일이 없거나 primary/backup 모두 유효하지 않으면 Continue를 비활성화한다. Single Player는 primary 또는 backup 파일이 있으면 확인을 받으며, 시작 버튼만으로 삭제하지 않고 다음 정상 저장이 기존 Repository 규칙으로 교체한다. 저장 후 Home 복귀는 저장이 실패하면 Pause에 남는다.


## v4→v5 / Host persistence

기존 v1→v2→v3→v4 이관·World/Player payload를 유지하며 검증한 v4를 v5로 전진한다. IsMultiplayer가 SP와 Host를 명시적으로 구분한다. optional Multiplayer(WorldId/HostId/식별자별 PlayerSnapshot)와 Placeable.Creator를 추가했다. nested null 객체의 JSON 처리에 mode 판정을 맡기지 않는다.

SP는 world-v1.json, Host는 같은 저장 디렉터리의 multiplayer-world-v5.json이다. 같은 SaveRepository/checksum/temporary flush/재검증/atomic replace/정상 backup을 사용하고 사용자 SP 파일을 멀티로 덮어쓰지 않는다. WorldSnapshotCodec/PlayerSnapshotCodec는 SP와 Host가 공유한다. 저장된 seed·game time부터 자원/동물/Camp/Night/시설·Door/Chest/Workbench tier/가공 cargo/ground/Player 상태를 복원하며 offline 시간을 더하지 않는다.

Host는 승인된 snapshot 저장 성공 후 NGO Session을 종료한다. 저장 실패는 자연스러운 안내와 함께 세션을 유지한다. load primary/backup 실패는 로비에 남고 새 월드를 대체 생성하지 않는다. 재Host는 저장을 먼저 확인하고 새 Relay/Local connection을 만든다. Join Code/Relay allocation/NetworkObject ID는 영구 Save가 아니다.

기기 생성 Guid는 재입장 연결용이며 인증 계정이 아니다. Host가 참가자 상태를 기억하고 정상 reJoin에서 spawn 전 복원한다. 멀티 HP0은 Defeated를 유지하고 SP HP>0 규칙은 유지한다. 자동 retry/Host Migration/crash 보장은 없다. 검사·정확한 저장 범위·최종 통합 QA 한계는 MULTIPLAYER_FOUNDATION과 building-persistence-foundation 증거를 따른다.

## v5→v6 / 금속 교정

MetalSaveMigration은 기존 Reinforced Chest32를 내용물 위치 그대로36칸으로 확장하고 사라진 대체 Recipe Seen ID를 제거한다. 기존 일반 Furnace의 Steel 현재/대기 cargo는 Iron+Coal 재료 Pickup으로, 완료 결과는 Steel Pickup으로 보존한다. 새 Blast Furnace queue는 시설 Item/Recipe ID와 기존 codec으로 저장한다. v6은 기본 Furnace의 Steel cargo를 거절한다. SP/Host 파일명(world-v1.json/multiplayer-world-v5.json), 체크섬·atomic backup 정책은 유지한다. v1~v5 이관과 실제 SP Load/Local Host 재Host 증거는 METAL_PROGRESSION을 따른다.

## v7→v8 / 탐험 기반

현재 schema8, v1~v7 migration 지원. HomeId를 개인 PlayerSnapshot에 추가했다. v7 이전에는 지정 기지가 없으므로 빈 참조로 시작하며 스폰/첫 제작대로 임의 추정하지 않는다. v6 이하 generation1 보존, v7 generation2의 terrain/resource identity는 그대로다. Torch는 기존 PlaceableSnapshot stable ID와 Item/HP/위치/회전/Creator로 저장하고 NetworkObjectId는 저장 ID가 아니다.

Home 좌표는 Save의 별도 중복 truth가 아니라 살아 있는 또는 dormant 제작대 ID에서 해결한다. 없어진 참조는 저장을 파괴하지 않고 HUD가 기지 지정 안내로 처리한다. Workbench upgrade가 ID를 보존하므로 Home도 이어진다. 지형 cache/job/네트워크 파생 좌표는 저장하지 않는다. [검수/실제 파일 크기](EXPLORATION_REVIEW.md). 기존 atomic/backup/checksum/파일명은 유지한다.

## Workbench I–II Content Expansion (2026-10-18)

현재 Save9/Protocol11, v1~v8 호환. Waterskin Charges/Quality, Player Utility, Cooking Pot cargo와 신규 설치물은 기존 codec에 포함된다. 빈/구 stack은0/Empty, 이전 플레이어는 Utility 없음이다. 기존 지형 버전과 source ID를 유지한다. QA는 Play 전 IsolatedQaProfile을 사용하고 사용자 저장/백업을 fixture로 사용하지 않는다. [상태 의미](CONTENT_EXPANSION.md).

지역 회복도 기존 Save9 Recovery.Pending/Harvests/Serial/CandidateCursor와 ResourceSnapshot을 사용한다. g/c/e tombstone과 살아 있는 p를 기준으로 지역 목표를 재구성한다. old v2 depletion은 활성화 때 새 delay부터 시작하고 새 Due는 reload/rehost로 초기화하지 않는다. 현 정책은 [RESOURCE_TEMPO](RESOURCE_TEMPO.md)에 있다.


## Workbench III 현재 상태 · 2026-10-19

현재 Save10(v1–v9 지원), Protocol12. PlaceableSnapshot에 TurretSnapshot(ammo3/역할 module5/heading/loaded/reload remaining)과 BeaconNumber, PlayerSnapshot에 navigation mode/BeaconId를 저장한다. item/type/위치/HP는 기존 placeable 저장을 재사용한다. 기존 generation1/2는 보존하며 신규 generation3만 III natural content를 생성한다. 비포탑의 null nested state가 Unity JSON에서 빈 객체로 복원되는 경우는 빈 내용만 허용하고 cargo가 들어 있으면 거부한다. 포탑 추가 저장 파일은 없다.
