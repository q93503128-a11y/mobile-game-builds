# Inventory / Hotbar / Storage

PlayerInventory는36칸(Hotbar9+Storage27), Wood Chest는24칸이다. Item ID를 기준으로 합치고 MaxStack은 ItemDefinition이 정본이다. 재료50, 도구/검1, 빈 Wooden Cup20, Raw/Clean Cup1이다.

일반 Drag는 전체 Stack을 옮긴다. 빈 슬롯/같은 Item 합치기/전체 교환을 지원하고 Inventory↔Hotbar↔Chest에 같은 InventoryModel.Transfer를 사용한다. 부분 수량을 다른 Item 위에 놓아 교환하지 않는다. 빈 화면 드랍은 취소이며 삭제하지 않는다.

## 모바일 소분

슬롯 Long Press0.45초→Item Actions→Split. 선택한 슬롯의 Split 버튼으로도 연다. 현재/선택 수량, −1/+1/Half/Confirm/Cancel을 제공한다. 선택 범위1~현재수량−1이며 채운 컵과 단일 장비에는 소분을 제공하지 않는다.

Confirm은 임의의 빈 슬롯에 분배하지 않는다. 선택 수량을 Ghost로 들고 있는 상태를 만든다. 원하는 Inventory/Hotbar/Chest 슬롯을 누르거나 드래그하여 배치하고, 명확한 Drop to ground에 놓으면 선택 수량만 버린다. 원본 모델은 배치 성공 전까지 유지한다. 원본 슬롯에는 예약 후 남을 수량을 표시한다. 상단 Cancel 또는 가방 닫기는 원본 수량을 보존한다. 상자 대상에는 선택 테두리를 표시한다.

PlayerItemDrop은 물/장애물 없는 근처 지점을 확인한 뒤 WorldPickup을 만들고 정확한 수량만 소비한다. 실패하면 원본을 유지한다. 버린 아이템은 즉시 자동 회수하지 않는다. WorldPickup은 Pool을 재사용한다.

빈 컵이 있어야 자연 수원에서 Raw Cup을 얻는다. 마시면 Empty Cup을 반환한다. 상태별 Item ID와 모든 슬롯을 Save/Load하고 Chest의 용기 상태도 유지한다. 소분 중인 UI 포인터 상태는 저장하지 않으며 원본 아이템은 저장에 남는다.

가방/Chest 입력 잠금은 세계 정지와 분리한다. 멀티 가방 전달은 Foundation 3의 Host 권위 기반으로 제공하며 실제2인 통합 QA는 남아 있다. Friendly Fire는 관계 필터 기반 OFF이며 Team 시설을 일반 공격으로 파괴하지 않는다.

Inventory의 작은 캐릭터 Preview와 실제 Head/Body 슬롯에서 Armor를 장착·교체·해제한다. 슬롯을 누르면 보유한 호환 장비를 강조하고, 선택 장비의 슬롯별 Defense 비교와 Equip을 제공한다. 장착 슬롯 선택에서는 Unequip을 제공한다. Profile에는 장비 조작을 중복 구현하지 않는다. 아직 없는 Accessory/Back 슬롯은 노출하지 않는다. 장착물은Inventory에서빠지고해제/교체시반환하며공간없으면해제를거절한다. Armor/Skill은별도Snapshot으로저장한다. Reinforced Chest는독립36칸이며기존Storage Chest24는유지한다. Chest UI는가변용량과세로스크롤을지원하고기존Transfer/Split/Drop을공유한다.


Armor를 가방에서 맞는 Head/Body 슬롯으로 Drag하면 즉시 장착한다. 이미 장착된 방어구는 교환 시 소지품으로 돌아온다. 장착 슬롯에서 빈 가방 칸으로 Drag하면 해제하며 들어갈 공간이 없으면 원본을 유지하고 Feedback을 표시한다. 선택 Armor는 Primary 또는 Interact로 같은 PlayerEquipment API를 통해 장착한다. 도구/무기/설치/소비 입력과 구분한다. 큰 Modal 뒤 Bag/Hotbar/Attack/Dash/Interact/Pause는 입력을 받지 않는다. Modal 안 Close로 닫고,Split의 Cancel은 Modal 계층에서 작동한다.

## Multiplayer Foundation 3

기존36/9 슬롯/Stack/Drag/Split/Item Actions/Armor UI를 재사용한다. 멀티 변경은 owner 요청→Host revision/slot/수량 검증→공통 Model mutation→Owner-only slot delta다. Client 모델은 읽기 전용이다. 멀티 Ground pickup은 한 stack을 전량 넣을 수 있을 때만 소비하고 full bag이면 그대로 남긴다. Player Drop은 예약 감소와 Ground 생성의 성공/실패를 묶는다. Foundation 3 당시 Chest/Crafting/시설은 제외했다. 현재 공용 Chest24/36은의 server-write 슬롯 delta와 기존 split/merge/drag UI를 연결한다. 가방·시설 revision을 함께 검사하며 held split은 시작 revision을 유지한다. 두 Player 동시 열기에는 독점 lock을 만들지 않는다. 계정 저장은 없고 현재 Host 저장은 아래 후속 범위를 따른다. 실제2인 검증은 미수행이며 자세한 규칙/자동검사는 MULTIPLAYER_FOUNDATION을 따른다.


Foundation5는 Host의 v5 파일에 식별자별 Inventory/선택/장비/HP·생존/XP·Gold 등을 보존한다. 기기 생성 Guid는 계정 인증이 아니며 정상 재입장 시 새 actor spawn 전 복원한다. 실제 remote 재Join 검수는 남아 있다. 현재 저장 구조와 한계는 SAVE_PERSISTENCE/MULTIPLAYER_FOUNDATION을 따른다.

## 초기 금속

총 Item48종이다. Copper Ore/Tin Ore/Coal/Copper Ingot/Tin Ingot/Bronze/Steel은 각각50 Stack 재료다. 기존 Iron IDs/36·9 슬롯/Chest24·36/장착/드랍/소분을 유지한다. 합금의 모든 입력은 한 transaction으로 예약되며 하나라도 부족하면 소비0, output 공간 부족이면 cargo 유지다. SP 저장 복원과 Host 저장·재Host의 새 재료·예약 queue 검사는 METAL_PROGRESSION을 따른다.
