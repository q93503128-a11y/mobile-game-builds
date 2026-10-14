# Seed 기반 월드 생성1차

WorldGenerationDefinition/TemperateBiome와 ResourceGenerationRule이 정본이다. 현재192×144유닛 유한 월드, 기본Seed9472026. WorldGenerator.SelectedSeed로 같은 Seed를 다시 선택할 수 있고 RandomSeed/사망 후 Try again은 무작위 새 월드를 만든다. 같은Seed는 기본 지형·물·Road·자원 위치/스케일을 재현한다. 동물 이동·적 Encounter는 기본 생성 재현 보장과 별도다. 변경된 자원/시설/상자/생존 상태는 Save/Load v1 Delta로 복원한다.

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

ResourceIdentity는 안정적인 r<index> ID를 부여하고 손상/파괴/재생 상태만 저장한다. Tree/Log300초, Stone1200초, Ore2400초 뒤 주변6~26유닛의 적합한 Land를 검사한다. Camera 안/Player12유닛 안/물/길/충돌/기지 내부 후보는 제외한다. 막히면 다음 시도까지 대기하며 즉시 같은 좌표에 솟지 않는다. 작은 풀은 비충돌 장식이다. 별도의 작은 관목36개는 HP12/Wood1의 비차단 자원이며90초 재생을 사용한다. Sapling/Young Tree 성장은 향후 콘텐츠다. 닭/멧돼지 각1 Population Slot은180/300초 후 적합한 화면 밖 지역에 재생한다. 재생 Timer와 지역 Slot도 저장한다. 설치·상자·처리·시간·XP/Gold는 Save/Load v1에 포함한다. Animal Breeding은 아직 없다. 첫 생성은 동기식이며 모바일 로딩/성능 프로파일링은 다음 검증이 필요하다. 광범위한 물리 NavMesh/복잡한 기지 길찾기/임의 형태 기지의 완전한 내부 판정은 없다. 여러Biome/던전/POI/최종 World Generator도 아니다.

Shallow는 WorldNavigation의 Terrain Movement Modifier0.7을 사용하고 Deep은 도보 이동을 막는다. Boat는 향후 Vehicle별 이동 판정으로 확장할 방향이며 실제 탑승/수상 이동은 없다.
