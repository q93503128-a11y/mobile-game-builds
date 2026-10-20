using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class AnimalPopulation
    {
        private readonly Dictionary<string,AnimalSnapshot> sleeping=new Dictionary<string,AnimalSnapshot>();
        private readonly Dictionary<WorldChunk,HashSet<string>> sleepingChunks=new Dictionary<WorldChunk,HashSet<string>>();
        private void StoreSleeper(AnimalSnapshot state){sleeping[state.Id]=state;var key=WorldChunk.At(state.Position,World.Definition.ChunkSize);if(!sleepingChunks.TryGetValue(key,out var ids))sleepingChunks[key]=ids=new HashSet<string>();ids.Add(state.Id);}
        private void RemoveSleeper(string id){var key=WorldChunk.At(sleeping[id].Position,World.Definition.ChunkSize);sleeping.Remove(id);if(sleepingChunks.TryGetValue(key,out var ids)){ids.Remove(id);if(ids.Count==0)sleepingChunks.Remove(key);}}
        private readonly HashSet<int> initializedRegions=new HashSet<int>();
        private readonly HashSet<int> expandedRegions=new HashSet<int>();
        private readonly Dictionary<long,int> logicalCounts=new Dictionary<long,int>();
        private int recoveryCursor;
        private long CountKey(int rule,int region)=>((long)rule<<32)|(uint)region;
        private int LogicalCount(int rule,int region){logicalCounts.TryGetValue(CountKey(rule,region),out var n);return n;}
        private void AdjustLogical(int rule,int region,int delta){var key=CountKey(rule,region);logicalCounts.TryGetValue(key,out var n);logicalCounts[key]=Math.Max(0,n+delta);}
        private int StreamTarget(int rule,WorldChunk key)
        {var r=Definition.Rules[rule];if(r.RegionalTarget<=0)return 2;var random=new System.Random(WorldSeeds.Derive(World.Seed,SeedDomain.Creature,key.X,key.Y,rule+701));return random.NextDouble()<r.ChunkChance ? Math.Max(r.RegionalTarget,r.Prefab.Member.Definition.MinimumGroup) : 0;}
        private readonly List<string> streamActorKeys=new List<string>();
        private readonly Dictionary<WorldChunk,int> chunkCounts=new Dictionary<WorldChunk,int>();
        private int ChunkQuota=>Mathf.Max(2,Definition.HardCap/Mathf.Max(1,World.Chunks.WantedCount));
        private int InChunk(WorldChunk key){var count=0;foreach(var a in live.Values)if(a!=null && WorldChunk.At(a.transform.position,World.Definition.ChunkSize).Equals(key))count++;return count;}
        private void ResetStreamed(){live.Clear();sleeping.Clear();sleepingChunks.Clear();pending.Clear();initializedRegions.Clear();expandedRegions.Clear();logicalCounts.Clear();serial=groupSerial=cursor=recoveryCursor=0;}
        public void ActivateChunk(WorldChunk key)
        {
            if(!World.Map.Streamed || !SimulationAuthority.Allows(this))return;var region=WorldMap.Encode(key);streamActorKeys.Clear();if(sleepingChunks.TryGetValue(key,out var ids))streamActorKeys.AddRange(ids);
            var count=InChunk(key);foreach(var id in streamActorKeys){if(ActiveCount>=Definition.HardCap || count>=ChunkQuota)break;if(SpeciesCount(sleeping[id].Rule)>=Definition.Rules[sleeping[id].Rule].Population)continue;Spawn(sleeping[id]);RemoveSleeper(id);count++;}
            var first=initializedRegions.Add(region);var expanded=expandedRegions.Add(region);
            // Save11 expansion preserves the original five rule indices and all living IDs.
            if(first || expanded){var start=first ? 0 : 5;var length=Definition.Rules.Length-start;var offset=(int)((uint)WorldSeeds.Derive(World.Seed,SeedDomain.Creature,key.X,key.Y,991)%length);for(var i=0;i<length;i++)SpawnStreamedGroup(start+(i+offset)%length,key,true);}
        }
        private int SpawnStreamedGroup(int rule,WorldChunk chunk,bool initial)
        {
            var region=WorldMap.Encode(chunk);var deficit=StreamTarget(rule,chunk)-LogicalCount(rule,region);if(deficit<=0 || live.Count+sleeping.Count>=50000)return 0;
            var creature=Definition.Rules[rule].Prefab.Member.Definition;var random=new System.Random(WorldSeeds.Derive(World.Seed,SeedDomain.Creature,chunk.X,chunk.Y,initial ? rule : ++cursor));var spawned=0;var habitatFound=false;
            for(var attempt=0;attempt<24;attempt++)
            {
                var p=chunk.Origin(World.Definition.ChunkSize)+new Vector2((float)random.NextDouble(),(float)random.NextDouble())*World.Definition.ChunkSize;
                if(!creature.Matches(World.Map,p))continue;habitatFound=true;if(!Navigation.CanSpawn(p,creature.SeparationRadius+.15f,initial ? 7 : Definition.PlayerDistance,!initial))continue;
                var group="group"+chunk+":"+rule+":"+(++groupSerial);var count=Math.Min(deficit,random.Next(creature.MinimumGroup,creature.MaximumGroup+1));
                for(var i=0;i<count;i++){var position=p+Vector2.right*i*creature.PreferredSpacing;if(!creature.Matches(World.Map,position) || !Navigation.CanSpawn(position,creature.SeparationRadius+.15f,initial ? 7 : Definition.PlayerDistance,!initial))continue;var id=initial ? "a"+chunk+":"+rule+":"+i : "n"+(++serial);if(live.ContainsKey(id) || sleeping.ContainsKey(id))continue;var state=new AnimalSnapshot{Id=id,Species=creature.Id,Rule=rule,GroupId=group,Region=region,Position=position,Home=p,HP=Definition.Rules[rule].Prefab.Health.Maximum};AdjustLogical(rule,region,1);if(ActiveCount<Definition.HardCap && InChunk(chunk)<ChunkQuota && SpeciesCount(rule)<Definition.Rules[rule].Population)Spawn(state);else StoreSleeper(state);spawned++;}return spawned;
            }
            if(initial && habitatFound)Schedule(rule,region,Clock.Model.Elapsed);return 0;
        }
        private AnimalSnapshot Snapshot(AnimalIdentity a)
        {var s=a.State;return new AnimalSnapshot{Id=s.Id,Species=s.Species,GroupId=s.GroupId,Rule=s.Rule,Region=s.Region,Position=a.transform.position,Home=s.Home,HP=a.Animal.Health.Current};}
        public void UnloadChunk(WorldChunk key)
        {
            streamActorKeys.Clear();foreach(var pair in live)if(pair.Value!=null && WorldChunk.At(pair.Value.transform.position,World.Definition.ChunkSize).Equals(key))streamActorKeys.Add(pair.Key);SleepActors();
        }
        public void StreamReconcile(WorldChunkRuntime runtime)
        {
            streamActorKeys.Clear();chunkCounts.Clear();foreach(var pair in live)if(pair.Value!=null){var key=WorldChunk.At(pair.Value.transform.position,World.Definition.ChunkSize);chunkCounts.TryGetValue(key,out var count);if(!runtime.Wants(pair.Value.transform.position) || count>=ChunkQuota)streamActorKeys.Add(pair.Key);else chunkCounts[key]=count+1;}SleepActors();
            foreach(var key in runtime.ActiveChunks.Keys)ActivateChunk(key);
        }
        private void SleepActors(){foreach(var id in streamActorKeys){var actor=live[id];if(actor.Animal.Health.IsAlive)StoreSleeper(Snapshot(actor));live.Remove(id);PrefabPool.Release(actor.gameObject);}}
        private AnimalSnapshot[] CaptureStreamed(){var result=new List<AnimalSnapshot>(sleeping.Values);foreach(var a in live.Values)if(a!=null && a.Animal.Health.IsAlive)result.Add(Snapshot(a));return result.ToArray();}
        private void RestoreStreamed(AnimalSnapshot[] states,AnimalEcologySnapshot ecology)
        {ResetStreamed();foreach(var s in states){StoreSleeper(s);AdjustLogical(s.Rule,s.Region,1);}if(ecology!=null){serial=ecology.Serial;groupSerial=ecology.GroupSerial;cursor=ecology.Cursor;pending.AddRange(ecology.Pending);foreach(var r in ecology.InitializedRegions??Array.Empty<int>())initializedRegions.Add(r);foreach(var r in ecology.ExpandedRegions??Array.Empty<int>())expandedRegions.Add(r);}}
        private int TickStreamed()
        {
            var count=0;var examined=Math.Min(32,pending.Count);for(var n=0;n<examined && pending.Count>0;n++){recoveryCursor%=pending.Count;var t=pending[recoveryCursor];var key=WorldMap.Decode(t.Region);if(LogicalCount(t.Rule,t.Region)>=StreamTarget(t.Rule,key)){pending.RemoveAt(recoveryCursor);continue;}if(t.Due>Clock.Model.Elapsed || !World.Chunks.Wants(key.Center(World.Definition.ChunkSize))){recoveryCursor++;continue;}count+=SpawnStreamedGroup(t.Rule,key,false);t.Due=Clock.Model.Elapsed+Definition.Rules[t.Rule].RespawnSeconds;recoveryCursor++;if(count>=2)break;}return count;
        }
    }
}
