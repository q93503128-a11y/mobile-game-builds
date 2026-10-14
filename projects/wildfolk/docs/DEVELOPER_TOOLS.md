# Editor Developer Harness

Wildfolk > Developer > Test Harness는 Unity Editor Play Mode 전용 창이다. Editor 폴더의 코드와 DeveloperTestProfile 자산은 게임 HUD에 연결되지 않고 Release에서 접근할 수 없다. Windows 입력이나 창 포커스 제어를 자동 실행하지 않는다.

제공 기능: Item ID/수량 지급, 현재 장비 전체 지급, HP/Hunger/Thirst/XP/Gold 변경, Day/Time, Zombie/Goblin/Chicken/Boar Spawn, Teleport, Seed 생성/재생성, 가까운 적 정리, Early Game/Stone Tier/Iron Tier 프로필, Combat Sandbox, 명시적 Save/Load, Data Validation.

프로필은 실제 현재 장비/재료/시설만 사용한다. 도구 변경은 자동 저장을 끄므로 테스트 세팅이 일반 자동 저장으로 남지 않는다. 명시적인 Save는 가능하다. Sandbox는 열린 플레이 세션의 주변을 검증 환경으로 구성하고 자동 Spawn/재생/시간을 멈춘다. 실제 Tree/Stone/Ore와 Target을 배치하고 현재 장비를 제공한다. 별도 제품 씬이나 개발 버튼을 HUD에 남기지 않는다. 씬 재시작으로 일반 플레이로 돌아간다.

창의 Last attack은 실제 PlayerCombat의 타격 대상 수와 적용 피해 합계를 표시한다. 재미/손맛/난이도 판정 도구가 아니다. MCP는 같은 공개 DeveloperHarness API로 상태를 구성하고 실제 화면을 캡처할 수 있다.

ProjectDataValidator는 Item/Recipe ID, 참조 재료/결과, Station/Tier/Stack, Loot 수량·확률, Resource 최소 Tool Tier, Equipment 업그레이드 순환, 생성/적 데이터, Container 상태 관계, 실행 중 Save ID 충돌을 검사한다.
