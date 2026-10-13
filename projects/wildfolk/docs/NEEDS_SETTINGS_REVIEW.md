# 생존 수치·물·모닥불·설정 검수

기준일 2026-10-13, 시작 커밋 f2cd92ea3fa111e9db5e05f46c233c57db4011e7. 기존 이동/전투/채집/드랍/36칸/9칸 Hotbar/드래그/Workbench를 유지하고 Pause/설정/생존/물/모닥불 조리를 추가했다.

## 자동화

최종 hidden headless Unity 6000.3.19f1 복사본에서 EditMode **22/22**, PlayMode **22/22** 통과. 코드·데이터·저장 씬 기준이며 실제 Windows 마우스·키보드를 조작하지 않았다. 테스트의 PlayPointerDriver는 Unity 내부 가상 포인터 이벤트를 사용한다.

- EditMode: 스택/이동/교환/원자적 제작 회귀, 비용 보관 실패 무손실, 소진 시 간격 피해 계산/음식 Clamp, 로컬 PlayerPrefs 별도 키 저장·재로드, 실제 10아이템/6Recipe/3시설/소비품 참조·수치 및 씬 연결.
- PlayMode: 기존 이동/적/전투/자원 효율/드랍/재시작/보스 HUD/시설·드래그 흐름 유지. Pause→Settings 실제 UI 슬라이더/Reset/Back/Continue, AudioSource 채널 음량, Singleplayer 정지와 Multiplayer 메뉴의 입력 차단·적/허기 진행 및 로컬 사망 비동결, Water 획득/섭취/Max 무소비, 실제 Campfire 제작/Preview/배치/Use/Cook/시간/Take/섭취, 공간 부족 결과 보관, 시설 파괴 시 원재료/완성 결과 드랍, 거리 거부, 중립 멧돼지 고기 드랍.
- 추가 UI 회귀: 80%/140% 조작 요소 겹침 방지, 가방이 인벤토리 패널 바깥에 유지됨과 실제 가방 클릭 열기/닫기.
- 정적 소스 검사: Landscape/메타 **196개 고유 GUID**/스프라이트/개발자 문구 검사 통과. 최종 라이브 Editor 콘솔 오류 0건.

첫 PlayMode 21개 중 거리 사례 1개는 테스트 순간 이동 직후 Rigidbody 보간 반영을 기다리지 않아 실패했다. fixture 대기를 수정했고 21/21 통과했다. 이후 가방 패널 겹침 수정/레이아웃 회귀를 추가해 22/22 다시 통과했다. 이를 실제 손가락·실기 플레이 검증으로 표현하지 않는다.

## MCP Game View 시각 검수

Unity MCP execute_code의 ScreenCapture로 실제 Game View와 Overlay Canvas를 캡처하고 이미지를 직접 확인했다. 기존 Game View 해상도만 갱신했으며 Windows Computer Use/마우스/키보드, EditorWindow.Focus/GetWindow를 사용하지 않았다. 여러 단계 Editor 상태에서 is_focused=false를 확인했다. 해상도 변경 후 Canvas/레이아웃 갱신을 기다려 캡처했다.

| 화면 | 직접 확인한 증거 |
|---|---|
| 새 게임/HUD/물/중립 동물 | images/needs-start.png |
| Pause Continue/Settings | images/needs-pause.png |
| Settings 1920/2400/1280 | images/needs-settings-1920.png, needs-settings-2400.png, needs-settings-1280.png |
| 80% 조작·2400×1080 | images/needs-controls80-2400.png |
| 140% 조작·1280×720 | images/needs-controls140-1280.png |
| 수원/사용 아이콘 | images/needs-water.png |
| Campfire 월드 외형 | images/needs-campfire.png |
| Hand 레시피 | images/needs-handcraft.png |
| 조리 진행 1920/1280, 완료·Take 2400 | images/needs-cooking-1920.png, needs-cooking-1280.png, needs-cooking-2400.png |

최소 두 차례 이상 검수했다. 첫 설정 슬라이더 Fill/Handle의 Rect sizeDelta와 Unity Slider의 anchor 처리가 중복되어 막대가 패널 밖으로 커지는 문제를 바로 수정했다. 큰 조작 크기의 조이스틱·공격 버튼을 올려 Hotbar/가방 간격을 확보했다. Workbench에는 도구/검, Hand에는 Workbench/Campfire, Campfire에는 음식만 표시해 제작 목록 넘침을 예방했다. 넓은 화면에서 기존 가방 버튼이 Inventory 패널 테두리에 겹쳐 인벤토리 중에는 패널 바깥 좌하단으로 옮기고 다시 검수했다.

설치된 모닥불/Raw Meat는 시각 검수용 런타임 fixture로 추가했다. 조리 시간은 실제 StationCooking 시간으로 진행했고 캡처 시점을 예약했다. 자동 PlayMode는 실제 UI 제작/배치/조리/수령 흐름을 별도로 확인했다. fixture를 씬/초기 인벤토리에 저장하지 않고 Play 종료로 제거했다. 새 게임은 계속 빈 인벤토리이며 설치된 작업대·모닥불이 없다.

## 한계와 사용자 검증

실제 Android/iOS 빌드, 손가락·멀티터치, 노치, 발열/프레임, 앱 강제 종료/실기 재실행을 검증하지 않았다. PlayerPrefs 저장/새 객체 재로드를 자동 검사했지만 실제 폰 재실행 검증은 별개다. 음량 채널 값/슬라이더 연결을 검증했으며 청취 품질/단말 음량은 사용자 확인이 필요하다. Music 채널에는 아직 곡이 없고 성공 Use의 짧은 원본 SFX만 있다.

80%에서 작은 글씨/아이콘과 손가락 터치 여유, 140%에서 하단 플레이 공간 감소, 소비품 선택 시 시설보다 섭취가 우선인 흐름, 조리 후 Take 수령, 배치/전투 감각과 재미/세부 생존 속도는 실제 사용자가 판단해야 한다. 아트 변형이 적고 불/물은 정적인 그림이며 조리 입력 아이콘은 작다. 중립 멧돼지는 정지한 한 공급원이고 재생성/장기 식량 생태가 없어 무한 생존 콘텐츠가 완성된 것은 아니다.

새 적/보스 실체/바이옴/던전/절차 월드/고급 시설/방어구/직업/금속/포탑/실제 멀티플레이/월드 Save/용기·오염·정수는 추가하지 않았다. 현재 구현과 미래 방향을 문서에서 분리했다.
