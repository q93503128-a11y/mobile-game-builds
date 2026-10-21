using System;
using System.Collections.Generic;
using UnityEditor;
namespace Wildfolk.Editor
{
    internal static class PoiDataValidator
    {
        internal static void Validate(List<string> errors)
        {
            var world=AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>("Assets/Wildfolk/Data/WorldGeneration.asset");var d=world.PoiContent;
            if(d==null || d.Prefab==null || d.Prefab.Visual==null || d.Prefab.ClosedSprite==null || d.Prefab.OpenedSprite==null || d.WellCapacity!=3 || d.WellRefillSeconds!=480 || d.Campsite.Length!=3 || d.Graveyard.Length!=3 || d.QuarryRules.Length!=4){errors.Add("Invalid POI definition");return;}
            foreach(var v in d.Campsite)Check(v.Items,errors);Check(d.Graveyard,errors);
            for(var i=0;i<d.QuarryRules.Length;i++){var r=d.QuarryRules[i];if(r<0 || r>=world.Resources.Length || world.Resources[r].Prefab.Definition.DropItem.Id!=(i<2 ? "stone" : i==2 ? "copper_ore" : "coal"))errors.Add("Invalid Quarry resource");}
            foreach(var name in new[]{"BoneTrap","NetworkBoneTrap","SpikeTrap","NetworkSpikeTrap"}){var p=AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Wildfolk/Prefabs/"+name+".prefab").GetComponent<SpikeTrap>();if(p.SingleUse!=name.Contains("Bone") || p.Damage!=(name.Contains("Bone")?32:16))errors.Add("Invalid trap role: "+name);}
            foreach(var key in new[]{"ui.poi.supplies","ui.poi.guarded","ui.poi.well_empty","ui.poi.well_container","ui.poi.well_filled","ui.poi.unavailable","poi.Campsite","poi.Graveyard","poi.Well"})if(!L.Has(key))errors.Add("Missing POI localization: "+key);
        }
        private static void Check(ItemAmount[] items,List<string> errors)
        {if(items==null || items.Length<1 || items.Length>6){errors.Add("Invalid POI supplies");return;}var ids=new HashSet<string>();foreach(var i in items)if(i.Item==null || !ids.Add(i.Item.Id) || i.Count<1 || i.Count>i.Item.MaxStack || i.Charges!=0 || i.Quality!=LiquidState.Empty || Array.IndexOf(new[]{"torch","cooked_meat","cooked_fish","arrow","field_dressing","trail_ration","clean_water_cup","bone","medicinal_herb"},i.Item.Id)<0)errors.Add("Invalid POI reward");}
    }
}
