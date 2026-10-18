using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class ResourceTempoPlayTests
    {
        private WorldSaveService save;private WorldGenerator world;
        [UnitySetUp]public IEnumerator Load()
        {
            WorldGenerator.SelectedGenerationVersion=2;WorldGenerator.SelectedSeed=9472026;Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();Assert.That(DevelopmentProfile.Isolated,Is.True);save.AutoSaveEnabled=false;world=save.World;save.Clock.enabled=false;
            save.Player.GetComponent<PlayerMotor>().enabled=false;save.Player.GetComponent<PlayerInputAdapter>().enabled=false;world.GetComponent<ZombieSpawner>().enabled=world.GetComponent<GoblinSpawner>().enabled=false;world.Chunks.DrainPending();
        }
        private void Move(Vector2 p){save.Player.transform.position=p;save.Player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();world.Chunks.Reconcile();world.Chunks.DrainPending();}
        [UnityTest]public IEnumerator MineralDebtRecoversElsewhereAndSurvivesSleepSaveReloadWithoutDuplicates()
        {
            var source=world.Map.Resources.First(p=>p.Rule==2 && p.Position.magnitude>300);Move(source.Position+Vector2.down*2);yield return null;
            var key=WorldChunk.At(source.Position,64);var region=WorldMap.Encode(key);var originals=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Where(x=>x.Rule==2 && WorldChunk.At(x.transform.position,64).Equals(key)).ToArray();
            var originalPositions=originals.Select(s=>(Vector2)s.transform.position).ToArray();
            foreach(var node in originals)node.Node.Harvest(save.Catalog.Item("stone_pickaxe").Tool,10000,save.Player);yield return null;
            Assert.That(world.Renewal.Population.Pending.Count(t=>t.Rule==2 && t.Region==region),Is.EqualTo(1));var due=world.Renewal.Population.Pending.Single(t=>t.Rule==2 && t.Region==region).Due;
            var debt=world.Renewal.CaptureRecovery();var backlog=debt.Pending.ToList();for(var i=0;i<1000;i++)backlog.Add(new RecoveryTicket{Rule=2,Region=WorldMap.Encode(new WorldChunk(30+i%32,30+i/32)),Due=0});debt.Pending=backlog.ToArray();world.Renewal.Population.Restore(debt);
            Move(Vector2.zero);save.Clock.Model.SetElapsed(due+1);world.Renewal.Tick();Assert.That(world.Renewal.Capture().Any(s=>s.Rule==2 && s.Id.StartsWith("p")),Is.False,"inactive region must not spawn");
            var snapshot=save.Capture();save.Validate(snapshot);save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));yield return null;Move(source.Position+Vector2.right*30);yield return null;
            Assert.That(world.Renewal.Population.Pending.Single(t=>t.Rule==2 && t.Region==region).Due,Is.EqualTo(due).Within(1e-8));
            for(var i=0;i<30 && !world.Renewal.Capture().Any(s=>s.Rule==2 && s.Id.StartsWith("p"));i++){world.Renewal.Tick();Assert.That(world.Renewal.LastRecoveryChecks,Is.LessThanOrEqualTo(16));Assert.That(world.Renewal.LastCandidateChecks,Is.LessThanOrEqualTo(768));yield return null;}
            var recovered=world.Renewal.Capture().First(s=>s.Rule==2 && s.Id.StartsWith("p"));Assert.That(originalPositions.All(s=>Vector2.Distance(s,recovered.Position)>4),Is.True);Assert.That(world.Map.ResourceSiteAllowed(2,recovered.Position),Is.True);
            snapshot=save.Capture();save.Validate(snapshot);save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));yield return null;Move(recovered.Position+Vector2.right*25);yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Count(s=>s.Id==recovered.Id),Is.EqualTo(1));
            world.Renewal.MaterializeRecovered(WorldChunk.At(recovered.Position,64),world.Environment);Assert.That(UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Count(s=>s.Id==recovered.Id),Is.EqualTo(1));
            var p=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Single(s=>s.Id==recovered.Id);p.Node.Harvest(save.Catalog.Item("stone_pickaxe").Tool,10000,save.Player);yield return null;
            Assert.That(world.Renewal.Capture().Any(s=>s.Id==recovered.Id),Is.False,"depleted recovered IDs must not grow tombstone history");save.Validate(save.Capture());
        }
        [UnityTest]public IEnumerator FullHarvestYieldsAndPanPermissionsUseCanonicalNodes()
        {
            foreach(var rule in new[]{1,2,5,6,7})
            {
                var source=world.Map.Resources.First(p=>p.Rule==rule);Move(source.Position+Vector2.down*2);yield return null;var node=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Single(x=>x.Id==source.Id).Node;
                var item=node.Definition.DropItem;var count=node.Definition.DropCount;var before=WorldPickup.Active.Count;var tool=save.Catalog.Item(rule==6 ? "prospecting_pan" : "iron_pickaxe").Tool;
                if(rule==6)Assert.That(node.Harvest(save.Catalog.Item("steel_pickaxe").Tool,10000,save.Player),Is.Zero);
                Assert.That(node.Harvest(tool,1,save.Player),Is.EqualTo(1));Assert.That(WorldPickup.Active.Count,Is.EqualTo(before));
                node.Harvest(tool,10000,save.Player);yield return null;Assert.That(WorldPickup.Active.Any(x=>x.Item==item && x.Count==count),Is.True,item.Id);
            }
        }
        [UnityTearDown]public IEnumerator Done(){Time.timeScale=1;yield return null;}
    }
}
