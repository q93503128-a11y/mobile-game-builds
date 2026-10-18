# Exploration Foundation 1 검수 기록

2026-10-18. 기준4502ab8702dec45aee9bb656c04a12e66980f75a에서 구현·Unity 검수·최종 소스/문서 검토를 마쳤다. 사용자 사용량 요청에 따라 중간에 저장 후 중단했고 재개 시 통과한 전체 검사를 반복하지 않았다. Git 전달 상태는 이 파일을 포함하는 main 이력에서 확인한다. 상세 원시 측정·실패/재검사 이력은 [JSON](validation/exploration-foundation.json), 실제 Game View 증거14장은 [이미지 폴더](images/exploration/)에 있다.

## 검사와 실제 조작

- 최신 개별 결과 기준 EditMode117/117, 관련 PlayMode47/47. 통합 실행 후 실패/변경 사례만 재검사한 합계이며 최종 전체 단일 실행 결과는 아니다. 전체 PlayMode118개를 재실행하지 않았다. NUnit 실행 합계222.83초이며 Editor 시작/임포트 시간은 별도다.
- 초기 통합의3 Edit 실패는 protocol/설치물 수의 구 기대값과 비전투 Compass의 Tool 가정,1 Play 실패는 Load 직후 동기 chunk 생성을 가정한 fixture였다. 실제 의미를 보존하도록 검사와 명시적 fixture 동기화를 수정했고 해당 사례를 재검사했다. 초기 authoring using/테스트 asmdef 참조 누락도 컴파일 후 교정했다.
- 실제 MCP: Torch/Compass 제작과 실제 카드 버튼, Home 지정, 낮/밤,4개 지상 biome, 동굴 입구/중층/심부 무광원·휴대·설치3개, portal6회, 연속244.52unit 이동, Save/Load, Local Host 분리2지역18·4논리지역36, 중복 설치 거절, 저장 후 재Host ID 유지.
- 연속 이동 fixture는 기존 Player Motor 입력을 사용하되 청크 경계를 분리 측정하려고 Collider를 잠시 비활성화했다. 장애물 통과 테스트나 일반 플레이 완주로 해석하지 않는다.
- 발견 후 수정: 뒤집힌 손 불꽃, 낮 과발광, Global Light 참조 오연결,1280 설명문 겹침, Client 야외 조명용 Host 시간 sample, 초기 동기 chunk 로드 가정. 기존 씬 객체 ID는 모두 보존하고 필요한 카드/버튼/HUD만 추가했다.
- 최종 Console 오류0, 컴파일 진행 없음, MainMenu/Play 중지/비포커스. 정본과 live Assets의 의미 있는 차이0. 소스 검사763 GUID, 참조·Landscape·금지 UI 텍스트 및 git diff --check 통과. Windows 입력·창 Focus/Show/GetWindow는 사용하지 않았다.

## 측정 수치와 한계

| 상황 | 관측 수치 |
|---|---|
| 새 SP 실제 저장 파일 |32,753 bytes|
| 연속 탐험 후 저장 |63,580 bytes|
| 확장 탐험 후 저장 |154,049 bytes: initialized chunk78, 자원 delta30, 시설6, 바닥 stack30, 저장 동물501|
| Host fixture 저장 |148,813 bytes|
| 연속 이동 Editor frame |평균10.19ms / P95 17.31ms / 최대89.93ms|
| 연속 이동 활성 상태 |안정9 / 관측 최대12 chunk, 측정 종료2458 GO·405 자원·75 생물|
| 연속 이동 Editor GC |평균454,547 bytes/frame / 최대4,352,909 bytes/frame|
| 분리4논리지역 Local Host |36 chunk /6843 GO /1256 자원/72 생물|
| 분리4논리지역 Editor frame |평균9.96ms / P95 16.35ms / 최대25.91ms|
| 분리4논리지역 Editor GC |평균271,495 bytes/frame / 최대1,481,675 bytes/frame|
| 지형 예산 분할 최대 관측 |SP3.41ms / Host3.76ms (목표2.5ms, row/upload 초과 가능)|
| 비활성 지형 cache |LRU 최대24개;4논리지역 측정 texture4,643,476 bytes|
| 재방문 terrain BuildChunk |기존48.57~59.46ms 생성 / cache hit 첫3.41ms, 이후0.037~0.051ms|

캐시 수치는 지형 부분만이며 자원/시설/AI 전체 활성화 비용이 아니다. 측정은 SP1920×1080/Host1280×720의 PC Editor이고 Intel Iris Xe를 사용했다. Editor/MCP 및 일부 background 검사 비용이 섞인다. 전체 Editor GC를 순수 게임 allocation으로 보거나, 원인 미분리 최대89.93ms spike를 해결했다고 주장하지 않는다. 모바일60fps 합격이 아니다. Mono의 GetAllocatedBytesForCurrentThread가0을 반환한 baseline은 allocation 근거로 쓰지 않았다.

미방문42k chunk를 직렬화하지 않으며 비재생 광물 tombstone·시설 cargo·Home·Torch ID가 재방문/Load/reHost 뒤 유지됐다. 시설 재활성의 elapsed game time 처리로 Steel2를 확인했다. 저장 크기의 장기 선형 성장(바닥 아이템·방문 생물·고갈 기록)은 남는다. 임의 TTL 삭제 정책은 도입하지 않았다.

## 저장 파일 사건

초기 검수의 임시 repository 적용 전 컴파일 실패로 AutoSave가 원본 primary를 한 번 교체했다. .backup에 남은 시작 원본을 byte 단위로 복원했다. 최종 primary SHA-256은 시작과 같은57d560f19e49507fbc7ac00b48e1dda5759732a181d1c0baa441ad4536da7970이다. 그 이전의 더 오래된 backup은 순환됐고 복구하지 못했다. 이후 검수는 임시 저장 경로를 사용했다. primary 진행 보존과 과거 backup 이력 보존을 혼동하지 않는다.

## 다음 범위

실제 Android/iOS·remote Relay·4개 연결 peer·장기 단말 성능/재미/밸런스는 미검수다. 분리 actor fixture는 실제4인 접속이 아니다. 기존 MP HUD와 모달 위 HP 표시의 시각 완성도, Editor frame spike/GC의 빌드 단위 원인 분리, 장기 dropped item/생물 상태 저장 정책이 후속 품질 과제다.

WBIII·Diamond·실제 Lantern progression·전체 Map은 미구현이다. 이번 검수에서 Workbench III/Diamond 구현 진입을 막는 기능 결함은 발견하지 못했다. 출시 progression 완성 또는 출시 성능 합격을 의미하지 않는다.

## 시간 기록

중단 전 세션: KST09:43:37~10:28:22,44분45초.
사전 감사·조사·기준측정: 약3분.
초기 구현·에셋 반영: 약9분.
자동검사 순수 실행 합계:3분42.83초 (Editor 시작/임포트 제외).
MCP 시각·성능 검수: 약30분 (다른 단계와 병행).
중단 전 문서·정리: 약22분 (다른 단계와 병행).
재개 후 최종 검토·문서·Git: KST15:38:03부터; 종료 시간은 최종 보고에 기록한다.
단계별 근사 구간은 겹치므로 합산하지 않는다. 사용자 중단 대기 시간은 작업 세션 합계에서 제외한다.
