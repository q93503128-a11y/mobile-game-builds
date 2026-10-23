# 다음 재개 지점 — 두 번째 요청 중단

전체 Undead King production pass는 아직 완료하지 않았다. **main 로컬 변경 보존, commit/push 전**이다. 다음 재개는 아래 마지막 감사/검증/문서 마감이며 Mount 등 새 기능을 시작하지 않는다.

## 재개 전

1. Repo `C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical`에서 반드시 `git fetch origin main` 후 HEAD/remote와 로컬 변경 비교. 현재 둘 다 `82084f79bcf8cce11454b21ef67e79a3174bb82d`. 상위 폴더도 별도 repo이므로 canonical에서 Git 실행. reset/덮어쓰기 금지.
2. projects/wildfolk/AGENTS.md, README와 관련 정본 읽기. 원래 요청 `C:/Users/User/.codex/attachments/df407cb2-0383-439e-85a0-2cedc5d8f3cc/붙여넣은 텍스트.txt`.
3. Live Unity `C:/UnityProjects/Wildfolk`,6000.3.19f1/MCP Wildfolk@1367f605af2f8ba4. Windows 입력·Focus/Show 금지. 모든 QA는 SessionState Wildfolk.QA.Directory 격리. 원본 save hash와 mtime 불변(checkpoint-latest.json).

## 이번 재개에서 추가 완료

- 실제2프로세스 LAN: owner Bone10 소환/단일 boss replica/HP9000·speed2.1/phase2 3adds/mark replica 및 Downed 이전/full-party wipe→Returning·gradual regen/기존 Home respawn/재진입/owner accessory equip/공유Core1·Crown 최대1roll/각 생존 참가자 XP120·Gold35 1회/Host save·reHost/소모한 밤·장신구·loot 중복 없음 통과.
- 최초 LAN 실패1: QA Survivor의 innate 치명상 생존 효과가 Downed를 막았음. Explorer 무노드 fixture로 수정. 실패2: NetworkBoss 초기화가 Spawn 전이라 local Time→server Time epoch가 달라 awakening 지연. listening singleton server clock을 pre-spawn부터 쓰도록 production 수정, 실제 LAN에 pre-spawn clock assertion 추가 후 통과.
- 최초 Diamond trial은 왕묘 모서리에서 boss와 입력 bot이 멈춰 측정 중단. 공용 boss Chase도 Returning과 같은 bounded sidestep 후보를 사용하도록 수정했고 bot도 local avoidance 사용. 수정 후 ordinary-stat Steel/Diamond clear는 resumed-verification/*-result.json과 checkpoint-latest.json 참조. 최초 Steel257.4641초는 수정 전 별도 trial. 측정은 game time이며 TimeScale2, 실제 모바일 수동 플레이/손맛 합격은 아니다. 기본HP100/무투자 Survivor·Steel방어8/Diamond방어10, 유한 연고12개, 보스HP 강제전환/자동힐/전투 중 teleport 없음.
- 관련 EditMode42/42, PlayMode3/3 통과. 새 Transactions test는 offering write failure Bone/night/actor rollback, pending-death exact roll reload, resolved reload no duplicate, reward write failure XP/Gold/pickup rollback 및 retry once를 검증한다. minion pool 재사용 시 일반 AI/lifecycle/loot/speed 복원을 보강하고 Lifecycle에 회귀 assertion 추가, 최신3/3 통과.
- Actual MCP Defense PASS: Spike16/BoneTrap32, Ballista/Repeater 실포탄 피해와 각1탄 소비, wall LOS, 실제 외부 포탄이 Returning HP/state를 바꾸지 않음, 막은 wood wall을 일반Health 피해로 파괴. 이 기능 fixture는 보스 정지/controlled healing을 사용하며 balance clear와 다르다.
- 언어 Gameplay/Home/Gameplay KO/EN3회 통과, visible MainMenu Class screenshot 확인, console0. 초기 UI QA는 inactive label의 serialized Class Journal까지 검사해 실패했으나 활성 라벨만 검사하도록 fixture 수정(숨긴 화면은 OnEnable에서 갱신되는 정상 동작). 기존 오류는 fresh transition에서 재현되지 않아 localized-label unsubscribe 방어 수정과 반복 회귀를 기록한다.
- Crown 영문 설명 띄어쓰기, Skull Mark anchor1.95→1.25, off-arm/legs neutral pose reset, Primary accessory icon, BossHud 언어 event 갱신 보강. 2×2 equipment layout은 앞 구간에서1920/1280 확인.
- README/기획/구조/수치/콘텐츠/creature/world/inventory/save/character/art 정본을 실제136/111/14/21과 boss 정책으로 갱신했다. REVIEW 최종 마감은 아직 남음. 과거 review는 보존.

## 남은 순서 — 여기서 재개

1. **마지막 수동 감사 후보를 먼저 확인/수정:** BossWorld.Summon 실패 경로에서 Release(spawned)가 NetworkBossWorld.Publish를 호출한 뒤 records를 prior로 되돌린다. 서버 Bone/night rollback은 통과하나 Client offering mirror가 잠깐 잘못 남을 수 있다. prior record를 먼저 복구하고 release/publish하도록 순서를 검토한 뒤 관련 transaction 검사. RollReward eligibility도 disconnected network actor의 IsSpawned/접속 조건을 명시적으로 확인하면 좋다(현재 Resolve는 authority Players cache로 접속자를 거른다). 아직 이 후보 수정은 적용하지 않았다.
2. **최종 실제 LAN 재검사:** Editor/BossLanQa.cs에 새 `telegraph` handshake(같은 attack index/server timestamp/direction와 Edge.enabled 검사)를 작성·컴파일했지만 아직 실행 전이다. 앞서 통과한 LAN은 phase/mark/권위/보상/reHost까지이며 이 새 assert를 수행했다고 보고하지 말 것. 필요하면 reHost 후 inactive same-night Bone no-consume를 근접 조건에서 추가해 경계를 더 직접 검사. Parent boss_run_lan.py는 hidden batch client, Live MCP BossLanQa.Host()가 Host. 해당 test project가 다른 Unity test에 열리지 않았는지 먼저 확인. client 초기화 약60초. Host를 먼저 실행해 current-run.txt 갱신 후 Client 실행.
3. ResourceNetworkState fixed-string read OverflowException/NetworkStone size mismatch가 완주한 LAN client 로그에서1회 발생했다. 보스 wire가 아닌 수정하지 않은 resource 경로이며 원인 확정 전. 증거 resumed-verification/resource-replication-anomaly.txt. 다음 LAN에서 재현 여부 확인하고 원인/미해결 한계를 정확히 기록. 기존 문제라고 단정하거나 clean-console LAN이었다고 말하지 않는다. 이번 범위를 대형 resource networking 재설계로 확대하지 않는다.
4. seed -324239329 generation rule6 source repair exhaustion은 catalog134 clone과136 모두 같은 예외를 재현. generation-comparison.txt. 해당 Build 소스는 수정하지 않았다. 별도 기존 generation 한계로 기록하고 boss 범위를 확대하지 않는다.
5. 마지막 화면(변경한 낮은 mark anchor/중간 windup 포즈/Returning bar/crown description) 필요한 부분만 갱신. 기존screens는 대부분 전 구간 캡처다. MainMenu Class 새 캡처는 이번 구간 것이다. 자동/controlled fixture/ordinary-stat input bot/actual LAN/physical mobile을 분리.
6. REVIEW의28항과 최종24항 보고 마감. HP9000 및26/41/21/26/36, .8/1.15/.75/.95/1.3, phase70/35, adds2/3/4/5, 원본아트15PNG, Bone10/world boss별 한밤1회, Core1/Crown20%, Save14/Profile1/Protocol21. 보상 economy/범위/기존 turret DPS·healing·class/dash 근거 포함.
7. ClassXP는 world commit 뒤 별도 로컬 profile에 전달하므로 강제 종료의 두 저장소 사이 유실 window는 남는다. normal world physical/common reward duplication은 거래 테스트에서 방지. Relay/Android/실제4device/수동touch/장기경제/active mark 중 disconnect는 아직 실제 검증하지 않았다.3/4P add 수는 model tests이며4device라고 하지 않는다.
8. 필요한 관련 검사만 재실행, meta/source/data/diff check, 원본 save hash+mtime 확인. main diff 검토 → commit → push → remote SHA==HEAD → clean → STOP. 현재 commit/push 전이다.

## 파일/도구

- previous full architecture/run context: CONTINUE-first-pause.md. 최신 결과: checkpoint-latest.json, resumed-verification/, boss-editmode.xml, boss-playmode.xml, timing.json. 단계별 token 사용은 도구에서 제공되지 않아 null이다.
- parent helpers(정본 밖): boss_work.py(sync/stamp/hash), boss_pull.py(지정 authored data/prefab/scenes와 missingmeta pull), boss_tests.py(EditMode42 filter/PlayMode3), boss_run_lan.py(hidden Client). BossCombatQa.Start("steel"|"diamond"), BossProductionQa.Start("Lifecycle"|"Transactions"|"Defense"|"Ui"|"Visual"|"Equipment")은 Editor-only 격리 Play용.
- .unity 파일을 canonical→Live로 복사하면 Reload modal이 떠서 금지. Editor에서 author 후 pull. Script sync 뒤 AssetDatabase.Refresh(ForceSynchronousImport), compile/console 확인. Play 직후 domain 준비 query 후 mutation. execute_code CodeDom은 C#6이다.
- pause 후 에디터는 MainMenu/Play off로 정리한다. 다음 재개에 QA Directory를 다시 설정한다. 사용자 save/Profile을 QA에 사용하지 않는다.
