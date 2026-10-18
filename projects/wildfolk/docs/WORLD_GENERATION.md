# World Generation / Biome Foundation v2

2026-10-18 현재: generation2 지형 유지 / **Save8 / Protocol9**, Item50 / Recipe38. Torch·Compass·제작대 Home·동굴 Exit·제한 지형 cache/분할 생성을 제공한다. [탐험 정본](EXPLORATION_FOUNDATION.md)과 [실제 검수/성능](EXPLORATION_REVIEW.md)를 따른다. 아래 이전 날짜/Version 설명은 각 단계의 기록이다.

2026-10-17. 표시명 Lowfire / 내부 Wildfolk. 신규 BaseGenerationVersion2 / Save7 / Protocol8. Item48, Recipe36, 장비 Tier4 유지.

## 크기 / 생성

실제 MoveSpeed4.2 unit/s, Sprint1.55배, Stamina100/소비14/s를 읽었다. 52분 기본 도보×4.2×60을64단위로 올려 **13120×13120** 유한 영역을 정했다. 실제 대륙은 불규칙한 해안 반경 약5900~6300, 시작지→해안 약23~25분, 해안간 약47~50분 직선 도보다. Sprint/우회/채집을 포함한 실측 탐험 시간은 아니다. Forest anchor 약210unit/50초, Rocky 약650unit/2.6분, main river 약2~3분이다.

Root Seed에서 Continental101 / Terrain211 / Hydrology307 / Biome401 / Cave503 / Resource601 / POI701 / Road809 / Vegetation907 / Creature1009 domain을 deterministic hash로 파생한다. SourceId stable hash/청크 좌표를 추가한다. 광물 후보를 먼저 놓고 식생을 뒤에 놓으므로 식생 후보 변경이 수계/광맥을 함께 이동시키지 않는다.

크기·대륙→연속 환경장/수계→128unit macro biome scoring/작은 component 병합/전이→진행 anchor/동굴→중요·일반 POI→일부 길→보장 공급/한정 후보 보정→검증→청크별 자연 공급·식생·서식지 materialization 순서다. Moisture/Forestation/Rockiness/Height/Roughness/Water Influence를 사용한다. 전체 월드 무한 reroll은 없다. 공급처 최대1200후보, 입구 최대96후보에서 보정하며 필수조건 실패는 명시적 예외다.

## 환경 / 물

SurfaceBiome은 **Grassland / Forest / Wetlands / Rocky** 네 개다. SurfaceEnvironmentDefinition의 여러 field 선호도와 가중 score를 정규화하고 macro fragment를 합친 뒤 bilinear weight로 지면을 연결한다. 128unit macro grid는 화면 타일이 아니다.

- Grassland: 평탄한 시야/기지 후보, Wood·Stone·Plant, 근처 Pond, 별도 StarterSupply Iron3개.
- Forest: 산림도에 따라 수목 밀도 증가. Deep Forest는 같은 Forest 내부 밀도 단계다. 기존 Deer/Boar/Rabbit/Camp를 사용한다.
- Wetlands: 높은 수분·낮은 높이·수계 영향의 조합. River 자체와 다르다. 젖은 땅·수변 식생·퇴적층을 제공한다.
- Rocky: 돌/광맥 증가, 식생 감소, 지층 표현과 거친 ridge 통행 제한, Quarry/Mine/입구 선호.

River는 Hydrology Feature다. main channel/tributary/Lake/Pond/river bank/gravel/shallow/deep water를 제공하고 여러 바이옴을 지난다. 깊은 물은 이동 전 지형 검사로 차단하며 얕은 물은 기존 감속을 사용한다. 약240unit 교차 여울 후보와 주 강/연못/호수 수원을 제공한다. 외곽은 대륙→coast→shallow sea→deep ocean이며 새 월드에 사각 투명벽을 만들지 않는다.

## Caverns

별도 Underground Region이며 SurfaceBiome 추가가 아니다. 면적과18분 도보 권역 밀도에서 현재8 network/12 surface입구를 산출한다. 절반은 두 입구가 같은 network에 연결된다. 12개 비대칭 타원 chamber/corridor,8방 loop,두 심부 dead-end branch가 있다. Entry/Chamber/LargeCavern/OrePocket/WaterChamber/Nest/DeepRuin 구성을 사용한다.

Depth는 가장 가까운 실제 입구에서 graph 경로 거리의 정규화 값이다. Deep Caverns는 다른 ID가 아닌 심부 단계다. 넓은 branch/방 구성, source depth, Skeleton/검 Skeleton encounter, ambient .64→.3 hook으로 차이를 만든다. HP 팽창은 없다. Underground Water는 방 한쪽 pool로 중심 통로를 보존한다. 기존 Interact로 진입/귀환하며 Host가 생존/Running/거리/목적지 지형을 검사한다. 같은 scene의 분리 좌표대에 표시한다. GameplayLight의 ambient/local/exposure만 제공하고 Torch/Lantern item/연료 UX는 없다.

## 자연 공급 / 진행

NaturalSourceDefinition은 환경 tag,Surface·Cave,depth/moisture/rock 범위,확률/밀도/간격,자연 회복,StarterSupply를 가진다. HarvestDefinition은 ResourceDefinition 안에서 tool kind/tier/method/mode를 분리한다. Workbench 해금은 공급처 존재를 바꾸지 않는다.

환경 tag: GrassSurface/ForestFloor/RockFace/ExposedRock/RockRidge/RiverBank/RiverGravel/WetGround/ShallowWaterEdge/CaveFloor/CaveWall/DeepCave/CrystalPocket/UndergroundWater/CoalSeam. UndergroundWater tag는 확장 정의이며 실제 수중 판정은 cave pool geometry다.

Copper는 노출 암석/동굴+Iron Pickaxe 이상,Coal은 seam/동굴+Stone Pickaxe 이상,Iron은 초반 일부와 암석/동굴이다. Tin은 실제 main channel의 적합한 RiverGravel+Prospecting Pan이다. Wetlands 이름만으로 생성하지 않는다. Bronze/별도 Blast Furnace/Steel/장비와 Reinforced Chest36의 기존 흐름을 유지한다.

generation2 Mineral은 모두 비재생이며 파괴 stable ID tombstone을 유지한다. Tree/Plant/Log 등은 게임 시간 뒤 환경 후보에서 새 pID/다른 위치로 회복한다. unload/save/load로 광물을 재생시키지 않는다. generation1만 구 population/recovery를 유지한다.

실제 Catalog recipe를 재귀 계산해4인 Steel 도구·방어구,4인 Pan,공유 Blast Furnace/Workbench upgrade 재료에1.5배 여유를 적용한다. drop yield에 따른 node수,광물별 최소9 node와3개 이상128unit cluster를 검증한다. 시작점→Forest/보장 공급처는 bounded16unit flood+2unit edge/1unit endpoint 지형 검사를 한다. 임의 미래 지역 전체/동적 건축물까지 검증하는 navigation은 아니다.

DiamondPocketSource는 disabled/ResourceRule=-1/depth .8~1의 미래 hook뿐이다. Diamond item/drop/equipment/보장량은 없고 최종 tier로 고정하지 않는다.

## POI / 길 / 생물

도보2.5분/630unit 격자 후보에 환경/통행/물 거리/시작지 제외를 적용한다. Campsite/Ruin/Well/Quarry/Graveyard/Mine/GoblinCamp/CaveEntrance를 제공한다. Camp는 실제 기존 Goblin state에 연결하며 다른 landmark는 시각적 탐험 표시다. 새 보상 상자/퀘스트/건물 실내는 없다. tools/generate_world_art.py는7개 신규 landmark 원본이다.

terrain과 anchor 이후 시작지→Forest/Rocky/여울,일부 중요한 POI 주변 trail을 만든다. 모든 POI를 연결하지 않는다.256unit road index를 기존 낮 Goblin candidate도 사용한다. 모든 trail을 A*로 재작성한 것은 아니다.

기존 CreatureDefinition/habitat/Family/Faction/Traits와 AI를 사용한다. Host만 최초 방문 청크 무리를 결정하고 HP/Home/Group/ID를 sleep state로 보관한다. cap80을 wanted chunk quota로 나눠 특정 지역 독점을 피한다. 기존 야간 cap18/Camp,동굴 encounter marker와 살아 있는 적을 저장한다. 새 종은 없다.

## 청크 / 저장 / 성능

64unit 청크는 약15.2초 도보다. 중앙 scheduler 하나가0.25초에 살아 있는 NetworkPlayer 주변3×3 합집합을 계산한다.1인9개/떨어진4인 최대36개다. 최초 화면은 첫 update에 채우고 이후 frame당1개 대기를 materialize한다. 청크마다 Update는 없다. Client는 local owner 주변 지형만 만들며 자원/AI/시설 state는 Host canonical이다.

128² sample+1texel halo=130² RGB24 texture/청크,세계 좌표 샘플/bilinear 연결이다. 거대한 단일 world texture는 없다. camera-relative Y sorting으로 큰 좌표에서 지면 뒤에 가려지는 문제를 방지한다. 생성/activation spike와 모바일60fps는 실기 profiling 대상이며 달성을 선언하지 않는다.

Host는 가까운 actor/object만 NGO spawn하고 client 청크 interest로 Show/Hide한다. 매 frame 전체 snapshot 전송은 없다. 먼 resource/시설/chest/pickup/creature의 논리 상태는 남긴다. 시설 SimulatedAt과 게임 시간으로 재활성화/저장 때 queue를 계산한다. 오프라인 시간은 더하지 않는다. remote observer hide/show/재접속은 release QA 대상이다.

Save7:Root Seed/BaseGenerationVersion/g·c·p deltas/비재생 tombstone/멀리 있는 시설·상자·아이템/생물·동굴 적·feature state. v1~v6를 읽고 v6이하는 generation1로 고정한다. 구192×144/1024×768/Rule0~7을 재배치하지 않는다. 새 저장 필드와 NetworkRegionTravel/생성 version handshake 때문에 Protocol8이 필요하며 Protocol7 peer와 혼합하지 않는다. 파일명 world-v1.json/multiplayer-world-v5.json 유지.

## 공개 자료에서 취한 원칙

타 게임의 내부 알고리즘을 추정하거나 asset/code/layout을 복제하지 않았다.

- [Terraria 공식 소개](https://store.steampowered.com/app/105600/Terraria/): 무작위 세계의 지하/재료 탐사를 진행에 연결.
- [Minecraft 공식1.18](https://feedback.minecraft.net/hc/en-us/articles/4415128577293-Minecraft-Java-Edition-1-18): simulation distance와 표시 범위를 구분.
- [DST 공식 Cave 안내](https://support.klei.com/hc/en-us/articles/360029557132-Does-Don-t-Starve-Together-have-caves-or-ruins-content): 지상과 동굴의 별도 영역. 별도 server shard는 구현하지 않음.
- [Vintage Story WorldGen](https://wiki.vintagestory.at/Modding:WorldGen_Concept/en): 환경/지질/공급 조건 분리.
- [Core Keeper 공식 소개](https://store.steampowered.com/app/1621690/Core_Keeper/): 지하 환경·위험·재료 발견의 연결.
- [Valheim 공식 FAQ](https://www.valheimgame.com/faq/): 기존 탐험/건축 지역을 보호하는 업데이트 정책.
- [Grounded 공식1.0](https://grounded.obsidian.net/news/grounded/grounded-1-0-release): landmark와 지하/상부 탐험 지역의 구분.
- [Necesse 개발사 소개](https://store.steampowered.com/app/1169040/Necesse/): 무한 절차 탐험과 비교해 큰 유한 대륙 선택.
- [7 Days to Die 개발사 소개](https://store.steampowered.com/app/251570/7_Days_to_Die/): 환경과 POI가 함께 있는 탐험 밀도.

## 검증 / 후속

결과는 [WORLD_FOUNDATION_REVIEW](WORLD_FOUNDATION_REVIEW.md)에 기록한다. fixture의 순간 이동/재료 지급/시간 전진은 자연 진행과 구분한다. 모바일/remote Relay/4인 stress/장시간 탐험·부족도·성능은 미확인이다.

Workbench III/Diamond progression/IV~VI/새 biome·섬·Boat/Torch·Lantern gameplay/지도·Compass·HomeMarker·waypoint·팀 위치는 후속이다. 미래 확장은 additive content/unexplored backfill/외곽 확장/새 지하 layer와 별도 version을 검토하며 기존 base terrain을 자동 교체하지 않는다.

## Exploration Foundation 1 확장 (2026-10-18)

현재 Save8/Protocol9. 위 v2 생성 알고리즘·Seed domain·자원 ID는 바꾸지 않았다. [탐험 정본](EXPLORATION_FOUNDATION.md)에 조명/Compass/Home, 중앙 scheduler의4unit hysteresis, 지형 row 분할2.5ms 목표/LRU24를 기록했다. 안정된1/2/4지역9/18/36, 경계 retention 최대16/player·4인64다. 이전 단락의 매번 texture 폐기/1인9 고정 설명은 이 확장 이전 기록이다. 측정값/단말 한계는 [검수](EXPLORATION_REVIEW.md)를 따른다.
