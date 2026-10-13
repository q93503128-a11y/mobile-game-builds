using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class ProgressionPlayTests
    {
        private PlayPointerDriver input;
        private PlayerMotor player;
        private PlayerInventory inventory;
        private PlayerCrafting crafting;
        private GameplayHud hud;
        private WorldClock clock;
        [UnitySetUp] public IEnumerator Load()
        {
            input=new PlayPointerDriver();Time.timeScale=1;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            player=Object.FindFirstObjectByType<PlayerMotor>();inventory=player.GetComponent<PlayerInventory>();crafting=player.GetComponent<PlayerCrafting>();hud=Object.FindFirstObjectByType<GameplayHud>();clock=Object.FindFirstObjectByType<WorldClock>();
        }
        [UnityTearDown] public IEnumerator Release(){input.Dispose();Time.timeScale=1;yield return null;}
        private RecipeDefinition Recipe(string id){foreach(var r in crafting.Catalog.Recipes)if(r.Id==id)return r;Assert.Fail(id);return null;}
        private RecipeCardView Card(string id){foreach(var c in Object.FindFirstObjectByType<CraftingPanel>().Cards)if(c.Recipe.Id==id)return c;Assert.Fail(id);return null;}
        private void Position(Vector2 position,Vector2 facing){player.GetComponent<Rigidbody2D>().position=position;player.transform.position=position;player.Avatar.Facing.Face(facing);Physics2D.SyncTransforms();}
        private WorldPlaceable Place(string id,Vector2 position) => Object.Instantiate(Recipe(id).Result.Placement.Prefab,position,Quaternion.identity);
        [UnityTest] public IEnumerator ChestUITransfersMergesSwapsAndRefreshesEquippedHotbar()
        {
            var chest=Place("storage_chest",new Vector2(0,-2.45f)).GetComponent<StorageChest>();var other=Place("storage_chest",new Vector2(4,-2.45f)).GetComponent<StorageChest>();yield return null;
            inventory.Model.Add(Recipe("wood_sword").Result,1);inventory.Model.Add(Recipe("workbench").Ingredients[0].Item,20);
            chest.Model.Add(Recipe("workbench").Ingredients[0].Item,45);inventory.SelectSlot(0);yield return null;Position(new Vector2(0,-.35f),Vector2.down);yield return null;
            yield return input.Click(hud.Attack);var panel=Object.FindFirstObjectByType<ChestPanel>();Assert.That(panel.View.activeSelf,Is.True);Assert.That(Time.timeScale,Is.EqualTo(1));
            Assert.That(Object.FindFirstObjectByType<InventoryPanel>().View.activeSelf,Is.False);
            yield return input.Drag(panel.PlayerSlots[1],PlayPointerDriver.Point(panel.ChestSlots[0]));Assert.That(chest.Model.GetSlot(0).Count,Is.EqualTo(50));Assert.That(inventory.Model.GetSlot(1).Count,Is.EqualTo(15));
            yield return input.Drag(panel.PlayerSlots[0],PlayPointerDriver.Point(panel.ChestSlots[1]));Assert.That(player.GetComponent<PlayerEquipment>().ActiveTool,Is.Null);
            yield return input.Drag(panel.ChestSlots[1],PlayPointerDriver.Point(panel.PlayerSlots[1]));Assert.That(inventory.Model.GetSlot(1).Item.Id,Is.EqualTo("wood_sword"));Assert.That(chest.Model.GetSlot(1).Count,Is.EqualTo(15));
            yield return input.Drag(panel.ChestSlots[0],new Vector2(5,Screen.height*.5f));Assert.That(chest.Model.GetSlot(0).Count,Is.EqualTo(50));Assert.That(other.Model.GetSlot(0).IsEmpty,Is.True);
            yield return input.Click(hud.InventoryButton);Assert.That(hud.Session.IsRunning,Is.True);
            Assert.That(player.GetComponent<PlayerStorage>().OpenChest(chest),Is.True);yield return null;Assert.That(panel.ChestSlots[0].Count.text,Is.EqualTo("50"));
            chest.Placeable.Health.ApplyDamage(chest.Placeable.Health.Maximum);yield return null;yield return null;yield return null;
            Assert.That(panel.View.activeSelf,Is.False);Assert.That(hud.Session.IsRunning,Is.True);Assert.That(Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None).Length,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator DoorUseOpensClosesAndRejectsClosingOnAnActor()
        {
            var door=Place("wood_door",new Vector2(0,-2.45f)).GetComponent<WorldDoor>();yield return null;Position(new Vector2(0,-.35f),Vector2.down);yield return null;
            yield return input.Click(hud.Attack);Assert.That(door.IsOpen,Is.True);Assert.That(door.Barrier.enabled,Is.False);
            yield return input.Click(hud.Attack);Assert.That(door.IsOpen,Is.False);Assert.That(door.Barrier.enabled,Is.True);
            yield return input.Click(hud.Attack);Position(door.transform.position,Vector2.down);yield return null;
            Assert.That(door.Toggle(),Is.False);Assert.That(door.IsOpen,Is.True);
            Position(new Vector2(0,-.35f),Vector2.down);yield return null;Assert.That(door.Toggle(),Is.True);
        }
        [UnityTest] public IEnumerator ZombieIsBlockedByWallAndDealsVisibleHealthDamage()
        {
            var enemy=Object.FindFirstObjectByType<EnemyController>();Position(Vector2.zero,Vector2.right);
            enemy.GetComponent<Rigidbody2D>().position=new Vector2(2.5f,0);enemy.transform.position=new Vector2(2.5f,0);
            var wall=Place("wood_wall",new Vector2(1.3f,0));wall.transform.rotation=Quaternion.Euler(0,0,90);Physics2D.SyncTransforms();yield return null;
            var hp=player.GetComponent<Health>().Current;yield return new WaitForSeconds(1.4f);
            Assert.That(wall.Health.Current,Is.LessThan(wall.Health.Maximum));Assert.That(player.GetComponent<Health>().Current,Is.EqualTo(hp));Assert.That(enemy.transform.position.x,Is.GreaterThan(wall.transform.position.x));
            wall.Health.ApplyDamage(wall.Health.Maximum);yield return new WaitForSeconds(1.6f);Assert.That(player.GetComponent<Health>().Current,Is.LessThan(hp));
        }
        [UnityTest] public IEnumerator NightSpawnUsesSameZombieOutsideCameraAndStopsAtCap()
        {
            var spawner=Object.FindFirstObjectByType<ZombieSpawner>();Assert.That(spawner.TrySpawn(),Is.False);
            clock.Model.Advance(clock.Definition.DaySeconds+clock.Definition.EveningSeconds);yield return null;
            for(var i=0;i<spawner.Definition.NightLimit-1;i++)Assert.That(spawner.TrySpawn(),Is.True);
            Assert.That(spawner.ActiveCount,Is.EqualTo(spawner.Definition.NightLimit));Assert.That(spawner.TrySpawn(),Is.False);
            foreach(var enemy in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            {
                if(Vector2.Distance(enemy.transform.position,new Vector2(6.6f,-.2f))<1)continue;
                Assert.That(spawner.IsValidPosition(enemy.transform.position),Is.True);Assert.That(enemy.Clock,Is.SameAs(clock));
            }
            Assert.That(spawner.IsValidPosition(player.transform.position),Is.False);
            hud.Session.OpenPause();var before=clock.Model.Elapsed;yield return new WaitForSecondsRealtime(.2f);Assert.That(clock.Model.Elapsed,Is.EqualTo(before));
            hud.Session.Resume();hud.Session.Mode=SessionMode.Multiplayer;hud.Session.OpenPause();before=clock.Model.Elapsed;yield return new WaitForSeconds(.2f);Assert.That(clock.Model.Elapsed,Is.GreaterThan(before));
        }
        [UnityTest] public IEnumerator OreRequiresStonePickaxeAndDropsRealOreAfterValidHits()
        {
            ResourceNode ore=null;foreach(var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(node.Definition.Kind==ResourceKind.IronOre){ore=node;break;}
            ore.transform.position=new Vector2(0,3);Position(new Vector2(0,1),Vector2.up);Physics2D.SyncTransforms();yield return null;
            var hp=ore.Health.Current;yield return input.Click(hud.Attack);yield return new WaitForSeconds(.5f);Assert.That(ore.Health.Current,Is.EqualTo(hp));
            inventory.Model.Add(Recipe("pickaxe").Result,1);inventory.Model.Add(Recipe("stone_pickaxe").Result,1);inventory.Model.Add(Recipe("iron_pickaxe").Result,1);
            inventory.SelectSlot(0);yield return null;yield return input.Click(hud.Attack);yield return new WaitForSeconds(.8f);Assert.That(ore.Health.Current,Is.EqualTo(hp));
            inventory.SelectSlot(1);yield return null;yield return input.Click(hud.Attack);yield return new WaitForSeconds(.65f);Assert.That(ore.Health.Current,Is.EqualTo(hp-30));
            inventory.SelectSlot(2);yield return null;yield return input.Click(hud.Attack);yield return new WaitForSeconds(.55f);Assert.That(ore==null,Is.True);
            var drop=Object.FindFirstObjectByType<WorldPickup>();Assert.That(drop.Item.Id,Is.EqualTo("iron_ore"));Assert.That(drop.Count,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator FurnaceSmeltsWithProgressAndWorkbenchMakesIronEquipment()
        {
            var furnace=Place("furnace",new Vector2(0,-2.45f)).GetComponent<CraftingStation>();yield return null;var smelt=Recipe("iron_ingot");
            inventory.Model.Add(smelt.Ingredients[0].Item,1);Assert.That(crafting.OpenStation(furnace),Is.True);yield return null;
            Assert.That(Card("cooked_meat").gameObject.activeSelf,Is.False);yield return input.Click(Card("iron_ingot").CraftButton);
            var processor=furnace.GetComponent<StationProcessing>();Assert.That(processor.IsBusy,Is.True);Assert.That(inventory.Model.CountItem(smelt.Ingredients[0].Item),Is.Zero);
            yield return new WaitForSeconds(.3f);Assert.That(Card("iron_ingot").Status.text,Does.Contain("Smelting"));
            yield return new WaitForSeconds(6.1f);yield return input.Click(Card("iron_ingot").CraftButton);Assert.That(inventory.Model.CountItem(smelt.Result),Is.EqualTo(1));
            hud.Session.CloseInventory();Object.Destroy(furnace.gameObject);yield return null;var bench=Place("workbench",new Vector2(0,-2.45f)).GetComponent<CraftingStation>();yield return null;
            var iron=Recipe("iron_sword");foreach(var cost in iron.Ingredients)inventory.Model.Add(cost.Item,cost.Count);Assert.That(crafting.OpenStation(bench),Is.True);Assert.That(crafting.Craft(iron),Is.EqualTo(CraftStatus.Ready));
            Assert.That(inventory.Model.CountItem(iron.Result),Is.EqualTo(1));Assert.That(iron.Result.Tool.Tier.Rank,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator RecipeScrollCanReachAndCraftStorageChestThroughPointerInput()
        {
            var bench=Place("workbench",new Vector2(0,-2.45f)).GetComponent<CraftingStation>();yield return null;
            foreach(var cost in Recipe("storage_chest").Ingredients)inventory.Model.Add(cost.Item,cost.Count);
            Assert.That(crafting.OpenStation(bench),Is.True);yield return null;yield return null;
            var panel=Object.FindFirstObjectByType<CraftingPanel>();var scroll=panel.Scroll;
            Assert.That(scroll.content.rect.height,Is.GreaterThan(scroll.viewport.rect.height));
            yield return input.Drag(scroll,PlayPointerDriver.Point(scroll)+Vector2.up*220);
            Assert.That(scroll.verticalNormalizedPosition,Is.LessThan(1));
            scroll.verticalNormalizedPosition=0;yield return null;yield return null;
            yield return input.Click(Card("storage_chest").CraftButton);Assert.That(inventory.Model.CountItem(Recipe("storage_chest").Result),Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ChickenWandersFleesOnApproachAndDoesNotAttack()
        {
            NeutralAnimal chicken=null;foreach(var animal in Object.FindObjectsByType<NeutralAnimal>(FindObjectsSortMode.None))if(animal.name.Contains("Chicken"))chicken=animal;
            Assert.That(chicken,Is.Not.Null);Assert.That(chicken.GetComponent<EnemyController>(),Is.Null);
            var start=chicken.transform.position;yield return new WaitForSeconds(.4f);Assert.That(Vector2.Distance(start,chicken.transform.position),Is.GreaterThan(.05f));
            Position((Vector2)chicken.transform.position+Vector2.down*1.3f,Vector2.up);yield return null;yield return input.Click(hud.Attack);yield return new WaitForSeconds(.25f);
            Assert.That(chicken.IsFleeing,Is.True);Assert.That(chicken.Health.Current,Is.LessThan(chicken.Health.Maximum));
            var hp=player.GetComponent<Health>().Current;yield return new WaitForSeconds(.4f);Assert.That(player.GetComponent<Health>().Current,Is.EqualTo(hp));
            chicken.Health.ApplyDamage(chicken.Health.Maximum);yield return null;var drop=Object.FindFirstObjectByType<WorldPickup>();Assert.That(drop.Item.Id,Is.EqualTo("raw_meat"));Assert.That(drop.Count,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ArmorFoundationEquipsOneSlotAndSurvivalDamageBypassesDefense()
        {
            var armor=ScriptableObject.CreateInstance<ArmorDefinition>();var item=ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                armor.Defense=3;armor.WornSprite=Recipe("workbench").Result.Icon;item.Id="armor_fixture";item.DisplayName="Armor";item.MaxStack=1;item.Category=ItemCategory.Armor;item.Armor=armor;item.Icon=armor.WornSprite;
                inventory.Model.Add(item,1);inventory.SelectSlot(0);yield return null;
                var equipment=player.GetComponent<PlayerEquipment>();Assert.That(equipment.TryEquipSelectedArmor(),Is.True);Assert.That(inventory.Model.CountItem(item),Is.Zero);Assert.That(equipment.Defense,Is.EqualTo(3));Assert.That(equipment.ArmorVisual.enabled,Is.True);
                var health=player.GetComponent<Health>();var before=health.Current;health.ApplyDamage(8);Assert.That(before-health.Current,Is.EqualTo(5));before=health.Current;health.ApplyDamage(2,true);Assert.That(before-health.Current,Is.EqualTo(2));
            }
            finally {Object.Destroy(armor);Object.Destroy(item);}
        }
    }
}
