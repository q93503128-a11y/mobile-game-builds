# 월드 생성·생존 구조 1차 검수

이 문서는 4bee7d4 단계의 기록이다. 현재 저장/재생/전투/물/소분 변경은 STABILITY_REVIEW와 현재 코드/데이터를 따른다.

2026-10-14, 기준 커밋 `5f2fa38016ec08316f9ee10674f51e3a66bf1774`에서 진행했다. Unity 6000.3.19f1 LTS / Universal 2D의 초기 구현이며 최종 아트·월드 생성·출시 밸런스가 아니다.

## 실제 Game View 확인

Unity MCP로 Play 상태를 구성하고 `ScreenCapture.CaptureScreenshot`으로 실제 Game View를 저장하여 이미지를 직접 확인했다. Windows 마우스/키보드/터치는 조작하지 않았다. 기존 Game View에 Play Unfocused를 적용했으며 마지막 확인에서 Editor 포커스는 false였다.

| 확인 대상 | 실제 캡처 |
|---|---|
| 안전한 시작 공간, 빈 Hotbar, 분리된 큰 공격/작은 상호작용 버튼 | [시작](images/worldgen-start.png) |
| 전체 유한 월드, 강·호수·연못·길·자원 분포 | [전체](images/worldgen-overview.png) |
| 이동 가능한 강 여울과 Dirt Path | [강](images/worldgen-river.png) |
| 불규칙 호수 경계와 수변 | [호수](images/worldgen-lake.png) |
| 숲/바위/통나무, 캐릭터 아래 작은 풀 | [숲](images/worldgen-forest.png) |
| 기본 Goblin 외형과 길 주변 화면 | [Goblin](images/worldgen-road-goblin.png) |
| Zombie 지면 출현 → 활동 | [출현](images/worldgen-emergence.png), [활동](images/worldgen-night-active.png) |
| 새벽 잠복 → Zombie 없는 화면 | [잠복](images/worldgen-burrow.png), [새벽](images/worldgen-dawn-clear.png) |
| Chest 24칸, 선택 테두리, Split half | [1920](images/worldgen-chest-1920.png), [2400](images/worldgen-chest-2400.png), [1280](images/worldgen-chest-1280.png) |
| Furnace 투입3/출력3/Batch3·Capacity3/진행 표시 | [1920](images/worldgen-furnace-1920.png), [2400](images/worldgen-furnace-2400.png), [1280](images/worldgen-furnace-1280.png) |
| 드래그 중에만 나타나는 Drop to ground, 30→15/15 | [드랍 영역](images/worldgen-drop-zone-1280.png) |

17개 증거 이미지를 보존한다. 일반 플레이의 카메라 확대율은 기존 기준을 유지한다. 전체 월드 이미지만 검수용으로 카메라를 축소했다. 적 외형/출현/잠복 및 상자/제련로/드랍 UI는 같은 제품 프리팹·컨트롤러를 MCP로 구성한 런타임 검수 상태다. 모든 적이 자연 출현한 장시간 플레이 기록이나 실제 사용자 터치 검증으로 해석하지 않는다. 런타임 검수 배치는 저장하지 않았으며 종료 후 Play=false, 씬 dirty=false, 수동 Resource/Zombie 인스턴스=0을 확인했다.

## 검수 후 반영한 수정

- 지형 색상 보간을 바로잡아 길과 여울이 실제로 드러나고 호수 수변이 부드럽게 이어지도록 수정했다.
- 작은 풀은 Ground Decoration으로 통일하고 나무/시설/캐릭터는 Base Y 정렬을 사용한다. 큰 통나무는 이동을 막는 파괴 가능한 Wood 자원이다.
- Control Size 140%에서 커진 조이스틱/공격 버튼이 Hotbar에 겹치지 않도록 위치를 올리고 작은 Interact도 함께 조정했다.
- 상자는 3×8칸과 아래 플레이어 가방/Hotbar를 한 화면에 배치했다. 상자 Split 대상에 선택 테두리를 추가했다.
- 제련 카드의 긴 상태 표시를 나눠 결과 수량, 투입 수량, Batch/Capacity, 남은 시간, 진행 막대가 버튼과 겹치지 않도록 정리했다.
- 공격 입력의 시설 열기 책임을 제거했고 자신의 설치물은 자동 공격 대상에서 제외했다.

## 자동화 결과

| 검사 | 최종 결과 | 범위 |
|---|---:|---|
| Unity EditMode | 34/34 통과 | 기존 데이터/씬 계약 + Seed 결정성, 12개 Seed의 시작/자원/간격, 여울 연결·지형 색상, 도구 타격수, 업그레이드 비용, 분할/부분 이동, Population/Loot 데이터 |
| Unity PlayMode | 39/39 통과 | 기존 이동·카메라·가방·드래그·제작·설치·전투·시설·생존·Settings/Audio 회귀 + 실제 생성 물 충돌/여울/정렬, Attack/Interact 분리, 출현/잠복/Light, Goblin 경계·선공·보상, Drop/Split/Chest, Furnace 3·2개 처리·파괴 회수, 회복 지연 |
| 정적 소스 검사 | 통과 | Landscape, 필수 파일, 327개 고유 Asset GUID, orphan meta, Sprite 참조, 화면 개발자 문구, Chest24/Furnace3, 포커스를 요청하지 않는 View 도구 |

EditMode 최종 실행은 UTC 00:29:34~00:29:36, PlayMode는 UTC 00:46:35~00:50:44이다. 테스트는 숨겨진 batchmode 별도 프로젝트 복사본에서 실행했다. 입력 검증은 Unity Input System의 가상 장치를 사용했으며 OS 입력을 대신 조작하지 않았다. 최종 검증 시 복사본과 열린 프로젝트, GitHub용 client의 Assets/Packages/ProjectSettings 파일 해시 차이는 0이었다.

## 현재 한계와 사용자 검증

- 넓은 지형과 기본 밀도 변화는 생겼지만 숲의 군집/수변 디테일/길 형태가 단순하다. 전체 지도에서는 자원 분포가 여전히 고르게 흩어진 느낌이며 탐험 목표나 POI가 없다.
- Goblin과 Zombie 출현/잠복은 기본 시각 표현이다. 공격 예고의 가독성, 실제 이동 중 정렬, 야간 압박은 사람이 플레이하며 판단해야 한다.
- 1280×720에서도 요소는 잘리지 않았지만 실제 기기에서 글자·상자 슬롯·Split/Drop 영역의 터치 크기와 엄지 동선을 확인해야 한다. 노치/실제 세이프에어리어는 검증하지 않았다.
- 기본 Seed에서 동기 생성 시간은 Editor에서 약3.9초로 관측했다. 런타임 Transform은 UI를 포함해 약1,526개였다. Android 프로파일링/메모리/발열/장시간 프레임 성능은 측정하지 않았으며 최적화 완료라고 판단하지 않는다.
- 자원과 동물은 현재 재생성하지 않는다. 초기 동물은 닭1/멧돼지1로 고기 공급도 유한하다. 장기 무한 생존 루프는 완성되지 않았다.
- 건물 내부 Spawn 회피는 근접 충돌과 4방향 Barrier 검사 기반이다. 복잡한 임의 형태 기지의 내부 판정과 완전한 길찾기는 추가 작업이 필요하다.
- 월드 변경 영구 저장, 실제 네트워크/인원별 Scaling, 다중 Biome/Chunk Streaming/POI/Dungeon, Sprint UI, 전체 Armor/Skill Tree, 전문 제작대/Meta Coin/Shop은 없다. XP/Gold는 현재 세션 값이다.

사람이 직접 확인할 항목은 실제 터치 이동/공격/Interact, 집 앞 전투, Drop 취소와 정확한 영역 조작, 상자 소분·전송, 제련 수령, 여러 밤과 기지 조명 반응, 강 건너 탐험, 채집 속도·생존 난이도·장시간 성능이다.
