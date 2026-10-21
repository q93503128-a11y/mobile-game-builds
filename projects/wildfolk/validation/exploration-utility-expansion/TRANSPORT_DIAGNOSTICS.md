# 2-E transport 검수 이력

기존 NetworkSession prefab / MultiplayerSceneBuilder는 DisconnectTimeoutMS4000(4초), queue128이었다. 설치된 Unity Transport NetworkParameterConstants 기본은30000이다. 로그의 Multiplayer/MainMenu 씬 초기화가6초 이상 걸리는 상황에서 Client가 Playing/approved=true/connected=true 후 ClosedByRemote를 받았다.

|실행|조건|결과|
|---|---|---|
|play-client|4초/queue128|초기 sync 실패, receive queue full 경고|
|play-client-queue|4초/QA queue4096|초기 sync 실패|
|play-client-ready|4초/QA queue4096, Host ready 확인|승인/연결 후 remote closure, owner 미생성|
|play-client-diagnostic|Host 시작 전 Client launch|진단 fixture 시작 순서 오류, 중단; 제품 실패로 판단하지 않음|
|play-client-30s|정본30초/기본queue128|실제2P 신규 utility 왕복 통과|
|play-client-final|HUD 추가 검수 fixture가 Host ready 전 Client launch|시작 순서 오류로 접속 실패; XML 보존|
|play-client-hud|Host Playing/WorldReady 확인 후 Client 시작,30초/128|실제2P 재통과, 실제 Host HUD 화살표/문구와 disconnect 제거까지 통과|

queue4096 QA 변경은 제거했다. canonical 변경은 builder/prefab의 동일 DisconnectTimeoutMS 필드 하나뿐이다. Join12초/approval/20Hz/input authority/protocol 외 다른 접속 정책은 유지했다. Client 재시도/timeout 전체 설계를 확장하지 않았다. 진단 log.tsv 원자료는 headless.tsv, 실행 결과는 각 XML이다.

초기 EditMode의 새 비전투 Tool 분류 가정1건은 테스트 조건을 수정해 재통과했다. 첫 SP fixture는 실제 SceneFlow.Load가 coroutine owner를 파괴하므로 중단됐으며, 실제 scene reload 결과는 MCP로 별도 확인하고 자동 fixture에서는 같은 파일의 검증된 snapshot Apply 경로를 사용했다. 최초 Host logical receiver는 이전 Host 위치0만 기대한 fixture 오류였으며 최신 유효 신호를 고르는 실제 정책에 맞춰 조건을 교정했다. 두 경우 실제 production 행동을 테스트 편의로 변경하지 않았다. 실패/중단을 새 기능 통과로 집계하지 않는다.

실제 원격2P LAN은 Editor Host+headless Client이며 4개 logical sender 검사와 구분한다. Android/Relay/4기기/지연·손실, Client 실제 화면은 미검증이다. 전체 Unity startup .log는 일부 혼합 인코딩 경로가 있어 로컬 원자료로 유지하고 커밋된 XML/TSV/RESULTS/본 기록을 검수 증거로 사용한다.
