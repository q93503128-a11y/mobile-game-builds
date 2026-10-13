# Wildfolk

Unity **6000.3.19f1 LTS / Universal 2D**, 모바일 Landscape 탑다운 생존 게임의 초기 플레이 버전이다. 원형 캐릭터의 방향 표시, 9칸 Hotbar, 도끼/곡괭이 장착과 사용을 제공한다. 최종 아트와 완성된 생존 게임은 아니다.

![현재 플레이 화면](docs/images/tools-1920.png)

## 실행과 조작

1. Unity Hub에서 `client/`를 프로젝트로 추가하고 Unity 6000.3.19f1로 연다.
2. `Assets/Wildfolk/Scenes/Wildfolk.unity`를 연다. Build Settings의 첫 씬이다.
3. `Wildfolk > View 1920x1080`으로 기준 화면을 선택하고 Play를 누른다.
4. 하단 중앙 Hotbar의 Axe/Pickaxe를 선택한다. 빈 슬롯이나 재료를 선택하면 맨손이다. 시작 장비는 Axe 1/Pickaxe 1이며 기본 선택은 Axe다.
5. 좌하단 조이스틱으로 이동하고 가까운 나무·돌·적 앞에서 우하단 공격/사용 버튼을 누르거나 유지한다. 도끼는 나무, 곡괭이는 돌에 효율적이다.
6. 바닥 아이템에 가까이 가면 획득한다. 우상단 가방으로 인벤토리를 연다. Hotbar 9칸/Storage 7칸과 실제 선택을 확인하고 가방 또는 X로 닫는다.

WASD와 Space는 에디터 보조 입력이다. 숫자키 슬롯 선택은 구현하지 않았다. 인벤토리를 열면 게임이 일시 정지되고 이동·공격을 차단한다. 사망하면 Try again으로 새 필드를 시작하며 시작 장비를 다시 지급한다. 저장과 이어하기는 없다. 상단 중앙은 실제 보스가 등록될 때만 표시되는 HUD 기반이다.

`Wildfolk > View 2400x1080`은 긴 화면비 확인용이다. 해당 Unity 버전의 내부 Game View API를 사용한다. `Wildfolk > Build Playable Scene`은 기본 씬/프리팹을 재생성하므로 수작업 배치를 보존해야 할 때는 사용하지 않는다. 기존 데이터 에셋은 덮어쓰지 않는다.

## 구성과 검증

- `client/Assets/Wildfolk/Runtime/`: 입력·이동·전투·Health·자원·드랍·인벤토리·적·HUD.
- `client/Assets/Wildfolk/Data/`: 실제 수치와 Wood/Stone 정의의 단일 정본.
- `client/Assets/Wildfolk/Editor/`: 아트 임포트와 데이터/프리팹/씬/HUD 생성 도구.
- `client/Assets/Wildfolk/Tests/`: EditMode와 PlayMode 자동 검증.
- `docs/DESIGN_BIBLE.md`, `BALANCE.md`, `ARCHITECTURE.md`: 현재 기획·수치·코드 연결.
- `docs/HOTBAR_TOOLS_REVIEW.md`: 이번 자동 검증/방향/도구/UI 시각 검수와 사용자 테스트 범위. PLAY_REVIEW/VISUAL_REVIEW는 이전 단계의 기록.
- `tools/generate_visual_assets.py`: 독립적으로 만든 PNG 아트 생성 원본. Pillow 12.3.0 필요.
- `tests/validate_visual_project.py`: 에디터 없이 소스 구조·참조·모바일 설정·금지 UI 문구 검사.

아트 재생성: `python tools/generate_visual_assets.py client/Assets/Wildfolk/Art`.
소스 검사: `python tests/validate_visual_project.py client`.
Unity Test Runner에서 Wildfolk.Tests.EditMode/PlayMode를 실행한다. 이번에는 주 에디터를 보존하는 별도 복사본에서 Unity -batchmode -nographics -runTests로 검증했다. PlayMode 테스트는 Input System 장치 이벤트를 실제 UI 모듈로 전달한다.

Library/Temp/로그/로컬 검증 프로젝트는 정본에서 제외한다. 협동·저장·제작·건축·아이템 이동·도구 내구도·갑옷·실제 보스·직업·계정·상점은 구현하지 않았다. 실제 사용감과 모바일 직접 조작은 사용자가 테스트한다.
