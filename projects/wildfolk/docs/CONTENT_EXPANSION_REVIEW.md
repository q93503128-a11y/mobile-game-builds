# Workbench I–II Content Expansion 검수

2026-10-18. 구현·관련 검사·실제 Unity MCP 검수 완료. Git 전달은 이 파일을 포함하는 main 이력에서 확인한다. [상세 증거/실패 이력](validation/content-expansion.json), [대표 Game View4장](images/content-expansion/). 재료 지급/순간이동/시간 전진/AI 정지는 격리 fixture이며 자연 진행 전체 완주가 아니다.

## 자동 검사

EditMode 최신 개별123/123, 관련 PlayMode44/44. 통합 검사 후 실패/변경 사례만 좁혀 재검사한 집계이며 마지막 단일 전체 실행 결과가 아니다. NUnit runner 합계369.71초이며 Editor 시작/임포트/MCP 대기는 별도다.

최초 Edit7건은 신규 count·Lantern·Recipe 분류·자원 kind/배치 검증 조건이었다. 최초 Play22건 중 공통 원인은 NetworkPlayer prefab의 Primary 참조 누락으로 canonical builder/prefab을 교정했다. Cooking Pot의 공짜 초기 시설 편입도 막아 기존5개를 유지했다. 이후 기존 fixture의 단일 Campfire recipe 가정·옛 식물 드랍/ID·수명이 있는 동물 target·unthrottled frame 대기를 수정했다. 별도 Host 실행이 겹친 포트 충돌은 live Host 종료/포트 확인 후 최종 검사를 진행해 해소했다. 실패 이력을 JSON에 보존한다.

마지막 Host 사례에 실제 NetworkProjectile spawn/서버 상태/탄약 한 번 소모 검사를 추가해 해당 사례만 재검사했다.

최종 Console/compile 오류0, MainMenu/Play 중지/비포커스. 원래 두 scene의 기존 객체/component ID 제거0. 전체 PlayMode 검사를 모두 반복했다는 주장은 하지 않는다.

## 실제 MCP 동작

- Small Plant→Fiber와 Berry/Mushroom 실제 Harvest를 확인했다. UI 카드로 Bow/Arrow/Rod/Spear 제작을 실행했고 Wood/Stone/Iron Spear 피해12/20/29를 확인했다. Bow는 사이에 둔 제작대에 막혔으며 막히지 않는 위치에서55→41 명중했다.
- SP/Host 자연 물 cast→입질→catch와 즉시 중복 보상 거절. 입질 아이콘/안내문을 보강했다.
- Campfire Cooked Fish/Roasted Mushroom, Cooking Pot Mixed Stew 예약·수령·섭취로 Hunger10→75/Thirst10→28을 확인했다. 다른 Stew는 data/공유 처리기 검사로 검증했다.
- Waterskin 채우기→정수→3회 사용→네 번째 거절→보충. SP Load의Clean2/3과 Host 재Host의Clean1/3을 확인했다.
- Bow1→Reinforced Bow1에서 원래 Bow가0으로 소비됐다. 고급 Arrow/Bolt 묶음, Crossbow 발사/즉시 재발사 거절, 기본 Bolt12→11/Steel Bolt8→7을 확인했다.
- Local Host에서 신규 설치물13종을 기존 건축 요청으로 모두 배치했다.7개 Door/Gate는 Host 열기 후 Barrier 해제를 확인했다. 같은 revision의 중복 설치는 거절됐다. Trap 실제 피해16,0.45초 내 추가 피해 없음/trigger1.
- Host Cooking Pot 처리·음식 사용과 시설19개 저장→종료→재Host를 확인했다. Lantern/물부대 상태와 최초 Gate ID638f667879104594b48d65c5189d2847을 유지했다.
- Lantern을 착용하고 Sword/Pickaxe를 들고 심부 동굴을 확인했다. ambient.18/착용광원 강도.738/활성 chunk9/resource89. 적에게 사망한 정지 SP fixture 캡처는 증거에서 제외하고 Host에서 게임 시간을 잠시 멈춰 살아 있는 광원 화면을 다시 확인했다. Lantern에 적 면역/새 AI 효과는 없다.
- 1920×1080/1280×720/844×390. Utility 슬롯이 힌트와 겹친 문제를 세 슬롯 한 줄로 교정했다. 물부대 설명 영역과 탄약 추가 피해 안내를 정리했다. 실제 notch/터치 목표 크기 합격은 아니다.

## 경제 / 저장 / 한계

Item86/Recipe71. 카테고리 Tools10/Facilities8/Survival19/Armor6/Weapons14/Building14. [전체 제작·source/sink·광물 예산](CONTENT_EXPANSION.md), [음식/낚시](FOOD_AND_FISHING.md).

Seed9472026의4인 보장 목표/실제 공급: Iron197/198,Copper72/72,Tin31/33,Coal220/220. 기존 source/광물 위치를 옮기지 않고 부족분을 별도 결정적 ID로 추가한다. 비재생 원칙을 유지한다. SP QA 파일83,808bytes, 최초 Host81,962bytes는 서로 다른 구성/시점이며 직접 용량 성능 비교가 아니다.

원본 world-v1.json과 .backup의 SHA-256/mtime가 시작과 같다. 이번에는 backup rotation을 쓰지 않았다. Play 전 별도 QA 경로와 기본 설정/멀티 식별자 격리를 적용했다. normal profile 복귀 후 Play를 중지했다. Windows 입력/Computer Use/Focus/Show/GetWindow는 사용하지 않았다.

실제 Android/iOS·Relay·4기기·장기 생존/경제·실제 터치는 미검수다. 기존 Editor GC/chunk spike가 해결됐다거나 단말 FPS 영향이 없다고 주장하지 않는다. 일부 검사는 다른 Editor 작업과 병행했다. 탄약은 앞쪽 가방 슬롯부터 사용하므로 선택 UX 여지가 있다. Utility/신규 아트는 알파 수준이고 MP HUD는 기존 구조다. Trap의 짧은 재사용 타이머는 transient이며 설치물 종류/위치/HP를 저장한다.

I~II의 요청 기능을 막는 남은 결함은 이번 검수에서 발견하지 못했다. 다음 III/Diamond/Turret Workbench 기획·구현으로 진입할 수 있으나 출시/장기 밸런스 합격은 아니다.

## 시간

시작 KST17:10:35. 단계별 값은 도구 기록에 근거한 근사 구간이며 병행 시간은 합산하지 않는다. 최종 총 경과와 Git 종료 시각은 완료 보고에 기록한다.
사전 감사·짧은 조사·QA 격리: 약6분.
초기 구현·아트·에셋 반영: 약15분.
MCP 검수·실패 사례 수정/검사: 약35분.
문서·최종 검토: 약16분, 검사와 병행.
자동검사 순수 실행 합계:369.71초.
