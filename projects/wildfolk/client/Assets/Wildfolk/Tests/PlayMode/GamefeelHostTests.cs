using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class GamefeelHostTests
    {
        private NetworkSession session;private NetworkPlayer player;
        private IEnumerator Host()
        {
            var task=session.Host(ConnectionRoute.LocalNetwork);yield return new WaitUntil(()=>task.IsCompleted);Assert.That(task.Result,Is.True,session.LastFailure);Assert.That(session.EnterGame(),Is.True);
            var end=Time.realtimeSinceStartup+10;while(Time.realtimeSinceStartup<end){yield return new WaitForSecondsRealtime(.02f);player=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(player!=null)break;}
            Assert.That(player,Is.Not.Null);player.Motor.enabled=player.GetComponent<PlayerInputAdapter>().enabled=false;
            var world=NetworkCreatureWorld.Active;world.Clock.enabled=world.Animals.enabled=world.Night.enabled=world.Goblins.enabled=false;
        }
        [UnityTest]public IEnumerator RestoredHostProcessingRegistersAfterSpawnAndFishingCancellationPublishesNone()
        {
            SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();session=NetworkSession.Instance;
            session.WorldRepository=new SaveRepository(Path.Combine(Application.temporaryCachePath,"lowfire-gamefeel-"+Guid.NewGuid().ToString("N"),"world.json"));session.ProfileOverride=Guid.NewGuid().ToString("N");session.NewWorldGenerationVersion=2;yield return Host();
            var world=NetworkCreatureWorld.Active;var bag=player.GetComponent<PlayerInventory>();var f=UnityEngine.Object.FindObjectsByType<NetworkFacility>(FindObjectsSortMode.None).Single(x=>x.Station!=null && x.Station.Definition.Station==StationKind.Furnace);
            player.transform.position=f.transform.position+Vector3.down;player.GetComponent<Rigidbody2D>().position=player.transform.position;Physics2D.SyncTransforms();var recipe=world.Catalog.Recipe("iron_ingot");foreach(var c in recipe.Ingredients)bag.Model.Add(c.Item,c.Count*3);
            Assert.That(player.GetComponent<NetworkFacilityInteractor>().Open(f),Is.True);f.Processor.enabled=false;Assert.That(f.Processor.AddAll(recipe,bag),Is.GreaterThan(0));Assert.That(f.Processor.Queue.WaitingCount,Is.EqualTo(2));Assert.That(session.TrySaveWorld(),Is.True);
            session.Leave();yield return new WaitUntil(()=>session.State==NetworkSessionState.Idle);yield return new WaitForSecondsRealtime(.2f);yield return Host();
            f=UnityEngine.Object.FindObjectsByType<NetworkFacility>(FindObjectsSortMode.None).Single(x=>x.Station!=null && x.Station.Definition.Station==StationKind.Furnace);
            yield return new WaitForSeconds(6.5f);Assert.That(f.Processor.OutputCount,Is.EqualTo(3));Assert.That(f.Processor.Queue.WaitingCount,Is.Zero);
            world=NetworkCreatureWorld.Active;bag=player.GetComponent<PlayerInventory>();bag.Model.Add(world.Catalog.Item("fishing_rod"),1);bag.SelectSlot(Enumerable.Range(0,bag.Model.Capacity).First(i=>bag.Model.GetSlot(i).Item==world.Catalog.Item("fishing_rod")));
            player.transform.position=new Vector2(world.World.Map.Continental.RiverAt(1000)+7,1000);player.GetComponent<Rigidbody2D>().position=player.transform.position;world.World.Chunks.Reconcile();world.World.Chunks.DrainPending();player.Motor.Avatar.Facing.Face(Vector2.left);yield return null;
            Assert.That(player.GetComponent<PlayerFishing>().Use(),Is.True);var activity=player.GetComponent<NetworkActivities>();Assert.That(activity.Activity.Value.Kind,Is.EqualTo(ActivityKind.Fishing));
            player.transform.position+=Vector3.up*2;yield return null;Assert.That(activity.Activity.Value.Kind,Is.EqualTo(ActivityKind.None));
        }
        [UnityTearDown]public IEnumerator Close(){if(session!=null && session.State!=NetworkSessionState.Idle){session.Leave(true,true);yield return new WaitUntil(()=>session.State==NetworkSessionState.Idle);}Time.timeScale=1;}
    }
}
