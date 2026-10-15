# Lowfire 개발 규칙

- 작업 전 README.md와 변경 영역의 docs 정본을 읽는다. 기획은 DESIGN_BIBLE, 구조는 ARCHITECTURE, 수치는 BALANCE를 참조하며 여기에 중복 복사하지 않는다.
- 신규 시스템/UI/UX/Visual/Gameplay는 가능한 한 유사 상용 게임을 조사하고 원칙을 적용한다. Asset/Code/Layout은 그대로 복제하지 않는다. 신규 캐릭터 아트는 기존 코드/Pillow 방식을 따른다.
- 플레이어 표시 이름은 Lowfire다. projects/wildfolk 경로, Wildfolk namespace와 저장/설정 식별자는 유지한다.
- main에서 직접 작업·commit·push한다. PR과 임시 branch는 만들지 않는다.
- 정본 코드를 직접 수정한다. Patch/Hotfix/Override를 누적하지 않고 교체한 구현·죽은 코드·사용하지 않는 Asset을 정리한다. 기존 정상 기능을 임의로 삭제·축소하지 않는다.
- 플레이어 화면에 DEBUG/TEST/TODO/WIP, 내부 변수명, 개발자 메시지를 노출하지 않는다. Developer Tool은 Editor 전용이다.
- 모바일 Landscape, Safe Area, 엄지 도달 범위와 작은 화면의 가독성을 고려한다.
- 상태 변화 때만 갱신한다. 불필요한 Update/UI rebuild/GC allocation을 피한다.
- 자동검사와 실제 Unity Play 테스트를 구분하며 직접 수행하지 않은 검증을 했다고 보고하지 않는다.
- 작은 수정마다 전체 회귀를 반복하지 않는다. 일반 개발 패스는 관련 최소검사로 검증하고, 여러 패스 후 별도 통합 QA를 수행한다. 통과한 검사는 관련 변경·실패가 있을 때만 다시 실행한다.
- 구현과 관련 정본 문서를 함께 갱신하고 실제 구현/미래 방향/검증 한계를 구분한다.
- Unity MCP와 코드 작업은 허용된다. Windows 마우스·키보드/Computer Use와 Unity 창을 앞으로 가져오는 Focus/Show 호출은 금지한다.
- 단계별 시작·종료 시각과 경과 시간을 기록하고 완료 보고에 요약한다. 도구 대기는 측정 가능한 범위에서 구분하며 겹친 시간을 중복 합산하거나 미측정 시간을 추정치 없이 단정하지 않는다.
