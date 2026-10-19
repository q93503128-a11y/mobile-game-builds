# 전투 관계와 적 이동

기본 Fist와 현재 Wood/Stone/Iron Axe/Pickaxe/Sword는 모두 MaxTargets1이다. 이전 코드도 한 대상만 피해를 주었으나 방향과 무관한 가장 가까운 대상을 선택했다. 현재는 Facing Sector 안에서 살아 있는 유효 대상을 거리/중앙 정렬 점수로 선택하고 동일 Health의 중복 Collider를 중복 타격하지 않는다. Range는 Collider 표면까지의 거리다. 밀착/일반 거리/범위 끝은 자동 검증하며, 존재가 확인되지 않은 밀착 버그를 가정하지 않았다.

AttackProfile은 MaxTargets/ArcAngle/Range/PierceCount/AoERadius/SelectionRule을 표현한다. 현재 Arc120도/Range1.6/MaxTargets1/Pierce0/AoE0이다. MaxTargets를 늘린 Sector 선택은 검증 가능하지만 출시 장비는 모두1이다. 실제 Pierce/AoE 공격 방식, Greatsword/Spear/Sweep/광역 Skill은 아직 없다.

CombatAffiliation의 PlayerTeam/TeamStructure와 TeamId를 사용한다. 같은 Team의 Player/길들인 개체/시설은 일반 공격으로 손상되지 않는다. WildAnimal/NeutralGoblin은 공격 가능하며 Goblin은 피격으로 Aggro한다. Friendly는 후보에서 제외되므로 뒤의 Hostile을 막지 않는다. 현재 Multiplayer Foundation 2는 Host 권위 Creature 전투 기반을 제공하며 실제2인 통합 QA는 남아 있다. 아군 길들이기는 없다. 철거/회수는 향후 별도 명시적 기능이다.

Goblin은 OuterTerritory5에서 주시, InnerTerritory2.5 또는 PersonalSpace1.8에서 Aggro, 선공에는 즉시 Aggro한다. 기존 Leash10을 유지한다.

Zombie는 LocalThreatTarget, GlobalHardCap18, ActivePursuerCount를 나눈다. 기본 LocalRadius45, 감지36, 추적 대상 상실거리50/대기15초다. Player에게60 이상 떨어져 화면 밖에서 추적하지 않는 상태가20초 유지되면 보상 없이 잠복하고 Slot을 반환한다. 실제 Pursuer는 곧바로 삭제하지 않는다. 아직 Player 한 명 기준이며 여러 지역의 Multiplayer Scaling은 없다.

GameplayLight의 Safe Radius4, Grace2초를 유지한다. Dim 영역은 반경의1.35배까지이며 감지0.8배, Safe 영역에서 이동0.75배이다. 밝은 곳의 신규 Spawn/Aggro를 막고 추적은 Grace 뒤 포기한다. 빛의 픽셀을 읽지 않는다.

WorldNavigation은 생성 지형/깊은 물/시설/자원 충돌과 Spawn 적합성을 공통으로 판별한다. EnemySteering은 Radius1.35/Strength0.8의 부드러운 분리와 국소 우회를 사용한다. 적 Collider는 Trigger이며 Player를 물리적으로 밀지 않는다. Zombie의 방어시설 공격을 유지한다. 전체 경로 탐색이나 복잡한 기지의 완전한 내부 판정은 아니다.

PrefabPool은 Zombie/Goblin/WorldPickup을 재사용한다. HP/피격/AI/생활주기/아이템 수량/지연 상태를 초기화하고 자연 Despawn은 사망 보상을 호출하지 않는다. 자원·시설·동물까지 무조건 Pooling하지 않는다. Projectile/VFX는 실제 콘텐츠가 생길 때 확장한다.

PlayerMotor는 PlayerInputCommands.Move를 사용한다. 터치/키보드는 PlayerInputAdapter에 모으고 IPlayerInputSource로 자동 입력을 주입할 수 있다. 이동/Primary/Interact/Inventory/Pause 명령은 기기 API를 몰라도 호출할 수 있다.

## Sprint / Dash / Armor

Armor Head/Body가Health.DamageReduction으로합산된다. 기존Friendly Fire/Single Target/생존피해우회는유지한다. CharacterStats는HP/Stamina/회복/효율의Skill집계경계다.

보행은Stamina를소비하지않는다. Sprint는Joystick Hold/Hysteresis와기존CanSprint/SprintEfficiency를사용한다. Dash는현재Move또는Facing,즉시Stamina소비,0.18초물리이동이다. 0.08유닛Substep별Map walkable+CircleCast를검사하고자기Collider는제외한다. 깊은물/기지벽/자원장애물을통과하지않고무적/공격판정은없다. UI열림/사망/고갈/짧은Cooldown은재입력을거절한다. Android최종체감은사용자테스트다.


## 생태·밀착·문 품질 변경

Target Collider와 Player Collider 간 실제 거리≤0.12이면 Facing Arc만 완화한다. Origin→ClosestPoint Range와 Single Target/Friendly Fire OFF는 그대로다. Fist/Axe/Pickaxe/Sword의 정면·측면·뒤 밀착,정상 정면·측면,Range 경계/밖을 자동 검사한다.

NeutralAnimal은 Player Collider를 단단한 장애물로 검사하지 않고 SoftSeparation으로 생물 쪽을 보정한다. 종별 반경/느슨한 Group 간격은 CreatureDefinition 데이터다. Goblin도 공통 Group 경보/간격을 쓰며 직접 주변 Ally Assist,Leash와 제한된 새총 재배치를 사용한다. Skeleton은 공통 EnemyController의 준비/Swing/Hit/Recovery와 Variant Weapon을 따른다. 빛 효과 조건은 LightSensitive이며 Undead 이름/Family 자체가 아니다.

Door는 왼쪽 Hinge/고정 Frame/0.32초 easing으로 열린다. Collider는 eased Open70%에서 해제하고 닫을 때 다시 활성화한다. 점유 상태의 닫기는 거절한다. 공격 중 transient Pose와 발사된 돌은 Save에서 재개하지 않는다. 실제 체감/난이도/경로 자연스러움은 사용자가 확인한다.

## 멀티 전투 기반

공격 입력은 owner RPC로 요청만 전달하고 Host의 같은 PlayerCombat/AttackProfile이 쿨다운·Range/Facing/Point-Blank/Single Target/weapon stats를 판정한다. 멀티 Primary는 NetworkCreature만 대상으로 하며 채집/시설 피해와 PvP를 열지 않는다. Creature는 Host가 생존 Player를 선택하고 준비/타격 때 고정한다. 대상 사망은 추적 초기화와 재선택으로 처리한다. 사망은 Health lethal latch와 NetworkCreature.DeathResolved로 한 번만 처리하고 Client Died/로컬 reward roll은 실행하지 않는다. Ground Loot/Inventory/건축/World persistence는 이번 범위가 아니다. 구체적인 자동검사와 실제2인 검증 미수행은 MULTIPLAYER_FOUNDATION을 따른다.

## Workbench I–II Content Expansion (2026-10-18)

출시 알파 무기군은 Sword/Spear/Bow/Crossbow 네 개다. Spear는 기존 melee의 긴 Range/좁은 Arc/큰 thrust 데이터다. Bow와 Crossbow는 하나의 PlayerRanged와 기존 projectile을 공유한다. Bow→Reinforced Bow는 이전 Bow를 소비하고 Crossbow는 별개다. 기본 Arrow/Bolt는 renewable 재료, 고급탄은 Bronze/Steel 선택 소비다. 실제 공격력·초당 공격·Range를 UI에 표시한다. [수치·Recipe](CONTENT_EXPANSION.md).

## 통합 감사 교정 (2026-10-19)

SP/Host death는 같은 HitFeedback settle을 사용하며 총0.35초 후 pooling/despawn한다. 보상은 lethal 시점에 확정하며 collider와 AI/lifecycle은 즉시 중지한다. 자연 burrow/despawn은 여전히 보상 없이 별도 경로다. 피격0.16초는 event-start coroutine으로 보존했다. CameraFollow는 초기 화면/지역 bounds 변경/화면 폭 초과 불연속 이동만 snap하고 일반 이동은 기존 SmoothDamp를 유지한다. 세부 근거와 검수 한계는 GAMEFEEL_AUDIT를 따른다.
