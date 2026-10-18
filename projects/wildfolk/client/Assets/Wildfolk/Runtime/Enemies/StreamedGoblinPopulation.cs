using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class GoblinSpawner
    {
        private readonly Dictionary<string,CampSnapshot> sleepingCamps=new Dictionary<string,CampSnapshot>();
        private readonly Dictionary<string,GoblinActorSnapshot> sleepingGoblins=new Dictionary<string,GoblinActorSnapshot>();
        private readonly HashSet<string> initializedCamps=new HashSet<string>();
        private readonly List<string> sleepingKeys=new List<string>();
        public void ActivateChunk(WorldChunk key)
        {
            if(!World.Map.Streamed || !SimulationAuthority.Allows(this))return;
            sleepingKeys.Clear();foreach(var c in sleepingCamps.Values)if(WorldChunk.At(c.Position,World.Definition.ChunkSize).Equals(key))sleepingKeys.Add(c.Id);foreach(var id in sleepingKeys){var s=sleepingCamps[id];SpawnCamp(s.Position,s.Id,s.RefillAt,false);sleepingCamps.Remove(id);}
            foreach(var a in World.Map.Continental.Anchors)if(a.Kind==WorldAnchorKind.GoblinCamp && WorldChunk.At(a.Position,World.Definition.ChunkSize).Equals(key) && initializedCamps.Add(a.Id))SpawnCamp(a.Position,a.Id);
            sleepingKeys.Clear();foreach(var s in sleepingGoblins.Values)if(WorldChunk.At(s.Position,World.Definition.ChunkSize).Equals(key))sleepingKeys.Add(s.Id);foreach(var id in sleepingKeys){var s=sleepingGoblins[id];SpawnActor(s.Ranged,s.Position,s.GroupId,s.Home,s.CampId,s.Id,s.HP);sleepingGoblins.Remove(id);}
        }
        public void UnloadChunk(WorldChunk key)=>SleepOutside(key,null);
        public void StreamReconcile(WorldChunkRuntime runtime)=>SleepOutside(default,runtime);
        private void SleepOutside(WorldChunk key,WorldChunkRuntime runtime)
        {
            sleepingKeys.Clear();foreach(var pair in actors)if(pair.Value!=null && Outside(pair.Value.transform.position,key,runtime))sleepingKeys.Add(pair.Key);
            foreach(var id in sleepingKeys){var a=actors[id];var s=a.State;s.Position=a.transform.position;s.Home=a.Actor.Home;s.HP=a.Actor.OwnHealth.Current;sleepingGoblins[id]=s;actors.Remove(id);PrefabPool.Release(a.gameObject);}
            for(var i=camps.Count-1;i>=0;i--){var c=camps[i];if(c!=null && Outside(c.transform.position,key,runtime)){sleepingCamps[c.State.Id]=c.State;camps.RemoveAt(i);PrefabPool.Release(c.gameObject);}}
        }
        private bool Outside(Vector2 p,WorldChunk key,WorldChunkRuntime runtime)=>runtime==null ? WorldChunk.At(p,World.Definition.ChunkSize).Equals(key) : !runtime.Wants(p);
        private void RestoreStreamed(GoblinEcologySnapshot state)
        {
            restored=true;sleepingCamps.Clear();sleepingGoblins.Clear();initializedCamps.Clear();if(state==null)return;serial=state.Serial;groupSerial=state.GroupSerial;
            foreach(var c in state.Camps){sleepingCamps.Add(c.Id,c);initializedCamps.Add(c.Id);}foreach(var s in state.Actors)sleepingGoblins.Add(s.Id,s);
        }
        private GoblinEcologySnapshot CaptureStreamed(GoblinEcologySnapshot state)
        {var locations=new List<CampSnapshot>(state.Camps);locations.AddRange(sleepingCamps.Values);var saved=new List<GoblinActorSnapshot>(state.Actors);saved.AddRange(sleepingGoblins.Values);state.Camps=locations.ToArray();state.Actors=saved.ToArray();return state;}
    }
}
