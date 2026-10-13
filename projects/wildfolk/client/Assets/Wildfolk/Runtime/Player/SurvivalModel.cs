using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class SurvivalModel
    {
        public SurvivalDefinition Definition { get; }
        public float Hunger { get; private set; }
        public float Thirst { get; private set; }
        private float hungerClock, thirstClock;
        public SurvivalModel(SurvivalDefinition definition)
        { Definition=definition; Hunger=definition.MaximumHunger; Thirst=definition.MaximumThirst; }
        public int Advance(float seconds)
        {
            if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            var damage=AdvanceMeter(Hunger, Definition.HungerPerSecond, seconds, ref hungerClock);
            damage+=AdvanceMeter(Thirst, Definition.ThirstPerSecond, seconds, ref thirstClock);
            Hunger=Mathf.Max(0,Hunger-Definition.HungerPerSecond*seconds);
            Thirst=Mathf.Max(0,Thirst-Definition.ThirstPerSecond*seconds);
            return damage;
        }
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
        }
    }
}
