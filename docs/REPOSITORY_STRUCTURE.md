# 저장소 구조 정본

`mobile-game-builds`는 여러 모바일 게임을 함께 관리하는 모노레포다.

## 1. 루트 구조

```text
mobile-game-builds/
├─ README.md
├─ RULES.md
├─ .gitignore
├─ docs/
│  └─ REPOSITORY_STRUCTURE.md
├─ projects/
│  └─ <project-id>/
│     ├─ README.md
│     ├─ docs/
│     ├─ client/
│     ├─ server/
│     ├─ data/
│     ├─ tools/
│     └─ tests/
└─ shared/
   └─ README.md
```

## 2. 프로젝트 폴더 계약

각 프로젝트는 아래 의미를 유지한다.

- `README.md`: 프로젝트 목적, 실행 방법, 현재 상태, 정본 문서 링크
- `docs/`: Design Bible, 시스템 명세, 벤치마크, 밸런스 근거, 아키텍처
- `client/`: 모바일 클라이언트 코드와 에셋
- `server/`: 인증, 저장, 경제, 가챠, 랭킹, 길드, 채팅 등 서버 권위 코드
- `data/`: 정적 게임 데이터, 밸런스 표, 스키마, 로컬라이징 원본
- `tools/`: 데이터 검증, 변환, 빌드 보조, 감사 도구
- `tests/`: 자동 테스트와 테스트 픽스처

폴더가 아직 필요하지 않으면 억지로 빈 파일을 채우지 않는다. 필요해지는 순간 위 위치에 만든다.

## 3. `shared/` 사용 조건

`shared/`는 “나중에 쓸 것 같은 코드”를 모으는 장소가 아니다.

다음 조건을 모두 만족할 때만 공용화한다.

1. 실제 두 개 이상의 프로젝트에서 동일한 책임으로 사용됨
2. 프로젝트 고유 규칙을 몰라도 동작함
3. 공용화가 중복 제거에 실제로 이득임
4. 별도 테스트가 있음

조건을 만족하지 않으면 각 프로젝트 안에 둔다.

## 4. 문서 정본 계약

각 프로젝트의 `docs/DESIGN_BIBLE.md`가 기획 최상위 정본이다.

세부 문서는 Design Bible을 확장하며, 서로 충돌하면 Design Bible을 갱신해 정합성을 회복한다.

권장 세부 문서:

- `BENCHMARKS.md`
- `MOBILE_CONVERSION.md`
- `ECONOMY_AND_GACHA.md`
- `COMBAT_AND_STAGES.md`
- `PARTS_SYSTEM.md`
- `LIVE_SERVICE.md`
- `SERVER_ARCHITECTURE.md`
- `ROADMAP.md`

## 5. 금지 구조

- 프로젝트 루트 밖에 프로젝트 전용 코드를 흩뿌리는 것
- `misc/`, `old/`, `backup/`, `final-final/` 같은 의미 불명 폴더
- 동일 데이터의 JSON/CSV/코드 상수 이중 정본
- 임시 패치 파일을 영구 보관하는 구조
- 빌드 결과물, 로컬 로그, 개인 백업을 Git에 커밋하는 것

## 6. 구조 변경 절차

루트 구조 변경이 정말 필요한 경우:

1. 변경 이유를 문서화한다.
2. 기존 프로젝트에 미치는 영향을 확인한다.
3. `README.md`, `RULES.md`, 이 문서를 같은 작업에서 갱신한다.
4. 새 구조로 이동한 뒤 구 경로를 남겨두지 않는다.
