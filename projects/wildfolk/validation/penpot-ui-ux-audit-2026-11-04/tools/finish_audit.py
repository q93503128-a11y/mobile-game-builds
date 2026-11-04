"""Author the research handoff from inspected evidence. Writes only this audit directory."""
import sys,json,collections,os,re,hashlib,datetime
from pathlib import Path
sys.dont_write_bytecode=True
import audit_session as a
import write_reports as w
R=a.ROOT
def write(name,text):w.write(name,text)
def table(h,rows):return w.table(h,rows)
def src(cls,m=None):return w.source(cls,m)
def shot(name):return '['+name+'](evidence/live/'+name+'.png)'
def old(name):return '['+name+'](../post-commercial-ui-qa-tools/evidence/'+name+'.png)'
COMMON='''감사 재개 시 fetch한 main: `82d19b64de0157d924a05f87ea8ee698f5d6db6b`. 제품 source/data는 이전 `48d6528`과 동일하다. game 변경·Penpot 제작 없이 조사했다.

FACT/CONFIRMED는 현재 source/이번 audit의 실제 화면, SOURCE-INFERRED는 source로 도출한 동작·위험, HISTORICAL은 과거 캡처, PROPOSAL은 제안, FUTURE GOAL은 사용자 차기 목표, UNVERIFIED는 검증 범위 밖이다. 디자인 심각도는 감사 우선순위이며 모든 항목을 출시 차단 코드 버그로 해석하지 않는다.
'''
ISSUES=[]
def issue(id,severity,screen,problem,evidence,impact,cause,recommend,boundary,status='CONFIRMED'):
 ISSUES.append(dict(id=id,severity=severity,screens=screen,problem=problem,evidence=evidence,impact=impact,cause=cause,recommendation=recommend,boundary=boundary,status=status))
def issues():
 issue('UX01','P1','E05/A03/A02','Store에서 연 Help의 Back이 Account로 돌아간다.',shot('store-help-back-ko-844')+'; '+src('StoreScreen','Contact')+'; '+src('AccountProfileScreen','CloseHelp'),'구매 문의 후 상품/이력 맥락을 잃고 불필요한 단계가 생김','Store.Close의 fromHome만 존재; 하위 Help origin 없음','동일 Help component에 caller/return target을 명시하고 Store/Account 경로를 구분','navigation controller 변경 필요; support submit/계정 서비스는 보존')
 issue('UX02','P1','H03/E01/E02/E03/I01/C01/P02','모바일에서 panel 전체 축소에 의존하여 본문·수치가 너무 작아진다.',shot('settings-v2-en-844')+'; '+shot('resume-v2-inventory-en-844')+'; '+src('ModalLayout','Apply'),'비용·소유/필요·상태를 읽는 시간이 늘고 dense UI에서 오조작 위험','1920 reference font와 fixed panel을 scale-to-fit; ControlSize는 메뉴 font를 키우지 않음','844 기준 실제 읽기 크기부터 정하고 열/세부정보를 재배치; type 최소크기와 scroll을 함께 검증','layout/type 교체; 비용/slot/recipe 축소 금지')
 issue('UX03','P1','E02/E03','출석의 표시 전용 칸들이 큰 버튼과 같은 표면을 사용하며 월간28칸이 균일하게 빽빽하다.',shot('attendance-monthly-v2-ko-844')+'; '+src('EconomyScreen','Refresh'),'tap할 수 있는 보상·단순 stamp·오늘 수령 action의 구분이 약함','Button으로 작성한 타일을 runtime 비활성; 날짜·stamp·Gem 텍스트 반복','RewardTile과 actionable claim을 별개 component로 설계; 오늘/획득/미래는 icon+문구+대비로 구분','신규7/월간28·server UTC·하나의 claim action·실제 보상 유지')
 issue('UX04','P1','G08','현장 수리/회수 카드가 큰 화면 폭을 차지하며 두 긴 행동과 재료 설명이 동시에 전투 시야를 점유한다.',old('gameplay--hud-repair-ko-844')+'; '+src('MaintenanceUiBuilder','Arrange'),'근처 기지에 있을 때 이동·위험 관찰보다 운영 패널이 앞서 보임','840×360 contextual root, title/cost/refund/actions/hint가 항상 함께 표시','대상·상태·주 행동부터; 상세 비용/반환은 읽기 쉬운 확장영역. Hold 진행은 행동 옆에 유지','환급/비용/PlayerBuilt/contents/hold 중단·Host 검증은 그대로','HISTORICAL + SOURCE-INFERRED')
 issue('UX05','P1','I01/I02/C01/C02/C03','가방·제작이 거대한 utility panel에 작은 설명/상태/비용을 함께 담아 핵심 작업보다 데이터 해독이 앞선다.',shot('resume-v2-hand-crafting-ko-844')+'; '+shot('resume-v2-inventory-ko-844')+'; '+src('CraftingUiBuilder','Arrange'),'새 플레이어가 재료 부족/출력 수령/장비 선택의 다음 행동을 빠르게 구분하기 어려움','fixed 다열 grid와 선택 상세, 다중 재료 string, 관련 action 위치 분산','dense pilot에서 선택-필요재료-명시 행동 순서 수립; 장비4/36slot과 station 구분 유지','UI 재배치만; 자동제작·무한가방·queue 자동수령 추가 금지')
 issue('UX06','P1','H03/S02/G13','결과를 바꾸는 행동의 확인·비용 노출 계약이 일관되지 않다. Reset은 즉시, 왕묘 Interact는 조건 통과 시 바로 Bone10을 소비한다.',src('UserPreferences','ResetDefaults')+'; '+src('RoyalTomb','Use')+'; '+src('BossWorld','Summon'),'Reset을 단순 navigation으로 오해하거나 왕묘에서 자원 소비를 미리 예측하기 어려울 수 있음','일반 Action와 mutation action의 공통 affordance; 왕묘 전용 확인 화면 없음','Reset 범위와 즉시 반영을 명시. 왕묘는 현재 비용/조건을 action 전에 전달하는 UX 검토','새 confirmation/interaction 단계는 미래 동작 변경 승인 필요. 이번에는 추가 안 함','SOURCE-INFERRED')
 issue('UX07','P1','A01/A02/E01/E03/P01/P02','거의 같은 대형 종이 sheet/직사각 action을 여러 과제에 반복해 계정 관리·기록·진행·보상의 시각적 성격이 희미하다.',shot('profile-ko-844')+'; '+shot('class-overview-ko-844')+'; '+shot('account-ko-844'),'상용 품질의 과제별 위계가 약하고 빈 영역과 과밀 영역이 공존','JournalUiBuilder.Sheet(1720×1000), 동일 소재·rule·action vocabulary 반복','책 테마를 기본값으로 고정하지 말고 핵심 task별 container를 고른다. 숲/잉크/불씨 정체성은 후보로 비교','최종 미술 스타일은 사용자 pilot 승인 전 확정 금지')
 issue('UX08','P2','E01/E05/P02','비활성 action의 밝은 글자/옅은 배경이 읽기 어려우며 이유가 다른 영역에 놓인다.',shot('achievements-v2-ko-844')+'; '+src('JournalStyle','Action'),'offline/point부족/이미선택/구매불가를 단순 회색 버튼으로 읽게 됨','공용 disabled palette와 별도 status text','disabled label 가독성 보존 + action 가까이 짧은 사유; 불가 상태도 디자인 variant로 작성','비활성 gate를 우회하지 않음')
 issue('UX09','P2','E01','영문 Progression/Exploration이 일부16:9 캡처에서 단어 중간 줄바꿈된다.',shot('achievements-v2-en-1920')+'; '+shot('achievements-v2-en-1280'),'헤더 스캔 방해·번역 품질이 낮아 보임','210-wide tab/font32·pixel scale 변화·임의 word wrap','짧은 승인 라벨/가변 폭/의도된2행 규칙을 선택, 3viewport EN/KO 검수','문구 변경은 차기 localization 정본 패스에서')
 issue('UX10','P2','E01/C01/I06/G08','일부 상세와 목록은 scroll이 필요하지만 mask와 바닥 절단만으로 그 사실을 알리기 어렵다.',shot('achievements-v2-en-1920')+'; '+old('final-probe--wbiii-costs-ko')+'; '+src('CraftingUiBuilder','Build'),'잠금·tier·추가 비용을 없는 정보로 오해할 수 있음','여러 ScrollRect가 scrollbar/fade/remaining cue 없이 존재; 일부는 scrollbar 있음','scroll edge cue·선택 위치 유지·독립 scroll의 역할 구분; mask 밖 내용을 overflow 버그로 잘못 복구하지 않음','기존 모든 내용·필터와 scroll 접근성 유지')
 issue('UX11','P2','H03/S02','Language가 다음 언어로 순환하는 일반 버튼이며 슬라이더와 Reset의 범위도 시각상 설명이 적다.',shot('settings-v2-ko-844')+'; '+src('SettingsPanel','NextLanguage'),'선택 목록을 기대한 사용자가 동작을 예상하기 어려움','cycle callback; 공용 action component','현재 EN/KO의 명시 selection variant와 값 feedback을 설계; Reset 범위 안내','selection modal 추가 시 keyboard/Back 경로까지 명세')
 issue('UX12','P2','G01/G10/G12','HUD가 여러 가장자리에 작은 수치·class·time·상황문구로 분산되어 우선 관찰 대상이 약하다.',shot('qa-new-world-hud-ko-844')+'; '+src('CharacterUiBuilder','ArrangeHud'),'생존 경고/전투·상호작용/목표를 동시에 읽기 어려움','독립 HUD controller와 고정 좌표 배치','HP/needs/상태, navigation/context, thumb actions의3영역으로 pilot 검증','SP에 항상 Gold/Gem이 표시된다고 가정 금지; 현재 MP Wallet과 구분')
 issue('UX13','P2','P02/P05','작은 node icon·짧은 이름·lock/seal이 여러 branch에서 비슷하게 보인다.',shot('class-tree-ko-844')+'; '+shot('resume-v2-common-tree-en-844'),'노드 미리보기 선택과 학습/잠금/point부족 구분이 약함','128cell atlas의 비슷한 경량 조합 + 120reference node','기호군의 굵기·실루엣·상태 badge/선 두께 통일, 선택 상세 위계 강화','실제6/54node와 prerequisite/point 유지; 가짜 node 없음')
 issue('UX14','P2','H01/G03/P01','Lucide 선 아이콘, 자체 node 도형, item sprite, fist/portrait의 시각 언어가 혼재한다.',shot('home-v2-en-844')+'; '+shot('qa-new-world-hud-ko-844')+'; '+src('JournalSymbolAssets','Names'),'동일 제품이라는 인상이 약함','용도별 다른 제작 원본/획선·표현밀도','navigation/action/stat/item 용도별 규칙을 정하고 같은 역할 안에서 통일','Item 의미/라이선스/지급 ID 유지; 단순 전체 monochrome 변환은 채택 안 함')
 issue('UX15','P2','H01/H02/M01','Single Player는 관리 화면이 아니라 새 세계 시작이고 Host는 별도 고정 저장을 사용한다.',src('HomeScreen','SinglePlayer')+'; '+src('SaveRepository','MultiplayerDefault'),'사용자의 같은 world solo/host·비파괴 선택 목표와 현재 흐름이 다름','단일 SP/Host 파일과 IsMultiplayer/HostId gate','현재 warning 유지, 미래 World Card/Continue-selected-world UX는 별도 architecture 승인 후','현 기능 버그/이미 multi-world 구현으로 보고하지 않음','SOURCE-INFERRED / FUTURE GAP')
 issue('UX16','P2','M01','route 전환 때 이전 연결 오류 문구가 남아 반대 route의 안내와 섞일 수 있다.',shot('multiplayer-verified-internet-ko-844')+'; '+src('MultiplayerMenu','ToggleRoute'),'Room Code와 IPv4 중 무엇을 고쳐야 하는지 혼란','route 전환은 Destination만 지우고 NetworkSession.MessageKey는 유지','route-scoped validation/error 수명 정의; 입력 field 바로 옆 설명','새 네트워크 메커니즘이 아닌 UI state lifetime 검토','SOURCE-INFERRED + audit capture')
 issue('UX17','P2','I07/C05/C06/C07','runtime supplies/포탑/navigation 패널은 최근 공용 journal/field 작성 경로와 별도 스타일 helper를 쓴다.',src('CampLootPanel','Build')+'; '+src('TierThreeHud','Build'),'드물게 여는 고급 시스템에서 다른 제품처럼 보일 위험','fixed 1080/1100 panel + 자체 Label/Button palette','희귀 표면도 Penpot component/반응형 계약에 포함','외관은 source 위험; 해당 rare panel 새 runtime 캡처는 UNVERIFIED','SOURCE-INFERRED')
 issue('UX18','P2','G11','토스트가 단일 label을 짧게 교체하므로 여러 결과가 연속하면 중요한 실패/환급 정보가 묻힐 수 있다.',src('PlayerFeedback','ShowKey')+'; '+src('PlayerFeedback','ShowMaintenance'),'bag-full/save-failure와 성공 pickup을 구분해 재확인하기 어려움','1.5–3s 단일 until/key/label, queue 없음','우선순위/지속시간/중복 합산 정책 설계; 중요 실패는 행동 가까이 유지','새 알림센터/로그 저장 mechanics는 자동 추가하지 않음','SOURCE-INFERRED')
 issue('UX19','P2','E05/E06','구매 OFF인데 상품3개·부활환산·구매복구/이력/문의가 한 화면에서 공간을 계속 차지한다.',shot('store-ko-844')+'; '+src('StoreScreen','Refresh'),'현재 할 수 없는 과제에 주의를 빼앗기고 하단 링크가 약함','기능 구현을 유지한 unavailable preview 상태','OFF/계정필요/진행중/완료를 독립 frame으로 간결하게 구성','fake 가격/무료 Gem/실구매 활성화 없음')
 issue('UX20','P2','T01','Loading의 기본 Text Lowfire·단색막·arc가 Home wordmark/캠프 표현과 다르다.',src('TransitionOverlay','Create')+'; [과거 연결 화면](../commercial-ui-mobile/completion/connecting-en-844.png)','전환이 제품 경험보다 시스템 대기 화면처럼 보임','runtime 독립 canvas/text/arc','기존 로고/선택한 motion token을 재사용하되 실제 phase/취소/실패 명확성 우선','가짜 progress %/불필요한 overdraw 추가 금지','HISTORICAL + SOURCE-INFERRED')
 issue('UX21','P2','G09/G13/World','월드 광석·생물 실루엣·수면 재질과 UI polish의 격차가 전체 품질 인상에 남는다.',shot('qa-new-world-hud-ko-844')+'; [이전 forensic](../forensic-presentation-world-audit-2026-11-03/FORENSIC_AUDIT.md)','UI만 교체해도 게임 전체가 상용 완성도로 보장되지 않음','절차/Pillow 기반 alpha art와 서로 다른 제작 시점','UI pilot 배경은 실제 게임 화면을 사용하고 world art 의존성은 별도 승인 항목으로 분리','생물/물 수정·새 character art는 이번 범위 밖. facing correction을 미수정 버그로 재보고하지 않음','HISTORICAL + current context')
 issue('UX22','P2','D01/H01/C03','Home이라는 말이 메인 야영지 메뉴, 무료 사망 복귀 지점, Workbench 나침반 목표에 걸쳐 쓰인다.',old('gameplay--death-ko-844')+'; '+src('DeathHud','Respawn')+'; '+src('CraftingPanel','SetHome'),'사망 화면에서 앱 메뉴로 나가는 것으로 오해할 수 있음','별도 system anchor에 같은 용어','실제 목적지에 맞는 행동 문구와 간단한 결과 설명을 Penpot에 명시','무료 world respawn을 main menu 전환으로 바꾸지 않음','HISTORICAL + SOURCE-INFERRED')
 a.save('issues',ISSUES)
 text='# 시각·UX 문제와 우선순위\n\n'+COMMON+'\n**P0 확인0 / P1 7 / P2 15.** P1은 중요한 체감·가독성·navigation 문제, P2는 polish·정보설계·source 위험이다. crash/save 거래의 전면 무결성 인증은 아니다. 서버 OFF/미구현 future goal 자체를 임의 P0로 올리지 않았다.\n\n'
 text+=table(['ID','등급','화면','구체적 문제','증거 수준'],[(x['id'],x['severity'],x['screens'],x['problem'],x['status']) for x in ISSUES])+'\n'
 for x in ISSUES:
  text+=f'## {x["id"]} · {x["severity"]}\n\n'+table(['항목','내용'],[('화면',x['screens']),('문제',x['problem']),('근거',x['evidence']),('영향',x['impact']),('원인',x['cause']),('제안',x['recommendation']),('변경 경계',x['boundary']),('신뢰/재현',x['status']+'; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도')])+'\n'
 text+='''## 먼저 다룰10개

UX01 → UX02 → UX05 → UX04 → UX03 → UX07 → UX06 → UX10 → UX08 → UX09. 먼저 origin/행동 계약을 잠그고, type·slot·dense layout을 pilot에서 검증한 뒤 장식·아이콘으로 간다. 실제 구현은 차기 별도 승인 후다.

## 과거 교정과 현재 판단 분리

HP 숫자/bar 정렬, repair와 hotbar 직접 겹침, same-frame pause의 maintenance 잔상, paper frame 밖 Back, 항상 떠 있는 Sync, Class prerequisite/point 구분, floating joystick ownership, 공용 AudioButton 누락은 옛 교정 기록이 있다. 이번 source/화면에서 그 원래 문제를 동일하게 다시 확인하지 못했다. 현재 큰 maintenance 카드·작은 본문·동일 panel 반복은 그 교정을 취소하면 자동 해결되는 문제가 아니다. rollback은 권장하지 않는다.

현재 canvas/scene data가 과거 캡처 시점과 같은지 대조했고, 48d6528 이후 이 감사 외 제품 변경은 없다. 그래도 역사 fixture는 자연 progression/physical mobile proof가 아니다. 보스 가독성·real ad/video·IAP pending 실제 체감은 UNVERIFIED로 남긴다.
'''
 write('VISUAL_UX_ISSUES',text)

def world():
 text='# 현재 저장/협동 UX와 미래 multi-world 경계\n\n'+COMMON+'\n'
 text+=table(['현재 FACT','source','UX 의미'],[
 ('SP world-v1.json / Host multiplayer-world-v5.json 고정 slot',src('SaveRepository','Default'),'임의 여러 world 목록 없음. 두 모드 slot이 존재한다는 것과 multi-world는 다름'),
 ('Home Continue 검증; SinglePlayer 새 seed + 교체 확인',src('HomeScreen','SinglePlayer'),'Single Player가 world manager인 것처럼 wireframe 금지'),
 ('primary→backup 검증·checksum·temporary flush·atomic replace',src('SaveRepository','Read'),'invalid/backup/read error를 명확한 카드 상태로 보존; silent new world 금지'),
 ('Host load는 IsMultiplayer + HostId==LocalProfile 필요',src('NetworkSession','Host'),'현재 SP 파일을 Host repository로 지정해도 바로 이용 불가'),
 ('공유 WorldSnapshotCodec + MP participant snapshot',src('NetworkWorldPersistence','Capture'),'공유 terrain/시설 codec은 재사용 가능; Player mirror/host metadata 추가 필요'),
 ('Client join은 host 파일/동료 진행의 권위가 아님',src('NetworkInventory','Execute'),'Join한 world를 로컬 소유 목록으로 복제한 것처럼 표시 금지'),
 ('Host migration 없음; host 종료→세션 종료',src('NetworkSession','Leave'),'world 관리와 방 세션 수명을 분리; client exit 의미 따로'),
 ('GoldSources2개; 표시 max Gold, Purchase first eligible source',src('ClassProfiles','GoldAvailable'),'world가 늘면 자금 출처 선택 정책 필요; 임의 합산 금지'),
 ('Class Gold intent/PendingXP/GemReviveIntent가 WorldPath 참조',src('ClassRewardPersistence','Recover')+' · '+src('CloudGemReviveWallet','Reconcile'),'파일 rename/delete/move가 unresolved 거래를 단절시킬 수 있음')])
 text+='''
## FUTURE GOAL — 사용자가 요구한 UX

Continue는 가장 최근 플레이한 유효 world를 재개한다. Single Player는 world 선택/관리로 들어가고 같은 world를 혼자 열거나 Host할 수 있다. 다른 사람 world에 Join하는 길은 따로 둔다. solo-world/MP-world를 불필요하게 별도 보관함으로 나누지 않는다. 새 world를 만든다고 기존 world를 파괴하지 않는다. rename/delete를 안전하게 제공한다. **현재 구현 상태가 아니다.**

## 변경 종류별 분리

| 영역 | presentation만 가능한 것 | 시스템 변경/결정이 필요한 것 | 수용 기준 |
|---|---|---|---|
| 정보구조 | Play/Join/Settings의 역할·명칭·Back 시안 | Continue의 선택 repository 결정 | 마지막 유효 world와 실제 repository가 일치 |
| World card | 이름/최근일/일차/호스트 가능 상태의 mock 구조 | world index·stable identity·metadata·유효성 열거 | 손상/backup/미호환/사라진 파일을 구분 |
| 생성 | form·취소·확인 hierarchy | 파일 위치/중복 이름/seed 정책/슬롯 commit | 생성 실패가 기존 world/index를 파괴하지 않음 |
| rename | 편집/성공·실패 frame | 표시명 metadata와 파일명 분리, pending path 유지 | 순수 rename으로 receipt·save reference가 안 바뀜 |
| delete | 명시 대상·영구영향·재확인·cancel 디자인 | live session lock·pending 거래·backup/trash 정책 | active/pending 대상 삭제 제한, 실패 recoverability |
| 같은 world solo/host | Play alone/Host 액션 선택 디자인 | IsMultiplayer/HostId/participants 전환·소유·신규 참가자 정책 | host player mirror·개인 inventory/life/home가 유지 |
| Join | 별도 entry·code/address input | Session 연결은 기존 경계 재사용; remembered remote list는 신기능 | local ownership과 구분, host-left recovery 유지 |
| Cloud | 명확한 계정/세계 저장 범위 문구 | 현재 CloudProfile은 세계 업로드 아님; world cloud는 별도 설계 | 지원 안 하는 world sync를 약속하지 않음 |

## 같은 solo world를 Host로 열 수 있는가

SOURCE-INFERRED: terrain/시설/resource/boss snapshot codec을 공유하므로 재사용의 출발점은 있다. 그러나 현재 Host는 multiplayer metadata와 로컬 HostId를 요구하고 SP는 그 값이 없다. 기본 transport/scene만 바꾸면 안전하게 된다는 결론은 성립하지 않는다. 변환(또는 공통 world envelope) 정책, owner character를 어느 participant로 삼을지, remote 재입장, solo 복귀 시 다른 참가자의 보존 방식, save validation, pending journal reference를 설계해야 한다.

PROPOSAL: world의 stable ID와 표시 이름/최근 플레이 metadata를 별개로 관리하고 session mode는 열 때 선택하는 방향을 우선 평가한다. 이는 확정 schema가 아니다. Save14 payload를 유지한 외부 index만으로 충분한지, 새 envelope/schema가 필요한지는 위 identity/participant invariant를 정한 뒤 결정한다. Protocol23/Save14 버전을 지금 임의로 올리지 않는다.

## 아직 정해야 할 제품 결정

1. 최근 플레이의 정의: load 성공/실제 world 진입/정상 save 중 언제 기록할지, 잘못된 recent entry fallback.
2. world 이름 길이·중복 허용·KO/EN 입력·금칙/경로문자 처리. 표시 이름을 파일 경로로 직접 쓰지 않는 방향 권고.
3. Guest/linked 프로필 교체와 local Host identity 변경의 관계; ownership 이전을 단순 rename처럼 취급하지 않기.
4. Gold class 구매의 payer world 선택과 전체 표시 방식. 현재 max/first eligible 동작을 무단 합산 wallet으로 바꾸지 않기.
5. world 삭제 시 backup/trash/보관기간·복구 UI, 미확정 revive/Class 거래를 먼저 해결하는 정책.
6. 여러 world를 동시에 열 수 있는지, 같은 파일의 solo/Host 동시 사용 lock, 불완전 종료 시 recoverability.

## Migration·테스트 계획 — 이번에는 미실행

기존 고정 SP/Host 파일과 backup을 읽기/검증한 뒤 새 위치/index를 준비하고, 새 상태를 검증하기 전 원본을 제거하지 않는다. pending intent를 모르면 자동 이동/삭제하지 않는다. 기존 participant/SaveId/seed/generation/boss reward/night identity와 Class receipt를 보존한다. 기존1–13→14 reader 및 checksum/backup 정책을 유지한다.

검수: primary 손상/backup 정상, 두 slot 동시에 존재, 새 생성 실패, 중복·긴 이름/경로문자, rename 도중 종료, 삭제 취소·잠긴 world, pending Gold/ClassXP/GemRevive, solo→Host→solo 왕복, remote reconnect/Host 종료, dead/downed normalization, 기존1/2/3 generation과 session identity. 이런 테스트가 없이 Penpot world card가 생겼다는 이유로 multi-world READY라고 하지 않는다.
'''
 write('WORLD_SAVE_AND_MULTIPLAYER_UX',text)

def architecture():
 text='# UI 작성·원본 자산·Unity 통합 경계\n\n'+COMMON+'\n'
 text+='''## 실제 pipeline

```mermaid
flowchart LR
 KRA[Krita 원본2개] --> PNG[UI PNG]
 SVG[Lucide SVG7개] --> PNG
 PY[Pillow generators] --> PNG
 PNG --> Import[VisualArtImporter / JournalSymbolAssets]
 Import --> Builders[Editor builders / Arrange]
 Builders --> Scene[3 serialized UGUI scenes]
 Scene --> Bind[Runtime state and actor binding]
 Bind --> UI[Text / Image / Button / ScrollRect]
 Runtime[CampLoot / TierThree / Transition] --> UI
```

주요 제품 UI는 UGUI Canvas/CanvasScaler/GraphicRaycaster/RectTransform과 legacy Text다. UI Toolkit/UXML 기반 제품 UI로 재기술하지 않는다. Editor builder가 scene을 작성한 것이 “게임을 켤 때 모든 UI를 생성한다”는 뜻은 아니다. 런타임 생성은 아래 예외 경로다. 세 scene과671개 control의 상세는 source-manifest/scene-inventory/serialized-controls에 남겼다.

'''
 text+=table(['presentation group','작성 원본','runtime truth / bind','교체 경계'],[
 ('Home·정적 야영 배경',src('FrontendSceneBuilder','Build'),'HomeScreen·SceneFlow','scene layout/배경 art; save 행동 유지'),
 ('공용 sheet/book/action/node',src('JournalUiBuilder','Sheet')+'; '+src('JournalStyle','Action'),'JournalSkillNode states','재질/색/획선/spacing·type; gate/노드 수 유지'),
 ('Account/Profile/Store/기록',src('AccountUiBuilder','Build')+'; '+src('EconomyUiBuilder','Build')+'; '+src('StoreUiBuilder','Build'),'Account/Economy/StoreService','view와 presenter 상태 유지; 서버 계약 변경 금지'),
 ('HUD/touch/slot',src('GameplayHudBuilder','Build')+'; '+src('GameplayUiFactory','Button'),'Session·Joystick·HoldInput·ProfileHud','tap/hold/multitouch/cancel 의미 유지'),
 ('Inventory/Chest/Split',src('InventoryUiBuilder','Arrange')+'; '+src('ChestUiBuilder','Arrange')+'; '+src('StackSplitUiBuilder','Arrange'),'InventoryModel/Drag/Equipment/Host revisions','36/9/24/36 및 destination 성공 전 원본 보존'),
 ('Craft/queue/upgrade',src('CraftingUiBuilder','Arrange'),'PlayerCrafting/StationProcessing','item/recipe/queue/tier gate를 source에서 바인딩'),
 ('Class/Common growth',src('JournalContentAssets')+'; '+src('CharacterUiBuilder','BuildInformation'),'ClassProfiles/PlayerProgression','world growth와 profile class를 별개로 유지'),
 ('수리·해체',src('MaintenanceUiBuilder','Arrange'),'ClassHud/PlayerBuildingService','정확한 cost/refund/hold/release/context'),
 ('Death/Downed',src('DeathUiBuilder','Build'),'PlayerDeath/Wallet/NetworkPlayer','무료 Home 및 reserve/commit/late callback 보존'),
 ('MP local UI',src('NetworkInventoryUiBuilder')+'; '+src('MultiplayerUiBuilder','Build'),src('NetworkInventoryUi','Bind'),'scene callback을 런타임 local actor에 교체함. 복사된 오래된 listener 사용 금지'),
 ('Runtime-only exceptions',src('CampLootPanel','Build')+'; '+src('TierThreeHud','Build')+'; '+src('TransitionOverlay','Create'),'supplies/turret/navigation/real scene·network phase','새 디자인 통합 때 scene builder만 고쳐서는 반영 안 됨'),
 ('Feedback catch icon',src('PlayerFeedback','Awake'),'action event + 짧은 transient state','toast policy와 icon layout; reward truth 아님')])
 text+='''
## 원본·asset inventory

| 자산/원본 | 현재 사용/생성 | 재사용 판단·주의 |
|---|---|---|
| art-source/LowfireWordmark.kra | Art/lowfire_wordmark.png | 편집 원본·merged preview 확인. 브랜드 후보; 사용자 승인 없이 재제작 안 함 |
| art-source/LowfireFieldJournal.kra | Art/lowfire_journal.png | 큰 제본 bitmap. 모든 화면에 재사용할 필요 없음. 현재 9slice panel과 다름 |
| docs/design/lowfire-home-a.svg,b.svg,c.svg | 옛 Penpot Home3안의 portable SVG | 역사 concept; 전체 UI screen/component kit가 아님. 새 Penpot 연결/보드 없음 |
| art-source/navigation-symbols/*.svg + sources.json | Lucide7개→ui_play/users/settings/profile/award/calendar/next PNG | 실제 source commit/hash·ISC/Feather 고지 유지. UI tint 적용 |
| generate_journal_icons.py | node/focus/seal/lock/Gold/Gem 도형 | 자체 Pillow; Crystal과 PremiumGem 분리 |
| generate_journal_symbols.py + JournalSymbolAssets | 11×6 atlas,128cell66개(Class6/common6/classnodes54) | sprite names/IDs·slice metadata 유지. 기존 개별 world symbol과 다른 사용처 |
| generate_class_icons.py | class6 PNG | 자체 기하 원본, journal atlas와 목적 차이 명시 |
| generate_visual_assets.py | 기본 sprite·ground/panel/controls 등, ecology 확장 | 실행하면 다른 art까지 덮을 수 있음. 이번 실행 안 함 |
| generate_metal/world/exploration/content/tier_three/creature_loot/medicine/food/fishing/sidegrade/boss_art.py | 각 item/world/creature/POI/보스 art | 현재 alpha presentation; UI redesign을 이유로 재생성 금지 |
| NotoSansKR.ttf / NotoSansKR-OFL.txt | Resources font, legacy Text | 현재 EN/KO 실제 font; 게임 문자열을 PNG에 굽지 않음 |
| panel.png + .meta | 18px 사방 border, Image.Type.Sliced | 실제 일반 panel 9slice 대상. lowfire_journal도 자동 9slice라고 가정하면 안 됨 |
| Art/WorldSprite.mat | Universal Render Pipeline/2D/Sprite-Lit-Default | MCP 실제 shader 조회. world lighting 영향; UGUI default material과 구분 |
| UI material | 보통 m_Material=0 UGUI default | 새 shader/mask는 성능·색·Canvas batching 검증 필요 |
| WorldSpriteRenderer/URP lighting | 월드/아이템/portrait layers | GUI vector export로 actor pose/방향/장비 renderer가 자동 대체되지 않음 |
| AudioRuntime/AudioButton/Lowfire mixer | 기존 버튼 click/back·action cue | 새 view에서도 binding 유지; click음은 reward 성공 receipt가 아님 |

자산hash/KRA ZIP metadata는 source-art-index.json. 일반 Importer는 Sprite Single/PPU100(ground예외)/Bilinear/Clamp/no mipmap/CompressedHQ/4096 상한 경로를 갖는다. journal atlas는 Multiple/FullRect/Uncompressed/2048 상한/NPOT None인 별도 importer다. 현재 파일 meta가 builder의 모든 기본과 같다고 가정하지 않는다. import 코드는 이번에 실행하지 않았다.

## Penpot export ≠ 작동하는 Unity UI

| 항목 | 인계해야 할 값 | Unity에서 남는 작업 |
|---|---|---|
| SVG | 원본 viewBox/획선/실루엣/의미 ID/license | 프로젝트에 vector 렌더 지원이 있다고 가정 금지; 현재처럼 raster PNG 또는 별도 승인 dependency |
| PNG | scale·transparent padding·logical bounds·pivot | Sprite import/PPU/filter/압축/atlas·reference rebinding |
| Panels | 9slice border와 늘어나도 되는 중앙/보존 corner | Image.Type.Sliced·border metadata·text inset |
| Icons | role/name/selected/disabled/pressed tint | 상태 binding·hit rect·raycastTarget |
| Type | role/weight/lineheight/한영 길이 budget | live LocalizedLabel/L.Format; Glyph·TextGenerator·pixel snapping 검수 |
| Layout | anchor/constraints/min-size/insets/scroll content | RectTransform/Layouts/Mask/SafeArea 적용, 모든 content index 보존 |
| Motion | duration/easing/reduced-motion 대안/취소시 종료 | 기존 state lifetime과 input lock에 맞는 tween/animation; no fake loading |
| State | normal/pressed/disabled/busy/success/error | 실제 service/Host result가 상태를 바꿈; client optimistic grant 금지 |
| Performance | atlas 분할/texture budget/scrim alpha | overdraw·Canvas rebuild·GC·device GPU/font memory 측정 |

## 기존 구조를 유지하며 대체하기

PROPOSAL: 우선 presentation hierarchy, tokens, image/type/spacing을 교체하되 controller/service·SerializeField 참조·UnityEvent·pointer adapter를 재연결한다. generated scene만 손수 고치고 canonical builder를 그대로 두면 다음 authoring에서 덮어써진다. 후속 통합은 각 canonical builder/Arrange와 runtime-only helper를 함께 정리하는 명시 범위가 필요하다. 이번 audit에는 그 변경을 하지 않았다.

Item slot/보상 tile/button/tree node는 재사용 component 후보다. 다만 현재 의미가 다르다: 보상 tile은 비조작, slot은 pointer/drag/drop, primary는 press hold, confirmation은 transaction 시작 전 확인이다. 동일 사각형 prefab 하나로 강제로 통합하면 기능을 잃는다. 새 framework나 theme override layer를 누적하지 않는다.

월드 art replacement는 별도 승인·source ownership 패스다. 캐릭터/보스는 여러 SpriteRenderer와 pose/anchor/facing/hit/death/pooling을 공유하므로 새 art가 곧 animation 완성이라는 뜻이 아니다. 이전 facing correction은 유지하고 임의 REBUILD/rollback하지 않았다.
'''
 write('UI_ARCHITECTURE_AND_ASSETS',text)

if __name__=='__main__':issues();world();architecture();print('Wrote issues, world/save boundaries, architecture/assets reports.')
