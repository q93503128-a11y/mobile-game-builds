# Lowfire 1.0 · Launch Scope

2026-10-25 기준 인게임 범위를 잠근다. 현재 실제 정본은 Item136 / Recipe111 / Save14 / Profile2 / Protocol23 / generation3다.

## 1.0 인게임

생존 시작 → Workbench I → Iron/Copper/Tin/Bronze → Workbench II → Steel → Caverns/Deep Caverns → Workbench III → Diamond/Crystal → 6Class 성장 → Graveyard → Undead King.

현재 biome/cavern/deep cavern, 생존·제작·건축·방어·생물/적, Death/Revive, 1~4인 협동 경계, 기존 Accessory를 포함한다. 검증하지 않은 실제4인/Relay/모바일 품질을 출시 완료로 간주하지 않는다.

## 출시 전 외부 시스템

이번 Foundation1: Account, Cloud Account Profile, Alpha 참여/영구 entitlement 이전 기반, 운영·문의·개인정보/삭제 hook. 외부 설정은 SETUP_REQUIRED와 RELEASE_BLOCKERS를 따른다.

Foundation3에서 선택형 Rewarded Ads 서버/SDK 기반과 Profile·업적·출석 화면을 연결했다. 광고 기본 OFF와 실제 provider/SSV·기기 외부 설정 차단을 유지한다. 다음 별도 패스는 IAP/Store, Audio, 최종 출시 polish다. Alpha와 현재 Production의 현금 결제는 코드·서버 양쪽에서 항상 거절한다.

## 후속 업데이트

Mount, WB IV–VI, Boss Core 사용처, 추가 Boss/대형 biome, Defense Objective/Day10 major event, Electricity/Guns, Pet/Breeding/Merchant/Farming, Story/Permanent Skin, Recall/Bedroll/Backpack, 본격 Status Effect content.

이 목록은 이번 패스 구현 목록이 아니다. Foundation1 완료 후 다음 사용자 지시까지 멈춘다.

Foundation1.5는 development/alpha 서버 기반을 실제 배포·검증했으며 인게임 범위는 그대로다. PGS/Google·Privacy·실기기/production 출시 gate는 남아 있다. 다음 Foundation2는 별도 지시 후 development 서버 기반에서 시작할 수 있고, 이번 패스는 여기서 멈춘다.

## Launch Economy Foundation2 · 현재 경계

Foundation2는 계정 Gem/업적/출석/Profile·Engineer120Gem·SP부활8Gem을 이번 범위로 구현한다. Foundation3 광고 기반은 별도 ADS.md를 따른다. IAP/Store 현재 상태는 아래 Foundation4를 따르며 Audio/최종 polish는 자동 진행하지 않는다. [경제 전체 규칙](ECONOMY.md) · [실행·검증](../validation/launch-economy-foundation/REVIEW.md).

## Foundation3 현재 상태

보상 광고2placement·Economy2 verified receipt·프로필/업적/출석 기록지 구현. Google AdMob11.6.0, client/server/production live 기본OFF. 실제 Android video/consent/SSV/Cloud Tasks 외부 설정과 출시 gate는 남는다. Foundation4를 자동 시작하지 않는다. [광고 정본](ADS.md).

2026-10-27 사용자 시간 제한으로 Foundation3는 중간 체크포인트에서 멈췄다. 전체 완료가 아니며 [이어받기](../validation/rewarded-ads-meta-ui/RESUME.md)를 따른다. 전체 완료 전 IAP를 시작하지 않는다.

2026-10-28 Foundation3 재개 마감: **저장소 구현·관련 검증 완료 / 광고 운영 미활성**. 위10-27 중간 체크포인트는 당시 기록이다. Node131/131·focused Edit9/Play7·data/Android58/58·native Development AAB packaging·90 typed UI capture·실제 live MCP 격리 확인을 마쳤다. 실제 광고/SSV/기기/개인정보·외부 운영 gate는 유지한다. [최신 마감 기록](../validation/rewarded-ads-meta-ui/REVIEW.md#2026-10-28-재개-완료). 다음 기능은 자동 시작하지 않는다.

## Foundation4 현재 상태

Profile 하위 Gem Store, 제안24/72/144 consumable, Google 서버 검증·Economy2 지급/consume 복구·환불/지원 기반이 현재 범위다. Alpha 실제 구매 금지·Production 기본OFF, 신규 sink/직업 현금 판매/구독/아이템 판매 없음. 외부 설정·실기기 Google 결제와 공개 출시 승인은 남는다. [IAP 정본](IAP.md) · [검증](../validation/store-iap/REVIEW.md). Foundation4에서 멈추며 Audio/Foundation5는 자동 시작하지 않는다.

## Launch Foundation5 — Audio

구현·저장소 검증 완료. Music6/Ambience6/SFX89, 실제 Mixer·행동·보스·확정 보상·멀티 presentation과 기존 세 음량 설정을 연결했다. [AUDIO](AUDIO.md) · [출처](AUDIO_LICENSES.md) · [검증](../validation/audio-production/REVIEW.md). Save/Protocol과 운영 결제·광고 OFF 유지. 실제 Android 청취/운영 광고·결제 audio focus와 공개 출시 승인은 별도이며 Foundation6는 시작하지 않는다.
