using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class GamefeelPlayTests
    {
        private WorldSaveService save;private PlayerInventory player;
        [UnitySetUp]public IEnumerator Enter()
        {
            Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=1;WorldGenerator.SelectedSeed=9472026;
            yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;player=save.Player;
            save.Clock.enabled=save.World.Animals.enabled=save.World.Renewal.enabled=false;
            save.World.GetComponent<ZombieSpawner>().enabled=save.World.GetComponent<GoblinSpawner>().enabled=false;
            player.GetComponent<PlayerMotor>().enabled=player.GetComponent<PlayerInputAdapter>().enabled=false;
        }
        [UnityTest]public IEnumerator SchedulerRunsJobsButNotDisabledFacilitiesAndResumesAfterCollect()
        {
            var station=Object.Instantiate(save.Catalog.Item("furnace").Placement.Prefab,new Vector2(2,0),Quaternion.identity).GetComponent<StationProcessing>();
            var recipe=save.Catalog.Recipe("iron_ingot");foreach(var cost in recipe.Ingredients)player.Model.Add(cost.Item,cost.Count*2);
            Assert.That(station.Add(recipe,player,2),Is.True);var initial=station.Remaining;
            yield return new WaitForSeconds(.3f);Assert.That(station.Remaining,Is.LessThan(initial));
            station.enabled=false;var remaining=station.Remaining;yield return new WaitForSeconds(.25f);Assert.That(station.Remaining,Is.EqualTo(remaining));
            station.enabled=true;yield return new WaitForSeconds(.25f);Assert.That(station.Remaining,Is.LessThan(remaining));
            station.Advance(10);Assert.That(station.OutputCount,Is.EqualTo(2));Assert.That(station.Collect(player),Is.True);
            Assert.That(station.OutputCount,Is.Zero);Assert.That(station.IsBusy,Is.False);
        }
        [UnityTest]public IEnumerator DeathDisablesCollisionImmediatelyPaysOnceAndRestoresPooledAppearance()
        {
            var spawner=Object.FindFirstObjectByType<ZombieSpawner>();var enemy=PrefabPool.Spawn(spawner.Prefab,new Vector2(8,0),Quaternion.identity);
            enemy.Session=spawner.Session;enemy.Clock=spawner.Clock;enemy.PlayerHealth=player.GetComponent<Health>();enemy.enabled=false;
            var lifecycle=enemy.GetComponent<ZombieLifecycle>();lifecycle.Clock=spawner.Clock;lifecycle.RestoreActive();
            var health=enemy.GetComponent<Health>();var feedback=enemy.GetComponent<HitFeedback>();var scale=feedback.Visual.localScale;
            var notifications=0;health.Died+=()=>notifications++;
            Assert.That(health.ApplyDamage(10000,true,player),Is.GreaterThan(0));Assert.That(health.ApplyDamage(10000,true,player),Is.Zero);
            Assert.That(notifications,Is.EqualTo(1));Assert.That(enemy.GetComponent<Collider2D>().enabled,Is.False);Assert.That(enemy.gameObject.activeSelf,Is.True);
            Assert.That(lifecycle.enabled,Is.False,"Death must stop dawn burrowing from competing with the death pose");
            yield return new WaitForSeconds(.16f);Assert.That(feedback.Visual.localScale.y,Is.LessThan(scale.y));
            yield return new WaitForSeconds(.25f);Assert.That(enemy.gameObject.activeSelf,Is.False);
            var reused=PrefabPool.Spawn(spawner.Prefab,new Vector2(8,0),Quaternion.identity);Assert.That(reused,Is.SameAs(enemy));
            Assert.That(lifecycle.Phase,Is.EqualTo(ZombiePhase.Emerging));Assert.That(reused.GetComponent<Collider2D>().enabled,Is.False);Assert.That(feedback.Visual.localScale,Is.EqualTo(scale));Assert.That(health.IsAlive,Is.True);
        }
        [UnityTest]public IEnumerator WorkbenchDetailsDoNotOverlapUpgradeRequirementsAndModalBlocksMovement()
        {
            var station=Object.Instantiate(save.Catalog.Item("workbench").Placement.Prefab,new Vector2(2,0),Quaternion.identity).GetComponent<CraftingStation>();
            Assert.That(player.GetComponent<PlayerCrafting>().OpenStation(station),Is.True);yield return null;
            var panel=Object.FindFirstObjectByType<CraftingPanel>();var details=panel.SummaryScroll.GetComponent<RectTransform>();var upgrade=panel.UpgradeScroll.GetComponent<RectTransform>();
            var a=new Vector3[4];var b=new Vector3[4];details.GetWorldCorners(a);upgrade.GetWorldCorners(b);Assert.That(a[0].y,Is.GreaterThan(b[1].y));
            player.GetComponent<PlayerInputCommands>().SetMove(Vector2.one);Assert.That(player.GetComponent<PlayerInputCommands>().Move,Is.EqualTo(Vector2.zero));
            PlayerFeedback.ShowCrafted(save.Catalog.Recipe("compass"));var feedback=Object.FindFirstObjectByType<PlayerFeedback>();Assert.That(feedback.Label.transform.parent.name,Is.EqualTo("Safe Area"));
            Assert.That(feedback.Label.text,Does.Contain(save.Catalog.Item("compass").LocalizedName));
        }
        [UnityTest]public IEnumerator CameraSnapsAcrossDiscontinuousTravelAndKeepsNormalFollowSmooth()
        {
            var camera=Camera.main;player.transform.position=new Vector3(60,20,0);player.GetComponent<Rigidbody2D>().position=player.transform.position;
            yield return null;yield return null;Assert.That(Vector2.Distance(camera.transform.position,player.transform.position),Is.LessThan(.05f));
            player.transform.position+=Vector3.right;player.GetComponent<Rigidbody2D>().position=player.transform.position;
            yield return null;Assert.That(camera.transform.position.x,Is.LessThan(player.transform.position.x));
        }
    }
}
