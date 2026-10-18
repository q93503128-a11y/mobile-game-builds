using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class ExplorationPlayTests
    {
        private WorldSaveService save;private PlayerInventory player;private WorldGenerator world;
        [UnitySetUp]public IEnumerator Enter()
        {
            Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=2;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;player=save.Player;world=save.World;save.Clock.enabled=false;player.GetComponent<PlayerMotor>().enabled=false;player.GetComponent<PlayerInputAdapter>().enabled=false;
            world.GetComponent<ZombieSpawner>().enabled=world.GetComponent<GoblinSpawner>().enabled=false;world.Chunks.DrainPending();yield return null;
        }
        private void Move(Vector2 p){player.transform.position=p;player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();world.Chunks.Reconcile();world.Chunks.DrainPending();}
        private void Clear(Vector2 p){foreach(var r in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(p,r.transform.position)<4)r.gameObject.SetActive(false);Physics2D.SyncTransforms();}
        private void Select(string id){var item=save.Catalog.Item(id);if(player.Model.CountItem(item)==0)player.Model.Add(item,4);for(var i=0;i<PlayerInventory.HotbarSize;i++)if(player.Model.GetSlot(i).Item==item){player.SelectSlot(i);return;}Assert.Fail("Fixture item needs a hotbar slot");}
        [UnityTest]public IEnumerator TorchCraftHeldMovementPlacementAndValidationShareOneItem()
        {
            var recipe=save.Catalog.Recipe("torch");foreach(var c in recipe.Ingredients)player.Model.Add(c.Item,c.Count);world.Session.OpenInventory();player.GetComponent<PlayerCrafting>().OpenHandCrafting();Assert.That(player.GetComponent<PlayerCrafting>().Craft(recipe),Is.EqualTo(CraftStatus.Ready));world.Session.CloseInventory();Select("torch");yield return null;
            var equipment=player.GetComponent<PlayerEquipment>();var source=player.GetComponent<PlayerLightSource>();Assert.That(equipment.HeldVisual.sprite,Is.SameAs(recipe.Result.HeldSprite));Assert.That(equipment.ActiveTool,Is.Null);Assert.That(source.Gameplay.Definition,Is.SameAs(recipe.Result.PortableLight));Assert.That(source.Gameplay.enabled,Is.True);
            var motor=player.GetComponent<PlayerMotor>();Clear(player.transform.position);motor.enabled=true;player.GetComponent<PlayerInputCommands>().SetMove(Vector2.down);yield return new WaitForSeconds(.25f);player.GetComponent<PlayerInputCommands>().SetMove(Vector2.zero);motor.enabled=false;Assert.That(Vector2.Distance(source.Gameplay.transform.position,equipment.HeldVisual.transform.position+Vector3.up*.35f),Is.LessThan(.1f));
            var placement=player.GetComponent<PlayerPlacement>();placement.Facing.Face(Vector2.down);var p=(Vector2)player.transform.position+Vector2.down*placement.Settings.PlacementDistance;Clear(p);var before=player.Model.CountItem(recipe.Result);Assert.That(placement.TryPlace(),Is.True);yield return null;Assert.That(player.Model.CountItem(recipe.Result),Is.EqualTo(before-1));Assert.That(placement.CanPlaceAt(recipe.Result.Placement,p),Is.False);
            Move(new Vector2(world.Map.Continental.RiverAt(1000)+7,1000));var water=new Vector2(world.Map.Continental.RiverAt(1000)+4,1000);Assert.That(placement.CanPlaceAt(recipe.Result.Placement,water),Is.False);Assert.That(player.Model.CountItem(recipe.Result),Is.EqualTo(before-1));
            Select("wood");yield return null;Assert.That(source.Gameplay.enabled,Is.False);Assert.That(equipment.HeldVisual.enabled,Is.False);
        }
        [UnityTest]public IEnumerator HomeAndPlacedTorchSurviveSleepSaveLoadAndWorkbenchUpgrade()
        {
            var home=Object.Instantiate(save.Catalog.Item("workbench").Placement.Prefab,new Vector2(2,0),Quaternion.identity);var station=home.GetComponent<CraftingStation>();var navigation=player.GetComponent<PlayerNavigation>();Assert.That(navigation.AssignHome(station),Is.True);var id=home.SaveId;
            var torch=Object.Instantiate(save.Catalog.Item("torch").Placement.Prefab,new Vector2(-2,-2),Quaternion.identity);var torchId=torch.SaveId;
            var crafting=player.GetComponent<PlayerCrafting>();Assert.That(crafting.OpenStation(station),Is.True);foreach(var c in station.Definition.UpgradeCosts)player.Model.Add(c.Item,c.Count);Assert.That(crafting.UpgradeWorkbench(),Is.True);world.Session.CloseInventory();yield return null;Assert.That(navigation.HomeId,Is.EqualTo(id));
            Move(new Vector2(1800,500));yield return null;Assert.That(navigation.TryHome(world,out var position),Is.True);Assert.That(position,Is.EqualTo(new Vector2(2,0)));Assert.That(WorldPlaceable.Active.Any(f=>f.SaveId==torchId),Is.False);
            var snapshot=save.Capture();save.Validate(snapshot);Assert.That(snapshot.Player.HomeId,Is.EqualTo(id));Assert.That(snapshot.Placeables.Count(f=>f.Id==torchId),Is.EqualTo(1));save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));yield return null;Move(Vector2.zero);yield return null;
            Assert.That(navigation.HomeId,Is.EqualTo(id));Assert.That(navigation.TryHome(world,out position),Is.True);Assert.That(WorldPlaceable.Active.Count(f=>f.SaveId==torchId),Is.EqualTo(1));var restored=WorldPlaceable.Active.Single(f=>f.SaveId==id);Assert.That(restored.Definition.WorkbenchTier,Is.EqualTo(2));restored.Health.ApplyDamage(restored.Health.Current);yield return null;Assert.That(navigation.TryHome(world,out _),Is.False);
        }
        [UnityTest]public IEnumerator TerrainBudgetCacheAndMineralTombstoneRemainBounded()
        {
            var old=new Vector2(32,32);Move(old);var generated=world.Terrain.GeneratedTiles;Move(new Vector2(900,300));yield return null;var hits=world.Terrain.CacheHits;Move(old);yield return null;Assert.That(world.Terrain.CacheHits,Is.GreaterThan(hits));Assert.That(world.Terrain.CachedTileCount,Is.LessThanOrEqualTo(WorldTerrainRenderer.InactiveCacheCapacity));
            var ore=world.Map.Continental.GuaranteedResources.First(r=>r.Rule==5);Move(ore.Position+Vector2.down*2);yield return null;var node=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Single(r=>r.Id==ore.Id);node.Node.Health.ApplyDamage(node.Node.Health.Maximum);yield return null;Move(old);Move(ore.Position+Vector2.down*2);yield return null;Assert.That(Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Any(r=>r.Id==ore.Id),Is.False);
            Assert.That(world.Terrain.TerrainTileCount,Is.EqualTo(world.Chunks.ActiveCount));Assert.That(world.Terrain.TerrainWorkCount,Is.Zero);
            var ids=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Select(r=>r.Id).ToArray();Assert.That(ids.Distinct().Count(),Is.EqualTo(ids.Length));
        }
        [UnityTest]public IEnumerator CaveReadableFloorTorchAndExitBearingRemainDistinctFromHome()
        {
            var cave=world.Map.Continental.Caves[0];var room=cave.Rooms.OrderByDescending(r=>r.Depth).First();Move(room.Center);yield return null;var ambient=GameplayLight.Sample(world.Map,room.Center).Ambient;Assert.That(ambient,Is.GreaterThanOrEqualTo(.17f));Assert.That(ambient,Is.LessThan(.25f));
            var lighting=Object.FindFirstObjectByType<WorldLighting>();Assert.That(lighting.Global.lightType,Is.EqualTo(UnityEngine.Rendering.Universal.Light2D.LightType.Global));yield return new WaitForSeconds(.6f);Assert.That(lighting.Global.intensity,Is.EqualTo(ambient).Within(.01f));
            Select("torch");yield return null;var sample=GameplayLight.Sample(world.Map,player.GetComponent<PlayerLightSource>().Gameplay.transform.position);Assert.That(sample.Exposure,Is.GreaterThan(ambient+.6f));Assert.That(player.GetComponent<PlayerLightSource>().Gameplay.Definition.DetersEnemies,Is.False);
            var target=NavigationBearing.CaveExit(cave,room.Center);Assert.That(room.Neighbors.Any(i=>cave.Rooms[i].Center==target),Is.True);
            var p=room.Center+Vector2.down*2;Clear(p);Move(room.Center);player.GetComponent<CharacterFacing>().Face(Vector2.down);Assert.That(player.GetComponent<PlayerPlacement>().TryPlace(),Is.True);yield return null;
            var snapshot=save.Capture();save.Validate(snapshot);Assert.That(snapshot.Placeables.Any(f=>f.Item=="torch"),Is.True);
        }
    }
}
