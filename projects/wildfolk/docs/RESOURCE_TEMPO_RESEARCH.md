# 출시 게임의 자원 획득 구조 조사

2026-10-18, 코드 변경 전 검색. 개발사 자료와 게임별 wiki에 기재된 플레이 규칙을 구분했다. 타 게임의 현재 평균 progression 시간을 직접 플레이로 측정하지 않았고, unknown multiplayer yield scaling을 있다고 가정하지 않았다. 아래 발견당 장비 수는 명시된 수치의 단순 산술이며 실제 운반/위험/가공 시간은 제외한다.

|게임|발견·보상 / 제작 부담|고갈·대체 공급|협동·초중반 원칙 / Lowfire 적용|
|---|---|---|---|
|Core Keeper|초기에는 ore가 든 벽을 채굴한다. 이후 boulder 하나는1800ore를 드릴로 장시간 추출한다. Iron Sword는 Iron Bar8이므로1800/8=225자루 상당의 철을 담지만 금·가공·해금과 긴 추출 시간이 별도다.|boulder는 유한하며 보통 pickaxe로 캘 수 없다. 새 지역 탐험과 나중의 추출 장비가 공급 규모를 바꾼다.|협동이 공유 대형 발견의 보상을 나눌 수 있는 구조다. Lowfire는 땅을 파지 않으므로1800을 복제하지 않고 '발견 하나의 보상 증가'만 적용. 자동화는 추가하지 않음.|
|Terraria|ore block 여러 개가 모인 vein을 발굴한다. Iron Bar는 Ore3, 현행 desktop/console/mobile Iron armor set은60bar라180ore. vein 크기가 달라 발견 지점 수를 단일 숫자로 환산하지 않았다.|원래 vein의 타이머 재생과 별개로 fishing crate가 광석/주괴의 대체 공급이다. 버전·crate 종류로 대상 금속이 다르다.|개별 장비 비용과 공유 시설을 나누고 고갈 이후 별도 경로가 존재한다. Lowfire는 fishing을 ore lottery로 바꾸지 않고 지역 recovery로 장기 save 공급을 보장.|
|Valheim|커다란 Copper deposit의 세그먼트를 실제 지형 아래까지 캐며, wiki 예시의90% 채굴 평균은 Copper117. Bronze Axe8bronze에 Copper16+Tin8이므로 구리만 보면 한 deposit이 여러 사람 장비 재료가 된다. Tin은 별도 수변 탐색이 필요.|금속 deposit은 비재생이다. 큰 초기 매장량과 다른 지역 탐험이 전제다. Lowfire처럼3개씩 주는 고정 노드와 동등하지 않다.|공유 작업대와 개인 장비를 구분. 큰 광상을 파는 대가인 장거리 운반·지형 굴착을 Lowfire에 그대로 요구하지 않는다. 전문 재료의 환경 정체성만 유지.|
|Don't Starve Together|보통 Boulder 하나에서 Rocks3+Flint+Nitre 등 복수 결과. 광물 장비 tier를 Lowfire와 일대일로 환산하기 어렵다.|Petrified Tree는 재생 가능한 돌/부싯돌/초석 대체 경로다. 초기 petrification44–65일, 이후21–32일이라는 wiki 수치는 DST의 긴 day 단위이고120초 회복의 근거로 복제하지 않는다.|top-down 고정 자원이라도 최초 고갈과 장기 공급을 분리한다. Lowfire도 기존 source family/지역에만 recovery를 허용하고 기지 앞 고정 좌표 반복 respawn을 제거.|
|Minecraft|Mojang1.18은 지하 큰 Iron/Copper ore vein과 고도/biome에 따른 분포를 명시한다. Dripstone Caves의 더 큰 copper seams처럼 환경 발견이 보상 규모와 연결된다.|철은 제련 외에도 구조물/loot/일부 생물에서 나온다. 공식 Iron Ingot 소개는 복수 공급 경로를 설명한다.|넓은 탐험과 큰 발견이 풍부함을 뒷받침한다. Lowfire는 자유 굴착이 없으므로 원재료당 방문/채집 부담을 줄이고 기존 Skeleton Iron loot과 Cave 공급을 보존.|

한 지점에서 몇 개가 나오는지뿐 아니라 **장비 한 세트를 위해 몇 번 찾고, 몇 번 채굴하고, 다음 공급을 어떻게 받는가**를 함께 비교했다. 자료로 보장되지 않는 '상용 게임은 모두N분 안에 철 장비 완료' 같은 숫자를 만들지 않았다. 동일한2–4인 인원 배율이 자동 적용된다고 가정하지 않았다.

## 출처

- [Core Keeper Mining / boulder 구조](https://corekeeper.atma.gg/en/Mining), [Boulder1800 설명](https://corekeeper.atma.gg/en/Ore_Boulders), [Iron Bar crafting material](https://corekeeper.atma.gg/en/Iron_Bar). 게임별 community wiki의 명시 수치다.
- [Terraria Crates](https://terraria.wiki.gg/wiki/Crate), [Iron armor의 현행60bar와 legacy75bar 구분](https://terraria.wiki.gg/de/wiki/Eisenr%C3%BCstung), [Iron Bar](https://terraria.wiki.gg/wiki/Iron_Bar). 일부 페이지는 본문 fetch403이어서 검색 인덱스의 명시 구절까지 확인했다.
- [Valheim Copper Deposit](https://valheim.gamecore.wiki/en/locations/copper-deposit/), [Bronze Axe](https://valheim.fandom.com/wiki/Bronze_axe), [Bronze 조합](https://valheim.fandom.com/fr/wiki/Bronze). community wiki 기준이며 패치별 세부 수치 차이는 Lowfire 설정 근거로 직접 사용하지 않았다.
- [DST Boulder](https://dontstarve.wiki.gg/wiki/Boulder), [Petrified Tree](https://dontstarve.wiki.gg/wiki/Petrified_Tree). 개발사 수치 보증으로 표현하지 않았다.
- [Mojang 공식1.18](https://feedback.minecraft.net/hc/en-us/articles/4415128577293-Minecraft-Java-Edition-1-18), [공식 Dripstone Caves](https://www.minecraft.net/en-us/article/around-block--dripstone-caves), [공식 Iron Ingot](https://www.minecraft.net/en-us/article/taking-inventory--iron-ignot).

Lowfire의 최종75/105/120/150/165초는 위 게임에서 복사한 값이 아니다. 실제 recipe/source 감사,4인 노드 수, 제한된 지역 보충과 실제 Unity 채집 결과를 근거로 정한 초기 후보이며 장시간 체감은 후속 사람 테스트 대상이다.
