# 현재 저장/협동 UX와 미래 multi-world 경계

감사 재개 시 fetch한 main: `82d19b64de0157d924a05f87ea8ee698f5d6db6b`. 제품 source/data는 이전 `48d6528`과 동일하다. game 변경·Penpot 제작 없이 조사했다.

FACT/CONFIRMED는 현재 source/이번 audit의 실제 화면, SOURCE-INFERRED는 source로 도출한 동작·위험, HISTORICAL은 과거 캡처, PROPOSAL은 제안, FUTURE GOAL은 사용자 차기 목표, UNVERIFIED는 검증 범위 밖이다. 디자인 심각도는 감사 우선순위이며 모든 항목을 출시 차단 코드 버그로 해석하지 않는다.

| 현재 FACT | source | UX 의미 |
|---|---|---|
| SP world-v1.json / Host multiplayer-world-v5.json 고정 slot | [SaveRepository.Default](../../client/Assets/Wildfolk/Runtime/Persistence/SaveRepository.cs#L11) | 임의 여러 world 목록 없음. 두 모드 slot이 존재한다는 것과 multi-world는 다름 |
| Home Continue 검증; SinglePlayer 새 seed + 교체 확인 | [HomeScreen.SinglePlayer](../../client/Assets/Wildfolk/Runtime/UI/HomeScreen.cs#L24) | Single Player가 world manager인 것처럼 wireframe 금지 |
| primary→backup 검증·checksum·temporary flush·atomic replace | [SaveRepository.Read](../../client/Assets/Wildfolk/Runtime/Persistence/SaveRepository.cs#L49) | invalid/backup/read error를 명확한 카드 상태로 보존; silent new world 금지 |
| Host load는 IsMultiplayer + HostId==LocalProfile 필요 | [NetworkSession.Host](../../client/Assets/Wildfolk/Runtime/Networking/NetworkSession.cs#L65) | 현재 SP 파일을 Host repository로 지정해도 바로 이용 불가 |
| 공유 WorldSnapshotCodec + MP participant snapshot | [NetworkWorldPersistence.Capture](../../client/Assets/Wildfolk/Runtime/Networking/NetworkWorldPersistence.cs#L32) | 공유 terrain/시설 codec은 재사용 가능; Player mirror/host metadata 추가 필요 |
| Client join은 host 파일/동료 진행의 권위가 아님 | [NetworkInventory.Execute](../../client/Assets/Wildfolk/Runtime/Networking/NetworkInventory.cs#L79) | Join한 world를 로컬 소유 목록으로 복제한 것처럼 표시 금지 |
| Host migration 없음; host 종료→세션 종료 | [NetworkSession.Leave](../../client/Assets/Wildfolk/Runtime/Networking/NetworkSession.cs#L139) | world 관리와 방 세션 수명을 분리; client exit 의미 따로 |
| GoldSources2개; 표시 max Gold, Purchase first eligible source | [ClassProfiles.GoldAvailable](../../client/Assets/Wildfolk/Runtime/Persistence/ClassProfiles.cs#L89) | world가 늘면 자금 출처 선택 정책 필요; 임의 합산 금지 |
| Class Gold intent/PendingXP/GemReviveIntent가 WorldPath 참조 | [ClassRewardPersistence.Recover](../../client/Assets/Wildfolk/Runtime/Persistence/ClassRewardPersistence.cs#L29) · [CloudGemReviveWallet.Reconcile](../../client/Assets/Wildfolk/Runtime/Accounts/CloudGemReviveWallet.cs#L45) | 파일 rename/delete/move가 unresolved 거래를 단절시킬 수 있음 |

## FUTURE GOAL — 사용자가 요구한 UX

Continue는 가장 최근 플레이한 유효 world를 재개한다. Single Player는 world 선택/관리로 들어가고 같은 world를 혼자 열거나 Host할 수 있다. 다른 사람 world에 Join하는 길은 따로 둔다. solo-world/MP-world를 불필요하게 별도 보관함으로 나누지 않는다. 새 world를 만든다고 기존 world를 파괴하지 않는다. rename/delete를 안전하게 제공한다. **현재 구현 상태가 아니다.**

## 변경 종류별 분리

| 영역 | presentation만 가능한 것 | 시스템 변경/결정이 필요한 것 | 수용 기준 |
|---|---|---|---|
| 정보구조 | Play/Join/Settings의 역할·명칭·Back 시안 | Continue의 선택 repository 결정 | 마지막 유효 world와 실제 repository가 일치 |
| World card | 이름/최근일/일차/호스트 가능 상태의 mock 구조 | world index·stable identity·metadata·유효성 열거 | 손상/backup/미호환/사라진 파일을 구분 |
| 생성 | form·취소·확인 hierarchy | 파일 위치/중복 이름/seed 정책/슬롯 commit | 생성 실패가 기존 world/index를 파괴하지 않음 |
| rename | 편집/성공·실패 frame | 표시명 metadata와 파일명 분리, pending path 유지 | 순수 rename으로 receipt·save reference가 안 바뀜 |
| delete | 명시 대상·영구영향·재확인·cancel 디자인 | live session lock·pending 거래·backup/trash 정책 | active/pending 대상 삭제 제한, 실패 recoverability |
| 같은 world solo/host | Play alone/Host 액션 선택 디자인 | IsMultiplayer/HostId/participants 전환·소유·신규 참가자 정책 | host player mirror·개인 inventory/life/home가 유지 |
| Join | 별도 entry·code/address input | Session 연결은 기존 경계 재사용; remembered remote list는 신기능 | local ownership과 구분, host-left recovery 유지 |
| Cloud | 명확한 계정/세계 저장 범위 문구 | 현재 CloudProfile은 세계 업로드 아님; world cloud는 별도 설계 | 지원 안 하는 world sync를 약속하지 않음 |

## 같은 solo world를 Host로 열 수 있는가

SOURCE-INFERRED: terrain/시설/resource/boss snapshot codec을 공유하므로 재사용의 출발점은 있다. 그러나 현재 Host는 multiplayer metadata와 로컬 HostId를 요구하고 SP는 그 값이 없다. 기본 transport/scene만 바꾸면 안전하게 된다는 결론은 성립하지 않는다. 변환(또는 공통 world envelope) 정책, owner character를 어느 participant로 삼을지, remote 재입장, solo 복귀 시 다른 참가자의 보존 방식, save validation, pending journal reference를 설계해야 한다.

PROPOSAL: world의 stable ID와 표시 이름/최근 플레이 metadata를 별개로 관리하고 session mode는 열 때 선택하는 방향을 우선 평가한다. 이는 확정 schema가 아니다. Save14 payload를 유지한 외부 index만으로 충분한지, 새 envelope/schema가 필요한지는 위 identity/participant invariant를 정한 뒤 결정한다. Protocol23/Save14 버전을 지금 임의로 올리지 않는다.

## 아직 정해야 할 제품 결정

1. 최근 플레이의 정의: load 성공/실제 world 진입/정상 save 중 언제 기록할지, 잘못된 recent entry fallback.
2. world 이름 길이·중복 허용·KO/EN 입력·금칙/경로문자 처리. 표시 이름을 파일 경로로 직접 쓰지 않는 방향 권고.
3. Guest/linked 프로필 교체와 local Host identity 변경의 관계; ownership 이전을 단순 rename처럼 취급하지 않기.
4. Gold class 구매의 payer world 선택과 전체 표시 방식. 현재 max/first eligible 동작을 무단 합산 wallet으로 바꾸지 않기.
5. world 삭제 시 backup/trash/보관기간·복구 UI, 미확정 revive/Class 거래를 먼저 해결하는 정책.
6. 여러 world를 동시에 열 수 있는지, 같은 파일의 solo/Host 동시 사용 lock, 불완전 종료 시 recoverability.

## Migration·테스트 계획 — 이번에는 미실행

기존 고정 SP/Host 파일과 backup을 읽기/검증한 뒤 새 위치/index를 준비하고, 새 상태를 검증하기 전 원본을 제거하지 않는다. pending intent를 모르면 자동 이동/삭제하지 않는다. 기존 participant/SaveId/seed/generation/boss reward/night identity와 Class receipt를 보존한다. 기존1–13→14 reader 및 checksum/backup 정책을 유지한다.

검수: primary 손상/backup 정상, 두 slot 동시에 존재, 새 생성 실패, 중복·긴 이름/경로문자, rename 도중 종료, 삭제 취소·잠긴 world, pending Gold/ClassXP/GemRevive, solo→Host→solo 왕복, remote reconnect/Host 종료, dead/downed normalization, 기존1/2/3 generation과 session identity. 이런 테스트가 없이 Penpot world card가 생겼다는 이유로 multi-world READY라고 하지 않는다.
