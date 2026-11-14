# Phase 4D — 실제 Unity 캡처 인덱스

모든 이미지는 정본 Unity Editor Play Mode의 Game View에서 ScreenCapture.CaptureScreenshot으로 캡처했고, PNG 원본 해상도로 직접 확인했다. 캡처 자동 경계 검사와 시각 품질 검토는 별도 판정이다.

## 기본 화면 / 해상도

| 언어·크기 | 상태 | 이미지 |
|---|---|---|
| KO · 844×390 | 기본 인벤토리, 선택 아이템 없음 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-base.png) |
| KO · 844×390 | Wood 일반 아이템 선택 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-item-selected.png) |
| EN · 844×390 | 기본 인벤토리, 선택 아이템 없음 | [PNG](captures/phase4d-final-20261114-recheck/inventory-en-844x390-base.png) |
| KO · 1280×720 | 기본 인벤토리 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-1280x720-base.png) |
| EN · 1280×720 | 기본 인벤토리 | [PNG](captures/phase4d-final-20261114-recheck/inventory-en-1280x720-base.png) |
| KO · 1920×1080 | 기본 인벤토리 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-1920x1080-base.png) |
| EN · 1920×1080 | 기본 인벤토리 | [PNG](captures/phase4d-final-20261114-recheck/inventory-en-1920x1080-base.png) |
| EN · 844×390 | 긴 Undead King's Crown 이름과 설명 | [PNG](captures/phase4d-final-20261114-recheck/inventory-en-844x390-long-item-name.png) |

## 장비·분할 상태 — KO · 844×390

| 상태 | 확인 내용 | 이미지 |
|---|---|---|
| 장착품 선택 | 현재 장비 상태와 해제 버튼 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-equipment-selected.png) |
| 교체 후보 | 현재 장비명과 상승 비교, 교체 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-replacement-candidate.png) |
| 교체 완료 | 기존 PlayerEquipment 모델을 통한 장착 결과 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-replacement-applied.png) |
| 하향 교체 후보 | 감소 화살표와 음수 변화 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-replacement-downgrade-candidate.png) |
| 비호환 후보 | Accessory에 Body 갑옷 선택, 빨간 상태, 장착 버튼 없음 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-incompatible-candidate.png) |
| 해제 완료 | Head 해제 후 빈 상태 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-equipment-unequipped.png) |
| 빈 장비 | 네 장비 슬롯 모두 비어 있는 상태 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-empty-equipment.png) |
| 분할 모달 | Wood 15/30, 반/−1/+1/취소/확인 | [PNG](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-split-modal.png) |

16 PNG의 헤더 크기 전수 확인 결과: 844×390 12장, 1280×720 2장, 1920×1080 2장; 오류 0. 총 3,231,249 bytes.

## 전후 비교

Phase 4B에서는 장비 이름/아이콘의 겹침과 소형 해상도에서 잘 읽히지 않는 라벨이 관찰됐다. Phase 4C는 장비를 2×2 아이콘 중심으로 만들고 반복 상태 문구를 줄였다. Phase 4D는 그 상태에서 선택 아이템 정보 영역을 늘리고, 현재 장착 이름·교체 결과·상승/하락 비교·비호환 동작을 읽을 수 있게 보강했다.

- [Phase 4C KO · 844×390 교체 후보](../inventory-phase4c-commercial-ui-2026-11-07/captures/final-verified6/inventory-ko-844x390-replacement-candidate.png)
- [Phase 4D KO · 844×390 같은 교체 후보 상태](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-replacement-candidate.png)
- [Phase 4D KO · 844×390 선택 없음 기본 화면](captures/phase4d-final-20261114-recheck/inventory-ko-844x390-base.png)
- [Phase 4D EN · 긴 장비 설명](captures/phase4d-final-20261114-recheck/inventory-en-844x390-long-item-name.png)

같은 교체 후보 상태의 원본 픽셀 비교에서 4C는 작은 슬롯 캡션 중심이지만 4D는 현재 장착품 이름, 1→2 상승 비교, 교체 동작을 분리해 보여준다. 기본 화면 비교는 두 캡처의 선택 상태 차이(4C base의 현재 데이터 선택, 4D base의 빈 선택)를 감안해 레이아웃과 위계를 따로 살폈다. 상태 표시, 모달 대비, 장비 아이콘 크기도 확인했다. 검토 화면에서 텍스트/아이콘 중첩, 잘림, 화면 이탈은 발견하지 못했다. 최종 디자인 승인은 사용자 검토 전까지 pending이다.