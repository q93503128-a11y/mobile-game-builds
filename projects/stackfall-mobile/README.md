# Stackfall Mobile

기존 웹/PC용 `q93503128-a11y/Stackfall`의 핵심 전투 아이디어를 바탕으로 모바일 환경에 맞게 새로 설계하는 스테이지형 라이브서비스 액션 RPG 프로젝트다.

이 프로젝트는 웹판의 단순 포팅이 아니다. 전투 정체성은 참고하되 입력, 카메라, 적 밀도, 스테이지 길이, UI, 성장, 경제, 서버, 소셜, 라이브서비스 구조는 모바일 기준으로 재설계한다.

## 기술 기준

- 엔진: `Unity 6000.3.19f1 (Unity 6.3 LTS)`
- changeset: `7689f4515d75`
- 언어: `C#`
- 우선 플랫폼: `Android`
- 후속 플랫폼: `iOS`
- 기존 웹 Stackfall은 참고 원본/전투 연구 자료로 유지
- 실제 모바일 클라이언트는 `projects/stackfall-mobile/client/`을 Unity 프로젝트 루트로 사용
- 새 빈 Unity 프로젝트를 만들지 않고 `client/`를 Unity Hub에서 `디스크에서 프로젝트 추가`로 연다.
- 매 작업마다 Unity Editor를 열지 않는다. 기능 묶음/테스트 마일스톤에서 import·compile·Play Mode·모바일 빌드 검증을 수행한다.
- 테스트/인계 시점에는 최신 `projects/stackfall-mobile/` 폴더를 ZIP으로 채팅에 직접 전달한다.

세부 규칙은 [`docs/ENGINE_WORKFLOW.md`](./docs/ENGINE_WORKFLOW.md)를 따른다.

## 핵심 방향

- 한 스테이지 약 5~15분
- 뱀서류/생존 액션 기반 자동 공격 + 모바일 이동 중심 조작
- 공격/지원 능력은 메인 스테이지 진행에 따라 순차 해금
- 액티브/지원 능력은 출격 전 후보 덱으로 편성하고, 전투 중 3택으로 실제 빌드를 완성
- 영구 성장의 핵심은 전통 RPG 장비가 아닌 `기체 부품`
- 부품 장착·등급·돌파에 따라 실제 인게임 기체의 형태와 실루엣 변화
- 메인 스테이지 + 던전 + 보스 + 무한 콘텐츠 + 비동기 경쟁 + 랭킹
- 국가/글로벌/길드 단위 경쟁 및 채팅
- 강한 가챠/패키지/패스/월정액/이벤트 수익화 구조
- 서버 권위 경제·가챠·랭킹·길드·채팅
- 외부 성공작의 검증된 정보구조·성장 구조·수치 범위를 벤치마킹하되 고유 표현과 전투 구조로 재설계
- 플레이어 화면에는 개발 메모·진행률·디버그/테스트 문구를 노출하지 않음

## 기획 정본

최상위:
1. [`docs/DESIGN_BIBLE.md`](./docs/DESIGN_BIBLE.md)
2. [`docs/ROADMAP.md`](./docs/ROADMAP.md)
3. [`docs/PLAYER_TEXT_POLICY.md`](./docs/PLAYER_TEXT_POLICY.md)

전투/모바일 변환:
4. [`docs/MOBILE_CONVERSION.md`](./docs/MOBILE_CONVERSION.md)
5. [`docs/PC_DNA_MAP.md`](./docs/PC_DNA_MAP.md)
6. [`docs/COMBAT_FOUNDATION.md`](./docs/COMBAT_FOUNDATION.md)
7. [`docs/CONTENT_CATALOG_V0.md`](./docs/CONTENT_CATALOG_V0.md)
8. [`docs/SKILL_EVOLUTION_V0.md`](./docs/SKILL_EVOLUTION_V0.md)
9. [`docs/UNLOCK_PROGRESSION_V0.md`](./docs/UNLOCK_PROGRESSION_V0.md)
10. [`docs/ACTIVE_LOADOUT_V0.md`](./docs/ACTIVE_LOADOUT_V0.md) — 액티브 8/지원 8 후보 덱, 전투 슬롯 6+4
11. [`docs/CHAPTER_ENEMY_BOSS_RULES.md`](./docs/CHAPTER_ENEMY_BOSS_RULES.md)
12. [`docs/CHAPTER_CATALOG_V0.md`](./docs/CHAPTER_CATALOG_V0.md)

성장/경제:
13. [`docs/PARTS_SYSTEM.md`](./docs/PARTS_SYSTEM.md)
14. [`docs/S_PROTOTYPES_V0.md`](./docs/S_PROTOTYPES_V0.md)
15. [`docs/BENCHMARKS.md`](./docs/BENCHMARKS.md)
16. [`docs/BALANCE_V0.md`](./docs/BALANCE_V0.md)
17. [`docs/GACHA_BANNERS_V0.md`](./docs/GACHA_BANNERS_V0.md)
18. [`docs/ECONOMY_V0.md`](./docs/ECONOMY_V0.md)

UI/반복/소셜:
19. [`docs/HOME_HANGAR_UI.md`](./docs/HOME_HANGAR_UI.md) — 첫 테스트 전 홈/편성/전투/결과 제품 껍데기 기준
20. [`docs/EXTERNAL_UI_ASSETS.md`](./docs/EXTERNAL_UI_ASSETS.md) — 외부 CC0 UI/아이콘 출처·라이선스·체크섬
21. [`docs/CHALLENGE_MODES_V0.md`](./docs/CHALLENGE_MODES_V0.md)
22. [`docs/GUILD_CHAT_V0.md`](./docs/GUILD_CHAT_V0.md)

기술/서버/데이터:
23. [`docs/SERVER_ARCHITECTURE.md`](./docs/SERVER_ARCHITECTURE.md)
24. [`docs/ENGINE_WORKFLOW.md`](./docs/ENGINE_WORKFLOW.md)
25. [`docs/DATA_SCHEMA_V0.md`](./docs/DATA_SCHEMA_V0.md)
26. [`data/README.md`](./data/README.md)

공용 개발 규칙은 저장소 루트의 [`RULES.md`](../../RULES.md)를 따른다.

## 검증/밸런스 도구

- `tools/balance/gacha_sim.py`
- `tools/balance/progression_sim.py`
- `tools/data/validate_static_data.py`
- `tools/source/validate_runtime_source.py`
- `tools/source/validate_first_test_readiness.py`
- `.github/workflows/stackfall-mobile-static.yml`

## 기존 Stackfall과의 관계

가져올 핵심:
- 중첩에 따라 공격 동작이 실제로 변화
- 집중/공전/연쇄/중력/드론/관통/광선
- 엘리트/보스/이벤트로 전투 흐름 변화

그대로 가져오지 않음:
- 마우스 수동 조준
- PC 전용 화면 밀도
- 과도한 투사체
- 무한 생존형 장기 스케일링
- 웹판 수치 직접 이식
- 클라이언트 단독 저장

## 현재 단계

`Phase 1B interactive presentation shell integrated → Unity verification pending`

전투 코어 위에 모바일 제품 껍데기 소스가 연결되었다.

현재 연결:
- 홈/격납고 메인 화면
- 상단 프로필/전투력/재화
- 중앙 조립 기체 전시
- 하단 탭
- 메인 스테이지 카드/출격 CTA
- 액티브 8 + 지원 8 출격 덱 편성
- 전투 런타임 진입/정리
- 결과 화면
- 홈/다음 스테이지 복귀
- 우주 로딩 화면
- 설정 / 우편 / 임무 / 상점 / 소환 외형 화면
- 홈의 라이브서비스형 빠른 진입/이벤트 배너
- 외부 CC0 기능 아이콘 8종
- 파일럿 프로필 / 7일 출석 / 이벤트 허브
- 초기 8챕터 선택/미리보기
- 부품 보관함 / 기체 프리셋 외형
- 도전 콘텐츠 허브
- 길드 기능 허브
- 하단 회수/도전/길드 탭의 최종 정보구조 미리보기
- 짧은 앱 스타트업/브랜드 화면
- 360×640급 소형 화면용 compact UI 분기
- 우편/임무/출석/무료 보급 상태에 연동되는 홈 알림 배지
- 첫 Unity 테스트용 정적 프리플라이트 검사
- 표준 회수 / 프로토타입 공명 / 목표 부품 조율 회수 허브
- 우편 개별/전체 수령 및 임무 보상 수령 상태
- 임무 일일/주간/업적, 상점 추천/일일/재화, 부품 6슬롯 실제 탭 전환
- 출석 보급 수령, 무료 일일 보급, 크리스탈 부족 소환 안내
- 설정 토글/그래픽 품질 선택 상태 유지
- 부품 선택/장착 후 기체 화면·출격 편성 요약 즉시 반영
- 공용 보상 모달 / 안내 모달 / 토스트 피드백

다만 Unity Editor import/compile 및 실제 Play Mode 화면 검증은 아직 하지 않았으므로 사용자 첫 플레이 테스트는 계속 보류한다. Unity 6000.3.19f1에서 오류 0과 전체 화면 흐름을 확인한 뒤 첫 테스트 게이트를 연다.
