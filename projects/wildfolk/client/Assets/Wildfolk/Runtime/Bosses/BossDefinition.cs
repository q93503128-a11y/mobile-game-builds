using System;
using UnityEngine;
namespace Wildfolk
{
    public enum BossState:byte { Awakening,Chase,Windup,Strike,Recovery,Ritual,Transition,Returning,Dying,Ended }
    public enum BossAttackShape:byte { Fan,Line,Circle,Summon }
    [Serializable] public sealed class BossAttack
    {
        public string Id;
        public BossAttackShape Shape;
        public int Damage,FirstPhase=1,LastPhase=3,Repeats=1,FollowUp=-1;
        public float Windup=.8f,Recovery=.7f,Range=2.8f,Width=1.2f,Angle=110,Advance;
    }
    [CreateAssetMenu(menuName="Wildfolk/Boss")]
    public sealed class BossDefinition:ScriptableObject
    {
        public string Id,NameKey;
        public BossEncounter Prefab;
        public RoyalTomb Tomb;
        public VitalityDefinition Vitality;
        public LootTable Rewards,MinionRewards;
        public EnemyController Minion;
        public ItemDefinition Offering;
        public WorldAnchorKind AnchorKind=WorldAnchorKind.Graveyard;
        public int OfferingCount=10;
        public Vector2 SpawnOffset=new Vector2(0,-2.8f);
        public float ClearingRadius=10;
        public float Speed=2.1f,Territory=23,Grace=4,ReturnSpeed=.78f,RegenDelay=2,RegenFraction=.01f;
        public float Awakening=3.6f,Ritual=1.5f,DeathSeconds=2.5f,LootPause=.4f,SummonInterval=28,DecisionDelay=.45f;
        public float LastPhaseSpeed=1.05f,LastPhaseRecovery=.9f,MinionSpeed=1.11f,StructureDamage=3;
        public float[] Thresholds={.7f,.35f};
        public BossAttack[] Attacks=Array.Empty<BossAttack>();
        public int[] Schedule={0,1,0,2,0,3};
        public int Phase(float ratio){var p=1;foreach(var t in Thresholds)if(ratio<=t)p++;return p;}
        public int AddCount(int living)=>Mathf.Clamp(living,1,4)+1;
        public bool CanSummon(long lastNight,int day,DayPhase phase,bool active)=>phase==DayPhase.Night && lastNight!=day && !active;
    }
    [Serializable] public sealed class BossSnapshot
    {
        public string Boss,Tomb;
        public long LastNight;
        public Vector2 Home,Position;
        public bool Active,PendingReward;
        public int HP;
        public string[] Eligible=Array.Empty<string>();
        public SavedStack[] RolledLoot=Array.Empty<SavedStack>();
        public BossSnapshot Copy()=>new BossSnapshot{Boss=Boss,Tomb=Tomb,LastNight=LastNight,Home=Home,Position=Position,Active=Active,PendingReward=PendingReward,HP=HP,
            Eligible=(string[])Eligible.Clone(),RolledLoot=Array.ConvertAll(RolledLoot,s=>new SavedStack{Item=s.Item,Count=s.Count,Charges=s.Charges,Quality=s.Quality})};
    }
}
