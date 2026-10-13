# 낮밤·기지·장비 진행 검수

2026-10-13, 기준 `011a6bbb4b4c3ce7bbdd7610d5367bd59819eb0a`. Unity 6000.3.19f1 LTS / Universal 2D. 이번 구현은 극초반 진행 기반이며 최종 아트·밸런스가 아니다.

## 현재 구현

- Wood/Stone/Iron Tier 데이터, 실제 아이템 22종과 Recipe 17종. 기존 Wood 수치는 유지했다. Stone/Iron 도구는 채집 효율·사용 속도, 검은 전투 성능을 높였다. 정확한 값은 BALANCE/CRAFTING과 Data 에셋이 정본이다.
- Iron Ore 노드 3개, Stone Tier 이상의 Pickaxe만 채굴 가능. HP/파괴/드랍/줍기 구조를 공유한다. Furnace는 Workbench에서 제작·배치하고 Ore1을 6초 처리하여 Ingot1을 Take로 받는다. 연료는 없다.
- 기존 Cooking을 StationProcessing으로 대체하여 시간 처리·재료 예약·결과 대기·파괴 시 회수를 공유한다. Campfire/Cooking과 Furnace/Smelting은 각각 Station/Recipe 데이터로 구분한다.
- Day240/Evening60/Night150/Dawn30초, URP Global Light 2D와 시설 주변의 작은 따뜻한 조명. WorldSprite 재질도 Sprite-Lit-Default로 바꿔 실제 조명을 받는다. 작은 Day/Phase HUD를 추가했다.
- 기존 Zombie만 사용한다. 낮90초/최대1, 밤15초/최대6. 카메라 밖·플레이어12유닛 밖·시설5유닛 밖의 빈 외곽 위치에 생성한다. 닫힌 벽/문을 공격하고 파괴하며 열린 문을 통과한다. 새벽에 기존 적을 강제로 삭제하지 않는다.
- Wood Wall/Door/Storage Chest는 기존 제작·설치 구조를 사용한다. 벽/문은 0/90도 축 배치, 충돌과 체력이 있다. 문은 Use로 열고 닫으며 닫는 자리에 다른 물체가 있으면 거절한다.
- Chest마다 독립18칸. 위 Chest, 아래 플레이어27+Hotbar9. 기존 드래그로 모델 사이 이동/부분 합치기/교환을 처리하고 데이터 두 쪽을 갱신한 뒤 알림을 보낸다. 상자 화면에서 월드는 계속 움직이며 플레이어 입력만 막는다. 파괴된 상자 내용은 드랍한다.
- Chicken1/Boar1은 공통 중립 이동·도망 AI이며 플레이어를 공격하지 않는다. Raw Meat 드랍은 각각1/2다. 적대 AI와 분리했다.
- Armor category/definition/defense/빈 장비 슬롯/외형 소켓만 마련했다. 실제 Armor 아이템·Recipe·장착 UI는 없다. 일반 피격은 방어 차감 후 최소1, Hunger/Thirst 피해는 방어를 우회한다.
- 기존 전자음 action.wav와 생성기를 삭제했다. 실제 Music/SFX AudioSource 및 Master×채널 볼륨·설정 저장을 유지했다. 현재 연결된 SFX/BGM 에셋은 없으며 무음이다.

## MCP Game View 검수

라이브 Unity의 ScreenCapture로 렌더된 Game View를 캡처하고 직접 읽었다. Windows 마우스/키보드나 Computer Use는 사용하지 않았다. 기존 Game View만 해상도를 바꾸고 Repaint했으며, Play 진입 설정을 Play Unfocused로 수정했다. 창을 활성화하는 호출은 하지 않았다.

시간대·설치물·가득 찬 UI를 빠르게 비교하기 위해 MCP 코드로 Play 중 시간·아이템·시설을 설정한 검수 장면을 사용했다. 일부 UI 캡처에서는 적/스포너를 잠시 껐다. 야간 압박 캡처는 같은 Zombie를 총6개 생성하고 실제 접근을 진행한 장면이다. 따라서 이미지는 긴 생존 플레이의 증거가 아니다. 종료 시 Play를 멈췄고 검수용 상태는 씬에 저장하지 않았다. 저장 씬의 시설 수0, dirty=false를 확인했다.

| 확인 화면 | 이미지 |
|---|---|
| 낮, 시간 HUD, 맨손 시작 | [1920×1080](images/progression-day.png) |
| 저녁, 벽/문/작업대/모닥불/제련로/상자 | [1920×1080](images/progression-evening.png) |
| 밤, 중립색 조명과 시설 빛 | [1920×1080](images/progression-night.png) |
| 기존 Zombie 야간 접근 | [1920×1080](images/progression-night-pressure.png) |
| 새벽 | [1920×1080](images/progression-dawn.png) |
| Chest18+Player36 | [1920×1080](images/progression-chest-1920.png), [2400×1080](images/progression-chest-2400.png), [1280×720](images/progression-chest-1280.png) |
| 제련 중/완료 대기 UI | [1920×1080](images/progression-smelting-1920.png), [2400×1080](images/progression-smelting-2400.png), [1280×720](images/progression-smelting-1280.png) |
| 장비9종, 작업대 스크롤 목록 | [1920×1080](images/progression-tiers.png) |

두 차례 이상 수정 전/후를 비교했다. 다음 실제 문제를 고쳤다.

1. 같은 광석 프리팹을 반복 저장하면서 앞서 만든 인스턴스 위치가 원점으로 돌아갔다. WorldArt에서 프리팹 인스턴스 Transform 변경을 기록하도록 수정했고, 광석3개의 위치·거리 회귀 검사를 추가했다.
2. 큰 인벤토리/상자 창이 좌상단 HUD와 겹쳤다. ModalLayout이 상단 HUD와 가장자리 가방 영역을 예약한 나머지 공간에 패널을 맞춘다. 세 Landscape 해상도와 headless의 좁은 비율에서 확인했다.
3. 레시피17종을 기존 고정 목록에 모두 표시할 수 없었다. 실제 ScrollRect/클리핑/작은 스크롤바로 바꾸고 드래그 스크롤 후 Chest 제작까지 검사했다.
4. 가로로 배치한 벽/문 위 HP바도 함께 회전했다. HP바를 화면 기준으로 유지했다.

밤에 화면을 파랗게 덮는 필터는 없고 오브젝트 실루엣은 읽힌다. 닭은 회색 몸통과 작은 볏/부리로 표시했다. 다만 Stone/Iron 아이콘의 작은 회색 머리 부분은 비슷하고, 작은 화면의 Recipe 재료 글자는 여전히 작다. 상자/제작 UI가 열린 동안 월드 가시 영역은 줄어든다. 이 가독성과 실제 터치 적합성은 후속 사용자 판단이 필요하다.

## 자동 검증

최종 스크롤바를 포함한 소스를 별도 복사본으로 실행했다. 검증 Unity는 hidden batchmode/nographics이며 라이브 에디터 창·OS 입력을 사용하지 않는다.

| 검사 | 최종 결과 | UTC 실행 시간 |
|---|---|---|
| EditMode | 27/27 Passed, 실패/건너뜀0 | 12:29:22–12:29:23 |
| PlayMode | 31/31 Passed, 실패/건너뜀0 | 12:29:55–12:31:24 |
| Python 소스 검사 | PASS, 287 unique asset GUIDs | 최종 정본 복사 후 |

기존 이동/전투/채집/줍기/36슬롯/제작/설치/생존/조리/설정을 포함한 회귀와 함께 다음을 검사했다.

- Tier별 성능과 Ore 채굴 제한, 독립 재료/처리 데이터, 시간대·조명 경계.
- 실제 UI 포인터 드래그로 Chest↔Inventory 이동·합치기·교환, 장착 갱신, 상자 파괴 드랍과 HUD 겹침.
- 문 열기/닫기와 점유된 닫기 거절, Zombie의 벽 차단·공격·파괴.
- 같은 Zombie 프리팹 야간 생성 위치·최대6·일시정지 정책.
- 실제 광석 공격/드랍과 Furnace Smelt→6초→Take→Iron 제작.
- 작업대 UI 드래그 스크롤 후 Recipe 실행.
- 닭 이동/도망/비공격/사망 드랍, Armor 테스트 fixture의 슬롯/방어/외형 소켓.

PlayMode의 포인터는 테스트 프로세스 내부 Input System 가상 장치다. Windows 마우스나 사용자 컴퓨터의 실제 입력 장치를 조작하지 않았다. 결과 XML/로그/복사본은 로컬 tests/artifacts에 보관하고 Git에서 제외한다. 재실행할 테스트 코드는 client/Assets/Wildfolk/Tests에 있다.

초기 검증 실패는 광석 위치와 범위, 스크롤 입력 fixture, 밀집 자원에서 목표가 섞인 기존 테스트였다. 프리팹 위치의 실제 버그를 고치고 테스트는 목표 자원을 분리하고 정상 드래그 입력으로 변경했다. 이후 패널 예약 변경에서 좁은 비율의 가방 겹침1건이 발생하여 수평 여백을 보완했고, 마지막27/31개가 모두 통과했다.

검수 도중 Play 중 C#을 갱신하여 도메인 재로드 후 런타임 모델이 사라진 오류도 발생했다. 임시 복구 코드를 추가하지 않고 Play 종료→컴파일→새 Play로 확인했다. 최종 fresh Play 콘솔에서 Error/Warning0을 확인했다.

## 남은 판단과 의도적인 제한

- 사용자가 직접 확인할 것: 실제 모바일 드래그·스크롤·버튼 크기, 문 사이 이동과 설치 감각, 야간 밝기, 낮밤 길이/적 압박/재료 소요량/채집 및 전투 손맛.
- Android 실기, 사람의 터치 플레이, 장시간 생존, 성능 프로파일링, 재미·세부 밸런스는 검증하지 않았다.
- Zombie는 간단한 장애물 회피와 장벽 공격이다. 복잡한 기지에서 길찾기 보장은 없다. 시설 주변 거리 검사는 있지만 임의 형태의 밀폐 기지 내부를 분석하는 알고리즘은 없다.
- 필드는 고정48×27 테스트 월드이며 Tree24/Stone10/Ore3, Chicken1/Boar1은 재생성하지 않는다. 광석 총9개로 현 Iron 도구3종을 만들 수 있으나 여유 자원은 없다.
- 건축 그리드/자동 스냅·회수/수리, 연료, 내구도, 실제 Armor 아이템은 없다.
- 새 적, Boss, 대규모 동물, Biome/Dungeon/절차 월드, 고급 시설/전기/자동화, 실제 Multiplayer/World Save는 구현하지 않았다.
- Mine Survival의 극초반 흐름을 비교 방향으로 삼되 코드/에셋/UI/수치를 복제하지 않았다. 이번 수치는 자체 초기값이며 확정 밸런스나 외부 게임의 측정값이 아니다.
