# 2026-10-15 생태·UI·구조물 패스 검수

기준 main: 61e57abca91c0a9e02e2a85e4c635a952e7494dd. 신규 캐릭터는 기존 Pillow 코드 아트 방식이다. 생성형 이미지 시트2개는 사용자의 요청으로 폐기했고 Unity에 Import하지 않았다. 기존 Armor4개는 유지했다.

## 자동 검증

캐시된 tests/artifacts/unity 한 개를 재사용했다. 새 대화형 에디터·프로젝트 복제본을 계속 만들지 않았다. OS 마우스/키보드나 Computer Use를 사용하지 않았다. BatchMode/Nographics의 Unity Test Runner와 Input System Pointer 이벤트로 실제 UGUI 흐름을 검사했다. 원본 플레이어 저장파일을 테스트 데이터로 덮어쓰지 않았다.

| 실행 파일 | 전체 | 통과 | 실패 |
|---|---:|---:|---:|
| ecology-related-edit.xml | 6 | 6 | 0 |
| ecology-related-play.xml | 11 | 7 | 4 |
| ecology-related-recheck.xml | 4 | 4 | 0 |
| ecology-save-related.xml | 3 | 3 | 0 |
| ecology-final-edit.xml | 67 | 65 | 2 |
| ecology-final-play.xml | 79 | 66 | 13 |
| ecology-edit-recheck.xml | 3 | 2 | 1 |
| ecology-flows-recheck.xml | 15 | 14 | 1 |
| ecology-tool-data-recheck.xml | 1 | 1 | 0 |
| ecology-world-policy-recheck.xml | 1 | 1 | 0 |
| ecology-motion-recheck.xml | 3 | 3 | 0 |
| ecology-save-position-recheck.xml | 3 | 3 | 0 |
| ecology-spawn-safety-recheck.xml | 1 | 1 | 0 |

최종 전체 회귀는 EditMode67/PlayMode79 각각 한 번 실행했다. 최초 전체 결과는65/67,66/79였다. 실패 후 관련 항목만 재검사했으며 전체를 다시 돌리지 않았다. 마지막 결과를 테스트별로 합치면 현재146개가 모두 통과한 기록을 갖는다. ControlSizeLimits…BagClearOfInventory는 변경된 Modal 정책에 맞게 ControlSizeLimits…ModalCentered로 이름을 바꾼 같은 테스트다. 단일 전체 실행146/146 통과나 Android 실기 통과로 표현하지 않는다.

실제 수정한 실패는 Animal의 Player 장애물 판정, Modal 위 Held Cancel의 잘못된 레이어, Rusted Sword Tool의 효율 데이터 누락 등이다. 기존 UI 경로·인벤토리 닫기·즉시 문 Collider·기존 Item33개·구 Save Version3을 가정한 테스트는 새 계약에 맞게 수정했다. 밤 공격 정책 검사는 고정 짧은 대기 대신 피해 발생/3초 제한으로 검사했다. 기능을 우회하거나 테스트만을 위해 게임 규칙을 바꾸지 않았다.

마지막 코드 검토에서 무리 중심에만 적용되던 BaseDistance 검사를 각 생성 개체에도 공유했다. 실제 벽을 배치한 지역 복구 테스트에서 새 동물 위치와 기지 거리를 검사한1건도 통과했다. 이 변경은 아트/UI/모션을 바꾸지 않아 화면을 다시 캡처하지 않았다.

추가 확인: 4종 도구×7개 접촉/Arc/거리 조건28가지, Armor Drag/Swap/해제/공간 부족/선택 Use, 실제 버튼 Split Cancel, 생태 인구 재충원/새 ID, 범위 제한 경보, 무리 구성 변화, 새총 단계/실제 투사체 피해/제한된 재배치, 스켈레톤 변형/Light Trait 독립성, 열린 문/Armor/동물/적 Save v1~v4 이관을 포함한다. ProjectDataValidator 오류0. Python 소스 검사에서 Landscape/Unity 버전/528개 고유GUID/Sprite 참조/노출 문구를 통과했다.

## 실제 Game View 검수

1920×1080,2400×1080,1280×720에서19가지 상태57장을 ScreenCapture로 만들었다. 카메라 전용 렌더나 그림 합성이 아니다. 문/새총 중간 상태는 실제 Update로 진행한 뒤 해당 컴포넌트를 정지해 캡처했다. 이후 새총 손과 탄환 위치 정렬 관련4상태12장, Ready/Recovery easing 관련2상태6장만 추가 검사했다. 아래 모음은 각 상태의 최신 캡처를 축소해서 배치한 것이다.

| 해상도 | UI | 환경·전투 |
|---|---|---|
|1920×1080|[UI](images/ecology-1920x1080-ui.png)|[월드](images/ecology-1920x1080-world.png)|
|2400×1080|[UI](images/ecology-2400x1080-ui.png)|[월드](images/ecology-2400x1080-world.png)|
|1280×720|[UI](images/ecology-1280x720-ui.png)|[월드](images/ecology-1280x720-world.png)|

직접 확인한 결과와 수정:

- Information/Tree/Inventory가 SafeArea 중앙에 위치하고 Gameplay HUD가 어두운 배경 아래에 남는다. 뒤 Bag/Attack/Interact의 입력은 CanvasGroup과 Modal 레이어가 차단한다.
- Armor Drag 대상 강조, Leather/Iron4종의 착용/아이콘 일관성, 한국어 글꼴과 취소 버튼 노출을 확인했다.
- 갈색 원2개는 Combat Sandbox Target의 player_body SpriteRenderer였다. 두 개 모두 월드 루트, 갈색 tint, sorting0이었다. [원인 재현](images/ecology-artifact-baseline.png)을 남기고 생성 루프를 제거했다. Player의 분리된 Armor overlay 문제는 아니었다.
- Wood/Stone의 두께·재질·연결과 [문 중간 Hinge 회전](images/ecology-1920x1080-door-opening.png), 고정 Frame/열린 통로를 확인했다.
- 새총 Draw의 당기는 손과 Pouch 위치 어긋남을 실제 캡처에서 발견해 같은 Frame/손 좌표로 계산하도록 수정했다. Ready/Recovery 손 이동도 easing했다. [Draw](images/ecology-1920x1080-slingshot-draw.png) / [Release](images/ecology-1920x1080-slingshot-release.png).
- 움직이는 Goblin 무리 캡처에서 겹침이 강해 Separation .53→.85, PreferredSpacing2.7로 조정했다. 전투 Collider 크기는 유지했다. 변경 후 [실제 이동·경보 화면](images/ecology-1920x1080-groups-alerted.png)에서 사슴3/3 도주, Goblin5/5 전투, 당시 최소 Goblin 간격1.096을 기록했다. 이 한 장의 값은 모든 상황의 최소 간격 보장이 아니다. 무리 구성 자동 검사는 원거리0~2 변화도 확인했다.
- [5종 동물](images/ecology-1920x1080-animals.png), [캠프](images/ecology-1920x1080-goblin-camp.png), [검 스켈레톤](images/ecology-1920x1080-skeleton-sword.png)의 실제 Sprite/그림자/장비를 확인했다.

촬영 후 Play Mode를 종료했다. 기본1920×1080/영어, 씬 dirty=false, AutoSave=true, Player Motor/Input=true로 복구했다. Editor 상태의 is_focused=false를 확인했으며 창 앞으로 가져오기/Windows 입력을 사용하지 않았다. 촬영 C# 콜백과 테스트 산출물은 Assets나 Release에 넣지 않았다.

## 한계와 사용자 확인

1280×720에서 새총 Band/작은 Pebble, 스켈레톤 무기 디테일은 약하게 읽힌다. 손 동작과 Hinge가 정상 진행한다는 검증과 사람이 자연스럽다고 느끼는 평가는 구분한다. 캠프는 텐트·불·상자와 상주3명으로 만든 첫 규모이며 마을/정찰 루틴까지 확장하지 않았다. 지역 인구46/HardCap80, 무리 간격, 고블린 압박/새총 속도/빛 민감도/드랍 확률은 기능 검증용 초기값으로 재미·밸런스 확정이 아니다.

사용자가 직접 확인할 항목: 터치 Drag/Use와 Modal 중앙 느낌, 문 열림/닫힘 체감, 밀착 공격 체감, 동물 밀도/무리 자연스러움, Goblin 협공 난도, 새총 발사 가독성/회피, 스켈레톤 난도, Android 성능/배터리/터치. 이번에는 Android APK 성능 실측이나 장시간 밸런스 플레이를 하지 않았다.
