using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
namespace Wildfolk.Editor
{
    internal static class QualityDataValidator
    {
        internal static void Validate(List<string> errors)
        {
            var keys=new HashSet<string>();foreach(var entry in L.Catalog.Entries)
            {
                if(!keys.Add(entry.Key) || entry.Values==null || entry.Values.Length!=L.Catalog.Languages.Length){errors.Add("Invalid localization entry: "+entry.Key);continue;}
                var placeholders=Tokens(entry.Values[0]);foreach(var value in entry.Values)if(string.IsNullOrWhiteSpace(value) || Tokens(value)!=placeholders)errors.Add("Invalid translation/format: "+entry.Key);
            }
            foreach(var guid in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/Wildfolk/Data"}))
            {var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));if(!keys.Contains("item."+item.Id+".name"))errors.Add("Missing item translation: "+item.Id);}
            foreach(var guid in AssetDatabase.FindAssets("t:CharacterGrowthDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var data=AssetDatabase.LoadAssetAtPath<CharacterGrowthDefinition>(AssetDatabase.GUIDToAssetPath(guid));var skills=new Dictionary<string,CharacterSkill>();foreach(var s in data.Skills)if(s!=null && !skills.ContainsKey(s.Id))skills.Add(s.Id,s);
                foreach(var skill in skills.Values)
                {
                    if(skill.PointCost<1 || !keys.Contains("ui.skill."+skill.Id+".name"))errors.Add("Invalid skill cost/localization: "+skill.Id);
                    if(string.IsNullOrEmpty(skill.Prerequisite))continue;
                    if(!skills.TryGetValue(skill.Prerequisite,out var prerequisite) || skill.PrerequisiteRank<1 || skill.PrerequisiteRank>prerequisite.MaximumRank){errors.Add("Invalid skill prerequisite: "+skill.Id);continue;}
                    var seen=new HashSet<string>();var node=skill;while(node!=null){if(!seen.Add(node.Id)){errors.Add("Skill prerequisite cycle: "+skill.Id);break;}if(string.IsNullOrEmpty(node.Prerequisite))break;skills.TryGetValue(node.Prerequisite,out node);}
                }
            }
            foreach(var guid in AssetDatabase.FindAssets("t:WorldGenerationDefinition",new[]{"Assets/Wildfolk/Data"}))
            {var world=AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>(AssetDatabase.GUIDToAssetPath(guid));foreach(var rule in world.Resources)if(!rule.RecoveryConfigured || rule.TargetDensityRatio<=0 || rule.HarvestExclusionRadius<0 || rule.HarvestExclusionSeconds<0 || rule.ClusterSpacing<0)errors.Add("Invalid resource population policy");}
        }
        private static string Tokens(string value)
        {if(value==null)return "";var list=new List<string>();foreach(Match match in Regex.Matches(value,@"\{\d+(?::[^}]+)?\}"))list.Add(match.Value);list.Sort(StringComparer.Ordinal);return string.Join("|",list);}
    }
}
