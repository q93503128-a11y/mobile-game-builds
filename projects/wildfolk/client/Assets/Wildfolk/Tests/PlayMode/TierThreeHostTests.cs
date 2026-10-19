using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class TierThreeHostTests
    {
        private NetworkSession session;private NetworkPlayer player;
        private IEnumerator Host()
        {var task=session.Host(ConnectionRoute.LocalNetwork);yield return new WaitUntil(()=>task.IsCompleted);Assert.That(task.Result,Is.True,session.LastFailure);Assert.That(session.EnterGame(),Is.True);var end=Time.realtimeSinceStartup+45;while(Time.realtimeSinceStartup<end){yield return new WaitForSecondsRealtime(.05f);player=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(player!=null)break;}Assert.That(player,Is.Not.Null);player.Motor.enabled=player.GetComponent<PlayerInputAdapter>().enabled=false;var world=NetworkCreatureWorld.Active;world.Clock.enabled=world.Animals.enabled=world.Night.enabled=world.Goblins.enabled=false;}
        [UnityTest]public IEnumerator HostAuthorityPersistsTurretAmmoModulesHealthAndTeamBeaconThroughRehost()
        {
            SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();session=NetworkSession.Instance;session.WorldRepository=new SaveRepository(Path.Combine(Application.temporaryCachePath,"lowfire-tier3-"+Guid.NewGuid().ToString("N"),"world.json"));session.ProfileOverride=Guid.NewGuid().ToString("N");session.NewWorldGenerationVersion=3;yield return Host();
            var world=NetworkCreatureWorld.Active;var catalog=world.Catalog;var bag=player.GetComponent<PlayerInventory>();world.World.Chunks.DrainPending();var t=NetworkSpawnFactory.Spawn(catalog.Item("ballista").Placement.Prefab,(Vector2)player.transform.position+Vector2.right*2,Quaternion.identity).GetComponent<WorldTurret>();var beacon=NetworkSpawnFactory.Spawn(catalog.Item("signal_beacon").Placement.Prefab,(Vector2)player.transform.position+Vector2.left*2,Quaternion.identity);world.FlushPending();yield return null;
            var turretId=t.GetComponent<WorldPlaceable>().SaveId;var beaconId=beacon.SaveId;var net=t.GetComponent<NetworkTurret>();Assert.That(net.IsServer && net.IsSpawned,Is.True);bag.Model.Add(catalog.Item("ballista_bolt"),27);var slot=Enumerable.Range(0,bag.Model.Capacity).First(i=>bag.Model.GetSlot(i).Item==catalog.Item("ballista_bolt"));Assert.That(player.GetComponent<NetworkTierThreePlayer>().Operate(t,TurretOperation.Deposit,slot),Is.True);
            foreach(var id in new[]{"ammo_box","rotation_gear","reinforced_frame","auto_loader","auto_aim"}){bag.Model.Add(catalog.Item(id),1);slot=Enumerable.Range(0,bag.Model.Capacity).First(i=>bag.Model.GetSlot(i).Item==catalog.Item(id));Assert.That(player.GetComponent<NetworkTierThreePlayer>().Operate(t,TurretOperation.Install,slot),Is.True);}
            t.GetComponent<Health>().ApplyDamage(17);var hp=t.GetComponent<Health>().Current;var nav=player.GetComponent<PlayerNavigation>();nav.SelectNavigation(NavigationMode.Beacon,beaconId);Assert.That(session.TrySaveWorld(),Is.True);
            session.Leave();yield return new WaitUntil(()=>session.State==NetworkSessionState.Idle);yield return new WaitForSecondsRealtime(.3f);yield return Host();world=NetworkCreatureWorld.Active;world.World.Chunks.DrainPending();yield return null;
            var restored=WorldPlaceable.Active.Single(p=>p.SaveId==turretId).GetComponent<WorldTurret>();Assert.That(restored.AmmoCount(),Is.EqualTo(27));Assert.That(restored.AmmoSlots,Is.EqualTo(3));Assert.That(restored.Modules.All(m=>m!=null),Is.True);Assert.That(restored.GetComponent<Health>().Current,Is.EqualTo(hp));Assert.That(restored.GetComponent<Health>().Maximum,Is.EqualTo(270));Assert.That(BeaconDirectory.Points.ContainsKey(beaconId),Is.True);Assert.That(player.GetComponent<PlayerNavigation>().BeaconId,Is.EqualTo(beaconId));Assert.That(player.GetComponent<PlayerNavigation>().Mode,Is.EqualTo(NavigationMode.Beacon));
        }
        [UnityTearDown]public IEnumerator Close(){if(session!=null && session.State!=NetworkSessionState.Idle){session.Leave(true,true);yield return new WaitUntil(()=>session.State==NetworkSessionState.Idle);}Time.timeScale=1;}
    }
}
