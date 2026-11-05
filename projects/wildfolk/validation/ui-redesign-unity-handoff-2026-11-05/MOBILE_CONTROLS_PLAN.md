# 모바일 컨트롤 배치·검증 계획

## 현재 입력 계약 (변경 금지)

| 입력 | 현재 동작 | 디자인 위험 |
|---|---|---|
| 이동 | 화면 왼쪽 영역 VirtualJoystick, touch release/cancel/focus loss/state change 시 zero 처리 | 고정 원판 artwork가 touch start 영역과 다른 위치를 암시하면 입력 놓침 |
| Primary | 오른쪽 Primary, tap 및 hold-repeat attack. release/state reset 시 반복 종료 | 작은 hit target·공격과 interact 겹침 |
| Interact | 대상을 바탕으로 단일 click | 공격과 너무 가깝거나 화면 edge에 두면 오입력 |
| Revive | downed teammate 인근에서 Interact hold로 구조; 이동/피해/line of sight/session interruption에 취소 | 일반 interact와 hold 구조를 시각·텍스트로 구분 |
| Dash | 짧은 이동 효과; 무적 프레임이 아님 | 회피/무적처럼 보이는 비주얼/툴팁 금지 |
| Class active | class에 종속된 ability; 일부는 hold/release/target 필요 | 항상 같은 입력/효과인 것처럼 그리지 않음 |
| Hotbar | 9 슬롯 선택/아이템 사용과 연결 | 화면 폭을 점유하고 공격 터치 경로와 경쟁 |
| Pause | SP Pause에서 world tick 정지, 다른 메뉴는 다르게 tick | 기기/MP 상태와 pause 의미 분리 |
| Stamina | 사용 시 transient arc/overlay | 영구 막대로 표시해 이미 구현된 상태를 바꾸지 않음 |

자세한 입력 ID/취소 전이는 기존 CONTROL_AND_ACTION_MAP.md와 SCREEN_INVENTORY.md를 따릅니다.

## 손 영역 설계안

왼손은 이동, 오른손은 주 행동을 기본으로 유지합니다. 844×390에서는 중요 컨트롤을 safe area 바깥에 두지 않고 thumb pivot 주변으로 묶되, 버튼 중심 간 거리·hitbox 겹침·hotbar 접근을 실기기에서 측정합니다.

- **A: 공격 중심 클러스터** — Primary 가장 큰 버튼. Interact는 Primary 위쪽/안쪽, Dash는 바깥쪽 작은 버튼. 긴 hold 구조를 위해 Interact 주변 여백 확보.
- **B: 분산 행동 클러스터** — Primary 오른쪽 아래, Interact 위쪽, Dash 오른쪽 바깥. 접근성보다 간격을 우선해 공격-상호작용 오탭을 줄이는 안.
- 두 안 모두 왼손 조이스틱, 아래 hotbar, 위 pause를 보존합니다. 시안 배치만으로 승자를 선택하지 말고 실제 폰에서 전투/채집/상자 이용 세션으로 비교합니다.
- 시각 크기와 hitbox는 같은 것이라고 가정하지 않습니다. 최소 Android 48dp 권장은 참고하되 Unity canvas units로 직접 환산하지 않습니다.

## 실제 기기 검증 프로토콜

1. 화면 대각/양손 엄지 reach를 확인할 Android 기기, 해상도/density/gesture navigation 상태를 기록합니다.
2. A/B 위치를 각 10분 이상 사용해 실제 행동 횟수, mis-tap/놓친 hold/엄지 이동/시야 가림을 관찰합니다. 참가자가 한 명뿐이면 통계적 usability 증명으로 표현하지 않습니다.
3. 이동+반복공격, 이동+대시, 공격 후 상호작용, downed teammate 구조 hold, class 능력 중단, 핫바 전환을 검사합니다.
4. 왼손/오른손 기본 사용자와 손 크기 차이를 가능한 사용자로 검토하고, 사용성 조정은 수치와 관찰로 남깁니다.
5. 844×390 Game View 캡처는 시각 검토에만 쓰고 기기 접촉/DP/프레임/성능 통과로 주장하지 않습니다.
6. 출시 판정에는 터치 취소, 화면 회전/포커스 loss, safe area/cutout, 일시정지·대화상자 전환의 재현 확인이 필요합니다.

## 수동 위치 편집: 비용·출시 판단

**현재 상태:** Settings의 Control Size 0.8–1.4만 구현. 개별 위치·투명도·왼손잡이 편집은 없습니다.

**기본 배치가 실기기 검증을 통과하면 후속 업데이트로 미룰 수 있습니다.** 출시 차단 필수라고 단정하지 않습니다. 다만 기본 배치가 접근성이나 오입력 요구를 만족하지 못하면 필수 재평가입니다.

**예상 비용: 중~높음, 위험도: 중~높음.** 단순 Drag UI가 아니라 다음 설계가 필요합니다.
- 안정된 action IDs와 기기 safe-area 기준 normalized anchors/scale/opacity
- 최소 hitbox, 상호 overlap/edge/cutout 검사 및 복원 가능한 defaults
- 미리보기/편집 모드에서 게임 action을 차단하고 취소/적용 semantics 정의
- resolution/aspect/density 변경과 orientation에 대한 재배치/migration
- local setting vs profile/cloud ownership 결정 및 버전·rollback schema
- accessibility와 알파값 저하가 입력 인식성을 해치지 않는 bounds
- 기존 ControlSizeLayout/settings/save 책임과의 통합, KO/EN·기기 회귀

이번 단계에서는 이 schema, 저장 데이터, UI를 구현하지 않습니다. 사용자 승인을 받고 default 레이아웃 측정 결과가 있어야 우선순위를 결정할 수 있습니다.

## 출처

- Android 48dp target과 gesture/cutout: https://developer.android.com/guide/topics/ui/accessibility/views/apps-views · https://developer.android.com/develop/ui/views/layout/display-cutout · https://developer.android.com/design/ui/mobile/guides/layout-and-content/edge-to-edge
- 공식 mobile examples and principles: [EVIDENCE_INDEX.md](EVIDENCE_INDEX.md).
