# Resource Tempo / Workbench I–II Economy

2026-10-18. 기준 remote main `71f18ad601908670be9382b76567eeb980409ef0`. Item86/Recipe71, **Save9 / Protocol11**, generation1/2 유지. 구현값은 초기 조정값이며 사람의 blind exploration 속도와 출시 밸런스 합격을 뜻하지 않는다.

## 변경 전 실제 감사

|Source|HP|완료 수율 전→후|회복 전→후 (게임 초)|최소 도구|
|---|---:|---:|---|---|
|Tree|150|Wood6→6|300→300|맨손 가능|
|Fallen Log|50|Wood4→4|480→480|맨손 가능|
|Stone|50|3→6|600→75|맨손 가능 / Pickaxe 권장|
|Iron|75|3→6|v2 비재생, legacy720 →120|Stone Pickaxe|
|Copper|90|4→6|v2 비재생, legacy600 →150|Iron Pickaxe|
|Tin|75→50|3→5|v2 비재생, legacy720 →165|Prospecting Pan only|
|Coal|90|4→10|v2 비재생, legacy600 →105|Stone Pickaxe|
|Small Plant|12|Fiber2→2|90→90|맨손|
|Berry|18|3→3|180→180|맨손|
|Mushroom|12|2→2|240→240|맨손|

모든 source는 hit마다 지급하지 않고 파괴 완료 때 한 번 지급한다. Tin은 Pan25로 3회→2회 완료다. 광물별 기존 환경/채굴 도구 gate를 유지했다. Iron/Copper/Coal 지상·동굴, Tin은 main channel의 실제 RiverGravel/수변 조건이며 Wetlands 명칭만으로 생성되지 않는다. 자연 존재는 Workbench 해금과 무관하다.

## 예산 기준

필수 개인: Steel Axe/Pickaxe/Sword/Head/Body의 전체 선행 장비 chain, Pan, Compass, Empty Cup 각1. 공유: Workbench1+II upgrade1, Campfire1, Furnace1, Blast Furnace1, Chest1. 개인 장비의 선행 소비를 생략하지 않는다.

선택/작은 건축 여유: 기존 recipe의 ProgressionPerPlayer/Shared 값. 개인 Reinforced Bow/Crossbow/Lantern, Bronze Arrow24/Steel Bolt16. 공유 Pot1, 강화 Wall6/Door1/Gate1, Trap1/Brazier1. 그 외 무제한 건축·음식·사고 손실은 포함하지 않고 추가1.5 안전 여유를 별도로 둔다.

ProgressionEconomy는 EssentialPerPlayer/EssentialShared와 선택 root metadata, 실제 Ingredient/ResultCount, Placeable UpgradeCosts를 사용한다. 공유 시설은 한 번, 개인은 인원수만큼. Bronze3 제작 잔량을 다음 요청에 넘겨 batch 올림을 중복 청구하지 않는다. 향후 tier 추가 시 recipe root metadata와 station upgrade data를 연결한다. rank4 하드코딩 비용 계산은 제거했다.

|원재료|공유|개인 1명|4명 개인 합|1P 필수|4P 필수|1P 선택 포함|4P 선택 포함|4P ×1.5 (올림)|
|---|---:|---:|---:|---:|---:|---:|---:|---:|
|wood|32|25|100|57|132|98|230|345|
|stone|25|13|52|38|77|76|115|173|
|iron_ore|5|30|120|35|125|39|135|203|
|coal|3|17|68|20|71|50|140|210|
|copper_ore|2|2|8|4|10|20|44|66|
|tin_ore|1|0|0|1|1|9|18|27|
|leather|0|5|20|5|20|9|36|54|
|plant_fiber|0|0|0|0|0|4|16|24|
|feather|0|0|0|0|0|4|16|24|

중간재(선택 포함 실제 생산):

|인원|Iron Ingot|Copper Ingot|Tin Ingot|Bronze|Steel|
|---|---:|---:|---:|---:|---:|
|1|39|20|9|27|17|
|4|135|44|18|54|68|

개인 필수 Iron30 = Iron 장비14 + Steel15 + Compass1. 공유5 = WB upgrade2 + Blast Furnace3. 4P 필수 Coal71 = 개인68 + 공유3. 선택 포함 Coal140. Iron Ore1→Iron Ingot1(2초), Copper/Tin Ore1+Coal1→Ingot1(3초), Copper2+Tin1→Bronze3(5초), Iron1+Coal1→Steel1(6초)를 유지한다. 기존 저장의 이미 예약된 processing cargo도 같은 비용/결과로 해석된다.

## 완전 채집할 source 수

자원 공유/잔량을 모아 사용하는 계산이다. 이동·실수·전투·처리 대기는 아래 수율 비율에 포함되지 않는다.

|자원|1P 전→후|4P 전→후|수율 개선 배수|
|---|---:|---:|---:|
|stone|26→13|39→20|2.00×|
|iron_ore|13→7|45→23|2.00×|
|coal|13→5|35→14|2.50×|
|copper_ore|5→4|11→8|1.50×|
|tin_ore|3→2|6→4|1.67×|

4인 필수만 계산하면 Iron42→21, Coal18→8 지점이다. Tin의 수율/타수 개선을 함께 보면 hit당 지급량은 1→2.5다. 전체 실플레이 시간을 2.5배 단축했다고 주장하지 않는다.

## 최종 지역 회복

- 대륙형64unit chunk를 population region으로 사용한다. 최초 deterministic baseline을 세고, 저장된 c/g/e tombstone 및 살아 있는 p ID를 적용한다. 광물·Stone 목표는 초기 개수×0.85 올림. 빈 지역에 광물을 임의로 심지 않는다. 기존 식생의 설정은 유지한다.
- 원본 ID는 영구 tombstone, 회복 source는 새 전역 serial p ID다. 회복 p를 다시 캐면 해당 p 상태를 제거하고 지역 deficit만 남겨 반복 채집으로 tombstone이 무한 증가하지 않게 한다.
- 부족한 (rule,chunk)당 ticket 하나. 한 보충 뒤 다시 delay를 적용한다. timer마다 모든 원래 노드를 일괄 복원하지 않는다.
- 중앙 scheduler5초, 활성 청크×rule의 indexed ticket 조회, 회전 cursor로 최대16ticket/최대2source, 후보는 ticket당 최대48. 먼 지역의 누적 ticket을 순회하지 않아 오래된 save의 휴면 backlog가 현재 지역을 굶기지 않는다.
- 후보는 같은 청크의 seed/cursor 기반 위치. SourceAllowed 환경/통행/길/물, 최근 채집 제외(Stone/광물 delay×2; radius6, Tin4), 공간 overlap/건물/밀폐 공간/플레이어18unit/카메라를 검사한다. 실제 물리 spatial index를 쓰며 전체 live resource pair search는 하지 않는다.
- 막힌 후보는 대기한다. 건축물을 통과해 강제로 생성하지 않는다. same-coordinate(+3,+4) 복원 경로는 제거했다.
- SP와 Host가 같은 ResourceRegeneration/Population을 실행한다. Client가 회복을 결정하지 않는다. delta NGO spawn/state만 사용하고 full-world broadcast를 추가하지 않았다.

## 저장과 생성 호환

Save9의 기존 ResourceSnapshot/Recovery.Pending/Harvests/Serial/CandidateCursor 형식을 그대로 사용한다. 새 save schema가 필요하지 않아 version을 올리지 않았다. Protocol11은 pickup owner feedback RPC와 현재 content policy를 구분한다. 사용자 저장을 이관 fixture로 쓰지 않는다.

새 저장의 pending Due는 게임 시간 기준으로 그대로 보존하고 오프라인 시간을 더하지 않는다. sleep region은 실제 spawn하지 않고 reactivation 때 deficit를 재구성한다. 기존 v2 광물 tombstone에 지역 ticket이 없으면 활성화 때 새 delay부터 예약한다. 이전 개별 RespawnAt은 구 save 읽기 필드로 남지만 신규 대륙 회복을 작동시키지 않는다. old p source도 기존 ID/위치로 복원한다.

현재 수율을 g/c/e 기본 배치 계산에 직접 사용하면 기존 save 자원 위치/ID가 바뀌므로 immutable LayoutSourceCount/LayoutSupplementCount 및 VegetationStream을 분리했다. 이것은 저장 호환용 배치 baseline이며 현재 진행 예산은 recipe graph가 계산한다. 새 요구량이 baseline보다 커지면 e 보충만 뒤에 추가한다. 4seed의 기존1154 entry에서 누락/위치 변경0. generation1 지형·r ID도 재생성/재배치하지 않는다.

## 환경과 추가 공급

Iron: 초기 소량/Surface Rocky/Cave, Skeleton 75% 1–2 Ore. Coal: exposed seam/Rocky/Cave. Copper: RockFace/Rocky/Cave, Pan 기술용. Tin: 실제 main-channel RiverGravel, Pan 전용. Mine/Quarry는 현재 landmark뿐이며 보상 광석 상자나 merchant가 아니다. 신규 우회 loot 공급처는 추가하지 않았다.

## Validator

4인 필수+선택 budget×1.5와 accessible guaranteed mineral yield를 비교하고, 광물당3개 이상의128unit cluster 및 spawn에서의 bounded terrain path를 확인한다. 시작3×3청크의 Wood/Stone도 같은 비용 예산을 충족해야 한다. Forest와 첫 Cave의 모든 surface entrance에도 경로 검사를 적용한다. 동적 건축물·모든 자연 랜덤 source·모든 후반 POI의 전역 navigation 증명은 아니다.

## Closure / 조사 / 실제 검수

[86 item Source–Sink](RESOURCE_ECONOMY_AUDIT.md), [출시 게임 조사](RESOURCE_TEMPO_RESEARCH.md), [실제 실행 결과와 제한](RESOURCE_TEMPO_REVIEW.md)를 따른다. WBIII/Diamond/Defense Workshop/자동화/boss/새 전투·navigation/UI redesign은 없다.
