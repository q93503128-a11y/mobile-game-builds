# Class Foundation — 사용자 요청으로 중단한 재개 지점

> 재개 후 구현·검증 완료. 현재 결과는 [REVIEW 최종 절](REVIEW.md)을 따른다. 아래는 중단 당시 상태를 보존한 기록이다.
2026-10-22. **구현/검증 진행 중이며 완료 판정·commit·push 전이다.** 사용자가 이번 검증 지점을 정리하고 다음에 이어가도록 요청했다. 과거 creature correction이 아니라 최신 첨부의 **Character Progression + Class Foundation** 작업이다.

## 기준 / 위치

- 시작 시 remote fetch 확인 SHA: `cbe3c14115ee5529eed7babd062df801e7981b99`. main 직접 작업. 현재 HEAD도 동일하며 많은 미커밋 변경을 보존했다.
- canonical checkout: `C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical`.
- Live Unity: `C:/UnityProjects/Wildfolk`, Unity6000.3.19f1. MCP instance `Wildfolk@1367f605af2f8ba4`를 pin한다.
- 최신 실제 요청: `C:/Users/User/.codex/attachments/f0183b60-eb13-4277-b5d6-544c0bca23f2/붙여넣은 텍스트.txt`. 전체 요청을 읽고 마지막50항목 보고/금지사항을 따른다.
- 시작 시 remote main을 다시 fetch하고 현재 dirty main과 비교한다. 미커밋 구현을 reset/checkout으로 지우지 않는다. PR/임시 branch 금지.
- AGENTS/README 및 CHARACTER_PROGRESSION/SAVE_PERSISTENCE/ARCHITECTURE/BUILDING/BALANCE/MULTIPLAYER_FOUNDATION을 읽는다. Unity skill 적용. Windows 입력과 Unity Focus/Show 금지. 실제 사용자 save 수정 금지.

## 구현한 범위

- 무제한 선형 Level, 구4Skill 보존+Metabolism/FieldCare, 6Skill 최대rank10/3/3/3/5/3. 구 Stride reciprocal 비용 보존. 큰XP는 구간합/이진탐색.
- Classes.json:6Class/54node, 클래스별 로컬 durable profile(소유/XP/선택/배분/receipt), milestone50/180/400/750/1200/1800. 현재6entry이며 코드 max6 없음.
- Survivor lethal intercept/cooldown, Warrior held Guard/공격/정면·최종HP heavy, Hunter generic Quarry/창/원거리, generic Medicine·Medic 구조, Explorer category/관측표식, Engineer 수리/buffer/Overclock, 공용 안전 회수.
- Survivor무료, 4개Gold class각60, Engineer production 가격/provider 미확정으로 unavailable. 로컬 profile checksum은 인증 backend가 아니다. LocalAlphaClassAdmission은 소유/XP 로컬 claim 신뢰라는 한계를 문서화했다.
- Save13/Profile1/Protocol20. SessionClass/치명상 방지 cooldown/구매 receipt/PlayerBuilt 필드. Item134/Recipe111/generation3 유지.
- MainMenu6카드/9node UI, 실제6공용 node, MP Character/Skill UI,6종 자체 기하 아이콘. 생성기 `tools/generate_class_icons.py`.
- 캐노니컬 문서 초안 갱신. REVIEW 외부7게임 비교/결정/가격·XP 가정 산술은 구현 전에 작성했다. 최종 검수 결과 절은 아직 작성 중이다.

## 확인된 검사 / 아직 주장하면 안 되는 것

- `models-corrected.xml`: Class EditMode20/20.
- PlayMode 고유9개 통과(여러 관련 실행의 합집합): `flows-service-corrected.xml`에서5개, `flows-final-corrections.xml`에서Hunter/Medicine/Warrior/Lobby4개. **전체를 한 번에9개 통과했다고 말하지 않는다.**
- `flows-transactions-expiry.xml`: Lobby/Turret+Exploration2/2. Gold240→4개 해금, 중복 구매, 선행/point/무료respec, committed receipt 복구/차감 전 intent취소, 임시QA wallet7Gem경계, 한국어54node 영역, Overclock 만료, 표식 교체·만료.
- 실제 visible MCP에서 Lobby/Buildings/Warrior/Hunter/Medicine을 실행했다. 일부 screenshot은 HUD5Hz 갱신 이전이라 증거 개선이 필요해 Shot helper에0.3초 대기를 추가했다. `VisibleSequence`는 **아직 실행하지 않았다.**
- Python source validator PASS(1406 GUID), MCP ProjectDataValidator errors0. 마지막 컴파일 완료·Console error0·Play중지·MainMenu를 확인했다.
- 실제 2프로세스 LAN **부분 통과**. 물리적4인이나 전체 LAN 완료를 주장하지 않는다.
  - R1(Warrior Host/Medic Client): Guard 복제, self54HP/ally20→65 및 아이템1개, Downed치료 거절, GoldenTime약3.52초/재시도 중복 없음,3.2초 구조50HP/보호3초, session lock 통과. `actual-lan`의7/8번째 실행 TSV 참고.
  - R2(Explorer Host/Engineer Client): 관측표식 복제, 무료수리, buffer30/선흡수, 두 포탑1.35reload/1.45rotation 복제/무료ammo·module없음, 회수Wood2, 저장, 동일세션 reconnect가 다른Survivor claim에도Engineer유지 통과.
  - 최신10번째 실행은 R2 reHost의 Host 생성까지 진행 후 QA coordination `phase.txt` sharing violation에서 중단. **게임 코드 실패가 아니라 검사 파일 동시 읽기/쓰기 문제**다. Read helper를 FileShare.ReadWrite|Delete로 수정했고 마지막 컴파일은 통과했으나 재실행 전이다.
  - R2 reHost client복원/임시표식 미저장, R3 Hunter/Survivor, `FourActors()`와 PERF 구간은 **미실행**.

## LAN 중 찾아 고친 실제 문제

1. NetworkPlayer prefab의 NetworkClassActions/NetworkCommonGrowth 누락 → 실제 prefab 추가 및 pull 완료. 현재 canonical GUID/프리팹이 기준.
2. MP Building release가 completion latch를 풀지 않음 → Hold(null,Repair,false) 호출. 이후 연속 수리/보강 LAN 통과.
3. 회수 후 held 요청에서 Unity destroyed Component에 `?.GetComponent` 호출 → Unity null 비교로 교체. 이후 회수Wood2 LAN 통과.
4. MP scene unload 중 StaminaHud가 파괴된 PlayerLocomotion 참조 → Player/Session null guard. 이후 unload/reHost 구간 Console0.
5. Overclock의 원격 임시 상태/남은 시간 복제 추가. durable TurretSnapshot에는 넣지 않았다.
6. Quarry가 pool 재활성화된 다른 생물을 따라가지 않도록 CreatureMember.SpawnGeneration 비교 추가.

QA 자체에서는 staticPhase 조기완료 race, 살브 stack합침, population fixture 파괴, 클라이언트 HP/건물 spawn/revive해제 replication 대기, 자연회복 혼입, 표식선 위 teammate 배치, 파일 read sharing을 고쳤다. 초기 `flows-compile-corrected.xml`의 성공은 Phase race로 **무효**다. failed XML을 최종 성공으로 세지 않는다.

## 다음에 할 일 — 필요한 최소 검증 후 완료

1. 변경 보존 확인→remote fetch→AGENTS/docs/첨부 재확인. Source/data 우선. 현재 사용자 save hash/mtime 비교 기록을 재확인한다.
2. 부모 workspace helper `class_work.py`, `class_pull.py`, `class_run_lan.py`를 활용한다. **다른 class_*.py 구현 생성기는 비멱등성이므로 재실행하지 않는다.** `class_docs_finish.py`, `class_finalize_code.py`도 재실행 금지.
3. LAN 클라이언트 test project에 최신 canonical Assets를 반영한다. 최신 Read helper는 Live에만 sync됐고 headless 복사는 아직이므로 반드시 복사한다. `projects/wildfolk/tests/artifacts/unity` 사용. 동시에 이 project의 두 Unity process를 띄우지 않는다.
4. MCP pin→isolated SessionState `Wildfolk.QA.Directory` 확인→Play→domain reload가 끝난 뒤 `ClassLanQa.Host(2)`로 이미 통과한R1을 생략한다. Host의 새current-run 생성 확인 후 `python class_run_lan.py <새 label>`로 숨김 headless Client. Play 직후 즉시 execute_code하면 old domain 때문에 성공false/null이 나올 수 있다.
5. actual LAN R2 reHost/R3 및 FourActors를 끝낸다. `ClassLanQa.Phase`, run TSV/failed 파일, Console 확인. 게임 action은 실제 NGO ownerRPC이며 파일은 단계/ACK만 쓴다. headless 실행은 nographics여서 screenshot 주장 불가. Host visible 캡처는 실제 GameView.
6. CommonGrowth Client 투자/MP UI를 실제 LAN에서 추가 확인하면 좋다(현재 Host FieldCare3투자는R1에서 확인했으며 실제 Client 투자 end-to-end는 남음).
7. 관련 EditMode 회귀: ClassProgressionModelTests, CharacterPassModelTests, NetworkPersistenceModelTests, StabilityModelTests, PoiContentModelTests, DeathFoundationModelTests 중 영향 범위. 작은 수정마다 전체 회귀 반복 금지. 새 test는 사용자 검증 범위 안에서만.
8. `ClassFoundationQa.Begin("VisibleSequence")` 실제MCP·격리save로 실행, 화면1920×1080 확인. 성공뒤 PNG를 직접 열어 KO/owned/locked/tree/HUD overflow와 raycast/input 확인. CommonLevel1001 표시 포함. 아직 실행 안 된 검사/성능을 통과로 기록하지 않는다.
9. REVIEW 결과·한계·소요시간/usage와 README/docs의 진행 상태를 최종 정리한다. Android/Relay/실제4인/실제Ads·IAP/주관적재미 미검증 명시. 원본save hash/mtime 전후 동일 확인.
10. git status→수동diff→main commit→push→remote SHA 확인→HEAD일치→clean. **현재 commit/push는 하지 않았고 worktree는 dirty다.** 최신요청50항목 보고를 실제 결과대로 작성한다. 다음 별도 콘텐츠 작업은 자동 시작하지 않는다.

## 실행 주의

- sync는 `.unity`를 복사하지 않도록 수정했다. 열린 scene 파일을 밖에서 덮으면 Unity Reload modal이 MCP를 막는다. Scene은 Editor authoring→class_pull 순서.
- 새 script/meta는 Live import→class_pull 후 headless copy. GUID를 각 프로젝트가 따로 만들게 하지 않는다.
- refresh scope **all**. 컴파일/도메인리로드 완료와 Console0를 확인하고 새 type 사용.
- `ClassContentAssets.Install()` 전체 재실행 금지(HUD이미존재). 부분 authoring은 해당 source method/현재 prefab을 직접 고친다.
- 현재 종료 상태는 Editor Play중지/MainMenu/Console error0이고 hidden Client도 종료했다. 실제 사용자 save 전후 기록은 `user-save-before.json` / `user-save-checkpoint.json`.
- stage/time/계정 한도는 timing-usage.json. 작업별 정확한 tokens/cost는 도구에서 제공되지 않는다. account-wide 반올림 snapshot과 경과시간을 작업 소비량으로 단정하지 않는다. 5h window가 작업 도중 reset돼 시작73%→종료83%를 단순차감하면 잘못이다.
