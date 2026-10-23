using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class ResourceRegeneration
    {
        private readonly List<string> streamKeys=new List<string>();
        private readonly Dictionary<WorldChunk,Dictionary<string,ResourceSnapshot>> chunkChanges=new Dictionary<WorldChunk,Dictionary<string,ResourceSnapshot>>();
        private int recoveryCursor;
        private readonly List<RecoveryTicket> activeTickets=new List<RecoveryTicket>();
        public int LastRecoveryChecks {get;private set;}
        public int LastCandidateChecks {get;private set;}
        public void ReleaseWorld(){foreach(var a in live.Values)if(a!=null)PrefabPool.Release(a.gameObject);live.Clear();}
        private void Index(ResourceSnapshot state)
        {var key=WorldChunk.At(state.Position,World.Definition.ChunkSize);if(!chunkChanges.TryGetValue(key,out var list))chunkChanges[key]=list=new Dictionary<string,ResourceSnapshot>();list[state.Id]=state;}
        public void PrepareChunk(WorldChunk key,List<ResourcePlacement> sources)
        {
            var region=WorldMap.Encode(key);if(Population.HasRegion(region))return;chunkChanges.TryGetValue(key,out var changed);
            Population.InitializeRegion(region,sources,changed?.Values,Clock.Model.Elapsed);
        }
        public void Materialize(ResourcePlacement p,Transform parent)
        {
            if(World.GetComponent<BossWorld>()?.ExcludesResource(p.Rule,p.Position)==true)return;
            if(live.ContainsKey(p.Id))return;
            if(changes.TryGetValue(p.Id,out var state) && state.Destroyed)return;
            var node=World.CreateResource(p.Id,p.Rule,p.Position,p.Scale,parent);if(state!=null)node.Health.RestoreHealth(state.HP);
        }
        public void MaterializeRecovered(WorldChunk key,Transform parent)
        {
            if(!chunkChanges.TryGetValue(key,out var entries))return;
            streamKeys.Clear();foreach(var s in entries.Values)if(!s.Destroyed && s.Id.StartsWith("p") && !live.ContainsKey(s.Id))streamKeys.Add(s.Id);
            foreach(var id in streamKeys){var s=changes[id];if(World.GetComponent<BossWorld>()?.ExcludesResource(s.Rule,s.Position)==true)continue;var recovered=World.CreateResource(s.Id,s.Rule,s.Position,s.Scale,parent);recovered.Health.RestoreHealth(s.HP);}
        }
        private void RecordStreamed(ResourceIdentity identity,ResourceSnapshot state,bool destroyed)
        {
            if(destroyed)
            {
                if(!live.Remove(state.Id))return;
                Population.Remove(state.Rule,state.Position,Clock.Model.Elapsed,true);
                // Only baseline c/g/e sources need permanent tombstones. A depleted p source becomes population debt.
                if(state.Id.StartsWith("p")){changes.Remove(state.Id);var key=WorldChunk.At(state.Position,World.Definition.ChunkSize);if(chunkChanges.TryGetValue(key,out var list))list.Remove(state.Id);return;}
            }
            changes[state.Id]=state;Index(state);
        }
        public void UnloadChunk(WorldChunk chunk)
        {
            streamKeys.Clear();foreach(var pair in live)if(pair.Value!=null && WorldChunk.At(pair.Value.transform.position,World.Definition.ChunkSize).Equals(chunk))streamKeys.Add(pair.Key);
            foreach(var id in streamKeys){var identity=live[id];live.Remove(id);PrefabPool.Release(identity.gameObject);}
        }
        private int TickStreamed()
        {
            LastRecoveryChecks=LastCandidateChecks=0;if(World.Chunks==null)return 0;
            var now=Clock.Model.Elapsed;Population.Expire(now);var spawned=0;activeTickets.Clear();
            // Dormant history cannot delay a player's current region. Only active chunk/rule pairs are looked up.
            foreach(var key in World.Chunks.ActiveChunks.Keys)for(var r=0;r<World.Definition.Resources.Length;r++){var t=Population.FindTicket(r,WorldMap.Encode(key));if(t!=null)activeTickets.Add(t);}
            var limit=Mathf.Min(16,activeTickets.Count);
            while(LastRecoveryChecks<limit && activeTickets.Count>0)
            {
                LastRecoveryChecks++;recoveryCursor%=activeTickets.Count;var ticket=activeTickets[recoveryCursor++];
                var key=WorldMap.Decode(ticket.Region);if(!World.Chunks.ActiveChunks.TryGetValue(key,out var root))continue;
                if(Population.Count(ticket.Rule,ticket.Region)>=Population.Target(ticket.Rule,ticket.Region)){Population.RemoveTicket(ticket);continue;}
                if(ticket.Due>now || !FindStreamedPosition(ticket.Rule,ticket.Region,out var position))continue;
                var rule=World.Definition.Resources[ticket.Rule];var scale=(rule.ScaleRange.x+rule.ScaleRange.y)*.5f;var id="p"+(++Population.Serial);
                var node=World.CreateResource(id,ticket.Rule,position,scale,root.transform);Population.Add(ticket.Rule,position);
                var state=new ResourceSnapshot{Id=id,Rule=ticket.Rule,Position=position,Scale=scale,HP=node.Health.Maximum};changes[id]=state;Index(state);
                Population.RemoveTicket(ticket);Population.Schedule(ticket.Rule,ticket.Region,now);
                if(++spawned>=2)break;
            }
            return spawned;
        }
        private bool FindStreamedPosition(int ruleIndex,int region,out Vector2 point)
        {
            var rule=World.Definition.Resources[ruleIndex];
            for(var attempt=0;attempt<48;attempt++)
            {
                LastCandidateChecks++;point=Population.Candidate(ruleIndex,region);
                if(!World.Map.ResourceSiteAllowed(ruleIndex,point) || Population.Excluded(ruleIndex,point,Clock.Model.Elapsed))continue;
                // Physics uses the active spatial index, including nodes, creatures and player buildings.
                if(Navigation.CanSpawn(point,Mathf.Max(rule.Spacing,rule.ClusterSpacing),18))return true;
            }
            point=default;return false;
        }
    }
}
