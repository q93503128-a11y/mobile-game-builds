using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public static class WorldProgressionSupply
    {
        public static void Supplement(ContinentalWorldLayout world,WorldGenerationDefinition data)
        {
            var needed=RequiredYield(data);foreach(var pair in needed)
            {
                var rule=pair.Key;var r=data.Resources[rule];if(r.Family!=ResourceFamily.Mineral)continue;
                var actual=0;foreach(var p in world.GuaranteedResources)if(p.Rule==rule)actual+=r.Prefab.Definition.DropCount;
                var count=Mathf.Max(r.LayoutSupplementCount,Mathf.CeilToInt((float)(pair.Value-actual)/r.Prefab.Definition.DropCount));
                for(var i=0;i<count;i++)
                {
                    var center=r.Prefab.Definition.HarvestMethod==HarvestMethod.Panning ? new Vector2(world.RiverAt(i%3*240)+8,i%3*240) : world.RockyAnchor+new Vector2((i%3-1)*160,110);
                    var found=false;for(var a=0;a<2000;a++){var angle=a*2.399963f;var radius=8+Mathf.Sqrt(a)*2.2f;var p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;if(!world.SourceAllowed(rule,p))continue;var clear=true;foreach(var old in world.GuaranteedResources)if((p-old.Position).sqrMagnitude<16){clear=false;break;}if(!clear)continue;
                        // Supplements may overlap a natural candidate; the supplement is skipped there, never the old source.
                        var prior=world.GenerateChunk(WorldChunk.At(p,data.ChunkSize));foreach(var old in prior)if((p-old.Position).sqrMagnitude<16){clear=false;break;}if(!clear)continue;
                        world.GuaranteedResources.Add(new ResourcePlacement(rule,p,1,"e"+rule+":"+i));found=true;break;}
                    if(!found)throw new InvalidOperationException("No safe additive progression source site");
                }
            }
        }
        public static Dictionary<int,int> RequiredYield(WorldGenerationDefinition data,int players=4)
        {
            var budget=ProgressionEconomy.Calculate(data.ProgressionCatalog,players,maximumTier:2);var result=new Dictionary<int,int>();
            for(var i=0;i<data.Resources.Length;i++)if(budget.Total.TryGetValue(data.Resources[i].Prefab.Definition.DropItem,out var count))result[i]=Mathf.CeilToInt(count*data.ProgressionSafetyFactor);
            return result;
        }
        public static void Build(ContinentalWorldLayout world,WorldGenerationDefinition data)
        {
            for(var rule=0;rule<Mathf.Min(8,data.Resources.Length);rule++)
            {
                var r=data.Resources[rule];var harvest=r.Prefab.Definition;var mineral=r.Family==ResourceFamily.Mineral;
                var count=r.LayoutSourceCount;
                var starter=0;foreach(var source in data.NaturalSources)if(source.Enabled && source.ResourceRule==rule)starter=Mathf.Max(starter,source.StarterSupply);
                for(var i=0;i<count;i++)
                {
                    var cluster=mineral ? i%3 : 0;Vector2 center;
                    var panning=harvest.HarvestMethod==HarvestMethod.Panning;
                    if(panning)center=new Vector2(world.RiverAt(cluster*240)+8,cluster*240);
                    else if(i<starter)center=new Vector2(-45,25)+Vector2.right*i*12;
                    else if(mineral)center=world.RockyAnchor+new Vector2(cluster*160-160,cluster%2*180);
                    else center=new Vector2(-20+rule*8,4);
                    var found=false;var selected=Vector2.zero;
                    for(var attempt=0;attempt<1200;attempt++)
                    {
                        var angle=attempt*2.399963f;var radius=2+Mathf.Sqrt(attempt)*2.2f;var p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                        if(!world.SourceAllowed(rule,p))continue;var clear=true;foreach(var old in world.GuaranteedResources)if((old.Position-p).sqrMagnitude<16){clear=false;break;}if(!clear)continue;selected=p;found=true;break;
                    }
                    if(!found)throw new InvalidOperationException("Deterministic source repair exhausted for rule "+rule);
                    world.GuaranteedResources.Add(new ResourcePlacement(rule,selected,1,"g"+rule+":"+i));
                }
            }
        }
    }
    public static class WorldGenerationValidator
    {
        public static void Validate(ContinentalWorldLayout world,WorldGenerationDefinition data)
        {
            if(!world.Walkable(Vector2.zero,4) || world.Caves.Count==0)throw new InvalidOperationException("Unusable spawn or missing cave");
            var starter=new Dictionary<ItemDefinition,int>();
            for(var y=-1;y<=1;y++)for(var x=-1;x<=1;x++)foreach(var p in world.GenerateChunk(new WorldChunk(x,y)))
            {var r=data.Resources[p.Rule];if(r.Family!=ResourceFamily.Timber && r.Family!=ResourceFamily.Stone)continue;var item=r.Prefab.Definition.DropItem;starter.TryGetValue(item,out var n);starter[item]=n+r.Prefab.Definition.DropCount;}
            var budget=WorldProgressionSupply.RequiredYield(data);foreach(var pair in budget)
            {var rule=data.Resources[pair.Key];if(rule.Family==ResourceFamily.Timber || rule.Family==ResourceFamily.Stone){if(!starter.TryGetValue(rule.Prefab.Definition.DropItem,out var supply) || supply<pair.Value)throw new InvalidOperationException("Insufficient starting wood or stone");continue;}if(rule.Family!=ResourceFamily.Mineral)continue;var amount=0;var clusters=new HashSet<WorldChunk>();foreach(var p in world.GuaranteedResources)if(p.Rule==pair.Key){amount+=data.Resources[p.Rule].Prefab.Definition.DropCount;clusters.Add(WorldChunk.At(p.Position,128));if(!world.SourceAllowed(p.Rule,p.Position))throw new InvalidOperationException("Invalid guaranteed source environment");}if(amount<pair.Value || clusters.Count<3)throw new InvalidOperationException("Insufficient geographically distributed four-player progression yield");}
            foreach(var cave in world.Caves){var deep=false;foreach(var room in cave.Rooms){if(room.Depth>=.8f)deep=true;if(room.Neighbors.Count==0 || !world.Walkable(room.Center,1))throw new InvalidOperationException("Disconnected or blocked cave room");}if(!deep)throw new InvalidOperationException("No deep cavern progression");}
            ProgressionReachability.Validate(world);DeepCavernContent.Validate(world,data);
        }
    }
}
