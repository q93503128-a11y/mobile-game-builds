using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class AnimalPopulation : MonoBehaviour
    {
        public AnimalPopulationDefinition Definition;
        public WorldGenerator World;
        public WorldClock Clock;
        public WorldNavigation Navigation;
        private readonly Dictionary<string,AnimalIdentity> live=new Dictionary<string,AnimalIdentity>();
        private readonly List<RecoveryTicket> pending=new List<RecoveryTicket>();
        private int[,] targets;
        private int serial,groupSerial,cursor;
        private double nextAttempt;
        public int ActiveCount=>live.Count;
        public IReadOnlyList<RecoveryTicket> Pending=>pending;
        public static int Region(WorldMap map,Vector2 p)
        {var size=map.Definition.Size;return Mathf.Clamp(Mathf.FloorToInt((p.x/size.x+.5f)*4),0,3)+4*Mathf.Clamp(Mathf.FloorToInt((p.y/size.y+.5f)*3),0,2);}
        public int Count(int rule,int region)
        {var count=0;foreach(var actor in live.Values)if(actor!=null && actor.State.Rule==rule && actor.State.Region==region)count++;return count;}
        public int Target(int rule,int region)=>targets[rule,region];
        public int SpeciesCount(int rule){var count=0;for(var i=0;i<12;i++)count+=Count(rule,i);return count;}
        private Vector2 Candidate(int region,System.Random random)=>Candidate(World.Definition.Size,region,random);
        private static Vector2 Candidate(Vector2 size,int region,System.Random random)
        {return new Vector2(((region%4+(float)random.NextDouble())/4-.5f)*size.x,((region/4+(float)random.NextDouble())/3-.5f)*size.y);}
        private void Initialize()=>targets=BuildTargets(World.Map,Definition);
        public static int[,] BuildTargets(WorldMap map,AnimalPopulationDefinition definition)
        {
            var targets=new int[definition.Rules.Length,12];
            for(var rule=0;rule<definition.Rules.Length;rule++)
            {
                var suitable=new List<int>();var creature=definition.Rules[rule].Prefab.Member.Definition;var sample=new System.Random(map.Seed^rule*397);
                for(var region=0;region<12;region++)for(var i=0;i<64;i++)if(creature.Matches(map,Candidate(map.Definition.Size,region,sample))){suitable.Add(region);break;}
                for(var i=0;i<definition.Rules[rule].Population && suitable.Count>0;i++)targets[rule,suitable[i%suitable.Count]]++;
            }
            return targets;
        }
        public void Generate()
        {
            live.Clear();pending.Clear();serial=groupSerial=cursor=0;nextAttempt=0;Initialize();
            for(var rule=0;rule<Definition.Rules.Length;rule++)for(var region=0;region<12;region++)
            {while(Count(rule,region)<Target(rule,region) && ActiveCount<Definition.HardCap){if(SpawnGroup(rule,region,false)==0)break;}Schedule(rule,region,Clock.Model.Elapsed);}
        }
        private bool FindPosition(int rule,int region,bool outside,out Vector2 p)
        {
            var random=new System.Random(World.Seed^++cursor*7919);var creature=Definition.Rules[rule].Prefab.Member.Definition;
            for(var i=0;i<96;i++)
            {
                p=Candidate(region,random);
                if(CanSpawn(creature,p,outside))return true;
            }
            p=default;return false;
        }
        private bool CanSpawn(CreatureDefinition creature,Vector2 position,bool outside)
        {
            if(!creature.Matches(World.Map,position) || !Navigation.CanSpawn(position,creature.SeparationRadius+.15f,Definition.PlayerDistance,outside))return false;
            foreach(var placed in FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None))if(Vector2.Distance(placed.transform.position,position)<Definition.BaseDistance)return false;
            return true;
        }
        private int SpawnGroup(int rule,int region,bool outside)
        {
            if(!FindPosition(rule,region,outside,out var center))return 0;
            var creature=Definition.Rules[rule].Prefab.Member.Definition;var random=new System.Random(World.Seed^++groupSerial*1741);
            // A group may cross its regional target by a few animals; the species and performance caps remain strict.
            var amount=Math.Min(random.Next(creature.MinimumGroup,creature.MaximumGroup+1),Math.Min(Definition.HardCap-ActiveCount,Definition.Rules[rule].Population-SpeciesCount(rule)));
            var group="ag"+groupSerial;var spawned=0;
            for(var i=0;i<amount;i++)
            {
                var position=center;
                if(i>0){var angle=(float)random.NextDouble()*Mathf.PI*2;position+=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*creature.PreferredSpacing*(.8f+(float)random.NextDouble()*.4f);}
                if(!CanSpawn(creature,position,outside))continue;
                var state=new AnimalSnapshot{Id="n"+(++serial),Rule=rule,Species=creature.Id,GroupId=group,Region=region,Home=center,Position=position};Spawn(state);spawned++;
            }
            return spawned;
        }
        private void Spawn(AnimalSnapshot state)
        {
            var actor=Instantiate(Definition.Rules[state.Rule].Prefab,state.Position,Quaternion.identity,World.Environment);
            actor.Player=World.Player.transform;actor.Session=World.Session;actor.Region=World.Region;actor.Member.Anchor=state.Home;actor.Member.GroupId=state.GroupId;actor.Separation.Player=actor.Player;
            var loot=actor.GetComponent<LootOnDeath>();loot.Player=World.Player;loot.Pickup=World.Pickup;loot.Settings=World.Settings;
            state.Missing=false;if(state.HP>0)actor.Health.RestoreHealth(state.HP);state.HP=actor.Health.Current;state.RespawnAt=0;
            var identity=actor.gameObject.AddComponent<AnimalIdentity>();identity.State=state;identity.Animal=actor;identity.Owner=this;identity.Bind();live.Add(state.Id,identity);
        }
        public void Removed(AnimalIdentity identity)
        {live.Remove(identity.State.Id);Schedule(identity.State.Rule,identity.State.Region,Clock.Model.Elapsed);}
        private void Schedule(int rule,int region,double now)
        {if(Count(rule,region)<Target(rule,region) && SpeciesCount(rule)<Definition.Rules[rule].Population && !pending.Exists(t=>t.Rule==rule && t.Region==region))pending.Add(new RecoveryTicket{Rule=rule,Region=region,Due=now+Definition.Rules[rule].RespawnSeconds});}
        private void Update(){if(World.Session.WorldIsRunning && Clock.Model.Elapsed>=nextAttempt){nextAttempt=Clock.Model.Elapsed+5;Tick();}}
        public int Tick()
        {
            var count=0;var now=Clock.Model.Elapsed;
            for(var i=pending.Count-1;i>=0;i--)
            {
                var ticket=pending[i];if(now<ticket.Due)continue;
                if(Count(ticket.Rule,ticket.Region)>=Target(ticket.Rule,ticket.Region) || SpeciesCount(ticket.Rule)>=Definition.Rules[ticket.Rule].Population){pending.RemoveAt(i);continue;}
                if(ActiveCount<Definition.HardCap && count<5)count+=SpawnGroup(ticket.Rule,ticket.Region,true);
                ticket.Due=now+Definition.Rules[ticket.Rule].RespawnSeconds;
                if(Count(ticket.Rule,ticket.Region)>=Target(ticket.Rule,ticket.Region))pending.RemoveAt(i);
            }
            return count;
        }
        public AnimalSnapshot[] Capture()
        {
            var result=new List<AnimalSnapshot>();foreach(var identity in live.Values)if(identity!=null && identity.Animal.Health.IsAlive){var s=identity.State;result.Add(new AnimalSnapshot{Id=s.Id,Species=s.Species,Rule=s.Rule,GroupId=s.GroupId,Region=s.Region,HP=identity.Animal.Health.Current,Position=identity.transform.position,Home=s.Home});}return result.ToArray();
        }
        public AnimalEcologySnapshot CaptureEcology()
        {var tickets=new RecoveryTicket[pending.Count];for(var i=0;i<tickets.Length;i++){var t=pending[i];tickets[i]=new RecoveryTicket{Rule=t.Rule,Region=t.Region,Due=t.Due};}return new AnimalEcologySnapshot{Serial=serial,GroupSerial=groupSerial,Cursor=cursor,Pending=tickets};}
        public void Restore(AnimalSnapshot[] states,AnimalEcologySnapshot ecology=null)
        {
            foreach(var identity in live.Values)if(identity!=null){identity.gameObject.SetActive(false);Destroy(identity.gameObject);}live.Clear();pending.Clear();Initialize();
            serial=ecology==null ? 0 : ecology.Serial;groupSerial=ecology==null ? 0 : ecology.GroupSerial;cursor=ecology==null ? 0 : ecology.Cursor;
            if(ecology!=null)pending.AddRange(ecology.Pending);
            foreach(var state in states){if(state.Missing)continue;Spawn(state);}
            for(var r=0;r<Definition.Rules.Length;r++)for(var region=0;region<12;region++)Schedule(r,region,Clock.Model.Elapsed);
        }
    }
}
