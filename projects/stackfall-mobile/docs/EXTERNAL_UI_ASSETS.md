# Stackfall Mobile 외부 UI 자산 기록

상태: `Phase 1B presentation shell`

목적: 프로그래머 아트만으로 플레이어-facing UI를 구성하지 않고, 상업 이용이 명확한 외부 무료 자산을 선별해 제품 화면의 완성도를 높인다. 외부 게임의 스크린샷·고유 아트·브랜드 이미지를 복제하지 않는다.

## 1. 현재 런타임 포함 자산

경로:

`client/Assets/Resources/StackfallExternal/`

현재 포함 PNG:

- `gear.png` — 설정
- `gift.png` — 우편/보상
- `market.png` — 상점
- `trophy.png` — 임무/도전
- `news.png` — 이벤트/공지
- `pouch.png` — 회수/소환
- `lock.png` — 잠금 상태
- `chat.png` — 길드/소셜 계열

사용 방식:

- Unity `Resources.Load<Texture2D>("StackfallExternal/<name>")`로 읽는다.
- 아이콘은 정보 구분용이며 게임 규칙/상태의 정본이 아니다.
- 아이콘이 로드되지 않아도 내부 ID나 파일 경로를 플레이어에게 표시하지 않는다.

## 2. 출처와 라이선스

직접 가져온 파일 위치:

- `https://github.com/Bit-Serenity-Studios/Match3/tree/main/assets/icons`

해당 저장소의 자산 라이선스 기록:

- `https://github.com/Bit-Serenity-Studios/Match3/blob/main/ASSETS_LICENSES.md`

그 기록은 `assets/icons/` UI/icon set을 Kenney 및 Quaternius의 CC0 원천을 미러링한 자산으로 명시하고, 라이선스를 `CC0 1.0 (public domain)`으로 기록한다. 상업적 사용 가능, 필수 저작자 표시 없음, copyleft 없음으로 기록되어 있다.

Stackfall Mobile에서는 이 중 일반적인 기능 아이콘만 사용한다. 로고, 상표, 타 게임의 고유 캐릭터/브랜드 요소는 사용하지 않는다.

## 3. 파일 체크섬

2026-09-15 반영본 SHA-256:

- `chat.png` — `1e876e76c7f5efe9d5c734310cb66b4292c513dffc326093ea9a744050c0923d`
- `gear.png` — `d20dd95cb1f0d527671f10ff44259c84527705fcda8ed9d74c043960fca4bb27`
- `gift.png` — `794120874003b5b06da0abd0428ba1d76ebc2355f8937ab9f36e83d4f5291d77`
- `lock.png` — `cb242cecafabfd15a6bd713c29ff7325106e05c2e94e816e12f9dcc2beccc98e`
- `market.png` — `f099499f277566027c749cdb334d09a999084e3f1a0a76cb712810abb1408bfd`
- `news.png` — `2af1610dec2612b9c554a77818ccbf9fc9ca0581f2bfdd0d4f6b8ab2085d9619`
- `pouch.png` — `998a152ce808c19530ae0ece44f066433f9be90cbcdb7f44c10f132b66b30110`
- `trophy.png` — `505d5570aad9fb0478fcc1e5ec4b3bd2377f593a20013b2cff97c8e955abc515`

## 4. 시각 참고 자산

실제 런타임 파일을 그대로 복제하지 않고 UI 언어를 고를 때 참고한 무료 자산:

### Kenney — UI Pack - Sci-Fi

- 공식 페이지: `https://kenney.nl/assets/ui-pack-sci-fi`
- 분류: 2D UI Pack
- 구성: 버튼/패널/슬라이더/SF 인터페이스 계열
- 공식 표시 라이선스: Creative Commons CC0

적용 원리:

- 각진 SF 패널
- 청록/남색 계열 강조
- 기술 장비용 정보 카드처럼 보이는 위계
- 작은 기능 아이콘 + 큰 핵심 CTA 조합

Stackfall Mobile은 Kenney 팩의 화면 배치를 복제하지 않는다. 현재 런타임 패널은 UI Toolkit으로 자체 조립하며, 외부 자산은 아이콘과 시각 언어 보강에 사용한다.

## 5. 로딩 화면

로딩 화면의 우주 배경은 현재 외부 일러스트를 직접 번들하지 않는다.

대신 런타임 UI로 다음 요소를 조합한다.

- 별 필드
- 멀리 있는 행성
- 희미한 성운
- 중앙 조립 기체 엠블럼
- 진행 바
- 전술 팁

이 방식은 특정 게임의 우주 일러스트를 복제하지 않으면서도 화면 전환을 제품 화면처럼 보이게 한다. 이후 실제 배경 이미지가 필요할 경우 CC0/상업 이용 허용 출처와 체크섬을 이 문서에 먼저 추가한 뒤 사용한다.

## 6. 추가 자산 도입 규칙

새 외부 UI/이미지를 추가할 때 반드시 같은 변경에서 기록한다.

1. 원본 URL
2. 원 저작자/배포자
3. 라이선스
4. 상업 이용 가능 여부
5. attribution 요구 여부
6. 실제 프로젝트 내 파일 경로
7. 가능하면 SHA-256

라이선스가 불명확한 타 게임 스크린샷, 추출 APK 리소스, 팬 위키 이미지, 워터마크 이미지 등은 런타임 자산으로 사용하지 않는다.
