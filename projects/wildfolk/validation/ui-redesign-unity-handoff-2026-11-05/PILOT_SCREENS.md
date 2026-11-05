# Unity 파일럿 화면 명세

상태: 구현 전 설계 제안. 본 문서는 Unity 변경 승인이나 완성 mockup이 아닙니다.

## 공통 기준

- 검토 캔버스: 1920×1080, 1280×720, 844×390 landscape. 1920은 시각 기준, 1280은 재배치 확인, 844는 실제 모바일 우선 시나리오입니다.
- 캔버스 픽셀을 Android dp로 간주하지 않습니다. safe area, 실제 density, 손가락 차폐를 실기기에서 검증합니다.
- UGUI의 기존 CanvasScaler/SafeAreaLayout/ModalLayout 계약과 localization/audio binding을 보존합니다.
- 글자를 이미지에 굽지 않습니다. 열·카드·모달을 고정 비율로 한꺼번에 축소하지 않습니다.
- 모든 화면에서 Korean/English 긴 문자열, 빈/실패/선택/비활성/진행 상태를 같이 검토합니다.

## Pilot 1 — Main Menu (H01/H02)

**구성 제안:** 기존 야영 숲 배경과 Lowfire wordmark를 살리고, 한 개의 전경 메뉴 영역 안에서 Continue와 Single Player를 우선 그룹으로 둡니다. Continue는 검증된 저장이 있을 때만 실행 가능하며 세이브가 없거나 손상되었을 때 분명한 상태/대체 행동을 표시합니다. Single Player는 새 랜덤 세계 시작을 설명하고, 기존 저장이 있을 때만 H02 교체 확인을 표시합니다. Multiplayer, Class, Settings, Profile, Gem Store, Achievements, Attendance의 기능은 숨기거나 삭제하지 않고 보조 내비게이션으로 둡니다. 현재 다중 월드 선택, 알림 inbox, 임의 이벤트 배지를 새로 만들지 않습니다.

| 캔버스 | 배치 |
|---|---|
| 1920×1080 | 큰 배경 여백은 허용하되 패널의 빈 하단을 줄임. 왼쪽 브랜드/월드 장면, 오른쪽 주 메뉴; 보조 메뉴는 한 줄 하단/상단 rail |
| 1280×720 | 1920보다 여백을 줄인 독립 비율. title/subtitle를 다른 행으로 고정하고 최장 EN/KO를 포함해 오버랩 검사 |
| 844×390 | 가로 좁은 전용 구성. 주 액션은 엄지 reach 내에 충분한 높이, 보조 기능은 의미가 보이는 icon+label 또는 스크롤 strip. 세이브 상태 문구와 Continue hitbox 분리 |

**상태/검증:** first launch(no save), valid save, corrupt-primary/backup fallback, no valid recovery, new-world H02 cancel/confirm, transition loading/failure, MP request/failure. 씬 전환 중 중복 입력 잠금과 Home origin/back 보존 확인. 버튼 텍스트가 background image에 포함되지 않는지 확인.

**현재 작성 경계:** Editor/FrontendSceneBuilder.cs와 Runtime/UI/HomeScreen.cs. Home controller/SceneFlow/Save authority는 그대로 두고 view/layout, sprite references만 교체합니다. MainMenu 씬의 generated scene은 canonical builder 경로로 다시 생성해야 하며 손수 저장한 scene tweak만 남기지 않습니다.

## Pilot 2 — Gameplay HUD (G01; input surfaces G02–G08 검증)

**구성 제안:** 게임 화면을 최우선으로 두고 HUD를 3개 그룹으로 읽습니다: 생존 상태/레벨, 시야를 덜 가리는 pause·시간 정보, 아래 hotbar 및 오른쪽 action cluster. 최소 정보부터 시야/손가락을 확인하고 확대 가능한 상태 상세를 분리합니다. 새 Sprint 버튼이나 영구 stamina bar를 추가하지 않습니다.

| 캔버스 | 배치 |
|---|---|
| 1920×1080 | 생존 패널 상단 corner, time/pause 반대 corner, 9 slot 하단 중앙, left movement zone, right action zone |
| 1280×720 | hit size를 글자와 같이 축소하지 않도록 그룹 여백 재설정; HUD state가 행동 클러스터 위에 겹치지 않음 |
| 844×390 | 좌하 이동 thumb zone과 우하 주 행동 zone을 우선. 상태 라벨/수치는 한 그룹으로 compact하게 묶고 hotbar는 가림/누름을 실제 폰에서 시험. pause/safe area는 손가락 가장자리와 분리 |

**동작 매핑:** G02 floating movement joystick; G03 Primary tap/hold 반복 공격; G04 Interact는 클릭, downed teammate rescue는 hold; G05 Class ability는 현재 class 기능/hold 정책; G06 class marker/quarry feedback; G07 Medic ally treatment hold/interrupt; G08 시설 repair/dismantle context hold; G09 boss bar/telegraph, G10 compass/status, G11 toast, G12 meal/medicine/stamina overlay, G13 contextual world prompts. 각 표면의 현재 진입 조건과 입력 취소는 SCREEN_INVENTORY와 CONTROL_AND_ACTION_MAP에 기록된 그대로 유지합니다.

**현재 작성 경계:** Editor/GameplayHudBuilder.cs, Runtime/UI/GameplayHud.cs와 SurvivalHud/TimeHud/HotbarView/ControlSizeLayout 등 기존 controller-view 경계. 기존 VirtualJoystick/HoldAttackInput/InteractButtonView input adapter를 교체하거나 새 state source를 만들지 않습니다. InputSystem을 별도로 추정해 테스트 대체하지 않습니다.

**검증 시나리오:** idle/explore, hotbar switch, repeated attack, contextual interact, quick class activation, dash interrupt, boss telegraph, prompt/toast collision, day/night readability, all control-size values. 양손 기본·한손/왼손잡이 접근은 별도 기기 조사; 출시 편의 설정처럼 보이는 UI를 승인 없이 추가하지 않습니다. SP Pause/Death는 기존 시간 정지, inventory/crafting은 기존 world-tick 정책을 확인합니다.

## Pilot 3 — Inventory / Equipment (I01; I02–I05 핵심 회귀)

**구성 제안:** Hotbar9, Storage27과 Head/Body/Utility/Accessory 장비 4칸을 서로 다른 영역으로 명시합니다. 선택 슬롯은 프레임/라벨/상세 정보로 연결하며, 현재 입력 동작에 맞게 손가락 drag target과 수량 숫자를 확보합니다. 아이템 상세 pane은 가능한 경우 고정 위치를 두되 844에서는 슬롯 목록과 상세 pane의 단계 전환을 프로토타입으로 비교합니다. 아이템/장비 이동, 선택, split/partial drag, stack 수량은 live UI입니다.

| 캔버스 | 배치 |
|---|---|
| 1920×1080 | 좌측 장비/캐릭터, 중앙 36-slot inventory grouping, 우측 선택 상세와 현재 지원 동작 |
| 1280×720 | 같은 의미 그룹을 유지하되 여백 축소. 36칸과 주요 행동이 겹치지 않게 분할 |
| 844×390 | 페이지 전체를 36개의 미세 target로 축소 금지. 9칸 hotbar/27칸 storage 구분과 선택 상세를 우선, 수직 scroll/페이지 전환/selected item details 방법을 시안 비교 후 결정. 실기기 손가락으로 drag |

**상태/검증:** 빈/부분/가득 찬 슬롯, 선택/장착/비교, 스택 long press 후 actions, split cancel/confirm, held partial drag, invalid drop 원상복귀, inventory-to-equipment와 chest transfer regression. 장비 4칸과 inventory 36칸을 재구성하더라도 서버/아이템 이동 authority는 동일합니다.

**현재 작성 경계:** Editor/InventoryUiBuilder.cs, Runtime/UI/InventoryPanel.cs, InventorySlotView/InventoryEquipment/InventoryDragController/InventoryDropZone/StackSplitPanel. InventoryPanel의 기존 감사 manifest hash가 현재 main source와 다르므로 11/4 screenshot을 current rendering과 동일하다고 간주하지 않습니다. 현재 main source를 직접 기준으로 렌더 캡처를 재취득한 뒤 시각 승인합니다.

## 공통 완료 조건

- 3개의 해상도, KO/EN, longest strings, 모든 입력 가능한 상태를 캡처하고 차이를 검토
- 현재 UI state/controller wiring과 새 artwork의 크기·slice를 연결
- 터치/포인터 드래그/취소/중복입력/씬 전환 확인
- safe area/cutout, real-device finger reach, brightness/contrast, 장시간 지속 동작을 Android 기기에서 확인
- 기존 Save/Profile/Protocol/Economy/Multiplayer 데이터와 빌드의 변경이 없음을 diff와 회귀 확인으로 검증
- 사용자에게 각 파일럿 결과를 보여 주고 승인받기 전 전체 이관으로 확대하지 않음
