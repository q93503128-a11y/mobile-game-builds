# Phase 4D — 남은 Armor Details 텍스트 레이아웃 결함

**상태: PASS** — 두 선호 높이 실패를 재현하고 수정했다. Phase 4D 최종 디자인 승인은 사용자 검토 사항이며, Android 실기기 검증은 이 작업에서도 수행하지 않았다.

## 재현과 원인

2026-11-15 최신 `main` (`cc799c3c7ed5a656b07a9c1dfa57069b6ea50b55`)에서 수정 전 Unity Game View QA를 다시 실행했다. 다음 두 상태에서 동일한 실패가 재현됐다.

- 긴 영어 왕관 설명: `Armor Details` RectTransform 높이 **86**, Text의 `preferredHeight` **124.5**.
- 장비 4칸 비움: 같은 높이 **86**, 같은 `preferredHeight` **124.5**.

두 번째 QA 상태는 장비만 비운 상태다. 선택 중인 `Undead King's Crown`은 그대로 남아 있으므로, 영어 캡처에서 첫 사례와 같은 전체 설명을 계속 표시한다. 새 QA는 이 설정을 명시적으로 검사해 빈 슬롯 검사를 빈 설명 검사로 잘못 취급하지 않게 했다.

직접 원인은 `ArrangeEquipment`가 모든 설명에 86 높이를 고정하고, `Text`는 `VerticalWrapMode.Overflow`로 할당 영역 밖에도 그리던 것이다. 영어 문자열이 현재 Canvas 배율에서 세 줄로 배치될 때 필요한 실제 높이는 124.5였다. 기존 화면은 글자가 얼핏 들어가 보였지만 컴포넌트가 소유한 영역은 부족했다. 이전 원본 증거: [긴 설명 1280×720 EN](captures/visual-correction-20261114-final-v2/inventory-en-1280x720-long-item-name.png), [장비 비움 1280×720 EN](captures/visual-correction-20261114-final-v2/inventory-en-1280x720-empty-equipment.png).

## 수정

설명 높이는 내용과 현재 렌더 배율을 기준으로 다시 계산한다. 짧은 문구는 승인된 86 높이와 기존 위치를 유지한다. 비교 문구가 없는 상태에서 설명이 이를 넘으면 비어 있는 비교 행을 상세 영역으로 재배치해 설명에 `preferredHeight + 6`을 할당한다. 실제 비교 문구가 있는 경우에는 비교 문구를 숨기지 않는다. 높이 계산은 선택·언어 변경 때와 화면 크기 또는 Canvas scale factor가 바뀔 때 갱신한다.

글자 크기는 기존 30을 유지하고 best-fit 축소를 끈 상태로 둔다. 세로 overflow는 계속 `Overflow`이며 문구를 잘라내지 않는다. 추가된 런타임 경계 검사는 상세 텍스트와 선택 상태, 아이템 이름/카테고리, 네 장비 슬롯, Equip/Unequip/Split 영역이 실제로 겹치지 않는지 검사한다.

## Unity Game View 결과

격리 프로필, AutoSave 비활성 상태의 실제 Unity 6000.3.19f1 Play Mode Game View에서 두 문제 상태를 각각 **844×390, 1280×720, 1920×1080** 및 **KO/EN**으로 캡처했다. 원본 PNG 12장을 해상도별 픽셀 크기로 직접 열어 글자, 아이콘, 슬롯, 버튼의 충돌 여부를 확인했다.

| 상태 | KO 844×390 | EN 844×390 | KO 1280×720 | EN 1280×720 | KO 1920×1080 | EN 1920×1080 |
|---|---|---|---|---|---|---|
| 긴 왕관 설명 | [PNG](captures/armor-details-reflow-20261115-final/inventory-ko-844x390-long-item-name.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-en-844x390-long-item-name.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-ko-1280x720-long-item-name.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-en-1280x720-long-item-name.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-ko-1920x1080-long-item-name.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-en-1920x1080-long-item-name.png) |
| 네 슬롯 비움, 왕관 설명 선택 유지 | [PNG](captures/armor-details-reflow-20261115-final/inventory-ko-844x390-empty-equipment.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-en-844x390-empty-equipment.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-ko-1280x720-empty-equipment.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-en-1280x720-empty-equipment.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-ko-1920x1080-empty-equipment.png) | [PNG](captures/armor-details-reflow-20261115-final/inventory-en-1920x1080-empty-equipment.png) |

실제 높이 검사에서 두 상태 모두 한국어 설명은 844/1280/1920에서 각각 86/86/86 영역 안에 들어갔다. 영어 설명은 844에서 **86 / 77.80736**, 1280에서 **130.5 / 124.5**, 1920에서 **130 / 124** (할당 / 선호 높이)로 모두 통과했다. QA 로그의 전체 자동 텍스트 경계 실패는 **0**이다. 50개 PNG의 헤더 해상도 수량은 844×390 22개, 1280×720 22개, 1920×1080 6개이며 총 **12,924,450 bytes**다.

화면 검토에서 설명 문구가 비교 영역까지 자연스럽게 확장되고 액션 버튼 위에서 끝난다. 아이템 이름·카테고리·상태, 장비 프레임/아이콘, 장착 버튼과 Split 사이에 글자나 프레임 충돌이 없다. 자동 preferred-height/RectTransform 검사와 원본 픽셀 시각 검수는 별도로 확인했다.

- [QA 결과](captures/armor-details-reflow-20261115-final/qa-result.txt)
- [상세 QA 로그](captures/armor-details-reflow-20261115-final/qa-live.tsv)
- 전체 상태 링크는 [캡처 인덱스](CAPTURE_INDEX.md)를 참조한다.

## 집중 회귀 결과

- EditMode **2/2 통과**, job `93c4a80f555845319a31c9cb432609ad`: Wildfolk와 Multiplayer 씬의 장비 레이아웃, 고정 글자 크기, Overflow 유지, 기존 짧은 문구 영역 검사.
- PlayMode **5/5 통과**, job `c6169967e05d4204a7ca716c67a2fa71`: 교체/선택 초기화, 드래그·장비 해제, Split 초기화, 롱프레스 분할, 터치 분할·명시적 드롭.
- 새 실제 Game View QA: **50 PNG**, 해상도 검증 오류 0, preferred-height 경계 실패 0, 상세/장비 슬롯/액션 버튼 겹침 검사 실패 0.
- Unity 컴파일 오류는 새로 발생하지 않았다. 기존 Google.IOSResolver assembly-load 오류 항목은 남아 있다.

보호 기준 101개 파일(기존 100개 변경/미추적 자료와 `ProjectSettings/EditorSettings.asset`)은 종료 SHA-256이 작업 전 스냅샷과 모두 일치한다. PlayMode 검사 후 Editor가 `m_EnterPlayModeOptions`를 `0`에서 `1`로 바꿨지만, 기존 값 `None`을 Unity `EditorSettings` API로 복구하고 저장한 뒤 해시 일치를 확인했다.

PlayMode 테스트 실행은 기존 기록에서 비어 있던 `client/UserData`에 테스트용 `world-v1.json`과 Economy 파일 2개를 생성/갱신했다. 세 파일의 SHA-256 백업을 `D:\Lowfire-Phase4D-test-side-effects-20261115\UserData`에 만들고, 작업 전 부재 상태로 돌아가도록 프로젝트 내 테스트 생성 파일과 빈 디렉터리만 제거했다. 별도 시각 QA는 임시 프로필·AutoSave 비활성 상태로 실행했고 QA 결과에서 월드 저장 미기록을 확인했다.

이번 변경은 Phase 4D 인벤토리 상세 레이아웃, 해당 QA/테스트, 증거 문서와 캡처에 한정한다. 저장/멀티플레이 계약과 게임플레이 로직, Phase 4E는 수정하지 않았다.
