using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class GoblinSpawner : MonoBehaviour
    {
        public GoblinDefinition Definition;
        public GoblinController Prefab,RangedPrefab;
        public GoblinCampDefinition CampDefinition;
        public WorldGenerator World;
        public WorldClock Clock;
        public GameplaySession Session;
        public Health Player;
        public Camera Camera;
        private readonly Dictionary<string,GoblinIdentity> actors=new Dictionary<string,GoblinIdentity>();
        private readonly List<GoblinCamp> camps=new List<GoblinCamp>();
        private readonly List<string> missingKeys=new List<string>();
        private int serial,groupSerial;
        private float elapsed;
        public IReadOnlyList<GoblinCamp> Camps=>camps;
        public int ActiveCount{get{Prune();return actors.Count;}}
        private bool restored;
        private void Start(){if(!restored && SimulationAuthority.Allows(this) && camps.Count==0)GenerateCamps();}
        public void Register(GoblinController goblin)
        {
            foreach(var actor in actors.Values)if(actor.Actor==goblin)return;
            Bind(goblin,new GoblinActorSnapshot{Id="g"+(++serial),GroupId=goblin.Member.GroupId,Home=goblin.Home,Position=goblin.transform.position,Ranged=goblin.Ranged!=null,HP=goblin.OwnHealth.Current});
        }
        private void Bind(GoblinController actor,GoblinActorSnapshot state)
        {var identity=actor.GetComponent<GoblinIdentity>();if(identity==null)identity=actor.gameObject.AddComponent<GoblinIdentity>();identity.Bind(this,actor,state);actors.Add(state.Id,identity);}
        public void Removed(GoblinIdentity actor){actors.Remove(actor.State.Id);var camp=camps.Find(c=>c.State.Id==actor.State.CampId);if(camp!=null && camp.State.RefillAt<=0)camp.State.RefillAt=Clock.Model.Elapsed+CampDefinition.RefillSeconds;}
        private void Prune()
        {missingKeys.Clear();foreach(var pair in actors)if(pair.Value==null || !pair.Value.gameObject.activeInHierarchy)missingKeys.Add(pair.Key);foreach(var key in missingKeys){var actor=actors[key];if(actor!=null)Removed(actor);else actors.Remove(key);}}

        private bool OutsideCamera(Vector2 p){var v=Camera.WorldToViewportPoint(p);return v.x<-.05f || v.x>1.05f || v.y<-.05f || v.y>1.05f;}
        private void Update()
        {
            if(!SimulationAuthority.Allows(this) || !Session.WorldIsRunning || !Player.IsAlive)return;Prune();
            elapsed+=Time.deltaTime;if(elapsed<Definition.SpawnInterval)return;elapsed=0;TickCamps();
            if(Clock.Model.Phase==DayPhase.Day && UnityEngine.Random.value<Definition.EncounterChance)TrySpawn();
        }
        public GoblinController SpawnActor(bool ranged,Vector2 position,string group,Vector2 home,string campId=null,string id=null,int hp=0)
        {
            if(!SimulationAuthority.Allows(this))throw new InvalidOperationException("Host authority required.");
            var actor=PrefabPool.Spawn(ranged ? RangedPrefab : Prefab,position,Quaternion.identity);actor.Session=Session;actor.PlayerHealth=Player;actor.Member.GroupId=group;actor.Member.AdditionalTraits=string.IsNullOrEmpty(campId) ? CreatureTrait.None : CreatureTrait.CampBound;actor.SetHome(home);actor.Separation.Player=Player.transform;actor.Steering.Navigation=World.GetComponent<WorldNavigation>();actor.GetComponent<LootOnDeath>().Player=World.Player;
            if(hp>0)actor.OwnHealth.RestoreHealth(hp);
            Bind(actor,new GoblinActorSnapshot{Id=id ?? "g"+(++serial),GroupId=group,CampId=campId,Home=home,Position=position,Ranged=ranged,HP=actor.OwnHealth.Current});return actor;
        }
        public GoblinController[] SpawnWarband(Vector2 center,bool safe=true)
        {
            if(!SimulationAuthority.Allows(this))return Array.Empty<GoblinController>();
            var size=UnityEngine.Random.Range(Definition.MinimumWarband,Definition.MaximumWarband+1);var result=new List<GoblinController>();var ranged=0;var group="gw"+(++groupSerial);
            if(ActiveCount+size>CampDefinition.HardCap)return result.ToArray();
            var positions=new Vector2[size];for(var i=0;i<size;i++){var angle=i*Mathf.PI*2/size;positions[i]=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*1.8f;if(safe && !World.GetComponent<WorldNavigation>().CanSpawn(positions[i],.7f,15,true))return result.ToArray();}
            for(var i=0;i<size;i++)
            {
                var p=positions[i];
                var isRanged=ranged<Definition.MaximumRanged && UnityEngine.Random.value<Definition.RangedChance;if(isRanged)ranged++;
                result.Add(SpawnActor(isRanged,p,group,center));
            }
            return result.ToArray();
        }
        public bool TrySpawn()
        {
            if(!SimulationAuthority.Allows(this) || Clock.Model.Phase!=DayPhase.Day || !Session.WorldIsRunning)return false;
            var roaming=0;foreach(var actor in actors.Values)if(string.IsNullOrEmpty(actor.State.CampId))roaming++;
            if(roaming>=Definition.Population || ActiveCount+Definition.MinimumWarband>CampDefinition.HardCap)return false;
            for(var attempt=0;attempt<64;attempt++)
            {
                Vector2 p;if(World.Map.Streamed){if(!World.Map.Continental.TryRoadPoint(Player.transform.position,UnityEngine.Random.value,out p))return false;}else{var road=World.Map.Roads[UnityEngine.Random.Range(0,World.Map.Roads.Count)];var segment=UnityEngine.Random.Range(1,road.Length);p=Vector2.Lerp(road[segment-1],road[segment],UnityEngine.Random.value);}
                if(Vector2.Distance(p,Player.transform.position)>45 || !World.GetComponent<WorldNavigation>().CanSpawn(p,2.8f,18,true))continue;
                return SpawnWarband(p).Length>0;
            }
            return false;
        }
        private bool ClearCamp(Vector2 p,bool outside)
        {
            if(!World.GetComponent<WorldNavigation>().CanSpawn(p,4,CampDefinition.PlayerDistance,outside))return false;
            foreach(var building in WorldPlaceable.Active)if(Vector2.Distance(p,building.transform.position)<CampDefinition.BaseDistance)return false;
            return true;
        }
        public GoblinCamp SpawnCamp(Vector2 position,string id=null,double refill=0,bool populate=true)
        {
            if(!SimulationAuthority.Allows(this))throw new InvalidOperationException("Host authority required.");
            var state=new CampSnapshot{Id=id ?? "camp"+(camps.Count+1),Position=position,RefillAt=refill};
            var camp=NetworkSpawnFactory.Spawn(CampDefinition.Prefab,position,Quaternion.identity,World.Environment);camp.State=state;camps.Add(camp);
            if(populate)Refill(camp,false);return camp;
        }
        public void GenerateCamps()
        {
            if(!SimulationAuthority.Allows(this))return;
            restored=true;foreach(var actor in actors.Values)if(actor!=null)PrefabPool.Release(actor.gameObject);actors.Clear();
            foreach(var camp in camps)if(camp!=null){PrefabPool.Release(camp.gameObject);}camps.Clear();serial=groupSerial=0;
            if(World.Map.Streamed){sleepingCamps.Clear();sleepingGoblins.Clear();initializedCamps.Clear();return;}
            var random=new System.Random(World.Seed^19273);
            for(var i=0;i<CampDefinition.InitialCamps;i++)for(var attempt=0;attempt<200;attempt++)
            {var size=World.Definition.Size;var p=new Vector2(((float)random.NextDouble()-.5f)*(size.x-16),((float)random.NextDouble()-.5f)*(size.y-16));if(ClearCamp(p,false)){SpawnCamp(p);break;}}
        }
        private int Refill(GoblinCamp camp,bool outside)
        {
            var count=0;foreach(var actor in actors.Values)if(actor.State.CampId==camp.State.Id)count++;
            var spawned=0;var ranged=0;foreach(var actor in actors.Values)if(actor.State.CampId==camp.State.Id && actor.State.Ranged)ranged++;
            for(var i=count;i<CampDefinition.Residents && ActiveCount<CampDefinition.HardCap;i++)
            {
                var success=false;
                for(var attempt=0;attempt<48;attempt++)
                {
                    var p=camp.State.Position+UnityEngine.Random.insideUnitCircle*CampDefinition.PatrolRadius;
                    if(!World.GetComponent<WorldNavigation>().CanSpawn(p,.7f,outside ? CampDefinition.PlayerDistance : 5,outside))continue;
                    var isRanged=ranged<1 && UnityEngine.Random.value<Definition.RangedChance;if(isRanged)ranged++;
                    SpawnActor(isRanged,p,camp.State.Id,camp.State.Position,camp.State.Id);spawned++;success=true;break;
                }
                if(!success)break;
            }
            camp.State.RefillAt=count+spawned>=CampDefinition.Residents ? 0 : Clock.Model.Elapsed+CampDefinition.RefillSeconds;return spawned;
        }
        public int TickCamps(){if(!SimulationAuthority.Allows(this))return 0;Prune();var count=0;foreach(var camp in camps)if(camp.State.RefillAt>0 && Clock.Model.Elapsed>=camp.State.RefillAt)count+=Refill(camp,true);return count;}
        public GoblinEcologySnapshot Capture()
        {
            Prune();var saved=new List<GoblinActorSnapshot>();foreach(var actor in actors.Values){var s=actor.State;saved.Add(new GoblinActorSnapshot{Id=s.Id,GroupId=s.GroupId,CampId=s.CampId,Ranged=s.Ranged,Position=actor.transform.position,Home=actor.Actor.Home,HP=actor.Actor.OwnHealth.Current});}
            var locations=new List<CampSnapshot>();foreach(var camp in camps)locations.Add(new CampSnapshot{Id=camp.State.Id,Position=camp.State.Position,RefillAt=camp.State.RefillAt});var state=new GoblinEcologySnapshot{Serial=serial,GroupSerial=groupSerial,Camps=locations.ToArray(),Actors=saved.ToArray()};return World.Map.Streamed ? CaptureStreamed(state) : state;
        }
        public void Restore(GoblinEcologySnapshot state)
        {
            if(!SimulationAuthority.Allows(this))return;
            restored=true;foreach(var actor in actors.Values)if(actor!=null)PrefabPool.Release(actor.gameObject);actors.Clear();foreach(var camp in camps)if(camp!=null){PrefabPool.Release(camp.gameObject);}camps.Clear();
            if(World.Map.Streamed){RestoreStreamed(state);return;}if(state==null){GenerateCamps();return;}serial=state.Serial;groupSerial=state.GroupSerial;
            foreach(var camp in state.Camps)SpawnCamp(camp.Position,camp.Id,camp.RefillAt,false);
            foreach(var actor in state.Actors)SpawnActor(actor.Ranged,actor.Position,actor.GroupId,actor.Home,actor.CampId,actor.Id,actor.HP);
        }
    }
}
