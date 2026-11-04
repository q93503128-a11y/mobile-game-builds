# 화면·상호작용 표면 전수 목록

기준은 fetched main `48d6528758d18bf3df41ed02ad9aa0f268654a51`이다. 이 보고서는 게임 수정 승인이 아니다.

**CHECKPOINT / DRAFT — 사용자 요청으로 중단. 전수 감사 완료본이 아니다.** 자동 추출 목록과 수동 대조 초안이며, 잔여 surface/action 검수와 6개 보고서는 RESUME.md에 남겼다.

FACT는 현재 source/data 사실, CONFIRMED는 이번 Unity에서 관찰한 해당 화면·행동, SOURCE-INFERRED는 source로 확인했지만 그 상태를 실제 재현하지 않은 항목, HISTORICAL은 과거 캡처/검사, PROPOSAL은 제안, FUTURE GOAL은 사용자의 차기 목표, UNVERIFIED는 추가 증거가 필요한 항목이다. 시각적 평가 자체는 감사자의 판단이며 기능 검증과 구분한다.

발견 등록 66개: 현재 코드에 존재하는 표면 57개, 미래·미구현·역사 전용 9개. 한 메뉴 안의 독립 과제(예: Split, held drag, queue, downed banner)도 표면으로 센다. EN/KO·해상도·recipe111·class6·slot 반복을 별도 화면 수로 부풀리지 않았다. 화면 수는 전체 페이지 수나 실플레이 완료 수가 아니다.

| ID | 실제 표시/역할 | 상태 | 진입 → 이탈 | 근거 |
|---|---|---|---|---|
| H01 | Home / 야영지 | IMPLEMENTED | 앱 MainMenu; Pause 저장 후 복귀 → 앱 기본 화면 | [HomeScreen](../../client/Assets/Wildfolk/Runtime/UI/HomeScreen.cs#L8) |
| H02 | 새 세계 교체 확인 | IMPLEMENTED | H01 SinglePlayer: Repository.Exists → Cancel → H01; BeginNew → T01 | [HomeScreen](../../client/Assets/Wildfolk/Runtime/UI/HomeScreen.cs#L8) |
| H03 | 설정 | IMPLEMENTED | Home.Settings → Back → H01 | [SettingsPanel](../../client/Assets/Wildfolk/Runtime/UI/SettingsPanel.cs#L5) |
| M01 | 협동 Create or Join | IMPLEMENTED | H01 Multiplayer → Back → H01; Single → H02 또는 T01 | [MultiplayerMenu](../../client/Assets/Wildfolk/Runtime/Networking/MultiplayerMenu.cs#L6) |
| M02 | 대기실 | IMPLEMENTED | NetworkSession.Waiting → Back → Leave(false) → H01 | [MultiplayerMenu](../../client/Assets/Wildfolk/Runtime/Networking/MultiplayerMenu.cs#L6) |
| P01 | 직업 개요 | IMPLEMENTED | H01 Class → Back → H01 | [ClassLobbyScreen](../../client/Assets/Wildfolk/Runtime/UI/ClassLobbyScreen.cs#L6) |
| P02 | 직업 Skill Tree | IMPLEMENTED | P01 OpenTree → Back → P01 | [ClassLobbyScreen](../../client/Assets/Wildfolk/Runtime/UI/ClassLobbyScreen.cs#L6) |
| P03 | 직업 구매 확인 | IMPLEMENTED | P01 AskPurchase gate 통과 → Cancel → P01; Confirm → P01 결과 | [ClassLobbyScreen](../../client/Assets/Wildfolk/Runtime/UI/ClassLobbyScreen.cs#L6) |
| A01 | Profile & Records | IMPLEMENTED | H01 Profile; A02 Back → Back → H01 | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) |
| A02 | Account | IMPLEMENTED | A01 Account; E05 Contact가 경유 → Back → A01 | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) |
| A03 | Help & Feedback | IMPLEMENTED | A02 Help; E05 Contact → CloseHelp → A02 항상: Store origin 소실 | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) |
| A04 | Feedback category dropdown | IMPLEMENTED | A03 category click → 선택 또는 바깥 닫기 → A03 | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) |
| A05 | 계정 삭제 확인 | IMPLEMENTED | A02 Delete: Linked gate → Cancel → A02; Confirm 2회 → service 요청 | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) |
| A06 | Account/Cloud 충돌 선택 | DISABLED_IMPLEMENTED | AccountConnection.AccountConflict → KeepLocal 또는 명시 UseExisting/UseCloud → service; Back → A01 | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) |
| E01 | 업적 | IMPLEMENTED | H01 direct 또는 A01 → Back → 호출 H01 또는 A01 | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) |
| E02 | 신규 출석 | IMPLEMENTED | H01/A01 Attendance; Board0 → Back → 호출 H01/A01 | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) |
| E03 | 월간 출석 | IMPLEMENTED | E02 Board1 → Back → 호출 H01/A01 | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) |
| E04 | 기간 출석 / Events | PARTIAL | Board2; 재선택 시 event index cycle → Back → 호출 H01/A01 | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) |
| E05 | Gem Store | IMPLEMENTED | H01 Gem 또는 A01 Store → Close → fromHome H01 아니면 A01; Help → A03 | [StoreScreen](../../client/Assets/Wildfolk/Runtime/UI/StoreScreen.cs#L7) |
| E06 | 구매 내역 | IMPLEMENTED | E05 ToggleHistory → ToggleHistory → E05; Back → origin | [StoreScreen](../../client/Assets/Wildfolk/Runtime/UI/StoreScreen.cs#L7) |
| E07 | 선택형 광고 상태 | DISABLED_IMPLEMENTED | A01 optional WatchGem/Retry → A01 내 상태; 외부 native ad 후 복귀 | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) |
| T01 | Loading / Transition overlay | IMPLEMENTED | SceneFlow.Go; NetworkSession Watch → network Cancel → Leave; failed load → recovery; SP loading 취소 불가 | [TransitionOverlay](../../client/Assets/Wildfolk/Runtime/UI/TransitionOverlay.cs#L10) |
| G01 | Gameplay HUD | IMPLEMENTED | Wildfolk 또는 Multiplayer 로컬 actor bind → Bag/Character/Pause 또는 die | [GameplayHud](../../client/Assets/Wildfolk/Runtime/UI/GameplayHud.cs#L6) |
| G02 | Floating movement / Sprint | IMPLEMENTED | Running 왼쪽 입력 영역 touch → release/cancel/focus loss/state change → zero | [VirtualJoystick](../../client/Assets/Wildfolk/Runtime/Input/VirtualJoystick.cs#L6) |
| G03 | Primary action | IMPLEMENTED | Running 오른쪽 primary → release/state/focus reset | [HoldAttackInput](../../client/Assets/Wildfolk/Runtime/Input/HoldAttackInput.cs#L7) |
| G04 | Interact / Revive hold | IMPLEMENTED | Running context target 또는 downed teammate → release/exit/state change | [InteractButtonView](../../client/Assets/Wildfolk/Runtime/UI/InteractButtonView.cs#L6) |
| G05 | Class active | IMPLEMENTED | 해당 class+Running → hold release/session change | [ClassHud](../../client/Assets/Wildfolk/Runtime/UI/ClassHud.cs#L6) |
| G06 | Class marker / Quarry feedback | IMPLEMENTED | Hunter quarry 또는 signal-capable class → duration expiry; state change | [ClassHud](../../client/Assets/Wildfolk/Runtime/UI/ClassHud.cs#L6) |
| G07 | Medic ally treatment | IMPLEMENTED | Medic+near living peer+selected medicine → finish/release interruption | [ClassHud](../../client/Assets/Wildfolk/Runtime/UI/ClassHud.cs#L6) |
| G08 | Repair / Dismantle context | IMPLEMENTED | 가까운 PlayerBuilt 시설+Running → 멀어짐/메뉴/state; hold release cancels | [ClassHud](../../client/Assets/Wildfolk/Runtime/UI/ClassHud.cs#L6) |
| G09 | Boss bar / world telegraphs | IMPLEMENTED | BossIdentity.Active alive → death/disable 숨김 | [BossHud](../../client/Assets/Wildfolk/Runtime/UI/BossHud.cs#L6) |
| G10 | Navigation status | IMPLEMENTED | Compass 또는 survey/signal/rescue hint → 조건 종료/Off | [NavigationHud](../../client/Assets/Wildfolk/Runtime/UI/NavigationHud.cs#L5) |
| G11 | Feedback / toast | IMPLEMENTED | action result events → 1.5–3s timeout; 다음 메시지가 교체 | [PlayerFeedback](../../client/Assets/Wildfolk/Runtime/UI/PlayerFeedback.cs#L5) |
| G12 | Medicine / Meal / Stamina overlays | IMPLEMENTED | 효과 진행 또는 stamina 사용 → 만료; full delay .7s fade; menu hide 정책 다름 | [MealHud](../../client/Assets/Wildfolk/Runtime/UI/MealHud.cs#L5) |
| G13 | World interaction prompts | IMPLEMENTED | 물/문/동굴/우물/Graveyard/RoyalTomb 등 in range → action 결과 또는 멀어짐 | [PlayerInteraction](../../client/Assets/Wildfolk/Runtime/Player/PlayerInteraction.cs#L5) |
| I01 | Inventory / Equipment | IMPLEMENTED | Running Bag; crafting OpenBag → Close → Running | [InventoryPanel](../../client/Assets/Wildfolk/Runtime/UI/InventoryPanel.cs#L6) |
| I02 | Item details / Comparison | IMPLEMENTED | I01 selected item/equipped slot → selection change; I01 close | [InventoryEquipment](../../client/Assets/Wildfolk/Runtime/UI/InventoryEquipment.cs#L5) |
| I03 | Item Actions | IMPLEMENTED | stack count>1 long press .45s → Cancel → parent; Split → I04 | [StackSplitPanel](../../client/Assets/Wildfolk/Runtime/UI/StackSplitPanel.cs#L5) |
| I04 | Split Stack | IMPLEMENTED | selected Split 또는 I03 → Cancel → parent; Confirm → I05 | [StackSplitPanel](../../client/Assets/Wildfolk/Runtime/UI/StackSplitPanel.cs#L5) |
| I05 | Held partial / Drag overlay | IMPLEMENTED | full drag 또는 I04 Confirm → Cancel/invalid empty release preserves source; valid destination commits | [InventoryDragController](../../client/Assets/Wildfolk/Runtime/UI/InventoryDragController.cs#L7) |
| I06 | Chest transfer | IMPLEMENTED | PlayerStorage.Open in range → Close → Running; storage invalid closes | [ChestPanel](../../client/Assets/Wildfolk/Runtime/UI/ChestPanel.cs#L5) |
| I07 | Camp / POI Supplies | IMPLEMENTED | secured or guarded IWorldSupplies.Use → Close/state/range invalid → Running | [CampLootPanel](../../client/Assets/Wildfolk/Runtime/UI/CampLootPanel.cs#L5) |
| C01 | Crafting / Station | IMPLEMENTED | Inventory hand; 시설 Interact → Close → Running; Bag → I01 | [CraftingPanel](../../client/Assets/Wildfolk/Runtime/UI/CraftingPanel.cs#L6) |
| C02 | Sequential processing queue | IMPLEMENTED | Pot/Furnace/Blast Sequential station → parent Close; outputs remain world | [CraftingPanel](../../client/Assets/Wildfolk/Runtime/UI/CraftingPanel.cs#L6) |
| C03 | Workbench upgrade / Set Home | IMPLEMENTED | C01 WBI/II → parent Close; upgrade keeps open context | [CraftingPanel](../../client/Assets/Wildfolk/Runtime/UI/CraftingPanel.cs#L6) |
| C04 | Building placement preview | IMPLEMENTED | selected placeable + Primary → item selection/menu; confirm primary consumes only success | [PlayerPlacement](../../client/Assets/Wildfolk/Runtime/World/PlayerPlacement.cs#L5) |
| C05 | Turret controls | IMPLEMENTED | in-range turret Interact → Close/state/range invalid → Running | [TierThreeHud](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L7) |
| C06 | Beacon controls | IMPLEMENTED | Beacon Interact → Close; Track/Recover closes | [TierThreeHud](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L7) |
| C07 | Navigation target chooser | IMPLEMENTED | Compass Choose → selection/Close → Running | [TierThreeHud](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L7) |
| C08 | Telescope controls | IMPLEMENTED | selected Telescope use/Inventory Equip label Use → Close or Pause → Exit | [TierThreeHud](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L7) |
| P04 | Character Information | IMPLEMENTED | Gameplay profile → Close → Running; OpenTree → P05 | [CharacterScreen](../../client/Assets/Wildfolk/Runtime/UI/CharacterScreen.cs#L5) |
| P05 | Common growth tree | IMPLEMENTED | P04 Skill Tree → Back → P04; Close() API는 authoring에 미바인딩 | [SkillTreeScreen](../../client/Assets/Wildfolk/Runtime/UI/SkillTreeScreen.cs#L5) |
| S01 | Pause | IMPLEMENTED | Gameplay Pause or Escape → Resume → Running; settings Back → S01 | [PauseMenu](../../client/Assets/Wildfolk/Runtime/UI/PauseMenu.cs#L4) |
| S02 | Pause Settings | IMPLEMENTED | S01 Settings → Back → S01 (caller-specific) | [SettingsPanel](../../client/Assets/Wildfolk/Runtime/UI/SettingsPanel.cs#L5) |
| D01 | Death / Revive choices | IMPLEMENTED | PlayerLife Dead/Respawning → Free Home → 같은 월드 HomeAnchor; 앱 Home 아님 | [DeathHud](../../client/Assets/Wildfolk/Runtime/UI/DeathHud.cs#L5) |
| D02 | Downed / rescue countdown | IMPLEMENTED | MP lethal downed → 24s deadline or all-down → Dead; revived → Running | [DeathHud](../../client/Assets/Wildfolk/Runtime/UI/DeathHud.cs#L5) |
| D03 | Helping teammate / Rescue | IMPLEMENTED | alive peer near downed, hold G04 → release/move/damage/LoS/disconnect interrupts | [DeathHud](../../client/Assets/Wildfolk/Runtime/UI/DeathHud.cs#L5) |
| F01 | Multiple world selection | FUTURE_GOAL | 사용자 차기 목표; 현재 단일 저장 진입만 있음 → 해당 없음 | 미구현 / 별도 source 없음 |
| F02 | World rename/delete/management | FUTURE_GOAL | world metadata/index/안전 삭제·migration 결정 필요 → 해당 없음 | 미구현 / 별도 source 없음 |
| F03 | World create options / host same world | FUTURE_GOAL | 현재 random seed; Solo/Host 별도 파일. config UI만 추가 불가 → 해당 없음 | 미구현 / 별도 source 없음 |
| F04 | Public server browser / Matchmaking | NOT_IMPLEMENTED | 현재 Room Code 또는 IPv4 직접 참여; 발견 목록 없음 → 해당 없음 | 미구현 / 별도 source 없음 |
| F05 | Quest / Tutorial journal | NOT_IMPLEMENTED | 현재 tooltip/toast와 Recipe 상태만; quest mechanics 발명 금지 → 해당 없음 | 미구현 / 별도 source 없음 |
| F06 | Full map / minimap | NOT_IMPLEMENTED | 개발용 WorldOverview는 Editor; 제품 Compass와 다름 → 해당 없음 | 미구현 / 별도 source 없음 |
| F07 | Durability / repair equipment | NOT_IMPLEMENTED | 현재 시설 HP Repair; 개별 무기 내구 UI/장비 수리 없음 → 해당 없음 | 미구현 / 별도 source 없음 |
| F08 | Back gear / skin shop | NOT_IMPLEMENTED | future Back anchor는 실제 장비 칸/상품 아님 → 해당 없음 | 미구현 / 별도 source 없음 |
| X01 | 구 Character 3-tab / detached CLASS / player Load | HISTORICAL_ONLY | 현재 Character/tree 분리, Class Home행; player Load 제거됨 → 해당 없음 | 미구현 / 별도 source 없음 |

## 모든 표면에 적용하는 판정 규칙

Sheet/Book/Field는 화면 중앙의 blocking overlay이며 popup 바깥 tap-to-dismiss를 자동으로 가정하지 않는다. HUD/world는 non-modal이다. Item Actions/Split은 부모 modal 위의 blocking child, held ghost는 raycast하지 않는 별도 표시다. 오프라인의 업적·출석·Store preview는 존재하지만 수령·구매 권한은 없다. Alpha/Google/광고/IAP OFF를 미구현 UI와 혼동하지 않는다. modal이 세계 시간을 정지시키는지도 별도다: Inventory/Character/Tree/Station은 세계가 진행하고 SP Pause/Dead만 정지한다.

## H01 · Home / 야영지

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / FrontendSceneBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 앱 MainMenu; Pause 저장 후 복귀 |
| Back/종료 | 앱 기본 화면 |
| 주·보조·전체 control | Continue; Single Player; Multiplayer; Class; Settings; Profile; Gem Store; 업적; 출석 |
| scroll | 없음 |
| 동적 데이터 | 유효 save 일차; 실제 Gem/cache 또는 —; 배지 |
| empty/loading/error/disabled | 저장 없음 / 유효 / primary 손상·backup 가능 / 전체 invalid / 전환 중 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 기존 숲·Krita wordmark + 오른쪽 제본 종이 + 좌하 records |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [HomeScreen](../../client/Assets/Wildfolk/Runtime/UI/HomeScreen.cs#L8) · [FrontendSceneBuilder](../../client/Assets/Wildfolk/Editor/FrontendSceneBuilder.cs#L15) |
| 화면 증거 | [home-v2-en-1280.png](evidence/live/home-v2-en-1280.png), [home-v2-en-1920.png](evidence/live/home-v2-en-1920.png), [home-v2-en-844.png](evidence/live/home-v2-en-844.png) · metadata 동명 JSON |

## H02 · 새 세계 교체 확인

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / FrontendSceneBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | H01 SinglePlayer: Repository.Exists |
| Back/종료 | Cancel → H01; BeginNew → T01 |
| 주·보조·전체 control | Start New World; Cancel |
| scroll | 없음 |
| 동적 데이터 | 기존 단일 저장 교체 경고 |
| empty/loading/error/disabled | 확인 / 취소 / transition guard |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [HomeScreen](../../client/Assets/Wildfolk/Runtime/UI/HomeScreen.cs#L8) · [FrontendSceneBuilder](../../client/Assets/Wildfolk/Editor/FrontendSceneBuilder.cs#L15) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## H03 · 설정

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / SurvivalUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Home.Settings |
| Back/종료 | Back → H01 |
| 주·보조·전체 control | Control size; Master; BGM; SFX sliders; Language cycle; Reset; Back |
| scroll | 없음 |
| 동적 데이터 | 0.8–1.4; 0–1 audio; EN/KO; 백분율 |
| empty/loading/error/disabled | 정상 / 즉시 언어·오디오 반영; Reset 즉시 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [SettingsPanel](../../client/Assets/Wildfolk/Runtime/UI/SettingsPanel.cs#L5) · [SurvivalUiBuilder](../../client/Assets/Wildfolk/Editor/SurvivalUiBuilder.cs#L7) |
| 화면 증거 | [settings-v2-en-1280.png](evidence/live/settings-v2-en-1280.png), [settings-v2-en-1920.png](evidence/live/settings-v2-en-1920.png), [settings-v2-en-844.png](evidence/live/settings-v2-en-844.png) · metadata 동명 JSON |

## M01 · 협동 Create or Join

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / MultiplayerUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | H01 Multiplayer |
| Back/종료 | Back → H01; Single → H02 또는 T01 |
| 주·보조·전체 control | Internet/LAN 전환; code/address 입력; Host; Join; Single Player; Back |
| scroll | 없음 |
| 동적 데이터 | route; saved Host 유무; status |
| empty/loading/error/disabled | Idle / invalid code·IPv4 / Connecting / failure / Closing |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [MultiplayerMenu](../../client/Assets/Wildfolk/Runtime/Networking/MultiplayerMenu.cs#L6) · [MultiplayerUiBuilder](../../client/Assets/Wildfolk/Editor/MultiplayerUiBuilder.cs#L8) |
| 화면 증거 | [multiplayer-internet-ko-844.png](evidence/live/multiplayer-internet-ko-844.png), [multiplayer-invalid-ko-844.png](evidence/live/multiplayer-invalid-ko-844.png), [multiplayer-lan-ko-844.png](evidence/live/multiplayer-lan-ko-844.png) · metadata 동명 JSON |

## M02 · 대기실

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / MultiplayerUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | NetworkSession.Waiting |
| Back/종료 | Back → Leave(false) → H01 |
| 주·보조·전체 control | Host Enter; Internet Copy Code; Back |
| scroll | 없음 |
| 동적 데이터 | Room Code 또는 LAN ready; server 인원/정원4; client waiting |
| empty/loading/error/disabled | Host / Client / room full / duplicate / host left |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [MultiplayerMenu](../../client/Assets/Wildfolk/Runtime/Networking/MultiplayerMenu.cs#L6) · [MultiplayerUiBuilder](../../client/Assets/Wildfolk/Editor/MultiplayerUiBuilder.cs#L8) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## P01 · 직업 개요

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / JournalContentAssets |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | H01 Class |
| Back/종료 | Back → H01 |
| 주·보조·전체 control | 6 Card preview; Select; Purchase; OpenTree; Back |
| scroll | 없음 |
| 동적 데이터 | 선택/소유/해금 비용; Class XP/points; Gold/Gem |
| empty/loading/error/disabled | owned / locked / selected / insufficient / profile failed |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 양면 journal, 도표/초상/상세 |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [ClassLobbyScreen](../../client/Assets/Wildfolk/Runtime/UI/ClassLobbyScreen.cs#L6) · [JournalContentAssets](../../client/Assets/Wildfolk/Editor/JournalContentAssets.cs#L9) |
| 화면 증거 | [class-overview-ko-844.png](evidence/live/class-overview-ko-844.png) · metadata 동명 JSON |

## P02 · 직업 Skill Tree

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / JournalContentAssets |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | P01 OpenTree |
| Back/종료 | Back → P01 |
| 주·보조·전체 control | 9 Node select per class; Invest; Respec; Back |
| scroll | 없음 |
| 동적 데이터 | 6×9 실제 node·3가지 branch; prerequisite; XP point |
| empty/loading/error/disabled | locked / prerequisite / available / learned / no point / not lobby |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 양면 journal, 도표/초상/상세 |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [ClassLobbyScreen](../../client/Assets/Wildfolk/Runtime/UI/ClassLobbyScreen.cs#L6) · [JournalContentAssets](../../client/Assets/Wildfolk/Editor/JournalContentAssets.cs#L9) |
| 화면 증거 | [class-tree-ko-844.png](evidence/live/class-tree-ko-844.png) · metadata 동명 JSON |

## P03 · 직업 구매 확인

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / JournalContentAssets |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | P01 AskPurchase gate 통과 |
| Back/종료 | Cancel → P01; Confirm → P01 결과 |
| 주·보조·전체 control | Confirm; Cancel |
| scroll | 없음 |
| 동적 데이터 | 정확한 Class 이름·비용·Gold/PremiumGem |
| empty/loading/error/disabled | Gold transaction / Engineer server unlock / failed |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [ClassLobbyScreen](../../client/Assets/Wildfolk/Runtime/UI/ClassLobbyScreen.cs#L6) · [JournalContentAssets](../../client/Assets/Wildfolk/Editor/JournalContentAssets.cs#L9) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## A01 · Profile & Records

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / EconomyUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | H01 Profile; A02 Back |
| Back/종료 | Back → H01 |
| 주·보조·전체 control | Account; Achievements; Attendance; Store; optional ad; Retry; privacy choices; Back |
| scroll | 없음 |
| 동적 데이터 | 선택 직업/월드 level; linked/Alpha; Gem; completion/points/badges |
| empty/loading/error/disabled | offline catalogue / linked / syncing / failure / reward state |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) · [EconomyUiBuilder](../../client/Assets/Wildfolk/Editor/EconomyUiBuilder.cs#L10) |
| 화면 증거 | [profile-ko-844.png](evidence/live/profile-ko-844.png) · metadata 동명 JSON |

## A02 · Account

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / AccountUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | A01 Account; E05 Contact가 경유 |
| Back/종료 | Back → A01 |
| 주·보조·전체 control | Connect; Sync; Alpha enroll; SignOut; Help; Privacy; Delete; CopySupport; Back |
| scroll | 없음 |
| 동적 데이터 | provider; cloud 상태; support ID; Alpha |
| empty/loading/error/disabled | Offline / Connecting / CloudSyncing / Connected / DeletionPending / Failed |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) · [AccountUiBuilder](../../client/Assets/Wildfolk/Editor/AccountUiBuilder.cs#L10) |
| 화면 증거 | [account-ko-844.png](evidence/live/account-ko-844.png) · metadata 동명 JSON |

## A03 · Help & Feedback

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / AccountUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | A02 Help; E05 Contact |
| Back/종료 | CloseHelp → A02 항상: Store origin 소실 |
| 주·보조·전체 control | Category dropdown; message1500; consent; Submit; Back |
| scroll | 없음 |
| 동적 데이터 | 선택 category6; 명시 진단 동의; ticket/failure |
| empty/loading/error/disabled | editing / submitting / rate limited / success / failed |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) · [AccountUiBuilder](../../client/Assets/Wildfolk/Editor/AccountUiBuilder.cs#L10) |
| 화면 증거 | [store-help-ko-844.png](evidence/live/store-help-ko-844.png) · metadata 동명 JSON |

## A04 · Feedback category dropdown

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / AccountUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | A03 category click |
| Back/종료 | 선택 또는 바깥 닫기 → A03 |
| 주·보조·전체 control | 6 Toggle-like option; vertical scroll |
| scroll | 옵션 목록 |
| 동적 데이터 | Bug/Game/Account/Purchase/Player/Other 현 언어 caption |
| empty/loading/error/disabled | UGUI Dropdown template; single choice |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) · [AccountUiBuilder](../../client/Assets/Wildfolk/Editor/AccountUiBuilder.cs#L10) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## A05 · 계정 삭제 확인

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / AccountUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | A02 Delete: Linked gate |
| Back/종료 | Cancel → A02; Confirm 2회 → service 요청 |
| 주·보조·전체 control | Confirm deletion; Cancel |
| scroll | 없음 |
| 동적 데이터 | 첫 경고 → 재확인 경고 → pending/error |
| empty/loading/error/disabled | 2-step / DeletionPending / Failed |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) · [AccountUiBuilder](../../client/Assets/Wildfolk/Editor/AccountUiBuilder.cs#L10) |
| 화면 증거 | [delete-unconfirmed-ko-844.png](evidence/live/delete-unconfirmed-ko-844.png) · metadata 동명 JSON |

## A06 · Account/Cloud 충돌 선택

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | DISABLED_IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / AccountUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | AccountConnection.AccountConflict |
| Back/종료 | KeepLocal 또는 명시 UseExisting/UseCloud → service; Back → A01 |
| 주·보조·전체 control | Use existing; Keep local; Use cloud; Back |
| scroll | 없음 |
| 동적 데이터 | LastError account_conflict/cloud_conflict; normal actions hidden |
| empty/loading/error/disabled | 구현됨; 제품 backend OFF에서 자연 도달 불가 |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [AccountProfileScreen](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L6) · [AccountUiBuilder](../../client/Assets/Wildfolk/Editor/AccountUiBuilder.cs#L10) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## E01 · 업적

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / EconomyUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | H01 direct 또는 A01 |
| Back/종료 | Back → 호출 H01 또는 A01 |
| 주·보조·전체 control | 7 category; family select22; Claim selected; Claim all; error-only Retry; Back |
| scroll | family 목록 + tier 상세 |
| 동적 데이터 | 22 family58 tier; progression/point/Gem; server claimability |
| empty/loading/error/disabled | offline read-only / syncing / empty category / completed / claimable / failed |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) · [EconomyUiBuilder](../../client/Assets/Wildfolk/Editor/EconomyUiBuilder.cs#L10) |
| 화면 증거 | [achievements-v2-en-1280.png](evidence/live/achievements-v2-en-1280.png), [achievements-v2-en-1920.png](evidence/live/achievements-v2-en-1920.png), [achievements-v2-en-844.png](evidence/live/achievements-v2-en-844.png) · metadata 동명 JSON |

## E02 · 신규 출석

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / EconomyUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | H01/A01 Attendance; Board0 |
| Back/종료 | Back → 호출 H01/A01 |
| 주·보조·전체 control | 신규/월간/Events tabs; Attend; error-only Retry; Back; tiles 비조작 |
| scroll | 없음 |
| 동적 데이터 | 누적7회; 실제 보상3/4/5/6/7/8/12; UTC reset |
| empty/loading/error/disabled | offline / today done / eligible / complete / failure |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) · [EconomyUiBuilder](../../client/Assets/Wildfolk/Editor/EconomyUiBuilder.cs#L10) |
| 화면 증거 | [attendance-v2-en-1280.png](evidence/live/attendance-v2-en-1280.png), [attendance-v2-en-1920.png](evidence/live/attendance-v2-en-1920.png), [attendance-v2-en-844.png](evidence/live/attendance-v2-en-844.png) · metadata 동명 JSON |

## E03 · 월간 출석

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / EconomyUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | E02 Board1 |
| Back/종료 | Back → 호출 H01/A01 |
| 주·보조·전체 control | same tabs; Attend; Retry; Back; 28 tiles 비조작 |
| scroll | 없음 |
| 동적 데이터 | 28방문; 7·14·21·28회6/8/10/16Gem; stamp는 통화 아님 |
| empty/loading/error/disabled | UTC 월 경계 / eligible / complete / offline |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) · [EconomyUiBuilder](../../client/Assets/Wildfolk/Editor/EconomyUiBuilder.cs#L10) |
| 화면 증거 | [attendance-monthly-v2-en-1280.png](evidence/live/attendance-monthly-v2-en-1280.png), [attendance-monthly-v2-en-1920.png](evidence/live/attendance-monthly-v2-en-1920.png), [attendance-monthly-v2-en-844.png](evidence/live/attendance-monthly-v2-en-844.png) · metadata 동명 JSON |

## E04 · 기간 출석 / Events

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | PARTIAL / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / EconomyUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Board2; 재선택 시 event index cycle |
| Back/종료 | Back → 호출 H01/A01 |
| 주·보조·전체 control | Board tabs; Attend(정의+eligibility 필요); Back |
| scroll | 없음 |
| 동적 데이터 | 현재 배포 Events=[]; 없으면 no-event |
| empty/loading/error/disabled | 구현됨 empty; 실제 event 보상은 미공개 |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) · [EconomyUiBuilder](../../client/Assets/Wildfolk/Editor/EconomyUiBuilder.cs#L10) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## E05 · Gem Store

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / StoreUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | H01 Gem 또는 A01 Store |
| Back/종료 | Close → fromHome H01 아니면 A01; Help → A03 |
| 주·보조·전체 control | 3 package select; Purchase; Recover; History; Purchase help; Back |
| scroll | 없음 |
| 동적 데이터 | 24/72/144Gem; platform price; wallet; selected uses; status |
| empty/loading/error/disabled | OFF preview / Fetching / Starting / Pending / Validating / Consuming / Completed / Cancelled / AccountRequired / Failed / ReviewRequired |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [StoreScreen](../../client/Assets/Wildfolk/Runtime/UI/StoreScreen.cs#L7) · [StoreUiBuilder](../../client/Assets/Wildfolk/Editor/StoreUiBuilder.cs#L10) |
| 화면 증거 | [store-ko-844.png](evidence/live/store-ko-844.png) · metadata 동명 JSON |

## E06 · 구매 내역

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / StoreUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | E05 ToggleHistory |
| Back/종료 | ToggleHistory → E05; Back → origin |
| 주·보조·전체 control | History toggle; Back; Recover; Support |
| scroll | history 목록 |
| 동적 데이터 | 서버 구매 이력; 비어 있음; 미확정 결제 상태 |
| empty/loading/error/disabled | empty / loading / failure / history |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [StoreScreen](../../client/Assets/Wildfolk/Runtime/UI/StoreScreen.cs#L7) · [StoreUiBuilder](../../client/Assets/Wildfolk/Editor/StoreUiBuilder.cs#L10) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## E07 · 선택형 광고 상태

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | DISABLED_IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | MainMenu / EconomyUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | A01 optional WatchGem/Retry |
| Back/종료 | A01 내 상태; 외부 native ad 후 복귀 |
| 주·보조·전체 control | Watch; Retry; 실제 UMP privacy options |
| scroll | 없음 |
| 동적 데이터 | +1Gem; loading/unavailable/cancelled/pending/received |
| empty/loading/error/disabled | 구현됨; Ads Enabled=false; native provider 화면은 외부 |
| 온라인·MP | 로컬 presentation; backend/account 서버 상태의 gate 유지 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [EconomyScreen](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L7) · [EconomyUiBuilder](../../client/Assets/Wildfolk/Editor/EconomyUiBuilder.cs#L10) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## T01 · Loading / Transition overlay

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / runtime Build |
| 구성·modal | blocking overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | SceneFlow.Go; NetworkSession Watch |
| Back/종료 | network Cancel → Leave; failed load → recovery; SP loading 취소 불가 |
| 주·보조·전체 control | Cancel 조건부; spinner |
| scroll | 없음 |
| 동적 데이터 | brand; phase; slow 안내20s; world readiness45s |
| empty/loading/error/disabled | Connecting / waiting / loading / slow / failed / cancelled |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 별도 차단층/ghost/transition |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [TransitionOverlay](../../client/Assets/Wildfolk/Runtime/UI/TransitionOverlay.cs#L10) · 미구현 / 별도 source 없음 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G01 · Gameplay HUD

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / GameplayHudBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Wildfolk 또는 Multiplayer 로컬 actor bind |
| Back/종료 | Bag/Character/Pause 또는 die |
| 주·보조·전체 control | profile; Bag; hotbar9; Pause 또는 MP Leave; gameplay actions |
| scroll | 없음 |
| 동적 데이터 | HP/XP/level/points; hunger/thirst; day/phase; Gold; class |
| empty/loading/error/disabled | Running / modal input lock / SP freeze / MP world continues |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [GameplayHud](../../client/Assets/Wildfolk/Runtime/UI/GameplayHud.cs#L6) · [GameplayHudBuilder](../../client/Assets/Wildfolk/Editor/GameplayHudBuilder.cs#L11) |
| 화면 증거 | HISTORICAL [gameplay--hud-repair-ko-844.png](../post-commercial-ui-qa-tools/evidence/gameplay--hud-repair-ko-844.png) · 0ae9feae |

## G02 · Floating movement / Sprint

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / GameplayHudBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Running 왼쪽 입력 영역 touch |
| Back/종료 | release/cancel/focus loss/state change → zero |
| 주·보조·전체 control | pointer down/drag/up; 자동 sprint threshold |
| scroll | 없음 |
| 동적 데이터 | deadzone .12; first pointer ownership; transient base |
| empty/loading/error/disabled | idle visual hidden / moving / sprint / exhausted |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [VirtualJoystick](../../client/Assets/Wildfolk/Runtime/Input/VirtualJoystick.cs#L6) · [GameplayHudBuilder](../../client/Assets/Wildfolk/Editor/GameplayHudBuilder.cs#L11) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G03 · Primary action

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / GameplayHudBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Running 오른쪽 primary |
| Back/종료 | release/state/focus reset |
| 주·보조·전체 control | tap; attack-only hold repeat |
| scroll | 없음 |
| 동적 데이터 | 선택 item 기반 Attack/Place/Consume/Equip/utility |
| empty/loading/error/disabled | availability / cooldown / no ammo / no tool / placement blocked |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [HoldAttackInput](../../client/Assets/Wildfolk/Runtime/Input/HoldAttackInput.cs#L7) · [GameplayHudBuilder](../../client/Assets/Wildfolk/Editor/GameplayHudBuilder.cs#L11) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G04 · Interact / Revive hold

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / GameplayHudBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Running context target 또는 downed teammate |
| Back/종료 | release/exit/state change |
| 주·보조·전체 control | click Use; hold rescue 우선 |
| scroll | 없음 |
| 동적 데이터 | facility/door/water/loot/equipment icon; downed 우선 |
| empty/loading/error/disabled | unavailable / context / teammate hold / interrupted |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [InteractButtonView](../../client/Assets/Wildfolk/Runtime/UI/InteractButtonView.cs#L6) · [GameplayHudBuilder](../../client/Assets/Wildfolk/Editor/GameplayHudBuilder.cs#L11) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G05 · Class active

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / ClassContentAssets |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 해당 class+Running |
| Back/종료 | hold release/session change |
| 주·보조·전체 control | Warrior Guard hold; Engineer Overclock click |
| scroll | 없음 |
| 동적 데이터 | class ability; cooldown; eligible turret |
| empty/loading/error/disabled | class-specific / ready / cooldown / not eligible |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [ClassHud](../../client/Assets/Wildfolk/Runtime/UI/ClassHud.cs#L6) · [ClassContentAssets](../../client/Assets/Wildfolk/Editor/ClassContentAssets.cs#L11) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G06 · Class marker / Quarry feedback

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / ClassContentAssets |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Hunter quarry 또는 signal-capable class |
| Back/종료 | duration expiry; state change |
| 주·보조·전체 control | Mark; CycleMeaning3 |
| scroll | 없음 |
| 동적 데이터 | bearing/distance/condition; team marker list |
| empty/loading/error/disabled | available / cooldown / detailed branch / expired |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [ClassHud](../../client/Assets/Wildfolk/Runtime/UI/ClassHud.cs#L6) · [ClassContentAssets](../../client/Assets/Wildfolk/Editor/ClassContentAssets.cs#L11) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G07 · Medic ally treatment

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / ClassContentAssets |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Medic+near living peer+selected medicine |
| Back/종료 | finish/release interruption |
| 주·보조·전체 control | Medicine click |
| scroll | 없음 |
| 동적 데이터 | 선택 의약; ally range; role gates |
| empty/loading/error/disabled | no ally / no medicine / active / cooldown |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [ClassHud](../../client/Assets/Wildfolk/Runtime/UI/ClassHud.cs#L6) · [ClassContentAssets](../../client/Assets/Wildfolk/Editor/ClassContentAssets.cs#L11) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G08 · Repair / Dismantle context

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / MaintenanceUiBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 가까운 PlayerBuilt 시설+Running |
| Back/종료 | 멀어짐/메뉴/state; hold release cancels |
| 주·보조·전체 control | Repair hold; Dismantle hold; materials scroll |
| scroll | 재료·반환 상세 |
| 동적 데이터 | 대상 이름/HP/buffer; 보유·필요; 실제 반환; hold fill |
| empty/loading/error/disabled | healthy / recent damage / missing / contains cargo / active / complete / bag full |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [ClassHud](../../client/Assets/Wildfolk/Runtime/UI/ClassHud.cs#L6) · [MaintenanceUiBuilder](../../client/Assets/Wildfolk/Editor/MaintenanceUiBuilder.cs#L6) |
| 화면 증거 | HISTORICAL [gameplay--hud-repair-ko-844.png](../post-commercial-ui-qa-tools/evidence/gameplay--hud-repair-ko-844.png) · 0ae9feae |

## G09 · Boss bar / world telegraphs

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / GameplayHudBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | BossIdentity.Active alive |
| Back/종료 | death/disable 숨김 |
| 주·보조·전체 control | 표시 전용 |
| scroll | 없음 |
| 동적 데이터 | 보스 이름/HP; Returning color; attack VFX는 world renderer |
| empty/loading/error/disabled | active / returning / dying; numeric/phase label 없음 |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [BossHud](../../client/Assets/Wildfolk/Runtime/UI/BossHud.cs#L6) · [GameplayHudBuilder](../../client/Assets/Wildfolk/Editor/GameplayHudBuilder.cs#L11) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G10 · Navigation status

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / TierThreeAssets |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Compass 또는 survey/signal/rescue hint |
| Back/종료 | 조건 종료/Off |
| 주·보조·전체 control | 표시 전용; 별도 C07 선택 |
| scroll | 없음 |
| 동적 데이터 | North/target arrow; distance; cave exit/Home/Beacon; rescue 우선 |
| empty/loading/error/disabled | no Home / no target / near / survey phases / peer signal |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [NavigationHud](../../client/Assets/Wildfolk/Runtime/UI/NavigationHud.cs#L5) · [TierThreeAssets](../../client/Assets/Wildfolk/Editor/TierThreeAssets.cs#L8) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G11 · Feedback / toast

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / GameplayHudBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | action result events |
| Back/종료 | 1.5–3s timeout; 다음 메시지가 교체 |
| 주·보조·전체 control | 표시 전용 |
| scroll | 없음 |
| 동적 데이터 | pickup 합산; crafting/save/errors; fishing icon; refund 최대2+more |
| empty/loading/error/disabled | success / blocked / item requirement / bag full / save fail |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [PlayerFeedback](../../client/Assets/Wildfolk/Runtime/UI/PlayerFeedback.cs#L5) · [GameplayHudBuilder](../../client/Assets/Wildfolk/Editor/GameplayHudBuilder.cs#L11) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G12 · Medicine / Meal / Stamina overlays

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / SurvivalUiBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 효과 진행 또는 stamina 사용 |
| Back/종료 | 만료; full delay .7s fade; menu hide 정책 다름 |
| 주·보조·전체 control | 표시 전용 |
| scroll | 없음 |
| 동적 데이터 | treatment seconds; Meal icon/time; player-adjacent arc |
| empty/loading/error/disabled | active / no effect / full / modal |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [MealHud](../../client/Assets/Wildfolk/Runtime/UI/MealHud.cs#L5) · [SurvivalUiBuilder](../../client/Assets/Wildfolk/Editor/SurvivalUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## G13 · World interaction prompts

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / GameplaySceneBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 물/문/동굴/우물/Graveyard/RoyalTomb 등 in range |
| Back/종료 | action 결과 또는 멀어짐 |
| 주·보조·전체 control | Interact/Primary context; 별도 quest/map screen 없음 |
| scroll | 없음 |
| 동적 데이터 | 도구 조건; 물 품질; tomb 밤/Bone/guard/이미 소환 여부 |
| empty/loading/error/disabled | 거리/시야/guard/time/material gate; 결과 toast |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | SpriteRenderer/월드 ghost 및 context feedback |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [PlayerInteraction](../../client/Assets/Wildfolk/Runtime/Player/PlayerInteraction.cs#L5) · [GameplaySceneBuilder](../../client/Assets/Wildfolk/Editor/GameplaySceneBuilder.cs#L10) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## I01 · Inventory / Equipment

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / InventoryUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Running Bag; crafting OpenBag |
| Back/종료 | Close → Running |
| 주·보조·전체 control | 36 select; equipment4 select; Equip/Unequip; Crafting; Split; Close; drag |
| scroll | 없음 |
| 동적 데이터 | Hotbar9+Storage27; portrait; stack/charges; item category |
| empty/loading/error/disabled | empty / selected / equipment filter / full / dragging |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [InventoryPanel](../../client/Assets/Wildfolk/Runtime/UI/InventoryPanel.cs#L6) · [InventoryUiBuilder](../../client/Assets/Wildfolk/Editor/InventoryUiBuilder.cs#L8) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## I02 · Item details / Comparison

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / InventoryUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | I01 selected item/equipped slot |
| Back/종료 | selection change; I01 close |
| 주·보조·전체 control | Equip/Unequip; Telescope Use; item preview |
| scroll | 없음 |
| 동적 데이터 | stat description; Head/Body defense delta; utility/accessory text |
| empty/loading/error/disabled | equipped / incompatible / insufficient bag space; durability bar 없음 |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [InventoryEquipment](../../client/Assets/Wildfolk/Runtime/UI/InventoryEquipment.cs#L5) · [InventoryUiBuilder](../../client/Assets/Wildfolk/Editor/InventoryUiBuilder.cs#L8) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## I03 · Item Actions

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / StackSplitUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | stack count>1 long press .45s |
| Back/종료 | Cancel → parent; Split → I04 |
| 주·보조·전체 control | Split; Cancel |
| scroll | 없음 |
| 동적 데이터 | 아이템 icon/name |
| empty/loading/error/disabled | 단일 stack은 열리지 않음 |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [StackSplitPanel](../../client/Assets/Wildfolk/Runtime/UI/StackSplitPanel.cs#L5) · [StackSplitUiBuilder](../../client/Assets/Wildfolk/Editor/StackSplitUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## I04 · Split Stack

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / StackSplitUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | selected Split 또는 I03 |
| Back/종료 | Cancel → parent; Confirm → I05 |
| 주·보조·전체 control | −1;+1;Half;Confirm;Cancel |
| scroll | 없음 |
| 동적 데이터 | 1..total−1 선택량/전체량 |
| empty/loading/error/disabled | min / max / source changed |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [StackSplitPanel](../../client/Assets/Wildfolk/Runtime/UI/StackSplitPanel.cs#L5) · [StackSplitUiBuilder](../../client/Assets/Wildfolk/Editor/StackSplitUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## I05 · Held partial / Drag overlay

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / InventoryUiBuilder |
| 구성·modal | blocking overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | full drag 또는 I04 Confirm |
| Back/종료 | Cancel/invalid empty release preserves source; valid destination commits |
| 주·보조·전체 control | drag; drop slot/equipment; Drop to ground; Cancel |
| scroll | 없음 |
| 동적 데이터 | ghost; reserved remainder; compatible marker |
| empty/loading/error/disabled | whole / partial / stale revision / unsupported equipment-chest / no space |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 별도 차단층/ghost/transition |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [InventoryDragController](../../client/Assets/Wildfolk/Runtime/UI/InventoryDragController.cs#L7) · [InventoryUiBuilder](../../client/Assets/Wildfolk/Editor/InventoryUiBuilder.cs#L8) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## I06 · Chest transfer

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / ChestUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | PlayerStorage.Open in range |
| Back/종료 | Close → Running; storage invalid closes |
| 주·보조·전체 control | 양측 slot select/drag; Split; Close |
| scroll | 각 가방6열 vertical |
| 동적 데이터 | Chest24/강화36; Player36; shared model/revisions |
| empty/loading/error/disabled | empty / full / competing owner / unavailable |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [ChestPanel](../../client/Assets/Wildfolk/Runtime/UI/ChestPanel.cs#L5) · [ChestUiBuilder](../../client/Assets/Wildfolk/Editor/ChestUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## I07 · Camp / POI Supplies

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / runtime Build |
| 구성·modal | blocking overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | secured or guarded IWorldSupplies.Use |
| Back/종료 | Close/state/range invalid → Running |
| 주·보조·전체 control | 6 Take buttons; Close |
| scroll | 없음 |
| 동적 데이터 | 6 rolled item/name/count; guarded/remaining/empty |
| empty/loading/error/disabled | guarded disables; empty; bag full; stale revision |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 런타임 고정 좌표 어두운 사각 panel, 별도 label/button helper |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [CampLootPanel](../../client/Assets/Wildfolk/Runtime/UI/CampLootPanel.cs#L5) · 미구현 / 별도 source 없음 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## C01 · Crafting / Station

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / CraftingUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Inventory hand; 시설 Interact |
| Back/종료 | Close → Running; Bag → I01 |
| 주·보조·전체 control | category7; craftable filter; recipe select; per-card craft/cook/take; Bag; Close |
| scroll | recipe list + item details + upgrade costs |
| 동적 데이터 | Hand/WBI/II/III/Campfire/Pot/Furnace/Blast/Turret WB variants; costs; description; NEW |
| empty/loading/error/disabled | Ready/MissingMaterials/NoSpace/StationRequired/Processing/Collect; selection fallback |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [CraftingPanel](../../client/Assets/Wildfolk/Runtime/UI/CraftingPanel.cs#L6) · [CraftingUiBuilder](../../client/Assets/Wildfolk/Editor/CraftingUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## C02 · Sequential processing queue

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / CraftingUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Pot/Furnace/Blast Sequential station |
| Back/종료 | parent Close; outputs remain world |
| 주·보조·전체 control | +1; Add all; waiting−1; Collect |
| scroll | 없음 |
| 동적 데이터 | current recipe/input/output/time; waiting count; ready output |
| empty/loading/error/disabled | idle / waiting / processing / blocked output / bag full |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [CraftingPanel](../../client/Assets/Wildfolk/Runtime/UI/CraftingPanel.cs#L6) · [CraftingUiBuilder](../../client/Assets/Wildfolk/Editor/CraftingUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## C03 · Workbench upgrade / Set Home

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / CraftingUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | C01 WBI/II |
| Back/종료 | parent Close; upgrade keeps open context |
| 주·보조·전체 control | Upgrade; Set Home (navigation anchor) |
| scroll | cost scroll |
| 동적 데이터 | tier-specific actual costs; next tier; chosen Home |
| empty/loading/error/disabled | insufficient / upgradeable / III no-upgrade; not respawn Home |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [CraftingPanel](../../client/Assets/Wildfolk/Runtime/UI/CraftingPanel.cs#L6) · [CraftingUiBuilder](../../client/Assets/Wildfolk/Editor/CraftingUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## C04 · Building placement preview

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / GameplaySceneBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | selected placeable + Primary |
| Back/종료 | item selection/menu; confirm primary consumes only success |
| 주·보조·전체 control | Primary placement; movement/aim |
| scroll | 없음 |
| 동적 데이터 | valid/invalid ghost; terrain/overlap/range; selected stock |
| empty/loading/error/disabled | allowed / water/overlap / stale/full / no item |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | SpriteRenderer/월드 ghost 및 context feedback |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [PlayerPlacement](../../client/Assets/Wildfolk/Runtime/World/PlayerPlacement.cs#L5) · [GameplaySceneBuilder](../../client/Assets/Wildfolk/Editor/GameplaySceneBuilder.cs#L10) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## C05 · Turret controls

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / runtime Build |
| 구성·modal | blocking overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | in-range turret Interact |
| Back/종료 | Close/state/range invalid → Running |
| 주·보조·전체 control | aim drag; Fire; Reload; Deposit; ammo3 withdraw; module5 install/remove |
| scroll | 없음 |
| 동적 데이터 | ammo slots; HP; reload/time; modules; overclock |
| empty/loading/error/disabled | unloaded/ready/reloading/no ammo/locked slot/not compatible |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 런타임 고정 좌표 어두운 사각 panel, 별도 label/button helper |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [TierThreeHud](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L7) · 미구현 / 별도 source 없음 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## C06 · Beacon controls

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / runtime Build |
| 구성·modal | blocking overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Beacon Interact |
| Back/종료 | Close; Track/Recover closes |
| 주·보조·전체 control | Track; Recover; Close |
| scroll | 없음 |
| 동적 데이터 | Beacon number |
| empty/loading/error/disabled | range/ownership/space authoritative gates |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 런타임 고정 좌표 어두운 사각 panel, 별도 label/button helper |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [TierThreeHud](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L7) · 미구현 / 별도 source 없음 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## C07 · Navigation target chooser

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / runtime Build |
| 구성·modal | blocking overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Compass Choose |
| Back/종료 | selection/Close → Running |
| 주·보조·전체 control | Off; Automatic; Home; CaveExit; previous/next Beacon; Track; Close |
| scroll | 없음 |
| 동적 데이터 | sorted Beacon number; no target |
| empty/loading/error/disabled | no beacon / missing Home / outside cave |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 런타임 고정 좌표 어두운 사각 panel, 별도 label/button helper |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [TierThreeHud](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L7) · 미구현 / 별도 source 없음 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## C08 · Telescope controls

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / runtime Build |
| 구성·modal | blocking overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | selected Telescope use/Inventory Equip label Use |
| Back/종료 | Close or Pause → Exit |
| 주·보조·전체 control | direction drag; Close |
| scroll | 없음 |
| 동적 데이터 | aim hint; rendered distant view |
| empty/loading/error/disabled | IsViewing blocks regular actions; no simulation expansion |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 런타임 고정 좌표 어두운 사각 panel, 별도 label/button helper |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [TierThreeHud](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L7) · 미구현 / 별도 source 없음 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## P04 · Character Information

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / CharacterUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Gameplay profile |
| Back/종료 | Close → Running; OpenTree → P05 |
| 주·보조·전체 control | Skill Tree; Close |
| scroll | 없음 |
| 동적 데이터 | world LV/XP/HP/Defense/Stamina/Needs; class selected/status |
| empty/loading/error/disabled | 4Hz display; class/world distinction; no equip manipulation |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 양면 journal, 도표/초상/상세 |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [CharacterScreen](../../client/Assets/Wildfolk/Runtime/UI/CharacterScreen.cs#L5) · [CharacterUiBuilder](../../client/Assets/Wildfolk/Editor/CharacterUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## P05 · Common growth tree

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / CharacterUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | P04 Skill Tree |
| Back/종료 | Back → P04; Close() API는 authoring에 미바인딩 |
| 주·보조·전체 control | 6 select; Invest; Back |
| scroll | 없음 |
| 동적 데이터 | rank/prerequisite/point; 6 common skills |
| empty/loading/error/disabled | locked / available / learned / max / insufficient; Respec 없음 |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 양면 journal, 도표/초상/상세 |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [SkillTreeScreen](../../client/Assets/Wildfolk/Runtime/UI/SkillTreeScreen.cs#L5) · [CharacterUiBuilder](../../client/Assets/Wildfolk/Editor/CharacterUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## S01 · Pause

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / SurvivalUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | Gameplay Pause or Escape |
| Back/종료 | Resume → Running; settings Back → S01 |
| 주·보조·전체 control | Resume; Save; Save & return home; Settings |
| scroll | 없음 |
| 동적 데이터 | save result toast; scene/role-specific availability |
| empty/loading/error/disabled | SP frozen; MP has Leave instead of global pause |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [PauseMenu](../../client/Assets/Wildfolk/Runtime/UI/PauseMenu.cs#L4) · [SurvivalUiBuilder](../../client/Assets/Wildfolk/Editor/SurvivalUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## S02 · Pause Settings

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / SurvivalUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | S01 Settings |
| Back/종료 | Back → S01 (caller-specific) |
| 주·보조·전체 control | 동일4 sliders; language; Reset; Back |
| scroll | 없음 |
| 동적 데이터 | Local user preference shared H03 |
| empty/loading/error/disabled | isolated RAM / ordinary PlayerPrefs; world save와 무관 |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [SettingsPanel](../../client/Assets/Wildfolk/Runtime/UI/SettingsPanel.cs#L5) · [SurvivalUiBuilder](../../client/Assets/Wildfolk/Editor/SurvivalUiBuilder.cs#L7) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## D01 · Death / Revive choices

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / DeathUiBuilder |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | PlayerLife Dead/Respawning |
| Back/종료 | Free Home → 같은 월드 HomeAnchor; 앱 Home 아님 |
| 주·보조·전체 control | SP Ad/Retry; 8Gem; Free Home; MP only Home |
| scroll | 없음 |
| 동적 데이터 | Durable/save failure; payment status; ad-used/short/unavailable |
| empty/loading/error/disabled | dead / respawning / pending / late callback reject / failure |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 1720×1000 종이 한 장, 대부분 동일 사각 action |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [DeathHud](../../client/Assets/Wildfolk/Runtime/UI/DeathHud.cs#L5) · [DeathUiBuilder](../../client/Assets/Wildfolk/Editor/DeathUiBuilder.cs#L6) |
| 화면 증거 | HISTORICAL [gameplay--death-ko-844.png](../post-commercial-ui-qa-tools/evidence/gameplay--death-ko-844.png) · 0ae9feae |

## D02 · Downed / rescue countdown

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / DeathUiBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | MP lethal downed |
| Back/종료 | 24s deadline or all-down → Dead; revived → Running |
| 주·보조·전체 control | 표시+teammate rescue via G04 |
| scroll | 없음 |
| 동적 데이터 | remaining seconds; hold progress |
| empty/loading/error/disabled | Downed / Reviving / Dead; offline SP downed 없음 |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [DeathHud](../../client/Assets/Wildfolk/Runtime/UI/DeathHud.cs#L5) · [DeathUiBuilder](../../client/Assets/Wildfolk/Editor/DeathUiBuilder.cs#L6) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## D03 · Helping teammate / Rescue

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | IMPLEMENTED / SOURCE-INFERRED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | Wildfolk / Multiplayer / DeathUiBuilder |
| 구성·modal | nonmodal HUD; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | alive peer near downed, hold G04 |
| Back/종료 | release/move/damage/LoS/disconnect interrupts |
| 주·보조·전체 control | Interact hold; release; move cancel |
| scroll | 없음 |
| 동적 데이터 | 4s progress, Medic modifiers |
| empty/loading/error/disabled | Rescuing locks attacks until interruption; success40%HP/2s protection |
| 온라인·MP | SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님 |
| 현 visual | 투명 world overlay, 수치/작은 icon/원형 touch controls |
| Localization | L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | [DeathHud](../../client/Assets/Wildfolk/Runtime/UI/DeathHud.cs#L5) · [DeathUiBuilder](../../client/Assets/Wildfolk/Editor/DeathUiBuilder.cs#L6) |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## F01 · Multiple world selection

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | FUTURE_GOAL / FUTURE_GOAL; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 사용자 차기 목표; 현재 단일 저장 진입만 있음 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## F02 · World rename/delete/management

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | FUTURE_GOAL / FUTURE_GOAL; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | world metadata/index/안전 삭제·migration 결정 필요 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## F03 · World create options / host same world

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | FUTURE_GOAL / FUTURE_GOAL; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 현재 random seed; Solo/Host 별도 파일. config UI만 추가 불가 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## F04 · Public server browser / Matchmaking

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | NOT_IMPLEMENTED / NOT_IMPLEMENTED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 현재 Room Code 또는 IPv4 직접 참여; 발견 목록 없음 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## F05 · Quest / Tutorial journal

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | NOT_IMPLEMENTED / NOT_IMPLEMENTED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 현재 tooltip/toast와 Recipe 상태만; quest mechanics 발명 금지 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## F06 · Full map / minimap

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | NOT_IMPLEMENTED / NOT_IMPLEMENTED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 개발용 WorldOverview는 Editor; 제품 Compass와 다름 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## F07 · Durability / repair equipment

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | NOT_IMPLEMENTED / NOT_IMPLEMENTED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 현재 시설 HP Repair; 개별 무기 내구 UI/장비 수리 없음 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## F08 · Back gear / skin shop

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | NOT_IMPLEMENTED / NOT_IMPLEMENTED; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | future Back anchor는 실제 장비 칸/상품 아님 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## X01 · 구 Character 3-tab / detached CLASS / player Load

| 항목 | 확인 내용 |
|---|---|
| 구현/신뢰 | HISTORICAL_ONLY / HISTORICAL_ONLY; runtime 증거는 아래 링크의 실제 상태만 |
| 씬·작성 원본 | 없음 / — |
| 구성·modal | fullscreen overlay; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다 |
| 진입 | 현재 Character/tree 분리, Class Home행; player Load 제거됨 |
| Back/종료 | 해당 없음 |
| 주·보조·전체 control | 해당 없음 |
| scroll | 없음 |
| 동적 데이터 | 현재 구현으로 인계 금지 |
| empty/loading/error/disabled | 해당 없음 |
| 온라인·MP | local frontend; 세션 요청은 NetworkSession |
| 현 visual | 1900×980 어두운 녹색 matte utility panel |
| Localization | 설계 미정 |
| 문제/보존 경계 | 해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음 |
| source | 요청/현재 source의 부재 대조 |
| 화면 증거 | 현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조 |

## Dead / 역사·도구 분리

- `SkillTreeScreen.Close()`는 API로 존재하지만 현재 Common Tree authoring에는 Back만 연결된다. Close 버튼이 있는 화면으로 인계하지 않는다. P05 → P04 → Close가 실제 UI 경로다.
- `AccountProfileScreen.Character`는 객체가 남지만 authoring에서 비활성화된다. Profile identity는 EconomyScreen 쪽이 담당한다. 사라진 중복 데이터 표시를 신기능으로 다시 만들지 않는다.
- 숨긴 Dropdown Template, 비활성 Attendance tile, 미선택 node, 비가동 death view는 dead UI가 아니다. 실제 조건에서 활성화하거나 표시 전용으로 사용하는 구조다.
- WorldOverview/Showcase/DeveloperHarness/Combat Sandbox/QA fixture/EditorWindow는 플레이어 화면 수에 포함하지 않는다. 제품 지도·도감·치트 메뉴가 구현됐다는 뜻이 아니다.
- 각 문서의 옛 Item48/132, Recipe99/109, Save11/13, Protocol21, 실제2인 미수행은 그 절의 당시 기록이다. 현재 Item136/Recipe111/Save14/Profile2/Protocol23과 구분한다.
