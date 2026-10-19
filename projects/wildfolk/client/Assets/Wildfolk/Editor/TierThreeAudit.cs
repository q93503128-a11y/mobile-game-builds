using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class TierThreeAudit
    {
        public static string Export()
        {
            var root="Captures/tier-three";Directory.CreateDirectory(root);var catalog=AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/Wildfolk/Data/ContentCatalog.asset");var data=AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>("Assets/Wildfolk/Data/WorldGeneration.asset");var text=new StringBuilder();
            foreach(var i in catalog.Items)
            {text.Append("ITEM\t"+i.Id+"\tstack="+i.MaxStack);if(i.Tool!=null)text.Append("\tdamage="+i.Tool.Damage+"\tcooldown="+i.Tool.Cooldown+"\trange="+i.Tool.Attack.Range);if(i.Armor!=null)text.Append("\tdefense="+i.Armor.Defense);if(i.EquippedLight!=null)text.Append("\tlight_radius="+i.EquippedLight.Radius);text.AppendLine();}
            foreach(var r in catalog.Recipes){text.Append("RECIPE\t"+r.Id+"\t"+r.RequiredStation+"\t"+r.RequiredWorkbenchTier+"\t"+r.Result.Id+"="+r.ResultCount+"\t");foreach(var a in r.Ingredients)text.Append(a.Item.Id+"="+a.Count+" ");text.AppendLine();}
            for(var players=1;players<=4;players+=3){var b=ProgressionEconomy.Calculate(catalog,players);Write(text,players,"SharedRaw",b.Shared);Write(text,players,"PersonalRaw",b.Personal);Write(text,players,"OptionalRaw",b.Optional);Write(text,players,"TotalRaw",b.Total);Write(text,players,"Produced",b.Produced);}
            foreach(var seed in new[]{9472026,1,42,20261019})
            {
                var map=new WorldMap(data,seed,3);var supply=new Dictionary<ItemDefinition,int>();foreach(var p in map.Resources){var r=data.Resources[p.Rule].Prefab.Definition;supply.TryGetValue(r.DropItem,out var n);supply[r.DropItem]=n+r.DropCount;}Write(text,seed,"InitialSupply",supply);text.AppendLine("VALIDATOR\t"+seed+"\tPASS\taccessible_caves="+map.Continental.Caves.Count);
                var old=new WorldMap(data,seed,2);foreach(var p in old.Resources)if(p.Rule<8){var found=false;foreach(var next in map.Resources)if(next.Id==p.Id){if(next.Rule!=p.Rule || next.Position!=p.Position)throw new InvalidOperationException("Earlier source moved");found=true;break;}if(!found)throw new InvalidOperationException("Earlier source disappeared");}
            }
            File.WriteAllText(root+"/audit.tsv",text.ToString());return root+"/audit.tsv";
        }
        private static void Write(StringBuilder text,int players,string category,Dictionary<ItemDefinition,int> values)
        {var keys=new List<ItemDefinition>(values.Keys);keys.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));foreach(var i in keys)text.AppendLine("ECONOMY\t"+players+"\t"+category+"\t"+i.Id+"\t"+values[i]);}
    }
}
