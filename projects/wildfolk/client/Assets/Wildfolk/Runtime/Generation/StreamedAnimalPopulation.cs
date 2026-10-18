using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class AnimalPopulation
    {
        private readonly Dictionary<string,AnimalSnapshot> sleeping=new Dictionary<string,AnimalSnapshot>();
        private readonly HashSet<int> initializedRegions=new HashSet<int>();
        private readonly List<string> streamActorKeys=new List<string>();
        private readonly Dictionary<WorldChunk,int> chunkCounts=new Dictionary<WorldChunk,int>();
        private int ChunkQuota=>Mathf.Max(2,Definition.HardCap/Mathf.Max(1,World.Chunks.WantedCount));
        private int InChunk(WorldChunk key){var count=0;foreach(var a in live.Values)if(a!=null && WorldChunk.At(a.transform.position,World.Definition.ChunkSize).Equals(key))count++;return count;}
        private void ResetStreamed(){live.Clear();sleeping.Clear();pending.Clear();initializedRegions.Clear();serial=groupSerial=cursor=0;}
        public void ActivateChunk(WorldChunk key)
        {
            if(!World.Map.Streamed || !SimulationAuthority.Allows(this))return;var region=WorldMap.Encode(key);streamActorKeys.Clear();foreach(var pair in sleeping)if(WorldChunk.At(pair.Value.Position,World.Definition.ChunkSize).Equals(key))streamActorKeys.Add(pair.Key);
            var count=InChunk(key);foreach(var id in streamActorKeys){if(ActiveCount>=Definition.HardCap || count>=ChunkQuota)break;Spawn(sleeping[id]);sleeping.Remove(id);count++;}
            if(initializedRegions.Add(region))for(var rule=0;rule<Definition.Rules.Length;rule++)SpawnStreamedGroup(rule,key,true);
        }
        private int SpawnStreamedGroup(int rule,WorldChunk chunk,bool initial)
        {
            if(!initial && ActiveCount>=Definition.HardCap || World.Map.Continental.Underground(chunk.Center(World.Definition.ChunkSize)))return 0;
            var creature=Definition.Rules[rule].Prefab.Member.Definition;var random=new System.Random(WorldSeeds.Derive(World.Seed,SeedDomain.Creature,chunk.X,chunk.Y,initial ? rule : ++cursor));var spawned=0;
            for(var attempt=0;attempt<24;attempt++)
            {
                var p=chunk.Origin(World.Definition.ChunkSize)+new Vector2((float)random.NextDouble(),(float)random.NextDouble())*World.Definition.ChunkSize;
                if(!creature.Matches(World.Map,p) || !Navigation.CanSpawn(p,.7f,7,false))continue;
                var group="group"+chunk+":"+rule+":"+(++groupSerial);var count=random.Next(creature.MinimumGroup,creature.MaximumGroup+1);
                for(var i=0;i<count && (initial || ActiveCount<Definition.HardCap);i++){var position=p+Vector2.right*i*creature.PreferredSpacing;if(!creature.Matches(World.Map,position) || !World.Map.IsWalkable(position,.7f))continue;var id=initial ? "a"+chunk+":"+rule+":"+i : "n"+(++serial);var state=new AnimalSnapshot{Id=id,Species=creature.Id,Rule=rule,GroupId=group,Region=WorldMap.Encode(chunk),Position=position,Home=p,HP=Definition.Rules[rule].Prefab.Health.Maximum};if(ActiveCount<Definition.HardCap && InChunk(chunk)<ChunkQuota)Spawn(state);else sleeping[id]=state;spawned++;}return spawned;
            }
            return 0;
        }
        private AnimalSnapshot Snapshot(AnimalIdentity a)
        {var s=a.State;return new AnimalSnapshot{Id=s.Id,Species=s.Species,GroupId=s.GroupId,Rule=s.Rule,Region=World.Map.PopulationRegion(a.transform.position),Position=a.transform.position,Home=s.Home,HP=a.Animal.Health.Current};}
        public void UnloadChunk(WorldChunk key)
        {
            streamActorKeys.Clear();foreach(var pair in live)if(pair.Value!=null && WorldChunk.At(pair.Value.transform.position,World.Definition.ChunkSize).Equals(key))streamActorKeys.Add(pair.Key);SleepActors();
        }
        public void StreamReconcile(WorldChunkRuntime runtime)
        {
            streamActorKeys.Clear();chunkCounts.Clear();foreach(var pair in live)if(pair.Value!=null){var key=WorldChunk.At(pair.Value.transform.position,World.Definition.ChunkSize);chunkCounts.TryGetValue(key,out var count);if(!runtime.Wants(pair.Value.transform.position) || count>=ChunkQuota)streamActorKeys.Add(pair.Key);else chunkCounts[key]=count+1;}SleepActors();
            foreach(var key in runtime.ActiveChunks.Keys)ActivateChunk(key);
        }
        private void SleepActors(){foreach(var id in streamActorKeys){var actor=live[id];if(actor.Animal.Health.IsAlive)sleeping[id]=Snapshot(actor);live.Remove(id);PrefabPool.Release(actor.gameObject);}}
        private AnimalSnapshot[] CaptureStreamed(){var result=new List<AnimalSnapshot>(sleeping.Values);foreach(var a in live.Values)if(a!=null && a.Animal.Health.IsAlive)result.Add(Snapshot(a));return result.ToArray();}
        private void RestoreStreamed(AnimalSnapshot[] states,AnimalEcologySnapshot ecology)
        {ResetStreamed();foreach(var s in states)sleeping.Add(s.Id,s);if(ecology!=null){serial=ecology.Serial;groupSerial=ecology.GroupSerial;cursor=ecology.Cursor;pending.AddRange(ecology.Pending);foreach(var r in ecology.InitializedRegions??Array.Empty<int>())initializedRegions.Add(r);}}
        private int TickStreamed()
        {
            var count=0;for(var i=pending.Count-1;i>=0;i--){var t=pending[i];if(t.Due>Clock.Model.Elapsed || !World.Chunks.Wants(WorldMap.Decode(t.Region).Center(World.Definition.ChunkSize)))continue;count+=SpawnStreamedGroup(t.Rule,WorldMap.Decode(t.Region),false);pending.RemoveAt(i);if(count>=5)break;}return count;
        }
    }
}
