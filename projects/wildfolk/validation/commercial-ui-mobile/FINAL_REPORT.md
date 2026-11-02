# Commercial UI / Mobile Presentation — 최종 검수 보고

2026-11-02. **요청 범위의 저장소 구현·관련 검증 마감. 디자인은 PC/Unity 검수 후보이며 사용자 시각 승인과 실기기 검증은 별도다. Internal Test RC2: NOT READY.** AAB/업로드/Production·광고·IAP 활성화를 하지 않았다. 이 문서는 2026-10-31/11-01의 실제 기록과 이번 마감 검수를 합친 보고이며, 과거 결과를 오늘 새로 플레이한 것처럼 취급하지 않는다.

## 1–6. 기준·문제·외부 비교·디자인

|항목|최종 내용 / 증거|
|---|---|
|1. 시작 SHA|전체 패스 `d32c07ce7dd598ac3f607d33fd1c2f75d16bf76c`; 이번 재개는 fetch한 clean main `8ee7c686f300d0c9d85654daff63eaac72ee7f66`|
|2. 종료 SHA|이 보고서를 포함하는 main 마감 커밋이 종료점이다. 자기 자신의 SHA를 문서에 넣는 대신 push 후 최종 응답에 local HEAD/remote main의 실제 full SHA를 기록한다. 작업 내용은 `files-changed.txt`와 Git diff로 추적한다.|
|3. 기존 문제|저장 없는 Continue 강조, 기능이 몰린 Profile, 같은 무게의 직사각형 버튼, Settings 정렬, 오프라인 Logout, 삭제 문장의 제본선 침범, 빈 출석, 모호한 잠금, 작은 슬롯/제작 target, MP 중복 HP/역할 이름 겹침, 고정 joystick, 전환 안내 부재. 실제 baseline과 수정 과정은 [REVIEW](REVIEW.md)에 보존했다.|
|4. 상용 비교|Mine Survival, Last Day on Earth, Westland Survival, Grim Soul, Survivor.io, Archero2, Don't Starve PE, Terraria Mobile 및 별도 DST Pocket Edition의9게임 공개 이미지/공식 도움말. 2026-10-31에 확인한 이미지·버전·보조 자료의 한계를 [비교표](REVIEW.md#public-commercial-comparison), [출처/hash](reference-index.json)로 남겼다. 현재 모든 이미지가 출시 client 화면이라는 주장은 하지 않는다.|
|5. 채택/배제|Play 우선, 상태별 행동, 출석 보드, 업적 family 진행, 연결된 skill graph, 양손 입력, 맥락별 primary/secondary를 채택했다. 타 게임 asset/layout 복제, 새 통화·가챠·퀘스트·가짜 가격/로딩%·과도한 알림은 배제했다. 각 실제 사례/차이/이유는 위 비교표에 있다.|
|6. 디자인 system|기존 숲·야영지·field journal, Noto Sans KR, 잉크/종이/olive/ember. Primary·secondary·danger·link·row·tab·reward tile·modal을 구분한다. reference spacing8/16/24/40/64, title42–48/body32–36/caption28–30, 주요 action120–132 높이를 실제 viewport에서 축소 검수했다. reference 단위를 Android dp로 단정하지 않는다. hover 필수 tooltip은 쓰지 않는다.|

## 7–23. 화면·입력별 결과

|항목|최종 동작 / 실제 검수|
|---|---|
|7. Main Menu before/after|유효 저장은 Continue, 없으면 새 세계가 primary다. Play/멀티/설정/Profile은 icon+label, Class는 기존 emblem 행, Gem/업적/출석은 별도 캠프 탐색 영역이다. chest는 campfire에서 떨어뜨리고 Settings label/target을 함께 정렬했다. [이전](resume-20261031/screens/before-main-en-1920.png) · [최신](resume-20261101/after-home-empty-en-844.png) · [실제 QA Continue](resume-20261101/after-home-continue-ko-844.png).|
|8. Meta navigation|Profile/업적/출석/Class가 Home에서 직접 열린다. Gem Store Back은 실제 진입 화면으로 돌아간다. Profile은 identity/class/level/서버 기록 요약과 account 중심이며 기록 링크는 보조 행이다. 실제 pointer navigation 검사도 통과했다.|
|9. Attendance|신규7일/월간7×4, 오늘/수령/미래, 실제 Gem milestone과 방문 stamp를 구분한다. 계정이 없으면 catalogue를 읽기 전용으로 보여주며 event 없음은 설명한다. reward/UTC/reset/claim 권위 불변. [월간 fixture](resume-20261031/screens/monthly-fixture-ko-844.png).|
|10. Achievement|22family/58tier/7category를 family 진행/현재·다음 목표/보상/claim로 표현한다. 58개 동일 카드 나열이 아니다. claim/claim-all는 기존 서버 결과를 따르고 실제 claimable 상태만 badge다.|
|11. Profile|class/level, 연결 요약, 확인된 Gem/업적 요약. 비연결 상태는 광고 Load 버튼을 숨기고 연결 필요를 설명한다. 연결/cloud 가능 fixture에서는 기존 Load가 돌아온다. [연결 안 됨](resume-20261101/after-profile-ko-844.png) · [연결 fixture](resume-20261101/after-profile-connected-fixture-en-844.png).|
|12. Multiplayer|Create/Join과 Waiting room을 분리하고 code/인원/방장 대기를 표시한다. MP Class entry는 Home의 공통 Class로 통합했으며 admission/소유 검증은 그대로다. 실제 두 프로세스의2/4대기·진입·퇴장·저장·재Host/Continue를 확인했다. Relay나4기기 테스트가 아니다.|
|13. Class|6직업의 실제 role/설명, 소유·가격·현재 선택, XP/포인트/다음 milestone, 선택/해금 CTA와 별도 tree다. unlock/level/비용/능력은 source catalog를 따르며 신규 class/수치 변경이 없다.|
|14. Skill tree|실제 선행 관계의 node 중심 연결과 패딩, learned/selected/prerequisite lock/point shortage를 구분했다. 아래 이름/노드 target을 보정했다. [직업 tree](resume-20261031/screens/menu-stage-class-tree-ko-844.png).|
|15. Account|anonymous/authenticated-unlinked/offline/linked/cloud unavailable/conflict/deleting을 로컬 typed fixture로 확인했다. 미연결 Logout/Delete를 숨기고 상황에 맞는 연결/동기화/충돌 선택만 노출한다. Google 실제 로그인이 아니다.|
|16. Delete account|평면 종이에서 삭제 대상·보존되는 local world/최소 구매 기록 정책·비가역 경고를 나누고 Cancel/danger CTA를 분리했다. 기존 backend transaction/실제 정책을 바꾸지 않았다. [삭제 경고](resume-20261031/screens/menu-stage-delete-ko-844.png).|
|17. Store|24/72/144 Gem 유지. 실제 수량/icon/선택 상태와 revive 용도, platform price가 있을 때만 가격을 표시한다. OFF는 이용 불가와 읽기 전용 설명, 구매 CTA 숨김. recovery/history/support는 tertiary다. 긴 가격/pending은 typed billing fixture이며 실제 Google 결제가 아니다.|
|18. Loading|실제 New/Continue/Home/Host/Join/NGO client load 경계에 phase+spinner, 가짜% 없음. New≈5.12s/Continue≈6.28s/Home≈.86s는 이전 Editor 관찰과 probe overhead 포함이다. Client 이벤트 구독 누락은 실제 peer에서 재현·교정했다. 마지막 캡처에서 취소 뒤 남던 Connecting 문구도 발견해 Close가 진행 문구만 정리하도록 고쳤다. 실패/Host 종료/저장 실패 문구는 보존한다. [최종 취소 화면](completion/network-final/join-cancelled-en-844.png). 이번에는 invalid-code, 연결 중 Cancel, no-server 실패 후 메뉴 복귀를 실제 관찰했다. 로컬 연결 실패≈5.08s이며 SDK 종료 경로와 기존12초 watchdog을 사용한다. 기대한 transport error가 Editor Error Pause를 걸었을 때 QA만 unpause했고 해당 사실을 기록했다.|
|19. HUD|공통 HP/허기/갈증/level/day/time과 별도 class/gold. MP의 중복 Health/Level/XP 문구를 제거하고 로컬 actor에 공통 layout을 bind했다. latest peer healthLabels=1/ClassControls 정상.|
|20. Floating joystick|넓은 왼쪽 영역의 첫 touch 위치에 반투명 base, knob 제한/deadzone, pointer 소유·release/cancel/focus/modal reset. low/high/center, 이동+공격/상호작용, hotbar/pause, 이번 빠른 재터치/사망 modal의 새 touch 거절을 실제 Input System으로 확인했다. PC adapter는 유지한다.|
|21. 오른쪽 controls|공격 가장 큼, 상호작용/대시 보조. 80/100/140% Warrior/Explorer와 실제 근처 peer가 있는 Medic fixture를 확인했다. 치료는 중앙을 가리지 않는 class active 위치, 의미 선택은 표식 아래로 옮겼다. [Medic](resume-20261101/hud-mp-medic-en-844-size-1.4.png) · [Explorer 수정 후](resume-20261101/explorer-final-en-844-size-1.4.png).|
|22. Safe area|HUD/common tree와 Home을 축소 safe-area fixture에서 확인했다. Home58px-left/32px-right 모사에서도 잘리지 않았다. [화면](resume-20261101/after-home-simulated-insets-en-844.png). 물리 notch/gesture 기기 검수는 미수행이다.|
|23. Responsive|84개의 기본14화면×한영×1920/1280/844 기록, 바뀐 Home/Profile/Continue18+fixture2+badge/inset2의 최신22기록을 구분한다. 기록된 box 높이 잘림0. 가방128reference-slot/제작120높이/action/상자 좌우scroll/소분을 모바일에 맞췄고 내용·수량·예약/드래그 권위는 보존했다.|

## 24–30. 월드·회귀·보안·성능

|항목|최종 판단 / 한계|
|---|---|
|24. 물|Seed9472026의 lake/river/coast, 과거 continental 이미지와 sprite/material/filter/compression/history를 대조했다. 64unit/128pixel/Bilinear/1pixel border는 기존 continental 경로에 이미 있었다. 최근 회귀를 입증하지 못해 shader/지형/generation을 바꾸지 않았다. legacy generation의 다른 렌더를 blanket 복원하지 않았다.|
|25. 생물 외형|13동물+Zombie/Skeleton/검 variant/Goblin/Slingshot의18표현을 식별했다.17distinct prefab/import, 눈·facing·pivot·PPU·material/tint 및 history 확인. Goblin art/meta는 `4bee7d4` 이후 변경이 없어 확인한 눈/눈썹을 최근 회귀로 판단하지 않았다. 실제 잘못된 sprite 연결을 확인하지 못해 art를 임의 복구하지 않았다.|
|26. 이동|4.5–5.5초 controlled spawn/gallery와 death sample, King phase1 chase/windup을 관찰했다. post-death displacement0. Crawler는 추가 근접 fixture에서 camouflage→Warning→chase/Windup→Action→Recovery, 다리 해제/사망 정지를 확인했다. [원본 요약](resume-20261101/crawler-summary.json). 객관적 새 movement 결함이 재현되지 않아 이동/HP/speed/path 수치를 바꾸지 않았다. 코너/장시간/interpolation stress나 자연 진행 전부를 플레이한 것은 아니다.|
|27. gameplay 회귀|기존 crafting/장비/분류/chest touch scroll/실제 pointer navigation/skill/prerequisite/Save14·Continue 관련 검사를 단계별로 수행했다. QA fixture는 자연 progression으로 보고하지 않는다. 최신 source/data는 Item136/Recipe111이고 경제/전투/생존/Workbench 변화는 없다.|
|28. multiplayer 회귀|최종 실제 LAN은 client overlay=true, 한 HP, 로컬 joystick/layout/ClassControls; client Home, Host save/leave/reHost/Continue. raw failed first overlay evidence도 보존했다. 이번 수정은 UI audio여서 이미 통과한 두 peer 검수를 반복하지 않았다.|
|29. account/store/economy 보안|UGS/PGS/auth/deletion/ad eligibility/server verification/구매 token/지급·중복방지는 기존 경계다. 광고/IAP/Production OFF, 허위 상품/가격/인증값 없음. 클릭음은 서버 보상/구매 성공음이 아니고 실제 transaction cue가 기존 confirmed receipt를 따른다.|
|30. 성능·Audio|11-01 Home180frame: Transform170→170, Canvas.BuildBatch max0, main-thread p95≈2.8971ms, layout p95≈.0026ms, Editor GC median13417bytes. Android FPS/zero GC를 뜻하지 않는다. 오늘 발견한 Main127버튼 audio binding 누락은 공통 authoring에 AudioButton.Add를 적용하고 저장 씬에 복원했다. 실제 Settings click: 교정 전 cue0/voice0 → 후 UiClick1/voice1. transient Cancel에도 UiBack을 연결했다. 새 음원/매 frame Update는 없고 현재 mute/one-owner/30source/reload 검사를 통과했다. 이 audio listener 교정 후 전체 Profiler를 새로 측정한 것으로 주장하지 않는다.|

## 31–37. 증거·보호·남은 승인

|항목|내용|
|---|---|
|31. screenshots|[이전/중간 provenance](resume-20261031/screens.json), [최신 메뉴/HUD provenance](resume-20261101/screens.json), [이번 연결/사망/input](completion/screens.json). 구버전 after/잘못된 tiny frame/foreground 오류/죽은 player 물 fixture는 합격 증거에서 제외했다. 외부 reference 원본은 product art에 넣지 않았다.|
|32. KO/EN|기존 en/ko 정본과 actual Noto Sans KR. principal menu3viewports/상태/역할 HUD를 양언어로 확인했다. 이번 마지막 failure/death probe는 EN844이며 이를 추가 KO 전체 검사로 부풀리지 않는다. 이전 두 언어 해당 화면 증거는 보존했다.|
|33. 사용자 save|각 체크포인트와 오늘의 전후 SHA256/size/mtime을 비교한다. QA profile/save와 fake service만 사용하고 실제 user file을 변경하지 않는다. 종료 시 MainMenu/Edit/Play OFF, 임시 Editor helper 제거/QA flag clear.|
|34. version|Item136/Recipe111, Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3/IAP1, package com.gltam.lowfire 유지.|
|35. modified files|공통 UI builders/세 씬/입력·HUD·전환·화면 state·관련 tests/docs, 기존 art와 licensed navigation7개. 정확한 전체 패스 목록은 [files-changed.txt](files-changed.txt). 이번 추가 product 변경은 공통 button audio·transition cancel audio·MainMenu의 빠진127 binding 및 취소 뒤 stale Connecting 정리이며 관련 actual-controls 회귀 검사1개를 추가했다.|
|36. 남은 주관적 polish/미확인|사용자의 PC/Unity 디자인·가독성 승인, 실제 Android 엄지 reach/동시 touch/해상도·notch·오디오 청취·FPS, Relay/4physical peers/장시간 코너·interpolation stress, 실제 Google·광고·구매/외부 setup. 코드 수정으로 대신 승인했다고 하지 않는다. 확인한 범위에서 미해결 재현 P0/P1은 없지만 모든 가능한 버그 부재를 증명한 것은 아니다.|
|37. RC2|**NOT READY — 사용자 PC 검수와 별도 RC2 요청 전.** 본 UI pass를 마감하며 STOP. AAB, Console 업로드, Production/Ads/IAP 활성화, balance/new gameplay는 자동 진행하지 않는다.|

## 마지막 검증과 Git

- 오늘 PlayMode5/5(마지막 취소 상태 수정 후 재검사): 실제 Home navigation/transition Cancel sound, disabled category silence, mute/mixer, single owner/30sources/reload, real menu pointer navigation. [raw](completion/test-job-cf2026f8bb7642748e2ad47cd7469646.json).
- 오늘 EditMode4/4: Frontend2+SceneContract2. [raw](completion/test-job-6b95e7b5684c44c18683081dc825ec9c.json). ProjectDataValidator/소스 validator·최종 save/editor 증거는 completion/에 함께 저장한다.
- 앞선111Edit/67Play, 다음27Play,11-01의14Play/4Edit는 **각 단계의 겹칠 수 있는 검사 집합**이다. 합산한 전체 suite라고 하지 않는다. 당시 실패와 관련 재검사 결과는 REVIEW/raw JSON에 남겼다.
- 실제 마지막12개 QA check와 transport Error Pause 처리 기록: [처음12checks](completion/checks.txt) · [마지막 취소/실패4checks](completion/network-final/checks.txt). 처음 취소는 기능적으로 복귀했지만 상태 문구까지 통과한 것은 아니며, 마지막4checks가 이를 보완한다. 오늘 조회된 Scene Settings click의 이전/이후 cue·voice 결과도 completion/에 있다. 사람/기기에서 소리를 청취한 증거로 바꾸지 않는다.
- main 직접, PR/임시 branch 없음. 문서·씬 저장→diff 검사→commit/push→fetch→local HEAD==remote main 및 clean을 확인하고 최종 응답에 full SHA를 보고한다.
- 단계별 시간/사용량 한계: [completion/timing.json](completion/timing.json). tokenUsage=null은 도구에서 작업별 실제 token/billing 수치를 제공하지 않는다는 뜻이며 추정치가 아니다.
