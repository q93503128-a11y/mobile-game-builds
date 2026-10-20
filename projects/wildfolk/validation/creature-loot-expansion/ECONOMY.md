# 제작 경제 비교 · 실제 에셋 계산

2026-10-20. 아래는 실제 Recipe/ProgressionEconomy 결과와 명시적 선택 시나리오의 산술이다. 실제 획득/사냥 시간·분당 수율·체감 밸런스를 측정한 결과가 아니다. 배치 결과 잔량과 공유 시설1회/개인1·4회 구분은 economy.json을 따른다.

## 기존 진행·선택 방어기지 전후

Diamond 장비6개/공유 II→III 포함 필수 Diamond13/49, 필수 Crystal10/22는 그대로다. 새 raw3종은 필수 진행에0개다. 아래 선택 방어기지는 기존 metadata의 Ballista1/Repeater1/Beacon1/모듈별2/전용탄60/기본 Bolt120과 기존 선택 root를 포함한다.

|Raw|변경 전1P|현재1P|변경 전4P|현재4P|
|---|---:|---:|---:|---:|
|chitin|0|4|0|4|
|coal|121|121|226|226|
|copper_ore|40|40|68|68|
|crystal|16|16|28|28|
|diamond|13|13|49|49|
|feather|14|14|26|26|
|iron_ore|83|83|197|197|
|leather|17|15|50|48|
|plant_fiber|6|6|24|24|
|stone|98|98|149|149|
|tin_ore|19|19|30|30|
|wood|188|188|332|332|

변경은 공유 Frame2개의 Leather를 각각1 줄이고 Chitin2를 더한 부분뿐이다. Bronze/Steel/Part 제작량은57/58/14(1P),90/118/14(4P)로 유지하며 raw에 중복 합산하지 않는다. 모든 플레이어가 이 방어기지를 만들어야 한다는 뜻이 아니다.

## 새 선택 kit

개인별 Bone Arrow96 + Barbed Bolt64 + Vest1 + Salve4 + Bone Broth2 + Restorative Broth2 + Trail Ration2, 공유 Bone Trap4를 명시적으로 선택했다. 아래는 이 kit만의 추가 raw이며 위 방어기지와 합칠 때 중복 구조물을 추가하지 않는다.

|추가 Raw|1P|4P|
|---|---:|---:|
|berry|8|32|
|bone|28|88|
|chitin|12|48|
|feather|16|64|
|leather|1|4|
|medicinal_gland|6|24|
|mushroom|6|24|
|plant_fiber|10|40|
|raw_meat|4|16|
|wood|36|108|

## 반복 비용과 공급

- Bone Arrow12발마다 Bone2+Wood1+Feather1. 기존 Arrow12발의 Wood2+Feather1과 사냥 재료/목재 절약을 교환한다. 피해+3이며 Bronze+6보다 낮다.
- Barbed Bolt8발마다 Chitin1+Wood2+Feather1. 쇠뇌의 선택 탄이며 Steel Bolt+10보다 낮은+6. Repeater는 기존 지정 기본 Bolt를 계속 사용한다.
- Salve1회 Gland1+Fiber2→HP25, Bone Broth1회 Bone2+Mushroom1+Raw Meat1→H24/T12/HP18, Restorative1회 Gland1+Mushroom1+Berry2→H22/T10/Stamina35. 모두4초 소비 간격이며 효과는 즉시다.
- Chicken은 Feather1+Raw1 확정, Boar는 Leather1+Raw2 확정을 유지한다. Crow는 Feather1 75%/Raw1 25%이므로 같은 처치당 확정 닭 보상을 대체하지 않는다. Feather 직접 Recipe sink는4→6, Leather15→16, Raw Meat3→5로 기존 재료 수요도 남는다. Wood45→48이며 upgrade 비용은 별도다.
- Skeleton Bone1–2 확정, Deer Bone1, Wolf/Goat Bone1–2, Giant Boar Bone2, Rabbit50%/Bat40%가 공급한다. Gland는 Leech1 확정/Bat1 75%, Chitin은 Stone Crawler1/Crystal Crawler2 확정이다. 서로 다른 habitat에 수요를 분산한다.
- 자연 Crystal 한 source는6개, 회복150초. Crawler1회 처치 기대값은0.15개이므로 **기대값으로40회 처치가6개**다. 결과 보장이나40회 소요시간 실측이 아니다. Diamond/Part는 신규 생물 drop0이고 기존 채굴/제작 gate는 유지한다.
- Camp1개의 기대 묶음: Bone Arrow12, Salve1.5, Bronze1.5, Leather1.5, Part0.2, Barbed Bolt2.8. 기본4묶음은 확정이고 선택2묶음은 독립 확률이다. 보급은 공유1회이며4인에게4번 생성하지 않는다. 재방문·주민 보충은 재추첨하지 않는다. 필수 progression이 아닌 비용 절약이다.

## Population과4인 한계

공유 활성 HardCap80 유지. 기존5종 cap46+신규8종 cap52=98은 동시에 보장하는 수가 아니라 habitat별 soft 목표다. 1P/4P 모두 같은 cap/회복 간격이며 사람 수로 전리품·Camp를 복제하지 않는다. 청크 quota는 WantedCount에 따라 분배한다. 회복 tick5초/최대32 ticket/최대2 group/각24후보이며 각 species의 지연과 chance가 추가 적용된다. 출현·사냥 시간은 지형·이동·전투에 의존하므로 분당 생산량을 단정하지 않는다.4인 분담/경쟁·장기 소비 감각과 실제 단말 성능은 사용자 테스트 대상이다.
