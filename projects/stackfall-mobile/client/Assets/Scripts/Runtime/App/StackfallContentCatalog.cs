using System;
using System.Collections.Generic;

namespace StackfallMobile.Runtime.App
{
    public readonly struct AbilityPreview
    {
        public AbilityPreview(string name, string role, int unlockStage, bool startsUnlocked)
        {
            Name = name;
            Role = role;
            UnlockStage = unlockStage;
            StartsUnlocked = startsUnlocked;
        }

        public string Name { get; }
        public string Role { get; }
        public int UnlockStage { get; }
        public bool StartsUnlocked { get; }
    }

    public readonly struct UnlockPreview
    {
        public UnlockPreview(int stage, string name, string category)
        {
            Stage = stage;
            Name = name;
            Category = category;
        }

        public int Stage { get; }
        public string Name { get; }
        public string Category { get; }
    }

    public static class StackfallContentCatalog
    {
        public static readonly IReadOnlyList<AbilityPreview> ActiveDeck = new[]
        {
            new AbilityPreview("코어 펄스", "집중 사격", 1, true),
            new AbilityPreview("펄스 블레이드", "근접 공전", 1, true),
            new AbilityPreview("중력 우물", "집속 제어", 1, true),
            new AbilityPreview("별빛 캐스터", "고속 연사", 3, false),
            new AbilityPreview("혜성 창", "직선 관통", 7, false),
            new AbilityPreview("성좌 프리즘", "보스 집속", 10, false),
            new AbilityPreview("이온 사슬", "연쇄 정리", 14, false),
            new AbilityPreview("성운 부메랑", "곡선 왕복", 18, false)
        };

        public static readonly IReadOnlyList<AbilityPreview> SupportDeck = new[]
        {
            new AbilityPreview("출력 증폭기", "공격 출력", 1, true),
            new AbilityPreview("가속 회로", "공격 주기", 1, true),
            new AbilityPreview("수집 자기장", "경험치 흡인", 1, true),
            new AbilityPreview("자가 복구 프로토콜", "조건부 회복", 1, true),
            new AbilityPreview("비상 방벽", "피격 완화", 1, true),
            new AbilityPreview("취약점 해석기", "치명타", 2, false),
            new AbilityPreview("광자 팽창기", "범위 확장", 4, false),
            new AbilityPreview("파열 증폭기", "강타 증폭", 5, false)
        };

        private static readonly UnlockPreview[] Unlocks =
        {
            new UnlockPreview(2, "취약점 해석기", "지원"),
            new UnlockPreview(3, "별빛 캐스터", "공격"),
            new UnlockPreview(4, "광자 팽창기", "지원"),
            new UnlockPreview(5, "파열 증폭기", "지원"),
            new UnlockPreview(7, "혜성 창", "공격"),
            new UnlockPreview(8, "광자 가속기", "지원"),
            new UnlockPreview(10, "성좌 프리즘", "공격"),
            new UnlockPreview(12, "거성 사냥 프로토콜", "지원"),
            new UnlockPreview(14, "이온 사슬", "공격"),
            new UnlockPreview(16, "연쇄 과부하", "지원"),
            new UnlockPreview(18, "성운 부메랑", "공격"),
            new UnlockPreview(20, "다중 투영기", "지원")
        };

        public static bool IsUnlocked(AbilityPreview ability, int highestClearedStage)
        {
            return ability.StartsUnlocked || highestClearedStage >= ability.UnlockStage;
        }

        public static bool TryGetNextUnlock(int afterStage, out UnlockPreview preview)
        {
            foreach (var unlock in Unlocks)
            {
                if (unlock.Stage > afterStage)
                {
                    preview = unlock;
                    return true;
                }
            }

            preview = default;
            return false;
        }

        public static bool TryGetUnlockAtStage(int stage, out UnlockPreview preview)
        {
            foreach (var unlock in Unlocks)
            {
                if (unlock.Stage == stage)
                {
                    preview = unlock;
                    return true;
                }
            }

            preview = default;
            return false;
        }

        public static string ChapterName(int stage)
        {
            var chapter = Math.Max(1, (stage - 1) / 10 + 1);
            return chapter switch
            {
                1 => "폐기 궤도장",
                2 => "채굴 위성",
                3 => "냉각 시설",
                4 => "붉은 정제소",
                5 => "중력 연구소",
                6 => "파손 함대",
                7 => "공허 경계",
                8 => "성간 묘지",
                _ => $"섹터 {chapter}"
            };
        }
    }
}
