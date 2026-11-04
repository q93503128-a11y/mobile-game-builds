# Lowfire UI/UX forensic audit — checkpoint

**사용자 중단 요청으로 체크포인트 저장. 전수 감사 완료본이 아니다.**

조사 기준 remote main은 `48d6528758d18bf3df41ed02ad9aa0f268654a51`이다. 시작 시 fetch와 clean main 일치를 확인했다. 게임 source/content/scene/config/save를 수정하지 않았으며 Penpot 보드도 만들지 않았다. 기존 Git 디자인 원본만 조사하라는 사용자 후속 지시를 따른다.

## 확보한 범위

- 제품 씬 MainMenu/Wildfolk/Multiplayer 3개를 정적 열거했다. 선택한 관련 C#105개(UI51/관련 runtime29/Editor authoring25), scene control instance671개를 인덱싱했다. 이는 모든 코드의 의미 검토나 모든 화면의 시각 합격을 뜻하지 않는다.
- 표면66개 등록 초안: 현재 구현 표면57개와 미래·미구현·역사 전용9개. 반복 item/recipe/class/language를 별도 화면 수로 세지 않았다.
- 의미별 action185개와 조건별 navigation route103개의 초안이 있다. service별 계약 및 원본 instance path를 보존했다. 잔여 수동 대조가 필요하다.
- 격리 Unity에서 Home/Settings/Achievements/신규·월간 Attendance의 EN/KO ×1920×1080/1280×720/844×390 캡처를 확보했다. Store/Help/Account/Class/MP entry와 QA 새 게임 HUD도 추가했다. 보존한 현재 PNG45개 중 이번 턴에 직접 연 현재 화면은7개다. **모든 캡처를 시각 검수한 것이 아니다.** 과거 이미지4개와 Krita merged preview2개도 열어 봤다.
- `Store → Help & Feedback → Back → Account`를 현재 source와 실제 Unity 화면에서 확인했다. 일반 Store.Close의 origin-aware Back과 별도 결함이다.
- 실제 새 게임은 격리 QA에서 시작 화면까지만 확인했다. progression 플레이/상자/제작 거래/죽음/보스/Host 세션을 이번에 실플레이했다고 주장하지 않는다.

## 보고서 상태

| 요청 산출물 | 현재 상태 |
|---|---|
| [README.md](README.md) | 체크포인트 요약 |
| [SCREEN_INVENTORY.md](SCREEN_INVENTORY.md) | 66개 등록·source·상태·controls 초안, 수동 보강 필요 |
| [CONTROL_AND_ACTION_MAP.md](CONTROL_AND_ACTION_MAP.md) | 185개 action 초안, 잔여 service/gesture 대조 필요 |
| [NAVIGATION_AND_STATES.md](NAVIGATION_AND_STATES.md) | 103개 route 초안, caller-sensitive Back 및 배제한 드라이버 오류 기록 |
| VISUAL_UX_ISSUES.md | 미작성; 현·역사 증거 검토 후 우선순위 확정 필요 |
| UI_ARCHITECTURE_AND_ASSETS.md | 미작성; source-art/작성 원본 인덱스 확보 |
| MOBILE_AND_LOCALIZATION.md | 미작성; 3해상도 한영 캡처 확보, 전수 시각·touch 검수 아님 |
| WORLD_SAVE_AND_MULTIPLAYER_UX.md | 미작성; 실제 단일 SP/Host slot·거래 의존성 조사 노트 RESUME에 있음 |
| PENPOT_DESIGN_HANDOFF.md | 미작성; 보존해야 할 control inventory 초안 확보 |
| REDESIGN_ROADMAP.md | 미작성; 실제 디자인/통합은 이번 범위 밖 |
| [EVIDENCE_INDEX.md](EVIDENCE_INDEX.md) | 확보한 증거·한계·현재 보호검사 |

다음에는 [RESUME.md](RESUME.md)부터 읽고 최신 remote main을 다시 fetch한다. 남은6개 보고서와 초안 대조·링크 검증·최종 11개 완결성 검토를 끝내야 원래 감사가 완료된다. 이번 체크포인트 push는 원래 요청 전체 완료가 아니다.

## 현재 중요한 관찰

Help return은 확인된 기능 문제다. 작은 화면의 본문·출석 밀도, 반복되는 큰 종이 패널, selected/disabled 구분, 유지보수 패널의 시야 점유는 조사 대상이다. severity 개수와 최종 Top10은 아직 확정하지 않았다. 과거 “관련 검수 통과”를 상용 디자인 승인으로 바꾸지 않는다.

현재 Save14/Profile2/Protocol23, Item136/Recipe111, backend/광고/구매 OFF를 유지했다. 미래 multi-world와 같은 world의 solo/Host 이용은 구현돼 있지 않으며 UI 교체만으로 약속할 수 없다. 사용자 실제 save 보호검사는 [protection-check.json](protection-check.json), 에디터 종료 상태는 [live-final.json](live-final.json), 측정 시간은 [timing.json](timing.json)을 따른다. 작업별 실제 token/billing 사용량은 도구에서 제공하지 않아 null이다.
