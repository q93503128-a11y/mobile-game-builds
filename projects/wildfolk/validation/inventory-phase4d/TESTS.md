# Phase 4D — 회귀 검사 결과

## Unity EditMode

**2/2 통과** — job 68d0e0e006b3499ea73865e4a62819e3 (succeeded, 비포커스 Editor).

- Wildfolk.Tests.InventoryEquipmentLayoutTests.EquipmentTargetsAndLabelsFitSharedInventoryLayout("Assets/Wildfolk/Scenes/Wildfolk.unity")
- Wildfolk.Tests.InventoryEquipmentLayoutTests.EquipmentTargetsAndLabelsFitSharedInventoryLayout("Assets/Wildfolk/Scenes/Multiplayer.unity")

레이아웃 타깃, 네 장비 아이콘/프레임 크기, 텍스트 높이, 상태/상세 영역 겹침 여부, 주 장착 동작과 분할 버튼의 위계를 검사한다.

## Unity PlayMode

**6/6 통과** — job d1cdfdaf2566406891cb31515aacdf03 (succeeded, 실패 0, 비포커스 Editor).

재검증 전 첫 실행은 신규 초기화 검사가 기대한 활성 버튼을 찾아 실패했다. InventoryDragController.Start에서 초기 Split 상태를 계산하도록 보완했으며, 다음 재검증의 전체 6개 검사가 통과했다.

- Wildfolk.Tests.EcologyPlayTests.InventorySplitButtonInitializesWhenInventoryWasAlreadyOpen
- Wildfolk.Tests.EcologyPlayTests.InventoryReplacementLabelsShowCurrentGearDirectionAndResetSelection
- Wildfolk.Tests.EcologyPlayTests.ActualPointerArmorDragSwapUnequipAndUse
- Wildfolk.Tests.GameplayPlayTests.InventoryButtonsOpenCloseAndModalBlocksGameplayInput
- Wildfolk.Tests.StabilityPlayTests.LongPressSplitHoldsChosenAmountUntilExplicitPlacementOrCancel
- Wildfolk.Tests.WorldPassPlayTests.SplitHalfViaTouchButtonAndExplicitDropZonePreserveItems

검증 범위는 장비 교체·해제·드래그, 선택 상태 초기화, 모달 입력 차단, 롱프레스/터치 소분과 명시적 배치/드롭이다. 불필요한 전체 테스트 카탈로그는 실행하지 않았다.

## Unity Play Mode 시각 QA

- 격리된 유일 임시 QA 프로필, AutoSave 비활성, 월드 저장 쓰기 없음.
- 최종 로그 기록 3,088개, FAIL 0개, 자동 텍스트 경계 실패 0개.
- 16개 Game View PNG의 IHDR 크기 검증 오류 0개.
- 실제 이미지는 원본 픽셀 크기로 별도 검토했다. 자동 경계 통과만으로 시각 품질을 대신하지 않았다.
- QA 후 Play Mode를 멈추고 Wildfolk.QA.Directory override를 비우며 SaveRepository.Default()를 복원했다. 프로젝트 UserData는 생성되지 않았다.

## 콘솔 / 미수행

Console의 기존 Error 1개는 Google.IOSResolver.dll 로드 실패다. Unity 6000.3.19f1 설치에 iOS Xcode 확장이 없다. Phase 4D 스크립트 컴파일 이후 새 C# 컴파일 오류는 없고, 위 테스트는 통과했다.

Android 실기기·Safe Area·dp/터치 검증, Android 빌드, 전체 회귀, 멀티플레이 2기기 검증은 이 범위에서 실행하지 않았다. 저장/멀티플레이 코드 변경은 없다.

## 변경 범위

- client/Assets/Wildfolk/Runtime/UI/InventoryEquipment.cs
- client/Assets/Wildfolk/Editor/InventoryUiBuilder.cs
- client/Assets/Wildfolk/Resources/Localization.json
- client/Assets/Wildfolk/Scenes/Wildfolk.unity
- client/Assets/Wildfolk/Scenes/Multiplayer.unity
- client/Assets/Wildfolk/Tests/EditMode/InventoryEquipmentLayoutTests.cs
- client/Assets/Wildfolk/Tests/PlayMode/EcologyPlayTests.cs
- client/Assets/Wildfolk/Editor/InventoryPhase4DVisualQa.cs 및 .meta (Editor 전용 검증 도구)
- projects/wildfolk/docs/INVENTORY.md
- projects/wildfolk/validation/inventory-phase4d/의 보고서와 최종 캡처 16장

다른 UI와 저장/네트워크/게임플레이 시스템은 수정하지 않았다.

## 2026-11-14 최종 시각 교정 회귀

**EditMode 2/2 통과** — job `38bb9632e1b240e4b3552ac1d8cca5f4`.

- `Wildfolk.Tests.InventoryEquipmentLayoutTests.EquipmentTargetsAndLabelsFitSharedInventoryLayout("Assets/Wildfolk/Scenes/Wildfolk.unity")`
- `Wildfolk.Tests.InventoryEquipmentLayoutTests.EquipmentTargetsAndLabelsFitSharedInventoryLayout("Assets/Wildfolk/Scenes/Multiplayer.unity")`

카테고리 텍스트 영역과 프레임·선택 외곽선·상태 문구의 실제 RectTransform 간격, 2×2 타깃 유지와 장비 셀 배치가 두 씬에서 통과했다.

**PlayMode 5/5 통과** — job `6fe7c4e3686f4e76bbc00dc3a77a6b66`.

- `Wildfolk.Tests.EcologyPlayTests.InventoryReplacementLabelsShowCurrentGearDirectionAndResetSelection`
- `Wildfolk.Tests.EcologyPlayTests.ActualPointerArmorDragSwapUnequipAndUse`
- `Wildfolk.Tests.EcologyPlayTests.InventorySplitButtonInitializesWhenInventoryWasAlreadyOpen`
- `Wildfolk.Tests.StabilityPlayTests.LongPressSplitHoldsChosenAmountUntilExplicitPlacementOrCancel`
- `Wildfolk.Tests.WorldPassPlayTests.SplitHalfViaTouchButtonAndExplicitDropZonePreserveItems`

교체 직후 새 장착품 선택/역방향 안내 초기화, 기존 후보를 다시 선택하는 흐름, 포인터 장착·해제, 빈 선택에서 Split 비활성화, 터치/롱프레스 분할을 확인했다.

### 최종 Game View QA

- 정본 Editor에서 `ScreenCapture.CaptureScreenshot`으로 PNG 46장(3개 기본 해상도 KO/EN, 10개 상태의 844×390/1280×720 KO/EN)을 생성했다. 크기별 수량은 844×390 22장, 1280×720 22장, 1920×1080 2장, 총 10,564,171 bytes다.
- QA 시작 시 사용자 저장 경로 대신 고유 임시 프로필을 검증했고 AutoSave 비활성 및 월드 저장 미기록을 확인했다. 종료 후 프로필 오버라이드를 비우고 기본 SaveRepository로 복원했으며 `client/UserData`는 생성되지 않았다.
- 런타임 검사는 빈 선택에서 `SplitButton.interactable == false`, 30 Wood 선택에서 true, disabled tint의 명도 차이를 확인했다.
- 자동 텍스트 preferred-height 경고 2개는 모두 `Armor Details`(rect 86, preferred 124.5)에서 영어 긴 설명과 빈 장비 상태에서 기록됐다. 이 두 경고는 숨기거나 PASS로 바꾸지 않았다. 각 화면은 원본 PNG에서 확인했으며, 실제 이미지에 텍스트와 버튼/프레임의 중첩은 보이지 않았다. 자동 텍스트 높이 상태는 **PARTIAL**로 기록한다.
- 캡처 인덱스에는 844×390/1280×720의 10개 상태와 KO/EN 각각의 원본 PNG 링크가 있다. Android 기기 검증은 **UNVERIFIED**다.

이 변경은 지정된 인벤토리 파일, SP/MP 두 씬, QA 도구와 검증 자료에만 한정했다. Phase 4E와 다른 UI는 시작하거나 수정하지 않았다.
