using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public enum DeepSiteKind { CrystalPocket,DiamondPocket,DeepMine,AbandonedWorkshop }
    public readonly struct DeepSite
    {
        public readonly string Id;public readonly Vector2 Position;public readonly DeepSiteKind Kind;
        public DeepSite(string id,Vector2 position,DeepSiteKind kind){Id=id;Position=position;Kind=kind;}
    }
    // Additive generation-three content. Generation-one/two maps never enter this path.
    public static class DeepCavernContent
    {
        public static IEnumerable<DeepSite> Sites(ContinentalWorldLayout world)
        {
            if(world.GenerationVersion<3)yield break;
            foreach(var cave in world.Caves)
            {
                var deep=new List<CavernRoom>();foreach(var room in cave.Rooms)if(room.Depth>=.6f)deep.Add(room);
                deep.Sort((a,b)=>b.Depth.CompareTo(a.Depth));
                for(var i=0;i<deep.Count;i++)
                {
                    var kind=i==0 ? (cave.Id%3==0 ? DeepSiteKind.DiamondPocket : DeepSiteKind.CrystalPocket) : i==1 ? DeepSiteKind.DeepMine : DeepSiteKind.AbandonedWorkshop;
                    yield return new DeepSite("deep:"+cave.Id+":"+i,deep[i].Center,kind);
                }
            }
        }
        public static void Build(ContinentalWorldLayout world,WorldGenerationDefinition data)
        {
            var budget=ProgressionEconomy.Calculate(data.ProgressionCatalog,4);
            for(var rule=0;rule<data.Resources.Length;rule++)
            {
                var r=data.Resources[rule];if(r.Family!=ResourceFamily.Mineral)continue;
                var item=r.Prefab.Definition.DropItem;var gem=item.Id=="diamond" || item.Id=="crystal";
                budget.Total.TryGetValue(item,out var cost);var target=Mathf.CeilToInt(cost*data.ProgressionSafetyFactor);var supplied=0;
                foreach(var p in world.GuaranteedResources)if(p.Rule==rule)supplied+=r.Prefab.Definition.DropCount;
                var count=Mathf.Max(gem ? world.Caves.Count*(item.Id=="diamond" ? 3 : 2) : 0,Mathf.CeilToInt((float)(target-supplied)/r.Prefab.Definition.DropCount));
                var sites=new List<DeepSite>(Sites(world));
                if(gem)sites.RemoveAll(s=>s.Kind!=(item.Id=="diamond" ? DeepSiteKind.DiamondPocket : DeepSiteKind.CrystalPocket));
                if(gem && sites.Count>1)count=Mathf.Max(count,Mathf.CeilToInt((float)cost/r.Prefab.Definition.DropCount/(sites.Count-1))*sites.Count);
                for(var i=0;i<count;i++)
                {
                    var center=gem ? sites[i%sites.Count].Position : r.Prefab.Definition.HarvestMethod==HarvestMethod.Panning ? new Vector2(world.RiverAt(i%3*240)+8,i%3*240) : world.RockyAnchor+new Vector2(i%3*160-160,110);
                    var found=false;
                    for(var a=0;a<1600;a++)
                    {
                        var angle=(a+i*19)*2.399963f;var radius=3+Mathf.Sqrt(a)*.65f;var p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                        if(!world.SourceAllowed(rule,p))continue;
                        var clear=true;foreach(var old in world.GuaranteedResources)if((old.Position-p).sqrMagnitude<16){clear=false;break;}if(!clear)continue;
                        foreach(var old in world.GenerateChunk(WorldChunk.At(p,data.ChunkSize)))if((old.Position-p).sqrMagnitude<16){clear=false;break;}if(!clear)continue;
                        world.GuaranteedResources.Add(new ResourcePlacement(rule,p,1,"iii:"+rule+":"+i));found=true;break;
                    }
                    if(!found && WorldProgressionSupply.TryRiverFallback(world,data,rule,center,true,out var fallback)){world.GuaranteedResources.Add(new ResourcePlacement(rule,fallback,1,"iii:"+rule+":"+i));found=true;}
                    if(!found)throw new InvalidOperationException("Deep progression source placement exhausted");
                }
            }
        }
        public static void Validate(ContinentalWorldLayout world,WorldGenerationDefinition data)
        {
            if(world.GenerationVersion<3)return;
            var budget=ProgressionEconomy.Calculate(data.ProgressionCatalog,4);
            foreach(var id in new[]{"diamond","crystal"})
            {
                var amount=0;var caves=new HashSet<int>();var perCave=new Dictionary<int,int>();
                foreach(var p in world.GuaranteedResources)
                {var source=data.Resources[p.Rule].Prefab.Definition;if(source.DropItem.Id!=id)continue;if(!world.Underground(p.Position) || !world.SourceAllowed(p.Rule,p.Position))throw new InvalidOperationException("Deep source outside accessible cavern");amount+=source.DropCount;var cave=world.CaveAt(p.Position).Id;caves.Add(cave);perCave.TryGetValue(cave,out var supplied);perCave[cave]=supplied+source.DropCount;}
                foreach(var n in perCave.Values)if(amount-n<budget.Total[data.ProgressionCatalog.Item(id)])throw new InvalidOperationException("Deep supply depends on one cavern");
                if(caves.Count<3 || amount<Mathf.CeilToInt(budget.Total[data.ProgressionCatalog.Item(id)]*data.ProgressionSafetyFactor))throw new InvalidOperationException("Insufficient initial four-player deep supply");
            }
        }
    }
}
