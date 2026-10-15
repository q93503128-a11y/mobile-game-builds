using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    public sealed class WorldSaveService : MonoBehaviour
    {
        public ContentCatalog Catalog;
        public WorldGenerator World;
        public WorldClock Clock;
        public PlayerInventory Player;
        public bool AutoSaveEnabled=true;
        public SaveRepository Repository {get;set;}
        public string LastError {get;private set;}
        public SaveGameRules Rules;
        private void Awake()
        {Repository=SceneFlow.Repository;if(Rules==null)Rules=Resources.Load<SaveGameRules>("SaveGameRules");}
        private void Start(){var game=SceneFlow.TakePending();if(game!=null)Apply(game);}
        public void Validate(SavedGame game)=>Rules.Validate(game);
        public SavedGame Capture()
        {
            var health=Player.GetComponent<Health>();var needs=Player.GetComponent<PlayerSurvival>();var progression=Player.GetComponent<PlayerProgression>();
            var game=new SavedGame{Seed=World.Seed,Elapsed=Clock.Model.Elapsed,Player=new PlayerSnapshot{Position=Player.transform.position,HP=health.Current,Selected=Player.SelectedIndex,Inventory=Player.Model.Capture(),Survival=needs.Model.Capture(),XP=progression.Experience,Gold=progression.Gold,Growth=progression.Capture(),HeadArmor=Player.GetComponent<PlayerEquipment>().HeadArmor==null ? null : Player.GetComponent<PlayerEquipment>().HeadArmor.Id,BodyArmor=Player.GetComponent<PlayerEquipment>().BodyArmor==null ? null : Player.GetComponent<PlayerEquipment>().BodyArmor.Id,Stamina=Player.GetComponent<PlayerLocomotion>().Stamina.Current,StaminaWait=Player.GetComponent<PlayerLocomotion>().Stamina.RecoveryWait,SeenRecipes=Player.GetComponent<PlayerCrafting>().CaptureSeen(),CombatRecovery=Mathf.Max(0,needs.Definition.RegenDelay-(Time.time-health.LastCombatTime))},Resources=World.Renewal.Capture(),Recovery=World.Renewal.CaptureRecovery(),Animals=World.Animals.Capture()};
            game.AnimalEcology=World.Animals.CaptureEcology();game.GoblinEcology=GetComponent<GoblinSpawner>().Capture();
            var night=new List<NightCreatureSnapshot>();if(Clock.Model.Phase==DayPhase.Night)foreach(var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))if(enemy.OwnHealth.IsAlive && enemy.Lifecycle.Phase!=ZombiePhase.Burrowing)night.Add(new NightCreatureSnapshot{Species=enemy.Member.Definition.Id,Variant=enemy.Variant!=null && enemy.Variant.Equipped ? enemy.Variant.Definition.Id : null,Position=enemy.transform.position,HP=enemy.OwnHealth.Current});game.NightCreatures=night.ToArray();
            var placed=new List<PlaceableSnapshot>();
            foreach(var facility in FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None))
            {
                if(!facility.Health.IsAlive)continue;var chest=facility.GetComponent<StorageChest>();var process=facility.GetComponent<StationProcessing>();var door=facility.GetComponent<WorldDoor>();
                placed.Add(new PlaceableSnapshot{Id=facility.SaveId,Item=facility.Definition.Item.Id,Position=facility.transform.position,Rotation=facility.transform.eulerAngles.z,HP=facility.Health.Current,Inventory=chest==null ? null : chest.Model.Capture(),Processing=process==null ? null : process.Capture(),DoorOpen=door!=null && door.IsOpen});
            }
            game.Placeables=placed.ToArray();var pickups=new List<PickupSnapshot>();
            foreach(var pickup in FindObjectsByType<WorldPickup>(FindObjectsSortMode.None))if(pickup.Item!=null && pickup.Count>0)pickups.Add(new PickupSnapshot{Item=pickup.Item.Id,Count=pickup.Count,Position=pickup.transform.position});
            game.Pickups=pickups.ToArray();return game;
        }
        public bool Save()
        {
            if(World.Map==null || !Player.GetComponent<Health>().IsAlive)return false;
            try{Repository.Write(Capture(),Validate);LastError=null;return true;}
            catch(Exception error)when(error is InvalidDataException || error is IOException || error is ArgumentException || error is InvalidOperationException){LastError=error.Message;return false;}
        }
        public void SaveFromMenu()=>PlayerFeedback.ShowKey(Save() ? "feedback.saved" : "feedback.save_failed");
        public bool Load()
        {
            SavedGame game;
            try{game=Repository.Read(Validate);LastError=null;}
            catch(Exception error)when(error is InvalidDataException || error is IOException || error is ArgumentException || error is InvalidOperationException){LastError=error.Message;PlayerFeedback.ShowKey("feedback.load_failed");return false;}
            SceneFlow.Continue(game,Repository);return true;
        }
        public void SaveAndHome(){if(!Save()){PlayerFeedback.ShowKey("feedback.save_failed");return;}GetComponent<UserPreferences>().Flush();AutoSaveEnabled=false;SceneFlow.Home(Repository);}
        public void Apply(SavedGame game)
        {
            Validate(game);World.Session.Resume();World.Session.CloseInventory();World.Session.CloseCharacter();World.Session.CloseSkillTree();if(World.Seed!=game.Seed || World.Renewal.HasChanges)World.Generate(game.Seed);
            Clock.Model.SetElapsed(game.Elapsed);
            Player.GetComponent<Rigidbody2D>().position=game.Player.Position;Player.transform.position=game.Player.Position;Physics2D.SyncTransforms();
            foreach(var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))PrefabPool.Release(enemy.gameObject);
            foreach(var goblin in FindObjectsByType<GoblinController>(FindObjectsSortMode.None))PrefabPool.Release(goblin.gameObject);
            foreach(var pickup in FindObjectsByType<WorldPickup>(FindObjectsSortMode.None))PrefabPool.Release(pickup.gameObject);
            foreach(var facility in FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None)){facility.gameObject.SetActive(false);Destroy(facility.gameObject);}
            World.Renewal.Restore(game.Resources,game.Recovery);World.Animals.Restore(game.Animals,game.AnimalEcology);
            foreach(var state in game.Placeables)
            {
                var facility=Instantiate(Catalog.Item(state.Item).Placement.Prefab,state.Position,Quaternion.Euler(0,0,state.Rotation));facility.SaveId=state.Id;facility.Health.RestoreHealth(state.HP);
                var chest=facility.GetComponent<StorageChest>();if(chest!=null){chest.BindVisitor(Player);chest.Model.Restore(state.Inventory,Catalog);}
                var process=facility.GetComponent<StationProcessing>();if(process!=null)process.Restore(state.Processing,Catalog,Player);
                var door=facility.GetComponent<WorldDoor>();if(door!=null)door.RestoreOpen(state.DoorOpen);
            }
            GetComponent<GoblinSpawner>().Restore(game.GoblinEcology);
            GetComponent<ZombieSpawner>().Restore(game.NightCreatures);
            var player=game.Player;Player.Model.Restore(player.Inventory,Catalog);Player.SelectSlot(player.Selected);Player.GetComponent<PlayerProgression>().Restore(player.Growth,player.XP,player.Gold);Player.GetComponent<CharacterStats>().Apply();
            var equipment=Player.GetComponent<PlayerEquipment>();equipment.RestoreArmor(string.IsNullOrEmpty(player.HeadArmor) ? null : Catalog.Item(player.HeadArmor),string.IsNullOrEmpty(player.BodyArmor) ? null : Catalog.Item(player.BodyArmor));Player.GetComponent<PlayerLocomotion>().Restore(player.Stamina,player.StaminaWait);Player.GetComponent<PlayerCrafting>().RestoreSeen(player.SeenRecipes);var health=Player.GetComponent<Health>();health.RestoreHealth(player.HP);health.RestoreCombatTime(player.CombatRecovery,Player.GetComponent<PlayerSurvival>().Definition.RegenDelay);
            Player.GetComponent<PlayerSurvival>().Model.Restore(player.Survival);
            foreach(var state in game.Pickups){var pickup=PrefabPool.Spawn(World.Pickup,state.Position,Quaternion.identity);pickup.Initialize(Catalog.Item(state.Item),state.Count,Player,World.Settings,true);}
            Physics2D.SyncTransforms();
        }
        private void OnApplicationPause(bool paused){if(paused && AutoSaveEnabled && Application.isPlaying)Save();}
        private void OnApplicationQuit(){if(AutoSaveEnabled && World.Map!=null)Save();}
    }
}
