# Food / Fishing / Reusable Water

현재 음식/낚시는 [콘텐츠 정본](CONTENT_EXPANSION.md)을 따른다. Hunger drain.08/s, Thirst.12/s와 기존 Raw Water 불이익을 유지한다. 음식은 timed combat buff 없이 기본 Hunger/Thirst를 회복한다.

|음식|Hunger|Thirst|공급/시설|
|---|---:|---:|---|
|Berry|8|3|renewable bush|
|Mushroom|5|0|renewable source|
|Raw Fish|6|0|낚시|
|Roasted Mushroom|16|0|Campfire5s|
|Cooked Fish|26|0|Campfire5s|
|Mushroom Stew|35|10|Cooking Pot7s|
|Meat Stew|48|12|Cooking Pot8s|
|Fish Stew|46|15|Cooking Pot8s|
|Mixed Stew|65|18|Cooking Pot10s|

기존 Cooked Meat와 Cup 정수는 유지한다. Cooking Pot은 독립 설치물이며 기존 sequential 처리기를 재사용한다. 입력을 예약하고 완료 결과는 Take로 수령하며 full bag에서는 cargo를 보존한다. 파괴/휴면/Save/reHost는 기존 처리 상태 규칙과 같다.

Fishing Rod는 WB I Wood4+Fiber3. 바라보는 방향1.5~5unit의 유효 자연 수면에서 cast.45s→3초 시점 입질→2.2초 입력 창→Raw Fish1이다. 성공 후.6초 회수한다. 이동1.2unit 초과/도구 변경/사망/모달은 취소한다. 실패/가방 포화는 보상을 생성하지 않으며 입질 창이 남으면 다시 입력할 수 있다. 보상 성공은 phase를 바꾸어 중복 요청을 거절한다. Host 권위이며 client는 line/bobber/입질을 시간 표본으로 표시한다.

Waterskin은 WB I Leather2+Fiber3, 한 item/3회분/stack1이다. 빈 상태 Primary는 채우기, 채운 상태 Primary는 마시기, Interact는 근처 자연 물에서 보충이다. 정수된 가득 찬 용기를 Raw로 덮지 않는다. 부분 충전/정수 용기를 보충하면 Raw3/3이 되므로 다시 정수해야 한다. Campfire의 Purify Waterskin은 Raw3/3만10초 처리해 Clean3/3으로 돌려준다. 각 drink는 기존 Cup의 Raw15 또는 Clean38 회복/불이익을 사용하며.5초 입력 간격을 둔다. 한 용기의 서로 다른 charge/quality가 합쳐지지 않는다.

2026-10-19 교정: remote rod 표현은 owner-only inventory 대신 replicated PlayerEquipment.SelectedItem을 따른다. 취소/만료/회수 종료는 기존 ActivityKind.None을 publish하며 다음 입질 안내 상태를 초기화한다. 새 protocol 필드나 낚시 시간/보상 변경은 없다.
