# 전투 관계와 적 이동

기본 Fist와 현재 Wood/Stone/Iron Axe/Pickaxe/Sword는 모두 MaxTargets1이다. 이전 코드도 한 대상만 피해를 주었으나 방향과 무관한 가장 가까운 대상을 선택했다. 현재는 Facing Sector 안에서 살아 있는 유효 대상을 거리/중앙 정렬 점수로 선택하고 동일 Health의 중복 Collider를 중복 타격하지 않는다. Range는 Collider 표면까지의 거리다. 밀착/일반 거리/범위 끝은 자동 검증하며, 존재가 확인되지 않은 밀착 버그를 가정하지 않았다.

AttackProfile은 MaxTargets/ArcAngle/Range/PierceCount/AoERadius/SelectionRule을 표현한다. 현재 Arc120도/Range1.6/MaxTargets1/Pierce0/AoE0이다. MaxTargets를 늘린 Sector 선택은 검증 가능하지만 출시 장비는 모두1이다. 실제 Pierce/AoE 공격 방식, Greatsword/Spear/Sweep/광역 Skill은 아직 없다.

CombatAffiliation의 PlayerTeam/TeamStructure와 TeamId를 사용한다. 같은 Team의 Player/길들인 개체/시설은 일반 공격으로 손상되지 않는다. WildAnimal/NeutralGoblin은 공격 가능하며 Goblin은 피격으로 Aggro한다. Friendly는 후보에서 제외되므로 뒤의 Hostile을 막지 않는다. 실제 네트워크나 아군 길들이기 기능은 없다. 철거/회수는 향후 별도 명시적 기능이다.

Goblin은 OuterTerritory5에서 주시, InnerTerritory2.5 또는 PersonalSpace1.8에서 Aggro, 선공에는 즉시 Aggro한다. 기존 Leash10을 유지한다.

Zombie는 LocalThreatTarget, GlobalHardCap18, ActivePursuerCount를 나눈다. 기본 LocalRadius45, 감지36, 추적 대상 상실거리50/대기15초다. Player에게60 이상 떨어져 화면 밖에서 추적하지 않는 상태가20초 유지되면 보상 없이 잠복하고 Slot을 반환한다. 실제 Pursuer는 곧바로 삭제하지 않는다. 아직 Player 한 명 기준이며 여러 지역의 Multiplayer Scaling은 없다.

GameplayLight의 Safe Radius4, Grace2초를 유지한다. Dim 영역은 반경의1.35배까지이며 감지0.8배, Safe 영역에서 이동0.75배이다. 밝은 곳의 신규 Spawn/Aggro를 막고 추적은 Grace 뒤 포기한다. 빛의 픽셀을 읽지 않는다.

WorldNavigation은 생성 지형/깊은 물/시설/자원 충돌과 Spawn 적합성을 공통으로 판별한다. EnemySteering은 Radius1.35/Strength0.8의 부드러운 분리와 국소 우회를 사용한다. 적 Collider는 Trigger이며 Player를 물리적으로 밀지 않는다. Zombie의 방어시설 공격을 유지한다. 전체 경로 탐색이나 복잡한 기지의 완전한 내부 판정은 아니다.

PrefabPool은 Zombie/Goblin/WorldPickup을 재사용한다. HP/피격/AI/생활주기/아이템 수량/지연 상태를 초기화하고 자연 Despawn은 사망 보상을 호출하지 않는다. 자원·시설·동물까지 무조건 Pooling하지 않는다. Projectile/VFX는 실제 콘텐츠가 생길 때 확장한다.

PlayerMotor는 PlayerInputCommands.Move를 사용한다. 터치/키보드는 PlayerInputAdapter에 모으고 IPlayerInputSource로 자동 입력을 주입할 수 있다. 이동/Primary/Interact/Inventory/Pause 명령은 기기 API를 몰라도 호출할 수 있다.
