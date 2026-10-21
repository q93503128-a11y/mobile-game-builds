# Content Expansion 2-E · Exploration / Utility

## 구현 전 감사와 결정 · 2026-10-21

시작 fetch의 main/HEAD는 `65d2e9865fe971d9e4a9cdad42b1f27d051bf463`. Item132 / Recipe109 / Save11 / Protocol16. 이 절은 runtime 수정 전에 작성했다. 실제 자료는 Item/Recipe asset, PlayerNavigation/NavigationHud/NetworkNavigation, PlayerTelescope/WorldChunkPresentation, SignalBeacon, PlayerLightSource, PlayerWaterskin, PlayerFishing, ResourceDefinition/HarvestDefinition, PlayerSurvival, PlayerSnapshotCodec다.

### 기존 Utility inventory

모든 물품 stack/선택·장비는 기존 Save11 codec과 Host inventory 권위를 사용한다. 아래 영속 여부는 추가 효과의 상태다. WB는 Workbench.

|Item / system|Tier / 실제 Recipe|점유|사용 / 실제 효과|Host / Client|Save|중복|실제 문제|장거리 가치|
|---|---|---|---|---|---|---|---|---|
|Compass|WBI Wood2+Iron ingot1|1 slot, 소지만 필요|북쪽, Home, 동굴 출구, 선택 Beacon의 방향/거리. Automatic/Off/Home/CaveExit/Beacon|Home 설정 검증과 Beacon 목록 서버, HUD 로컬|Home stable ID·mode·선택 Beacon 저장|기본 navigation 담당|경로 자체를 계산하지 않음|높음, 이미 의미 있음|
|Telescope|WBIII Crystal2+Bronze1+Leather1|1 slot, 손 선택|160m 앞 camera, terrain/decor/static POI presentation. 이동·공격 정지, 피격/교체 취소|서버 scope 상태, 소유자 presentation|viewing 저장 안 함|POI/지형, 광물 조사와 다름|resource/AI/loot/Camp는 보이지 않음|지형 정찰|
|Signal Beacon|WBIII Crystal2+Bronze1+Mechanical part1+Wood2|휴대 stack50, 설치 후 시설|여러 개 영속 설치, 번호 목록에서 하나 추적, 회수 가능|서버 배치/공유 directory·회수|stable ID·위치·번호·선택 저장|영구 목적지|즉석 협동 주목 요청에는 비용/설치·선택 절차 큼|장기 경로 기준|
|Torch|Hand Wood1+Coal1→4|stack50, 손 선택 또는 설치|반경7.5 조명, 설치거리3.5, HP40, 연료 없음|서버 배치, light presentation|설치물 영속|저가/고정 조명|손 사용 충돌, 먼 투척은 없음|초기/동굴 경로|
|Lantern|WBII Bronze2+Leather1+Coal2|1 slot, Utility 장비1|반경8.5, strength .9, 양손 자유|서버 equip, 로컬/동기 light|장비 저장|Torch의 휴대 편의|Utility 슬롯 경쟁|높음|
|Crystal Lantern|WBIII Lantern1+Crystal2+Bronze1|1 slot, Utility 장비1|반경11.475, 기존보다35% 넓음, 공격/퇴마 없음|동일|장비 저장|동일 슬롯 상위 조명|Crystal 비용|깊은 동굴|
|Prospecting Pan|WBII Copper ingot2+Wood2|1 slot, 손 선택|Panning 전용 Tin 퇴적층 물리 채취, damage25, range1.8, cooldown1.4, nodeHP50/drop5|서버 harvest/tool gate|도구 stack·자원 delta 저장|탐지가 아닌 채집|이름만 보고 radar로 오해 가능|Tin 공급에 필수|
|Waterskin|WBI Leather2+Fiber3; Campfire raw3/3→clean3/3,10s|1 slot/3 charge|자연 물 근처 채우기, raw/clean 마시기|서버 charge/품질/생존 상태|charge/quality 저장|병 생산 chain 불필요|빈 물통 refill 준비 필요|SP 생존, MP 자연 H/T drain은 현재 꺼짐|
|Food / Meal|Hand/Fire/Pot. Hunter Meat2+Bone1+Mushroom1+Berry1; Forager Herb1+Mushroom2+Berry1; Trail Fish1+Herb1+Berry2, Pot10s|각 stack50|nutrition, 세 Meal 중1개150s: HP+8/자연HP+.15/stamina recovery+10%,4s 공용 간격|서버 소비/효과, owner HUD|소비 결과만 저장, Meal transient|Medicine와 역할 분리|준비 선택 이미 있음|높음|
|Medicine|기존 Salve/Dressing, 기존 Herb/Gland 사용|각 stack50|Salve HP30 즉시, Dressing20/8s, 공용4s|서버 소비/회복|소비/실제HP만, 남은치료 transient|Meal과 다름|피격 중 사용 제약|높음|
|Sleep / Bed / Bedroll|없음|없음|플레이어 sleep 없음. chunk의 sleeping actor는 휴면 streaming|해당 없음|해당 없음|새 휴식은 시간/밤/멀티 정책 필요|현장 휴식 미해결|이 패스 도입 근거 부족|
|Map / waypoint / tracking|위 navigation + 정적 POI/cave graph|별도 map item 없음|전체 map/fog UI 없고 직선 bearing과 Telescope 정찰|기존 서버 beacon/home|위와 같음|새 permanent marker 불필요|동료 즉석 위치 신호 없음|기존 기능 활용|
|Inventory / storage|36칸: hotbar9+27, Chest24/Reinforced36|재료 stack50/장비1|무게/Backpack 없음|revision 검증 서버 거래|전체 stack 저장|용량 증가는 별도 progression|도구마다 한 칸 선택 비용 존재, 기술적 용량 결함 증거 없음|새 bag 보류|

### Pain point matrix

|범주|판정|증거 / 이번 대응|
|---|---|---|
|A 길 찾기|부분 해결|Compass Home/Exit/Beacon, 경로 계획은 플레이어. 유지|
|B 위치 표시|이미 해결됨|persistent Beacon 여러 개. 중복 설치물 배제|
|C 동료 위치 공유|부분 해결|Beacon은 공유되나 즉시 주목 신호 없음. 한 개의 짧은 신호 도입|
|D 자원 탐색|미해결|Pan은 Tin 채취, Telescope 자원 제외. 제한적 Ore Surveyor 도입|
|E POI 발견|부분 해결|Telescope static POI, 직접 이동 유지. 보상 변경 없음|
|F 동굴/야간 조명|이미 해결됨|Torch 고정/휴대, Lantern hands-free, Crystal 반경 개선. 추가0|
|G 장거리 생존|이미 해결됨|3charge 물통/정화, 회복/Meal/food 준비. MP 자연 H/T 재설계 안 함|
|H 귀환|부분 해결|Home/Exit bearing, teleport 없음. 큰 월드 귀로는 실제 비용이지만 위험/안전 목적지 정책 미정, Recall 보류|
|I 현장 휴식|미해결|Sleep 기반 없음. 밤·다인 동의·respawn 결정을 작은 utility로 끼워 넣지 않음|
|J Inventory 부담|부분 해결|36칸과 stack50, 장비 슬롯 tradeoff. 품목2개 선택 비용을 명시, bag 추가 안 함|

## 외부 자료 비교

2026-10-21 공개 wiki 본문과 검색 색인 본문 조사. wiki.gg 일부 직접 열기403은 검색 색인으로 보완했다. 설치 게임 직접 플레이를 한 것으로 간주하지 않는다. 숫자를 그대로 복사하지 않고 아래 실제 사례에서 적용 원칙을 분리했다.

|게임|해당 시스템|실제 사례 / 출처|장점|Lowfire와 차이|적용 여부|이유|
|---|---|---|---|---|---|---|
|Grounded|resource survey|[Resource Surveyor](https://grounded.wiki.gg/wiki/Resource_Surveyor): Hedge 스위치 해금, 발견한 Field Station/lab의 radar 영역에서 분석/발견 자원을 map square로 검색, creature 재료는 생물 위치로 표시|검색 대상과 탐색망 성장|Lowfire map/radar망 없음, chunk 미생성 정보 금지|탐색 보조만 채택|서버 현재 local mineral만, map 좌표·creature 검색·세계 radar 배제|
|Grounded|persistent marker|[Buoyant Marker](https://grounded.wiki.gg/wiki/Buoyant_Marker): 수중 설치 표지, icon/label/color와 거리 표시|장기 수중 경로 기준|수영 없고 기존 Beacon 있음|추가 배제|새 표지 시설은 중복|
|Terraria|ore/treasure highlight|[Spelunker](https://terraria.wiki.gg/wiki/Spelunker), [Spelunker Glowstick](https://terraria.wiki.gg/wiki/Spelunker_Glowstick): potion의 광물/보물 강조, glowstick의 같은 색 glimmer는 가치 구분 한계|수색 시간 감소, 표현 강도 선택|Lowfire 희소 Diamond/Crystal 발견을 보존해야 함|coarse bearing만 채택|벽 너머 정확한 sprite/좌표 reveal 배제|
|Terraria|return/preparation|[Recall Potion](https://terraria.wiki.gg/wiki/Recall_Potion), [Magic Mirror](https://terraria.wiki.gg/wiki/Magic_Mirror): 소비형/재사용형 모두 spawn 복귀|장거리 왕복 비용 완화|Lowfire Home은 Workbench 참조, 안전 spawn/전투 제한과 별개|보류|무료 무한 탈출/안전하지 않은 bench destination 도입 안 함|
|Terraria|temporary light|[Glowstick](https://terraria.wiki.gg/wiki/Glowstick): 손/투척, 수중,5분 만료|앞쪽·물속 임시 확인|Lowfire 수중 플레이 없음, 연료 없는 설치 Torch와 넓은 Lantern|추가 배제|단일 틈을 위해 projectile 추가할 가치 부족|
|Valheim|proximity/slot|[Wishbone](https://valheim.fandom.com/wiki/Wishbone): Bonemass 후 buried silver/muddy scrap/treasure에 접근하면 빛·소리 빈도 증가, Megingjord/Wisplight와 장비 슬롯 경쟁|정확한 점 대신 근접 힌트, 장비 선택|Lowfire boss unlock/무게 장비 없음|정지·손 선택 tradeoff 채택|새 utility equip slot 만들지 않음|
|Valheim|map share|[Cartography table](https://valheim.fandom.com/wiki/Cartography_table): 기록/읽기 단계로 발견 지형·pin 공유, 자신의/공유 pin 구분|발견 정보 소유와 갱신 명확|Lowfire 전체 map 없음, Beacon directory만 공유|map table 배제|전체 map 확장과 새 station은 범위 밖|
|Valheim|temporary ping|[Wiki 커뮤니티 사례](https://valheim.fandom.com/f/p/4400000000000056989): map middle-click이 잠깐 world/map 주목 표시|지속 pin과 즉시 주목을 분리|모바일 Lowfire map/middle-click 없음|현재 위치의 짧은 신호 채택|채팅·지도 전체 개편 없이 기존 Primary/HUD 사용|
|Core Keeper|return|[Recall Idol](https://corekeeper.atma.gg/en/Recall_Idol): 소비형 channel로 The Core 복귀, Larva statue 제작과 loot 획득|목적지 명확, 준비 비용 존재|Lowfire The Core 같은 보장된 중심 거점 없음|보류|소비형도 목적지/위험 정책 선행 필요|
|Core Keeper|MP map|[Cartography Table](https://corekeeper.atma.gg/en/Cartography_Table): Iron Workbench, Plank40+Iron10, 다른 탐험가와 map 발견 공유|협동 탐험 축적|Lowfire map 없음, Beacon은 현재 목적지만|station 추가 배제|utility 때문에 지도 전체·새 station 안 만듦|
|DST|compass/map share|[Map/DST](https://dontstarve.wiki.gg/wiki/Map/DST), [Map Scroll](https://dontstarve.wiki.gg/wiki/Map_Scroll), [Cartographer desk](https://dontstarve.wiki.gg/wiki/Cartographer%27s_Desk): 손 Compass로 다른 플레이어에게 위치 보임, scroll은 작성 시점 발견 지형을1회 공유, surface/cave 구분|장비 tradeoff/정적과 live 정보 구분|Lowfire Compass 이미 Home/Exit용, live 추적 없음|상시 추적 배제, 명시적 신호만|소지 Compass에 자동 글로벌 동료 추적을 붙이지 않음|
|DST|rest/prep|[Straw Roll](https://dontstarve.wiki.gg/wiki/Straw_Roll), [Tent Roll](https://dontstarve.wiki.gg/wiki/Tent_Roll): 잠은 Hunger 비용, Walter의10회 Camper Tent는 HP/sanity 회복과 hunger 비용|쉬는 것도 자원 선택|Lowfire sanity/개인 수면·밤 합의 없음|Bedroll 보류|새밤 회피/무료 치료/respawn을 utility로 만들지 않음|
|Necesse|survey/prep|[Spelunker Potion](https://necessewiki.com/Spelunker_Potion): ore/Caveling10분 강조, Void Shard2+Gold1+Glass bottle와 Void Alchemy Table|탐험 출발 전 소비품 준비|Lowfire 금/유리/연금술 chain 없음|reusable 짧은 survey 채택|공급 chain과 지속 reveal 배제|
|Necesse|return/prep|[Recall Scroll](https://necessewiki.com/Recall_Scroll), [First Cave](https://necessewiki.com/Guide:Getting_Started/Your_First_Cave): 지상/지하에서 respawn 복귀, cave loot/상인 공급, bed가 respawn 기준|하강 전에 귀환 준비 가능|Lowfire bed/merchant 없음, POI 보상은2-F|보류|기반 없는 scroll/loot loop 복제 안 함|

### 후보별 결정 · 구현 계약

|후보|외부 사례→현재 문제→가능성|중복 / 대안|결정·구체적 제약|
|---|---|---|---|
|Ore Surveyor|Grounded/Terraria/Valheim/Necesse→local 광물 수색 도구 없음→기존 collider/streaming/tool gate 사용 가능|Pan은 채취, Telescope는 terrain. 전역 radar/10분 wall highlight/새 potion chain 배제|WBII Wood2+Bronze2+Leather1→1,stack1. 손 선택, 정지1.2s channel, 이동/피격/사망/modal/교체 취소. 시작부터12s cooldown, 결과6s, 반경18. 현재 active chunk의 살아 있는 Mineral 중 보유 채취 도구로 캘 수 있고 terrain/시설 벽으로 막히지 않은 가장 가까운 후보를8방향+광물명만 표시. 좌표/거리/정확한 강조/아이템 지급 없음. query는 완료 시1회, fixed128 collider/32 ray buffer. saturation은 모호한 결과, 없는 것으로 단정 금지|
|Signal Whistle|Valheim 짧은 ping/DST 명시적 위치 공유→지금 여기 주목 요청 없음→기존 Primary와 nav arrow 재사용|영구 표지는 Beacon, 새 Beacon II 배제|WBI Wood2+Fiber2→1,stack1. 현재 위치20s,30s cooldown. sender당1개/4P최대4개. 같은 surface/cave region,60m 안에서만 표시, receiver HUD는 가장 최근 유효한 타인1개. 자신의 성공 피드백은 짧게 표시. Host 위치/시각만, expiry/despawn 즉시 제거. 재접속·Load·reHost에 남지 않음. AI aggro/수집/폭발/투사체 없음|
|추가 light|Terraria 임시 투척→앞쪽 light 틈→기존 Torch 배치3.5와 손 자유 Lantern으로 충분|Lantern/Crystal/Torch 중복|0개, 원거리 투척만을 위한 framework 만들지 않음|
|Recall|Terraria/Core Keeper/Necesse→큰 월드 왕복 부담은 있음→명확한 안전 목적지와 전투 정책이 아직 없음|Compass는 길 안내, teleport와 다르나 새 위험 정책 필요|보류. 밤/전투/POI 보상 회피를 작은 편의로 도입하지 않음|
|Bedroll|DST→현장 회복 질문→Lowfire Sleep 기반 자체 없음|Medicine/Meal 회복과 중복, 향후 base/night 침범|보류. respawn/storage/밤 skip 추가 없음|
|Backpack|장비 선택/stack 준비→36칸 부담→기술적 병목 증거 없음|inventory revision/save/skill 범위 확장|추가0, 각 새 도구가1칸을 요구하는 선택 유지|
|Compass/Pan 개선|기존 source 감사→이미 명확한 navigation/Tin 채취|새 기능 억지 추가는 중복|기능 변경0|

### 경제·경계 사전 판정

2 item /2 recipe, 신규 raw0/station0. 예상134/111, WBI36/WBII25/WBIII11. Surveyor 개인1개: Wood2 Bronze2 Leather1;4인 각각=Wood8 Bronze8 Leather4. Whistle 개인1개:Wood2 Fiber2;4인=Wood8 Fiber8. 모두 reusable, 반복 Wood/Fiber/탄약 소비0. 두 개 전체를4인이 휴대하면 Wood16 Bronze8 Leather4 Fiber8와 각2칸. 팀1개씩 공유하면 Wood4 Bronze2 Leather1 Fiber2. Crystal/Mechanical Part/Steel 소비0으로 WBIII Beacon/Telescope/Lantern/Turret 진입 자원 경쟁을 추가하지 않는다. Bronze2는 Lantern과 명시적인 중기 선택 비용이다. 기존 물통/음식/약 준비 비용 유지.

Host가 실제 사용·cooldown·query·결과·신호 위치/만료를 결정하고 Client는 기존 Primary owner request/sequence와 복제 상태만 사용한다. Survey 상태는 owner만, 신호는 명시적으로 공유된 위치만 복제하며 원격 미생성 resource 정보 없음. marker 최대4, HUD1, survey local fixed buffer. Save11 유지: item ID만 저장, channel/결과/ping/cooldown 저장하지 않음. 새 NetworkBehaviour/state 때문에 Protocol17로 증가. 실사용 재미 최종 판정은 사용자 QA다.

## 실행 결과

### 실제 구현과 제한

계약대로2Item/2Recipe를 append해 기존 Item0~131/Recipe0~108 순서를 보존했다.134/111,Save11/Protocol17. 기존 Utility의 gameplay 효과 변경0, Crafting/Inventory 설명과 기존 nav HUD만 확장했다. Survey 완료 결과와 Whistle은 저장하지 않는다. 채널 시작의 cooldown은 취소 후에도 남는다. Selected Survey 안내→최근 유효한 동료 신호1개→기존 Compass 목적지 순서이며, 신호 수신에는 Compass를 강제하지 않는다. 자신의 성공 신호는2초 피드백이다. 새 Utility 장비 슬롯·Sound/투사체·AI 효과는 없다.

**검수 중 필요한 최소 네트워크 교정:** 최초 별도 Client 시도는 신규 사용 이전 초기 world sync에서 실패했다. queue128→4096의 QA 전용 재시도도 실패했다. 이후 승인/연결/상태 로그에서 Client가 approved/connected/Playing 후 remote closure를 받았고, 실제 씬 초기화6초 이상에 비해 정본 transport inactivity가4000ms임을 확인했다. `NetworkSession.prefab`과 `MultiplayerSceneBuilder`의 동일 필드만 설치된 Unity Transport의 기본30000ms로 복구했다. Join12초/승인/20Hz/queue128은 그대로다. QA queue 확장은 제거했다. **최종 실제2P는 정본30000ms/queue128에서 통과했다.** 초기 실패를 성공으로 덮어쓰지 않았고 XML/diagnostic 로그를 남겼다. 기능과 관련 없는 네트워크 재설계는 하지 않았다.

### 자동 검사와 실제 MCP의 구분

|범위|결과 / 증거|한계|
|---|---|---|
|초기 EditMode43개|42pass, 새 utility를 기존 전투 tool로 가정한 SceneContract1fail. 비전투 utility 분류 조건을 교정한 재검사1/1pass|초기 실패 XML 유지; 고유 테스트별 최신 결과로 집계|
|최종 관련 EditMode|edit-final.xml 참조: Utility 수치/1P4P재료·stack ID·양 언어·8방향·좌표 미직렬화·Survey/Signal roundtrip·영역·transport 설정 및 접속/Home 계약|전체 EditMode 반복 아님|
|SP PlayMode1|play-sp.xml pass: 실제 prefab/terrain, card 제작, 기존8Utility, local/empty/tool gate/wall/cave/chunk boundary/ungenerated, 각취소,12초재사용,Save11 Apply/죽음|고정 seed9472026과 제어된 QA actor/자원 배치|
|Host PlayMode1|play-host.xml pass: 권위/중복serial, owner 결과,4개 logical sender의spam/overlap/range/cave/expiry/despawn,Save/reHost|4개 sender는 동일Host 내 logical actor. 원격4P가 아님|
|실제 별도프로세스2P|play-client-30s.xml pass + live.tsv: live Unity Host와 headless Unity Client LAN, 양방향 signal/Client Primary survey/Client query0/Host query1/owner 결과/중복요청 미연장/실제disconnect HUD제거|Relay/다른 PC/4기기/지연·손실/Client화면 검수 아님|
|Unity MCP1920×1080|기존 Compass/Telescope/Beacon/Torch/Lantern/CrystalLantern/Pan/Waterskin 직접실행. 새2개 제작/손선택/효과/HUD/cancel/cooldown/Host/reHost. 실제 SceneFlow Load 재로드에서도2item각1/CrystalLantern보존·SurveyNone/Signal0 확인|실제 사용자 save는 쓰지 않았음|
|추가 MCP buffer/death|130개 QA collider에서128까지만 검사, Crowded1query/0.2763ms. 실제 죽음에서Cancelled/재사용false|Editor 수치이며 Android frame budget 보장 아님|
|Save호환|기존 Save11 snapshot과 기존 ID 순서를 유지, 새stack capture/restore, 실제파일Reload/HostSave/reHost|Android강제종료/Cloud/hostmigration 별도|

### 조회 성능

runtime은 channel 완료에만 OverlapCircle을 호출한다. live completion마다 QueryCount가 정확히1 증가했고 active/pending/presentation chunk 수는 전후 동일했다. wall과 terrain 경로 검사는 제한된 후보에만 수행한다. query_ms 원자료는 live.tsv, headless.tsv. 측정은 wall-clock Editor 실행이며 기기 최악값/60fps로 환산하지 않는다. 버퍼 포화는 Crowded로 알리고 ore 없음으로 단정하지 않는다. client의 QueryCount는 실제 LAN 검수에서0이었다.

### 사용자 QA / 미검증 / 판정

- 미검증: Android 실제 조작/가독성·성능,4기기,Relay,장거리 지연/손실,headless Client의1920화면. 실제2P LAN과4 logical sender를4remote 검수로 부르지 않는다.
- 사용자 QA: 조사기1칸+채취도구1칸과 Bronze2 비용을 실제 원정에서 지불할 가치가 있는지,18unit/정지1.2s/12s대기가 탐색을 보조하는지,20초/60m신호가 회합에 충분한지 확인한다. 재미는 Codex가 최종 판정하지 않는다.
- 기술 판정 **A**: 자원 수색/즉시 협동 위치라는 서로 다른 두 빈 역할만 추가했고 기존 navigation/정찰/영구표지/채취/조명을 유지했다. Recall/Bedroll/bag/새raw/Station/POI보상은 추가하지 않았다. 신규 utility 관련 자동·SP·Host·실제2P 흐름 통과. 다음2-F는 이 작업에서 시작하지 않는다.


### 최종 집계

고유 테스트별 가장 최근 실행 기준 EditMode 49/49, PlayMode 3/3 통과. 전체 실행별 성공/실패 XML과 최신 집계는 RESULTS.json을 따른다. 최종 LAN 반복은 play-client-hud.xml이며 HUD tick(.15s) 뒤 실제 화살표/문구 일치를 추가 검증했다. GUID/source validator1348 통과, 실제 data validator0. live query 7개 표본 0.0167–1.81ms, saturation128개0.2763ms. 사용자 원본 save2개 hash/mtime 전후 동일. Editor는 MainMenu/Edit mode로 복귀하고 IsolatedQaProfile을 종료했다.
