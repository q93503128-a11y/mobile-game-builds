using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class ResourceTempoAudit
    {
        [Serializable]public sealed class Amount {public string Item;public int Count;}
        [Serializable]public sealed class Budget {public int Players;public Amount[] Shared,Personal,Optional,Total,Produced;}
        [Serializable]public sealed class SeedReport {public int Seed;public Amount[] Guaranteed,Required;public float FirstIron,FirstCoal,Rocky,Wetlands,Cave;public bool Valid;}
        private static WorldGenerationDefinition Data=>AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>("Assets/Wildfolk/Data/WorldGeneration.asset");
        private static Amount[] Values(Dictionary<ItemDefinition,int> values)
        {var list=new List<Amount>();foreach(var p in values)list.Add(new Amount{Item=p.Key.Id,Count=p.Value});list.Sort((a,b)=>string.CompareOrdinal(a.Item,b.Item));return list.ToArray();}
        public static string Economy()
        {
            Directory.CreateDirectory("Captures/resource-tempo");var text=new System.Text.StringBuilder();
            for(var players=1;players<=4;players+=3){var b=ProgressionEconomy.Calculate(Data.ProgressionCatalog,players);var report=new Budget{Players=players,Shared=Values(b.Shared),Personal=Values(b.Personal),Optional=Values(b.Optional),Total=Values(b.Total),Produced=Values(b.Produced)};File.WriteAllText("Captures/resource-tempo/budget-"+players+".json",JsonUtility.ToJson(report,true));text.AppendLine(JsonUtility.ToJson(report));}
            return text.ToString();
        }
        public static string Seed(int seed)
        {
            var d=Data;var w=new WorldMap(d,seed,2);var supply=new Dictionary<ItemDefinition,int>();var need=new Dictionary<ItemDefinition,int>();var firstIron=float.MaxValue;var firstCoal=float.MaxValue;
            foreach(var p in w.Resources){var r=d.Resources[p.Rule];var item=r.Prefab.Definition.DropItem;supply.TryGetValue(item,out var n);supply[item]=n+r.Prefab.Definition.DropCount;if(r.Prefab.Definition.Kind==ResourceKind.IronOre)firstIron=Mathf.Min(firstIron,p.Position.magnitude);if(r.Prefab.Definition.Kind==ResourceKind.Coal)firstCoal=Mathf.Min(firstCoal,p.Position.magnitude);}
            foreach(var pair in WorldProgressionSupply.RequiredYield(d))need[d.Resources[pair.Key].Prefab.Definition.DropItem]=pair.Value;
            var report=new SeedReport{Seed=seed,Guaranteed=Values(supply),Required=Values(need),FirstIron=firstIron,FirstCoal=firstCoal,Rocky=w.Continental.RockyAnchor.magnitude,Wetlands=w.Continental.SedimentAnchor.magnitude,Cave=w.Continental.Caves[0].SurfaceEntrances[0].magnitude,Valid=true};
            Directory.CreateDirectory("Captures/resource-tempo");File.WriteAllText("Captures/resource-tempo/seed-"+seed+".json",JsonUtility.ToJson(report,true));return JsonUtility.ToJson(report);
        }
        public static string GenerationFingerprint(int seed)
        {
            var d=Data;var m=new WorldMap(d,seed,2,false);var f=new System.Text.StringBuilder();
            foreach(var p in m.Resources)Line(f,seed,p);foreach(var k in new[]{new WorldChunk(0,0),new WorldChunk(-10,0),new WorldChunk(9,2)})foreach(var p in m.Continental.GenerateChunk(k))Line(f,seed,p);
            File.WriteAllText("Captures/resource-tempo/generation-"+seed+".tsv",f.ToString());return "Written "+seed;
        }
        private static void Line(System.Text.StringBuilder f,int seed,ResourcePlacement p)
        {f.AppendLine(seed+"\t"+p.Id+"\t"+p.Rule+"\t"+p.Position.x.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"\t"+p.Position.y.ToString("R",System.Globalization.CultureInfo.InvariantCulture));}
    }
}
