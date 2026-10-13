# 인벤토리와 입력 규칙

## 단일 모델

InventoryModel의 실제 배열은 36칸이다. 0~8은 Hotbar, 9~35는 Storage다. HotbarView/InventoryPanel은 같은 Owner.Model과 PlayerInventory.SelectedIndex를 읽는다. UI SlotView 배열은 표시 참조이며 아이템 데이터를 복제하지 않는다.

모델 변경 이벤트가 Hotbar/인벤토리/제작/장착을 갱신한다. 선택 인덱스는 이동 후에도 유지하므로 선택 슬롯의 아이템이 빠지면 맨손, 다른 도구가 들어오면 해당 도구다.

## 드래그

가방을 연 상태에서 Pointer Event로 조작한다. Desktop Mouse와 Mobile Pointer의 BeginDrag/Drag/Drop/EndDrag가 같은 InventorySlotDrag/InventoryDragController를 통과한다.

| 대상 | 동작 |
|---|---|
| 빈 슬롯 | 전체 스택 이동 |
| 같은 아이템 | 최대 스택까지 합침, 잔량은 Source 유지 |
| 다른 아이템 | 두 스택 교환 |
| 같은 슬롯/가득 찬 같은 아이템/빈 Source | 변경 없음 |
| 슬롯 밖 | 취소, 원래 데이터 유지 |

Storage↔Hotbar, Hotbar 내부 이동에 같은 MoveStack을 사용한다. 드래그는 인벤토리 안의 Hotbar 행에서 가능하고 닫힌 하단 Hotbar는 선택용이다. 원본 아이콘을 흐리게 하고 Ghost가 포인터를 따라가며 목표 슬롯을 녹색/적색으로 표시한다. Ghost는 Raycast를 차단하지 않는다. UI가 닫히거나 사망/Pause하면 드래그를 취소한다. 동시 포인터가 진행 중 드래그를 탈취하지 못한다. 반 스택 분할은 없다.

## 제작/수량 안전성

Add는 수용량을 반환하고 바닥 드랍 잔량을 유지한다. 제작은 임시 트랜잭션 배열에서 비용/결과 공간을 확인한 뒤 실제 배열에 한 번에 적용한다. 이는 일시적 검증 복사본이며 별도 인벤토리 정본이 아니다. 실패하면 재료/아이템/변경 이벤트가 유지된다. 완전히 소비한 재료 슬롯은 결과 공간으로 사용할 수 있다. 배치는 선택 슬롯과 예상 아이템이 맞을 때 1개를 소비한다.

## Inventory와 Pause

Inventory 상태는 플레이어 이동/공격/도구/배치 입력을 차단한다. Time.timeScale을 변경하지 않는다. 적/드랍/월드는 계속 움직이며 공격받을 수 있다.

Pause 상태와 동결 정책을 분리했다. GameplaySession은 UI/입력 상태를 관리하고 WorldPauseController만 현재 싱글플레이 Pause/Defeat 정책에 따라 Time.timeScale을 변경한다. SessionMode.Multiplayer에서는 Pause와 로컬 Defeat가 전역 동결을 만들지 않는다. 실제 멀티플레이는 없다. Continue는 일반 플레이로 돌아가며 Inventory를 자동 재개하지 않는다.

가방은 Hotbar 왼쪽의 별도 버튼이고 우상단은 Pause다. Settings는 Pause 메뉴 안에 실제 슬라이더로 제공하며 테스트 버튼은 없다.

소비품은 선택 슬롯에서 Use하여 1개를 줄이며 수치가 이미 Max이면 거부한다. 조리는 TryConsumeCosts가 비용을 원자적으로 시설로 넘기고 완성 후 Take로 결과를 원자적으로 수령한다. 결과 공간이 없으면 시설에 남는다.

가방 바로가기는 일반 플레이에서 Hotbar 왼쪽, 인벤토리가 열린 동안은 패널 바깥 좌하단에 표시하며 다시 눌러 닫을 수 있다.

## Storage Chest

각 StorageChest는 독립 InventoryModel18칸을 Awake에서 생성한다. PlayerStorage는 살아 있고 가까운 상자를 열고, 떠나거나 파괴/사망/Pause하면 닫는다. 상자 UI와 일반 Inventory UI는 동시에 표시하지 않는다. 상자 상단18/하단PlayerStorage27+Hotbar9를 표시하고 기존 Drag/Ghost를 공유한다.

InventoryModel.Transfer(source,index,target,index)는 동일 모델 내부와 서로 다른 모델 사이의 전체 이동/한도 합치기/교환을 모두 처리한다. 두 배열을 먼저 수정한 다음 각 Changed 이벤트를 한 번 발생시켜 중간 상태/아이템 복제를 막는다. 실패/바깥 Drop은 변경하지 않는다. 선택 Hotbar 물품이 빠지면 장착도 갱신한다. 상자마다 내용을 닫기/재접근 동안 유지하고 파괴 시 월드 드랍으로 보존한다. 월드 Save가 없어 앱 재실행에는 보존되지 않는다.

상자를 열어도 Time.timeScale을 바꾸지 않고 적/시간/허기·갈증/시설 작업은 진행한다. 슬롯 분할/빠른 이동/잠금/멀티 권한/상자 저장 시스템은 없다.

Inventory/Chest의 모달 패널은 생존·시간 HUD 아래에 표시하며 좁은/넓은 화면의 남은 폭·높이에 맞춘다. 가방 바로가기는 패널 바깥 좌하단에 남고, 세계 상태 HUD는 계속 읽을 수 있다.
