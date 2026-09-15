# Stackfall Mobile 엔진 및 Unity 작업 규칙

상태: `LOCKED`

이 문서는 Stackfall Mobile의 엔진 선택과 Unity 적용/검증 주기를 고정한다.

## 1. 엔진

- 엔진: `Unity 6000.3.19f1 (Unity 6.3 LTS)`
- changeset: `7689f4515d75`
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
├─ server/
├─ data/
├─ tools/
├─ docs/
└─ README.md
```

Unity가 생성하는 `Library/`, `Temp/`, `Logs/`, `obj/`, 빌드 산출물 등은 Git에 넣지 않는다.

## 3. 사용자가 Unity Hub에서 여는 방법

새 빈 프로젝트를 만들지 않는다.

1. 전달받은 `STACKFALL_MOBILE.zip` 압축 해제
2. Unity Hub의 `프로젝트` 화면으로 이동
3. `디스크에서 프로젝트 추가(Add project from disk)` 선택
4. `stackfall-mobile/client/` 폴더 선택
5. Unity `6000.3.19f1`로 열기

`client/` 폴더에는 이미 `Assets/`, `Packages/`, `ProjectSettings/`가 있으므로 이것이 완성된 Unity 프로젝트 루트다.

빈 Universal 2D/3D 프로젝트를 새로 만든 뒤 파일을 덮어씌우는 방식은 사용하지 않는다. 프로젝트 설정/패키지/메타 정보가 갈라질 수 있기 때문이다.

## 4. 매 작업마다 Unity Editor를 열지 않는다

개발은 항상 Unity Editor를 실행한 상태로 진행하지 않는다.

Unity를 열지 않고 연속 진행 가능한 작업:
- 기획/벤치마크/밸런스
- 데이터 스키마/콘텐츠 테이블
- 순수 C# 로직 및 코드 리뷰
- 서버 코드
- 검증기/시뮬레이터/툴
- 문서/테스트 케이스
- 안전하게 검증 가능한 텍스트 기반 설정

단, 미검증 통합이나 임시 코드가 무제한 쌓인다는 뜻은 아니다. 코드 구조와 참조는 작업마다 정리하고 죽은 코드/대체 구현은 즉시 제거한다.

## 5. Unity 검증이 반드시 필요한 작업

다음은 Unity Editor 또는 실제 모바일 빌드 검증 전 최종 완료로 판정하지 않는다.

- Scene/Prefab
- SerializedField 연결
- Animator/Animation
- Input System
- Physics/Layer/Tag
- URP/Renderer/Shader/VFX
- Addressables/AssetBundle
- Package 추가/업데이트
- ProjectSettings
- Android/iOS 플랫폼 설정
- 네이티브 SDK, 광고, IAP, 로그인, 푸시
- Safe Area/해상도/UI 레이아웃
- 실제 프레임/메모리/발열에 영향을 주는 최적화

## 6. 첫 사용자 테스트 게이트

첫 테스트는 전투 원형만 덩그러니 실행한 상태에서 하지 않는다.

최소한 다음이 연결된 뒤 사용자에게 테스트를 요청한다.

1. 홈/격납고
2. 상단 재화/프로필/전투력
3. 중앙 기체 전시
4. 하단 기본 탭
5. 메인 스테이지 카드와 `출격`
6. 액티브/지원 출격 편성 화면
7. 전투
8. 결과 화면
9. 홈 복귀 또는 다음 스테이지 이동

전투 쪽 최소 조건:
- 이동/자동조준
- 3종 이상 액티브
- 3택 강화
- 경험치/레벨업
- 엘리트
- 보스
- 클리어/실패

이 조건 전에는 사용자에게 `테스트해 달라`고 요구하지 않는다.

## 7. 기본 검증 주기

### A. 기능 묶음 검증

1. Unity 프로젝트 열기
2. 전체 import/compile 오류 0
3. 관련 Scene/Prefab 참조 확인
4. Play Mode 수동 테스트
5. Console error 확인
6. 실패 시 같은 묶음에서 수정

### B. 전투 마일스톤 검증

- 실제 5~15분 플레이
- FPS/프레임 스파이크
- 메모리 증가/누수
- 적/투사체/VFX 가독성
- 터치 조작
- 앱 백그라운드/복귀

### C. 모바일 빌드 게이트

- 설치/실행
- 화면비/Safe Area
- 터치
- 일시정지/복귀
- 저사양 성능
- 발열/배터리
- 저장/복구

결제·광고·로그인 이후에는 해당 SDK도 실제 모바일 빌드에서 검증한다.

## 8. 사용자 로컬 Unity 적용 방식

사용자가 매 개발 턴마다 자신의 Unity 프로젝트를 갱신할 필요는 없다.

기본 흐름:
1. ChatGPT가 GitHub 정본에서 작업
2. 기능 묶음까지 `main`에 축적
3. 테스트/인계 시 최신 프로젝트 폴더를 ZIP으로 채팅에 직접 전달
4. 사용자는 ZIP의 `client/`를 Unity Hub에서 프로젝트로 추가
5. import/compile
6. 수동 플레이/모바일 테스트
7. 문제를 같은 정본에서 수정

### 8.1 채팅 전달 패키지 규칙

- 테스트 시 GitHub 링크만 던지지 않는다.
- `projects/stackfall-mobile/` 최신 정본을 짧은 ZIP으로 첨부한다.
- 기본 파일명: `STACKFALL_MOBILE.zip`
- `Library/`, `Temp/`, `Logs/`, `obj/`, Build, 캐시, 백업 제외
- 개발 진행상황/테스트 설명은 채팅에서만 전달
- 게임 안에는 개발자 메모/진행률/테스트 문구 금지

## 9. 완료 판정 원칙

- 문서: 문서 검증으로 완료 가능
- 순수 로직: 코드/테스트 검증으로 중간 완료 가능
- Unity 직렬화/에셋/플랫폼 의존: Editor 검증 전 최종 완료 금지
- 모바일 UX/성능: 실제 모바일 빌드 전 최종 완료 금지

## 10. 쓰레기 코드 금지

공용 `RULES.md`를 그대로 적용한다.

금지:
- 임시 구현과 정식 구현 병존
- 사용하지 않는 MonoBehaviour/ScriptableObject/Prefab 방치
- 테스트용 분기를 릴리스 코드에 상시 유지
- 이전/신규 데이터 스키마를 이유 없이 병존
- TODO만 붙이고 구조 문제 방치

교체가 끝난 구현은 같은 작업에서 제거하고 참조까지 정리한다.
