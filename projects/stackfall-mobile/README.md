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
- 플레이어 화면에는 개발 메모·진행률·디버그/테스트 문구를 노출하지 않음

## 기획 정본

최상위:

1. [`docs/DESIGN_BIBLE.md`](./docs/DESIGN_BIBLE.md) — 전체 게임 방향
2. [`docs/ROADMAP.md`](./docs/ROADMAP.md) — 구현 순서/게이트
3. [`docs/PLAYER_TEXT_POLICY.md`](./docs/PLAYER_TEXT_POLICY.md) — 플레이어 노출 문구 절대 규칙

전투/모바일 변환:

4. [`docs/MOBILE_CONVERSION.md`](./docs/MOBILE_CONVERSION.md) — PC/Web → 모바일 변환 규칙
5. [`docs/PC_DNA_MAP.md`](./docs/PC_DNA_MAP.md) — 기존 Stackfall 기능 전수 분류
6. [`docs/COMBAT_FOUNDATION.md`](./docs/COMBAT_FOUNDATION.md) — 스테이지/조작/레벨업/적 밀도/보스 전투 기준
7. [`docs/CONTENT_CATALOG_V0.md`](./docs/CONTENT_CATALOG_V0.md) — 첫 공격/지원/일반 부품 제작 후보
8. [`docs/CHAPTER_ENEMY_BOSS_RULES.md`](./docs/CHAPTER_ENEMY_BOSS_RULES.md) — 챕터/적/엘리트/보스 제작 규칙

성장/경제:

9. [`docs/PARTS_SYSTEM.md`](./docs/PARTS_SYSTEM.md) — 코어/프레임/드라이브/임팩터/오비터/리액터 영구 성장
10. [`docs/S_PROTOTYPES_V0.md`](./docs/S_PROTOTYPES_V0.md) — 첫 S 프로토타입 4세트 × 6부품
11. [`docs/BENCHMARKS.md`](./docs/BENCHMARKS.md) — 외부 성공작 구조/수치 조사
12. [`docs/BALANCE_V0.md`](./docs/BALANCE_V0.md) — 첫 가챠/등급/중복/전투 수치 범위와 Monte Carlo 결과
13. [`docs/GACHA_BANNERS_V0.md`](./docs/GACHA_BANNERS_V0.md) — 표준/픽업/목표 부품/천장/중복 규칙

기술/서버:

14. [`docs/SERVER_ARCHITECTURE.md`](./docs/SERVER_ARCHITECTURE.md) — 서버 권위 경계
15. [`docs/ENGINE_WORKFLOW.md`](./docs/ENGINE_WORKFLOW.md) — Unity 작업/검증 방식

공용 개발 규칙은 저장소 루트의 [`RULES.md`](../../RULES.md)를 따른다.

## 밸런스 도구

- [`tools/balance/gacha_sim.py`](./tools/balance/gacha_sim.py) — S 확률/천장/픽업/고정 소환 예산 Monte Carlo 시뮬레이터

밸런스 도구의 출력은 참고 결과이며 Design Bible/밸런스 정본을 자동으로 대체하지 않는다.

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

`Pre-production / Phase 0`

현재까지:
- 공용 모바일 저장소 구조/규칙 고정
- Unity 6.3 LTS 확정
- 플레이어 노출 문구에서 개발/디버그/진행상황 표현 금지 고정
- 기존 웹 Stackfall 공격/패시브/이벤트/초월체의 모바일 전환 분류 완료
- 모바일 전투 기반 V0 고정
- 전투 공격 12종/지원 16종/일반 영구 부품 18종 후보 정리
- 6부품 영구 성장 V0 고정
- 첫 S 프로토타입 4세트 × 6부품 효과/외형 설계
- 표준 회수/프로토타입 공명/목표 부품 조율 구조 설계
- 챕터 10스테이지 단위와 적/엘리트/보스 제작 규칙 설계
- 가챠/등급/중복/기본 전투 수치의 첫 시뮬레이션 범위 고정
- 가챠 Monte Carlo 도구화 및 1차 분포 확인

Phase 0의 다음 핵심은 `전투 내 스킬의 단계별 진화표`, `초기 8개 챕터의 실제 적/보스 카탈로그`, `30/90/180일 경제 시뮬레이션`이다. 이 세 축이 충분히 고정되면 Phase 1 Unity 전투 프로토타입으로 이동한다.
