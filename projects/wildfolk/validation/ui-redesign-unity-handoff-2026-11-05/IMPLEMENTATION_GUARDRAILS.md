# Unity UI 구현 가드레일

이 문서는 차기 승인된 구현 패스의 보호 조건입니다. 본 설계 패스는 Unity source/scene/prefab/assets를 수정하지 않았습니다.

## 소스·씬 작성 경계

- UI는 UGUI와 현재 프로젝트의 화면/controller 구조를 사용합니다. UI Toolkit 또는 새 테마/override 계층으로 재작성하지 않습니다.
- 정본 경로는 현재 Editor canonical builders, Runtime UI view/controller, serialized scene 연결입니다. 파일럿에서 FrontendSceneBuilder, GameplayHudBuilder, InventoryUiBuilder 및 관련 Arrange 메서드를 확인해 기존 builder를 갱신합니다.
- canonical builder가 다시 생성하는 scene을 손으로 수정해 최종 디자인으로 남기지 않습니다. builder/scene의 목적 차이를 먼저 검토하고 생성 결과를 저장합니다.
- HomeScreen, GameplayHud, InventoryPanel 등의 화면 외관 변경은 view/presentation 책임만 변경합니다. 저장·세션·서비스의 controller 계약을 UI에서 재현하지 않습니다.
- CampLootPanel, TierThreeHud, TransitionOverlay 같은 runtime-created UI는 serialized scene에 존재한다고 가정하지 않고 생성·수명·release 경로를 실제 source에서 따릅니다.
- 새 generic patch/hotfix/override component를 여러 builder 위에 누적하지 않습니다. shared component를 바꿀 경우 모든 소비처와 builder를 먼저 목록화합니다.

핵심 소스: Editor/FrontendSceneBuilder.cs, Editor/GameplayHudBuilder.cs, Editor/InventoryUiBuilder.cs; Runtime/UI/HomeScreen.cs, GameplayHud.cs, InventoryPanel.cs, SettingsPanel.cs, ModalLayout.cs, SafeAreaLayout.cs, ControlSizeLayout.cs. 실제 경로는 EVIDENCE_INDEX에 정리되어 있습니다.

## 제품 기능 불변조건

| 영역 | 유지 조건 |
|---|---|
| 화면 구성 | 구현 ID 57개 및 기존 EN/KO; Stage 1.5/Penpot에만 있는 기능은 제품 기능으로 추가하지 않음 |
| 인벤토리 | 36칸=Hotbar9+Storage27; 장비 Head/Body/Utility/Accessory 4개 |
| 아이템/제작 | Item136/Recipe111; 아이템 이동/분할/장착·해제/수량·제작 비용/순차 station queue 실제 권위와 정확히 연결 |
| 생존·입력 | HP/Hunger/Thirst와 class/성장 상태 실제값; Primary hold 반복 공격·Interact click·rescue hold·Dash 짧은 이동·Stamina transient 표현 보존 |
| 시간 | Inventory/Crafting와 현행 메뉴의 world tick을 보존. 싱글플레이 Pause와 Death는 현재 정책대로 멈춤 |
| 성장 | 6 classes, 54 class nodes 및 6 common skills의 condition/ownership/choice 유지 |
| 월드·저장 | Continue는 검증된 현재 SP 저장, Single Player는 새 랜덤 seed와 H02 replacement confirm; Save14/Profile2를 임의 변경하지 않음 |
| 멀티플레이 | LAN Host/Join, host 별도 save, authority/room code/leave/error semantics. browser/host migration을 화면이 구현된 것처럼 추가하지 않음 |
| 계정/경제 | Protocol23, account cloud/identity, premium currency, achievement/attendance/store/ad gates, purchase receipt authority 유지 |
| 피드백 | 기존 AudioRuntime/AudioButton/Lowfire mixer binding. click cue를 success/reward receipt로 오인시키지 않음 |
| 시스템 | Localization LocalizedLabel/localized values, SafeAreaLayout, ModalLayout, canvas scaler, EventSystem/pointer owner/cancel binding 유지 |

상세 field/action contract는 해당 canonical docs와 UI audit control/state map을 따릅니다. 위 수치들은 당시 repository README 기준이며 새 개발 시작 시 해당 최신 정본을 다시 확인합니다.

## 입력·상태 안정성

- Pointer down/up/cancel, focus loss, modal open, actor death, scene transition 시 held input이 반드시 release/reset됩니다.
- 2개 이상 held touch의 pointer ownership은 실제 input adapter에 맞춰 확인합니다. Button visual root와 transparent hitbox를 혼동하지 않습니다.
- confirm/consume/dismantle/delete/purchase actions는 명확한 intent/confirm 필요 여부를 현행 계약에 따릅니다. 디자인 목적으로 비용·확인 수준을 바꾸지 않습니다.
- Slot drop이 invalid/empty일 때 현재 preservation semantics를 유지하고, UI preview가 server result 전 item movement를 확정하지 않습니다.
- ScrollRect 영역은 상호 작용 요소를 덮지 않고, drag/drop gesture와 scroll gesture를 방해하지 않습니다.
- AudioButton을 포함한 기존 focus/navigation path와 gamepad/keyboard 동작이 있는 곳은 계속 연결합니다.

## 품질·검증 규칙

1. 변경 전 baseline source, scene/prefab GUID, builder output, existing tests and save format을 기록합니다.
2. 파일럿 당사자/사용자 승인 전 전체 이관으로 확대하지 않습니다.
3. 캡처/Unity build/device run을 직접 실행하지 않으면 그 사실을 명확히 남깁니다. Game View만으로 Android touch/dp/performance 합격을 주장하지 않습니다.
4. KO/EN 긴 문자열, 상태 조합, destructive/cancel, offline/network fail, long list, maximum stack/cost를 검증합니다.
5. 기능 테스트는 UI 모양만 보지 않습니다: save roundtrip, inventory transfer, economy balance, world tick, host authority, input reset.
6. Unity Editor canonical build → compilation → relevant EditMode/PlayMode → isolated Save/Load → physical Android screen/touch/profile 순서로 회귀합니다.
7. 데이터/프로토콜 schema 변경은 별도 승인과 migration 검증이 필요하며 화면만 바꾼다는 이유로 변경하지 않습니다.
8. sprite atlas/memory/overdraw/CPU/GPU profiling은 실제 target Android 기기 결과로 보고합니다.

## 출시·운영

- 광고/결제/online providers가 OFF 또는 SETUP_REQUIRED면 디자인에서 판매 가능 상태를 암시하지 않습니다.
- 새로운 remote service, key, URL, entitlement, save slot, IAP item, monetization CTA는 생성하지 않습니다.
- 이 handoff는 승인 전 설계 자료입니다. UI 구현 또는 release approval이 아닙니다.
