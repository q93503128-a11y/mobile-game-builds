# Character / Armor / Locomotion 검증

2026-10-14. 기준 main `8e341b0f80eb82a87feef0d29a683692721c8f71`에서 이어서 구현했다. 실제 열린 Unity 프로젝트와 기준 client의 Assets/Packages/ProjectSettings 해시가 일치했고 작업 트리는 깨끗했다.

## 자동 기능 검사

새 EditMode 9개와 PlayMode 10개를 추가했다. Armor 장착/교체/해제/방어/최소 피해/생존 피해 우회, Level/잔여 XP/Point, Rank/효과/저장, Sprint Hold/Hysteresis/고갈/회복/생존 조건, Dash 방향/비용/물/벽/UI 차단, v1 변환/장비·Skill·Stamina 복원, Workbench Upgrade/기존 Recipe/ID/Chest32 저장, Category/Filter/정렬/NEW, Editor Profile/Release HUD 비노출을 검사한다. 입력은 Unity 내부 가상 포인터와 Input Source를 사용했다.

전체 회귀는 EditMode 52/52, PlayMode 60개 중57 통과/3 실패였다. 두 실패는 새 SurvivalHud 컴포넌트가 Canvas 전체에 연결되어 기존 Bounds/독립 Scale 검사와 충돌한 것이고, 실제 미터에 맞춘 별도 Transform으로 정리했다. 나머지는 새 고정 Tier 정렬 뒤에도 Chest를 목록 마지막에서 찾던 테스트 가정이었다. 실제 카드 위치로 스크롤하고 가상 포인터로 제작하는 검사로 수정했다. 실패3개와 변경 제작 UI를 포함한 대상5/5가 통과했다. 변경 데이터/Validator 대상2/2도 통과했다. 전체 Suite를 반복 실행하거나 단일 60/60 실행이라고 기록하지 않는다.

검사는 기존 Library 캐시가 있는 복사본 하나를 재사용하고 hidden headless Unity에서 실행했다. 원본 Editor/Windows 입력을 자동 플레이 도구로 사용하지 않았다. XML/로그는 `tests/artifacts/`의 ignored 로컬 증거다.

- `character-related-edit.xml`: 새 모델9/9.
- `character-related-play.xml`: 최초 새 Play 기능9/9.
- `character-category-final.xml`: 선택형 Category 입력1/1.
- `character-tools-new-recheck.xml`: Developer Profile/잠금 전 확인과 NEW2/2.
- `character-release-edit.xml`: 전체52/52.
- `character-release-play.xml`: 전체57/60, 실패와 수정 내역은 위 설명.
- `character-ui-final.xml`: 수정 후 대상5/5.
- `character-data-final.xml`: 변경 데이터 대상2/2.
- `character-modal-final.xml`: 마지막 HUD 아래 여백 변경 후 Control Size/Settings/제작 스크롤/Chest Transfer 대상4/4.

## MCP 시각 검사

실제 Game View를 1920×1080 /2400×1080 /1280×720에서 확인했다. Profile/HP/XP/미터/Point 알림, Stamina 표시, Attack/Dash/Interact 크기와 위치, Character 세 탭, Iron 장비 실루엣, Workbench I 업그레이드, II Recipe/NEW/Category, Chest32를 확인했다. 최초 가죽 장비/장비 탭/스킬 탭/제작 검수 이후 최종 세 해상도를 검사했으며 수정된 영역만 추가 검사했다.

수정한 부분: Category를 순환 버튼에서 선택 메뉴로 변경, 잠금 Recipe 확인이 미래 NEW를 없애지 않도록 처리, 업그레이드 줄과 첫 Recipe 사이 간격 확보, Profile와 시간 HUD 아래로 Inventory/Chest 여백 확보. 최종 캡처는 `images/character-<state>-<width>.png`의24개다. fixture는 Play Mode에서만 구성하고 종료 후 수동 시설/자원0과 깨끗한 저장 씬을 확인한다. Windows Mouse/Keyboard/Focus/GetWindow/Android 실기 조작은 하지 않았다.

| 화면 | 1920 | 2400 | 1280 |
|---|---|---|---|
| Profile /Stamina /Dash |[화면](images/character-hud-1920.png)|[화면](images/character-hud-2400.png)|[화면](images/character-hud-1280.png)|
| Information |[화면](images/character-information-1920.png)|[화면](images/character-information-2400.png)|[화면](images/character-information-1280.png)|
| Equipment |[화면](images/character-equipment-1920.png)|[화면](images/character-equipment-2400.png)|[화면](images/character-equipment-1280.png)|
| Skills |[화면](images/character-skills-1920.png)|[화면](images/character-skills-2400.png)|[화면](images/character-skills-1280.png)|
| Workbench II |[화면](images/character-workbench-ii-1920.png)|[화면](images/character-workbench-ii-2400.png)|[화면](images/character-workbench-ii-1280.png)|
| Category |[화면](images/character-categories-1920.png)|[화면](images/character-categories-2400.png)|[화면](images/character-categories-1280.png)|
| Upgrade |[화면](images/character-upgrade-1920.png)|[화면](images/character-upgrade-2400.png)|[화면](images/character-upgrade-1280.png)|
| Chest32 |[화면](images/character-chest-1920.png)|[화면](images/character-chest-2400.png)|[화면](images/character-chest-1280.png)|

## 사용자 검증과 제한

Armor 외형 만족도, 작은 Body/Hand Gear 차이의 강도, Profile/Character UX, Sprint 진입 체감, Dash 엄지 접근성, Stamina 회복·성장·재료/Workbench II 진행 속도는 사용자 테스트 대상이다. Armor 실루엣은 기본 얼굴을 유지하지만 가죽/철의 구분은 색과 외곽에 많이 의존하며 최종 아트가 아니다. 1280 제작 재료 설명과 Upgrade 비용은 작은 글씨이므로 실제 기기 가독성을 더 확인해야 한다.

확률형 질병/완성 Status Framework/Respec/전문 Bench/계정 Pet/Meta Coin/영구 Cosmetic/Main Menu/World Select/번식/고티어/Dungeon/New Biome를 추가하지 않았다. Android 장시간 성능·실제 손가락 입력·재미·출시 밸런스 합격을 주장하지 않는다. Raw Water/기존 사망 정책을 유지했다.
