# Stackfall Mobile

기존 웹/PC용 `q93503128-a11y/Stackfall`의 핵심 전투 아이디어를 바탕으로 모바일 환경에 맞게 새로 설계하는 스테이지형 라이브서비스 액션 RPG 프로젝트다.

이 프로젝트는 웹판의 단순 포팅이 아니다. 전투 정체성은 참고하되 입력, 카메라, 적 밀도, 스테이지 길이, UI, 성장, 경제, 서버, 소셜, 라이브서비스 구조는 모바일 기준으로 재설계한다.

## 기술 기준

- 엔진: `Unity 6.3 LTS`
- 언어: `C#`
- 우선 플랫폼: `Android`
- 후속 플랫폼: `iOS`
- 기존 웹 Stackfall은 참고 원본/전투 연구 자료로 유지
- 실제 모바일 클라이언트는 `projects/stackfall-mobile/client/`을 Unity 프로젝트 루트로 사용
- 매 작업마다 Unity Editor를 열지 않는다. 코드·데이터·문서는 GitHub 정본에서 계속 개발하고, 기능 묶음/테스트 마일스톤에서 Unity import·compile·Play Mode·모바일 빌드 검증을 수행한다.

세부 규칙은 [`docs/ENGINE_WORKFLOW.md`](./docs/ENGINE_WORKFLOW.md)를 따른다.

## 핵심 방향

- 한 스테이지 약 5~15분
- 뱀서류/생존 액션 기반 자동 공격 + 모바일 이동 중심 조작
- 영구 성장의 핵심은 전통 RPG 장비가 아닌 `기체 부품`
- 부품 장착·등급·돌파에 따라 실제 인게임 기체의 형태와 실루엣 변화
- 메인 스테이지 + 던전 + 보스 + 무한 콘텐츠 + 비동기 경쟁 + 랭킹
- 국가/글로벌/길드 단위 경쟁 및 채팅
- 강한 가챠/패키지/패스/월정액/이벤트 수익화 구조
- 서버 권위 경제·가챠·랭킹·길드·채팅
- 외부 성공작의 검증된 구조와 수치 범위를 벤치마킹하되 고유 표현과 전투 구조로 재설계

## 기획 정본

1. [`docs/DESIGN_BIBLE.md`](./docs/DESIGN_BIBLE.md)
2. [`docs/MOBILE_CONVERSION.md`](./docs/MOBILE_CONVERSION.md)
3. [`docs/BENCHMARKS.md`](./docs/BENCHMARKS.md)
4. [`docs/ROADMAP.md`](./docs/ROADMAP.md)
5. [`docs/ENGINE_WORKFLOW.md`](./docs/ENGINE_WORKFLOW.md)
6. [`docs/SERVER_ARCHITECTURE.md`](./docs/SERVER_ARCHITECTURE.md)

공용 개발 규칙은 저장소 루트의 [`RULES.md`](../../RULES.md)를 따른다.

## 기존 Stackfall과의 관계

기존 웹판에서 참고할 핵심:

- 여러 공격 장비를 조합해 화면과 공격 방식이 실제로 변하는 구조
- 집중 사격, 공전, 연쇄, 중력, 드론, 관통, 광선 등 서로 다른 공격 정체성
- 중첩에 따라 무기가 수치뿐 아니라 동작 단계까지 변화하는 설계
- 엘리트/보스/이벤트로 전투 흐름을 끊어주는 구조

그대로 가져오지 않는 것:

- 마우스 기반 수동 조준
- PC 화면 크기를 전제로 한 탄막과 정보 밀도
- 무한 생존 중심 시간 구조
- 웹판의 현재 수치와 성장곡선
- 클라이언트 단독 저장을 전제로 한 구조

## 현재 단계

`Pre-production / Design Bible 0.x`

코드를 성급히 늘리기보다 먼저 전투, 스테이지, 부품, 성장, 경제, 가챠, 콘텐츠 해금, 서버 권위 경계를 문서로 고정한다.
