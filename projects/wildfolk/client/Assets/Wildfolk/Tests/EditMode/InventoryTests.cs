using System;
using NUnit.Framework;
using UnityEditor;

namespace Wildfolk.Tests
{
    public sealed class InventoryTests
    {
        private ItemDefinition wood;
        private ItemDefinition stone;
        [SetUp] public void LoadItems()
        {
            wood=AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Wildfolk/Data/Wood.asset");
            stone=AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Wildfolk/Data/Stone.asset");
            Assert.That(wood,Is.Not.Null);Assert.That(stone,Is.Not.Null);
        }

        [Test] public void StartsEmpty()
        {
            var inventory=new InventoryModel(3);
            for(var i=0;i<inventory.Capacity;i++) Assert.That(inventory.GetSlot(i).IsEmpty,Is.True);
        }

        [Test] public void SameItemMergesBeforeUsingAnotherSlot()
        {
            var inventory=new InventoryModel(3);
            inventory.Add(wood,2);inventory.Add(wood,3);
            Assert.That(inventory.GetSlot(0).Count,Is.EqualTo(5));Assert.That(inventory.GetSlot(1).IsEmpty,Is.True);
        }

        [Test] public void DifferentMaterialsDoNotMerge()
        {
            var inventory=new InventoryModel(3);
            inventory.Add(wood,3);inventory.Add(stone,2);
            Assert.That(inventory.GetSlot(0).Item,Is.SameAs(wood));Assert.That(inventory.GetSlot(1).Item,Is.SameAs(stone));
        }

        [Test] public void MaxStackOverflowUsesNextSlot()
        {
            var inventory=new InventoryModel(3);
            Assert.That(inventory.Add(wood,wood.MaxStack+4),Is.EqualTo(wood.MaxStack+4));
            Assert.That(inventory.GetSlot(0).Count,Is.EqualTo(wood.MaxStack));Assert.That(inventory.GetSlot(1).Count,Is.EqualTo(4));
        }

        [Test] public void FullInventoryReturnsUnacceptedAmountWithoutLosingExistingItems()
        {
            var inventory=new InventoryModel(2);
            var requested=wood.MaxStack*3+4;
            var accepted=inventory.Add(wood,requested);
            Assert.That(accepted,Is.EqualTo(wood.MaxStack*2));
            Assert.That(requested-accepted,Is.EqualTo(wood.MaxStack+4));
            Assert.That(inventory.Add(stone,2),Is.Zero);
            Assert.That(inventory.GetSlot(1).Count,Is.EqualTo(wood.MaxStack));
        }

        [Test] public void NotifiesOnceForAnAcceptedBatchAndNeverForRejectedBatch()
        {
            var inventory=new InventoryModel(1);var notifications=0;
            inventory.Changed+=()=>notifications++;
            inventory.Add(wood,wood.MaxStack);inventory.Add(stone,1);
            Assert.That(notifications,Is.EqualTo(1));
        }

        [Test] public void InvalidAmountsCannotMutateInventory()
        {
            var inventory=new InventoryModel(2);
            Assert.Throws<ArgumentOutOfRangeException>(()=>inventory.Add(wood,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>inventory.Add(wood,-1));
            Assert.That(inventory.GetSlot(0).IsEmpty,Is.True);
        }
    }
}
