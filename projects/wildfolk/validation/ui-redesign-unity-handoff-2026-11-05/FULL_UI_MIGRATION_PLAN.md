# 전체 UI 이관 계획 — 구현 표면 57개

## 단계와 승인점

각 구현 표면 ID는 아래 기본 이관 단계에 정확히 한 번 배정했습니다. 번역·해상도·조합별 반복 캡처는 별도 표면 ID를 추가하지 않습니다. 단계별 완료 뒤 사용자 리뷰와 다음 단계 승인 게이트를 둡니다. 마지막 Stage 7은 재현·통합 QA이며 새 ID 배정이 아닙니다.

| 단계 | 화면 ID 및 대상 | 핵심 재사용/경계 | 주요 회귀 위험 |
|---|---|---|---|
| 1. 파일럿 | H01, H02, G01, I01 | Frontend/GameplayHud/Inventory canonical builder; localization, SafeArea, Modal, AudioButton | 세이브 진입/교체확인, world tick, 입력 hit area, slot/drop |
| 2. 실시간 플레이 | G02–G13, T01, D01–D03 | movement/action adapters, feedback, death/rescue, transitions | pointer cancel, hold release, boss/interaction ordering, revive/exit/save/lifecycle |
| 3. 인벤토리·제작 | I02–I07, C01–C08 | slot/details/split/drag, recipe, chest/POI, station/placement | 잘못된 transfer/partial drag, 제작 비용·순차 queue·world tick·시설 authority |
| 4. 성장·직업 | P01–P05 | class/skill node detail, progression, Character/Pause routes | 6 class/54 class node 상태·조건·구매 권한, save/profile |
| 5. 설정·세션 | H03, S01–S02, M01–M02 | Settings, Pause and LAN host/join waiting | SP pause semantics, MP server/host authority, caller back-route, connection error state |
| 6. 메타·계정 | A01–A06, E01–E07 | account/store/profile/attendance/achievements | account deletion confirmations, offline/online authority, receipt/disabled routes, origin/back loss |
| 7. 통합 QA | 새 ID 없음; 위 57 전체 조합 | KO/EN, 844×390/1280×720/1920×1080, all implemented states | 누락·중복·현지화·modal stack·저장·MP·성능·contrast |

### 화면별 ID 완전 목록

- **Stage 1 / 4개:** H01, H02, G01, I01
- **Stage 2 / 16개:** T01; G02, G03, G04, G05, G06, G07, G08, G09, G10, G11, G12, G13; D01, D02, D03
- **Stage 3 / 14개:** I02, I03, I04, I05, I06, I07; C01, C02, C03, C04, C05, C06, C07, C08
- **Stage 4 / 5개:** P01, P02, P03, P04, P05
- **Stage 5 / 5개:** H03, S01, S02, M01, M02
- **Stage 6 / 13개:** A01, A02, A03, A04, A05, A06; E01, E02, E03, E04, E05, E06, E07

**Coverage:** 4+16+14+5+5+13 = 57 unique IMPLEMENTED / DISABLED_IMPLEMENTED IDs. F01–F08는 미래/미구현 표면, X01은 과거 UI 표면이므로 migration 대상이 아닙니다. 이 목록을 UI 감사 inventory와 자동 비교하는 문서 검증을 커밋 전에 수행합니다.

## 단계 완료·테스트 게이트

### Stage 1 — Pilot
세 화면 각 KO/EN×3개 해상도 상태캡처, 재현 가능한 데이터 fixture, 기존 입력·라우팅·월드 시간 정책, 실기기 터치 검증. 사용자가 세 화면과 토큰을 승인해야 Stage2 시작.

### Stage 2 — Gameplay & lifecycle
실제 walk/attack/interact/dash/class/rescue/death/boss/cave prompts, cancel/lifecycle, pause, loading fail/recovery를 재현. 터치 오작동과 화면 가림/가독성 기록.

### Stage 3 — item economy/crafting
Hotbar/storage/equipment/chest/camp supply, action/split/drag invalid restore, station recipes/upgrade/queue/placement/turret/beacon/telescope. Save/economy/recipe cost 변화가 없는지 서비스·validator 회귀.

### Stage 4 — progression/class
직업 6개와 Class 54개 노드를 전체 상태 확인, 해금 조건·선택·구매 확인·결과, common skill tree, profile load/back route 점검.

### Stage 5 — settings/multiplayer
모든 오디오/언어/control size settings, pause resume/back route, offline/connection error, LAN host/join/waiting, separate host save, authority flow 확인. fake browser/session migration 금지.

### Stage 6 — account/meta
상태별 Account/conflict/help/delete confirm, achievements/attendance/event selection, store/history/offline and disabled ads. purchase/ads grant는 공식 service/test policy에 따른 경우에만 QA; client UI가 구매완료를 결정하지 않음.

### Stage 7 — integrated verification
57 ID별 EN/KO screenshot/state coverage matrix 완성, 844/1280/1920, portrait rotation/device inset (orientation contract remains landscape), font/contrast, pointer/scroll, touch cancel, screen-reader/a11y if supported, Save/Profile/Protocol, Multiplayer host authority, offline backend, Android GPU/texture/overdraw, cold/warm loading. 신규 문제를 고친 뒤 다시 전체 회귀. 여기서 UI implementation 완료를 판단하고 배포와 별도 승인.

## Future / history 제외 관리

F01 multiworld selection; F02 world CRUD/metadata; F03 world create options; F04 public matchmaking/browser; F05 quest/tutorial journal; F06 full map/minimap; F07 equipment durability/repair; F08 back gear/skin shop; X01 former three-tab character/detached class/player load. 차기 디자인이 이러한 방향을 제안하면 제품 기능/저장·network/economy가 먼저 승인·구현되어야 하며, 이번 57 표면 안에 억지로 포함하지 않습니다.

## 필요 검증 자료

- 실제 Unity main-source 캡처(11/04 감사 캡처와 비교), 해상도/언어/기기 메타데이터
- state/action별 실행 로그 및 기능 회귀 결과
- Android 실제 기기 model/density/safe-area/finger trial
- atlas/runtime profiling (GPU, texture, allocations, render time)
- reviewer/user approvals: pilot, token/art system, 단계 rollout, release candidate
