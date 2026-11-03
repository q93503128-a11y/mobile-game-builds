# 물 표현 추가 감사 · 2026-11-03

**추가 감사 완료 / 제품 변경 NONE / 물 표현 재작성은 미승인 제안.** 이번에는 이전에 남겨 둔 현재 lake/coast 실행 비교와 공식 배포 이미지 확인을 보완했다. 방향 교정이나 이미 통과한13/16개 검사를 다시 수행하지 않았다. 물 표면의 세부 정보가 줄어든 것은 확인되지만, 현재 대륙 지형에 맞는 완성된 이전 표현을 그대로 복구할 수 있는 상태는 아니다.

## 기준과 보호

시작 전에 remote main을 fetch했다. clean main의 local HEAD=origin/main=`421a31e75bcb0335887d57b77ff5520fd9c45cb2`. 프로젝트 AGENTS/README와 물·생성·구조·기획 정본, 사전 구현 게이트/후속 교정 기록을 읽었다. dev-playbook도 최신 main을 fetch/read했다.

canonical=`C:/Users/User/Documents/ChatGPT/문승준 모바일/canonical/projects/wildfolk`, native=`C:/UnityProjects/Wildfolk`, Unity6000.3.19f1. Unity MCP 로컬 HTTP 연결로만 관찰했으며 Windows 입력/Computer Use/foreground Focus·Show를 사용하지 않았다. 기존 GameView 크기 선택/Repaint로1920×1080을 사용했다.

새 격리 profile=`Captures/Codex_QA_WaterAudit20261103`. 실제 save/Continue를 로드하지 않았고 AutoSave를 껐다. 사용자 저장6개는 SHA256/bytes/mtime_ns가 처음과 같다. 시작에 native MainMenu/Wildfolk 씬에 canonical과 다른 trailing whitespace가 있었으나 의미는 같았다. 씬을 덮어쓰거나 저장하지 않았고, 관련9개 source/data/material/scene의 native hash가 끝에도 같다. 기존 Audio/Mixer/NetworkPlayer 차이도 덮어쓰지 않았다. 임시 Editor observer는 native에만 두었다가 제거했다.

## 실제 실행 비교

seed9472026, 낮1일30초, Camera orthographic size9,1920×1080, 같은 material/shader와 조명을 사용했다. 각 generation reader로 실제 월드를 생성하고 지형을 표시했다. 첫8개 비교에서는 HUD·prop·생물 Sprite만 숨겨 물/지면을 대조했다. 생성기나 sampler를 교체하지 않았다. 모든 위치는 관찰을 위한 teleport이며 자연 발견·진행으로 세지 않는다.

Gen2/Gen3는 같은 좌표를 사용했다. Gen1은 현재 지원되는 옛 reader/formula이며 지형과 좌표가 다르다. **옛 전체 client를 별도 실행한 것은 아니다.** Gen1에는 대륙 해안이 없어 coast 대응 화면을 만들지 않았다.

|수역|Gen2/3 공통 화면 중심|대조 결과|
|---|---|---|
|강|626.8638,120|PNG 전체 bytes 동일,2073600pixels 차이0|
|호수 동쪽 물가|−967.077,−850|동일|
|서쪽 해안|−6027.55859,0|동일|

[실제 실행8개 메타데이터](cases.json) · [PNG hash/동일성](gen2-gen3-image-comparison.json). 중복된 Gen2 PNG3개는 Git에 다시 넣지 않았다. 원본은 native Captures에 있으며, 동일 hash의 Gen3 파일을 저장했다.

|수역|현재 Gen3 실제 지형 화면|현재 보존된 Gen1 경로의 실제 화면|
|---|---|---|
|강|[현재 강](evidence/g3-river.png)|[보존된 옛 강 표현](evidence/g1-river.png)|
|호수|[현재 호수](evidence/g3-lake.png)|[보존된 옛 호수 표현](evidence/g1-lake.png)|
|해안|[현재 해안](evidence/g3-coast.png)|해당 지형 없음|

추가로 **Gen3의 prop/HUD를 포함한 실제 장면**도 [호수](evidence/g3-lake-with-hud.png), [해안](evidence/g3-coast-with-hud.png)에서 확인했다. 카메라는 물가에 고정하고 실제 Player는 WaterDistance2.6667/3.9998인 마른 땅에 배치했다. 개발용 속도/전투/장비 변경은 하지 않았다. 시각 비교를 위한 seed/시간/카메라 통제와 teleport를 사용했으며 자연 플레이 경험은 아니다.

최초 context 캡처는 ScreenCapture의 프레임 종료 이전에 카메라를 다음 위치로 옮겨 lake라는 이름에 coast가 찍혔다. 그 파일은 증거에서 제외했다. 카메라가 안정된 후 요청하고 완전한 PNG/IEND와1920×1080을 확인한 `*-with-hud.png`만 사용했다. 첫 observer RPC의 지역변수/람다 이름 충돌도 도구 코드에서 고쳤다. 제품 버그로 분류하거나 성공 증거에 섞지 않았다.

## 무엇이 달라졌는가

각 수역의 물 내부64×64=4096점에서 현재 실제 sampler를 조회했다. 이는 **CPU source color 표본**이며 GPU readback·지각 품질 점수·전체 수역 통계가 아니다. 지정 patch의 모든 점은 IsWater/IsDeepWater=true였다.

|reader / 수역|물 내부 고유 RGB24 수|휘도 표준편차|
|---|---:|---:|
|Gen3 강/호수/깊은 해안|각1|각0|
|Gen2 강/호수/깊은 해안|각1|각0|
|보존된 Gen1 강|102|0.009755|
|보존된 Gen1 호수|136|0.011934|

Gen2/3는 물 안쪽에서 최종 Lerp가1이 되면 깊이/해안 조건의 고정색만 남는다. 지면 grain을 먼저 계산해도 이 영역의 최종색에는 남지 않는다. 따라서 **텍스처 해상도만 높이면 잔물결·색 변화가 되살아난다는 가정은 틀리다.** 물가의 sampling 문제와 물 내부 detail 부재는 별개다.

텍스처는 모두 RGB24/Bilinear/no mipmaps다. 현재 청크는64unit/128sample에 halo1씩을 둔130², 즉0.5unit/sample이다. 보존된 Gen1은192unit/1024sample=0.1875unit/sample,1024×768이다. 현 카메라18unit/1080px 기준 각각 약30px와11.25px/sample에 해당한다. 이 차이는 현재 물가가 더 크게 꺾이고 흐릿하게 보이는 요인이지만, flat interior의 원인 전부는 아니다. Android 성능/화면 dp 측정은 아니다.

## 역사적 근거와 rollback 판정

Gen2 직전 `cf4ecd689623ea91b2d1815f010e66bcdd5cebe5`의 `WorldTerrainRenderer.Sample`과 현재 보존된 Gen1 Sample을 비교했다. 공백과 새 streamed dispatch guard만 제외하면 함수 본문이 동일하다. 옛 World Size/RiverWidth/RoadWidth/TextureWidth/Biome 참조, TemperateBiome의 Grass/Forest/Dirt/Water/Shallows/DensityFrequency도 같다. [실제 비교 필드와 hash](retained-legacy-provenance.json).

즉 **이전 물의 grain/ripple이 있었고 현재 대륙 표현에서 빠진 차이는 실제로 확인된다.** Gen2 도입 `4502ab8`의 다른 sampler로 넘어간 때의 변화다. 최근 UI commit 때문에 갑자기 나빠진 것은 아니다. 사용자가 기억하는 정확한 세계/버전은 여전히 모른다. 당시 전체 client의 조명/postprocessing까지 동일하게 재현했다고 주장하지 않는다.

전체 Gen1 renderer를 rollback하면 단일 TemperateBiome의 Grass/Forest 팔레트와 단일 FordY 기반 얕은 물 계산까지 가져오게 된다. 현재4개 surface biome 혼합·여러 여울·해안/바다 깊이와 같은 의미가 아니다. 따라서 전체 rollback은 부적절하다. 과거 수면 detail 원리를 현재 지형에 맞게 다시 작성하는 것은 **표현 REBUILD**로 남긴다. 현재 source/data/material/geometry는 수정하지 않았다.

## 공식 이미지에서 직접 확인한 비교

원본3개는 공개 공식 도메인에서 실제 파일을 받아 직접 열어 확인했다. 외부 이미지는 작업 폴더에서만 조사했고 Git/game asset에는 넣지 않았다. 링크·hash·수신 크기는 [외부 이미지 index](external-reference-index.json)에 있다. 모두 공개 홍보/게임플레이 정지 이미지이며 촬영 client version/최신 mobile 상태는 미확인이다. 정지 화면으로 애니메이션·반사 구현 방식·실전 가독성이나 성능을 추정하지 않는다.

|게임|해당 시스템|실제 사례 / 공식 source|장점|Lowfire와 차이|적용 여부|이유|
|---|---|---|---|---|---|---|
|Core Keeper|넓은 수면·물 위 대상|[publisher의 Atlantean Worm 화면](https://fireshinegames.co.uk/wp-content/uploads/Atlantean-Worm.webp), [게시 페이지](https://fireshinegames.co.uk/featured/core-keeper/). 어두운 푸른 수면 위 가는 선/반짝임, 보트 주변 흔적과 투영 형태가 보임|큰 평면에도 표면 정보가 있고 물 위 대상과 접촉 맥락이 읽힘|pixel art/발광/보트/대형 수상 boss이며 Lowfire는 무광 자연색·도보 중심|표면 정보와 계층 원칙만 제안에 반영|네온 팔레트·보트 wake·반사 shader를 그대로 도입할 근거는 아님. 정지 화면만으로 움직임/구현을 단정하지 않음|
|Don't Starve: Shipwrecked|수면 pattern·파도·물 위 대상|[공식 Store 배포 화면](https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/393010/ss_bf68d2d933102471be63278902c4d4d4b6cdf5ee.jpg), [Store](https://store.steampowered.com/app/393010/Dont_Starve_Shipwrecked/). 녹색 면에 잔무늬/작은 흰 물결, 큰 파도와 배/괴물 주변 고리가 보임|평면이 단색 판처럼 비지 않고 배/위험의 크기 차이가 읽힘|파도·항해가 gameplay인 작품; Lowfire에는 해당 mechanics 없음|절제된 표면 무늬와 대상 우선 계층만 제안|큰 파도·흰 고리·항해 규칙을 복제하면 현재 전투 tell과 경쟁하고 불필요한 mechanics를 암시|
|Don't Starve|살아 있는 동물의 지면·시점 전달|[Klei 공식 Beefalo 화면](https://assets.klei.com/f/259446/1280x720/8b19912de9/dont_starve_gamplay_beefalo.jpg), [게시 페이지](https://www.klei.com/games/dont-starve). 몸/머리/발과 그림자의 같은 지면 관계가 보임|살아 서 있는 동물이라는 인상이 지속됨|Lowfire Wolf/Goat는 정면형 단일 그림의 whole-sprite 회전이 눈에 띔|아트 시점 검수 기준만 제안|한 이미지로 그 게임의 방향 애니메이션/회전 구현을 추론하지 않음. 사용자가 원래 본 종을 식별하는 근거도 아님|

기존 외부 조사에서 열지 못했던 원본 일부를 이번에 확인했다. 모든 게임/모바일 버전의 완전한 화면 benchmark가 끝났다는 뜻은 아니다.

## 동물·다른 감사 항목의 현재 상태

완료된 방향 교정은 유지했다. 이번에 그 코드나 테스트를 다시 바꾸지 않았다. 원래 누운 개체의 종·상태는 추가 원본 정보가 없어 아직 특정할 수 없다. 기존 after 갤러리는 Wolf/Goat의 시점 문제를 보여 주고, 이전 실제4종 death→pool 검사는 stale corpse 가설을 지지하지 않았다. 따라서 정확한 원본 식별과 주관적 아트 재작성 판단을 분리한다.

기존12seed/동굴·묘지·도로 수치를 새 조사인 것처럼 다시 인용하지 않았다. 장애물을 직접 피하는 자연 발견성, 원격 multiplayer, Android, 전체 progression/King 전투는 이번 범위가 아니다.

## 검증·마감과 다음 결정

제품 구현 변경이 없으므로 새 NUnit/거대 회귀를 실행하지 않았다. ProjectDataValidator 오류0, 최종 compile 오류0, Console은 기존 CS0252/CS0414 경고2개다. 사용자 저장6개와 관련 native source/data/material/scene hash가 그대로다. MainMenu/Edit, Play OFF, scene clean, QA SessionState empty, 임시 observer 제거를 확인했다. [상태·검증](validation.json).

원래 감사 요청 §19는 객관적 최소 교정만 허용하고 주관적 재설계는 명시적 승인을 요구한다. 다음에 제안하는 구체적인 범위는 [현재 지형을 보존하는 물 표면·물가 표현 재작성](PROPOSAL.md)이다. **아직 구현하지 않았다.** 이 물 제안은 repair UI/도로/동물 전체 재설계나 새 기능을 함께 승인받으려는 묶음이 아니다.

정확한 이번 변경은 [files-changed.txt](files-changed.txt), 단계별 UTC 시각/경과는 [timing.json](timing.json). 지표·아트 설계·성능을 단정하지 않고 이번 실제 관측과 남은 한계를 구분한다. main commit/push 후 실제 full SHA/local=remote/clean은 최종 응답과 작업 폴더 `water-audit-20261103/final-git.json`에 남긴다.
