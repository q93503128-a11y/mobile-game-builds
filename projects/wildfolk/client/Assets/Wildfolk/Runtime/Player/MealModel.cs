using System;
namespace Wildfolk
{
    // Exactly one temporary meal. Replacement never adds time or heals current HP.
    public sealed class MealModel
    {
        public ItemDefinition Item{get;private set;}
        public float Remaining{get;private set;}
        public bool Active=>Item!=null && Remaining>0;
        public float Bonus(MealEffect effect)=>Active && Item.Consumable.Meal.Effect==effect ? Item.Consumable.Meal.Amount : 0;
        public void Begin(ItemDefinition item,float durationMultiplier=1)
        {var meal=item?.Consumable?.Meal;if(meal==null || meal.Duration<=0 || float.IsNaN(meal.Duration) || float.IsInfinity(meal.Duration))throw new ArgumentException("Invalid meal");Item=item;Remaining=meal.Duration*durationMultiplier;}
        public bool Advance(float seconds)
        {if(seconds<0 || float.IsNaN(seconds) || float.IsInfinity(seconds))throw new ArgumentOutOfRangeException(nameof(seconds));if(!Active)return false;Remaining=Math.Max(0,Remaining-seconds);if(Remaining>0)return false;Clear();return true;}
        public void Clear(){Item=null;Remaining=0;}
    }
}
