# Stackfall Mobile 정적 데이터

이 폴더는 전투/콘텐츠/경제 정적 데이터의 텍스트 정본을 둔다.

초기 구조:

```text
data/
  stages/
  enemies/
  bosses/
  skills/
  parts/
  sets/
  economy/
  banners/
  loot/
  rewards/
  challenges/
  localization/
```

## 원칙

- Unity ScriptableObject만을 유일 정본으로 사용하지 않는다.
- 사람이 diff 가능한 JSON 계열 텍스트 데이터를 우선한다.
- ID는 `docs/DATA_SCHEMA_V0.md` 규칙을 따른다.
- 실제 표시 문자열은 localization 데이터로 분리한다.
- 서버 권위 경제/가챠 값은 클라이언트 데이터만으로 확정하지 않는다.
- 임시 테스트 데이터는 제품 데이터와 같은 파일에 섞지 않는다.
- 사용하지 않는 샘플/더미 데이터는 작업 종료 시 제거한다.

## 검증

`tools/data/validate_static_data.py`가 다음 범주의 기본 오류를 검사한다.

- JSON 파싱 실패
- ID 중복
- 잘못된 ID 형식
- 음수 HP/피해/비용 등 대표 수치
- 누락된 대표 참조
- 확률 범위/합계
- 비어 있는 주요 배열

실제 스키마가 추가될수록 검증 규칙도 같은 작업에서 확장한다.
