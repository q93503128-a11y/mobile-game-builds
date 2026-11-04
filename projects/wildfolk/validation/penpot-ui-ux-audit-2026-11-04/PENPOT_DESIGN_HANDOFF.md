# Penpot 전체 UI 재설계 인계 명세

감사 재개 시 fetch한 main: `82d19b64de0157d924a05f87ea8ee698f5d6db6b`. 제품 source/data는 이전 `48d6528`과 동일하다. game 변경·Penpot 제작 없이 조사했다.

FACT/CONFIRMED는 현재 source/이번 audit의 실제 화면, SOURCE-INFERRED는 source로 도출한 동작·위험, HISTORICAL은 과거 캡처, PROPOSAL은 제안, FUTURE GOAL은 사용자 차기 목표, UNVERIFIED는 검증 범위 밖이다. 디자인 심각도는 감사 우선순위이며 모든 항목을 출시 차단 코드 버그로 해석하지 않는다.

**산출물은 설계 명세이며 실제 Penpot board를 만들지 않았다.** 최종 스타일·새 동작·멀티월드 구현은 승인 전 미확정이다. 이 문서와 SCREEN_INVENTORY의 현재 controls, CONTROL_AND_ACTION_MAP의185개 action, NAVIGATION의103route를 함께 인계한다.

## 1. 목표와 불변조건

Lowfire의 landscape survival/crafting 정체성을 살리면서 task hierarchy·읽기 크기·조작 결과를 일관되게 만든다. 숲·불씨·잉크·기존 wordmark는 재사용 후보지만 모든 화면을 책/과도한 양피지/금장/gradient로 만드는 스타일을 확정하지 않는다. 전체 화면을 같은 oversize 종이와 동일 사각 버튼으로 반복하지 않는다. 랜덤 fantasy ornament로 빈 공간을 채우지 않는다.

현재 기능 보존: Save14/프로필2/Protocol23·내부Wildfolk identity,36bag/9hotbar/장비4, Chest24/36,111recipes/WorkbenchI–III/Batch·Sequential 구분,6common skills/6classes54nodes, Host authoritative inventory/시설/보상, 서버 account/Gem/claim/verified purchase, 무료 world Home 복귀와 death revision, disabled provider gates. 데이터 수량·가격·수율·경제/전투 수치를 디자인 편의 때문에 바꾸지 않는다.

## 2. 작업 파일 구조와 frame 명명

PROPOSAL page groups:00 Brief/Token;01 Components;02 Home/World;03 Gameplay;04 Inventory/Crafting;05 Progression;06 Multiplayer;07 Account/Records/Store;08 Death/Transition;09 Responsive/States;10 FutureGoals. 각 현재 frame은 `SCREEN-ID / State / EN|KO / Viewport`, component는 의미 role로 이름을 붙인다. screen counts는 같은 item/class 반복이나 locale별 frame 복제 수와 다르다.

FACT 아래57개 현재 표면은 모두 구현 인계 대상이다. generic parent 한 장만 그려 child/rare/disabled를 빼지 않는다. H01 valid/empty/corrupt, C01 station variant와 E05 purchase state 같은 상태는 실제 logic과 연결한 frame variants로 풀어 쓴다. F/X는 현재 구현 frame에서 분리한 future/reference page다.

| Frame ID | 과제 | 필수 state variants | 현재 control 보존 | 진입/이탈 |
|---|---|---|---|---|
| H01 | Home / 야영지 | 저장 없음 / 유효 / primary 손상·backup 가능 / 전체 invalid / 전환 중 | Continue; Single Player; Multiplayer; Class; Settings; Profile; Gem Store; 업적; 출석 | 앱 MainMenu; Pause 저장 후 복귀 → 앱 기본 화면 |
| H02 | 새 세계 교체 확인 | 확인 / 취소 / transition guard | Start New World; Cancel | H01 SinglePlayer: Repository.Exists → Cancel → H01; BeginNew → T01 |
| H03 | 설정 | 정상 / 즉시 언어·오디오 반영; Reset 즉시 | Control size; Master; BGM; SFX sliders; Language cycle; Reset; Back | Home.Settings → Back → H01 |
| M01 | 협동 Create or Join | Idle / invalid code·IPv4 / Connecting / failure / Closing | Internet/LAN 전환; code/address 입력; Host; Join; Single Player; Back | H01 Multiplayer → Back → H01; Single → H02 또는 T01 |
| M02 | 대기실 | Host / Client / room full / duplicate / host left | Host Enter; Internet Copy Code; Back | NetworkSession.Waiting → Back → Leave(false) → H01 |
| P01 | 직업 개요 | owned / locked / selected / insufficient / profile failed | 6 Card preview; Select; Purchase; OpenTree; Back | H01 Class → Back → H01 |
| P02 | 직업 Skill Tree | locked / prerequisite / available / learned / no point / not lobby | 9 Node select per class; Invest; Respec; Back | P01 OpenTree → Back → P01 |
| P03 | 직업 구매 확인 | Gold transaction / Engineer server unlock / failed | Confirm; Cancel | P01 AskPurchase gate 통과 → Cancel → P01; Confirm → P01 결과 |
| A01 | Profile & Records | offline catalogue / linked / syncing / failure / reward state | Account; Achievements; Attendance; Store; optional ad; Retry; privacy choices; Back | H01 Profile; A02 Back → Back → H01 |
| A02 | Account | Offline / Connecting / CloudSyncing / Connected / DeletionPending / Failed | Connect; Sync; Alpha enroll; SignOut; Help; Privacy; Delete; CopySupport; Back | A01 Account; E05 Contact가 경유 → Back → A01 |
| A03 | Help & Feedback | editing / submitting / rate limited / success / failed | Category dropdown; message1500; consent; Submit; Back | A02 Help; E05 Contact → CloseHelp → A02 항상: Store origin 소실 |
| A04 | Feedback category dropdown | UGUI Dropdown template; single choice | 6 Toggle-like option; vertical scroll | A03 category click → 선택 또는 바깥 닫기 → A03 |
| A05 | 계정 삭제 확인 | 2-step / DeletionPending / Failed | Confirm deletion; Cancel | A02 Delete: Linked gate → Cancel → A02; Confirm 2회 → service 요청 |
| A06 | Account/Cloud 충돌 선택 | 구현됨; 제품 backend OFF에서 자연 도달 불가 | Use existing; Keep local; Use cloud; Back | AccountConnection.AccountConflict → KeepLocal 또는 명시 UseExisting/UseCloud → service; Back → A01 |
| E01 | 업적 | offline read-only / syncing / empty category / completed / claimable / failed | 7 category; family select22; Claim selected; Claim all; error-only Retry; Back | H01 direct 또는 A01 → Back → 호출 H01 또는 A01 |
| E02 | 신규 출석 | offline / today done / eligible / complete / failure | 신규/월간/Events tabs; Attend; error-only Retry; Back; tiles 비조작 | H01/A01 Attendance; Board0 → Back → 호출 H01/A01 |
| E03 | 월간 출석 | UTC 월 경계 / eligible / complete / offline | same tabs; Attend; Retry; Back; 28 tiles 비조작 | E02 Board1 → Back → 호출 H01/A01 |
| E04 | 기간 출석 / Events | 현재 Events=[] empty state; configured board 지원은 source, 실제 보상 미공개 | Board tabs; Attend(정의+eligibility 필요); Back | Board2; 재선택 시 event index cycle → Back → 호출 H01/A01 |
| E05 | Gem Store | OFF preview / Fetching / Starting / Pending / Validating / Consuming / Completed / Cancelled / AccountRequired / Failed / ReviewRequired | 3 package select; Purchase; Recover; History; Purchase help; Back | H01 Gem 또는 A01 Store → Close → fromHome H01 아니면 A01; Help → A03 |
| E06 | 구매 내역 | empty / loading / failure / history | History toggle; Back; Recover; Support | E05 ToggleHistory → ToggleHistory → E05; Back → origin |
| E07 | 선택형 광고 상태 | 구현됨; Ads Enabled=false; native provider 화면은 외부 | Watch; Retry; 실제 UMP privacy options | A01 optional WatchGem/Retry → A01 내 상태; 외부 native ad 후 복귀 |
| T01 | Loading / Transition overlay | Connecting / waiting / loading / slow / failed / cancelled | Cancel 조건부; spinner | SceneFlow.Go; NetworkSession Watch → network Cancel → Leave; failed load → recovery; SP loading 취소 불가 |
| G01 | Gameplay HUD | Running / modal input lock / SP freeze / MP world continues | profile; Bag; hotbar9; Pause 또는 MP Leave; gameplay actions | Wildfolk 또는 Multiplayer 로컬 actor bind → Bag/Character/Pause 또는 die |
| G02 | Floating movement / Sprint | idle visual hidden / moving / sprint / exhausted | pointer down/drag/up; 자동 sprint threshold | Running 왼쪽 입력 영역 touch → release/cancel/focus loss/state change → zero |
| G03 | Primary action | availability / cooldown / no ammo / no tool / placement blocked | tap; attack-only hold repeat | Running 오른쪽 primary → release/state/focus reset |
| G04 | Interact / Revive hold | unavailable / context / teammate hold / interrupted | click Use; hold rescue 우선 | Running context target 또는 downed teammate → release/exit/state change |
| G05 | Class active | class-specific / ready / cooldown / not eligible | Warrior Guard hold; Engineer Overclock click | 해당 class+Running → hold release/session change |
| G06 | Class marker / Quarry feedback | available / cooldown / detailed branch / expired | Mark; CycleMeaning3 | Hunter quarry 또는 signal-capable class → duration expiry; state change |
| G07 | Medic ally treatment | no ally / no medicine / active / cooldown | Medicine click | Medic+near living peer+selected medicine → finish/release interruption |
| G08 | Repair / Dismantle context | healthy / recent damage / missing / contains cargo / active / complete / bag full | Repair hold; Dismantle hold; materials scroll | 가까운 PlayerBuilt 시설+Running → 멀어짐/메뉴/state; hold release cancels |
| G09 | Boss bar / world telegraphs | active / returning / dying; numeric/phase label 없음 | 표시 전용 | BossIdentity.Active alive → death/disable 숨김 |
| G10 | Navigation status | no Home / no target / near / survey phases / peer signal | 표시 전용; 별도 C07 선택 | Compass 또는 survey/signal/rescue hint → 조건 종료/Off |
| G11 | Feedback / toast | success / blocked / item requirement / bag full / save fail | 표시 전용 | action result events → 1.5–3s timeout; 다음 메시지가 교체 |
| G12 | Medicine / Meal / Stamina overlays | active / no effect / full / modal | 표시 전용 | 효과 진행 또는 stamina 사용 → 만료; full delay .7s fade; menu hide 정책 다름 |
| G13 | World interaction prompts | 거리/시야/guard/time/material gate; 결과 toast | Interact/Primary context; 별도 quest/map screen 없음 | 물/문/동굴/우물/Graveyard/RoyalTomb 등 in range → action 결과 또는 멀어짐 |
| I01 | Inventory / Equipment | empty / selected / equipment filter / full / dragging | 36 select; equipment4 select; Equip/Unequip; Crafting; Split; Close; drag | Running Bag; crafting OpenBag → Close → Running |
| I02 | Item details / Comparison | equipped / incompatible / insufficient bag space; durability bar 없음 | Equip/Unequip; Telescope Use; item preview | I01 selected item/equipped slot → selection change; I01 close |
| I03 | Item Actions | 단일 stack은 열리지 않음 | Split; Cancel | stack count>1 long press .45s → Cancel → parent; Split → I04 |
| I04 | Split Stack | min / max / source changed | −1;+1;Half;Confirm;Cancel | selected Split 또는 I03 → Cancel → parent; Confirm → I05 |
| I05 | Held partial / Drag overlay | whole / partial / stale revision / unsupported equipment-chest / no space | drag; drop slot/equipment; Drop to ground; Cancel | full drag 또는 I04 Confirm → Cancel/invalid empty release preserves source; valid destination commits |
| I06 | Chest transfer | empty / full / competing owner / unavailable | 양측 slot select/drag; Split; Close | PlayerStorage.Open in range → Close → Running; storage invalid closes |
| I07 | Camp / POI Supplies | guarded disables; empty; bag full; stale revision | 6 Take buttons; Close | secured or guarded IWorldSupplies.Use → Close/state/range invalid → Running |
| C01 | Crafting / Station | Ready/MissingMaterials/NoSpace/StationRequired/Processing/Collect; selection fallback | category7; craftable filter; recipe select; per-card craft/cook/take; Bag; Close | Inventory hand; 시설 Interact → Close → Running; Bag → I01 |
| C02 | Sequential processing queue | idle / waiting / processing / blocked output / bag full | +1; Add all; waiting−1; Collect | Pot/Furnace/Blast Sequential station → parent Close; outputs remain world |
| C03 | Workbench upgrade / Set Home | insufficient / upgradeable / III no-upgrade; not respawn Home | Upgrade; Set Home (navigation anchor) | C01 WBI/II → parent Close; upgrade keeps open context |
| C04 | Building placement preview | allowed / water/overlap / stale/full / no item | Primary placement; movement/aim | selected placeable + Primary → item selection/menu; confirm primary consumes only success |
| C05 | Turret controls | unloaded/ready/reloading/no ammo/locked slot/not compatible | aim drag; Fire; Reload; Deposit; ammo3 withdraw; module5 install/remove | in-range turret Interact → Close/state/range invalid → Running |
| C06 | Beacon controls | range/ownership/space authoritative gates | Track; Recover; Close | Beacon Interact → Close; Track/Recover closes |
| C07 | Navigation target chooser | no beacon / missing Home / outside cave | Off; Automatic; Home; CaveExit; previous/next Beacon; Track; Close | Compass Choose → selection/Close → Running |
| C08 | Telescope controls | IsViewing blocks regular actions; no simulation expansion | direction drag; Close | selected Telescope use/Inventory Equip label Use → Close or Pause → Exit |
| P04 | Character Information | 4Hz display; class/world distinction; no equip manipulation | Skill Tree; Close | Gameplay profile → Close → Running; OpenTree → P05 |
| P05 | Common growth tree | locked / available / learned / max / insufficient; Respec 없음 | 6 select; Invest; Back | P04 Skill Tree → Back → P04; Close() API는 authoring에 미바인딩 |
| S01 | Pause | SP frozen; MP has Leave instead of global pause | Resume; Save; Save & return home; Settings | Gameplay Pause or Escape → Resume → Running; settings Back → S01 |
| S02 | Pause Settings | isolated RAM / ordinary PlayerPrefs; world save와 무관 | 동일4 sliders; language; Reset; Back | S01 Settings → Back → S01 (caller-specific) |
| D01 | Death / Revive choices | dead / respawning / pending / late callback reject / failure | SP Ad/Retry; 8Gem; Free Home; MP only Home | PlayerLife Dead/Respawning → Free Home → 같은 월드 HomeAnchor; 앱 Home 아님 |
| D02 | Downed / rescue countdown | Downed / Reviving / Dead; offline SP downed 없음 | 표시+teammate rescue via G04 | MP lethal downed → 24s deadline or all-down → Dead; revived → Running |
| D03 | Helping teammate / Rescue | Rescuing locks attacks until interruption; success40%HP/2s protection | Interact hold; release; move cancel | alive peer near downed, hold G04 → release/move/damage/LoS/disconnect interrupts |

## 3. State 확장 규칙

| 계열 | 최소 variants |
|---|---|
| 모든 action | normal/pressed/selected/disabled+사유/busy. 실제 사용하지 않는 hover/focus를 mobile 기본 정보로 의존하지 않기 |
| Home | no-save / valid Continue / invalid primary·backup recovery / unavailable / transition; 새 세계 교체 확인은 현재 흐름 |
| Account | backend OFF/offline/connecting/syncing/connected/account-conflict/cloud-conflict/deletion pending/fail; default diagnostics consent false |
| Store | OFF preview/account required/fetching/platform price/purchase starting/pending/server validation/consuming/completed/cancelled/failure/review required/empty history/recovery. fake 가격 금지 |
| Achievements | offline catalogue / category/family/tier / incomplete / completed / claimable / claimed / sync fail / pending. UI animation이 지급을 확정하지 않음 |
| Attendance | 7/28/Events-empty/configured; stamp/Gem; claimed/today/future; UTC reset/complete/offline. 개별 tile은 display |
| Inventory | empty/populated/stack1/multi/container charges/equipment slot selected/compatible/incompatible/full bag/whole drag/partial-held/invalid destination/stale revision |
| Craft | each Station; Ready/MissingMaterials/NoSpace/StationRequired/Processing/Collect; category empty/filter; selected fallback; recipe NEW/seen; long description/cost6줄 |
| Queue | idle/current/waiting/output/blocked capacity/full bag; +1/AddAll/withdraw waiting/Collect를 구분 |
| Class/common | preview vs actual selected; owned/unowned/purchase; prerequisite locked/point shortage/learned/max; Gold/Gem 해금·world/common과 account/class 구분 |
| Death | SP ad unavailable/used/retry/pending/Gem8/short/offline/save failed/free Home; MP downed/reviving/dead/Home; interrupted rescue/expired/late callback |
| Loading/MP | connect/cancel/room host/client/room full/duplicate/invalid input/failed/load player/slow/no actor/host left/save failed/closing; 가짜 percentage 없음 |

## 4. Common component catalog

| Component | 필수 parts/states | 금지할 기능 손실 |
|---|---|---|
| PrimaryButton | label/icon/pressed/disabled reason/busy | click=transaction success로 취급 금지 |
| SecondaryButton | neutral/selected/disabled | 모든 secondary를 primary와 동일 강조 금지 |
| DestructiveButton | action 대상·영향·확인/cancel | Delete·회수·새세계 교체를 일반 Back처럼 표현 금지 |
| IconButton | glyph/accessible label/hit area | icon 크기=hit rect 고정 금지 |
| NavigationItem / Tab | current/selected/notification/label | tab 전환과 claim·purchase 혼동 금지 |
| Card / ListRow | selected/detail/progress/locked/busy | class preview와 실제 선택 구분 |
| ItemSlot / InventoryGrid | icon/count/charges/selected/compatible/empty/drag marker |9+27 index·장비4·양쪽 Chest 용량/transfer 보존 |
| HotbarSlot |1–9/select/count/charges/selected | PC number와 touch 선택 index 유지 |
| Tooltip / DetailPanel | title/icon/type/stats/comparison/cost/long text | 실제 설명 줄 삭제/PNG text bake 금지 |
| ScrollContainer | viewport/content/edge cue/scrollbar/empty | masked content를 잘림이라며 제거 금지 |
| Modal / Confirm | blocking scrim/title/body/actions/caller/Back | 뒤 controls click leakage·잘못된 return 방지 |
| Toggle | label/off/on/disabled | diagnostics default false 유지 |
| Slider | range/value/hit area/drag/disabled | ControlSize와 audio 범위·live setting 유지 |
| Dropdown / Selection | caption/list/open/selected/cancel/scroll | 현재 feedback6 category; 언어 modal은 제안임을 표시 |
| InputField | hint/value/caret/selection/limit/keyboard/error | feedback1500/route64와 기존 validation 보존 |
| ProgressBar / ResourceBar | current/max/warning/number/label | HP/XP/needs/stamina 서로 다른 의미 유지 |
| CurrencyDisplay | Gold/Gem icon/value/unknown/cache/pending | Crystal과 Gem·world Gold/account wallet 혼동 금지 |
| RewardTile | stamp/Gem/claimed/today/future/locked | 존재하지 않는 per-cell claim action 추가 금지 |
| LoadingIndicator | real phase/slow/cancel policy/failure | fake %·provider 성공 추정 금지 |
| Toast | success/error/item/count/priority/lifetime | 중요한 save/consumption failure가 보이지 않게 덮이지 않기 |
| TouchActionButton | primary tap/hold; interact click/hold rescue; dash | 모든 것을 onClick 하나로 평탄화 금지 |
| SkillNode / Branch | selection/prerequisite/learned/point/connection | node 수/필요조건·공용rank와 class one-time node 구분 |
| ContextAction / HoldProgress | target/cost/refund/recent damage/hold/cancel | exact refund/contents/full-bag/Host gates 유지 |
| WorldSaveCard | name/recent/day/valid/backup/locked/actions | FUTURE GOAL; 현재 구현 UI라고 표시 금지 |

## 5. Design token 제안 — 확정 색상/스타일 아님

Color roles: canvas/world-scrim, surface/context/modal, on-surface primary/secondary/muted, action-primary/secondary/destructive, selected/focus, border/divider, success/warning/error/disabled, HP/XP/hunger/thirst/stamina, Gold/Gem. 저대비 muted나 초록/빨강만으로 성공/불가를 전달하지 않는다. 기존 palette 수치 자체가 상용 품질을 보장하지 않는다.

Type roles: brand(기존wordmark 후보), screen title, section title, selected-item/class identity, body, stat/value, button label, caption, slot count. KO/EN glyph·weight·lineheight를 함께 지정하고 auto-shrink를 기본 해결책으로 쓰지 않는다. PROPOSAL spacing는4/8 unit 기반4·8·12·16·24·32·48, optical alignment 예외는 이유와 함께 component에 기록한다. 현재1920reference 좌표에 그대로 곱하는 것이 아니다.

Surface roles는 HUD의 최소한 overlay, dense inventory utility, task-focused settings/account, progression diagram, reward calendar를 구분한다. paper theme가 맞는지 자체가 pilot 질문이다. gold border/noise/shadow/gradient 효과를 용도 없이 누적하지 않는다.

## 6. Responsive / interaction 명세

세 viewport와EN/KO 최소 조합, notch 좌/우·gesture bottom inset, ControlSize80/100/140, 긴 비용/이름/숫자/가격·오류/키보드열림 stress frames를 둔다.844에서 읽는 순서·touch target·가려지는 world를 먼저 검증하고16:9에 확장한다. layout constraints와 최소 type, scroll 방향/초기 위치/selection 보존을 써 놓는다.

Back은 호출 위치를 따르는 convention을 제안하지만 현재 예외/버그를 숨기지 않는다: Help 현재Account return(UX01), ClassTree→Overview, CommonTree→Character, PauseSettings→Pause, Inventory/Craft Close→Running, Death Home→world anchor. 변경된 return 규칙은 별도 controller 구현 과제로 표기한다. 하위 modal Cancel은 부모 상태/원본 stack을 보존한다.

처리중 재탭/닫기, Cancel과 실제 network operation 취소, late callbacks, account-bound reward, stale transfer를 디자인 flow에 포함한다. animation 시간과 입력 lock의 관계, disabled sound/success cue, scroll을 시작했을 때 slot drag가 아닌 상태, pointer release/exit cancellation을 명세한다.

## 7. Unity delivery와 approval

Penpot source/component IDs·export manifest·PNG/SVG resolution·9slice border·pivot·padding·font license·token 표·state transition 표·모든 SCREEN/ACT/NAV ID 연결을 묶어 전달한다. 문자열/상품가격/실제 아이템 수치를 이미지에 굽지 않는다. 기존 PNG/atlas를 재사용하면 원본 path와 asset identity를 기록한다. 새 art는 source ownership과 생성 경로를 갖춘다.

Unity integration은 별도 승인 후 canonical builders/serialized references/runtime-only builders를 변경해야 한다. Penpot export만으로 callbacks·drag/hold·safearea·service receipt·MP authority가 작동하는 것이 아니다. overlay/rebuild/폰트·texture memory/overdraw·GC·Android touch QA가 남는다.

F01–F03(여러 world·관리·solo/Host 동일world)는 FUTURE GOAL page에서만 설계한다. world index/schema/migration/identity/pending 거래 승인이 먼저다. F04–F08 미구현 mechanics와 X01 역사 UI를 새 제품 기능으로 채워 넣지 않는다. world/character art 교체는 별도 의존성이다. 최종 스타일과 기능 변경을 디자이너가 일방 확정하지 않는다.

## 8. 공식 공개 사례 비교 — 배치/미술 복제 아닌 원칙 검토

조사일2026-11-04. 공개 patch/지원 문서의 구체 동작을 읽었다. 스토어 이미지 몇 장이나 게임 이름만으로 전체 품질을 일반화하지 않는다. 옛 patch는 그 버전의 사례이며2026 현재 화면의 실측 증거가 아니다. 게임마다 재료·권위·저장 구조가 달라 mechanics를 그대로 이식하지 않는다.

| 게임 | 시스템 | 실제 사례/공식 근거 | 장점 | Lowfire 차이 | 적용 여부 | 이유·배제 대안 |
|---|---|---|---|---|---|---|
| Terraria Mobile | touch crafting/navigation | [505 1.4.3 patch](https://support.505games.com/support/solutions/articles/150000146926-terraria-console-mobile-v1-4-3-patch-notes-2022-april-27-): expand/collapse/close controls·touch crafting/filter·페이지별 Reset·toggle/hold Auto Select 명시 | touch 동작과 창 범위를 분명하게 함 | Lowfire는36slot/9hotbar·split-held·station queue·role hold 계약 | 원칙 채택 PROPOSAL | task별 controls/설정 범위·hold를 명세. Terraria inventory 배치/Auto Select mechanics 추가는 배제 |
| Crashlands2 | 정보 가독성/전환 | [공식1.2 patch](https://blog.bscotch.net/post/crashlands-2-patch-12/): stat name 영역 확장으로 언어 대응, loading tip 위치 안정화, feedback clipboard 확인 추가 | 긴 문구·복사/대기 행동의 결과가 명확 | Lowfire EN tabs word-middle wrap·loading 독립 style·support clipboard | 원칙 채택 PROPOSAL | 언어 스트레스 frame/행동 feedback 설계. 같은 patch의 compound autocraft는 Lowfire 재료예약/명시Collect 계약과 달라 배제 |
| Crashlands | inventory philosophy | [공식 게임 설명](https://www.bscotch.net/games/crashlands): inventory management 부담 제거를 제품 특징으로 삼음 | 정리 피로 감소 | Lowfire의36칸·Chest24/36·split/장비교환·Host revision은 실제 게임 규칙 | 직접 채택 안 함 | 가방 무한화 대신 선택/상세/목적지 위계를 개선. UX 개선을 경제·용량 변경으로 해결하지 않음 |
| Core Keeper | scroll/행동 결과 | [퍼블리셔1.0.0.16 PDF](https://fireshinegames.co.uk/wp-content/uploads/Core-Keeper-Patch-notes-Post-1.0-1.0.0.16.pdf): p3 여러 item salvage 요약 정확성, p2–3 ScrollView 화살표 기능, p5 disconnect/error UI 조정 | 결과 예측·숨은 내용 발견·연결 실패 이해 | Lowfire repair/refund/held ghost/queue·Host save-fail 분리 필요 | 원칙 채택 PROPOSAL | 정확한 비용/환급 preview·scroll cue·명시 실패 경로. 자동 salvage/새 네트워크 기능·UI 배치 복제는 배제 |
| Don't Starve Together Mobile | solo/Host/Join 이해 | [Playdigious 공식 지원](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/795-can-i-play-solo/?p=android): Host Game에서1인/Local Only로 혼자 플레이 안내 | world와 참여 인원 선택을 구분하여 설명 가능 | Lowfire는현재SP/Host고정파일·IsMultiplayer/HostId제약 | 미래 정보구조 참고만 | 같은 world solo/host FUTURE GOAL에 맞는 개념 후보. 기존 Lowfire가 호환된다는 증거가 아니므로 저장/권위 설계 전 버튼만 추가하는 대안 배제 |

비교 결론: 작은 글자를 대형 고정 panel 축소로 해결하지 않고, 명시 action/실제 비용/return context/scroll·상태를 보존한다. 무한 bag·자동 제작·새 station·새 reward는 이번 UI 인계에 추가하지 않는다. 외부 게임 자료로 Lowfire의 미관이나 Android 합격을 대신 판정하지 않는다.
