# 사용자 종료 요청에 따른 중간 체크포인트 · Foundation2 미완료

2026-10-26 사용자가 노트북 종료를 위해 현재 작업까지만 마치고 멈추도록 요청했다. 구현 대부분과 핵심 서비스 검증은 끝났지만 **Foundation2 완료/출시 준비 완료로 선언하지 않는다**. 다음 작업은 Foundation2 잔여 검증 마무리이며 Ads/IAP/Store/Audio로 넘어가지 않는다.

## 재개 순서

1. remote main을 다시 fetch하고 이 체크포인트 이후 변경을 먼저 확인한다. AGENTS/README/ECONOMY/이 REVIEW와 HANDOFF를 읽는다. main 직접 작업·canonical source·PR/임시 branch 금지.
2. 마지막 graphical viewport 검사에서 `text bounds achievements-ko/Selected Achievement (1280,720)`가 실패했다. 세부 업적 Text는 independent ScrollRect에 들어가 있으나 해상도/locale 전환 후 preferredHeight와 content rect 높이 갱신이 불일치한다. **원인은 아직 확정하지 않았다**. manual size 계산 대신 canonical ContentSizeFitter/정확한 layout 갱신을 검토하고, 기존 글꼴 크기를 유지한 채 긴 EN/KO 조건·tier 끝까지 스크롤 가능한지 확인한다. assertion을 삭제하거나 overflow 허용으로 숨기지 않는다. Editor/EconomyUiBuilder와 Runtime/UI/EconomyScreen이 정본이다.
3. 최종1920x1080/1280x720/844x390 viewport 모두 다시 통과시키고 실제 PNG를 눈으로 검수한다. latest visual/failed.txt, checks.txt 및 PNG는 실패를 포함한 실제 실행 기록이다. 이전48/79초 통과 기록을 최종 layout 통과로 대체하지 않는다. account/profile badge, received/empty event/offline/insufficient Engineer·확인 modal, SP Gem 버튼을 확인한다.
4. UI 수정 후 관련 최소 Edit/Play와 Alpha/Production Android script compile을 같은 최종 source로 재확인한다. 이번 Android57/57 assembly compile은 통과했지만 마지막 UI scroll 필드 변경 전 실행이다. 이번 pass의 새 AAB는 만들지 않았으며 기존 Foundation1.5 AAB를 최신으로 주장하지 않는다.
5. 전체 변경의 최종 수동 diff/계약/문서 감사와 source validator·secret scan·실제 save hash/mtime/bytes 비교를 마무리한다. REVIEW의 현재 상태와 최종 결과를 갱신한다. live `C:/UnityProjects/Wildfolk`는 이번에 **동기화하지 않았다**. 동기화할 경우 사용자 수정과 HEAD baseline을 먼저 비교하며 실제 사용자 save/Profile2를 QA로 쓰지 않는다.
6. 완료될 때 main commit/push/remote==HEAD/clean을 다시 확인하고 사용자 요청의 전체 최종 보고를 한다. Google/PGS/OAuth/Privacy/삭제 URL은 기존 미설정이고 backend Enabled=false다. 가짜 값/secret을 넣지 않는다.

## 이미 확인한 것

- 서버64/64, 최종 EditMode74/74, PlayMode 최종 두 실행의 고유17개 통과(세부 checkpoint-validation.json). 마지막 추가7개는 Class mirror dirty 상태와 실제 인증 연결 상태 분리, explicit offline 유지 검사를 포함한다.
- 실제 development 서비스33개: wallet0, direct write 차단, 완료와수령 분리, 중복/ClaimAll/출석/day위조 거절, revoke/refund/admin/Engineer와 Account projection, 계정 분리. 실제 Unity SDK에서도 출석3/관리자 QA준비+200/Engineer−120/부활−8/실패예약−8·환급+8/무료Home·Save14를 확인했다. 최종75Gem은 QA값이며 사용자 잔액이 아니다.
- 실제2프로세스 Protocol22 LAN/resource/보스/loot/reHost와 server→owner progress duplicate coalescing 통과. Relay/실제Google/실기기 Android/실제 터치는 이번 패스에서 검증하지 않았다.
- development/alpha의 LowfireAccount/LowfireIntegrityVerify/LowfireEconomy와 Private economy_config 배포. production mutation 없음. 이번에 만든7 QA identity의 Protected account/support/economy를 모두 삭제한 뒤 Authentication을 삭제했고 임시 cleanup Cloud Code도 삭제했다(qa-cleanup.json). 재개 시 삭제한 계정/token을 재사용하지 않는다.
- 실제 사용자 world2개 SHA256/mtime/bytes 일치. token/credential pattern 전수검사3541 text files0건. secret scan은 패턴 검사의 한계가 있으며 최종 diff 감사가 남아 있다. 작업 중 process command line 조회에 Hub 인증 인자가 출력된 실수를 사용자에게 알렸고 파일/Git에 복사하지 않았다. 이후 조회는 PID로 제한했다.

## 도구와 저장소

현재 세션 Unity MCP가 도구 목록에 없어 hidden isolated Editor batch로 검사했다. Windows 입력/Focus/Show는 사용하지 않는다. 재개 시 도구 목록을 새로 확인한다. parent workspace의 economy_*.py는 작업 편의용 로컬 orchestration일 뿐 정본이 아니며 canonical Editor QA/Node tests/data validator가 저장소에 있다. QA project는 C:/UnityProjects/LowfireAndroidQa/tests/artifacts/unity 및 canonical/projects/wildfolk/tests/artifacts/unity(ignored)다. 모든 이번 QA 프로세스는 종료 후 끊는다.

## 버전과 경제

Save14 / localProfile2 / CloudProfile1 / EconomySchema1 / Protocol22 / generation3. 신규 RPC 때문에 Protocol만 올렸다. 초기0, 업적22family58tier/특수9/213Gem/785Point, 신규7회45, 월간28회40, Engineer120, revive8. 월초정렬30/90일287/372는 실제 측정 아닌 행동·출석 모델이다. 월간 일반slot은 출석 기록만 있고 Gem은7/14/21/28회다. source ownership/수용·배제 이유와 ledger capacity/modified Host 한계는 ECONOMY와 REVIEW에 기록돼 있다.
