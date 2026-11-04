"""Finalize audit indexes/reports only. Does not change Unity or product files."""
import sys,json,hashlib,datetime,collections,os
sys.dont_write_bytecode=True
import audit_session as a
import write_reports as w
R=a.ROOT
REPORTS=['README','SCREEN_INVENTORY','CONTROL_AND_ACTION_MAP','NAVIGATION_AND_STATES','VISUAL_UX_ISSUES','UI_ARCHITECTURE_AND_ASSETS','MOBILE_AND_LOCALIZATION','WORLD_SAVE_AND_MULTIPLAYER_UX','PENPOT_DESIGN_HANDOFF','REDESIGN_ROADMAP','EVIDENCE_INDEX']
S=json.loads((R/'screens.json').read_text(encoding='utf8'))
ISS=json.loads((R/'issues.json').read_text(encoding='utf8'))
def write(n,t):w.write(n,t)
def table(h,rows):return w.table(h,rows)
def j(n,x):a.save(n,x)

def indexes():
 old={x['file']:x for x in json.loads((R/'live-evidence-index.json').read_text(encoding='utf8'))}
 contact={x['file']:x for x in json.loads((R/'evidence/review-sheets/manifest.json').read_text(encoding='utf8'))}
 rows=[]
 for p in sorted((R/'evidence/live').glob('*.png')):
  key=p.relative_to(R).as_posix();meta=json.loads(p.with_suffix('.json').read_text(encoding='utf8'))
  bad=p.name.startswith(('resume-inventory-','resume-hand-crafting-','resume-character-','resume-common-tree-','resume-pause-'))
  c=contact[p.name]
  row=dict(file=key,sha256=hashlib.sha256(p.read_bytes()).hexdigest(),utc=meta['utc'],viewport=meta['screen'],language=meta['language'],scene=meta['scene'],visuallyOpened=True,reviewMode='contact-sheet; selected originals also opened',reviewSheet='evidence/review-sheets/'+c['sheet'],tile=c['tile'],acceptedAsMenuEvidence=not bad,evidenceKind='loading/failure overlay ONLY; underlying menu excluded' if bad else 'rendered UI in stated QA state',note=old.get(key,{}).get('note',''))
  if bad:row['note']='Initial resume attempted callbacks before ready. Pixels show loading/failure, NOT named menu. No ordinary-player progression failure established. Clean retry/v2 succeeded.'
  rows.append(row)
 assert len(rows)==87 and sum(x['acceptedAsMenuEvidence'] for x in rows)==67
 j('live-evidence-index',rows)
 hist=json.loads((R/'historical-image-index.json').read_text(encoding='utf8'))
 extra=['resume-20261031/screens/chest-en-844.png','resume-20261031/screens/split-dialog-en-844.png','resume-20261031/screens/store-pending-fixture-ko-844.png','final-probe--wbiii-costs-ko.png','final-probe--wbiii-refunds-en.png','gameplay--pause-ko-844.png']
 for x in hist:
  if any(x['file'].endswith(y) for y in extra):x['visuallyOpenedInThisAudit']=True
 assert sum(x['visuallyOpenedInThisAudit'] for x in hist)==10
 j('historical-image-index',hist)

UNVERIFIED=[
 ('U01','Android 실제 touch/두 엄지·reach','PC Game View와 callback 관찰뿐. joystick/drag/scroll/hold 실제 기기·80/100/140% 필요'),
 ('U02','물리 notch/gesture inset·DPI','safe-area source와 옛 fixture만. 세 viewport는 실제 phone 아님'),
 ('U03','Android IME/feedback·room input','소프트 키보드·한글 composition/caret/Back 실기 미확인'),
 ('U04','접근성·최종 합성색 contrast','TalkBack·색각·동적 글자·광학 glyph/contrast ratio 전수 측정 안 함'),
 ('U05','실 provider IAP·pending/recovery','구매 OFF. 과거 fixture 가격·상태는 실제 결제가 아님'),
 ('U06','실 rewarded video/consent·receipt','광고 OFF. SDK 영상·닫기/실패·지급 체감 관찰 안 함'),
 ('U07','실 auth/cloud/account conflict','backend OFF. source state 계약만 조사'),
 ('U08','실 support submit·privacy/deletion','외부 전송/삭제 요청 안 함. deletion warning은 숨긴 callback QA preview'),
 ('U09','4인 Internet/LAN·waiting/client','실 Host/Join 세션 안 열음. route entry 화면만 관찰'),
 ('U10','Host save-failure/leave/disconnect','source 권위·예외 경로만. 실제 실패 유도/복구 안 함'),
 ('U11','SP death/ad/Gem/save pending','현재 사망·소비 안 함. source와 과거 death 화면만'),
 ('U12','MP downed/rescue/interruption','동료/24s/4s/late callback 실제 재현 안 함'),
 ('U13','item-heavy bag/장비/분할 commit','현재 빈 bag rendered, Chest/split은 역사 화면. transfer/full/stale 실제 거래 안 함'),
 ('U14','Station I–III/queue/upgrade 실제 처리','source와 역사 비용/반환 화면. enqueue/withdraw/collect/cost 소비 실행 안 함'),
 ('U15','Camp/POI supplies','runtime creator·Host revision source만. 현재 panel 외관/claim 미재현'),
 ('U16','Turret/Beacon/Compass/Telescope','희귀 runtime panels source만. 실제 장비·navigation 사용 미재현'),
 ('U17','Boss/야간/음향/자연 progression','HUD code와 이전 증거 후보. 이번은 실제 전 구간 플레이·Audio 청취 아님'),
 ('U18','Android 성능/발열/메모리','source allocation/texture 위험 조사만. profiler·build·기기 FPS 검증 안 함'),
 ('U19','multi-world migration/Gold source 정책','미래 미구현. world index/identity/pending path/solo-host 전환 설계·실험 필요'),
 ('U20','첫 resume loading timeout 및 rare modal 입력 누수','전환 중 드라이버 조작 영향 미분리. 깨끗한 재진입은 ready/overlay없음. 일반 route 반복/실기와 rare topmost raycast 별도 검증')]

def coverage():
 rows=[]
 live={'H01':'home-v2','H03':'settings-v2','M01':'resume-mp','P01':'class-overview','P02':'class-tree','A01':'profile-ko','A02':'account-ko','A03':'store-help-ko','A05':'delete-unconfirmed','E01':'achievements-v2','E02':'attendance-v2','E03':'attendance-monthly-v2','E05':'store-ko','G01':'qa-new-world-hud','I01':'resume-v2-inventory','I02':'resume-v2-inventory','C01':'resume-v2-hand-crafting','P04':'resume-v2-character','P05':'resume-v2-common-tree','S01':'resume-v2-pause-en','S02':'resume-v2-pause-settings'}
 historical={'I03','I04','I06','C02','C03','G08','D01'}
 for s in S:
  ids=[x['id'] for x in ISS if s['id'] in x['screens'].split('/')]
  if s['controller']=='—':level='FUTURE / MISSING / HISTORICAL ONLY';disposition='현재 구현 frame에 섞지 않음'
  elif s['id'] in live:level='CURRENT rendered subset';disposition=live[s['id']]+'; 해당 empty/OFF/preview 상태만. 상태 전체 합격 아님'
  elif s['id'] in historical:level='SOURCE + HISTORICAL';disposition='현재 rendered UNVERIFIED; 과거 fixture/행동 화면과 current source 차이를 분리'
  else:level='SOURCE; current rendered UNVERIFIED';disposition='state/controls 계약 인계; 외관·실기 정상 주장 안 함'
  rows.append(dict(id=s['id'],level=level,evidence=disposition,issues=ids,designDisposition='문제 '+','.join(ids)+' 참고' if ids else '현 기능 보존; per-screen state와 handoff frame variants를 기준으로 재설계/후속 검수'))
 j('screen-coverage',rows)
 p=R/'SCREEN_INVENTORY.md';t=p.read_text(encoding='utf8');t+='\n## 표면별 시각 증거·디자인 인계 판정\n\n현재 rendered subset은21종이며 locale/해상도 반복67장과 다른 단위다. source만으로 외관을 판정하지 않았다. 같은 ID의 모든 상태가 관찰됐다는 뜻이 아니다.\n\n'+table(['ID','검수 수준','증거·한계','관련 문제','인계 판정'],[(x['id'],x['level'],x['evidence'],','.join(x['issues']) or '특정 결함 미확정',x['designDisposition']) for x in rows]);p.write_text(t,encoding='utf8')
 return rows

def architecture():
 extra={'AttackIconView':'G03','ClassHoldButton':'G05–G08','ClassWorldFeedback':'G05/G11','ControlSizeLayout':'G02–G08','DirectionDrag':'G05/C08','EquipmentSlotDrag':'I02/I05','GameplayUiLayers':'G01/all gameplay modals','HotbarView':'G01/I01','InventoryDropZone':'I05','InventorySlotDrag':'I01/I06','InventorySlotView':'I01/I06','ItemStats':'I01/C01','JournalSkillNode':'P02/P05','Localization':'all EN/KO','LocalizedLabel':'all static labels','MedicineHud':'G07','ModalLayout':'all fixed panels','PlayerPortrait':'I01/P04','PlayerRescueMarker':'D02/D03','PlayerStaminaHud':'G01','ProfileHud':'G01/MP wallet','RecipeCardView':'C01','StaminaArc':'G01/G03','SurvivalHud':'G01','TimeHud':'G01'}
 manifest=json.loads((R/'source-manifest.json').read_text(encoding='utf8'));rows=[]
 for x in manifest:
  if '/Runtime/UI/' not in x['file']:continue
  cls=x['file'].split('/')[-1][:-3];owner='/'.join(s['id'] for s in S if s['controller']==cls) or extra.get(cls)
  assert owner,(cls,'unowned UI')
  rows.append((w.source(cls),owner,'view/binding/helper 계약. controller/service 권위·input/locale 수명과 분리하여 외관 대체'))
 p=R/'UI_ARCHITECTURE_AND_ASSETS.md';t=p.read_text(encoding='utf8');t+='\n## Runtime/UI51개 ownership 대조\n\n상위 표면에 포함한 adapter/helper도 누락하지 않았다. 실제 개별 source hash/method/binding은 source-manifest가 정본이다. Transition/Camp/TierThree는 runtime 생성 경로이며 scene-only 열거에 의존하지 않는다.\n\n'+table(['source','화면/계열','교체 경계'],rows);p.write_text(t,encoding='utf8')

def research():
 p=R/'PENPOT_DESIGN_HANDOFF.md';t=p.read_text(encoding='utf8');t+='''
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
''';p.write_text(t,encoding='utf8')

def overview():
 links='\n'.join('- ['+n+'.md]('+n+'.md)' for n in REPORTS)
 write('README','''# Lowfire 전체 UI/UX forensic audit — 최종 조사본

**요청한 연구·명세·근거 검토를 완료했다. 현재 UI를 상용 출시 품질로 승인한 보고서는 아니다.** 게임·Penpot·실서비스는 변경하지 않았다. 다음 작업은 사용자 승인 하의 디자인이며 자동 착수하지 않는다.

초기 제품 기준 main `48d6528758d18bf3df41ed02ad9aa0f268654a51`; 재개 fetch main `82d19b64de0157d924a05f87ea8ee698f5d6db6b`. 둘 사이 제품 source/data/docs/art는 동일하고 감사 체크포인트만 추가됐다. 실제 Unity는 `C:/UnityProjects/Wildfolk`6000.3.19f1이다. 기존 native/canonical 차이를 조사했으며 sync/authoring/build를 실행하지 않았다.

## 결론과 완결성

등록 표면66개 = 현재57개 + 미래·미구현·역사전용9개. 의미 action185개/직렬 control instance671개/조건별 route103개. 제품 UI씬3개, Runtime/UI51개·관련 runtime29개·Editor authoring25개를 source/scene/controller/service로 대조했다. 반복slot/recipe/class/언어를 화면 수로 부풀리지 않았다.

현재67장에 해당하는21종 표면의 제한된 상태를 직접 이미지로 검토했다. PNG총87장 중20장은 Loading/실패막에 가려져 메뉴 증거에서 제외했다. 과거180장 인덱스 중10장을 열람하고 현재 source와 구분했다. contact sheet는 비교용으로 축소했으며 선택 원본을 별도 열었다. **57개 모든 표면·모든 상태를 현재 실제 조작으로 검수한 것은 아니다.** source coverage와 visual coverage를 분리했고20개 미검증 질문을 EVIDENCE_INDEX에 남겼다. 이는 다음 작업에서 직접 검수해야 할 경계이며 누락을 합격으로 처리하지 않았다.

P0 확인0 / P1 7 / P2 15. Store→Help→Back이 Account로 돌아가는 문제는 현재 source+Unity로 확인했다. 작은 화면의 본문 축소, 가방·제작 밀도, 큰 maintenance 카드, 출석 표시/버튼 혼동, 반복 종이 panel, 비용·즉시변경 결과 노출이 주요 우선순위다. 과거 수정 기록을 지우거나 같은 결함이 남았다고 단정하지 않았다. 상세 신뢰 수준은 VISUAL_UX_ISSUES를 따른다.

## 기능 보존과 다음 요청

Save14/Profile2/Protocol23,36bag/9hotbar/장비4·Chest24/36,111recipes/WorkbenchI–III와처리방식,6common/6classes54nodes, 서버 receipt·Host 권위·pending recovery·무료 world Home를 보존한다. provider OFF와 offline read-only를 성공 거래처럼 꾸미지 않는다. multi-world/같은world solo-host는 현재 기능이 아니라 저장 identity·schema/network/Gold·pending path 정책이 필요한 별도 목표다.

Penpot 권장 pilot은 **Main Menu·Gameplay HUD·Inventory**다. Stage1 core tokens/components와 Stage2 세 pilot 방향·모바일 한영 variants부터 사용자 승인을 받는다. 전체를 같은 책 테마로 확정하지 않으며 기능을 줄여 화면을 단순화하지 않는다. 실제Penpot board·Unity 통합·새 AAB/RC·배포는 이번에 하지 않았다.

## 보고서11개

'''+links+'''

## 실제 검증·보호

EVIDENCE_INDEX는 source/scene·현재/역사캡처·20미검증·adversarial20항목을 연결한다. final-validation.json은11문서/로컬링크/line/ID/hash/scope/controls 대조 결과다. resume-protection-check.json은 실제 사용자 파일·native3362파일·prefs registry hash의 전후비교다. Unity는MainMenu/Edit/PlayOFF/dirty없음/QA marker없음,GameView1920×1080 복원. Save/Protocol변경없음. 과거 checkpoint 기록·제외캡처를 삭제하지 않았다.

자동 gameplay/NUnit/data validator/Android build·물리터치·실제광고/결제/계정삭제/4인세션·전progression/오디오청취는 수행하지 않았다. 이번 report-only 변경의 검증과 거래·출시 QA를 혼동하지 않는다.

기존 RESUME.md/checkpoint-validation.json은 당시 중간 기록이다. 현재 완료 여부는이 README와final-validation을 따른다. 시간은timing.json과timing-resume-20261104.json의실제boundary이며사용자중단공백은합산하지않았다. token/billing 작업별수치는도구에서미제공(null)이며추정하지않았다.
''')

def evidence():
 rows=json.loads((R/'live-evidence-index.json').read_text(encoding='utf8'))
 hist=json.loads((R/'historical-image-index.json').read_text(encoding='utf8'))
 text='''# Evidence index — 최종 조사·미검증 경계

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

'''
 text+=table(['이번열어본과거이미지','Gitimagecommit/date','해석'],[('['+x['file'].split('/')[-1]+']('+os.path.relpath(a.CAN/x['file'],R).replace('\\','/')+')',x['lastImageCommitAndDate'],'HISTORICAL; fixture/pending가격은실service아님' if 'fixture' in x['file'] else 'HISTORICAL; current source대조만') for x in hist if x['visuallyOpenedInThisAudit']])
 text+='''
기존Krita [FieldJournal](evidence/source-art/LowfireFieldJournal.png)/[Wordmark](evidence/source-art/LowfireWordmark.png)mergedpreview2개를직접열람했다. 기존Penpot는옛Home책시안이며사용자지시로MCP연결/보드작업은하지않았다. 전체새UI원본이이미있다고주장하지않는다.

## UNVERIFIED20개

항목수는후속검증질문20개이며미관찰개별상태/화면의개수가아니다. 한질문안의관련상태를묶었다. 화면별정확한수준은screen-coverage를따른다.

'''+table(['ID','미검증영역','이유/후속검증'],UNVERIFIED)
 text+='''
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
'''
 write('EVIDENCE_INDEX',text)
 j('unverified',[dict(id=i,area=n,reason=r,status='UNVERIFIED') for i,n,r in UNVERIFIED])

if __name__=='__main__':
 indexes();coverage();architecture();research();overview();evidence()
 print('Closed 11 reports;87 indexed/67 accepted current frames;20 explicitly unverified.')
