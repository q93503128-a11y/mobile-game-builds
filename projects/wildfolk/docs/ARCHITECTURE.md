# 현재 코드 연결

Runtime(Input System/UGUI), Editor 생성 도구, Test 어셈블리를 분리한다. 씬 전체를 런타임에 생성하지 않는다.

| 영역 | 연결 |
|---|---|
| 입력/이동 | UI.inputactions → InputSystemUIInputModule → VirtualJoystick/HoldAttackInput → PlayerMotor/FistCombat |
| 외형 | PlayerAvatar의 몸통/독립 양손/부착 위치, 별도 HitFeedback pivot, DepthSort |
| 전투/HP | FistCombat → Collider의 공통 Health → WorldHealthBar/HitFeedback/사망 구독자 |
| 채집 | ResourceDefinition(Vitality/DropItem/DropCount) → ResourceNode → WorldPickup |
| 획득 | WorldPickup → InventoryModel.Add → 수용량만 제거하고 잔량 유지 |
| 인벤토리 | ItemDefinition + ItemStack → InventoryModel.Changed → InventoryPanel/SlotView |
| 세션 | GameplaySession Running/Inventory/Defeated → 입력 차단/Time.timeScale/재시작 |
| 적 | EnemyController 상태 전환/장애물 검사 → 명시적 Health 피해, Trigger/Kinematic |
| 보스 HUD | BossIdentity 등록/해제 → BossHud 이름/실제 Health |

실제 수치는 ScriptableObject 에셋 한 곳에서 관리하고 기존 에셋은 씬 재생성 때 보존한다. 무기/도구 시스템을 미리 구현하지 않았다.

저장된 Wildfolk.unity와 Player/Enemy/WorldPickup 프리팹을 사용한다. 세션·목표·UI 참조는 명시적으로 연결하며 Prefab 인스턴스 변경을 기록한 뒤 저장한다. WorldArt/GameplayUiFactory가 반복 구성을 공유한다.

빈 인벤토리로 시작한다. 고정 숫자/테스트 아이템/가짜 보스는 없다. 보스 fixture는 자동 테스트에서만 생성/제거한다. EventSystem은 포인터 위치/클릭 액션만 사용한다. 런타임 포커스 정책을 우회하지 않는다. 비활성 에디터의 자동 테스트에서는 입력 설정을 테스트 범위 안에서만 변경/복원한다.

적 경로 찾기는 주변 장애물 좌우 방향 검사 수준으로 복잡한 지형의 최단 경로나 탈출을 보장하지 않는다. 적 지속 생성이나 반복 VFX는 없다. 성능 최적화 완료를 주장하지 않는다. 네트워크/저장 데이터는 없다.
