# Lowfire

Unity **6000.3.19f1 LTS / Universal 2D**, 모바일 Landscape(1920×1080) 탑다운 생존 게임이다. 표시명 Lowfire, 내부 Wildfolk namespace·경로·저장 식별자는 유지한다. 현재 alpha 콘텐츠는 **Item123 / Recipe100 / Save11 / Protocol14**, Workbench I→II→III와 Wood/Stone/Iron/Steel/Diamond 장비다.

## 실행과 플레이

Unity Hub에서 `client/`를 열고 `Assets/Wildfolk/Scenes/MainMenu.unity`에서 Play한다. Continue는 검증된 저장, Single Player는 새 무작위 Seed 월드다. Gameplay 씬 직접 실행은 기본 Seed9472026이다. 신규 generation3는13120×13120유닛 유한 대륙, Grassland/Forest/Wetlands/Rocky와 Caverns를 제공한다. 기존 generation1/2의 지형과 자원 ID는 보존한다.

낮 채집·도구/시설 제작 → Copper/Tin/Bronze/Steel → Deep Caverns Diamond/Crystal → III 장비·탐험·방어로 이어진다. 밤 Zombie/Skeleton, 낮 Goblin 무리·Camp, 동물·야생 위험 생물13종을 제공한다. 신규8종은 Bat/Stone Crawler/Bog Leech/Wolf/Mountain Goat/Crow/Giant Boar/Crystal Crawler다. Bone/Chitin/Medicinal Gland는 탄약·함정·장비·치료·음식으로 소비된다. Goblin Camp 보급품은 거주 적 처치 후 획득하는 비재충전6칸 상자다.

Primary는 공격·도구·설치·선택 소비품·Armor 장착, Interact는 시설·문·물·상자다. Joystick/Sprint/Dash, Inventory36(Hotbar9+Storage27), 일반 Chest24/강화36칸, 드래그·합치기·소분·명시적 버리기를 제공한다. Inventory/제작/보급품 화면에서는 플레이 입력만 막고 세계는 진행한다. Singleplayer Pause/Defeat는 정지한다.

기본 Workbench는 Wood6. 장비 업그레이드는 이전 장비를 재료로 소비한다. 설치 제작대 I→II는 Wood8+Stone6+Iron2, II→III는 Wood10+Stone10+Bronze4+Steel4+Crystal4+Diamond1이다. 별도 II/III Recipe는 없다. Furnace는 Iron/Copper/Tin/Bronze, Blast Furnace는 Steel, Campfire는 단순 가열·정수, Cooking Pot은 복합 조리, Turret Workbench는 포탑·탄약·모듈을 맡는다. 전체 비용/시간은 제작 정본을 따른다.

Telescope는 원거리 표시만 요청하며 simulation 범위를 늘리지 않는다. Compass/Home/동굴 Exit/선택 Beacon 안내, Torch와 Utility Lantern/Crystal Lantern을 제공한다. Ballista/Repeater는 실제 호환 탄약을 소비하며 모듈로 장전·조준·보관·회전·내구를 조합한다.

Hunger/Thirst, 재사용 Cup/Waterskin, 낚시, HP/Stamina 즉시 회복 음식·연고를 제공한다. 치료 효과가 있는 새 소모품은 공용4초 간격을 사용한다. 성장은 월드 캐릭터의 Level/4 Node Skill이며 영구 계정 성장과 구분한다.

## 저장과 협동

Save11은 기존1~10을 읽고 다음 정상 Save에서 이관한다. Seed·generation version·game time·자원/생물 logical delta·시설·queue·가방·장비·성장·Camp 보급 잔량을 저장한다. Editor는 프로젝트 UserData, 빌드는 persistentDataPath, 격리 QA는 전용 Captures 경로를 쓴다. 원래 파일명과 checksum/atomic replace/backup을 유지한다. Home Continue/Pause Save·Save & return home, 기기 Background/Quit 저장을 제공하며 플레이어용 Load 버튼은 없다.

Multiplayer는 Relay Room Code 또는 Local Network Host/Join이다. Host가 이동·전투·spawn/AI·채집·loot·가방·시설·상자·save를 결정한다. Client는 요청과 replica 표시다. 최대4인 admission 구조를 유지하며 실제4인 Relay·모바일·장시간 통합 검수는 별도다. Host 종료 시 세션도 끝나며 Host migration/Account/Cloud는 없다.

## 정본·개발·검증

- 작업 전 [AGENTS](AGENTS.md)를 읽는다. main 직접 작업, canonical source 수정, PR/임시 branch 금지.
- [기획](docs/DESIGN_BIBLE.md), [콘텐츠/전체100 Recipe](docs/CONTENT_EXPANSION.md), [생물·드랍](docs/CREATURES_AND_LOOT.md), [제작](docs/CRAFTING.md), [음식](docs/FOOD_AND_FISHING.md), [수치](docs/BALANCE.md), [월드](docs/WORLD_GENERATION.md), [저장](docs/SAVE_PERSISTENCE.md), [구조](docs/ARCHITECTURE.md).
- [이번 외부 비교·구현·경제·검수](validation/creature-loot-expansion/REVIEW.md). 과거 validation/review는 당시 기록이다.
- 소스 검사: `python tests/validate_visual_project.py client`. Unity EditMode/PlayMode는 Assets/Wildfolk/Tests. 자동 검사와 실제 Play 검수는 구분한다.
- 신규 생물/아이콘은 기존 Pillow 방식의 `tools/generate_creature_loot_art.py` 원본이다. 기존 art 출처는 [SOURCE_ART](docs/SOURCE_ART.md). 외부 게임 asset/code/layout을 복제하지 않는다.
- Unity MCP/code는 허용한다. Windows 입력과 Focus/Show는 금지한다. View 메뉴는 기존 Game View만 Repaint하고 PlayUnfocused를 적용한다. 씬/에셋 재생성은 수작업 변경을 먼저 확인한다.

실제 터치·손맛·재미·장시간 경제·Android 성능은 사용자 검증 대상이다. BGM/SFX 콘텐츠는 비어 있다. Boss/Undead King/Defense Objective/IV~VI/전기/총기/직업/Pet/번식/상인/영구 Skin은 이번에 추가하지 않았다.

약초는 Forest/Wetlands에서 맨손으로 채집한다. 응급 붕대는 Hand에서 Fiber2+Herb1→20HP/8초, 기존 회복 연고는 Gland1+Herb1+Fiber2→즉시30HP다. 공용4초·붕대 비중첩, 자연회복/음식·기존 Station은 유지한다. [의약 조사·경제·검수](validation/medicine-expansion/REVIEW.md). 다음 예정은 Food Expansion2-B다.
