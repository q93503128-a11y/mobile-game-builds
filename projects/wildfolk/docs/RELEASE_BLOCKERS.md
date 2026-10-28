# Launch Backend · Release Blockers

Foundation1.5 상태는 **PARTIALLY ACTIVE**다. development/alpha 환경·Cloud Code/Cloud Save/Access Control/index는 실제 배포했고 development 실제 왕복과 인터넷 Relay를 검증했다. 아래 항목이 제품 활성화/공개 Alpha·Production을 막는다. BackendSettings.Enabled=false 유지.

- 사용자 확정 package `com.gltam.lowfire`에 연결할 Play Console PGS v2, Android/Web OAuth, 실제 서명 SHA, tester/track, UGS provider secret. 공개 Web client ID/PGS app ID/Google project number가 아직 없다.
- 실제 PlayGamesSettings resource·PGS Android manifest. plugin Web client ID와 BackendSettings 불일치를 release validator가 차단한다.
- Play Integrity API/Cloud project 연결, Secret Manager credential, 실제 cert/version/license/requestHash/freshness 승인·거절. Alpha enrollment는 닫혀 있다.
- 실제 Privacy Policy 및 외부 웹 삭제 요청 URL·운영 절차/Data safety. 가짜 URL로 gate를 통과시키지 않는다.
- 문의 담당/비활성 ticket30일 정리 job·로그 retention·삭제 요청 처리 주기, 원격 완료 통지와 다른 기기/OS 백업 정책. 현재 요청은 pending이며 완료로 오인시키지 않는다.
- 실제 Google 로그인·Link/conflict/복구·Alpha/Production AAB·출시 서명·실기기 UX·4기기. 2026-10-26 Development AAB는 실제 Gradle/IL2CPP/ARM64·debug 서명·manifest/포함 설정 검사를 통과했다([결과](../validation/launch-backend-activation/android-development/REVIEW.md)). 개발 AAB와 두 채널 C# compile·2프로세스 Relay가 실제 출시/기기 검수를 대신하지 않는다.
- Production policy/gate/index/Cloud Code/Access Control 배포는 별도 출시 작업이다. 이번에는 production의 기존 기본 환경·스크립트·정책을 변경하지 않았다.
- node-forge1.4.0의 RSA **검증** advisory GHSA-86w9-cpqp-85rv가 npm audit에 남는다(현재 patched release 없음). 현재 Lowfire는 SHA256과 RS256 **서명**만 사용하고 그 검증 API를 노출/호출하지 않는다. Account bundle에는 RSA가 포함되지 않는다. 공개 출시 전 dependency 상태/대체책을 재검토하고 미래 외부 서명 검증에 해당 API를 재사용하지 않는다. [근거와 범위](../validation/launch-backend-activation/REVIEW.md).

현재 Foundation2 Gem/Achievement/Attendance 구현·관련 QA를 완료했지만 실제 사용자 Google 계정/공개 출시 준비는 미완료다. IAP/Audio는 다음 사용자 지시 전 시작하지 않는다. Ads Foundation3 상태는 아래를 따른다.

## Launch Economy Foundation2 · 현재 경계

Foundation2의 development/alpha 경제 배포와 개발 QA는 공개 출시 승인이 아니다. 실제 Google/PGS/Integrity/공개URL/실기기 Android·터치·공개 Production 활성화는 미완료다. Foundation2.1은 remote Engineer 서버 증명과 Economy2 archive/지원 도구를 구현하고 실제 Development LAN/Relay/512행 이관을 확인했다. 최종 Android Alpha/Production compile·실제 SDK·별도 death/Boss/resource LAN 회귀는 통과했다. root3MiB/index2000key·4MiB 운영 경고 전달/비용 모니터링은 공개 출시 전 필요하며 IAP는 아직 구현하지 않으며 Ads Foundation3 상태는 아래를 따른다. [경제 전체 규칙](ECONOMY.md) · [실행·검증](../validation/launch-economy-foundation/REVIEW.md).

Foundation2.1 재개 검증 중 Cloud Code의 Authentication Get Player 조회에서 실제429(code50, Retry-After69초)를 관측했다. 대기 후 동일 서버 검증 경계로 실제 SDK 회귀를 통과했으며 client bool/캐시·admin credential 우회는 적용하지 않았다. 공개 활성화 전 upstream 인증 요청량·지연·429와 Cloud Save bytes/operation quota를 함께 관찰하고, 장애 시 Survivor/무료Home을 유지하는 운영 대응을 검수한다. [공식 Client API 제한](https://docs.unity.com/en-us/oas-player-auth-client/1.0.0) · [실행 근거](../validation/economy-premium-mp-ledger/completion/service-rate-diagnostic.json).

## Foundation3 Ads 출시 차단

코드/격리QA와실제광고운영을구분한다. 실제 AdMob등록·unit2개·공개callback/CloudRun/CloudTasks/서버secret·UMP정책/Privacy URL·Android실기기영상·consent/SSV/retry·운영알림은미완료다. Alpha는 기본 official sample test unit을 사용한다. owned-unit 경로는 native SDK test-device guard를 구현했지만 실제 등록 기기/SSV 검증은 미실행이다. Development native Gradle/IL2CPP AAB packaging은 통과했다. 초반 managed source snapshot의 SDK packaging 검증이며 최종 source 출시 AAB/실기기/지역 consent 검증을 대체하지 않는다. Google/PGS/Integrity및기존출시차단은유지한다. Production live기본OFF,추가게임컨텐츠/IAP자동착수없음. [정확한외부설정](ADS.md).
