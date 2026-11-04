# Lowfire UI Visual Direction — Stage 1 Review

상태: **시각 방향 비교 완료, 사용자 선택 대기**  
Penpot 파일: Lowfire — UI Redesign  
File ID: c6f51e89-5d00-81ce-8008-e483b7f6725d  
저장 버전: Stage 1 — visual QA checkpoint — 2026-11-04  
저장 당시 Penpot revision: 23

## 결과와 범위

Forest Utility, Crafted Survival, Modern Adventure 세 방향을 Penpot 편집 객체로 제작했다. 방향별 디자인 요약, 메인 메뉴, HUD, 인벤토리, 844×390 체크 화면과 공통 컴포넌트 비교가 파일에 있다. 최종 디자인 방향은 선택하지 않았다.

Unity 코드, 씬, 프리팹, 게임 아트와 저장 데이터는 변경하지 않았다. 배경 그림은 시각 검토용 프로토타입이며 출시 아트가 아니다. Penpot 보드는 정적 디자인 시안이므로 실제 입력 처리, 안전 영역 대응, 동적 데이터 갱신, 저장·네트워크 동작을 검증하지 않는다.

## 방향 비교

| 방향 | 화면 구성과 시각 언어 | 장점 | 단점과 확인할 점 | 이번 시안의 핵심 |
|---|---|---|---|---|
| A — Forest Utility | 월드 아트를 넓게 보이고, 숲색·밝은 표면·명확한 주 행동으로 정보를 정리한다. | 메뉴 위계와 아이템 정보가 빠르게 읽힌다. 배경과 UI가 자연스럽게 연결된다. | 인벤토리의 밝은 표면이 화면 대부분을 덮는다. 메뉴 세로 밀도는 작은 화면에서 조정이 필요하다. | 배경 우선, 세 개의 상시 자원, 핫바와 모서리 조작부를 분리한다. |
| B — Crafted Survival | 어두운 캠프색, 직물·장비에서 가져온 따뜻한 재질색, 굵은 카드와 경계선으로 준비 상태를 강조한다. | 세 방향 중 제작·장비의 촉감과 전투 준비 정체성이 가장 강하다. 인벤토리 선택 정보와 장비 영역을 명확히 나눈다. | 패널이 연속되면 월드 가림과 시각적 무게가 커진다. 슬롯 이미지 대비와 장식 절제가 필요하다. | 따뜻한 아이템 슬롯 배경을 보정하고, HUD의 세 자원 카드를 넓게 재배치한다. |
| C — Modern Adventure | 얇고 정돈된 표면, 제한된 강조색, 큰 행동 버튼과 열린 월드 시야를 사용한다. | 월드 가림이 가장 적고 메뉴의 주 행동이 선명하다. 가로 화면의 빈 공간을 여유로 활용한다. | 얇은 선과 투명 표면은 밝은 배경에서 대비가 약해질 수 있다. 작은 기기에서 실제 대비와 글자 크기를 추가 확인해야 한다. | 캠프 메뉴 항목과 이어하기 설명에 밝은 터치 표면을 추가해 배경 위 가독성을 보강했다. |

### 예비 추천

**C — Modern Adventure를 우선 후보로 추천한다.** 현재 구현 범위에 없는 기능을 보태지 않으면서 월드 시야와 주 행동의 위계를 잘 유지하고, 가로 모바일 화면에서도 정보가 비교적 덜 답답하다. C 메뉴의 캠프 항목은 배경과 섞이는 문제가 있어 카드형 터치 표면으로 보정했다.

최종 선택은 사용자가 한다. A의 안정적인 표면 대비와 B의 따뜻한 슬롯 재질을 C에 일부 결합하는 혼합 방향도 검토할 수 있다. 이번 추천은 시안 PNG를 비교한 결과이며 실제 기기 사용성 시험 결과는 아니다.

## 참고 게임에서 가져온 원칙

외부 자료의 구체 사례와 저작권 있는 화면 복제 금지 원칙은 [이전 Penpot 디자인 핸드오프](../penpot-ui-ux-audit-2026-11-04/PENPOT_DESIGN_HANDOFF.md)의 조사표에 기록되어 있다. 이번 시안에는 다음 원칙만 적용했다.

| 게임과 근거 | 관찰한 시스템 | Lowfire에 적용한 원칙 | 적용하지 않은 요소 |
|---|---|---|---|
| [Terraria Mobile 1.4.3 공식 패치 노트](https://support.505games.com/support/solutions/articles/150000146926-terraria-console-mobile-v1-4-3-patch-notes-2022-april-27-) | 접기·닫기, 터치 제작·필터, 페이지별 초기화와 길게 누르기 조작 | 화면별 닫기와 터치 영역을 분명하게 보이고 현재 Lowfire 행동과 맞춘다. | Terraria의 배치나 Auto Select 동작을 복제하거나 새 입력 기능을 추가하지 않는다. |
| [Crashlands 2 공식 1.2 패치](https://blog.bscotch.net/post/crashlands-2-patch-12/) | 번역을 위해 능력치 이름 영역 확장, 팁 위치와 피드백 결과 정리 | 라벨 폭과 긴 번역 문자열을 고려하고 행동 결과를 알 수 있게 한다. | 게임별 메뉴 구성과 자동 제작 동작은 복제하지 않는다. |
| [Core Keeper 공식 패치 노트](https://fireshinegames.co.uk/wp-content/uploads/Core-Keeper-Patch-notes-Post-1.0-1.0.0.16.pdf) | 회수 요약의 정확성, 스크롤 단서, 연결 실패 안내 | 선택한 아이템 정보와 스크롤 가능 영역을 명확히 하고 오류 상태를 숨기지 않는다. | Lowfire의 제작·환급·네트워크 권한과 다른 자동 분해나 오류 동작은 도입하지 않는다. |
| [Don’t Starve Together Mobile 공식 지원 문서](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/795-can-i-play-solo/?p=android) | 혼자 플레이와 Host 진입 관계 안내 | 싱글 플레이, 이어하기, 멀티플레이의 차이를 시작 화면에서 읽게 한다. | Lowfire의 현재 저장·Host 구조가 같은 기능을 지원한다고 암시하지 않는다. 다중 월드 선택을 넣지 않는다. |

## Penpot 파일 구성

| 페이지 | 포함 보드 |
|---|---|
| 00 — Design Brief | 00 — Decision brief · Three directions |
| 01 — Direction A | A0 — Direction summary · Forest Utility; A1 — Main Menu; A2 — HUD; A3 — Inventory |
| 02 — Direction B | B0 — Direction summary · Crafted Survival; B1 — Main Menu; B2 — HUD; B3 — Inventory |
| 03 — Direction C | C0 — Direction summary · Modern Adventure; C1 — Main Menu; C2 — HUD; C3 — Inventory |
| 04 — Component Comparison | 세 방향의 버튼·슬롯·자원 바 라이브러리 컴포넌트 비교 |
| 05 — Responsive Checks | 세 방향의 HUD 및 인벤토리 844×390 체크 |

보드 ID는 같은 폴더의 RESUME.md에 기록했다. 파일 안에는 방향별 Primary 버튼, Inventory 슬롯, ResourceBar로 구성된 **재사용 가능한 Penpot 컴포넌트 9개**가 있다. 이 컴포넌트는 Unity 프리팹과 연결되어 있지 않다.

## 화면 검토와 보정

19개 PNG를 Penpot에서 내보내 픽셀 단위로 직접 확인하고, 같은 파일을 previews 폴더에 저장했다.

- 방향 요약 보드 3개
- 메인 메뉴 3개, HUD 3개, 인벤토리 3개 — 각 1920×1080
- HUD 3개, 인벤토리 3개 — 각 844×390
- 컴포넌트 비교 1개

확인 항목은 정보 위계, 한글 라벨 가독성, 텍스트 잘림, 요소 중첩, 버튼 간격, 슬롯과 아이템 대비, 화면 모서리 조작 영역, 작은 화면의 주요 영역 잘림이다.

검토 과정에서 발견한 문제를 다음처럼 수정했다.

1. HUD 상단에 체력·허기·갈증만 남겼다. 원래 상시 자원처럼 보이던 스태미나 바는 제거하고, 레벨·경험치 진행 표시로 바꿨다. 스태미나는 실제 게임처럼 플레이어 주변의 짧은 사용 중 표시로 그렸다.
2. 세 HUD에 날짜·시간 샘플과 플레이어 위치 표시를 추가했다. 844×390 보드에도 같은 정보 위계를 적용했다.
3. B 컴포넌트 비교의 어두운 슬롯 배경에서 모자 아이템 이미지가 묻혀 배경색을 밝은 샌드색으로 조정했다.
4. C 메인 메뉴의 캠프 항목과 이어하기 설명이 배경 위에서 약해 개별 밝은 터치 표면을 추가했다.
5. B HUD의 제거된 네 번째 자원 카드 배경이 세 번째 카드와 겹치는 것을 발견해 배경과 탭을 제거했다.
6. A HUD 체력 라벨을 새 텍스트 노드로 교체해 PNG 렌더링에서도 변경 글자가 표시되는지 확인했다.

최종 내보내기에서 세 방향의 주요 화면과 반응형 화면에 심각한 잘림이나 중첩은 보이지 않았다. 다만 844×390에서 작은 보조 라벨과 아이템 수량은 실제 기기·시청 거리에서 재확인해야 한다. PNG 내보내기는 기기 터치 시험이나 Unity 런타임 시험을 대체하지 않는다.

## Unity 이관 시 예상 작업과 한계

- Penpot 컴포넌트를 Unity UI 컴포넌트로 옮기고 기존 버튼·슬롯·HUD 동작과 각각 연결해야 한다.
- 화면 비율, Safe Area, 터치 목표 크기, 번역 문자열 길이, 한글 폰트 포함 여부를 실제 Android 기기에서 확인해야 한다.
- 인벤토리 드래그·분할·장착 전환, 핫바 선택, 조이스틱과 전투 입력은 디자인 파일만으로 동작하지 않는다.
- 반투명 표면과 배경 이미지의 대비, 오버드로우, 폰트·텍스처 비용은 Unity에서 따로 확인한다.
- 배경 일러스트는 프로토타입용 생성 이미지이며, 제품에 쓸 기존 원본 아트로 오해하지 않는다.

이번 단계는 디자인 제안만 완료했다. Unity 변경은 사용자가 방향을 선택하고 별도 구현 승인을 한 뒤 진행한다.

## 미리보기

로컬 PNG 증거는 [previews](previews/)에 저장했다. Penpot 보드가 편집 가능한 정본이고 PNG는 비교·검토용 내보내기다.

| 비교 | Forest Utility | Crafted Survival | Modern Adventure |
|---|---|---|---|
| 방향 요약 | [PNG](previews/direction-a-summary.png) | [PNG](previews/direction-b-summary.png) | [PNG](previews/direction-c-summary.png) |
| 메인 메뉴 | [PNG](previews/menu-a-1920x1080.png) | [PNG](previews/menu-b-1920x1080.png) | [PNG](previews/menu-c-1920x1080.png) |
| HUD | [1920×1080](previews/hud-a-1920x1080.png) · [844×390](previews/hud-a-844x390.png) | [1920×1080](previews/hud-b-1920x1080.png) · [844×390](previews/hud-b-844x390.png) | [1920×1080](previews/hud-c-1920x1080.png) · [844×390](previews/hud-c-844x390.png) |
| 인벤토리 | [1920×1080](previews/inventory-a-1920x1080.png) · [844×390](previews/inventory-a-844x390.png) | [1920×1080](previews/inventory-b-1920x1080.png) · [844×390](previews/inventory-b-844x390.png) | [1920×1080](previews/inventory-c-1920x1080.png) · [844×390](previews/inventory-c-844x390.png) |

공통 라이브러리는 [컴포넌트 비교 PNG](previews/component-comparison.png)를 참조한다.

## 다음 결정

사용자는 A, B, C 중 하나를 선택하거나, C를 바탕으로 A의 대비와 B의 슬롯 재질을 결합하는 혼합안을 요청할 수 있다. 선택 전에는 어떤 방향도 최종 정본으로 확정하지 않는다.

## 작업 시간

- Penpot 확인·디자인 보정·화면 내보내기·시각 검수·리뷰 초안: 2026-11-04 18:14:16–19:17:19 KST, 총 1시간 3분 3초.
- 세부 작업별 시간 분할과 모델 토큰 사용량은 별도로 측정할 수 없었다. Git 반영 시간은 최종 완료 보고에 추가한다.
