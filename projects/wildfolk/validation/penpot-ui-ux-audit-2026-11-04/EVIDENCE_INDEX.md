# Evidence index — checkpoint

**PARTIAL.** 원본/현재/역사/미검증을 분리한다. 모든 source path와 line은 아래 JSON 및 상세 초안의 링크에 있다. 미작성6개 최종 보고서의 근거 검토까지 끝난 상태가 아니다.

## 정본·열거 증거

| 파일 | 내용 | 판정·한계 |
|---|---|---|
| [source-manifest.json](source-manifest.json) | 선택된 C#105개 hash/line/method/binding 인덱스 | 자동 열거; 모든 줄의 수동 검토를 뜻하지 않음 |
| [scene-inventory.json](scene-inventory.json) | MainMenu/Wildfolk/Multiplayer 오브젝트·Canvas·component 수 | 실제 canonical scene YAML |
| [scene-surfaces.json](scene-surfaces.json) | UI/관련 component path·line·authoring active | authoring inactive는 dead 판정 아님 |
| [serialized-controls.json](serialized-controls.json) | control instance671개, callback target/method, hierarchy/line | runtime Bind/gesture 별도 보완 필요 |
| [screens.json](screens.json) | 등록66개; 현재 surface57/미구현·미래·역사9 | 최종 전수 수동 검토 전 초안 |
| [actions.json](actions.json) | 의미별185개; service/gate/source, 반복 instance path | 매핑 초안; 실제 transaction 실행 검사 아님 |
| [navigation.json](navigation.json) | 조건별 route103개와 source | rendered 모든 route 재현한 것 아님 |
| [source-art-index.json](source-art-index.json) | art-source10개 hash, KRA metadata/ZIP members | KRA 직접 수정/새 Penpot 없음 |
| [native-canonical-comparison.json](native-canonical-comparison.json) | Native/main 비교 | source 씬은 trailing whitespace, NetworkPlayer는 block order만 다름; Audio folder GUID4개·Editor play option 차이 별도. sync 안 함 |

## 현재 Unity 관찰

[live-initial.json](live-initial.json): 실제 프로젝트 `C:/UnityProjects/Wildfolk`, Unity6000.3.19f1, MainMenu/Edit/clean/QA marker 없음에서 시작했다. 기존 localhost MCP에 연결했으며 새 서버/에디터를 시작하지 않았다.

[live-evidence-index.json](live-evidence-index.json)에 보존한 PNG45개의 SHA256·UTC·viewport·language·scene·이번 직접 열람 여부를 기록했다. 동명 JSON은 실제 Text/Selectable 상태다. JSON은 screenshot pixels 검토의 대체물이 아니다. 첫 루프에 settings-en-844가 Attendance와 중첩된 원인은 잘못된 감사 드라이버 경로였고, 문제를 제품 결함으로 집계하지 않았다. rejected PNG/metadata는 로컬 evidence/rejected에 보존·Git 제외, [live-frontend-attempt1.json](live-frontend-attempt1.json)에 원인을 기록했다.

| 이번 직접 연 현재 이미지 | 관찰 | 범위 |
|---|---|---|
| [home-v2-en-844](evidence/live/home-v2-en-844.png) | 오른쪽 book/primary·좌하 records·Settings/Profile | EN844 저장 없음 |
| [settings-v2-en-844](evidence/live/settings-v2-en-844.png) | control/audio4 sliders·cycle language·Reset | 값 변경·실기 touch는 안 함 |
| [achievements-v2-en-844](evidence/live/achievements-v2-en-844.png) | 7tabs·목록/상세·disabled claim | offline catalogue |
| [attendance-monthly-v2-ko-844](evidence/live/attendance-monthly-v2-ko-844.png) | 월간28칸/stamp/Gem/연결 안내 | tile별 claim 화면 아님 |
| [store-help-back-ko-844](evidence/live/store-help-back-ko-844.png) | Help Back이 Account로 복귀 | 현재 callback 경로 CONFIRMED |
| [class-overview-ko-844](evidence/live/class-overview-ko-844.png) | 6class preview·selected·locked·tree entry | profile 신규 Survivor, 구매 없음 |
| [qa-new-world-hud-ko-844](evidence/live/qa-new-world-hud-ko-844.png) | 빈9slot·HUD·새 세계 실제 시작 | progression/전투 테스트 아님 |

그 외38개 보존 PNG는 캡처/metadata 확보 상태이며 이 체크포인트에서 직접 전부 열어 보지 않았다. 1920/1280/844 한영 matrix 합격으로 보고하지 않는다. `delete-unconfirmed-*`는 linked가 아닌 QA에서 callback으로 띄운 warning preview이고 삭제 요청을 실행하지 않았다. `multiplayer-verified-internet/lan-*`는 표시 route를 확인해 캡처했다. 그 외 transport filename은 route 유지 때문에 정확한 label 근거가 아니며 JSON과 pixels를 읽어야 한다.

## 과거 증거와 디자인 원본

[historical-image-index.json](historical-image-index.json)은 요청된4개 이전 validation directory의 PNG180개를 인덱싱했다. Git에 이미지가 추가/수정된 commit/date와 당시 실제 runtime build SHA는 같다고 보장하지 않는다. 기존 report의 capture metadata를 추가 대조해야 한다.

이번 직접 연 과거4개는 모두 `post-commercial-ui-qa-tools/evidence/`다. Git image commit은 `0ae9feae389c24abae40bc68da9569dd2eb5a676`(2026-11-02). 현재 행동 합격 증거로 바꾸지 않는다.

- [Inventory](../post-commercial-ui-qa-tools/evidence/gameplay--inventory-en-844.png)
- [Repair context](../post-commercial-ui-qa-tools/evidence/gameplay--hud-repair-ko-844.png)
- [Death](../post-commercial-ui-qa-tools/evidence/gameplay--death-ko-844.png)
- [Medic tree](../post-commercial-ui-qa-tools/evidence/variants--class-3-tree-en-844.png)

KRA ZIP에서 추출한 기존 [FieldJournal merged preview](evidence/source-art/LowfireFieldJournal.png)와 [Wordmark merged preview](evidence/source-art/LowfireWordmark.png)를 열어 봤다. 원본은 `art-source/*.kra`; 새 디자인이 아니다. `docs/design/lowfire-home-a.svg~c.svg`는 과거 Home 시안3개다. 사용자 지시에 따라 Penpot MCP 재연결/보드 작업은 하지 않았다. 전체 UI Penpot 원본이 있다고 주장하지 않는다.

## 미검증·미완성 — 다음 실행에서 닫을 항목

1. 나머지 현재 캡처38개 시각 검토 및 viewport별 비교.
2. 모든57개 현재 surface의 시각 증거 확보/없는 경우 명확한 UNVERIFIED.
3. Inventory/Chest/stack/drag 실제 live 관찰과 각 parameter/state 대조.
4. Crafting/Workbench I–III/queue/long details/upgrade/maintenance 현재 관찰 보강.
5. Character/common tree/current class states·unbound APIs 수동 대조.
6. Camp/POI/Turret/Beacon/navigation/Telescope 런타임 생성 표면 관찰.
7. death/downed/rescue/pending/late callback 및 MP-only 표면의 현재 rendered evidence.
8. 실제 Host/client/failed save/disconnection/modal raycast ownership; 이번 networking 세션을 열지 않음.
9. Backend-enabled conflict·cloud·claim·actual ad/IAP/native consent·deletion states. 제품 OFF 유지; provider 실제 활성화는 이번 범위 밖.
10. 물리 Android touch/thumb reach/notch/gesture/IME/keyboard/audio/성능. 이번 PC 캡처로 대체 금지.
11. source별 presentation asset/generator/import/slice/material/font/audio ownership 보고서 완성.
12. 원래11개 보고서·P0/P1/P2·Top10·Penpot frames/components·future approval gates·최종 adversarial self-review.

## 보호·검사

[protection-check.json](protection-check.json): 실제 사용자 파일6개에 대해 SHA256/size/mtime_ns 불변. Native Assets/Wildfolk·ProjectSettings·Packages3362개 파일도 전후 불변이며 추가파일0개다. 정확한 native 수는 JSON을 정본으로 삼는다. 원본 사용자 값/credential은 보고서에 복사하지 않았다. QA directory는 이 감사의 evidence/qa이고 Git 제외다.

UserPreferences/LocalSettings가 격리 profile에서 in-memory store를 사용하는 것을 source로 확인했다. 실제 PlayerPrefs/registry를 편집하지 않았다. 다만 registry 전후 snapshot은 확보하지 않았으므로 registry hash 불변 검사를 했다고 주장하지 않는다.

[live-final.json](live-final.json): MainMenu 편집 모드, Play OFF/paused=false, scene dirty=false, QA marker 지움, Game View1920×1080으로 복원. Save/Protocol을 올리지 않았다. NUnit/data-validator/Android build/자연 progression/오디오 청취/paid service 검사는 이번 읽기 전용 감사에서 실행하지 않았다.

시간은 [timing.json](timing.json)의 실제 UTC boundary로 비중복 기록한다. 도구 대기와 동시에 한 source 검토를 분리 측정하지 못한 구간은 합쳐진 wall-clock이다. 작업별 token/billing API는 제공되지 않아 null로 남겼다.
