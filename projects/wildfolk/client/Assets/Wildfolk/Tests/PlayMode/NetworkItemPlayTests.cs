using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class NetworkItemPlayTests
    {
        private NetworkPlayer player;private NetworkCreatureWorld world;
        [UnitySetUp]public IEnumerator Host()
        {
            Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();
            var session=NetworkSession.Instance;var task=session.Host(ConnectionRoute.LocalNetwork);for(var i=0;i<300 && !task.IsCompleted;i++)yield return null;Assert.That(task.Result,Is.True);Assert.That(session.EnterGame(),Is.True);
            for(var i=0;i<300;i++){yield return null;player=Object.FindFirstObjectByType<NetworkPlayer>();if(player!=null && player.IsSpawned)break;}Assert.That(player,Is.Not.Null);world=NetworkCreatureWorld.Active;
            world.Clock.enabled=world.Animals.enabled=world.Goblins.enabled=world.Night.enabled=world.World.Renewal.enabled=false;Freeze(player);yield return null;yield return null;
        }
        private void Freeze(NetworkPlayer actor){actor.GetComponent<PlayerInputAdapter>().enabled=false;actor.GetComponent<NetworkInventory>().enabled=false;actor.Motor.enabled=false;actor.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;}
        private void Place(NetworkPlayer actor,Vector2 position){actor.GetComponent<Rigidbody2D>().position=position;actor.transform.position=position;actor.Motor.Avatar.Facing.Face(Vector2.up);Physics2D.SyncTransforms();}
        private NetworkPlayer Other(Vector2 position){var actor=Object.Instantiate(NetworkSession.Instance.PlayerPrefab,position,Quaternion.identity).GetComponent<NetworkPlayer>();actor.NetworkObject.Spawn();Freeze(actor);return actor;}
        [UnityTest]public IEnumerator SharedResourceFinalHitDropsOnceAndRecoversWithNewIdentity()
        {
            var identity=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).First(r=>r.Node.name.Contains("SmallPlant"));var node=identity.Node;var rule=identity.Rule;var region=world.World.Renewal.Population.Region(rule,node.transform.position);var position=(Vector2)node.transform.position;var id=identity.Id;var netId=node.GetComponent<NetworkObject>().NetworkObjectId;
            Place(player,position+Vector2.down*.7f);var other=Other(position+new Vector2(.1f,-.7f));node.Health.RestoreHealth(1);var deaths=0;node.Health.Died+=()=>deaths++;
            Assert.That(player.Commands.Attack(),Is.True);Assert.That(other.Commands.Attack(),Is.True);yield return new WaitForSeconds(.25f);
            Assert.That(deaths,Is.EqualTo(1));Assert.That(NetworkSession.Instance.Manager.SpawnManager.SpawnedObjects.ContainsKey(netId),Is.False);
            Assert.That(Object.FindObjectsByType<NetworkGroundItem>(FindObjectsSortMode.None).Sum(g=>g.State.Value.Count),Is.EqualTo(1));
            foreach(var r in Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Where(r=>r.Rule==rule && world.World.Renewal.Population.Region(rule,r.transform.position)==region).ToArray())
            {if(world.World.Renewal.Population.Count(rule,region)<world.World.Renewal.Population.Target(rule,region))break;r.Node.Health.ApplyDamage(r.Node.Health.Maximum);}
            yield return null;world.Clock.Model.SetElapsed(world.Clock.Model.Elapsed+world.World.Definition.Resources[rule].RespawnSeconds+1);
            Assert.That(world.World.Renewal.Tick(),Is.GreaterThan(0));yield return null;yield return null;
            var recovered=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).First(r=>r.Id.StartsWith("p"));Assert.That(recovered.Id,Is.Not.EqualTo(id));Assert.That(recovered.GetComponent<NetworkResource>().IsSpawned,Is.True);Assert.That(recovered.GetComponent<NetworkResource>().State.Value.Id.ToString(),Is.EqualTo(recovered.Id));
        }
        [UnityTest]public IEnumerator PickupRaceFullBagDropReplayAndRetiredPlayerCannotDuplicate()
        {
            var inventory=player.GetComponent<NetworkInventory>();var other=Other(player.transform.position);var second=other.GetComponent<NetworkInventory>();var wood=world.Catalog.Item("wood");var stone=world.Catalog.Item("stone");inventory.Inventory.Model.Add(wood,wood.MaxStack*36);
            var ground=NetworkGroundItem.Drop(stone,5,player.transform.position,null,false);yield return null;yield return new WaitForSeconds(.8f);var id=ground.NetworkObjectId;
            Assert.That(inventory.Execute(InventoryOperation.Pickup,0,0,0,id,inventory.Revision.Value),Is.False);Assert.That(ground.State.Value.Count,Is.EqualTo(5));
            Assert.That(second.Execute(InventoryOperation.Pickup,0,0,0,id,second.Revision.Value),Is.True);Assert.That(inventory.Execute(InventoryOperation.Pickup,0,0,0,id,inventory.Revision.Value),Is.False);Assert.That(second.Inventory.Model.CountItem(stone),Is.EqualTo(5));
            other.Commands.Session.OpenInventory();var revision=second.Revision.Value;Assert.That(second.Execute(InventoryOperation.Drop,0,0,1,0,revision),Is.True);Assert.That(Object.FindObjectsByType<NetworkGroundItem>(FindObjectsSortMode.None).Count(g=>g.IsSpawned),Is.EqualTo(1));Assert.That(second.Execute(InventoryOperation.Drop,0,0,1,0,revision),Is.False);Assert.That(second.Inventory.Model.GetSlot(0).Count,Is.EqualTo(4));yield return null;yield return null;
            Assert.That(Object.FindObjectsByType<NetworkGroundItem>(FindObjectsSortMode.None).Sum(g=>g.State.Value.Count),Is.EqualTo(1));
            var remaining=Object.FindFirstObjectByType<NetworkGroundItem>().NetworkObjectId;var oldRevision=second.Revision.Value;other.NetworkObject.Despawn();Assert.That(second.Execute(InventoryOperation.Pickup,0,0,0,remaining,oldRevision),Is.False);
        }
        [UnityTest]public IEnumerator CreatureLootAndNearbySessionRewardsCommitExactlyOnce()
        {
            var near=Other((Vector2)player.transform.position+Vector2.right*8);var far=Other((Vector2)player.transform.position+Vector2.right*40);
            var enemy=PrefabPool.Spawn(world.Night.SecondaryPrefab,(Vector2)player.transform.position+Vector2.up*3,Quaternion.identity);enemy.Session=world.Session;enemy.PlayerHealth=player.Health;enemy.Clock=world.Clock;enemy.Lifecycle.Clock=world.Clock;enemy.Lifecycle.Definition=world.Night.Definition;enemy.Variant.SetEquipped(true);yield return null;yield return null;enemy.enabled=false;
            var table=enemy.GetComponent<LootOnDeath>().Table;var a=player.GetComponent<PlayerProgression>();var b=near.GetComponent<PlayerProgression>();var c=far.GetComponent<PlayerProgression>();var xp=a.Experience;var gold=a.Gold;var count=world.DeathCount;
            Random.InitState(927);enemy.OwnHealth.ApplyDamage(enemy.OwnHealth.Maximum,true,player.GetComponent<PlayerInventory>());Assert.That(enemy.OwnHealth.ApplyDamage(1),Is.Zero);var result=world.LastDeath;world.RecordDeath(result);yield return null;yield return null;
            Assert.That(world.DeathCount,Is.EqualTo(count+1));Assert.That(a.Experience,Is.EqualTo(xp+table.Experience));Assert.That(a.Gold,Is.EqualTo(gold+table.Gold));Assert.That(b.Experience,Is.EqualTo(table.Experience));Assert.That(b.Gold,Is.EqualTo(table.Gold));Assert.That(c.Experience,Is.Zero);Assert.That(c.Gold,Is.Zero);
            Assert.That(Object.FindObjectsByType<NetworkGroundItem>(FindObjectsSortMode.None).Sum(g=>g.State.Value.Count),Is.EqualTo(result.Items.Sum(i=>i.Count)));
            Assert.That(player.GetComponent<NetworkInventory>().Rewards.Value.Gold,Is.EqualTo(a.Gold));
        }
        [UnityTest]public IEnumerator ExistingInventoryUiSplitsMovesSelectsAndEquipsThroughHost()
        {
            var inventory=player.GetComponent<NetworkInventory>();var model=inventory.Inventory.Model;var wood=world.Catalog.Item("wood");model.Add(wood,9);
            var ui=Object.FindFirstObjectByType<NetworkInventoryUi>();ui.Bag.onClick.Invoke();yield return null;var panel=ui.GetComponent<InventoryPanel>();var drag=ui.GetComponent<InventoryDragController>();
            Assert.That(panel.View.activeSelf,Is.True);Assert.That(panel.Slots.Length,Is.EqualTo(36));Assert.That(ui.GetComponentInChildren<HotbarView>(true).Slots.Length,Is.EqualTo(9));Assert.That(panel.CraftingButton,Is.Not.Null);Assert.That(panel.Storage,Is.Not.Null);
            drag.HoldPartial(0,panel.Slots[0],false,4);Assert.That(drag.PlaceHeld(20,false),Is.True);Assert.That(model.GetSlot(0).Count,Is.EqualTo(5));Assert.That(model.GetSlot(20).Count,Is.EqualTo(4));panel.Select(20);Assert.That(inventory.Selected.Value,Is.EqualTo(20));
            var armor=world.Catalog.Items.First(i=>i.Armor!=null && i.Armor.Slot==ArmorSlot.Head);model.Add(armor,1);var slot=Enumerable.Range(0,36).First(i=>!model.GetSlot(i).IsEmpty && model.GetSlot(i).Item==armor);Assert.That(player.Equipment.TryEquipFromSlot(slot),Is.True);Assert.That(player.Equipment.HeadArmor,Is.SameAs(armor));Assert.That(player.Equipment.Unequip(ArmorSlot.Head,slot),Is.True);
            Assert.That(inventory.Slots.Count,Is.EqualTo(36));Assert.That(inventory.Slots.ReadPerm,Is.EqualTo(NetworkVariableReadPermission.Owner));Assert.That(inventory.Revision.ReadPerm,Is.EqualTo(NetworkVariableReadPermission.Owner));
            var tool=world.Catalog.Items.First(i=>i.Tool!=null);model.Add(tool,1);var toolSlot=Enumerable.Range(0,36).First(i=>!model.GetSlot(i).IsEmpty && model.GetSlot(i).Item==tool);panel.Select(toolSlot);Assert.That(ui.AttackIcon.sprite,Is.SameAs(tool.Icon));
            player.Commands.Session.CloseInventory();var before=model.CountItem(wood);inventory.enabled=true;NetworkGroundItem.Drop(wood,1,player.transform.position,null,false);yield return new WaitForSeconds(1.1f);Assert.That(model.CountItem(wood),Is.EqualTo(before+1));
        }
        [UnityTearDown]public IEnumerator Cleanup(){if(NetworkSession.Instance!=null && NetworkSession.Instance.State!=NetworkSessionState.Idle){NetworkSession.Instance.Leave();for(var i=0;i<300 && NetworkSession.Instance.State!=NetworkSessionState.Idle;i++)yield return null;}Time.timeScale=1;}
    }
}
