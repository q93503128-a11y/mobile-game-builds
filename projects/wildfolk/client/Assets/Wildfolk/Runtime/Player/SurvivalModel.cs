using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class SurvivalModel
    {
        public SurvivalDefinition Definition { get; }
        public float Hunger { get; private set; }
        public float Thirst { get; private set; }
        public float ConditionRatio=>Mathf.Min(Hunger/Definition.MaximumHunger,Thirst/Definition.MaximumThirst);
        public bool CanSprint=>ConditionRatio>=Definition.CriticalThreshold;
        public float SprintEfficiency=>CanSprint ? Mathf.Lerp(.55f,1,Mathf.InverseLerp(Definition.CriticalThreshold,Definition.RegenThreshold,ConditionRatio)) : 0;
        public float MovementCondition=>ConditionRatio<Definition.MovementPenaltyThreshold ? Definition.CriticalMoveMultiplier : 1;
        private float hungerClock, thirstClock;
        public float RecoveryBlockRemaining {get;private set;}
        public float DrainPenaltyRemaining {get;private set;}
        public float DrainMultiplier {get;private set;}=1;
        public SurvivalModel(SurvivalDefinition definition)
        { Definition=definition; Hunger=definition.MaximumHunger; Thirst=definition.MaximumThirst; }
        public int Advance(float seconds)
        {
            if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            var penalized=Mathf.Min(seconds,DrainPenaltyRemaining);
            var damage=AdvanceSegment(penalized,Definition.ThirstPerSecond*DrainMultiplier);
            damage+=AdvanceSegment(seconds-penalized,Definition.ThirstPerSecond);
            AdvanceEffects(seconds);
            return damage;
        }
        public void AdvanceEffects(float seconds)
        {
            if(seconds<0 || float.IsNaN(seconds) || float.IsInfinity(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));
            RecoveryBlockRemaining=Mathf.Max(0,RecoveryBlockRemaining-seconds);
            DrainPenaltyRemaining=Mathf.Max(0,DrainPenaltyRemaining-seconds);
            if(DrainPenaltyRemaining==0)DrainMultiplier=1;
        }
        private int AdvanceSegment(float seconds,float thirstRate)
        {
            if(seconds<=0)return 0;
            var damage=AdvanceMeter(Hunger,Definition.HungerPerSecond,seconds,ref hungerClock)+AdvanceMeter(Thirst,thirstRate,seconds,ref thirstClock);
            Hunger=Mathf.Max(0,Hunger-Definition.HungerPerSecond*seconds);Thirst=Mathf.Max(0,Thirst-thirstRate*seconds);return damage;
        }
        public void SetMeters(float hunger,float thirst)
        {Hunger=Mathf.Clamp(hunger,0,Definition.MaximumHunger);Thirst=Mathf.Clamp(thirst,0,Definition.MaximumThirst);hungerClock=thirstClock=0;}
        public SurvivalSnapshot Capture()=>new SurvivalSnapshot{Hunger=Hunger,Thirst=Thirst,HungerClock=hungerClock,ThirstClock=thirstClock,RecoveryBlock=RecoveryBlockRemaining,DrainPenalty=DrainPenaltyRemaining,DrainMultiplier=DrainMultiplier};
        public void Restore(SurvivalSnapshot state)
        {SetMeters(state.Hunger,state.Thirst);hungerClock=state.HungerClock;thirstClock=state.ThirstClock;RecoveryBlockRemaining=state.RecoveryBlock;DrainPenaltyRemaining=state.DrainPenalty;DrainMultiplier=state.DrainMultiplier;}
        private int AdvanceMeter(float value,float rate,float seconds,ref float clock)
        {
            var emptyTime=value<=0 ? seconds : rate>0 ? Mathf.Max(0,seconds-value/rate) : 0;
            if(emptyTime<=0) { clock=0; return 0; }
            clock+=emptyTime;
            var ticks=(int)(clock/Definition.DamageInterval);clock-=ticks*Definition.DamageInterval;
            return ticks*Definition.EmptyDamage;
        }
        public bool CanRestore(ConsumableDefinition food) => food!=null &&
            ((food.HungerRestore>0 && Hunger<Definition.MaximumHunger) || (food.ThirstRestore>0 && Thirst<Definition.MaximumThirst));
        public void Restore(ConsumableDefinition food)
        {
            Hunger=Mathf.Min(Definition.MaximumHunger,Hunger+food.HungerRestore);
            Thirst=Mathf.Min(Definition.MaximumThirst,Thirst+food.ThirstRestore);
            if(Hunger>0) hungerClock=0;if(Thirst>0) thirstClock=0;
            RecoveryBlockRemaining=Mathf.Max(RecoveryBlockRemaining,food.RecoveryBlockSeconds);
            DrainPenaltyRemaining=Mathf.Max(DrainPenaltyRemaining,food.DrainPenaltySeconds);
            if(food.DrainPenaltySeconds>0)DrainMultiplier=Mathf.Max(DrainMultiplier,food.ThirstDrainMultiplier);
        }
    }
}
