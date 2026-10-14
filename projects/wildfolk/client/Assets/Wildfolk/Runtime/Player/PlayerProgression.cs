using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class PlayerProgression : MonoBehaviour
    {
        public CharacterGrowthDefinition Definition;
        public int Level {get;private set;}=1;
        public long CurrentXP {get;private set;}
        public long Experience {get;private set;}
        public long Gold {get;private set;}
        public int SkillPoints {get;private set;}
        private int[] ranks;
        public event Action Changed;
        public int RequiredXP=>Definition.RequiredXP(Level);
        private void Awake()=>ranks=new int[Definition.Skills.Length];
        public int Rank(int index)=>ranks[index];
        public float Bonus(SkillEffect effect){var result=0f;for(var i=0;i<ranks.Length;i++)if(Definition.Skills[i].Effect==effect)result+=ranks[i]*Definition.Skills[i].PerRank;return result;}
        public bool Unlocked(int index)
        {var skill=Definition.Skills[index];if(ranks[index]>0 || string.IsNullOrEmpty(skill.Prerequisite))return true;for(var i=0;i<ranks.Length;i++)if(Definition.Skills[i].Id==skill.Prerequisite)return ranks[i]>=skill.PrerequisiteRank;return false;}
        public bool CanInvest(int index)=>index>=0 && index<ranks.Length && Unlocked(index) && SkillPoints>=Definition.Skills[index].PointCost && ranks[index]<Definition.Skills[index].MaximumRank;
        public bool Invest(int index)
        {if(!CanInvest(index))return false;SkillPoints-=Definition.Skills[index].PointCost;ranks[index]++;Changed?.Invoke();return true;}
        public void SetValues(long experience,long gold)
        {
            if(experience<0 || gold<0)throw new ArgumentOutOfRangeException();
            Level=1;CurrentXP=0;Experience=0;Gold=gold;SkillPoints=0;ranks=new int[Definition.Skills.Length];Gain(experience);Changed?.Invoke();
        }
        private void Gain(long xp)
        {Experience=checked(Experience+xp);CurrentXP=checked(CurrentXP+xp);while(Level<Definition.MaximumLevel && CurrentXP>=RequiredXP){CurrentXP-=RequiredXP;Level++;SkillPoints=checked(SkillPoints+Definition.PointsPerLevel);}}
        public void Reward(int experience,int gold)
        {if(experience<0 || gold<0)throw new ArgumentOutOfRangeException();Gain(experience);Gold=checked(Gold+gold);Changed?.Invoke();}
        public GrowthSnapshot Capture()
        {var skills=new SavedSkill[ranks.Length];for(var i=0;i<ranks.Length;i++)skills[i]=new SavedSkill{Id=Definition.Skills[i].Id,Rank=ranks[i]};return new GrowthSnapshot{Level=Level,CurrentXP=CurrentXP,Points=SkillPoints,Skills=skills};}
        public void Restore(GrowthSnapshot state,long xp,long gold)
        {GrowthValidation.Validate(state,Definition,xp);Level=state.Level;CurrentXP=state.CurrentXP;Experience=xp;Gold=gold;SkillPoints=state.Points;ranks=new int[Definition.Skills.Length];foreach(var skill in state.Skills)for(var i=0;i<ranks.Length;i++)if(Definition.Skills[i].Id==skill.Id)ranks[i]=skill.Rank;Changed?.Invoke();}
    }
}
