using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class NetworkItemModelTests
    {
        private ItemDefinition Wood=>AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Wildfolk/Data/Wood.asset");
        [Test]public void DropFailureAndExceptionRestoreOriginalStackWithoutPublishing()
        {
            var model=new InventoryModel(36);model.Add(Wood,2);var changes=0;model.Changed+=()=>changes++;
            Assert.That(model.TryDropSlot(0,Wood,1,()=>false),Is.False);Assert.That(model.GetSlot(0).Count,Is.EqualTo(2));Assert.That(changes,Is.Zero);
            Assert.Throws<InvalidOperationException>(()=>model.TryDropSlot(0,Wood,1,()=>throw new InvalidOperationException()));Assert.That(model.GetSlot(0).Count,Is.EqualTo(2));Assert.That(changes,Is.Zero);
            Assert.That(model.TryDropSlot(0,Wood,1,()=>true),Is.True);Assert.That(model.GetSlot(0).Count,Is.EqualTo(1));Assert.That(changes,Is.EqualTo(1));
            Assert.That(model.TryDropSlot(0,Wood,2,()=>true),Is.False);Assert.That(model.TryDropSlot(0,Wood,0,()=>true),Is.False);
        }
        [Test]public void ReplicaInventoryRejectsMutationAndFullStackPickupIsAtomic()
        {
            var model=new InventoryModel(1);model.Add(Wood,49);Assert.That(model.TryAddAmounts(new[]{new ItemAmount{Item=Wood,Count=2}}),Is.False);Assert.That(model.CountItem(Wood),Is.EqualTo(49));
            model.IsReadOnly=true;Assert.That(model.Add(Wood,1),Is.Zero);Assert.That(model.ConsumeSlot(0,Wood,1),Is.False);Assert.That(model.TryDropSlot(0,Wood,1,()=>true),Is.False);Assert.That(model.TryAddAt(0,Wood),Is.False);Assert.That(model.CountItem(Wood),Is.EqualTo(49));
        }
        [Test]public void EveryResourceAndGroundDropUsesRegisteredNetworkPrefab()
        {
            var go=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkEcology.prefab");var world=go.GetComponent<NetworkCreatureWorld>();
            foreach(var rule in world.World.Definition.Resources)Assert.That(world.Prefabs.Single(e=>e.Source==rule.Prefab.gameObject).NetworkPrefab.GetComponent<NetworkResource>(),Is.Not.Null);
            Assert.That(world.Prefabs.Single(e=>e.Source==world.World.Pickup.gameObject).NetworkPrefab.GetComponent<NetworkGroundItem>(),Is.Not.Null);
            var player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkPlayer.prefab");Assert.That(player.GetComponent<NetworkInventory>(),Is.Not.Null);Assert.That(player.GetComponent<PlayerItemDrop>(),Is.Not.Null);Assert.That(player.GetComponent<PlayerInputCommands>().AllowInventory,Is.True);
        }
    }
}
