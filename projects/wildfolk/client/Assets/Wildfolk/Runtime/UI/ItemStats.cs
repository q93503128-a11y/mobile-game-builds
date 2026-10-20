namespace Wildfolk
{
    public static class ItemStats
    {
        public static string Food(ConsumableDefinition food)=>food.Kind==ConsumableKind.Medicine ? (food.GradualHealthRestore>0 ? L.Format("ui.medicine.gradual",food.GradualHealthRestore,food.HealingDuration) : L.Format("ui.medicine.instant",food.HealthRestore))+"\n"+L.Format("ui.medicine.limit",food.UseCooldownSeconds) : L.Format("ui.food.stats",food.HungerRestore,food.ThirstRestore)+(food.HealthRestore>0 || food.StaminaRestore>0 ? "\n"+L.Format("ui.food.recovery",food.HealthRestore,food.StaminaRestore,food.UseCooldownSeconds) : "");
        public static string Armor(ArmorDefinition armor)=>L.Format("ui.inventory.armor_detail",L.Text("ui.slot."+armor.Slot),armor.Defense)+(armor.StaminaRecoveryBonus>0 ? "\n"+L.Format("ui.armor.recovery",armor.StaminaRecoveryBonus*100) : "");
        public static string Description(ItemDefinition item)
        {
            if(item.Tool==null)return "";
            var ranged=item.Ranged;var damage=ranged==null ? item.Tool.Damage : ranged.Damage;
            var seconds=ranged==null ? item.Tool.Cooldown : ranged.DrawSeconds+ranged.RecoverySeconds;
            var range=ranged==null ? item.Tool.Attack.Range : ranged.Range;
            return L.Format("ui.weapon.stats",damage,(1/seconds).ToString("0.00"),range.ToString("0.0"));
        }
    }
}
