# English / 한국어

기본 English, 현재 실제 지원은 en/ko 두 언어다. Settings의 Language 선택을 누르면 다음 언어로 전환하고 활성 UI와 다음에 열 화면에 즉시 반영한다. 미래 언어 버튼/가짜 번역은 없다.

Resources/Localization.json의 Languages/Entries가 정본이다. ui.*, setting.*, feedback.*, item.<ID>.name의 키와 위치 Placeholder를 사용한다. L.Text/Format, LocalizedLabel, 동적 화면의 언어 이벤트를 공유한다. Item의 Authoring DisplayName은 Editor/Data 이름으로 유지하며 제품 표시에는 LocalizedName을 사용한다. 번역 수·빈 값·중복 키·Placeholder·Item 키를 검사한다. 언어 추가는 Catalog에 언어와 각 행의 번역을 추가하는 경계이며 UI/게임 로직을 다시 쓰지 않는다.

HUD, Character Information, Inventory Equipment, Skill Tree, Crafting/Workbench, Campfire/Furnace, Chest, Split/Item Actions, Pause/Settings와 일반 Feedback이 대상이다. Editor Developer Console은 대상에서 제외한다. 누락 키는 제품 화면에 내부 키를 노출하지 않고 오류 로그로 검출한다.

Language는 wildfolk.user.language PlayerPrefs에 저장한다. Control Size/Master/BGM/SFX와 같은 Local User Preference이며 World Save/새 World/Load가 언어를 바꾸지 않는다. 기본값 초기화는 English로 돌아간다. 검수 Fixture의 언어는 검수 후 원래 설정으로 복원한다.

한국어 Glyph는 [Google Fonts Noto Sans KR](https://github.com/google/fonts/tree/main/ofl/notosanskr)의 weight500 정적 인스턴스를 사용한다. 원본의 [SIL OFL](https://github.com/google/fonts/blob/main/ofl/notosanskr/OFL.txt)을 함께 제공한다. Windows 시스템 글꼴을 배포하지 않는다. 실제 Android 폰트 렌더링/터치 가독성은 실기 확인 대상이다.


Rabbit/Deer/Sheep/Skeleton/검 Skeleton/새총 Goblin/Camp/Rusted Sword와 Armor 실패 Feedback,Developer Console Section/설명/Tooltip/경고/필드 라벨은 같은 English/한국어 키 구조로 추가했다. 기본 English와 Local Preference를 유지한다. 종·Variant·Item의 authoring ID/Prefab 이름은 내부 식별자이며 플레이어 문구로 출력하지 않는다.

Home은 home.* 키를 기존 en/ko Catalog에서 관리한다. Continue/Single Player/Multiplayer/저장 요약/덮어쓰기 확인/이용 불가 안내/Save & return home을 번역한다. Home/Pause Settings는 같은 Preferences 키를 저장하고 다음 씬에서도 유지한다. 숨겨진 Home Label은 기존 LocalizedLabel.OnEnable에서 다시 표시될 때 갱신하며 숨은 UI를 매 프레임 갱신하지 않는다.


Lowfire 표시 이름은 brand.name(en/ko 공통 Lowfire)과 Home의 Krita wordmark PNG에 반영했다. home.camp(At the camp/야영지에서)를 사용하며 기존 eyebrow/tagline/CHOOSE YOUR PATH와 중복 Pause Load 문구 키를 제거했다. 저장 일차는 Day {0} · Saved world / {0}일차 · 저장된 세계로 간결하게 표시한다. 로고는 고정 영문 브랜드 자산이고 행동/안내는 기존 Noto Sans KR와 Localization을 사용한다. 내부 namespace, wildfolk.user.* 키와 저장 위치는 바꾸지 않았다. PlayerSettings.productName도 저장·배포 식별자에 영향이 없도록 이번 표시 UI 작업에서는 유지한다.
