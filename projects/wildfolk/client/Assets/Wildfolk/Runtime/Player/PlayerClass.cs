using System;
using System.Linq;
using UnityEngine;
namespace Wildfolk
{
    public enum DamageKind { Unspecified,Melee,Spear,Ranged,Environment }
    // All class identity checks live here. Combat, survival, utility and building code consume capabilities.
    public sealed class PlayerClass:MonoBehaviour,IReviveModifiers,ICooperativeReviveModifiers
    {
        public ClassDefinition Definition {get;private set;}
        public string Profile {get;private set;}
        public bool Locked {get;private set;}
        public bool Guarding {get;private set;}
        public Health Quarry {get;private set;}
        public double QuarryUntil {get;private set;}
        public float SurvivalCooldown=>Mathf.Max(0,(float)(survivalReady-Now));
        public double ProtectionUntil {get;private set;}
        public double ActiveReady {get;private set;}
        public double HeavyUntil {get;private set;}
        public int SurvivingHealth{get;private set;}
        public bool LastHeavy {get;private set;}
        public event Action Changed;
        private float[] modifiers=new float[Enum.GetValues(typeof(ClassEffect)).Length];
        private double survivalReady,guardLease,counterUntil,counterReady,stationarySince,homeUntil,lastStep,chaseSeconds;
        private uint quarryGeneration;
        private bool counterPrepared,chasePrepared;private Vector2 previous;
        private Health health;private PlayerLocomotion locomotion;private PlayerInputCommands commands;private PlayerInventory bag;
        public bool Authority=>SimulationAuthority.Allows(this);
        // Admission lock and snapshot restore run before NetworkObject.Spawn. Use the session clock there too.
        public double Now {get{var manager=Unity.Netcode.NetworkManager.Singleton;return GetComponent<NetworkPlayer>()!=null && manager!=null && manager.IsListening?manager.ServerTime.Time:Time.timeAsDouble;}}
        public bool Has(ClassTrait trait)=>Definition!=null && Definition.Trait==trait;
        public float Value(ClassEffect effect)=>modifiers[(int)effect];
        public bool Low=>health!=null && health.Ratio<=.3f;
        public float MedicineReceived=>1+(GetComponent<PlayerProgression>()?.Bonus(SkillEffect.MedicineReceived)??0)+(Low?Value(ClassEffect.LowMedicine):0);
        public float HealingOutgoing(Health target)=>1+Value(ClassEffect.HealingOut)+(target.Ratio<=.3f?Value(ClassEffect.EmergencyHeal):0);
        public float MedicineCooldownMultiplier=>1-Value(ClassEffect.MedicineCooldown);
        public float HotDurationMultiplier=>1-Value(ClassEffect.HotSpeed);
        public float NutritionMultiplier=>1+Value(ClassEffect.Nutrition);
        public float MealDurationMultiplier=>1+Value(ClassEffect.MealDuration);
        public float NeedsMultiplier=>1-(GetComponent<PlayerProgression>()?.Bonus(SkillEffect.Metabolism)??0)-(Now<homeUntil?Value(ClassEffect.HomeDrain):0);
        public float RecoveryMultiplier=>1+(Low?Value(ClassEffect.LowRecovery):0);
        public float PenaltyRelief=>Low?Value(ClassEffect.LowPenalty):0;
        public float OpticMultiplier=>Has(ClassTrait.Exploration)?1.15f+Value(ClassEffect.OpticRange):1;
        public float SurveyMultiplier=>Has(ClassTrait.Exploration)?1.15f+Value(ClassEffect.SurveyRange):1;
        public float SignalMultiplier=>Has(ClassTrait.Exploration)?1.15f+Value(ClassEffect.SignalRange):1;
        public bool CanMark=>Has(ClassTrait.Exploration);
        public bool CanHealAlly=>Has(ClassTrait.Medicine);
        public bool FreeRepair=>Has(ClassTrait.Engineering);
        public bool HasActive=>Definition?.Active==true;
        public float DownedDurationMultiplier=>1;
        public float ReviveSpeedMultiplier=>1+Value(ClassEffect.ReviveSpeed);
        public float RevivedHealthMultiplier=>1+Value(ClassEffect.ReviveHealth);
        public float ReviveProtectionBonus=>Value(ClassEffect.ReviveProtection);
        public float DownedDrainMultiplier=>1-Value(ClassEffect.Stabilize);
        public float GoldenTimeBonus=>Value(ClassEffect.GoldenTime);
        private void Awake(){health=GetComponent<Health>();locomotion=GetComponent<PlayerLocomotion>();commands=GetComponent<PlayerInputCommands>();bag=GetComponent<PlayerInventory>();previous=transform.position;}
        private void Start(){if(!Locked && GetComponent<NetworkPlayer>()==null)Lock(ClassProfiles.LocalId,ClassProfiles.Local);}
        private ClassProgressSession progression;
        public long ClassXP=>progression?.Account.Find(Definition.Id)?.XP??0;
        public bool Lock(string profile,AccountProgression account)=>Lock(profile,new ClassProgressSession(account,true));
        public bool Lock(string profile,ClassProgressSession admitted)
        {
            var account=admitted.Account;
            if(Locked)return false;account.Validate();var progress=account.Find(account.Selected);if(progress?.Owned!=true)return false;
            progression=admitted;Profile=profile;Definition=ClassCatalog.Current.Find(progress.Id);Array.Clear(modifiers,0,modifiers.Length);
            foreach(var id in progress.Nodes){var node=Array.Find(Definition.Nodes,n=>n.Id==id);modifiers[(int)node.Effect]+=node.Amount;}
            Locked=true;stationarySince=Now;lastStep=Now;HomeDeparture();Changed?.Invoke();return true;
        }
        public string[] Allocation()=>Definition==null?Array.Empty<string>():Definition.Nodes.Where(n=>Value(n.Effect)>0).Select(n=>n.Id).ToArray();
        // Authoritative replica initialization is read-only to gameplay clients.
        public void Replica(string id,ulong bits)
        {
            Definition=ClassCatalog.Current.Find(id);if(Definition==null)return;Array.Clear(modifiers,0,modifiers.Length);
            for(var i=0;i<Definition.Nodes.Length;i++)if((bits&(1UL<<i))!=0)modifiers[(int)Definition.Nodes[i].Effect]+=Definition.Nodes[i].Amount;
            Locked=true;Changed?.Invoke();
        }
        public ulong AllocationBits(){ulong bits=0;if(Definition!=null)for(var i=0;i<Definition.Nodes.Length;i++)if(Value(Definition.Nodes[i].Effect)>0)bits|=1UL<<i;return bits;}
        public bool GrantXP(int xp,string receipt)
        {
            if(!Authority || !Locked || xp<0)return false;
            var success=progression.Grant(Profile,Definition.Id,xp,receipt);
            if(success)GetComponent<NetworkClassActions>()?.SendProfile();return success;
        }
        public void HomeDeparture(){homeUntil=Now+60;}
        public void RestoreCooldown(float remaining){survivalReady=Now+remaining;ProtectionUntil=0;SetGuard(false);Quarry=null;QuarryUntil=0;}
        public void OffensiveAction(){if(ProtectionUntil>Now){ProtectionUntil=0;Changed?.Invoke();}}
        public bool BlocksDamage=>Now<ProtectionUntil;
        public bool PreventLethal(ref int damage)
        {
            if(!Has(ClassTrait.Survival) || Now<survivalReady || damage<health.Current)return false;
            SurvivingHealth=Mathf.Max(1,Mathf.RoundToInt(health.Maximum*Value(ClassEffect.SurvivalHealth)));damage=Mathf.Max(0,health.Current-SurvivingHealth);
            survivalReady=Now+360*(1-Value(ClassEffect.SurvivalCooldown));ProtectionUntil=Now+1.5;
            if(Value(ClassEffect.SurvivalStamina)>0)locomotion.Stamina.Restore(Mathf.Min(locomotion.Stamina.Maximum,locomotion.Stamina.Current+Value(ClassEffect.SurvivalStamina)),locomotion.Stamina.RecoveryWait);
            Changed?.Invoke();return true;
        }
        public bool SetGuard(bool held)
        {
            if(!Authority)return false;
            if(held && (!Has(ClassTrait.Guard) || !health.IsAlive || commands==null || !commands.Session.IsRunning || commands.IsScouting || GetComponent<PlayerDeath>()?.CanAct==false || locomotion.Stamina.Current<=0))return false;
            guardLease=Now+.5;if(Guarding==held)return true;Guarding=held;if(!held)GetComponent<PlayerAvatar>()?.Facing.ResetPose();
            if(!held && Value(ClassEffect.GuardRecovery)>0)locomotion.Stamina.ShortenRecovery(Value(ClassEffect.GuardRecovery));Changed?.Invoke();return true;
        }
        public float GuardMove(Vector2 direction)
        {if(!Guarding)return 1;var forward=direction.sqrMagnitude>.01f && Vector2.Dot(direction.normalized,GetComponent<PlayerAvatar>().Facing.Direction)>.65f;return Mathf.Min(.9f,.65f+Value(ClassEffect.GuardMove)+(forward?Value(ClassEffect.GuardForward):0));}
        public float AttackInterval=>Guarding?1.6f-Value(ClassEffect.GuardInterval):1;
        public bool Frontal(Vector2 source)
        {var delta=source-(Vector2)transform.position;return delta.sqrMagnitude>.00001f && Vector2.Dot(GetComponent<PlayerAvatar>().Facing.Direction,delta.normalized)>=Mathf.Cos((120+Value(ClassEffect.GuardAngle))*.5f*Mathf.Deg2Rad);}
        public int Incoming(int damage,Health source,out bool guarded)
        {
            guarded=Guarding && source!=null && Frontal(source.transform.position);LastHeavy=false;
            var result=Mathf.Max(1,Mathf.CeilToInt(damage*(guarded?.5f:1)*(Now<HeavyUntil?1-Value(ClassEffect.GuardHeavy):1)));
            return result;
        }
        public void Received(int applied,bool guarded)
        {
            if(!guarded || applied<=0)return;counterUntil=Now+2;LastHeavy=(long)applied*5>=health.Maximum;
            if(LastHeavy && Value(ClassEffect.GuardHeavy)>0)HeavyUntil=Now+1.5;Changed?.Invoke();
        }
        public bool AimsAtQuarry(Vector2 direction)=>Quarry!=null && Quarry.IsAlive && Now<QuarryUntil && Vector2.Dot(direction.normalized,((Vector2)Quarry.transform.position-(Vector2)transform.position).normalized)>.8f;
        public float SprintMultiplier(Vector2 direction)=>AimsAtQuarry(direction)?1-.08f-Value(ClassEffect.QuarrySprint):1;
        public float SpearReach=>1+Value(ClassEffect.SpearReach);
        public float ShotRange(Vector2 direction)=>AimsAtQuarry(direction)?1+Value(ClassEffect.BowRange):1;
        public bool BeginMelee(){counterPrepared=Now<counterUntil && Value(ClassEffect.CounterDamage)>0;counterUntil=0;chasePrepared=chaseSeconds>=.8;chaseSeconds=0;return counterPrepared;}
        public bool SteadyShot=>Now-stationarySince>=.6;
        public int Outgoing(Health target,int amount,DamageKind kind,bool prepared)
        {
            float bonus=0;if(target==Quarry && Now<QuarryUntil)
            {
                bonus+=.08f;
                if(kind==DamageKind.Ranged){if(prepared)bonus+=Value(ClassEffect.SteadyShot);if(target.Ratio<=.3f)bonus+=Value(ClassEffect.FinishShot);}
                if(kind==DamageKind.Spear){if(chasePrepared)bonus+=Value(ClassEffect.ChaseSpear);if(target.Maximum>=120)bonus+=Value(ClassEffect.LargeSpear);}
            }
            if((kind==DamageKind.Melee || kind==DamageKind.Spear) && counterPrepared)bonus+=Value(ClassEffect.CounterDamage);
            return Mathf.Max(1,Mathf.RoundToInt(amount*(1+bonus)));
        }
        public void Hit(Health target,int applied,DamageKind kind)
        {
            if(target==null || applied<=0 || target.GetComponent<CreatureMember>()==null)return;
            if(Has(ClassTrait.Quarry)){Quarry=target;quarryGeneration=target.GetComponent<CreatureMember>().SpawnGeneration;QuarryUntil=Now+20+Value(ClassEffect.QuarryDuration);}
            if((kind==DamageKind.Melee || kind==DamageKind.Spear) && counterPrepared)
            {counterPrepared=false;if(Now>=counterReady && Value(ClassEffect.CounterStamina)>0){locomotion.Stamina.Restore(Mathf.Min(locomotion.Stamina.Maximum,locomotion.Stamina.Current+Value(ClassEffect.CounterStamina)),locomotion.Stamina.RecoveryWait);counterReady=Now+3;}}
            if(kind==DamageKind.Spear)chasePrepared=false;Changed?.Invoke();
        }
        public bool Overclock(WorldTurret turret)
        {
            if(!Authority || !FreeRepair || GetComponent<PlayerDeath>()?.CanAct!=true || !health.IsAlive || !commands.Session.IsRunning || Now<ActiveReady || turret==null || !turret.InRange(bag) || !ClassActionRules.Clear(health,turret.GetComponent<Health>()))return false;
            if(!turret.Overclock(12+Value(ClassEffect.OverclockDuration),.25f+Value(ClassEffect.OverclockPower),.35f+Value(ClassEffect.OverclockPower)))return false;
            ActiveReady=Now+30-Value(ClassEffect.OverclockCooldown);Changed?.Invoke();return true;
        }
        public float RepairTime(WorldPlaceable place)=>2*(1-Value(ClassEffect.RepairSpeed))*((place.Definition.Turret!=null && Value(ClassEffect.TurretService)>0 || place.GetComponent<SpikeTrap>()!=null && Value(ClassEffect.TrapService)>0)?.85f:1);
        public float RepairLock=>3-Value(ClassEffect.RepairLock);
        public float BufferCap(WorldPlaceable place)=>!FreeRepair?0:place.Health.Definition.MaximumHealth*(.2f+Value(ClassEffect.Overrepair)+(place.Definition.IsBarrier?Value(ClassEffect.BarrierBuffer):0)+(place.Definition.Turret!=null?Value(ClassEffect.TurretService):0)+(place.GetComponent<SpikeTrap>()!=null?Value(ClassEffect.TrapService):0));
        private void Update()
        {
            if(!Authority || !Locked)return;var elapsed=Math.Max(0,Now-lastStep);lastStep=Now;
            var moving=Vector2.Distance(transform.position,previous)>.001f || commands.Move.sqrMagnitude>.01f;previous=transform.position;
            if(moving)stationarySince=Now;
            if(moving && AimsAtQuarry(commands.Move))chaseSeconds+=elapsed;else chaseSeconds=0;
            if(Guarding){locomotion.Stamina.StopSprint();if(Now>guardLease || !commands.Session.IsRunning || !health.IsAlive || !locomotion.Stamina.Spend((float)elapsed*12*(1-Value(ClassEffect.GuardDrain))))SetGuard(false);}
            if(Quarry!=null && (!Quarry.IsAlive || !Quarry.gameObject.activeInHierarchy || Quarry.GetComponent<CreatureMember>()?.SpawnGeneration!=quarryGeneration || Now>=QuarryUntil)){Quarry=null;Changed?.Invoke();}
        }
        private void OnDisable(){Guarding=false;Quarry=null;ProtectionUntil=0;}
    }
    public static class ClassActionRules
    {
        private static readonly RaycastHit2D[] hits=new RaycastHit2D[32];
        public static bool Clear(Health from,Health to)
        {
            if(from==null || to==null)return false;var delta=(Vector2)(to.transform.position-from.transform.position);
            var n=Physics2D.Raycast(from.transform.position,delta.normalized,new ContactFilter2D{useTriggers=false},hits,delta.magnitude);if(n==hits.Length)return false;
            for(var i=0;i<n;i++){var h=hits[i].collider.GetComponentInParent<Health>();if(h!=from && h!=to)return false;}return true;
        }
    }
}
