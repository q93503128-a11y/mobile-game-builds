using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class ResourceRegeneration
    {
        private readonly List<string> streamKeys=new List<string>();
        private readonly Dictionary<string,double> renewableAt=new Dictionary<string,double>();
        public void ReleaseWorld(){foreach(var a in live.Values)if(a!=null)PrefabPool.Release(a.gameObject);live.Clear();}
        public void Materialize(ResourcePlacement p,Transform parent)
        {
            if(live.ContainsKey(p.Id))return;
            if(changes.TryGetValue(p.Id,out var state) && state.Destroyed)
            {
                if(!World.Map.Renewable(p.Rule))return;
                if(!renewableAt.TryGetValue(p.Id,out var due))renewableAt[p.Id]=due=state.RespawnAt;
                if(Clock.Model.Elapsed<due)return;
                // A new recovery ID at a different position is reserved; the old tombstone remains.
                var position=p.Position+new Vector2(3,4);if(!World.Map.ResourceSiteAllowed(p.Rule,position))return;
                var id="p"+(++Population.Serial);var n=World.CreateResource(id,p.Rule,position,p.Scale,parent);changes[id]=new ResourceSnapshot{Id=id,Rule=p.Rule,Position=position,Scale=p.Scale,HP=n.Health.Maximum};renewableAt.Remove(p.Id);state.RespawnAt=double.MaxValue;return;
            }
            var node=World.CreateResource(p.Id,p.Rule,p.Position,p.Scale,parent);if(state!=null)node.Health.RestoreHealth(state.HP);
        }
        public void MaterializeRecovered(WorldChunk key,Transform parent)
        {
            // Recovery can cross a chunk boundary. It must not depend on a base source in this chunk.
            streamKeys.Clear();foreach(var s in changes.Values)if(!s.Destroyed && s.Id.StartsWith("p") && !live.ContainsKey(s.Id) && WorldChunk.At(s.Position,World.Definition.ChunkSize).Equals(key))streamKeys.Add(s.Id);
            foreach(var id in streamKeys){var s=changes[id];var recovered=World.CreateResource(s.Id,s.Rule,s.Position,s.Scale,parent);recovered.Health.RestoreHealth(s.HP);}
        }
        private void RecordStreamed(ResourceIdentity identity,ResourceSnapshot state,bool destroyed)
        {
            if(destroyed){live.Remove(state.Id);if(World.Map.Renewable(state.Rule)){state.RespawnAt=Clock.Model.Elapsed+World.Definition.Resources[state.Rule].RespawnSeconds;renewableAt[state.Id]=state.RespawnAt;}}
            changes[state.Id]=state;
        }
        public void UnloadChunk(WorldChunk chunk)
        {
            streamKeys.Clear();foreach(var pair in live)if(pair.Value!=null && WorldChunk.At(pair.Value.transform.position,World.Definition.ChunkSize).Equals(chunk))streamKeys.Add(pair.Key);
            foreach(var id in streamKeys){var identity=live[id];live.Remove(id);PrefabPool.Release(identity.gameObject);}
        }
        private int TickStreamed()
        {
            if(World.Chunks==null)return 0;var before=live.Count;
            streamKeys.Clear();foreach(var pair in renewableAt)if(pair.Value<=Clock.Model.Elapsed && changes.TryGetValue(pair.Key,out var s) && World.Chunks.IsActive(s.Position))streamKeys.Add(pair.Key);
            // A bounded recovery tick checks only scheduled active sources.
            var due=streamKeys.ToArray();for(var i=0;i<due.Length && i<2;i++){var s=changes[due[i]];var key=WorldChunk.At(s.Position,World.Definition.ChunkSize);Materialize(new ResourcePlacement(s.Rule,s.Position,s.Scale,s.Id),World.Chunks.ActiveChunks[key].transform);}
            return Mathf.Max(0,live.Count-before);
        }
    }
}
