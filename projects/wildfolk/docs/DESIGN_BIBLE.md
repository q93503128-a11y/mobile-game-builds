# Wildfolk 기획 정본

기준일: 2026-10-13. main의 코드/데이터/씬이 실제 제공 기능의 정본이다.

## 환경과 방향

Unity 6000.3.19f1 LTS / Universal 2D, 모바일 Landscape, 기준 1920×1080의 탑다운 생존 게임이다. 이번 단계는 기존 이동/전투/채집/드랍/Hotbar를 유지하면서 맨손→Wood/Stone→Iron 제련/장비와 물/모닥불·기지 보관/방어의 극초반 생존 루프를 제공한다.

Mine Survival의 맨손 채집→기본 시설→작업대 도구/무기 제작이라는 직관적 흐름을 강하게 참고한다. 이는 사용자 지정 방향이며 상대 게임의 실제 수치를 측정한 결과가 아니다. 에셋/UI/고유 명칭·아이콘·맵을 복제하지 않는다. Minecraft 계열은 슬롯/Hotbar/드랍 관리 구조를 참고한다.

1~4인 협동, 무한 생존, 월드 저장/이어하기, 기지 생존은 지향점이며 미구현이다. 로그라이트 요소를 고려하되 영구 능력치 성장 비중은 낮게 잡고 직업 특성을 고려한다. 세부 네트워크/저장/직업/성장 규칙은 확정하지 않았다.

## 아트와 기존 기능

살구색 원형 몸체, 독립된 원형 양손, 작은 눈 두 개의 플레이어와 회녹색 원형 Zombie를 유지한다. 몸통 회전 없이 공통 CharacterFacing으로 눈/손/장비 방향을 표시한다. 입/볼터치/코/동물/옷 표현, 과한 광택/네온/파스텔/장난감 UI를 피한다. 자연색/무광/절제된 외곽선/접촉 그림자를 사용한다. 초기 아트이며 최종 아트가 아니다.

저장된 Wildfolk.unity에 플레이어, Zombie 1개, 나무 24개, 돌 10개, 철 광석 노드3개와 자연 필드를 유지한다. 작은 수원 1개와 중립 멧돼지1/닭1마리를 추가했다. 이동/카메라/공통 Health·체력바/대상 반응/자원 파괴/월드 드랍/근접 획득/스택/적 AI/사망 재시작/보스 HUD 기반을 보존한다. 실제 보스와 공용 타격 파티클은 없다.

## 현재 초반 루프

새 게임은 장비 지급 없이 빈 36슬롯과 맨손이다. StartingLoadout 코드/에셋을 제거했다. 맨손으로 나무를 채집하고 Wood 6개로 Workbench를 만들어 설치한다. 가까운 작업대를 바라보고 사용 버튼으로 제작 화면을 열며 Wood 각 5개로 기존 Axe/Pickaxe 및 새 Wood Sword를 제작한다.

아이템은 Wood/Stone/Iron Ore/Ingot 재료4종, Wood/Stone/Iron 도구·검9종, 시설·건축6종, 소비품3종으로 총22종이다. ToolDefinition 사용 프로필을 검도 공유한다. 제작 후 슬롯/Hotbar/장착을 즉시 갱신한다. 정확한 값은 BALANCE.md를 따른다.

## 인벤토리와 UI

- 실제 슬롯 0~8 = Hotbar 9칸, 9~35 = Storage 27칸. 동일 모델/선택을 공유한다.
- 가방 안에서는 Storage 3×9 위, Hotbar 1×9 아래, 오른쪽 제작 영역이다.
- 드래그로 전체 이동/같은 아이템 한도 합치기/다른 아이템 교환. 초과는 원래 슬롯에 남고 바깥 Drop은 취소한다.
- 드래그 아이콘/수량이 포인터를 따라가고 원래 아이콘을 흐리게 표시하며 Drop 대상 가능/불가능을 구분한다.
- 인벤토리는 플레이어 이동/사용만 차단하며 월드를 Pause하지 않는다.
- 가방은 하단 Hotbar 왼쪽의 독립 버튼이며 슬롯 9칸을 줄이지 않는다.
- 우상단 Pause는 Continue/Settings를 제공한다. 실제 월드 동결 정책은 별도 WorldPauseController이며 현재 싱글플레이에서는 정지한다.
- 상단 중앙은 실제 보스가 없으면 비우고 상시 재화/개발자 UI를 넣지 않는다. Safe Area/CanvasScaler 1920×1080을 사용한다.

## 제작/설치/상호작용

Recipe는 ID/결과/수량/재료/시설 요구를 가진다. 시설은 현재 None/Workbench/Campfire/Furnace다. 재료 소비와 결과 추가를 한 번에 확정하고 공간 부족이면 전체 취소해 손실을 막는다. UI에 결과 아이콘/이름/보유량·필요량/Craft 가능 여부를 표시한다.

Workbench 선택 시 PlayerAction이 배치로 전환한다. 앞쪽 2.1유닛 Preview, 거리 2.8유닛 제한, Footprint/Collider/필드 경계 검사, 유효/무효 색 표시를 제공한다. 설치 성공 때 해당 슬롯의 Workbench 1개를 소비한다. 큰 오브젝트/캐릭터/기존 설치물과 겹치면 거부한다.

작업대는 살아 있고 중심 거리 2.6유닛 이내이며 바라보는 방향 앞쪽일 때 상호작용 대상이 된다. 제작 권한도 같은 중심 거리/생존 판정을 재사용한다. 멀어지거나 시설이 제거되면 Hand crafting으로 돌아간다. WorldPlaceable/Health 기반은 있으나 회수/수리 도구와 완성된 철거 UX는 없다.

## 설정과 생존

Control Size 80~140%(기본 100%)는 플레이 중 조이스틱/Action/하단 Hotbar/가방에 적용하고 큰 조작 요소의 위치도 조정한다. 생존 HUD/인벤토리 패널 전체를 확대하지 않는다. Master/BGM/SFX 기본 음량은 100/65/85%이며 독립 AudioSource Music/SFX에 Master를 곱한다. 음악 곡은 없으며 기존 전자음은 제거했고 SFX/BGM 에셋은 비워두었다. 로컬 사용자 PlayerPrefs는 월드 저장과 분리한다.

Hunger/Thirst Max 100, 초당 0.08/0.12 감소, 각 0 상태에서 5초마다 HP 2 피해다. Inventory는 세계와 생존 수치를 계속 진행시킨다. Singleplayer Pause/Defeat는 별도 정책 컴포넌트가 동결하고 Multiplayer Pause/로컬 Defeat는 전역 동결을 만들지 않는다. 실제 멀티플레이를 제공한다는 뜻이 아니다.

상단 왼쪽에 음식/물방울 아이콘과 게이지·수치, 머리 위 HP를 표시하고 재료 수량 HUD는 없다. 소비품을 Hotbar에서 선택해 Use하면 1개를 소비한다. Raw Meat +8 Hunger, Cooked Meat +35 Hunger, Water +30 Thirst. 해당 수치가 Max이면 소비하지 않는다. 부작용/버프는 없다.

물은 재사용 WaterSource/WaterSourceDefinition과 WaterSource.prefab을 사용한다. 물가에서 한 회분 아이템을 얻는 방식으로 용기/오염/정수를 구현하지 않았다. 현재 단일 중립 멧돼지(HP20, Raw Meat 2 드랍)는 닭과 중립 이동·근접/피격 도망을 공유하며 적 AI는 없다. 한정 공급이며 무한 식량 생태는 없다.

Hand에서 Wood4+Stone3 → Campfire, 공통 배치/시설 구조를 재사용한다. Campfire에서 Raw Meat1 → 5초 → Cooked Meat1. 조리 재료는 시작 시 보관하며 Take로 결과를 인벤토리에 넣는다. 공간 부족이면 결과가 시설에 남고, 파괴 시 조리 중 원재료 또는 완성 결과를 월드 드랍으로 보존한다. 동시에 한 작업만 진행한다. 고급 시설을 실제 생성하지 않았다.

## 현재 추가 진행

EquipmentTier의 Rank1 Wood→Rank2 Stone→Rank3 Iron, ToolKind/ResourceDefinition 채굴 요구로 구분한다. Stone은 Wood보다 해당 역할의 피해/사용 속도가 개선되고 Iron은 Stone보다 개선된다. Iron Ore는 Pickaxe와 최소 Stone Rank를 함께 요구하므로 맨손/Wood/다른 종류로는 채굴하지 못한다. 원래 Health/피격 바/드랍/획득을 재사용한다.

Furnace는 Workbench에서 Stone8+Wood4로 제작하고 기존 배치를 사용한다. Ore1→6초→Ingot1, 현재 연료 없이 한 작업만 수행한다. 조리와 같은 StationProcessing의 예약·진행·결과 보관을 공유하되 StationKind.Furnace/ProcessingKind.Smelting 데이터를 분리한다. Iron 도구·검은 Workbench에서 각 Wood3+Ingot3이다.

벽 Wood2/HP100, 문 Wood3/HP80, 상자 Wood6/HP70은 모두 Workbench 제작이다. 벽/문은 플레이어 Facing 축에 수직으로 배치되며 닫힌 문은 충돌하고 열면 통과한다. 닫는 영역에 배우/다른 물체가 있으면 거부한다. Zombie가 막힌 벽/문을 공격해 파괴할 수 있다. 상자는 각18칸 독립 모델이며 플레이어36칸과 원자적 이동/합치기/교환한다. 상자 UI는 월드를 멈추지 않고 파괴 시 내용물을 드랍한다. 슬롯 분할/잠금/회수/수리는 없다.

세계 시간은 8분 주기(Day240/Evening60/Night150/Dawn30). URP 2D Lit 월드 재질과 Global Light2D의 부드러운 조명 변화, 모닥불/용광로의 국소 따뜻한 빛을 사용한다. UI는 조명 영향을 받지 않는다. 밤의 색은 낮은 채도의 중립 자연색이며 파란 필터가 아니다. 작은 Day/Phase HUD만 표시한다. 시간 변화로 생존 수치/물품을 리셋하지 않는다.

낮 생성 간격90초/활성상한1, 밤15초/상한6. 외곽 WorldSpawnRegion에서 카메라 밖5% 여유·플레이어12유닛 이상·건축물5유닛 이상·충돌 없는 후보만 사용한다. 밤에는 같은 Zombie가 원거리에서도 플레이어를 추적한다. 새 종류/복잡한 Wave/월드 생성은 없다. 낮에 이미 있는 적을 강제 소멸시키지 않는다. 복잡한 미로 길찾기는 없다.

닭1과 멧돼지1은 NeutralAnimal의 느린 이동·근접/피격 시 짧은 도망을 사용한다. 공격하지 않으며 공통 Health/ResourceNode 드랍을 공유한다. 닭HP20/Raw1, 멧돼지HP20/Raw2다. ArmorDefinition/Category/PlayerEquipment의 한 슬롯·방어 피해 감소·WornSprite 소켓만 준비했고 실제 Armor 아이템은 없다. 허기/갈증 피해는 방어를 무시한다.

## 이번에 제공하지 않는 것

새 적/보스/바이옴/던전/절차 월드/전체 방어구 티어/스킬트리/고급 제작대/포탑/실제 멀티플레이/저장·이어하기를 추가하지 않았다. 드래그 반 스택/아이템 버리기/도구 내구도/정식 방어구 장착 UI는 없다. 재미/세부 밸런스/실기 조작감은 사용자가 직접 판단한다.
