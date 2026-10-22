using System;
using UnityEngine;
namespace Wildfolk
{
    public enum PlayerLife : byte {Alive,Downed,Dead,Reviving,Respawning}
    [Serializable]public sealed class DeathSnapshot
    {
        public Vector2 Home;public string Run;
        public long Death;
        public bool Dead,AdUsed;
    }
    // Common lifecycle; SP offers and cooperative holds are separate policies below.
    public sealed class PlayerDeath : MonoBehaviour
    {
        public ReviveDefinition Definition;
        public PlayerLife State {get;private set;}
        public uint Revision {get;private set;}
        public Vector2 Home {get;private set;}
        public bool HomeInitialized {get;private set;}
        public string Run {get;private set;}
        public long DeathNumber {get;private set;}
        public bool AdUsed {get;private set;}
        public double DownUntil {get;private set;}
        public double HoldUntil {get;private set;}
        public double HoldStarted {get;private set;}
        public double ProtectedUntil {get;private set;}
        public PlayerDeath Rescuer {get;private set;}
        public PlayerDeath RescueTarget {get;private set;}
        public bool PaymentPending {get;private set;}
        public string MessageKey {get;private set;}
        public bool Durable {get;private set;}=true;
        public event Action Changed;
        public static event Action PartyWiped;
        public Func<bool> Persistence {get;set;}
        private Health health;private NetworkPlayer network;private Vector2 deathPosition,holdPosition;
        private double leaseUntil,nextPartyCheck;
        private readonly Collider2D[] overlaps=new Collider2D[64];
        private readonly RaycastHit2D[] walls=new RaycastHit2D[32];
        private GameplaySession Session=>GetComponent<PlayerInputCommands>().Session;
        private bool Multi=>Session!=null && Session.Mode==SessionMode.Multiplayer;
        public bool Authority=>network==null || network.IsSpawned && network.IsServer;
        public double Now=>network!=null && network.IsSpawned ? network.NetworkManager.ServerTime.Time : Time.time;
        public bool CanOffer=>!Multi && State==PlayerLife.Dead && !PaymentPending && Durable;
        public bool CanAd=>CanOffer && !AdUsed && ReviveServices.Rewarded.Available;
        public bool CanGem=>CanOffer && Definition.ReviveGemCost>0 && ReviveServices.Wallet.Available && ReviveServices.Wallet.Balance>=Definition.ReviveGemCost;
        public bool CanAct=>State==PlayerLife.Alive && RescueTarget==null && !PaymentPending;
        private void Awake(){health=GetComponent<Health>();network=GetComponent<NetworkPlayer>();Run=Guid.NewGuid().ToString("N");}
        private void OnEnable(){GetComponent<Health>().Damaged+=InterruptRescue;}
        private void OnDisable(){GetComponent<Health>().Damaged-=InterruptRescue;if(Authority){InterruptRescue();Rescuer?.InterruptRescue();}}
        public void InitializeHome(Vector2 position){if(HomeInitialized)return;Home=position;HomeInitialized=true;}
        // Future movable Home writes through this authority/persistence boundary; no bed gameplay here.
        public bool AssignHome(Vector2 position)
        {if(!Authority || State!=PlayerLife.Alive || !Safe(position))return false;var old=Home;Home=position;HomeInitialized=true;if(!Persist()){Home=old;return false;}Signal();return true;}
        public void Lethal()
        {
            if(!Authority || State!=PlayerLife.Alive)return;
            InterruptRescue();Rescuer?.InterruptRescue();deathPosition=transform.position;
            if(DeathNumber==long.MaxValue){Run=Guid.NewGuid().ToString("N");DeathNumber=0;}DeathNumber++;
            ProtectedUntil=0;ResetActions();
            State=Multi ? PlayerLife.Downed : PlayerLife.Dead;
            DownUntil=Multi ? Now+Definition.DownedSeconds*Mathf.Clamp(Modifiers(this)?.DownedDurationMultiplier??1,.1f,10) : 0;
            Revision++;ApplySession();Signal();Durable=Persist();Signal();
        }
        private void ResetActions()
        {
            var commands=GetComponent<PlayerInputCommands>();commands.ApplyMove(Vector2.zero);
            GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
            GetComponent<PlayerLocomotion>()?.CancelDash();GetComponent<PlayerCombat>()?.CancelAttack();GetComponent<PlayerRanged>()?.Cancel();
            GetComponent<PlayerFishing>()?.ResetTransient();GetComponent<PlayerExplorationUtility>()?.ResetTransient();GetComponent<PlayerTelescope>()?.Exit();
            GetComponent<PlayerSurvival>()?.ResetTransient();
        }
        private static IReviveModifiers Modifiers(PlayerDeath actor)=>actor.GetComponent<IReviveModifiers>();
        private void Update()
        {
            if(!Authority || !Multi)return;
            if(State==PlayerLife.Downed || State==PlayerLife.Reviving)
            {
                if(Now>=DownUntil){BecomeDead();return;}
                if(Rescuer!=null)
                {
                    if(Now>leaseUntil || !ValidRescuer(Rescuer) || ((Vector2)Rescuer.transform.position-holdPosition).sqrMagnitude>.04f){Rescuer.InterruptRescue();return;}
                    if(Now>=HoldUntil)CommitTeammate();
                }
                if(Now>=nextPartyCheck){nextPartyCheck=Now+.2;CheckPartyWipe();}
            }
        }
        public void BecomeDead()
        {
            if(!Authority || State!=PlayerLife.Downed && State!=PlayerLife.Reviving)return;
            Rescuer?.InterruptRescue();State=PlayerLife.Dead;DownUntil=HoldUntil=HoldStarted=0;Revision++;ApplySession();Signal();Durable=Persist();Signal();
        }
        private static void CheckPartyWipe()
        {
            var any=false;foreach(var p in NetworkPlayer.Active){if(!p.IsSpawned)continue;any=true;if(p.Health.IsAlive)return;}
            if(!any)return;
            var changed=false;foreach(var p in NetworkPlayer.Active){var d=p.GetComponent<PlayerDeath>();if(d!=null && (d.State==PlayerLife.Downed || d.State==PlayerLife.Reviving)){d.BecomeDead();changed=true;}}
            if(changed)PartyWiped?.Invoke();
        }
        public PlayerDeath NearbyDowned()
        {
            if(!Multi || State!=PlayerLife.Alive || !health.IsAlive)return null;
            PlayerDeath nearest=null;var distance=Definition.Range*Definition.Range;
            foreach(var p in NetworkPlayer.Active)
            {
                var d=p.GetComponent<PlayerDeath>();if(d==null || d==this || !p.IsSpawned || d.State!=PlayerLife.Downed && d.State!=PlayerLife.Reviving)continue;
                var delta=((Vector2)d.transform.position-(Vector2)transform.position).sqrMagnitude;
                if(delta>distance || !Clear(d))continue;distance=delta;nearest=d;
            }
            return nearest;
        }
        private bool Clear(PlayerDeath other)
        {
            var delta=(Vector2)(other.transform.position-transform.position);var count=Physics2D.Raycast(transform.position,delta.normalized,new ContactFilter2D{useTriggers=false},walls,delta.magnitude);
            if(count==walls.Length)return false;
            for(var i=0;i<count;i++){var h=walls[i].collider.GetComponentInParent<Health>();if(h!=health && h!=other.health)return false;}return true;
        }
        private bool ValidRescuer(PlayerDeath other)=>other!=null && other!=this && other.Authority && other.Multi && other.State==PlayerLife.Alive && other.health.IsAlive && (other.Session.IsRunning || other.Session.State==SessionState.Rescuing) && !other.GetComponent<PlayerInputCommands>().IsScouting && Vector2.Distance(transform.position,other.transform.position)<=Definition.Range && other.Clear(this);
        public bool ReceiveHold(PlayerDeath by,uint revision,bool held)
        {
            if(!Authority || !Multi)return false;
            if(!held){by?.InterruptRescue();return true;}
            if(revision!=Revision || State!=PlayerLife.Downed && State!=PlayerLife.Reviving || Now>=DownUntil || !ValidRescuer(by))return false;
            if(Rescuer!=null && Rescuer!=by)return false;
            if(Rescuer==null)
            {
                by.InterruptRescue();Rescuer=by;by.RescueTarget=this;holdPosition=by.transform.position;
                HoldStarted=Now;HoldUntil=Now+Definition.HoldSeconds/Mathf.Clamp(Modifiers(by)?.ReviveSpeedMultiplier??1,.1f,10);
                State=PlayerLife.Reviving;by.ResetLivingActions();by.ApplySession();by.Signal();Signal();
            }
            leaseUntil=Now+.5;return true;
        }
        private void ResetLivingActions(){GetComponent<PlayerInputCommands>().ApplyMove(Vector2.zero);GetComponent<PlayerLocomotion>().CancelDash();GetComponent<PlayerCombat>().CancelAttack();GetComponent<PlayerRanged>()?.Cancel();GetComponent<PlayerFishing>()?.ResetTransient();GetComponent<PlayerExplorationUtility>()?.ResetTransient();GetComponent<PlayerTelescope>()?.Exit();}
        public void InterruptRescue()
        {
            if(!Authority)return;CancelHold();
        }
        public void Detach(){CancelHold();if(Rescuer!=null)Rescuer.CancelHold();}
        private void CancelHold()
        {var target=RescueTarget;if(target==null)return;RescueTarget=null;
            if(target!=null && target.Rescuer==this){target.Rescuer=null;target.HoldUntil=target.HoldStarted=0;if(target.State==PlayerLife.Reviving)target.State=PlayerLife.Downed;target.Revision++;target.Signal();}
            ApplySession();if(target!=null)Signal();
        }
        private void CommitTeammate()
        {
            var by=Rescuer;if(!ValidRescuer(by) || Now>=DownUntil)return;
            if(!TryPosition(deathPosition,3,out var position))return;
            by.InterruptRescue();CommitRespawn(position,Mathf.Clamp01(Definition.RevivedHealth*(Modifiers(by)?.RevivedHealthMultiplier??1)),false,false);DownUntil=0;Signal();
        }
        public bool HomeRespawn()
        {
            if(!Authority || State!=PlayerLife.Dead)return false;
            // Cancel an outstanding ad before it can commit against the old death.
            PaymentPending=false;Revision++;
            if(!HomeInitialized || !TryPosition(Home,8,out var point)){MessageKey="death.position_failed";Signal();return false;}
            return CommitRespawn(point,1,true,false);
        }
        public bool GemRevive()
        {
            if(!Authority || !CanGem)return false;if(!TryPosition(deathPosition,3,out var point)){MessageKey="death.position_failed";Signal();return false;}
            var revision=Revision;var id=Transaction("gem");PaymentPending=true;Signal();bool result=false;
            try{result=ReviveServices.Wallet.TryCommitRevive(id,Definition.ReviveGemCost,()=>State==PlayerLife.Dead && Revision==revision && CommitRespawn(point,Definition.RevivedHealth,false,false));}
            finally{PaymentPending=false;Signal();}
            return result;
        }
        public bool RewardedRevive()
        {
            if(!Authority || !CanAd)return false;var revision=Revision;var run=Run;var number=DeathNumber;
            PaymentPending=true;MessageKey=null;Signal();
            try{ReviveServices.Rewarded.Show(Transaction("ad"),success=>
            {
                if(this==null || !PaymentPending || State!=PlayerLife.Dead || revision!=Revision || run!=Run || number!=DeathNumber)return;
                PaymentPending=false;
                if(success && !AdUsed && TryPosition(deathPosition,3,out var point))CommitRespawn(point,Definition.RevivedHealth,false,true);
                else MessageKey=success ? "death.position_failed" : "death.ad_failed";
                Signal();
            });}catch(Exception){PaymentPending=false;MessageKey="death.ad_failed";Signal();return false;}
            return true;
        }
        private string Transaction(string kind)=>Run+":"+DeathNumber+":"+kind;
        private bool CommitRespawn(Vector2 point,float ratio,bool home,bool ad)
        {
            var old=Capture();var prior=(Vector2)transform.position;
            State=PlayerLife.Respawning;ApplySession();
            if(home){Run=Guid.NewGuid().ToString("N");DeathNumber=0;AdUsed=false;}else if(ad)AdUsed=true;
            RestoreBody(point,ratio);State=PlayerLife.Alive;
            if(!Persist())
            {
                Run=old.Run;DeathNumber=old.Death;AdUsed=old.AdUsed;Move(prior);health.RestoreHealth(0);ProtectedUntil=0;State=PlayerLife.Dead;Durable=false;MessageKey="death.save_failed";ApplySession();Signal();return false;
            }
            Durable=true;PaymentPending=false;MessageKey=null;Revision++;ApplySession();Signal();return true;
        }
        private void RestoreBody(Vector2 point,float ratio)
        {Move(point);health.RestoreHealth(Mathf.Max(1,Mathf.RoundToInt(health.Maximum*ratio)));health.MarkCombat();ProtectedUntil=Now+Definition.ProtectionSeconds;}
        private void Move(Vector2 point){GetComponent<PlayerInputCommands>().ApplyMove(Vector2.zero);GetComponent<PlayerLocomotion>().CancelDash();GetComponent<Rigidbody2D>().position=point;transform.position=point;Physics2D.SyncTransforms();}
        public bool BlocksDamage=>State==PlayerLife.Alive && Now<ProtectedUntil;
        public void OffensiveAction(){if(!Authority)return;InterruptRescue();if(ProtectedUntil>Now){ProtectedUntil=0;Signal();}}
        private bool Safe(Vector2 point)
        {
            var nav=GetComponent<PlayerMotor>().Terrain;
            if(nav==null || !nav.Traversable(point,.38f) || nav.Surface(point)==TerrainSurface.Deep)return false;
            var n=Physics2D.OverlapCircle(point,.38f,new ContactFilter2D{useTriggers=false},overlaps);if(n==overlaps.Length)return false;
            for(var i=0;i<n;i++){var h=overlaps[i].GetComponentInParent<Health>();if(h!=health && overlaps[i].GetComponentInParent<NetworkPlayer>()==null)return false;}return true;
        }
        private bool TryPosition(Vector2 origin,float radius,out Vector2 result)
        {
            GetComponent<PlayerMotor>().Terrain?.World?.Chunks?.PreparePlayerReturn(origin,radius);
            result=origin;if(Safe(origin))return true;
            // Nearest valid local ring only: never silently teleport an in-place revive Home.
            for(var r=.25f;r<=radius;r+=.25f)for(var i=0;i<32;i++){var p=origin+new Vector2(Mathf.Cos(i*Mathf.PI/16),Mathf.Sin(i*Mathf.PI/16))*r;if(Safe(p)){result=p;return true;}}return false;
        }
        private bool Persist()
        {
            if(Persistence!=null)return Persistence();
            if(Multi)return NetworkWorldPersistence.Active!=null && NetworkWorldPersistence.Active.Ready && NetworkWorldPersistence.Active.Save();
            var save=FindFirstObjectByType<WorldSaveService>();return save!=null && save.Player.gameObject==gameObject && save.Save();
        }
        public DeathSnapshot Capture()=>new DeathSnapshot{Home=Home,Run=Run,Death=DeathNumber,Dead=State!=PlayerLife.Alive,AdUsed=AdUsed};
        public void Restore(DeathSnapshot state)
        {
            Rescuer=null;RescueTarget=null;PaymentPending=false;ProtectedUntil=DownUntil=HoldUntil=HoldStarted=0;
            Home=state.Home;HomeInitialized=true;Run=state.Run;DeathNumber=state.Death;AdUsed=state.AdUsed;State=state.Dead ? PlayerLife.Dead : PlayerLife.Alive;deathPosition=transform.position;Durable=true;Revision++;ApplySession();Signal();
        }
        public void ApplyReplica(PlayerStatusState s)
        {
            State=(PlayerLife)s.Life;Revision=s.LifeRevision;DownUntil=s.DownUntil;HoldStarted=s.HoldStarted;HoldUntil=s.HoldUntil;ProtectedUntil=s.ProtectedUntil;
            Session.ApplyLife(State,s.Helping);Changed?.Invoke();
        }
        private void ApplySession(){if(Session!=null)Session.ApplyLife(State,RescueTarget!=null);}
        private void Signal()=>Changed?.Invoke();
    }
}
