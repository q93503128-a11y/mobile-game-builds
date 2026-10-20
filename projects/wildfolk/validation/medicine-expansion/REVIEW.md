# Content Expansion 2-A · Medicine / Medicinal Resource Foundation

시작 remote main: a77a1fa7363968f322222b1689aef7b064b52687. 2026-10-20 UTC09:30:52 fetch/HEAD 일치·main·clean을 확인했다. 아래 외부 조사·기존 구조 감사를 완료한 후 구현했다. 최종 검수는 뒤 절에 구분한다.

## 구현 전 외부 비교

개발사 공개 patch note와 연결된 게임 Wiki를 우선했다. Grounded/DST Wiki와 Valheim 일부 페이지의 직접 읽기는403/402였고 검색에 공개된 해당 Wiki 본문으로 확인했다. Grounded1과2의 Bandage 규칙을 섞지 않았다. 접근 실패한 Valheim 개발사 주소나 모순된 SEO 재게시물은 근거에서 제외했다. 아래는 공개 gameplay 원칙의 비교이며 코드·아트·수치를 복제하지 않는다.

|게임|해당 시스템|실제 사례|장점|Lowfire와 차이|Lowfire 적용 여부|이유|
|---|---|---|---|---|---|---|
|Grounded|초반 손제작/지속치료|[Fiber Bandage](https://grounded.wiki.gg/wiki/Fiber_Bandage): Plant Fiber+Sap, Hand 제작, Trickle Regen. [개발사1.3](https://grounded.obsidian.net/news/grounded/update-1-3)의 Consumables/Smoothies 절은 Bandage·Smoothie·Meal의 공용 사용 간격과 즉시치료 Smoothie의 구분을 설명|싸게 준비하는 회복과 급한 회복을 구분하며 품목 교대로 제한을 피하지 못함|Lowfire의 기존4초 제한·100HP·공격 피해를 기준으로 판단해야 함|붕대의 지속치료 역할/공용 제한 채택. 해당 게임의 총 회복량·연구 upgrade·Smoothie Station은 미채택|같은 HP 회복도 준비 비용과 회복 시간으로 구분 가능|
|Core Keeper|식물+생물재료와 즉시/지속|[Healing Potion](https://corekeeper.atma.gg/en/Healing_Potion): Alchemy Table의 Slime+Heart Berry 즉시 회복. [Heart Berry Soda](https://corekeeper.atma.gg/en/Heart_Berry_Soda)는 일정 시간에 걸친 회복이며 Vending Machine 공급|같은 HP 회복을 시간 특성과 공급 조건으로 구분|Lowfire에는 Merchant/Vending/포션병 경제가 없음|Herb+Gland를 연고에 결합하고 저가 붕대를 별도로 둠. 구매·병 중간재와 여러 Potion tier는 배제|초반 식생 경로와 생물 비용의 강화 경로가 필요|
|Core Keeper|전문대의 실재 규모|[Alchemy Table](https://corekeeper.atma.gg/en/Alchemy_Table)의 Usage에는 Healing/Mana/Enrage/Keen/Arcane/Pact와 여러 Bomb·Grenade가 존재|하나의 생산군을 묶을 실제 Recipe 수요가 전문대의 이유|Lowfire는 현재 의약품2개와 기존 Pot 음식 정도|새 Station0. 두 개의 현장 조합을 위해 시설·가공 단계를 추가하지 않음|실재 의약 Recipe는2개이며 별도 processing 없음|
|Don't Starve Together|생물재료 치료와 식생 수요|[Spider Gland/DST](https://dontstarve.wiki.gg/wiki/Spider_Gland/DST), [Healing Salve](https://dontstarve.wiki.gg/wiki/Healing_Salve): Gland가 직접 치료/Salve 재료, Salve는 Ash·Rock과 조합. Honey Poultice는 별도 식생 재료 경로를 사용|생물재료가 강화된 치료의 비용이며 서식지별 준비 동기가 있음|Lowfire의 Gland는 Leech/Bat에 제한되고 Fiber는 실제 식물 재료|Gland 없는 초반 경로와 Gland를 쓰는 즉시 경로 채택. Wool/Cloth/추가 약재·농축산·부패는 배제|Gland RNG에 묶이지 않는 기본 치료가 필요|
|Valheim|생물/식물·가공·공용 제한|[Minor healing mead](https://valheim.fandom.com/wiki/Minor_healing_mead): Honey/Berries/Dandelion으로 Mead Ketill에서 base를 만들고 Fermenter에서 병 단위 생산. 회복이 일정 시간 진행되며 다른 healing mead에도 공용 제한 적용|회복 시점과 사전 준비, 같은 계열 교대 사용의 한계를 명확히 함|Lowfire의 작은 현장 치료2종에는 fermentation·batch 시설 부담이 과함|같은 치료 계열 제한만 채택. 긴 제한·발효 시간·주조대와 Mead tier는 미채택|기존4초로도 교대 연타를 제한하며 시설2개는 과도|
|Necesse|즉시/재생과 전문대|[Health Potions](https://necessewiki.com/Health_Potions): Sunflower+Glass Bottle의 즉시 회복과 potion 공용 제한. [Health Regen Potion](https://necessewiki.com/Health_Regen_Potion): Firemone+Bottle, Alchemy Table의 지속 재생. [Potions](https://necessewiki.com/Potions)에는 이동/채굴/공격 등 여러 기능군|시간 역할과 생산 재료가 구분되고 전문대는 충분한 기능군을 가짐|Lowfire는 다수 장기 buff·병·상위 치료 tier가 이번 범위가 아님|즉시/지속 구분 채택. 긴 Potion Sickness·5분 buff·Alchemy Table·상위 약품은 배제|장기 buff·다품목 전문대는 이번 수요보다 큼|

## 기존 실제 구조와 결정

기존121 Item/99 Recipe, Save11/Protocol13. Salve는 Gland1+Fiber2→즉시25HP, Bone Broth18HP, Mixed Stew8HP, Restorative는Stamina35다. HP 지원 음식은 공용4초 입력 제한이다. 자연회복은 H/T70% 이상·전투 후8초·0.5HP/s이며 보존한다. Multiplayer의 PlayerSurvival Update는 원래 꺼져 있으므로 새 치료는 서버의 기존 tick에서 별도로 진행해야 한다.

|결정|외부 사례→현재 문제|채택|대안과 배제 이유|
|---|---|---|---|
|자원 수/지역|DST의 식생·생물 경로, Core Keeper의 Slime+Berry→Gland만으로는 초반 치료 준비가 제한됨|새 raw는 Medicinal Herb1개. Forest/Wetlands, 맨손 채집. Berry/Mushroom와 다른 실루엣|Grassland 대량 starter/추가 약재/Wool/Cloth는 지역 방문 이유와 가방 공간을 약화|
|지속 vs 즉시|Grounded Bandage/Smoothie와 Necesse Regen/Health→현재 Salve 하나로 시간 역할이 없음|Field Dressing: Fiber2+Herb1, Hand,20HP/8초. Salve: Gland1+Herb1+Fiber2, Hand,30HP 즉시|붕대의 즉시 burst·추가 상위 Salve·여러 buff는 역할 중복|
|중첩/사용 제한|Grounded 공용 제한·Valheim/Necesse 계열 제한→교대 연타와 회복 중첩 방지 필요|기존 공용4초를 사용하고 단일 지속치료가 남으면 같은 붕대 사용 거부·미소비|긴 타 게임 cooldown·중첩 회복·효과 중 자원 낭비 refresh는 미채택|
|자연회복/음식|DST의 음식·치료 분기와 Core Keeper 별도 Potion→기존 안전 회복과 Pot 기능 보존 필요|자연0.5HP/s, 붕대2.5HP/s, 연고30즉시. 음식 최대18HP로 연고보다 낮아 음식 수치 변경0|자연회복 제거·음식 전체 개편은 범위 밖|
|Station|Core Keeper/Necesse의 다양한 Alchemy 기능군→Lowfire 새 의약품은2개뿐|전문 Station0, Hand Recipe1개 추가|미래 Recipe 수를 세어 Medicine Table을 만드는 것은 역할 부족|
|저장|기존 공격 pose·소비 cooldown은 transient→8초 효과를 영구 schema에 넣을 필요 없음|적용된HP와 소비 stack만 저장, Load/reHost에서 남은 치료·공용 간격 초기화|저장 후 완전한 새 치료를 다시 받기·잔여 상태 중복 복원은 금지|

Herb 기본안:64unit chunk당6후보·확률0.6·간격6, yield2, 지역 목표1.0·회복240초, StarterSupply0. 적합한 chunk의 지형/간격 검사 전 기대7.2 Herb이며 실제 생성량은 별도 seed 표본으로 확인한다. 개인8 Dressing+4 Salve는Herb12/Fiber24/Gland4,4인은48/96/16이다. 기존 자연 Fiber2/source와 Leech1/Bat0.75 Gland 기대 공급을 사용한다. 이 산술을 실제 획득 시간으로 보고하지 않는다. 기존 source ID/rule index는 유지하고 새 규칙을 끝에 추가한다. 최종 density·실제 표본·호환과 판정은 검수 결과 절에 남긴다.

## 조사 사례의 구체적 범위

Grounded1 Bandage는 Plant Fiber3+Sap2의 손제작이며 공식1.3은 Bandage/Smoothie/Meal 공용4초와 Bandage의 burst+trickle 변경을 명시한다. Lowfire는 burst를 넣지 않은 순수 지속치료로 분리했다. Core Keeper Healing Potion은 Slime3+Heart Berry3의 즉시30%, Soda는15.4HP/s×10초다. Alchemy Table의 실제14 Recipe는 Healing/Mana/Enrage/Keen/Arcane/Pact6종과 Crude/Bomb/Large/Mouse/Dynamite/Deposit/Grenade/Stun8종으로 두 의약품 규모와 다르다. DST Salve는 Gland1+Ash2+Rock1→20HP이며 Honey Poultice의 별도 경로와 비교했다. Valheim Minor healing mead는 Honey10+Blueberry5+Raspberry10+Dandelion1→base→6병 발효,50HP/10초·healing mead 공용2분이다. Necesse Health Potion은 Sunflower3+Glass Bottle2의50+최대HP10% 즉시·30초 제한, Regen Potion은 Firemone5+Bottle1의0.5HP/s·5분이다. 이 수치는 비교용 기록이며 Lowfire에 복사하지 않았다. 근거 링크는 위 각 행이다.

## 최종 구현과 경제

Item123 / Recipe100 / Save11 / Protocol14. 새 Item2(Herb/Dressing), 새 Recipe1, 새 Station0. 새 약품 tier0. 기존 Salve ID·Camp Salve 보상·신규8종·Bone/Chitin/Gland·음식 수치는 유지했다.

Herb: resource rule12·마지막 NaturalSource, Forest/Wetlands 표면 ForestFloor 또는 WetGround, 맨손 HP12/수율2, stack50.64유닛 청크당6후보/60%/간격6, 회복240초/지역 목표1.0/cluster3/반경6의480초 harvest exclusion이다. Starter0이며 Rocky/Grassland/지하 공급0이다. 기존 materialization/recovery를 재사용하고 별도 Update/전역 scan을 추가하지 않았다. Generation1의 새 independent salt12317 fallback12개는 Forest 조건에만 배치한다. 새 source는 기존 순서 뒤라 기존 rule·c/g/e/r ID와 좌표를 유지한다.

Field Dressing은 Fiber2+Herb1, Hand,20HP/8초·즉시0. 같은 치료가 남으면 추가 사용 거절·미소비한다. Salve는 기존 Gland1+Fiber2에Herb1을 더하고25→30HP 즉시로 조정했다. 두 치료 경로의 공통 Herb는 치료 준비를 Forest/Wetlands 방문과 연결한다. 기존 공용4초를 사용해 상호 교대 우회를 막는다. 자연회복은 H/T≥70%·최근 전투8초 이후0.5HP/s 그대로다. 붕대2.5HP/s는 안전 자연회복보다5배 빠르고 위험 생물 피해5–18에 즉시 대응하는 Salve와 시간 역할이 다르다. 음식 최고18HP(Bone Broth)<Salve30이므로 음식 수치 조정0이다.

### 실제 생성 표본

[supply.csv](supply.csv)는3seed×625=1,875개 청크의 결정적 생성 표본이다. chunk center의 Biome 수와 실제 Herb 위치 Biome 수를 별도로 기록했다. 경계 청크가 있으므로 아래 비율은 전체 biome 확률의 엄밀한 추정이나 실제 분당 획득량이 아니다.

|seed|Forest nodes / Herb|Wetlands nodes / Herb|Grassland/Rocky nodes|
|---|---:|---:|---:|
|9472026|103 / 206|73 / 146|0 / 0|
|17491|122 / 244|51 / 102|0 / 0|
|-81027|99 / 198|34 / 68|0 / 0|

합계Forest324 nodes/422 center chunks, Wetlands158/142다. Forest 중심 청크 기준 약1.54Herb/chunk, Wetlands약2.23이며 서식 조건·확률·충돌로 인해 검증 전 상한 기대7.2보다 낮다. 지역에 실제로 들어가 찾는 시간이 필요하며 starter 대량 지급은 없다.

|준비 시나리오|Dressing|Salve|Herb|Fiber|Gland|총 이론 회복|
|---|---:|---:|---:|---:|---:|---:|
|1인|8|4|12|24|4|280HP|
|4인 각 동일 준비|32|16|48|96|16|1,120HP|

Herb는1P6 / 4P24개 노드, Fiber는Small Plant 수율2이므로12 /48개 노드다. 표본 비율로 Herb 노드 예산은 Forest 중심 약8 /31청크, Wetlands 중심 약6 /22청크에 해당한다. 이는 탐험 거리·플레이 시간 보장이 아니다. 연고만1P4 /4P16개를 만들면Herb4/16·Fiber8/32·Gland4/16, 붕대만8/32개면Herb8/32·Fiber16/64가 필요하다. Gland는Leech1 확정 또는Bat0.75이므로1P4마리 Leech/기대5.33마리 Bat,4P16/21.33마리다. Broth1개당 Gland1과 경쟁한다. Leech 지역 목표2·회복180초, Bat 목표3·회복180초와 공용 활성80 cap 때문에 이 산술을 보장 공급률로 간주하지 않는다. 전 종 활성 상한을 끊임없이 처치하는 비현실적 상한도6Leech+10Bat=13.5Gland/180초이며 실제는 habitat·확률·청크 quota·공간·전투가 더 제한한다.

현재 I~III ProgressionEconomy 기본+선택 합계의 Fiber6/24(개인 필수2/8+선택4/16), 직전 correction의 원거리 kit+선택 기지16/64와 별도로 의약 준비24/96을 더하면 기본 그래프+의약30/120, 원거리 kit·기지+의약40/160이다. 이는 Small Plant15/60 또는20/80개다. 신규 공급원을 억지로 생물에 붙이지 않으며 기존 Plant Fiber90초 회복·탄약 Feather/Leather/Bone 비용은 그대로다. Herb1회 수율2는 붕대2개=40HP 또는Salve2개=60HP에 해당하지만 후자는Gland2/Fiber4를 더 요구한다.4초 공용 간격,8초 비중첩,stack20/가방 공간, 유한한 탐험·지역 보충·480초 동일 위치 제외로 순간 무한 회복/제자리 무한 채집을 제한한다. 장시간 저장 축적 자체는 다른 소모품처럼 가능하며 실제 장시간4인 소비율은 미측정이다.

## Authority / Save

Host가 harvest/drop/pickup/consumption/HP/단일 TreatmentModel/공용 nextConsume을 결정한다. MP의 기존 PlayerSurvival Update가 꺼져 있어 새 치료만 기존 NetworkPlayer 서버 tick에서 진행한다. Client는 PlayerStatusState.TreatmentUntil의 서버 시각으로 표시하며 요청에 회복량/대상/효과값을 보내지 않는다. Owner RPC는 단조 증가 sequence를 검사해 동일 요청을 사용 간격 이후 재생해도 거절한다. 새 직렬화 필드·RPC payload 때문에Protocol14가 필요하다.13 peer와 혼용하지 않는다.

Save11의 기존 inventory/resource codec을 사용한다. 적용된HP·소비 stack·Herb tombstone/ticket만 저장하고 남은 치료·공용 간격은 Load/reHost/사망에서 초기화한다. 새 치료를 완전히 재지급하지 않는다. 기존 Save11·v4/v10 migration·generation1/2/3 원래 source 호환 검사 통과. 사용자 UserData2개 hash와mtime_ns는 [before](user-save-before.json)/[after](user-save-after.json)가 동일하다. QA는 Captures 아래 격리 save/identity만 썼고 종료 후 profile을 해제했다.

## 검증 결과

- [automated-results.json](automated-results.json):관련 EditMode76개 고유 case 최종 모두 통과, PlayMode4/4 통과. 기존 migration·network serialization·resource economy·item/recipe 및 scene contracts 포함.
- 최초 Edit는74/76. 새 RPC request 인자와 지속회복 효과를 반영하지 않은 과거 contract2개를 갱신했다. 재실행에서 resource enum의 과거 기본 Meat 가정1개를 발견해 새 Herb case를 추가했다. 최신 case 결과를 합쳐76/76이며 실패 당시 XML/log도 보존했다. 통과한 전체 묶음을 반복하지 않았다.
- Data validator errors=[]; source validator1274 unique GUID/참조/Landscape PASS. 새6개 EN/KO key의 중복·빈값·format placeholder 일치를 검사했다.
- [live.tsv](live.tsv), [batch-play.tsv](batch-play.tsv):실제1920×1080 MCP Play에서 Forest/Wetlands의 자연 Herb→맨손 처치→yield2 pickup, Inventory 아이콘, 두 Hand 카드 제작, EN/KO 효과 설명, HP full 거절/미소비, 피해 중 HoT·비중첩·즉시30·양방향 공용4초, Save/Load를 확인했다.
- 실제 local Host의 자연 Herb 권위·drop/pickup, disabled survival 상태의 서버 HoT 진행, 요청 replay 거절(4초 이후 포함), 치료 중 저장→reHost의HP/소비량 보존·남은 효과 초기화를 확인했다.
- 최초 HUD가 HP Fill 자식이라 보이지 않는 문제를 Safe Area의 작은 Text로 교정하고 [SP](captures/10-sp-treatment-fixed.png)/[Host](captures/08-host-active.png)에서 확인했다. 새 buff bar는 없다.
- 최초 실제 reHost 시도는 자동 PlayMode Host와 동시에7777 포트를 사용해 transport bind 실패했다. 자동 Host 종료 후 실제 MCP를 순차 재실행해 Host/reHost 모두 통과했다. 실패 로그를 숨기지 않았다. Windows 입력/Focus/Show 호출0.
- 대표 화면: [Forest](captures/01-herb-Forest.png), [Wetlands](captures/01-herb-Wetlands.png), [Dressing 제작](captures/03-craft-field_dressing.png), [Salve 제작](captures/03-craft-healing_salve.png), [EN](captures/04-description-en.png), [Host 채집](captures/08-host-herb.png), [reHost](captures/09-rehost.png).

## 판정 / 한계

**A (이번 구현·관련 검증 범위)**. 저가 지속치료와 Gland를 쓰는 즉시치료를 분리했고 authority/save/economy 구조의 차단 문제는 발견하지 못했다. 원격 client·실제4인/Relay·Android touch/FPS·장시간 자연 파밍·재미와 전투 체감은 미검증이다.4인 표는 계산이며 실제4인 플레이 합격을 뜻하지 않는다. 후속 예정은 Content Expansion2-B:Food Expansion이고 이번에 구현하지 않았다.
