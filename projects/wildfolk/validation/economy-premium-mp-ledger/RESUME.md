# Foundation2.1 · 중단 및 다음 재개

2026-10-27 사용자 사용량 요청으로 현재 검증 묶음까지 마치고 중단한다. **전체 Foundation2.1은 아직 완료하지 않았다. 다음 기능은 시작하지 않는다.** 코드/문서/evidence를 main checkpoint commit으로 보존한다. 시작 SHA b9ce7c6ce63b698b7ec575faae0e3fde611f0fcf. 최종 checkpoint SHA는 git log의 해당 commit을 기준으로 한다.

## 완료한 묶음

서버 Engineer admission, 공통 LAN/Relay spawn gate, Host/remote/reconnect/unavailable, Economy2 archive/index/migration/영구 중복 방지/admin·deletion 경계, canonical docs. Node95/95, hidden Edit97/97, Play29통과+명시적LAN테스트의Host없음1실패, Live MCP Edit13/13·Play1/1. 실제 HTTP20/20/legacy512archive와 실제 두프로세스 LAN/Relay 성공. 가격/콘텐츠/Save14/Profile2/CloudProfile1/generation3 불변, Protocol23. Development/Alpha만 관련3script 배포, Production 변경 없음. 실제 Google/PGS 성공 아님.

## 다음 순서

1. remote main fetch → local HEAD 일치/clean → AGENTS/README 및 이 REVIEW와 정본 docs 재독. 현재 원격 소스를 우선한다.
2. source/code/diff 최종 검수와 남은 관련 필수 체크를 실행한다. source validator는 이번 checkpoint에서 PASS(1626 unique GUID)했다. 프로젝트에 이미 있는 검증 도구를 먼저 확인한다.
3. 최종 Android Alpha/Production compile 검증. 실제 device/PGS/OAuth/Integrity/Privacy·삭제URL은 사용자 설정 전 불가이며 가짜 설정을 만들지 않는다.
4. EconomyQa.ActualBatch로 새Schema2 실제 SDK 업적/출석/Engineer/부활8차감·실패환급/무료Home/Save14 회귀. 새 isolated identity만 만들고 QA목록을 기록한 뒤 private→player data→Auth 순으로 정리한다. 이전에 삭제한 QA ID를 재사용하지 않는다.
5. broad Play의 Explicit ActualSeparateLanClientLifecycle을 ready Host 없이 넣은1실패를 전체 통과라고 하지 않는다. 실제 ready Host를 먼저 띄우는 기존 DeathFoundationQa 별도2process 흐름으로 관련 death/Save/reHost 회귀를 실행한다. 필요 시 기존 Boss/resource LAN 회귀를 재사용한다. 이미 성공한 premium LAN/Relay/Node 모델을 변경 근거 없이 반복하지 않는다.
6. Live source는25파일 반영·MCP 검증 완료. 새 변경이 생기면 사용자 edit-mode 및 씬dirty 여부 확인, baseline 비교/backup 후 필요한 source만 반영한다. 실제UserData는 수정 금지. Play 테스트는 isolated QA 경로만.
7. Foundation2.1 요청의 최종20항목 보고를 완성하고 docs의 checkpoint/미완료 표시를 실제 결과에 맞게 갱신한다. main commit/push → remote SHA 재조회 → HEAD일치/clean 확인 후 STOP.

## 정리/보호 증거

qa-cleanup.json의 completed=true와 temporaryScriptsRemoved를 확인한다. 사용자 save 해시/mtime/bytes 전후 비교, Live MCP 최종 edit/MainMenu/console은 checkpoint-final.json을 따른다. 실제 secrets/session token은 Git/채팅에 저장하지 않는다. per-operation token은 도구로 측정할 수 없어 null이며 시간만 timing.json에 실측한다. 오래된 pass REVIEW는 덮어쓰지 않는다.
