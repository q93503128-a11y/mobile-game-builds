using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class MetalProgressionPlayTests
    {
        private WorldSaveService save;private PlayerInventory player;private PlayerCrafting crafting;
        [UnitySetUp]public IEnumerator Load()
        {
            Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=1;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;player=save.Player;crafting=player.GetComponent<PlayerCrafting>();
            save.Clock.enabled=save.World.Animals.enabled=save.World.Renewal.enabled=false;save.World.GetComponent<GoblinSpawner>().enabled=false;save.World.GetComponent<ZombieSpawner>().enabled=false;
            player.GetComponent<PlayerInputAdapter>().enabled=false;player.GetComponent<PlayerMotor>().enabled=false;
        }
        [UnityTearDown]public IEnumerator Done(){Time.timeScale=1;yield return null;}
        private void Position(Vector2 p){player.GetComponent<Rigidbody2D>().position=p;player.transform.position=p;player.GetComponent<CharacterFacing>().Face(Vector2.up);Physics2D.SyncTransforms();}
        private CraftingStation Place(string id,Vector2 p)=>Object.Instantiate(save.Catalog.Item(id).Placement.Prefab,p,Quaternion.identity).GetComponent<CraftingStation>();
        [UnityTest]public IEnumerator RejectedMiningHasZeroDamageProgressLootOrSuccessSignalsAndValidToolHitsOnce()
        {
            var node=Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).First(r=>r.Definition.Kind==ResourceKind.TinOre);WorldTestScene.MoveResource(node,new Vector2(0,1.4f));Position(Vector2.zero);
            var damageSignals=0;var deaths=0;node.Health.Damaged+=()=>damageSignals++;node.Health.Died+=()=>deaths++;
            var health=node.Health.Current;var before=Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None).Length;var weak=save.Catalog.Item("pickaxe");player.Model.Add(weak,1);player.SelectSlot(0);
            Assert.That(node.Harvest(weak.Tool,99,player),Is.Zero);Assert.That(node.Harvest(null,99,player),Is.Zero);Assert.That(node.Harvest(save.Catalog.Item("iron_sword").Tool,99,player),Is.Zero);
            var combat=player.GetComponent<PlayerCombat>();var denied=0;combat.HarvestDenied+=r=>denied++;
            Assert.That(combat.TryAttack(),Is.True);yield return new WaitForSeconds(.8f);
            Assert.That(node.Health.Current,Is.EqualTo(health));Assert.That(combat.LastHitCount,Is.Zero);Assert.That(combat.LastDamage,Is.Zero);Assert.That(damageSignals,Is.Zero);Assert.That(denied,Is.EqualTo(1));Assert.That(save.World.Renewal.Capture(),Is.Empty);Assert.That(Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None).Length,Is.EqualTo(before));
            var pick=save.Catalog.Item("prospecting_pan");player.Model.Add(pick,1);player.SelectSlot(1);Assert.That(combat.TryAttack(),Is.True);yield return new WaitForSeconds(1.3f);
            Assert.That(combat.LastHitCount,Is.EqualTo(1));Assert.That(node.Health.Current,Is.EqualTo(health-pick.Tool.DamageFor(ResourceKind.TinOre)));Assert.That(damageSignals,Is.EqualTo(1));
            var drop=node.Definition.DropItem;node.Harvest(pick.Tool,1000,player);node.Harvest(pick.Tool,1000,player);Assert.That(deaths,Is.EqualTo(1));yield return null;Assert.That(Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None).Count(p=>p.Item==drop),Is.EqualTo(1));
        }
        [UnityTest]public IEnumerator UpgradeUiScrollsArbitraryCostsAndPreservesTierAndMetalsAcrossSnapshotLoad()
        {
            var bench=Place("workbench",new Vector2(0,-2));yield return null;Position(Vector2.zero);Assert.That(crafting.OpenStation(bench),Is.True);
            var panel=Object.FindFirstObjectByType<CraftingPanel>();panel.Refresh();Assert.That(panel.Title.text,Does.Contain("I"));Assert.That(panel.UpgradeButton.interactable,Is.False);Assert.That(crafting.UpgradeWorkbench(),Is.False);
            foreach(var c in bench.Definition.UpgradeCosts)player.Model.Add(c.Item,c.Count);player.Model.Add(save.Catalog.Item("bronze"),5);player.Model.Add(save.Catalog.Item("steel"),4);yield return null;
            var original=bench.Definition;var clone=Object.Instantiate(original);clone.UpgradeCosts=original.UpgradeCosts.Concat(new[]{new ItemAmount{Item=save.Catalog.Item("bronze"),Count=1},new ItemAmount{Item=save.Catalog.Item("steel"),Count=1}}).ToArray();bench.Definition=clone;
            panel.Refresh();Canvas.ForceUpdateCanvases();Assert.That(panel.UpgradeRequirements.text.Split('\n').Length,Is.EqualTo(5));Assert.That(panel.UpgradeScroll.content.rect.height,Is.GreaterThan(panel.UpgradeScroll.viewport.rect.height));Assert.That(panel.UpgradeButton.interactable,Is.True);
            var stable=bench.GetComponent<WorldPlaceable>().SaveId;Assert.That(crafting.UpgradeWorkbench(),Is.True);Assert.That(crafting.UpgradeWorkbench(),Is.False);yield return null;
            Assert.That(crafting.Station.Definition.WorkbenchTier,Is.EqualTo(2));panel.Refresh();Assert.That(panel.UpgradeButton.gameObject.activeSelf,Is.False);Assert.That(panel.UpgradeScroll.gameObject.activeSelf,Is.False);
            var destroyed=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).First(r=>r.Rule==5);var destroyedId=destroyed.Id;destroyed.Node.Harvest(save.Catalog.Item("iron_pickaxe").Tool,10000,player);yield return null;
            var snapshot=save.Capture();save.Validate(snapshot);save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));yield return null;
            Assert.That(Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None).Single(p=>p.SaveId==stable).Definition.WorkbenchTier,Is.EqualTo(2));Assert.That(player.Model.CountItem(save.Catalog.Item("bronze")),Is.EqualTo(4));Assert.That(player.Model.CountItem(save.Catalog.Item("steel")),Is.EqualTo(3));Assert.That(Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Any(r=>r.Id==destroyedId),Is.False);Assert.That(save.World.Renewal.Capture().Single(r=>r.Id==destroyedId).Destroyed,Is.True);
            Object.Destroy(clone);
        }
        [UnityTest]public IEnumerator BasicRejectsSteelAdvancedNeedsNoNearbyBenchAndCargoSurvivesLoad()
        {
            var basic=Place("furnace",new Vector2(0,-2));var advanced=Place("blast_furnace",new Vector2(2,-2));yield return null;
            var process=advanced.GetComponent<StationProcessing>();process.enabled=false;var steel=save.Catalog.Recipe("steel");foreach(var c in steel.Ingredients)player.Model.Add(c.Item,c.Count*3);
            Assert.That(basic.GetComponent<StationProcessing>().Add(steel,player,1),Is.False);Assert.That(process.Add(steel,player,2),Is.True);process.Advance(2);
            Assert.That(process.Add(save.Catalog.Recipe("cooked_meat"),player,1),Is.False);Assert.That(basic.GetComponent<StationProcessing>().Add(save.Catalog.Recipe("cooked_meat"),player,1),Is.False);
            var id=advanced.GetComponent<WorldPlaceable>().SaveId;var snapshot=save.Capture();save.Validate(snapshot);save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));
            foreach(var restored in Object.FindObjectsByType<StationProcessing>(FindObjectsSortMode.None))restored.enabled=false;yield return null;
            process=Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None).Single(p=>p.SaveId==id).GetComponent<StationProcessing>();
            Assert.That(process.Recipe.Id,Is.EqualTo("steel"));Assert.That(process.Queue.WaitingCount,Is.EqualTo(1));process.Advance(20);Assert.That(process.OutputCount,Is.EqualTo(2));Assert.That(process.Collect(player),Is.True);Assert.That(player.Model.CountItem(steel.Result),Is.EqualTo(2));
        }
        [UnityTest]public IEnumerator ChestScrollWheelTouchAndBottomSlotDragPreserveContents()
        {
            var chest=Object.Instantiate(save.Catalog.Item("reinforced_chest").Placement.Prefab,new Vector2(0,-2),Quaternion.identity).GetComponent<StorageChest>();yield return null;Position(Vector2.zero);
            var stacks=Enumerable.Range(0,36).Select(i=>new SavedStack()).ToArray();stacks[35]=new SavedStack{Item="wood",Count=7};chest.Model.Restore(stacks,save.Catalog);Assert.That(player.GetComponent<PlayerStorage>().OpenChest(chest),Is.True);yield return null;Canvas.ForceUpdateCanvases();
            var ui=Object.FindFirstObjectByType<ChestPanel>();Assert.That(ui.ChestSlots.Count(v=>v.gameObject.activeSelf),Is.EqualTo(36));Assert.That(ui.Scroll.content.rect.height,Is.GreaterThan(ui.Scroll.viewport.rect.height));
            Assert.That(ui.Scroll.verticalScrollbar.handleRect.rect.height,Is.LessThanOrEqualTo(ui.Scroll.viewport.rect.height));var wheel=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){scrollDelta=new Vector2(0,-10)};ui.Scroll.OnScroll(wheel);Canvas.ForceUpdateCanvases();Assert.That(ui.Scroll.verticalNormalizedPosition,Is.LessThan(1));ui.Scroll.verticalScrollbar.value=0;yield return null;
            var slot=ui.ChestSlots[35];var handler=slot.GetComponent<InventorySlotDrag>();var point=RectTransformUtility.WorldToScreenPoint(null,slot.transform.position);var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=-1,position=point,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};handler.OnPointerDown(data);data.position+=Vector2.right*40;handler.OnBeginDrag(data);Assert.That(ui.Drag.IsDragging,Is.True);ui.PlayerSlots[20].GetComponent<InventorySlotDrag>().OnDrop(data);handler.OnEndDrag(data);Assert.That(player.Model.GetSlot(20).Count,Is.EqualTo(7));Assert.That(chest.Model.GetSlot(35).IsEmpty,Is.True);
            stacks[0]=new SavedStack{Item="wood",Count=3};stacks[35]=new SavedStack();chest.Model.Restore(stacks,save.Catalog);ui.Scroll.verticalNormalizedPosition=1;yield return null;var source=ui.ChestSlots[0].GetComponent<InventorySlotDrag>();point=RectTransformUtility.WorldToScreenPoint(null,source.transform.position);var touch=new UnityEngine.InputSystem.UI.ExtendedPointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=1,pointerType=UnityEngine.InputSystem.UI.UIPointerType.Touch,position=point,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};source.OnPointerDown(touch);touch.position+=Vector2.up*30;source.OnBeginDrag(touch);touch.position+=Vector2.up*100;source.OnDrag(touch);source.OnEndDrag(touch);Assert.That(ui.Drag.IsDragging,Is.False);Assert.That(chest.Model.GetSlot(0).Count,Is.EqualTo(3));Assert.That(ui.Scroll.verticalNormalizedPosition,Is.LessThan(1));
        }
    }
}
