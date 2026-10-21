# 현재 제작 콘텐츠 · Fishing / Aquatic 2-D

2026-10-21 Exploration2-E 반영. **Item134 / Recipe111 / Save11 / Protocol17**, 신규 월드 generation3. Workbench I→II→III와 Wood/Stone/Iron/Steel/Diamond를 제공한다. 실제 정본은 ContentCatalog/RecipeDefinition/각 행동·Loot 에셋이다. 아래 기존104개 표와2-D 신규5개,2-E 신규2개가 전체111개다. 이전 I–II/III 패스의 경위는 validation/tier-three와 기존 review 기록을 따른다.

## 이번 연결

Bone→반복 Arrow / 저비용 Trap / HP 지원 Broth, Chitin→Body sidegrade / 반복 Bolt / 기존 Frame, Medicinal Gland→Hand Salve / Stamina Broth다. 종 전용 raw material, Fur/Hide, Trophy, 새 전문 제작대는 추가하지 않았다. [Expansion 외부 비교와 결정 근거](../validation/creature-loot-expansion/REVIEW.md), [드랍 교정 당시 source→recipe→use 그래프](../validation/loot-plausibility-correction/CURRENT_GRAPH.md).

현재 Feather는 Chicken/Crow, Plant Fiber는 Small Plant가 공급한다. Sheep/Zombie/Goblin의 Fiber와 Goblin의 Bolt·새총 Goblin의 Feather, Skeleton의 원광 보너스는 개연성 교정에서 제거했다. 드랍 교정에서는 당시 Recipe99의 비용/배치량을 유지했다. 이후2-A는 아래 약품만 변경하며 기존 보유 stack·Camp 보급품도 보존한다. [공급·탄약 지속 비용 계산](../validation/loot-plausibility-correction/REVIEW.md)을 따른다.

활은 빠른 사격, 쇠뇌는 느린 장전과 높은 단발 피해를 유지한다. Bone Arrow는 기본 Arrow 대비 피해+3, Bronze Arrow보다 낮고, Barbed Bolt는 기본 Bolt 대비+6, Steel Bolt보다 낮다. Carapace Vest는 방어3(철 Body4보다 낮음), Stamina 회복속도+15%다. 숫자 상위 장비 tier를 추가하지 않는다.

Telescope는 제한된 지형 표시만 요청하고 simulation chunk를 늘리지 않는다. Signal Beacon은 한 개를 선택해 추적한다. Crystal Lantern은 기존 Lantern을 소비하는 Utility다. Ballista는 고피해/느린 사격, Repeater는 낮은 단발/빠른 사격이며 실제 호환 탄약을 소비한다. Auto Loader/Auto Aim/Ammo Box/Rotation Gear/Reinforced Frame은 기존 슬롯·cargo·회전·내구 모델을 사용한다.

## Station 감사 결과

|Station|현재 Recipe|역할 / 겹침 검토|
|---|---:|---|
|BlastFurnace|1|Steel1개. Coal을 쓰는 고급 합금 단계 자체가 역할.|
|Campfire|7|단순 가열/물·Trail Ration·고등어 소분식.|
|CookingPot|9|일반 Stew·즉시 회복 Broth·단일 기능 Meal3개.|
|Furnace|4|Iron/Copper/Tin/Bronze4개. 생물 가공을 억지로 넣지 않음.|
|None|10|현장 조립·기본 생존·붕대·연고·선택 생선 손질4개.|
|TurretWorkbench|8|포탑·탄약·모듈8개. Frame의 Leather2를 Leather1+Chitin2로 조정.|
|Workbench 1|36|기본 도구·장비·시설 조립. Bone Arrow/Trap2개 추가.|
|Workbench 2|25|중기 장비·시설. Vest/Barbed Bolt2개 추가.|
|Workbench 3|11|Diamond·유틸리티·Turret 진입. 이번 추가0.|

Workbench 수는 tier별 신규 Recipe다. I36 / II25 / III11이며 누적36/61/72다. Hand10, Campfire7, Pot9, Furnace4, Blast1, Turret8과 합쳐111이다. 전문 처리 시설을 추가하면 맡길 실재 Recipe가 너무 적고 기존 시설과 역할이 겹친다. Furnace의4개와 Blast의1개는 변경하지 않았다. 시설 수를 채우기 위한 새 중간재는 없다.

## 기존104 Recipe (신규5개는 마지막2-D 표)

None은 Hand, Workbench tier는 요구 최소값이다. 상위 Workbench는 하위 Recipe를 유지한다. 초는 해당 Recipe의 기본 처리시간이며 시설의 ProcessingSpeed가 적용된다. 병/용기 recipe의 Charges/Quality는 실제 데이터와 공용 inventory codec을 사용한다.

|Recipe ID|출력|Station / Tier|재료|초|
|---|---|---|---|---:|
|steel|steel×1|BlastFurnace / 1|iron_ingot×1 + coal×1|6|
|clean_water_cup|clean_water_cup×1|Campfire / 1|raw_water_cup×1|5|
|cooked_fish|cooked_fish×1|Campfire / 1|raw_fish×1|5|
|cooked_meat|cooked_meat×1|Campfire / 1|raw_meat×1|5|
|purify_waterskin|waterskin×1|Campfire / 1|waterskin×1|10|
|roasted_mushroom|roasted_mushroom×1|Campfire / 1|mushroom×1|5|
|trail_ration|trail_ration×1|Campfire / 1|raw_meat×1 + mushroom×1 + berry×2|7|
|bone_broth|bone_broth×1|CookingPot / 1|bone×2 + mushroom×1 + raw_meat×1|8|
|hunters_stew|hunters_stew×1|CookingPot / 1|raw_meat×2 + bone×1 + mushroom×1 + berry×1|10|
|forager_soup|forager_soup×1|CookingPot / 1|medicinal_herb×1 + mushroom×2 + berry×1|10|
|trail_broth|trail_broth×1|CookingPot / 1|raw_fish×1 + medicinal_herb×1 + berry×2|10|
|fish_stew|fish_stew×1|CookingPot / 1|raw_fish×1 + mushroom×1 + berry×2|8|
|meat_stew|meat_stew×1|CookingPot / 1|raw_meat×1 + mushroom×1 + berry×2|8|
|mixed_stew|mixed_stew×1|CookingPot / 1|raw_meat×1 + raw_fish×1 + mushroom×1 + berry×2|10|
|mushroom_stew|mushroom_stew×1|CookingPot / 1|mushroom×2 + berry×2|7|
|restorative_broth|restorative_broth×1|CookingPot / 1|medicinal_gland×1 + mushroom×1 + berry×2|8|
|bronze|bronze×3|Furnace / 1|copper_ingot×2 + tin_ingot×1|5|
|copper_ingot|copper_ingot×1|Furnace / 1|copper_ore×1 + coal×1|3|
|iron_ingot|iron_ingot×1|Furnace / 1|iron_ore×1|2|
|tin_ingot|tin_ingot×1|Furnace / 1|tin_ore×1 + coal×1|3|
|campfire|campfire×1|None / 1|wood×4 + stone×3|0|
|empty_cup|empty_cup×1|None / 1|wood×2|0|
|field_dressing|field_dressing×1|None / 1|plant_fiber×2 + medicinal_herb×1|0|
|healing_salve|healing_salve×1|None / 1|medicinal_gland×1 + medicinal_herb×1 + plant_fiber×2|0|
|torch|torch×4|None / 1|wood×1 + coal×1|0|
|workbench|workbench×1|None / 1|wood×6|0|
|ammo_box|ammo_box×1|TurretWorkbench / 3|wood×4 + steel×1|0|
|auto_aim|auto_aim×1|TurretWorkbench / 3|mechanical_part×1 + crystal×2 + bronze×1|0|
|auto_loader|auto_loader×1|TurretWorkbench / 3|mechanical_part×2 + steel×1 + bronze×1|0|
|ballista|ballista×1|TurretWorkbench / 3|wood×10 + steel×3 + leather×2 + mechanical_part×1|0|
|ballista_bolt|ballista_bolt×6|TurretWorkbench / 3|wood×2 + steel×1|0|
|reinforced_frame|reinforced_frame×1|TurretWorkbench / 3|steel×2 + leather×1 + chitin×2|0|
|repeater|repeater×1|TurretWorkbench / 3|wood×8 + bronze×3 + steel×2 + mechanical_part×2|0|
|rotation_gear|rotation_gear×1|TurretWorkbench / 3|mechanical_part×1 + bronze×2|0|
|arrow|arrow×12|Workbench / 1|wood×2 + feather×1|0|
|axe|axe×1|Workbench / 1|wood×5|0|
|bone_arrow|bone_arrow×12|Workbench / 1|bone×2 + wood×1 + feather×1|0|
|bone_trap|bone_trap×1|Workbench / 1|bone×2 + wood×3|0|
|bow|bow×1|Workbench / 1|wood×5 + plant_fiber×4|0|
|compass|compass×1|Workbench / 1|wood×2 + iron_ingot×1|0|
|cooking_pot|cooking_pot×1|Workbench / 1|stone×6 + iron_ingot×2|0|
|fishing_rod|fishing_rod×1|Workbench / 1|wood×4 + plant_fiber×3|0|
|furnace|furnace×1|Workbench / 1|stone×8 + wood×4|0|
|iron_axe|iron_axe×1|Workbench / 1|stone_axe×1 + iron_ingot×3|0|
|iron_body|iron_body×1|Workbench / 1|iron_ingot×3 + leather×1|0|
|iron_head|iron_head×1|Workbench / 1|iron_ingot×2 + leather×1|0|
|iron_pickaxe|iron_pickaxe×1|Workbench / 1|stone_pickaxe×1 + iron_ingot×3|0|
|iron_spear|iron_spear×1|Workbench / 1|stone_spear×1 + iron_ingot×3|0|
|iron_sword|iron_sword×1|Workbench / 1|stone_sword×1 + iron_ingot×3|0|
|leather_body|leather_body×1|Workbench / 1|leather×2 + wood×3|0|
|leather_head|leather_head×1|Workbench / 1|leather×1 + wood×2|0|
|pickaxe|pickaxe×1|Workbench / 1|wood×5|0|
|stone_axe|stone_axe×1|Workbench / 1|axe×1 + stone×4|0|
|stone_door|stone_door×1|Workbench / 1|stone×4 + wood×2|0|
|stone_fence|stone_fence×2|Workbench / 1|stone×4 + wood×1|0|
|stone_fence_gate|stone_fence_gate×1|Workbench / 1|stone×4 + wood×2|0|
|stone_gate|stone_gate×1|Workbench / 1|stone×7 + wood×3|0|
|stone_pickaxe|stone_pickaxe×1|Workbench / 1|pickaxe×1 + stone×4|0|
|stone_spear|stone_spear×1|Workbench / 1|wood_spear×1 + stone×4|0|
|stone_sword|stone_sword×1|Workbench / 1|wood_sword×1 + stone×5|0|
|storage_chest|storage_chest×1|Workbench / 1|wood×6|0|
|waterskin|waterskin×1|Workbench / 1|leather×2 + plant_fiber×3|0|
|wood_door|wood_door×1|Workbench / 1|wood×3|0|
|wood_fence|wood_fence×2|Workbench / 1|wood×3 + plant_fiber×1|0|
|wood_fence_gate|wood_fence_gate×1|Workbench / 1|wood×3 + plant_fiber×2|0|
|wood_gate|wood_gate×1|Workbench / 1|wood×6 + plant_fiber×2|0|
|wood_spear|wood_spear×1|Workbench / 1|wood×4 + plant_fiber×2|0|
|wood_sword|wood_sword×1|Workbench / 1|wood×5|0|
|wood_wall|wood_wall×1|Workbench / 1|wood×2|0|
|barbed_bolt|barbed_bolt×8|Workbench / 2|chitin×1 + wood×2 + feather×1|0|
|blast_furnace|blast_furnace×1|Workbench / 2|bronze×3 + iron_ingot×3 + stone×8 + wood×4|0|
|brazier|brazier×1|Workbench / 2|bronze×3 + stone×4 + coal×2|0|
|bronze_arrow|bronze_arrow×12|Workbench / 2|wood×2 + feather×1 + bronze×1|0|
|carapace_vest|carapace_vest×1|Workbench / 2|chitin×4 + leather×1 + plant_fiber×2|0|
|hunters_spear|hunters_spear×1|Workbench / 2|iron_spear×1 + bone×6 + leather×2|0|
|crossbow|crossbow×1|Workbench / 2|wood×6 + bronze×3 + leather×2|0|
|crossbow_bolt|crossbow_bolt×12|Workbench / 2|wood×2 + feather×1|0|
|lantern|lantern×1|Workbench / 2|bronze×2 + leather×1 + coal×2|0|
|prospecting_pan|prospecting_pan×1|Workbench / 2|copper_ingot×2 + wood×2|0|
|reinforced_bow|reinforced_bow×1|Workbench / 2|bow×1 + bronze×2 + leather×1|0|
|reinforced_chest|reinforced_chest×1|Workbench / 2|wood×8 + iron_ingot×2|0|
|reinforced_door|reinforced_door×1|Workbench / 2|stone×3 + wood×2 + bronze×2|0|
|reinforced_gate|reinforced_gate×1|Workbench / 2|stone×5 + wood×4 + bronze×3|0|
|reinforced_wall|reinforced_wall×1|Workbench / 2|stone×3 + wood×2 + bronze×1|0|
|spike_trap|spike_trap×1|Workbench / 2|wood×4 + stone×2 + bronze×1|0|
|steel_axe|steel_axe×1|Workbench / 2|iron_axe×1 + steel×3 + wood×2|0|
|steel_body|steel_body×1|Workbench / 2|iron_body×1 + steel×3 + leather×1|0|
|steel_bolt|steel_bolt×8|Workbench / 2|wood×2 + feather×1 + steel×1|0|
|steel_head|steel_head×1|Workbench / 2|iron_head×1 + steel×2 + leather×1|0|
|steel_pickaxe|steel_pickaxe×1|Workbench / 2|iron_pickaxe×1 + steel×3 + wood×2|0|
|steel_spear|steel_spear×1|Workbench / 2|iron_spear×1 + steel×3 + leather×1|0|
|steel_sword|steel_sword×1|Workbench / 2|iron_sword×1 + steel×4 + leather×1|0|
|stone_wall|stone_wall×1|Workbench / 2|stone×3 + wood×1|0|
|crystal_lantern|crystal_lantern×1|Workbench / 3|lantern×1 + crystal×2 + bronze×1|0|
|diamond_axe|diamond_axe×1|Workbench / 3|steel_axe×1 + diamond×1|0|
|diamond_body|diamond_body×1|Workbench / 3|steel_body×1 + diamond×3|0|
|diamond_head|diamond_head×1|Workbench / 3|steel_head×1 + diamond×2|0|
|diamond_pickaxe|diamond_pickaxe×1|Workbench / 3|steel_pickaxe×1 + diamond×2|0|
|diamond_spear|diamond_spear×1|Workbench / 3|steel_spear×1 + diamond×2|0|
|diamond_sword|diamond_sword×1|Workbench / 3|steel_sword×1 + diamond×2|0|
|mechanical_part|mechanical_part×2|Workbench / 3|bronze×1 + steel×1|0|
|signal_beacon|signal_beacon×1|Workbench / 3|crystal×2 + bronze×1 + mechanical_part×1 + wood×2|0|
|telescope|telescope×1|Workbench / 3|crystal×2 + bronze×1 + leather×1|0|
|turret_workbench|turret_workbench×1|Workbench / 3|wood×8 + stone×8 + bronze×4 + steel×4 + crystal×2 + mechanical_part×2|0|

## 제작대 업그레이드와 기존 진행

설치 I→II는 Wood8+Stone6+Iron Ingot2, II→III는 Wood10+Stone10+Bronze4+Steel4+Crystal4+Diamond1이다. 별도 II/III 조합법은 없다. 설치물 ID/위치/HP 비율과 열린 제작 참조를 보존한다. Diamond 개인6장비는 기존 Steel 장비를 실제로 소비하고 Diamond 합12를 쓴다. 공유 업그레이드1 포함1P13 / 4P49다. Mechanical Part는 Bronze1+Steel1→2개로 제작 가능하며 Camp RNG가 필수 조건이 아니다. Workbench IV~VI는 구조적 상한만 있고 실제 콘텐츠는 없다.

## Goblin Camp

보급품은 플레이어 Storage와 분리한6칸 take-only container다. 살아 있거나 잠든 거주 Goblin이 남으면 획득할 수 없다. 기본4묶음(Bone Arrow8–16/Salve1–2/Bronze1–2/Leather1–2), 선택2묶음(Mechanical Part1 20%/Barbed Bolt8 35%)을 최초1회 생성한다. 캠프 ID와 seed로 고정하고 잔량/개봉/revision을 저장한다. 거주 적 보충은 상자를 보충하지 않는다. Host만 range/alive/가방·상자 revision/공간을 검증해 묶음 전체를 교환한다.

## Medicine 2-A · 현재

추가 Item은 Medicinal Herb(약초, stack50)와 Field Dressing(응급 붕대, stack20)2개, Recipe는 Hand 붕대1개다. 2-A 도입 당시 2-A 도입 당시 Herb→붕대/연고2 sink, Gland→연고/Stamina Broth2 sink를 가진다. 붕대20HP/8초, Salve30HP 즉시, 공용4초·지속치료 비중첩이다. 연고 ID·보유 stack은 유지한다. 전문 Station/상위 약품/음식 수치 변경은 없다. [외부 비교·1P/4P 경제·검수](../validation/medicine-expansion/REVIEW.md).

## Food2-B · 현재

새 Item3/Recipe3, raw0/Station0이며 Pot9개다.일반 Stew H/T는Mushroom35/18,Meat52/10,Fish42/26,Mixed65/18+즉시HP8이다. Meal은150초 동안 하나만 유지한다. Hunter H44/T12·MaxHP+8(즉시heal0),Forager H30/T18·조건부자연회복+0.15HP/s,Trail H28/T22·Stamina회복+10%다. 동종은150초로갱신·다른Meal은교체하며시간가산없다. BoneBroth18HP/Restorative35Stamina/Ration20Stamina·의약품은이전효과대로다. [전체 효과·1P/4P 경쟁·검수](../validation/food-expansion/REVIEW.md).

## Sidegrade2-C · 현재

추가Item/Recipe각1로127/104. 사냥꾼 창은WBII IronSpear1+Bone6+Leather2, Damage27/Range3.35/Arc24°/Cooldown.90/Duration.63/단일대상이다. Iron29/2.8/.78보다거리만길고피해·속도·Arc가낮다. 기존Steel/Diamond체인은Hunter를재료로받지않는다. Head는Defense/Recovery외지원효과가없어보류하며Vest변경0,새raw/Station0. Bone5sink/Chitin3sink,Save11/Protocol15유지. [조사·장비감사·1P/4P·실제검수](../validation/creature-sidegrade-expansion/REVIEW.md).


## Fishing2-D · 현재 추가 Recipe5

현재 총Item132/Recipe109/Save11/Protocol16. 기존104개 Recipe와 이전 검수 기록은 보존한다. 신규 raw Item은 물고기4종, 완성 음식1개,새Station0이다.

|ID|결과|Station|재료|초|
|---|---|---|---|---:|
|fillet_crucian_carp|raw_fish1|Hand|crucian_carp1|0|
|fillet_perch|raw_fish2|Hand|perch1|0|
|fillet_mackerel|raw_fish1|Hand|mackerel1|0|
|fillet_eel|raw_fish2|Hand|eel1|0|
|fish_portions|fish_portions2|Campfire|mackerel1+berry1|8|

5catch identity와 수역 table·소비/손질 선택은 [음식 정본](FOOD_AND_FISHING.md),선정/배제 이유와외부비교·1P/4P·검증한계는 [2-D REVIEW](../validation/fishing-aquatic-expansion/REVIEW.md)를 따른다. Hand10/Campfire7/Pot9/WBI35/II24/III11이며 기존 금속/시설 예산은 바뀌지 않는다.

## Exploration Utility2-E

신규2Item/2Recipe, raw0/station0. 광물 조사기 `ore_surveyor`: WBII Wood2+Bronze2+Leather1→1. 신호 호각 `signal_whistle`: WBI Wood2+PlantFiber2→1. 두 도구는 stack1/reusable/손 선택이며 전투 ToolDefinition이 없다. Survey는 자원 수색, Whistle은 일시 협동 위치 신호다. 기존 utility 변경0, POI 보상 변경0. 총134/111, WBI36/II25/III11(누적36/61/72), Hand10/Fire7/Pot9/Furnace4/Blast1/Turret8. [역할 감사와 경제](../validation/exploration-utility-expansion/REVIEW.md).
