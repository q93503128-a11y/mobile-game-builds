"""Remaining research specification; no Penpot or game mutations."""
import sys,json
sys.dont_write_bytecode=True
import audit_session as a
import write_reports as w
from finish_audit import COMMON,table,src,shot,old
S=json.loads((a.ROOT/'screens.json').read_text(encoding='utf8'))
def mobile():
 text='# Landscape mobile · localization · 접근성\n\n'+COMMON+'\n'
 text+=table(['영역','1920×1080','1280×720','844×390','언어/한계'],[
 ('Home/Settings/업적/신규·월간출석','현재 capture+비교 열람','현재 capture+비교 열람','현재 capture+비교 열람','EN/KO. 동일 상태의 desktop Game View이며 physical phone 아님'),
 ('Inventory/Hand Craft','resume-v2 현재 capture','resume-v2 현재 capture','resume-v2 현재 capture','EN/KO 신규 빈 가방/재료 없음; item-heavy 상태 미검증'),
 ('Character/Common Tree/Pause/Pause Settings','source/역사','source/역사','resume-v2 capture','844 EN/KO; 3viewport 전체라고 확대하지 않음'),
 ('Store/Profile/Account/Class/Help','source/역사','source/역사','현재 KO capture','OFF/new profile; auth/owned/pending 실 서비스 상태 아님'),
 ('MP entry Internet/LAN','resume EN capture','source/역사','현재 KO capture','Host/Join 실제 세션 생성 안 함; waiting/clients source·역사'),
 ('queues/Chest/split/turret/beacon/Death/Boss','source·일부 역사','source·일부 역사','source·일부 역사','이번 progression·fixture로 새 rendered 상태 만들지 않음')])
 text+='''
## FACT: 현재 화면 적응 방식

CanvasScaler는1920×1080 reference, matchWidthOrHeight=.5이다. SafeAreaLayout은 Screen.safeArea/실제 Screen 크기로 anchors를 갱신한다. ModalLayout은 SafeArea에서 reserve를 뺀 공간과 reference panel 비율을 비교해 한 번에 축소한다. 이는 breakpoint별 열 재배치가 아니다. 이미지가 모두 화면 안에 들어가도 본문이 읽기 좋은 크기라는 보장은 없다.

ControlSizeLayout은 .8–1.4 범위의 joystick visual/primary/interact/dash/hotbar/bag와 class touch 위치를 조정한다. 메뉴 본문·node/close/button 전체를 확대하는 접근성 설정이 아니다. joystick은 넓은 입력 zone과 first-touch 위치의 transient visual을 분리하고 sprint는 .9 hold/.72 유지 등 stamina 정책으로 작동한다. 별도 Sprint button을 새 디자인에 이미 존재하는 기능처럼 넣지 않는다.

현재 legacy Text는 Noto Sans KR 정적 폰트를 쓴다. L/Localization.json은 en/ko2개 언어, LocalizedLabel은 OnEnable/L.Changed, 동적 panel은 자체 Refresh를 쓴다. Settings 언어는 사용자 preference이며 world save와 무관하다. `L.Format`은 placeholder 숫자/문구를 합성하므로 문장을 이미지로 굽거나 UI에서 문자열을 별도로 복사해 관리하지 않는다.

## 측정값을 해석하는 방법

동명 capture JSON의 `pixelFont`는 fontSize×transform.lossyScale.y, target pixelWidth/Height는 실제 RectTransform 크기×lossyScale이다. 화면에 나온 glyph의 optical 높이·물리 dp·엄지 정확도와 같지 않다. 특히 gameplay 뒤의 비활성 HUD는 metadata에서 active라 보여도 CanvasGroup이 입력을 차단한다. target 통계는 enabled/실제 topmost view와 함께 읽는다.

844 inventory의128reference slot은 약49render pixel, item 선택 제목은 약13pixel/slot hint는 약9pixel 수준이다. Craft 재료24reference는 약9pixel, 공용 tree 일부72reference Back은 약26pixel 높이까지 줄어든다. 값은 resume-v2의 동명 JSON을 정본으로 삼는다. “120reference니까120px touch”라고 주장하지 않는다. Loading에 가려진 첫 resume 캡처의 underlying metadata는 visual evidence로 채택하지 않았다.

## 미래 Penpot용 위험 matrix

| 위험 | 현재 근거 | 16:9 두 viewport | 844×390 | PROPOSAL / 수용 기준 |
|---|---|---|---|---|
| 일괄 축소 | ModalLayout.Apply | 상대 spacing 같아도 글자 wrap 차이 | 세로390 제한으로 본문 작음 | 최소 type 먼저, 열/그룹/scroll 재배치; 정보 삭제 금지 |
| Safe Area | 실제 anchor code; 옛 inset fixture | reference margin 확보 | 양쪽 notch/하단 gesture 추가 감소 | 모든 frame에 safe/content bounds; 실기 inset 별도 검수 |
| Thumb reach | left floating/right controls/9hotbar | 모니터 클릭으로 편의 판단 불가 | full-width bottom belt·class action 혼잡 | 양손 실제 reach map; 80/100/140%와 role variants |
| Tap/hold/scroll | pointer ownership·.45s/.18s 분기 | mouse와touch가 다름 | chest scroll/drag 오인 위험 | 실제 touch에서 fast vertical scroll·longpress·partial placement 취소 |
| 한영 expansion | word-middle wrap; 설명/비용 string | EN tab210폭에 단어 분리 | 작은 label/footer가 더 압축 | 각 언어 layout sample; 의미 있는 줄바꿈; 임의 font shrink 금지 |
| 숫자 증가 | classXP/Gold/수량/time/다중비용 | 긴 숫자 suffix | 작은 slot count/9number | max-count·large-value stress text frame 포함 |
| 입력 keyboard | Feedback multiline1500; MP destination64 | desktop keyboard는 screen 안 가림 | Android IME가 viewport 차지 | keyboard-open/selection/caret/submit/Back cancel frame, 실기 검수 |
| 대비/색 | disabled palette/selected learned/valid green | 배경·URP·scrim 조합 | 작은 glyph와 muted fill | status를 색만으로 표시하지 않기; 최종 실제 합성색 contrast 측정 |
| screen-reader/접근성 | sendNavigationEvents=false/UGUI touch 중심 | 키보드 adapter는 게임 이동용 | TalkBack semantics 미검증 | 접근성 대응 범위를 제품 결정. screenshot으로 compliance 주장 금지 |
| motion/audio | spinner·toast·stamina fade·AudioButton | Editor 청취/기기측정 없음 | frame pacing·haptic·mute 가시성 | cue state와 reduced-motion 대안 설계; 없는 haptics를 FACT로 넣지 않음 |

## PROPOSAL: 디자인 token/레이아웃 요구

- 844×390에서 필수 본문은 우선14–16 logical px 상당의 읽기 후보로 디자인하고16:9에서도 hierarchy를 유지한다. 실제 Unity pixel/DPI로 환산·실기 승인 전 최종 최소값으로 확정하지 않는다.
- 주요 touch target은44–48 logical px 후보, destructive/긴 hold는 더 넓은 간격을 검토한다. 아이콘 그림과 hit area는 분리한다. 844render pixel을 dp라고 부르지 않는다.
- 정보가 많으면 큰 종이 전체 축소보다 scroll/detail disclosure·breakpoint별 열수 변화로 해결한다.36slot/9hotbar/recipe111/node 수를 줄이지 않는다.
- 긴 KO/EN 문장·빈 값·—·긴 가격·서버 오류·pending·large XP·재료6줄·긴 item name·날짜 UTC를 실제 데이터/승인 문구로 채운다. fake 가격/credential을 넣지 않는다.
- viewport 세 가지마다 safe area0 기본 + notch 좌/우 + gesture bottom 시안을 둔다. 실제 Screen.safeArea 적용과 기기 터치는 후속 Unity QA의 별도 gate다.

## 감사 결과와 범위

현재 메뉴45개와 재개 관찰 자료의 image/metadata는 EVIDENCE_INDEX를 따른다. bad-driver first matrix와 overlay-가림 캡처는 별도 기록했다. visible target과 underlying panel data를 혼동하지 않는다. 이 보고서는 Android DPI/화면 확대/TalkBack/색각/contrast 전수 적합성·60fps·multitouch 합격 인증이 아니다.
'''
 w.write('MOBILE_AND_LOCALIZATION',text)

def handoff():
 text='# Penpot 전체 UI 재설계 인계 명세\n\n'+COMMON+'\n**산출물은 설계 명세이며 실제 Penpot board를 만들지 않았다.** 최종 스타일·새 동작·멀티월드 구현은 승인 전 미확정이다. 이 문서와 SCREEN_INVENTORY의 현재 controls, CONTROL_AND_ACTION_MAP의185개 action, NAVIGATION의103route를 함께 인계한다.\n\n'
 text+='''## 1. 목표와 불변조건

Lowfire의 landscape survival/crafting 정체성을 살리면서 task hierarchy·읽기 크기·조작 결과를 일관되게 만든다. 숲·불씨·잉크·기존 wordmark는 재사용 후보지만 모든 화면을 책/과도한 양피지/금장/gradient로 만드는 스타일을 확정하지 않는다. 전체 화면을 같은 oversize 종이와 동일 사각 버튼으로 반복하지 않는다. 랜덤 fantasy ornament로 빈 공간을 채우지 않는다.

현재 기능 보존: Save14/프로필2/Protocol23·내부Wildfolk identity,36bag/9hotbar/장비4, Chest24/36,111recipes/WorkbenchI–III/Batch·Sequential 구분,6common skills/6classes54nodes, Host authoritative inventory/시설/보상, 서버 account/Gem/claim/verified purchase, 무료 world Home 복귀와 death revision, disabled provider gates. 데이터 수량·가격·수율·경제/전투 수치를 디자인 편의 때문에 바꾸지 않는다.

## 2. 작업 파일 구조와 frame 명명

PROPOSAL page groups:00 Brief/Token;01 Components;02 Home/World;03 Gameplay;04 Inventory/Crafting;05 Progression;06 Multiplayer;07 Account/Records/Store;08 Death/Transition;09 Responsive/States;10 FutureGoals. 각 현재 frame은 `SCREEN-ID / State / EN|KO / Viewport`, component는 의미 role로 이름을 붙인다. screen counts는 같은 item/class 반복이나 locale별 frame 복제 수와 다르다.

FACT 아래57개 현재 표면은 모두 구현 인계 대상이다. generic parent 한 장만 그려 child/rare/disabled를 빼지 않는다. H01 valid/empty/corrupt, C01 station variant와 E05 purchase state 같은 상태는 실제 logic과 연결한 frame variants로 풀어 쓴다. F/X는 현재 구현 frame에서 분리한 future/reference page다.

'''
 rows=[]
 for s in S:
  if s['controller']=='—':continue
  rows.append((s['id'],s['name'],s['states'],s['controls'],s['entry']+' → '+s['exit']))
 text+=table(['Frame ID','과제','필수 state variants','현재 control 보존','진입/이탈'],rows)
 text+='''
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
'''
 w.write('PENPOT_DESIGN_HANDOFF',text)

def roadmap():
 text='# 재설계 순서·승인·검수 기준\n\n'+COMMON+'\n**아래 Stage1–6는 미래 계획이다. 이번 감사에서는 어느 Stage의 디자인·코드 구현도 시작하지 않았다.**\n\n'
 text+='''## Pilot 판단

| Pilot | 역할 | 먼저 고정할 것 | 통과 조건 |
|---|---|---|---|
| Main Menu | 브랜드/주행동/부행동/forest와UI 조화 | H01 valid·empty·failure와 current new-world warning | Continue/새세계/Join/설정의 의미, 한영 type, provider OFF 안내가 즉시 읽힘 |
| Gameplay HUD | world 가독성/생존 위계/실제 thumb overlay | HP·needs·time·selectedhotbar9·primary/interact/dash·class variants | 필수 world 시야 확보,80/100/140%·각role·modal·death input ownership 명세 |
| Inventory | dense 데이터/component 재사용 stress |36slot·장비4·portrait·item detail·split/drag/drop | 긴 설명/수량/charges·호환표시·빈/full bag·scroll/hold가 모두 설명됨 |

Home이 브랜드를 세우기 좋지만 그것만 승인받고 전체 화면을 같은 책으로 확장하지 않는다. HUD는 overlay, Inventory는 density를 검증하므로 세 pilot을 같은 단계에서 서로 대조한다. 구현 순서는 dependencies 때문에 core tokens→slot/button/scroll/modal→Home/HUD/Inventory→Craft/Character→서비스/rare overlay가 적절하다. multi-world는 별도 architecture 결정 전 현재 Home 뒤 기능으로 붙이지 않는다.

## 단계별 handoff

| 단계 | 실제 산출물 | 선행·위험 | 수용 기준·USER APPROVAL GATE |
|---|---|---|---|
| Stage1 Token/core components | semantic tokens/type/spacing/actions/slots/scroll/modal/node·export conventions | 이번 audit의 state 계약·작은 화면 type·기존 art ownership | **Gate A: 사용자에게 시각 방향과 core kit 승인.** 최종 스타일·색을 혼자 고정하지 않음 |
| Stage2 Three pilots | Home/HUD/Inventory,3viewport·EN/KO·주요 state | all-sized sheet 자동확장 위험; role/class/touch 제약 | **Gate B: 세 pilot 실제 프레임 비교 승인.** 기능축소/가짜 controls 없이 상용 품질 후보 |
| Stage3 Cross-screen validation | task flow/Back/disabled/keyboard/long content review | prototype가 실제 callback·서버 기능처럼 오인될 위험 | **Gate C: navigation 변경·행동 문구·responsive 규칙 사용자 승인.** UX01 등 변경도 명시 |
| Stage4 Remaining screens |57current surfaces의 frames/variants + future page 분리 | rare/runtime panels·provider OFF·downed omitted 위험 | **Gate D: 전체 current feature checklist/상태/문구·exportmanifest 승인.** Future mechanics는 별도 |
| Stage5 Unity prep | source assets/import/slice/reference/builder·controller 변경계획/검수범위 | generated scene-only수정 재생성손실; identity·transaction 의존성 | **Gate E: 실제 Unity 통합 범위 사용자 승인.** 구현 전 rollback·원본 보존/Save·Protocol 영향 검토 |
| Stage6 Unity integration + QA | canonical source/authoring·실제 assets·관련검사·실기 검수 | callback·pointer·local actor/Host·서비스·DPI/성능 regression | **Gate F: 사용자 PC/Android 체감 승인과 technical checks.** 디자인시안·batchpass만으로 release 승인 금지 |

## 기능 유지 검수 gate

1. source SCREEN57·ACT185·NAV103별 대조표에 구현 후 새 evidence를 붙인다. 변경한 flow/제거한 action은 사용자 승인 근거를 기록한다.
2. Core controls: split confirm은 들기만, 목적지 성공 전 원본 보존; empty release/cancel·full bag·equipment4·charges/quality·stale revision.
3. Craft/queue: category/filter/NEW·station tier·정확 cost·Batch/Sequential·예약/withdraw/Collect·blocked output·long costs. 자동 수령/새 station 기능을 임의 추가하지 않음.
4. Navigation: StoreHelp caller, 일반 origin Back, pause settings caller, class/common tree, state reset, modal raycast/동시 pointer release, loading/cancel/late result.
5. Authority: Client replica는 지급·차감하지 않음; account/economy/purchase receipt·pending/unknown 유지; offline cache spending 금지. 실제 service 활성화는 별도 운영 승인.
6. Death/save: 무료 Home, 죽음/run revision·reserve/world commit/finish, Save14/backup/저장실패 화면 유지·Host leave/client disconnect. 사용자 실제 파일 미사용.
7. Responsive:3viewport EN/KO·notch·long text/값·80/100/140%·physical Android2엄지/IME·sound/mute·frame pacing.
8. Assets: editable source·license·stable IDs/slice/9slice·no missing refs, RGBA padding·font glyph·texture memory/overdraw/Canvas rebuild·GC.

## 별도 architecture 또는 art 승인이 필요한 일

multi-world index/Continue semantics/same-world solo-host/rename/delete/migration; 새로운 language modal 또는 비용확인 단계 같은 동작 변경; world/character/boss art redesign; quest/map/durability 등의 새 기능; provider setup/live payments/ad/production은 각각 다른 scope다. 감사에서 제안했다고 승인된 구현으로 취급하지 않는다.

지금 다음 권장 작업은 **Stage1~2의 명시 디자인 요청과 세 pilot 비교 승인**이다. 바로 전체 scene rewrite나 새 AAB/RC2/production 배포로 넘어가지 않는다.
'''
 w.write('REDESIGN_ROADMAP',text)

if __name__=='__main__':mobile();handoff();roadmap();print('Wrote mobile, Penpot handoff and redesign roadmap.')
