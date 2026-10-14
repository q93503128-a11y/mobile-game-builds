# Editor Developer Harness

Wildfolk > Developer > Open Test Console은 Unity Editor Play Mode 전용 창이다. Editor 폴더의 코드와 DeveloperTestProfile 자산은 게임 HUD에 연결되지 않고 Release에서 접근할 수 없다. Windows 입력이나 창 포커스 제어를 자동 실행하지 않는다.

제공 기능: Item ID/수량 지급, 현재 장비 전체 지급, HP/Hunger/Thirst/XP/Gold 변경, Day/Time, Zombie/Goblin/Chicken/Boar Spawn, Teleport, Seed 생성/재생성, 가까운 적 정리, Early Game/Stone Tier/Iron Tier 프로필, Combat Sandbox, 명시적 Save/Load, Data Validation.

프로필은 실제 현재 장비/재료/시설만 사용한다. 도구 변경은 자동 저장을 끄므로 테스트 세팅이 일반 자동 저장으로 남지 않는다. 명시적인 Save는 가능하다. Sandbox는 열린 플레이 세션의 주변을 검증 환경으로 구성하고 자동 Spawn/재생/시간을 멈춘다. 실제 Tree/Stone/Ore와 Target을 배치하고 현재 장비를 제공한다. 별도 제품 씬이나 개발 버튼을 HUD에 남기지 않는다. 씬 재시작으로 일반 플레이로 돌아간다.

창의 Last attack은 실제 PlayerCombat의 타격 대상 수와 적용 피해 합계를 표시한다. 재미/손맛/난이도 판정 도구가 아니다. MCP는 같은 공개 DeveloperHarness API로 상태를 구성하고 실제 화면을 캡처할 수 있다.

ProjectDataValidator는 Item/Recipe ID, 참조 재료/결과, Station/Tier/Stack, Loot 수량·확률, Resource 최소 Tool Tier, Equipment 업그레이드 순환, 생성/적 데이터, Container 상태 관계, 실행 중 Save ID 충돌을 검사한다.

Console은 Player / Equipment / Progression / World / Save / Validation Section으로 구성한다. Level/Skill Points/Stamina, 가죽·철 세트 지급/장착/해제, Workbench II Profile과 실제 설치물을 추가했다. 기존 Harness API를 공유하며 중복 치트를 만들지 않는다.

Set XP/Level은 Skill을 초기화한다. Set Points는 Level을 Point Budget과 일치시키고 Skill을 초기화한다. Clear Armor는 가방 공간이 있어야 해제한다. New Test State는 씬을 재시작하며 기존 사망 정책을 바꾸지 않는다. Console 창은 사용자가 메뉴를 눌렀을 때만 연다. Armor/Growth/Locomotion/Workbench Upgrade 데이터도 Validator에서 검사한다.


Resource Population Section: Force Recovery Tick, World Regen Time +1m/+5m/+10m/Custom seconds, 종류별 Current/Target 및 Pending Region/남은 시간/최근 Harvest 위치·제외 만료를 제공한다. 실제 40분 대기는 필요 없다. World Clock을 가속하며 자동 저장을 끈다. 현재 편집용 표시만 있고 Release Game View에는 나오지 않는다. Localization 번역/Placeholder, Skill 선행 순환, Resource 회복 정책도 Data Validator에서 검사한다.
