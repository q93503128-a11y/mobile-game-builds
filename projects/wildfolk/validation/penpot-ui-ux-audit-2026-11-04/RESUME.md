# 다음 실행 인계 — UI/UX 전수 감사

## 사용자 의도와 중단점

원래 요청은 첨부 `cbbed97d-9d9d-4e4a-a26a-d9f4d1266e9c/붙여넣은 텍스트.txt`의 research-only UI/UX 전수 감사11개 보고서다. 사용자는 현재 작업을 마치고 다음에 이어서 하자고 요청했다. 이 체크포인트는 의도적 중단이며 완료 선언이 아니다.

게임 source/assets/scenes/prefs/save/content/backend는 읽기 전용. 새 audit directory 안의 보고서/증거/helper만 쓴다. Windows input/Focus/Show, scene regeneration, Penpot board 생성/디자인 구현 금지. Penpot MCP 연결은 필요 없다고 사용자가 명시했다. Unity MCP는 연결했다. 다음에도 main 직접; fetch/status/AGENTS/README/dev-playbook 최신 확인. 이번 기준 `48d6528758d18bf3df41ed02ad9aa0f268654a51` 이후 다른 변경이 있으면 영향 범위를 대조한다.

## 작업 상태

작성한 요청 보고서는 README/SCREEN_INVENTORY/CONTROL_AND_ACTION_MAP/NAVIGATION_AND_STATES/EVIDENCE_INDEX 5개이며 모두 체크포인트다. 나머지6개는 아직 없다. source 열거66 surface/185 action/103 route는 최종 수가 아니라 현재 등록 초안 수다. `tools/write_reports.py`는 앞의3개 상세 초안을 다시 생성한다. 나머지 정성 보고서를 생성하거나 완성하지 않는다.

현재 evidence/live PNG45개 중7개만 이번 턴에 실제 열어서 검토했다. 나머지를 차례로 열고 화면별 시각 품질을 평가해야 한다. `live-evidence-index.json.visuallyOpened`를 실제 열었을 때만 갱신한다. 추가 캡처는 source/game에 write하지 않고 QA 경로에서만. 첫 부적절한 드라이버 캡처는 evidence/rejected에 로컬 보존·Git 제외했다.

## 다음 실행 순서

1. 최신 main·규칙을 확인하고 status 확인. live-final의 MainMenu/Edit/clean/QA flag 없음이 현재도 맞는지 MCP로 읽는다. 사용자 session을 가정하지 않는다.
2. 실제로 안 본 현재 캡처를 검토한다. 화면 view/language/viewport는 동명 JSON과 실제 pixels를 대조한다. filename만 믿지 않는다. 3viewport 캡처를 확보했다는 것과 3viewport 시각 검증은 다르다.
3. 실제 station/Chest/character/common tree/rare overlays는 source + 최신 역사 캡처 대조, 필요하면 추가 안전한 live 관찰. 비용·보상·계정·사용자 prefs·실제 save는 건드리지 않는다. 현재 HUD 자연 시작만 관찰했고 생산/죽음/보스는 미체험이다.
4. `screens.json` 66개와 `actions.json`185개를 수동 검토한다. family contract가 각 callback의 실제 side effect를 과장하지 않는지 확인한다. Action instance671개는 반복을 묶은 초안이다. callbacks runtime Bind를 확인하고 gesture release/cancel을 유지한다.
5. source navigation103개에서 conditional destination·정확한 source method를 보강한다. 실제 Back 외 다른 controller.Open을 직접 부른 인위적 경로를 제품 버그로 집계하지 않는다.
6. 남은6개 보고서를 작성한다: VISUAL_UX_ISSUES / UI_ARCHITECTURE_AND_ASSETS / MOBILE_AND_LOCALIZATION / WORLD_SAVE_AND_MULTIPLAYER_UX / PENPOT_DESIGN_HANDOFF / REDESIGN_ROADMAP. fact/proposal/future goal/unverified를 표시한다.
7. 원래 요청의20항 adversarial completeness checklist를 실제 증거와 연결한다. P0/P1/P2, 미검증 수, Top10, pilot3개, future approval gates, full frame/component/state brief를 완성한다. source검색만으로100% visual coverage 선언 금지.
8. 링크·근거·수·게임파일 불변·보호검사·diff를 확인하고 main commit/push/fetch/equality/clean 후 최종 보고. 측정 시각을 추가하고 token/billing은 알 수 없으면 null 유지.

## 이미 확인한 중요한 사실

- Store.Close는 `fromHome`에 따라 Home/Hub로 간다. Store.Contact는 Store 숨김→Profile.Open→Profile.Help. Account.CloseHelp는 항상 Account.View=true. 현재 Unity `store-help-back-ko-844.png`에서 Account 복귀를 확인했다. 옛 “Store Back fixed”와 범위를 구분한다.
- Home.SinglePlayer는 Repository.Exists면 새 세계 교체 확인, 아니면 random NewGame. multi-world selector가 아니다. Continue는 Rules.Validate한 primary/backup을 읽고 invalid면 몰래 새 게임으로 바꾸지 않는다.
- SaveRepository.Default=`world-v1.json`, MultiplayerDefault=`multiplayer-world-v5.json`. 자유로운 multi-world가 아니라 모드별 고정 slot이다. NetworkSession.Host는 IsMultiplayer + HostId 일치를 검사한다. 단순히 SP 파일을 Host에 지정하면 호환되지 않는다.
- WorldSnapshotCodec 공유는 미래 통합의 출발점일 뿐이다. 참가자 owner/WorldId·HostId/현재 Player mirror/Host authority/죽음·pending 거래/migration 검토가 필요하다.
- ClassProfiles.GoldSources는 SP와MP fixed repositories 두 개다. GoldAvailable은 합산이 아니라 가장 큰 payer Gold; Purchase는 배열 순서상 지불 가능한 첫 repository를 쓴다. ClassPurchaseIntent/PendingXP와 GemReviveIntent가 WorldPath를 참조한다. world rename/delete/파일 이동은 이 intent 해결·source identity와 연계해야 한다.
- Settings.SetLanguage는 string이 아니라 index API. QA Directory가 지정되면 LocalSettings는 static memory dictionary를 쓴다. 일반 실행은0.5s debounce + Flush로 PlayerPrefs 저장. Reset은 즉시이며 확인 dialog 없다.
- Attendance tile28개는 Button 컴포넌트지만 Refresh에서 전부 interactable=false/Transition.None. 하나의 Attend action이 현재 board의 서버 eligibility를 사용한다. newcomer7/monthly28; monthly stamp는 통화·아이템이 아니다. Events=[]; 가짜 보드 추가 금지.
- Recipe111/Inventory36(9+27)/Chest24·36, Workbench I–III, Campfire Batch/Pot·Furnace·Blast Sequential, TurretWB를 유지한다. C01 shared panel의 variant이며 새 station UI mechanics를 발명하지 않는다.
- Common SkillTree authoring에는 Back만 있다. Close() API는 남지만 직접 Close button으로 연결되지 않는다. Character→Tree→Back→Character→Close가 실제 경로.
- MP NetworkInventoryUi.Bind가 Close/Crafting/Bag/Upgrade/Character 콜백을 로컬 actor로 교체한다. Serialized listener만으로 동작을 단정하면 안 된다.
- Death Free Home은 앱 MainMenu가 아니라 같은 world의 초기 HomeAnchor 복귀. Workbench Compass Home과 다르다. SP Ad/Gem/무료Home, MP Downed24s/teammate hold4s/Dead무료Home 경계. 원정·death revision과 실제 server reserve/world commit/finish를 보존한다.
- CampLootPanel, TierThreeHud, TransitionOverlay, PlayerFeedback catch icon은 runtime creation 경로다. 나머지는 대체로 Editor builder가 작성한 serialized UGUI다. 구현되지 않은 UI Toolkit·자동 Penpot→Unity 통합으로 설명하지 않는다.
- Krita2개 ZIP/merged previews, docs/design SVG3개, Lucide source7개/sources.json 확인. 기존 Penpot은 옛 Home A/B/C, 전체 UI 시안이 아니다. KRA는 scene dependency가 아니라 편집 원본. UI PNG/art generators/9slice/import/fonts/material/animation의 상세 ownership 보고서는 남아 있다.

## 상용 참고 조사: 이번에 연 공식 자료

- DST Mobile Solo: https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/795-can-i-play-solo/?p=android — Host Game·1player·Local Only. solo/co-op world identity를 나누지 않는 원칙 참고; Lowfire 현 SP/MP save 호환이 생기는 근거는 아니다.
- Crashlands2 1.2: https://blog.bscotch.net/post/crashlands-2-patch-12/ — compound rollup/craftability tracking, 언어별 stat label 공간, loading tip에 따른 bar 흔들림 수정. 정보 명료성 참고. Lowfire의 재료자동제작·경제 변경 도입을 승인한 것이 아니다.
- Crashlands official: https://www.bscotch.net/games/crashlands — inventory management 자체를 제거한 다른 구조. Lowfire36slot/장비/상자를 그대로 무한가방으로 바꾸는 대안은 범위 밖.
- Core Keeper publisher PDF: https://fireshinegames.co.uk/wp-content/uploads/Core-Keeper-Patch-notes-Post-1.0-1.0.0.16.pdf — primary source opened. 상세 해당 사례를 다음 실행에서 PDF text와 정확히 대조해야 한다. 검색 결과만 보고 UI 사례 검증 완료로 표시하지 않는다.
- Terraria505 1.4.3: https://support.505games.com/support/solutions/articles/150000146926-terraria-console-mobile-v1-4-3-patch-notes-2022-april-27- — search excerpt만 현재 확인. 실제 본문 open/필요 부분 확인 후 transferable principle 비교표에 넣는다.

상용 게임을 실제로 플레이하거나 이번에 상용 화면 full-resolution 비교를 완료한 것은 아니다. 기존 docs/UI_RESEARCH 및 옛 REVIEW는 HISTORICAL로 구분한다. 최종 스타일을 일방 확정하지 않는다.

## MCP/helper 재개 주의

Unity tool namespace는 이번 도구 목록에 없었으나 기존 localhost HTTP MCP(`127.0.0.1:8080/mcp`)에 `tools/audit_session.py`로 연결했다. 기존 client를 import하되 session state는 이 폴더 private에만 쓴다. 새 server/editor를 시작하지 않는다. `execute_code`는 in-memory C#이며 Unity source script를 생성하지 않는다.

`capture_frontend.py`는 나중에 개선이 필요한 기록용 드라이버다. 반복 후 route가 유지되면 internet/lan filename이 실제 route와 다를 수 있다. `multiplayer-verified-*`와 실제 JSON labels가 더 정확하다. 숨겨진 Delete callback preview는 정상 linked 진입 증거가 아니다. blindly rerun하지 않는다.

`audit_session.py`의 native/user-before snapshot은 이번 실행 baseline이다. 다음 실행에서는 별도 session baseline 파일을 만들어야 하며 이전 보호 증거를 덮지 않는다. timing phase는 비중복 wall-clock이고 source검토/도구대기가 섞인 구간이다. native-main 비교는 whitespace/serialization-order/4 folder GUID/Editor preference 차이를 기록했으며 native sync는 하지 않았다.
