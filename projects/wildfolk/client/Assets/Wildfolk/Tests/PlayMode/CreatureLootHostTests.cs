using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class CreatureLootHostTests
    {
        private NetworkSession session;private NetworkPlayer player;
        private IEnumerator Host()
        {var task=session.Host(ConnectionRoute.LocalNetwork);yield return new WaitUntil(()=>task.IsCompleted);Assert.That(task.Result,Is.True,session.LastFailure);Assert.That(session.EnterGame(),Is.True);var end=Time.realtimeSinceStartup+45;while(Time.realtimeSinceStartup<end){yield return new WaitForSecondsRealtime(.05f);player=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(player!=null)break;}Assert.That(player,Is.Not.Null);player.Motor.enabled=player.GetComponent<PlayerInputAdapter>().enabled=false;var world=NetworkCreatureWorld.Active;world.Clock.enabled=world.Animals.enabled=world.Night.enabled=world.Goblins.enabled=false;world.World.Chunks.DrainPending();}
        [UnityTest]public IEnumerator HostRejectsStaleLootAndBagRevisionsAndRehostKeepsSpentSlots()
        {
            SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;UnityEngine.Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();session=NetworkSession.Instance;session.WorldRepository=new SaveRepository(Path.Combine(Application.temporaryCachePath,"lowfire-creature-loot-"+Guid.NewGuid().ToString("N"),"world.json"));session.ProfileOverride=Guid.NewGuid().ToString("N");session.NewWorldGenerationVersion=3;yield return Host();
            var world=NetworkCreatureWorld.Active;var bag=player.GetComponent<PlayerInventory>();var pos=(Vector2)player.transform.position;var camp=world.Goblins.SpawnCamp(pos+Vector2.left*2,"qa-host-camp",0,false);world.FlushPending();yield return null;var net=camp.GetComponent<NetworkCamp>();Assert.That(net.IsSpawned && net.IsServer,Is.True);Assert.That(net.Loot.Count,Is.EqualTo(6));Assert.That(net.Secured.Value,Is.True);Assert.That(camp.SupplyVisual.sprite,Is.SameAs(camp.SupplyIcon));
            player.transform.position=camp.Location.position+Vector3.down;player.GetComponent<Rigidbody2D>().position=player.transform.position;var request=player.GetComponent<NetworkFacilityInteractor>();var inv=player.GetComponent<NetworkInventory>();var revision=camp.State.LootRevision;var bagRevision=inv.Revision.Value;var amount=camp.State.Loot[0].Count;var before=bag.Model.CountItem(world.Catalog.Item("bone_arrow"));Assert.That(request.ExecuteCampLoot(net.NetworkObjectId,0,revision,bagRevision),Is.True);Assert.That(request.ExecuteCampLoot(net.NetworkObjectId,0,revision,bagRevision),Is.False);Assert.That(bag.Model.CountItem(world.Catalog.Item("bone_arrow")),Is.EqualTo(before+amount));Assert.That(net.Opened.Value,Is.True);Assert.That(net.Loot[0].Count,Is.Zero);Assert.That(camp.SupplyVisual.sprite,Is.SameAs(camp.OpenedSprite));
            Assert.That(request.ExecuteCampLoot(net.NetworkObjectId,1,camp.State.LootRevision,bagRevision),Is.False,"stale independent bag revision");var state=JsonUtility.ToJson(camp.State);Assert.That(session.TrySaveWorld(),Is.True);session.Leave();yield return new WaitUntil(()=>session.State==NetworkSessionState.Idle);yield return new WaitForSecondsRealtime(.3f);yield return Host();world=NetworkCreatureWorld.Active;yield return null;camp=world.Goblins.Camps.Single(c=>c.State.Id=="qa-host-camp");Assert.That(JsonUtility.ToJson(camp.State),Is.EqualTo(state));Assert.That(camp.State.Loot[0]==null || camp.State.Loot[0].Count==0,Is.True);Assert.That(player.GetComponent<PlayerInventory>().Model.CountItem(world.Catalog.Item("bone_arrow")),Is.EqualTo(before+amount));
        }
        [UnityTearDown]public IEnumerator Close(){CampLootPanel.CloseFocused();if(session!=null && session.State!=NetworkSessionState.Idle){session.Leave(true,true);yield return new WaitUntil(()=>session.State==NetworkSessionState.Idle);}Time.timeScale=1;}
    }
}
