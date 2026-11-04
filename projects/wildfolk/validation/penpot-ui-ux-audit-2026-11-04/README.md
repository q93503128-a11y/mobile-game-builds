# Lowfire 전체 UI/UX forensic audit — 최종 조사본

**요청한 연구·명세·근거 검토를 완료했다. 현재 UI를 상용 출시 품질로 승인한 보고서는 아니다.** 게임·Penpot·실서비스는 변경하지 않았다. 다음 작업은 사용자 승인 하의 디자인이며 자동 착수하지 않는다.

초기 제품 기준 main `48d6528758d18bf3df41ed02ad9aa0f268654a51`; 재개 fetch main `82d19b64de0157d924a05f87ea8ee698f5d6db6b`. 둘 사이 제품 source/data/docs/art는 동일하고 감사 체크포인트만 추가됐다. 실제 Unity는 `C:/UnityProjects/Wildfolk`6000.3.19f1이다. 기존 native/canonical 차이를 조사했으며 sync/authoring/build를 실행하지 않았다.

## 결론과 완결성

등록 표면66개 = 현재57개 + 미래·미구현·역사전용9개. 의미 action185개/직렬 control instance671개/조건별 route103개. 제품 UI씬3개, Runtime/UI51개·관련 runtime29개·Editor authoring25개를 source/scene/controller/service로 대조했다. 반복slot/recipe/class/언어를 화면 수로 부풀리지 않았다.

현재67장에 해당하는21종 표면의 제한된 상태를 직접 이미지로 검토했다. PNG총87장 중20장은 Loading/실패막에 가려져 메뉴 증거에서 제외했다. 과거180장 인덱스 중10장을 열람하고 현재 source와 구분했다. contact sheet는 비교용으로 축소했으며 선택 원본을 별도 열었다. **57개 모든 표면·모든 상태를 현재 실제 조작으로 검수한 것은 아니다.** source coverage와 visual coverage를 분리했고20개 미검증 질문을 EVIDENCE_INDEX에 남겼다. 이는 다음 작업에서 직접 검수해야 할 경계이며 누락을 합격으로 처리하지 않았다.

P0 확인0 / P1 7 / P2 15. Store→Help→Back이 Account로 돌아가는 문제는 현재 source+Unity로 확인했다. 작은 화면의 본문 축소, 가방·제작 밀도, 큰 maintenance 카드, 출석 표시/버튼 혼동, 반복 종이 panel, 비용·즉시변경 결과 노출이 주요 우선순위다. 과거 수정 기록을 지우거나 같은 결함이 남았다고 단정하지 않았다. 상세 신뢰 수준은 VISUAL_UX_ISSUES를 따른다.

## 기능 보존과 다음 요청

Save14/Profile2/Protocol23,36bag/9hotbar/장비4·Chest24/36,111recipes/WorkbenchI–III와처리방식,6common/6classes54nodes, 서버 receipt·Host 권위·pending recovery·무료 world Home를 보존한다. provider OFF와 offline read-only를 성공 거래처럼 꾸미지 않는다. multi-world/같은world solo-host는 현재 기능이 아니라 저장 identity·schema/network/Gold·pending path 정책이 필요한 별도 목표다.

Penpot 권장 pilot은 **Main Menu·Gameplay HUD·Inventory**다. Stage1 core tokens/components와 Stage2 세 pilot 방향·모바일 한영 variants부터 사용자 승인을 받는다. 전체를 같은 책 테마로 확정하지 않으며 기능을 줄여 화면을 단순화하지 않는다. 실제Penpot board·Unity 통합·새 AAB/RC·배포는 이번에 하지 않았다.

## 보고서11개

- [README.md](README.md)
- [SCREEN_INVENTORY.md](SCREEN_INVENTORY.md)
- [CONTROL_AND_ACTION_MAP.md](CONTROL_AND_ACTION_MAP.md)
- [NAVIGATION_AND_STATES.md](NAVIGATION_AND_STATES.md)
- [VISUAL_UX_ISSUES.md](VISUAL_UX_ISSUES.md)
- [UI_ARCHITECTURE_AND_ASSETS.md](UI_ARCHITECTURE_AND_ASSETS.md)
- [MOBILE_AND_LOCALIZATION.md](MOBILE_AND_LOCALIZATION.md)
- [WORLD_SAVE_AND_MULTIPLAYER_UX.md](WORLD_SAVE_AND_MULTIPLAYER_UX.md)
- [PENPOT_DESIGN_HANDOFF.md](PENPOT_DESIGN_HANDOFF.md)
- [REDESIGN_ROADMAP.md](REDESIGN_ROADMAP.md)
- [EVIDENCE_INDEX.md](EVIDENCE_INDEX.md)

## 실제 검증·보호

EVIDENCE_INDEX는 source/scene·현재/역사캡처·20미검증·adversarial20항목을 연결한다. final-validation.json은11문서/로컬링크/line/ID/hash/scope/controls 대조 결과다. resume-protection-check.json은 실제 사용자 파일·native3362파일·prefs registry hash의 전후비교다. Unity는MainMenu/Edit/PlayOFF/dirty없음/QA marker없음,GameView1920×1080 복원. Save/Protocol변경없음. 과거 checkpoint 기록·제외캡처를 삭제하지 않았다.

자동 gameplay/NUnit/data validator/Android build·물리터치·실제광고/결제/계정삭제/4인세션·전progression/오디오청취는 수행하지 않았다. 이번 report-only 변경의 검증과 거래·출시 QA를 혼동하지 않는다.

기존 RESUME.md/checkpoint-validation.json은 당시 중간 기록이다. 현재 완료 여부는이 README와final-validation을 따른다. 시간은timing.json과timing-resume-20261104.json의실제boundary이며사용자중단공백은합산하지않았다. token/billing 작업별수치는도구에서미제공(null)이며추정하지않았다.
