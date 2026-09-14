# Stackfall Mobile 초기 데이터 스키마 정본 V0

상태: `Phase 0`

목적: Unity 구현 전에 ID, 참조 관계, 밸런스 데이터의 책임을 고정한다. 새 콘텐츠를 추가할 때 핵심 코드를 반복 수정하지 않도록 데이터 주도 구조를 사용한다.

## 1. 기본 원칙

- 정적 게임 데이터는 코드에 직접 흩어 쓰지 않는다.
- 모든 콘텐츠는 안정적인 문자열 ID를 가진다.
- 표시 이름은 ID와 분리한다.
- 클라이언트 표시용 값과 서버 권위 값의 정본을 구분한다.
- 확률/보상/상점/가챠 데이터는 서버에서도 검증 가능해야 한다.
- 데이터 버전과 스키마 버전을 분리한다.
- 삭제된 ID를 다른 의미로 재사용하지 않는다.

## 2. ID 규칙

예:

- `skill.core_pulse`
- `part.core.overload_01`
- `enemy.scrap_drone`
- `boss.gravity_warden`
- `stage.ch01_01`
- `banner.prototype_overload_001`
- `reward.stage.ch01_01.first_clear`

규칙:
- 영문 소문자 + 숫자 + `_` + `.`
- 표시용 한국어/영어 이름을 ID에 넣지 않음
- 숫자만으로 의미를 판단하지 않게 접두어 사용

## 3. StageDefinition

필수:
- `id`
- `chapterId`
- `index`
- `mode`
- `targetDurationSec`
- `waveSetId`
- `bossId`
- `recommendedPower`
- `firstClearRewardId`
- `repeatRewardId`
- `unlockRequirements`

선택:
- `midBossId`
- `environmentRuleIds`
- `specialObjectiveId`

## 4. EnemyDefinition

- `id`
- `role`: swarm / melee / ranged / controller / tank / support
- `baseHp`
- `baseDamage`
- `moveSpeed`
- `collisionRadius`
- `behaviorId`
- `rewardXp`
- `rewardDropTableId`
- `tags`

실루엣/연출 데이터와 전투 수치를 한 거대한 파일에 합치지 않는다.

## 5. BossDefinition

- `id`
- `baseHp`
- `baseDamage`
- `phaseThresholds`
- `patternSetId`
- `enrageRuleId`
- `immunityTags`
- `rewardTableId`
- `rankingWeight`

패턴은 별도 PatternDefinition으로 분리해 재사용 가능하게 한다.

## 6. SkillDefinition

- `id`
- `family`
- `maxRank`
- `baseBehaviorId`
- `rankEffects[]`
- `tags`
- `targetingProfileId`
- `visualProfileId`

각 Rank 효과는 숫자 변경과 행동 변경을 명확히 분리한다.

예:
- `damageMultiplier`
- `cooldownMultiplier`
- `addPierce`
- `enableSplit`
- `behaviorModifierIds`

## 7. PartDefinition

- `id`
- `slot`: core / frame / drive / impactor / orbiter / reactor
- `seriesId`
- `baseRarity`
- `isPrototype`
- `baseStatProfileId`
- `uniqueEffectIds`
- `setId`
- `appearanceProfileId`
- `upgradeTrackId`
- `salvageValueProfileId`

## 8. PartSetDefinition

- `id`
- `memberPartIds`
- `bonus2`
- `bonus4`
- `bonus6`
- `themeTags`

세트 효과는 부품 자체 효과와 중복 정의하지 않는다.

## 9. UpgradeTrackDefinition

- `id`
- `slotLevelMax`
- `levelCosts[]`
- `breakthroughCosts[]`
- `rarityMultipliers`
- `duplicateRequirements`
- `effectUnlocks`

슬롯 레벨과 아이템 돌파 트랙을 분리한다.

## 10. BannerDefinition

- `id`
- `bannerType`
- `startAt`
- `endAt`
- `currencyId`
- `singleCost`
- `tenCost`
- `poolId`
- `sRate`
- `epicRate`
- `pityLimit`
- `featuredSetIds`
- `featuredWeight`
- `guaranteeRuleId`
- `pityCarryGroup`
- `targetTuningEnabled`

클라이언트는 이 값을 보여줄 수 있지만 실제 추첨 결과는 서버가 확정한다.

## 11. LootPoolDefinition

- `id`
- `entries[]`

entry:
- `itemId`
- `weight`
- `minAmount`
- `maxAmount`
- `conditions`

검증:
- 음수 weight 금지
- 비어 있는 pool 금지
- 참조 item 존재 여부 확인

## 12. RewardDefinition

- `id`
- `entries[]`
- `choiceGroup`
- `serverGrantPolicy`

보상은 UI가 직접 생성하지 않고 보상 ID를 통해 서버/클라이언트가 같은 내용을 해석한다.

## 13. ChallengeDefinition

- `id`
- `type`
- `entryRuleId`
- `rewardTrackId`
- `rankingRuleId`
- `resetScheduleId`
- `powerScalingProfileId`

## 14. EconomyDefinition

재화:
- credit
- crystal
- part_material
- breakthrough_material
- tuning_data
- guild_coin
- arena_ticket
- boss_ticket

각 재화:
- source 목록
- sink 목록
- 보유 상한 여부
- 서버 권위 여부

프리미엄 재화와 현금 상품 관련 값은 서버 권위.

## 15. AccountProgress

서버 저장 후보:
- accountVersion
- playerLevel
- stageProgress
- currencies
- slotLevels
- ownedParts
- partAscensions
- presets
- unlockedFeatures
- pityStates
- mailClaims
- challengeStates
- guildId
- seasonStates

## 16. BattleSession

서버가 발급:
- `sessionId`
- `accountId`
- `stageId`
- `seed`
- `startedAt`
- `loadoutSnapshotHash`
- `entryCostReceipt`

클라이언트 제출:
- `completed`
- `elapsedTime`
- `bossDamage`
- `kills`
- `resultDigest`

서버는 가능한 범위에서 비정상 값을 검증하고 최종 보상을 확정한다.

## 17. Guild/Chat

Guild:
- guildId
- name
- tag
- country
- level
- exp
- members
- research
- bossProgress
- permissions

ChatMessage:
- messageId
- channelType
- channelId
- senderId
- sentAt
- text
- moderationState

개발자 로그/stack trace를 ChatMessage로 생성하지 않는다.

## 18. 데이터 파일 구조 후보

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

초기에는 JSON 또는 Unity에서 안정적으로 가져올 수 있는 텍스트 데이터로 관리한다. ScriptableObject만을 유일 정본으로 두어 diff/검증이 어려워지는 구조는 피한다.

## 19. 자동 검증 필수

- ID 중복
- 누락 참조
- 잘못된 enum
- 음수 HP/피해/비용
- 확률 범위
- 가챠 확률 합
- pityLimit 0 이하
- 시작일 > 종료일
- reward/loot 빈 목록
- 존재하지 않는 부품이 세트에 포함됨
- stage에 존재하지 않는 boss 참조
- 순환 unlock dependency

## 20. 버전 관리

- `schemaVersion`: 구조 변경
- `dataVersion`: 밸런스/콘텐츠 변경

구조 변경 시 마이그레이션 또는 하위 호환 처리 필요.

## 21. Phase 1 적용 순서

첫 Unity 프로토타입에서는 전체 스키마를 한 번에 구현하지 않는다.

우선:
1. SkillDefinition
2. EnemyDefinition
3. StageDefinition
4. BossDefinition
5. 최소 PartDefinition

전투 코어가 안정된 뒤 경제/배너/길드 데이터를 연결한다.
