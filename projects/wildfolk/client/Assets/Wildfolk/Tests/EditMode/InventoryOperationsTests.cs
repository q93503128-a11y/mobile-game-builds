using NUnit.Framework;
using UnityEditor;

namespace Wildfolk.Tests
{
    public sealed class InventoryOperationsTests
    {
        private ItemDefinition wood,stone,axe;
        private RecipeDefinition bench;
        [SetUp] public void LoadData()
        {
            wood=AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Wildfolk/Data/Wood.asset");
            stone=AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Wildfolk/Data/Stone.asset");
            axe=AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Wildfolk/Data/Axe.asset");
            bench=AssetDatabase.LoadAssetAtPath<RecipeDefinition>("Assets/Wildfolk/Data/WorkbenchRecipe.asset");
        }
        [Test] public void WholeStackMovesBetweenStorageAndHotbarWithOneNotification()
        {
            var model=new InventoryModel(36);model.Add(wood,7);var events=0;model.Changed+=()=>events++;
            Assert.That(model.MoveStack(0,20),Is.True);Assert.That(model.GetSlot(0).IsEmpty,Is.True);
            Assert.That(model.GetSlot(20).Count,Is.EqualTo(7));Assert.That(events,Is.EqualTo(1));
            Assert.That(model.MoveStack(20,8),Is.True);Assert.That(model.GetSlot(8).Count,Is.EqualTo(7));
        }
        [Test] public void MergeStopsAtMaxStackAndKeepsOverflowAtSource()
        {
            var model=new InventoryModel(36);model.Add(wood,70);
            Assert.That(model.MoveStack(0,1),Is.True);
            Assert.That(model.GetSlot(0).Count,Is.EqualTo(20));Assert.That(model.GetSlot(1).Count,Is.EqualTo(50));
            Assert.That(model.CountItem(wood),Is.EqualTo(70));
        }
        [Test] public void FullSameItemDestinationAndSameSlotDoNotMutateOrNotify()
        {
            var model=new InventoryModel(36);model.Add(wood,70);var events=0;model.Changed+=()=>events++;
            Assert.That(model.MoveStack(1,0),Is.False);Assert.That(model.MoveStack(0,0),Is.False);
            Assert.That(model.MoveStack(35,0),Is.False);Assert.That(events,Is.Zero);
            Assert.That(model.CountItem(wood),Is.EqualTo(70));
        }
        [Test] public void DifferentItemsSwapWithoutChangingCounts()
        {
            var model=new InventoryModel(36);model.Add(wood,7);model.Add(axe,1);
            model.MoveStack(0,1);Assert.That(model.GetSlot(0).Item,Is.SameAs(axe));
            Assert.That(model.GetSlot(1).Item,Is.SameAs(wood));Assert.That(model.CountItem(wood),Is.EqualTo(7));
        }
        [Test] public void MissingMaterialsNeverConsumeAnything()
        {
            var model=new InventoryModel(36);model.Add(wood,5);var events=0;model.Changed+=()=>events++;
            Assert.That(model.TryExchange(bench.Ingredients,bench.Result,1),Is.EqualTo(ExchangeResult.MissingMaterials));
            Assert.That(model.CountItem(wood),Is.EqualTo(5));Assert.That(events,Is.Zero);
        }
        [Test] public void FullInventoryRejectsCraftAtomically()
        {
            var model=new InventoryModel(2);model.Add(wood,50);model.Add(stone,50);var events=0;model.Changed+=()=>events++;
            Assert.That(model.TryExchange(bench.Ingredients,bench.Result,1),Is.EqualTo(ExchangeResult.NoSpace));
            Assert.That(model.CountItem(wood),Is.EqualTo(50));Assert.That(model.CountItem(stone),Is.EqualTo(50));
            Assert.That(model.CountItem(bench.Result),Is.Zero);Assert.That(events,Is.Zero);
        }
        [Test] public void FullyConsumedMaterialSlotCanHoldCraftResult()
        {
            var model=new InventoryModel(1);model.Add(wood,6);var events=0;model.Changed+=()=>events++;
            Assert.That(model.TryExchange(bench.Ingredients,bench.Result,1),Is.EqualTo(ExchangeResult.Success));
            Assert.That(model.GetSlot(0).Item,Is.SameAs(bench.Result));Assert.That(model.GetSlot(0).Count,Is.EqualTo(1));
            Assert.That(events,Is.EqualTo(1));
        }
        [Test] public void MultipleResultStacksCannotPartiallyConsumeIngredients()
        {
            var model=new InventoryModel(1);model.Add(wood,6);
            Assert.That(model.TryExchange(bench.Ingredients,axe,2),Is.EqualTo(ExchangeResult.NoSpace));
            Assert.That(model.CountItem(wood),Is.EqualTo(6));Assert.That(model.CountItem(axe),Is.Zero);
        }
        [Test] public void PlacementConsumptionRequiresMatchingSelectedItem()
        {
            var model=new InventoryModel(36);model.Add(wood,3);
            Assert.That(model.ConsumeSlot(0,bench.Result,1),Is.False);Assert.That(model.CountItem(wood),Is.EqualTo(3));
            Assert.That(model.ConsumeSlot(0,wood,1),Is.True);Assert.That(model.GetSlot(0).Count,Is.EqualTo(2));
        }
    }
}
