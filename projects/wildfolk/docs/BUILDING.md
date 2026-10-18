# Building / Placement

현재 설치물11종은 Workbench I/II, Campfire, Furnace, Blast Furnace, Chest24/36, Wood/Stone Wall, Wood Door, Torch다. Recipe·해금·처리 수치는 [CRAFTING](CRAFTING.md), [METAL_PROGRESSION](METAL_PROGRESSION.md)이 정본이다.

PlayerPlacement가 기존 Hotbar 선택과 Primary 설치/미리보기를 맡는다. 범위, 유한 좌표, 영역 bounds, footprint 중심·모서리·변의 지면/물/통행, Physics overlap을 검사한다. 지상과 Cavern 모두 같은 경로다. Door/Wall 회전·연결·문 통행 규칙은 기존 WorldDoor/WallVisual을 유지한다.

SP는 InventoryModel.TryDropSlot 예약→Instantiate 성공→commit이며 실패 시 원본 슬롯을 복원한다. Host는 NetworkConstruction의 owner/sender/sequence/연결/생존/selected slot/inventory revision과 같은 placement 검사를 추가한다. 등록 variant의 NGO spawn 성공까지 포함하고 실패 객체를 정리한다. Creator metadata는 개인 소유 잠금이 아니다.

WorldPlaceable.SaveId가 저장 ID다. chunk sleep은 PlaceableSnapshot, Load/re-host는 같은 ID를 복원한다. Workbench I→II도 위치/ID/HP 비율을 보존한다. Home은 그 제작대 ID를 참조하고 별도 building ownership을 만들지 않는다. 설치 Torch는 하나의 휴대 Torch item을 소비하며 기존 설치·저장·동기화 구조를 공유한다.

먼 시설은 active GameObject/조명 없이 sleep snapshot과 게임 시간으로 처리하고, 재방문/Save 때 cargo를 계산한다. 실제 오프라인 시간은 더하지 않는다. 설치물 회수/개인 lock/새 건축 tier/Workbench III는 이번 범위에 추가하지 않았다. 권위와 검수 한계는 [멀티](MULTIPLAYER_FOUNDATION.md), [탐험](EXPLORATION_FOUNDATION.md), [저장](SAVE_PERSISTENCE.md)을 따른다.
