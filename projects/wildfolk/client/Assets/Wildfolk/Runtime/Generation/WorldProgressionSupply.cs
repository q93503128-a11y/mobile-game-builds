using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public static class WorldProgressionSupply
    {
        private static Dictionary<int,int> BaseYield(WorldGenerationDefinition data,int players=4)
        {
            var budget=new Dictionary<ItemDefinition,int>();var catalog=data.ProgressionCatalog;
            if(catalog==null)throw new InvalidOperationException("World progression catalog is missing");
            foreach(var recipe in catalog.Recipes)
            {
                if(recipe.Result.Tool!=null && recipe.Result.Tier.Rank==4 || recipe.Result.Armor!=null && recipe.Result.Tier.Rank==4)Cost(recipe.Result,players,catalog,budget,new HashSet<ItemDefinition>());
                else if(recipe.Result.Placement!=null && recipe.Result.Placement.Station==StationKind.BlastFurnace)Cost(recipe.Result,1,catalog,budget,new HashSet<ItemDefinition>());
                else if(recipe.Result.Tool!=null && recipe.Result.Tool.HarvestMethod==HarvestMethod.Panning)Cost(recipe.Result,players,catalog,budget,new HashSet<ItemDefinition>());
            }
            foreach(var item in catalog.Items)if(item.Placement!=null && item.Placement.Station==StationKind.Workbench && item.Placement.UpgradeTo!=null)foreach(var cost in item.Placement.UpgradeCosts)Cost(cost.Item,cost.Count,catalog,budget,new HashSet<ItemDefinition>());
            var result=new Dictionary<int,int>();for(var i=0;i<data.Resources.Length;i++)if(budget.TryGetValue(data.Resources[i].Prefab.Definition.DropItem,out var count))result[i]=Mathf.CeilToInt(count*data.ProgressionSafetyFactor);return result;
        }
        public static void Supplement(ContinentalWorldLayout world,WorldGenerationDefinition data)
        {
            var needed=RequiredYield(data);foreach(var pair in needed)
            {
                var rule=pair.Key;var r=data.Resources[rule];if(r.Family!=ResourceFamily.Mineral)continue;
                var actual=0;foreach(var p in world.GuaranteedResources)if(p.Rule==rule)actual+=r.Prefab.Definition.DropCount;
                var count=Mathf.Max(0,Mathf.CeilToInt((float)(pair.Value-actual)/r.Prefab.Definition.DropCount));
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
        private static void Cost(ItemDefinition item,int amount,ContentCatalog catalog,Dictionary<ItemDefinition,int> budget,HashSet<ItemDefinition> path)
        {
            RecipeDefinition recipe=null;foreach(var r in catalog.Recipes)if(r.Result==item){recipe=r;break;}
            if(recipe==null){budget.TryGetValue(item,out var value);budget[item]=value+amount;return;}
            if(!path.Add(item))throw new InvalidOperationException("Cyclic progression recipe");var units=Mathf.CeilToInt((float)amount/recipe.ResultCount);foreach(var c in recipe.Ingredients)Cost(c.Item,c.Count*units,catalog,budget,path);path.Remove(item);
        }
        public static Dictionary<int,int> RequiredYield(WorldGenerationDefinition data,int players=4)
        {
            var result=BaseYield(data,players);var budget=new Dictionary<ItemDefinition,int>();var catalog=data.ProgressionCatalog;
            foreach(var recipe in catalog.Recipes)if(recipe.ProgressionPerPlayer>0 || recipe.ProgressionShared>0)Cost(recipe.Result,recipe.ProgressionPerPlayer*players+recipe.ProgressionShared,catalog,budget,new HashSet<ItemDefinition>());
            for(var i=0;i<data.Resources.Length;i++)if(budget.TryGetValue(data.Resources[i].Prefab.Definition.DropItem,out var count)){result.TryGetValue(i,out var old);result[i]=old+Mathf.CeilToInt(count*data.ProgressionSafetyFactor);}return result;
        }
        public static void Build(ContinentalWorldLayout world,WorldGenerationDefinition data)
        {
            var budget=BaseYield(data);for(var rule=0;rule<Mathf.Min(8,data.Resources.Length);rule++)
            {
                var r=data.Resources[rule];var harvest=r.Prefab.Definition;var required=budget.TryGetValue(rule,out var need) ? need : 0;var mineral=r.Family==ResourceFamily.Mineral;
                var count=mineral ? Mathf.Max(9,Mathf.CeilToInt((float)required/harvest.DropCount)) : r.StartCount+4;
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
            var budget=WorldProgressionSupply.RequiredYield(data);foreach(var pair in budget)
            {if(data.Resources[pair.Key].Family!=ResourceFamily.Mineral)continue;var amount=0;var clusters=new HashSet<WorldChunk>();foreach(var p in world.GuaranteedResources)if(p.Rule==pair.Key){amount+=data.Resources[p.Rule].Prefab.Definition.DropCount;clusters.Add(WorldChunk.At(p.Position,128));if(!world.SourceAllowed(p.Rule,p.Position))throw new InvalidOperationException("Invalid guaranteed source environment");}if(amount<pair.Value || clusters.Count<3)throw new InvalidOperationException("Insufficient geographically distributed four-player progression yield");}
            foreach(var cave in world.Caves){var deep=false;foreach(var room in cave.Rooms){if(room.Depth>=.8f)deep=true;if(room.Neighbors.Count==0 || !world.Walkable(room.Center,1))throw new InvalidOperationException("Disconnected or blocked cave room");}if(!deep)throw new InvalidOperationException("No deep cavern progression");}
            ProgressionReachability.Validate(world);
        }
    }
}
