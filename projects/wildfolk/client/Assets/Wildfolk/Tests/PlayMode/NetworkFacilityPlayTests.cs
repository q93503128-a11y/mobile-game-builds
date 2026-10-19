using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Wildfolk.Tests
{
    // One real Local Host, two server-owned fixture actors. No remote/Relay claim.
    public sealed class NetworkFacilityPlayTests
    {
        private NetworkPlayer player,other;
        private NetworkCreatureWorld world;
        private NetworkFacilityInteractor A=>player.GetComponent<NetworkFacilityInteractor>();
        private NetworkFacilityInteractor B=>other.GetComponent<NetworkFacilityInteractor>();
        private InventoryModel Bag(NetworkPlayer actor)=>actor.GetComponent<PlayerInventory>().Model;
        [UnitySetUp]public IEnumerator Host()
        {
            Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();
            var session=NetworkSession.Instance;session.NewWorldGenerationVersion=1;session.WorldRepository=new SaveRepository(System.IO.Path.Combine(Application.temporaryCachePath,"lowfire-net-"+System.Guid.NewGuid().ToString("N"),"world.json"));session.ProfileOverride=System.Guid.NewGuid().ToString("N");var task=session.Host(ConnectionRoute.LocalNetwork);for(var i=0;i<300 && !task.IsCompleted;i++)yield return null;Assert.That(task.Result,Is.True);Assert.That(session.EnterGame(),Is.True);
            for(var i=0;i<300;i++){yield return new WaitForSecondsRealtime(.02f);player=Object.FindFirstObjectByType<NetworkPlayer>();if(player!=null && player.IsSpawned)break;}
            Assert.That(player,Is.Not.Null);world=NetworkCreatureWorld.Active;world.Clock.enabled=world.Animals.enabled=world.Goblins.enabled=world.Night.enabled=world.World.Renewal.enabled=false;
            other=Object.Instantiate(session.PlayerPrefab,player.transform.position,Quaternion.identity).GetComponent<NetworkPlayer>();other.NetworkObject.Spawn();
            foreach(var actor in new[]{player,other}){actor.GetComponent<PlayerInputAdapter>().enabled=false;actor.GetComponent<NetworkInventory>().enabled=false;actor.Motor.enabled=false;actor.GetComponent<PlayerInteraction>().enabled=false;actor.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;}
            yield return null;yield return null;
            Assert.That(Object.FindObjectsByType<NetworkFacility>(FindObjectsSortMode.None).Length,Is.EqualTo(5));
        }
        private NetworkFacility Facility(string name)
        {var definition=world.Prefabs.Single(e=>e.Source.name==name).Source.GetComponent<WorldPlaceable>().Definition;return Object.FindObjectsByType<NetworkFacility>(FindObjectsSortMode.None).Single(f=>f.GetComponent<WorldPlaceable>().Definition==definition);}
        private void Place(NetworkPlayer actor,Vector2 position){actor.GetComponent<Rigidbody2D>().position=position;actor.transform.position=position;Physics2D.SyncTransforms();}
        private void Open(NetworkFacility facility){foreach(var actor in new[]{player,other}){Place(actor,(Vector2)facility.transform.position+Vector2.down);actor.Commands.Session.CloseInventory();Assert.That(actor.GetComponent<NetworkFacilityInteractor>().Open(facility),Is.True);}}
        private bool Do(NetworkFacilityInteractor actor,FacilityOperation op,NetworkFacility f,RecipeDefinition recipe=null,int from=0,int to=0,int amount=0,byte sides=0,uint? bagRevision=null,uint? facilityRevision=null)
            =>actor.Execute(op,f==null ? ulong.MaxValue : f.NetworkObjectId,bagRevision??actor.Bag.Revision.Value,facilityRevision??(f==null ? 0 : f.Revision.Value),recipe==null ? -1 : Array.IndexOf(world.Catalog.Recipes,recipe),from,to,amount,sides);
        [UnityTest]public IEnumerator ChestWithdrawDepositSplitStaleAndFullBagHaveNoLoss()
        {
            var f=Facility("StorageChest");Open(f);var wood=world.Catalog.Item("wood");f.Chest.Model.Add(wood,10);var revision=f.Revision.Value;
            Assert.That(Do(A,FacilityOperation.Transfer,f,from:0,to:0,sides:1,facilityRevision:revision),Is.True);
            Assert.That(Do(B,FacilityOperation.Transfer,f,from:0,to:0,sides:1,facilityRevision:revision),Is.False);Assert.That(Bag(player).CountItem(wood)+Bag(other).CountItem(wood),Is.EqualTo(10));
            Bag(other).Add(wood,8);revision=f.Revision.Value;
            Assert.That(Do(A,FacilityOperation.Transfer,f,from:0,to:0,amount:4,sides:2,facilityRevision:revision),Is.True);
            Assert.That(Do(B,FacilityOperation.Transfer,f,from:0,to:0,amount:3,sides:2,facilityRevision:revision),Is.False);
            Assert.That(Do(B,FacilityOperation.Transfer,f,from:0,to:0,amount:3,sides:2),Is.True);Assert.That(f.Chest.Model.GetSlot(0).Count,Is.EqualTo(7));
            Bag(other).Add(wood,wood.MaxStack*36);var count=f.Chest.Model.CountItem(wood);revision=f.Revision.Value;
            Assert.That(Do(B,FacilityOperation.Transfer,f,from:0,to:0,amount:1,sides:1),Is.False);Assert.That(f.Chest.Model.CountItem(wood),Is.EqualTo(count));Assert.That(f.Revision.Value,Is.EqualTo(revision));
            var total=Bag(player).CountItem(wood)+Bag(other).CountItem(wood)+f.Chest.Model.CountItem(wood);
            Assert.That(Do(A,FacilityOperation.Transfer,f,from:0,to:1,amount:2,sides:3),Is.True);Assert.That(Do(B,FacilityOperation.Transfer,f,from:0,to:2,amount:2,sides:3,facilityRevision:revision),Is.False);
            Assert.That(Bag(player).CountItem(wood)+Bag(other).CountItem(wood)+f.Chest.Model.CountItem(wood),Is.EqualTo(total));
            Place(other,(Vector2)f.transform.position+Vector2.right*20);Assert.That(Do(B,FacilityOperation.Transfer,f,from:0,to:2,sides:1),Is.False);
            Assert.That(f.Slots.Count,Is.EqualTo(24));Assert.That(Facility("ReinforcedChest").Slots.Count,Is.EqualTo(36));yield return null;
        }
        [UnityTest]public IEnumerator HandCraftAtomicityAndWorkbenchUpgradeCommitOnce()
        {
            var cup=world.Catalog.Recipes.Single(r=>r.RequiredStation==StationKind.None && r.Result.Id=="empty_cup");Bag(player).Add(cup.Ingredients[0].Item,cup.Ingredients[0].Count);player.Commands.Session.OpenInventory();var rev=A.Bag.Revision.Value;
            Assert.That(Do(A,FacilityOperation.Craft,null,cup,bagRevision:rev),Is.True);Assert.That(Do(A,FacilityOperation.Craft,null,cup,bagRevision:rev),Is.False);Assert.That(Bag(player).CountItem(cup.Result),Is.EqualTo(cup.ResultCount));Assert.That(Bag(player).CountItem(cup.Ingredients[0].Item),Is.Zero);
            var f=Facility("Workbench");Open(f);var definition=f.Station.Definition;foreach(var cost in definition.UpgradeCosts){Bag(player).Add(cost.Item,cost.Count);Bag(other).Add(cost.Item,cost.Count);}rev=f.Revision.Value;
            Assert.That(Do(A,FacilityOperation.Upgrade,f,facilityRevision:rev),Is.True);Assert.That(Do(B,FacilityOperation.Upgrade,f,facilityRevision:rev),Is.False);Assert.That(Do(B,FacilityOperation.Upgrade,f),Is.False);Assert.That(f.Station.Definition.WorkbenchTier,Is.EqualTo(2));
            foreach(var cost in definition.UpgradeCosts)Assert.That(Bag(other).CountItem(cost.Item),Is.EqualTo(cost.Count));
            var recipe=world.Catalog.Recipes.First(r=>r.RequiredStation==StationKind.Workbench && r.RequiredWorkbenchTier==2);foreach(var cost in recipe.Ingredients)Bag(player).Add(cost.Item,cost.Count);
            Assert.That(Do(A,FacilityOperation.Craft,f,recipe),Is.True);Assert.That(Bag(player).CountItem(recipe.Result),Is.EqualTo(recipe.ResultCount));
            Assert.That(Do(A,FacilityOperation.Craft,f,cup),Is.False);yield return null;
        }
        [UnityTest]public IEnumerator SteelUsesSeparateHostFacilityAndReservationsCommitOnce()
        {
            var furnace=Facility("Furnace");var bench=Facility("Workbench");furnace.transform.position=bench.transform.position+Vector3.right*3;furnace.Processor.enabled=false;
            var recipe=world.Catalog.Recipe("steel");foreach(var c in recipe.Ingredients)Bag(player).Add(c.Item,c.Count*2);Open(furnace);var revision=furnace.Revision.Value;
            Assert.That(Do(A,FacilityOperation.AddOne,furnace,recipe),Is.False);Assert.That(furnace.Revision.Value,Is.EqualTo(revision));foreach(var c in recipe.Ingredients)Assert.That(Bag(player).CountItem(c.Item),Is.EqualTo(c.Count*2));
            Open(bench);foreach(var c in bench.Station.Definition.UpgradeCosts)Bag(player).Add(c.Item,c.Count);Assert.That(Do(A,FacilityOperation.Upgrade,bench),Is.True);Assert.That(bench.Tier.Value,Is.EqualTo(2));
            Assert.That(furnace.Processor.Add(recipe,A.Bag.Inventory,1),Is.False);furnace=world.Create(world.Catalog.Item("blast_furnace").Placement.Prefab,bench.transform.position+Vector3.right*25,Quaternion.identity).GetComponent<NetworkFacility>();furnace.Processor.enabled=false;yield return null;Open(furnace);revision=furnace.Revision.Value;Assert.That(Do(A,FacilityOperation.AddOne,furnace,recipe,facilityRevision:revision),Is.True);Assert.That(Do(B,FacilityOperation.AddOne,furnace,recipe,facilityRevision:revision),Is.False);
            Assert.That(furnace.Processor.Recipe.Id,Is.EqualTo("steel"));foreach(var c in recipe.Ingredients)Assert.That(Bag(player).CountItem(c.Item),Is.EqualTo(c.Count));furnace.Processor.Advance(6);Assert.That(furnace.Processor.OutputCount,Is.EqualTo(1));Assert.That(Do(A,FacilityOperation.Collect,furnace),Is.True);Assert.That(Bag(player).CountItem(recipe.Result),Is.EqualTo(1));yield return null;
        }
        [UnityTest]public IEnumerator FurnaceQueueAndCompletionRacesKeepOutputAndReservedInputs()
        {
            var f=Facility("Furnace");Open(f);f.Processor.enabled=false;var recipe=world.Catalog.Recipe("iron_ingot");var input=recipe.Ingredients[0].Item;Bag(player).Add(input,4);Bag(other).Add(input,4);var rev=f.Revision.Value;
            Assert.That(Do(A,FacilityOperation.AddOne,f,recipe,facilityRevision:rev),Is.True);Assert.That(Do(B,FacilityOperation.AddOne,f,recipe,facilityRevision:rev),Is.False);Assert.That(Do(B,FacilityOperation.AddAll,f,recipe),Is.True);Assert.That(f.Processor.Queue.WaitingCount,Is.EqualTo(4));
            rev=f.Revision.Value;Assert.That(Do(A,FacilityOperation.Withdraw,f,recipe,facilityRevision:rev),Is.True);Assert.That(Do(B,FacilityOperation.Withdraw,f,recipe,facilityRevision:rev),Is.False);Assert.That(f.Processor.Queue.WaitingCount,Is.EqualTo(3));
            f.Processor.Advance(recipe.ProcessingSeconds-.001f);rev=f.Revision.Value;Assert.That(Do(A,FacilityOperation.Collect,f),Is.False);f.Processor.Advance(.002f);
            Assert.That(Do(A,FacilityOperation.Collect,f,facilityRevision:rev),Is.False);rev=f.Revision.Value;
            Assert.That(Do(A,FacilityOperation.Collect,f,facilityRevision:rev),Is.True);Assert.That(Do(B,FacilityOperation.Collect,f,facilityRevision:rev),Is.False);Assert.That(Bag(player).CountItem(recipe.Result),Is.EqualTo(1));
            Bag(other).Add(world.Catalog.Item("wood"),50*36);f.Processor.Advance(100);var output=f.Processor.OutputCount;Assert.That(output,Is.EqualTo(3));Assert.That(Do(B,FacilityOperation.Collect,f),Is.False);Assert.That(f.Processor.OutputCount,Is.EqualTo(output));
            Assert.That(Do(A,FacilityOperation.Collect,f),Is.True);Assert.That(Bag(player).CountItem(recipe.Result),Is.EqualTo(4));yield return null;
            f.Processor.Restore(new ProcessingSnapshot{Sequential=true,Recipe=recipe.Id,Batch=1,Remaining=.001f,Waiting=new[]{new ProcessingEntry{Recipe=recipe.Id,Count=1}},Output=new[]{new ProcessingEntry{Recipe=recipe.Id,Count=100}}},world.Catalog,A.Bag.Inventory);
            rev=f.Revision.Value;f.Processor.Advance(.002f);Assert.That(f.Processor.OutputCount,Is.EqualTo(100));Assert.That(f.Processor.Remaining,Is.Zero);Assert.That(Do(A,FacilityOperation.Collect,f,facilityRevision:rev),Is.False);
            Assert.That(Do(A,FacilityOperation.Collect,f),Is.True);Assert.That(f.Processor.OutputCount,Is.EqualTo(1));Assert.That(Do(A,FacilityOperation.Collect,f),Is.True);
            var oldBag=B.Bag.Revision.Value;other.NetworkObject.Despawn(false);f.Processor.Advance(10);Assert.That(B.Execute(FacilityOperation.Collect,f.NetworkObjectId,oldBag,f.Revision.Value),Is.False);Assert.That(Do(A,FacilityOperation.Collect,f),Is.True);Assert.That(Bag(player).CountItem(recipe.Result),Is.EqualTo(106));Object.Destroy(other.gameObject);yield return null;
        }
        [UnityTest]public IEnumerator CampfireMeatAndWaterRemainSharedAfterActorRetires()
        {
            var f=Facility("Campfire");Open(f);f.Processor.enabled=false;var meat=world.Catalog.Recipe("cooked_meat");Bag(player).Add(meat.Ingredients[0].Item,1);Bag(other).Add(meat.Ingredients[0].Item,1);var rev=f.Revision.Value;
            Assert.That(Do(A,FacilityOperation.Craft,f,meat,facilityRevision:rev),Is.True);Assert.That(Do(B,FacilityOperation.Craft,f,meat,facilityRevision:rev),Is.False);Assert.That(Bag(other).CountItem(meat.Ingredients[0].Item),Is.EqualTo(1));
            var oldBag=A.Bag.Revision.Value;player.NetworkObject.Despawn(false);f.Processor.Advance(10);Assert.That(A.Execute(FacilityOperation.Collect,f.NetworkObjectId,oldBag,f.Revision.Value),Is.False);
            rev=f.Revision.Value;Assert.That(Do(B,FacilityOperation.Collect,f,facilityRevision:rev),Is.True);Assert.That(Do(B,FacilityOperation.Collect,f,facilityRevision:rev),Is.False);Assert.That(Bag(other).CountItem(meat.Result),Is.EqualTo(1));
            var water=world.Catalog.Recipe("clean_water_cup");Bag(other).Add(water.Ingredients[0].Item,1);Assert.That(Do(B,FacilityOperation.Craft,f,water),Is.True);rev=f.Revision.Value;f.Processor.Advance(10);Assert.That(Do(B,FacilityOperation.Collect,f,facilityRevision:rev),Is.False);Assert.That(Do(B,FacilityOperation.Collect,f),Is.True);Assert.That(Bag(other).CountItem(water.Result),Is.EqualTo(1));
            Object.Destroy(player.gameObject);yield return null;
        }
        [UnityTest]public IEnumerator ExistingUiUsesSharedChestAndHandCraftingWithLocalActorBindings()
        {
            var ui=Object.FindFirstObjectByType<NetworkInventoryUi>();ui.Bag.onClick.Invoke();yield return null;var inventory=ui.GetComponent<InventoryPanel>();inventory.CraftingButton.onClick.Invoke();yield return null;Assert.That(ui.GetComponent<CraftingPanel>().View.activeSelf,Is.True);Assert.That(inventory.Crafting,Is.SameAs(A.Crafting));
            ui.GetComponent<CraftingPanel>().InventoryButton.onClick.Invoke();Assert.That(inventory.View.activeSelf,Is.True);player.Commands.Session.CloseInventory();
            var f=Facility("ReinforcedChest");Place(player,(Vector2)f.transform.position+Vector2.down);Assert.That(A.Open(f),Is.True);yield return null;var chest=ui.GetComponent<ChestPanel>();Assert.That(chest.View.activeSelf,Is.True);Assert.That(chest.ChestSlots.Count(s=>s.gameObject.activeSelf),Is.EqualTo(36));
            var wood=world.Catalog.Item("wood");f.Chest.Model.Add(wood,9);var drag=ui.GetComponent<InventoryDragController>();drag.HoldPartial(0,chest.ChestSlots[0],true,4);Assert.That(drag.PlaceHeld(20,false),Is.True);Assert.That(Bag(player).GetSlot(20).Count,Is.EqualTo(4));Assert.That(f.Chest.Model.GetSlot(0).Count,Is.EqualTo(5));
            drag.HoldPartial(0,chest.ChestSlots[0],true,2);f.Chest.Model.Add(wood,1);Assert.That(drag.PlaceHeld(21,false),Is.False);Assert.That(Bag(player).GetSlot(21).IsEmpty,Is.True);drag.Cancel();
            foreach(var button in chest.View.GetComponentsInChildren<UnityEngine.UI.Button>(true))if(button.name=="Close")button.onClick.Invoke();Assert.That(player.Commands.Session.State,Is.EqualTo(SessionState.Running));yield return null;
            f=Facility("Workbench");Place(player,(Vector2)f.transform.position+Vector2.down);Assert.That(A.Open(f),Is.True);yield return null;ui.GetComponent<CraftingPanel>().InventoryButton.onClick.Invoke();Assert.That(Do(A,FacilityOperation.Upgrade,f),Is.False);Assert.That(A.Crafting.Station,Is.Null);
        }
        [UnityTearDown]public IEnumerator Cleanup(){if(NetworkSession.Instance!=null && NetworkSession.Instance.State!=NetworkSessionState.Idle){NetworkSession.Instance.Leave();for(var i=0;i<300 && NetworkSession.Instance.State!=NetworkSessionState.Idle;i++)yield return null;}Time.timeScale=1;}
    }
}
