# 제안: 현재 지형을 보존하는 물 표현 재작성

**PROPOSAL ONLY — 미승인/미구현.** 이번 감사의 [실제 비교와 공식 이미지 사례](REVIEW.md)를 근거로 한 후속 작업 범위다. 원래 감사 프롬프트의 주관적 재설계 승인 경계 때문에 제품 구현을 시작하지 않았다.

## 해결할 현재 문제

1. 깊은 수면 내부는 고정색이라 큰 화면에서 빈 판처럼 보인다. 현재4096점 source 표본과 실제 강/호수/해안 화면으로 확인했다.
2. 0.5unit 지면 sample과 Bilinear 경계가 물가의 큰 계단/번짐을 만든다. 과거 수면 detail을 되살리는 문제와 별개다.
3. 호수/강/얕은 바다/깊은 바다의 구분이 플레이어가 읽는 실제 통행·낚시 규칙과 어긋나면 안 된다.

## 제안 범위

- 현재 무광 자연색의 범위에서 물 내부에 절제된 world-space grain/짧은 잔물결 정보를 준다. Core Keeper의 표면 정보, Shipwrecked의 반복 무늬 원리를 참고하되 외부 palette/asset/layout을 복제하지 않는다. 구체 색·밀도·강도는 실제 비교 화면으로 검수하며 지금 확정 수치를 발명하지 않는다.
- 현재 지형의 signed water distance/깊이/해안 분류를 읽어 젖은 물가·얕은 물·깊은 물을 구분한다. 시각적으로 만든 별도 충돌/가짜 수역을 추가하지 않는다. 물가의 큰 계단/번짐을 줄이되 통행 경계는 그대로 유지한다.
- 수면 detail은 플레이어·광석·동물·적 공격 tell보다 약하게 유지한다. 큰 흰 파도/고리·네온 반사·항해를 암시하는 wake는 넣지 않는다.
- 우선 기존 청크 sampler/생성 시점의 baked 표현 경계에서 해결 가능한지 검증한다. 공용 WorldSprite shader를 바꿔 모든 sprite에 영향을 주거나 매 프레임 청크 texture를 다시 쓰는 방식은 선택하지 않는다. 별도 animated shader가 필요하다는 결론이면 그 비용/영향을 먼저 제시한다.

## 반드시 보존할 것

Seed 결정성, WorldMap/ContinentalWorldLayout의 지형·수계·여울·물 거리·통행 query, 낚시/물 채취/Pan의 대상·수량·규칙, 자원/POI/동굴/묘지의 ID와 배치, Save14/Protocol23/generation3,1/2 호환, 기존 경제/행동은 변경하지 않는다. 지형 크기·biome·도로·생물 아트·UI를 함께 재설계하지 않는다.

## 채택하지 않은 대안

|대안|이유|
|---|---|
|Gen1 renderer 전체 rollback|단일 biome 색과 단일 FordY 계산이 현재4biome/여러 여울/해안 의미와 다름|
|청크 해상도만 일괄 증가|고정색 수면에 없는 detail을 만들 수 없고 texture 메모리·생성 비용만 늘 수 있음|
|Point filter로 즉시 변경|프로젝트 전체 미술 성격과 경계를 바꿀 수 있고 수면 detail 부재는 해결하지 못함|
|새 wave/boat/reflection mechanics|현재 요청과 무관하고 전투·탐험 신호를 어지럽힐 수 있음|
|모든 sprite 공용 shader의 즉시 교체|범위가 커지고 기존 art/lighting까지 회귀 위험을 만듦|

## 구현 후 확인할 완료 조건

- 같은 seed/좌표/시각/카메라의 강·호수·해안 before/after.1920×1080,1280×720,844×390 및 낮/밤/조명 주변에서 실제 화면을 확인한다. 검수 화면으로 결정하며 테스트 통과를 상용 품질 승인으로 바꾸지 않는다.
- 기존 물/깊이/여울/통행/낚시 query의 입력·출력과 stable world fingerprint가 표현 변경 전후 동일해야 한다. generation1/2 reader와 Save/Load 경계를 관련 최소검사로 확인한다.
- 청크 경계에서 물 무늬가 끊기거나 LRU 재표시 시 위치가 변하지 않아야 한다. 월드 좌표 기준으로 표현한다.
- 같은 cache/생성 조건에서 청크 texture bytes·sampling/terrain slice 시간을 전후 측정한다. Android 성능은 실제 기기 검증 전 합격이라 하지 않는다.
- 외부 asset/code/layout 복제, 사용자 save 변경, release 활성화나 AAB 작업 없이 끝낸다.

승인은 이 범위의 후속 구현 착수에 대한 것이며, 최종 아트 합격이나 다른 REBUILD 작업의 일괄 승인이 아니다.
