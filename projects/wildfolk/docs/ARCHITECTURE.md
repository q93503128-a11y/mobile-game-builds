# 현재 코드 연결

Runtime(Input System/UGUI), Editor 생성 도구, Test 어셈블리를 분리한다. 씬 전체를 런타임에 생성하지 않는다.

| 영역 | 연결 |
|---|---|
| 입력/이동 | UI.inputactions → InputSystemUIInputModule → VirtualJoystick/HoldAttackInput → PlayerMotor/PlayerCombat |
| 방향/외형 | 공통 CharacterFacing이 눈/양손 위치·손 회전을 담당. PlayerAvatar는 몸통의 약한 이동 bob만 담당 |
| 슬롯 정본 | InventoryModel의 16슬롯 하나 + PlayerInventory의 SelectedIndex 하나 |
| Hotbar/인벤토리 | HotbarView는 슬롯 0~8을 읽고 InventoryPanel은 전체를 읽음. 모두 PlayerInventory.SelectSlot/변경 이벤트 사용 |
| 장착 | 선택 ItemDefinition.Tool → PlayerEquipment → 오른손 HeldVisual/공격 버튼 아이콘 |
| 전투 | PlayerCombat이 시작 시 도구 프로필 고정, 모션 중간 범위 재검사와 실제 피해, 선택 도구 변경 시 공격 취소 |
| 효율 | ResourceDefinition.Kind + ToolDefinition.Efficiencies → 데이터로 자원 피해 계산 |
| 시작 장비 | StartingLoadout 에셋 → PlayerInventory.Awake 지급. InventoryModel에 시작 장비 하드코딩 없음 |
| Health/드랍 | 공통 Health/WorldHealthBar/HitFeedback → ResourceNode 사망 → WorldPickup → InventoryModel.Add |
| 세션/적 | 기존 GameplaySession/EnemyController/CameraFollow/BossIdentity/BossHud 유지 |

FistCombat을 PlayerCombat으로 이름/구현을 교체하고 구 파일을 제거했다. 맨손 전투와 도구 전투를 별도 중복 시스템으로 만들지 않았다. CharacterFacing은 플레이어/Zombie가 공유한다. Body가 회전하지 않고 눈/손만 방향을 표시한다.

GameplaySettings/VitalityDefinition/ResourceDefinition/ItemDefinition/ToolDefinition/StartingLoadout 에셋이 실제 데이터 정본이다. 생성 도구는 기존 에셋을 보존한다. 기존 StoneResource에 Stone 종류를 기록했고 새 데이터 기본값도 같은 종류로 생성한다. 도끼/곡괭이의 자원 효율에 아이템 ID 조건문을 사용하지 않는다.

저장된 Wildfolk.unity와 Player/Enemy/WorldPickup 프리팹을 사용한다. 세션/목표/UI 참조를 연결하고 Prefab 인스턴스 변경을 기록해 저장한다. WorldArt/GameplayUiFactory와 슬롯 생성 코드가 반복 구성을 공유한다.

Hotbar는 별도 인벤토리/복제 ItemStack 배열을 갖지 않는다. UI의 SlotView 배열은 표시 참조뿐이다. 도구는 인벤토리 슬롯에 실제 존재하며 시작 지급 설정에서 Axe 1/Pickaxe 1을 제공한다. 빈 슬롯/재료 선택 시 장착을 해제한다. 선택 도구가 바뀌면 진행 중 공격을 취소하고 기존 쿨다운을 유지한다. 인벤토리의 다른 재료 수량이 바뀌어도 같은 도구의 공격을 불필요하게 취소하지 않는다.

이벤트는 종료 시 해제한다. UI 포커스 설정은 자동 테스트 범위에서만 변경/복원하고 런타임에서는 우회하지 않는다. 실제 보스/개발자 버튼은 없다. 적 길찾기는 단순 장애물 좌우 검사이며 네트워크/저장/제작/아이템 이동/도구 내구도는 없다.
