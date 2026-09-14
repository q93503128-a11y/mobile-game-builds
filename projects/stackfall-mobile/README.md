# Stackfall Mobile

기존 웹/PC용 `q93503128-a11y/Stackfall`의 핵심 전투 아이디어를 바탕으로 모바일 환경에 맞게 새로 설계하는 스테이지형 라이브서비스 액션 RPG 프로젝트다.

이 프로젝트는 웹판의 단순 포팅이 아니다. 전투 정체성은 참고하되 입력, 카메라, 적 밀도, 스테이지 길이, UI, 성장, 경제, 서버, 소셜, 라이브서비스 구조는 모바일 기준으로 재설계한다.

## 기술 기준

- 엔진: `Unity 6000.3.16f1 (Unity 6.3 LTS)`
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
8. [`docs/SKILL_EVOLUTION_V0.md`](./docs/SKILL_EVOLUTION_V0.md) — 12개 공격의 Rank 1~6 전투 진화
9. [`docs/CHAPTER_ENEMY_BOSS_RULES.md`](./docs/CHAPTER_ENEMY_BOSS_RULES.md) — 챕터/적/엘리트/보스 제작 규칙
10. [`docs/CHAPTER_CATALOG_V0.md`](./docs/CHAPTER_CATALOG_V0.md) — 초기 8챕터 실제 적/엘리트/보스 카탈로그

성장/경제:

11. [`docs/PARTS_SYSTEM.md`](./docs/PARTS_SYSTEM.md) — 코어/프레임/드라이브/임팩터/오비터/리액터 영구 성장
12. [`docs/S_PROTOTYPES_V0.md`](./docs/S_PROTOTYPES_V0.md) — 첫 S 프로토타입 4세트 × 6부품
13. [`docs/BENCHMARKS.md`](./docs/BENCHMARKS.md) — 외부 성공작 구조/수치 조사
14. [`docs/BALANCE_V0.md`](./docs/BALANCE_V0.md) — 첫 가챠/등급/중복/전투 수치 범위와 Monte Carlo 결과
15. [`docs/GACHA_BANNERS_V0.md`](./docs/GACHA_BANNERS_V0.md) — 표준/픽업/목표 부품/천장/중복 규칙
16. [`docs/ECONOMY_V0.md`](./docs/ECONOMY_V0.md) — 30/90/180일 무료/유료 성장 목표

UI/반복/소셜:

17. [`docs/HOME_HANGAR_UI.md`](./docs/HOME_HANGAR_UI.md) — 홈/격납고/기체/회수/도전 정보구조
18. [`docs/CHALLENGE_MODES_V0.md`](./docs/CHALLENGE_MODES_V0.md) — 던전/보스랭킹/탑/아레나/길드보스 보상 루프
19. [`docs/GUILD_CHAT_V0.md`](./docs/GUILD_CHAT_V0.md) — 길드/국가·글로벌 채팅/모더레이션 규칙

기술/서버/데이터:

20. [`docs/SERVER_ARCHITECTURE.md`](./docs/SERVER_ARCHITECTURE.md) — 서버 권위 경계
21. [`docs/ENGINE_WORKFLOW.md`](./docs/ENGINE_WORKFLOW.md) — Unity 작업/검증 방식
22. [`docs/DATA_SCHEMA_V0.md`](./docs/DATA_SCHEMA_V0.md) — 정적/계정/전투 세션 데이터 구조
23. [`data/README.md`](./data/README.md) — 정적 데이터 정본 폴더 규칙

공용 개발 규칙은 저장소 루트의 [`RULES.md`](../../RULES.md)를 따른다.

## 검증/밸런스 도구

- [`tools/balance/gacha_sim.py`](./tools/balance/gacha_sim.py) — S 확률/천장/픽업/고정 소환 예산 Monte Carlo
- [`tools/balance/progression_sim.py`](./tools/balance/progression_sim.py) — 대표 세트/목표 부품/조율 데이터 포함 장기 획득 Monte Carlo
- [`tools/data/validate_static_data.py`](./tools/data/validate_static_data.py) — JSON 파싱/ID/참조/수치/확률 기본 정적 검증
- [`tools/source/validate_runtime_source.py`](./tools/source/validate_runtime_source.py) — Runtime 임시/디버그/개발문구/위험한 구현 패턴 검사
- [`.github/workflows/stackfall-mobile-static.yml`](../../.github/workflows/stackfall-mobile-static.yml) — 정적 데이터/소스 위생/도구 구문 검사

도구 출력은 참고/검증 결과이며 Design Bible과 밸런스 정본을 자동으로 대체하지 않는다.

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

`Phase 1 source implementation complete → Unity validation gate`

Phase 0는 완료되었다. Phase 1 첫 전투 프로토타입의 소스 구현도 완료되어 현재는 Unity 실제 검증을 기다린다.

현재 Phase 1 소스 범위:
- 모바일 드래그 이동 + PC 확인용 키보드 이동
- 자동 조준
- 풀링 기반 적/경험치/투사체/중력장
- 일반 적 압력 + 엘리트 3회 + 5분 최종 보스
- 엘리트 예고 돌진
- 보스 광역 펄스/돌진 패턴
- 경험치/레벨업 + 실제 3택 강화 선택
- 코어 펄스 / 펄스 블레이드 / 중력 우물
- 선체/동력/보스 타이머/보스 체력 HUD
- 클리어/실패 상태
- 위험 범위 시각표현과 실제 판정 반경 정합

다음 게이트는 Unity Editor import/compile, Play Mode 5분 실플레이, Console error 0, 이후 Android 테스트 빌드다. 이 검증 전에는 Phase 1을 완료로 판정하지 않는다.
