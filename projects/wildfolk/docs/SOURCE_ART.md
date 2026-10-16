# 품질 교정 Asset 출처

기본 세계/캐릭터/Item Icon은 기존 자체 제작 아트를 유지했다. 이번에는 Worn Overlay 네 PNG와 Dash, 글꼴만 교체했다. 최종 파일은 client/Assets/Wildfolk/Art 및 Resources에 있다. 기존 손/눈/아이템 Icon은 유지한다. Asset Generator는 큐레이션된 Overlay/Dash를 덮어쓰지 않는다.

Armor는 imagegen built-in 편집 모드로 제작했다. 기존 해당 Worn PNG를 편집 대상으로, 이번 작업에서 직접 캡처한 quality-baseline-equipment.png를 스타일 참고로 사용했다. 모두 투명 배경이다. 생성 픽셀을 다시 그리지 않고 투명 여백/기존150×150 Sprite Canvas에 맞추는 Import packaging만 수행했다. 원본은 로컬 generated_images에 보존한다.

공통 Prompt: direct overhead top-down, front/down; circular peach base and two small eyes visible through transparent open center/front; standalone wearable overlay only, no face/body/eyes/hands in output; natural dark outline, matte readable broad planes, no band/ring/front-view RPG chestplate, no neon/candy/pastel/text/horns.

| Asset | Prompt 차이 | 원본 생성 파일 |
|---|---|---|
| iron_head_worn | 넓은 round domed rear steel crown, substantial stepped side/cheek guards, transparent lower face opening, 2~3 large seams; grey headband 제거 | exec-ae17673e-5274-4d80-8d0e-9fabc4f5768d.png |
| iron_body_worn | broad steel shoulder plates and rear upper back plate, angular stepped silhouette, thicker than leather, open center/front, three steel-gray planes | exec-baab6edd-a9c3-4549-be01-6ab2d86ae658.png |
| leather_head_worn | stitched chestnut brown soft cap, three large leather segments, slim tapered sideguards, clear difference from domed steel, open lower center | exec-3cd003cd-98f5-4530-ad22-f084ef5f7822.png |
| leather_body_worn | thin brown upper back yoke, small layered shoulder flaps, narrow uneven tapered segments, sparse broad straps/stitches, open center | exec-c38098da-784c-4efe-91f2-a1bcdf218ca1.png |

원본 디렉터리: C:/Users/User/.codex/generated_images/01a132a7-29f9-7291-8050-442ea921a350/. 표는 사용한 디자인 Prompt 요약이며 같은 결과를 픽셀 단위로 재생성한다는 보장은 하지 않는다.

Dash는 [Lucide chevrons-right](https://github.com/lucide-icons/lucide/blob/main/icons/chevrons-right.svg)의 stroke를 무광 자연색으로 조정한96×96 PNG다. Art/dash-source.svg와 Lucide-LICENSE.txt에 원본/ISC 및 Feather MIT 고지를 보존한다. 외부 게임 Icon을 추출하지 않았다.

Font는 [Google Fonts Noto Sans KR](https://github.com/google/fonts/tree/main/ofl/notosanskr)의 wght500 static instance다. Resources/NotoSansKR.ttf와 NotoSansKR-OFL.txt를 함께 제공한다. 다른 앱/Windows 시스템 글꼴은 배포하지 않는다. 세부 최종 아트 품질·실기 가독성은 후속 사용자 검수 대상이다.


## 2026-10-15 신규 생물·구조물

사용자 지시에 따라 기존 tools/generate_visual_assets.py의 Pillow polygon/ellipse/line 제작을 ecology_art로 확장했다. rabbit/deer/sheep/skeleton/bone_hand/rusted_sword/wood_wall/stone_wall/wall_post/door_frame/door_leaf/wood_door/slingshot/goblin_tent/camp_box/pebble PNG가 실제 Art 폴더와 Prefab/Item에서 사용된다. --ecology-only로 이 부분만 재생성하며 기본 전체 실행도 같은 함수를 사용한다. 교체된 옛 벽·문 제작 블록은 제거했다.

이번에 시도한 imagegen 시트2개는 사용자 요청으로 원본 파일까지 폐기했고 프로젝트에 Import하지 않았다. 기존 Armor4개는 이전 큐레이션 결과를 유지한다. 새 애니메이션은 기존 캐릭터 손 Transform·새 Frame/Leaf Sprite와 Band LineRenderer를 시간에 따라 조작하며 외부 게임 애니메이션을 추출하지 않았다.


## 2026-10-15 Lowfire Home UI

Krita MCP의 native path/ellipse/line/polygon으로 Lowfire 소문자 혼합 lettering(815×215), 불씨색 i 점과 제본·종이 가장자리·접힌 귀가 있는 수첩 패널(768×896)을 제작했다. Imagegen/Blender/외부 게임 Asset을 사용하지 않았다. 캐릭터 아트 제작 방식은 기존 코드/Pillow다.

편집 원본: art-source/LowfireWordmark.kra, LowfireFieldJournal.kra. 실제 UI: client/Assets/Wildfolk/Art/lowfire_wordmark.png, lowfire_journal.png 및 Sprite import meta. Krita preview 뒤 Unity1920×1080 Home에서 확인했다. Krita의 기존 허용 폴더인 Documents/ChatGPT/테스트/LowfireHomeAssets로 저장/내보낸 다음 일반 파일 복사로 정본과 LIVE에 배치했다. 허용 루트/설정을 변경하지 않았다. 종이의 빈 중앙은 실제 Unity Button/Text를 배치하는 영역이며 메뉴 글자를 이미지에 굽지 않았다.

## 2026-10-16 초기 금속

tools/generate_metal_art.py의 결정적인 Pillow polygon 제작으로10개 PNG를 만들었다. Copper는 낮고 넓은 산화색 광맥, Tin은 갈라진 높은 회색층과 밝은 수평 결, Coal은 어두운 납작한 층상 덩어리다. 재료 아이콘7개와 resource sprite3개를 실제 Item/Prefab에서 사용한다. 기존 접촉 그림자/Health/HitFeedback을 공유하고 새 이미지 생성 모델/외부 게임 Asset/오디오를 사용하지 않았다. 1920×1080 숨김 Unity 자동 렌더의 원본 위치를 사람이 시각 검수했으며 실제 MCP/Game View 조작 테스트는 미수행이다.
