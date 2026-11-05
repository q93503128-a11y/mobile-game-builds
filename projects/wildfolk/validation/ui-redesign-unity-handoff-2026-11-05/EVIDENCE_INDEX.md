# 근거 색인 및 검증 범위

## 저장소 기준과 필수 문서

- Game repository: q93503128-a11y/mobile-game-builds, main 3ce07a31f03ac90a9edf5c93e877a2f03cc4ff0a (2026-11-05 fetch; 로컬과 원격 동일, 수정 전 clean)
- Shared rules: q93503128-a11y/dev-playbook, main e022e9e4ff3947fcbd7f5b0237c5905d7c0409c7 (동일 상태)
- Read: projects/wildfolk/AGENTS.md, README.md, RULES.md; docs/DESIGN_BIBLE.md, docs/ARCHITECTURE.md 및 UI/Inventory/Localization/Save/Multiplayer/economy/progression 관련 정본 문서.
- 새 구현 시작 전 모든 원격 main을 다시 fetch하고 위 사실/버전이 바뀌었는지 확인합니다.

## 기존 UI 감사

기존 보고서: [penpot-ui-ux-audit-2026-11-04](../penpot-ui-ux-audit-2026-11-04/README.md)

요청상 필독 10개 문서와 그에 연결된 inventory/navigation/action/state/visual reports를 읽었습니다. 실제 감사 캡처와 JSON metadata는 ../penpot-ui-ux-audit-2026-11-04/evidence/live/ 아래 있습니다.

열어본 실제 Unity Game View PNG:
- home-v2-ko-844.png
- qa-new-world-hud-ko-844.png
- resume-v2-inventory-ko-844.png
- resume-v2-hand-crafting-ko-844.png
- settings-v2-en-844.png
- store-help-back-ko-844.png
- class-overview-ko-844.png
- multiplayer-verified-lan-ko-844.png
- achievements-v2-ko-844.png
- attendance-v2-ko-844.png

관련 JSON에는 viewport·language·view state 및 가능한 UI bounds가 있습니다. 예: 844×390 Game View의 quick slot font 약 5.98 frame px, time 약 7.97, level 약 8.76, health number 약 11.95; hotbar slot target 폭 약 44.62, interact 약 59.76. 이는 Game View 좌표 추정이지 physical dp/device measurement가 아닙니다. 캡처에 조이스틱이 안 보일 수 있으며 입력 시작 시 floating하는 현행 구현과 맞습니다.

감사 요약은 66개 UI surface / 57 current code surfaces / 8 future/unimplemented / 1 historical / 185 semantic actions; issue ranking P0=0, P1=7, P2=15. 이 숫자는 화면 완료 개수나 실기기 결과가 아닙니다. 기존 assessment UX01–UX22 세부 내용은 VISUAL_UX_ISSUES.md에 둡니다.

## Penpot A/B/C evidence

폴더: [penpot-visual-direction-stage1-2026-11-04](../penpot-visual-direction-stage1-2026-11-04/REVIEW.md), previews/.

모든 주요 미리보기를 열어 비교:
- direction-a-summary.png, direction-b-summary.png, direction-c-summary.png, component-comparison.png
- menu-a-1920x1080.png, menu-b-1920x1080.png, menu-c-1920x1080.png
- hud-a-1920x1080.png, hud-a-844x390.png; hud-b-1920x1080.png, hud-b-844x390.png; hud-c-1920x1080.png, hud-c-844x390.png
- inventory-a-1920x1080.png, inventory-a-844x390.png; inventory-b-1920x1080.png, inventory-b-844x390.png; inventory-c-1920x1080.png, inventory-c-844x390.png

A Forest Utility: 읽기 쉬운 상태·효용 구획은 유지 후보; 큰 밝은 표면은 장면 시야를 줄입니다.
B Crafted Survival: 슬롯/장비의 재료감은 유지 후보; 어둡고 무거운 공통 패널은 축소합니다.
C Modern Adventure: 시야와 주 행동 위계는 유지 후보; 844에서 투명 경계/작은 글자/상태와 XP 겹침은 수정 대상입니다.

## Stage 1.5 evidence

폴더: [penpot-hybrid-stage1-5-2026-11-04](../penpot-hybrid-stage1-5-2026-11-04/REVIEW.md), DESIGN_SYSTEM.md, MOBILE_CONTROLS_UX.md, IMPLEMENTATION_BOUNDARIES.md, previews/.

실제 열어본: hybrid-main-menu-844.png, hybrid-main-menu-1280.png, hybrid-main-menu-1920.png.
- 844에서 메뉴 panel과 보조 기능 밀도를 낮춘 이점.
- 1280·1920에서 큰 panel의 빈 하단, Single Player title/subtitle 겹침, Multiplayer의 강한 CTA 위계, 선택처럼 보이는 meta actions.
- 844는 Penpot SVG markup으로 생성한 preview path라 공식 Penpot PNG export나 Android proof가 아닙니다. 나머지 intermediate boards는 corrected/final set이 아닙니다.
- Page 06 has three menu boards but mismatched child counts (updated 844=61, 1280/1920=56), while 07–09 are incomplete. No final direction approval.

## 현재 source와 캡처 일치성

현재 main에서 UI source hashes를 대조했습니다:
| 파일 | 현재 SHA-256 |
|---|---|
| Editor/FrontendSceneBuilder.cs | 8309a11b84a495cc62897ca9e616857b7f566580839dba29c37faa594b0f9372 |
| Editor/InventoryUiBuilder.cs | 2fb566bfb0c830c3f264073e20282fd9714d71b65ea4c9d1f1e29351c7ab2c8b |
| Runtime/UI/HomeScreen.cs | a20873ecc09d02ad594ffb784f81d30898eb3b7927b1708d26bcb763820df3fd |
| Runtime/UI/GameplayHud.cs | aa99c3aa21adf08d45000d83ad58ea76438fb5e3b3b8255e3c584f4362ce6cd9 |
| Runtime/UI/InventoryPanel.cs | 33a53d98e41987efb317863e6d7907f964b0e43ab12e79494da3de1f4c50a3bc |
| Editor/GameplayHudBuilder.cs | bfa7d69fd37b059c21db716d15ce6973780961e2a085067e0760324838d56100 |

기존 감사 source-manifest의 FrontendSceneBuilder, InventoryUiBuilder, HomeScreen, GameplayHud 해시는 일치합니다. InventoryPanel은 당시 manifest f68606da70011209b71760a9548fc54bb375247c3589309b25620465ae90424e이며 현재 main과 달라졌습니다. 따라서 당시 InventoryPanel 캡처를 current rendering으로 단정하지 않고 다음 실행 때 새 캡처를 확보합니다. GameplayHudBuilder는 기존 manifest에 없었습니다.

실제 code reference:
- Editor/FrontendSceneBuilder.cs: MainMenu, 1920×1080 reference layout
- Editor/GameplayHudBuilder.cs: 204×204 action, 150×150 interact, 138×138 dash, 1080×120 hotbar, 112×112 bag visual baseline; ControlSizeLayout behavior
- Editor/InventoryUiBuilder.cs
- Runtime/UI/HomeScreen.cs, GameplayHud.cs, InventoryPanel.cs, SettingsPanel.cs, ModalLayout.cs, SafeAreaLayout.cs, ControlSizeLayout.cs
- README: Unity 6000.3.19f1 LTS, URP 2D, alpha status and product versions

## 기존 그래픽·라이선스 evidence

실제 asset/licensing inventory는 기존 audit의 UI_ARCHITECTURE_AND_ASSETS.md와 source-art-index.json에 있습니다.
- LowfireWordmark/LowfireFieldJournal KRA→PNG. Journal image 자체는 automatically 9-slice가 아닙니다.
- panel.png has 18 px border and Unity sliced sprite metadata; test actual corners before reuse.
- NotoSansKR.ttf + NotoSansKR-OFL.txt.
- navigation symbols sourced from Lucide; source list and ISC/Feather license note must remain with derivative set.
- Existing Lowfire item/class icons and JournalSymbolAssets atlas.
- UI/audio uses current AudioRuntime and AudioButton integration.

## 외부 공식 상용·플랫폼 자료 비교

| 공식 자료 | 확인한 실제 사례 | 이 문서에 적용한 원칙 |
|---|---|---|
| [Terraria Mobile 1.4.3 notes, 505 Games Support](https://support.505games.com/support/solutions/articles/150000146926-terraria-console-mobile-v1-4-3-patch-notes-2022-april-27-) | crafting/guide/reforge/shop/build tools/wiring/filter touch polish, configurable UI, per-page reset, Auto Select toggle/hold | 복잡한 게임 기능마다 터치 affordance와 설정 범위를 함께 설계, reset state 명확 |
| [Don't Starve Together mobile controls, Playdigious](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/791-can-i-customize-touch-controls/?p=ios-) | Edit Mode로 joystick/action button 위치와 크기 조절 가능, 단 개별 이동/fixed-floating joystick 설정은 없음 | editor는 범위가 한정돼도 별도 기능 비용 있음; default controls 먼저 실기기 검증 |
| [DST solo](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/795-can-i-play-solo/?p=android) 및 [pause](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/834-can-i-pause-the-game/?p=ios) | Local Only host, MP에서 서버 host만 pause; Solo auto-pause option | 네트워크 시간 의미를 시각만으로 단일화하지 않음 |
| [DST 2.2.0 update, Playdigious](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/879-update-2-2-0---october-6-2026/?p=ios) | double-tap inventory item equip/unequip/eat, near-text HUD option and targeting fixes | 슬롯/액션 모드를 분명히 하고 HUD label 위치도 조정 가능한지 후속 검증 |
| [Diablo Immortal accessibility, Blizzard](https://news.blizzard.com/en-us/article/23805083/making-a-game-for-everyonediablo-immortals-accessibility-features) | mobile skill button repositioning, chat font size, combat visibility and accidental touch constraints | 터치 크기와 화면 시야는 별도 평가, 조작 편집은 근거와 비용으로 결정 |
| [Crashlands 2 v1.2, Butterscotch Shenanigans](https://blog.bscotch.net/post/crashlands-2-patch-12/) | stat-label width expansion to avoid translated text shrinking; loading bar stability; diagnostic copy feedback | 긴 번역 폭을 확보하고 loading/error UI layout을 고정 |
| [Core Keeper Post-1.0.0.16 notes, Fireshine Games](https://fireshinegames.co.uk/wp-content/uploads/Core-Keeper-Patch-notes-Post-1.0-1.0.0.16.pdf) | salvage summary accuracy, scroll category arrows, network error/main menu overlap fixes | action summary가 실제 결과와 맞고 scroll/error route가 발견 가능해야 함 |

| 플랫폼 기준 | 결정에 주는 체크 |
|---|---|
| [Android touch target guidance](https://developer.android.com/guide/topics/ui/accessibility/views/apps-views) | 48dp target 권고를 참고; 844 frame unit을 dp로 환산하지 말고 기기 density에서 검증 |
| [Android display cutout](https://developer.android.com/develop/ui/views/layout/display-cutout) and [edge-to-edge guidance](https://developer.android.com/design/ui/mobile/guides/layout-and-content/edge-to-edge) | safe/cutout/system gesture insets에 주요 터치/정보 배치 금지 |
| [WCAG contrast minimum](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum) and [non-text contrast](https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast) | 텍스트 및 주요 UI component contrast를 측정할 체크리스트; 통과 인증 주장 아님 |

모든 외부 자료는 개발사/퍼블리셔 또는 플랫폼/표준 공식 자료입니다. 사례는 패턴 추출 근거일 뿐 art/layout 복제 권한이 아닙니다.

## 확인하지 않은 항목

- Unity MCP 사용, 현 Unity Editor Play Mode, 최신 게임빌드 실행, Android 실기기 조작/가독성/DP/cutout: 미검증
- UI 성능, texture/GPU/overdraw, 실제 draw calls, battery: 미측정
- contrast 수치와 screen reader/accessibility runtime: 미측정
- Stage1.5 뒤의 current Penpot object graph/export/heartbeat 및 각 component status: 검증하지 않음. 기존 preview/문서만 사용
- 모든 57 state의 direct live reproduction: 미수행. 기존 감사에 확인상태별 FACT/CONFIRMED/SOURCE-INFERRED 분리가 남아 있음
- Google Play/Store purchase/ads online authority: 활성화·실거래 검증하지 않음

다음 승인된 파일럿에서는 source/scene 상태를 다시 확인하고 현재 소스에서 화면을 재캡처한 뒤 Android 기기 검증을 진행합니다.
