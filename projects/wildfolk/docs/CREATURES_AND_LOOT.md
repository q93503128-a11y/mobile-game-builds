# 현재 Creature / Loot

CreatureFamily=Animal/Undead/Humanoid, Faction=Wildlife/Goblin/Undead와 Player 개념, Traits=Herding/FleeTogether/AllyAssist/PackAggro/Territorial/Ranged/CampBound/Nocturnal/LightSensitive, Habitat=Grassland/Forest/ForestEdge/Roadside/Camp/NightSurface를 실제 데이터에 지정한다. Family와 Faction과 Trait은 별도다. Scout/Ruin 등 Enum 확장 지점은 있지만 실제 Scout 콘텐츠나 Ruin을 추가하지 않았다.

Animal은 지역·종 목표와 Group ID/간격/Anchor를 사용한다. Deer/Sheep/Chicken/Rabbit은 가까운 동일 Group에 도주 경보를 전달한다. Boar는 단독/작은 Group과 기존 비공격 도주를 유지한다. 강한 Rigidbody Push 없이 생물 쪽 이동 보정으로 Player/생물 겹침을 완화한다.

Goblin은 근접+새총 Warband와 작은 Camp 거주 개체다. 직접 이웃 경보7/위협에서12/Leash10을 사용하고 전달 받은 경보는 재전파하지 않는다. 새총은0.65초 제한 재배치 후 거리 안에서 발사하며 독립 Projectile가 실제 피해를 준다. 별도 Bow/Player Ammo 콘텐츠는 없다.

Skeleton은 Zombie와 공통 EnemyController의 준비/Swing/Hit/Recovery를 사용한다. 검 Variant가 실제 손 Sprite·Damage·Reach·Loot를 바꾸고 별도 AI 코드를 복제하지 않는다. LightSensitive 데이터만 빛 효과를 받는다. Undead Family는 자동 빛 약점 조건이 아니다.

| 종 | XP / Gold | 현재 일반 Item Drop | Rare |
|---|---:|---|---|
| Chicken |0 /0|Raw Meat1,Feather1 (기존 유지)|없음|
| Boar |0 /0|Raw Meat2,Leather1 (기존 유지)|없음|
| Rabbit |0 /0|Raw Meat1|없음|
| Deer |0 /0|Raw Meat2~3,Leather1 75%|없음|
| Sheep |0 /0|Raw Meat2|없음|
| Zombie |5 /1|Wood1~2 60%,Leather1 20%|없음|
| Goblin/새총 Goblin |8 /3|Stone1~3 80%,Wood1~2 60%|없음|
| Skeleton |7 /2|Stone1~2 75%,Iron Ore1 20%|없음|
| 검 Skeleton |8 /2|Stone1~2 75%,Iron Ore1 20%|Rusted Sword1 12%|

LootTable.Items/RareItems/Experience/Gold가 정본이며 종 이름 if문으로 드랍을 고르지 않는다. 실제 공격자의 PlayerProgression에 처치 보상을 지급하며 자연 소멸은 보상을 만들지 않는다. 새 Wool/Bone/Scrap/Pebble Ammo 아이템은 없다. 현재 Armor는 기존 Leather/Iron Head/Body4개이며 철이 최종 장비 단계라는 의미가 아니다. 자세한 초기 수치는 BALANCE에 기록한다.
