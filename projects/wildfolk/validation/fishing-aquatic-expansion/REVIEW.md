# Content Expansion 2-D · Fishing / Aquatic

2026-10-21. 시작 remote main을 fetch하여 `a6eafaec742fcc83d95c087370dfade38c213d56` 확인, local main과 일치·clean. 아래 조사/감사/결정은 runtime 수정 전에 기록했다. 실제 구현·검증은 뒤에 별도로 갱신한다.

## 기존 구조 감사

- Item127/Recipe104/Save11/Protocol15. Rod: WBI Wood4+Fiber3, stack1, ToolDefinition 없이 bool FishingRod와 held sprite. PlayerPrimaryAction→PlayerFishing.Use, Host는 기존 owner Primary RPC의 단조 request 번호를 검사한다.
- PlayerFishing이 WorldMap.Contains/IsWater를1.5~5unit/.5 간격으로 검색해 **첫 물**을 선택한다. Casting .45초, cast 시작3초에 Bite,2.2초 반응창, 성공 후 .6초 회수. 최소3.6초/최대5.8초+사용자 재입력 시간이다. 실제 관찰 시간은 검증 절에 구분한다.
- 보상은 Fish 참조의 raw_fish1을 InventoryModel.TryExchange로 원자 지급한다. 가방 full은 보상 없이 Bite 유지·재시도, 성공 시 Caught라 중복 지급을 막는다. 그러나 Use 자체는 지난 시간/이동을 재검사하지 않아 Update 직전 늦은 입력을 별도로 보강할 필요가 있다.
- 사망/선택 도구 변경/1.2unit 이동/Session 모달은 Update에서 Cancel. ActivityState(Sequence/Kind/Started/Duration/Point)로 line/bobber/bite를 표시한다. 새 catch 정체성은 전송하지 않는다. 원격 보상 표시도 없다.
- WorldMap generation1은 frozen legacy 강/호수/연못과 signed WaterDistance를 제공한다. generation2/3는 Continental.Environment.Biome의 Wetlands, CoastDistance<0 바다, signed WaterDistance와 DeepWater를 제공한다. 수직 수심은 없다. WaterDistance는 육지에서 물 안쪽으로 들어간 거리의 기하 근사다. Ford가 깊은물 도보 판정을 바꾸므로 DeepWater와 거리 조건을 함께 사용한다.
- Caverns/Deep Caverns pool은 존재하지만 WaterDistance는 -2/15의 이진 값이다. 여기에 정밀 수심을 만들어내지 않는다. 동굴은 기존 공용 raw_fish fallback만 유지한다. generation1에는 Wetlands/Ocean을 추측하지 않는다.
- WorldClock.Model.Phase는 Day/Evening/Night/Dawn,480초 cycle의 Night150초. Host 시계를 cast 시점에 읽으면 조건 확정이 가능하다.
- RecipeDefinition.Ingredients는 정확한 ItemDefinition+Count/Charges/Quality 배열이다. Fish tag/group은 없다. 가방/제작/NGO/Save에 걸친 새 ingredient language 대신 손질 결과를 기존 raw_fish로 연결한다.
- Save11은 item ID/stack/selected 및 공통 queue를 저장한다. 낚시 phase/미수령 catch는 저장하지 않는다. PlayerSnapshotCodec.Apply는 Survival transient만 초기화하므로 같은 객체 Load 때 Fishing도 명시 초기화해야 한다. reHost는 새 actor가 Idle로 시작한다.
- 현재 cooked_fish H26/T0, fish_stew42/26, mixed_stew65/18/HP8, trail_broth28/22+150초 recovery10%. 기존 레시피/Meal3/의약/Skill4개를 보존할 수 있다.

## 외부 공개 자료 비교

공식/공식 커뮤니티 Wiki를 우선했다. wiki.gg 일부 본문 open은403, Fandom 일부는robots 차단이므로 이 경우 검색에 실제 공개된 해당 Wiki 표/본문만 확인했다. 제3자 가이드의 추측 확률은 사용하지 않았다. 아래 확률은 외부 구조의 사례이며 Lowfire에 복제하지 않는다. Community Wiki의 버전/정확도 한계도 있다.

|게임|해당 시스템|실제 사례|장점|Lowfire와 차이|Lowfire 적용 여부|이유|
|---|---|---|---|---|---|---|
|Core Keeper|물/지역과 보상|[Fishing](https://corekeeper.atma.gg/en/Fishing): neutral water는 biome, seawater/lava 등은 물 종류가 table을 결정. Dirt의 Orange/Blue Guppy·Rock Jaw·Gem Crab에 서로 다른 빈도, 몰드 물은 Mold Shark 등이 있다.|낚을 장소가 발견물과 연결|Lowfire는 액체 이동·용암낚시 없음|지역별 table 채택|실제 있는 inland/Wetlands/Ocean만 사용|
|Core Keeper|비어류/진행|같은 Fishing 자료에 Copper/Tin/Scarlet ore, Scrap/장신구·열쇠/상자가 존재하며 fishing power gate와 bait·skill이 작동.|음식 외 보상 폭|Lowfire의 채광/제작대 축을 우회할 위험|비어류·power gate 배제|이번에는 광물·장비·잡동사니를 낚지 않음|
|Core Keeper|음식과 clutter|[Orange Cave Guppy](https://corekeeper.atma.gg/en/Orange_Cave_Guppy)는 생식/조리 역할과 조리 후 채광 효과를 구분. [Cooking](https://corekeeper.atma.gg/en/Cooking)은 두 재료 조합별 별도 stack, 동일 조합만 합쳐지고 rarity도 나뉜다.|재료별 식사 선택|Meal/36칸 가방에 조합 결과를 늘릴 필요 약함|효과 다중 추가 배제|손질 공용재와 소수 음식으로 제한|
|Don't Starve Together|수역/계절/식재료|[Ocean Fishes](https://dontstarve.wiki.gg/wiki/Ocean_Fishes): Mudfish는 Coastal/Swell, Deep Bass는 Swell/Rough, Dandy Lionfish는 위험 바다 쪽. Fallounder는 가을, Ice Bream은 겨울. 큰 생선/작은 생선은 공용 고기·fish 음식값으로 처리된다.|발견 장소와 요리 호환을 함께 유지|Lowfire에 계절·배·해상 생물 simulation 없음|공용재 처리 원칙 채택|이동하는 물고기 simulation 대신 cast context|
|DST|선택 미끼/시간|[Lures](https://dontstarve.wiki.gg/wiki/Nightflyer_Spinnerbait): bare hook도 가능. Berry/Seed는 일회 소비, Sunrise/Dusky/Nightflyer는 시간대별 효율 차이. [Sea Fishing Rod](https://dontstarve.wiki.gg/wiki/Sea_Fishing_Rod)는 ocean fish 외 Wobster/해양 잔해도 가능.|강제 미끼 없이 준비 선택, 시간대 활용|현재3초 대기는 이미 짧고 새 미끼 UI/slot 필요|무미끼 유지, 밤 보상만 작게 채택|시간 감소/희귀 보정 필수화 방지|
|Terraria|수역/수심/희귀·상자|[Fishing](https://terraria.wiki.gg/wiki/Fishing): biome와 height/liquid가 catch를 제한, 작은 호수는 power 불이익. Ocean은 bobber 위치 판정. crate는 별도 roll이며 광물/장비도 공급.|수면 위치와 진행 연결|Lowfire에는 tile 물량/낚시 power 없음|bobber 위치 원칙만 채택|가상의 호수 부피·상자 경제를 만들지 않음|
|Terraria|음식 외 용도/도구/시간|[Specular Fish](https://terraria.wiki.gg/wiki/Specular_Fish)는 지하 Forest/Snow에서 잡혀 Seafood Dinner·Recall/Wormhole Potion에 사용. Fishing 자료는 bait 필수, rod/bait/potion의 power와 시간대 보정, Angler의 biome/높이 quest를 설명.|skill 없이 장비·장소·소비 목적이 진행 형성|Lowfire는 potion/Angler/quest 범위 밖|역할 차이만 채택|이번 progression은 장소 발견·손질/조리 선택, skill/quest 없음|
|Necesse|육지 거리/도구|[Fishing](https://www.necessewiki.com/Fishing): 적합 수면은 육지2tile 이상, Baby Shark 등 quest fish는10tile 초과의 Deep Water. Bait는 power 증가, potion/Depths Catcher는 여러 line. [Rods](https://necessewiki.com/Fishing_Rods)는 Wood/Iron/Overgrown 등의 power 차이.|측정 가능한 장소 조건·장비 진행|Lowfire shore rod는5unit,2tile/10tile을 그대로 쓰면 접근 불가|거리 기반 깊은수면 채택, rod0|현재 rod 범위에서 읽히는 -2.25unit 조건|
|Necesse|지역별 비음식 sink|[Swampfish](https://www.necessewiki.com/Swampfish)는 Swamp surface에서 잡혀 Ranger/Battle/Rapid Potion과 Relic 제작에 사용된다. shallow 확률 언급은 Wiki의 brief testing으로 명시되어 있어 수치 근거로 쓰지 않음.|지역 어종에 반복 수요|별도 연금/Relic 체계 없음|지역 identity 채택·포션 배제|현재 지원 음식/공용재 범위에 맞춤|
|Valheim|종/처리/보상|[Fish](https://valheim.fandom.com/wiki/Fish): Meadows Perch, semi-deep Pike, Ocean Tuna/Coral Cod와 다른 bait·stamina 요구. [Raw fish](https://valheim.fandom.com/wiki/Raw_fish)는 종/quality별 다른 손질 수율, Cooked Fish→Wraps 등 공용 조리. 일부 fish에 ore/보석 bonus 존재.|종 정체성을 유지하며 식재료 통합|Lowfire에 quality·낚시 stamina·merchant 없음|처리 모델B 채택, bonus 배제|품질 stack 조합과 진행 우회 방지|
|Valheim|Bait progression|[Bait](https://valheim.fandom.com/wiki/Bait): 기본은 Haldor 구매, Trophy와 기본 bait로 지역별 bait 제작.|전투→낚시 준비 연결|상인·trophy·필수bait loop 없는 Lowfire|배제|기존재료를 억지 bait로 소비할 근거 부족|
|Grounded|aquatic 지역/자원|[Koi Pond](https://grounded.wiki.gg/wiki/Koi_Pond): 깊은 수중·산소 bubble·Koi/Spider 위험, Eelgrass/Wax/Tadpole. [Raw Tadpole Meat](https://grounded.wiki.gg/wiki/Raw_Tadpole_Meat)는 조리/건조·Decoy Bait·Liquid Gills에 사용.|환경·도구·채집·소비가 연결|본격 수영/산소/수중 전투가 전제|향후 후보로 보류|이번 aquatic은 수면·shore fishing 안에서 끝냄|

## 핵심 결정: 사례 → 현재 문제 → 채택 → 배제

|결정|외부 사례|Lowfire 현재 문제|채택 원칙|채택하지 않은 대안과 이유|
|---|---|---|---|---|
|Catch 수/정체성|Core Keeper 수역 table, Necesse Swampfish, Valheim 처리 수율|단일 Raw Fish|기존 공용 catch+4종으로5 identity, 장소/밤/수율/용도 차이|6종 강제·도감·색놀이 배제,36칸 부담|
|수역/깊은물|Terraria bobber, Necesse 육지 거리|자동 첫 물 선택은 깊은 조건 도달 불가|동일 방향의 연속 수면 안 최대5unit 지점, signed거리≤-2.25와 실제 DeepWater,cast 수직깊이가 아님을 기록|세계 생성 변경·가상 cave수심·수영 배제|
|밤/희귀|DST 시간 lure, Terraria 시간 power|밤 위험에 낚시 보상 차이 없음|Night에 내륙/습지 eel 낮은 가중치,기본식은 모든 시간 가능|night-only 핵심재·meal gate·강제 희귀 농사 배제|
|Fish 모델/ingredient|Valheim 공용 Raw, DST 음식값, Core Keeper stack 분화|정확 ID 재료만 지원|B:4species 보유→Hand 손질1/2raw, 공용 food4종 그대로|A의species별 기존요리 복제/C의전부즉시raw1/범용group언어 배제|
|음식/Recipe|Valheim 공용조리, Grounded 조리/건조, Core Keeper 조리 차별|기존 Meal 이미3개|손질4recipe+고등어 소분식1recipe,새Meal0|어종마다Cooked/Stew 복제·새buff 배제|
|Bait/Rod|DST선택barehook와시간lure, Necesse/Terraria 장비power, Valheim필수bait|3초입질로 시간단축가치 작고 rod변수data 없음|bait0/rod0;현재rod로모든catch가능|slot/UI/새worm/powerframework·필수상위rod배제|
|비어류/비음식|Terraria상자·Specular포션, Necesse포션, Valheimbonus, CoreKeeper광물|낚시로광물/치료진행대체위험|이번Fish only,기존음식연결|junk/광물/RNG장비/어색한Gland 배제|
|Station/skill|Valheim손질, CoreKeeperpot/skill, Necesse도구|전문station수요없음|Hand4·Campfire1,새Station/Skill0|가공station5개미만·전용tree·XP배제|

## 확정할 구현 계약

Species는 모두 새 silhouette, stack50,기존 raw_fish ID/stack 보존. 생식은 작은 영양만 제공하고 Meal/HP/Stamina 효과없음.

|Catch|조건|가중치|잡는 수량|생식H/T|Hand 손질|별도 용도|
|---|---|---:|---:|---|---|---|
|작은 민물고기(raw_fish)|내륙·습지·동굴 fallback|내륙8/습지6/동굴1|1|6/0|기존재 그대로|기존4Fish음식|
|붕어(crucian_carp)|Wetlands 수면|4|1|6/6|1→raw1|물가 소량수분 vs 조리재 선택|
|농어(perch)|내륙/습지 깊은수면|2|1|6/0|1→raw2|안쪽수면의 높은 손질수율|
|고등어(mackerel)|바다(CoastDistance<0)|1|1|6/0|1→raw1|Campfire 고등어1+Berry1/8초→생선 소분식2(H20/T2),일시효과0|
|장어(eel)|내륙/습지 Night|.5|1|8/0|1→raw2|밤 위험의 선택적 수율,core gate없음|

매칭 entry의 가중치를 정규화. 예:일반 내륙 얕은낮100%raw,깊은낮raw80/perch20%;습지 얕은낮raw60/crucian40%;습지깊은밤raw6/crucian4/perch2/eel.5→48/32/16/4%. 바다100%mackerel,동굴100%raw(실제공급유지). 전체 분포를 한 확률로 잘못 합산하지 않는다.

Runtime table은 waterflags/daycondition/min shore inset/output/count/weight만 가진다. cast 시점 Host가 context/night와 결과를 확정해full bag retry에 reroll하지 않는다. Use의시간·alive·모달·이동·도구를 즉시재검사하고 성공phase는inventory 원자거래뒤한번만변경한다. Save/Load는pending을버린다.

PlayerFeedback의짧은아이콘+현지화이름+수량을사용한다.네트워크에catch index/count가추가되므로Protocol16,Save11.새RPC로Client가보상을제출하지않는다.실제보상은기존Host inventory.본문의향후검증결과를조사당시기록과구분해아래추가한다.

## 1P/4P 경제 · 구현 전 산술

기존 최소3.6초/성공1:16.67catch/min.반응.3초가정3.9초:15.38/min.실패·여행·재료조달·조리제외 상한이며 자연 플레이 실측 아님.새 rod/bait 비용0.낚싯대 비용1P Wood4/Fiber3,4P16/12(+공유WBI6Wood는처음만)이다.

손질 후 raw 기대량/catch:내륙얕은낮1,깊은낮1.2,얕은밤9/8.5=1.0588,깊은밤13/10.5=1.2381;습지얕은낮1,깊은낮14/12=1.1667,얕은밤11/10.5=1.0476,깊은밤15/12.5=1.2;바다1·동굴1.0.최대 평균향상23.81%,4P동시생산4배·시간각자필요.

Hunger유지4.8/min:CookedFish26→.1846raw/min(1P),.7385(4P).기본입질3.9초가정낚시.72초/min/2.88person秒,Campfire5초/개로.923/3.692station초/min.30분144H는6CookedFish(156H)와6raw,4P各6=24raw;최소누적23.4/93.6person秒+조리30/120초,실제낚시복귀/식재료찾기/저장·fail제외.

TrailBroth150초유지→.4raw/.4Herb/.8Berry/min/person,4P1.6/1.6/3.2.30분12raw/Herb12/Berry24,4P48/48/96.낚시46.8/187.2person秒,Pot120/480초。Food2-B효과·소비변경없음.

FishStew1인42/26에raw1+기존재료, Mixed는raw+meat양쪽필요.각정확비용과새Recipe완료자료는구현후data audit로재출력한다.바다소분식은catch1+Berry1→40H/4T총량,일반공용raw조리26H와Berry별도8H/3T보다6H/1T높지만8초처리·두portion수령,바다여행과Berry가필요.장기보존시간시스템이없어보존buff라고주장하지않는다.

기존낚시는이미공짜시간식량원이다.이번은기본공급을줄이지않고최대평균23.81%의장소보너스.사냥CookedMeat35/개,Boar기본2Meat=70H와Leather/XP/Gold·기타생물material은낚시로공급불가.따라서사냥의제작보상은보존하지만실제분당사냥성공/낚시편의우열·장시간farm은미실측이다.

## 구현·검증 결과

완료 판정 **A(기능·구조 기준)**. 실제 재미/터치 손맛/장기 경제 합격은 사용자 검수에 남긴다. Item132/Recipe109/Save11/Protocol16. 신규 Item5=물고기4+소분식1,Recipe5=Hand손질4+Campfire1. 새Station/Bait/Rod/Skill/Meal효과/비어류0. 다음 예정은2-E Exploration / Utility이며 착수하지 않았다.

### 실제 변경과 감사 중 추가 발견

- 위 확정 table대로 구현했다. `FishingLootTable.cs`의정의/순수Roll과`PlayerFishing`의상태·권위를분리했다. 실제DayPhase.Night만밤으로읽고cast시확정한다.최초수면이후.5unit샘플에연속된물의가장먼지점으로선택하며월드생성은변경하지않았다.
- generation1은Inland/기존수면fallback,2/3는실제Wetlands/Ocean/거리조건.3seed9472026/17491/-81027의실제map에서접근가능한습지·깊은수면·바다와동굴poolfallback을검사했다.실제기하거리이며수직depth라고주장하지않는다.
- 생선손질4개가같은raw_fish를출력하자기존ProgressionEconomy의'결과물마다한Recipe'가정을발견했다.처음실행은`Ambiguous progression recipe`로초기화실패했다.정본Recipe에`OptionalConversion`metadata를추가해자연획득재의선택손질을필수진행제조경로에서제외했다.이표시는ingredientgroup언어/recipe우회가아니며일반플레이는기존원자교환을쓴다.Validator는선택변환에progressionroot수량을허용하지않는다.기존127Item/104Recipe를잘라낸catalog와현재catalog의1P/4P·II/III예산이동일함을자동검사했다.
- catalog기존순서를보존하고5개Item을뒤에추가했다.기존Fish4recipe·원료stack·Bone/Chitin/Medicine/Skill/Meal수치변경0.두게임씬에새카드5개씩추가했고기존오브젝트제거0,씬별4380줄추가(직렬화UI)다.기존카드/시설/자원수작업배치를재생성하지않았다.
- 기존scene저장시공백/직렬화순서차이는정리했다.새hand카드는어종아이콘/현지화손질명/원료/결과수율을제작전에보인다.영어·한국어에서'결과:생선×2'를직접확인했다.결과물상세는기존영양을추가표시한다.별도결과모달은없다.
- `PlayerFeedback`의catch아이콘은40px,이름/수량은기존짧은Text이고2초후사라진다.언어변경에반응한다.수역/안쪽수면/밤안내는cast때짧게표시한다.기본line/bobber/bite는유지한다.
- 모든새아트는Pillow원본160×128,외부asset0.붕어의넓은몸/농어의가시등/고등어꼬리·유선형/장어곡선이같은색칠실루엣이되지않게했다.

### 실제 table / 가방 부담

상세산술과조건별확률·예상cast수는[catch-economy.json](catch-economy.json),전체음식/재료/시간은[food-data.json](food-data.json)이다.기대값은장기평균이지개별보장량이아니다.

|수면|시간|raw|붕어|농어|장어|손질후raw/catch|
|---|---|---:|---:|---:|---:|---:|
|내륙 얕음|낮/저녁/새벽|100%|0|0|0|1|
|내륙 안쪽|낮/저녁/새벽|80%|0|20%|0|1.2|
|내륙 얕음|밤|94.118%|0|0|5.882%|1.05882|
|내륙 안쪽|밤|76.190%|0|19.048%|4.762%|1.23810|
|습지 얕음|낮/저녁/새벽|60%|40%|0|0|1|
|습지 안쪽|낮/저녁/새벽|50%|33.333%|16.667%|0|1.16667|
|습지 얕음|밤|57.143%|38.095%|0|4.762%|1.04762|
|습지 안쪽|밤|48%|32%|16%|4%|1.2|

바다는고등어100%,동굴공용raw100%.낮/저녁/새벽구분을추가어종gate로만들지않았다.밤전용장어도raw2를주는선택보너스다.예를들어습지안쪽밤에서장어평균25cast(3.9초가정97.5초),내륙안쪽낮의농어평균5cast(19.5초).특정밤한번의획득을보장하지않는다.희귀물고기를얻지않아도기존모든Fish음식과진행이가능하다.

최대로한가방에공용raw+4species+소분식의6종stack(36칸중16.67%,Rod별도1)이될수있다.한습지안쪽밤의실제catch는4종,바다는1종이다.Hand손질은추가Station없이종stack을공용1stack으로합치고,농어/장어는2수율을준다.기존raw를species로역변환하는recipe나품질별stack은없다.소분식2개가결과일때공간검사도기존TryExchange/processor를쓴다.

### 1P/4P 공급·소비

|항목|1P|4P|조건/의미|
|---|---:|---:|---|
|Rod 제작|Wood4/Fiber3|Wood16/Fiber12|기존WBI;공유WBI최초Wood6별도|
|Bait/Rod upgrade 반복비|0|0|추가0|
|성공catch/min|15.38|61.54|3.9초,4P는동시낚시;여행/실패제외|
|가장높은평균raw/min|19.05|76.19|내륙안쪽밤,선택손질포함;손질입력시간제외|
|30분 H144유지 CookedFish|6raw/6요리|24raw/24요리|H156공급,최소가열30/120station초|
|동일기간 기본catch노동|23.4초|93.6person秒|3.9초×6/24,상한생산에기댄산술하한|
|30분 TrailBroth 연속150초|raw12/Herb12/Berry24|raw48/Herb48/Berry96|12/48회·Pot120/480초;Meal150초유지계산|
|FishStew 1회/인|raw1/Mushroom1/Berry2|raw4/Mushroom4/Berry8|8초/회,H42/T26|
|MixedStew 1회/인|Meat1/raw1/Mushroom1/Berry2|Meat4/raw4/Mushroom4/Berry8|10초/회,H65/T18/즉시HP8|
|소분식 1batch/인|고등어1/Berry1→2|고등어4/Berry4→8|8초/회,H40/T4총량/인·일시효과0|

**현재MP자연H/T감소는기존대로꺼져있다.**따라서4P H유지는SP와동일소모를가정한비교예산이며현재멀티의실제자연유지필요량은0이다.선택적Meal/음식소비는발생한다.이를실제4인장기소모실측으로표시하지않는다.

물고기영양만으로Hunger를쉽게유지할수있는것은기존3.6초Raw1부터의경제다.이번은기본공급을삭감하지않고장소보너스최대23.81%를추가했다.바다소분식은추가Berry/왕복탐험을요하며고기/동물Material/XP/Gold를주지않는다.사냥kill/min을측정하지않았으므로낚시와사냥의실제시간효율이균형이라고단정하지않는다.실제장시간비교는사용자검수다.

### 자동 검사

최종**60 unique Passed=EditMode56+PlayMode4**,[test-summary.json](test-summary.json).현재각testfullname의최신결과로집계했다.무관한대형회귀를추가로반복하지않았다.

|실행|결과|해석|
|---|---|---|
|edit-fishing.xml|39/47,8실패|선택손질metadata를갱신한liveasset이canonical QA복사에반영되기전snapshot에서모두동일Ambiguous progression원인.원본XML유지|
|edit-correction.xml|19/19|정상asset동기화후Fishing7개+금속진행검사.3seed실제수역과1P/4P진행예산동일성포함|
|edit-final.xml|15/15|남은4개seed/기존source실패사례와Stability·현지화/직렬화확인|
|play-fishing.xml|4/4|실제SP종/취소/가방/손질/조리/Save,Host보상/reHost,기존낚시·기존Host처리/취소회귀|
|ProjectDataValidator|errors0|Item132/Recipe109/단일Fishingtable/조건fallback/비용·영양·현지화/선택변환root검사|
|validate_visual_project.py|PASS,1332 GUID|소스구조·Landscape·sprite참조·표시문구;최종meta수는최종검사기록을따름|

초기Play진입의Ambiguous예외후에는정상데이터로재진입했다.QA Select helper도'빈Hotbar칸이항상있다'는가정때문에첫live흐름의Save직전멈췄다.전체Hotbar가차면선택칸과교환하도록**Editor검수도구만**수정하고최종SP흐름을재실행해완료했다.실패를성공으로덮지않고[live-mcp.tsv](live-mcp.tsv)에순서대로남겼다.최종consoleerror/warning0.

### Unity MCP 실제 검수 · 1920×1080

격리`Codex_QA_Fishing2D`,원본사용자저장2개의SHA256/mtime_ns전후동일([before](user-save-before.json)/[after](user-save-after.json)).fixture의순간이동/재료지급/시간설정/RNGseed선택을사용했다.**실제catchtable/실제geometry/실제elapsed입질과inventory거래**를실행했으며자연농사/진행시간측정은아니다.

- WBI에서원래비용의Rod를실제제작카드로제작.육지cast거절/line·bobber·bite·성공·반응실패확인.
- 실제내륙/습지/바다위치에cast,raw/붕어/농어/고등어/장어모두1개지급확인.장어는Night설정후기존table에서seed만선택했다.전용확률강제/보상직접지급은하지않았다.밤외/얕은물제외는순수table검사로확인했다.
- 물가측정예:습지land(748.70,1537)→물(743.70,1537),shore inset3/DeepTrue.배나수영없이기존5unit로충족했다.정확한다른좌표는TSV.
- 최종5cast 실측cast→bite3.0026~3.0210초,회수완료까지3.7374~3.9132초(화면캡처·fixture반응포함).예산3.9초는가정으로별도표시했다.대기시간을부당하게늘리거나줄이지않았다.
- miss는무보상,이동/도구변경/모달/사망취소,가방full의pending고정과공간확보후1회지급,동일frame중복reel거절.
- 실제Hand4종손질,Campfire소분식/기존CookedFish,Pot기존FishStew/TrailBroth의예약/대기/Take와수량확인.소분식은H/T만적용·Meal/HP없음.기존rawstack과새species2개씩이Save11/Load에서정확히복원되고pending낚시는Idle.
- 실제LocalHost:owner요청1cast→2reel,동일/stale요청과회수중새요청모두추가보상0,서버eel1/replicatedCatchIndex·Count확인.대기중HostSave→종료→reHost에서eel1만보존·Rod선택/Idle복원.
- 작은catch아이콘/이름/수량,EN/KO손질수율표시를GameView에서확인했다.자동Play와실제MCP로그는별개로보존했다.
- 종료시HostLeave·PlayStop·MainMenu·격리profileOFF로복귀.

대표이미지: [해안고등어](captures/mackerel-caught.png),[밤장어](captures/eel-caught.png),[손질수율KO](captures/prepare-yield-ko.png),[손질수율EN](captures/prepare-yield-en.png),[Host](captures/host-caught.png),[reHost](captures/rehost.png).16장모두1920×1080이다.

### Network/Save와 미검증

Host가context/time/table roll/성공/보상과inventory를결정한다.Client는ownerPrimary요청과Activity표현/owner안내만수행한다.ActivityState의새CatchIndex/CatchCount는NGO직렬화왕복검사,Protocol16을적용했다.대기/입질/Caught는transient이며Save11필드추가없음.기존raw_fish와요리recipeID는보존했다.

원격별도Client2/4인Relay,Android/터치/성능/발열,장시간자연낚시·사냥비교·실제여행시간·희귀획득편차·재미/손맛은미검증이다.실제수중탐험/산소/잠수/보트/낚시Skill은구현하지않았다.현재얕은물에서도안쪽표시가없는경우가있으며실제DeepWater와signed거리둘다만족해야한다.동굴깊이를이진pool값으로꾸며내지않았다.
