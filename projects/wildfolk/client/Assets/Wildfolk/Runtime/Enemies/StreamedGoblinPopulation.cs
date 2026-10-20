using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class GoblinSpawner
    {
        private readonly Dictionary<string,CampSnapshot> sleepingCamps=new Dictionary<string,CampSnapshot>();
        private readonly Dictionary<string,GoblinActorSnapshot> sleepingGoblins=new Dictionary<string,GoblinActorSnapshot>();
        private readonly Dictionary<WorldChunk,HashSet<string>> campChunks=new Dictionary<WorldChunk,HashSet<string>>(),goblinChunks=new Dictionary<WorldChunk,HashSet<string>>();
        private WorldChunk Chunk(Vector2 p)=>WorldChunk.At(p,World.Definition.ChunkSize);
        private static void Index(Dictionary<WorldChunk,HashSet<string>> chunks,WorldChunk key,string id){if(!chunks.TryGetValue(key,out var ids))chunks[key]=ids=new HashSet<string>();ids.Add(id);}
        private static void Unindex(Dictionary<WorldChunk,HashSet<string>> chunks,WorldChunk key,string id){if(chunks.TryGetValue(key,out var ids)){ids.Remove(id);if(ids.Count==0)chunks.Remove(key);}}
        private readonly Dictionary<string,int> sleepingResidentCounts=new Dictionary<string,int>();
        private void SleepingResident(string camp,int delta){if(string.IsNullOrEmpty(camp))return;sleepingResidentCounts.TryGetValue(camp,out var n);sleepingResidentCounts[camp]=System.Math.Max(0,n+delta);}
        private readonly HashSet<string> initializedCamps=new HashSet<string>();
        private readonly List<string> sleepingKeys=new List<string>();
        public void ActivateChunk(WorldChunk key)
        {
            if(!World.Map.Streamed || !SimulationAuthority.Allows(this))return;
            sleepingKeys.Clear();if(campChunks.TryGetValue(key,out var campIds))sleepingKeys.AddRange(campIds);foreach(var id in sleepingKeys){var s=sleepingCamps[id];SpawnCamp(s.Position,s.Id,s.RefillAt,false,s);sleepingCamps.Remove(id);Unindex(campChunks,key,id);}
            foreach(var a in World.Map.Continental.Anchors)if(a.Kind==WorldAnchorKind.GoblinCamp && WorldChunk.At(a.Position,World.Definition.ChunkSize).Equals(key) && initializedCamps.Add(a.Id))SpawnCamp(a.Position,a.Id);
            sleepingKeys.Clear();if(goblinChunks.TryGetValue(key,out var goblinIds))sleepingKeys.AddRange(goblinIds);foreach(var id in sleepingKeys){var s=sleepingGoblins[id];SpawnActor(s.Ranged,s.Position,s.GroupId,s.Home,s.CampId,s.Id,s.HP);sleepingGoblins.Remove(id);Unindex(goblinChunks,key,id);SleepingResident(s.CampId,-1);}
        }
        public void UnloadChunk(WorldChunk key)=>SleepOutside(key,null);
        public void StreamReconcile(WorldChunkRuntime runtime)=>SleepOutside(default,runtime);
        private void SleepOutside(WorldChunk key,WorldChunkRuntime runtime)
        {
            sleepingKeys.Clear();foreach(var pair in actors)if(pair.Value!=null && Outside(pair.Value.transform.position,key,runtime))sleepingKeys.Add(pair.Key);
            foreach(var id in sleepingKeys){var a=actors[id];var s=a.State;s.Position=a.transform.position;s.Home=a.Actor.Home;s.HP=a.Actor.OwnHealth.Current;sleepingGoblins[id]=s;Index(goblinChunks,Chunk(s.Position),id);SleepingResident(s.CampId,1);actors.Remove(id);PrefabPool.Release(a.gameObject);}
            for(var i=camps.Count-1;i>=0;i--){var c=camps[i];if(c!=null && Outside(c.transform.position,key,runtime)){sleepingCamps[c.State.Id]=c.State;Index(campChunks,Chunk(c.State.Position),c.State.Id);camps.RemoveAt(i);PrefabPool.Release(c.gameObject);}}
        }
        private bool Outside(Vector2 p,WorldChunk key,WorldChunkRuntime runtime)=>runtime==null ? WorldChunk.At(p,World.Definition.ChunkSize).Equals(key) : !runtime.Wants(p);
        private void RestoreStreamed(GoblinEcologySnapshot state)
        {
            restored=true;sleepingCamps.Clear();sleepingGoblins.Clear();campChunks.Clear();goblinChunks.Clear();sleepingResidentCounts.Clear();initializedCamps.Clear();if(state==null)return;serial=state.Serial;groupSerial=state.GroupSerial;
            foreach(var c in state.Camps){sleepingCamps.Add(c.Id,c);Index(campChunks,Chunk(c.Position),c.Id);initializedCamps.Add(c.Id);}foreach(var s in state.Actors){sleepingGoblins.Add(s.Id,s);Index(goblinChunks,Chunk(s.Position),s.Id);SleepingResident(s.CampId,1);}
        }
        private GoblinEcologySnapshot CaptureStreamed(GoblinEcologySnapshot state)
        {var locations=new List<CampSnapshot>(state.Camps);foreach(var camp in sleepingCamps.Values)locations.Add(JsonUtility.FromJson<CampSnapshot>(JsonUtility.ToJson(camp)));var saved=new List<GoblinActorSnapshot>(state.Actors);saved.AddRange(sleepingGoblins.Values);state.Camps=locations.ToArray();state.Actors=saved.ToArray();return state;}
    }
}
