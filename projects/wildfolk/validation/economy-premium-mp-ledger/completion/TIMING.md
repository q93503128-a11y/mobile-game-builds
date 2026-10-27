# 재개 단계 시간 · KST

벽시계 구간이며 서로 겹치는 process 시간을 중복 합산하지 않는다. 첫 fetch/required docs 읽기는 최초 marker 전 구간으로 미측정이다. Git 전송은 문서 생성 이후라 아래 합계에 미포함. 작업별 token 사용량은 도구 미제공으로 null이며 시간/실행횟수로 추정하지 않는다.

- Android compile/최초 SDK 검증: 13:50:52 → 13:53:27 KST, 2.57분.
- 서비스429진단/별도 death LAN 검증: 13:53:27 → 13:59:26 KST, 5.98분.
- 실제 SDK 재시도·업적 검증: 13:59:26 → 14:04:01 KST, 4.58분.
- Boss/resource LAN·QA 정리: 14:04:01 → 14:07:08 KST, 3.13분.
- 최종 문서/소스/save 검수: 14:07:08 → 14:14:18 KST, 7.16분.

재개 실측 총 벽시계 23.42분(이전 checkpoint 작업 제외). 개별 Unity tool waits는 *-process.json과 NUnit XML의 실제 duration에 남기며 병렬 구간을 다시 더하지 않는다.
