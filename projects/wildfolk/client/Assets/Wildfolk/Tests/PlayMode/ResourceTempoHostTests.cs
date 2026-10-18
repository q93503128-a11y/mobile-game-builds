using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class ResourceTempoHostTests
    {
        private NetworkSession session;private NetworkCreatureWorld net;private NetworkPlayer player;
        private IEnumerator Host()
        {
            var task=session.Host(ConnectionRoute.LocalNetwork);var deadline=Time.realtimeSinceStartup+15;
            while(!task.IsCompleted && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(task.IsCompleted && task.Result,Is.True,session.LastFailure);Assert.That(session.EnterGame(),Is.True);
            deadline=Time.realtimeSinceStartup+15;do{yield return null;player=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);}while(player==null && Time.realtimeSinceStartup<deadline);
            Assert.That(player,Is.Not.Null);net=NetworkCreatureWorld.Active;net.Clock.enabled=net.Night.enabled=net.Goblins.enabled=false;player.Motor.enabled=false;player.GetComponent<PlayerInputAdapter>().enabled=false;
        }
        private void Move(Vector2 p){player.transform.position=p;player.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();net.World.Chunks.Reconcile();net.World.Chunks.DrainPending();net.FlushPending();}
        [UnityTest]public IEnumerator AuthorityRecoversDistinctRegionsAndRehostDoesNotDuplicateSavedSources()
        {
            Assert.That(DevelopmentProfile.Isolated,Is.True);SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();session=NetworkSession.Instance;
            session.WorldRepository=new SaveRepository(Path.Combine(DevelopmentProfile.Directory,"resource-tempo-host-"+Guid.NewGuid().ToString("N")+".json"));session.ProfileOverride=Guid.NewGuid().ToString("N");session.NewWorldGenerationVersion=2;yield return Host();
            var regions=net.World.Map.Resources.Where(p=>p.Rule==2 && p.Position.magnitude>300).Select(p=>WorldChunk.At(p.Position,64)).Distinct().Take(4).ToArray();Assert.That(regions.Length,Is.EqualTo(4));
            foreach(var region in regions)
            {
                Move(region.Center(64));yield return null;var originals=UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Where(x=>x.Rule==2 && WorldChunk.At(x.transform.position,64).Equals(region)).ToArray();
                foreach(var node in originals){Assert.That(node.GetComponent<NetworkResource>().IsServer,Is.True);node.Node.Harvest(net.Catalog.Item("stone_pickaxe").Tool,10000,player.GetComponent<PlayerInventory>());}yield return null;
            }
            var pending=net.World.Renewal.CaptureRecovery().Pending;foreach(var region in regions)Assert.That(pending.Count(t=>t.Rule==2 && t.Region==WorldMap.Encode(region)),Is.EqualTo(1));
            net.Clock.Model.SetElapsed(pending.Max(t=>t.Due)+1);var first=regions[0];Move(first.Center(64)+Vector2.right*40);yield return null;
            for(var i=0;i<40 && !net.World.Renewal.Capture().Any(s=>!s.Destroyed && s.Id.StartsWith("p") && s.Rule==2);i++){net.World.Renewal.Tick();net.FlushPending();yield return null;}
            var recovered=net.World.Renewal.Capture().First(s=>!s.Destroyed && s.Id.StartsWith("p") && s.Rule==2);var recoveryId=recovered.Id;Assert.That(session.TrySaveWorld(),Is.True);
            session.Leave();var end=Time.realtimeSinceStartup+10;while(session.State!=NetworkSessionState.Idle && Time.realtimeSinceStartup<end)yield return null;yield return new WaitForSecondsRealtime(.5f);yield return Host();Move(recovered.Position+Vector2.right*30);yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Count(s=>s.Id==recoveryId),Is.EqualTo(1));net.World.Renewal.MaterializeRecovered(WorldChunk.At(recovered.Position,64),net.World.Environment);net.FlushPending();yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Count(s=>s.Id==recoveryId),Is.EqualTo(1));Assert.That(net.World.Renewal.CaptureRecovery().Pending.Select(t=>t.Rule+":"+t.Region).Distinct().Count(),Is.EqualTo(net.World.Renewal.CaptureRecovery().Pending.Length));
        }
        [UnityTearDown]public IEnumerator Done(){if(session!=null && session.State!=NetworkSessionState.Idle){session.Leave(true,true);var end=Time.realtimeSinceStartup+10;while(session.State!=NetworkSessionState.Idle && Time.realtimeSinceStartup<end)yield return null;}Time.timeScale=1;}
    }
}
