using UnityEngine;
namespace Wildfolk
{
    public static class PlayerSnapshotCodec
    {
        public static PlayerSnapshot Capture(PlayerInventory Player)
        {
            var health=Player.GetComponent<Health>();var needs=Player.GetComponent<PlayerSurvival>();var progression=Player.GetComponent<PlayerProgression>();
            return new PlayerSnapshot{Position=Player.transform.position,HP=health.Current,Selected=Player.SelectedIndex,Inventory=Player.Model.Capture(),Survival=needs.Model.Capture(),XP=progression.Experience,Gold=progression.Gold,Growth=progression.Capture(),HeadArmor=Player.GetComponent<PlayerEquipment>().HeadArmor==null ? null : Player.GetComponent<PlayerEquipment>().HeadArmor.Id,BodyArmor=Player.GetComponent<PlayerEquipment>().BodyArmor==null ? null : Player.GetComponent<PlayerEquipment>().BodyArmor.Id,Stamina=Player.GetComponent<PlayerLocomotion>().Stamina.Current,StaminaWait=Player.GetComponent<PlayerLocomotion>().Stamina.RecoveryWait,SeenRecipes=Player.GetComponent<PlayerCrafting>().CaptureSeen(),CombatRecovery=Mathf.Max(0,needs.Definition.RegenDelay-(Time.time-health.LastCombatTime))};
        }
        public static void Apply(PlayerInventory Player,PlayerSnapshot state,ContentCatalog Catalog)
        {
            var body=Player.GetComponent<Rigidbody2D>();if(body!=null)body.position=state.Position;Player.transform.position=state.Position;
            var player=state;Player.Model.Restore(player.Inventory,Catalog);Player.SelectSlot(player.Selected);Player.GetComponent<PlayerProgression>().Restore(player.Growth,player.XP,player.Gold);Player.GetComponent<CharacterStats>().Apply();
            var equipment=Player.GetComponent<PlayerEquipment>();equipment.RestoreArmor(string.IsNullOrEmpty(player.HeadArmor) ? null : Catalog.Item(player.HeadArmor),string.IsNullOrEmpty(player.BodyArmor) ? null : Catalog.Item(player.BodyArmor));Player.GetComponent<PlayerLocomotion>().Restore(player.Stamina,player.StaminaWait);Player.GetComponent<PlayerCrafting>().RestoreSeen(player.SeenRecipes);var health=Player.GetComponent<Health>();health.RestoreHealth(player.HP);health.RestoreCombatTime(player.CombatRecovery,Player.GetComponent<PlayerSurvival>().Definition.RegenDelay);
            Player.GetComponent<PlayerSurvival>().Model.Restore(player.Survival);

            if(state.HP==0)Player.GetComponent<GameplaySession>()?.ReplicateDefeat();
        }
    }
}
