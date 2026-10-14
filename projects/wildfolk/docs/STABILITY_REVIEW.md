# 기반 안정화 패스 검수

2026-10-14, 기준 `4bee7d46d13c4ebe84be58c1579376a01587b161`. 두 사용자 요구문 중 최신 Single Target/용기 물 규칙을 우선했다. 기존 Unity6000.3.19f1 LTS / Universal2D Landscape를 유지했다.

## 직접 확인한 화면

Unity MCP와 실제 Game View의 ScreenCapture를 사용했다. Windows 마우스/키보드/Computer Use와 Android 실기 조작은 수행하지 않았다. Editor 창을 자동 활성화하지 않았다. 검수용 상태는 DeveloperHarness와 같은 제품 프리팹/컨트롤러로 구성한 런타임 배치이며 저장된 제품 씬에 남기지 않는다.

- [일반 HUD](images/stability-start-1920.png): Developer 메뉴/테스트 버튼/내부 수치가 플레이 화면에 없음을 확인.
- [컵 상태](images/stability-cups-1920.png): Empty/Raw/Clean을 테두리·내용물·물방울 표시로 구분. 처음에는 빈 컵과 Raw가 비슷해 보여 물방울 표시를 추가했다.
- Split 수량 창: [1920](images/stability-split-1920.png), [2400](images/stability-split-2400.png), [1280](images/stability-split-1280.png). −1/+1/Half/Confirm/Cancel과 수량을 확인.
- 들고 있는 부분 Stack: [1920](images/stability-partial-1920.png), [1280](images/stability-partial-1280.png). Ghost를 원본 슬롯에서 옮겨 남은 수량과 선택 수량이 겹치지 않게 수정.
- Chest24칸과 컵 상태/선택 테두리: [1920](images/stability-chest-1920.png), [2400](images/stability-chest-2400.png), [1280](images/stability-chest-1280.png).
- 물 끓이기와 기존 고기 처리: [1920](images/stability-boiling-1920.png), [2400](images/stability-boiling-2400.png), [1280](images/stability-boiling-1280.png). 긴 물 재료 문구가 버튼에 가까워 보여 처리 재료 행 위치를 올렸다.
- [얕은 여울](images/stability-shallows-1920.png), [작은 관목](images/stability-small-plant-1920.png): 얕은 물 표현/구분, 비차단 자원 실루엣과 접촉 그림자를 확인. 관목 그림자를 크기에 맞게 줄였다.
- [Zombie 분리](images/stability-zombie-separation-1920.png): 같은 위치에서 시작한3개 개체가 완전히 한 점으로 포개지지 않음을 확인. 실전 다수 적·좁은 기지의 체감은 별도 검증 대상.
- [Combat Sandbox](images/stability-sandbox-1920.png): 현재 Tree/Stone/Ore/Target와 현재 장비 검증 환경. 제품 HUD에 개발자 결과 숫자를 표시하지 않는다.
- [Save/Load 메뉴](images/stability-save-menu-1920.png): Pause 메뉴의 실제 저장/불러오기 경로 확인.

Sandbox에서 Iron Sword로 실제1대상/36피해를 MCP로 확인했다. 작은 관목을 파괴하고90.1초의 게임 시간을 진행한 뒤 같은 ID의 자원이 화면 밖/Player12유닛 밖에서 재생하는 것도 확인했다. 화면 검수는 실제 사용자 터치/손맛/재미 검증을 대신하지 않는다.

## 자동 검사

| 검사 | 결과 |
|---|---|
| EditMode 최종 묶음 |43/43 통과|
| PlayMode 전체 회귀 묶음 |50개 중49통과,1실패|
| 실패한 전투 회귀 항목 수정 후 재검사 |1/1 통과|
| 새 기능 집중 검사 |13개 중12통과 후 저장 형식 수정, 저장 항목1/1 재검사 통과|
| 정적 소스/참조/노출 문구 검사 |통과,377개 고유 Asset GUID|
| Editor ProjectDataValidator |통과|

최종 PlayMode 실패는 상향 Facing 상태로 옆의 적을 때리는 이전360도 선택 규칙의 검사 설정이었다. 전방 Sector 규칙에 맞게 적을 바라보도록 검사 설정만 수정했고 다시 통과했다. 이후50개 전체를 반복 실행하지 않았다. 최종 Production Runtime은 전체 회귀 실행 코드와 같으며, 수정된 검사 및 그림자 표시를 포함한 복사본으로 마지막 항목을 검사했다. 단일 실행50/50이라고 보고하지 않는다.

자동 검사는 재사용한 별도 복사본의 숨겨진 batchmode Unity에서 실행했다. 가상 Input System 장치와 IPlayerInputSource를 사용했고 실제 OS 입력을 대신 조작하지 않았다. Runtime/검사 소스의 마지막 복사본 해시 차이0, 열린 프로젝트와 GitHub용 client의 소스 해시 차이0을 확인했다.

범위: 기존 이동/카메라/가방/드래그/제작/시설/도구/전투/Settings/Audio/낮밤 회귀, 현재 모든 기본 장비1대상/앞뒤 선택, 밀착/근거리/범위 끝, Friendly 필터와2대상 테스트 설정, 컵 채취/Raw 불이익/끓이기/반환, 실제 파일 Save→씬 Load/Player·시설6종·Chest·Processing·파괴 자원 복원, Timer 유지/자원·동물 재생, Long Press/부분 Stack/Cancel, Pool 재사용, Goblin 개인 공간/분리, 얕은 물 속도, 원거리 Zombie Slot 반환/새 지역 위협, checksum/backup/불완전 temporary/미지원 Version/잘못된 Inventory 거절이다.

저장 임시 파일 검증에서 Unity JSON의 빈 배열/빈 처리 상태 해석 문제를 발견해 유효한 빈 상태를 명확히 판별하도록 수정했다. 정상 backup이 손상 primary로 덮이지 않는 검사도 수행했다.

## 현재 한계와 직접 테스트

- Android Pause/Background/강제 종료·파일 교체·실제 터치/노치/발열/메모리/장시간 성능은 검증하지 않았다. Pool 구현을 성능 최적화 완료로 평가하지 않는다.
- Editor 동기 월드 생성과 전체 지형 텍스처를 유지한다. World Generation은1차이며 숲 군집/길/수변/탐험 목표가 단순하다.
- Navigation은 공통 지형·물·충돌·국소 우회와 방어시설 공격 기반이다. 복잡한 미로/기지 내부 판정/전체 경로 탐색은 완성되지 않았다.
- 자연 재생은 게임 시간 기준이며 오프라인 시간은 더하지 않는다. 지역 동물 Slot은 현재 닭/멧돼지 각1이다. 번식/새끼/성장/농축산은 없다.
- Save v1은 로컬 단일 플레이 Snapshot이다. 적 순간 AI/소분 포인터 상태/Account/Cloud/네트워크 권위를 저장하지 않는다. 미래 데이터·생성 알고리즘 호환성 변경에는 Version/Migration이 필요하다.
- 기본 장비는 MaxTargets1이다. MaxTargets를 늘린 Sector 선택은 검사했지만 실제 Greatsword/Spear/Pierce/AoE/Skill 콘텐츠는 없다. Boat/고급 용기/정수 시설/전체 Unlock Tree/Armor/전문 제작대도 없다.
- 실제 재미·Single Target 손맛·물 확보 귀찮음·갈증 압박·밤 Population/빛 안전 체감·좁은 공간 분리·재생 주기는 사용자 테스트로 판단한다.

다음 직접 플레이: 빈 컵 제작→채취→비상 Raw 소비→Campfire 정수→컵 재사용, Long Press→원하는 수량→Hotbar/Chest/Drop/Cancel, 저장 후 앱 재시작/Load, 기지 전투와 문/벽, 여러 지역으로 이동하며 밤 위협/조명/적 중첩을 확인한다. Death/Respawn/드랍·재화 패널티/Revive 정책은 새로 구현하지 않았다.
