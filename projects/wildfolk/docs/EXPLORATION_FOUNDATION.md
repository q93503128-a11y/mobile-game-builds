# Exploration Foundation 1

2026-10-18 현재 콘텐츠 확장: **Save9 / Protocol10, Item86 / Recipe71**. [현재 정본](CONTENT_EXPANSION.md). 아래 이전 버전 설명은 해당 패스의 기록이다.


2026-10-18. Lowfire 표시명 / Wildfolk 내부명. BaseGenerationVersion2 지형·Seed·광물 ID를 유지한다. 이번 변경은 탐험 장비/귀환 안내/청크 런타임 기반이며 Workbench III 또는 출시 progression 완성이 아니다.

## 조사와 결정

구현 전에 기존 설치/가방/시설/Save7/Protocol8/월드 코드를 읽고 출시 게임 네 개를 비교했다. 아래는 공개된 동작과 Lowfire의 결정이다. 타 게임의 비공개 청크/저장 알고리즘을 추정하지 않았다.

| 비교 게임 / 공개 근거 | 확인한 원칙 | Lowfire 적용 |
|---|---|---|
| [Minecraft Lodestone](https://www.minecraft.net/en-us/article/block-week--lodestone), [1.18 simulation distance](https://feedback.minecraft.net/hc/en-us/articles/4415128577293-Minecraft-Java-Edition-1-18) | 기본 귀환 기준과 지정 목적지는 구별할 필요가 있다. 표시 범위와 simulation 범위를 구분할 수 있다. | 스폰으로 Home을 강제하지 않는다. 플레이어가 지정한 제작대의 영속 ID를 사용한다. 지형 캐시와 살아 있는 객체를 분리한다. |
| [Terraria 시작 가이드](https://terraria.wiki.gg/wiki/Guide:Getting_started), [게임 조작](https://terraria.wiki.gg/wiki/Game_controls) | 소량 재료로 여러 횃불을 만들며, 손에 들기/설치를 같은 아이템으로 다룬다. 이 자료는 공식 커뮤니티 위키이며 내부 엔진 근거는 아니다. | 하나의 Torch로 운반/설치, 저렴한 묶음 제작. 별도 설치 횃불 item은 만들지 않는다. |
| [Core Keeper 공식 배포 패치](https://fireshinegames.co.uk/wp-content/uploads/Core-Keeper-patch-notes-1.0-to-1.0.1.9.pdf) | Torch quick-swap의 원래 슬롯 복원과 설치된 조명의 발광은 검수해야 할 실제 품질 경계다. | 기존 Hotbar 선택/장비 appearance를 재사용하고, 선택 해제·재방문·저장 뒤에도 광원 상태를 검수한다. 해당 패치로 비공개 streaming 구현을 주장하지 않는다. |
| [Grounded Koi Pond](https://grounded.obsidian.net/news/grounded/early-access-update-0-4-the-koi-pond-out-now), [1.1 개선](https://grounded.obsidian.net/news/grounded/a-holiday-treat) | 설치 광원·표식은 탐험을 보조하지만 다수 표식 UI 및 화염 건축물의 비용/화면 소음도 관리해야 한다. | 작은 방향 표시 하나와 설치 횃불 trail. 지도·무제한 표식·불 particle·추가 AI 반응은 넣지 않는다. |

출시 게임을 로컬에서 실행한 비교 QA가 아니라 공개 자료 조사다. 조명 장비의 점유 trade-off는 채택하되, 동굴에서 완전 암흑이나 즉사 규칙은 도입하지 않는다. 손 점유 없는 Lantern은 미래 장비로 남긴다.

## 조명과 제작

- Hand crafting: Wood1 + Coal1 → Torch4. Coal은 기존 Stone Pickaxe부터 채집할 수 있으므로 Workbench II/III를 요구하지 않는다. 시작 맨손 즉시 제작은 아니며 기존 초기 Stone 단계에 연결된다. 추가 연료 자원/아이템을 만들지 않는다.
- Torch는 stack50, 설치 HP40/footprint0.5×0.5, 기존 최대 설치 거리3.5, 따뜻한 색 RGB(1,.69,.34), 반경7.5/강도.9다. 연료/내구도 소모는 이번에 없다. 무료 영구 조명의 최종 경제 밸런스는 확정하지 않았다.
- PortableLight 데이터와 HeldSprite를 ItemDefinition에 연결한다. PlayerLightSource는 held/equipped mount를 구분하고 미래 SetEquipped hook을 제공한다. 실제 Wearable/Lantern recipe/slot/progression은 없다.
- 기존 Held tool 대신 횃불을 표시하고 Primary는 설치다. 다른 도구/무기를 동시에 쓰지 않는다. Movement/Sprint/Hotbar/기존 NetworkPlayer held appearance 경로를 유지한다.
- LocalFirelight 한 컴포넌트를 held/placed 양쪽에 사용한다. 짧은 intensity transition, 부드러운 URP Point Light falloff, 무그림자/무 bloom/무 particle/무 새 오디오다. 가까운 플레이어 읽기용 작은 저강도 광원이 별도로 있다.
- Cave ambient는 입구 .52에서 심부 .18까지다. 플레이어/가까운 지형 실루엣을 남기고 횃불의 넓은 시야로 차이를 만든다. 동굴 조명은 표면 낮밤과 독립적이다. 시설빛도 동굴 ambient를 따른다.
- Torch는 GameplayLightDefinition.DetersEnemies=false. 기존 시설 Safe/Dim/Grace 규칙은 보존하고 Torch로 새 aggro/언데드 피해/안전 구역을 만들지 않는다.

설치는 PlayerPlacement/NetworkConstruction의 지면·물·모든 footprint corner/통행·거리·overlap 검사를 공유한다. InventoryModel.TryDropSlot 예약과 성공 생성/NGO spawn을 한 transaction으로 묶고 실패를 복원한다. Host가 sender/sequence/revision/선택/생존/연결/거리/충돌을 검사한다. Creator는 metadata이며 소유 잠금은 없다.

## 길찾기

Compass: Workbench I, Wood2 + Iron Ingot1 → Compass1. 가방 어느 슬롯에든 가지고 있으면 작은 북쪽 dial과 귀환 방향/거리만 표시한다. 선택 손은 점유하지 않는다. 시작 영구 HUD/지도/미니맵은 아니다. Compass가 없어지면 표시도 사라진다.

Workbench I 또는 II의 제작 화면에서 `Set as home`으로 해당 제작대를 선택한다. PlayerSnapshot.HomeId 하나만 저장하며 좌표는 살아 있는 설치물 또는 sleeping snapshot에서 파생한다. Workbench 업그레이드는 원래 ID를 보존한다. 파괴/없는 ID는 안내를 숨기고 제작대 지정 안내를 표시한다. 스폰/respawn을 Home으로 자동 덮어쓰지 않는다.

지상에서는 Home의 직선 방향/거리를 보여 주며 경로·강 건너기 가능성을 보장하는 pathfinding은 아니다. 동굴에서는 지상 좌표를 직접 가리키지 않고 `Exit` 안내로 전환한다. 기존 chamber graph의 가까운 출구 방향, chamber 중심→다음 corridor 순서를 사용한다. 표면으로 돌아오면 Home이 다시 표시된다. 동굴 미터는 현재 안내 지점까지의 거리이며 전체 출구 이동거리나 Home까지의 총 거리가 아니다.

North는 카메라 축으로 투영하고 플레이어 facing과 분리한다. 자연 River/Coast/Road/biome/POI는 그대로 길찾기 단서다. Teammate/map/무제한 waypoint/death marker/fast travel은 후속 범위다.

NetworkNavigation은 기존 Player 위의 작은 Host 권위 동작이다. owner-only Home ID와 파생 좌표/유효 여부를 변경 시 전달한다. Host는 .5초마다 선택 Home의 유효성을 재확인한다. 별도 매 frame 위치 broadcast는 없다.

## 청크 / 성능 경계

전체 약42k logical chunk를 GameObject/Texture 배열로 만들지 않는다. macro biome weights/labels, 유한 anchor/cave/recipe 공급 목록만 시작에 생성한다. 실제 청크 dictionary는 활성 영역만 가진다.

기존 중앙 .25초 scheduler, 살아 있는 플레이어 주변3×3 합집합을 유지한다. 중복 pending을 제거한다. 경계에서 이미 활성인 청크는4unit margin까지 보존해 작은 왕복에 load/unload를 반복하지 않는다. 안정된1/2/4지역은9/18/36이며 경계 retention으로 순간 증가할 수 있다. player당 최대16,4인 최대64다. 멀리 점프하면 이전 지역은 다음 reconcile에 해제한다. Retention은 다른 플레이어가 아직 쓰는 영역을 제거하지 않는다.

지형은128 sample+halo130² RGB24/world-space 좌표를 유지한다. 새 청크는 row 단위로2.5ms 목표 예산에 나누어 생성한다. 한 row/최종 texture upload가 예산을 조금 초과할 수 있고 다른 materialization/AI 비용은 별도다. 동시에 전체 월드를 굽지 않는다. 첫 표시까지 짧은 지연이 가능하며 캐시 적중 시 기존 Sprite/Texture를 즉시 재사용한다.

비활성 지형 cache는 LRU24개다. 활성 texture와 분리하며 월드/Seed 변경 시 모두 폐기한다. 최대64 활성+24 cache 외에 전 월드 texture를 보존하지 않는다. unload된 partial job/buffer는 취소한다. production은 프레임 분할 경로를 사용하고 명시적인 DrainPending은 테스트/Editor fixture용 동기화다.

먼 자원/동물/시설/아이템은 기존 sleep state를 유지하며 실제 Update/Collider/Light presentation은 제거한다. 광물 고갈 tombstone은 그대로다. 시설 SimulatedAt/게임 시간으로 재활성/Save 때 처리하며 실제 오프라인 시간은 더하지 않는다. 새로운 ECS/Job/Addressables/범용 pool은 도입하지 않는다.

Ground Item은 기존 영구 저장이며 임의 TTL 삭제를 추가하지 않았다. 장기 플레이의 dropped stack·방문 생물 상태·고갈 tombstone은 선형 증가할 수 있다. 무한 방문/수천 dropped stack의 장기 용량 정책은 별도 설계/측정 대상이다.

## 저장 / 호환

Save8 / Protocol9. v1~v7을 읽고8로 이관한다. v6이하의 generation1 강제 및 구192×144 지형/자원 ID는 그대로다. v7 generation2는 같은 지형을 유지한다. 이전에는 Home 필드가 없으므로 기존 사용자 데이터에 Home을 추정해 넣지 않는다. 네트워크 protocol 변경은 새 Player 동작과 content index/prefab 추가를 반영하며 이전 peer와 혼합하지 않는다.

Torch는 일반 PlaceableSnapshot의 stable SaveId/Item/HP/위치/회전/Creator로 저장한다. NetworkObjectId를 저장 ID로 쓰지 않는다. Home은 개인 PlayerSnapshot에 ID만 저장한다. 미방문42k청크의 전체 지형/기본 자원 state를 직렬화하지 않는다.

검증 결과와 수치는 `EXPLORATION_REVIEW.md`, `validation/exploration-foundation.json`에 최종 실제 측정으로 기록한다. PC Editor와 logical actor fixture를 실제 Android/Relay/4기기 검수로 해석하지 않는다.

멀티 야외 조명 시간은 기존 NetworkWorldState의 Host 시간 sample(Elapsed/Server timestamp/Advancing)을0.5초마다 전달하고 Client가 server time으로 보간한다. 멈춘 Client WorldClock 때문에 밤에도 낮 광원으로 표시되던 경계를 연결했다. 별도 매 frame light/position RPC는 없으며 실제 remote Relay 화면 동기화는 미검수다.
