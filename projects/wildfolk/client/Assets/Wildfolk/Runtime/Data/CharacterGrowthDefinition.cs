using System;
using UnityEngine;
namespace Wildfolk
{
    public enum SkillEffect { MaximumHealth, MaximumStamina, StaminaRecovery, SprintEfficiency, Metabolism, MedicineReceived }
    [Serializable] public sealed class CharacterSkill
    {
        public string Id,Name,Prerequisite; public int PrerequisiteRank=1,PointCost=1;
        public Vector2 NodePosition; public Sprite Icon; [Min(1)] public int MaximumRank=3;
        public SkillEffect Effect; [Min(0)] public float PerRank;
        public bool Percentage=>Effect!=SkillEffect.MaximumHealth && Effect!=SkillEffect.MaximumStamina;
        public string Description=>L.Format("ui.skill."+Effect+".effect",PerRank*(Percentage?100:1));
    }
    [CreateAssetMenu(menuName="Wildfolk/Character Growth")]
    public sealed class CharacterGrowthDefinition : ScriptableObject
    {
        [Min(1)] public int FirstLevelXP=35,AdditionalXPPerLevel=20,PointsPerLevel=1;
        public CharacterSkill[] Skills=Array.Empty<CharacterSkill>();
        public long RequiredXP(int level)=>checked((long)FirstLevelXP+((long)level-1)*AdditionalXPPerLevel);
        public decimal TotalBefore(int level) {decimal n=(long)level-1;return n*(2m*FirstLevelXP+(n-1)*AdditionalXPPerLevel)/2;}
        public int LevelFor(long xp)
        {
            if(xp<0)throw new ArgumentOutOfRangeException(nameof(xp));
            int low=1,high=int.MaxValue;
            while(low<high){var mid=(int)((long)low+((long)high-low+1)/2);if(TotalBefore(mid)<=xp)low=mid;else high=mid-1;}
            return low;
        }
    }
}
