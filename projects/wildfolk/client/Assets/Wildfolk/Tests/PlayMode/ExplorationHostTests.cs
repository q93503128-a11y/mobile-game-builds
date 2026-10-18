using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class ExplorationHostTests
    {
        private NetworkSession session;private NetworkCreatureWorld net;private NetworkPlayer player;private SaveRepository repository;
        private IEnumerator Host()
        {
            var task=session.Host(ConnectionRoute.LocalNetwork);for(var i=0;i<600 && !task.IsCompleted;i++)yield return null;Assert.That(task.IsCompleted && task.Result,Is.True,session.LastFailure);Assert.That(session.EnterGame(),Is.True);
            for(var i=0;i<600;i++){yield return null;player=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(player!=null)break;}Assert.That(player,Is.Not.Null);
            net=NetworkCreatureWorld.Active;net.Clock.enabled=net.Goblins.enabled=net.Night.enabled=false;player.Motor.enabled=false;player.GetComponent<PlayerInputAdapter>().enabled=false;player.GetComponent<NetworkInventory>().enabled=false;net.World.Chunks.Reconcile();net.World.Chunks.DrainPending();net.FlushPending();yield return null;
        }
        private void Move(Vector2 p){player.Commands.Session.CloseInventory();player.transform.position=p;player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();net.World.Chunks.Reconcile();net.World.Chunks.DrainPending();net.FlushPending();}
        [UnityTest]public IEnumerator TorchAuthorityAtomicityFourRegionUnionAndHomeRehost()
        {
            SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();session=NetworkSession.Instance;
            repository=new SaveRepository(Path.Combine(Application.temporaryCachePath,"lowfire-exploration-"+Guid.NewGuid().ToString("N"),"world.json"));session.WorldRepository=repository;session.ProfileOverride=Guid.NewGuid().ToString("N");session.NewWorldGenerationVersion=2;yield return Host();
            var bag=player.GetComponent<PlayerInventory>();var torch=net.Catalog.Item("torch");bag.Model.Add(torch,4);var slot=Enumerable.Range(0,9).First(i=>bag.Model.GetSlot(i).Item==torch);bag.SelectSlot(slot);yield return null;
            var construction=player.GetComponent<NetworkConstruction>();var placement=player.GetComponent<PlayerPlacement>();player.Motor.Avatar.Facing.Face(Vector2.down);var point=(Vector2)player.transform.position+Vector2.down*placement.Settings.PlacementDistance;
            foreach(var resource in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(point,resource.transform.position)<4)resource.gameObject.SetActive(false);Physics2D.SyncTransforms();
            var revision=player.GetComponent<NetworkInventory>().Revision.Value;Assert.That(construction.ExecutePlace(revision,slot,point+Vector2.one*100),Is.False);Assert.That(bag.Model.CountItem(torch),Is.EqualTo(4));
            Assert.That(construction.ExecutePlace(revision,slot,point),Is.True);Assert.That(bag.Model.CountItem(torch),Is.EqualTo(3));Assert.That(construction.ExecutePlace(revision,slot,point),Is.False);Assert.That(bag.Model.CountItem(torch),Is.EqualTo(3));
            var placed=WorldPlaceable.Active.Single(p=>p.Definition.Item==torch);var torchId=placed.SaveId;Assert.That(placed.GetComponent<NetworkBuilding>().IsSpawned,Is.True);
            var home=WorldPlaceable.Active.First(p=>p.Definition.Station==StationKind.Workbench);Move((Vector2)home.transform.position+Vector2.down*2);Assert.That(player.GetComponent<NetworkNavigation>().Assign(home.GetComponent<Unity.Netcode.NetworkObject>().NetworkObjectId),Is.True);var homeId=home.SaveId;
            var actors=new System.Collections.Generic.List<NetworkPlayer>();foreach(var p in new[]{new Vector2(1800,500),new Vector2(-1800,500),new Vector2(0,-2500)})
            {var other=UnityEngine.Object.Instantiate(session.PlayerPrefab,p,Quaternion.identity).GetComponent<NetworkPlayer>();other.NetworkObject.Spawn();other.Motor.enabled=false;other.GetComponent<PlayerInputAdapter>().enabled=false;actors.Add(other);}
            net.World.Chunks.Reconcile();net.World.Chunks.DrainPending();net.FlushPending();yield return null;Assert.That(net.World.Chunks.ActiveCount,Is.InRange(36,39));var ids=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Select(r=>r.Id).ToArray();Assert.That(ids.Distinct().Count(),Is.EqualTo(ids.Length));
            foreach(var other in actors)other.NetworkObject.Despawn();Move(new Vector2(3000,1000));yield return null;Assert.That(player.GetComponent<PlayerNavigation>().TryHome(net.World,out _),Is.True);Assert.That(session.TrySaveWorld(),Is.True);var worldId=NetworkWorldPersistence.Active.Capture().Multiplayer.WorldId;
            session.Leave();for(var i=0;i<300 && session.State!=NetworkSessionState.Idle;i++)yield return null;yield return new WaitForSeconds(1);yield return Host();
            Assert.That(NetworkWorldPersistence.Active.Capture().Multiplayer.WorldId,Is.EqualTo(worldId));Assert.That(player.GetComponent<PlayerNavigation>().HomeId,Is.EqualTo(homeId));Move(point+Vector2.right*3);yield return null;Assert.That(WorldPlaceable.Active.Count(f=>f.SaveId==torchId),Is.EqualTo(1));Assert.That(WorldPlaceable.Active.Single(f=>f.SaveId==torchId).GetComponent<GameplayLight>(),Is.Null);Assert.That(WorldPlaceable.Active.Single(f=>f.SaveId==torchId).GetComponentInChildren<GameplayLight>().Definition.DetersEnemies,Is.False);
        }
        [UnityTearDown]public IEnumerator Close(){if(session!=null && session.State!=NetworkSessionState.Idle){session.Leave(true,true);for(var i=0;i<300 && session.State!=NetworkSessionState.Idle;i++)yield return null;}Time.timeScale=1;}
    }
}
