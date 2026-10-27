# Foundation3 재개 지점 · 2026-10-27 사용자 요청 중단

이번 변경을 main에 commit/push한 **중간 체크포인트**다. Foundation3 전체 완료/출시 완료가 아니다. 다음에는 새 branch/PR 없이 remote main을 먼저 fetch하고 AGENTS/README/ADS/ECONOMY/이 REVIEW를 읽는다. 시작기준은7d48ceb였으며 이checkpoint의실제SHA는git log/직전최종응답을확인한다.

이미 끝난 것:
- provider4route·실제UI Mine+5게임 조사, 기존가격변경없음,1Gem/UTC1회 정책/model.
- 공식GMA11.6.0 SDK/Androidadapter/UMP gate, defaultOFF, Developmentfixture release제외.
- signedquery verifier/durableCloudTasks ingress/OIDC+HMACworker, Economy2 CAS 영구receipt/idempotency/archive/삭제지원.
- scoped durable revive earnedproof + world save 성공시에만소모, Profile hub·업적/월간stamp UI 직렬화.
- Node124/124, Edit17/17, Play26/26, 최신nativevisual저장실패/부활/캡처PASS, AndroidAlpha58/Prod58compilePASS.
- Development/AlphaLowfireRewarded actual publish 기본OFF. 실제UGS SDK 정책차단·위조bool거절PASS; 새QAanonymous identity만data빈것확인후삭제.
- 실제사용자save2개fingerprint그대로. 열린 C:\UnityProjects\Wildfolk는동기화하지않았다.

다음 작업 (범위 확대/재조사 반복 금지):
1. 현재구현의마지막계약·security·관련DataValidator 점검을마무리한다. broad17/26이미통과한검사는필요없이반복하지않고,후속retrylabel/SDKfullscreen/scopedinterface관련잔여만선별한다. UI builder재실행은QA project에서만하며수동scene변경은보호한다.
2. Alpha는현재공식sampleunit만허용한다. owned-unitSSV와연결되는실제Alpha end-to-end가필요하면공식SDK test-device 보장을검증한명시guard를먼저설계/확인한다. 테스트키만넣었다고realunit을무조건허용하지말고지원되지않는Java API를추측해사용하지않는다. 현재samplecallback을금융증거로승격하면안된다.
3. 사용자실제AdMob/PGS/Privacy/CloudRun/queue/secret값은아직없다. ADS.md12단계에서준비가능한것만진행하고가짜URL/credential생성금지. 서버SSVtestingtool과영상test는분리기록한다. NativeGradle/AAB·실기기·지역UMP/GoogleSSV/queue운영은미실행임을유지한다.
4. LiveUnity반영전MCP연결/Play상태확인. 이번에는MCP도구가없어격리nativebatch만썼다. Windows입력/Focus/Show금지. UserData복사/수정금지. canonicalSDK별도Assets/GoogleMobileAds와Plugins/Android도포함해야하며Ads폴더만누락동기화하지않는다.
5. 남은범위의실제실행결과를REVIEW/문서에반영하고전체Foundation3완료조건을최종판정한다. 다시maincommit/push/remote==local/clean확인후멈춘다. IAP/Store/Audio/새콘텐츠를자동시작하지않는다.

운영 보존: private Development/Alphaads_config.Enabled=false,SsvReady=false,ProductionLiveEnabled=false. Production정책변경0. provider계정/실제adunit/공개callback/GoogleCloud프로젝트/secret이준비되기전새광고show는failclosed. 실제서비스credential은사용자가Dashboard/SecretManager에서직접설정한다.
