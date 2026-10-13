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

Pause 상태와 동결 정책을 분리했다. GameplaySession은 UI/입력 상태를 관리하고 WorldPauseController만 현재 싱글플레이 Pause/Defeat 정책에 따라 Time.timeScale을 변경한다. FreezeWorldForPause=false이면 메뉴 상태에서 월드를 동결하지 않는 정책을 사용할 수 있지만 실제 멀티플레이는 없다. Continue는 일반 플레이로 돌아가며 Inventory를 자동 재개하지 않는다.

가방은 Hotbar 왼쪽의 별도 버튼이고 우상단은 Pause다. 없는 Settings/테스트 버튼은 없다.
