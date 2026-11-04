import sys,json,re,os,collections,hashlib,subprocess
from pathlib import Path
sys.dont_write_bytecode=True
import audit_session as a
ROOT=a.ROOT; S=json.loads((ROOT/'screens.json').read_text(encoding='utf8')); A=json.loads((ROOT/'actions-raw.json').read_text(encoding='utf8'))
BASE='48d6528758d18bf3df41ed02ad9aa0f268654a51'
files=list(a.ASSETS.rglob('*.cs')); byname={p.stem:p for p in files}; sourceCache={}
def source(cls,method=None):
 p=byname.get(cls)
 if not p:return '미구현 / 별도 source 없음'
 t=sourceCache.setdefault(str(p),p.read_text(encoding='utf8'));lines=t.splitlines();pattern=r'\b'+re.escape((method or cls).split('.')[0])+r'\s*\('
 found=next((i+1 for i,l in enumerate(lines) if re.search(pattern,l) and any(x in l for x in ['public ','private ','internal ','protected '])),None)
 if not found:found=next((i+1 for i,l in enumerate(lines) if 'class '+cls in l),1)
 path=os.path.relpath(p,ROOT).replace('\\','/')
 return f'[{cls}{"."+method if method else ""}]({path}#L{found})'
def esc(v):return str(v).replace('|','∣').replace('\n','<br>')
def table(headers,rows):return '\n'.join(['| '+' | '.join(headers)+' |','|'+'|'.join('---' for _ in headers)+'|']+['| '+' | '.join(esc(x) for x in row)+' |' for row in rows])+'\n'
def write(name,text): (ROOT/(name+'.md')).write_text(text.strip()+'\n',encoding='utf8')
def j(name,v):a.save(name,v)
COMMON='''기준은 fetched main `48d6528758d18bf3df41ed02ad9aa0f268654a51`이다. 이 보고서는 게임 수정 승인이 아니다.

**CHECKPOINT / DRAFT — 사용자 요청으로 중단. 전수 감사 완료본이 아니다.** 자동 추출 목록과 수동 대조 초안이며, 잔여 surface/action 검수와 6개 보고서는 RESUME.md에 남겼다.

FACT는 현재 source/data 사실, CONFIRMED는 이번 Unity에서 관찰한 해당 화면·행동, SOURCE-INFERRED는 source로 확인했지만 그 상태를 실제 재현하지 않은 항목, HISTORICAL은 과거 캡처/검사, PROPOSAL은 제안, FUTURE GOAL은 사용자의 차기 목표, UNVERIFIED는 추가 증거가 필요한 항목이다. 시각적 평가 자체는 감사자의 판단이며 기능 검증과 구분한다.
'''

def screen_sources(s):return source(s['controller'])+' · '+source(s['builder']) if s['controller']!='—' else '요청/현재 source의 부재 대조'
def evidence_for(s):
 patterns={'H01':'home-v2','H03':'settings-v2','E01':'achievements-v2','E02':'attendance-v2','E03':'attendance-monthly-v2','E05':'store-ko','A01':'profile-ko','A02':'account-ko','A03':'store-help-ko','A05':'delete-unconfirmed','P01':'class-overview','P02':'class-tree','M01':'multiplayer-','I01':'inventory-','P04':'character-','P05':'shared-tree-','C01':'hand-crafting-','S01':'pause-','S02':'pause-settings-','I02':'inventory-'}
 live=sorted((ROOT/'evidence/live').glob('*.png'));p=patterns.get(s['id']);found=[x for x in live if p and x.name.startswith(p)]
 if found:return ', '.join(f'[{x.name}](evidence/live/{x.name})' for x in found[:3])+' · metadata 동명 JSON'
 historical={'I06':'gameplay--chest-en-844.png','G08':'gameplay--hud-repair-ko-844.png','D01':'gameplay--death-ko-844.png','G01':'gameplay--hud-repair-ko-844.png'}
 if s['id'] in historical:
  f=historical[s['id']];path=a.PROJECT/'validation/post-commercial-ui-qa-tools/evidence'/f
  if path.exists():return f'HISTORICAL [{f}](../post-commercial-ui-qa-tools/evidence/{f}) · 0ae9feae'
 return '현재 rendered 미확인; source/scene evidence. EVIDENCE_INDEX의 미검증 목록 참조'

def inventory():
 text='# 화면·상호작용 표면 전수 목록\n\n'+COMMON+'\n'
 text+=f'발견 등록 {len(S)}개: 현재 코드에 존재하는 표면 {sum(s["controller"]!="—" for s in S)}개, 미래·미구현·역사 전용 {sum(s["controller"]=="—" for s in S)}개. 한 메뉴 안의 독립 과제(예: Split, held drag, queue, downed banner)도 표면으로 센다. EN/KO·해상도·recipe111·class6·slot 반복을 별도 화면 수로 부풀리지 않았다. 화면 수는 전체 페이지 수나 실플레이 완료 수가 아니다.\n\n'
 text+=table(['ID','실제 표시/역할','상태','진입 → 이탈','근거'],[(s['id'],s['name'],s['status'],s['entry']+' → '+s['exit'],source(s['controller'])) for s in S])
 text+='\n## 모든 표면에 적용하는 판정 규칙\n\n'
 text+='Sheet/Book/Field는 화면 중앙의 blocking overlay이며 popup 바깥 tap-to-dismiss를 자동으로 가정하지 않는다. HUD/world는 non-modal이다. Item Actions/Split은 부모 modal 위의 blocking child, held ghost는 raycast하지 않는 별도 표시다. 오프라인의 업적·출석·Store preview는 존재하지만 수령·구매 권한은 없다. Alpha/Google/광고/IAP OFF를 미구현 UI와 혼동하지 않는다. modal이 세계 시간을 정지시키는지도 별도다: Inventory/Character/Tree/Station은 세계가 진행하고 SP Pause/Dead만 정지한다.\n\n'
 for s in S:
  text+=f'## {s["id"]} · {s["name"]}\n\n'
  text+=table(['항목','확인 내용'],[
   ('구현/신뢰',s['status']+' / '+s['confidence']+'; runtime 증거는 아래 링크의 실제 상태만'),('씬·작성 원본',s['scene']+' / '+s['builder']),('구성·modal',s['composition']+'; parent/caller는 진입·이탈 열과 NAVIGATION 표를 따른다'),('진입',s['entry']),('Back/종료',s['exit']),('주·보조·전체 control',s['controls']),('scroll',s['scroll']),('동적 데이터',s['data']),('empty/loading/error/disabled',s['states']),('온라인·MP',s['network']),('현 visual',{'home':'기존 숲·Krita wordmark + 오른쪽 제본 종이 + 좌하 records','sheet':'1720×1000 종이 한 장, 대부분 동일 사각 action','book':'양면 journal, 도표/초상/상세','field':'1900×980 어두운 녹색 matte utility panel','hud':'투명 world overlay, 수치/작은 icon/원형 touch controls','world':'SpriteRenderer/월드 ghost 및 context feedback','overlay':'별도 차단층/ghost/transition','legacy-runtime':'런타임 고정 좌표 어두운 사각 panel, 별도 label/button helper'}.get(s['style'],s['style'])),('Localization',s['localization']),('문제/보존 경계','해당 ID별 VISUAL_UX_ISSUES 및 CONTROL_AND_ACTION_MAP 참조. rendered 없는 경우 외관 합격을 선언하지 않음'),('source',screen_sources(s)),('화면 증거',evidence_for(s))])+'\n'
 text+='''## Dead / 역사·도구 분리

- `SkillTreeScreen.Close()`는 API로 존재하지만 현재 Common Tree authoring에는 Back만 연결된다. Close 버튼이 있는 화면으로 인계하지 않는다. P05 → P04 → Close가 실제 UI 경로다.
- `AccountProfileScreen.Character`는 객체가 남지만 authoring에서 비활성화된다. Profile identity는 EconomyScreen 쪽이 담당한다. 사라진 중복 데이터 표시를 신기능으로 다시 만들지 않는다.
- 숨긴 Dropdown Template, 비활성 Attendance tile, 미선택 node, 비가동 death view는 dead UI가 아니다. 실제 조건에서 활성화하거나 표시 전용으로 사용하는 구조다.
- WorldOverview/Showcase/DeveloperHarness/Combat Sandbox/QA fixture/EditorWindow는 플레이어 화면 수에 포함하지 않는다. 제품 지도·도감·치트 메뉴가 구현됐다는 뜻이 아니다.
- 각 문서의 옛 Item48/132, Recipe99/109, Save11/13, Protocol21, 실제2인 미수행은 그 절의 당시 기록이다. 현재 Item136/Recipe111/Save14/Profile2/Protocol23과 구분한다.
'''
 write('SCREEN_INVENTORY',text)

# Service and side-effect contracts keyed by controller, specialized per operation below.
contracts={
 'HomeScreen':('SceneFlow → SaveRepository / SaveGameRules','navigation; Continue는 검증된 snapshot 적용, BeginNew는 새 seed 전환·다음 저장에서 기존 단일 slot 교체','유효/손상·없음/transition guard'),
 'SettingsPanel':('UserPreferences → LocalSettings → L/GameAudio','로컬 설정 즉시; 일반 실행 PlayerPrefs 지연0.5s+Flush; 이번 QA RAM','en/ko cycle; world/profile와 분리'),
 'UserPreferences':('LocalSettings / L / AudioRuntime','로컬 설정만; Reset도 즉시, 별도 확인 없음','control .8–1.4; audio0–1'),
 'StoreScreen':('StoreService → IAP provider → 서버 Purchase verification','Buy만 실제 결제; client Gem 지급 금지; Recover 동일 미확정 거래 복구','PurchaseEnabled=false; account/busy/phase gate'),
 'AccountProfileScreen':('AccountService → CloudProfile / EconomyService','connection/cloud/지원·삭제 경계; 실제 세계 파일을 Cloud로 올리는 기능 아님','Enabled=false; linked/idle/conflict gate'),
 'MultiplayerMenu':('NetworkSession → RelayRoomConnection / LocalRoomConnection','연결/session lifetime; Host만 파일 저장, Join은 타인 월드 owner 요청','Idle/Connecting/Waiting/Closing; 형식 검사; 최대4'),
 'EconomyScreen':('EconomyService / RewardedAds → 서버 receipt','claim/spend는 인증된 서버 CAS, offline cache를 지급 근거로 쓰지 않음','offline browse만; server eligibility; 오류-only retry'),
 'ClassLobbyScreen':('ClassProfiles / EconomyService.UnlockEngineer','profile 선택/point/소유; Gold world debit+intent, Engineer server receipt','Lobby/owned/point/prerequisite/wallet gate'),
 'InventoryPanel':('PlayerInventory → InventoryModel / NetworkInventory','선택 slot은 player snapshot, 단순 UI 강조와 데이터 선택 구분','Session.Inventory;36칸'),
 'ChestPanel':('PlayerStorage → InventoryDragController → NetworkFacilityInteractor','Chest↔bag transfer는 bag+facility revision Host 검증; scroll은 UI만','거리/생존/slot/full/stale'),
 'InventoryEquipment':('PlayerEquipment → NetworkInventory','장비와 bag 원자 교환; 교체/해제 공간 부족은 보존','Head/Body/Utility/Accessory4; 비교는 display'),
 'InventoryDragController':('InventoryModel / PlayerInventory / PlayerItemDrop / NetworkFacilityInteractor','실제 목적지 성공 전 원본 유지; source revision 보존; Drop만 ground 생성+차감','pointer owner; inventory state; compatible/full/stale'),
 'InventorySlotDrag':('InventoryDragController / ScrollRect','selection/drag gesture 분기; source는 놓기 전 유지','long press .45s; 빠른 touch vertical <.18s scroll;16px 이동'),
 'EquipmentSlotDrag':('InventoryDragController → PlayerEquipment','올바른 slot 장착·빈 bag 해제; 양쪽 원자 교환','compatible, pointer, inventory, bag space'),
 'InventoryDropZone':('InventoryDragController → PlayerItemDrop','명시 ground drop만 소비; 빈 화면은 cancel','안전한 바닥·공간 확인 뒤 pickup 생성'),
 'StackSplitPanel':('InventoryDragController.HoldPartial','Confirm은 ghost를 드는 UI 상태; 아직 모델 소비 없음','count≥2; amount1..total−1'),
 'RecipeCardView':('PlayerCrafting → StationProcessing / InventoryModel / NetworkFacilityInteractor','즉시 exchange 또는 batch/queue 예약; 결과는 명시 수령','Station/tier/cost/output space/Session 검증'),
 'CraftingPanel':('PlayerCrafting / StationProcessing / PlayerNavigation','filter/select는 UI; enqueue·withdraw·collect·Home은 실제 상태','각 Station processing mode와 cost·cargo revision'),
 'PlayerCrafting':('InventoryModel.TryConsumeCosts / NetworkFacilityInteractor','Upgrade 재료 소비·tier 교체; hand/bag open은 UI','범위/WorkbenchProgression/Session; SaveId 유지'),
 'SkillTreeScreen':('PlayerProgression → NetworkCommonGrowth','Invest world point1 소비/rank 증가; UI preview와 분리','CanInvest prerequisite/rank/point; Respec없음'),
 'CharacterScreen':('GameplaySession','정보·navigation만; 장비 조작 없음','Character/SkillTree 상태'),
 'ClassHud':('PlayerClass / PlayerBuildingService / PlayerMedicine / PlayerTeamMarker / NetworkClassActions','role별 Guard/overclock/mark/medicine/시설repair·dismantle; MP Host 권위','range/LoS/alive/role/cooldown; hold release 중단'),
 'DeathHud':('PlayerDeath / CloudGemReviveWallet / RewardedAds / NetworkPlayer','SP 광고 성공1/run·Gem8 예약/commit/환급; Home 무료; MP Home 요청','Durable/death revision/pending; MP 유료 선택 숨김'),
 'HotbarView':('PlayerInventory.SelectSlot / NetworkInventory','선택 index 영속; 소비는 Primary에서','Running·index0..8'),
 'GameplaySession':('WorldPauseController / GameplayUiLayers / modal listeners','presentation state; SP Pause/Defeated timeScale0; 가방/성장 world continues','상태 gate; MP global freeze 없음'),
 'PauseMenu':('UserPreferences.Flush / GameplaySession','menu/settings visible 및 설정 flush; Save는 WorldSaveService','caller-sensitive Back'),
 'WorldSaveService':('SaveRepository.Write → SaveGameRules; SceneFlow.Home','checksum·atomic replace+backup; 저장 실패면 현 화면 유지','SP repository; 이번 audit에서는 save action 실행 안 함'),
 'MultiplayerArena':('LocalPlayer.Commands / NetworkSession.Leave','Dash server request; Leave Host 저장 후 shutdown; client는 Host world를 저장하지 않음','로컬 actor 존재; save failure retains host'),
 'PlayerLocomotion':('StaminaModel / PlayerMotor / movement authority','Dash stamina24/거리2.6/0.18s/cooldown; 해당 세계 상태','alive/running/stamina/obstacle/water; 무적 아님'),
 'VirtualJoystick':('PlayerInputAdapter → PlayerInputCommands.SetMove','포인터 입력→movement, 범위·state 검증; 별도 Sprint 버튼 없음','첫 pointer 소유; modal/focus/pause release'),
 'HoldAttackInput':('PlayerPrimaryAction.TryUse','탭 실제 Primary; hold는 Attack일 때만 repeat','Running; Action.Kind 변경시 반복 중단'),
 'NetworkAttackInput':('MultiplayerArena.LocalPlayer.Commands.Attack → owner RPC','client 직접 damage/item 소비 금지; 기존 MP repeat cadence','pointer owner; Telescope/Exploration 반복 제외'),
 'InteractButtonView':('PlayerInteraction.Use / PlayerInputCommands.HoldInteract','context click; downed면 hold rescue; callback을 일반 Button.Click으로 평탄화 금지','state/range/selected water/equipment; up/exit cancels'),
 'CampLootPanel':('IWorldSupplies.RequestTake / NetworkFacilityInteractor','공유6칸 전체 stack claim; 서버 bag+site revision; 재충전 없음','secured; available; in range; bag space'),
 'TierThreeHud':('WorldTurret.Execute / NetworkTierThreePlayer / PlayerNavigation / PlayerTelescope','ammo/module/Beacon recovery는 실제 world/bag; nav설정 snapshot; view/aim은 transient','range/session/ammo/module/line-of-sight authoritative gates'),
 'TransitionOverlay':('NetworkSession.Leave / failed callback','network 취소는 operation ticket 무효화; SP 비동기 load 중 Cancel 무효','load/connecting/failed 각각 다른 취소 정책'),
 'PlayerPlacement':('InventoryModel.TryDropSlot / NetworkConstruction.Place','검증된 위치에서 시설 생성 성공 때1개소비; Host/rollback','land/region/overlap/range/Running'),
 'RoyalTomb':('BossWorld.Summon / NetworkFacilityInteractor.SummonBoss','밤 Bone10 소비와 보스/LastNight를 durable world transaction','guardian clear/밤/범위/가방/revision/이미소환 gate')}
special={
 'StoreScreen.Contact':('Meta.Profile.Open → Help','Store 숨김, Account 경유 후 Feedback; CloseHelp는 Account로 복귀하여 원점 소실','현재 runtime CONFIRMED'),
 'StoreScreen.Close':('Home.ShowHome 또는 Meta.OpenHub','fromHome을 유지하는 정상 Store Back','Contact의 별도 경로와 혼동 금지'),
 'StoreScreen.Open':('StoreService.Open','제품 OFF catalogue 읽기·view 상태; 구매는 아님','origin cached'),
 'StoreScreen.Select':('StoreService.Select','선택 상품 preview만; Gem 결제/차감 없음','실제3상품 범위'),
 'StoreScreen.ToggleHistory':('StoreScreen history flag','Selection/Cards ↔ history scroll; 구매 없음','동일 parent Store'),
 'AccountProfileScreen.CloseHelp':('FeedbackView false; View true','항상 Account로 복귀; caller stack 없음','Store에서 연 경우도 동일'),
 'AccountProfileScreen.Delete':('DeleteView true; counter0','경고 presentation만; 삭제 요청은 아직 없음','제품 정상 진입은 Linked gate'),
 'AccountProfileScreen.ConfirmDelete':('AccountService.PrepareDeletion(true)','첫 click 경고 변경; 두 번째 click 실제 삭제 요청','보호된 동작; 이 audit에서 실행 안 함'),
 'AccountProfileScreen.CopySupport':('GUIUtility.systemCopyBuffer','지원 ID를 OS clipboard에 복사','실행 안 함; secret 복사 아님'),
 'AccountProfileScreen.Privacy':('BackendPolicy.ValidWeb → Application.OpenURL','실 URL만 외부 browser; 빈 URL이면 unavailable 문구','현재 URL없음'),
 'AccountProfileScreen.CategorySelection':('UGUI Dropdown.value','6분류 선택; 제출 전 외부 전송 없음','KO/EN option text는 Refresh에서 변경'),
 'AccountProfileScreen.DiagnosticConsent':('Toggle.isOn','기본false; Submit 시에만 허용된 진단 첨부','동의와 message 입력은 별도'),
 'AccountProfileScreen.MessageInput':('InputField.text','multiline 최대1500; Submit 전 server 전송 없음','키보드/IME가림 실기 미검증'),
 'EconomyScreen.Category':('category=index; selection/reset scroll','7category 전환, family·detail top으로; claim 없음','Rows6은 재사용되는 view, family22와 구분'),
 'EconomyScreen.Board':('tab/index; event cycle','신규/월간/Events 전환; 이미 Events이면 eventIndex++','타일28은 Refresh에서 interactable=false'),
 'EconomyScreen.Select':('selected family; detail scroll reset','선택만; 진행/수령 receipt 생성 없음','hidden rows 제외'),
 'EconomyScreen.ClaimSelected':('EconomyService.Claim','선택 family의 서버 claimable 보상만','클릭/애니메이션으로 보상 grant 금지'),
 'EconomyScreen.ClaimEverything':('EconomyService.ClaimAll','전체 eligible 보상을 같은 CAS에서 확정','일부 optimistic 지급 금지'),
 'EconomyScreen.ClaimAttendance':('EconomyService.Attend','server UTC/period/slot eligibility 판단 후 receipt','stamp는 새 currency/item 아님'),
 'ClassLobbyScreen.SelectCard':('selected preview index; node0','직업 preview만; 실제 선택 Class 변경 없음','selected seal과 preview highlight 구분'),
 'ClassLobbyScreen.SelectNode':('node preview index','9개 해당 직업 node 상세만; point 소비 없음','54개 전체를 동시 스크린으로 만들지 않음'),
 'ClassLobbyScreen.AskPurchase':('Confirmation.SetActive','소유/비용을 읽은 확인 dialog; 아직 차감 없음','Lobby && PurchaseButton.interactable'),
 'ClassLobbyScreen.ConfirmPurchase':('Gold ClassProfiles.Purchase / Gem UnlockEngineer','Gold source world debit+receipt 또는 서버120Gem 해금','두 통화/두 저장소 거래를 UI가 합치지 않음'),
 'CraftingPanel.SetHome':('PlayerNavigation.SelectHome','해당 Workbench를 Compass Home 지정','PlayerDeath.HomeAnchor와 별개'),
 'CraftingPanel.EnqueueOne':('StationProcessing.Add(...,1)','선택 recipe 재료 예약; 즉시 보상 아님','Sequential capacity/재료'),
 'CraftingPanel.EnqueueAll':('StationProcessing.AddAll','가능한 batch만큼 예약 소비','작업중/world continues'),
 'CraftingPanel.WithdrawOne':('StationProcessing.Withdraw','대기1개 반환; active job 무료 취소로 바꾸지 않음','Queue.CountWaiting>0; bag capacity'),
 'CraftingPanel.CollectResults':('StationProcessing.Collect','완료 결과 명시 bag 수령; 가득차면 유지','output 존재/원자 add'),
 'StackSplitPanel.Confirm':('InventoryDragController.HoldPartial','패널닫고 선택 수량 ghost 들기; 모델은 아직 원본','이후 명시 destination 성공 때 transfer'),
 'InventoryEquipment.SelectSlot':('SlotFilter; equippedSelected; Panel.Select','장비 필터/선택; 빈칸이면 호환 bag item preview','동일slot 재클릭 filter 해제'),
 'InventoryEquipment.Equip':('PlayerEquipment.TryEquip 또는 PlayerTelescope.Use','장비 원자 교환; Telescope는 가방닫고 망원경','display label Equip/Use가 item에 따라 변함'),
 'HomeScreen.SinglePlayer':('Repository.Exists → ConfirmNew else BeginNew','현재는 world selector가 아니라 새 세계 흐름','Future multi-world와 분리'),
 'HomeScreen.TryContinue':('RefreshSave → SceneFlow.Continue','Rules.Validate한 primary/backup snapshot만 전환','invalid면 생성으로 대체하지 않음'),
 'HomeScreen.BeginNew':('SceneFlow.NewGame','random seed/generation3 → Gameplay; 기존 slot 보존을 계속 보장하지 않음','새 세계 첫 save가 단일 slot 대체'),
 'InventorySlotDrag.LongPress':('InventorySlotDrag.Update → OpenItemActions','0.45s stationary stack selection/actions; UI state만','pointer 이동16px 초과/drag 중 제외'),
 'RecipeCardView.OnPointerClick':('PlayerCrafting.SetSelected → MarkSeen','선택/SeenRecipe가 snapshot에 남을 수 있음','상위tier NEW gate; 생산 없음'),
 'MultiplayerMenu.CopyCode':('GUIUtility.systemCopyBuffer','Internet room code만 복사','LAN에서는 숨김'),
 'DeathHud.Respawn':('PlayerDeath.HomeRespawn / NetworkPlayer.RequestHome','같은 월드 초기 spawn HomeAnchor로 무료 복귀; 앱 메인메뉴 아님','새 run/ad기회; 아이템/장비/성장 유지')}

def actions():
 rows=[]
 for idx,x in enumerate(sorted(A,key=lambda x:x['callback']),1):
  cls=x['controller'];method=x['method'];base=contracts[cls];service,effect,gate=special.get(x['callback'],base)
  if x['callback'] not in special and method in ['Back','Close','ShowHome','CloseHub','OpenSettings','OpenMultiplayer','OpenTree','OpenBag','OpenHandCrafting','OpenAccount','Help','CancelDelete','CancelPurchase','SelectNode','SelectCategory','ToggleFilter','Increase','Decrease','Half','ChooseSplit','CycleMeaning','Cancel']:
   service=cls+' presentation / session callback';effect='화면·선택·수량·hold 표시 상태 변경; 이 조작 자체는 비용·보상·소유권을 확정하지 않음';gate=base[2]+'; 일반 UserPreferences.Flush가 있는 Back은 기존 로컬 설정 flush 유지'
  if x['callback'] in ['EconomyScreen.OpenHub','EconomyScreen.OpenAchievements','EconomyScreen.OpenAttendance','EconomyScreen.Sync']:
   effect='화면·origin 상태 및 EconomyService.Sync/RewardedAds.Refresh 읽기·캐시 동기화; claim은 별도 명시 action';gate='offline catalogue; 연결 안 되면 claim 불가'
  if x['callback']=='ClassLobbyScreen.Open':
   effect='직업 개요 열기·preview 초기화·ClassProfiles.Reconcile로 기존 pending 거래 복구 가능; 새 구매를 시작하지 않음'
  if any(k in method for k in ['Scroll','OnDrag','OnPointerEnter','OnPointerExit']) and x['callback'] not in special and cls not in ['VirtualJoystick','HoldAttackInput','NetworkAttackInput','InteractButtonView']:
   if 'Scroll' in method:service='UGUI ScrollRect normalizedPosition';effect='表示位置のみ。データ・報酬変更なし。'.replace('表示位置のみ。データ・報酬変更なし。','표시 위치만 변경; 데이터·보상·소비 없음');gate='마스크 안 scroll; drag와 gesture 경합은 각각 명시'
  x.update(id=f'ACT-{idx:03}',service=service,effect=effect,gate=gate,source=source(cls,'Operate' if method.startswith('Operate.') else 'Build' if method in ['TrackBeacon','RecoverBeacon','TelescopeAim','TelescopeExit'] else 'Update' if method=='LongPress' else method))
  inst=x['instances'];control='; '.join(sorted(set(y['path'].split('/')[-1] for y in inst))[:4]) if inst else method+' (gesture/runtime)'
  x['control']=control;x['instanceCount']=len(inst);rows.append((x['id'],control,x['callback'],service,effect,gate,x['source']))
 j('actions',A)
 text='# Control → callback → state/service 계약\n\n'+COMMON+f'\n**{len(A)}개 의미별 action**. Scene 직렬 control671개와 런타임 listener/pointer 경로를 대조했다. 파라미터 반복(Hotbar9, inventory36, recipes111, 같은 카테고리·class별 node)은 같은 action으로 묶고 모든 instance path는 [actions.json](actions.json)과 [serialized-controls.json](serialized-controls.json)에 남긴다. Scrollbar는 대응 ScrollRect 조작으로 묶는다. `Operate.Fire` 같은 표기는 의미별 alias이며 실제 callback은 `Operate(TurretOperation.Fire)`다. pointer move/up/exit와 click을 분리해 hold/cancel 계약을 보존했다.\n\n'
 text+=table(['ID','control/gesture','실제 callback 또는 alias','controller/service','state·side effect·persistence/authority','gate/disabled','source'],rows)
 text+='''
## 동작 종류별 반드시 지킬 원칙

| 종류 | presentation-only | 실제 mutation 경계 | 실패·취소 |
|---|---|---|---|
| 가방/장비 | focus/filter/ghost/quantity | InventoryModel/PlayerEquipment, MP NetworkInventory·Facility revision | full/stale/invalid이면 원본 보존; 빈 화면 drop은 취소 |
| Craft | tab/filter/preview | 즉시 TryExchange 또는 processing 예약; 완료 Collect | output full 보존; queue 재료/결과는 시설에 남음 |
| Save | summary/확인 dialog | Rules.Validate → checksum → temporary flush → atomic replace/backup | 실패 안내 후 머무름; corrupted save를 new game로 몰래 교체 금지 |
| Gold Class | preview/confirm | Profile intent → world Gold debit+receipt → Profile ownership | 중단 recovery WorldPath 참조; 단순 wallet 숫자 animation으로 해금 금지 |
| Gem/IAP/Ads | preview/loading | 계정·환경 scoped server receipt, 검증·중복방지·pending recovery | cache 금액으로 소비 금지; provider unavailable은 unavailable |
| Death | 정보/선택 | 동일 death/run revision; Gem reserve→world save→Finish; 실패 보상 | late callback discard; 무료 Home 지속 제공 |
| Host session | route/input | Host load·participant restore·save; Client owner RPC | Host save failure는 session 유지; 강제퇴장 UX 별도 결정 |
| 설정 | label/slider/selection | LocalSettings user preference, world 저장과 무관 | QA는 RAM store, 제품 PlayerPrefs; 취소해도 자동 rollback되는 설정창이 아님 |

Attendance의28개 Button 컴포넌트는 runtime `interactable=false`, transition None이고 callback도 없다. per-cell claim으로 재설계하면 기능 변화다. InputField/Dropdown/Toggle은 framework state 변경 후 Submit에서 읽는다. 무바인딩 control과 display-only를 모든 경우 dead button으로 보고하지 않는다.

Runtime MP Bind는 일부 serialized event를 새 `ButtonClickedEvent`로 교체해 로컬 actor를 연결한다. Scene event 리스트만 보고 Close/Craft/Upgrade가 inert라고 판단하면 틀린다. 해당 경계는 NetworkInventoryUi.Bind다. 모든 클래스의 family-wide 제약은 위 표에 있으며 parameter별 item/recipe 비용은 기존 canonical data를 계속 읽는다.

## 이번 실행의 한계

실제 비용 지불·소분 commit·claim·구매·광고·계정 연결/삭제·Host 세션은 실행하지 않았다. UI 콜백 탐색과 데이터 조회만 관찰했다. 이 지도는 재설계 보존 계약이며 거래 회귀 테스트가 통과했다는 뜻이 아니다. 후속 Unity integration에서 실제 owner/Host·stale·cancel·late-callback 검사가 필요하다.
'''
 write('CONTROL_AND_ACTION_MAP',text)

ROUTES=[]
def route(src,event,dst,condition,cls,method):ROUTES.append(dict(id=f'NAV-{len(ROUTES)+1:03}',src=src,event=event,dst=dst,condition=condition,source=source(cls,method)))
def navigation():
 for src,event,dst,cond,cls,method in [
 ('App','launch','H01','MainMenu build index0','HomeScreen','OnEnable'),('H01','Continue','T01','유효 검증 save','HomeScreen','TryContinue'),('H01','SinglePlayer','H02','Repository.Exists','HomeScreen','SinglePlayer'),('H01','SinglePlayer','T01','save없음','HomeScreen','SinglePlayer'),('H02','Cancel','H01','새 세계 시작 안 함','HomeScreen','ShowHome'),('H02','BeginNew','T01','새 seed; 다음 save가 slot교체','HomeScreen','BeginNew'),('H01','Settings','H03','local','HomeScreen','OpenSettings'),('H03','Back','H01','설정은 즉시 반영','HomeScreen','ShowHome'),('H01','Multiplayer','M01','session create/resume','HomeScreen','OpenMultiplayer'),('M01','Back','H01','Leave(false)','MultiplayerMenu','Back'),('M01','Single','H02/T01','SP saveexists에 따름','MultiplayerMenu','Single'),('M01','Host/valid Join','T01','Idle; valid destination','MultiplayerMenu','Host'),('M01','invalid Join','M01','invalid code/address status','MultiplayerMenu','Join'),('T01','connection ready','M02','Waiting','TransitionOverlay','Watch'),('M02','Back','H01','Leave(false) shutdown','MultiplayerMenu','Back'),('M02','Enter','T01','Host only','MultiplayerMenu','Enter'),('M02','Host enters','T01','Client NGO Load event','NetworkSession','PresentSceneTransition'),('T01','world ready','G01','actor+terrain; safe session resume','TransitionOverlay','Begin'),('T01','Cancel','M01','network connecting; operation++','TransitionOverlay','Cancel'),('T01','failure/recovery','H01/M01','scene/network path별','TransitionOverlay','Fail'),
 ('H01','Class','P01','Lobby','ClassLobbyScreen','Open'),('P01','Back','H01','overview','ClassLobbyScreen','Back'),('P01','OpenTree','P02','선택 class preview','ClassLobbyScreen','OpenTree'),('P02','Back','P01','node 선택 유지; Open resets later','ClassLobbyScreen','Back'),('P01','AskPurchase','P03','구매 gate','ClassLobbyScreen','AskPurchase'),('P03','Cancel','P01','소비없음','ClassLobbyScreen','CancelPurchase'),('P03','Confirm','P01','success/failure status','ClassLobbyScreen','ConfirmPurchase'),
 ('H01','Profile','A01','OpenHub refresh','EconomyScreen','OpenHub'),('A01','Back','H01','CloseHub','EconomyScreen','CloseHub'),('A01','Account','A02','hub hidden','EconomyScreen','OpenAccount'),('A02','Back','A01','always hub fallback Home','AccountProfileScreen','Back'),('A02','Help','A03','no external submit','AccountProfileScreen','Help'),('A03','Back','A02','always Account; caller lost','AccountProfileScreen','CloseHelp'),('A03','Category','A04','UGUI template','AccountProfileScreen','Refresh'),('A04','choose/dismiss','A03','framework dropdown','AccountUiBuilder','Help'),('A02','Delete','A05','Linked normal entry','AccountProfileScreen','Delete'),('A05','Cancel','A02','counter reset next Open','AccountProfileScreen','CancelDelete'),('A05','Confirm×1','A05','first extra warning only','AccountProfileScreen','ConfirmDelete'),('A05','Confirm×2','A05','pending/failed text','AccountProfileScreen','ConfirmDelete'),('A02','conflict','A06','account/cloud conflict','AccountProfileScreen','Refresh'),('A06','UseExisting/UseCloud/KeepLocal','A02/A06','actual service response','AccountProfileScreen','UseCloud'),
 ('H01','Achievements','E01','fromProfile=false','EconomyScreen','OpenAchievements'),('A01','Achievements','E01','fromProfile=true','EconomyScreen','OpenAchievements'),('E01','Back','H01/A01','cached origin','EconomyScreen','Back'),('H01','Attendance','E02/E03/E04','tab retains previous; no forced newcomer','EconomyScreen','OpenAttendance'),('A01','Attendance','E02/E03/E04','fromProfile=true','EconomyScreen','OpenAttendance'),('E02/E03/E04','Board','E02/E03/E04','event reselect cycles index','EconomyScreen','Board'),('E02/E03/E04','Back','H01/A01','cached origin','EconomyScreen','Back'),('H01','Gem Store','E05','fromHome=true','StoreScreen','Open'),('A01','Gem Store','E05','fromHome=false','StoreScreen','Open'),('E05/E06','Back','H01/A01','cached origin','StoreScreen','Close'),('E05','History','E06','same Store view','StoreScreen','ToggleHistory'),('E06','History toggle','E05','same origin','StoreScreen','ToggleHistory'),('E05/E06','Purchase help','A03','Meta.Profile.Open→Help','StoreScreen','Contact'),('A01','Watch/Retry','E07','inline status; native ad external','EconomyScreen','WatchReward'),('E07','result','A01','confirmed/pending/error inline','EconomyScreen','Refresh'),
 ('G01','Bag','I01','Running; no context modal','GameplaySession','ToggleInventory'),('I01','Close','G01','Session.CloseInventory','GameplaySession','CloseInventory'),('G01','Profile','P04','Running','GameplaySession','OpenCharacter'),('P04','Close','G01','local session','CharacterScreen','Close'),('P04','Tree','P05','Character state','CharacterScreen','OpenTree'),('P05','Back','P04','no authored direct Close','SkillTreeScreen','Back'),('G01','Pause','S01','SP; MP Leave is separate','GameplaySession','OpenPause'),('S01','Resume','G01','SP unfreeze','GameplaySession','Resume'),('S01','Settings','S02','Menu hidden','PauseMenu','OpenSettings'),('S02','Back','S01','prefs Flush','PauseMenu','Back'),('S01','Save & Home','T01','save success only','WorldSaveService','SaveAndHome'),('S01','save failure','S01','toast; no transition','WorldSaveService','SaveAndHome'),
 ('I01','Crafting','C01','hand station','PlayerCrafting','OpenHandCrafting'),('C01','Bag','I01','clears station context','PlayerCrafting','OpenBag'),('C01','Close','G01','does not reopen bag','PlayerCrafting','CloseCrafting'),('G01','Station Interact','C01','in range/LoS','PlayerCrafting','OpenStation'),('C01','sequential station','C02','queue display variant','CraftingPanel','Refresh'),('C01','Workbench I/II','C03','upgrade/Home area visible','CraftingPanel','Refresh'),('G01','Chest Interact','I06','PlayerStorage context','PlayerStorage','Open'),('I06','Close','G01','storage clears; no return to bag','GameplaySession','CloseInventory'),('I01/I06','long press','I03','count>1','InventorySlotDrag','Update'),('I01/I06','Split','I04','selected count>1','InventoryDragController','OpenSplit'),('I03','Split','I04','ChooseSplit','StackSplitPanel','ChooseSplit'),('I03/I04','Cancel','I01/I06','parent remains; no consume','StackSplitPanel','Cancel'),('I04','Confirm','I05','partial held; source unchanged','StackSplitPanel','Confirm'),('I05','place/cancel','I01/I06','commit on valid target only','InventoryDragController','PlaceHeld'),('G01','Supply Interact','I07','camp/POI available','CampLootPanel','Open'),('I07','Close/out of range','G01','destroy transient panel','CampLootPanel','CloseFocused'),
 ('G01','Placeable selection','C04','preview while running','PlayerPlacement','LateUpdate'),('G01','Turret Interact','C05','in range','TierThreeHud','OpenTurret'),('G01','Beacon Interact','C06','target exists','TierThreeHud','OpenBeacon'),('G01','Compass Choose','C07','HasCompass','TierThreeHud','OpenNavigation'),('C05/C06/C07','Close/context invalid','G01','CloseInventory','TierThreeHud','Close'),('I01/G01','Telescope Use','C08','selected telescope','InventoryEquipment','Equip'),('C08','Close/Pause','G01','Telescope.Exit','TierThreeHud','Build'),('G01','world context','G13','tomb/door/water/cave etc','PlayerInteraction','TryInteract'),
 ('G01','SP lethal','D01','durable Dead; freeze','PlayerDeath','Lethal'),('G01','MP lethal','D02','not all-down; countdown','PlayerDeath','Lethal'),('G01','hold teammate','D03','near downed; Rescuing','PlayerInputCommands','HoldInteract'),('D03','release/interruption','G01','target still downed','PlayerDeath','InterruptRescue'),('D03/D02','rescue success','G01','HP/protection; world continues','PlayerDeath','CommitTeammate'),('D02','timeout/all-down','D01','Dead; MP only Home choice','PlayerDeath','Lethal'),('D01','SP ad/Gem confirmed','G01','same death revision; actual transaction','PlayerDeath','GemRevive'),('D01','cancel/failure','D01','retain death + free Home','DeathHud','Refresh'),('D01','Free Home','G01','same world HomeAnchor; not MainMenu','DeathHud','Respawn'),('G01','MP Leave','T01','Host save success; client shutdown','MultiplayerArena','Leave'),('G01/M02','host disconnect','H01/M01','session ends; no host migration','NetworkSession','Disconnected')]:route(src,event,dst,cond,cls,method)
 j('navigation',ROUTES)
 text='# Navigation / Back / state ownership\n\n'+COMMON+f'\n의미 있는 조건별 route **{len(ROUTES)}개**. 언어/해상도·슬롯별 전환은 중복 집계하지 않는다. slash는 조건부 목적지/표면군이며 버튼이 연결되는 모든 비용 transaction을 screen route로 세지는 않는다.\n\n'
 text+='''```mermaid
flowchart LR
 Home --> Settings
 Home --> ConfirmNew --> Loading --> Gameplay
 Home --> MPSetup --> Connecting --> Waiting --> Loading
 Home --> ClassOverview --> ClassTree
 Home --> Profile --> Account --> Feedback
 Home --> Store
 Profile --> Store
 Store --> Feedback
 Feedback -->|현재 Back| Account
 Gameplay --> Inventory --> Crafting
 Gameplay --> Character --> CommonTree
 Gameplay --> Pause --> PauseSettings
 Gameplay --> Death -->|무료 HomeAnchor| Gameplay
 Gameplay --> Downed --> Rescue --> Gameplay
 Pause -->|저장 성공| Home
```

## 알려진 Help return 문제: 현재 결론

**CONFIRMED**: QA MainMenu에서 Store.Open → Store.Contact → Account.CloseHelp를 호출하고 각 실제 화면/active view를 캡처했다. Help Back은 Store가 아니라 Account다. `Store.Close`의 정상 Back은 `fromHome`을 기억한다. `Store.Contact`는 Store를 숨기고 `Meta.Profile.Open(); Meta.Profile.Help();`를 부르며 `CloseHelp`는 Account.View=true만 한다. 따라서 과거 “Store Back은 actual origin” 문장은 일반 Back에 한해 맞고 이 하위 경로까지 해결했다는 근거가 아니다. 이번에는 수정하지 않았다.

## 현재 Back 계약과 상태 수명

- HomeScreen은 Home/Settings/Multiplayer/Confirm 네 view만 관리한다. Economy/Class/Store/Account는 각자 flags를 관리하며 중앙 navigation stack이 없다. 유효한 Back 콜백으로 빠져나와야 한다.
- Achievement/Attendance는 `fromProfile`, Store는 `fromHome`; 일반 Back은 caller-sensitive다. Account.Back은 항상 Profile Hub, Help Back은 항상 Account. Class.Back은 Tree 여부에 따라 Overview 또는 Home.
- Pause.Settings Back은 Pause menu이며 Home.Settings Back과 다르다. Inventory/Crafting/Chest Close는 보통 Running으로 닫고 이전 subpanel로 돌아가는 stack이 아니다. Common Tree Back은 Character이다.
- Class.Open은 selected profile·node0·status를 다시 읽고 tree를 닫는다. Class card 변경은 node0. Economy category는 selection과 양쪽 scroll top을 리셋한다. Attendance tab은 재진입 때 남을 수 있고 Events 재탭은 다음 event를 고른다. Craft category/filter는 station kind가 바뀔 때 초기화; 가려진 selected recipe는 첫 visible 항목으로 fallback한다.
- HUD Gameplay CanvasGroup은 Running에서만 raycast/interactable. Split은 parent 위로 sibling을 올리고 ghost는 raycast off다. CampLoot/TierThree는 Session.Inventory를 사용하며 inventory view의 exclusion flags로 중복 표시를 막는다. Maintenance는 session event에서 즉시 숨기고 hold를 중단한다.
- source-level 차단 근거는 있지만 모든 rare modal 조합의 실제 pointer leak 부재를 이 감사에서 증명한 것은 아니다. Inventory/Character/Station의 세계 진행은 의도된 정책이며 “pause 실패”로 바꾸지 않는다.
- Transition.Load는 SP load 중 Cancel을 숨긴다. Network connect는 Cancel/Leave operation ticket을 무효화하며 뒤늦은 async 응답을 적용하지 않는다. world 준비45초/느림20초는 실패 안내 경계이지 실제 진행률이 아니다.
- Host Leave는 정상 save 실패시 머무른다. `MultiplayerMenu.Back()`은 Leave(false) 다음 Home.ShowHome를 호출하므로 Host save 실패/Closing 상태의 화면 일치 여부는 source 위험점이며 이번 live Host 실패를 재현하지 않았다.

## 감사 드라이버의 잘못된 경로와 배제

첫 캡처 반복에서 Attendance.Back 없이 Home.ShowHome를 직접 불러 기존 Attendance view가 남은 화면을 만들었다. 이는 일반 사용자 route 재현이 아니다. `live-frontend-attempt1.json`에 원인을 남겼고 `home-v2/settings-v2/...`는 실제 Back을 거친 다시 캡처한 증거다. 이 인위적 overlay를 제품 버그로 집계하지 않는다. 삭제 화면은 숨겨진 Delete callback을 호출한 QA preview이며 Linked gate를 통과한 계정 삭제 체험이 아니다.

## 전체 route ledger

'''+table(['ID','From','event','To','조건·상태','source'],[(r['id'],r['src'],r['event'],r['dst'],r['condition'],r['source']) for r in ROUTES])
 write('NAVIGATION_AND_STATES',text)

if __name__=='__main__':
 inventory();actions();navigation()
 print(json.dumps({'screens':len(S),'actions':len(A),'routes':len(ROUTES)}))
