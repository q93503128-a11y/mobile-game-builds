# Food / Meal Identity Expansion 2-B

시작:2026-10-21 KST08:49:38(UTC10-20 23:49:38), remote fetch 후 main/HEAD=origin/main=5c3606002280fc4efca97052d9886ba5602bbcdd, clean. 아래 외부 조사·정본/데이터 감사를 마친 뒤 runtime 구현을 시작한다. 기존 Item123/Recipe100/Save11/Protocol14다. 과거 Review는 당시 기록으로 보존한다.

## 구현 전 공개 자료 비교

공식 개발사 노트와 연결 Wiki를 우선했다. Grounded Wiki는 직접403, Valheim Fandom은402/robots로 검색 색인에 공개된 본문을 사용했다. Necesse 일반 Food 페이지는 timeout이었으나 Getting Started/Tropical Stew 본문은 직접 읽었다. Grounded1/2·DST/Shipwrecked를 혼합하지 않았고 SEO 복제 Wiki/Reddit의 미확인 중첩 주장은 제외했다.

|게임|해당 시스템|실제 사례|장점|Lowfire와 차이|Lowfire 적용 여부|이유|
|---|---|---|---|---|---|---|
|Grounded1|기본식/Meal/Medicine|[Food & Water](https://grounded.wiki.gg/wiki/Food_%26_Water_%28Grounded%29)의 Roast는 단순 회복, [Miteloaf](https://grounded.wiki.gg/wiki/Miteloaf)는Mite Fuzz4+Mushroom2+Aphid Jerky1/Cookery·공격 Stamina 비용 감소. [공식1.3](https://grounded.obsidian.net/news/grounded/update-1-3)의 Bandage/Smoothie/Meal은공용4초,Snack은제외|가열식·준비식·약품의 시간/효과 역할 구분|Lowfire에는공격Stamina·Jerky·Cookery 진행이 없음|기본식 무buff,Meal만 작은 일시효과·기존4초 사용|신규가공/Station·공격시스템을 음식 때문에 늘리지 않음. Grounded 전체Meal이엄격히1슬롯이라는 미확인 주장은 하지 않음|
|Core Keeper|조합 요리/임시·영구 성장|[Cooking](https://corekeeper.atma.gg/en/Cooking)의2재료/10초 Pot,같은 수치는 높은 재료값만 사용.HeartBerry는임시MaxHP,BlueCaveGuppy는다른효과로조합.[Max health](https://corekeeper.atma.gg/en/Max_health)의GlazedHeartBerry는마지막같은효과가교체되며별도희귀음식의영구증가와구분|재료 특성이 결과 역할에 연결|Lowfire는고정Recipe와영구4Node가있고다중효과/요리Skill은범위밖|고정3선택·같은종류재섭취도비중첩/시간reset|조합폭발·희귀도·영구MaxHP·요리Skill추가배제. 재료결합규칙을서로다른음식전체중첩규칙으로일반화하지않음|
|DST|일반영양/기능식·동종비중첩|[Warly/DST](https://dontstarve.wiki.gg/wiki/Warly/Don%27t_Starve_Together)의11개전용Pot음식,[Chili Flakes](https://dontstarve.wiki.gg/wiki/Chili_Flakes)의Pepper3→2 seasoning,공격+20%/4분·같은효과재섭취는시간만갱신.[Volt Goat Chaud-Froid](https://dontstarve.fandom.com/wiki/Volt_Goat_Chaud-Froid)는Horn+Sweetener2의5분전기공격|영양과특정상황준비역할이명확|캐릭터전용요리/조미료/전기속성은Lowfire에없음|서로다른목적의준비식·단일효과원칙|Warly직업·추가Station·전투배수·복수조미료중첩은제외.동종시간갱신만참고|
|Valheim|활동별식사/슬롯상한|[Food](https://valheim.fandom.com/wiki/Food)는서로다른3음식슬롯.[Carrot soup](https://valheim.fandom.com/wiki/Carrot_soup)는Carrot3+Mushroom1/Cauldron1,HP15/Stamina45;DeerStew는CookedDeer1+Blueberry1+Carrot1,HP45/Stamina15|같은식사도탐험/위험대응선택이바뀜|Lowfire는기본HP100·Stamina100·Skill이있음|활동별3역할만채택하고Meal슬롯은1개|음식기반대규모최대치/3슬롯가산·25분유지는Skill정체성과480초cycle을침범|
|Necesse|식사1슬롯·품질/요리대|[Getting Started](https://necessewiki.com/Guide:Getting_Started)는Raw/Fine/Gourmet,새foodbuff가기존것을즉시교체한다고명시.RoastedBeef는공격,Bread는MaxHP.[Tropical Stew](https://necessewiki.com/Tropical_Stew)는Coconut2+Eggplant2+Chili1+Corn1/CookingPot,영양40·16분간공격/MaxHP/Armor|식사선택과교체가단순|Lowfire는다중능력식·농산물·장기buff필요없음|정확히1Meal·교체표시·기존Pot활용|Raw전체buff·다중능력·긴16분·농사/새재료는제외|

## 현재 전체 Consumable inventory (변경 전)

정본 Data asset을 GUID 연결해 추출했다. station 숫자는0 Hand/2 Campfire/5 CookingPot. 없는회복필드는0이며효과시간은의약품만8초다. 원본은before-consumables.json. RawMeat/Fish는현재HP피해/식중독이없고,RawWater만60초자연회복차단·갈증소모×1.5의불이익이있다. 존재하지않는RawMeat위험을만들지않는다.

|ID|Hunger|Thirst|HP즉시|Stamina즉시|공용간격초|Recipe/Station/처리초|
|---|---:|---:|---:|---:|---:|---|
|berry|8.0|3.0|0|0|0|자연채집/처치/낚시·물 채우기|
|bone_broth|24.0|12.0|18.0|0.0|4.0|bone×2+mushroom×1+raw_meat×1 / 5 / 8.0|
|clean_water_cup|0.0|38.0|0|0|0|raw_water_cup×1 / 2 / 5.0|
|cooked_fish|26.0|0.0|0|0|0|raw_fish×1 / 2 / 5.0|
|cooked_meat|35.0|0.0|0|0|0|raw_meat×1 / 2 / 5.0|
|field_dressing|0.0|0.0|0.0|0.0|4.0|plant_fiber×2+medicinal_herb×1 / 0 / 0.0|
|fish_stew|46.0|15.0|0|0|0|raw_fish×1+mushroom×1+berry×2 / 5 / 8.0|
|healing_salve|0.0|0.0|30.0|0.0|4.0|medicinal_gland×1+medicinal_herb×1+plant_fiber×2 / 0 / 0.0|
|meat_stew|48.0|12.0|0|0|0|raw_meat×1+mushroom×1+berry×2 / 5 / 8.0|
|mixed_stew|65.0|18.0|8.0|0.0|4.0|raw_meat×1+raw_fish×1+mushroom×1+berry×2 / 5 / 10.0|
|mushroom|5.0|0.0|0|0|0|자연채집/처치/낚시·물 채우기|
|mushroom_stew|35.0|10.0|0|0|0|mushroom×2+berry×2 / 5 / 7.0|
|raw_fish|6.0|0.0|0|0|0|자연채집/처치/낚시·물 채우기|
|raw_meat|8.0|0.0|0|0|0|자연채집/처치/낚시·물 채우기|
|raw_water_cup|0.0|15.0|0|0|0|자연채집/처치/낚시·물 채우기|
|restorative_broth|22.0|10.0|0.0|35.0|4.0|medicinal_gland×1+mushroom×1+berry×2 / 5 / 8.0|
|roasted_mushroom|16.0|0.0|0|0|0|mushroom×1 / 2 / 5.0|
|trail_ration|25.0|0.0|0.0|20.0|4.0|raw_meat×1+mushroom×1+berry×2 / 2 / 7.0|

## 핵심 결정 (구현 전)

|결정|외부 사례→Lowfire 문제|채택 원칙|배제 대안/이유|
|---|---|---|---|
|품목3개|Valheim활동별선택·DST상황식→기존3회복음식이즉시에집중|Hunter/Forager/Trail 각1효과,새raw0|음식10개·재료조합만다른복제·복수효과제외|
|단일Meal|Necesse교체·DST동종비중첩·Valheim3슬롯→buff합산폭증우려|한슬롯·새Meal교체·동일Meal150초로reset(시간가산없음)|여러buff동시유지·자동재섭취/자동연장제외|
|기간150초|외부4–25분은각게임tempo→Lowfirecycle480/낮240/보행4.2|150초는cycle31.25%/낮62.5%,정지없이630unit편도범위.전투/채집시간은더짧음|하루전체효과·영구성장·짧은8초약품과혼합제외|
|Hunter|ValheimHP/탐험식분리·Core임시MaxHP→Vitality rank10중복제한|Meat2+Bone1+Mushroom1+Berry1,Pot10초,H44/T12,MaxHP+8/150초·즉시치료0|Skill1rank10보다큰영구효과·무료8HPheal·비율heal제외;종료/교체시현재HP만안전clamp|
|Forager|Grounded준비식·Core임시효과→기존안전회복과의약역할보존|Herb1+Mushroom2+Berry1,Pot10초,H30/T18,조건부자연회복+0.15HP/s/150초|H/T70%·최근전투8초·RawWater차단우회금지;Dressing20/8초가속금지|
|Trail|ValheimStamina음식·Grounded공격Stamina→Recovery Skillrank15%보존|Fish1+Herb1+Berry2,Pot10초,H28/T22,Stamina회복+10%/150초|회복대기1.2초·Dash24·Sprint14/s변경없음;즉시Stamina35의Restorative와분리|
|일반Stew|DST영양/기능분기·Valheim식재료역할→Meat48/12와Fish46/15가비슷함|Mushroom35/18(식물/수분),Meat52/10(허기),Fish42/26(수분),Mixed65/18/HP8(비싼범용)|각Stew마다Status추가제외;기존비용/Raw/Campfire/Broth/Ration유지|
|Station|CorePot2재료·NecessePot·Grounded전문Cookery→현재Campfire6/Pot6|Campfire6/Pot9,새Station0|조리시설확장·농축산중간재는실제공급수요없음|
|Save/권위|기존Medicine효과는transient→Meal도영구schema불필요|Save11유지,Load/reHost/죽음시효과삭제.초과HP는영구Max까지만snapshot.서버상태필드추가로Protocol15|잔여효과영구복원·재접속무한연장·MaxHP초과invalidSave제외|

현재MP PlayerSurvival.Update는꺼져있어기존자연HP회복호출도없다.새Forager가Host에서도동일한조건으로작동하도록기존자연회복함수를분리해서버tick에도연결한다.Hunger/Thirst소모·Stamina engine은이번에바꾸지않는다.새regen시스템을별도로만들지않는다.실제Host검수에서이차이를명시한다.

Herb는의약2종+Forager/Trail의4sink, Bone은Arrow/Trap/Broth+Hunter의4sink가된다.150초1Meal은8분cycle당연속유지시3.2회이나유지강제/자동연장은없다.4인소비와의약준비합계·낚시성공시간하한은최종경제절에실제Recipe로계산한다.분당자연획득실측으로보고하지않는다.

## 최종 구현

Item126 / Recipe103 / Save11 / Protocol15. Item3·Recipe3·새raw0·새Station0,Campfire6/Pot9다. 기존일반Stew의3개H/T값만조정했고비용·기본식·Mixed·Bone/Restorative/Ration·의약품·식중독/부패·낚시·생태수율은바꾸지않았다.전체변경전/후는before-consumables.json / after-consumables.json.

|신규Meal|Pot비용·처리|Hunger/Thirst|단일효과150초|없는효과|
|---|---|---|---|---|
|Hunter's Stew / 사냥꾼 스튜|RawMeat2+Bone1+Mushroom1+Berry1 /10초|44/12|MaxHP+8|현재HP즉시회복·재생·Stamina추가없음|
|Forager Soup / 채집꾼 수프|Herb1+Mushroom2+Berry1 /10초|30/18|조건부자연HP회복+0.15/s|전투중치료·Dressing가속·Stamina추가없음|
|Trail Broth / 원정 국물|RawFish1+Herb1+Berry2 /10초|28/22|Stamina회복+10%|즉시Stamina·최대치·비용감소없음|

동시Meal1개.다른Meal은이전효과를제거하고교체,같은Meal은150초로reset(시간가산없음).일반음식/의약은기존Meal을유지한다.공용4초·alive·Running·가방실제소비후효과라는기존경계를재사용한다.H/T가가득차도준비를위한Meal섭취는허용한다.자동재섭취/자동연장/항상켜지는패시브기능은없다.

일반Stew:식물기반Mushroom35/18,허기Meat52/10,수분Fish42/26,비싼범용Mixed65/18/HP8.별도Status없음.BoneBroth18HP즉시는MaxHP만올리는Hunter와다르고Salve30보다낮다.Restorative35Stamina즉시는10%회복속도Trail과다르다.TrailRation은Campfire H25/Stamina20간편식,Meal효과없음.Dressing20HP/8초·Salve30HP즉시는위기치료,Forager는전투8초후·H/T70%·RawWater차단조건을지키는느린보조다.

### Skill 경계와 수치

150초는480초cycle31.25%·낮240초62.5%다.보행4.2로정지없이최대630unit이며전투/채집/경로시간은별도다.Hunter8<영구Vitality1rank10;Forager는기본0.5→0.65HP/s(유효시간전체150초라는비현실적상한에서도추가22.5HP)이며Dressing2.5HP/s와구분한다.Trail10%<Recovery1rank15%,대기1.2초·Dash24·Sprint14/s·MaxStamina100을유지한다.RecoverySkill/Armor와가산하므로최대Skill45%+Vest15%+Meal10%라면16×1.70=27.2/s,Meal없이는25.6/s다.곱셈중첩없음.

실제Host에서Vitality1rank:110→Meal118(현재90그대로)→만료110을확인했다.최대118까지채운상태의snapshot은110,Rank1유지였다.SkillPoint/영구MaxHP/성장XP를음식으로지급하지않는다. skill-interop.json에관측값을남겼다.

## 1인/4인 경제

이표는준비예산/완벽한입력의이론하한이며장시간자연파밍분당소비율·실제4인플레이측정이아니다.Meal을항상유지하거나3개를연달아먹으라는권장이아니다.일반식은Herb/Bone없어도사용가능하다.

|선택준비|Meat|Fish|Berry|Mushroom|Bone|Herb|Gland|
|---|---:|---:|---:|---:|---:|---:|---:|
|1인 Hunter1|2|0|1|1|1|0|0|
|1인 Forager1|0|0|1|2|0|1|0|
|1인 Trail1|0|1|2|0|0|1|0|
|1인 각1개를교체용으로보유|2|1|4|3|1|2|0|
|4인 각3종1개보유|8|4|16|12|4|8|0|
|1인8분동안Hunter만4개소비가정|8|0|4|4|4|0|0|
|1인8분동안Forager만4개소비가정|0|0|4|8|0|4|0|
|1인8분동안Trail만4개소비가정|0|4|8|0|0|4|0|
|4인8분동안Hunter만각4개|32|0|16|16|16|0|0|
|4인8분동안Forager만각4개|0|0|16|32|0|16|0|
|4인8분동안Trail만각4개|0|16|32|0|0|16|0|

8분/150초=3.2회.0/150/300/450초에4번섭취하면마지막효과120초가다음cycle에남는다.시간누적을허용한것은아니다.한cycle기본H/T소모는38.4/57.6이므로항상Meal4번을먹으면영양측면에서는과투자다.일반식·물+상황별Meal이라는선택을유지한다.

Herb공급은2/노드·Forest/Wetlands·회복240초/같은채집점반경6제외480초그대로다.2-A의3seed1,875청크표본은Forest324노드/422중심청크,Wetlands158/142;각약1.54/2.23Herb/중심청크이며경계·확률·지형검사차이로엄밀한전체분포나분당획득량은아니다.이번WorldGeneration/NaturalSource/수율diff없음을확인했고과거표본은재측정한것처럼보고하지않는다.

기존1인Dressing8+Salve4는Herb12/Fiber24/Gland4.새Forager또는Trail1회만보태면Herb13(+8.33%),4인은48→52(추가4Herb=2노드).3종각1개보유시Herb14/56(+16.67%);4분이아닌8분내내Forager/Trail유지시Herb16/64(+33.33%)다.기존6/24Herb노드에서8/32개로늘어나는예산이다.생물에Herb를붙이거나수율을올리지않았다.Gland는새Meal수요0으로의약/Restorative경쟁이더늘지않는다.

Berry3/관목,버섯2/패치,Herb2/노드이므로개인교체용3Meal은Berry관목2/버섯패치2/Herb노드1이필요하다(반올림잔량보존).버섯은Forest와Caverns,열매는Grassland/Forest로기존분포를쓴다.Meat는Chicken1/Boar2등기존자연스러운동물보상이며새Drop없음.

Hunter4개의Bone4는BoneArrow24발(2batch)또는BoneTrap2개또는BoneBroth2개와경쟁한다.4인Bone16은Arrow96발/Trap8/Broth8의대체비용이다.기존Skeleton/Wolf/Goat등1–2Bone공급을사용하며이를강제드랍보정으로해결하지않는다.

Trail의Fish1/150초는인당분당0.4fish,4인1.6fish다.현재낚시1성공→Fish1,첫입질3초/성공회수0.6초이므로완벽한최초입력기준1인8분4fish≥14.4초,4인16fish≥57.6초다.3종각1개준비에는개인1fish/4인4fish≥3.6/14.4초.이동·수면찾기·실패·가방·입력지연은포함하지않는다.Fish는필수식/전투회복전체에추가하지않고선택탐험Meal1종에만쓰므로새어종/수율조정없이유지했다.

## Authority / Save / 네트워크

Host가실제음식소비·H/T·즉시HP/Stamina·Meal시작/교체/시간·MaxHP/회복집계를결정한다.기존ownerPrimaryRPC의단조증가request를재사용해같은request를4초이후재전송해도거부한다.Client는MealItem/MealUntil과최종MaxHP를표시한다.새2필드로Protocol15가필요하며14peer와혼용하지않는다.

MP의Survival.Update가꺼져있던기존구조에서자연HP회복호출도없었다.Forager역할을위해기존AdvanceNaturalRecovery를서버tick에공유해기본0.5/s와조건부0.65/s를같이제공한다.별도HPregen시스템은만들지않았다.MPH/T소모·Starvation을새로켜지않으며SP소모는그대로다.RawWater불이익의임시타이머만Model.AdvanceEffects로분리해서버tick에서도진행한다.이를하지않으면새Host자연회복이영구차단될수있음을마감검토에서발견했다.기존SP의penalty구간별갈증계산순서와60초정책은보존한다.

Save11유지.새3item/recipe는기존Catalog codec,Meal은Medicine처럼transient다.죽음/Load/reHost에서제거한다.임시MaxHP초과분은base+Vitality까지만snapshot하며liveHP는저장때바꾸지않는다.기본108HP상태→Save100/Load100,호스트도동일.영구MaxHP110인경우118→snapshot110.소비된stack은되돌리지않고잔여Meal을새로지급/연장하지않는다.기존v4/v10migration검사와실제Save11/HostreHost통과.사용자UserData2개의hash/mtime_ns는before/after동일이다.

## 검증 / 실제 화면

- 자동:최종고유Edit70개통과.초기69개중67통과/2실패는새Bone4sink·Herb4sink를과거3/2로가정하던contract였다.현재수로수정해관련4개만재실행했다.마감RawWater타이머모델검사1개추가와관련Stability/Scene총17개통과.중복case는합계에서제외했다.
- 자동Play5/5:실제Pot3종/기능·기존Salve/Restorative/Vest·Save10migration·Host/reHost.이후추가한HostWaterBlock단독검사는별도XML결과를따른다.전체무관회귀는반복하지않았다.
- 실제MCP1920×1080:Pot버튼투입/10초처리/Take/아이콘,기존음식과3Recipe,HPfull영양에서Meal사용,Max108현재60불변,151초명시적시간진행의만료clamp,실제8.1초기본회복4HP/Forager5HP,실제Stamina16/s→17.6/s,저영양/최근전투/RawWater차단,Meal교체·기존효과제거·HP/Stamina즉시음식과의구분,Save/Load/죽음PASS.
- 실제LocalHost:소비/교체/표시상태·중복request/4초이후stale request·Forager서버회복·만료·Save/reHost·소비량보존PASS.HostRawWater59+2초명시적진행으로차단해제·H/T무소모보존·Forager회복재개도PASS.실제150초전체를기다린만료검수로보고하지않는다.
- UI:작은Meal아이콘/이름/분:초만추가.0.2초확인/상태변경시갱신,메뉴에서숨김.Pot기존순차UI가효과설명을숨기던문제를빈상단의FoodPreview로직접수정했다.한/영제작전H/T·효과·150초/교체규칙,Inventory의상세조건·공용4초,기존HP/Needs/Stamina/HUD와비중첩을실제캡처로확인했다.초기즉시캡처는0.2초표시갱신전이어서활성후HUD를별도캡처했다.
- 동기화중1차컴파일은새MealModel파일보다참조파일이먼저import돼실패했다.동기화완료후force refresh/console오류0확인.일부Play직후MCP호출은transport빈응답이어서phase/scene을확인후재호출했다.게임실패로오인하거나성공처리하지않았다.
- source validator1296 uniqueGUID/Landscape/참조/표시문자PASS,DataValidator errors=[],신규12 EN/KO key의중복/빈값/format placeholder검사.증거는automated-results.json·각XML·live.tsv·batch-play.tsv와captures/다.

## 판정 / 미검증 / 후속

**A — 이번구현·관련자동/실제검수범위.** 기본식·일반Stew·단일준비Meal의역할을구분했고Medicine/영구Skill을대체하지않는다.원격실client/4인Relay·packetloss·Android터치/FPS·장시간자연낚시/파밍/소모율·재미/전투체감은미검증이다.1P/4P경제표와LocalHost를실제4인밸런스합격으로보고하지않는다.기존MPH/T자연소모가꺼져있는차이는이번에도남아있으며실제4인경제검수시고려해야한다.

다음예정은Content Expansion2-C:Creature Material Sidegrade(Bone/Chitin).이번에구현하지않았다.작업시간·공유사용률관측은USAGE_AND_TIMING.md,최종commit/push/remoteSHA/clean은완료응답에기록한다.
