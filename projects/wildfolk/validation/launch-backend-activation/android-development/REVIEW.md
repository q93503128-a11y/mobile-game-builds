# Foundation1.5 · Development Android 실제 빌드 후속 (2026-10-26)

시작 remote/local main: `efc7ac5427888fcf62c3cd3ec0fcc9d554e56905`, fetch 후 clean 일치 확인. 종료 SHA는 이 기록을 담은 main commit으로 최종 보고에서 확인한다. 상태는 계속 **PARTIALLY ACTIVE**다. 사용자는 Google/PGS/OAuth/Integrity/공개 URL을 아직 설정하지 않았다고 확인했다.

## 실제 변경과 원인

1. `AccountCompileQa.DevelopmentBundle`을 기존 Editor QA 정본에 추가했다. 명시적인 격리 batch 프로젝트, ASCII 프로젝트/결과 경로, 기존 Development 환경, Enabled=false/AllowDevelopmentIdentity=false, custom keystore 없음, release define 없음, IL2CPP/ARM64를 요구한다. 기존 제품3씬을 Development AAB로 빌드하며 release validator는 수정하지 않는다.
2. `Assets/Plugins/Android/GooglePlayGamesManifest.androidlib`에는 `project.properties`와 폴더 meta만 있었고 manifest/코드/resource/실제 PGS ID는 없었다. 실제 AGP9.0.0/Gradle9.1.0이 이 빈 모듈의 namespace 누락으로 실패했다. 쓸모없는 두 파일과 빈 폴더를 정본에서 제거했다. 가짜 manifest/namespace/Google ID, Gradle 후처리 override는 추가하지 않았다. 실제 PGS Setup의 `GPGSUtil.GenerateAndroidManifest → WriteFile`이 부모 디렉터리와 manifest를 다시 생성한다. SDK 원본/설정 도구는 수정하지 않았다.
3. 원래 canonical 한글 경로는 Android 사전검사에서 거절됐다. 저장소를 이동하지 않고 `C:/UnityProjects/LowfireAndroidQa/tests/artifacts/unity`에 별도 복사본을 만들었다. 코드·JSON·씬·asmdef 일치 확인 후 기존 TestRunner 생성 에셋5개와 meta만 격리본에서 정리했다. 실제 사용자 UserData/인증 설정은 복사하지 않았다. live 프로젝트에는 시작 source와 일치하는 것을 확인한 뒤 같은 Editor helper와 빈 library 정리만 반영했다.

## 실제 검증

| 구분 | 결과 | 증거/한계 |
|---|---|---|
| ACTUAL UNITY LOCAL · 첫 시도 | 실패: non-ASCII 경로 | [receipt](unicode-path-failure.json), [process](unicode-path-process.json). native/Gradle 성공으로 간주하지 않음 |
| ACTUAL UNITY LOCAL · 영문 경로 | C#·셰이더·stripping·IL2CPP·ARM64 compile856 완료, Gradle 실패 | [빈 PGS library 실패](empty-library-failure.json). namespace 누락, AAB 없음 |
| ACTUAL UNITY LOCAL · 정리 후 | **Development AAB 성공**, 오류0/경고2 | [build](build.json), [process](build-process.json). 경고는 QA Editor의 UGS 미로그인 안내와 PGS 미설정 안내였다. Core source는 ProjectBound와 IsLoggedIn을 함께 검사하며 실제 공개 Unity project ID가 AAB globalgamemanagers에 포함된 것을 별도 확인했다. SDK 기본 환경 문자열은 runtime UgsBootstrap의 명시 SetEnvironmentName으로 대체되며 실제 Android 서비스 왕복은 아직 미검증이다 |
| Artifact inspection | bundletool validate exit0, jarsigner verified, 표준 Android Debug 인증서 | [inspection](bundle-inspection.json). production signing/업로드 아님 |
| Manifest/native | package com.gltam.lowfire, min25/target36, debuggable=true, Internet, ARM64만·libil2cpp.so | 실제 AAB에서 dump/ZIP 확인 |
| Embedded config | Enabled=false, AllowDevelopmentIdentity=false, Google 공개 ID 공란, 실제 development mapping | 실제 TextAsset 바이트를 JSON 파싱해 확인. Google 로그인 성공 증거 아님 |
| Secret patterns | AAB ZIP547개 entry 원문/NUL 제거 바이트에서 private key/GitHub/Google API/AWS/JWT 패턴0 | exhaustive proof 아님. 저장소 전수 검사는 secret-scan.json |
| Source validator | PASS, GUID1609 | [checks](checks.json). 제거한 폴더 GUID1개 감소, 아이템/씬 참조 오류 없음 |
| User save | 접근 가능한 실제2파일 hash/mtime/bytes 동일 | [save protection](user-save-protection.json) |
| ACTUAL SERVICE | 오늘 metadata 재조회 실패: Hub 미실행 | 서버 수정/재배포/새 QA 계정 없음. 전날 actual service76/LAN/Relay 증거는 당시 기록 유지 |
| MODEL / FIXTURE | 이번 추가 실행 없음 | runtime/server 미변경으로 EditMode/PlayMode/backend 회귀 반복 안 함. Unity MCP도 이번 도구 목록에 없음 |

개발용 산출물: `C:/UnityProjects/LowfireAndroidQa/results-clean-pgs/Lowfire-development.aab`, **59,985,054 bytes**, SHA256 `47273fcadc7f333c6f3f6fa425e3a8ce402f862b84c6fd8a3d0833ef034fbfd5`. Git에는 AAB/라이브러리/Gradle cache/빌드 로그를 넣지 않는다. bundletool validate와 jarsigner가 각각 구조/서명을 검사했으며 실제 폰 설치·터치·로그인·16KB 기기 실행 검증을 대신하지 않는다.

## 남은 경계와 다음 시작점

실제 PGS resources/manifest·Android/Web OAuth·서명 cert·Google project number·UGS provider/Secret Manager·Privacy/삭제 웹 URL이 여전히 없다. Alpha/Production build gate와 Enabled=false를 유지한다. 실제 Google 로그인/Link/복구/Integrity/Alpha 등록, Alpha·Production AAB/서명·실기기/4기기는 미검증이다. 원격 상태 재조회에는 Unity Hub 실행/로그인도 필요하다. node-forge verification advisory는 [공식 DB](https://github.com/advisories/GHSA-86w9-cpqp-85rv)에서 patched version 없음으로 재확인했고 기존 범위/출시 blocker를 유지한다.

콘텐츠/월드 소유권/Save14/Profile2/Protocol21·서버·환경·결제 정책은 변경하지 않았다. Foundation2는 자동 시작하지 않는다. 실제 외부 설정은 [SETUP_REQUIRED](../../../docs/SETUP_REQUIRED.md)를 따른다. 이전 REVIEW의 당시 미검증 기록은 덮어쓰지 않았다.

## 재현과 시간

ASCII 격리본에 canonical Assets/Packages/ProjectSettings를 준비하고 사용자 저장/인증 설정은 제외한다. `LOWFIRE_QA_PROJECT`/`LOWFIRE_BACKEND_RESULTS`를 명시한 숨겨진 batch 프로세스에서 Unity6000.3.19f1 `-batchmode -nographics -buildTarget Android -executeMethod Wildfolk.Editor.AccountCompileQa.DevelopmentBundle -quit`를 실행한다. 정상 PGS Setup 전에는 삭제한 빈 library skeleton을 재수입하지 않는다.

검사 명령: Unity bundled bundletool1.17.2의 `validate --bundle=...`, `dump manifest --bundle=... --module=base`, bundled JDK의 `jarsigner -verify`, `keytool -printcert -jarfile`. 실제 공개 cert 정보만 확인하며 비밀키/패스워드를 출력하지 않는다.

[단계 UTC·경과](timing.json): 첫 경로 실패680.064초, ASCII 준비64.740초, 최초 full build3328.014초, 정리 후 재빌드283.314초. startup/import/compiler/tool 대기는 각 process wall에 포함하며 중복 합산하지 않는다. 작업별 정확한 token 소비는 도구에서 제공되지 않아 null이다.

공식 근거: [Unity BuildPlayer](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html), [Build App Bundle](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/EditorUserBuildSettings-buildAppBundle.html), [Google PGS Unity 설정](https://developer.android.com/games/pgs/unity/unity-start), [Android module namespace](https://developer.android.com/build/configure-app-module#set-namespace). namespace 자체와 실제 앱/PGS credential을 구분하며, 이번에는 불완전한 빈 모듈을 정리했다.
