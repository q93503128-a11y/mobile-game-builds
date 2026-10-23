using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class BossDataValidator
    {
        public static void Validate(List<string> errors)
        {
            var ids=new HashSet<string>();foreach(var guid in AssetDatabase.FindAssets("t:BossDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var d=AssetDatabase.LoadAssetAtPath<BossDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if(string.IsNullOrEmpty(d.Id) || !ids.Add(d.Id) || !L.Has(d.NameKey) || d.Prefab==null || d.Vitality==null || d.Vitality.MaximumHealth<=0 || d.Tomb==null || d.Offering==null || d.OfferingCount<=0 || d.Rewards==null || d.Minion==null || d.MinionRewards==null)errors.Add("Incomplete boss definition: "+d.name);
                if(d.Thresholds.Length!=2 || d.Thresholds[0]<=d.Thresholds[1] || d.Thresholds[1]<=0 || d.Thresholds[0]>=1 || d.Grace<1 || d.RegenDelay<0 || d.RegenFraction<=0 || d.RegenFraction>.05f || d.Attacks.Length==0)errors.Add("Invalid boss lifecycle: "+d.name);
                foreach(var a in d.Attacks)if(a.Damage<=0 || a.Windup<.6f || a.Recovery<.3f || a.Range<=0 || a.Repeats<1 || a.Repeats>3 || a.FirstPhase<1 || a.LastPhase>3 || a.FollowUp>=d.Attacks.Length)errors.Add("Invalid boss attack: "+a.Id);
                foreach(var i in d.Schedule)if(i<0 || i>=d.Attacks.Length)errors.Add("Invalid boss schedule: "+d.name);
            }
            foreach(var guid in AssetDatabase.FindAssets("t:AccessoryDefinition",new[]{"Assets/Wildfolk/Data"}))
            {var a=AssetDatabase.LoadAssetAtPath<AccessoryDefinition>(AssetDatabase.GUIDToAssetPath(guid));if(a.Visual==null || a.Threshold<=0 || a.Threshold>=1 || a.DamageReduction<0 || a.DamageReduction>.25f || a.MovementBonus<0 || a.MovementBonus>.2f || !Enum.IsDefined(typeof(AccessoryAnchor),a.Anchor))errors.Add("Invalid accessory: "+a.name);}
        }
    }
}
