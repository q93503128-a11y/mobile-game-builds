# 인벤토리·상자·드랍·소분

PlayerInventory.Model은36칸,0~8Hotbar/9~35Storage다. Hotbar/UI/장착은 같은 배열과SelectedIndex를 읽는다. Chest는 PlaceableDefinition.StorageSlots=24의 독립 모델이며 UI 위3×8, 아래PlayerStorage27+Hotbar9다. 닫아도 세션 동안 내용이 남고 파괴 시 월드 드랍한다. 앱 재실행 보존은 없다.

InventorySlotDrag/InventoryDragController가 동일한 포인터 흐름으로 Player↔Chest↔Hotbar 이동한다. 빈 슬롯 전체 이동, 같은 아이템 한도 합치기와 잔량 유지, 다른 아이템 전체 교환, 가득 찬 동일 대상/빈Source/동일슬롯은 변화 없음이다. 두 모델을 모두 변경한 뒤 이벤트를 보내므로 UI에 중간 상태를 노출하지 않는다.

상자에서 탭한 스택도 테두리로 선택 표시한다. 스택을 탭하고 Split half를 누르면 같은 모델의 첫 빈 슬롯에 floor(count/2)를 옮긴다. Wood30→15/15,31→16/15다. 빈 슬롯이 없거나1개이면 변경하지 않는다. 나눠진 스택은 기존 드래그로 Inventory/Chest/Hotbar/바닥에 옮길 수 있다. Right Click/수량 입력 UI는 사용하지 않는다. 모델은 부분 수량 전송 API도 제공하지만 현재 제품 UX는Split half다.

드래그 중에만 상단 중앙 Drop to ground 영역이 나타난다. 거기에 놓아야 PlayerItemDrop이 가까운 빈 육지에 WorldPickup을 생성하고 Source 스택을 소비한다. 주변에 안전한 공간이 없으면 원래 스택을 유지한다. UI 밖 아무 곳에 놓는 것은 취소이며 아이템을 삭제하지 않는다. 플레이어가 버린 아이템은 제자리에 짧게 떨어지고 바로 재흡수하지 않으며 나중에 접근해 줍는다.

Ghost는 데이터 복사본이 아닌 표시이며 Raycast를 막지 않는다. 원본 아이콘을 흐리게 표시하고 가능한 대상/불가능 대상을 색으로 구분한다. UI닫힘/Pause/사망/Chest 변경으로 드래그와 선택을 취소한다. 동시 포인터가 진행 중 드래그를 탈취하지 못한다.

제작은 임시 배열에서 재료와 결과 공간을 검증하고 한 번에 확정한다. 배치는 성공 시 선택 슬롯1개를 소비한다. 장비 업그레이드는 Recipe에 적힌 이전 장비만 소비하며 직접 얻은 상위 장비는 하위 장비를 자동 삭제하지 않는다. XP/Gold는 별도PlayerProgression이므로 슬롯을 차지하지 않는다.

가방/상자는 로컬 이동·공격·상호작용만 막고 세계/적/시간/생존/처리를 진행한다. Singleplayer Pause/Defeat의 세계 동결은 별도 정책이다. 실제 네트워크는 없다. ModalLayout은 생존·시간HUD/가방 영역을 피하도록 패널을 맞춘다. 작은 화면의 실제 터치 적합성은 사용자가 판단한다.
