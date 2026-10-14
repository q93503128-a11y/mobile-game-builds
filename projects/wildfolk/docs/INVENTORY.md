# Inventory / Hotbar / Storage

PlayerInventory는36칸(Hotbar9+Storage27), Wood Chest는24칸이다. Item ID를 기준으로 합치고 MaxStack은 ItemDefinition이 정본이다. 재료50, 도구/검1, 빈 Wooden Cup20, Raw/Clean Cup1이다.

일반 Drag는 전체 Stack을 옮긴다. 빈 슬롯/같은 Item 합치기/전체 교환을 지원하고 Inventory↔Hotbar↔Chest에 같은 InventoryModel.Transfer를 사용한다. 부분 수량을 다른 Item 위에 놓아 교환하지 않는다. 빈 화면 드랍은 취소이며 삭제하지 않는다.

## 모바일 소분

슬롯 Long Press0.45초→Item Actions→Split. 선택한 슬롯의 Split 버튼으로도 연다. 현재/선택 수량, −1/+1/Half/Confirm/Cancel을 제공한다. 선택 범위1~현재수량−1이며 채운 컵과 단일 장비에는 소분을 제공하지 않는다.

Confirm은 임의의 빈 슬롯에 분배하지 않는다. 선택 수량을 Ghost로 들고 있는 상태를 만든다. 원하는 Inventory/Hotbar/Chest 슬롯을 누르거나 드래그하여 배치하고, 명확한 Drop to ground에 놓으면 선택 수량만 버린다. 원본 모델은 배치 성공 전까지 유지한다. 원본 슬롯에는 예약 후 남을 수량을 표시한다. 상단 Cancel 또는 가방 닫기는 원본 수량을 보존한다. 상자 대상에는 선택 테두리를 표시한다.

PlayerItemDrop은 물/장애물 없는 근처 지점을 확인한 뒤 WorldPickup을 만들고 정확한 수량만 소비한다. 실패하면 원본을 유지한다. 버린 아이템은 즉시 자동 회수하지 않는다. WorldPickup은 Pool을 재사용한다.

빈 컵이 있어야 자연 수원에서 Raw Cup을 얻는다. 마시면 Empty Cup을 반환한다. 상태별 Item ID와 모든 슬롯을 Save/Load하고 Chest의 용기 상태도 유지한다. 소분 중인 UI 포인터 상태는 저장하지 않으며 원본 아이템은 저장에 남는다.

가방/Chest 입력 잠금은 세계 정지와 분리한다. 실제 Multiplayer 네트워크와 Player 전달은 아직 없다. Friendly Fire는 관계 필터 기반 OFF이며 Team 시설을 일반 공격으로 파괴하지 않는다.

Character의Equipment 탭에서Head/Body Armor를장착·교체·해제한다. 장착물은Inventory에서빠지고해제/교체시반환하며공간없으면해제를거절한다. Armor/Skill은별도Snapshot으로저장한다. Reinforced Chest는독립32칸이며기존Storage Chest24는유지한다. Chest UI는용량에맞춰3/4행을표시하고기존Transfer/Split/Drop을공유한다.
