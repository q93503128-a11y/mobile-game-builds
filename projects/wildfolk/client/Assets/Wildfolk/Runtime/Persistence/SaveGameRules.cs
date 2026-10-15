using System;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    // References the same authored content as gameplay; creates no world GameObjects.
    public sealed class SaveGameRules : ScriptableObject
    {
        public ContentCatalog Catalog;
        public WorldGenerationDefinition World;
        public AnimalPopulationDefinition Animals;
        public PlayerInventory Player;
        public DayNightDefinition Clock;
        public ZombieSpawnDefinition Night;
        public EnemyController[] NightPrefabs;
        public GoblinController Goblin;
        public GoblinCampDefinition Camps;
        public void Validate(SavedGame game)
        {
            var growth=Player.GetComponent<PlayerProgression>().Definition;var locomotion=Player.GetComponent<PlayerLocomotion>().Definition;
            if(game.Version==1)
            {if(game.Player==null)throw new InvalidDataException("Missing player");game.Player.Growth=GrowthValidation.FromLegacy(game.Player.XP,growth);game.Player.HeadArmor=game.Player.BodyArmor=null;game.Player.Stamina=locomotion.MaximumStamina;game.Player.StaminaWait=0;game.Player.SeenRecipes=Array.Empty<string>();game.Version=2;}
            if(game.Version==2){SaveValidation.Validate(game,Catalog,World,Animals,Player.Settings.InventorySlots,Player.GetComponent<Health>().Definition.MaximumHealth,Player.GetComponent<PlayerSurvival>().Definition,growth,locomotion);SaveMigration.PopulationFromV2(game,World);}
            if(game.Version==3){SaveValidation.Validate(game,Catalog,World,Animals,Player.Settings.InventorySlots,Player.GetComponent<Health>().Definition.MaximumHealth,Player.GetComponent<PlayerSurvival>().Definition,growth,locomotion);SaveMigration.EcologyFromV3(game,World,Animals);}
            SaveValidation.Validate(game,Catalog,World,Animals,Player.Settings.InventorySlots,Player.GetComponent<Health>().Definition.MaximumHealth,Player.GetComponent<PlayerSurvival>().Definition,growth,locomotion);
            ValidateEnemies(game);
        }
        private void ValidateEnemies(SavedGame game)
        {
            if(game.NightCreatures==null || game.NightCreatures.Length>Night.HardCap)throw new InvalidDataException("Invalid nocturnal population");
            foreach(var state in game.NightCreatures){if(state==null)throw new InvalidDataException("Missing nocturnal actor");var prefab=SpeciesPrefab(state.Species);if(state.HP<=0 || state.HP>prefab.OwnHealth.Maximum || !new Bounds(Vector3.zero,new Vector3(World.Size.x,World.Size.y,1)).Contains(state.Position) || float.IsNaN(state.Position.x) || float.IsNaN(state.Position.y) || !string.IsNullOrEmpty(state.Variant) && (prefab.Variant==null || prefab.Variant.Definition.Id!=state.Variant))throw new InvalidDataException("Invalid nocturnal actor");}
            if(game.GoblinEcology!=null){if(game.GoblinEcology.Actors.Length>Camps.HardCap)throw new InvalidDataException("Faction cap exceeded");foreach(var actor in game.GoblinEcology.Actors)if(actor.HP>Goblin.OwnHealth.Maximum)throw new InvalidDataException("Invalid faction health");}
        }
        private EnemyController SpeciesPrefab(string id)
        {foreach(var prefab in NightPrefabs)if(prefab.Member.Definition.Id==id)return prefab;throw new InvalidDataException("Unknown nocturnal species");}
    }
}
