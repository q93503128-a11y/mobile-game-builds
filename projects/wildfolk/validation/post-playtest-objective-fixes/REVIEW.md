# Post-Playtest Objective Bugfix — 2026-10-30

- 시작 SHA: `c21ed6affbc5353c7a5c606be19c93dcd049ca57`. 작업 시작 시 remote main fetch, local main 동기화·clean 확인.
- 종료 SHA의 버전관리 참조: 이 파일의 최초 추가 커밋. `git log --diff-filter=A -1 --format=%H -- projects/wildfolk/validation/post-playtest-objective-fixes/REVIEW.md`로 식별한다. 실제 local/remote 해시는 실행 최종 보고에 기록한다.
- Unity 6000.3.19f1 / `C:/UnityProjects/Wildfolk`. AGENTS/README 및 제작·물·저장 정본과 기존 실플레이 보고서를 읽었다. A/B/C/D만 수정했다.
- 구조화된 전후 결과·검사별 결과·원본 저장 fingerprint는 [evidence.json](evidence.json).

## 원인·수정·실제 확인

|항목|확인한 원인|최소 수정|실제 검증|
|---|---|---|---|
|A 시설 상호작용|선택된 물주머니의 Fill 분기가 이미 검색된 시설보다 먼저 실행됐다. 모닥불 앞 같은 위치에서 검/도끼 성공, 빈/채운 물주머니 실패를 재현|기존 거리·방향·최근접 후보 검색은 유지하고, 수원 또는 대상 없음일 때만 물주머니 분기를 사용한다. Interact 아이콘도 같은 조건을 사용한다|검·빈 물주머니·채운 물주머니·도끼 모두 시설 열기. 충전량/품질 보존. 시설과 물이 모두 전방인 fixture에서 가까운 시설은 열리고 가까운 수원은 채수됨. 검+빈 컵의 채수 및 가득 찬 정수 용기 보존도 확인|
|B Craftable only|Ready/Collect 상태만 통과시켜 진행 중 작업과 가방 부족 상태의 완료물을 숨겼다|현재 시설의 Batch 작업, Sequential 현재/예약/완료 출력을 필터 예외로 유지한다. Station/Category 조건과 일반 재료 부족 Recipe 제외는 유지|요리5초·물주머니 정수10초를 실제 기다려 진행 카드→Ready→Take 확인. 필터 ON/OFF·Category·재열기·full bag 미수령 및 순차 예약/출력을 PlayMode에서 검증|
|C 상세 선택|Station 일치만 확인하고 현재 표시 목록에 선택이 남는지 확인하지 않았다. 빈 Tools 목록에 Compass 상세가 남는 것을 재현|선택이 보이면 유지, 숨겨지면 첫 표시 항목 선택, 표시 항목이 없으면 선택·제목·아이콘·설명 해제|A→B→A, 필터 생존/제외, 빈 목록, 활성 작업 예외, 닫기/재열기. 실제 MCP에서도 Tools 선택/Weapons 복귀·재열기·빈 상세를 확인|
|D 물가 방향|육지의 WaterSource 프록시를 바라보는 조건 때문에 가까이 접근하면 실제 수면 쪽이 아니라 지나온 프록시 쪽에서 성공했다|수원에 한해 현재 수면 거리장의 가까운 경계 방향을 사용하고, 그 방향 안 실제 수면도 확인한다. 기존 dot≥0.35·접근 거리·수원 탐색 범위는 유지. Waterskin 보충 방식은 변경하지 않음|동일 Seed/3위치×컵·물주머니×4방향×3반복, 수정 전72회/최종72회. 재현 근거가 확인되어 수정함|

D의 대표 재현 위치는 `(-12.28,8.48)`이다. 컵으로 수면 쪽을 향하면 **0/3**, 반대 방향은 **3/3**이었다. 최종은 수면 쪽 **3/3**, 반대 **0/3**, ±30° 각도 **각3/3**이다. 더 먼 육지 위치의 기존 성공과 실패도 보존했다. 세 번째 위치는 실제 수면 안으로, 주변 물을 바라보는 각 방향의 채수가 정상 동작한다. 모든 방향을 무조건 허용하거나 범위를 늘리지 않았다. 물주머니의 36개 결과는 전후 동일하다.

직선 수면 검사 중 비스듬한 기존 채수 실패를 발견했고, 전방 수면 부채꼴 중간안은 시설 겹침 검사에서 실패했다. 최종은 가까운 수면 경계 방향에 기존 허용각을 적용하여 두 회귀를 함께 통과했다. 실패 로그는 작업 폴더에 보관하고 최종 결과만으로 완료를 판정했다.

## 관련 검사

- EditMode **36/36**: Inventory/InventoryOperations/FoodModel/Progression/QualityModel.
- PlayMode **고유10개 최종 통과**: 신규7 + 기존3. 최초 신규7·기존3 통과 후, 마지막 물가 수정에 영향받은 시설·겹침·방향3개만 재실행하여3/3 통과했다. 동일 검사 중복 실행을 고유 수에 더하지 않았다.
- 기존 회귀3: 컵 채수·Raw 불이익·정수·빈 컵 반환, Furnace UI 예약/반환/수령/Save codec 복원, Chest/Inventory/Hotbar drag·장비 갱신.
- 실제 `ProjectDataValidator.Validate()` 오류0. 소스/에셋 구조 validator PASS(1,865 unique GUID).
- 초기 새 테스트의 미import로 선택0개였던 실행은 통과 수에서 제외했다. 강제 import 및 테스트 타입/실제 실행 수를 확인했다.
- 최종 코드/data/scene/asset **1,089개**가 canonical과 live에서 줄바꿈·후행 공백 외 동일하며 추가 live 파일도 없다.
- 마지막 Console 조회에는 MCP WebSocket transport 메시지1개가 있었다. 게임 오류로 세지 않았다. 컴파일 중 확인한 기존 AdMob obsolete 등8개 경고는 범위 밖으로 유지했다.

자동 PlayMode 검사는 fixture 기반이며 자연 progression 플레이가 아니다. MCP 재검도 같은 Seed의 격리 저장 복사본에서 위치·가방·AI/시간을 통제했다. 실제 UI/interaction 상태 및 Game View를 확인했지만 이를 새 플레이 경험이나 모바일/LAN/출시 검증으로 주장하지 않는다. Windows 입력·Focus/Show는 사용하지 않았다.

## 저장·버전·종료

- QA는 저장소 밖 `objective-bugfix-20261030/qa`만 사용했다. 기존 실플레이 QA checkpoint도 복사하여 원본을 보존했다.
- 실제 UserData의 Save 원본/백업2개 **SHA256·mtime_ns·size 동일**, 원래 Profiles 파일의 신규 생성 없음. 별도로 읽은 Unity SDK telemetry와 TestResults.xml의 변경은 사용자 Save/Profile 변경과 구분해 evidence에 기록했다.
- Play OFF / 원래 MainMenu dirty=false / QA 설정 원래 빈 값 복원 / 일회성 MCP callback0.
- **Save14 / Profile2 / CloudProfile1 / Economy2 / Protocol23 / generation3 / IAP Schema1 유지.** persistence/wire schema와 Item/Recipe/Station 수치 변경 없음.
- 주관적 UX·밸런스·진행 속도/힌트·정렬·VFX/SFX/gamefeel·Home 명칭·수관/드랍 가시성은 수정하지 않았다. 큰 구조 변경·새 기능·Android/Alpha/release 작업 없음.

## 변경 파일

- Runtime: `Player/PlayerInteraction.cs`, `UI/InteractButtonView.cs`, `UI/CraftingPanel.cs`.
- 검사: `Tests/PlayMode/PostPlaytestInteractionTests.cs`와 `.meta`.
- 정본 최소 갱신: `docs/CRAFTING.md`, `docs/FOOD_AND_FISHING.md`.
- 검증: 이 `REVIEW.md`, `evidence.json`.

원본 스크린샷·전후72회 TSV·중간 실패·도구 기록은 저장소 밖 `C:/Users/User/Documents/ChatGPT/문승준 모바일/objective-bugfix-20261030/`에 보관한다. 최종 Git SHA와 단계별 완료 시간은 실행 최종 보고 및 해당 폴더의 completion 기록을 따른다. 이 네 항목 처리 후 STOP.
