using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
namespace Wildfolk.Editor
{
    public static class EconomyDataValidator
    {
        public static void Validate(List<string> errors)
        {
            try{var c=EconomyCatalog.Current;var seen=new HashSet<string>();
                if(c.Schema!=1||c.Revision<1||c.EngineerCost<=0||c.ReviveCost<=0||c.Newcomer.Length!=7||c.Monthly.Length!=28||c.Categories.Length<5||c.Categories.Length>8)errors.Add("Invalid economic catalogue");
                foreach(var m in c.Metrics)if(!seen.Add("metric:"+m.Id)||m.Maximum<=0||m.MaxBatch<=0||m.MaxDaily<m.MaxBatch||!new[]{"count","mask"}.Contains(m.Mode))errors.Add("Invalid metric "+m.Id);
                foreach(var f in c.Families){if(!seen.Add("family:"+f.Id)||!c.Categories.Contains(f.Category)||c.Metric(f.Metric)==null||!L.Has(f.NameKey)||!L.Has(f.DescriptionKey)||f.Tiers.Length==0)errors.Add("Invalid achievement "+f.Id);var last=0;foreach(var t in f.Tiers){if(t.Threshold<=last||t.GemReward<=0||t.AchievementPoints<=0||t.Threshold>c.Metric(f.Metric).Maximum)errors.Add("Invalid achievement tier "+f.Id);last=t.Threshold;}}
                if(ClassCatalog.Current.Find("engineer").Price!=c.EngineerCost||AssetDatabase.LoadAssetAtPath<ReviveDefinition>("Assets/Wildfolk/Data/Revive.asset").ReviveGemCost!=c.ReviveCost)errors.Add("Economic prices disagree with gameplay data");
            }catch(Exception){errors.Add("Economic catalogue could not be validated");}
        }
    }
}
