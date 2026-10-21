# Content Expansion 2-C · Creature Material Sidegrades

2026-10-21. 시작 main `c7f9d162e381d697aec4d1412ffe4c75695108c6`; fetch 후 local=remote, clean. 아래 조사/감사/설계 결정을 구현 전에 기록했다. 과거 review는 당시 기록으로 보존한다.

## 공개 자료 비교 · 구현 전 게이트

공식 문서/패치 검색을 우선했으나 상세 제작·공격 데이터는 아래 공식/커뮤니티 Wiki에서 확인했다. Wiki.gg/Fandom 일부 직접 open은403/robots로 막혀 검색 엔진이 반환한 해당 페이지 본문/표를 사용했다. 접근 실패를 전체 페이지 열람으로 간주하지 않는다. Necesse/atma.gg는 직접 본문도 읽었다. 외부 숫자는 비교 설명에만 쓰며 Lowfire로 복제하지 않는다.

|게임|해당 시스템|실제 사례 / 공개 출처|장점|Lowfire와 차이|Lowfire 적용 여부|이유|
|---|---|---|---|---|---|---|
|Grounded|생물 무기·진행|[Bone Trident](https://grounded.wiki.gg/wiki/Bone_Trident): Tier2, Sunken Bone/수중 거미 부품/Silk Rope, 수중 찌르기. [Silk Rope 제작표](https://grounded.wiki.gg/wiki/Silk_Rope): Bone Dagger/Trident와 Insect Bow/Crow Crossbow를 각각 연결|재료가 전투 용도·서식지와 연결|금속 tier 대신 곤충 tier이며 수중 전투·피해 타입이 있음|용도 구분만 채택|Bone 풀 장비군/수중 framework는 필요하지 않음|
|Grounded|Armor·회복|[공식 Wiki 효과 정의](https://grounded.wiki.gg/wiki/Module:StatusEffectsModule): Spider 세트 Hunter's Prowess는 Stamina 회복, Antlion Quickdraw는 Bow/Crossbow 장전 시간, Bee는 사격 Slow|효과가 공격 방식과 연결|Lowfire Armor에는 장전/상태/세트 효과가 없음|Vest만 유지, Head 보류|모든 부위에 회복을 붙이는 대신 지원 기능 경계 존중. 이전 Fandom Spider 설명과 효과 정의가 달라 단품 수치 일반화는 배제|
|Valheim|생물 Armor 대 금속|[Root Set](https://valheim.fandom.com/wiki/Root_Set), [Armor 표](https://valheim.fandom.com/wiki/Armor): Root harnesk는 Iron보다 낮은 방어, 관통 저항/화염 약점, 덜한 이동 패널티. Mask는 독 저항, 세트는 Bow skill|낮은 방어라도 상황 선택, 부위별 역할|Lowfire는 단일 일반 피해/Defense, 저항·독 없음|명시적 장단점 원칙 채택|저항·세트 시스템 신설은 배제. 기존 Vest의 방어 손해를 유지|
|Valheim|Chitin sink·우회 위험|[Abyssal Razor](https://valheim.fandom.com/wiki/Abyssal_razor): Chitin/Finewood/Leather, 짧은 knife와 leap/backstab, 광석 없이 Iron급. [Crafting](https://valheim.fandom.com/wiki/Crafting): Harpoon과 Razor, Razor 강화에도 Chitin|재료 몇 개의 서로 다른 용도와 강화 sink|Lowfire에는 강화/수리 없음. 광석 우회는 이번 목표와 충돌|소수 목적 채택, 광석 우회 배제|Hunter는 Iron Spear를 실제 소비. Chitin은 이미 Bolt/Frame/Vest3개 sink가 있음|
|Core Keeper|행동 sidegrade|[Grubzooka](https://corekeeper.atma.gg/en/Grubzooka): Tin+Larva Meat, 느린 사격/관통/약한 벽 파괴. [Iron Bow](https://corekeeper.atma.gg/en/Iron_Bow): 더 빠른 사격·별도 multishot. [Scarlet Dagger](https://corekeeper.atma.gg/en/Scarlet_Dagger): 빠른 근접과 crit 보정|피해 수치 외 궤적·속도·대상 처리 차이|Lowfire는 관통/AoE 필드가 있어도 PlayerCombat에 실제 구현 없음|속도/Reach/Arc만 채택|데이터 필드 존재만으로 관통을 지원한다고 주장하지 않음. 기존 Bow/Crossbow 이미 속도/단발 선택이므로 새 활은 보류|
|Core Keeper|장비 이후 소비|[Larva Meat](https://corekeeper.atma.gg/en/Larva_Meat): 직접 섭취/조리 가능, Grubzooka 제작에도 사용|한 번 제작 후에도 음식 sink|Lowfire Bone은 기존 음식/탄약/함정 반복 sink4개|혼합 sink 원칙 채택|새 반복 소비품을 수 채우기로 추가하지 않음|
|Terraria|생물 Armor·gate|[Necro armor](https://terraria.wiki.gg/wiki/Necro_armor): Dungeon Bone+Cobweb, ranged damage/crit, Skeletron 이후 획득. [Fossil armor](https://terraria.wiki.gg/wiki/Fossil_armor): 방어 외 ammo 절약|재료 획득 gate·무기군 특성으로 구분|Lowfire에 class/crit/ammo 절약/세트 없음|기존 진행 보존만 채택|Head1개 때문에 원거리 stat/클래스 framework를 만들지 않음|
|Terraria|지속 sink와 Reach|[Bone](https://terraria.wiki.gg/wiki/Bone): 자체 소비 투척·건축·Spinal Tap 제작. [Underworld guide](https://terraria.wiki.gg/wiki/Guide:Surviving_in_the_Underworld): 강한 Muramasa와 긴 Blade of Grass 비교|재료를 장비 외에도 소비하고 Reach 자체가 선택이 됨|Lowfire에는 뼈 투척/가구 demand 없음|긴 대신 약한 창만 채택|새 투척·가구를 추가할 필요 없음. 외부 장비 명칭/외형은 복제하지 않음|
|Necesse|Armor·업그레이드|[Spider Helmet](https://necessewiki.com/Spider_Helmet): 낮은 방어+summon slot, Arachnid Helmet 제작 시 기존 Helmet 소비. [Cave tier Armor](https://necessewiki.com/Cave_tier_Armor): 부위별 knockback/move/summon 효과|역할별 부위, 이전 장비가 후속 재료가 됨|Lowfire에는 summon/knockback/세트가 없음|기존 장비 소비 원칙 채택, Head 보류|지원하지 않는 특성 복제 금지|
|Necesse|한 재료의 의미 있는 연결|[Cave Spider Gland](https://necessewiki.com/Cave_Spider_Gland): Spider3부위+Boomerang/Staff와 Poison Arrow50개, 후속 Arachnid/Widow에 금속과 혼합|일회 장비·탄약·후속 제작 연결|Lowfire는 소규모 전투·3개 Chitin sink, 독 없음|sink 혼합 원칙 채택|대규모 세트/무기군 수를 복제하면 내용보다 framework가 커짐|

## 실제 equipment/stat 감사

EquipmentDefinition이라는 통합 타입은 없다. ItemDefinition이 ToolDefinition/RangedWeaponDefinition/ArmorDefinition을 참조한다. 실제 에셋과 아래 실행 경계가 정본이다.

|타입 / 경계|현재 지원 변수|실제 사용 / 미지원|Network / Save|이번에 안전한 선택|
|---|---|---|---|---|
|Axe/Pickaxe/Sword/Spear · ToolDefinition→PlayerCombat|Damage, Duration, Cooldown, Attack.Range/ArcAngle/MaxTargets/SelectionRule, HandExtension/SwingArc, Tier, 자원 Efficiency|피해는 모션50%, 쿨다운과 단일 대상 선택·hit 재검증, Health별 dedupe. PierceCount/AoERadius는 미구현. Spear는 회전 없이 찌르기. 공격 Stamina0, 이동 속도 penalty0|Host 공통 Combat 판정. AttackState는 Tool catalog index/sequence/hand/시각, replica는 같은 Tool.Present. 가방 ID+Selected 저장, transient 공격은 저장하지 않음|Range+느린 Cooldown+낮은 Damage+좁은 Arc. 새 framework 불필요|
|Bow/Reinforced Bow · PlayerRanged|DrawSeconds/RecoverySeconds/Speed/Range/Damage/AmmoKind|.35초 draw, .45/.43 recovery, speed18/22, range14/16, damage14/19+탄약. 실제 StoneProjectile sweep/Host spawn, 한 발 소비|기존 NetworkActivities/NetworkProjectile, 가방 ID. 발사 중 상태는 transient|변수는 안전하지만 기존 활/쇠뇌와 겹쳐 이번 추가0|
|Crossbow · PlayerRanged|동일+Crossbow pose|.08 draw+1.47 recovery가 장전 역할. speed24/range19/damage31+ammo. 별도 magazine/reload stat 없음|위와 동일|새 장전·탄창 시스템 배제|
|Head/Body · ArmorDefinition→PlayerEquipment|Defense, StaminaRecoveryBonus, Tier, WornSprite/HandSprite, Slot|방어합→Health max(1,damage−defense). 회복 bonus만 CharacterStats 사용. 이동/무게/저항/공격효율 없음|Host Equip 원자 가방 교환, Status Head/Body index; Save HeadArmor/BodyArmor ID|Vest 유지, 같은 회복 Head는 보류|
|Utility|EquippedLight/Utility ID, 기존 별도 슬롯|빛/표현, 일반 combat modifier 없음|Utility index/ID codec|새 accessory/slot 없음|
|Skill/Meal·CharacterStats|영구 MaxHP/MaxStamina/Recovery/SprintEfficiency, 단일 일시 Meal|회복=16×(1+Skill+Body+Head+Meal)×생존Condition. Defense는 별도. Sprint=14/(1+SkillEfficiency)/생존Efficiency, Dash24. 공격 비용0. 이동 중 Combat은 Facing만 고정, 속도/이동 봉쇄 없음|Host 계산, client는 replica; Skill 저장, Meal transient|HP/Stamina/이동 수치 추가 없이 공격 방식 선택|

읽은 경계: Runtime/Data/{ToolDefinition,AttackProfile,ArmorDefinition,RangedWeaponDefinition}, Items/ItemDefinition, Combat/PlayerCombat, Player/{PlayerRanged,PlayerEquipment,PlayerMotor,CharacterStats}, Presentation/CharacterFacing, Networking/{NetworkPlayer,NetworkPlayerState}, Persistence/PlayerSnapshotCodec. Catalog index는 기존 순서를 유지하고 뒤에 append한다. Inventory/SavedStack은 Item ID를 복원하므로 새 stat 필드·Save migration·Protocol field가 필요 없다. 현재 melee에는 solid LOS 검사가 없다. 긴 창이 벽 너머를 치지 않도록 공통 선택/impact 경계에 기존 wildlife 방식의 nonalloc solid ray 검사만 넣는다(자신/목표 Health 제외, 가득 찬 버퍼는 거절). 새 전투 framework가 아니다.

## 결정 · 외부 사례 → 현재 문제 → 원칙 → 배제 대안

1. Grounded Bone Trident의 특정 역할+Terraria 긴 Blade of Grass/Core Keeper 공격 패턴 구분 → Lowfire Spear는 기존 Iron/Steel/Diamond 모두Range2.8 → Hunter는거리 유지형 1개 → 피해만 바꾼 Bone 무기군·관통/상태 무기 배제(실행 코드 없음).
2. Valheim Razor 광석 우회+Necesse 기존 Helmet 소비 → 진행 우회 위험 → WBII에서 Iron Spear1+Bone6+Leather2 소비 → Wood+Bone만으로 바로 만드는 안 배제. SteelTier는 기존 WBII 정렬/도구 metadata이며 새 tier가 아니다. Hunter로 Steel/Diamond recipe를 대체하지 못한다.
3. Root 부위별 저항/Spider summon/Fossil ammo절약 → Armor에는 Defense+Recovery만 있음 → Carapace Head 보류 → 회복 또+10%/새 저항/세트/이동 stat 신설 배제. Chitin은 기존 Vest/Bolt/Frame으로 일회·반복 역할이 이미 있음.
4. Grounded Spider회복과 Valheim 낮은 방어 선택 → Vest는 ironBody보다−1 defense, 회복+15% → 명백한 무효/과도 수치가 없어 유지 → Head 중복·Skill 회복 상향·Vest강제nerf 배제.
5. Terraria 소비 Bone/CoreKeeper 조리 Larva/Necesse PoisonArrow → 장비 제작 뒤 재료 사망 위험 → 기존 반복4Bone/2Chitin 역할 유지하고 일회창만 추가 → 신규raw/소비품/수리/장비강화 패딩 배제.
6. Grounded/Valheim Workbench와 Necesse 기존Anvil의 혼합 재료 → 현재 WB I35/II23/III11(누적35/58/69), 추가 실제1recipe이고 processing 필요0 → WBII에1개(24,누적59/70) → 전문Station은 역할/레시피 밀도·가공필요가 없어 배제. Pot/Furnace/Blast 등 변경0.

## 채택 설계 (검증 전 확정한 초기값)

Hunter's Spear / 사냥꾼 창. WBII, Iron Spear1+Bone6+Leather2→1. Damage27, Range3.35, Arc24°, Cooldown.90초, Duration.63초, MaxTargets1, HandExtension1.52. 기존 찌르기 사용, 원본의 무광 목재·가죽 결속·가공 Bone 보강과 작은 철촉. Icon과 길게 쥐는 HeldSprite는 별도 자체 Pillow art. impact에서 hand를 aim선으로 모아 시각적 촉끝과 판정을 맞춘다. 새광물/장비tier/Station/raw0. 추가장비/recipe각1, Head0.

|장비|피해|Range|Arc|간격|이론 DPS(방어0)|목적|
|---|---:|---:|---:|---:|---:|---|
|Iron Spear|29|2.8|28|.78|37.18|저렴하고 Hunter보다 빠른 근접|
|Hunter's Spear|27|3.35|24|.90|30.00|+0.55 거리, 좁고 느리고 약함|
|Steel Spear|35|2.8|28|.78|44.87|WBII 금속 전투 성능|
|Diamond Spear|42|2.8|28|.78|53.85|WBIII 최고 일반 피해, 기존 Steel소비|

전부 무탄약/공격Stamina0/단일대상. Hunter는Iron보다도 피해·DPS·Arc가 낮다. Sword는넓은근접, Bow/Crossbow는탄약/사거리·발사주기 선택으로 남는다. 수치상거리이득이실제안전/재미를보장하지않으며 사용자 체감 검수는 남는다.

## Vest / Skill / Meal 계산

Vest Defense3 vs IronBody4 유지. baseline 회복16/s, Vest18.4/s, RecoveryRank1=18.4/s, Trail=17.6/s; Vest+Rank1+Trail=22.4/s, Rank3+Vest+Trail=27.2/s. 가산이지 곱연산이 아니다. Dash24 회복은 delay1.2초 포함 baseline2.70초 vs Vest2.504초(−.196초); 100회복은7.45초 vs6.635초(−.815초). Sprint지속/최대Stamina는 직접 늘리지 않는다. Recovery1rank와같은효과량이지만−1방어와가방/부위 선택 비용이 있고 Skill은 영구+최대3rank/장비교체 독립, Meal은150초/소비·교체·Load삭제다. 공격 회복 사이클에 이득이 있다고 주장하지 않는다(공격비용0). Zombie8피해에 IronHead2+Body4면2,Head2+Vest3면3피해로50%증가(최소피해1 조건주의). 무조건상위호환아님.

## Bone / Chitin source→sink · 계산 (장시간 실측 아님)

공급변경0. Skeleton/검Variant/Wolf/Goat Bone1–2 평균1.5, Deer1, GiantBoar2, Rabbit50%×1=.5, Zombie50%×1=.5, Bat40%×1=.4. StoneCrawlerChitin1/CrystalCrawler2 확정. 종별 접근/지역/시간/전투 난도는 달라 kill/min·실제4P충분성을 단정하지 않는다.

|재료|일회 sink|반복/확장 sink|2-C 이후|
|---|---|---|---|
|Bone|Hunter6/인|Arrow2→12(발당1/6),Trap2→1(설치 후 피해/파괴·확장),Broth2→18HP,Stew1→150초Meal|기존4+새1=5 recipe. 무기 제작 후에도기존소비 유지|
|Chitin|Vest4/인|Bolt1→8(발당1/8),Frame2/포탑(설치·파괴·기지확장)|3recipe유지. Head보류로의무파밍없음|

|시나리오|1P|4P(공유loot·총소비)|해석|
|---|---:|---:|---|
|새Hunter만|Bone6+Leather2+IronSpear1|Bone24+Leather8+IronSpear4|Skeleton 기대 수율 환산4/16kill. Bone만 모으고 멈추는 실제 기대 처치수는 초과분 때문에 약4.22/16.22다. 최소3/12kill(2씩), 최대6/24(1씩). 한두 마리로 완료 불가. Leather는Boar1/kill기준2/8kill,다른공급병행가능|
|Hunter+Arrow24+Trap1+Broth1+Stew2|Bone16|Bone64|반복묶음10/40+일회6/24. 평균Skeleton10.67/42.67kill상당, 실제복합수렵은별도. 장비가반복소비를압도하지않음|
|Vest+Bolt24+Frame1/인|Chitin9|Chitin36|StoneCrawler9/36kill 또는Crystal5/18kill(정수ceil). Frame공유1이면4P30. 새Chitin비용0|
|HunterMeal상시30분 가정|12Stew→Bone12|48Stew→Bone48|기존선택소비. Arrow120추가시Bone20/80,Trap2추가4/16. 상시Meal/전탄고급탄을필수로가정하지않음|

Arrow1발당Bone1/6, Bolt1/8. 계속발사하는상한은Bow1/.8=1.25발/s→Bone.2083/s,ReinforcedBow1/.78→.2137/s(분당12.82),Crossbow1/1.55→Chitin.08065/s(분당4.84). 4P분당각51.28Bone/19.35Chitin. 이는상시연사소비상한이며자연사냥생산율이아니다. 기본Arrow/Bolt는생물재료가필요없고현재고급탄/식사는선택이다. 이패스공급/배치량조정근거없음.

기존 Iron Spear가 없다면 개인 체인 비용은 Wood4+Fiber2+Stone4+Iron3에 위 Bone6+Leather2를 더한다. 공유 WBII까지 처음부터 만드는 경우 Workbench Wood6와 업그레이드 Wood8+Stone6+Iron2가 별도다. 1P 합계 Wood18/Stone10/Iron5/Fiber2/Bone6/Leather2, 4P 공유 Bench1개는 Wood30/Stone22/Iron14/Fiber8/Bone24/Leather8이다. Furnace/채집 도구 등 기존 공통 기반 시설은 이 무기 추가 비용에 중복 합산하지 않았다.

## 구현·검증 결과 · 2026-10-21

### 실제 변경

- 신규 장비/Recipe 각1, Item127/Recipe104. 기존 Catalog127번째에 append했고 기존 순서를 보존했다. `equipment-data.tsv`는 Unity 실제 데이터 전체 추출이다.
- WB I35/II24/III11, 누적35/59/70. 기존 Recipe/드랍/탄약 수율/Frame/Vest/Skill/Meal 변경0, 새 Station/raw0.
- PlayerCombat의 선택과 impact가 solid LOS를 재검사한다. 버퍼32 재사용, 과다 collider면 거절. Spear의 손은 찌르기 impact에서 aim선으로 모인다. 새 attack field/framework 없음.
- UI 사거리 표시는3.35를3.4로 반올림하지 않도록 필요한 소수 둘째 자리까지 표시한다. Icon과 Held art만 신규 자체 제작했다.

### 자동 검사

|검사|실행 결과|증거 / 한계|
|---|---|---|
|ProjectDataValidator|오류0|Unity 실제 Item/Recipe/장비/참조/업그레이드 graph 검사. MCP 최종127/104/Protocol15 확인|
|소스·GUID·참조·가시문자|PASS,1305 unique GUID|`python tests/validate_visual_project.py client`, 실제 실행 로그 요약을 이 표에 보존|
|EditMode 최종65개|65 PASS|첫 실행64/65. 실패는 Bone sink 기대값4가 신규5를 반영하지 못한 기존 계약1개. 기대값 교정 후 해당 parameterized3개만 재실행3/3. 첫 실패 XML도 보존하며 새 bone5가 old bone4를 대체한 최종65개를 `test-summary.json`에 집계|
|신규 장비 EditMode4개|PASS|실제 tradeoff/Recipe/기존 체인,append index/stack ID복원,sink/Head보류,Vest유지,기존 Attack/Status NetworkSerialize 왕복,EN/KO와별도held art|
|PlayMode 관련7개|7 PASS|신규 SP+Host/reHost2개,기존밀착/Facing행렬·range경계·채굴거절/1회피해·전체기본무기·friendly/multi-target5개. 전체 unrelated 회귀는 반복하지 않음|
|Save/Protocol|11/15 유지|신규 필드 없음. 기존 inventory ID/Selected/armor codec. Host의 기존 Tool/Held index+Started/Sequence 재사용. 저장 중 공격 pose는 기존대로 transient|

초기 `edit-sidegrade.xml`의 실패를 숨기거나 통과로 바꾸지 않았다. `edit-sink-correction.xml`과 최종 aggregate를 함께 본다. 자동 Play의 [TSV](headless-play.tsv)와 실제 MCP [TSV](live-mcp.tsv)는 별도다. headless는 사진을 만들지 않는다.

### Unity MCP 실제 Play · 1920×1080

SP 실제 World/Workbench II에서 한국어 recipe/비용/제작 카드와 영어 명칭·효과, inventory icon, 선택/캐릭터 held, 실제 Primary 공격을 확인했다. WB I에서는 거절되고, WBII에서 IronSpear1/Bone6/Leather2가 정확히 소비되어 Hunter1개가 나왔다. 초기 전체 목록 capture에는 상세 선택만 있고 카드가 scroll 밖이어서, 실제 무기 카테고리+제작 가능 필터로 카드/비용이 보이는 최종 capture를 추가했다.

|실제 검수|관측|사진|
|---|---|---|
|Recipe/비용/효과|Iron1/Bone6/Leather2,피해27/초당공격1.11/사거리3.35. 한국어 카드/비용,영어 명칭·효과(제작 후 필터가 비어짐)|[한국어](captures/11-recipe-costs-ko.png),[영어 제작 후](captures/12-crafted-en.png)|
|가방·장착|compact icon과별도긴shaft,선택Tool일치|[가방](captures/03-inventory.png)|
|거리 차이|표면3.15에서Iron/Steel0, Hunter27;2.5에서Steel35/Diamond42|[Hunter impact](captures/04-hunter-impact.png)|
|판정·시각|3.34hit/3.42miss, impact 촉끝 offset(0,3.39),판정3.35와약.04 차이. 좁은Arc밖거절|[촉끝](captures/04-hunter-impact.png)|
|벽·impact재검증|벽뒤0,벽제거후27;공격선택후벽생성해도impact0|[벽 차단](captures/05-wall-block.png)|
|여러적·중복hurtbox|Health당27한번,두번째적무피해,늦은추가피해0,같은프레임Primary거절|[단일 대상](captures/06-single-target.png)|
|선택해제·Save/Load|기존Save11에서선택된ItemID/Tool복원,빈hotbar선택시held없음|[로드](captures/07-loaded.png),[해제](captures/08-unequipped.png)|
|Vest 경계|Vest1.15,Trail과1.25;추가MCP에서Endurance1선행+Recovery1+Vest+Trail=1.40(22.4/s),복원/해제후1.00|변경없는기존효과수치확인,체감판정아님|
|실제Local Host/reHost|Host제작/27피해/held·attack Tool index·target HP복제,stale request거절,저장후재Host에서선택장비·소비비용복원|[Host](captures/09-host-impact.png),[재Host](captures/10-rehost.png)|

SP/Host fixture의 enemy/재료/시설 배치는 격리 QA 편의를 위한 것이다. 자연 파밍 실측이나 실제 플레이 속도로 보고하지 않는다. tool에는 code/MCP만 사용했으며 Windows 입력/Unity Focus는 사용하지 않았다. 종료 시 Play OFF,MainMenu,Network Session Idle,격리profile OFF다. 원본 UserData2개는 [전](user-save-before.json)/[후](user-save-after.json) SHA256와mtime_ns가 모두 동일하다.

### Multiplayer 권위와 호환 범위

Crafting은 기존 Host station/tier/cost/가방 atomic exchange,선택은기존NetworkInventory,Primary는owner RPC→Host PlayerPrimaryAction/PlayerCombat→Health다. Client는Tool data를보내지않고기존catalogindex로표현만한다. AttackState/Held codec왕복은자동검사했고실제Local Host에서동일index와피해frame을관측했다. 기존15버전peer라도콘텐츠가다른예전빌드에새127번째index를표시할수있다는뜻은아니다. 협동은같은콘텐츠빌드끼리사용한다. 새network필드가없으므로Protocol은15로유지했다.

### 미검증 / 판정

- 실제 원격2~4인/Relay,Android/터치/성능,모든방향·회전중촉끝의사용자체감,장시간자연파밍과4P경쟁은 미검증이다.
- 기존모든창의아트/모션을재설계하지않았으며새Hunter의실제impact거리·시각일치를검수했다. Runtime공통LOS변경은기존근접/채굴관련최소회귀로검사했다.
- **A (이번 범위의 기술·콘텐츠 판정)**: 거리/피해/속도/Arc의 실재 선택, Iron 체인 보존, Bone 반복+일회/기존 Chitin 반복+Vest 선택 유지. Head 보류가 허용된 조건이며 불필요한 장비를 채우지 않았다. 재미·손맛을 합격 판정하지 않는다.
- 다음 작업은2-D Fishing / Aquatic Content Expansion이며 이번에 시작하지 않았다. 시작/단계별시간·공유계정사용량은 [USAGE_AND_TIMING](USAGE_AND_TIMING.md). 최종 commit/remote SHA와clean확인은최종보고에기록한다.
