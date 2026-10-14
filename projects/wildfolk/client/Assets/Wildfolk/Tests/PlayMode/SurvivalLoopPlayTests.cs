using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Wildfolk.Tests
{
    public sealed class SurvivalLoopPlayTests
    {
        private PlayPointerDriver input;
        private PlayerMotor player;
        private PlayerInventory inventory;
        private PlayerCrafting crafting;
        private PlayerEquipment equipment;
        private GameplayHud hud;
        private InventoryPanel panel;
        private CraftingPanel crafts;
        private HotbarView hotbar;

        [UnitySetUp] public IEnumerator Load()
        {
            input=new PlayPointerDriver();Time.timeScale=1;
            WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            player=Object.FindFirstObjectByType<PlayerMotor>();inventory=player.GetComponent<PlayerInventory>();
            crafting=player.GetComponent<PlayerCrafting>();equipment=player.GetComponent<PlayerEquipment>();
            hud=Object.FindFirstObjectByType<GameplayHud>();panel=Object.FindFirstObjectByType<InventoryPanel>();
            crafts=Object.FindFirstObjectByType<CraftingPanel>();hotbar=Object.FindFirstObjectByType<HotbarView>();
        }
        [UnityTearDown] public IEnumerator Release()
        {
            input?.Dispose();Time.timeScale=1;yield return null;
        }
        private RecipeDefinition Recipe(string id)
        {
            foreach(var recipe in crafting.Catalog.Recipes) if(recipe.Id==id) return recipe;
            Assert.Fail("Missing recipe: "+id);return null;
        }
        private RecipeCardView Card(string id)
        {
            foreach(var view in crafts.Cards) if(view.Recipe.Id==id) return view;
            Assert.Fail("Missing recipe card: "+id);return null;
        }
        private void Position(Vector2 position,Vector2 facing)
        {
            var body=player.GetComponent<Rigidbody2D>();body.position=position;body.linearVelocity=Vector2.zero;
            player.transform.position=position;
            player.Avatar.Facing.Face(facing);Physics2D.SyncTransforms();
        }
        private CraftingStation SpawnStation()
        {
            var definition=Recipe("workbench").Result.Placement;
            return Object.Instantiate(definition.Prefab,(Vector2)player.transform.position+Vector2.down*2.1f,Quaternion.identity).GetComponent<CraftingStation>();
        }

        [UnityTest] public IEnumerator NewGameStartsWith36EmptySlotsAndOnlyHandCrafting()
        {
            Assert.That(inventory.Model.Capacity,Is.EqualTo(36));
            for(var i=0;i<36;i++) Assert.That(inventory.Model.GetSlot(i).IsEmpty,Is.True);
            Assert.That(equipment.ActiveTool,Is.Null);Assert.That(equipment.HeldVisual.enabled,Is.False);
            Assert.That(hotbar.Slots.Length,Is.EqualTo(9));Assert.That(panel.Slots.Length,Is.EqualTo(36));
            yield return input.Click(hud.InventoryButton);
            Assert.That(Card("workbench").gameObject.activeSelf,Is.True);
            Assert.That(Card("workbench").CraftButton.interactable,Is.False);
            Assert.That(Card("axe").gameObject.activeSelf,Is.False);Assert.That(Card("wood_sword").gameObject.activeSelf,Is.False);
            Assert.That(Card("workbench").Materials.text,Does.Contain("0 / 6"));
        }

        [UnityTest] public IEnumerator InventoryKeepsEnemyAliveWhilePauseHasSeparateWorldPolicy()
        {
            var enemy=WorldTestScene.SpawnZombie();yield return new WaitForSeconds(1.3f);
            Position((Vector2)enemy.transform.position+Vector2.left*2.2f,Vector2.right);
            yield return new WaitForSeconds(.3f);
            var body=player.GetComponent<Rigidbody2D>();var origin=body.position;var hp=player.GetComponent<Health>();
            yield return input.Click(hud.InventoryButton);
            var before=hp.Current;yield return new WaitForSeconds(1.1f);
            Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(hp.Current,Is.LessThan(before));
            Assert.That(Vector2.Distance(origin,body.position),Is.LessThan(.02f));
            yield return input.Click(hud.Attack);Assert.That(player.Combat.IsAttacking,Is.False);
            yield return input.Click(hud.PauseButton);Assert.That(hud.PausePanel.activeSelf,Is.True);Assert.That(Time.timeScale,Is.Zero);
            var pausedHp=hp.Current;yield return new WaitForSecondsRealtime(.35f);Assert.That(hp.Current,Is.EqualTo(pausedHp));
            yield return input.Click(hud.PausePanel.transform.Find("Pause Panel/Continue"));
            Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(hud.Session.State,Is.EqualTo(SessionState.Running));
            hud.Session.Mode=SessionMode.Multiplayer;
            yield return input.Click(hud.PauseButton);Assert.That(Time.timeScale,Is.EqualTo(1));
            Assert.That(hud.Session.State,Is.EqualTo(SessionState.Paused));
            yield return input.Click(hud.PausePanel.transform.Find("Pause Panel/Continue"));
        }

        [UnityTest] public IEnumerator PointerDragMovesMergesSwapsAndRefreshesEquipmentWithoutReopening()
        {
            var wood=Recipe("workbench").Ingredients[0].Item;
            inventory.Model.Add(wood,70);inventory.Model.MoveStack(0,20);
            inventory.Model.Add(Recipe("axe").Result,1);inventory.Model.Add(Recipe("pickaxe").Result,1);
            inventory.SelectSlot(0);yield return null;
            Assert.That(equipment.ActiveTool,Is.SameAs(Recipe("axe").Result.Tool));
            yield return input.Click(hud.InventoryButton);
            var drag=Object.FindFirstObjectByType<InventoryDragController>();
            yield return input.Drag(panel.Slots[20],PlayPointerDriver.Point(panel.Slots[1]),()=>
            {Assert.That(drag.IsDragging,Is.True);Assert.That(drag.Ghost.gameObject.activeSelf,Is.True);Assert.That(panel.Slots[20].Icon.color.a,Is.LessThan(.5f));});
            Assert.That(inventory.Model.GetSlot(1).Count,Is.EqualTo(50));Assert.That(inventory.Model.GetSlot(20).Count,Is.EqualTo(20));
            yield return input.Drag(panel.Slots[20],PlayPointerDriver.Point(panel.Slots[1]));
            Assert.That(inventory.Model.GetSlot(20).Count,Is.EqualTo(20));
            yield return input.Drag(panel.Slots[0],PlayPointerDriver.Point(panel.Slots[21]));
            Assert.That(equipment.ActiveTool,Is.Null);Assert.That(hotbar.Slots[0].Icon.enabled,Is.False);
            yield return input.Drag(panel.Slots[2],PlayPointerDriver.Point(panel.Slots[0]));
            Assert.That(equipment.ActiveTool,Is.SameAs(Recipe("pickaxe").Result.Tool));
            yield return input.Drag(panel.Slots[0],PlayPointerDriver.Point(panel.Slots[1]));
            Assert.That(inventory.Model.GetSlot(0).Item,Is.SameAs(wood));Assert.That(inventory.Model.GetSlot(1).Item.Id,Is.EqualTo("pickaxe"));
            Assert.That(equipment.ActiveTool,Is.Null);
            yield return input.Drag(panel.Slots[21],new Vector2(5,Screen.height*.5f));
            Assert.That(inventory.Model.GetSlot(21).Item.Id,Is.EqualTo("axe"));
            Assert.That(drag.IsDragging,Is.False);Assert.That(drag.Ghost.gameObject.activeSelf,Is.False);
        }

        [UnityTest] public IEnumerator CraftIntoStorageThenDragToHotbarUpdatesPlacementImmediately()
        {
            var wood=Recipe("workbench").Ingredients[0].Item;
            inventory.Model.Add(wood,450);
            yield return input.Click(hud.InventoryButton);yield return input.Click(Card("workbench").CraftButton);
            Assert.That(inventory.Model.CountItem(wood),Is.EqualTo(444));
            Assert.That(inventory.Model.GetSlot(9).Item.Id,Is.EqualTo("workbench"));
            Assert.That(panel.Slots[9].Icon.enabled,Is.True);Assert.That(panel.Slots[9].Count.text,Is.EqualTo("1"));
            yield return input.Drag(panel.Slots[9],PlayPointerDriver.Point(panel.Slots[8]));
            yield return input.Click(panel.Slots[8]);yield return input.Click(hud.InventoryButton);
            yield return null;
            Assert.That(equipment.ActivePlaceable,Is.Not.Null);Assert.That(hotbar.Slots[8].Icon.sprite,Is.SameAs(Recipe("workbench").Result.Icon));
            Assert.That(player.GetComponent<PlayerPlacement>().Preview.activeSelf,Is.True);
        }

        [UnityTest] public IEnumerator WorkbenchRecipesRequireLiveStationWithinRangeAndCannotLoseItems()
        {
            inventory.Model.Add(Recipe("workbench").Ingredients[0].Item,5);
            var recipe=Recipe("axe");
            Assert.That(crafting.GetStatus(recipe),Is.EqualTo(CraftStatus.StationRequired));
            var station=SpawnStation();yield return null;
            Assert.That(crafting.OpenStation(station),Is.True);yield return null;
            Assert.That(Card("axe").CraftButton.interactable,Is.True);
            var outside=new Vector2(3,0);
            Position(outside,((Vector2)station.transform.position-outside).normalized);yield return new WaitForSeconds(.3f);
            Assert.That(player.GetComponent<PlayerPrimaryAction>().Kind,Is.EqualTo(PrimaryActionKind.Attack),"Collider overlap alone cannot grant station interaction.");
            Assert.That(crafting.AvailableStation,Is.EqualTo(StationKind.None));
            Assert.That(Card("axe").gameObject.activeSelf,Is.False);
            Assert.That(crafting.Craft(recipe),Is.EqualTo(CraftStatus.StationRequired));
            Assert.That(inventory.Model.CountItem(Recipe("workbench").Ingredients[0].Item),Is.EqualTo(5));
            hud.Session.CloseInventory();Position(Vector2.zero,Vector2.down);yield return new WaitForSeconds(.3f);
            var live=SpawnStation();yield return null;Assert.That(crafting.OpenStation(live),Is.True);
            live.Health.ApplyDamage(live.Health.Maximum);yield return null;yield return null;
            Assert.That(crafting.AvailableStation,Is.EqualTo(StationKind.None));Assert.That(Card("axe").gameObject.activeSelf,Is.False);
        }

        private IEnumerator Harvest(ResourceNode node)
        {
            WorldTestScene.MoveResource(node,new Vector2(0,3));
            Position(new Vector2(0,1),Vector2.up);
            yield return new WaitForSeconds(.3f);
            var point=PlayPointerDriver.Point(hud.Attack);input.Pointer(point,true);yield return null;
            var timeout=Time.time+8;
            while(node!=null && Time.time<timeout) yield return null;
            input.Pointer(point,false);yield return null;
            Assert.That(node==null,Is.True);
            var pickup=Object.FindFirstObjectByType<WorldPickup>();Assert.That(pickup,Is.Not.Null);
            Position(pickup.transform.position,Vector2.up);yield return new WaitForSeconds(.85f);
            Assert.That(Object.FindFirstObjectByType<WorldPickup>(),Is.Null);
        }
        private static ResourceNode Tree()
        {
            foreach(var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))
                if(node.Definition.Kind==ResourceKind.Wood && node.Health.Maximum==150)return node;
            Assert.Fail("Not enough trees in the saved field.");return null;
        }

        [UnityTest] public IEnumerator BareHandsToPlacedWorkbenchToCraftedToolsAndSwordThroughActualUI()
        {
            yield return Harvest(Tree());
            var wood=Recipe("workbench").Ingredients[0].Item;
            Assert.That(inventory.Model.CountItem(wood),Is.EqualTo(6));
            yield return input.Click(hud.InventoryButton);yield return input.Click(Card("workbench").CraftButton);
            Assert.That(inventory.Model.CountItem(wood),Is.Zero);Assert.That(inventory.Model.GetSlot(0).Item.Id,Is.EqualTo("workbench"));
            yield return input.Click(hud.InventoryButton);
            var placement=player.GetComponent<PlayerPlacement>();
            Position(new Vector2(.7f,1.4f),Vector2.right);yield return new WaitForSeconds(.3f);
            var obstacle=WorldTestScene.MatureTree();WorldTestScene.MoveResource(obstacle,placement.PreviewPosition);yield return null;
            Assert.That(placement.PreviewValid,Is.False);yield return input.Click(hud.Attack);
            Assert.That(inventory.Model.GetSlot(0).Count,Is.EqualTo(1));Assert.That(Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None),Is.Empty);
            Position(new Vector2(0,-.35f),Vector2.down);yield return new WaitForSeconds(.3f);
            Assert.That(placement.PreviewValid,Is.True);
            Assert.That(placement.CanPlaceAt(equipment.ActivePlaceable,new Vector2(20,20)),Is.False);
            yield return input.Click(hud.Attack);yield return null;
            Assert.That(inventory.Model.GetSlot(0).IsEmpty,Is.True);
            var bench=Object.FindFirstObjectByType<CraftingStation>();Assert.That(bench,Is.Not.Null);
            yield return input.Click(hud.Interact);
            Assert.That(crafting.AvailableStation,Is.EqualTo(StationKind.Workbench));Assert.That(panel.View.activeSelf,Is.True);
            yield return input.Click(hud.InventoryButton);yield return input.Click(hotbar.Slots[8]);
            for(var i=0;i<3;i++)yield return Harvest(Tree());
            Assert.That(inventory.Model.CountItem(wood),Is.EqualTo(18));
            Position(new Vector2(0,-.35f),Vector2.down);yield return new WaitForSeconds(.3f);
            yield return input.Click(hud.Interact);
            yield return input.Click(Card("axe").CraftButton);yield return input.Click(Card("pickaxe").CraftButton);yield return input.Click(Card("wood_sword").CraftButton);
            Assert.That(inventory.Model.CountItem(wood),Is.EqualTo(3));
            Assert.That(inventory.Model.CountItem(Recipe("axe").Result),Is.EqualTo(1));
            Assert.That(inventory.Model.CountItem(Recipe("pickaxe").Result),Is.EqualTo(1));
            Assert.That(inventory.Model.GetSlot(3).Item.Id,Is.EqualTo("wood_sword"));
            yield return input.Click(panel.Slots[3]);yield return input.Click(hud.InventoryButton);
            Assert.That(equipment.ActiveTool,Is.SameAs(Recipe("wood_sword").Result.Tool));Assert.That(equipment.HeldVisual.enabled,Is.True);
            var enemy=WorldTestScene.SpawnZombie();yield return new WaitForSeconds(1.3f);Position((Vector2)enemy.transform.position+Vector2.left*1.9f,Vector2.right);
            yield return new WaitForSeconds(.3f);
            var point=PlayPointerDriver.Point(hud.Attack);input.Pointer(point,true);yield return null;
            var timeout=Time.time+4;
            while(enemy!=null && Time.time<timeout) yield return null;
            input.Pointer(point,false);yield return null;
            Assert.That(enemy==null,Is.True);Assert.That(player.GetComponent<Health>().IsAlive,Is.True);
            Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None),Is.Empty);
        }
    }
}
