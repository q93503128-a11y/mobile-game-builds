# 다음 재개 지점 — 사용자 요청에 따른 중간 정리

**전체 production pass 미완료. 커밋/push 전이며 main의 로컬 변경을 보존했다.**
이 기록은 최종 완료 보고가 아니다. 다음 작업은 Undead King 패스의 남은 검증이며 Mount 등 다음 기능은 시작하지 않는다.

## 환경 / 기준

- 정본 repo: `C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical` (상위 폴더도 별도 Git repo이므로 반드시 canonical에서 Git 실행).
- 시작 fetch 후 main SHA: `82084f79bcf8cce11454b21ef67e79a3174bb82d`. 중간 종료 HEAD도 동일.
- 최신 요청: `C:/Users/User/.codex/attachments/df407cb2-0383-439e-85a0-2cedc5d8f3cc/붙여넣은 텍스트.txt`.
- 재개 시 remote main을 fetch하고 HEAD/작업 변경과 비교. 로컬 변경 reset/덮어쓰기 금지. AGENTS/README/관련 정본 다시 확인.
- Live Unity6000.3.19f1: `C:/UnityProjects/Wildfolk`, MCP `Wildfolk@1367f605af2f8ba4`.
- Windows 입력/Focus/Show 금지. 기존 VisualGameView는 열린 Game View를 Repaint할 뿐이다.
- 사용자 save는 사용하지 않는다. `user-save-before.json`과 `user-save-after.json`의 SHA256 및 mtime가 동일.

## 현재 구현

BossDefinition/Attack config → BossEncounter(상태·phase·공격·territory·소환수) → BossWorld(밤 identity·소환 transaction·보상 저장), NetworkBoss/NetworkBossWorld replica를 추가했다. 기존 EnemyController에 boss 이름 분기를 넣지 않았다.

왕묘는 기존 Graveyard guardian의 cleared feature로 영구 해제. Bone10, Night, world-wide boss별 lastNight로 성공 시 즉시 그 밤 사용. 처치/전멸/저장으로 같은 밤 재소환 불가.

HP9000, 이동2.1, 피해26/41/21/26/36, windup .8/1.15/.75/.95/1.3. Phase70/35%, Phase2 add2/3/4/5, mark은 wave 전멸까지 유지, 이동만1.11배. Phase3 새 wave 없음. Boss 본체 player-count scaling 없음.

Returning grace4초, 속도.78배, 외부 피해 gate, 2초 뒤 초당1% regen, 살아 있는 플레이어 재진입으로만 재개. 무작위 undead suppression은 encounter 내부. 왕묘 중심10유닛의 나무/돌 materialization을 비워 전투 중앙 가림 완화(광물/플레이어 건축물 제거 없음).

Original Pillow15PNG, 분절형 King/Tomb/core/crown. 기존 boss bar/time HUD 재사용, transition1.6초 강조. Item136/Recipe111, Save14(migrate13), Protocol21. Generic accessory index3, one-slot exchange/save/network/anchor visual. Crown20%, HP<=35% 이동+8%/incoming−10%, offense 없음. Core1 guaranteed. XP120/Gold35; Bone14–20/Iron2–4/Steel1–2.

MainMenu Class 표시 및 LocalizedLabel 안전한 unsubscribe 추가. 기존 언어 오류는 fresh scene transition 시 처음 재현되지 않았고 방어 수정의 반복 회귀는 아직 남았다.

## 완료된 검증 / 근거

- checkpoint.json / boss-editmode.xml / boss-playmode.xml: 관련 EditMode42/42, PlayMode2/2 최종 통과. 자동 검사는 실제 자연 클리어가 아니다.
- PlayMode 최초 실패는 서로 다른 격리 폴더 사이 SceneFlow.Repository cache 공유. QA 시작 시 SaveRepository.Default()로 갱신해 원인 수정.
- Source validator PASS(1467GUID), ProjectDataValidator0오류, git diff --check PASS.
- 실제 MCP Lifecycle 전부 PASS: guardian 실제 근접 처치, unlock/day no-consume/Bone10/한밤 차단/phase2 2adds/mark/phase3 기존wave 유지/clear/귀환·점진 regen/재진입/낮전환/active save/load/death pending/one core/forced Crown path/resolved reload no duplicate/다음밤 소환.
- Lifecycle은 HP 강제전환/제어된 healing을 쓴 기능 smoke이며 Steel 자연 클리어 성공으로 보고하지 말 것. 희귀 보상 path는 복제한 QA table만100%; 정본20%.
- Actual MCP Visual flow completed; 1920/1280 tomb/cleave/phase2/mark/crownfall/death/loot 캡처. 초기 장신구4칸 수평배치가 hotbar와 겹쳐 SP/MP scene 양쪽 2×2로 수정. 장비 filter도 Equippable로 통일. 2×2 accessory 화면은 최종 재확인했다.
- Live evidence: `C:/UnityProjects/Wildfolk/Captures/undead-king-boss/`; canonical screens/에 캡처 보존.
- 연구3게임 및 선택/거절 근거 REVIEW.md 표: Terraria Skeletron, Valheim Bonemass, Core Keeper Hive Mother. 코드/아트 복제 없음.

## 다음에 반드시 마칠 것 (우선순위)

1. 실제 두 프로세스 LAN. `Editor/BossLanQa.cs`는 컴파일되지만 **아직 실행하지 않았다**. 기존 ClassLanQa의 transport/file handshake 패턴 재사용. parent helper `boss_run_lan.py`로 hidden batch client 실행, Live MCP에서 BossLanQa.Host(). 네트워크 root/prefab mapping 이미 author됨. 초반 domain reload 시 tool false/null은 미실행일 수 있으므로 준비 상태 뒤 Start. 현재 LAN fixture는 2인 summon ownerRPC/3adds/mark transfer/wipe/return/accessory/reward/reHost를 검사하도록 작성했으나 fixture 및 production 실패 가능성을 실제 분석해야 한다. Client mark 늦은 spawn binding retry, 공개 BeginReturn/RollReward authority gate는 보강했다. Host world players cache는 authority에서만 조회해 client ConnectedClients 접근 방지.
2. Steel/Diamond **실제 보통 수치의** 스크립트 전투 측정. 현재3–5분 근거는 Steel43/.38=113.16DPS,35%uptime→227초 계산뿐. Diamond52/.38=136.84→188초 추정. 이론을 측정치로 쓰지 말 것. 기존 input commands와 유한한 salve를 사용하고 QA 자동힐/HP 강제값은 끈다. 실제 터치/재미 검증은 미수행으로 구분.
3. 기존 trap/turret 실피해·유효 target/LOS, 막힌 구조물 파괴, AFK 외부 공격 불가를 실제 검사. 코드만 읽고 완료로 말하지 말 것.
4. pending-death save reload / 저장 실패 rollback / 다시 load·reHost reward idempotency 강화. 현재 일반 XP/Gold+physical loot+resolved flag는 한 snapshot commit. ClassXP는 commit 이후 전달되어 crash window에서 유실 가능; 중복 physical/common reward는 방지하나 account/world cross-store atomic 보장은 없다. 이를 검토·정확히 한계 기록.
5. language transition 반복 회귀, actual Class menu label, accessory crown visual/threshold UI/MP. 최종 캡처에서는 mark가 근접 보스와 겹칠 수 있으므로 anchor 높이/가독성 검토. 영문 Crown 설명의 At35/move8/take10 띄어쓰기도 정리. Crownfall 등 캡처는 windup 초반이라 몸/검 전조 중간 프레임도 더 보기.
6. 최초 랜덤 새 월드 seed `-324239329`에서 WorldProgressionSupply.Build rule6 source repair exhaustion 발생. seed9472026은 정상. Boss 변경과 무관하다고 단정하지 말고 catalog134 clone과136을 같은 seed로 비교(새2item은 recipe 없어 economy budget은 이론상 동일). 필요한 경우 별도 기존 문제로 기록하되 범위 확대 금지.
7. 관련 현재 canonical docs (README, DESIGN_BIBLE, ARCHITECTURE, BALANCE, CREATURES_AND_LOOT, WORLD_GENERATION, INVENTORY, SAVE_PERSISTENCE, CHARACTER_PROGRESSION, SOURCE_ART 등) **아직 최종 갱신 전**. 현재 HEAD의 수치/미구현 문구 stale. REVIEW의 모든28항/최종24항 보고를 사실에 맞춰 마무리. 과거 review 기록은 덮어쓰지 않는다.
8. 수동 audit: authority/reward doublefire/minion cleanup/phase3summon/return/turrets/accessory duplicate/stale callbacks. 필요한 검사만 반복. 실제 Relay/Android/4device는 미수행 명시.
9. git status → diff 검토 → main commit → push → remote==HEAD → clean. 그 뒤 STOP. 현재는 커밋하지 않았다.

## 도구 / 재개 팁

parent `boss_work.py`: 경로, sync(Assets의 cs/json/asset/prefab/meta/png만; .unity 제외), stamp, 원본 save hash. `boss_pull.py`: Unity에서 author한 지정 data/prefab/씬과 누락meta를 canonical로 pull, YAML 순서 유지. `boss_tests.py EditMode|PlayMode`: canonical Assets를 별도 tests/artifacts/unity로 복사 후 hidden Unity batch, XML 출력. 한 test project에서 동시 Unity 프로세스 금지.

**열려 있는 scene 파일을 canonical→Live로 복사하지 않는다**(blocking Reload modal 발생). Scene 변경은 Unity Editor author 후 pull. execute_code는 CodeDom C#6. 새 소스 sync 뒤 실제 AssetDatabase.Refresh(ForceSynchronousImport), console 확인. manage_editor Play 직후 한 번 query해 domain 준비 후 mutation.

`BossProductionQa.Start("Lifecycle"|"Equipment"|"Visual")` 격리 QA. `Start`는 Play+DevelopmentProfile.Isolated 필요. QA SessionState Wildfolk.QA.Directory 먼저 설정. `ContentExpansionQa.Prepare()`는 repo 경로가 해당격리폴더인지 검사, autosave/clock/AI/motor/input 비활성화. 실제 combat bot은 motor 재활성 필요. PlayerDeath 상태가 Dead면 단순 HP 회복 대신 HomeRespawn 재사용.

원본 art 실행 Python: `C:/Users/User/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe` (Pillow). 일반 Python 실행 UTF8. 시간기록 timing.json은 겹치는 도구 실행 합산 금지. 단계별 모델 token 사용량은 도구에 노출되지 않아 수치 미기록.
