using System;
using UnityEngine;
namespace Wildfolk
{
    public enum SkillEffect { MaximumHealth, MaximumStamina, StaminaRecovery, SprintEfficiency }
    [Serializable] public sealed class CharacterSkill
    {public string Id,Name;public string Prerequisite;public int PrerequisiteRank=1;public int PointCost=1;public Vector2 NodePosition;public Sprite Icon;[Min(1)]public int MaximumRank=3;public SkillEffect Effect;[Min(0)]public float PerRank;
        public string Description=>L.Format("ui.skill."+Effect+".effect",PerRank*(Effect==SkillEffect.StaminaRecovery || Effect==SkillEffect.SprintEfficiency ? 100 : 1));
    }
    [CreateAssetMenu(menuName="Wildfolk/Character Growth")]
    public sealed class CharacterGrowthDefinition : ScriptableObject
    {
        [Min(1)]public int FirstLevelXP=35,AdditionalXPPerLevel=20,MaximumLevel=20,PointsPerLevel=1;
        public CharacterSkill[] Skills=Array.Empty<CharacterSkill>();
        public int RequiredXP(int level)=>level>=MaximumLevel ? 0 : checked(FirstLevelXP+(level-1)*AdditionalXPPerLevel);
    }
}
