# Evidence index — 최종 조사·미검증 경계

초기main48d6528 / 재개fetch main82d19b6. 감사 외 제품 변경 없음. FACT(source)/CONFIRMED(관찰)/SOURCE-INFERRED/HISTORICAL/PROPOSAL/FUTURE/UNVERIFIED를 각 보고서에서 구분했다. 캡처 파일명만으로 화면을 판단하지 않고 pixels/동명metadata·실제route를 대조했다.

## Source·scene·asset 근거

| 인덱스 | 범위·해석 |
|---|---|
| [source-manifest](source-manifest.json) | 선택된C#105개 hash/line/method/binding. Runtime/UI51ownership은 architecture에 전부 연결. 모든 프로젝트 코드의 전줄 의미검토라는 뜻은 아님 |
| [scene-inventory](scene-inventory.json) / [scene-surfaces](scene-surfaces.json) | MainMenu/Wildfolk/Multiplayer3제품씬 Canvas/관련component·원본line. inactive는dead아님 |
| [serialized-controls](serialized-controls.json) |671control instance. runtime Bind와 pointer 경로는actions에서추가 |
| [screens](screens.json) / [screen-coverage](screen-coverage.json) |66표면의현재57/미래등9구분,각표면 visual수준·한계·문제연결 |
| [actions](actions.json) / [navigation](navigation.json) |185callback/gesture+103route. 비용·권위·persistence와단순선택분리 |
| [source-art-index](source-art-index.json) | KRA/SVG등10원본 hash/metadata. KRA merged2preview직접열람,과거HomeSVG3·Lucide7원본/라이선스확인 |
| [native-canonical-comparison](native-canonical-comparison.json) | 씬whitespace·prefabblockorder·AudiofolderGUID4·Editorplayoption차이. runtimeC#·UI layout동일,동기화안함 |
| [issues](issues.json) |22이슈,관찰/추론/역사증거와구현경계 |

source/line link는 각상세보고서에있다. 최신README와ARCHITECTURE/DESIGN_BIBLE/HOME_FRONTEND/UI_METAGAME/UI_RESEARCH/INVENTORY/CRAFTING/CHARACTER_PROGRESSION/LOCALIZATION/MULTIPLAYER_FOUNDATION/SAVE_PERSISTENCE/ECONOMY/SOURCE_ART및기존validation/design/art-source를대조했다. dev-playbook remote main의global/development규칙도읽었다. 공식외부사례는PENPOT_DESIGN_HANDOFF의구체비교표와원문URL을따른다.

## 현재Unity이미지:87장 중67장 메뉴증거 채택

[live-evidence-index](live-evidence-index.json)는모든PNG의SHA256/실제UTC/viewport/언어/씬/검토sheet/채택여부를기록한다. [review manifest](evidence/review-sheets/manifest.json)는15contactsheet의tile→원본map이다. 이전45장은이번재개에전체비교열람했다. 새22장도검토했다. 축소sheet는픽셀정밀합격검사가아니며가독성/위계판단은선택원본과메타데이터를같이읽었다. 동명JSON은실제Text/Selectable상태이나가려진메뉴가보였다는증거가아니다.

| 그룹 | 실제관찰범위 | 한계 |
|---|---|---|
| home/settings/achievements/newcomer/monthly-v2 |EN/KO×1920/1280/844|저장없음/OFF.실기touch아님|
| Store/Help/HelpBack/Profile/Account/Class |KO844|빈profile·providersOFF.구매/claim안함|
| delete-unconfirmed |QA warning preview|Linked gate통과아님·Confirm실행안함|
| MPverified/새resume-mp |KO844/EN1920 Internet/LAN setup|Host/Join세션없음.옛transportfilename일부는route와다르므로pixels우선|
| resume-v2-inventory/hand-crafting |EN/KO×3viewport|새세계빈bag·재료없음.제작거래안함|
| resume-v2-character/common-tree/pause/pause-settings |EN/KO844|0point/새캐릭터.투자/저장안함|
| qa-new-world-hud |KO844|실제새세계시작부분만.전progression아님|

### 잘못된감사조작/가림증거배제

첫frontend루프는Attendance.Back없이Home.ShowHome를호출하여overlay가남았다. [attempt1](live-frontend-attempt1.json)에원인기록,evidence/rejected는보존·Git제외. 정상Back후v2로대체했다.

이번첫resume20장은전환완료전메뉴callback을호출했고Loading/실패막이메뉴를가렸다. `resume-inventory/hand-crafting/character/common-tree/pause*`의메뉴명은실제visible상태가아니다.20장을삭제하지않고acceptedAsMenuEvidence=false로명시했다. [첫navigationlog](resume-navigation-observed.json)는그시도기록이다. 가려진capture는메뉴증거/일반사용자P0근거로쓰지않는다.

새QA진입에서조기callback없이기다린결과 [clean-entry-observation-1](clean-entry-observation-1.json)의overlay=false/pending0/terrain0/Running을확인했다. 이후각capture직전TransitionOverlay.Visible이면중단하도록하여 [v2 navigationlog](resume-v2-navigation-observed.json)와20장v2를확보했다. 첫timeout의자연재현/프레임throttling/드라이버영향은U20미검증이다. 자연조작처럼마우스/키보드플레이했다고주장하지않는다.

## 역사·원본

[historical-image-index](historical-image-index.json)는기존4validationdir180PNG를인덱싱,그중10장직접열람했다. Git추가/수정commit/date는실제runtimebuildSHA와같다고보장하지않는다. 정확한과거capturebuild를알수없는경우현재합격증거로격상하지않았다.

| 이번열어본과거이미지 | Gitimagecommit/date | 해석 |
|---|---|---|
| [chest-en-844.png](../commercial-ui-mobile/resume-20261031/screens/chest-en-844.png) | 144a56a0b002b614bc6c23dcfe2b75de50eccd25 2026-10-31T16:00:51+09:00 | HISTORICAL; current source대조만 |
| [split-dialog-en-844.png](../commercial-ui-mobile/resume-20261031/screens/split-dialog-en-844.png) | 144a56a0b002b614bc6c23dcfe2b75de50eccd25 2026-10-31T16:00:51+09:00 | HISTORICAL; current source대조만 |
| [store-pending-fixture-ko-844.png](../commercial-ui-mobile/resume-20261031/screens/store-pending-fixture-ko-844.png) | 144a56a0b002b614bc6c23dcfe2b75de50eccd25 2026-10-31T16:00:51+09:00 | HISTORICAL; fixture/pending가격은실service아님 |
| [final-probe--wbiii-costs-ko.png](../post-commercial-ui-qa-tools/evidence/final-probe--wbiii-costs-ko.png) | 0ae9feae389c24abae40bc68da9569dd2eb5a676 2026-11-02T19:47:35+09:00 | HISTORICAL; current source대조만 |
| [final-probe--wbiii-refunds-en.png](../post-commercial-ui-qa-tools/evidence/final-probe--wbiii-refunds-en.png) | 0ae9feae389c24abae40bc68da9569dd2eb5a676 2026-11-02T19:47:35+09:00 | HISTORICAL; current source대조만 |
| [gameplay--death-ko-844.png](../post-commercial-ui-qa-tools/evidence/gameplay--death-ko-844.png) | 0ae9feae389c24abae40bc68da9569dd2eb5a676 2026-11-02T19:47:35+09:00 | HISTORICAL; current source대조만 |
| [gameplay--hud-repair-ko-844.png](../post-commercial-ui-qa-tools/evidence/gameplay--hud-repair-ko-844.png) | 0ae9feae389c24abae40bc68da9569dd2eb5a676 2026-11-02T19:47:35+09:00 | HISTORICAL; current source대조만 |
| [gameplay--inventory-en-844.png](../post-commercial-ui-qa-tools/evidence/gameplay--inventory-en-844.png) | 0ae9feae389c24abae40bc68da9569dd2eb5a676 2026-11-02T19:47:35+09:00 | HISTORICAL; current source대조만 |
| [gameplay--pause-ko-844.png](../post-commercial-ui-qa-tools/evidence/gameplay--pause-ko-844.png) | 0ae9feae389c24abae40bc68da9569dd2eb5a676 2026-11-02T19:47:35+09:00 | HISTORICAL; current source대조만 |
| [variants--class-3-tree-en-844.png](../post-commercial-ui-qa-tools/evidence/variants--class-3-tree-en-844.png) | 0ae9feae389c24abae40bc68da9569dd2eb5a676 2026-11-02T19:47:35+09:00 | HISTORICAL; current source대조만 |

기존Krita [FieldJournal](evidence/source-art/LowfireFieldJournal.png)/[Wordmark](evidence/source-art/LowfireWordmark.png)mergedpreview2개를직접열람했다. 기존Penpot는옛Home책시안이며사용자지시로MCP연결/보드작업은하지않았다. 전체새UI원본이이미있다고주장하지않는다.

## UNVERIFIED20개

항목수는후속검증질문20개이며미관찰개별상태/화면의개수가아니다. 한질문안의관련상태를묶었다. 화면별정확한수준은screen-coverage를따른다.

| ID | 미검증영역 | 이유/후속검증 |
|---|---|---|
| U01 | Android 실제 touch/두 엄지·reach | PC Game View와 callback 관찰뿐. joystick/drag/scroll/hold 실제 기기·80/100/140% 필요 |
| U02 | 물리 notch/gesture inset·DPI | safe-area source와 옛 fixture만. 세 viewport는 실제 phone 아님 |
| U03 | Android IME/feedback·room input | 소프트 키보드·한글 composition/caret/Back 실기 미확인 |
| U04 | 접근성·최종 합성색 contrast | TalkBack·색각·동적 글자·광학 glyph/contrast ratio 전수 측정 안 함 |
| U05 | 실 provider IAP·pending/recovery | 구매 OFF. 과거 fixture 가격·상태는 실제 결제가 아님 |
| U06 | 실 rewarded video/consent·receipt | 광고 OFF. SDK 영상·닫기/실패·지급 체감 관찰 안 함 |
| U07 | 실 auth/cloud/account conflict | backend OFF. source state 계약만 조사 |
| U08 | 실 support submit·privacy/deletion | 외부 전송/삭제 요청 안 함. deletion warning은 숨긴 callback QA preview |
| U09 | 4인 Internet/LAN·waiting/client | 실 Host/Join 세션 안 열음. route entry 화면만 관찰 |
| U10 | Host save-failure/leave/disconnect | source 권위·예외 경로만. 실제 실패 유도/복구 안 함 |
| U11 | SP death/ad/Gem/save pending | 현재 사망·소비 안 함. source와 과거 death 화면만 |
| U12 | MP downed/rescue/interruption | 동료/24s/4s/late callback 실제 재현 안 함 |
| U13 | item-heavy bag/장비/분할 commit | 현재 빈 bag rendered, Chest/split은 역사 화면. transfer/full/stale 실제 거래 안 함 |
| U14 | Station I–III/queue/upgrade 실제 처리 | source와 역사 비용/반환 화면. enqueue/withdraw/collect/cost 소비 실행 안 함 |
| U15 | Camp/POI supplies | runtime creator·Host revision source만. 현재 panel 외관/claim 미재현 |
| U16 | Turret/Beacon/Compass/Telescope | 희귀 runtime panels source만. 실제 장비·navigation 사용 미재현 |
| U17 | Boss/야간/음향/자연 progression | HUD code와 이전 증거 후보. 이번은 실제 전 구간 플레이·Audio 청취 아님 |
| U18 | Android 성능/발열/메모리 | source allocation/texture 위험 조사만. profiler·build·기기 FPS 검증 안 함 |
| U19 | multi-world migration/Gold source 정책 | 미래 미구현. world index/identity/pending path/solo-host 전환 설계·실험 필요 |
| U20 | 첫 resume loading timeout 및 rare modal 입력 누수 | 전환 중 드라이버 조작 영향 미분리. 깨끗한 재진입은 ready/overlay없음. 일반 route 반복/실기와 rare topmost raycast 별도 검증 |

## Adversarial self-review20항목

아래PASS는해당조사·분류/인계가수행됐다는뜻이다. live미검증을통과로바꾸지않는다.

| 요청check | 감사판정/증거 | 실행검증한계 |
|---|---|---|
| 모든UI씬 |PASS:scene-inventory3/scene-surfaces|다른시스템Editor/QA씬은제품surface로세지않음|
| UIbuilders |PASS:25authoring/source-manifest/architecture|재생성안함|
| runtimepanelcreator |PASS:Camp/TierThree/Transition/Feedback/source/ownership|rare외관U15–16|
| modal/notification |PASS:NAV/GameplayUiLayers/PlayerFeedback/UX18|rareinputleakU20|
| localization |PASS:L/LocalizedLabel/동적Refresh·EN/KO matrix|IME/기기U03|
| source-backednavigation |PASS:NAV103/source links/Mermaid|모든route실행아님|
| significantcontrols |PASS:ACT185+671instance/gesture/runtimeBind|비용거래U05–14|
| SPflow |PASS:Home/save/new/pause/deathsource·현재newgame|save/death거래U11|
| MPflow |PASS:setup/Host/client/authority/leave·미래구분|realpeerU09–10|
| offline/backendOFF |PASS:실제OFF화면/featuregate/source|enabledU05–08|
| account/store/reward |PASS:currentpreview+receipts/pending계약|외부transactionU05–08|
| inventory/craft/repair |PASS:현재빈bag/handcraft+source/역사|거래/유지보수U13–14|
| character/class/skill |PASS:현재신규preview/tree+Gold/profile계약|투자/구매거래안함|
| death/recovery |PASS:currentcode+역사화면·cost/runrevision|liveU11–12|
| Penpot/Krita |PASS:Git원본/mergedpreview/sourceownership|사용자지시Penpot연결불필요|
| 과거audit대조 |PASS:4dirindex/10열람·교정기록과현재분리|180전부열람아님|
| visual문제근거 |PASS:UX22개에path/source/confidence|정량contrastU04|
| actual/future분리 |PASS:FACT/INFERRED/HISTORICAL/FUTURE·worldtable|multi-worldU19|
| component인계 |PASS:57frame+variant/component/token/export/gates|실제Penpot/Unity통합안함|
| 제품무수정 |PASS:scope/hash검사·resume-protection-check|사용자파일경로/값은private보호|

## 보호·실행상태·검증

[resume-protection-check](resume-protection-check.json)는이번사용자파일6·native3362파일·prefs2registryroot를전후hash/size/mtime로비교한다. rawregistry값/credential은Git에쓰지않았다. 최초checkpoint의registry검사미수행기록은당시대로보존한다. [resume-editor-final](resume-editor-final.json):MainMenu/Edit/PlayOFF/pausedfalse/dirtyfalse/QAmarker빈값.GameView1920×1080복원. [resume-console](resume-console.json)은마지막error/warning조회0건이며과거모든로그무오류나전체game합격의뜻은아니다.

[final-validation](final-validation.json)은문서/ID/링크·line/hash/통제된Git범위대조다. tests/build/기기QA는미수행이며위20미검증을남겼다. 시간은 [timing](timing.json)과 [resume timing](timing-resume-20261104.json). 작업별token/billing은미제공(null). 체크포인트당시의RESUME/validation을지우거나완료였던것처럼고쳐쓰지않는다.
