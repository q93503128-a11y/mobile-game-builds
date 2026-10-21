using System;
using System.Collections.Generic;
using UnityEditor;
namespace Wildfolk.Editor
{
    internal static class ExplorationDataValidator
    {
        internal static void Validate(List<string> errors)
        {
            foreach(var g in AssetDatabase.FindAssets("t:ExplorationUtilityDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var d=AssetDatabase.LoadAssetAtPath<ExplorationUtilityDefinition>(AssetDatabase.GUIDToAssetPath(g));
                if(!Enum.IsDefined(typeof(ExplorationUtilityKind),d.Kind) || !Finite(d.Range) || d.Range<=0 || d.Range>60 || !Finite(d.ChannelSeconds) || d.ChannelSeconds<0 || !Finite(d.CooldownSeconds) || d.CooldownSeconds<=0 || !Finite(d.LifetimeSeconds) || d.LifetimeSeconds<=0 || d.LifetimeSeconds>d.CooldownSeconds || d.Kind==ExplorationUtilityKind.Survey && (d.Range>18 || d.ChannelSeconds<1))errors.Add("Invalid exploration utility: "+d.name);
            }
            foreach(var g in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/Wildfolk/Data"}))
            {var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g));if(item.ExplorationUtility!=null && (item.MaxStack!=1 || item.Tool!=null || item.Placement!=null || item.EquippedLight!=null || item.Icon==null || !L.Has("item."+item.Id+".description")))errors.Add("Invalid exploration item: "+item.Id);}
        }
        private static bool Finite(float f)=>!float.IsNaN(f)&&!float.IsInfinity(f);
    }
}
