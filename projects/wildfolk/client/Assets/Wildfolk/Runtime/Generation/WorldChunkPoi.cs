using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class WorldChunkRuntime
    {
        private readonly Dictionary<string,PoiSite> poiSites=new Dictionary<string,PoiSite>();
        public IReadOnlyDictionary<string,PoiSite> ActivePoiSites=>poiSites;
        public bool FeatureCleared(string id)=>features.TryGetValue(id,out var f) && f.Cleared;
        public PoiSiteSnapshot PoiState(string id)=>features.TryGetValue(id,out var f) ? f.Site : null;
        private void ClearPoiSites(){foreach(var site in poiSites.Values)if(site!=null)PrefabPool.Release(site.gameObject);poiSites.Clear();}
        private void UnloadPoiSites(WorldChunk key)
        {objectKeys.Clear();foreach(var pair in poiSites)if(WorldChunk.At(pair.Value.transform.position,World.Definition.ChunkSize).Equals(key))objectKeys.Add(pair.Key);foreach(var id in objectKeys){PrefabPool.Release(poiSites[id].gameObject);poiSites.Remove(id);}}
        private void MaterializePoi(WorldChunk key,Transform parent)
        {
            var config=World.Definition.PoiContent;if(config==null)return;Physics2D.SyncTransforms();
            foreach(var anchor in World.Map.Continental.Anchors)
            {
                if(!PoiContentDefinition.Supports(anchor.Kind) || !WorldChunk.At(anchor.Position,World.Definition.ChunkSize).Equals(key))continue;
                var legacy=features.TryGetValue(anchor.Id,out var feature);
                if(!legacy)features[anchor.Id]=feature=new WorldFeatureSnapshot{Id=anchor.Id,Visited=true};
                var state=feature.Site;
                if(state==null || state.Schema==0)
                {
                    state=feature.Site=new PoiSiteSnapshot{Schema=1,Legacy=legacy,Loot=legacy ? new InventoryModel(6).Capture() : config.Supplies(anchor.Kind,World.Seed,anchor.Id),Water=!legacy && anchor.Kind==WorldAnchorKind.Well ? config.WellCapacity : 0,RefillAt=legacy && anchor.Kind==WorldAnchorKind.Well ? World.Renewal.Clock.Model.Elapsed+config.WellRefillSeconds : 0};
                    if(anchor.Kind==WorldAnchorKind.Quarry && !legacy)state.Nodes=QuarryNodes(anchor,key);
                }
                if(anchor.Kind==WorldAnchorKind.Quarry)
                {foreach(var saved in state.Nodes)if(!saved.Destroyed){var node=World.CreateResource(saved.Id,saved.Rule,saved.Position,saved.Scale,parent);node.Health.RestoreHealth(saved.HP);}continue;}
                var site=NetworkSpawnFactory.Spawn(config.Prefab,anchor.Position,Quaternion.identity);site.Initialize(World,anchor,state);poiSites.Add(anchor.Id,site);
                if(anchor.Kind==WorldAnchorKind.Well)PoiSite.AdvanceWater(state,World.Renewal.Clock.Model.Elapsed,config.WellCapacity,config.WellRefillSeconds);
            }
        }
        private ResourceSnapshot[] QuarryNodes(WorldAnchor anchor,WorldChunk key)
        {
            var nodes=new List<ResourceSnapshot>();var rules=World.Definition.PoiContent.QuarryRules;
            for(var n=0;n<rules.Length;n++)
            {
                var rule=rules[n];var prefab=World.Definition.Resources[rule].Prefab;
                for(var attempt=0;attempt<24;attempt++)
                {
                    var angle=(n*6+attempt)*2.399963f;var radius=3+attempt/6f;var p=anchor.Position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                    if(!WorldChunk.At(p,World.Definition.ChunkSize).Equals(key) || !World.GetComponent<WorldNavigation>().CanSpawn(p,1.1f,0,false))continue;
                    var separate=true;foreach(var other in nodes)if(Vector2.Distance(other.Position,p)<2.3f)separate=false;if(!separate)continue;
                    nodes.Add(new ResourceSnapshot{Id="poi:"+anchor.Id+":"+n,Rule=rule,Position=p,Scale=1,HP=prefab.Health.Maximum});break;
                }
            }
            return nodes.ToArray();
        }
        public bool RecordPoiResource(ResourceSnapshot state)
        {
            if(!state.Id.StartsWith("poi:",StringComparison.Ordinal))return false;
            var end=state.Id.LastIndexOf(':');var id=state.Id.Substring(4,end-4);
            if(features.TryGetValue(id,out var f) && f.Site!=null)for(var i=0;i<f.Site.Nodes.Length;i++)if(f.Site.Nodes[i].Id==state.Id){f.Site.Nodes[i]=state;break;}
            return true;
        }
        public void TryPoiGuard(PoiSite site)
        {
            if(site.Kind!=WorldAnchorKind.Graveyard || features.ContainsKey(site.GuardId))return;
            Physics2D.SyncTransforms();for(var i=0;i<16;i++)
            {
                var a=i*Mathf.PI/8;var p=(Vector2)site.transform.position+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*5;
                if(!World.GetComponent<WorldNavigation>().CanSpawn(p,.6f,3,false))continue;
                var guard=World.GetComponent<ZombieSpawner>().SpawnCavern(p,false,site.GuardId);
                if(guard!=null)features[site.GuardId]=new WorldFeatureSnapshot{Id=site.GuardId,Visited=true};return;
            }
        }
        private void ReconcilePoi(){foreach(var site in poiSites.Values)if(site.Kind==WorldAnchorKind.Graveyard)TryPoiGuard(site);}
        private void NotifyPoiGuard(string id){if(!id.EndsWith(":guard",StringComparison.Ordinal))return;var poi=id.Substring(0,id.Length-6);if(poiSites.TryGetValue(poi,out var site))site.Notify();}
    }
}
