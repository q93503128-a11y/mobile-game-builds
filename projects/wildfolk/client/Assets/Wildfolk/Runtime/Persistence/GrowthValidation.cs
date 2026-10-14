using System;
using System.Collections.Generic;
using System.IO;
namespace Wildfolk
{
    public static class GrowthValidation
    {
        public static GrowthSnapshot FromLegacy(long xp,CharacterGrowthDefinition data)
        {
            var level=1;var current=xp;while(level<data.MaximumLevel && current>=data.RequiredXP(level)){current-=data.RequiredXP(level);level++;}
            var skills=new SavedSkill[data.Skills.Length];for(var i=0;i<skills.Length;i++)skills[i]=new SavedSkill{Id=data.Skills[i].Id};
            return new GrowthSnapshot{Level=level,CurrentXP=current,Points=(level-1)*data.PointsPerLevel,Skills=skills};
        }
        public static void Validate(GrowthSnapshot state,CharacterGrowthDefinition data,long xp)
        {
            if(state==null || state.Level<1 || state.Level>data.MaximumLevel || state.CurrentXP<0 || state.Points<0 || state.Skills==null || state.Skills.Length!=data.Skills.Length)throw new InvalidDataException("Invalid growth state");
            long earned=state.CurrentXP;for(var level=1;level<state.Level;level++)earned+=data.RequiredXP(level);
            if(earned!=xp || state.Level<data.MaximumLevel && state.CurrentXP>=data.RequiredXP(state.Level))throw new InvalidDataException("Invalid XP curve state");
            var spent=0;var ids=new HashSet<string>();foreach(var skill in state.Skills)
            {if(skill==null || !ids.Add(skill.Id))throw new InvalidDataException("Invalid skill ID");CharacterSkill definition=null;foreach(var s in data.Skills)if(s.Id==skill.Id)definition=s;if(definition==null || skill.Rank<0 || skill.Rank>definition.MaximumRank)throw new InvalidDataException("Invalid skill rank");spent+=skill.Rank;}
            if(spent+state.Points!=(state.Level-1)*data.PointsPerLevel)throw new InvalidDataException("Invalid skill points");
        }
        public static float Bonus(GrowthSnapshot state,CharacterGrowthDefinition data,SkillEffect effect)
        {var result=0f;foreach(var saved in state.Skills)foreach(var skill in data.Skills)if(saved.Id==skill.Id && skill.Effect==effect)result+=saved.Rank*skill.PerRank;return result;}
    }
}
