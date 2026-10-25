# Launch Backend · Release Blockers

Foundation1 소스 구현과 실제 서비스 오픈은 구분한다. 현재 backend는 disabled이며 아래 항목이 공개 Alpha/Production을 막는다.

- UGS production 한 개는 읽기 조회로 확인했다. development/alpha 생성, 실제 서비스 역할·기존 Relay 환경 확인 및 나머지 name/ID mapping이 필요하다.
- Play Console PGS v2 앱·서명·Web/Android OAuth·tester/track, UGS provider secret 설정과 생성된 Android manifest.
- Cloud Save index, 초기 creation gate/policy, Cloud Code publish, Access Control 배포·실제 direct-write rejection.
- Play Integrity service credential은 Secret Manager에만 설정; 실제 기기 app/version/cert/license/hash/freshness 검증.
- 실제 Privacy Policy URL과 외부 웹 삭제 요청 경로. 앱 안 request만으로 외부 경로가 준비됐다고 간주하지 않는다.
- 문의 처리·ticket retention·삭제 요청 처리 담당/실행 주기, 모든 환경 data cleanup과 Authentication 삭제의 end-to-end 검증.
- 서버 요청 접수 후 현재 기기 account cache/auth/session 정리의 실기기 검증 및 원격 완료 통지. 오프라인 기기의 world/cache를 원격에서 지웠다고 표시하지 않는다.
- 실제 Android Gradle/IL2CPP/AAB, Google 계정 로그인·복구, 실제 Relay/4기기/장시간 품질. script compile와 localhost LAN은 이 검증과 다르다.

이후 Gem/Achievement/Attendance/Ads/IAP/Audio는 별도 사용자 지시와 release pass다. 현재 Alpha/Production 현금 결제는 구조적으로 비활성이다.
