namespace Wildfolk
{
    public static class ItemStats
    {
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
