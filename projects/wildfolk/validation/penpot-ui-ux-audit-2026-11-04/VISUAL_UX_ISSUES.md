# 시각·UX 문제와 우선순위

감사 재개 시 fetch한 main: `82d19b64de0157d924a05f87ea8ee698f5d6db6b`. 제품 source/data는 이전 `48d6528`과 동일하다. game 변경·Penpot 제작 없이 조사했다.

FACT/CONFIRMED는 현재 source/이번 audit의 실제 화면, SOURCE-INFERRED는 source로 도출한 동작·위험, HISTORICAL은 과거 캡처, PROPOSAL은 제안, FUTURE GOAL은 사용자 차기 목표, UNVERIFIED는 검증 범위 밖이다. 디자인 심각도는 감사 우선순위이며 모든 항목을 출시 차단 코드 버그로 해석하지 않는다.

**P0 확인0 / P1 7 / P2 15.** P1은 중요한 체감·가독성·navigation 문제, P2는 polish·정보설계·source 위험이다. crash/save 거래의 전면 무결성 인증은 아니다. 서버 OFF/미구현 future goal 자체를 임의 P0로 올리지 않았다.

| ID | 등급 | 화면 | 구체적 문제 | 증거 수준 |
|---|---|---|---|---|
| UX01 | P1 | E05/A03/A02 | Store에서 연 Help의 Back이 Account로 돌아간다. | CONFIRMED |
| UX02 | P1 | H03/E01/E02/E03/I01/C01/P02 | 모바일에서 panel 전체 축소에 의존하여 본문·수치가 너무 작아진다. | CONFIRMED |
| UX03 | P1 | E02/E03 | 출석의 표시 전용 칸들이 큰 버튼과 같은 표면을 사용하며 월간28칸이 균일하게 빽빽하다. | CONFIRMED |
| UX04 | P1 | G08 | 현장 수리/회수 카드가 큰 화면 폭을 차지하며 두 긴 행동과 재료 설명이 동시에 전투 시야를 점유한다. | HISTORICAL + SOURCE-INFERRED |
| UX05 | P1 | I01/I02/C01/C02/C03 | 가방·제작이 거대한 utility panel에 작은 설명/상태/비용을 함께 담아 핵심 작업보다 데이터 해독이 앞선다. | CONFIRMED |
| UX06 | P1 | H03/S02/G13 | 결과를 바꾸는 행동의 확인·비용 노출 계약이 일관되지 않다. Reset은 즉시, 왕묘 Interact는 조건 통과 시 바로 Bone10을 소비한다. | SOURCE-INFERRED |
| UX07 | P1 | A01/A02/E01/E03/P01/P02 | 거의 같은 대형 종이 sheet/직사각 action을 여러 과제에 반복해 계정 관리·기록·진행·보상의 시각적 성격이 희미하다. | CONFIRMED |
| UX08 | P2 | E01/E05/P02 | 비활성 action의 밝은 글자/옅은 배경이 읽기 어려우며 이유가 다른 영역에 놓인다. | CONFIRMED |
| UX09 | P2 | E01 | 영문 Progression/Exploration이 일부16:9 캡처에서 단어 중간 줄바꿈된다. | CONFIRMED |
| UX10 | P2 | E01/C01/I06/G08 | 일부 상세와 목록은 scroll이 필요하지만 mask와 바닥 절단만으로 그 사실을 알리기 어렵다. | CONFIRMED |
| UX11 | P2 | H03/S02 | Language가 다음 언어로 순환하는 일반 버튼이며 슬라이더와 Reset의 범위도 시각상 설명이 적다. | CONFIRMED |
| UX12 | P2 | G01/G10/G12 | HUD가 여러 가장자리에 작은 수치·class·time·상황문구로 분산되어 우선 관찰 대상이 약하다. | CONFIRMED |
| UX13 | P2 | P02/P05 | 작은 node icon·짧은 이름·lock/seal이 여러 branch에서 비슷하게 보인다. | CONFIRMED |
| UX14 | P2 | H01/G03/P01 | Lucide 선 아이콘, 자체 node 도형, item sprite, fist/portrait의 시각 언어가 혼재한다. | CONFIRMED |
| UX15 | P2 | H01/H02/M01 | Single Player는 관리 화면이 아니라 새 세계 시작이고 Host는 별도 고정 저장을 사용한다. | SOURCE-INFERRED / FUTURE GAP |
| UX16 | P2 | M01 | route 전환 때 이전 연결 오류 문구가 남아 반대 route의 안내와 섞일 수 있다. | SOURCE-INFERRED + audit capture |
| UX17 | P2 | I07/C05/C06/C07 | runtime supplies/포탑/navigation 패널은 최근 공용 journal/field 작성 경로와 별도 스타일 helper를 쓴다. | SOURCE-INFERRED |
| UX18 | P2 | G11 | 토스트가 단일 label을 짧게 교체하므로 여러 결과가 연속하면 중요한 실패/환급 정보가 묻힐 수 있다. | SOURCE-INFERRED |
| UX19 | P2 | E05/E06 | 구매 OFF인데 상품3개·부활환산·구매복구/이력/문의가 한 화면에서 공간을 계속 차지한다. | CONFIRMED |
| UX20 | P2 | T01 | Loading의 기본 Text Lowfire·단색막·arc가 Home wordmark/캠프 표현과 다르다. | HISTORICAL + SOURCE-INFERRED |
| UX21 | P2 | G09/G13/World | 월드 광석·생물 실루엣·수면 재질과 UI polish의 격차가 전체 품질 인상에 남는다. | HISTORICAL + current context |
| UX22 | P2 | D01/H01/C03 | Home이라는 말이 메인 야영지 메뉴, 무료 사망 복귀 지점, Workbench 나침반 목표에 걸쳐 쓰인다. | HISTORICAL + SOURCE-INFERRED |

## UX01 · P1

| 항목 | 내용 |
|---|---|
| 화면 | E05/A03/A02 |
| 문제 | Store에서 연 Help의 Back이 Account로 돌아간다. |
| 근거 | [store-help-back-ko-844](evidence/live/store-help-back-ko-844.png); [StoreScreen.Contact](../../client/Assets/Wildfolk/Runtime/UI/StoreScreen.cs#L22); [AccountProfileScreen.CloseHelp](../../client/Assets/Wildfolk/Runtime/UI/AccountProfileScreen.cs#L30) |
| 영향 | 구매 문의 후 상품/이력 맥락을 잃고 불필요한 단계가 생김 |
| 원인 | Store.Close의 fromHome만 존재; 하위 Help origin 없음 |
| 제안 | 동일 Help component에 caller/return target을 명시하고 Store/Account 경로를 구분 |
| 변경 경계 | navigation controller 변경 필요; support submit/계정 서비스는 보존 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX02 · P1

| 항목 | 내용 |
|---|---|
| 화면 | H03/E01/E02/E03/I01/C01/P02 |
| 문제 | 모바일에서 panel 전체 축소에 의존하여 본문·수치가 너무 작아진다. |
| 근거 | [settings-v2-en-844](evidence/live/settings-v2-en-844.png); [resume-v2-inventory-en-844](evidence/live/resume-v2-inventory-en-844.png); [ModalLayout.Apply](../../client/Assets/Wildfolk/Runtime/UI/ModalLayout.cs#L12) |
| 영향 | 비용·소유/필요·상태를 읽는 시간이 늘고 dense UI에서 오조작 위험 |
| 원인 | 1920 reference font와 fixed panel을 scale-to-fit; ControlSize는 메뉴 font를 키우지 않음 |
| 제안 | 844 기준 실제 읽기 크기부터 정하고 열/세부정보를 재배치; type 최소크기와 scroll을 함께 검증 |
| 변경 경계 | layout/type 교체; 비용/slot/recipe 축소 금지 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX03 · P1

| 항목 | 내용 |
|---|---|
| 화면 | E02/E03 |
| 문제 | 출석의 표시 전용 칸들이 큰 버튼과 같은 표면을 사용하며 월간28칸이 균일하게 빽빽하다. |
| 근거 | [attendance-monthly-v2-ko-844](evidence/live/attendance-monthly-v2-ko-844.png); [EconomyScreen.Refresh](../../client/Assets/Wildfolk/Runtime/UI/EconomyScreen.cs#L16) |
| 영향 | tap할 수 있는 보상·단순 stamp·오늘 수령 action의 구분이 약함 |
| 원인 | Button으로 작성한 타일을 runtime 비활성; 날짜·stamp·Gem 텍스트 반복 |
| 제안 | RewardTile과 actionable claim을 별개 component로 설계; 오늘/획득/미래는 icon+문구+대비로 구분 |
| 변경 경계 | 신규7/월간28·server UTC·하나의 claim action·실제 보상 유지 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX04 · P1

| 항목 | 내용 |
|---|---|
| 화면 | G08 |
| 문제 | 현장 수리/회수 카드가 큰 화면 폭을 차지하며 두 긴 행동과 재료 설명이 동시에 전투 시야를 점유한다. |
| 근거 | [gameplay--hud-repair-ko-844](../post-commercial-ui-qa-tools/evidence/gameplay--hud-repair-ko-844.png); [MaintenanceUiBuilder.Arrange](../../client/Assets/Wildfolk/Editor/MaintenanceUiBuilder.cs#L8) |
| 영향 | 근처 기지에 있을 때 이동·위험 관찰보다 운영 패널이 앞서 보임 |
| 원인 | 840×360 contextual root, title/cost/refund/actions/hint가 항상 함께 표시 |
| 제안 | 대상·상태·주 행동부터; 상세 비용/반환은 읽기 쉬운 확장영역. Hold 진행은 행동 옆에 유지 |
| 변경 경계 | 환급/비용/PlayerBuilt/contents/hold 중단·Host 검증은 그대로 |
| 신뢰/재현 | HISTORICAL + SOURCE-INFERRED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX05 · P1

| 항목 | 내용 |
|---|---|
| 화면 | I01/I02/C01/C02/C03 |
| 문제 | 가방·제작이 거대한 utility panel에 작은 설명/상태/비용을 함께 담아 핵심 작업보다 데이터 해독이 앞선다. |
| 근거 | [resume-v2-hand-crafting-ko-844](evidence/live/resume-v2-hand-crafting-ko-844.png); [resume-v2-inventory-ko-844](evidence/live/resume-v2-inventory-ko-844.png); [CraftingUiBuilder.Arrange](../../client/Assets/Wildfolk/Editor/CraftingUiBuilder.cs#L56) |
| 영향 | 새 플레이어가 재료 부족/출력 수령/장비 선택의 다음 행동을 빠르게 구분하기 어려움 |
| 원인 | fixed 다열 grid와 선택 상세, 다중 재료 string, 관련 action 위치 분산 |
| 제안 | dense pilot에서 선택-필요재료-명시 행동 순서 수립; 장비4/36slot과 station 구분 유지 |
| 변경 경계 | UI 재배치만; 자동제작·무한가방·queue 자동수령 추가 금지 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX06 · P1

| 항목 | 내용 |
|---|---|
| 화면 | H03/S02/G13 |
| 문제 | 결과를 바꾸는 행동의 확인·비용 노출 계약이 일관되지 않다. Reset은 즉시, 왕묘 Interact는 조건 통과 시 바로 Bone10을 소비한다. |
| 근거 | [UserPreferences.ResetDefaults](../../client/Assets/Wildfolk/Runtime/Core/UserPreferences.cs#L16); [RoyalTomb.Use](../../client/Assets/Wildfolk/Runtime/Bosses/RoyalTomb.cs#L15); [BossWorld.Summon](../../client/Assets/Wildfolk/Runtime/Bosses/BossWorld.cs#L43) |
| 영향 | Reset을 단순 navigation으로 오해하거나 왕묘에서 자원 소비를 미리 예측하기 어려울 수 있음 |
| 원인 | 일반 Action와 mutation action의 공통 affordance; 왕묘 전용 확인 화면 없음 |
| 제안 | Reset 범위와 즉시 반영을 명시. 왕묘는 현재 비용/조건을 action 전에 전달하는 UX 검토 |
| 변경 경계 | 새 confirmation/interaction 단계는 미래 동작 변경 승인 필요. 이번에는 추가 안 함 |
| 신뢰/재현 | SOURCE-INFERRED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX07 · P1

| 항목 | 내용 |
|---|---|
| 화면 | A01/A02/E01/E03/P01/P02 |
| 문제 | 거의 같은 대형 종이 sheet/직사각 action을 여러 과제에 반복해 계정 관리·기록·진행·보상의 시각적 성격이 희미하다. |
| 근거 | [profile-ko-844](evidence/live/profile-ko-844.png); [class-overview-ko-844](evidence/live/class-overview-ko-844.png); [account-ko-844](evidence/live/account-ko-844.png) |
| 영향 | 상용 품질의 과제별 위계가 약하고 빈 영역과 과밀 영역이 공존 |
| 원인 | JournalUiBuilder.Sheet(1720×1000), 동일 소재·rule·action vocabulary 반복 |
| 제안 | 책 테마를 기본값으로 고정하지 말고 핵심 task별 container를 고른다. 숲/잉크/불씨 정체성은 후보로 비교 |
| 변경 경계 | 최종 미술 스타일은 사용자 pilot 승인 전 확정 금지 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX08 · P2

| 항목 | 내용 |
|---|---|
| 화면 | E01/E05/P02 |
| 문제 | 비활성 action의 밝은 글자/옅은 배경이 읽기 어려우며 이유가 다른 영역에 놓인다. |
| 근거 | [achievements-v2-ko-844](evidence/live/achievements-v2-ko-844.png); [JournalStyle.Action](../../client/Assets/Wildfolk/Runtime/UI/JournalSkillNode.cs#L21) |
| 영향 | offline/point부족/이미선택/구매불가를 단순 회색 버튼으로 읽게 됨 |
| 원인 | 공용 disabled palette와 별도 status text |
| 제안 | disabled label 가독성 보존 + action 가까이 짧은 사유; 불가 상태도 디자인 variant로 작성 |
| 변경 경계 | 비활성 gate를 우회하지 않음 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX09 · P2

| 항목 | 내용 |
|---|---|
| 화면 | E01 |
| 문제 | 영문 Progression/Exploration이 일부16:9 캡처에서 단어 중간 줄바꿈된다. |
| 근거 | [achievements-v2-en-1920](evidence/live/achievements-v2-en-1920.png); [achievements-v2-en-1280](evidence/live/achievements-v2-en-1280.png) |
| 영향 | 헤더 스캔 방해·번역 품질이 낮아 보임 |
| 원인 | 210-wide tab/font32·pixel scale 변화·임의 word wrap |
| 제안 | 짧은 승인 라벨/가변 폭/의도된2행 규칙을 선택, 3viewport EN/KO 검수 |
| 변경 경계 | 문구 변경은 차기 localization 정본 패스에서 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX10 · P2

| 항목 | 내용 |
|---|---|
| 화면 | E01/C01/I06/G08 |
| 문제 | 일부 상세와 목록은 scroll이 필요하지만 mask와 바닥 절단만으로 그 사실을 알리기 어렵다. |
| 근거 | [achievements-v2-en-1920](evidence/live/achievements-v2-en-1920.png); [final-probe--wbiii-costs-ko](../post-commercial-ui-qa-tools/evidence/final-probe--wbiii-costs-ko.png); [CraftingUiBuilder.Build](../../client/Assets/Wildfolk/Editor/CraftingUiBuilder.cs#L9) |
| 영향 | 잠금·tier·추가 비용을 없는 정보로 오해할 수 있음 |
| 원인 | 여러 ScrollRect가 scrollbar/fade/remaining cue 없이 존재; 일부는 scrollbar 있음 |
| 제안 | scroll edge cue·선택 위치 유지·독립 scroll의 역할 구분; mask 밖 내용을 overflow 버그로 잘못 복구하지 않음 |
| 변경 경계 | 기존 모든 내용·필터와 scroll 접근성 유지 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX11 · P2

| 항목 | 내용 |
|---|---|
| 화면 | H03/S02 |
| 문제 | Language가 다음 언어로 순환하는 일반 버튼이며 슬라이더와 Reset의 범위도 시각상 설명이 적다. |
| 근거 | [settings-v2-ko-844](evidence/live/settings-v2-ko-844.png); [SettingsPanel.NextLanguage](../../client/Assets/Wildfolk/Runtime/UI/SettingsPanel.cs#L10) |
| 영향 | 선택 목록을 기대한 사용자가 동작을 예상하기 어려움 |
| 원인 | cycle callback; 공용 action component |
| 제안 | 현재 EN/KO의 명시 selection variant와 값 feedback을 설계; Reset 범위 안내 |
| 변경 경계 | selection modal 추가 시 keyboard/Back 경로까지 명세 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX12 · P2

| 항목 | 내용 |
|---|---|
| 화면 | G01/G10/G12 |
| 문제 | HUD가 여러 가장자리에 작은 수치·class·time·상황문구로 분산되어 우선 관찰 대상이 약하다. |
| 근거 | [qa-new-world-hud-ko-844](evidence/live/qa-new-world-hud-ko-844.png); [CharacterUiBuilder.ArrangeHud](../../client/Assets/Wildfolk/Editor/CharacterUiBuilder.cs#L25) |
| 영향 | 생존 경고/전투·상호작용/목표를 동시에 읽기 어려움 |
| 원인 | 독립 HUD controller와 고정 좌표 배치 |
| 제안 | HP/needs/상태, navigation/context, thumb actions의3영역으로 pilot 검증 |
| 변경 경계 | SP에 항상 Gold/Gem이 표시된다고 가정 금지; 현재 MP Wallet과 구분 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX13 · P2

| 항목 | 내용 |
|---|---|
| 화면 | P02/P05 |
| 문제 | 작은 node icon·짧은 이름·lock/seal이 여러 branch에서 비슷하게 보인다. |
| 근거 | [class-tree-ko-844](evidence/live/class-tree-ko-844.png); [resume-v2-common-tree-en-844](evidence/live/resume-v2-common-tree-en-844.png) |
| 영향 | 노드 미리보기 선택과 학습/잠금/point부족 구분이 약함 |
| 원인 | 128cell atlas의 비슷한 경량 조합 + 120reference node |
| 제안 | 기호군의 굵기·실루엣·상태 badge/선 두께 통일, 선택 상세 위계 강화 |
| 변경 경계 | 실제6/54node와 prerequisite/point 유지; 가짜 node 없음 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX14 · P2

| 항목 | 내용 |
|---|---|
| 화면 | H01/G03/P01 |
| 문제 | Lucide 선 아이콘, 자체 node 도형, item sprite, fist/portrait의 시각 언어가 혼재한다. |
| 근거 | [home-v2-en-844](evidence/live/home-v2-en-844.png); [qa-new-world-hud-ko-844](evidence/live/qa-new-world-hud-ko-844.png); [JournalSymbolAssets.Names](../../client/Assets/Wildfolk/Editor/JournalSymbolAssets.cs#L11) |
| 영향 | 동일 제품이라는 인상이 약함 |
| 원인 | 용도별 다른 제작 원본/획선·표현밀도 |
| 제안 | navigation/action/stat/item 용도별 규칙을 정하고 같은 역할 안에서 통일 |
| 변경 경계 | Item 의미/라이선스/지급 ID 유지; 단순 전체 monochrome 변환은 채택 안 함 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX15 · P2

| 항목 | 내용 |
|---|---|
| 화면 | H01/H02/M01 |
| 문제 | Single Player는 관리 화면이 아니라 새 세계 시작이고 Host는 별도 고정 저장을 사용한다. |
| 근거 | [HomeScreen.SinglePlayer](../../client/Assets/Wildfolk/Runtime/UI/HomeScreen.cs#L24); [SaveRepository.MultiplayerDefault](../../client/Assets/Wildfolk/Runtime/Persistence/SaveRepository.cs#L21) |
| 영향 | 사용자의 같은 world solo/host·비파괴 선택 목표와 현재 흐름이 다름 |
| 원인 | 단일 SP/Host 파일과 IsMultiplayer/HostId gate |
| 제안 | 현재 warning 유지, 미래 World Card/Continue-selected-world UX는 별도 architecture 승인 후 |
| 변경 경계 | 현 기능 버그/이미 multi-world 구현으로 보고하지 않음 |
| 신뢰/재현 | SOURCE-INFERRED / FUTURE GAP; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX16 · P2

| 항목 | 내용 |
|---|---|
| 화면 | M01 |
| 문제 | route 전환 때 이전 연결 오류 문구가 남아 반대 route의 안내와 섞일 수 있다. |
| 근거 | [multiplayer-verified-internet-ko-844](evidence/live/multiplayer-verified-internet-ko-844.png); [MultiplayerMenu.ToggleRoute](../../client/Assets/Wildfolk/Runtime/Networking/MultiplayerMenu.cs#L26) |
| 영향 | Room Code와 IPv4 중 무엇을 고쳐야 하는지 혼란 |
| 원인 | route 전환은 Destination만 지우고 NetworkSession.MessageKey는 유지 |
| 제안 | route-scoped validation/error 수명 정의; 입력 field 바로 옆 설명 |
| 변경 경계 | 새 네트워크 메커니즘이 아닌 UI state lifetime 검토 |
| 신뢰/재현 | SOURCE-INFERRED + audit capture; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX17 · P2

| 항목 | 내용 |
|---|---|
| 화면 | I07/C05/C06/C07 |
| 문제 | runtime supplies/포탑/navigation 패널은 최근 공용 journal/field 작성 경로와 별도 스타일 helper를 쓴다. |
| 근거 | [CampLootPanel.Build](../../client/Assets/Wildfolk/Runtime/UI/CampLootPanel.cs#L24); [TierThreeHud.Build](../../client/Assets/Wildfolk/Runtime/UI/TierThreeHud.cs#L36) |
| 영향 | 드물게 여는 고급 시스템에서 다른 제품처럼 보일 위험 |
| 원인 | fixed 1080/1100 panel + 자체 Label/Button palette |
| 제안 | 희귀 표면도 Penpot component/반응형 계약에 포함 |
| 변경 경계 | 외관은 source 위험; 해당 rare panel 새 runtime 캡처는 UNVERIFIED |
| 신뢰/재현 | SOURCE-INFERRED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX18 · P2

| 항목 | 내용 |
|---|---|
| 화면 | G11 |
| 문제 | 토스트가 단일 label을 짧게 교체하므로 여러 결과가 연속하면 중요한 실패/환급 정보가 묻힐 수 있다. |
| 근거 | [PlayerFeedback.ShowKey](../../client/Assets/Wildfolk/Runtime/UI/PlayerFeedback.cs#L22); [PlayerFeedback.ShowMaintenance](../../client/Assets/Wildfolk/Runtime/UI/PlayerFeedback.cs#L26) |
| 영향 | bag-full/save-failure와 성공 pickup을 구분해 재확인하기 어려움 |
| 원인 | 1.5–3s 단일 until/key/label, queue 없음 |
| 제안 | 우선순위/지속시간/중복 합산 정책 설계; 중요 실패는 행동 가까이 유지 |
| 변경 경계 | 새 알림센터/로그 저장 mechanics는 자동 추가하지 않음 |
| 신뢰/재현 | SOURCE-INFERRED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX19 · P2

| 항목 | 내용 |
|---|---|
| 화면 | E05/E06 |
| 문제 | 구매 OFF인데 상품3개·부활환산·구매복구/이력/문의가 한 화면에서 공간을 계속 차지한다. |
| 근거 | [store-ko-844](evidence/live/store-ko-844.png); [StoreScreen.Refresh](../../client/Assets/Wildfolk/Runtime/UI/StoreScreen.cs#L13) |
| 영향 | 현재 할 수 없는 과제에 주의를 빼앗기고 하단 링크가 약함 |
| 원인 | 기능 구현을 유지한 unavailable preview 상태 |
| 제안 | OFF/계정필요/진행중/완료를 독립 frame으로 간결하게 구성 |
| 변경 경계 | fake 가격/무료 Gem/실구매 활성화 없음 |
| 신뢰/재현 | CONFIRMED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX20 · P2

| 항목 | 내용 |
|---|---|
| 화면 | T01 |
| 문제 | Loading의 기본 Text Lowfire·단색막·arc가 Home wordmark/캠프 표현과 다르다. |
| 근거 | [TransitionOverlay.Create](../../client/Assets/Wildfolk/Runtime/UI/TransitionOverlay.cs#L16); [과거 연결 화면](../commercial-ui-mobile/completion/connecting-en-844.png) |
| 영향 | 전환이 제품 경험보다 시스템 대기 화면처럼 보임 |
| 원인 | runtime 독립 canvas/text/arc |
| 제안 | 기존 로고/선택한 motion token을 재사용하되 실제 phase/취소/실패 명확성 우선 |
| 변경 경계 | 가짜 progress %/불필요한 overdraw 추가 금지 |
| 신뢰/재현 | HISTORICAL + SOURCE-INFERRED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX21 · P2

| 항목 | 내용 |
|---|---|
| 화면 | G09/G13/World |
| 문제 | 월드 광석·생물 실루엣·수면 재질과 UI polish의 격차가 전체 품질 인상에 남는다. |
| 근거 | [qa-new-world-hud-ko-844](evidence/live/qa-new-world-hud-ko-844.png); [이전 forensic](../forensic-presentation-world-audit-2026-11-03/FORENSIC_AUDIT.md) |
| 영향 | UI만 교체해도 게임 전체가 상용 완성도로 보장되지 않음 |
| 원인 | 절차/Pillow 기반 alpha art와 서로 다른 제작 시점 |
| 제안 | UI pilot 배경은 실제 게임 화면을 사용하고 world art 의존성은 별도 승인 항목으로 분리 |
| 변경 경계 | 생물/물 수정·새 character art는 이번 범위 밖. facing correction을 미수정 버그로 재보고하지 않음 |
| 신뢰/재현 | HISTORICAL + current context; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## UX22 · P2

| 항목 | 내용 |
|---|---|
| 화면 | D01/H01/C03 |
| 문제 | Home이라는 말이 메인 야영지 메뉴, 무료 사망 복귀 지점, Workbench 나침반 목표에 걸쳐 쓰인다. |
| 근거 | [gameplay--death-ko-844](../post-commercial-ui-qa-tools/evidence/gameplay--death-ko-844.png); [DeathHud.Respawn](../../client/Assets/Wildfolk/Runtime/UI/DeathHud.cs#L18); [CraftingPanel.SetHome](../../client/Assets/Wildfolk/Runtime/UI/CraftingPanel.cs#L16) |
| 영향 | 사망 화면에서 앱 메뉴로 나가는 것으로 오해할 수 있음 |
| 원인 | 별도 system anchor에 같은 용어 |
| 제안 | 실제 목적지에 맞는 행동 문구와 간단한 결과 설명을 Penpot에 명시 |
| 변경 경계 | 무료 world respawn을 main menu 전환으로 바꾸지 않음 |
| 신뢰/재현 | HISTORICAL + SOURCE-INFERRED; 같은 source/화면 조건으로 다시 확인 가능, rare/runtime 미확인 범위는 별도 |

## 먼저 다룰10개

UX01 → UX02 → UX05 → UX04 → UX03 → UX07 → UX06 → UX10 → UX08 → UX09. 먼저 origin/행동 계약을 잠그고, type·slot·dense layout을 pilot에서 검증한 뒤 장식·아이콘으로 간다. 실제 구현은 차기 별도 승인 후다.

## 과거 교정과 현재 판단 분리

HP 숫자/bar 정렬, repair와 hotbar 직접 겹침, same-frame pause의 maintenance 잔상, paper frame 밖 Back, 항상 떠 있는 Sync, Class prerequisite/point 구분, floating joystick ownership, 공용 AudioButton 누락은 옛 교정 기록이 있다. 이번 source/화면에서 그 원래 문제를 동일하게 다시 확인하지 못했다. 현재 큰 maintenance 카드·작은 본문·동일 panel 반복은 그 교정을 취소하면 자동 해결되는 문제가 아니다. rollback은 권장하지 않는다.

현재 canvas/scene data가 과거 캡처 시점과 같은지 대조했고, 48d6528 이후 이 감사 외 제품 변경은 없다. 그래도 역사 fixture는 자연 progression/physical mobile proof가 아니다. 보스 가독성·real ad/video·IAP pending 실제 체감은 UNVERIFIED로 남긴다.
