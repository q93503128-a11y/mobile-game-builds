# Lowfire 음향 정본

2026-10-30 수명 교정: 씬 전환 시 이전 encounter의 victory 예약·intro 유지 시간·duck timer를 초기화한다. 메인 메뉴/Continue에서 이전 전투의 예약음을 새로 재생하지 않는다. 영속 owner·source 수·기존 crossfade는 유지한다. [기술 검수](../validation/technical-health-vfx-correctness/REVIEW.md).

Foundation5 현재 상태: **구현·저장소 검증 완료**. 초기 무음 상태와 음원 준비104개 중간 저장은 REVIEW의 과거 기록이다. 현재는 **Music6 / Ambience6 / SFX89 =101파일**, **74개 cue**, Resources AudioBank와 실제 AudioMixer/런타임 연결을 제공한다. 지원되지 않는 나무 바닥 발소리 후보3개는 제거했다. 새 기능·재료·제작법·직업·경제 규칙은 추가하지 않았다.

## 소리의 방향과 출처

조용한 숲, 자연·물리 재질, 절제된 종이/가죽/목재 UI, 탐험 음악과 보스의 분명한 대비가 원칙이다. 이전 sine action.wav/생성기는 복구하지 않는다. 참조6게임 비교는 [REVIEW](../validation/audio-production/REVIEW.md), 작가/원본 링크는 [AUDIO_LICENSES](AUDIO_LICENSES.md), 파일별 원본·mirror commit/hash·편집·측정은 [assets.json](../validation/audio-production/assets.json)이다. 모두 CC0 source이며 새 AI 생성 음원은 없다. 원본은 배포/Git 밖에 보관한다. 실제 기기 청취 승인은 별도다.

## 음악·환경

| 상태 | Music 파일 | 환경 | 전환 조건 |
|---|---|---|---|
| Menu | menu | 낮 숲, 낮은 gain | 세계 없는 Home/메뉴 |
| Day | day | 낮 숲 + 드문 새 | 지상 낮 |
| Night | night | 밤 곤충 | Evening/Night |
| Cave | cave | cave | 실제 Continental 지하 |
| Deep | cave 공유 | deep_cave | Cave room Depth≥0.6 |
| Graveyard | graveyard | graveyard | 실제 묘지 anchor16m 이내 |
| Boss | boss | graveyard 공기 | 활성 보스35m 이내/가까운 실제 intro |

보스 우선, 다음 지하/묘지/시간 순이다. 일반 적마다 전투곡을 시작하지 않는다. 0.3초 context poll/3초 안정화로 경계 흔들림을 억제한다. 음악은 두 source, 환경은 두 source를 교차시킨다. volume slew parameter는 탐험1.5/보스0.6으로, music target0.7까지의 전체 gain ramp는 약1.05/0.42초다. 같은 곡인 Cave→Deep이나 같은 Menu 재로드는 곡을 다시 시작하지 않는다. Pause/Downed/Defeat 음악은0.32배, 가까운 telegraph는 잠시 추가0.5배다. 실제 보스 Dying에서 음악을 내리고 DeathSeconds+LootPause 후 victory를 예약한다. 정상 지역 복귀 시 일반 음악으로 돌아간다.

새 호출은16–34초 무작위 간격/긴 cue cooldown이며 보스/지하에서는 자연 호출을 하지 않는다. 가까운 실제 Campfire 하나를10m 감쇠+pan으로 겹친다. 합성 source wind1은 PureData로 제작된 기존 CC0 asset이며 현장 녹음으로 표시하지 않는다.

## Mixer·설정·수명

`Master → Music` 및 `Master → SFX → UI / Ambience`. 기존 세 slider/저장 key만 사용한다. 기본 **Master100% / Music65% / SFX85%**, 기존 control/language/reset 유지. 선형 slider→20log10 dB, 0은−80dB와 해당 AudioSource.mute를 함께 적용한다. 설정 로드/언어 변경/동기화 자체는 무음이다. GameAudio는 씬 소유 facade, `Lowfire Audio` 하나만 DontDestroyOnLoad다. camera listener는 하나다.

Oneshot25개(UI4/impact8/creature5/footstep4/critical4)+loop5개=**고정30 AudioSource**. 사전 할당 source/array를 재사용하며 타격마다 Instantiate/scene scan/receipt 문자열을 만들지 않는다. 우선순위와 category별 예약, cue cooldown, 변형의 즉시 반복 방지, 2D 거리 gain/pan을 사용한다. impact0.045초, step0.055초, UI0.09초, creature0.35초, bird8초가 기본이다. 보통22m, step11m/creature18m/critical38m이며 35m 보스 음악 범위는 별도다. 최대25 oneshot을 넘는 요청은 교체/억제한다. 모든 소리가 매 요청마다 재생된다는 보장은 의도적으로 하지 않는다.

## 연결한 행동

| 영역 | 실제 경계 / cue |
|---|---|
| 조작 | 실제 이동 거리, 정지/teleport 억제; 지상 grass/rock 및 지하 stone; 실제 dash/장비 선택 |
| 채집 | Health.Damaged의 wood/stone/ore/plant hit와 break, 실제 pickup |
| 제작·건설 | 결과 확정/processing 시작·완료·collect, 성공 place/upgrade/repair/dismantle, 실제 door edge/storage open |
| 생존 | 성공 food/drink/treatment, cast/bite/성공 catch, 실제 hurt/downed/death/recovery |
| 전투 | 승인된 swing/blade, 실제 draw/release/ammo commit, projectile collision, flesh/bone/chitin/body fall |
| 생물 | undead/goblin/slime/beast/wolf/bat/insect + sparse bird; 비공격 동물을 괴물처럼 울리지 않음 |
| 보스 | 새 Awakening, phase 증가, fan/line/circle warning, strike/slam/charge/ritual, hurt/death/victory |
| UI·메타 | 실제 Button click/back, category/tab/board page, journal; 성공 class/Engineer, achievement, attendance, Gold/Gem |

UI hover/scroll/HP 매 frame/일반 XP tick/save/cloud sync/재접속 초기 snapshot/자연 despawn은 의도적 무음이다. 별도 low-health 음향 alarm과 존재하지 않는 wood-floor 판정은 만들지 않았다. 원거리 bare-wall projectile hit는 기존 복제된 impact event가 없으므로 원격 despawn을 피격으로 꾸미지 않는다. 제3자의 건설 성공을 들려주기 위한 전용 RPC도 추가하지 않는다.

## 멀티·보상·외부 화면

발소리는 실제 복제 위치, 공격/활동/적·보스는 기존 sequence/state/started를 관찰한다. cold frame을 먼저 prime하고 Host에서 local+echo를 두 번 재생하지 않는다. food/drink/treatment는 기존 owner Growth JSON에 optional `ActionSequence/Action` semantic receipt 두 필드를 더했다. clip ID나 Audio RPC가 아니며 소비 확정 때 owner snapshot update 한 번이 추가될 수 있다. 따라서 **네트워크 비용0이라고 주장하지 않는다**. 과거 JSON은 기본0/None으로 읽고 재접속 첫 snapshot/같은 sequence는 재생하지 않는다. Save/Protocol wire type은 바꾸지 않는다.

Gem은 유효한 새 foreground 서버 지급/완료 결제에서만 재생한다. balance refresh, pending/취소/실패, background recover는 Gem 보상이 아니다. 세션의 bounded256 receipt와 기존 서버/거래 journal idempotency를 함께 사용하며 이 cache는 재화 권위가 아니다. Gold와 world Crystal은 Gem 음원과 분리한다. Native ad/billing overlay 및 앱 background는 game listener를 suspend하고 finally에서 복원한다. 외부 SDK 광고 자체의 소리·실기기 audio focus는 이번 공개 활성화 검증 대상이 아니다. Ads/IAP 운영 기본OFF를 유지한다.

## import·비용·검증

SFX는 mono32kHz/ADPCM/DecompressOnLoad, bed는 stereo32kHz/Vorbis/Streaming이며 Android override도 같다. importer normalize는 off다. 최종 source 파일 **13,591,135bytes**; hash 중복0, 모든101 asset은 bank에서 참조된다. 가장 큰 day는2,836,675bytes/234.856초다. decoded 모든 음악을 상주 PCM으로 합산하지 않는다. 실제 Editor clip 합계 메모리 **3,150,277bytes**는 Editor 측정이며 Android 상주 메모리 보장이 아니다. 실제 AAB 증분은 측정하지 않았다.

실제 non-nographics Unity 행동/보스/Continue에서 출력 buffer **13448개**, peak **0.381110**, RMS **0.022080**를 측정했다. 이는 사람의 청취 승인과 다르다. 실제2프로세스 LAN은 headless transport/event 검증이다. 30분 검사 상태: **실제1800초 이상 연속 출력 통과**. 상세 결과와 한계는 REVIEW 및 결과 JSON을 따른다.

Android Alpha/Production script compile **59/59 assemblies 통과, development fixture 제외**. native SDK/manifest/Gradle 변경이 없어 전체 AAB는 이번에 만들지 않았다. Save14/Profile2/CloudProfile1/Economy2/Protocol23/generation3/IAPSchema1 유지. 사용자 save는 QA isolated directory와 분리하고 전후 fingerprint로 확인한다. Foundation6/Final Polish/출시 작업은 시작하지 않는다.
