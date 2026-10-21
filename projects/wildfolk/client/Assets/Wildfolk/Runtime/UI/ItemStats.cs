namespace Wildfolk
{
    public static class ItemStats
    {
        public static string Food(ConsumableDefinition food)
        {
            if(food.Kind==ConsumableKind.Medicine)return (food.GradualHealthRestore>0 ? L.Format("ui.medicine.gradual",food.GradualHealthRestore,food.HealingDuration) : L.Format("ui.medicine.instant",food.HealthRestore))+"\n"+L.Format("ui.medicine.limit",food.UseCooldownSeconds);
            var text=L.Format("ui.food.stats",food.HungerRestore,food.ThirstRestore)+(food.HealthRestore>0 || food.StaminaRestore>0 ? "\n"+L.Format("ui.food.recovery",food.HealthRestore,food.StaminaRestore,food.UseCooldownSeconds) : "");
            if(food.Meal==null)return text;
            return text+"\n"+Meal(food.Meal)+"\n"+L.Format("ui.meal.rule",food.Meal.Duration,food.UseCooldownSeconds);
        }
        public static string Meal(MealDefinition meal)=>L.Format("ui.meal."+meal.Effect,meal.Effect==MealEffect.StaminaRecovery ? meal.Amount*100 : meal.Amount);
        public static string Armor(ArmorDefinition armor)=>L.Format("ui.inventory.armor_detail",L.Text("ui.slot."+armor.Slot),armor.Defense)+(armor.StaminaRecoveryBonus>0 ? "\n"+L.Format("ui.armor.recovery",armor.StaminaRecoveryBonus*100) : "");
        public static string Description(ItemDefinition item)
        {
            if(item.ExplorationUtility!=null)return L.Text("item."+item.Id+".description");
            if(item.Tool==null)return "";
            var ranged=item.Ranged;var damage=ranged==null ? item.Tool.Damage : ranged.Damage;
            var seconds=ranged==null ? item.Tool.Cooldown : ranged.DrawSeconds+ranged.RecoverySeconds;
            var range=ranged==null ? item.Tool.Attack.Range : ranged.Range;
            return L.Format("ui.weapon.stats",damage,(1/seconds).ToString("0.00"),range.ToString("0.##"));
        }
    }
}
