# Launch Foundation3 · Rewarded Ads / Meta UI

2026-10-27 시작 SHA `7d48ceba336d6e6c6f2a51aa0812376d4bcf3678`. remote fetch/main 일치/clean 확인. **사용자 시간 제한에 따른 중간 체크포인트이며 Foundation3 전체 완료가 아니다.** 이번 코드·격리 검증 결과를 main에 보존하고 남은 작업은 RESUME.md로 이어간다. 기존 가격/Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3 유지. Production live OFF, 실제 AdMob/CloudRun/SSV/기기 설정 미완료. 최종 Foundation3 종료 SHA는 아직 미정이며 이번 checkpoint SHA는 최종 응답/git log에 기록한다.

## 조사 품질과 실제 capture

현재 실행에서 격리 Unity가 생성한 before/의 KO/EN·1920/1280/844 capture를 열어 확인했다. profile-ko-844, monthly-ko-844, achievements-en-844에서 계정설정과 보상진입 혼재, 작은 text, 중앙 제본을 가로지르는 header와28칸, family의 다음 진행/보상 누락이 보인다. 가짜 SDK 성공으로 보고하지 않는 시각 fixture다. 외부6게임 실제 saved screenshot도 직접 열어 확인했고 reference-images.json에 source URL/hash/출처시기를 남겼다. 외부 pixel은 Assets/Git에 넣지 않는다. 현재 provider/운영 정책과 오래된 UI 예시를 혼동하지 않는다.

## Provider 비교 후 선택 · 2026-10-27

|Provider|실제 공식 사례/지원|검증·test·consent|초기 복잡도/크기·운영|Lowfire 적용/배제 이유|
|---|---|---|---|---|
|[Unity LevelPlay](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/rewarded-ad-integration-package)|Unity package8.5+ RewardedAd object/listener, Android와mediation.9.x 관리 중|[S2S](https://docs.unity.com/en-us/grow/levelplay/platform/settings/server-to-server-callback)는 back-end wallet에 권장, app/adunit/dashboard·privacy설정 필요|mediation과publisher도구 강점; network adapter 추가마다 dependency/앱크기 증가. 미측정AAB숫자는 제시하지 않음|비교했으나 기본 선택 아님. Unity게임이라는 이유로 정하지 않음; 현재2placement/단일network에 mediation복잡도 불필요|
|[Unity 공식 monetization 경로](https://unity.com/solutions/app-monetization)|LevelPlay mediation과Unity Ads demand를 각각 제시|Unity Ads client event만으로Gem을 지급하지 않음|IAP는 독립 제품이며 이번IAP추가 불필요|기존Advertisement Legacy를 새로 택하지 않음; 권장ecosystem 확인과 primary provider 선택을 구분|
|[Google AdMob Unity](https://developers.google.com/admob/unity/quick-start)|C# Unity2019.4+·Android24+/target35+, [11.6.0 release](https://github.com/googleads/googleads-mobile-unity/releases/tag/v11.6.0)2026-10-13 확인|[ECDSA SSV](https://developers.google.com/admob/unity/ssv), fixed Google keyserver/rotation, official sample test units, [UMP](https://developers.google.com/admob/unity/privacy) Update/CanRequestAds/privacy-options|단일network로 시작 가능, mediation나IAP필수 아님. unitypackage원본1,905,990B는 다운로드 크기이며 APK증가량 아님. 기존EDM과 중복설치 안 함|**primary production adapter 선택**. public-key SSV로 provider shared secret 불필요, 공식test ID·Android/consent문서·최신pin과2placement에 적합|
|[AppLovin MAX](https://support.axon.ai/en/max/getting-started)|Unity/Android Rewarded와mediation/integration manager|[공식S2S FAQ](https://support-preview.axon.ai/en/max/faq/how-server-to-server-callback-works):test ads에는S2S없음, EventKey+SHA1 token. testmode별도|다수network 운영에 강점; app등록·SDK key·EventKey 및adapter설정|primary로 미채택. Alpha test-only에서S2S실행 검증은 live와 구분해야 하며, 현재단일network2placement에는운영부담추가|

Google11.6.0 SDK/공식API를 pin하고 기존EDM을 재사용한다. SDK별 실제 device callback·no-fill·지역consent는 문서만으로 성공이라 하지 않는다. 최소API상승이 필요하면 SDK요구와 실제Android검증을 기록한다. 개인정보/아동대상/동의의 법적 상태를 추측하지 않으며 reviewed 설정·UMP 허용 전 ad request0을 기본으로 한다.

## 실제 UI reference 비교

|게임/출처|실제 사례/발견한 원칙|Lowfire 현재 문제|적용|배제와 이유|
|---|---|---|---|---|
|[Mine Survival 개발자 screenshot](https://wildsoda.blogspot.com/2020/10/mine-survival-survival-crafting-zombie.html), [현재GooglePlay](https://play.google.com/store/apps/details?id=com.WildSoda.MineSurvival&hl=en_US)|2020실제 crafting/inventory는 world가 남고 survival icon·동작영역이 구분됨.2026listing2.9.03 release note에ad delay indicator 추가 확인; 최근timer모양까지 본 것으로 주장 안 함|계정메타가중앙큰paper에설정까지섞이고 status행이작음|Play action 우선,한개secondary Profile/Records hub; unavailable/loading/실제retry상태 명시|art/recipe/HUD내용복제·Banner/Native광고는 채택 안 함|
|[Westland 공식28일캘린더](https://support.westland-survival.com/hc/en-us/articles/360019284358-Daily-rewards)|7×4타일마다그림·수량,DELIVERED stamp/currentstate/nexttimer.14일신규와28일성장별cycle|28칸이 제본과 겹침; 일반출석기록도 보상처럼 보임|single paper field-sheet7×4,24stamp vs4Gem milestone,상태색+도장+text.7일Day7강조|일별실물item/성장별보상표추가 없음. 기존보상값유지|
|[Last Day on Earth archived reward menu](https://www.techtudo.com.br/dicas-e-tutoriais/2020/04/dicas-de-last-day-on-earth-jogo-de-tiro-e-sobrevivencia-para-celular.ghtml), [공식광고FAQ](https://kefirgames.helpshift.com/hc/en/5-last-day-on-earth/faq/58-why-can-t-i-receive-energy-for-watching-ads/)|2020실제reward list는section nav/item/수량/action을분리.현재공식FAQ가ad reward availability testing한계를설명|Profile에로그인/지원/모든보상action이함께노출|계정설정별도,hub→Achievement/Attendance 명확한단계; 실패상태무료Home유지|타이머형유료guide/energy gate/coin상점미도입|
|[Grim Soul 공식7.4.0 screenshot](https://brickworksgames.zendesk.com/hc/en-gb/articles/28550759444637-What-s-new-in-update-7-4-0)|milestone상단과현재progress/claim 구분,완료대상highlight|family목록은stage수만있고next목표/보상이없음|family card에현재/다음threshold/다음Gem/단계,완료행은detail;작은toast/badge|DailyQuest/ScarletHunt/pass/bigglow는안만듦|
|[Survivor.io/Habby 공식screen](https://store.habby.com/game/3)|DailyDiscount첫card는+10Gem/coins를먼저보이고Ad Free·1ChancesLeft를표시|optionalad의reward/remaining/계정조건을읽을영역없음|Profile rewards의작은+1Gem CTA,명확한remaining와실제상태|네온/400%가치/paidpack/IAP/red-adbadge복제안함|
|[Stormfall archived screenshot](https://igrofania.ru/katalog/stormfall-saga-of-survival/)|2023실제7daystrip,Day1check,Day3큰milestone·전체7일한화면|신규7일의Day7강조가약함|7일한row/Day7명확한12Gem milestone,claimedseal|공격적offer/새story/풍선보상/단순픽셀복제안함|

Archero1자료는경제보조비교로만사용한다. Archero2 screenshot을봤다고하지않는다. 실제scene캡처에근거한 Lowfire문제→위principle→최종레이아웃을비교하며외부screenshot은상용asset로가져오지않는다.

## Gem 정책 · 조사/계산 후 확정

외부수치는환율이아니다. Habby공식DailyDiscount screenshot의ad10Gem·1Chance, [Archero커뮤니티wiki](https://wiki-archero.luhcaran.fr/en/wiki/menu/)의DailyAdPack30Gem/day, LDoE의energy/resource형광고를비교했다. Archero본문조회일부timeout으로현행2026수치보장에는쓰지않고보조참고로표시한다. Survivor.io/Archero는반복소비·다양한sink가있고Lowfire는Engineer1회120와부활8뿐이라금액을이식하지않는다. 숫자가공개되지않은게임의dailycap을지어내지않는다.

정책: **GemPerRewardedAd=1, DailyRewardedGemLimit=1Gem(1회)/서버UTC day, CooldownSeconds=0**.1/3/5/10ads/day uncapped감도와실제cap적용30/90일을같이보인다.1회/day만으로30/90추가Gem30/90,Engineer25%/75%,부활3.75/11.25회분이다.3/5/10회라면월90/150/300Gem이라한번뿐인Engineer와적은sink에과도하고월간40을압도한다. 실제cap1의월30은반복월간40보다작으며광고는recurrent공급의43%이다. 모든무광고보상·가격은그대로이고신규45+완료한월간2회80=125로광고0/업적0이어도Engineer120이가능하다.

기기시간은보상권위가아니다. server가show slot의UTC day와보상값을고정하며자정을넘는SSV/retry는그예약day권리를보존한다.31일등달력효과와F2실제행동가정은별도계산한다. revive ad는Gem을만들지않으므로피한8Gem소비는통화공급과분리한다.10ads/day보도록요구하는진행/신규sink0,adavailablebadge0,autofullscreen출석popup0.0cooldown에서는가짜timer가없고실제SDK load/retry또는serverdayreset만안내한다.

## 구현 경계 · 구현 전 설계 확정

* 정확히 rewarded_gem/rewarded_revive. Runtime은IRewardedAdProvider,GoogleSDK는AndroidAds assembly한곳. official SDKearned callback만completion이며closed/cancel/failure는보상0. Development provider는격리Development전용/release제외.
* Gem: recoverable account/online/consent/servereligibility를show전에확인. server operation+nonce를durablelocaljournal에기록→SDKcustom_data/user_id→SSV. provider-signedreceipt가검증된뒤에만Economy2 rewarded_ad CAS/영구source·전역ownerbinding.완료callbackbool은Gem지급권위가아님.
* AdMob callback은외부HTTPS GET이필요하고UGSCloudCode는authenticatedPOST다([ClientAPI](https://docs.unity.com/en-us/oas-cloud-code-client/1.0.0)).canonical ingress/worker를제공하고실제GCP계정/URL/UGS서버credential은설정전비활성. [CloudTasks](https://docs.cloud.google.com/tasks/docs/creating-http-target-tasks) durablequeue에접수한뒤만providerack,OIDC worker인증·재시도·서버SecretManager의relay인증을사용한다.동적가짜callbackURL/secret을만들지않는다.준비되지않으면Gemad show자체를막는다.
* 실제SDK/SSV/queue와메모리fixture를구분한다. publicGoogleECDSA검증키와원본encodedquery를검증하며서명하지않은clientreceipt를providerproof라하지않는다.지급unknown은serverop/receipt·localpending으로recover,중복기기는한CAS만credit.개인정보삭제inventory에는전역adreceiptbinding도포함한다.
* Guest SP revive는공식SDKearned completion을사용할수있다. localworld권위상clientmodified공격을완전히막는다는주장은없고Gem서버grant와분리한다.현재Run:death:ad에묶고성공한원정1회·40%HP·기존보호/안전위치/Save14 원자저장을유지.실패save는기회를소모하지않고earnedpending을보존,Home/새run에latecallback은사용불가.연결된계정의지원진단은client-reported임을표시하며재화권위가아니다.
* Alpha는공식testunits/SDKtestguard만,Production은ID가있어도명시activation전OFF. child/consent/privacy검토미설정은failclosed. UMP Update/CanRequestAds/privacy-options를adapter한곳에서처리하며법적상태를가정하지않는다.
* Profile/Records hub와accountsettings를분리하고Class Journal은별도. Achievement와Attendance는같은paperpalette의singlefield sheet를사용해제본과텍스트겹침을제거.새광고giantmainbutton/store/현금상품0.실제화면3viewport/KOEN검수전완료로표시하지않는다.

## 체크포인트 요구사항 매핑 (전체 완료 아님)

|항목|실제 구현·검증·제한|
|---|---|
|1 시작 SHA|7d48ceba336d6e6c6f2a51aa0812376d4bcf3678; 작업 전 fetch/clean/main 확인|
|2 종료 SHA|Foundation3 미완료. 이번 중간 commit은 git log 및 최종 응답; 다음은 RESUME.md|
|3 provider 비교|위 LevelPlay/AdMob/MAX/Unity 공식 route 비교표, 공식 SSV·test·consent·운영 차이 확인|
|4 선택|Google AdMob, 단일 network/공개키 ECDSA/공식 Unity Android SDK|
|5 SDK|11.6.0 official unitypackage; native ads25.5.0/UMP4.0.0; 원본 binary11개 SHA 일치|
|6 placements|rewarded_gem / rewarded_revive 정확히2개; banner/interstitial0|
|7 Gem reward|1Gem, client callback만으로 지급0|
|8 daily cap|서버 UTC1Gem/day; failed show/cancel은 claim 소모0|
|9 cooldown|0초; 실제 load/pending/reset 상태만 표시|
|10 economy|1/3/5/10 uncapped30일30/90/150/300; adopted30/90일최대30/90, model JSON/CSV|
|11 ad-free|신규45+완료월간2회80=125로Engineer120 가능; 유저행동 가정 명시|
|12 revive|기존 원정1회·40%HP·보호·안전위치. native 격리 실제 world save 실패→earned proof 재시도→한 번만 저장 검증|
|13 proof/SSV|Google ECDSA exact encoded query + fixed keyserver; durable task/HMAC/OIDC relay. 실제 Google/provider 콜백은 미실행|
|14 Economy2|같은 CAS ledger rewarded_ad와영구 source/global receipt owner; archive duplicate test 통과|
|15 retry|계정·환경 journal/이전 날짜 pending queue, reserved slot/nonce, verified server intent, unknown CAS retry|
|16 Alpha|공식sample unit만. fixture type은 Alpha/Production compiled runtime에 없음|
|17 Production|client/server/ingress live OFF, 실제unit/개인정보검토·명시activation 필요|
|18 privacy|UMP Update/CanRequestAds/required choices 구현. 실제 audience/URL/지역 consent·Google Cloud log 정책은 설정 필요|
|19 Mine Survival|개발자2020 실제screen + 현재Play2026 ad-delay-indicator release note; 최신timer모양을봤다고 하지 않음|
|20 다른 게임|Westland/GrimSoul/LDoE/Survivor.io/Stormfall 실제screen5개 추가 검토. Archero2미열람은카운트안함|
|21 Main Menu|현재6entry/Play우선. 잘못된1280/844 Profileoverlay 캡처는 rejection 기록 후 정상target 재캡처|
|22 Profile|프로필·기록hub/선택Class·레벨·Alpha·Gem·기록summary→업적/출석, 계정설정하위. Class Journal 별도|
|23 achievement|가족별달성단계+현재/다음목표/Gem, 스크롤상세/수령. 새actionfont32; detail reachable 검사|
|24 attendance|single paper, 신규7일/월간7×4/event분리; no-event state|
|25 monthly|24일 visit stamp, 7/14/21/28일 Gem6/8/10/16. claimed olive+seal/current ember/future muted|
|26 badge|업적/출석claimable만 작은dot; 광고availablebadge/자동전체출석popup0|
|27 identity|기존lowfire_journal/seal/ink/ember; 외부참고art를Assets로복사0|
|28 captures|Unity native1920×1080/1280×720/844×390. UI-only controlled background, 지정scale/safe area. 마지막visual/checks.json PASS|
|29 KO/EN|Hub/업적/신규·월간과광고available/loading/cancelled/unavailable/pending/received 모두typedfixture캡처. 작은화면fontcache문제발견후generator invalidate로교정|
|30 tests|최종Node124/124, EditMode17/17, PlayMode26/26. 마지막native visual은최신retry/저장실패/부활시나리오 포함. 별도LAN명시test 잘못선택run은중단하고성공으로세지않음|
|31 Android|최신Alpha58/Production58 assemblies C# compile PASS, fixture 제외. F3 Gradle/AAB/실기기 video는미실행|
|32 actual/fixture|실제UGS SDK default-OFF/위조bool 거절 PASS, fresh anonymous QA player 빈data확인후삭제. 실제PGS/AdMob video/GoogleSSV/queue/cloudcontainer는미실행|
|33 external setup|ADS.md12단계:사이트/메뉴/App/unit/callback/권한/secret/test/activation. 가짜ID·URL·secret없음|
|34 versions|Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3 유지. 광고journalSchema1 additive; 아이템/recipe경제수치불변|
|35 save protection|canonical/client/UserData + live/UserData 2파일 SHA/mtime/bytes 전후완전일치. Live Unity sync는 이번checkpoint에서안함|
|36 modified files|modified-files.txt, official SDK/license + backend source/ingress/support/tests + runtime/Android adapter/UI scene/builders/localization/docs/evidence|
|37 remaining|전체code/security/validator 최종완결감사, 최신클라이언트계약검사잔여, 실기기·Alpha owned-unitSSV guard계획/외부운영, Live sync. RESUME.md에순서기록|
|38 F4 readiness|영구receipt/ledger/운영경계재사용가능. IAP구현·provider purchase receipt검증·Store상품/오디오0. F3완료전F4시작금지|

## 이번 검증의 한계와 수정 근거

초기 visual에서출석reward두번째줄잘림과paper밖status를발견해행높이·footer위치를수정했다. 다음실제PNG에서claimed/current/future·모든Gem수량·status가paper안에보임을확인했다. 작은해상도KO Latinprefix가잘리는fontcache문제는textgenerator를scale전환때명시invalidate한최종캡처에서정상화했다. native UI-only render와실제상용광고영상을혼동하지않는다. 마지막visual은월간·Hub·loading/pending와SP earnedproof world-save-failure/retry/AdUsed Save14/무료Home reset을검사했다.

Node tests는124개PASS. nativeEdit17개/Play26개PASS는직접실행기록이며,후속작은UIretry-label·nativefullscreen gate는마지막Android/visual로검증했다. 별도클라이언트없이실행된명시LANtest선택실수run은완료로카운트하지않는다. 새로운2프로세스LAN/실제AndroidnativeGradle/device/provider 광고는이번체크포인트에없다. Production변경0,실제player save변경0,Live Unity동기화0.

## 시간·사용량 기록

`timing.json`, `process-history.json`, 각 `*-process.json`은UTC와실측경과시간을기록한다.병렬QA시간을합산하지않는다.초기fetch/조사일부와모든과거재실행타이머가완전하지않으므로전체작업시간을정밀측정한것으로단정하지않는다.작업별token/API사용량수치는도구에서제공되지않아null이며추정배분하지않는다.

|마지막 검사 단계|실측초|
|---|---:|
|ads-edit-final|40.657|
|ads-play-final|302.154|
|visual|119.249|
|android|120.206|
|rewarded-actual|62.819|
