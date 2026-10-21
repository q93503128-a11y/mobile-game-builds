# 음식·낚시·재사용 물

2026-10-21. Hunger0.08/s·Thirst0.12/s,기존 RawWater 불이익·낚시/물 구조는 유지한다. 기본 식재료·Campfire 간편식은 무buff,일반 Stew는영양,Higher Meal3종은일시준비선택이다. 아래는현재정본Data를읽은값이며음식의공용간격0은추가대기가없음을뜻한다(진행중공용간격은우회하지않음).

|ID|Hunger|Thirst|즉시HP|즉시Stamina|사용간격초|
|---|---:|---:|---:|---:|---:|
|berry|8|3|0|0|0|
|bone_broth|24|12|18|0|4|
|clean_water_cup|0|38|0|0|0|
|cooked_fish|26|0|0|0|0|
|cooked_meat|35|0|0|0|0|
|field_dressing|0|0|0|0|4|
|fish_stew|42|26|0|0|0|
|forager_soup|30|18|0|0|4|
|healing_salve|0|0|30|0|4|
|hunters_stew|44|12|0|0|4|
|meat_stew|52|10|0|0|0|
|mixed_stew|65|18|8|0|4|
|mushroom|5|0|0|0|0|
|mushroom_stew|35|18|0|0|0|
|raw_fish|6|0|0|0|0|
|raw_meat|8|0|0|0|0|
|raw_water_cup|0|15|0|0|0|
|restorative_broth|22|10|0|35|4|
|roasted_mushroom|16|0|0|0|0|
|trail_broth|28|22|0|0|4|
|trail_ration|25|0|0|20|4|

Salve는 현장 Hand, Trail Ration은 Campfire7초, Bone/Restorative Broth는 Pot8초다. 기존 Stew의 Hunger 우위·Cooked Meat의 단순 비용을 유지하고 HP/Stamina 지원을 선택하게 한다. 전체 비용은 CONTENT_EXPANSION을 따른다.

기존 Cooked Meat와 Cup 정수는 유지한다. Cooking Pot은 독립 설치물이며 기존 sequential 처리기를 재사용한다. 입력을 예약하고 완료 결과는 Take로 수령하며 full bag에서는 cargo를 보존한다. 파괴/휴면/Save/reHost는 기존 처리 상태 규칙과 같다.

Fishing Rod는 WB I Wood4+Fiber3. 바라보는 방향1.5~5unit의 유효 자연 수면에서 cast.45s→3초 시점 입질→2.2초 입력 창을 유지한다. 보상은 아래2-D 표를 따른다. 성공 후.6초 회수한다. 이동1.2unit 초과/도구 변경/사망/모달은 취소한다. 실패/가방 포화는 보상을 생성하지 않으며 입질 창이 남으면 다시 입력할 수 있다. 보상 성공은 phase를 바꾸어 중복 요청을 거절한다. Host 권위이며 client는 line/bobber/입질을 시간 표본으로 표시한다.

Waterskin은 WB I Leather2+Fiber3, 한 item/3회분/stack1이다. 빈 상태 Primary는 채우기, 채운 상태 Primary는 마시기, Interact는 근처 자연 물에서 보충이다. 정수된 가득 찬 용기를 Raw로 덮지 않는다. 부분 충전/정수 용기를 보충하면 Raw3/3이 되므로 다시 정수해야 한다. Campfire의 Purify Waterskin은 Raw3/3만10초 처리해 Clean3/3으로 돌려준다. 각 drink는 기존 Cup의 Raw15 또는 Clean38 회복/불이익을 사용하며.5초 입력 간격을 둔다. 한 용기의 서로 다른 charge/quality가 합쳐지지 않는다.

2026-10-19 교정: remote rod 표현은 owner-only inventory 대신 replicated PlayerEquipment.SelectedItem을 따른다. 취소/만료/회수 종료는 기존 ActivityKind.None을 publish하며 다음 입질 안내 상태를 초기화한다. 새 protocol 필드나 낚시 시간/보상 변경은 없다.

## Medicine2-A와 음식 경계

Salve는 음식 분류와 구분되는 Medicine이다. 응급 붕대는 즉시0/총20HP를8초간 회복하며 Fiber2+Herb1, Salve는Gland1+Herb1+Fiber2로 즉시30HP다. 두 약품과 기존 회복 음식의 입력 제한은 공용4초다. 붕대 효과가 남으면 추가 붕대는 소비하지 않는다. 설명/아이콘과 작은 잔여 치료 표시를 제공한다. 음식 최대 즉시 회복은 Bone Broth18로 Salve보다 낮아 음식 수치를 변경하지 않았다. 음식2-B의Meal은아래준비역할만추가한다.

## Food2-B · Meal 선택

|음식|Pot 재료/시간|H/T|150초 Meal 효과|
|---|---|---|---|
|사냥꾼 스튜|Meat2+Bone1+Mushroom1+Berry1 /10초|44/12|MaxHP+8·현재HP즉시회복없음|
|채집꾼 수프|Herb1+Mushroom2+Berry1 /10초|30/18|기존안전회복+0.15HP/s|
|원정 국물|Fish1+Herb1+Berry2 /10초|28/22|Stamina회복속도+10%|

한Meal만활성·새Meal은교체·같은Meal도150초로reset(시간가산없음).일반음식/의약은Meal을교체하지않는다.세Meal은즉시HP/Stamina를주지않으며공용4초를적용한다.H/T가가득차도준비목적으로사용할수있다.자동재섭취없음.

MushroomStew35/18은고기/낚시없는식물수분식,MeatStew52/10은허기우선,FishStew42/26은수분우선,Mixed65/18/HP8은두고기재료를쓰는범용영양식이다.각각Status를붙이지않는다.BoneBroth는즉시18HP,Restorative는즉시Stamina35,CampfireRation은휴대식H25/Stamina20으로유지한다.Salve30즉시와Dressing20/8초가위기치료를맡는다.

Forager는H/T≥70%·마지막전투8초·RawWater회복차단을모두지킨다.기본0.5→0.65HP/s이며Dressing속도를올리지않는다.MP에서도기존함수를서버tick에연결했다(이전에는Survival.Update가꺼져호출이없었음).MPH/T자연소모구조는이번에바꾸지않았다.Trail은기본16→17.6Stamina/s이며기존회복대기1.2초·Dash24·Sprint14/s를유지한다.Skill/Armor와기존가산집계를쓴다.

활성Meal은작은아이콘·이름·남은분:초로표시하고메뉴에서는숨긴다.Pot제작전영양·핵심효과·150초/교체규칙을보여주며Inventory에는상세조건·공용간격을표시한다.Load/reHost/죽음은Meal을초기화하고임시최대치를영구저장하지않는다. [조사·경제·검수](../validation/food-expansion/REVIEW.md).

Host의RawWater회복차단·소모불이익타이머는기존Model.AdvanceEffects로진행한다.MPH/T자연소모를켜지않으며SP구간별소모계산순서는보존한다.식사때문에일시차단이영구차단으로남지않는다.


## Fishing2-D · 수역·획득물·사용처

Item132/Recipe109/Save11/Protocol16. 기존 Rod(WBI Wood4/Fiber3) 하나로 모든 catch에 접근한다. 미끼와 낚시 Skill은 없다. 방향을 따라 .5unit씩 검색해 첫 수면부터 이어진 구간의 최대5unit 지점을 선택한다. 육지에서 수면 안쪽까지의 signed 거리 근사2.25unit 이상+실제 DeepWater인 곳을 '안쪽 수면'으로 안내한다. 실제 수직 수심은 없다. 더 넓은 물/접근 위치·방향이 농어 접근을 바꾸며 수영/정밀 타깃 입력을 요구하지 않는다.

|Catch|발견 조건|가중치|생식H/T|손질/용도|
|---|---|---:|---|---|
|기존 raw_fish|내륙·습지·동굴|8/6/1|6/0|기존4요리 재료 그대로|
|붕어|Wetlands|4|6/6|Hand1→raw1,소량수분 생식 선택|
|농어|내륙/습지 안쪽 수면|2|6/0|Hand1→raw2|
|고등어|CoastDistance<0 바다|1|6/0|Hand1→raw1 또는 Campfire 소분식|
|장어|내륙/습지 Night|.5|8/0|Hand1→raw2,핵심 진행 gate없음|

조건을 만족하는 가중치만 정규화한다. 내륙 얕은낮100%raw,깊은낮80%raw/20%농어;습지깊은밤48%raw/32%붕어/16%농어/4%장어;바다100%고등어. 표의'깊은'은 안쪽 수면 조건의 약칭이다. generation1은내륙,동굴은공용raw만 유지한다. Cavern pool의-2/15 이진 WaterDistance를수심으로사용하지 않는다.

새4종 stack50은 언제든 Hand 손질로 기존 stack과 합칠 수 있다. 실제 Recipe는 정확 Item ID로 받고 종별 요리 복제는 없다. 고등어1+열매1/8초→생선 소분식2개는 각H20/T2이며 HP/Stamina/Meal효과0이다. 보존시간 시스템이 없는 게임이므로 부패 지연이나 유통기한 효과를 주장하지 않는다. 기존 CookedFish26/0, FishStew42/26, Mixed65/18+HP8, Trail28/22+150초 회복10%를 유지한다.

Host가 cast 순간 수역·밤·가중치 결과를 정한다. 가방 full에도 같은 catch를 유지하며 Bite창 안에서 공간을 비우면1회만 지급한다. 성공은 짧은 아이콘+이름+수량이며 결과모달/ID/확률을 표시하지 않는다. 이동/모달/선택 변경/사망은 취소,Use도 즉시 조건/시간을 검사한다. Save/Load/reHost는 pending 낚시를 버리고 실제 받은 가방만 보존한다. [조사·경제·자동/MCP 검수](../validation/fishing-aquatic-expansion/REVIEW.md).
