using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class WorldFoundationPlayTests
    {
        private WorldSaveService save;private WorldGenerator world;private PlayerInventory player;
        [UnitySetUp]public IEnumerator Load()
        {
            Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=2;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;world=save.World;player=save.Player;save.Clock.enabled=false;player.GetComponent<PlayerMotor>().enabled=false;player.GetComponent<PlayerInputAdapter>().enabled=false;world.Chunks.DrainPending();yield return null;
        }
        private void Move(Vector2 p){player.transform.position=p;player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();world.Chunks.Reconcile();world.Chunks.DrainPending();}
        [UnityTest]public IEnumerator ChunkUnloadPreservesFiniteOreCargoChestAndSaveVersion()
        {
            var source=world.Map.Continental.GuaranteedResources.First(p=>p.Rule==5);Move(source.Position+Vector2.down*2);yield return null;var node=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Single(r=>r.Id==source.Id);Assert.That(node.Node.Harvest(save.Catalog.Item("iron_pickaxe").Tool,1000,player),Is.GreaterThan(0));yield return null;
            var chest=UnityEngine.Object.Instantiate(save.Catalog.Item("reinforced_chest").Placement.Prefab,source.Position+Vector2.right*10,Quaternion.identity).GetComponent<StorageChest>();chest.Model.Add(save.Catalog.Item("wood"),7);var chestId=chest.Placeable.SaveId;
            var blast=UnityEngine.Object.Instantiate(save.Catalog.Item("blast_furnace").Placement.Prefab,source.Position+Vector2.left*10,Quaternion.identity).GetComponent<StationProcessing>();blast.enabled=false;var id=blast.GetComponent<WorldPlaceable>().SaveId;var recipe=save.Catalog.Recipe("steel");foreach(var c in recipe.Ingredients)player.Model.Add(c.Item,c.Count*2);Assert.That(blast.Add(recipe,player,2),Is.True);
            Move(Vector2.zero);yield return null;Assert.That(WorldPlaceable.Active.Any(p=>p.SaveId==id),Is.False);save.Clock.Model.SetElapsed(30);var snapshot=save.Capture();save.Validate(snapshot);Assert.That(snapshot.BaseGenerationVersion,Is.EqualTo(2));Assert.That(snapshot.Version,Is.EqualTo(8));Assert.That(snapshot.Placeables.Single(s=>s.Id==id).Processing.Output.Single().Count,Is.EqualTo(2));Assert.That(snapshot.Resources.Single(s=>s.Id==source.Id).Destroyed,Is.True);
            save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));yield return null;Move(source.Position+Vector2.down*2);yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Any(r=>r.Id==source.Id),Is.False);Assert.That(WorldPlaceable.Active.Single(p=>p.SaveId==chestId).GetComponent<StorageChest>().Model.CountItem(save.Catalog.Item("wood")),Is.EqualTo(7));Assert.That(WorldPlaceable.Active.Single(p=>p.SaveId==id).GetComponent<StationProcessing>().OutputCount,Is.EqualTo(2));Assert.That(world.Chunks.ActiveCount,Is.LessThanOrEqualTo(9));Assert.That(world.Terrain.TerrainTileCount,Is.EqualTo(world.Chunks.ActiveCount));
        }
        [UnityTest]public IEnumerator CavernPortalConnectsSurfaceRoomGraphDeepBranchAndReturn()
        {
            var cave=world.Map.Continental.Caves[0];Move(cave.SurfaceEntrances[0]+Vector2.down);yield return null;var portal=UnityEngine.Object.FindObjectsByType<CavernPortal>(FindObjectsSortMode.None).Single(p=>p.AnchorId=="cave0:0" && !p.ReturnToSurface);Assert.That(portal.Use(player.GetComponent<PlayerInteraction>()),Is.True);world.Chunks.DrainPending();yield return null;
            Assert.That(world.Map.Continental.Underground(player.transform.position),Is.True);var entry=cave.Nearest(player.transform.position);Assert.That(entry.Depth,Is.Zero);var deepest=cave.Rooms.OrderByDescending(r=>r.Depth).First();Move(deepest.Center);yield return null;Assert.That(cave.Nearest(player.transform.position).Depth,Is.EqualTo(1));Assert.That(GameplayLight.Sample(world.Map,player.transform.position).Ambient,Is.LessThan(.4));Assert.That(world.GetComponent<ZombieSpawner>().ActiveCount,Is.GreaterThan(0));
            var snapshot=save.Capture();save.Validate(snapshot);Move(cave.Entry(0)+Vector2.down);yield return null;portal=UnityEngine.Object.FindObjectsByType<CavernPortal>(FindObjectsSortMode.None).Single(p=>p.AnchorId=="cave0:0" && p.ReturnToSurface);Assert.That(portal.Use(player.GetComponent<PlayerInteraction>()),Is.True);Assert.That(world.Map.Continental.Underground(player.transform.position),Is.False);
        }
        [UnityTest]public IEnumerator CurrentMetalLoopAndLegacyLayoutRemainSeparate()
        {
            var bench=UnityEngine.Object.Instantiate(save.Catalog.Item("workbench").Placement.Prefab,new Vector2(0,-2),Quaternion.identity).GetComponent<CraftingStation>();var crafting=player.GetComponent<PlayerCrafting>();Assert.That(crafting.OpenStation(bench),Is.True);foreach(var c in bench.Definition.UpgradeCosts)player.Model.Add(c.Item,c.Count);Assert.That(crafting.UpgradeWorkbench(),Is.True);foreach(var id in new[]{"prospecting_pan","blast_furnace","steel_pickaxe"}){var recipe=save.Catalog.Recipe(id);foreach(var c in recipe.Ingredients)player.Model.Add(c.Item,c.Count);Assert.That(crafting.Craft(recipe),Is.EqualTo(CraftStatus.Ready));}save.World.Session.CloseInventory();var snapshot=save.Capture();save.Validate(snapshot);
            world.Generate(9472026,true,1);yield return null;Assert.That(world.Map.Size,Is.EqualTo(new Vector2(192,144)));Assert.That(world.Map.GenerationVersion,Is.EqualTo(1));Assert.That(world.Terrain.Ground.enabled,Is.True);Assert.That(world.Chunks.enabled,Is.False);Assert.That(UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Length,Is.EqualTo(world.Map.Resources.Count));
            var legacy=save.Capture();legacy.Version=6;legacy.BaseGenerationVersion=0;save.Validate(legacy);Assert.That(legacy.Version,Is.EqualTo(8));Assert.That(legacy.BaseGenerationVersion,Is.EqualTo(1));save.Apply(SaveRepository.Decode(SaveRepository.Encode(legacy)));yield return null;Assert.That(world.Map.Size,Is.EqualTo(new Vector2(192,144)));Assert.That(world.Map.GenerationVersion,Is.EqualTo(1));
        }
        [UnityTearDown]public IEnumerator Done(){Time.timeScale=1;yield return null;}
    }
}
