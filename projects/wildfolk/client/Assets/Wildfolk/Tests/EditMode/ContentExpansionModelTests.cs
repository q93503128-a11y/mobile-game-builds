using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ContentExpansionModelTests
    {
        private static T D<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        [Test]public void CatalogHasRealRecipesFourWeaponFamiliesAndSustainableBasicAmmo()
        {
            var c=D<ContentCatalog>("ContentCatalog");Assert.That(c.Items.Length,Is.EqualTo(134));Assert.That(c.Recipes.Length,Is.EqualTo(111));Assert.That(c.Items.Select(i=>i.Id).Distinct().Count(),Is.EqualTo(134));Assert.That(c.Recipes.Select(i=>i.Id).Distinct().Count(),Is.EqualTo(111));
            foreach(var r in c.Recipes){Assert.That(r.ResultCount,Is.GreaterThan(0));foreach(var cost in r.Ingredients){Assert.That(c.Items.Contains(cost.Item),Is.True,r.Id);Assert.That(cost.Count,Is.GreaterThan(0));}}
            foreach(var id in new[]{"arrow","crossbow_bolt"}){var r=c.Recipe(id);Assert.That(r.ResultCount,Is.EqualTo(12));Assert.That(r.Ingredients.Select(x=>x.Item.Id),Is.EquivalentTo(new[]{"wood","feather"}));}
            Assert.That(c.Recipe("reinforced_bow").Ingredients.Any(x=>x.Item.Id=="bow" && x.Count==1),Is.True);Assert.That(c.Recipe("crossbow").Ingredients.Any(x=>x.Item.Id.Contains("bow")),Is.False);
            foreach(var id in new[]{"wood_spear","stone_spear","iron_spear"}){var t=c.Item(id).Tool;Assert.That(t.Attack.Range,Is.GreaterThan(c.Item("iron_sword").Tool.Attack.Range));Assert.That(t.Attack.ArcAngle,Is.LessThan(60));}
            Assert.That(c.Item("crossbow").Ranged.DrawSeconds+c.Item("crossbow").Ranged.RecoverySeconds,Is.GreaterThan(c.Item("bow").Ranged.DrawSeconds+c.Item("bow").Ranged.RecoverySeconds));
        }
        [Test]public void ContainerStateTransfersAndRollsBackWithoutBecomingNewItems()
        {
            var skin=D<ContentCatalog>("ContentCatalog").Item("waterskin");var a=new InventoryModel(3);var b=new InventoryModel(2);a.Add(skin,1,2,LiquidState.Clean);
            Assert.That(InventoryModel.Transfer(a,0,b,1),Is.True);Assert.That(b.GetSlot(1).Charges,Is.EqualTo(2));Assert.That(b.GetSlot(1).Quality,Is.EqualTo(LiquidState.Clean));
            Assert.That(b.TryDropSlot(1,skin,1,()=>false),Is.False);Assert.That(b.GetSlot(1).Charges,Is.EqualTo(2));
            var copy=new InventoryModel(2);copy.Restore(b.Capture(),D<ContentCatalog>("ContentCatalog"));Assert.That(copy.GetSlot(1).Charges,Is.EqualTo(2));
            Assert.Throws<ArgumentException>(()=>copy.SetWater(1,skin,-1,LiquidState.Raw));Assert.Throws<ArgumentException>(()=>copy.SetWater(1,skin,0,LiquidState.Clean));
        }
        [Test]public void PurificationReservesOnlyFullRawAndRestoresChargesAfterQueueSave()
        {
            var c=D<ContentCatalog>("ContentCatalog");var skin=c.Item("waterskin");var r=c.Recipe("purify_waterskin");var bag=new InventoryModel(1);bag.Add(skin,1,2,LiquidState.Raw);var q=new ProcessingQueue();Assert.That(q.Add(r,1,bag,5,1),Is.False);bag.SetWater(0,skin,3,LiquidState.Raw);Assert.That(q.Add(r,1,bag,5,1),Is.True);
            var state=q.Capture();var restored=new ProcessingQueue();restored.Restore(state,c,1);restored.Advance(20,1,10);Assert.That(restored.Collect(bag),Is.True);Assert.That(bag.GetSlot(0).Charges,Is.EqualTo(3));Assert.That(bag.GetSlot(0).Quality,Is.EqualTo(LiquidState.Clean));Assert.That(restored.Add(r,1,bag,5,1),Is.False);
        }
        [Test]public void CookingRecipesRestoreMoreThanSimpleFoodAndBuildingRolesAreReal()
        {
            var c=D<ContentCatalog>("ContentCatalog");foreach(var id in new[]{"mushroom_stew","meat_stew","fish_stew","mixed_stew"}){var r=c.Recipe(id);Assert.That(r.RequiredStation,Is.EqualTo(StationKind.CookingPot));Assert.That(r.Result.Consumable.HungerRestore,Is.GreaterThan(26));}
            foreach(var id in new[]{"wood_fence_gate","wood_gate","stone_door","stone_fence_gate","stone_gate","reinforced_door","reinforced_gate"})Assert.That(c.Item(id).Placement.Prefab.GetComponent<WorldDoor>(),Is.Not.Null);
            Assert.That(c.Item("wood_gate").Placement.Footprint.x,Is.GreaterThan(c.Item("wood_door").Placement.Footprint.x));Assert.That(c.Item("reinforced_wall").Placement.Vitality.MaximumHealth,Is.GreaterThan(c.Item("stone_wall").Placement.Vitality.MaximumHealth));
            Assert.That(c.Item("spike_trap").Placement.Prefab.GetComponent<SpikeTrap>(),Is.Not.Null);Assert.That(c.Item("lantern").EquippedLight,Is.Not.Null);
        }
        [TestCase(9472026)][TestCase(17491)]public void AddedSourcesAndBudgetDoNotMoveExistingSourceIds(int seed)
        {
            var data=D<WorldGenerationDefinition>("WorldGeneration");var old=UnityEngine.Object.Instantiate(data);old.Resources=old.Resources.Take(8).ToArray();old.NaturalSources=old.NaturalSources.Where(s=>s.ResourceRule<8).ToArray();old.ProgressionCatalog=UnityEngine.Object.Instantiate(data.ProgressionCatalog);old.ProgressionCatalog.Recipes=old.ProgressionCatalog.Recipes.Take(38).ToArray();
            var before=new WorldMap(old,seed,2,false);var after=new WorldMap(data,seed,2,false);
            foreach(var p in before.Resources)Assert.That(after.Resources.Any(x=>x.Id==p.Id && x.Rule==p.Rule && x.Position==p.Position),Is.True,p.Id);
            foreach(var k in before.Resources.Select(p=>WorldChunk.At(p.Position,64)).Distinct().Take(10)){var previous=before.Continental.GenerateChunk(k);var current=after.Continental.GenerateChunk(k);foreach(var p in previous)Assert.That(current.Any(x=>x.Id==p.Id && x.Rule==p.Rule && x.Position==p.Position),Is.True,p.Id);}
            var one=WorldProgressionSupply.RequiredYield(data,1);var four=WorldProgressionSupply.RequiredYield(data,4);foreach(var pair in four.Where(x=>data.Resources[x.Key].Family==ResourceFamily.Mineral)){Assert.That(pair.Value,Is.GreaterThanOrEqualTo(one[pair.Key]));Assert.That(after.Resources.Where(x=>x.Rule==pair.Key).Sum(x=>data.Resources[x.Rule].Prefab.Definition.DropCount),Is.GreaterThanOrEqualTo(pair.Value));}
            Assert.That(data.Resources.Take(8).Single(x=>x.Prefab.name=="SmallPlant").Prefab.Definition.DropItem.Id,Is.EqualTo("plant_fiber"));
            UnityEngine.Object.DestroyImmediate(old.ProgressionCatalog);UnityEngine.Object.DestroyImmediate(old);
        }
    }
}
