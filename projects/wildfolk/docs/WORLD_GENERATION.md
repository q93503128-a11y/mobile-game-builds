# Seed 기반 월드 생성1차

WorldGenerationDefinition/TemperateBiome와 ResourceGenerationRule이 정본이다. 현재192×144유닛 유한 월드, 기본Seed9472026. WorldGenerator.SelectedSeed로 같은 Seed를 다시 선택할 수 있고 RandomSeed/사망 후 Try again은 무작위 새 월드를 만든다. 같은Seed는 기본 지형·물·Road·자원 위치/스케일을 재현한다. 동물 이동·적 Encounter는 기본 생성 재현 보장과 별도다. 변경된 자원/시설/상자/생존 상태는 Save/Load v4(v1~v3 호환) Delta로 복원한다.

WorldMap은 System.Random(seed)만 사용하여 생성하며 UnityEngine.Random 상태를 바꾸지 않는다. Perlin 기반 숲/돌 밀도는 하나의 Temperate 내부 변화다. WorldGenerator는 런타임 초기화 시 생성 데이터를 프리팹에 연결한다. WorldTerrainRenderer가 하나의1024×768 RGB 텍스처로 자연색 지면/길/물/수변을 표시한다. 명확한 사각 타일·체커 경계를 만들지 않는다. 무한 Chunk Streaming은 없다.

강은 남북을 잇는 곡선이며 폭6유닛이고 Seed로 굽음/위치가 변한다. 강 y=-27.36 부근에 폭4.4유닛의 통과 여울을 제공한다. 주 호수는 서북 지역에서 Seed로 중심/크기가 달라지고 기복 있는 폐곡선으로 표시한다. 시작점 약11유닛의 작은 연못은 초기 물 접근을 보장한다. 물 표현과 깊은 물 Collider는 같은 WorldMap 경계 계산을 사용한다. 여울은 물이지만 이동 가능하다. 수변의 여러 WaterSource는 기존 수집 규칙을 사용한다.

Road2줄은 월드 일부와 여울을 연결하는 기본 Dirt Path다. IsRoad/RoadDistance로 실제 판별 가능하며 낮 Goblin의 후보 위치에 사용한다. 마을/POI 연결망은 아직 없다.

| 자원 | 총 개수 | 시작 근처 보장 | 시작 회피 반경 | 최소 배치 간격 |
|---|---:|---:|---:|---:|
| Tree |140|4(±8 범위)|3.5|4|
| Stone |55|3(±9 범위)|3.5|3.4|
| Iron Ore |16|0|18|4|
| Fallen Log |12|3(±8 범위)|3.5|3.2|
| Small Plant |36|0|3.5|2.4|

시작 자원을 먼저 모든 Rule에 대해 배치하고 나머지를 채운다. 물/확장 수변/길/Spawn 회피·종류 간 큰 최소 간격을 검사한다. Count를 채울 수 없는 설정은 실패를 숨기지 않는다. 자원 이름별 런타임 분기가 없고 Prefab/Count/StartCount/Radius/Spacing/밀도선호/Scale/선택 대체Sprite를 Rule로 관리한다.

Player Spawn은(0,0)이며 물·광석·큰 자원에 갇히지 않는다. 장식 최대480개 중 첫40개는 시작 화면 주변에 분포한다. Grass/Fern은 Collider가 없고 항상 캐릭터 아래다. 큰 FallenLog는 BoxCollider/Resource Health/Wood 드랍을 가진다. Tree/Stone 변형은 기존 프로젝트 아트다.

정렬은 WorldSorting의 Ground→GroundDecoration→Drop→Y 기반 Actor/TallObject→Overlay를 사용한다. 캐릭터/나무/시설은 BaseY의SortingGroup 기준이며 자원마다 임의 깊이 숫자를 박지 않는다. 작은 풀은 Player 위를 덮지 않는다.

자원 회복은 ResourcePopulation과 ResourceRegeneration이 담당한다. 기본 r<index>는 손상/파괴 Delta이고, 파괴된 ID를 부활시키지 않는다. 회복 개체는 단조 증가하는 p<serial> ID로 다른 위치에 생성한다. 4×3 논리 Region의 종류별 Population과 최초 Seed 분포 기반 TargetDensityRatio0.9를 비교한다. 이 Region 경계는 화면에 타일로 표시하지 않는다. 현재 수량이 Target보다 부족한 Region만 대기 후 한 개체씩 회복한다. 초기 Count140/55/16/12/36은 최초 생성 값이며 항상 같은 총량을 복원하는 Runtime 목표가 아니다.

Tree300/Log480/Stone600/Ore720/Small Plant90초는 초기 밀도 회복 대기값이다. 흔한 자원은 해당 Region 전체에서 후보를 찾고, Rare Mineral은 전체 월드 후보를 사용한다. 같은 좌표·6~26유닛 주변을 중심으로 다시 생성하지 않는다. Camera/Player(흔한12, Rare24)/물·수변/길/충돌·기지 내부/종류별 ClusterSpacing과 최근 Harvest 제외를 검사한다. 흔한 Harvest 제외8유닛/대기시간×2, Rare28유닛/1800초 및 Cluster 간격12유닛이다. 5초마다 최대2개 회복 시도, 최초 Count 합의1.25배는 성능용 Hard Limit이다. Soft Target이 실제 의사결정 기준이며 막힌 후보는 다음 Tick까지 대기한다.

Family/Rarity/NaturalRecovery/WorldRecoveryScope/TargetDensityRatio/Delay/Harvest Exclusion/ClusterSpacing은 Rule 데이터다. 생존 Recipe·RequiredTool/Tier와 별도 경계다. 미래의 Metal/Ore/Gem/Crystal/Alloy/특수·Biome·Dungeon 재료와 Surface/Cave/POI 공급처를 확장할 방향이며 현재 추가 광물·공급처는 없다. Diamond를 최종 Tier로 고정하지 않는다.

기본 손상·파괴 Delta, 살아 있는 회복 개체, Pending Region/시간, 최근 Harvest/만료, ID Serial과 후보 Cursor를 v3에 저장한다. v2의 파괴 상태는 유지하고 Region 부족량으로 이관하며 긴 과거 대기는 새 Delay 이내로 제한한다. 오프라인 경과를 더하지 않는다. 오래된 기본 Tombstone은 유지하고 회복 개체의 파괴 Tombstone은 제거해 Delta가 무한히 누적되지 않게 한다.

작은 풀은 비충돌 장식, Small Plant36은 HP12/Wood1의 비차단 자원이다. Sapling/Young Tree 성장은 없다. Chicken/Boar와 신규3종은 종별 지역 Population/Group 보충과 저장을 사용한다. Animal Breeding은 없다. 첫 월드 생성은 동기식이며 Android 로딩/성능은 실기 검증 대상이다. 임의 기지 내부의 완전한 판정, 전체 NavMesh, 여러 Biome/던전/POI/무한 World Generator는 아직 없다.

Shallow는 WorldNavigation의 Terrain Movement Modifier0.7을 사용하고 Deep은 도보 이동을 막는다. Boat는 향후 Vehicle별 이동 판정으로 확장할 방향이며 실제 탑승/수상 이동은 없다.


## 동물·Goblin Population

AnimalPopulationDefinition의 Rule 순서는 기존 Chicken/Boar 인덱스를 유지하고 Rabbit/Deer/Sheep을 추가한다. Creature Habitat를 논리 지역별로 샘플링해 종별 목표를 분배한다. ForestDensity<0.52 Grassland,≥0.6 Forest,0.36~0.66 ForestEdge이며 실제 새 Biome가 아니다. 적합한 육지/비물/비Road/장애물·기지 내부 제외/Player14/Base8 조건을 사용한다. 초기46목표/Hard Cap80이다.

초기 배치와 보충은 Group 규모를 사용한다. 집단은 지역 Soft Target을 일부 넘을 수 있지만 종별 총목표와 Hard Cap은 넘지 않는다. 죽은 개체 ID를 다시 사용하지 않고 살아 있는 동물만 Capture한다. 종/Region의 부족 타이머,ID Serial/Group Serial/Candidate Cursor를 별도 저장한다. v3의 살아 있는 개체는 위치/HP/ID/Home Anchor를 유지하고 죽은 Slot은 부족 지역의 대기로 이관한다. 새로운 종 부족량은 Load 후 보충 일정으로 등록한다.

작은 Camp 하나는 Seed 후보와 Player18/Base12·장애물 회피로 배치한다. 거주 개체3은 Group/Camp ID를 공유하며 죽은 개체의 원래 지점/ID를 부활시키지 않는다.480초 후 적합한 Camp 주변·카메라 밖에서 새 개체를 보충한다. 로밍 Warband는 낮 Road 후보에서3~5명(새총0~2)으로 생성한다. Camp/로밍 구분은 Camp ID와 Group/Traits 데이터다. 실제 Scout/Village/Fortress는 없다.

Night 예산은 Zombie와 Skeleton이 공유한다. 낮/밤·카메라·빛·시설·Player 회피는 기존 규칙을 유지한다. 기존 살아 있는 Night 종/검 Variant는 v4 저장에서 복원하며 순간 공격 상태와 투사체는 저장하지 않는다. 재미·생태 밀도·장시간 모바일 성능은 별도 사용자 확인이다.
