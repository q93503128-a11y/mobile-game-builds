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
