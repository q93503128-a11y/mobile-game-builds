# Wildfolk

Unity **6000.3.19f1 LTS / Universal 2D**, 모바일 Landscape(기준1920×1080) 탑다운 생존 게임의 초기 플레이 버전이다. 현재는 **Seed 기반 온대 월드 → 채집/기지/장비 업그레이드 → 낮 Road Goblin / 밤 Zombie → 새벽 잠복**의 기반을 제공한다. 최종 월드 생성·아트·밸런스가 아니다.

![새 게임](docs/images/worldgen-start.png)

## 실행과 진행

Unity Hub에서 client/를 열고 Assets/Wildfolk/Scenes/Wildfolk.unity에서 Play한다. 시작 Seed는 기본9472026, 맵192×144유닛이다. 같은 Seed는 같은 지형/물/길/기본 자원 배치를 재현한다. 죽은 뒤 Try again은 무작위 Seed의 빈 새 게임이다. 변경된 자원·시설·상자·재화 상태를 영구 저장하지 않는다.

시작 주변의 큰 쓰러진 통나무(HP50, Wood4) 또는 나무(HP150, Wood6)를 맨손으로 채집하고 드랍을 줍는다. Wood6으로 Hand crafting의 Workbench를 만든다. 선택 시설을 앞쪽 녹색 Preview에 큰 Primary 버튼으로 설치한다. 작은 Interact 버튼으로 시설을 연다. Wood Axe/Pickaxe/Sword는 각각 Wood5다. 이후 이전 장비+Stone→Stone 장비, 이전 Stone 장비+Ingot→Iron 장비를 제작한다. 내구도/자동 진화는 없다.

큰 버튼은 공격/도구/설치/선택 소비품 섭취만, 작은 버튼은 작업대·모닥불·제련로·문·상자·물 수집만 담당한다. 작업대 앞에서도 공격 때문에 UI가 열리지 않는다. 플레이어 공격은 자신의 설치물도 자동 타격하지 않는다. 조이스틱과 Hotbar9칸을 유지하며 가방은36칸(Hotbar9+Storage27)이다.

물가에서 Interact로 Water를 얻는다. 깊은 물은 막히고 강의 여울은 건널 수 있다. Campfire는 Raw1→5초→Cooked1. Furnace는 충분한 재료가 있으면 Ore 최대3개→6초→Ingot 최대3개, Take로 수령한다. 현재 연료는 없다. 상자는 각각24칸이며 위3×8 상자, 아래 플레이어27+9칸이다. 양쪽 드래그/합치기/교환과 Split half를 제공한다. 드래그 중 표시되는 Drop to ground 영역에 놓을 때만 월드 아이템으로 버린다.

가방/상자를 열면 플레이어 입력만 막고 세계는 진행한다. 우상단 Pause/Settings는 Singleplayer를 정지한다. Control Size80~140%, Master/BGM/SFX 설정은 로컬 PlayerPrefs로 보존한다. 실제 네트워크는 없다. 현재 SFX/BGM 에셋은 비어 있으며 음량 채널만 유지한다.

낮 Zombie는0, 밤은 땅에서 올라와 활동하고 새벽엔 추적/공격을 멈추고 잠복한다. 자연 잠복에는 보상이 없다. 낮 Goblin은 드물게 길에 등장하고 깊은 영역 접근/선공에 반응한다. 닭/멧돼지는 중립이며 고기+Feather/Leather를 드랍한다. 적 직접 처치 시 세션 XP/Gold를 별도 지갑에 기록하고 Inventory를 채우지 않는다. 레벨/Skill/Shop/Meta Coin은 없다.

## 정본과 검증

- 실제 아이템24종, Recipe17종, EquipmentTier3종, 실제 Biome1종이다.
- Runtime은 생성 데이터/지형 표시/자원/적 생활주기/입력/모델/UI를 분리한다. Editor는 프리팹·초기 데이터·씬 구성 도구다.
- [기획](docs/DESIGN_BIBLE.md), [월드](docs/WORLD_GENERATION.md), [수치](docs/BALANCE.md), [제작](docs/CRAFTING.md), [인벤토리](docs/INVENTORY.md), [구조](docs/ARCHITECTURE.md), [검수](docs/WORLD_GENERATION_REVIEW.md).
- 이전 *_REVIEW는 당시 단계의 역사 기록이다.
- 소스 검사: python tests/validate_visual_project.py client. Unity Tests는 Assets/Wildfolk/Tests의 EditMode/PlayMode다.
- 아트는 tools/generate_visual_assets.py(Pillow)의 프로젝트 원본이며 외부 게임 에셋을 사용하지 않는다.

Wildfolk > View 메뉴는 기존 Game View의 해상도만 갱신하고 Play Unfocused를 적용한다. Focus/GetWindow를 호출하지 않는다. Build Playable Scene은 씬·프리팹을 재생성하므로 수작업 씬 배치가 필요한 경우 주의한다. 기존 수치 데이터는 보존한다.

실기 터치·손맛·장시간 생존·성능은 사용자가 확인한다. 여러 Biome/POI/Dungeon/무한 Chunk/World Save/실제 Multiplayer/전체 Armor/Skill Tree/전문 제작대는 아직 없다.
