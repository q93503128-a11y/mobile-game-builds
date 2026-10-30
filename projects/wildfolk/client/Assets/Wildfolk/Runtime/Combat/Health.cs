using System;
using UnityEngine;

namespace Wildfolk
{
    public sealed class Health : MonoBehaviour
    {
        public VitalityDefinition Definition;
        public int Current { get; private set; }
        public int Maximum => Definition.MaximumHealth+maximumBonus;
        private int maximumBonus;
        public bool IsReplica {get;internal set;}
        private bool deathNotified,replicaInitialized;
        public void ApplyReplica(int value,bool hit=false)
        {
            var previous=Current;Current=Mathf.Clamp(value,0,Maximum);
            var damaged=hit || replicaInitialized && Current<previous;replicaInitialized=true;
            if(previous!=Current)Changed?.Invoke();if(damaged)Damaged?.Invoke();
        }
        public void SetMaximumBonus(int bonus){maximumBonus=Math.Max(0,bonus);Current=Math.Min(Current,Maximum);Changed?.Invoke();}
        public int DamageReduction { get; set; }
        public PlayerInventory LastAttacker { get; private set; }
        public float LastCombatTime { get; private set; }=float.NegativeInfinity;
        public bool IsAlive => Current > 0;
        public float Ratio => Maximum > 0 ? (float)Current / Maximum : 0;
        public event Action Changed;
        public event Action Damaged;
        public event Action Died;

        private void Awake()
        {
            if (Definition == null || Maximum <= 0) throw new InvalidOperationException("A valid vitality definition is required.");
            Current = Maximum;
            if(Application.isPlaying && GetComponent<AudioActor>()==null)gameObject.AddComponent<AudioActor>();
        }

        public int ApplyDamage(int amount,bool ignoreDefense=false,PlayerInventory attacker=null,Health source=null,DamageKind kind=DamageKind.Unspecified,bool prepared=false)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (GetComponent<DamageProtection>()?.Blocked==true || IsReplica || !IsAlive || deathNotified || (GetComponent<PlayerDeath>()?.BlocksDamage==true || GetComponent<PlayerClass>()?.BlocksDamage==true)) return 0;
            var dealer=source?.GetComponent<PlayerClass>();if(dealer!=null && kind!=DamageKind.Unspecified)amount=dealer.Outgoing(this,amount,kind,prepared);
            var role=GetComponent<PlayerClass>();var mitigation=ignoreDefense?amount:Math.Max(1,amount-DamageReduction);bool guarded=false;
            if(role!=null && !ignoreDefense)mitigation=role.Incoming(mitigation,source,out guarded);
            if(!ignoreDefense)mitigation=Math.Max(1,Mathf.CeilToInt(mitigation*(GetComponent<CharacterStats>()?.IncomingMultiplier??1)));
            var buffer=GetComponent<StructureMaintenance>();if(buffer!=null)mitigation=buffer.Absorb(mitigation);
            var applied=Math.Min(Current,mitigation);var survived=role!=null && role.PreventLethal(ref applied);
            Current -= applied;if(survived)Current=Math.Max(Current,role.SurvivingHealth);var died=Current==0;if(died)deathNotified=true;
            LastAttacker=attacker;LastCombatTime=Time.time;
            Changed?.Invoke();
            Damaged?.Invoke();
            role?.Received(applied,guarded);dealer?.Hit(this,applied,kind);
            if (died) Died?.Invoke();
            return applied;
        }
        public void MarkCombat()=>LastCombatTime=Time.time;
        public void RestoreHealth(int health)
        {if(health<0 || health>Maximum)throw new ArgumentOutOfRangeException(nameof(health));Current=health;deathNotified=health==0;LastAttacker=null;Changed?.Invoke();}
        public void ResetForSpawn(){replicaInitialized=false;RestoreHealth(Maximum);LastCombatTime=float.NegativeInfinity;}
        public void RestoreCombatTime(float remaining,float delay)=>LastCombatTime=remaining<=0 ? float.NegativeInfinity : Time.time-delay+remaining;
        public int Heal(int amount)
        {
            if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));
            if(IsReplica || !IsAlive)return 0;var restored=Math.Min(amount,Maximum-Current);Current+=restored;
            if(restored>0)Changed?.Invoke();return restored;
        }
    }
}
