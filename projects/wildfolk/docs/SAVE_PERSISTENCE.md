# Save / Load v1

정본은 WorldSaveService / SavedGame / SaveRepository / SaveValidation이다. 수동 Save game / Load game은 Pause 메뉴에 있다. 기기 Pause/Background/Quit에는 살아 있는 세션을 자동 저장한다. 새 게임의 첫 시작은 기존처럼 기본 Seed이며, 저장 게임은 명시적인 Load로 불러온다. 사망/재시작 규칙과 사망 보상 정책은 변경하지 않았다.

## 저장 범위

- Player: 위치, HP, Hunger/Thirst, 생존 피해 타이머, Raw Water 불이익 남은 시간, 전투 후 회복 대기, Inventory 슬롯/Item ID/수량, 선택 슬롯, XP/Gold.
- World: Seed, 누적 게임 시간. Day/Phase는 같은 DayNightDefinition으로 복원한다.
- Resource: 기본 배치의 안정적인 r<index> ID를 사용하고, 손상/파괴/재생 대기/재생 후 위치만 Delta로 기록한다.
- Animal: 종별 지역 Population Slot과 살아 있는 상태 또는 재생 대기 시간.
- Facility: 고유 Save ID, Item ID, 위치/회전/HP, Door 열림, Chest24칸, Processing Recipe/Batch/남은 시간.
- WorldPickup: Item ID/수량/위치. 로드 후 즉시 자동 획득하지 않도록 기존 지연 흐름을 사용한다.

Seed 기반 지형·물·길·전체 자원 생성 결과를 통째로 저장하지 않는다. 로드할 때 Base World를 만든 뒤 Delta와 시설을 적용한다. 소분 중인 UI의 들고 있는 표시와 적 AI 순간 상태는 저장하지 않는다. 소분은 배치 전까지 원본 모델을 바꾸지 않으므로 중단해도 아이템은 남는다. Zombie/Goblin은 로드 후 현재 시간의 Spawn 규칙에 따라 다시 구성된다.

## 파일 안정성

Version1 + JSON Payload + SHA256 검증값을 사용한다. Temporary 쓰기/Flush → 파일 재읽기 및 데이터 검증 → atomic replace 순서이며 이전 정상 파일은 backup으로 보존한다. 최초 저장은 같은 디렉터리의 rename이다. 손상된 primary를 덮어쓸 때 정상 backup을 덮지 않는다. 불완전한 temporary는 로드 대상으로 사용하지 않는다.

버전/체크섬/Item ID/수량/슬롯 수/위치/HP/Resource ID/시설 ID/Recipe/처리 수량/시간을 실제 적용 전에 검증한다. Primary가 손상되면 유효한 backup을 시도한다. 지원하지 않는 버전은 적용하지 않는다. 기존 저장 파일이 없는 첫 버전이며 미래 버전은 명시적인 Migration을 추가해야 한다.

Editor는 각 프로젝트의 UserData/world-v1.json, 빌드는 Application.persistentDataPath에 저장한다. 테스트 복사본과 열린 프로젝트의 Editor 저장 위치는 분리된다. Developer Harness의 변경은 자동 저장을 끄며 명시적인 Save만 테스트 상태를 보존한다.

현재 오프라인 경과 시간을 재생/제련에 더하지 않는다. 저장한 게임 시간부터 계속한다. Cloud Save/Account Save/Multiplayer 권위 저장은 없다. 데이터·Seed 생성 알고리즘의 호환성이 변경될 때 Version/Migration을 함께 변경해야 한다. Android의 실제 강제 종료/파일 시스템 동작은 실기 검증 대상이다.
