# UI / 장비 / 제작 / 자원 품질 교정 검수

기준 ee698ed3f5e35865a3509203cd7625336113f644. 새 콘텐츠를 추가하지 않았고 실제 Item33/Recipe25, Inventory36/Hotbar9/Chest24·32를 유지한다. 최종 상품 아트·재미·모바일 성능 합격을 선언하지 않는다.

## 직접 확인과 수정

라이브 Unity MCP Game View의 초기 장비/스킬 사진에서 상품 목록형 장비창, 연결 없는 네 카드, 얇은 철 투구를 확인했다. Inventory Equipment/독립 Tree/새 Worn Overlay로 대체했다. 중간1920 화면에서 가는 글꼴·잘리는 짧은 라벨·비쳐 보이는 월드·읽히지 않는 빈 슬롯을 발견해 weight500/텍스트 높이/무광 배경/선택 Inset 활성 범위를 수정했다. Recipe는 넓은 한 열에서 두 열로 다시 줄여 탐색 공간을 개선했다. 메뉴는 Profile 하단270 기준의 여유를 공유한다.

최종 1920×1080/2400×1080/1280×720 × English/Korean ×9 화면의54개 조합을 캡처했다. HUD·Character Information·Inventory Equipment·Tree·All Crafting·Furnace·Settings·Leather HUD·Building 재료 색상을 확인했다. 전체 해상도 배치를 처음부터 반복하지 않고 발견된 문제만 고쳤다.

첫 종합 사진에서 Stamina 값/Alpha는 정상이어도 Arc가 그려지지 않는 문제를 발견했다. CanvasRenderer 의존성을 명시하고 실제192 Vertex Mesh 검사와 화면 확인을 추가했다. 같은 가방 칸을 재선택할 때 장착 슬롯의 상세가 남는 문제도 Item 선택 이벤트로 수정했다. 이후 영향 있는 HUD/Inventory/Info/Tree30개 조합을 재검수했다. 재검수 중 이미 사망한 Fixture를 재사용한 배치는 폐기하고 새 세션에서 다시 구성했다. Full Stamina의 세 해상도 사진과 Alpha0도 확인했다. 첫54개 사진을 모두 합격했다고 기록하지 않는다.

세 해상도별 두 언어의 최신 조합은 아래 Contact Sheet다. 원본 캡처와 Manifest는 라이브 Captures/quality-final에 있고 테스트 복사본/로그는 Git에서 제외한다. 비교가 필요한 작은 글꼴·재료 수량·Gauge는 원본도 열어 확인했다.

- [1920 English](images/quality-1920x1080-en-overview.png), [1920 한국어](images/quality-1920x1080-ko-overview.png)
- [2400 English](images/quality-2400x1080-en-overview.png), [2400 한국어](images/quality-2400x1080-ko-overview.png)
- [1280 English](images/quality-1280x720-en-overview.png), [1280 한국어](images/quality-1280x720-ko-overview.png)

## 자동 검증 기록

| 실행 | 결과 | 처리 |
|---|---|---|
| 관련 EditMode |27 중26 통과|옛 재생 시간 기대값을 새 기준으로 수정|
| 관련 PlayMode |35 중31 통과|선택 Tab 레이아웃 동기화/번역 키/상자 언어 구독 종료/Fixture Item 키 수정|
| 영향 PlayMode |10 중9 통과|두 열 Recipe의 실제 Button 위치로 Scroll 검사 수정|
| Save/Scroll |2/2 통과|v2 Armor/이미 배운 Skill/파괴 Resource/예약 Furnace Cargo 유지 포함|
| 전체 EditMode 1회 |61 중60 통과|6초 Batch 기대값을2초 단위로 수정한 대상1/1 재검사 통과|
| 전체 PlayMode 1회 |67 중62 통과|구 Inventory 내부 Crafting 진입/혼합 Tier 위치/3개 Batch를 가정한 검사5개를 새 실제 버튼 흐름으로 갱신|
| 흐름 재검사 |5 중4 통과|결과 버튼이 실제 활성화된 뒤 누르도록 대기 조건 수정|
| Renderer/선택/제련/소분 |4/4 통과|실제 Arc Mesh, 동일 칸 재선택, 순차 결과/파괴 반환, 소분 포함|
| 변경 Model/Locale |3/3 통과|출력 보호/과거 Batch 보존/다중 번역 Placeholder 검사|

전체 묶음에서 실패했던 대상은 후속 관련 검사에서 모두 통과했다. 전체 묶음을 처음부터 재실행해 모두 통과했다고 주장하지 않는다. 기존 동물 재생의 Slot/Timer/화면 밖 조건도 별도 검사로 보존했다. Source Validator461개 고유 GUID/참조/Landscape/표시 문구와 Editor Data Validator도 통과했다.

MCP의 실제 Developer Harness에서 Force Tick0→World Time +721초→Rare 회복1을 확인했다. 기본 Ore16에서15/Target15, 파괴 ID 부활 없음, Harvest 제외2개/남은1079초를 확인했다. 기다리며40분을 보내지 않았다.

Windows 마우스/키보드와 Computer Use를 사용하지 않았다. 기존 Game View에 Play Unfocused/Repaint만 사용하고 Editor State의 is_focused=false를 확인했다. 테스트는 기존 캐시가 있는 하나의 무시된 Unity 복사본을 재사용한 hidden headless runner와 내부 Unity 가상 포인터다. 검수 Fixture는 자동 저장을 끄고 Play 종료 시 폐기했다. 언어는 원래 English로, Game View는1920×1080으로 복원했다.

## 남은 판단

단일 Iron 제련 화면은 미래 Recipe 목록에 비해 여백이 많다. 손 장비의 작은 재질 차이는720 화면에서 약하다. 작은 네 노드 Tree는 확장 전 초기 규모이며 아트 전체를 완성품으로 평가하지 않는다. Android 실제 DPI/엄지 이동·버튼 접근·장시간 프레임/저장 강제 종료·재생 속도와 진행 재미는 사용자 직접 테스트 대상이다. 이번에 새 Enemy/광물/Weapon Tier/Status/Pet/Main Menu를 만들지 않았다.
