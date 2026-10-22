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
        public long RequiredXP=>Definition.RequiredXP(Level);
        private void Awake()=>ranks=new int[Definition.Skills.Length];
        public int Rank(int index)=>ranks[index];
        public float Bonus(SkillEffect effect){var result=0f;for(var i=0;i<ranks.Length;i++)if(Definition.Skills[i].Effect==effect)result+=ranks[i]*Definition.Skills[i].PerRank;return result;}
        public bool Unlocked(int index)
        {var skill=Definition.Skills[index];if(ranks[index]>0 || string.IsNullOrEmpty(skill.Prerequisite))return true;for(var i=0;i<ranks.Length;i++)if(Definition.Skills[i].Id==skill.Prerequisite)return ranks[i]>=skill.PrerequisiteRank;return false;}
        public bool CanInvest(int index)=>index>=0 && index<ranks.Length && Unlocked(index) && SkillPoints>=Definition.Skills[index].PointCost && ranks[index]<Definition.Skills[index].MaximumRank;
        public bool Invest(int index)
        {var net=GetComponent<NetworkCommonGrowth>();if(net!=null && net.IsSpawned && !net.Applying)return net.Invest(index);if(!SimulationAuthority.Allows(this) || !CanInvest(index))return false;SkillPoints-=Definition.Skills[index].PointCost;ranks[index]++;Changed?.Invoke();return true;}
        public void SetValues(long experience,long gold)
        {
            if(experience<0 || gold<0)throw new ArgumentOutOfRangeException();
            Level=1;CurrentXP=0;Experience=0;Gold=gold;SkillPoints=0;ranks=new int[Definition.Skills.Length];Gain(experience);Changed?.Invoke();
        }
        private void Gain(long xp)
        {
            var total=checked(Experience+xp);var level=Definition.LevelFor(total);
            var points=checked(SkillPoints+checked((level-Level)*Definition.PointsPerLevel));
            CurrentXP=checked(total-(long)Definition.TotalBefore(level));Experience=total;Level=level;SkillPoints=points;
        }
        // Only the loot/death authority calls this entry. Debug grants use SetValues/Reward.
        public void GameplayReward(int experience,int gold,string receipt)
        {if(!SimulationAuthority.Allows(this) || experience<0 || gold<0 || string.IsNullOrEmpty(receipt))return;var total=checked(Experience+experience);var balance=checked(Gold+gold);var role=GetComponent<PlayerClass>();if(role!=null && !role.GrantXP(experience,receipt))return;Reward(experience,gold);}
        public void Reward(int experience,int gold)
        {if(experience<0 || gold<0)throw new ArgumentOutOfRangeException();var nextGold=checked(Gold+gold);Gain(experience);Gold=nextGold;Changed?.Invoke();}
        public GrowthSnapshot Capture()
        {var skills=new SavedSkill[ranks.Length];for(var i=0;i<ranks.Length;i++)skills[i]=new SavedSkill{Id=Definition.Skills[i].Id,Rank=ranks[i]};return new GrowthSnapshot{Level=Level,CurrentXP=CurrentXP,Points=SkillPoints,Skills=skills};}
        public void Restore(GrowthSnapshot state,long xp,long gold)
        {GrowthValidation.Validate(state,Definition,xp);Level=state.Level;CurrentXP=state.CurrentXP;Experience=xp;Gold=gold;SkillPoints=state.Points;ranks=new int[Definition.Skills.Length];foreach(var skill in state.Skills)for(var i=0;i<ranks.Length;i++)if(Definition.Skills[i].Id==skill.Id)ranks[i]=skill.Rank;Changed?.Invoke();}
    }
}
