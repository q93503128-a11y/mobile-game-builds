# 현재 초기 플레이 수치

기준일: 2026-10-14. 단일 정본은 client/Assets/Wildfolk/Data/ 에셋이다. 이전 사용자 지정 Recipe 비용과 이번 기능 검증에 허용된 여유 있는 초기 수치이며 상대 게임의 수치를 측정/복제하지 않았다. 재미와 세부 밸런스는 사용자 직접 테스트로 판단한다.

| 항목 | 값 |
|---|---:|
| 플레이어 HP / 이동 | 100 / 4.2유닛·초 |
| 카메라 SmoothTime | 0.18초 |
| 일반 나무 HP / Wood 드랍 | 150 / 6 |
| Fallen Log HP / Wood 드랍 | 50 / 4 |
| 돌 HP / Stone 드랍 | 50 / 3 |
| Zombie HP / 이동 | 40 / 2.2유닛·초 |
| 감지 / 추적 종료 | 4 / 8유닛 |
| 적 정지 / 최소 분리 | 1.35 / 1.12유닛 |
| 적 공격 사거리 / 피해 | 1.55유닛 / 8 |
| 적 준비 / 회복 | 0.38 / 1.15초 |
| 인벤토리 / Hotbar / Storage | 36 / 9 / 27칸 |
| 재료 / 도구·검 / Workbench 스택 | 50 / 1 / 10 |
| 드랍 퍼짐 / 이동 | 0.85유닛 / 0.3초 |
| 획득 지연 / 거리 | 0.7초 / 0.95유닛 |

적은 준비 시 정한 방향 dot > 0.4와 공격 사거리를 확인한다. 피격 반응 0.16초, 나무 흔들림 0.075유닛/3도, 돌 0.038유닛/0.7도, 생명체 squash 최대 10%를 유지한다. 바닥 아이템은 공간 부족 시 0.15초 간격으로 획득을 재시도한다.

## 사용 프로필

| 항목 | 맨손 | Wood Axe | Wood Pickaxe | Wood Sword |
|---|---:|---:|---:|---:|
| 적 피해 / Zombie 필요 타수 | 10 / 4 | 10 / 4 | 10 / 4 | 20 / 2 |
| Wood 효율 | 기본 | 3배 | 0.5배 | 0.25배 |
| Stone 효율 | 기본 | 0.5배 | 2.5배 | 0.25배 |
| 일반 나무 피해 / 타수 | 10 / 15 | 30 / 5 | 5 / 30 | 5 / 30 |
| 돌 피해 / 타수 | 10 / 5 | 5 / 10 | 25 / 2 | 5 / 10 |
| 모션 | 0.28초 | 0.44초 | 0.52초 | 0.34초 |
| 쿨다운 | 0.44초 | 0.62초 | 0.72초 | 0.46초 |
| 피해 시점 | 0.14초 | 0.22초 | 0.26초 | 0.17초 |
| 사거리 | 1.6 | 1.6 | 1.6 | 1.6유닛 |
| 손 확장 | 1.05 | 0.25 | 0.25 | 0.25유닛 |
| 휘두름 Arc | 없음 | ±65도 | ±55도 | ±75도 |

AxeTool/PickaxeTool 기존 프로필과 새 WoodSwordTool을 사용하며 ResourceKind 효율 데이터로 피해를 계산한다. Sword는 전투에 효율적이고 채집에 비효율적이다. 공용 Slash/Impact 파티클은 없다.

## 제작과 설치

| 결과 1개 | 시설 | 비용 |
|---|---|---:|
| Workbench | None | Wood 6 |
| Wood Axe | Workbench | Wood 5 |
| Wood Pickaxe | Workbench | Wood 5 |
| Wood Sword | Workbench | Wood 5 |
| Campfire | None | Wood 4 + Stone 3 |
| Cooked Meat | Campfire | Raw Meat 1, 5초 |

최초 작업대와 세 장비에는 총 Wood 21개(현재 성숙 나무4개 또는 통나무/나무 조합)가 필요하다. 무료 시작 장비를 제거했고 새 게임/재시작은 빈 인벤토리와 맨손이다.

설치 Preview 거리 2.1, 최대 거리 2.8유닛, Footprint 1.8×1.15유닛, Workbench HP 60. 시설 상호작용 중심 거리 2.6유닛, 앞 방향 dot ≥ 0.35다. 겹침/경계/거리 실패는 설치물을 소비하지 않는다. 제작 공간 부족은 재료를 소비하지 않는다.

도구 내구도/수리 비용/회수 규칙은 없다. XP/Gold 초기 처치 보상은 아래 표를 따른다. 이 수치는 출시 밸런스 확정값이 아니다.

## 생존·물·모닥불·설정

| 항목 | 실제 초기값 |
|---|---:|
| Hunger / Thirst 최대·시작 | 100 / 100 |
| Hunger 감소 | 0.08/초 (최대→0 약 20분50초) |
| Thirst 감소 | 0.12/초 (최대→0 약 13분53초) |
| 각 0 상태 피해 | 5초마다 HP 2; 둘 다 0이면 HP 4 |
| Raw Meat / Cooked Meat | Hunger +8 / +35 |
| Water | Thirst +30 |
| 소비품 최대 스택 | 20 |
| 수원 수집 / 재사용 간격 / 중심 거리 | Water 1 / 1초 / 3유닛 |
| 멧돼지 수 / HP / 드랍 | 1 / 20 / Raw Meat 2 + Leather 1 |
| Campfire HP / 스택 / Footprint | 60 / 10 / 1.5×1.15유닛 |
| 조리 투입 / 시간 / 결과 | Raw Meat1 / 5초 / Cooked Meat1 |
| Control Size 범위 / 기본 | 80~140% / 100% |
| Master / BGM / SFX 기본 | 100 / 65 / 85% |

수치 감소/조리는 scaled time이다. Inventory·Multiplayer 메뉴에서는 진행하고 Singleplayer Pause에서는 정지한다. 각 수치는 0/Max로 제한하며 음식/물은 해당 회복이 필요할 때만 사용한다. 물 획득은 소모품 사용과 별개로 보관 공간이 있으면 가능하다. 멧돼지 재생성과 음식 장기 공급은 없다. Campfire 설치/상호작용 거리·방향은 Workbench와 같다. 초기 생존 시간·회복량·Campfire 비용은 기능 검증을 위한 자체 초기값으로 Mine Survival의 관측 수치가 아니다.

## Wood → Stone → Iron

Wood 기존 프로필/비용은 유지했다. 이 단계 수치는 기능 검증에 맞춘 자체 초기값이며 상대 게임의 관측 수치가 아니다.

| 장비 | 기본 피해 | Wood/Stone/Ore 배율 | 모션/쿨다운 초 |
|---|---:|---|---|
| Stone Axe | 12 | 3.5 / 0.5 / 0.25 | 0.36 / 0.50 |
| Stone Pickaxe | 12 | 0.5 / 3.5 / 2.5 | 0.44 / 0.60 |
| Stone Sword | 28 | 0.25 / 0.25 / 0.25 | 0.31 / 0.42 |
| Iron Axe | 15 | 4 / 0.5 / 0.25 | 0.30 / 0.42 |
| Iron Pickaxe | 15 | 0.5 / 4 / 3 | 0.36 / 0.50 |
| Iron Sword | 36 | 0.25 / 0.25 / 0.25 | 0.28 / 0.38 |

사거리1.6/손 확장0.25, Arc Axe65/Pickaxe55/Sword75도 유지. 철 광석은 HP75/드랍Ore3/노드16개, Pickaxe+Tier Rank≥2만 채굴한다. Stone Pickaxe Ore피해30(3타), Iron Pickaxe45(2타)다. Axe/Sword의 Ore 배율이 있더라도 채굴 자격이 없어 피해를 주지 않는다. Wood Pickaxe/맨손도 불가. 현재 자원량은 Tree140×Wood6=840, FallenLog12×Wood4=48, Stone55×3=165, Ore16×3=48이다. 재생성은 없다.

| 새 Recipe | 비용 |
|---|---|
| Stone Axe / Pickaxe | 해당 Wood 장비1 + Stone4 |
| Stone Sword | Wood Sword1 + Stone5 |
| Furnace | Stone8 + Wood4 |
| Iron Ingot | Iron Ore1/개, 최대3개 동시6초, 연료 없음 |
| Iron Axe / Pickaxe / Sword | 해당 Stone 장비1 + Ingot3 |
| Wood Wall / Wood Door / Storage Chest | 각각 Wood2 / Wood3 / Wood6 |

| 시설 | HP | Footprint | 스택/슬롯 |
|---|---:|---|---|
| Wood Wall | 100 | 1.8×0.45 | 스택10 |
| Wood Door | 80 | 1.8×0.45 | 스택10 |
| Storage Chest | 70 | 1.5×1.1 | 스택10 / 저장24 |
| Furnace | 100 | 1.8×1.6 | 스택10 |

배치2.1/최대2.8/상호작용2.6유닛 유지. 벽/문 회전0/90도, 문 닫기 점유 검사85% Footprint. Zombie 건축 타격도 기존 피해8/준비0.38/쿨다운1.15/사거리1.55를 사용한다.

## 낮밤·적·빛·동물

| 항목 | 초기값 |
|---|---|
| Day / Evening / Night / Dawn | 240 / 60 / 150 / 30초, 총480초 |
| 낮 / 저녁 / 밤 intensity | 1 / 0.9 / 0.48 |
| 저녁 RGB / 밤 RGB | (1,.88,.72) / (.78,.83,.76) |
| 낮 Zombie | 0、생성 없음 |
| Night Target | 1~2:6、3~4:8、5~6:10、7~8:12、이후 두 밤마다+2 |
| Hard cap | 18 |
| 초기 출현 | Target의ceil(65%)、0.9초 간격 분산 |
| 보충 | Target50% 미만일 때15초마다1개 |
| Emergence / Burrow | 1.2 / 1.1초、전환 중 공격 없음 |
| Spawn Player 거리 / 후보 반경 | 최소12 / 20~32유닛 |
| Building 거리 / Camera 여유 | 5유닛 / 화면5% |
| 후보 횟수 / Footprint | 48 / 반경.65 |
| World / Region | 192×144 / 189×141유닛 |
| 안전빛 반경 / Strength / Threshold / Grace | 4유닛 / 1 / .65 / 2초 |
| 시설 시각 조명 Outer / Inner | 5.5 / 3유닛、intensity=(1−ambient)×1.2 |
| Goblin HP / 이동 / 순찰 | 50 / 2.4 / .65유닛·초 |
| Goblin Territory / Leash | 2.5 / 10유닛 |
| Goblin 공격 / Windup / Cooldown / Reach | 6 / .45 / 1.3초 / 1.5유닛 |
| Goblin 낮 Encounter | 35초마다30% 시도、최대3 |
| 닭 HP / 드랍 | 20 / Raw1+Feather1 |
| 멧돼지 HP / 드랍 | 20 / Raw2+Leather1 |
| 동물 Wander / Flee | .55 / 2.4유닛·초 |
| 동물 위협 거리 / 도망 / 방향 간격 / Home | 2.4 / 2.2초 / 4초 / 4유닛 |
| Zombie XP / Gold | 5 / 1 |
| Goblin XP / Gold | 8 / 3 |

동물 물품 드랍 확률은 각각100%, 각 개체의LootTable 데이터다. 적의 Item Table은 현재 비어 있다. 직접 처치 때만 별도세션 지갑에XP/Gold를 지급하며 자연Burrow/화면 밖 정리는 보상하지 않는다. Level/Skill/Gold 사용처/영구MetaCoin은 없다.

## 병렬 처리와 생존 회복

Furnace ParallelCapacity3/ProcessingSpeed1, Campfire1/1. Ore2 또는3은 동일한6초 Batch다. 처리 중 파괴 시 예약한 전량, 완료 후 파괴 시 완성 결과 전량을 드랍한다. 결과 공간 부족은 시설에 보존한다.

두 생존 수치의최소 비율≥70%이고 최근전투 이후8초이면HP.5/초 자연회복, 아래는0이다. Critical30% 미만은CanSprint=false, 30~70% SprintEfficiency=.55~1, 70% 이상1이다. 실제Sprint는없다. 최소비율15% 미만 일반이동90%, 그 외100%. 허기/갈증0 피해와음식 수치는 기존 값이다. Armor는 실제아이템없이 슬롯/수치/소켓 기반뿐이며 일반피해 최소1, 소진피해는방어를무시한다.

모든 수치는 기능 검증용 자체 초기값이며 출시 확정값이나 Mine Survival의 측정 수치가 아니다. 실제 진행 속도·야간 난이도·경제·음식 공급은 사용자 판단이 필요하다.
