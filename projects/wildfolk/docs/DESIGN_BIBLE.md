# Lowfire 기획 정본

2026-10-20. 최신 canonical 코드·데이터를 우선한다. Unity6000.3.19f1 LTS / Universal2D, 모바일 Landscape1920×1080. 플레이어 표시명 Lowfire, 내부 Wildfolk 식별자·저장 경로는 유지한다.

## 방향

낮 채집·탐험·생존·제작·기지와 밤 방어를 잇는 탑다운 생존 게임이다. Mine Survival 등의 공개된 흐름을 참고하되 고유 아트·코드·UI·비공개 수치를 복제하지 않는다. 자연색·무광·절제된 외곽선/그림자, 원형 플레이어·작은 눈·양손을 유지한다. 현재 아트와 밸런스는 alpha다.

## 현재 플레이 범위

큰 유한 대륙과4개 Surface Biome, 별도 Caverns/Deep Caverns, Seed·지형/수계/자원/POI 독립 stream, 가까운64unit 청크 simulation을 제공한다. 기존 generation1/2를 보존하고 신규 generation3에서 III 자원을 공급한다. 자원·생물은 지역 부족량을 안전한 새 위치·ID로 보충한다. 오프라인 시간을 재생에 더하지 않는다.

Workbench I→II→III가 progression spine이다. Wood/Stone/Iron/Steel/Diamond, Copper/Tin/Bronze 부품, Furnace/Blast Furnace, Campfire/Cooking Pot, 실제 수요8개를 가진 Turret Workbench를 제공한다. IV~VI는 구조적 상한이며 빈 tier 진입을 만들지 않는다. 전체121 Item/99 Recipe는 CONTENT_EXPANSION을 따른다.

13종 야생 생물과 Zombie/Skeleton/검 Variant·Goblin/새총 Goblin을 제공한다. Family/Faction/Trait/Habitat와 행동 데이터를 조합한다. 신규8종은 비행·위장 해제·물가 매복·무리 합류·hop/도주·돌진·결정 pulse를 구분한다. Bone/Chitin/Medicinal Gland는 실제 서로 다른 sink에 연결하고 종별 Trophy/중복 Fur/Hide를 만들지 않는다. Camp 전투→확보→일회 보급품 획득을 제공하며 진행 핵심을 RNG 상자에 의존시키지 않는다.

Hunger/Thirst, 물 용기 상태·정수, 낚시, 음식·의약품 즉시 HP/Stamina 회복을 제공한다. 복잡한 timed buff stack은 없다. 성장은 Level1~20과4개 Skill이며 Armor는 Head/Body와 공용 장비 교환을 사용한다. Gold는 세션/월드 캐릭터 재화이고 아직 소비처가 없다. Meta Coin/영구 계정 성장은 없다.

## 조작·UI·장비

Primary는 맨손/장비 공격·채집·설치·선택 소비품·Armor, Interact는 시설/상자/문/물이다. Inventory36/Hotbar9, 일반 Chest24/강화36, 드래그·소분·명시적 Drop을 제공한다. UI 밖 무작위 버리기는 취소한다. 모달은 Safe Area와 뒤 입력 차단을 사용한다. Inventory/제작/보급품은 세계 진행을 유지한다. Pause/Defeat는 싱글만 정지한다.

활·쇠뇌·검·창, 실제 호환 탄약과 Ballista/Repeater, 기능별5종 모듈이 있다. Telescope는 원거리 표시와 simulation을 분리한다. Torch·Lantern/Crystal Lantern·Compass/Home/동굴 Exit·한 개 선택 Beacon 안내를 제공한다. 상세 수치의 정본은 BALANCE와 에셋이다.

## 저장·협동

Save11(1~10 이관)/Protocol13. Host만 spawn/AI/목표/피해/사망/loot roll/보급 claim/보충/저장을 결정한다. Client는 입력 요청과 replica 표시를 한다. Relay/명시적 Local Network Host/Join, 최대4인 구조, 로컬 식별자별 가방·성장·장비와 공유 월드를 저장한다. Host migration·dedicated server·인증 계정·Cloud는 없다. 실제 원격4인/단말 성능·장기 생존·재미는 별도 검수다.

## 후속

이번 Creature/Loot/Material/Recipe 패스가 완료·검수된 다음 Undead King/Boss Foundation을 별도 착수한다. Boss framework/Day10 event/HP HUD/IV~VI/대형 새 Biome/Defense Objective/전기/총기/Class/Pet/Skin/Merchant/Farming/Story는 이번 범위가 아니다. 재미·손맛·진행 만족도를 사용자를 대신해 합격 처리하지 않는다. [이번 결정·제한](../validation/creature-loot-expansion/REVIEW.md).
