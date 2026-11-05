# 그래픽 에셋 제작 계획

## 결정 원칙

디자인 의미가 있는 실루엣·재질은 그림 에셋으로 제작합니다. 데이터에 따라 변하는 글자/값/수량과 입력/상태는 Unity의 실제 UI 컴포넌트로 표현합니다. 완성 화면 전체를 한 장 이미지로 베이크하지 않습니다. 그래야 localization, 상태 변경, 접근성, 해상도 대응, 기능 테스트가 유지됩니다.

## 에셋별 제작 방식

| 에셋 | 방식 | 결정/작업 |
|---|---|---|
| 로고/브랜드 | A 기존 LowfireWordmark 재사용 | 승인 파일의 투명도/여백만 확인. 저해상도 확대 금지 |
| 패널·프레임 | B 수정 + C 새 원본 | 숲의 얇은 목재/잎/불빛 힌트를 절제한 2–3 surface family를 원본 SVG/KRA로 제작 후 PNG sprite export |
| 9-slice | B 기존 panel.png 조사 | 18px 기존 slice metadata와 코너 압축을 확인. 늘어나는 edge에 무늬/곡선이 없을 때만 재사용; 장식/텍스처의 큰 panel에 무조건 확대 금지 |
| 버튼 배경 | C 새 그래픽 + D live text | Primary/secondary/destructive/disabled의 작은 배경 세트. 글자와 hitbox는 별도. 눌림/선택은 Unity state |
| 36 인벤토리 슬롯 | B 수정 + D | B의 슬롯 구획 원칙 사용. 슬롯 바탕은 반복 가능한 공통 sprite, 수량/내구도/선택/잠금은 live UI |
| 장비 슬롯/캐릭터 | A 기존 아이템·직업 그림 + D | 원본 장비/직업 일러스트와 아이콘 재사용. 현재 없는 캐릭터 그림은 추가 제작 제안 전 승인 |
| 체력/허기/갈증/XP | D Unity UI | fill/image, 숫자, 아이콘을 실제 값과 연결. 수치/막대를 구워 넣지 않음 |
| 아이콘 | A 기존 Lowfire 아이템/class 아이콘, Lucide navigation subset | 아이콘 역할 그룹별 optical size, stroke, silhouette 정리. 기존 Lucide license/source metadata 보존 |
| 선택/포커스 효과 | D Unity | 이중 outline/glow 남용 없이 surface+shape+label. 색만으로 구별하지 않음 |
| 팝업·알림 | B 수정 + D | 테두리와 배경 sprite 재사용, 문구/버튼/카운트다운 값 live text |
| 스크롤 단서 | D Unity | ScrollRect, 선택적 edge cue/섹션 화살표. 스크롤 상태와 연결하고 항상 장식으로 그리지 않음 |
| 터치 컨트롤 | C 제한된 버튼 배경 + D | 공격/상호작용 등의 명료한 별도 액션 glyph. 실제 입력 크기와 시각 크기 분리 가능하되 hit area는 확인 |
| 직업·스킬 그래픽 | A 기존 6 class 아이콘 + B/C 신규 픽토그램 후보 | 기존 분류/장식 체계 먼저 평가, 새 아이콘은 state, meaning, licensing 정의 후 제작. 능력 묘사 과장 금지 |

## 원본·라이선스 정책

- Lowfire 제작 KRA/PNG는 게임 제작 원본을 기준으로 수정 가능; 정본 경로는 SOURCE_ART.md와 EVIDENCE_INDEX를 확인합니다.
- NotoSansKR OFL 라이선스 파일을 보존하며 폰트 패밀리를 새로 도입하지 않습니다.
- Lucide 기반 UI icon의 ISC license와 현재 icon source manifest/저작자 공지를 보존합니다. 원본과 별도 라이선스가 없는 이미지 검색 결과나 게임 캡처를 게임 에셋으로 사용하지 않습니다.
- 신규 이미지·폰트·사운드를 추가한다면 파일별 출처, 버전, 라이선스, 상용/수정·배포 허용을 확인하고 LICENSES에 기록한 뒤 승인을 받습니다.
- 외부 상용게임 UI에서 레이아웃 원칙만 학습하며 그래픽/아이콘을 복제하지 않습니다.

## Unity 임포트·성능

1. 제작 원본은 편집 가능한 SVG/KRA 또는 허가된 벡터 소스로 보존하고, Unity용 PNG는 투명 여백·pixel-per-unit·slice border 포함 export preset으로 관리합니다.
2. 9-slice는 corner/edge/center가 844·1280·1920에서 변형되지 않는지 확인합니다. 가느다란 테두리가 작은 화면에서 흐릿해지는지 실제 device density에서 봅니다.
3. 반복 슬롯과 공통 버튼은 동일 sprite/material/atlas를 공유합니다. 문자와 숫자를 atlas 이미지로 베이크하지 않습니다.
4. Sprite Atlas는 UI 전용으로 묶되 최대 크기/압축 포맷을 플랫폼 기기 프로파일링 후 결정합니다. 메모리 예산과 압축은 데이터 없이 숫자를 임의로 정하지 않습니다.
5. alpha blending overdraw, atlas 중복, mipmap/필터링, texture import size를 확인합니다. 스크롤 목록의 아이템 아이콘은 기존 리소스를 재사용합니다.
6. 성능 완료 기준은 실제 Android에서 대표 화면 3개와 고밀도 인벤토리/클래스 그리드를 열어 GPU/CPU/메모리/프레임-time을 측정하는 것입니다. 현재 측정값은 없습니다.

## 에셋 패키지 승인 체크

- [ ] 원본 + export + license/source metadata 보존
- [ ] Slice border 및 resize preview 검사
- [ ] 한국어/영어 live text와 최대 라벨 길이 검사
- [ ] sprite atlas packing 및 Android 실제 texture memory 측정
- [ ] 844×390, 1280×720, 1920×1080 visual diff
- [ ] 후보 art가 실제 게임의 숲/생존 미감과 맞는지 사용자 승인
