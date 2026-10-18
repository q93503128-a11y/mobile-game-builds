using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class WorldFoundationHostTests
    {
        private NetworkSession session;private NetworkCreatureWorld net;private NetworkPlayer player;private SaveRepository repository;
        private IEnumerator Enter()
        {
            var task=session.Host(ConnectionRoute.LocalNetwork);for(var i=0;i<600 && !task.IsCompleted;i++)yield return null;Assert.That(task.IsCompleted && task.Result,Is.True,session.LastFailure);Assert.That(session.EnterGame(),Is.True);
            var end=Time.realtimeSinceStartup+15;while(Time.realtimeSinceStartup<end){yield return null;player=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(player!=null)break;}Assert.That(player,Is.Not.Null);
            net=NetworkCreatureWorld.Active;net.Clock.enabled=net.Goblins.enabled=net.Night.enabled=false;player.Motor.enabled=false;player.GetComponent<PlayerInputAdapter>().enabled=false;player.GetComponent<NetworkInventory>().enabled=false;net.World.Chunks.Reconcile();net.World.Chunks.DrainPending();net.FlushPending();yield return null;
        }
        private void Move(Vector2 p){player.Commands.Session.CloseInventory();player.transform.position=p;player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();net.World.Chunks.Reconcile();net.World.Chunks.DrainPending();net.FlushPending();}
        [UnityTest]public IEnumerator CurrentHostKeepsRemoteChunksCargoOreAndCaveAcrossRehost()
        {
            Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();session=NetworkSession.Instance;
            repository=new SaveRepository(Path.Combine(Application.temporaryCachePath,"lowfire-world-v2-"+Guid.NewGuid().ToString("N"),"world.json"));session.WorldRepository=repository;session.ProfileOverride=Guid.NewGuid().ToString("N");session.NewWorldGenerationVersion=2;yield return Enter();
            var world=net.World;Assert.That(world.Map.GenerationVersion,Is.EqualTo(2));var source=world.Map.Continental.GuaranteedResources.First(p=>p.Rule==5);Move(source.Position+Vector2.down*2);yield return null;
            var node=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Single(r=>r.Id==source.Id);node.Node.Health.ApplyDamage(node.Node.Health.Maximum);yield return null;
            var chest=net.Create(net.Catalog.Item("reinforced_chest").Placement.Prefab,source.Position+Vector2.right*12,Quaternion.identity);chest.GetComponent<StorageChest>().Model.Add(net.Catalog.Item("wood"),7);var chestId=chest.SaveId;
            var blast=net.Create(net.Catalog.Item("blast_furnace").Placement.Prefab,source.Position+Vector2.left*12,Quaternion.identity);var process=blast.GetComponent<StationProcessing>();process.enabled=false;var recipe=net.Catalog.Recipe("steel");var bag=player.GetComponent<PlayerInventory>();foreach(var c in recipe.Ingredients)bag.Model.Add(c.Item,c.Count*2);net.FlushPending();Move(source.Position+Vector2.left*12+Vector2.down*2);Assert.That(player.GetComponent<NetworkFacilityInteractor>().Open(blast.GetComponent<NetworkFacility>()),Is.True);Assert.That(process.Add(recipe,bag,1) && process.Add(recipe,bag,1),Is.True);var blastId=blast.SaveId;
            NetworkGroundItem.Drop(net.Catalog.Item("stone"),11,source.Position+Vector2.up*15,null,true);net.FlushPending();Move(Vector2.zero);yield return null;net.Clock.Model.SetElapsed(30);Assert.That(session.TrySaveWorld(),Is.True);
            var saved=NetworkWorldPersistence.Active.Capture();Assert.That(saved.BaseGenerationVersion,Is.EqualTo(2));Assert.That(saved.Resources.Single(r=>r.Id==source.Id).Destroyed,Is.True);Assert.That(saved.Placeables.Single(p=>p.Id==blastId).Processing.Output.Sum(o=>o.Count),Is.EqualTo(2));
            session.Leave();for(var i=0;i<300 && session.State!=NetworkSessionState.Idle;i++)yield return null;yield return new WaitForSeconds(1);yield return Enter();world=net.World;Move(source.Position+Vector2.down*2);yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Any(r=>r.Id==source.Id),Is.False);Assert.That(WorldPlaceable.Active.Single(p=>p.SaveId==chestId).GetComponent<StorageChest>().Model.CountItem(net.Catalog.Item("wood")),Is.EqualTo(7));Assert.That(WorldPlaceable.Active.Single(p=>p.SaveId==blastId).GetComponent<StationProcessing>().OutputCount,Is.EqualTo(2));
            Assert.That(UnityEngine.Object.FindObjectsByType<NetworkGroundItem>(FindObjectsSortMode.None).Any(p=>p.IsSpawned && p.State.Value.Count==11),Is.True);
            var other=UnityEngine.Object.Instantiate(session.PlayerPrefab,new Vector2(1800,500),Quaternion.identity).GetComponent<NetworkPlayer>();other.NetworkObject.Spawn();other.Motor.enabled=false;other.GetComponent<PlayerInputAdapter>().enabled=false;world.Chunks.Reconcile();world.Chunks.DrainPending();net.FlushPending();Assert.That(world.Chunks.ActiveCount,Is.EqualTo(18));Assert.That(world.Chunks.IsActive(other.transform.position),Is.True);other.NetworkObject.Despawn();yield return null;
            var cave=world.Map.Continental.Caves[0];Move(cave.SurfaceEntrances[0]+Vector2.down);yield return null;Assert.That(CavernPortal.Travel(world,player.gameObject,"cave0:0",false),Is.True);world.Chunks.DrainPending();net.FlushPending();yield return null;Assert.That(world.Map.Continental.Underground(player.transform.position),Is.True);Assert.That(session.TrySaveWorld(),Is.True);
            var ids=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Select(r=>r.Id).ToArray();Assert.That(ids.Distinct().Count(),Is.EqualTo(ids.Length));
        }
        [UnityTearDown]public IEnumerator Close(){if(session!=null && session.State!=NetworkSessionState.Idle){session.Leave(true,true);for(var i=0;i<300 && session.State!=NetworkSessionState.Idle;i++)yield return null;}Time.timeScale=1;}
    }
}
