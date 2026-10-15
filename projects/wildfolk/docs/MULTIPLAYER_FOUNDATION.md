# Multiplayer Foundation 1 — 2026-10-15

기준 663c0cfb7e33bd9e50a50c4c675afe5a9e6577ee. 이번 범위는 Home 보조 메뉴 소수정과 실제 2인 Player 상태 slice다. 기존 생존 게임 전체를 멀티로 전환한 패스가 아니다.

![실제 Host Game View](images/network-two-player-host.png)

[Home 텍스트 메뉴](images/network-home-text-menus.png) / [실제 Relay Host 메뉴](images/network-host-menu.png) / [두 프로세스 검수 데이터](evidence/network-foundation.json)

## 실제 제공 흐름

Home → Multiplayer → Internet(Room Code) 또는 Local Network → Host Game / Join → Host의 Enter Game → 협동 야영지(Multiplayer 씬)다. 기존 Single Player/Continue/Settings는 보존한다. 수첩 안 Single Player/Multiplayer/Settings는 기본 배경 박스를 제거하고 hover/focus/press 때만 약한 tint를 준다. Continue의 Primary 박스/배경/로고는 유지한다.

Internet Host는 공식 Relay allocation(Host 외 최대3명)을 만들고 GetJoinCodeAsync의 실제 결과만 표시한다. Join은 입력을 정리/형식 검사한 뒤 JoinAllocationAsync와 실제 Transport 접속을 수행한다. 가짜 코드나 IP를 Room Code로 표시하지 않는다. 대기·잘못된 코드·접속 실패·정원 초과·Host 종료는 en/ko 제품 문구로 표시하며 SDK 내부 메시지는 UI에 출력하지 않는다.

Local Network는 별도로 표시하며 IPv4[:port] 입력을 사용한다. 기본 포트7777, Host listen0.0.0.0, 같은 PC 검수 주소127.0.0.1이다. 로컬 주소는 인터넷 참가 코드가 아니다. 이번 실제 두 프로세스 검수는 로컬로 대체하지 않고 Relay/DTLS를 사용했다. 로컬 Host는 별도 자동 Play 검사에서 확인했다.

Host는 혼자 먼저 입장할 수 있고 접속 중인 Client/늦게 참가한 Client를 NGO SceneManager가 같은 씬으로 동기화한다. Host가 종료하면 Client도 연결을 정리하고 Home으로 돌아가 세션 종료 안내를 제공한다. Client만 나가면 해당 Player가 despawn되고 Host는 계속한다. Host migration, dedicated server, 자동 재접속은 없다.

## Networking stack와 외부 설정

- Netcode for GameObjects2.13.3
- Unity Transport2.7.4
- Multiplayer Services SDK2.4.0
- Authentication3.8.0/Core1.18.0은 패키지 의존성이다.

Unity 공식 Registry의 Unity6000.0 호환 버전을 확인하고 manifest/lock을 고정했다. [공식 Relay+NGO 가이드](https://docs.unity.com/en-us/mps-sdk/tutorials/relay-and-ngo)의 unified SDK/DTLS allocation·join 흐름을 사용했다. [NGO 문서](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/index.html)와 설치된 공식 package source에서 RPC invoke permission, scene events, prefab 등록, server NetworkTransform/NetworkRigidbody2D API를 확인했다.

기존 프로젝트가 Unity Cloud에 연결돼 있었고 실제 익명 인증/Relay 발급/참가가 성공했다. 현재 환경에서 추가 Dashboard/로그인 사용자 작업은 필요하지 않았다. 익명 인증은 Relay 접속을 위한 SDK bootstrap이며 제품의 Account/Login/Friend/Invite 기능을 구현한 것은 아니다. 다른 Unity Cloud 프로젝트로 옮길 때는 해당 프로젝트 연결/Authentication·Relay 서비스 설정이 필요하다. 서비스 사용 한도/다른 계정 환경/실제 모바일 기기는 이번 검증과 구분한다.

IRoomConnection이 RelayRoomConnection/LocalRoomConnection을 분리한다. 향후 Account/Friend/Invite/Room 경로를 바꾸더라도 Player 권위/이동 코드를 다시 구현하는 구조가 아니다.

## 권위와 동기화

NetworkSession root의 NetworkManager/UnityTransport 한 개만 씬을 넘는다. Home 진입마다 중복 생성하지 않는다. prefab은 양쪽 프로세스에서 StartHost/StartClient 전에 같은 PlayerPrefab 참조로 명시 등록한다. SDK가 자동 생성한 DefaultNetworkPrefabs.asset은 Editor authoring용 기본 목록이며 Runtime 등록의 정본은 NetworkSession.PlayerPrefab이다.

NetworkPlayer는 기존 Player prefab의 variant다. 실제 이동·Sprint·Dash·HP·장비 표현 코드를 복사하지 않는다. PlayerInputCommands의 IPlayerMovementAuthority 경계만 분리했고 싱글은 기존 직접 적용 경로를 사용한다. 입력 adapter는 owner에게만 활성화한다. replica에서는 Motor/Locomotion/Stats/Survival 시뮬레이션을 실행하지 않는다.

Host는 owner 입력 RPC만 수락하며 sender/ownership을 확인하고 유한한 방향 벡터만 크기1 이하로 정규화한다. Client는 위치·HP·Armor를 RPC로 지정하지 않는다. Host에서 기존 PlayerMotor/PlayerLocomotion/StaminaModel로 이동/스프린트/충돌 대시를 실행한다. 입력이 끊기면0.4초 뒤 정지한다. 같은 prefab/콘텐츠 버전의 실행을 전제로 하며 Catalog 순서/프로토콜 호환 변경 시 version 관리가 필요하다.

| 상태 | 실제 전달 방식 |
|---|---|
| spawn/despawn | Host의 NGO PlayerObject/ownership 및 접속 lifecycle |
| position/movement | server NetworkTransform + NetworkRigidbody2D, 위치 XY, 보간/threshold/unreliable delta |
| facing / moving / sprint / dash | server write Motion 상태, direction과 bit flags 변경 때만 dirty |
| HP / Maximum HP / Head / Body / held 외형 | Health/Equipment Changed 이벤트의 server write Status, Catalog 인덱스 |
| 보행 표현 | 공통 PlayerAvatar/CharacterFacing, replica는 읽은 이동 상태로 bob/방향 표현 |

네트워크 tick은20Hz다. 입력은 변경 또는0.15초 heartbeat만 전송하고 Dash는 행동 시 reliable owner RPC다. HP/Armor/held는 매 프레임 polling하거나 Inventory 배열을 보내지 않는다. Transform 외 모든 데이터를 매 프레임 전체 전송하지 않는다. 상태 replica는 표시용이며 별도 Inventory/Save 정본이 아니다. 고지연 예측/재조정/장거리 부하 최적화 완료를 주장하지 않는다.

## 야영지와 제외 범위

Multiplayer 씬은 기존 ground/tree/campfire/log Sprite와 유한한 경계, 카메라, joystick/dash/HP/Leave UI만 제공한다. WorldNavigation.StaticAreaSize를 통해 기존 이동·대시의 지형/충돌 경계를 재사용한다. WorldGenerator/WorldSaveService/AnimalPopulation/Creature AI/Resource/시설 작업은 생성하지 않는다. Creature 전투/플레이어 PvP/채집/파괴·복구/바닥 아이템/전체 Inventory/제작/Chest/가공/건축/Door/Loot/멀티 World persistence는 없다.

4명 admission을 수용하는 구조지만 실제 이번 검증은2명이다. 친구/초대/로그인/계정/Cloud Save/Host Migration/Dedicated Server/4인 부하 테스트는 미래 범위다. 살아 있는 전체 싱글 월드를 접속자마다 중복 실행하는 방식이 아니다.

## 실제 두 프로세스 Play 검수

기존 LIVE Unity Editor = Host A, 기존 tests/artifacts/unity를 재사용한 hidden batchmode 그래픽 Unity Editor = Client B로 실제 동시에 실행했다. 새 clone/대화형 Editor 창을 만들지 않았고 OS 입력/Unity Focus/Show 호출은 하지 않았다. 일반 NUnit 자동검사와 별도의 실제 NGO+Relay 접속이다. Editor 전용 NetworkReviewHarness가 기존 입력 source를 주입하고 양쪽의 실제 Player 상태를 읽었다.

실제 확인 결과:

- Relay 코드 발급/Client 참가/같은 씬, 서로 다른 owner Player2개 생성.
- A 이동을 B의 실제 actor에서, B 이동을 A의 actor에서 확인. 양쪽 방향 변화와 보행 상태 확인.
- A/B Sprint와 Dash flags 및 실제 위치 변화를 각각 확인.
- Host에서 A HP73/Iron Head·Body/Wood Sword, B HP58/Leather Head·Body/Axe를 적용하고 양쪽 HP/장비 데이터와 실제 SpriteRenderer를 확인.
- Host1920×1080 Game View에서 두 캐릭터·HP바·장비·joystick/dash/Leave UI를 확인. 대시 Sprite 참조와 Canvas 정렬을 수정해 나무가 UI를 덮지 않게 했다.
- Client 카메라의1920×1080 offscreen 렌더에서도 원격/로컬 Armor·held 표현을 보았다. 이 캡처는 batchmode viewport와 Canvas 비율이 달라 Client UI 레이아웃 합격 자료로 사용하지 않았다. 캡처 시 native ALLOC_TEMP 진단1건이 나와 해당 custom render capture 코드는 최종 Editor 도구에서 제거했다. 최종 제품의 기기 성능/메모리 합격을 주장하지 않는다.
- Client 종료 → Host player1개/세션 유지. 같은 실제 코드로 늦게 재참가 → 기존 Host HP/장비를 수신.
- Host의 실제 Leave 버튼 → 양쪽 Home/actor0/Idle, Client net.host_left 안내 상태.

초기 연결 검수 중 batchmode PlayerLoop 진행과 NetworkPrefab 등록 누락을 수정했다. Editor-only 검수 pump로 백그라운드 Play를 진행하고 runtime prefab 등록을 세션 시작 경로로 고쳤다. 수정된 Runtime으로 위 이동/상태/종료 검수를 수행했다. 큰 회귀 전체를 수정마다 반복한 것은 아니다.

싱글 sanity는 별도 검수 Repository에서 실제 New Game→이동→Inventory→Pause Save & return home을 확인했다. mode=Singleplayer, movement authority adapter=null, network Player0/Manager not listening, Preferences1이었다. Home Continue 가능 상태와 원래 KO/Control1.20191717/Master1/Music1/SFX1을 유지했다. 실제 UserData/world-v1.json 전후 SHA256은57D560F19E49507FBC7AC00B48E1DDA5759732A181D1C0BAA441AD4536DA7970로 동일했다. 이 파일의 이전 패스 해시와 다름은 이번 작업 시작 전 사용자 저장 상태이며 이번 검수로 덮지 않았다.

## 최소 자동검사

- EditMode NetworkFoundationTests + FrontendContractTests:5/5 통과,0.1539864초.
- PlayMode NetworkFoundationPlayTests + FrontendPlayTests:5/5 통과,10.5900562초.
- validate_visual_project.py: PASS,554 unique asset GUIDs/참조/표시 문구/기존 Landscape 구조.

EditMode는 비유한 입력·입력 크기 제한, 코드/IPv4 구분, server write/owner RPC permission, 기존 Home/저장 Content 참조/언어다. PlayMode는 로컬 Host의 등록/씬/authoritative spawn/이동·대시·정리와 기존 Home의 저장 상태/버튼/공유 설정/정적 Home의 최소 사례다. 전체 Creature/모든 Save/전체 프로젝트 회귀가 아니다. XML/로그는 tests/artifacts/network-edit.*, network-play.*로 Git 제외다.

NetworkReviewHarness는 Editor 폴더에만 있으며 제품 빌드에 포함하지 않는다. CLI Client(directory/real room code)와 Monitor(directory,role)를 제공한다. command JSON의 Sequence/X/Y/Dash/Leave/Quit와 명시적 Fixture가 있는 경우의 Host 전용 HP/장비만 처리한다. telemetry는 atomic replace로 쓴다. OS 키·마우스를 주입하거나 raw 네트워크 오류를 UI로 출력하는 도구가 아니다.

실제 Android/서로 다른 기기·회선/고지연/패킷 손실/장시간/4인 부하/서비스 한도/공격·전체 생존 동기화는 미검증이다. UI를 포함한 양쪽 모바일 실기 합격이나 완성된 멀티 생존 게임으로 표현하지 않는다.

## 시간 기록(KST)

실제 시각의 경과 시간이며 생각/코드/컴파일·기동 대기를 포함한다. 중복 실행 시간을 두 번 합산하지 않았다. 가장 긴 구간은 실제 두 프로세스의 초기 접속/프리팹 등록/백그라운드 Play 보정과 검수다. 최초 그래픽 Client는 새 networking 패키지 Import/컴파일·기동에 수 분이 걸렸고 같은 검수 프로젝트를 재사용했다. 종료 동선 및 수정 확인 때문에 Client 프로세스를 재시작했으며 새 clone을 만들지 않았다.

정본·공식 자료·패키지 확인:20:43:48→20:50:40,6분52초.

구현·첫 컴파일/연결 시도:20:50:40→21:03:21,12분41초.

실제 두 프로세스 검수·관련 수정:21:03:21→21:32:06,28분45초.

싱글 sanity:21:32:06→21:34:08,2분02초.

검수 도구 정리·최소 자동검사:21:34:08→21:39:24,5분16초(문서 작업 병행 포함).

문서 최종 검토:21:39:24→21:44:52,5분28초.

Git 마무리는21:44:52부터다. commit/push/원격 확인과 전체 경과는 완료 보고에 실제 종료 시각으로 추가한다. 각 소요 시간은 한 줄에 한 단계만 표기한다.
