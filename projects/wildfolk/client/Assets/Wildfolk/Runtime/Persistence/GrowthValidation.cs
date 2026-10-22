using System;
using System.Collections.Generic;
using System.IO;
namespace Wildfolk
{
    public static class GrowthValidation
    {
        public static GrowthSnapshot FromLegacy(long xp,CharacterGrowthDefinition data)
        {
            var level=data.LevelFor(xp);var skills=new SavedSkill[data.Skills.Length];
            for(var i=0;i<skills.Length;i++)skills[i]=new SavedSkill{Id=data.Skills[i].Id};
            return new GrowthSnapshot{Level=level,CurrentXP=xp-(long)data.TotalBefore(level),Points=checked((level-1)*data.PointsPerLevel),Skills=skills};
        }
        // Only old schemas may omit newly introduced IDs or retain XP at the old level20 cap.
        public static void Migrate(GrowthSnapshot state,CharacterGrowthDefinition data,long xp)
        {
            if(state==null || state.Level<1 || state.Level>20 || state.CurrentXP<0 || state.Points<0 || state.Skills==null || state.Skills.Length!=4)throw new InvalidDataException("Invalid old growth");
            if(data.TotalBefore(state.Level)+state.CurrentXP!=xp || state.Level<20 && state.CurrentXP>=data.RequiredXP(state.Level))throw new InvalidDataException("Invalid old XP");
            var oldIds=new HashSet<string>(new[]{"vitality","endurance","recovery","stride"});var seen=new HashSet<string>();var spent=0;
            foreach(var s in state.Skills){if(s==null || !oldIds.Contains(s.Id) || !seen.Add(s.Id) || s.Rank<0 || s.Rank>3)throw new InvalidDataException("Invalid old skill");spent+=s.Rank;}
            if(spent+state.Points!=(state.Level-1)*data.PointsPerLevel)throw new InvalidDataException("Invalid old points");
            var next=FromLegacy(xp,data);next.Points-=spent;
            foreach(var old in state.Skills)foreach(var s in next.Skills)if(s.Id==old.Id)s.Rank=old.Rank;
            state.Level=next.Level;state.CurrentXP=next.CurrentXP;state.Points=next.Points;state.Skills=next.Skills;
        }
        public static void Validate(GrowthSnapshot state,CharacterGrowthDefinition data,long xp)
        {
            if(state==null || state.Level<1 || state.CurrentXP<0 || state.Points<0 || state.Skills==null || state.Skills.Length!=data.Skills.Length)throw new InvalidDataException("Invalid growth state");
            if(data.TotalBefore(state.Level)+state.CurrentXP!=xp || state.CurrentXP>=data.RequiredXP(state.Level))throw new InvalidDataException("Invalid XP curve state");
            long spent=0;var ids=new HashSet<string>();foreach(var skill in state.Skills)
            {if(skill==null || !ids.Add(skill.Id))throw new InvalidDataException("Invalid skill ID");CharacterSkill definition=null;foreach(var s in data.Skills)if(s.Id==skill.Id)definition=s;if(definition==null || skill.Rank<0 || skill.Rank>definition.MaximumRank)throw new InvalidDataException("Invalid skill rank");spent+=(long)skill.Rank*definition.PointCost;}
            if(spent+state.Points!=(long)(state.Level-1)*data.PointsPerLevel)throw new InvalidDataException("Invalid skill points");
        }
        public static float Bonus(GrowthSnapshot state,CharacterGrowthDefinition data,SkillEffect effect)
        {var result=0f;foreach(var saved in state.Skills)foreach(var skill in data.Skills)if(saved.Id==skill.Id && skill.Effect==effect)result+=saved.Rank*skill.PerRank;return result;}
    }
}
