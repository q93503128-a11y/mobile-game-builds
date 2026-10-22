# Building / Placement

현재 설치물11종은 Workbench I/II, Campfire, Furnace, Blast Furnace, Chest24/36, Wood/Stone Wall, Wood Door, Torch다. Recipe·해금·처리 수치는 [CRAFTING](CRAFTING.md), [METAL_PROGRESSION](METAL_PROGRESSION.md)이 정본이다.

PlayerPlacement가 기존 Hotbar 선택과 Primary 설치/미리보기를 맡는다. 범위, 유한 좌표, 영역 bounds, footprint 중심·모서리·변의 지면/물/통행, Physics overlap을 검사한다. 지상과 Cavern 모두 같은 경로다. Door/Wall 회전·연결·문 통행 규칙은 기존 WorldDoor/WallVisual을 유지한다.

SP는 InventoryModel.TryDropSlot 예약→Instantiate 성공→commit이며 실패 시 원본 슬롯을 복원한다. Host는 NetworkConstruction의 owner/sender/sequence/연결/생존/selected slot/inventory revision과 같은 placement 검사를 추가한다. 등록 variant의 NGO spawn 성공까지 포함하고 실패 객체를 정리한다. Creator metadata는 개인 소유 잠금이 아니다.

WorldPlaceable.SaveId가 저장 ID다. chunk sleep은 PlaceableSnapshot, Load/re-host는 같은 ID를 복원한다. Workbench I→II도 위치/ID/HP 비율을 보존한다. Home은 그 제작대 ID를 참조하고 별도 building ownership을 만들지 않는다. 설치 Torch는 하나의 휴대 Torch item을 소비하며 기존 설치·저장·동기화 구조를 공유한다.

먼 시설은 active GameObject/조명 없이 sleep snapshot과 게임 시간으로 처리하고, 재방문/Save 때 cargo를 계산한다. 실제 오프라인 시간은 더하지 않는다. 설치물 회수/개인 lock/새 건축 tier/Workbench III는 이번 범위에 추가하지 않았다. 권위와 검수 한계는 [멀티](MULTIPLAYER_FOUNDATION.md), [탐험](EXPLORATION_FOUNDATION.md), [저장](SAVE_PERSISTENCE.md)을 따른다.

## Workbench I–II Content Expansion (2026-10-18)

현재 설치물24종. Wood Fence/Fence Gate/Gate, Stone Door/Fence/Fence Gate/Gate, Cooking Pot, Reinforced Wall/Door/Gate, Spike Trap, Brazier를 추가했다. Door는2.1unit, Gate는3.4unit 출입구다. 공통 WorldDoor의 충돌·애니메이션·닫힘 점유 검사/Host 권위/Save ID를 사용한다. 보강 Wall/Door/Gate HP360/320/420. Trap은 HP100, 피해16/1.2초이며 같은 팀 플레이어/시설과 자원을 공격하지 않는다. 세부 비용은 [콘텐츠 표](CONTENT_EXPANSION.md).

## Repair / Overrepair / Dismantle (Class Foundation)

PlayerBuilt 설치물에 공통2초 Hold 수리/회수. 최근3초 피해·이동·피해·상태·거리/LOS·대상 파괴·재료 부족/슬롯 revision 변경은 취소하며 한 번 누름에 한 거래만 완료한다. 일반 수리는 부족 HP비율에 따른 RepairMaterials를 ceil 차감한다. full repair는 공통 Wood/Stone/일반 금속으로 construction의 약25%(작은 정수 비용은 최소1개라25%를 초과할 수 있음). Diamond/Crystal/MechanicalPart를 수리비로 요구하지 않는다. 실제 전 설치물 비용은 validation/class-progression/building-economy.tsv.

Engineer HP수리는 무료, full HP에서 별도 buffer를 보강한다. 기본20% +A2 10% +대상 전문화10%, 원래 definition HP기준이며 MaxHP/Frame을 변경하지 않는다. buffer부터 피해를 흡수하고 같은 세션 chunk sleep/wake만 보존, Save/load/reHost는 지운다. BoneTrap 단발 소비는 수리로 되돌리지 않는다. SpikeTrap은 HP수리 대상이다.

회수는 HP90%이상 canonical construction100%, 그 미만 각 재료 floor(비용×현재HP비율). WB III는 base+각 upgrade 비용을 정확히 한 번 포함한다. batch 제작 분수 비용은90%이상 portable item1개 반환, 미만0개. 기존 무료/자연 시설은 PlayerBuilt=false라 회수할 수 없다. Creator는 개인 잠금이 아니다. chest/queue/reserved/output/turret 탄약·Loaded·module이 남으면 차단하며 빈 가방 공간이 충분할 때 원자 지급 후 제거한다.

Overclock은 원래 포탑의 reload/rotation만 임시 강화한다. damage/range/AutoAim/Loader/ammo/module을 지급하거나 소비한 탄약을 복원하지 않는다. node 수치는 CHARACTER_PROGRESSION 정본을 따른다.
