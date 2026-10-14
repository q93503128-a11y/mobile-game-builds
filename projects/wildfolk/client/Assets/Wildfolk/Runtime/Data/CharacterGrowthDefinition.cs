using System;
using UnityEngine;
namespace Wildfolk
{
    public enum SkillEffect { MaximumHealth, MaximumStamina, StaminaRecovery, SprintEfficiency }
    [Serializable] public sealed class CharacterSkill
    {public string Id,Name;public Sprite Icon;[Min(1)]public int MaximumRank=3;public SkillEffect Effect;[Min(0)]public float PerRank;
        public string Description=>Effect==SkillEffect.MaximumHealth ? "+"+PerRank.ToString("0")+" maximum health per rank" : Effect==SkillEffect.MaximumStamina ? "+"+PerRank.ToString("0")+" maximum stamina per rank" : Effect==SkillEffect.StaminaRecovery ? "+"+(PerRank*100).ToString("0")+"% stamina recovery per rank" : "+"+(PerRank*100).ToString("0")+"% sprint efficiency per rank";
    }
    [CreateAssetMenu(menuName="Wildfolk/Character Growth")]
    public sealed class CharacterGrowthDefinition : ScriptableObject
    {
        [Min(1)]public int FirstLevelXP=35,AdditionalXPPerLevel=20,MaximumLevel=20,PointsPerLevel=1;
        public CharacterSkill[] Skills=Array.Empty<CharacterSkill>();
        public int RequiredXP(int level)=>level>=MaximumLevel ? 0 : checked(FirstLevelXP+(level-1)*AdditionalXPPerLevel);
    }
}
