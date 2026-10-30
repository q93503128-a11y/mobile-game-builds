using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Wildfolk.Tests
{
    public sealed class PostPlaytestInteractionTests
    {
        private WorldSaveService save;
        private PlayerInventory bag;
        private PlayerInputCommands commands;
        private CraftingPanel panel;
        private PlayerCrafting crafting;

        [UnitySetUp] public IEnumerator Enter()
        {
            Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=-484461830;
            yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;save.Clock.enabled=false;
            bag=save.Player;commands=bag.GetComponent<PlayerInputCommands>();crafting=bag.GetComponent<PlayerCrafting>();
            panel=Object.FindFirstObjectByType<CraftingPanel>();
            bag.GetComponent<PlayerInputAdapter>().enabled=false;bag.GetComponent<PlayerMotor>().enabled=false;
            bag.GetComponent<PlayerSurvival>().enabled=false;bag.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;
            save.World.GetComponent<ZombieSpawner>().enabled=false;save.World.GetComponent<GoblinSpawner>().enabled=false;
            foreach(var enemy in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))enemy.gameObject.SetActive(false);
            EmptyBag();Position(Vector2.zero,Vector2.right);yield return null;
        }
        [UnityTearDown] public IEnumerator Release(){Time.timeScale=1;yield return null;}
        private void EmptyBag()
        {
            var state=new SavedStack[bag.Model.Capacity];for(var i=0;i<state.Length;i++)state[i]=new SavedStack();
            bag.Model.Restore(state,save.Catalog);bag.SelectSlot(0);
        }
        private void Position(Vector2 at,Vector2 facing)
        {
            bag.transform.position=at;bag.GetComponent<Rigidbody2D>().position=at;
            bag.GetComponent<CharacterFacing>().Face(facing);save.World.Chunks.Reconcile();save.World.Chunks.DrainPending();Physics2D.SyncTransforms();
        }
        private CraftingStation Place(string id,Vector2 at)=>Object.Instantiate(save.Catalog.Item(id).Placement.Prefab,at,Quaternion.identity).GetComponent<CraftingStation>();
        private RecipeCardView Card(string id)=>panel.Cards.Single(card=>card.Recipe.Id==id);
        private void Give(string id,int count=1)=>bag.Model.Add(save.Catalog.Item(id),count);
        private void Select(string id)
        {
            for(var i=0;i<PlayerInventory.HotbarSize;i++)if(bag.Model.GetSlot(i).Item==save.Catalog.Item(id)){bag.SelectSlot(i);return;}
            Assert.Fail("Fixture item must be in the hotbar: "+id);
        }
        private void AssertSelectionVisible()
        {
            var visible=panel.Cards.Where(card=>card.gameObject.activeSelf).ToArray();
            if(visible.Length==0)
            {
                Assert.That(crafting.SelectedRecipe,Is.Null);Assert.That(panel.SelectedTitle.text,Is.Empty);
                Assert.That(panel.MaterialSummary.text,Is.Empty);Assert.That(panel.SelectedIcon.gameObject.activeSelf,Is.False);
            }
            else Assert.That(visible.Any(card=>card.Recipe==crafting.SelectedRecipe),Is.True);
        }

        [UnityTest] public IEnumerator CampfireOpensWithSwordEmptyOrFilledWaterskinAndTool()
        {
            var station=Place("campfire",new Vector2(2,0));Give("iron_sword");Give("waterskin");Give("stone_axe");Give("empty_cup");
            var view=Object.FindFirstObjectByType<GameplayHud>().Interact;
            foreach(var item in new[]{"iron_sword","waterskin","waterskin","stone_axe"})
            {
                Select(item);var filled=item=="waterskin" && bag.Model.GetSlot(1).Charges==0;
                if(item=="waterskin")bag.Model.SetWater(1,save.Catalog.Item(item),filled?3:0,filled?LiquidState.Clean:LiquidState.Empty);
                var charges=bag.Model.GetSlot(1).Charges;yield return null;
                Assert.That(bag.GetComponent<PlayerInteraction>().Target,Is.SameAs(station));Assert.That(view.Icon.sprite,Is.SameAs(station.Icon));
                Assert.That(view.Button.IsInteractable(),Is.True);view.Button.onClick.Invoke();yield return null;
                Assert.That(crafting.Station,Is.SameAs(station));Assert.That(panel.View.activeSelf,Is.True);
                Assert.That(bag.Model.GetSlot(1).Charges,Is.EqualTo(charges));Assert.That(bag.Model.CountItem(save.Catalog.Item("empty_cup")),Is.EqualTo(1));
                crafting.CloseCrafting();yield return null;
            }
            Position(Vector2.zero,Vector2.left);Select("waterskin");yield return null;
            Assert.That(commands.Interact(),Is.False,"A facility behind the player is not globally prioritized");
            Assert.That(crafting.ScreenOpen,Is.False);
        }

        [UnityTest] public IEnumerator FacilityWaterOverlapKeepsContextAndWaterCollection()
        {
            var edge=new Vector2(-12.28f,8.48f);Position(edge,Vector2.right);
            var station=Place("campfire",edge+Vector2.right*1.5f);Give("waterskin");Give("iron_sword");Give("empty_cup");Select("waterskin");
            yield return null;Assert.That(commands.Interact(),Is.True);yield return null;
            Assert.That(crafting.Station,Is.SameAs(station));Assert.That(bag.Model.GetSlot(0).Charges,Is.Zero);
            crafting.CloseCrafting();Position(edge,(save.World.Map.PondCenter-edge).normalized);yield return null;
            Assert.That(bag.GetComponent<PlayerInteraction>().Target,Is.TypeOf<WaterSource>());
            Assert.That(commands.Interact(),Is.True);Assert.That(crafting.ScreenOpen,Is.False);
            Assert.That(bag.Model.GetSlot(0).Charges,Is.EqualTo(3));Assert.That(bag.Model.GetSlot(0).Quality,Is.EqualTo(LiquidState.Raw));
            Select("iron_sword");yield return null;Assert.That(commands.Interact(),Is.True);
            Assert.That(bag.Model.CountItem(save.Catalog.Item("raw_water_cup")),Is.EqualTo(1));
            Position(edge+Vector2.right*6,Vector2.left);yield return null;
            Assert.That(commands.Interact(),Is.False,"Existing source and station distances are still enforced");
        }

        [UnityTest] public IEnumerator CupFacesNaturalWaterAtReportedShorePositionsRepeatedly()
        {
            foreach(var at in new[]{new Vector2(-10.22f,7.35f),new Vector2(-12.28f,8.48f)})
            for(var repeat=0;repeat<3;repeat++)foreach(var angle in new[]{0,180,30,-30})
            {
                yield return new WaitForSeconds(1.1f);EmptyBag();Give("empty_cup");
                var toward=angle!=180;var toWater=(save.World.Map.PondCenter-at).normalized;var radians=angle*Mathf.Deg2Rad;
                Position(at,new Vector2(toWater.x*Mathf.Cos(radians)-toWater.y*Mathf.Sin(radians),toWater.x*Mathf.Sin(radians)+toWater.y*Mathf.Cos(radians)));yield return null;
                Assert.That(commands.Interact(),Is.EqualTo(toward),at+" angle="+angle+" repeat="+repeat);
                Assert.That(bag.Model.CountItem(save.Catalog.Item("raw_water_cup")),Is.EqualTo(toward?1:0));
            }
        }

        [UnityTest] public IEnumerator CookingRemainsVisibleThroughProcessingBlockedClaimAndCollection()=>BatchOperation("cooked_meat");
        [UnityTest] public IEnumerator PurificationRemainsVisibleThroughProcessingBlockedClaimAndCollection()=>BatchOperation("purify_waterskin");
        private IEnumerator BatchOperation(string id)
        {
            var station=Place("campfire",new Vector2(2,0));var recipe=save.Catalog.Recipe(id);
            Assert.That(bag.Model.TryAddAmounts(recipe.Ingredients),Is.True);yield return null;
            Assert.That(commands.Interact(),Is.True);yield return null;
            panel.ToggleFilter();var card=Card(id);Assert.That(card.gameObject.activeSelf,Is.True);
            var other=id=="cooked_meat"?"clean_water_cup":"cooked_meat";Assert.That(Card(other).gameObject.activeSelf,Is.False);
            card.CraftButton.onClick.Invoke();yield return null;var job=station.GetComponent<StationProcessing>();
            Assert.That(job.IsBusy,Is.True);Assert.That(job.IsReady,Is.False);Assert.That(card.gameObject.activeSelf,Is.True);AssertSelectionVisible();
            panel.ToggleFilter();Assert.That(Card(other).gameObject.activeSelf,Is.True);panel.ToggleFilter();
            Assert.That(crafting.SelectedRecipe,Is.SameAs(recipe));
            panel.CategoryButtons[(int)recipe.Category+1].onClick.Invoke();Assert.That(card.gameObject.activeSelf,Is.True);
            crafting.CloseCrafting();yield return null;Assert.That(commands.Interact(),Is.True);yield return null;
            Assert.That(card.gameObject.activeSelf,Is.True);AssertSelectionVisible();
            var full=new SavedStack[bag.Model.Capacity];for(var i=0;i<full.Length;i++)full[i]=new SavedStack{Item="wood",Count=50};bag.Model.Restore(full,save.Catalog);
            job.Advance(recipe.ProcessingSeconds+1);yield return null;
            Assert.That(job.IsReady,Is.True);Assert.That(crafting.GetStatus(recipe),Is.EqualTo(CraftStatus.NoSpace));
            Assert.That(card.gameObject.activeSelf,Is.True);Assert.That(card.CraftButton.IsInteractable(),Is.False);
            Assert.That(bag.Model.TryConsumeCosts(new[]{new ItemAmount{Item=save.Catalog.Item("wood"),Count=50}}),Is.True);yield return null;
            Assert.That(card.CraftButton.IsInteractable(),Is.True);card.CraftButton.onClick.Invoke();yield return null;
            Assert.That(job.IsBusy,Is.False);Assert.That(bag.Model.CountItem(recipe.Result),Is.EqualTo(recipe.ResultCount));
            Assert.That(card.gameObject.activeSelf,Is.False);AssertSelectionVisible();
        }

        [UnityTest] public IEnumerator SequentialCurrentWaitingAndUnclaimedOutputSurviveCraftableFilter()
        {
            var station=Place("furnace",new Vector2(2,0));var iron=save.Catalog.Recipe("iron_ingot");var copper=save.Catalog.Recipe("copper_ingot");
            Assert.That(bag.Model.TryAddAmounts(iron.Ingredients),Is.True);Assert.That(bag.Model.TryAddAmounts(copper.Ingredients),Is.True);yield return null;
            Assert.That(commands.Interact(),Is.True);yield return null;var job=station.GetComponent<StationProcessing>();
            Assert.That(job.Add(iron,bag,1),Is.True);Assert.That(job.Add(copper,bag,1),Is.True);panel.ToggleFilter();
            Assert.That(job.Queue.CountWaiting(copper),Is.EqualTo(1));Assert.That(Card(iron.Id).gameObject.activeSelf,Is.True);Assert.That(Card(copper.Id).gameObject.activeSelf,Is.True);
            Assert.That(Card("tin_ingot").gameObject.activeSelf,Is.False);AssertSelectionVisible();
            job.Advance(100);yield return null;Assert.That(job.Recipe,Is.Null);Assert.That(job.OutputCount,Is.GreaterThan(0));
            Assert.That(Card(iron.Id).gameObject.activeSelf,Is.True);Assert.That(Card(copper.Id).gameObject.activeSelf,Is.True);
            panel.Collect.onClick.Invoke();yield return null;Assert.That(job.OutputCount,Is.Zero);
            Assert.That(bag.Model.CountItem(iron.Result),Is.EqualTo(iron.ResultCount));Assert.That(bag.Model.CountItem(copper.Result),Is.EqualTo(copper.ResultCount));AssertSelectionVisible();
        }

        [UnityTest] public IEnumerator CategoryFilterAndReopenKeepDetailOnVisibleRecipeOrClearIt()
        {
            Place("workbench",new Vector2(2,0));Give("wood",40);yield return null;Assert.That(commands.Interact(),Is.True);yield return null;
            panel.CategoryButtons[(int)RecipeCategory.Weapons+1].onClick.Invoke();var sword=save.Catalog.Recipe("wood_sword");
            Card(sword.Id).OnPointerClick(null);panel.ToggleFilter();Assert.That(crafting.SelectedRecipe,Is.SameAs(sword));AssertSelectionVisible();
            panel.ToggleFilter();Assert.That(crafting.SelectedRecipe,Is.SameAs(sword));
            panel.CategoryButtons[(int)RecipeCategory.Tools+1].onClick.Invoke();Assert.That(crafting.SelectedRecipe,Is.Not.SameAs(sword));AssertSelectionVisible();
            panel.CategoryButtons[(int)RecipeCategory.Weapons+1].onClick.Invoke();AssertSelectionVisible();
            var missing=panel.Cards.First(card=>card.gameObject.activeSelf && crafting.GetStatus(card.Recipe)!=CraftStatus.Ready);missing.OnPointerClick(null);
            panel.ToggleFilter();Assert.That(crafting.SelectedRecipe,Is.Not.SameAs(missing.Recipe));AssertSelectionVisible();
            var retained=crafting.SelectedRecipe;crafting.CloseCrafting();yield return null;Assert.That(commands.Interact(),Is.True);yield return null;
            Assert.That(crafting.SelectedRecipe,Is.SameAs(retained));AssertSelectionVisible();
            EmptyBag();yield return null;Assert.That(panel.Cards.Count(card=>card.gameObject.activeSelf),Is.Zero);AssertSelectionVisible();
            panel.ToggleFilter();Assert.That(panel.Cards.Any(card=>card.gameObject.activeSelf),Is.True);AssertSelectionVisible();
        }
    }
}
