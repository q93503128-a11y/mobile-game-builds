# 음식·낚시·재사용 물

2026-10-20. Hunger drain0.08/s, Thirst0.12/s와 Raw Water 불이익을 유지한다. 음식/연고는 즉시 HP 또는 Stamina도 회복할 수 있으며 timed buff stack은 없다. 새 회복품과 Mixed Stew는 공용4초 소비 간격이다. Hunger/Thirst가 가득 차도 해당 HP/Stamina가 부족하면 사용한다. Host 권위·alive/Running·실제 stack 소모 후 적용하며 실패·죽음·cooldown에는 소비하지 않는다.

|Item ID|Hunger|Thirst|즉시 HP|즉시 Stamina|
|---|---:|---:|---:|---:|
|clean_water_cup|0|38|0|0|
|cooked_meat|35|0|0|0|
|raw_meat|8|0|0|0|
|raw_water_cup|0|15|0|0|
|berry|8|3|0|0|
|mushroom|5|0|0|0|
|raw_fish|6|0|0|0|
|roasted_mushroom|16|0|0|0|
|cooked_fish|26|0|0|0|
|mushroom_stew|35|10|0|0|
|meat_stew|48|12|0|0|
|fish_stew|46|15|0|0|
|mixed_stew|65|18|8|0|
|healing_salve|0|0|30|0|
|bone_broth|24|12|18|0|
|restorative_broth|22|10|0|35|
|trail_ration|25|0|0|20|

Salve는 현장 Hand, Trail Ration은 Campfire7초, Bone/Restorative Broth는 Pot8초다. 기존 Stew의 Hunger 우위·Cooked Meat의 단순 비용을 유지하고 HP/Stamina 지원을 선택하게 한다. 전체 비용은 CONTENT_EXPANSION을 따른다.

기존 Cooked Meat와 Cup 정수는 유지한다. Cooking Pot은 독립 설치물이며 기존 sequential 처리기를 재사용한다. 입력을 예약하고 완료 결과는 Take로 수령하며 full bag에서는 cargo를 보존한다. 파괴/휴면/Save/reHost는 기존 처리 상태 규칙과 같다.

Fishing Rod는 WB I Wood4+Fiber3. 바라보는 방향1.5~5unit의 유효 자연 수면에서 cast.45s→3초 시점 입질→2.2초 입력 창→Raw Fish1이다. 성공 후.6초 회수한다. 이동1.2unit 초과/도구 변경/사망/모달은 취소한다. 실패/가방 포화는 보상을 생성하지 않으며 입질 창이 남으면 다시 입력할 수 있다. 보상 성공은 phase를 바꾸어 중복 요청을 거절한다. Host 권위이며 client는 line/bobber/입질을 시간 표본으로 표시한다.

Waterskin은 WB I Leather2+Fiber3, 한 item/3회분/stack1이다. 빈 상태 Primary는 채우기, 채운 상태 Primary는 마시기, Interact는 근처 자연 물에서 보충이다. 정수된 가득 찬 용기를 Raw로 덮지 않는다. 부분 충전/정수 용기를 보충하면 Raw3/3이 되므로 다시 정수해야 한다. Campfire의 Purify Waterskin은 Raw3/3만10초 처리해 Clean3/3으로 돌려준다. 각 drink는 기존 Cup의 Raw15 또는 Clean38 회복/불이익을 사용하며.5초 입력 간격을 둔다. 한 용기의 서로 다른 charge/quality가 합쳐지지 않는다.

2026-10-19 교정: remote rod 표현은 owner-only inventory 대신 replicated PlayerEquipment.SelectedItem을 따른다. 취소/만료/회수 종료는 기존 ActivityKind.None을 publish하며 다음 입질 안내 상태를 초기화한다. 새 protocol 필드나 낚시 시간/보상 변경은 없다.

## Medicine2-A와 음식 경계

Salve는 음식 분류와 구분되는 Medicine이다. 응급 붕대는 즉시0/총20HP를8초간 회복하며 Fiber2+Herb1, Salve는Gland1+Herb1+Fiber2로 즉시30HP다. 두 약품과 기존 회복 음식의 입력 제한은 공용4초다. 붕대 효과가 남으면 추가 붕대는 소비하지 않는다. 설명/아이콘과 작은 잔여 치료 표시를 제공한다. 음식 최대 즉시 회복은 Bone Broth18로 Salve보다 낮아 음식 수치를 변경하지 않았다. 음식 전체 확장은 다음2-B에서 판단한다.
