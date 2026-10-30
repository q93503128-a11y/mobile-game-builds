# Pre-Internal-Test Technical Health + VFX Correctness

2026-10-30. 판정: **READY — 이번 기술 범위의 Internal Test Build 준비**. Android 실기기 성능·청취/터치 승인이나 공개 출시/결제/광고 활성화 승인이 아니다. 빌드는 실행하지 않았다.

## 1. 시작 SHA

remote main fetch 후 `1f5cc388749764c6060394552597062b2099f7b0`, local HEAD와 일치·main clean을 확인했다. 최종 commit/push SHA와 clean 확인은 완료 응답에서 기록한다. 임시 branch/PR은 만들지 않는다.

## 2. Audit 범위

제품 수정 전에 AGENTS/README와 ARCHITECTURE, COMBAT_NAVIGATION, MULTIPLAYER_FOUNDATION, SAVE_PERSISTENCE, AUDIO, DEVELOPER_TOOLS 등 관련 정본을 읽었다. C# **442파일/21,516줄**을 패턴별 분류했다(Runtime331/AndroidAds1/AndroidBilling1/Editor109). 이 숫자는 전 파일 정독·성능 측정을 뜻하지 않는다. 아래 경계와 후보를 직접 읽고 Unity baseline으로 확인한 뒤 수정했다.

| 영역 | 실제 읽은 핵심 경계 | 판정 |
|---|---|---|
| player/input/combat | commands, primary action, melee/ranged, Health, class, death, avatar | 첫 replica 피격만 F1; 일반 공격/사망 회귀 유지 |
| world/streaming | generator, chunk scheduler/presentation/objects, renewal, snapshot codec | 제한 청크·단계 생성; 준비 중 객체 수를 steady state와 비교하지 않음 |
| interaction/craft/build | interaction, crafting panel/card, processing, placement, storage | 직전 우선순위/선택 교정7개 회귀 통과; 규칙 변경 없음 |
| enemy/boss | enemy/goblin/slingshot, loot, minion reset, encounter/world/telegraph/visual | 권위·동일 타격·wave/return/death·pool reset 확인 |
| save/scene | repository, validators, codecs, SceneFlow, world save, player death persistence | checksum/temporary/replace/backup 구조 유지; 실제 Continue 왕복 |
| network | session, player/creature/resource/building/boss/projectile, spawn factory, tick subscription | 실제 두 프로세스 LAN; 초기 상태를 새 타격과 구분 |
| UI/events/tasks | HUD/panels/localization/meta UI, account/economy/wallet/ad/store async | 씬 구독 계수·서비스 busy/epoch 경계 확인; 미재현 온라인 후보는 DEFER |
| presentation/audio | HitFeedback, actor audio, persistent landscape/voices, lighting/sorting/camera | 씬 소유 오디오 예약 F2; 존재하지 않는 효과 추가 안 함 |
| tooling/resources | explicit Editor harness, fixture policy, catalog caches, logging | 개발 도구와 제품 경로 분리; QA 관측 오류도 별도 기록 |

## 3. FIX NOW

| ID / 중요도 | 실제 문제·재현 | 수정 |
|---|---|---|
| F1 / P1 | 초기 `ApplyReplica(80,false)`만으로 Damaged1/HitFeedback coroutine 시작. 실제 LAN late join에서도 이미 HP49/50인 돌이 새로 맞은 것처럼 반응: Host8/Client9 이벤트, 초기 프레임 추가1 | Health가 첫 snapshot을 baseline으로 prime. 이후 HP 감소/명시한 새 hit는 한 번만 통지; 재사용 시 prime 초기화 |
| F2 / P1 | 지연 BossDefeated 후 실제 Save and Home 버튼 → `MainMenu:BossVictory`1. 영속 landscape의 encounter timer가 씬을 넘음 | BindScene에서 victoryAt/bossUntil/duckUntil/bossDead 초기화. 영속 owner·crossfade 구조는 유지 |
| F3 / P2, 테스트 | 저장 회귀가 현재 Save14를 여전히10으로 기대 | 현재 정본14를 엄격히 검사 |
| F4 / P2, 테스트 | ecology registry 계약이 현재66개를64로 기대 | 현재66개·중복 없음·shared brain 계약 유지 |
| F5 / P2, 테스트 | Recipe111 화면에서 보이지 않는 Workbench/Axe/Pickaxe 버튼 좌표 클릭. 제작되지 않아 Wood6/450이 그대로 남음(2개 테스트) | 기존 CraftVisible helper로 실제 카드를 viewport에 넣고 클릭; 자원/결과 기대값 유지 |
| F6 / P2, 테스트 | retarget 테스트가 Survivor의 기존 lethal safeguard 이후 HP1을 죽음으로 간주 | 보호 시간을 기다리고 실제 HP0을 assert한 뒤 retarget 검사 |

F3/F5/F6의 4개 PlayMode 실패는 수정 전 baseline QA 복사본에서도 **동일 메시지로 4/4 재현**했다. 게임 규칙을 바꿔 테스트에 맞추지 않았다. F4도 이번에 바꾸지 않은 기존 prefab data와 validator로 확인했다. 확인된 P0는 없다.

## 4. DEFER

| 후보/검증 공백 | 이유·다음 확인 |
|---|---|
| Editor에서 관측한 GC allocation의 Android 비용 | call stack/실기기 귀속 미측정. 아래 수치를 기기 예산으로 해석하지 않으며, 내부 빌드의 기기 profiler로 확인 |
| guard modifier와 replica 공격 자세 시간 | NetworkPlayer의 tool duration 표현 경로를 후보로 기록. 이번 LAN의 explorer fixture는 guard modifier를 검증하지 않음. 실제 class LAN에서 재현 후 판단 |
| 온라인 UI await 후 view 수명 | 현재 운영 backend/ads/purchase OFF에서 지연 외부 completion을 재현하지 않음. service epoch/busy와 view 경계를 더 검사할 후속 후보; 결제 작업을 화면 종료 때문에 임의 취소하는 refactor 안 함 |
| Relay jitter/손실·4인·저사양·실기기 pause/native overlay | 이번 LocalNetwork 두 프로세스/Editor 검증과 구분. 공개 SDK 활성화·계정 설정은 기존 RELEASE_BLOCKERS 유지 |

이 항목들은 새로 확정한 제품 P0/P1이 아니다. 검증하지 않은 동작을 통과했다고 보고하지 않는다.

## 5. IGNORE

주관적인 타격감·색/크기/속도 조정, 새 particle/trail/shake 추가, class/balance/progression/UI 재설계는 제외했다. throttled/cached Find/GetComponent와 일회성 chunk Instantiate를 grep 결과만으로 결함 처리하지 않았다. LINQ `Select` 오탐, Launch/Initialize 전에 덮어쓰는 pool 상태, 의도한 fail-closed transaction error log, 명시적으로 호출하는 Editor QA 도구도 임의 제거하지 않았다.

## 6. 실제 수정한 기술 문제

제품 코드는 **Health.cs / AudioLandscape.cs 두 파일**만 변경했다. snapshot에는 Changed를 전달해 HP/bar를 복원하면서 초기 Damaged를 억제한다. 새 명시 hit는 HP가 같아도 피격으로 처리한다. 새 serialized field, save/network payload, 새 RPC, 전역 catch, 신규 pool/manager를 추가하지 않았다. 별도로 위의 오래된 테스트 가정5개를 교정했다.

## 7. 실제 수정한 VFX correctness

F1은 초기 복원 때의 거짓 hit animation을 제거한 것이다. 타격당 기존 반응은 유지한다. F2는 이전 encounter가 새 씬에서 victory/duck/intro lease를 실행하는 수명 오류를 제거한 것이다. 효과 art·timing·강도와 음원은 변경하지 않았다.

## 8. Update / allocation

NavigationHud는 .15초·label 변경 검사, crafting progress는 열린 화면의 .2초 갱신 등 기존 제한을 확인했다. chunk는 .25초 reconcile/한 청크 activation 및 terrain work budget을 사용한다. boss collision buffer/telegraph point array는 재사용한다. pool의 reset component 배열과 실제 actor/UI 경로에는 allocation이 남아 있다. 측정된 GC를 전부 제품 hot path 탓으로 단정하거나 무측정 cache/refactor를 추가하지 않았다.

## 9. Events / subscriptions

Health/HitFeedback/AudioActor, boss/telegraph, HUD/panel/localization, network tick/NetworkVariable의 subscribe/unsubscribe 짝을 확인했다. 최종 준비 완료 후 3회 월드→Home 왕복에서 **Home L.Changed11, 죽은 Unity 대상0, audio owner1/source30**이 유지됐다. 월드 대상 수는 encounter/활성 UI가 달라 단순 합계 비교로 누수를 판정하지 않는다.

## 10. Coroutine / task lifecycle

HitFeedback는 disable 때 coroutine을 중단하고 재사용 때 pose/color를 복원한다. boss/minion/shot의 종료·despawn/return 경계도 검사했다. F2는 persistent timer의 scene cancellation이다. Account/Economy/Store/Revive 서비스의 busy/epoch/reconcile 보호를 확인했으나 실제 미설정 외부 SDK의 지연/취소를 전부 검증한 것은 아니다. QA observer와 recorder는 종료·dispose했고 Play를 끄고 원래 QA flag를 복원했다.

## 11. Pooling

기존 PrefabPool의 prefab별 반환 한도32, 중복 Release 방지, HP/reset callbacks, scene owner 파괴를 유지했다. 실제 20분 반복에서는 **반환 root + active projectile + active pickup =9**가 모든 표본에서 일정했다. 반환 root만 세면 active8 발사 중0으로 보이므로 이를 증가/소실로 해석하지 않는다. minion의 일반 AI/loot/lifecycle 복원 회귀와 replica re-prime 검사가 통과했다. network actor 수명은 NGO despawn 경로로 검증했다.

## 12. VFX inventory

`실제`는 Unity 렌더링/상태를 이번에 관측, `LAN`은 실제 headless transport/상태·event, `시험`은 자동 Play/Edit fixture, `source`는 정적 확인이다. 전부 자연 플레이로 경험했다는 뜻이 아니다.

| Event | Source | Trigger | Owner | Lifetime | Pool? | Networked? | 관측 correctness |
|---|---|---|---|---|---|---|---|
| player/creature hurt | HitFeedback | Health.Damaged | actor Visual | .16s | IPoolReset | HP/hit receipt | F1 수정, 시험/LAN에서 초기0·새 hit1 |
| wood/tree shake | HitFeedback(Tree) | 실제 harvest damage | resource | .16s | root 수명 | NetworkResource HP/Hits | 실제 나무15 hits·목재6 줍기, 초기 replica quiet |
| stone/ore shake | HitFeedback(Stone) | harvest damage | resource | .16s | root 수명 | 동일 | LAN 실제 Harvest→HP/receipt, 중복 없음 |
| creature death squash | HitFeedback.BeginDeath/LootOnDeath | 확정 사망 | actor | pose .22s/정리 .35s | reset | server death | 시험/보스 guard kill; drop 반복 없음 |
| melee hand/tool arc | PlayerCombat/CharacterFacing | accepted attack | player | 정의 duration | 재사용 pose | AttackState/time | 실제 guardian melee/회귀; guard 수정자 표현은 DEFER |
| enemy windup/strike | EnemyController/facing | brain attack state | creature | 정의 windup/recovery | reset | creature frame | 관련 전투 회귀·LAN 권위 유지 |
| slingshot draw/band | SlingshotAttack/LineRenderer | Begin/release | goblin | 정의 draw/recovery | reset | shot state/time | authoritative projectile1 회귀 통과 |
| bow/crossbow pose | PlayerRanged/facing | aim/release | player | 정의 draw | transient reset | 기존 action/projectile | 관련 source/시험, 신규 trail 없음 |
| ammunition sprite | StoneProjectile | Launch | shot root | collision/range | 기존 pool | NetworkProjectile | 599×8 실제 반복; network 중복 spawn 없음 |
| bare-wall impact burst | 없음 | 없음 | 없음 | 없음 | 없음 | 전용 receipt 없음 | 존재하지 않는 효과/원격 hit를 만들지 않음 |
| pickup hop/bob | WorldPickup | Initialize→collect | drop root | pickup/despawn | 기존 pool | NetworkGroundItem | 실제 wood6 수집·LAN core 소유권, 반환 상태 bounded |
| placement preview | PlayerPlacement | placeable 선택 | local player | 선택/Running 동안 | 단일 preview | owner local | 실제 valid preview→1개 소비/설치; invalid 회귀 |
| placement/crafting puff | 없음 | 없음 | 없음 | 없음 | 없음 | 없음 | UI/state/audio 보상이며 새 시각효과 추가 안 함 |
| door motion/barrier | WorldDoor | Toggle | placeable | 기존 전환 시간 | 시설 수명 | building state | source/관련 scene·persistence 회귀 |
| hand/back torch light | PlayerLightSource/LocalFirelight | 선택/장착 | player/light | 장착 동안 | 단일 light | 기존 장비 | source/기존 계약; 이번 실기기 GPU 검증 아님 |
| facility/world light | FacilityGlow/WorldLighting | clock/region | world/placeable | 씬/시설 | 지속 component | world clock | 실제 밤/묘지 렌더링; 새 light 생성 루프 없음 |
| health/boss bars | WorldHealthBar/BossHud | bind/Changed | scene UI/actor | bind 동안 | 재사용 UI | HP/identity | 첫 HP 복원·boss 해제/scene 회귀 |
| death/down/revive presentation | PlayerDeath/DeathHud | life state | player/scene HUD | life state | 기존 actor | PlayerStatus | 실제 Dead HP0→Free Home→Alive HP100/Home |
| class quarry marker | ClassWorldFeedback | 유효 quarry | player child | 대상 유효 동안 | 단일 sprite | class state | source; 모든 class의 실제 시각 검수는 아님 |
| boss awakening/phase/pose | UndeadKingVisual | encounter state/age | boss child | encounter | encounter 소유 | BossFrame | 실제 phase/mark/return/death 캡처 |
| boss fan warning | BossTelegraph.Edge | Windup | boss local | windup만 | 점 배열67 재사용 | attack/direction/started | 실제42점·local space, LAN 방향/시각 일치 |
| boss line warning/crack | BossTelegraph | line windup/strike/recovery | boss local | windup / strike·초기 recovery | 동일 | 동일 | 실제4점·crack 전환 확인 |
| boss Crownfall circle | BossTelegraph | circle Windup | boss local | windup | 동일 | 동일 | 실제41점·local space·enabled 확인 |
| boss crown/dust/skull | UndeadKingVisual | phase/state/marked | boss sprite | 해당 상태 | 기존 renderer | boss state/target | 실제 렌더링·LAN mark transfer/clear |
| boss minion presentation | BossMinion/EnemyController | wave | add actor | wave→retire/reset | 기존 pool | authority spawn | solo2/LAN3, wipe retire·일반 재사용 회귀 |
| persistent BGM/ambience/voice | AudioRuntime/AudioLandscape | context/confirmed event | DDOL owner | scene context/clip | voice25+landscape5 | 기존 semantic state | F2 수정; owner1/source30, death/victory 각1 |
| dash trail/hit stop/camera shake | 별도 구현 없음 | 없음 | 없음 | 없음 | 없음 | 없음 | 카메라 follow/region snap은 존재. 새 효과 추가 안 함 |
| ParticleSystem/TrailRenderer | 현재 효과 자산/런타임에서 미사용 | 없음 | 없음 | 없음 | 없음 | 없음 | soak particle0·관련 scene 계약 유지 |

## 13. Combat VFX

관련 melee/적 windup/projectile/death 회귀를 실행했다. 실제 hold attack UI로 성숙 나무 HP150을 15×10으로 소진하고 render를 확인했다. boss fixture는 실제 command melee로 guardian을 처치했다. 첫 manual timing 표본 .333초에는 MCP 실행 프레임 stall이 섞여 있어 .28초 punch timing의 기기 성능/애니메이션 결함 근거로 사용하지 않는다. 이후 시각 크기·hit stop·음색 취향은 수정하지 않았다.

## 14. Gathering VFX

나무 타격/소진 후 Wood6가 바닥에 생성됐다. 공격 거리와 pickup 반경이 달라 처음 수집0이었고, 실제 motor로 drop에 걸어가6개를 수집했다. 이를 드랍 소실로 분류하지 않았다. 그 자원만으로 UI callback에서 Workbench를 제작해 Wood6→0, preview valid, placement command1회로 소비1/설치1을 확인했다. stone/ore는 이번 LAN의 실제 Harvest/replica 상태와 관련 시험으로 검증했으며 동일 수작업 진행을 모두 반복한 것은 아니다.

## 15. Boss VFX

현재 Lifecycle fixture의 **33 PASS**와 실제 화면/상태를 확인했다. awakening, phase2/3, adds/mark, fan/line/circle windup→strike/recovery, return, death/loot, 다음 밤을 관측했다. Crownfall은 별도 phase3 HP/teleport/healing fixture에서도 실제 scheduler가 선택한41점 circle을 캡처했다. [fan](boss-fan.png) · [line](boss-line.png) · [circle](boss-circle.png).

fixture는 seed9472026, teleport, Steel 장비/Bone50, healing/direct damage, 복제한 QA reward table의 crown proc 강제를 사용한다. 자연 보스 clear 시간/난도 검증이 아니다. 원본 reward20%와 제품 asset은 수정하지 않았다. 창 Focus/Show 도구를 호출하는 기존 Visual scenario는 사용하지 않았다.

## 16. Network duplication

QA 복사본에만 passive Health probe를 추가하고, 기존 resource harness에 부분 채굴 자원1개를 남겼다. baseline은 Host8/Client9, 추가1은 `join_resource` 최초 프레임의 HP49/50이었다. 최종은 **Host8/Client8**, 객체 ID/name/HP별 multiset까지 일치하고 초기 프레임 피격0이다. HP 감소+hit receipt가 같은 snapshot에 있어도 한 번이다. 이번 workload 밖의 모든 동시 hit를 보장한다는 주장은 하지 않는다.

## 17. Scene / Continue / reload

실제 New Game/Save and Home/Continue callback을 사용했다. 첫 3회에서 제작대1개가 유지됐다. 고정1초 대기의 월드 표본 일부는 chunk 복원 중이어서 steady state 비교에서 제외했다. 이후 **pending chunk/terrain work가 끝난 3회**를 따로 기록했다. Home 구독11/죽은 대상0/owner1/source30 유지, 잘못된 victory0이다. 별도 복원 완료 시 resource414/chunks9/pending0/terrain work0도 확인했다. 최종은 **MainMenu, Play OFF, scene dirty=false, QA flag 원복**이다.

## 18. UI runtime health

직전 interaction priority/crafting selection7개가 통과했다. 실제 crafting/pickup/placement/사망/Home/Continue를 확인했다. disabled 광고/재화 revive를 무료 성공으로 꾸미지 않았다. 사망 관측기는 runtime listener를 persistent listener로 잘못 조회해 예외가 났다. `DeathHud.Home.onClick`으로 교정했고 Editor pause를 해제해 다시 관측했다. 이 QA 오류와 MCP websocket 경고는 제품 결함 수에 넣지 않았다. 기록 보존 후 Console을 비웠고 마지막 씬 왕복에서 warning/error0을 확인했다.

## 19. QA / debug contamination

Windows 입력/Focus/Show를 사용하지 않았다. Editor harness를 제품에 설치하거나 prefab/scene을 regenerate하지 않았다. LAN passive probe는 **별도 QA 프로젝트에만** 존재하며 canonical/live 프로젝트에 복사하지 않았다. 첫 LAN 시작 파일 sharing race, 긴 QA 경로의 import 문제, Editor 폴더 probe attach 실패는 QA 준비 실패로 구분했다. probe가 없던 run을 피격 계수 검증으로 계산하지 않았다. prefab/data/native SDK/운영 설정은 변경하지 않았다.

## 20. Profiling evidence

| 측정 | 결과 | 제한 |
|---|---|---|
| 실제 Editor movement360 frames | frame mean7.525ms / P9511.304ms / max27.612ms; main7.499ms | Intel Iris Xe, Editor/MCP 포함; Android/저사양 FPS 아님 |
| 같은 구간 GC | mean312,084 / max839,425 bytes/frame | Editor 포함, allocation stack 귀속 미측정 |
| 이동/월드 | (0,0)→(17.869,0), active/max chunks9, resources354/creatures53, terrain pending0 | motor/physics 정상 이동, input을 MCP command로 공급 |
| 20분 반복 | main mean6.374ms/max84.950ms; GC mean60,693/max436,307 bytes/frame | 통제된 idle+burst workload. QA 복사/두 Editor import 등 외부 작업도 병행 |

## 21. Long-running check

**실제 벽시계1200.006초**, UTC09:50:23→10:10:23, 187,777 frame/599 burst. burst마다 기존 projectile8과 Berry pickup1을 생성/소진했다. 시간 가속 결과를20분으로 환산한 것이 아니다. AI/motor/survival/clock을 통제했고 위치·아이템 fixture를 사용한 **baseline soak**다. 최종 수정 후의 별도20분 run이라고 주장하지 않는다(이 pool workload의 제품 경로는 바꾸지 않았고 변경 경로는 focused/LAN/scene 회귀).

모든 표본에서 pool cohort9, audio owner1/source30, player Damaged subscribers7, particle0. managed heap857,694,208–979,996,672 bytes는 GC sawtooth이며 native allocated461,214,390–462,787,070 bytes 범위였다. 단조 증가 누수를 관측하지 않았다는 범위의 결론이며 무누수 증명은 아니다.

## 22. LAN

최종 실제2 Unity Editor 프로세스/LocalNetwork127.0.0.1 transport가 완료됐다. resource observer set/HP/late join/chunk revisit, harvest tombstone, boss offering rollback, same-night 중복 방지, timestamp/direction telegraph, phase/adds/mark transfer, wipe/return/respawn, reward once/core pickup owner, disconnect/rejoin/rehost를 기존 harness로 검사했다. **Host/Client complete**, source 최대30, BossDeath/BossVictory 각1. headless라 두 화면의 pixel/청취 비교는 하지 않았다. 이번에는 Relay/기기 LAN을 실행하지 않았다.

## 23. Actual vs simulated

| 종류 | 실제 수행 | 하지 않은 주장 |
|---|---|---|
| graphical Unity MCP | fresh UI 시작, motor 이동, tree hold attack, pickup 이동, craft/place, Save/Home/Continue, controlled death/Home, boss 렌더 관측 | 인간 터치·전체 자연 progression·자연 boss clear |
| 자동 tests | 최종 unique **EditMode35 / PlayMode45**, validator0 errors | automated pass를 플레이 체감 합격으로 바꾸지 않음 |
| 두 프로세스 LAN | 실제 transport·replica·event·geometry flags | 사람2명 화면 비교/Relay network quality |
| 20분 soak | 실제1200초, 통제된 burst | 자연 생존20분·Android 성능 |
| source audit | critical 경계 정독+442파일 triage | 모든 코드의 결함 부재 증명 |

initial Edit35는34 pass/1 stale count 실패, initial integration Play33은29 pass/4 실패였다. baseline 재현·위 fixture 교정 후 실패 대상만 다시 실행했다. 첫4개 rerun은3 pass/1 offscreen tool card 실패였고 그 마지막 대상을 기존 scroll helper로 고쳐1/1 통과했다. focused12도 통과했다. 이전 실패 기록을 지우거나 합계에 중복 가산하지 않는다.

## 24. Explicitly NOT changed

전투 체감/시각 polish/밸런스/progression/UI design, Creature/Loot/Material/Recipe111/Station, Undead King 콘텐츠/공격 수치, 광고·IAP·backend 운영 설정, save/schema/protocol, native SDK와 Android manifest/Gradle은 변경하지 않았다. AAB/build/signing/upload/production activation/다음 polish를 시작하지 않았다.

## 25. Versions

**Save14 / Profile2 / CloudProfile1 / Economy2 / Protocol23 / generation3 / IAP schema1** 유지. 코드의 replica prime bool은 private transient 상태다. 새 serialized/persistent/network field가 아니며 migration을 만들지 않았다.

## 26. User save protection

격리 QA directory/peer 프로젝트만 사용했다. real live/canonical UserData 및 LocalLow의 기존 world primary+backup **2파일**은 SHA256·mtime_ns·size가 전후 완전히 같고 새 실제 profile도 생기지 않았다. Unity SDK 자체 cache/TestResults는 이 save fingerprint 대상과 구분했다. QA flag는 원래 빈 값으로 복원했다. save payload/credential은 Git에 넣지 않았다.

## 27. Modified files

제품: `Runtime/Combat/Health.cs`, `Runtime/Presentation/AudioLandscape.cs`.

시험: `Tests/PlayMode/ReplicaFeedbackPlayTests.cs`(+meta), `AudioPlayTests.cs`, `NetworkCreaturePlayTests.cs`, `NetworkPersistencePlayTests.cs`, `SurvivalLoopPlayTests.cs`, `Tests/EditMode/NetworkCreatureContractTests.cs`.

문서: `docs/COMBAT_NAVIGATION.md`, `docs/AUDIO.md`, 이 REVIEW, compact evidence JSON과 선택한3개 실제 화면. 전체 raw captures/QA 복사본/Library/로그는 저장소 밖에 둔다.

## 28. Remaining technical risks

Android sustained frame/memory/thermal, native audio focus·touch·ad/IAP 실기기, Relay jitter·4인, 모든 class modifier의 원격 시각 동기화, 실제 온라인 UI 지연 completion은 아직 직접 검증하지 않았다. 기존 외부 설정 blocker를 해결한 것처럼 표시하지 않는다. 이번 workload에서 확인한 제품 P0/P1은 남아 있지 않다. 이것은 모든1.0 콘텐츠/기기/실패 조합의 전수 검사 결과가 아니다.

## 29. Internal Test Build readiness

**READY (이번 기술 범위)**. F1/F2 수정, 관련 회귀80 unique/validator, 실제 UI/씬/보스, 두 프로세스 LAN, baseline20분 bounded run, 사용자 save 보호 근거가 있다. 후속 내부 빌드에서는 기기 성능/GC·터치·오디오와 DEFER class/온라인 경계를 우선 확인한다. 공개 출시 readiness와 운영 활성화는 기존 RELEASE_BLOCKERS를 따른다. 이 패스는 여기서 STOP한다.

시간은 evidence의 벽시계 단계로 기록한다. 병행 soak는 총 시간에 중복 합산하지 않으며 도구/재개/관측기 교정 대기를 포함한다. 작업별 토큰 수는 제공되지 않아 null이다.
