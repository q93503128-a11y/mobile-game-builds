# Forensic facing correction · 2026-11-03

**객관적으로 확인한 방향 오류의 최소 교정과 관련 검증 완료.** 사용자가 이번 작업 단위에서 중단을 요청했다. 물/UI/도로/월드 배치/아트 재설계는 시작하지 않았다. 이 결과는 상용 시각 품질·RC2·실기기 출시 승인이 아니다. 이전 감사의 미확인 사항은 아래에 그대로 남긴다.

## 1. 기준과 허용 범위

- 시작 시 remote main을 fetch하고 clean main의 HEAD와 대조했다: `6e75f35721191ccb1c8a91a75dd403d85d4045a4`. dev-playbook 최신 main `e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7`의 전역/개발 규칙도 fetch/read했다.
- 최초 감사의 제품 기준은 `0ae9feae389c24abae40bc68da9569dd2eb5a676`; 이번 시작 커밋은 감사 자료만 추가한 상태였다. [사전 구현 게이트](../IMPLEMENTATION_GATE.md), [전체 감사·20행 분류표](../FORENSIC_AUDIT.md), [외부 사례와 접근 한계](../EXTERNAL_REFERENCES.md)를 기준으로 최소 BUGFIX만 진행했다.
- canonical=`C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical/projects/wildfolk`; native=`C:/UnityProjects/Wildfolk`, Unity6000.3.19f1. Windows 입력/Computer Use/foreground Focus·Show를 사용하지 않았다. 기존 로컬 HTTP Unity MCP를 사용했고, VisualGameView의 기존 GameView 크기 선택/Repaint만 사용했다.
- main 직접 작업. 원래 정상 기능, AI/속도/공격/서식지/드랍/레시피/물/생성기/도로/저장·네트워크 스키마를 바꾸지 않았다.

## 2. 재현 → 원인 → 최소 수정

`WildlifeActionBrain.Present`의 회전은 local −Y를 머리 축으로 가정한다. 실제 원본 그림에서 머리가 +Y인 네 종은 오른쪽 의도에도 왼쪽을 보였다. 원본 생성 코드와 이전 실제 프리팹 비교 화면, 배회/도주 자료가 원인을 독립적으로 뒷받침한다. 도입 커밋은 신규8종 확장 `3667bae`이며 최근 UI pass가 원인이 아니다.

이번에 먼저 추가한 실제 Unity PlayMode 회귀는 수정 전 8개 중 아래4종만 실패했다. 첫 오른쪽 입력에서 머리·의도 내적이 정확히−1이었다. 나머지4종은 통과했다. [수정 전 결과](tests-red.json).

|종|원본 머리 축|현재 보정|변경 이유|
|---|---|---:|---|
|Bat|+Y|180°|공유 −Y 기준과 반대|
|Wolf|+Y|180°|동일|
|Mountain Goat|+Y|180°|동일|
|Crow|+Y|180°|실제 이동·부리 반전도 재현|
|Stone Crawler / Bog Leech / Giant Boar / Crystal Crawler|−Y|기본0°|기존 올바른 방향 유지|

기존 `WildlifeActionDefinition`에 기본0인 `VisualFacingOffset`을 추가하고, 공용 `Present`에서 그 각도만 더한다. 네 기존 Action asset만180으로 지정했다. 기존 `CreatureLootAssets` authoring도 같은 값을 작성하며, 데이터 validator는 NaN/Infinity를 거부한다. 종 ID로 runtime 행동을 분기하거나 별도 override/patch 계층을 만들지 않았다. Asset builder 전체 재생성은 관련 없는 프리팹/콘텐츠를 다시 작성할 수 있어 실행하지 않았고, 수정한 기존 authoring 경로를 소스 검토했다.

공유 `PresentReplica`도 같은 설정을 읽는다. 의도 방향과 표현이 일치해야 한다는 불변조건이며, 모든 순간의 이동 속도와 머리를 강제 일치시키는 AI 변경은 아니다. 특히 Bat의 급강하 Recovery 중 뒤로 이탈하는 기존 행동은 유지했다. 휴면 Stone Crawler의 고정 위장 pose도 유지했다.

## 3. 새 검증 결과

|검증|이번 실행 결과|증명 범위 / 한계|
|---|---|---|
|수정 전 재현|PlayMode4 pass /4 fail|위 네 종에서 내적−1; 의도한 red 결과|
|최소 교정 직후|같은8개 전부 pass|수정과 실패 원인 연결; 최종13개에 포함되므로 고유 개수에 중복 합산하지 않음|
|최종 관련 PlayMode|**13/13 pass**, skip0|아래 구성; [전체 결과](tests-playmode.json)|
|관련 EditMode|**16/16 pass**, skip0|신규4개 포함, 기존 생물/드랍/데이터/이관/권위 계약; [전체 결과](tests-editmode.json)|
|ProjectDataValidator|**오류0**|[결과](data-validator.json)|
|소스·에셋 구조 검사|PASS|1890 unique GUID, Landscape, sprite references, visible text; `python tests/validate_visual_project.py client`|
|별도 MCP Play 관찰|25 이동 표본 + 죽음/재사용2 상태|아래 실제 화면·계측; 자동 테스트 결과와 구분|

최종 PlayMode13개 구성:

- 8종 각각 SP/권위 표현 경로와 Network prefab의 공용 replica 표현, 상하좌우·대각8방향, Idle/Warning/Windup/Action/Recovery5단계: 총640조합. 원본 머리 축은 보정 필드에서 역산하지 않고 아트·갤러리로 고정한 독립 기준을 사용했다. 실제 원격 Client 통신 검증은 아니다.
- Stone Crawler 위장/각성 상태1개.
- 실제 물리 이동을 하는 Crow 배회와8방향 도주1개. 이동량과 머리 축을 대조하고, 도주가 자극 방향에서 멀어지는 기존 AI도 확인했다.
- 교정4종 실제 치명타→죽은 몸 축소→반납→동일 인스턴스 풀 재사용1개. 모두 HP 최대, living scale, Collider/brain 활성, 올바른 머리 방향 복구.
- 기존 hostile6종 실제 공격의 단일 피해/중복 hurtbox 방지1개, Crystal pulse의 벽 차단1개.

풀 재사용 검사는 통과했고 관련 제품 코드는 고치지 않았다. 이것으로 사용자가 원래 본 ‘누운 동물’의 정확한 종·상태를 소급 확정하지 않는다. 모든 사망/네트워크 생명주기나 아트 시점이 정상이라는 주장도 아니다.

## 4. 실제 Unity 화면과 관찰

별도 격리 profile=`Captures/Codex_QA_Facing20261103`. source prefab을 실제 Play에서 배치했다. 갤러리는 AI를 멈춘 비교용이며, 이후 Crow 관찰은 기존 PlayerLoop AI와 Rigidbody2D 이동이다. 원래 행동 속도/설정은 그대로다. 환경·HUD를 숨기고 근처 자극 위치를 제어한 **행동 검증 fixture**이며 자연 탐험·정상 progression으로 세지 않는다.

- [8종 오른쪽](evidence/01-gallery-right-after.png) / [대각선](evidence/02-gallery-diagonal-after.png): 실제 그림의 머리가 지시 방향을 향하는 것을 직접 확인했다. 늑대/산양의 정면형 그림 시점은 여전히 어색할 수 있으며 새 아트나 상용 승인으로 포장하지 않는다.
- [실제 Crow 오른쪽 도주](evidence/03-crow-flee-right.png) / [왼쪽](evidence/03-crow-flee-left.png) / [대각선](evidence/03-crow-flee-up-right.png): 배회5표본과4개 도주 방향 각5표본에서 머리·실제 평균속도 내적 최소 **0.9999981**. flipX/flipY=false. [전체 벡터·시각](crow-runtime-after.json).
- 이 별도 관찰에서는 매 표본마다 플레이어 위치를 기존 Crow로부터3unit 떨어진 곳에 재배치해 자극 방향을 유지했다. 수율/속도/AI 방향을 직접 쓰지 않았다. 자동 PlayMode 도주 검사는 이 지속 재배치를 사용하지 않았다.
- [실제 죽음](evidence/04-crow-death.png)에서는 Visual scale=(1.08,0.25,1), [재사용](evidence/05-crow-reused.png)에서는 같은 인스턴스·HP14/14·scale=(1,1,1)로 돌아왔다.
- 이전 감사의 [수정 전 오른쪽 프리팹 화면](../evidence/17-wildlife-right-before.png)과 [배회](../bird-live-before-wander.json) / [도주](../bird-live-before-flee.json)는 당시 자료다. 오늘 새 before 이동 자료로 재표기하지 않았다. 오늘의 새 before 재현은 위 red 테스트다.

관찰 도구의 시행착오도 구분한다. 임시 Editor observer의 SessionState 이름 충돌을 바로 고쳤고, 최초 camera fixture가 AudioListener를 잠깐 꺼 발생한 경고도 fixture에서 고쳤다. 첫 이동 observer는 평균변위 표본이 flee→wander 방향 전환을 가로질러 순간 머리와 비교하기 부적절했다. 이를 게임 버그나 성공 증거로 쓰지 않고 외부 작업 폴더 `probe-first-pass/`에 보존했다. 이후 자극 거리 유지·이전 의도 기록으로 고친 observer의 결과만 위 수치에 사용했다. 임시 observer는 native에서 제거했으며 canonical 제품에는 추가하지 않았다.

## 5. 보호·최종 상태

사용자 실제 저장 파일6개의 SHA256·크기·mtime_ns가 시작과 같다. 격리 QA 외의 Continue/save를 로드하거나 수정하지 않았다. native의 기존 Audio 메타/Mixer 및 NetworkPlayer 차이를 덮어쓰지 않았다. 변경한 runtime/editor/action/test 파일은 canonical과 native가 일치한다.

최종 MainMenu/Edit, Play OFF/paused false/compiling false, scene clean, QA SessionState empty. 임시 observer `.cs`/`.meta` 제거. 관찰 종료 직전 Console warning/error0, 최종 재컴파일 Console은 기존 CS0252/CS0414 경고2개와 오류0이다. [최종 상태](editor-final.json), [Console](console-final.json), [검증·보호](validation.json).

**Item136 / Recipe111 / Save14 / Profile2 / CloudProfile1 / Economy2 / Protocol23 / generation3 / IAP1 유지.** 새 시각 값은 config asset에서만 읽으며 wire/snapshot에 필드를 더하지 않았다. 실제 LAN/Relay·Android·AAB·RC2·Console·운영 광고/IAP는 이번에 실행/변경하지 않았다.

## 6. 남은 판단 / 다음 재개

이 작은 방향 오류 교정 단위는 완료했다. 사용자 요청에 따라 여기서 멈춘다. [전체 감사 분류표](../FORENSIC_AUDIT.md)의 다른 판정은 그대로다.

- Repair/Salvage UI, 물 표현, 도로/early anchor 배치, cave/grave 발견 보조, 자원/동물 그림 시점의 REBUILD는 **제안만**이며 별도 사용자 승인 전 구현하지 않는다.
- 원래 누운 개체는 미확정이다. 이번 풀4종 검사는 stale corpse 가설을 지지하지 않았지만 모든 종/조건을 배제하지 않는다.
- 현재 새 lake/coast와 동일 조건의 역사적 renderer 실행 비교, 최신 상용 모바일 화면의 완전한 원본 해상도 비교, 장애물을 직접 피하는 자연 발견성 연구는 미완료다. 기존 강/역사적 이미지·source hash·12seed 자료의 관측 범위는 유지한다.
- 전체 progression/왕 전투/원격 Client/실기기 시각·성능 승인은 이번 검증 범위가 아니다.

다음 재개 시 remote main을 다시 fetch하고 현재 [RESUME](../RESUME.md)를 따른다. 방향 수정/이미 통과한 관련 검사를 이유 없이 다시 시작하지 않는다. 원래 사용자 프롬프트는 RESUME에 기록했다.

## 7. 파일·시간

정확한 이번 commit 대상은 [files-changed.txt](files-changed.txt). 수정은 기존 canonical 경로와 검증 문서에 직접 반영했다. 이전 감사의 당시 기록은 지우지 않고 날짜가 있는 후속 절로 연결했다. 이 문서를 담는 main commit이 종료점이며 실제 full SHA/local=remote/clean은 push 후 최종 응답과 작업 폴더 `final-git.json`에 기록한다.

[timing.json](timing.json)은 단계별 UTC 시작/종료/경과 시간이다. 초기 측정 전 준비는 제외한다. 단계는 겹치지 않으며 도구 대기·검증 대기를 포함하는 벽시계 시간이다. test-job started/finished와 순수 test duration도 각 결과 JSON에 따로 있어 대기 포함 구간과 구분할 수 있다. 최종 commit/push 단계는 외부 `final-git.json`에 추가한다. 작업별 토큰/과금 사용량은 제공되지 않아 null이다.
