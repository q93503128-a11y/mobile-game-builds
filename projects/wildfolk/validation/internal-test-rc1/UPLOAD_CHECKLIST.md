# RC1 · 사용자 수동 업로드

2026-10-30 [내부 테스트 공식 안내](https://support.google.com/googleplay/android-developer/answer/9845334?hl=en)와 [release 생성 안내](https://support.google.com/googleplay/android-developer/answer/9859348)를 확인했다. 이번 작업에서는 Console 업로드·release·tester 변경을 하지 않는다.

1. AAB 옆 `BUILD_METADATA.json`의 SHA256을 파일과 대조하고, 업로드 키/DPAPI 복구 자료를 안전하게 백업한다. 비밀번호는 채팅/Git에 보내지 않는다.
2. Play Console에서 Lowfire 앱을 선택한다. 아직 앱 항목이 없다면 사용자 계정에서 먼저 생성한다. 첫 artifact 업로드 후 package `com.gltam.lowfire`는 고정된다.
3. **Test and release → Testing → Internal testing → Create new release**. 버튼이 막히면 Dashboard가 실제 요구하는 항목을 확인한다. 다른 track을 선택하지 않는다.
4. 첫 release의 **Play App Signing** 안내를 진행한다. 기존 다른 앱 키를 재사용하지 말고 이 앱의 app signing 선택을 검토한다. 이 AAB의 stable upload certificate와 Google이 관리할 app signing certificate는 구분한다.
5. `Lowfire-Internal-RC1-v1.0.0-rc1-2.aab`를 업로드한다. package/Lowfire/versionCode2/API36/ARM64와 Console 오류를 확인한다. `More`에서 보존한 native symbols를 추가할 수 있다. 이미 bundle에 포함된 symbols와 동일 빌드 자료만 사용한다.
6. release 이름 `Lowfire Internal RC1`과 아래 notes를 입력하고 **Save as draft → Next**로 검토한다. 없는 Privacy/Delete URL·PGS/Integrity/상품 값을 임의로 채우지 않는다. 계정·광고·현금 구매 OFF를 유지한다.
7. **Testers** 탭에서 사용자 테스트 목록과 실제 feedback URL/email을 설정하고 저장한다. 내부 tester 지정 자체가 license tester 설정이나 실결제 차단을 보장하지 않는다. RC1의 구매 차단을 해제하지 않는다.
8. 오류를 해결한 뒤 **내부 테스트**임을 다시 확인하고 해당 화면의 rollout/start test를 사용자가 실행한다. 제공되는 opt-in 링크로 설치한다. 최초 링크/배포 반영에 시간이 걸릴 수 있다. 공개·Closed/Open/Production 배포는 하지 않는다.

Notes 초안(한국어 locale에 입력):

> Lowfire 첫 내부 테스트 빌드입니다. 오프라인 생존·제작·전투·저장과 음향을 확인합니다. 계정 연결, 광고 보상, 현금 구매는 비활성화되어 있습니다. 기기 성능, 터치, 화면 회전/복귀, 오디오와 저장 후 Continue를 테스트해 주세요.

설치 후 첫 검증: Android 버전/기종 기록 → 콜드 실행 → 새 게임/저장/강제 종료/Continue → Settings/Profile/업적/출석/Store OFF → 터치/Safe Area/오디오 focus/성능 → LAN/Alpha Relay를 별도로 기록. 실제 기기 결과는 현재 **UNVERIFIED**다. 재설치/Android backup 복원은 별도 프로필로 검증하며 실제 사용자 save를 지우지 않는다.

PGS/OAuth·Integrity를 나중에 활성화할 때 Console **App integrity**에서 Play app signing 인증서 지문을 확인한다. 지금 제공하는 upload 인증서를 Play 설치본의 서명으로 단정하지 않는다. 외부 설정은 [SETUP_REQUIRED](../../docs/SETUP_REQUIRED.md)를 따른다. 내부 테스트는 불완전한 앱 설정으로도 가능하지만 Console의 실제 오류/권한·계정 상태를 여기서 완료했다고 주장하지 않는다.
