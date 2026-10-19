# Editor Developer Harness

Wildfolk > Developer > Open Test Console은 Unity Editor Play Mode 전용 창이다. Editor 폴더의 코드와 DeveloperTestProfile 자산은 게임 HUD에 연결되지 않고 Release에서 접근할 수 없다. Windows 입력이나 창 포커스 제어를 자동 실행하지 않는다.

제공 기능: Item ID/수량 지급, 현재 장비 전체 지급, HP/Hunger/Thirst/XP/Gold 변경, Day/Time, Chicken/Boar/Rabbit/Deer/Sheep/Zombie/Skeleton/검 Skeleton/Goblin/새총 Goblin Spawn, Teleport, Seed 생성/재생성, 가까운 적 정리, Early Game/Stone Tier/Iron Tier 프로필, Combat Sandbox, 명시적 Save/Load, Data Validation.

프로필은 실제 현재 장비/재료/시설만 사용한다. 도구 변경은 자동 저장을 끄므로 테스트 세팅이 일반 자동 저장으로 남지 않는다. 명시적인 Save는 가능하다. Sandbox는 열린 플레이 세션의 주변을 검증 환경으로 구성하고 자동 Spawn/재생/시간을 멈춘다. 실제 Tree/Stone/Ore와 Target을 배치하고 현재 장비를 제공한다. 별도 제품 씬이나 개발 버튼을 HUD에 남기지 않는다. 씬 재시작으로 일반 플레이로 돌아간다.

창의 Last attack은 실제 PlayerCombat의 타격 대상 수와 적용 피해 합계를 표시한다. 재미/손맛/난이도 판정 도구가 아니다. MCP는 같은 공개 DeveloperHarness API로 상태를 구성하고 실제 화면을 캡처할 수 있다.

ProjectDataValidator는 Item/Recipe ID, 참조 재료/결과, Station/Tier/Stack, Loot 수량·확률, Resource 최소 Tool Tier, Equipment 업그레이드 순환, 생성/적 데이터, Container 상태 관계, 실행 중 Save ID 충돌을 검사한다.

Console은 Player / Equipment / Progression / World / Save / Validation Section으로 구성한다. Level/Skill Points/Stamina, 가죽·철 세트 지급/장착/해제, Workbench II Profile과 실제 설치물을 추가했다. 기존 Harness API를 공유하며 중복 치트를 만들지 않는다.

Set XP/Level은 Skill을 초기화한다. Set Points는 Level을 Point Budget과 일치시키고 Skill을 초기화한다. Clear Armor는 가방 공간이 있어야 해제한다. New Test State는 씬을 재시작하며 기존 사망 정책을 바꾸지 않는다. Console 창은 사용자가 메뉴를 눌렀을 때만 연다. Armor/Growth/Locomotion/Workbench Upgrade 데이터도 Validator에서 검사한다.


Resource Population Section: Force Recovery Tick, World Regen Time +1m/+5m/+10m/Custom seconds, 종류별 Current/Target 및 Pending Region/남은 시간/최근 Harvest 위치·제외 만료를 제공한다. 실제 40분 대기는 필요 없다. World Clock을 가속하며 자동 저장을 끈다. 현재 편집용 표시만 있고 Release Game View에는 나오지 않는다. Localization 번역/Placeholder, Skill 선행 순환, Resource 회복 정책도 Data Validator에서 검사한다.


## Console UX / 생태 단축 검사

기존 Wildfolk > Developer > Open Test Console 메뉴를 유지한다. 사용자가 직접 메뉴를 눌렀을 때만 창을 연다. Quick Test에는 HP·생존 회복/현재 Tier 도구/Armor/기존 Skill 보존한 다음 Level Point/Day/Night/Sandbox/Resource Tick/Save/Load를 둔다. 각 Section 및 주요 Action에 English/한국어 한 줄 설명·Tooltip을 제공하고 변경 경고와 Save 덮어쓰기/Load 미저장 손실 경고를 표시한다.

Spawn은10종·Variant와 Warband/Camp를 지원한다. Trigger Ally Assist/Trigger Herd Flee는 가장 가까운 대상과 주변 Group에 적용한다. Force Animal Population Tick/Force Camp Population Tick은 기한·안전 후보·상한을 그대로 검사하며 실제 시간 대기는 하지 않는다. +Regen 시간 단축은 Resource Pending/Harvest 제외 타이머만 앞당기고 낮/밤 WorldClock은 바꾸지 않는다. 동물/Camp 기한은 Set Day/Time으로 앞당긴 뒤 Force Tick할 수 있다.

Combat Sandbox의 무의미한 갈색 Player Body Renderer2개는 제거했다. 실제 자원과 Goblin을 배치한다. 기존 PlayerEquipment/Inventory/Spawn/Save API를 그대로 사용하는 단축 동작이며 Release HUD에 Console을 추가하지 않는다. 테스트 저장의 성공 여부와 프레임 캡처는 실제 수행한 것만 보고한다.

## 탐험 성능 기록

Editor 전용 ExplorationProfiler.Begin(stage, frameCount, moveX, moveY)는 Unity ProfilerRecorder의 Main Thread/GC Allocated In Frame과 frame delta, 활성 GO/chunk/resource/creature/terrain/cache 수치를 Captures/exploration/profile-*.json으로 기록한다. EditorApplication.update에서 한 rendered frame당 한 번만 수집하고 완료/Play 종료 시 해제한다. Scene/Prefab/게임 UI에는 probe를 저장하지 않는다. Editor·MCP·동시 batch 작업 overhead가 포함된 탐색 수치이며 standalone player allocation/FPS나 모바일 실기 수치가 아니다.

## Workbench I–II Content Expansion (2026-10-18)

ContentExpansionAssets는 기존 scene IDs를 보존하는 콘텐츠 authoring이다. RefreshBindings는 player 행동/Utility UI 참조만 갱신한다. IsolatedQaProfile.Begin은 Play 전에 별도 Captures 디렉터리를 선택하고 기본 설정/멀티 식별자를 격리한다. End는 Play 중지 후 호출한다. ContentExpansionQa는 격리 확인을 통과한 Editor fixture API다.

## Gamefeel 통합 검수

Editor 전용 `GamefeelQa.Begin()`은 fresh isolated Play에서 command/UI callback/drag handler 흐름을 기록한다. `Resume()`은 준비된 QA 월드에서 전투 이후 checkpoint를 실행한다. ContentExpansionQa.Prepare가 격리 profile/repository를 검사한다. Resource teleport/아이템 지급/정지 표적/시설 Advance를 사용하며 자연 progression/사람의 터치 검수와 다르다. SceneFlow.Load가 이전 coroutine을 종료하므로 로드 후 상태는 MCP로 별도 조회한다. `Captures/gamefeel/play.tsv`와 snapshot을 저장하고 정본 요약은 validation/gamefeel에 선별한다. 실제 저장/설정은 사용하지 않는다.
