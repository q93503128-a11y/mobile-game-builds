# 품질 교정 UI 조사 — 2026-10-14

출시 게임의 실제 UI 캡처와 개발자 설명을 비교했다. 라이브 프로젝트의 장비/스킬 화면도 먼저 직접 캡처했다. 상대 아트/좌표/아이콘/수치의 복제가 아니라 정보 계층·입력 동선·공간 배분에 대한 자체 관찰이다. 참고 사진 원본은 무시된 tests/artifacts/ui-references에 보관하며 제품 Asset으로 넣지 않는다. 일부 사진에는 글쓴이의 주석이 있고, Diablo IV는 2023 UI, PoE는 Atlas UI라는 범위를 구분한다.

| 영역 | 실제 참고 화면 | 관찰과 Wildfolk 결정 |
|---|---|---|
| Inventory Equipment | [Minecraft Inventory](https://www.minecraft101.net/g/user-interface.html), [Terraria Mobile 개발자 회고](https://medium.com/@watsonwelch/the-making-of-terraria-mobile-part-3-crafting-a-new-ui-4fb84708c767) | Portrait/실제 장착 슬롯과 소지품을 구분한다. Minecraft의 인벤토리 인접 장비와 Terraria의 역할별 독립 구역을 비교해, Wildfolk는 작은 Preview/Head·Body/36칸/선택 비교를 한 화면에 배치한다. 모바일에서 장비 카드 네 개를 훑는 상품 목록을 제거한다. |
| Skill Tree | [Diablo IV 공식 화면/설명](https://news.blizzard.com/en-us/article/23938756/make-sanctuary-yoursplay-your-way-in-diablo-iv), [2023 출시판 UI](https://www.gadgets360.com/games/reviews/diablo-4-review-pc-release-date-gameplay-story-expansions-progression-combat-skills-shop-microtransactions-lilith-iv-blizzard-entertainment-4117925), [PoE 공식 Atlas 화면](https://x.com/pathofexile/status/2052485258730422712) | D4의 가지·Rank·남은 Point와 PoE의 연결된 투자 경로를 비교했다. Atlas는 캐릭터 Skill의 규칙 참고가 아니라 그래프 가독성 참고다. 큰 트리/스크롤/전체 해금 규칙을 복제하지 않고 실제 네 노드·선행 연결·선택 상세·상태·투자 CTA로 줄인다. |
| Crafting | Terraria Mobile, [DST 실제 Crafting 화면](https://forums.kleientertainment.com/klei-bug-tracker/dont-starve-together-beta-branch/having-the-crafting-menu-open-doesnt-refresh-inventory-hud-r34375/), Minecraft Crafting/Furnace | Terraria의 재료/Recipe 구역과 DST의 Category→Recipe→선택 결과/재료 수량 구조를 비교한다. DST 사진은 2022 Beta branch에서 캡처된 실제 게임 UI이며 제안 Mockup은 사용하지 않았다. Wildfolk에서는 제작 화면의 36칸 Inventory를 제거하고 Category별 두 열 Recipe/선택 결과/개별 재료 보유량을 우선한다. All의 Category Section 안에서 Tier 순서를 유지한다. |
| Stamina / Dash | [BOTW HUD 캡처](https://gamebanana.com/requests/25132), [Genshin 모바일 실플레이 주석 화면](https://www.hoyolab.com/article/38409517) | Zelda의 행동 가까운 임시 Stamina와 Genshin의 오른쪽 행동군/전진 Chevron을 비교했다. Zelda는 모바일 게임이 아니므로 위치·임시 정보 계층만 참고한다. Wildfolk는 Player 아래쪽 Arc/Full 뒤 Fade, 기본 Sprint·Dash와 공격 옆 전진 Chevron을 적용한다. Genshin의 많은 행동 버튼은 그대로 들이지 않는다. |

상용 화면의 정확한 터치 크기/반응 시간/경제를 측정했다고 주장하지 않는다. 구현 화면은 Wildfolk의 자연색·무광·성숙한 Casual Survival과 1920×1080 Landscape 기준으로 별도 검수한다. 작은 Skill Tree와 초기 Armor는 최종 상품 아트/출시 품질 선언이 아니다.

Dash는 외부 게임 Icon을 추출하지 않고 [Lucide chevrons-right](https://github.com/lucide-icons/lucide/blob/main/icons/chevrons-right.svg)를 무광 팔레트로 래스터화했다. [ISC/MIT 고지](https://github.com/lucide-icons/lucide/blob/main/LICENSE)를 제공한다. Armor 네 Overlay는 imagegen의 built-in 편집 결과를 기존 Sprite Canvas에 맞춰 Import했으며 기존 손/얼굴 실루엣은 유지했다.


## 생태·구조물·전투 조사 — 2026-10-15

| 영역 | 실제 출시 게임/1차 자료 | 참고 원칙과 Wildfolk 적용 |
|---|---|---|
| Spawn/Population | [Minecraft Creator spawning deep dive](https://learn.microsoft.com/en-us/minecraft/creator/documents/spawning/entityspawningdeepdive?view=minecraft-bedrock-stable),[공식 Mobs 소개](https://www.minecraft.net/en-us/article/minecraft-mobs) | Habitat/밀도/거리/시간/Group 크기를 독립 조건으로 둔다.4×3 종별 부족 보충/Camera·Player·Base 회피/Hard Cap에 적용하며 고정 한 개체 부활과 분리한다. |
| Herd/Flock | Minecraft 공식 소개의 작은 초식 무리,[Klei DST Update280490](https://kleiforums.com/forums/topic/94257-game-update-280490/)의 지역 Herd 갱신 설명 | 느슨한 근접 이웃 인식/간격/영역이 중요하다. Klei 페이지 본문은 접근 오류였으므로 검색에 노출된 업데이트 설명만 확인했고 정확한 반경/타이밍은 복제하지 않았다. Wildfolk는 직접 이웃 경보와 가벼운 Cohesion,종별 간격을 따로 둔다. |
| Group AI/Camp | [출시 Diablo Immortal Monster Commander 업데이트](https://news.blizzard.com/en-us/article/24246207/sing-the-ballad-of-the-moon),[Valheim 게임 데이터 직접 조사 자료(비공식)](https://www.valheim.tools/locations/fuling-village) | 역할이 다른 동료가 함께 읽히고 야영지는 거주 공간으로 읽혀야 한다. Commander의 정확한 진형을 복제하지 않고 Goblin 근접+새총 보조/직접 주변 지원/작은 천막·불·상자로 적용한다. 경보7/최대12/단계1 제한은 Wildfolk 설계 수치다. |
| Wall/Door | [Minecraft Brick 건축 소개](https://www.minecraft.net/en-us/article/build-it--brick-),[Project Zomboid 개발자 Door 영상](https://projectzomboid.com/blog/news/2023/11/one-door-openz/),[현재 Build 상태](https://projectzomboid.com/blog/news/2017/02/buildstatus/) | 연결된 두꺼운 재질/열린 통로를 읽게 한다. PZ의 기존 즉시 전환→문짝 애니메이션 개선 설명과 비교해 고정 Frame/외곽 Hinge/.32초 easing/충돌 동기화를 적용한다. PZ 수치를 측정한 결과가 아니다. |
| 새총 실제 동작 | [SimpleShot 제조사 release 설명](https://simple-shot.com/blogs/ask-simpleshot/ttf-to-pfs-shooting-tips),[frame/band/pouch 구성](https://simple-shot.com/collections/slingshot-parts) | 고정 프레임/당겨 저장한 장력/당기는 손의 놓기/밴드 복귀를 분리한다.0.64초 발사와 짧은 뒤쪽 손 잔류는 게임의 Telegraph 설계이며 실제 발사 속도 복제가 아니다. |
| Modal/Equipment/Combat | 기존 Minecraft/Terraria Mobile/Diablo IV 출시 UI 자료 | Equipment를 소지품과 가깝게 두고 Drag/빠른 Use를 공통 API로 연결한다. 큰 Modal의 중심·차단 계층을 일관되게 유지한다. 밀착에서 Collider 기준으로 Arc를 완화하되 정상 거리의 방향과 Single Target은 유지한다. |

외부 Sprite/Animation/Layout은 추출하지 않았다. 신규 생물·시설은 사용자 지시에 따라 기존 Python/Pillow 도형 제작을 확장했다. 생성 이미지2장은 폐기했고 Unity Asset에 적용하지 않았다. 상용 게임 영상의 프레임별 타이밍/모바일 실기 난이도/정확한 Spawn 수치를 측정했다고 주장하지 않는다.

## Home / Frontend — 2026-10-15

| 상용 게임·1차 자료 | 관찰 범위 | Wildfolk 적용 |
|---|---|---|
| [Minecraft Bedrock UI팀](https://www.minecraft.net/en-us/article/creating-create-new-world),[공식 Play/Server 진입](https://www.minecraft.net/en-us/article/how-play-minecraft-server) | Play를 먼저 두고 세계 생성·설정·친구 참여를 단계화하는 흐름과 UI팀의 명확한 의사결정/정보 위계 설명 | 단일 저장이므로 유효 Continue 우선, 새 세계는 별도 선택·교체 확인. Bedrock의 많은 옵션/Marketplace/배경을 복제하지 않음 |
| [Terraria Mobile 공식505 가이드](https://support.505games.com/support/solutions/articles/150000147250-how-do-i-set-up-a-multiplayer-game-in-the-mobile-version-of-terraria-),[Single Player 저장 안내](https://support.505games.com/support/solutions/articles/150000147328-will-my-progress-on-the-server-get-saved-once-i-close-the-server-) | 모드 선택 뒤 Host/Join으로 나누는 모바일 흐름과 Single Player의 Main Menu 복귀 시 저장. 연결된2019 포럼 본문은 현재 루트 페이지로 Redirect되어 상세 좌표/현행 화면을 확인하지 못함 | Home의 Single/Multiplayer 구분, Pause의 저장 후 복귀. 실제 네트워크가 없는 이번 화면은 명확한 이용 불가 안내만 제공 |
| [Don't Starve Together Mobile Host](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/788-how-to-create-a-server/),[Solo](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/795-can-i-play-solo/?p=all),[Klei Options](https://support.klei.com/hc/en-us/articles/5832923019668-DST-How-to-find-your-Klei-User-ID-KUID) | Main의 Host Game→세계/세션 설정, Local Only/1인과 함께 사용할 수 있는 분리, Options 경로 | Home과 gameplay 전환을 분리하되 향후 Host/Room 연결 경계를 유지. 계정/서버 준비 절차는 이번에 복제하지 않음 |

세 자료의 CTA/이어하기 대 새 게임/설정/협동 진입 원칙을 비교했다. Safe Area·엄지 도달·배경 대비의 정확한 상대 게임 좌표는 측정하지 않았으며 Wildfolk에서1920×1080의 오른쪽 행동군/왼쪽 타이틀·정적 야영 배경/기존 SafeAreaLayout으로 설계했다. 상단 작은 Settings만 두지 않고 엄지로 접근할 수 있는 메뉴 하단 버튼을 제공한다. 외부 게임 Asset/Code/Layout을 복제하지 않았다.


## Lowfire Home 시각 재설계 — 2026-10-15

이번 패스에서 위 Minecraft UI팀의 [Create New World 설계](https://www.minecraft.net/en-us/article/creating-create-new-world), [DST Mobile Host 안내](https://playdigious.helpshift.com/hc/en/39-don-t-starve-together/faq/788-how-to-create-a-server/)를 다시 확인했다. 행동 계층/모드 분리를 참고했고, [Valheim 공식 patch 1.0.14](https://www.valheimgame.com/news/patch-1-0-14/)의 Main Menu particle overdraw 개선은 배경 효과를 늘리지 않고 기존 정적 야영지를 유지하는 판단에 참고했다. 이 자료에서 특정 타이틀 글꼴·수첩 패널·터치 좌표를 도출했다고 주장하지 않는다. 성능 향상을 직접 측정한 결과도 아니다.

Penpot MCP에 1920×1080 세 Board를 실제 작성하고 PNG export로 비교했다. [A 야영지 표지판](design/lowfire-home-a.svg)은 짙은 목재/큰 이어하기/보조 버튼이다. [B 야전 수첩](design/lowfire-home-b.svg)은 왼쪽 제목/오른쪽 제본 종이/큰 이어하기/구분선 아래 두 모드/하단 Settings다. [C 간결한 생존 메뉴](design/lowfire-home-c.svg)는 반투명 어두운 면과 가로 모드 두 개다. Safe Area 점선은 설계 참고선이며 제품에는 출력하지 않는다. 배경의 원/도형은 기존 배경의 자리만 표현하는 시안이고 게임용 신규 월드 아트가 아니다.

B를 선택했다. 숲과 구분되는 밝은 종이가 버튼 계층을 읽게 하고, 제본/작은 불씨색으로 제목과 야영지의 재질감을 연결한다. A는 다시 어두운 상자/규칙적인 쌓기와 가까웠고 C는 두 모드의 폭과 일반 앱 메뉴 느낌이 남았다. 기존 allcaps와 홍보 문구를 없애고 Krita lettering 및 실제 UI Sprite로 적용했다. 최초 검수의 비활성 Continue가 짙은 갈색이어서 활성 상태와 구분이 약했으며, 최종본에서는 회색으로 구분했다.

Penpot file 0f3fb038-01f9-8046-8008-cad21bd7de79 / page 4e37fba4-57b1-80c6-8008-cb14dda39fa0 (Lowfire · Home concepts). 원본 Board는 사용자 Penpot 파일에 남아 있고 저장소 SVG는 작성한 동일 시안의 휴대용 원본이다. 선택 B의 글꼴 자리표시는 Unity에서 Krita 로고로 교체했다. 출시 로고·실기 터치 접근성 확정이 아니다.
