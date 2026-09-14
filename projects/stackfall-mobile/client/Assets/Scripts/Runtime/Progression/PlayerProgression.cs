using System;
using UnityEngine;

namespace StackfallMobile.Runtime.Progression
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        [SerializeField] private int startingLevel = 1;

        private int _level;
        private int _experience;
        private int _experienceToNext;

        public int Level => _level;
        public int Experience => _experience;
        public int ExperienceToNext => _experienceToNext;

        public event Action<int, int, int> ExperienceChanged;
        public event Action<int> LevelGained;

        private void Awake()
        {
            ResetProgression();
        }

        public void GrantExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _experience += amount;

            while (_experience >= _experienceToNext)
            {
                _experience -= _experienceToNext;
                _level++;
                _experienceToNext = CalculateExperienceRequirement(_level);
                LevelGained?.Invoke(_level);
            }

            ExperienceChanged?.Invoke(_level, _experience, _experienceToNext);
        }

        public void ResetProgression()
        {
            _level = Mathf.Max(1, startingLevel);
            _experience = 0;
            _experienceToNext = CalculateExperienceRequirement(_level);
            ExperienceChanged?.Invoke(_level, _experience, _experienceToNext);
        }

        private static int CalculateExperienceRequirement(int level)
        {
            return Mathf.RoundToInt(8f + 3.2f * level + 1.15f * Mathf.Pow(level, 1.24f));
        }
    }
}
