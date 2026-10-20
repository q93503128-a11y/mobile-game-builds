# 과거 중단 기록 · 이후 재개 완료

2026-10-20 재개하여 아래 미완료 목록의 작업을 완료했다. 현재 구현/검수/A판정은 [REVIEW](REVIEW.md)를 따른다. 아래는2026-10-19 중단 당시 원문이며 현재 작업 목록이 아니다.

# Creature / Loot Expansion 1 — 중단·재개 기록

사용자가 2026-10-19 사용량 사유로 “지금 하는 것까지만 하고 다음에 이어서”를 요청했다. **미완료이며 커밋·푸시하지 않았다. A/B/C 완료 판정은 보류한다.** Workbench III는 이전 패스에서 완료된 상태다.

## 저장소와 작업 위치

- Repository: `q93503128-a11y/mobile-game-builds`, `main` 직접 작업. PR/임시 branch 금지.
- 작업 시작에 origin main을 fetch했다. 시작/중단 시 local HEAD와 origin/main: `252972006bdc6436cafb31147574f50e0c62a3cc`.
- Canonical checkout: `C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical`.
- Unity live project: `C:/UnityProjects/Wildfolk`, Unity6000.3.19f1 Universal2D. 두 경로는 별도 복사본이다.
- Unity MCP instance: `Wildfolk@1367f605af2f8ba4`.
- 중단 시 Unity Play=false / paused=false / 컴파일 오류0 / QA SessionState 해제. 현재 씬 Wildfolk. Windows 조작·Focus/Show는 사용하지 않았다.
- 실제 사용자 UserData의2개 파일은 시작/중단 시 SHA256과 mtime_ns가 동일하다. `user-save-before.json`, `user-save-after-pause.json` 참조.
- `Wf 1` 채팅의 최종 보고도 읽었다. 이전 III commit이 현재 시작 SHA임을 확인했다. 현재 코드·데이터를 우선하며 과거 보고 숫자를 복사하지 않는다. 예: 실제 Mechanical Part는 Bronze1+Steel1→2개다.

## 반드시 먼저 할 일

1. `git fetch origin main`으로 다시 확인하되 **현재 dirty main의 변경을 reset/삭제하지 않는다.** 원격이 전진했으면 기존 작업과 먼저 비교한다.
2. AGENTS/README, 이번 REVIEW/이 RESUME와 관련 canonical docs를 읽는다. unity-mcp-orchestrator skill/resource 절차를 따른다.
3. 사용자 저장 파일 hash/mtime 보호 상태를 다시 확인한다. Play 전에 `IsolatedQaProfile.Begin(...)`을 반드시 설정한다.
4. 다음 절의 미완료 작업을 계속한다. 조사와 asset 생성 전체를 처음부터 반복하지 않는다.

## 완료한 조사·감사

- Grounded / Core Keeper / Valheim / Don't Starve Together / Terraria / Necesse / Mine Survival의 실제 공개 drop/recipe/가공/조리/거점 사례를 조사했다.
- REVIEW에 출처 링크와 비교표(게임/시스템/사례/장점/차이/적용/이유), 각 주요 결정의 외부 사례→Lowfire 문제→원칙→배제 대안을 기록했다.
- 신규 설계·게임 코드 작성은 해당 조사와 baseline graph 감사 후 시작했다.
- baseline110 items/91 recipes/9 loot tables/12 natural sources를 Unity에서 추출: `baseline.json`, `BASELINE_GRAPH.md`.
- 현재121 items/99 recipes를 Unity에서 추출: `after.json`, `CURRENT_GRAPH.md`.
- Station 현재 개수: Hand5 / WB I35·II전용23·III전용11(누적35/58/69) / Campfire6 / Pot6 / Furnace4 / Blast1 / Turret8.
- 새 raw material3개(Bone/Chitin/Medicinal Gland), 새 Station0, 종 전용 raw0. Bone3 sink/Chitin3 sink/Gland2 sink.

## 작성한 구현·에셋

- 신규8종: Bat, Stone Crawler, Bog Leech, Wolf, Mountain Goat, Crow, Giant Boar, Crystal Crawler. 기존5규칙의 인덱스는 유지하고 뒤에8개 추가했다.
- `WildlifeActionDefinition` + `WildlifeActionBrain` + 공용 Scheduler. 종 ID별 runtime if 없음. Flight/Swoop, Camouflage/unfold, Shore ambush, bounded same-pack assist, Hop/flee, Charge/recovery, crystal pulse/LOS를 데이터 조합으로 구현했다.
- 지형/solid sweep, HP/실제 접촉 피해, 중복 pulse 피해 방지, Host 권위, chunk sleep, 지역 보충을 연결했다. 활성 공용 HardCap80 유지, 종 cap 합계98은 soft habitat 목표다.
- 신규 동물은 개별 FixedUpdate AI를 건너뛰고 공용 Scheduler에서0.05초마다 최대32개 처리한다. 행동 frame 최대10Hz, HP 변화 즉시. Client는 replica 표시만 한다.
- 초기 생성 순서를 deterministic 분산하여 기존 동물 우선 생성에 새 종이 밀리는 문제를 줄였다. 기존 방문 청크에 Save11 ExpandedRegions로 새 규칙을1회 초기화한다. 알맞은 habitat인데 충돌로 최초 생성 실패하면 recovery ticket을 남기도록 보강했다.
- 휴면 동물·Goblin·Camp는 chunk index로 조회하도록 수정했다. 마지막 Goblin/Camp index 변경은 아래 최신 persistence 회귀 실행에 포함됐다.
- 신규 아트는 기존 Pillow pipeline의 직접 제작 원본: `tools/generate_creature_loot_art.py`, Art PNG/메타, species/profile/loot/vitality/prefab/network prefab. 외부 게임 asset/code/layout 복제 없음.
- 신규8 Recipe: bone_arrow×12, bone_trap, bone_broth, carapace_vest, barbed_bolt×8, healing_salve, restorative_broth, trail_ration.
- 기존 드랍도 수정했다. Chicken/Boar 유지, Rabbit/Deer Bone, Sheep Fiber, Zombie Fiber/Bone, Skeleton Bone 중심, Goblin Bolt/Fiber/Leather, 새총 Goblin Feather/Fiber/Leather. Sword rare12% 유지.
- 기존 Reinforced Frame: Steel2+Leather1+Chitin2. Mixed Stew: HP8/공용 소비 cooldown4초 추가.
- Consumable의 실제 HP/Stamina 즉시 회복/공용 cooldown, full hunger 상태의 회복품 사용, Armor Stamina 회복속도 보너스, 실제 UI 설명을 연결했다.
- Goblin Camp:6칸 take-only 보급품, persistent identity=`Camp ID+:supplies`, 최초 deterministic roll/개봉/잔량/revision. 거주 적 생존(휴면 포함) 시 잠금. Host range/alive/가방·상자 revision/공간 검증. whole-stack atomic 교환, 재생 없음, 저장/reHost 필드 연결.
- 캠프 보상: Bone Arrow8–16 / Salve1–2 / Bronze1–2 / Leather1–2 확정, Part1 20%, Barbed Bolt8 35%. RNG가 Diamond/Part progression의 필수 조건이 아님.
- Save10→11(기존1~10 이관), Protocol12→13. 순간 공격 pose/target은 저장하지 않으며 살아 있는 identity/group/home/HP와 camp 상태를 보존한다.
- canonical asset과 두 씬의99개 제작 카드를 갱신했다. 기존 source GUID를 바꾸지 않았다. 기존 Player/TierIII 에셋의 무의미한 재직렬화는 복사하지 않았다.

## 검증 결과와 수정 경위

- 첫 live compile0 errors, Unity ProjectDataValidator0 errors,121/99 확인.
- `edit-initial.xml`:64/66. 오래된 placeable count30→31, 회복 연고의 Hunger0을 허용하는 효과 합 검사로 수정했다.
- `edit-final.xml`: **66/66 통과**,14.1954487초(Test runner 내부 시간).
- `play-initial.xml`:22/26. 실패4개는 구5종 정확 개수/Save10 기대값/손상 stack 예외 타입이었다. 원래5종 보존을 검사하도록 범위를 고쳤고 Save11/ArgumentException 계약을 반영했다.
- `play-final.xml`: **21/21 통과**,125.2062553초. 신규 행동·연고/Stamina/장비·Camp claim/full bag/guard/range/save load, 관련 구생태 회귀와 TierIII Play를 포함한다.
- 초기 Host 자동 검사에서 캠프 duplicate/stale bag revision/Save/reHost와 기존 Turret reHost가 통과했다. 실제 remote Client나4인 Relay 검증은 아직 하지 않았다.
- 소스 구조 검사: `python projects/wildfolk/tests/validate_visual_project.py projects/wildfolk/client` → **1256 unique GUID, sprite refs, Landscape, visible text 통과**. 이후 변경은 C# 위주지만 최종 마감 때 다시 관련 변경 범위를 확인한다.
- 현재 실행 중이던 마지막 검사: `persistence-final.xml` / `.log` (시작 UTC12:23:23). 아래 중단 추가 결과를 따른다. 최신 Camp 빈 슬롯 수정, 휴면 index/capture 수정과 실제 loot modal 자동 검사를 포함한다.

### 실제 MCP/Game View

- 1920×1080, unfocused, 격리 QA로 `CreatureLootQa.Begin()`을 실행했다. teleport/재료 지급/표적 fixture를 사용했으며 자연 진행 시간 측정이 아니다.
-8종 gallery, 실제 authored habitat에서 주요 행동/경고/공격/회복, Stone 위장 해제, Wolf pack, Goat/Crow 도주, death/drop/자동 pickup을 확인했다. 최신 전체 실행은8종과 신규 제작/실제 처리·Furnace/Bast/Camp UI까지 진행했다.
-8개 신규 Recipe를 UI callback으로 제작했고, Campfire/Pot/Furnace/Blast의 실제 처리·수령도 확인했다.
- 캠프 guarded 거절→처치→take-only UI→획득→replay 거절→가방에 정확히1회 지급까지 확인했다.
- 발견·수정: 새 모달을 활성 상태로 AddComponent하여 SafeArea 설정 전 OnEnable이 호출되는 NullReference. 패널을 비활성으로 구성 후 활성화하도록 수정했다. 실제 화면에서 해당 오류가 사라졌다.
- 발견·수정: WB II→III 비용6줄 때문에 장비 통계가 처음 화면에서 가려짐. 실제 효과를 설명 첫 부분에 배치했다. 새 화면 캡처는 captures/02-workbench-materials.png.
- **마지막 실제 MCP Save() 실패:** 멀리 잠든 자연 Camp의 희귀 빈 슬롯이 null이어서 `InventoryModel.Restore`가 Invalid saved stack을 냈다. 초기 자동 검사는 encode/decode 뒤 validate하여 이 경계를 놓쳤다.
- **수정 소스는 작성·컴파일했으나 live 재검수는 미실행:** Camp 최초6칸과 claim 후 빈칸, network replica 빈칸을 `new InventoryModel(6).Capture()` / `new SavedStack()` 형식으로 통일했다. 휴면 Camp capture도 deep copy했다. `CreatureLootPlayTests`에 휴면 중 direct Capture validation/실제 Save와 실제 modal 검사를 추가했다.
- 따라서 `captures/live.tsv`의 마지막 FAIL은 **수정 전 기록**이다. 삭제하거나 성공으로 바꾸지 않는다. 이후 실행 결과는 별도 보존한다.
- `CreatureLootQa.BeginHost()`도 작성했으나 **실제 MCP Host/reHost 실행은 아직 안 했다.** 자동 Host 통과와 혼동하지 않는다.
- QA fixture 자체도 수정했다: drop이 없는 roll에서 death animation 종료를 기다리도록 했고, pickup loop 도중 이미 자동 획득된 pooled item(Count0)을 건너뛰도록 했다. 게임 오류로 분류하지 않는다.

## 다음에 남은 작업

1. 최신 persistence 결과 확인 후 필요한 실패만 수정. 새 검사를 반복해서 모두 돌리지 않는다. 현 단계 미해결은 실제 MCP empty-slot 수정 후 Save/Load/reHost 재확인이다.
2. canonical C#/Localization/PNG를 live에 동기화→AssetDatabase.Refresh→컴파일 확인. Play 전 isolated QA 설정→기존 Game View1920×1080/PlayUnfocused 유지. Focus/Show 호출 금지.
3. `CreatureLootQa.Begin()` 전체 흐름 또는 필요한 실제 수정 경계를 검수. 잠든 자연 Camp와 빈 희귀 칸이 있는 상태의 Save()를 직접 확인한다. 현재 isolated QA는 해제돼 있으므로 반드시 다시 Begin할 것.
4. 실제 MCP `SceneFlow.Home(S.Repository)`→Home 준비 후 `CreatureLootQa.BeginHost()` 실행: Host camp claim/save/leave/reHost 실제 확인. 원격 client/4인 Relay 미검수는 명확히 남긴다.
5. 소비품/장비 설명 영어·한국어, take-only UI empty/full bag/guarded/range close, 새 item명의 잘림을 최종 확인. 일부 KO 캡처가 저장돼 있다. 사용자 재미/손맛은 대신 합격 처리하지 않는다.
6. `after.json`/`economy.json`의 실제 데이터를 바탕으로 경제 표와 REVIEW를 완성한다. essential/core 진행에는 new raw 재료가 없고 Diamond13/49, Crystal 필수10/22는 유지된다. 대표 선택 방어기지 포함 raw는1P Wood188 Stone98 Iron83 Coal121 Copper40 Tin19 Diamond13 Crystal16 Leather15 Fiber6 Feather14 Chitin4;4P Wood332 Stone149 Iron197 Coal226 Copper68 Tin30 Diamond49 Crystal28 Leather48 Fiber24 Feather26 Chitin4. 중간재는 raw에 더하지 않는다.
7. 선택 반복 kit의 명시적 분모를 정하고1P/4P 재료 예산을 계산한다. 예: 개인 BoneArrow96/BarbedBolt64/Vest1/Salve4/BoneBroth2/RestorativeBroth2/TrailRation2 + 공유 BoneTrap4. 이는 실제 플레이 측정이 아니라 recipe 계산임을 표시한다. 새 material의 기대 드랍, 원래 자원/Chicken/Boar 가치, Crystal source6 vs15%×1 보조 drop, Camp1회 비용 절약을 비교한다. farm 분당 실측값을 만들지 않는다.
8. canonical 문서 마감: CONTENT_EXPANSION/CRAFTING/CREATURES_AND_LOOT는 현재 새 상태로 재작성했다. **README/BALANCE/WORLD_GENERATION/SAVE_PERSISTENCE/ARCHITECTURE/FOOD_AND_FISHING/CHARACTER_PROGRESSION 및 필요 시 DESIGN_BIBLE은 아직 현재 패스 반영이 남았다.** REVIEW 후반 실제 결과/경제/제한/다음 단계/판정도 미완료다. 기존과 현재 숫자를 섞지 않는다.
9. 프로필·레시피 Tier 정렬, validator의 새 effect/profile 수치 검증 등 구현 diff를 마감 검토한다. 신규 runtime은 data-driven이고 회복/timing 등은 초기 밸런스다.
10. 관련 소스 검사/최종 validator/변경된 저장·네트워크 계약을 확인하고 userdata hash/mtime를 비교한다. 자동·실제 검수·미검수 경계를 분명히 적고 A/B/C를 결정한다.
11. 사용자가 재개한 뒤 **실제 완료된 경우에만** main diff 검토→commit→push→remote SHA/HEAD 일치→clean worktree 확인. 현재는 dirty이며 커밋하지 않는다.

## 재현 도구와 파일 주의

- 사용자 첨부 원문: `C:/Users/User/.codex/attachments/81220f1c-f28a-4538-9560-94f5dd3182b8/붙여넣은 텍스트.txt`. 최종27개 보고 항목과 조사 품질 gate를 보존한다.
- workspace 외부 helper: `C:/Users/User/Documents/ChatGPT/문승준 모바일/creature_loot_work.py` (source 동기화/선택 asset 복사/hidden headless tests), `creature_loot_docs.py` (문서 생성 초안), `pause_creature_loot.py` (사용자 저장 보호/중단 artifact 보존). helper는 canonical repository 밖이다.
- **creature_loot_docs.py를 무심코 재실행하지 않는다.** 마감 문서 수동 변경을 덮어쓴다. Mechanical Part prose typo(Bronze2→1)는 중단 스크립트에서 이미 바로잡았다.
- 테스트 캐시 프로젝트: `canonical/projects/wildfolk/tests/artifacts/unity` (git ignored). live Editor와 별개로 hidden `-batchmode -nographics -runTests`를 사용했다. Windows 창을 앞으로 가져오지 않는다.
- Pillow는 default Python313에 없으므로 bundled Python: `C:/Users/User/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe`.
- `tools/__pycache__/`는 art generator 실행 중 생성된 임시 untracked 파일이다. 자동 승인 검사가 삭제 명령을 차단하여 남겼다. 소스 변경이 아니므로 최종 commit 대상에 섞지 않는다. 다른 방식으로 우회 삭제하지 않았다.
- 신규 .cs/.png/.asset/.prefab의 Unity .meta를 canonical에 보존했다. 기존 meta/Player/TierIII의 공백·기본 필드 재직렬화만의 차이는 가져오지 않았다. Asset copy helper의 선택 목록을 읽고 사용할 것.
- screenshots/log/XML은 이번 validation 디렉터리, 원본 live 화면은 `C:/UnityProjects/Wildfolk/Captures/creature-loot-expansion`에 있다. 원래 사용자 저장과 격리 QA 저장을 혼동하지 않는다.

## 시간 기록

- 조사·baseline·소스 작성:2026-10-19 KST20:29:59→20:55:10,25분11초.
- 초기 Unity asset/컴파일 확인:20:55:10→20:58:32,3분22초. 첫 asset build 내부 측정24.265909초는 이 구간 안에 포함하며 중복 합산하지 않는다.
- 통합 구현·QA 수정·문서 초안:20:58:32→21:24:11,25분39초. 백그라운드 검사는 이 구간과 겹친다.
- 중단 정리:21:24:11부터. 종료 시각/경과와 진행 중 검사 결과는 아래 중단 추가 기록에 남긴다.

## 중단 추가 결과 · 2026-10-19 21:27:00 KST

- 마지막 `persistence-final.xml`: **12/12 통과**, Test runner 내부150.7116684초(UTC12:23:49→12:26:20). 빈 슬롯·휴면 capture/index·실제 loot modal과 Host/reHost의 자동 회귀가 통과했다. 수정 후 실제 MCP Save/Load/Host 흐름은 다음 세션에서 검수한다.
- Unity Play 종료/QA 프로필 해제/현재 컴파일 오류0. 사용자 실제 저장2개 hash·mtime 동일.
- `git diff --check`에는 Unity 재직렬화 YAML의 빈 문자열 필드 trailing whitespace가 남아 있다. 최종 diff 정리 때 확인한다. 중단 요청에 따라 지금은 추가 수정하지 않았다.
- 중단 정리21:24:11→21:27:00,2분49초. 백그라운드 검사 시간은 작업과 겹치므로 중복 합산하지 않는다.
- main 변경 보존, **미완료·미커밋·미푸시**. 다음 작업은 수정 후 실제 MCP 검수→경제·정본 문서 마감→최종 관련 검사→완료된 경우 Git 반영이다.
