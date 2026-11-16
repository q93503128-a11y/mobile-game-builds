# Lowfire 실제 플레이 시각 품질 회귀 조사

- 조사 일시: 2026-11-16 (KST)
- 정본: `D:\Lowfire-Workspace\mobile-game-builds\projects\wildfolk\client`
- Unity: `6000.3.19f1`
- MCP 인스턴스: `client@33b387e077436073`
- 판정: **PARTIAL** — 인벤토리 캐릭터 누락은 원인을 확인해 수정하고 재현 검증했다. 흐림은 원래 해상도 PNG에서 재현되지 않았고, 844×390 화면의 작은 글자와 승인 시안과의 디자인 차이는 남아 있어 전체 시각 품질을 PASS로 처리하지 않는다.

## 조사 및 보호 기준

작업 시작 시 `main`과 `origin/main`은 `eed3aa61d34025643f2a2f64fd6e89c1fc8ab3f4`였다. 시작 시점 보호 목록은 632개 파일, 181,641,038바이트였다. 변경·미추적 로컬 파일, 88개의 수정된 `.meta`, `DefaultNetworkPrefabs.asset`, `EditorSettings.asset`을 포함해 해시를 다시 확인했으며 불일치는 0개다. 이 자료는 커밋하지 않았다.

Unity MCP가 가리킨 프로젝트 경로는 정본과 일치하고 Unity 버전도 `6000.3.19f1`이었다. 한 개의 Unity 인스턴스가 연결되어 있었다. 작업 종료 시 PlayMode를 끝내고 QA 프로필을 비웠으며 `SceneFlow.Repository`를 정본 `UserData\world-v1.json`으로 되돌렸다. Game View는 `Full HD (1920x1080)` 및 조사 시작 시 배율 `0.48`로 복구했다. 이번 조사에서 추가한 844×390·1280×720 프리셋만 제거했고 기존 `1080x1920`, `Mobile (19:9 Aspect)` 프리셋은 남겼다.

## 원인 및 수정

### 인벤토리에서 캐릭터가 사라진 원인

실제 기본 월드의 `PlayerAvatar`, `PlayerEquipment`, `SpriteRenderer` 소스 연결은 존재했다. `PlayerPortrait`도 월드 외형 동기화와 `Image.enabled` 상태를 맞추고 있었지만, 프리팹의 `Skin`, 손, 눈 이미지 오브젝트가 비활성 계층에 있었다. `Image.enabled = true`만으로는 해당 오브젝트가 Canvas 렌더링 계층에 들어오지 않아 Game View에 아무것도 그려지지 않았다.

`PlayerPortrait.FindTarget`이 재사용하는 비활성 placeholder GameObject를 활성화한 뒤 기존 이미지에 SpriteRenderer 외형을 연결하도록 고쳤다. 새 아트나 월드 캐릭터 오브젝트 복제는 추가하지 않았다. 실제 정상 새 월드의 844×390 캡처에서 5개의 기존 얼굴·손 레이어가 Canvas depth에 등록되고 캐릭터 표시가 나타남을 확인했다. 월드 캐릭터의 탑다운 아트와 실제 장비 로직은 그대로다.

이전 QA 검사기는 스프라이트와 컴포넌트 enabled만 확인하고 GameObject 활성 계층과 Canvas 등록 여부를 놓쳤다. 검사기를 활성 계층, `isActiveAndEnabled`, `Graphic.depth >= 0`도 검사하도록 강화했다. 기존 PlayMode 회귀 테스트에도 같은 렌더 계층 검증을 추가했다.

### 작아 보이거나 흐린 화면

조사한 Editor Game View는 `Full HD (1920x1080)`, 줌 배율 `0.48`이었다. `Mobile (19:9 Aspect)`는 목록에 있었지만 선택된 상태가 아니었고, 1.3 배율도 측정되지 않았다. Game View 줌은 Editor 패널 안에서 화면을 작게 보이게 하지만 `ScreenCapture.CaptureScreenshot`의 출력 해상도를 줄이지 않는다.

CanvasScaler 기준 해상도는 1920×1080, `MatchWidthOrHeight=0.5`였다. 런타임 scale factor는 844×390에서 `0.3984199`, 1280×720에서 `0.6666667`이었다. 캡처 PNG의 실제 크기는 요청한 두 해상도와 일치했다. UI 카메라는 RenderTexture를 사용하지 않았고 URP render scale은 1이었다. 검사한 UI 텍스처는 Bilinear 필터링이며 mipmap을 사용하지 않았다. 기본 Quality tier는 0 `Very Low`(AA 0)였고, 비교를 위해 잠시 높인 tier 5도 UI 글자 선명도를 유의미하게 바꾸지 않았다. Quality 설정은 원래 tier 0으로 복구했다.

844×390 원본 픽셀에서 글자는 작지만 가장자리 흐림이나 RenderTexture 저하는 재현되지 않았다. 따라서 사용자가 본 흐림의 원인을 특정해 해결했다고 주장하지 않는다. Editor 축소 표시와 작은 실제 모바일 글자 크기는 구별되는 현상이다. 전역 UI 확대는 슬롯·버튼 충돌 위험 때문에 적용하지 않았다.

### 인벤토리와 제작·출석 화면

정상 흐름 캡처는 격리 저장 경로에서 실제 MainMenu 출석 버튼, `Start new world`, 게임 HUD 인벤토리 버튼, 인벤토리 제작 버튼을 통해 열었다. 새 월드 인벤토리는 아이템 주입 없는 빈 상태이며, 별도의 장비·아이템 QA fixture 캡처와 구분했다. 출석 화면에서는 계정이 오프라인이고 `Online=False`였으며 출석 보상 수령 동작은 호출하지 않았다. 영문 제작 캡처 중 이전 두 파일은 실제 UI가 아니라 장시간 플레이 후 사망 오버레이였음이 확인되어 사용하지 않았다. 영문 캡처는 새 정상 플레이 흐름에서 다시 만들었다.

844×390에서는 인벤토리의 기존 탑다운 캐릭터가 보이지만 시각적 크기는 작다. 이는 월드 아트가 실제로 사용하는 얼굴·손 중심 탑다운 스프라이트라 전신 캐릭터 시안과는 외형이 다르다. 이번 요청의 기존 월드 캐릭터 아트 재사용 제한을 지켰으며 시안과 동일한 전신으로 바꾸지 않았다. 실제 첫 월드는 빈 인벤토리여서 아이템이 채워진 QA fixture 화면과도 다르게 보인다. 승인 시안과의 디자인 격차는 남아 있다.

제작 화면의 844×390 캡처에서는 목록의 아래 항목이 ScrollRect 뷰포트 아래로 이어진다. 스크롤 방식으로 접근 가능하며 버튼과 다른 화면 요소의 충돌은 확인되지 않았다. 출석 안내 문구는 844×390에서 작고 아래쪽 여백이 좁지만 캡처에서 문구가 빠진 것은 확인하지 못했다. 이번에는 제작·출석 레이아웃을 재디자인하지 않았다.

## 캡처 및 육안 검수

총 19개 Game View PNG를 정본의 `captures/`에 보관했다. 모두 원본 픽셀을 직접 열어 검토했고 PNG IHDR 치수를 검사했다. 캡처 묶음은 3,228,787바이트이며 844×390 또는 1280×720이다.

- 전후 비교: [인벤토리 한국어 844×390 이전](captures/inventory-ko-844x390-before.png), [이후](captures/inventory-ko-844x390-after.png), [1280×720 이전](captures/inventory-ko-1280x720-before.png), [이후](captures/inventory-ko-1280x720-after.png)
- 실제 새 월드 영어: [월드](captures/world-en-844x390.png), [인벤토리 844×390](captures/inventory-en-844x390-after.png), [인벤토리 1280×720](captures/inventory-en-1280x720-after.png)
- 실제 제작: [한국어 844×390](captures/crafting-ko-844x390.png), [영어 844×390](captures/crafting-en-844x390.png), [한국어 1280×720](captures/crafting-ko-1280x720.png), [영어 1280×720](captures/crafting-en-1280x720.png)
- 실제 출석: [한국어 844×390](captures/attendance-ko-844x390.png), [영어 844×390](captures/attendance-en-844x390.png), [한국어 1280×720](captures/attendance-ko-1280x720.png), [영어 1280×720](captures/attendance-en-1280x720.png)
- 렌더 설정 비교: [기본 tier 0](captures/inventory-en-1280x720-after.png), [임시 tier 5](captures/inventory-en-1280x720-quality5.png)

화면별 검토 결과를 요약하면, 아이템·버튼의 겹침은 확인하지 못했고 실제 캐릭터 누락은 수정 후 보이지 않는다. 844×390에서는 텍스트가 작고, 인벤토리 외형은 승인 전신 시안과 다르며, 제작 목록 일부는 스크롤 아래에 있다. 이 잔여 차이는 전체 품질 승인으로 처리하지 않았다.

## 테스트 및 콘솔

- 관련 PlayMode: `Wildfolk.Tests.CharacterPassPlayTests.InventoryCharacterPreviewTracksRealEquipmentWithoutChangingGameplayState` — **1 passed, 0 failed**. 장비 착용·교체·해제·장신구 상태 및 저장 상태 보존 검증을 포함한다.
- 변경 스크립트 컴파일: 완료, 신규 컴파일 오류 없음.
- Console: 기존 `Google.IOSResolver.dll`이 `UnityEditor.iOS.Extensions.Xcode` 의존성 오류로 로드되지 않는 항목이 남아 있다. 이번 변경과 무관한 프로젝트 환경 문제다. 전체 테스트, Android 기기 테스트, Android 빌드는 수행하지 않았다.
- 정상 플레이 캡처는 실제 UI 이벤트로 열었고 저장소는 `C:\Users\skanya\AppData\Local\Temp\Lowfire-VisualRegression-QA-20261116-02\world-v1.json`이었다. 저장 데이터가 프로젝트 `UserData`로 쓰이지 않도록 격리했다. 기본 `UserData\world-v1.json` 및 `.backup`의 마지막 수정 시각은 조사 시작 전으로 유지됐다. 보호 목록에는 이 ignored 파일의 시작 SHA가 없으므로 시작-종료 바이트 비교는 **UNVERIFIED**다.
- .meta GUID 연결의 프로젝트 전체 재임포트/검증은 수행하지 않았다. 이번 수정 대상 프리뷰의 실제 Sprite 연결과 Canvas 렌더 등록은 확인했다. 전체 AssetDatabase GUID 상태는 **UNVERIFIED**다.

## 변경 파일과 잔여 항목

- `client/Assets/Wildfolk/Runtime/UI/PlayerPortrait.cs` — 기존 preview placeholder 활성화
- `client/Assets/Wildfolk/Editor/InventoryPhase4DVisualQa.cs` — 실제 Canvas 렌더 계층을 확인하도록 QA 보강
- `client/Assets/Wildfolk/Tests/PlayMode/CharacterPassPlayTests.cs` — 렌더 등록 회귀 assertion
- 이 보고서와 원본 Unity Game View 캡처 19개

미해결: 사용자가 본 흐림은 별도 실기기/사용자 표시 환경에서 재현하지 못함, 844×390 UI 글자 크기는 여전히 작음, 기존 월드 탑다운 스프라이트는 승인 전신 시안과 다름, 전체 `.meta` GUID 연결과 Android 기기 렌더링은 UNVERIFIED. 이 결과로 다른 UI를 승인하거나 Phase 4E를 시작하지 않는다.
