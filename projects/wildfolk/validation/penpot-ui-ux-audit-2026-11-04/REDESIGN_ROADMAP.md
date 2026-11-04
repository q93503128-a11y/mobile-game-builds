# 재설계 순서·승인·검수 기준

감사 재개 시 fetch한 main: `82d19b64de0157d924a05f87ea8ee698f5d6db6b`. 제품 source/data는 이전 `48d6528`과 동일하다. game 변경·Penpot 제작 없이 조사했다.

FACT/CONFIRMED는 현재 source/이번 audit의 실제 화면, SOURCE-INFERRED는 source로 도출한 동작·위험, HISTORICAL은 과거 캡처, PROPOSAL은 제안, FUTURE GOAL은 사용자 차기 목표, UNVERIFIED는 검증 범위 밖이다. 디자인 심각도는 감사 우선순위이며 모든 항목을 출시 차단 코드 버그로 해석하지 않는다.

**아래 Stage1–6는 미래 계획이다. 이번 감사에서는 어느 Stage의 디자인·코드 구현도 시작하지 않았다.**

## Pilot 판단

| Pilot | 역할 | 먼저 고정할 것 | 통과 조건 |
|---|---|---|---|
| Main Menu | 브랜드/주행동/부행동/forest와UI 조화 | H01 valid·empty·failure와 current new-world warning | Continue/새세계/Join/설정의 의미, 한영 type, provider OFF 안내가 즉시 읽힘 |
| Gameplay HUD | world 가독성/생존 위계/실제 thumb overlay | HP·needs·time·selectedhotbar9·primary/interact/dash·class variants | 필수 world 시야 확보,80/100/140%·각role·modal·death input ownership 명세 |
| Inventory | dense 데이터/component 재사용 stress |36slot·장비4·portrait·item detail·split/drag/drop | 긴 설명/수량/charges·호환표시·빈/full bag·scroll/hold가 모두 설명됨 |

Home이 브랜드를 세우기 좋지만 그것만 승인받고 전체 화면을 같은 책으로 확장하지 않는다. HUD는 overlay, Inventory는 density를 검증하므로 세 pilot을 같은 단계에서 서로 대조한다. 구현 순서는 dependencies 때문에 core tokens→slot/button/scroll/modal→Home/HUD/Inventory→Craft/Character→서비스/rare overlay가 적절하다. multi-world는 별도 architecture 결정 전 현재 Home 뒤 기능으로 붙이지 않는다.

## 단계별 handoff

| 단계 | 실제 산출물 | 선행·위험 | 수용 기준·USER APPROVAL GATE |
|---|---|---|---|
| Stage1 Token/core components | semantic tokens/type/spacing/actions/slots/scroll/modal/node·export conventions | 이번 audit의 state 계약·작은 화면 type·기존 art ownership | **Gate A: 사용자에게 시각 방향과 core kit 승인.** 최종 스타일·색을 혼자 고정하지 않음 |
| Stage2 Three pilots | Home/HUD/Inventory,3viewport·EN/KO·주요 state | all-sized sheet 자동확장 위험; role/class/touch 제약 | **Gate B: 세 pilot 실제 프레임 비교 승인.** 기능축소/가짜 controls 없이 상용 품질 후보 |
| Stage3 Cross-screen validation | task flow/Back/disabled/keyboard/long content review | prototype가 실제 callback·서버 기능처럼 오인될 위험 | **Gate C: navigation 변경·행동 문구·responsive 규칙 사용자 승인.** UX01 등 변경도 명시 |
| Stage4 Remaining screens |57current surfaces의 frames/variants + future page 분리 | rare/runtime panels·provider OFF·downed omitted 위험 | **Gate D: 전체 current feature checklist/상태/문구·exportmanifest 승인.** Future mechanics는 별도 |
| Stage5 Unity prep | source assets/import/slice/reference/builder·controller 변경계획/검수범위 | generated scene-only수정 재생성손실; identity·transaction 의존성 | **Gate E: 실제 Unity 통합 범위 사용자 승인.** 구현 전 rollback·원본 보존/Save·Protocol 영향 검토 |
| Stage6 Unity integration + QA | canonical source/authoring·실제 assets·관련검사·실기 검수 | callback·pointer·local actor/Host·서비스·DPI/성능 regression | **Gate F: 사용자 PC/Android 체감 승인과 technical checks.** 디자인시안·batchpass만으로 release 승인 금지 |

## 기능 유지 검수 gate

1. source SCREEN57·ACT185·NAV103별 대조표에 구현 후 새 evidence를 붙인다. 변경한 flow/제거한 action은 사용자 승인 근거를 기록한다.
2. Core controls: split confirm은 들기만, 목적지 성공 전 원본 보존; empty release/cancel·full bag·equipment4·charges/quality·stale revision.
3. Craft/queue: category/filter/NEW·station tier·정확 cost·Batch/Sequential·예약/withdraw/Collect·blocked output·long costs. 자동 수령/새 station 기능을 임의 추가하지 않음.
4. Navigation: StoreHelp caller, 일반 origin Back, pause settings caller, class/common tree, state reset, modal raycast/동시 pointer release, loading/cancel/late result.
5. Authority: Client replica는 지급·차감하지 않음; account/economy/purchase receipt·pending/unknown 유지; offline cache spending 금지. 실제 service 활성화는 별도 운영 승인.
6. Death/save: 무료 Home, 죽음/run revision·reserve/world commit/finish, Save14/backup/저장실패 화면 유지·Host leave/client disconnect. 사용자 실제 파일 미사용.
7. Responsive:3viewport EN/KO·notch·long text/값·80/100/140%·physical Android2엄지/IME·sound/mute·frame pacing.
8. Assets: editable source·license·stable IDs/slice/9slice·no missing refs, RGBA padding·font glyph·texture memory/overdraw/Canvas rebuild·GC.

## 별도 architecture 또는 art 승인이 필요한 일

multi-world index/Continue semantics/same-world solo-host/rename/delete/migration; 새로운 language modal 또는 비용확인 단계 같은 동작 변경; world/character/boss art redesign; quest/map/durability 등의 새 기능; provider setup/live payments/ad/production은 각각 다른 scope다. 감사에서 제안했다고 승인된 구현으로 취급하지 않는다.

지금 다음 권장 작업은 **Stage1~2의 명시 디자인 요청과 세 pilot 비교 승인**이다. 바로 전체 scene rewrite나 새 AAB/RC2/production 배포로 넘어가지 않는다.
