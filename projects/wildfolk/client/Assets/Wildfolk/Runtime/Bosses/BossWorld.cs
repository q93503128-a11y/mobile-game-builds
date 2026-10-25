using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    [DefaultExecutionOrder(-90)]
    public sealed class BossWorld:MonoBehaviour
    {
        public BossDefinition[] Definitions=Array.Empty<BossDefinition>();
        public WorldGenerator World;
        public WorldClock Clock;
        public WorldNavigation Navigation;
        public readonly List<PlayerInventory> Players=new List<PlayerInventory>(4);
        public readonly List<BossEncounter> Encounters=new List<BossEncounter>();
        private readonly List<BossSnapshot> records=new List<BossSnapshot>();
        private WorldMap clearingMap;private readonly List<WorldAnchor> clearingSites=new List<WorldAnchor>();
        public bool ExcludesResource(int rule,Vector2 position)
        {
            if(World.Map==null || !World.Map.Streamed || rule<0)return false;
            var family=World.Definition.Resources[rule].Family;if(family!=ResourceFamily.Timber && family!=ResourceFamily.Stone)return false;
            if(clearingMap!=World.Map){clearingMap=World.Map;clearingSites.Clear();foreach(var a in World.Map.Continental.Anchors)foreach(var d in Definitions)if(a.Kind==d.AnchorKind){clearingSites.Add(a);break;}}
            foreach(var a in clearingSites)foreach(var d in Definitions)if(a.Kind==d.AnchorKind && (a.Position-position).sqrMagnitude<d.ClearingRadius*d.ClearingRadius)return true;return false;
        }
        private NetworkCreatureWorld network;private float next;
        public bool Authority=>network==null || network.IsAuthority;
        private void Awake(){network=GetComponent<NetworkCreatureWorld>();}
        public BossDefinition Definition(string id){foreach(var d in Definitions)if(d.Id==id)return d;return null;}
        public BossSnapshot Record(string id)
        {foreach(var s in records)if(s.Boss==id)return s;var n=new BossSnapshot{Boss=id};records.Add(n);return n;}
        public bool Suppresses(Vector2 position){foreach(var e in Encounters)if(e!=null && e.State!=BossState.Ended && Vector2.Distance(position,e.Home)<=e.Definition.Territory)return true;return false;}
        private void Update()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.25f;Players.Clear();
            if(network==null){if(World.Player!=null && World.Player.GetComponent<Health>().IsAlive)Players.Add(World.Player);}
            else if(network.IsAuthority)foreach(var p in NetworkPlayer.Active)if(p.Health.IsAlive && p.IsSpawned && p.NetworkManager.ConnectedClients.ContainsKey(p.OwnerClientId))Players.Add(p.GetComponent<PlayerInventory>());
        }
        public void Materialize(PoiSite site)
        {
            if(site.GetComponentInChildren<RoyalTomb>()!=null)return;
            foreach(var definition in Definitions)if(definition.AnchorKind==site.Kind)
            {var tomb=Instantiate(definition.Tomb,site.transform);tomb.transform.localPosition=Vector3.zero;tomb.Initialize(this,site,definition);break;}
        }
        public bool Summon(RoyalTomb tomb,PlayerInventory player,long expectedNight)
        {
            if(!Authority || tomb==null || player==null || !player.GetComponent<Health>().IsAlive || !player.GetComponent<PlayerInputCommands>().Session.IsRunning || player.GetComponent<PlayerInputCommands>().IsScouting || Vector2.Distance(tomb.transform.position,player.transform.position)>player.Settings.StationInteractionRange)return false;
            var d=tomb.Definition;var r=Record(d.Id);
            if(!tomb.Unlocked)return Feedback(player,"boss.tomb.sealed");
            if(Clock.Model.Phase!=DayPhase.Night)return Feedback(player,"boss.tomb.cold");
            if(expectedNight!=Clock.Model.Day || !d.CanSummon(r.LastNight,Clock.Model.Day,Clock.Model.Phase,r.Active))return Feedback(player,"boss.tomb.silent");
            // Do not overwrite a resolved night receipt while its local profile credit still needs recovery.
            if(network==null){if(World.Player.GetComponent<PlayerClass>()?.RecoverBossXP()==false)return false;}
            else foreach(var actor in NetworkPlayer.Active)if(actor.GetComponent<PlayerClass>()?.RecoverBossXP()==false)return false;
            var prior=r.Copy();BossEncounter spawned=null;
            var ok=player.Model.TryConsumeCosts(new[]{new ItemAmount{Item=d.Offering,Count=d.OfferingCount}},()=>
            {
                r.LastNight=Clock.Model.Day;r.Tomb=tomb.Site.Id;r.Home=(Vector2)tomb.transform.position+d.SpawnOffset;r.Position=r.Home;r.HP=d.Vitality.MaximumHealth;r.Active=true;r.PendingReward=false;r.RolledLoot=Array.Empty<SavedStack>();r.Eligible=Array.Empty<string>();
                try{spawned=Spawn(d,r,false);if(Checkpoint()){GetComponent<NetworkBossWorld>()?.Publish();return true;}}
                catch(Exception e){Debug.LogError("Boss summon could not commit: "+e.GetType().Name);}
                records[records.IndexOf(r)]=prior;if(spawned!=null)Release(spawned);return false;
            });
            if(!ok)Feedback(player,"boss.tomb.offering");return ok;
        }
        private static bool Feedback(PlayerInventory p,string key){var net=p.GetComponent<NetworkPlayer>();if(net==null || net.IsOwner)PlayerFeedback.ShowKey(key);return false;}
        private BossEncounter Spawn(BossDefinition definition,BossSnapshot state,bool restored)
        {var e=NetworkSpawnFactory.Spawn(definition.Prefab,state.Position,Quaternion.identity);Encounters.Add(e);e.Initialize(this,state,restored);return e;}
        public void Release(BossEncounter e){Encounters.Remove(e);GetComponent<NetworkBossWorld>()?.Publish();PrefabPool.Release(e.gameObject);}
        public BossSnapshot[] Capture(){foreach(var e in Encounters)if(e!=null)e.Capture();return records.ConvertAll(s=>s.Copy()).ToArray();}
        public void ResetWorld(){for(var i=Encounters.Count-1;i>=0;i--)if(Encounters[i]!=null){Encounters[i].gameObject.SetActive(false);PrefabPool.Release(Encounters[i].gameObject);}Encounters.Clear();records.Clear();}
        public void Restore(BossSnapshot[] states)
        {ResetWorld();foreach(var s in states){var r=s.Copy();records.Add(r);if(r.Active)Spawn(Definition(r.Boss),r,true);}GetComponent<NetworkBossWorld>()?.Publish();}
        public bool Checkpoint()
        {if(!Authority)return false;return network==null?World.Session.GetComponent<WorldSaveService>().Save():GetComponent<NetworkWorldPersistence>().Save();}
        private static string Id(PlayerInventory p)=>p.GetComponent<NetworkPlayer>()?.ProfileId??ClassProfiles.LocalId;
        private bool Present(PlayerInventory p)
        {if(p==null || !p.GetComponent<Health>().IsAlive)return false;if(network==null)return true;var n=p.GetComponent<NetworkPlayer>();return n!=null && n.IsSpawned && n.NetworkManager.ConnectedClients.ContainsKey(n.OwnerClientId);}
        public void RollReward(BossEncounter e)
        {
            if(!Authority)return;var r=e.Record;if(r.PendingReward)return;r.PendingReward=true;r.HP=0;
            var loot=e.Definition.Rewards.Roll();r.RolledLoot=loot.ConvertAll(l=>new SavedStack{Item=l.Item.Id,Count=l.Count}).ToArray();
            var eligible=new List<string>();foreach(var p in e.Participants)if(Present(p) && Vector2.Distance(p.transform.position,e.transform.position)<=NetworkCreatureWorld.RewardRadius)eligible.Add(Id(p));r.Eligible=eligible.ToArray();
        }
        public bool ResolveReward(BossEncounter e)
        {
            var r=e.Record;if(!Authority || !r.PendingReward)return false;
            if(network!=null && NetworkPlayer.Active.Count==0)return false;
            var created=new List<WorldPickup>();var recipients=new List<PlayerProgression>();var before=new List<GrowthSnapshot>();var xp=new List<long>();var gold=new List<long>();
            foreach(var p in Players)if(Present(p) && Array.IndexOf(r.Eligible,Id(p))>=0 && Vector2.Distance(p.transform.position,e.transform.position)<=NetworkCreatureWorld.RewardRadius)
            {var g=p.GetComponent<PlayerProgression>();recipients.Add(g);before.Add(g.Capture());xp.Add(g.Experience);gold.Add(g.Gold);}
            var receipt=r.Boss+":"+r.Tomb+":"+r.LastNight;
            var durable=new List<ClassRewardPersistence.ClassXpCommit>();var roles=new List<PlayerClass>();
            var repository=network==null?World.Session.GetComponent<WorldSaveService>().Repository:NetworkSession.Instance.WorldRepository;
            try
            {
                foreach(var p in recipients)
                {
                    var role=p.GetComponent<PlayerClass>();if(role?.PersistentProgression!=true)continue;
                    var intent=new ClassXpIntent{Receipt=receipt,ClassId=role.Definition.Id,WorldPath=repository.Path,Boss=r.Boss,Tomb=r.Tomb,Night=r.LastNight,Seed=World.Seed,Generation=World.Map.GenerationVersion,Experience=e.Definition.Rewards.Experience};
                    var commit=role.BeginBossXP(intent);if(commit==null)throw new InvalidOperationException("Class reward intent unavailable");durable.Add(commit);roles.Add(role);
                }
            }
            catch(Exception error)when(error is System.IO.IOException || error is UnauthorizedAccessException || error is ArgumentException || error is InvalidOperationException)
            {foreach(var commit in durable){try{commit.Rollback();}finally{commit.Dispose();}}return false;}
            // World reward state, common XP/Gold and physical stacks commit atomically.
            var rolled=r.RolledLoot;var eligibleIds=r.Eligible;r.Active=false;r.PendingReward=false;r.RolledLoot=Array.Empty<SavedStack>();r.Eligible=Array.Empty<string>();
            try
            {
                for(var i=0;i<rolled.Length;i++)
                {var s=rolled[i];var angle=i*2.399963f;var p=(Vector2)e.transform.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.65f;var drop=PrefabPool.Spawn(World.Pickup,p,Quaternion.identity);drop.Initialize(World.Definition.ProgressionCatalog.Item(s.Item),s.Count,World.Player,World.Settings);created.Add(drop);}
                foreach(var p in recipients)p.Reward(e.Definition.Rewards.Experience,e.Definition.Rewards.Gold);
                network?.FlushPending();if(!Checkpoint())throw new InvalidOperationException("Reward save failed");
            }
            catch(Exception error)
            {
                foreach(var drop in created)if(drop!=null)PrefabPool.Release(drop.gameObject);
                for(var i=0;i<recipients.Count;i++)recipients[i].Restore(before[i],xp[i],gold[i]);r.Active=true;r.PendingReward=true;r.RolledLoot=rolled;r.Eligible=eligibleIds;
                foreach(var commit in durable){try{commit.Rollback();}finally{commit.Dispose();}}
                Debug.LogError("Boss reward commit deferred: "+error.GetType().Name);return false;
            }
            for(var i=0;i<durable.Count;i++){var commit=durable[i];try{if(commit.Complete(out var total))roles[i].AcceptBossXP(total,receipt);}finally{commit.Dispose();}}
            foreach(var p in recipients){var role=p.GetComponent<PlayerClass>();if(role!=null && !role.PersistentProgression)role.GrantXP(e.Definition.Rewards.Experience,receipt);}
            r.RolledLoot=Array.Empty<SavedStack>();r.Eligible=Array.Empty<string>();GetComponent<NetworkBossWorld>()?.Publish();return true;
        }
    }
}
