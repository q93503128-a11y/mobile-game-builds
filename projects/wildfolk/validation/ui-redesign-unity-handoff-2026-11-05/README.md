# Lowfire UI 재설계 — Unity 이관 설계 기준선

작성일: 2026-11-05
상태: Phase 1 설계 기준선 제안 — 사용자 승인 전
저장소 기준: mobile-game-builds main 3ce07a31f03ac90a9edf5c93e877a2f03cc4ff0a
공통 규칙 기준: dev-playbook main e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7
작업 시작: 2026-11-05 13:43:44 KST
문서 작성·검증 완료: 2026-11-05 14:10:54 KST (시작부터 27분 10초; Git 전송 제외)

## 결정 요약

권고 방향은 **현행 Unity 기능을 그대로 유지하는 Forest Survival UI**입니다. A의 텍스트·상태 가독성, C의 열린 시야와 액션 위계, B의 슬롯/아이템 식별을 결합하되 A/B/C 어느 한 화면도 그대로 채택하지 않습니다. Stage 1.5는 참고 자료이지 승인된 최종안이 아닙니다.

1. 공통 UI 어휘는 낮은 채도의 숲색, 따뜻한 중립 표면, 절제된 ember 강조색, 선명한 라이브 텍스트로 제한합니다.
2. 전체 장면을 덮는 종이/책 패널, 웹 대시보드처럼 보이는 카드 그리드, 투명 테두리만으로 만든 주요 액션은 최종 기준에서 제외합니다.
3. 기존 Lowfire 로고, 아이템/직업 그림, NotoSansKR, 검증 가능한 아이콘은 재사용합니다. 패널·버튼의 시각적 핵심은 원본 벡터/래스터 아트로 만들고 Unity의 9-slice와 레이아웃으로 구성합니다.
4. 첫 구현 후보는 메인 메뉴, 인게임 HUD, 인벤토리입니다. 844×390은 별도 재배치가 필요하며 단순 축소를 완료 기준으로 삼지 않습니다.
5. HUD 기본 배치는 왼손 이동, 오른손 주요 행동으로 두되 공격/상호작용/대시의 손가락 이동 거리는 두 가지 프로토타입으로 비교합니다. 위치·투명도 저장 편집기는 기본 레이아웃이 실제 기기에서 검증된 뒤 결정합니다.

## 산출물

- [DESIGN_DECISIONS.md](DESIGN_DECISIONS.md) — A/B/C와 Stage 1.5의 요소별 KEEP/REFINE/REBUILD/DISCARD 판단
- [VISUAL_SYSTEM.md](VISUAL_SYSTEM.md) — 후보 토큰, 타이포그래피, 화면 크기별 레이아웃 규칙
- [ASSET_PRODUCTION_PLAN.md](ASSET_PRODUCTION_PLAN.md) — 아트 원본, 라이선스, 임포트, 아틀라스와 9-slice 계획
- [PILOT_SCREENS.md](PILOT_SCREENS.md) — 세 파일럿의 실제 기능 매핑 및 검증 기준
- [MOBILE_CONTROLS_PLAN.md](MOBILE_CONTROLS_PLAN.md) — 현재 입력 계약과 두 엄지 조작 검증안
- [FULL_UI_MIGRATION_PLAN.md](FULL_UI_MIGRATION_PLAN.md) — 현재 구현 화면 57개를 빠짐없이 분배한 단계 계획
- [IMPLEMENTATION_GUARDRAILS.md](IMPLEMENTATION_GUARDRAILS.md) — 데이터/저장/네트워크/씬 빌더 보존 규칙
- [EVIDENCE_INDEX.md](EVIDENCE_INDEX.md) — 근거 파일, 실제 캡처, 출처, 미검증 범위

## 근거와 범위

현재 게임 구현·문서가 과거 대화나 시안보다 우선합니다. 디자인 감사는 66개 표면을 인벤토리화하고 그중 57개를 현 구현, 8개를 미래 항목, 1개를 역사 항목으로 구분했습니다. 의미 행동은 185개입니다. 자세한 목록은 [기존 화면 인벤토리](../penpot-ui-ux-audit-2026-11-04/SCREEN_INVENTORY.md)와 [기능/상태 이관표](FULL_UI_MIGRATION_PLAN.md)를 참조하세요.

Penpot A/B/C 전체 주요 미리보기와 Stage 1.5 미리보기를 열어 비교했고, 기존 감사의 Unity Game View 캡처를 증거로 검토했습니다. 캡처는 2026-11-04 Unity Game View 기준이며 실제 Android 기기 캡처가 아닙니다. Stage 1.5의 844 미리보기는 Penpot SVG markup 경로 산출물이고 공식 PNG export나 Android QA가 아닙니다. 1280·1920 중간본에는 미해결 겹침/과잉 여백이 있습니다.

이 패스에서 Unity 소스, 씬, 프리팹, 에셋, Penpot 파일을 수정하거나 Unity/Android에서 실행 검증하지 않았습니다. 문서는 설계 제안이며 화면 방향과 팔레트는 승인된 최종안이 아닙니다. 새 화면 구현 전 사용자가 방향과 파일럿 세 화면의 기준을 승인해야 합니다.

## 승인 게이트

1. 이 문서의 통합 방향과 KEEP/REFINE/REBUILD/DISCARD 선택을 검토합니다.
2. Main Menu/HUD/Inventory 각 3개 기준 해상도의 와이어/시각안을 승인합니다.
3. 승인 뒤에만 에셋 원본과 Unity 파일럿 구현을 진행합니다.
4. 파일럿은 실제 기기에서 터치, 가독성, 안전영역, 장시간 UI 열림 동작을 확인합니다.
5. 세 파일럿 승인 전 전체 화면 이관이나 컨트롤 저장 스키마 추가를 시작하지 않습니다.

## 다음 단계 — Codex Luna 구현 범위 제안

사용자가 이 방향과 파일럿 기준을 승인하면 Luna의 첫 구현 작업을 다음으로 제한합니다.

- Main Menu: H01/H02, 기존 FrontendSceneBuilder와 HomeScreen의 표현층
- Gameplay HUD: G01의 레이아웃 및 G02–G08 컨트롤 배치·시각 상태. 기존 입력 계약은 유지
- Inventory: I01의 레이아웃/아이템 표시. 기존 36칸·4 장비 슬롯과 I02–I05 상호작용 회귀 확인
- 정본 작성 지점: FrontendSceneBuilder, GameplayHudBuilder, InventoryUiBuilder와 관련 Runtime UI view. 필요 아트는 ASSET_PRODUCTION_PLAN의 라이선스·원본·9-slice 규칙을 따라 새로 연결
- 완료 조건: 844×390, 1280×720, 1920×1080 및 EN/KO, 주요 빈/선택/오류/확인 상태; Unity compile 및 해당 기존 회귀; 승인된 Android 기기에서 터치·safe-area 확인
- 제외: 전체 UI 일괄 이관, 새 컨트롤 편집/저장 schema, 새 기능, Save/Profile/Protocol/Economy/Multiplayer 동작 변경, 배포

이 범위의 구현은 이번 단계에 포함되지 않습니다. 방향·토큰·파일럿 명세에 대한 사용자 승인을 받은 뒤에만 시작합니다.
