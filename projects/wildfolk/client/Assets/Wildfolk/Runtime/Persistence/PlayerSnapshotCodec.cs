using UnityEngine;
namespace Wildfolk
{
    public static class PlayerSnapshotCodec
    {
        public static PlayerSnapshot Capture(PlayerInventory Player)
        {
            var health=Player.GetComponent<Health>();var needs=Player.GetComponent<PlayerSurvival>();var progression=Player.GetComponent<PlayerProgression>();
            var persistentHP=Mathf.Min(health.Current,health.Definition.MaximumHealth+Player.GetComponent<CharacterStats>().PersistentHealthBonus);
            return new PlayerSnapshot{NavigationMode=Player.GetComponent<PlayerNavigation>().Mode,BeaconId=Player.GetComponent<PlayerNavigation>().BeaconId,Utility=Player.GetComponent<PlayerEquipment>().Utility==null ? null : Player.GetComponent<PlayerEquipment>().Utility.Id,HomeId=Player.GetComponent<PlayerNavigation>()?.HomeId,Position=Player.transform.position,HP=persistentHP,Selected=Player.SelectedIndex,Inventory=Player.Model.Capture(),Survival=needs.Model.Capture(),XP=progression.Experience,Gold=progression.Gold,Growth=progression.Capture(),HeadArmor=Player.GetComponent<PlayerEquipment>().HeadArmor==null ? null : Player.GetComponent<PlayerEquipment>().HeadArmor.Id,BodyArmor=Player.GetComponent<PlayerEquipment>().BodyArmor==null ? null : Player.GetComponent<PlayerEquipment>().BodyArmor.Id,Stamina=Player.GetComponent<PlayerLocomotion>().Stamina.Current,StaminaWait=Player.GetComponent<PlayerLocomotion>().Stamina.RecoveryWait,SeenRecipes=Player.GetComponent<PlayerCrafting>().CaptureSeen(),CombatRecovery=Mathf.Max(0,needs.Definition.RegenDelay-(Time.time-health.LastCombatTime))};
        }
        public static void Apply(PlayerInventory Player,PlayerSnapshot state,ContentCatalog Catalog)
        {
            Player.GetComponent<PlayerSurvival>().ResetTransient();
            Player.GetComponent<PlayerFishing>()?.ResetTransient();
            var body=Player.GetComponent<Rigidbody2D>();if(body!=null)body.position=state.Position;Player.transform.position=state.Position;
            Player.GetComponent<PlayerNavigation>()?.RestoreHome(state.HomeId);Player.GetComponent<PlayerNavigation>()?.RestoreNavigation(state.NavigationMode,state.BeaconId);
            Player.GetComponent<PlayerEquipment>().RestoreUtility(string.IsNullOrEmpty(state.Utility) ? null : Catalog.Item(state.Utility));var player=state;Player.Model.Restore(player.Inventory,Catalog);Player.SelectSlot(player.Selected);Player.GetComponent<PlayerProgression>().Restore(player.Growth,player.XP,player.Gold);Player.GetComponent<CharacterStats>().Apply();
            var equipment=Player.GetComponent<PlayerEquipment>();equipment.RestoreArmor(string.IsNullOrEmpty(player.HeadArmor) ? null : Catalog.Item(player.HeadArmor),string.IsNullOrEmpty(player.BodyArmor) ? null : Catalog.Item(player.BodyArmor));Player.GetComponent<PlayerLocomotion>().Restore(player.Stamina,player.StaminaWait);Player.GetComponent<PlayerCrafting>().RestoreSeen(player.SeenRecipes);var health=Player.GetComponent<Health>();health.RestoreHealth(player.HP);health.RestoreCombatTime(player.CombatRecovery,Player.GetComponent<PlayerSurvival>().Definition.RegenDelay);
            Player.GetComponent<PlayerSurvival>().Model.Restore(player.Survival);

            if(state.HP==0)Player.GetComponent<GameplaySession>()?.ReplicateDefeat();
        }
    }
}
