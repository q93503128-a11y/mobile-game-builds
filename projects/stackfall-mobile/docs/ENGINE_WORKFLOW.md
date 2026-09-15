# Stackfall Mobile 엔진 및 Unity 작업 규칙

상태: `LOCKED`

이 문서는 Stackfall Mobile의 엔진 선택과 Unity 적용/검증 주기를 고정한다.

## 1. 엔진

- 엔진: `Unity 6.3 LTS`
- 언어: `C#`
- 우선 플랫폼: `Android`
- 후속 플랫폼: `iOS`
- 기존 웹 Stackfall은 참고 원본/전투 실험 자료로 유지하며 모바일 제품 코드는 별도 구현한다.

엔진을 임의로 Godot, Web/Capacitor, 네이티브 Android 등으로 변경하지 않는다. 변경이 필요하면 기술적 근거와 마이그레이션 비용을 먼저 검토하고 Design Bible과 이 문서를 함께 갱신한다.

## 2. Unity 프로젝트 위치

실제 클라이언트는 아래 구조를 사용한다.

```text
projects/stackfall-mobile/
├─ client/                 # Unity 프로젝트 루트
│  ├─ Assets/
│  ├─ Packages/
│  └─ ProjectSettings/
├─ server/                 # 서버/백엔드 관련 코드
├─ data/                   # 밸런스·콘텐츠 원본 데이터/스키마
├─ tools/                  # 검증기·변환기·시뮬레이션
├─ docs/
└─ README.md
```

Unity가 생성하는 `Library/`, `Temp/`, `Logs/`, `obj/`, 빌드 산출물 등은 Git에 넣지 않는다.

## 3. 매 작업마다 Unity Editor를 열지 않는다

개발은 `항상 Unity Editor를 실행한 상태`로 진행하지 않는다.

다음 작업은 Unity를 열지 않고 연속 진행할 수 있다.

- 기획서/벤치마크/밸런스 설계
- 데이터 스키마와 콘텐츠 테이블 작성
- 순수 C# 로직 작성 및 코드 리뷰
- 서버 코드
- 검증기/시뮬레이터/툴 작성
- 문서와 테스트 케이스 작성
- Unity 프로젝트의 텍스트 기반 설정 중 안전하게 검증 가능한 변경

즉, 여러 작업을 GitHub `main`에 누적한 뒤 정해진 검증 지점에서 Unity를 열어 한꺼번에 확인해도 된다.

단, `Unity를 열지 않는다`는 것이 임시 코드나 미검증 통합을 무제한 쌓는다는 뜻은 아니다. 코드 구조·참조·데이터 계약은 작업마다 정리하고, 죽은 코드와 대체된 구현은 즉시 제거한다.

## 4. Unity 검증이 반드시 필요한 작업

다음 변경은 Unity Editor 또는 실제 모바일 빌드 검증 전에는 완료로 판정하지 않는다.

- Scene/Prefab 변경
- SerializedField 연결
- Animator/Animation 변경
- Input System 설정
- Physics/Layer/Tag 설정
- URP/Renderer/Shader/VFX 변경
- Addressables/AssetBundle 관련 변경
- Package 추가/업데이트
- ProjectSettings 변경
- Android/iOS 플랫폼 설정
- 네이티브 SDK, 광고, IAP, 로그인, 푸시 연동
- Safe Area 및 해상도/UI 레이아웃
- 실제 프레임/메모리/발열에 영향을 주는 전투 최적화

이러한 변경도 파일 작성 자체는 먼저 할 수 있지만, Unity import/compile/play/build 검증 전에는 `검증 대기` 상태로 취급한다.

## 5. 기본 검증 주기

매 작은 커밋마다 Unity를 실행하지 않는다. 대신 다음 단위에서 검증한다.

### A. 기능 묶음 검증

밀접한 기능 한 묶음이 끝났을 때:

1. Unity 프로젝트 열기
2. 전체 import/compile 오류 0 확인
3. 관련 Scene/Prefab 참조 확인
4. Play Mode 수동 테스트
5. Console error 확인
6. 실패하면 그 묶음에서 바로 수정

### B. 전투 마일스톤 검증

전투/스테이지 기능의 큰 단계가 끝났을 때:

- 실제 5~15분 플레이
- FPS 및 프레임 스파이크
- 메모리 증가/누수
- 적·투사체·VFX 가독성
- 터치 조작
- 앱 백그라운드/복귀

### C. 모바일 빌드 게이트

주기적으로 Android 실기기/에뮬레이터 빌드를 만든다.

필수 확인:

- 설치/실행
- 화면비와 Safe Area
- 터치
- 일시정지/복귀
- 저사양 성능
- 발열/배터리
- 저장/복구

결제·광고·로그인 단계 이후에는 해당 SDK도 실제 모바일 빌드에서 검증한다.

## 6. 사용자 로컬 Unity 적용 방식

사용자가 매 개발 턴마다 자신의 로컬 Unity 프로젝트를 갱신할 필요는 없다.

기본 흐름:

1. ChatGPT가 GitHub 정본에서 작업
2. 일정 기능 묶음까지 `main`에 축적
3. 테스트/인계 시점에 최신 프로젝트 폴더를 ZIP으로 이 채팅에 직접 전달
4. 사용자는 전달받은 폴더의 `client/`를 Unity Hub에서 열어 import/compile
5. 수동 플레이/모바일 테스트
6. 발견된 문제를 같은 정본에서 수정

### 6.1 채팅 전달 패키지 규칙

- 사용자가 테스트해야 하는 시점에는 GitHub 링크만 던지지 않는다.
- `projects/stackfall-mobile/` 최신 정본을 짧은 이름의 ZIP으로 채팅에 첨부한다.
- 기본 파일명: `STACKFALL_MOBILE.zip`
- `Library/`, `Temp/`, `Logs/`, `obj/`, 빌드 산출물, 로컬 캐시, 백업은 ZIP에서 제외한다.
- 테스트 설명과 개발 진행상황은 ZIP 내부 게임 UI에 넣지 않고 채팅에서만 전달한다.
- 게임 안에는 개발자 메모/진행률/테스트 문구를 넣지 않는다.
- 사용자가 별도로 전체 저장소를 요구하지 않는 한 `.git`과 공용 저장소의 다른 프로젝트는 전달하지 않는다.

## 7. 완료 판정 원칙

- 문서 작업은 문서 검증으로 완료 가능
- 순수 로직은 코드/테스트 검증으로 중간 완료 가능
- Unity 직렬화/에셋/플랫폼 의존 작업은 Editor 검증 전 최종 완료 금지
- 모바일 UX/성능 관련 작업은 실제 모바일 빌드 검증 전 최종 완료 금지
- Unity를 오랫동안 열지 않았더라도 마지막 검증 이후 누적된 변경 목록을 명확히 유지한다.

## 8. 쓰레기 코드 금지

공용 `RULES.md`를 그대로 적용한다.

특히 다음을 금지한다.

- 임시 구현과 정식 구현을 동시에 남김
- 사용하지 않는 MonoBehaviour/ScriptableObject/Prefab을 방치
- 테스트용 분기를 릴리스 코드에 상시 유지
- 이전 데이터 스키마와 새 스키마를 이유 없이 병존
- 에디터 테스트를 안 했다는 이유로 `TODO`만 붙이고 구조 문제를 방치

교체가 끝난 구현은 같은 작업에서 제거하고 참조까지 정리한다.
