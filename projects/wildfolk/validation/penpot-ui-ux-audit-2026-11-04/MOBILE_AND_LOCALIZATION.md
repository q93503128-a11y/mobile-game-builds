# Landscape mobile · localization · 접근성

감사 재개 시 fetch한 main: `82d19b64de0157d924a05f87ea8ee698f5d6db6b`. 제품 source/data는 이전 `48d6528`과 동일하다. game 변경·Penpot 제작 없이 조사했다.

FACT/CONFIRMED는 현재 source/이번 audit의 실제 화면, SOURCE-INFERRED는 source로 도출한 동작·위험, HISTORICAL은 과거 캡처, PROPOSAL은 제안, FUTURE GOAL은 사용자 차기 목표, UNVERIFIED는 검증 범위 밖이다. 디자인 심각도는 감사 우선순위이며 모든 항목을 출시 차단 코드 버그로 해석하지 않는다.

| 영역 | 1920×1080 | 1280×720 | 844×390 | 언어/한계 |
|---|---|---|---|---|
| Home/Settings/업적/신규·월간출석 | 현재 capture+비교 열람 | 현재 capture+비교 열람 | 현재 capture+비교 열람 | EN/KO. 동일 상태의 desktop Game View이며 physical phone 아님 |
| Inventory/Hand Craft | resume-v2 현재 capture | resume-v2 현재 capture | resume-v2 현재 capture | EN/KO 신규 빈 가방/재료 없음; item-heavy 상태 미검증 |
| Character/Common Tree/Pause/Pause Settings | source/역사 | source/역사 | resume-v2 capture | 844 EN/KO; 3viewport 전체라고 확대하지 않음 |
| Store/Profile/Account/Class/Help | source/역사 | source/역사 | 현재 KO capture | OFF/new profile; auth/owned/pending 실 서비스 상태 아님 |
| MP entry Internet/LAN | resume EN capture | source/역사 | 현재 KO capture | Host/Join 실제 세션 생성 안 함; waiting/clients source·역사 |
| queues/Chest/split/turret/beacon/Death/Boss | source·일부 역사 | source·일부 역사 | source·일부 역사 | 이번 progression·fixture로 새 rendered 상태 만들지 않음 |

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
