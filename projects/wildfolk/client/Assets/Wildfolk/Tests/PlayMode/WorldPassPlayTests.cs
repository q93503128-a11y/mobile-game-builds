using System.Linq;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class WorldPassPlayTests
    {
        private PlayPointerDriver input;private PlayerInventory inventory;private PlayerMotor player;private GameplayHud hud;private WorldGenerator world;private PlayerCrafting crafting;
        [UnitySetUp]public IEnumerator Load()
        {input=new PlayPointerDriver();Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=1;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;inventory=Object.FindFirstObjectByType<PlayerInventory>();player=inventory.GetComponent<PlayerMotor>();hud=Object.FindFirstObjectByType<GameplayHud>();world=Object.FindFirstObjectByType<WorldGenerator>();crafting=inventory.GetComponent<PlayerCrafting>();}
        [UnityTearDown]public IEnumerator Release(){input.Dispose();Time.timeScale=1;yield return null;}
        private RecipeDefinition Recipe(string id){foreach(var r in crafting.Catalog.Recipes)if(r.Id==id)return r;throw new System.InvalidOperationException(id);}
        private WorldPlaceable Place(string id,Vector2 p)=>Object.Instantiate(Recipe(id).Result.Placement.Prefab,p,Quaternion.identity);
        private void Position(Vector2 p,Vector2 facing){player.GetComponent<Rigidbody2D>().position=p;player.transform.position=p;player.Avatar.Facing.Face(facing);Physics2D.SyncTransforms();}
        private void ClearStart(){foreach(var n in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(n.transform.position.sqrMagnitude<100)n.gameObject.SetActive(false);}
        [UnityTest]public IEnumerator AttackNeverOpensStationOrChestAndInteractDoes()
        {
            ClearStart();Position(Vector2.zero,Vector2.down);var bench=Place("workbench",new Vector2(0,-2.1f));yield return null;yield return null;
            var hp=bench.Health.Current;yield return input.Click(hud.Attack);yield return new WaitForSeconds(.5f);
            Assert.That(hud.Session.IsRunning,Is.True);Assert.That(crafting.Station,Is.Null);Assert.That(bench.Health.Current,Is.EqualTo(hp));
            yield return input.Click(hud.Interact);Assert.That(crafting.Station,Is.SameAs(bench.GetComponent<CraftingStation>()));
            hud.Session.CloseInventory();Object.Destroy(bench.gameObject);yield return null;
            var chest=Place("storage_chest",new Vector2(0,-2.1f)).GetComponent<StorageChest>();yield return null;yield return null;
            yield return input.Click(hud.Attack);Assert.That(inventory.GetComponent<PlayerStorage>().IsOpen,Is.False);
            yield return input.Click(hud.Interact);Assert.That(inventory.GetComponent<PlayerStorage>().Chest,Is.SameAs(chest));
        }
        [UnityTest]public IEnumerator ActualGeneratedWorldHasWaterCollisionsFordAndGroundSorting()
        {
            var expected=0;foreach(var rule in world.Definition.Resources)expected+=rule.Count;Assert.That(world.Map.Resources.Count,Is.EqualTo(expected));Assert.That(world.Map.IsWalkable(player.transform.position,2),Is.True);
            var colliders=new Collider2D[8];var deep=world.Map.LakeCenter;
            Assert.That(Physics2D.OverlapCircle(deep,.4f,new ContactFilter2D{useTriggers=false},colliders),Is.GreaterThan(0));
            var ford=new Vector2(world.Map.RiverAt(world.Map.FordY),world.Map.FordY);Assert.That(Physics2D.OverlapCircle(ford,.4f,new ContactFilter2D{useTriggers=false},colliders),Is.Zero);
            foreach(var sprite in world.Environment.GetComponentsInChildren<SpriteRenderer>())if(sprite.name=="Ground Foliage")Assert.That(sprite.sortingOrder,Is.EqualTo(WorldSorting.GroundDecoration));
            Assert.That(world.Terrain.Ground.bounds.size.x,Is.EqualTo(world.Definition.Size.x).Within(.1f));
            var first=world.Map.Resources[0].Position;world.Generate(9472026);yield return null;Assert.That(world.Map.Resources[0].Position,Is.EqualTo(first));
            world.Generate(42);yield return null;Assert.That(world.Map.Resources[0].Position,Is.Not.EqualTo(first));
        }
        [UnityTest]public IEnumerator DawnBurrowsWithoutRewardsAndEmergenceCannotAttack()
        {
            ClearStart();var enemy=WorldTestScene.SpawnZombie();var life=enemy.GetComponent<ZombieLifecycle>();Position(enemy.transform.position+Vector3.left*1.4f,Vector2.right);
            var hp=player.GetComponent<Health>().Current;yield return new WaitForSeconds(.55f);Assert.That(life.Phase,Is.EqualTo(ZombiePhase.Emerging));Assert.That(player.GetComponent<Health>().Current,Is.EqualTo(hp));
            yield return new WaitForSeconds(.7f);Assert.That(life.Phase,Is.EqualTo(ZombiePhase.Active));
            var clock=enemy.Clock;var cycle=clock.Definition.CycleSeconds;clock.Model.Advance(clock.Definition.DaySeconds+clock.Definition.EveningSeconds+clock.Definition.NightSeconds-(float)(clock.Model.Elapsed%cycle)+.01f);
            yield return null;Assert.That(life.Phase,Is.EqualTo(ZombiePhase.Burrowing));hp=player.GetComponent<Health>().Current;
            yield return new WaitForSeconds(1.25f);Assert.That(enemy==null || !enemy.gameObject.activeInHierarchy,Is.True);Assert.That(player.GetComponent<Health>().Current,Is.EqualTo(hp));
            Assert.That(inventory.GetComponent<PlayerProgression>().Experience,Is.Zero);Assert.That(inventory.GetComponent<PlayerProgression>().Gold,Is.Zero);foreach(var drop in Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None)){Assert.That(drop.Item,Is.Not.Null);Assert.That(drop.Count,Is.GreaterThan(0));}
        }
        [UnityTest]public IEnumerator SafeLightBlocksNewAggroAndEndsExistingPursuitAfterGrace()
        {
            ClearStart();var enemy=WorldTestScene.SpawnZombie();Position(Vector2.zero,Vector2.right);yield return new WaitForSeconds(1.35f);
            enemy.GetComponent<Rigidbody2D>().position=new Vector2(5,0);enemy.transform.position=new Vector2(5,0);yield return new WaitForSeconds(.2f);Assert.That(enemy.State,Is.EqualTo(EnemyState.Chase));
            var fire=Place("campfire",new Vector2(0,-2.1f));yield return null;
            Assert.That(GameplayLight.IsSafe(player.transform.position),Is.True);yield return new WaitForSeconds(2.2f);Assert.That(enemy.State,Is.EqualTo(EnemyState.Idle));
            enemy.CancelPursuit();yield return new WaitForSeconds(.3f);Assert.That(enemy.State,Is.EqualTo(EnemyState.Idle));
            Object.Destroy(fire.gameObject);yield return null;yield return new WaitForSeconds(.2f);Assert.That(enemy.State,Is.Not.EqualTo(EnemyState.Idle));
        }
        [UnityTest]public IEnumerator SplitHalfViaTouchButtonAndExplicitDropZonePreserveItems()
        {
            ClearStart();var wood=Recipe("workbench").Ingredients[0].Item;inventory.Model.Add(wood,30);yield return input.Inventory(hud);
            var panel=Object.FindFirstObjectByType<InventoryPanel>();var drag=Object.FindFirstObjectByType<InventoryDragController>();
            yield return input.Click(panel.Slots[0]);yield return input.Click(drag.SplitButton);yield return input.Click(drag.SplitPanel.ConfirmButton);Assert.That(inventory.Model.GetSlot(0).Count,Is.EqualTo(30));yield return input.Click(panel.Slots[1]);Assert.That(inventory.Model.GetSlot(0).Count,Is.EqualTo(15));Assert.That(inventory.Model.GetSlot(1).Count,Is.EqualTo(15));
            yield return input.Drag(panel.Slots[1],new Vector2(5,Screen.height*.5f));Assert.That(inventory.Model.CountItem(wood),Is.EqualTo(30));
            yield return input.Drag(panel.Slots[1],PlayPointerDriver.Point(drag.DropZone.transform),()=>Assert.That(drag.DropZone.activeSelf,Is.True));
            Assert.That(inventory.Model.CountItem(wood),Is.EqualTo(15));var pickup=Object.FindFirstObjectByType<WorldPickup>();Assert.That(pickup.Count,Is.EqualTo(15));Assert.That(drag.DropZone.activeSelf,Is.False);
            yield return new WaitForSeconds(1);Assert.That(pickup,Is.Not.Null);Position(pickup.transform.position,Vector2.up);yield return new WaitForSeconds(.3f);Assert.That(inventory.Model.CountItem(wood),Is.EqualTo(30));
            hud.Session.CloseInventory();var chest=Place("storage_chest",(Vector2)player.transform.position+Vector2.down*2.1f).GetComponent<StorageChest>();chest.Model.Add(wood,30);yield return null;
            Assert.That(inventory.GetComponent<PlayerStorage>().OpenChest(chest),Is.True);yield return null;var chestPanel=Object.FindFirstObjectByType<ChestPanel>();
            yield return input.Click(chestPanel.ChestSlots[0]);Assert.That(chestPanel.ChestSlots[0].Selection.enabled,Is.True);
            yield return input.Click(drag.ChestSplitButton);Assert.That(drag.SplitPanel.View.activeSelf,Is.True,"Chest split dialog opens");yield return input.Click(drag.SplitPanel.ConfirmButton);Assert.That(drag.IsHoldingPartial,Is.True,"Chest split holds partial stack");yield return input.Click(chestPanel.ChestSlots[1]);Assert.That(chest.Model.GetSlot(0).Count,Is.EqualTo(15));Assert.That(chest.Model.GetSlot(1).Count,Is.EqualTo(15));
            yield return input.Drag(chestPanel.ChestSlots[1],PlayPointerDriver.Point(chestPanel.PlayerSlots[9]));Assert.That(inventory.Model.CountItem(wood),Is.EqualTo(45));Assert.That(chest.Model.CountItem(wood),Is.EqualTo(15));
        }
        [UnityTest]public IEnumerator FurnaceSequentialQueuePreservesFinishedAndReservedCargoWhenDestroyed()
        {
            ClearStart();var furnace=Place("furnace",new Vector2(0,-2.1f)).GetComponent<CraftingStation>();yield return null;var recipe=Recipe("iron_ingot");var process=furnace.GetComponent<StationProcessing>();
            inventory.Model.Add(recipe.Ingredients[0].Item,5);Assert.That(crafting.OpenStation(furnace),Is.True);yield return null;var panel=Object.FindFirstObjectByType<CraftingPanel>();for(var i=0;i<3;i++)yield return input.Click(panel.AddOne);
            Assert.That(process.BatchCount,Is.EqualTo(1));Assert.That(process.Queue.WaitingCount,Is.EqualTo(2));Assert.That(inventory.Model.CountItem(recipe.Ingredients[0].Item),Is.EqualTo(2));
            process.Advance(6.1f);yield return new WaitUntil(()=>panel.Collect.interactable);yield return input.Click(panel.Collect);Assert.That(inventory.Model.CountItem(recipe.Result),Is.EqualTo(3));yield return input.Click(panel.AddAll);Assert.That(process.Queue.WaitingCount,Is.EqualTo(1));furnace.Health.ApplyDamage(furnace.Health.Maximum);yield return null;
            var drop=Object.FindFirstObjectByType<WorldPickup>();Assert.That(drop.Item,Is.SameAs(recipe.Ingredients[0].Item));Assert.That(Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None).Where(p=>p.Item==recipe.Ingredients[0].Item).Sum(p=>p.Count),Is.EqualTo(2));
        }
        [UnityTest]public IEnumerator SurvivalRegeneratesOnlyAfterDelayAndWhenBothMetersAreHigh()
        {
            var needs=inventory.GetComponent<PlayerSurvival>();needs.Health.ApplyDamage(10);var hp=needs.Health.Current;
            yield return new WaitForSeconds(2);Assert.That(needs.Health.Current,Is.EqualTo(hp));
            yield return new WaitForSeconds(needs.Definition.RegenDelay);Assert.That(needs.Health.Current,Is.GreaterThan(hp));
            needs.Model.Advance(800);hp=needs.Health.Current;yield return new WaitForSeconds(2.3f);Assert.That(needs.Health.Current,Is.EqualTo(hp));
            Assert.That(needs.CanSprint,Is.False);Assert.That(needs.SprintEfficiency,Is.Zero);
        }
        [UnityTest]public IEnumerator GoblinPatrolsAggrosOnlyOnTerritoryOrHitAndGivesSeparateRewards()
        {
            ClearStart();var spawner=Object.FindFirstObjectByType<GoblinSpawner>();spawner.enabled=false;var goblin=Object.Instantiate(spawner.Prefab,new Vector2(6,0),Quaternion.identity);goblin.Session=hud.Session;goblin.PlayerHealth=inventory.GetComponent<Health>();goblin.GetComponent<LootOnDeath>().Player=inventory;
            yield return new WaitForSeconds(.5f);Assert.That(goblin.InCombat,Is.False);
            Position(new Vector2(4,0),Vector2.right);yield return new WaitForSeconds(.2f);Assert.That(goblin.InCombat,Is.True);
            Position(new Vector2(-8,0),Vector2.right);yield return new WaitForSeconds(.3f);Assert.That(goblin.InCombat,Is.False);
            Position(new Vector2(1,0),Vector2.right);goblin.OwnHealth.ApplyDamage(1,false,inventory);yield return null;Assert.That(goblin.InCombat,Is.True);
            var rewards=inventory.GetComponent<PlayerProgression>();goblin.OwnHealth.ApplyDamage(goblin.OwnHealth.Current,false,inventory);yield return null;Assert.That(rewards.Experience,Is.EqualTo(8));Assert.That(rewards.Gold,Is.EqualTo(3));
            var zombie=WorldTestScene.SpawnZombie();zombie.OwnHealth.ApplyDamage(zombie.OwnHealth.Current,false,inventory);yield return null;Assert.That(rewards.Experience,Is.EqualTo(13));Assert.That(rewards.Gold,Is.EqualTo(4));
            foreach(var drop in Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None)){Assert.That(drop.Item,Is.Not.Null);Assert.That(drop.Count,Is.GreaterThan(0));}
        }
    }
}
