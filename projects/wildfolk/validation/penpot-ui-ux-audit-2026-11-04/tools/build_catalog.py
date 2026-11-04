"""Source-backed semantic inventory. Repeated slots/cards are parameterized actions."""
import sys,json,re,collections
sys.dont_write_bytecode=True
import audit_session as a
S=[]
def screen(id,name,cls,builder,entry,back,controls,data,states,scroll='없음',style='field',status='IMPLEMENTED'):
 S.append(dict(id=id,name=name,controller=cls,builder=builder,entry=entry,exit=back,controls=controls,data=data,states=states,scroll=scroll,style=style,status=status))
# Distinct tasks/interaction surfaces, not one screen per language/recipe/class/item.
screen('H01','Home / 야영지','HomeScreen','FrontendSceneBuilder','앱 MainMenu; Pause 저장 후 복귀','앱 기본 화면','Continue; Single Player; Multiplayer; Class; Settings; Profile; Gem Store; 업적; 출석','유효 save 일차; 실제 Gem/cache 또는 —; 배지','저장 없음 / 유효 / primary 손상·backup 가능 / 전체 invalid / 전환 중',style='home')
screen('H02','새 세계 교체 확인','HomeScreen','FrontendSceneBuilder','H01 SinglePlayer: Repository.Exists','Cancel → H01; BeginNew → T01','Start New World; Cancel','기존 단일 저장 교체 경고','확인 / 취소 / transition guard',style='sheet')
screen('H03','설정','SettingsPanel','SurvivalUiBuilder','Home.Settings','Back → H01','Control size; Master; BGM; SFX sliders; Language cycle; Reset; Back','0.8–1.4; 0–1 audio; EN/KO; 백분율','정상 / 즉시 언어·오디오 반영; Reset 즉시',style='sheet')
screen('M01','협동 Create or Join','MultiplayerMenu','MultiplayerUiBuilder','H01 Multiplayer','Back → H01; Single → H02 또는 T01','Internet/LAN 전환; code/address 입력; Host; Join; Single Player; Back','route; saved Host 유무; status','Idle / invalid code·IPv4 / Connecting / failure / Closing',style='sheet')
screen('M02','대기실','MultiplayerMenu','MultiplayerUiBuilder','NetworkSession.Waiting','Back → Leave(false) → H01','Host Enter; Internet Copy Code; Back','Room Code 또는 LAN ready; server 인원/정원4; client waiting','Host / Client / room full / duplicate / host left',style='sheet')
screen('P01','직업 개요','ClassLobbyScreen','JournalContentAssets','H01 Class','Back → H01','6 Card preview; Select; Purchase; OpenTree; Back','선택/소유/해금 비용; Class XP/points; Gold/Gem','owned / locked / selected / insufficient / profile failed',style='book')
screen('P02','직업 Skill Tree','ClassLobbyScreen','JournalContentAssets','P01 OpenTree','Back → P01','9 Node select per class; Invest; Respec; Back','6×9 실제 node·3가지 branch; prerequisite; XP point','locked / prerequisite / available / learned / no point / not lobby',style='book')
screen('P03','직업 구매 확인','ClassLobbyScreen','JournalContentAssets','P01 AskPurchase gate 통과','Cancel → P01; Confirm → P01 결과','Confirm; Cancel','정확한 Class 이름·비용·Gold/PremiumGem','Gold transaction / Engineer server unlock / failed',style='sheet')
screen('A01','Profile & Records','EconomyScreen','EconomyUiBuilder','H01 Profile; A02 Back','Back → H01','Account; Achievements; Attendance; Store; optional ad; Retry; privacy choices; Back','선택 직업/월드 level; linked/Alpha; Gem; completion/points/badges','offline catalogue / linked / syncing / failure / reward state',style='sheet')
screen('A02','Account','AccountProfileScreen','AccountUiBuilder','A01 Account; E05 Contact가 경유','Back → A01','Connect; Sync; Alpha enroll; SignOut; Help; Privacy; Delete; CopySupport; Back','provider; cloud 상태; support ID; Alpha','Offline / Connecting / CloudSyncing / Connected / DeletionPending / Failed',style='sheet')
screen('A03','Help & Feedback','AccountProfileScreen','AccountUiBuilder','A02 Help; E05 Contact','CloseHelp → A02 항상: Store origin 소실','Category dropdown; message1500; consent; Submit; Back','선택 category6; 명시 진단 동의; ticket/failure','editing / submitting / rate limited / success / failed',style='sheet')
screen('A04','Feedback category dropdown','AccountProfileScreen','AccountUiBuilder','A03 category click','선택 또는 바깥 닫기 → A03','6 Toggle-like option; vertical scroll','Bug/Game/Account/Purchase/Player/Other 현 언어 caption','UGUI Dropdown template; single choice','옵션 목록',style='sheet')
screen('A05','계정 삭제 확인','AccountProfileScreen','AccountUiBuilder','A02 Delete: Linked gate','Cancel → A02; Confirm 2회 → service 요청','Confirm deletion; Cancel','첫 경고 → 재확인 경고 → pending/error','2-step / DeletionPending / Failed',style='sheet')
screen('A06','Account/Cloud 충돌 선택','AccountProfileScreen','AccountUiBuilder','AccountConnection.AccountConflict','KeepLocal 또는 명시 UseExisting/UseCloud → service; Back → A01','Use existing; Keep local; Use cloud; Back','LastError account_conflict/cloud_conflict; normal actions hidden','구현됨; 제품 backend OFF에서 자연 도달 불가',style='sheet',status='DISABLED_IMPLEMENTED')
screen('E01','업적','EconomyScreen','EconomyUiBuilder','H01 direct 또는 A01','Back → 호출 H01 또는 A01','7 category; family select22; Claim selected; Claim all; error-only Retry; Back','22 family58 tier; progression/point/Gem; server claimability','offline read-only / syncing / empty category / completed / claimable / failed','family 목록 + tier 상세',style='sheet')
screen('E02','신규 출석','EconomyScreen','EconomyUiBuilder','H01/A01 Attendance; Board0','Back → 호출 H01/A01','신규/월간/Events tabs; Attend; error-only Retry; Back; tiles 비조작','누적7회; 실제 보상3/4/5/6/7/8/12; UTC reset','offline / today done / eligible / complete / failure',style='sheet')
screen('E03','월간 출석','EconomyScreen','EconomyUiBuilder','E02 Board1','Back → 호출 H01/A01','same tabs; Attend; Retry; Back; 28 tiles 비조작','28방문; 7·14·21·28회6/8/10/16Gem; stamp는 통화 아님','UTC 월 경계 / eligible / complete / offline',style='sheet')
screen('E04','기간 출석 / Events','EconomyScreen','EconomyUiBuilder','Board2; 재선택 시 event index cycle','Back → 호출 H01/A01','Board tabs; Attend(정의+eligibility 필요); Back','현재 배포 Events=[]; 없으면 no-event','구현됨 empty; 실제 event 보상은 미공개',style='sheet',status='PARTIAL')
screen('E05','Gem Store','StoreScreen','StoreUiBuilder','H01 Gem 또는 A01 Store','Close → fromHome H01 아니면 A01; Help → A03','3 package select; Purchase; Recover; History; Purchase help; Back','24/72/144Gem; platform price; wallet; selected uses; status','OFF preview / Fetching / Starting / Pending / Validating / Consuming / Completed / Cancelled / AccountRequired / Failed / ReviewRequired',style='sheet')
screen('E06','구매 내역','StoreScreen','StoreUiBuilder','E05 ToggleHistory','ToggleHistory → E05; Back → origin','History toggle; Back; Recover; Support','서버 구매 이력; 비어 있음; 미확정 결제 상태','empty / loading / failure / history','history 목록',style='sheet')
screen('E07','선택형 광고 상태','EconomyScreen','EconomyUiBuilder','A01 optional WatchGem/Retry','A01 내 상태; 외부 native ad 후 복귀','Watch; Retry; 실제 UMP privacy options','+1Gem; loading/unavailable/cancelled/pending/received','구현됨; Ads Enabled=false; native provider 화면은 외부',style='sheet',status='DISABLED_IMPLEMENTED')
screen('T01','Loading / Transition overlay','TransitionOverlay','runtime Build','SceneFlow.Go; NetworkSession Watch','network Cancel → Leave; failed load → recovery; SP loading 취소 불가','Cancel 조건부; spinner','brand; phase; slow 안내20s; world readiness45s','Connecting / waiting / loading / slow / failed / cancelled',style='overlay')
screen('G01','Gameplay HUD','GameplayHud','GameplayHudBuilder','Wildfolk 또는 Multiplayer 로컬 actor bind','Bag/Character/Pause 또는 die','profile; Bag; hotbar9; Pause 또는 MP Leave; gameplay actions','HP/XP/level/points; hunger/thirst; day/phase; Gold; class','Running / modal input lock / SP freeze / MP world continues',style='hud')
screen('G02','Floating movement / Sprint','VirtualJoystick','GameplayHudBuilder','Running 왼쪽 입력 영역 touch','release/cancel/focus loss/state change → zero','pointer down/drag/up; 자동 sprint threshold','deadzone .12; first pointer ownership; transient base','idle visual hidden / moving / sprint / exhausted',style='hud')
screen('G03','Primary action','HoldAttackInput','GameplayHudBuilder','Running 오른쪽 primary','release/state/focus reset','tap; attack-only hold repeat','선택 item 기반 Attack/Place/Consume/Equip/utility','availability / cooldown / no ammo / no tool / placement blocked',style='hud')
screen('G04','Interact / Revive hold','InteractButtonView','GameplayHudBuilder','Running context target 또는 downed teammate','release/exit/state change','click Use; hold rescue 우선','facility/door/water/loot/equipment icon; downed 우선','unavailable / context / teammate hold / interrupted',style='hud')
screen('G05','Class active','ClassHud','ClassContentAssets','해당 class+Running','hold release/session change','Warrior Guard hold; Engineer Overclock click','class ability; cooldown; eligible turret','class-specific / ready / cooldown / not eligible',style='hud')
screen('G06','Class marker / Quarry feedback','ClassHud','ClassContentAssets','Hunter quarry 또는 signal-capable class','duration expiry; state change','Mark; CycleMeaning3','bearing/distance/condition; team marker list','available / cooldown / detailed branch / expired',style='hud')
screen('G07','Medic ally treatment','ClassHud','ClassContentAssets','Medic+near living peer+selected medicine','finish/release interruption','Medicine click','선택 의약; ally range; role gates','no ally / no medicine / active / cooldown',style='hud')
screen('G08','Repair / Dismantle context','ClassHud','MaintenanceUiBuilder','가까운 PlayerBuilt 시설+Running','멀어짐/메뉴/state; hold release cancels','Repair hold; Dismantle hold; materials scroll','대상 이름/HP/buffer; 보유·필요; 실제 반환; hold fill','healthy / recent damage / missing / contains cargo / active / complete / bag full','재료·반환 상세',style='hud')
screen('G09','Boss bar / world telegraphs','BossHud','GameplayHudBuilder','BossIdentity.Active alive','death/disable 숨김','표시 전용','보스 이름/HP; Returning color; attack VFX는 world renderer','active / returning / dying; numeric/phase label 없음',style='hud')
screen('G10','Navigation status','NavigationHud','TierThreeAssets','Compass 또는 survey/signal/rescue hint','조건 종료/Off','표시 전용; 별도 C07 선택','North/target arrow; distance; cave exit/Home/Beacon; rescue 우선','no Home / no target / near / survey phases / peer signal',style='hud')
screen('G11','Feedback / toast','PlayerFeedback','GameplayHudBuilder','action result events','1.5–3s timeout; 다음 메시지가 교체','표시 전용','pickup 합산; crafting/save/errors; fishing icon; refund 최대2+more','success / blocked / item requirement / bag full / save fail',style='hud')
screen('G12','Medicine / Meal / Stamina overlays','MealHud','SurvivalUiBuilder','효과 진행 또는 stamina 사용','만료; full delay .7s fade; menu hide 정책 다름','표시 전용','treatment seconds; Meal icon/time; player-adjacent arc','active / no effect / full / modal',style='hud')
screen('G13','World interaction prompts','PlayerInteraction','GameplaySceneBuilder','물/문/동굴/우물/Graveyard/RoyalTomb 등 in range','action 결과 또는 멀어짐','Interact/Primary context; 별도 quest/map screen 없음','도구 조건; 물 품질; tomb 밤/Bone/guard/이미 소환 여부','거리/시야/guard/time/material gate; 결과 toast',style='world')
screen('I01','Inventory / Equipment','InventoryPanel','InventoryUiBuilder','Running Bag; crafting OpenBag','Close → Running','36 select; equipment4 select; Equip/Unequip; Crafting; Split; Close; drag','Hotbar9+Storage27; portrait; stack/charges; item category','empty / selected / equipment filter / full / dragging',style='field')
screen('I02','Item details / Comparison','InventoryEquipment','InventoryUiBuilder','I01 selected item/equipped slot','selection change; I01 close','Equip/Unequip; Telescope Use; item preview','stat description; Head/Body defense delta; utility/accessory text','equipped / incompatible / insufficient bag space; durability bar 없음',style='field')
screen('I03','Item Actions','StackSplitPanel','StackSplitUiBuilder','stack count>1 long press .45s','Cancel → parent; Split → I04','Split; Cancel','아이템 icon/name','단일 stack은 열리지 않음',style='field')
screen('I04','Split Stack','StackSplitPanel','StackSplitUiBuilder','selected Split 또는 I03','Cancel → parent; Confirm → I05','−1;+1;Half;Confirm;Cancel','1..total−1 선택량/전체량','min / max / source changed',style='field')
screen('I05','Held partial / Drag overlay','InventoryDragController','InventoryUiBuilder','full drag 또는 I04 Confirm','Cancel/invalid empty release preserves source; valid destination commits','drag; drop slot/equipment; Drop to ground; Cancel','ghost; reserved remainder; compatible marker','whole / partial / stale revision / unsupported equipment-chest / no space',style='overlay')
screen('I06','Chest transfer','ChestPanel','ChestUiBuilder','PlayerStorage.Open in range','Close → Running; storage invalid closes','양측 slot select/drag; Split; Close','Chest24/강화36; Player36; shared model/revisions','empty / full / competing owner / unavailable','각 가방6열 vertical',style='field')
screen('I07','Camp / POI Supplies','CampLootPanel','runtime Build','secured or guarded IWorldSupplies.Use','Close/state/range invalid → Running','6 Take buttons; Close','6 rolled item/name/count; guarded/remaining/empty','guarded disables; empty; bag full; stale revision',style='legacy-runtime')
screen('C01','Crafting / Station','CraftingPanel','CraftingUiBuilder','Inventory hand; 시설 Interact','Close → Running; Bag → I01','category7; craftable filter; recipe select; per-card craft/cook/take; Bag; Close','Hand/WBI/II/III/Campfire/Pot/Furnace/Blast/Turret WB variants; costs; description; NEW','Ready/MissingMaterials/NoSpace/StationRequired/Processing/Collect; selection fallback','recipe list + item details + upgrade costs',style='field')
screen('C02','Sequential processing queue','CraftingPanel','CraftingUiBuilder','Pot/Furnace/Blast Sequential station','parent Close; outputs remain world','+1; Add all; waiting−1; Collect','current recipe/input/output/time; waiting count; ready output','idle / waiting / processing / blocked output / bag full',style='field')
screen('C03','Workbench upgrade / Set Home','CraftingPanel','CraftingUiBuilder','C01 WBI/II','parent Close; upgrade keeps open context','Upgrade; Set Home (navigation anchor)','tier-specific actual costs; next tier; chosen Home','insufficient / upgradeable / III no-upgrade; not respawn Home','cost scroll',style='field')
screen('C04','Building placement preview','PlayerPlacement','GameplaySceneBuilder','selected placeable + Primary','item selection/menu; confirm primary consumes only success','Primary placement; movement/aim','valid/invalid ghost; terrain/overlap/range; selected stock','allowed / water/overlap / stale/full / no item',style='world')
screen('C05','Turret controls','TierThreeHud','runtime Build','in-range turret Interact','Close/state/range invalid → Running','aim drag; Fire; Reload; Deposit; ammo3 withdraw; module5 install/remove','ammo slots; HP; reload/time; modules; overclock','unloaded/ready/reloading/no ammo/locked slot/not compatible',style='legacy-runtime')
screen('C06','Beacon controls','TierThreeHud','runtime Build','Beacon Interact','Close; Track/Recover closes','Track; Recover; Close','Beacon number','range/ownership/space authoritative gates',style='legacy-runtime')
screen('C07','Navigation target chooser','TierThreeHud','runtime Build','Compass Choose','selection/Close → Running','Off; Automatic; Home; CaveExit; previous/next Beacon; Track; Close','sorted Beacon number; no target','no beacon / missing Home / outside cave',style='legacy-runtime')
screen('C08','Telescope controls','TierThreeHud','runtime Build','selected Telescope use/Inventory Equip label Use','Close or Pause → Exit','direction drag; Close','aim hint; rendered distant view','IsViewing blocks regular actions; no simulation expansion',style='legacy-runtime')
screen('P04','Character Information','CharacterScreen','CharacterUiBuilder','Gameplay profile','Close → Running; OpenTree → P05','Skill Tree; Close','world LV/XP/HP/Defense/Stamina/Needs; class selected/status','4Hz display; class/world distinction; no equip manipulation',style='book')
screen('P05','Common growth tree','SkillTreeScreen','CharacterUiBuilder','P04 Skill Tree','Back → P04; Close → Running','6 select; Invest; Back; Close','rank/prerequisite/point; 6 common skills','locked / available / learned / max / insufficient; Respec 없음',style='book')
screen('S01','Pause','PauseMenu','SurvivalUiBuilder','Gameplay Pause or Escape','Resume → Running; settings Back → S01','Resume; Save; Save & return home; Settings','save result toast; scene/role-specific availability','SP frozen; MP has Leave instead of global pause',style='field')
screen('S02','Pause Settings','SettingsPanel','SurvivalUiBuilder','S01 Settings','Back → S01 (caller-specific)','동일4 sliders; language; Reset; Back','Local user preference shared H03','isolated RAM / ordinary PlayerPrefs; world save와 무관',style='sheet')
screen('D01','Death / Revive choices','DeathHud','DeathUiBuilder','PlayerLife Dead/Respawning','Free Home → 같은 월드 HomeAnchor; 앱 Home 아님','SP Ad/Retry; 8Gem; Free Home; MP only Home','Durable/save failure; payment status; ad-used/short/unavailable','dead / respawning / pending / late callback reject / failure',style='sheet')
screen('D02','Downed / rescue countdown','DeathHud','DeathUiBuilder','MP lethal downed','24s deadline or all-down → Dead; revived → Running','표시+teammate rescue via G04','remaining seconds; hold progress','Downed / Reviving / Dead; offline SP downed 없음',style='hud')
screen('D03','Helping teammate / Rescue','DeathHud','DeathUiBuilder','alive peer near downed, hold G04','release/move/damage/LoS/disconnect interrupts','Interact hold; release; move cancel','4s progress, Medic modifiers','Rescuing locks attacks until interruption; success40%HP/2s protection',style='hud')
for row in [
 ('F01','Multiple world selection','FUTURE_GOAL','사용자 차기 목표; 현재 단일 저장 진입만 있음'),
 ('F02','World rename/delete/management','FUTURE_GOAL','world metadata/index/안전 삭제·migration 결정 필요'),
 ('F03','World create options / host same world','FUTURE_GOAL','현재 random seed; Solo/Host 별도 파일. config UI만 추가 불가'),
 ('F04','Public server browser / Matchmaking','NOT_IMPLEMENTED','현재 Room Code 또는 IPv4 직접 참여; 발견 목록 없음'),
 ('F05','Quest / Tutorial journal','NOT_IMPLEMENTED','현재 tooltip/toast와 Recipe 상태만; quest mechanics 발명 금지'),
 ('F06','Full map / minimap','NOT_IMPLEMENTED','개발용 WorldOverview는 Editor; 제품 Compass와 다름'),
 ('F07','Durability / repair equipment','NOT_IMPLEMENTED','현재 시설 HP Repair; 개별 무기 내구 UI/장비 수리 없음'),
 ('F08','Back gear / skin shop','NOT_IMPLEMENTED','future Back anchor는 실제 장비 칸/상품 아님'),
 ('X01','구 Character 3-tab / detached CLASS / player Load','HISTORICAL_ONLY','현재 Character/tree 분리, Class Home행; player Load 제거됨')]:
 screen(row[0],row[1],'—','—',row[3],'해당 없음','해당 없음','현재 구현으로 인계 금지','해당 없음',status=row[2])
for s in S:
 s['scene']='MainMenu' if s['id'][0] in 'HMAE' or s['id'] in ['P01','P02','P03'] else 'Wildfolk / Multiplayer' if s['id'][0] not in 'FX' else '없음'
 s['composition']='fullscreen overlay' if s['style'] in ['sheet','book','field'] else 'nonmodal HUD' if s['style'] in ['hud','world','home'] else 'blocking overlay'
 s['network']='로컬 presentation; backend/account 서버 상태의 gate 유지' if s['id'][0] in 'AE' else 'SP 직접 모델 / MP owner request→Host 검증; 표시가 권위 아님' if s['scene']=='Wildfolk / Multiplayer' else 'local frontend; 세션 요청은 NetworkSession'
 s['localization']='L / LocalizedLabel EN·KO, 동적 문자열 L.Changed; 고정 Lowfire 브랜드' if s['controller']!='—' else '설계 미정'
 s['confidence']='SOURCE-INFERRED' if s['controller']!='—' else s['status']
a.save('screens',S)

# Resolve persistent callback targets from actual YAML, rather than inferring by method name.
controls=json.loads((a.ROOT/'serialized-controls.json').read_text(encoding='utf8'))
targets={}
for path in (a.ASSETS/'Scenes').glob('*.unity'):
 text=path.read_text(encoding='utf8')
 for m in re.finditer(r'^--- !u!\d+ &(-?\d+)(?: stripped)?\n(.*?)(?=^--- !u!|\Z)',text,re.M|re.S):
  c=re.search(r'^  m_EditorClassIdentifier: (.*)',m[2],re.M)
  if c:targets[path.name,m[1]]=c[1].strip().split('::')[-1].split('.')[-1]
actions={}
for c in controls:
 for b in c['callbacks']:
  cls=targets.get((c['scene'],b['target']),'UNRESOLVED');key=cls+'.'+b['method']
  item=actions.setdefault(key,{'callback':key,'controller':cls,'method':b['method'],'instances':[],'origin':'serialized'})
  item['instances'].append({'scene':c['scene'],'path':c['path'],'line':c['line'],'type':c['type']})
# Nonserialized listeners, pointer interfaces and live-created panels; one action per semantic operation.
dynamic={
 'VirtualJoystick':['OnPointerDown','OnDrag','OnPointerUp'],
 'HoldAttackInput':['OnPointerDown','OnPointerUp'],
 'NetworkAttackInput':['OnPointerDown','OnPointerUp'],
 'InteractButtonView':['Click','OnPointerDown','OnPointerUp','OnPointerExit'],
 'InventorySlotDrag':['OnPointerClick','OnBeginDrag','OnDrag','OnEndDrag','OnDrop','LongPress'],
 'EquipmentSlotDrag':['OnBeginDrag','OnDrop','OnEndDrag'],
 'InventoryDropZone':['OnDrop'],
 'InventoryDragController':['PlaceHeld','DropHeldWorld','Cancel'],
 'RecipeCardView':['OnPointerClick'],
 'ClassHud':['Overclock','Mark','Medicine','Guard','Service'],
 'DeathHud':['Reward','Pay','Respawn'],
 'CampLootPanel':['Take','CloseFocused'],
 'TierThreeHud':['Close','Deposit','Install','Operate.Aim','Operate.Fire','Operate.Reload','Operate.Withdraw','Operate.Remove','Choose.Off','Choose.Automatic','Choose.Home','Choose.CaveExit','Cycle','TrackBeacon','RecoverBeacon','OpenNavigation','TelescopeAim','TelescopeExit'],
 'TransitionOverlay':['Cancel'],
 'MultiplayerMenu':['DestinationChanged'],
 'AccountProfileScreen':['CategorySelection','DiagnosticConsent','MessageInput'],
 'EconomyScreen':['FamilyScroll','DetailScroll'],
 'CraftingPanel':['RecipeScroll','SummaryScroll','UpgradeScroll'],
 'ChestPanel':['PlayerScroll','ChestScroll'],
 'StoreScreen':['HistoryScroll'],
 'ClassHud':['Overclock','Mark','Medicine','Guard','Service','MaterialsScroll']}
for cls,methods in dynamic.items():
 for method in methods:
  key=cls+'.'+method
  actions.setdefault(key,{'callback':key,'controller':cls,'method':method,'instances':[],'origin':'runtime / gesture'})
a.save('actions-raw',list(actions.values()))
print(json.dumps({'screens':len(S),'implementedSurfaces':sum(s['controller']!='—' for s in S),'semanticActions':len(actions),'unresolvedCallbacks':[k for k in actions if k.startswith('UNRESOLVED')]}))
