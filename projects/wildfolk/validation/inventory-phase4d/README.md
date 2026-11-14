# Lowfire Phase 4D — 인벤토리 UI 상용 품질 검수

**상태:** 구현 및 Unity Editor 검증 완료. 최종 디자인 승인은 사용자 캡처 검토 대기이며, Android 실기기 터치 검증은 미수행이다.

## 기준과 보호

- 저장소: q93503128-a11y/mobile-game-builds, main.
- 보호 스냅샷 시각: 2026-11-14 13:32:39 +09:00, 당시 2b61bad8179934572e9323c931b6f78f546fcf9c.
- main은 이후 2026-11-14 13:35:06 +09:00에 origin/main의 f0b709c1d5e41003a01e0b4e195c06305cde1d9d로 fast-forward되었다. 비교 결과 이 구간의 커밋은 루트 README와 projects/forward-rpg 문서만 바꾸며 Wildfolk 파일은 변경하지 않았다. Phase 4D는 이 최신 main에서 작업했다.
- 공통 개발 규칙: q93503128-a11y/dev-playbook 최신 main 6c6f3e0fbb9e223fb2030248fa8db1c9c674a9e4; 전역 및 개발 규칙을 읽었다.
- 프로젝트 자료: projects/wildfolk/AGENTS.md, README.md, docs/INVENTORY.md, Unity CLI 환경 검증, Phase 4B/4C 검수 및 캡처를 대조했다.
- 스냅샷의 기존 변경 100개(기존 미커밋 .meta 88개, DefaultNetworkPrefabs.asset 1개, 기존 미추적 파일 11개)는 종료 시점 SHA-256 전부 일치했다(불일치 0). 삭제·초기화·커밋하지 않았다.
- DefaultNetworkPrefabs.asset은 변경하지 않았다. 69개 고유 GUID 참조를 조사했다. 68개는 Assets의 .meta에서, Netcode NetworkPrefabsList.cs 스크립트 GUID 1개는 설치된 Library/PackageCache/com.unity.netcode.gameobjects@277a288a1cea의 .meta에서 확인되어 미해결 0개다. 이 파일의 기존 변경 자체는 보호된 채 남아 있다.
- 이전 경로 C:/UnityProjects/Wildfolk는 존재하지 않았다. 별도 로컬 자료를 프로젝트에 병합하지 않았다.

## 구현

Phase 4C의 데이터·조작 구조를 유지하고 선택 아이템 정보와 장비 교체 상태를 재배치했다.

- 선택 이름과 카테고리 아래 설명 영역을 넓혀 844×390에서도 고정 글자 크기로 내용을 읽을 수 있게 했다. 긴 영어 장비명 및 설명은 줄바꿈으로 표시한다.
- Head / Body / Utility / Accessory 2×2 장비 배치와 기존 슬롯 행동을 유지했다. 아이콘·프레임·상태 텍스트와 터치 타깃 간 간격을 늘리고 선택 강조는 아이콘을 덮지 않는다.
- 교체 후보는 현재 장착 아이템 이름, Equip/Replace 문구, 방어력 이전→이후 값과 상승·하락 화살표를 보여준다. 비호환 항목은 빨간 설명으로 표시하고 장착 동작을 제공하지 않는다. 장착 중인 항목은 Unequip, 빈 슬롯은 슬롯 안에 빈 상태를 표시한다.
- 장착/교체 버튼을 기본 동작으로, 분할 버튼을 보조 동작으로 구분했다. 드래그 상태와 장비 슬롯 선택을 회귀 검사했다.
- 통합 QA에서 이미 Inventory 상태인 씬에 새 InventoryDragController가 활성화되면 빈 선택 상태의 분할 버튼 초기화가 누락되는 경우를 발견했다. Start에서 UpdateSplit을 계산하도록 수정했고 전용 PlayMode 회귀 검사와 시각 QA가 통과했다.
- Storage 27 + Hotbar 9, 드래그 이동·합치기·분할, 장비 장착·교체·해제, Save/멀티플레이 모델과 기타 화면은 바꾸지 않았다. Wildfolk.unity 및 Multiplayer.unity의 인벤토리 UI 레이아웃만 함께 갱신했다.

코드·씬·검사 파일은 변경 파일 목록(TESTS.md#변경-범위)을 참고한다. 저장소 규칙은 docs/INVENTORY.md의 Phase 4D 항목에도 반영했다.

## 실제 Unity 시각 검증

- Unity 6000.3.19f1, MCP instance client@33b387e077436073가 정본 D:/Lowfire-Workspace/mobile-game-builds/projects/wildfolk/client에 연결됐다.
- 실제 Play Mode Game View에서 ScreenCapture.CaptureScreenshot으로 16개 PNG를 만들었다. 844×390 한국어·영어, 1280×720 한국어·영어, 1920×1080 한국어·영어와 장비/분할 상호작용 장면을 포함한다.
- PNG 헤더로 파일 크기를 확인했다. 844×390 12개, 1280×720 2개, 1920×1080 2개이며 크기 오류는 0개다. 이미지 합계는 3,231,249 bytes.
- 자동 검사: qa-live.tsv의 3,088개 기록에 FAIL 0개, 텍스트 경계 실패 0개. 자동 경계 통과와 사람의 시각 검토는 별도로 판정했다.
- 원본 픽셀 크기로 텍스트 가독성, 아이콘·문자 겹침, 슬롯/버튼 간격, 대비와 상태 위계를 확인했다. Phase 4C의 교체 후보 및 844×390 레이아웃을 같은 후보 상태의 Phase 4D 캡처와 비교했다. 기본 화면 캡처는 선택 상태가 달라 그 차이를 별도로 감안했다. 검토한 화면에서 잘림·겹침·화면 이탈은 보이지 않았다.
- 캡처 인덱스(CAPTURE_INDEX.md)와 Phase 4B 대비 Phase 4C/4D 화면을 참조한다. 최종 캡처는 captures/phase4d-final-20261114-recheck/에 있다.

## 테스트 및 Editor 상태

- EditMode: 2/2 통과. 두 게임플레이 씬의 장비 타깃·레이아웃 검사 (job 68d0e0e006b3499ea73865e4a62819e3).
- PlayMode: 6/6 통과 (job d1cdfdaf2566406891cb31515aacdf03). 빈 선택 초기 분할 비활성화, 교체 정보/선택 초기화, 포인터 드래그·교체·해제, 모달 입력 차단, 롱프레스 분할, 터치 분할/드랍.
- 자세한 메서드명, 작업 ID, 실행 한계는 TESTS.md.
- QA 프로필은 고유 임시 경로를 사용했고 AutoSave를 껐다. QA 완료 로그는 월드 저장이 기록되지 않았다고 확인한다. 테스트 후 Wildfolk.QA.Directory 재정의는 비우고 기본 저장소 경로로 돌렸다. UserData는 생성되지 않았다.
- Editor는 테스트 후 Edit Mode의 MainMenu.unity로 돌아왔고 비포커스 상태였다. Unity Focus/Show 및 Windows GUI 자동 조작은 사용하지 않았다.
- 현재 Console 조회의 유일한 Error 항목은 기존 Google Google.IOSResolver.dll 로드 오류다(설치된 Editor에 iOS Xcode 확장이 없음). Phase 4D C# 컴파일 오류는 확인되지 않았고 집중 테스트는 통과했다. 이 환경 오류는 이번 범위에서 수정하지 않았다.
- 설치 경로는 D:/Unity/Hub/Editor/6000.3.19f1/Editor/Unity.exe다. 2026-11-08 Unity CLI 환경 보고서는 CLI 1.0.0-beta.17, Pipeline 0.10.0-exp.1, Android Build Support/SDK·NDK/OpenJDK 설치를 기록한다. 이번에는 Android 빌드를 하지 않았다.
- 직접 Unity CLI 테스트 재실행은 이미 열린 Editor 인스턴스 때문에 두 번째 실행을 시작하지 못했다. 포커스 없는 Unity MCP 비동기 테스트 러너에서 요청한 집중 EditMode/PlayMode 검사를 완료했다. 전체 테스트와 Android 빌드는 하지 않았다.

## 한계와 승인

- Android 기기에서 density/dp·Safe Area·실제 터치 정확도·한 손 도달성은 UNVERIFIED다.
- 최종 디자인 승인은 PENDING이며 사용자가 실제 캡처를 본 뒤 결정한다.
- Phase 4D 외 UI 확장, Phase 4E, 전체 테스트, Android 빌드, 멀티플레이 변경은 수행하지 않았다.
- 중간 캡처 반복 폴더와 로그는 로컬에 보존하고 이번 커밋에서는 제외했다. 커밋에는 captures/phase4d-final-20261114-recheck/*.png의 최종 16장만 포함한다.

## 다시 읽을 자료

- 상용 게임 사례와 적용 판단: RESEARCH.md
- 실제 캡처 목록 및 비교: CAPTURE_INDEX.md
- 회귀 검사와 범위: TESTS.md
- 측정된 작업 시간: TIME_LOG.md