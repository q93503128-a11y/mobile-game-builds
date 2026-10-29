using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class NeedsCookingPlayTests
    {
        private PlayPointerDriver input;
        private PlayerMotor player;
        private PlayerInventory inventory;
        private PlayerCrafting crafting;
        private PlayerSurvival needs;
        private GameplayHud hud;
        private LocalSettings original;
        private string previousQa;private int previousGeneration;
        [UnitySetUp] public IEnumerator Load()
        {
            previousQa=UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","");UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",System.IO.Path.Combine(System.IO.Path.GetTempPath(),"LowfireNeedsTests",System.Guid.NewGuid().ToString("N")));previousGeneration=WorldGenerator.SelectedGenerationVersion;WorldGenerator.SelectedGenerationVersion=WorldGenerationVersion.Legacy;SceneFlow.Repository=SaveRepository.Default();ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(System.IO.Path.Combine(DevelopmentProfile.Directory,"classes")));
            // This original pointer fixture expects the single legacy pond and an empty bag.
            // Streamed generation3 actions are exercised by AudioProductionQa separately.
            original=LocalSettings.Load();input=new PlayPointerDriver();Time.timeScale=1;
            WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            player=Object.FindFirstObjectByType<PlayerMotor>();inventory=player.GetComponent<PlayerInventory>();
            crafting=player.GetComponent<PlayerCrafting>();needs=player.GetComponent<PlayerSurvival>();hud=Object.FindFirstObjectByType<GameplayHud>();
        }
        [UnityTearDown] public IEnumerator Release()
        {
            input.Dispose();Time.timeScale=1;original.Store();yield return null;WorldGenerator.SelectedGenerationVersion=previousGeneration;SceneFlow.Repository=null;UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",previousQa);
        }
        private RecipeDefinition Recipe(string id)
        {foreach(var recipe in crafting.Catalog.Recipes) if(recipe.Id==id) return recipe;Assert.Fail(id);return null;}
        private RecipeCardView Card(string id)
        {foreach(var card in Object.FindFirstObjectByType<CraftingPanel>().Cards) if(card.Recipe.Id==id) return card;Assert.Fail(id);return null;}
        private ItemDefinition Raw => Recipe("cooked_meat").Ingredients[0].Item;
        private void Position(Vector2 position,Vector2 facing)
        {player.GetComponent<Rigidbody2D>().position=position;player.transform.position=position;player.Avatar.Facing.Face(facing);Physics2D.SyncTransforms();}
        private CraftingStation SpawnFire()
        {return Object.Instantiate(Recipe("campfire").Result.Placement.Prefab,(Vector2)player.transform.position+Vector2.down*2.1f,Quaternion.identity).GetComponent<CraftingStation>();}
        [UnityTest] public IEnumerator PauseSettingsSlidersApplyToControlsAndRealAudioChannels()
        {
            yield return input.Click(hud.PauseButton);
            yield return input.Click(hud.PausePanel.transform.Find("Pause Panel/Settings"));
            var settings=Object.FindFirstObjectByType<SettingsPanel>();Assert.That(settings.gameObject.activeSelf,Is.True);
            Assert.That(Time.timeScale,Is.Zero);
            yield return input.Click(settings.Control);
            Assert.That(player.Joystick.transform.localScale.x,Is.EqualTo(1.1f).Within(.03f));
            Assert.That(hud.Attack.transform.localScale.x,Is.EqualTo(1.1f).Within(.03f));
            Assert.That(Object.FindFirstObjectByType<SurvivalHud>().transform.localScale,Is.EqualTo(Vector3.one));
            yield return input.Click(settings.Master);yield return input.Click(settings.Music);yield return input.Click(settings.Sfx);
            var audio=Object.FindFirstObjectByType<GameAudio>();
            Assert.That(audio.MusicGain,Is.EqualTo(.25f).Within(.04f));Assert.That(audio.SfxGain,Is.EqualTo(.25f).Within(.04f));
            var before=needs.Model.Hunger;yield return new WaitForSecondsRealtime(.15f);Assert.That(needs.Model.Hunger,Is.EqualTo(before));
            yield return input.Click(settings.transform.Find("Reset"));
            Assert.That(settings.Control.value,Is.EqualTo(1));Assert.That(settings.Music.value,Is.EqualTo(.65f));
            yield return input.Click(settings.transform.Find("Back"));Assert.That(settings.gameObject.activeSelf,Is.False);
            yield return input.Click(hud.PausePanel.transform.Find("Pause Panel/Continue"));Assert.That(hud.Session.IsRunning,Is.True);
        }
        private static Rect Bounds(Component component)
        {
            var corners=new Vector3[4];((RectTransform)component.transform).GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);
        }
        [UnityTest] public IEnumerator ControlSizeLimitsKeepTouchControlsSeparateAndModalCentered()
        {
            var preferences=Object.FindFirstObjectByType<UserPreferences>();
            var hotbar=Object.FindFirstObjectByType<HotbarView>();
            foreach(var scale in new[]{.8f,1.4f})
            {
                preferences.SetControl(scale);yield return null;yield return null;
                Assert.That(Bounds(player.Joystick).Overlaps(Bounds(hud.InventoryButton)),Is.False);
                Assert.That(Bounds(hud.Attack).Overlaps(Bounds(hotbar)),Is.False);
                Assert.That(Bounds(hud.Interact).Overlaps(Bounds(hud.Attack)),Is.False);
                Assert.That(Bounds(player.Joystick).Overlaps(Bounds(hotbar)),Is.False);
                yield return input.Inventory(hud);
                var surface=Object.FindFirstObjectByType<InventoryPanel>().View.transform.Find("Inventory Panel");
                Assert.That(((RectTransform)surface).anchoredPosition,Is.EqualTo(Vector2.zero));
                Assert.That(Object.FindFirstObjectByType<GameplayUiLayers>().Gameplay.blocksRaycasts,Is.False);
                yield return input.Inventory(hud);Assert.That(hud.Session.IsRunning,Is.True);
            }
        }
        [UnityTest] public IEnumerator MultiplayerPauseBlocksLocalInputButContinuesWorldNeedsAndEnemy()
        {
            hud.Session.Mode=SessionMode.Multiplayer;
            var enemy=WorldTestScene.SpawnZombie();yield return new WaitForSeconds(1.3f);Position((Vector2)enemy.transform.position+Vector2.left*.85f,Vector2.right);
            yield return new WaitForSeconds(.2f);var hp=needs.Health.Current;var hunger=needs.Model.Hunger;
            yield return input.Click(hud.PauseButton);Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return input.Click(hud.Attack);Assert.That(player.Combat.IsAttacking,Is.False);
            var deadline=Time.time+3.5f;
            while(needs.Health.Current==hp && Time.time<deadline)yield return null;
            Assert.That(needs.Health.Current,Is.LessThan(hp),"Enemy must remain active in the Multiplayer pause policy: "+enemy.State);Assert.That(needs.Model.Hunger,Is.LessThan(hunger));
            var freeze=Time.timeScale;needs.Health.ApplyDamage(needs.Health.Current);yield return null;
            Assert.That(Time.timeScale,Is.EqualTo(freeze),"Local defeat must not freeze a multiplayer world.");
        }
        [UnityTest] public IEnumerator WaterInteractionAndSelectedConsumptionDoNotWasteFullMeters()
        {
            var source=Object.FindFirstObjectByType<WaterSource>();var water=source.Definition.Container.Raw;
            Position((Vector2)source.transform.position+Vector2.down*2.2f,Vector2.up);yield return new WaitForSeconds(.2f);
            Assert.That(player.GetComponent<PlayerInteraction>().TargetWater,Is.SameAs(source));
            inventory.Model.Add(source.Definition.Container.Empty,1);yield return input.Click(hud.Interact);Assert.That(inventory.Model.CountItem(water),Is.EqualTo(1));
            inventory.SelectSlot(0);yield return null;
            needs.Model.Restore(water.Consumable);
            Assert.That(player.GetComponent<PlayerPrimaryAction>().TryUse(),Is.False);Assert.That(inventory.Model.CountItem(water),Is.EqualTo(1));
            needs.Model.Advance(100);yield return input.Click(hud.Attack);
            Assert.That(inventory.Model.CountItem(water),Is.Zero);Assert.That(inventory.Model.CountItem(source.Definition.Container.Empty),Is.EqualTo(1));Assert.That(needs.Model.Thirst,Is.GreaterThan(99));
            Position(Vector2.zero,Vector2.up);yield return null;Assert.That(source.Collect(inventory),Is.False);
        }
        [UnityTest] public IEnumerator CampfireRecipePlacementCookingProgressAndTakeUseActualButtons()
        {
            var recipe=Recipe("campfire");foreach(var cost in recipe.Ingredients) inventory.Model.Add(cost.Item,cost.Count);
            yield return input.Inventory(hud);yield return input.Click(Object.FindFirstObjectByType<InventoryPanel>().CraftingButton);yield return input.Click(Object.FindFirstObjectByType<CraftingPanel>().CategoryButtons[(int)recipe.Category+1]);yield return input.Click(Card("campfire").CraftButton);
            Assert.That(inventory.Model.GetSlot(0).Item,Is.SameAs(recipe.Result));
            yield return input.Inventory(hud);Position(new Vector2(0,-.35f),Vector2.down);yield return new WaitForSeconds(.2f);
            yield return input.Click(hud.Attack);yield return null;
            Assert.That(inventory.Model.GetSlot(0).IsEmpty,Is.True);
            var station=Object.FindFirstObjectByType<CraftingStation>();Assert.That(station.Definition.Station,Is.EqualTo(StationKind.Campfire));
            inventory.Model.Add(Raw,1);inventory.SelectSlot(8);yield return null;
            yield return input.Click(hud.Interact);Assert.That(crafting.AvailableStation,Is.EqualTo(StationKind.Campfire));
            yield return input.Click(Card("cooked_meat").CraftButton);
            var cooking=station.GetComponent<StationProcessing>();Assert.That(cooking.IsBusy,Is.True);Assert.That(inventory.Model.CountItem(Raw),Is.Zero);
            yield return new WaitForSeconds(.3f);Assert.That(Card("cooked_meat").Progress.gameObject.activeSelf,Is.True);
            Assert.That(Card("cooked_meat").Status.text,Does.Contain("Cooking"));
            yield return input.Inventory(hud);yield return new WaitForSeconds(5.1f);
            Assert.That(cooking.IsReady,Is.True);Assert.That(crafting.OpenStation(station),Is.True);yield return null;yield return null;
            yield return input.Click(Card("cooked_meat").CraftButton);
            var food=Recipe("cooked_meat").Result;Assert.That(inventory.Model.CountItem(food),Is.EqualTo(1));Assert.That(cooking.IsBusy,Is.False);
            inventory.SelectSlot(0);hud.Session.CloseInventory();needs.Model.Advance(400);yield return input.Click(hud.Attack);
            Assert.That(inventory.Model.CountItem(food),Is.Zero);Assert.That(needs.Model.Hunger,Is.GreaterThan(99));
        }
        [UnityTest] public IEnumerator FullInventoryKeepsCookedOutputAndDestroyedFireDropsCargo()
        {
            var station=SpawnFire();yield return null;inventory.Model.Add(Raw,1);Assert.That(crafting.OpenStation(station),Is.True);
            Assert.That(crafting.Craft(Recipe("cooked_meat")),Is.EqualTo(CraftStatus.Processing));
            var wood=Recipe("workbench").Ingredients[0].Item;inventory.Model.Add(wood,36*50);yield return new WaitForSeconds(5.1f);
            var job=station.GetComponent<StationProcessing>();Assert.That(job.IsReady,Is.True);
            Assert.That(crafting.Craft(Recipe("cooked_meat")),Is.EqualTo(CraftStatus.NoSpace));Assert.That(job.IsReady,Is.True);
            station.Health.ApplyDamage(station.Health.Maximum);yield return null;yield return null;
            var drop=Object.FindFirstObjectByType<WorldPickup>();Assert.That(drop,Is.Not.Null);Assert.That(drop.Item,Is.SameAs(Recipe("cooked_meat").Result));Assert.That(drop.Count,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator DestroyedFireReturnsUncookedInputAndRangeCannotStartCooking()
        {
            var station=SpawnFire();yield return null;inventory.Model.Add(Raw,1);
            Position(new Vector2(4,0),Vector2.down);yield return new WaitForSeconds(.1f);Assert.That(crafting.OpenStation(station),Is.False);
            Position(Vector2.zero,Vector2.down);yield return new WaitForSeconds(.1f);Assert.That(crafting.OpenStation(station),Is.True);
            Assert.That(crafting.Craft(Recipe("cooked_meat")),Is.EqualTo(CraftStatus.Processing));
            station.Health.ApplyDamage(station.Health.Maximum);yield return null;yield return null;
            var drop=Object.FindFirstObjectByType<WorldPickup>();Assert.That(drop.Item,Is.SameAs(Raw));Assert.That(drop.Count,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator OneNeutralMeatSourceDropsRawWithoutAddingAnEnemyType()
        {
            Assert.That(Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None),Is.Empty);
            NeutralAnimal animal=null;foreach(var node in Object.FindObjectsByType<NeutralAnimal>(FindObjectsSortMode.None))if(node.Member.Definition.Id=="boar")animal=node;
            Assert.That(animal,Is.Not.Null);Assert.That(animal.GetComponent<EnemyController>(),Is.Null);
            Position((Vector2)animal.transform.position+Vector2.down*1.6f,Vector2.up);yield return new WaitForSeconds(.2f);
            animal.Health.ApplyDamage(animal.Health.Maximum);yield return new WaitForSeconds(HitFeedback.DeathDuration+.05f);
            Assert.That(animal==null,Is.True);WorldPickup drop=null;foreach(var item in Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None))if(item.Item==Raw)drop=item;Assert.That(drop,Is.Not.Null);Assert.That(drop.Count,Is.EqualTo(2));
            Position(drop.transform.position,Vector2.up);yield return new WaitForSeconds(.85f);Assert.That(inventory.Model.CountItem(Raw),Is.EqualTo(2));
        }
    }
}
