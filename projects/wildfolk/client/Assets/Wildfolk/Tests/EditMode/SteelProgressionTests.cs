using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
namespace Wildfolk.Tests
{
    public sealed class SteelProgressionTests
    {
        private static T Data<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private ContentCatalog Catalog=>Data<ContentCatalog>("ContentCatalog");
        [Test]public void HarvestMethodsAndMinimumToolsAreIndependent()
        {
            var copper=Data<ResourceDefinition>("CopperOreNodeResource");var coal=Data<ResourceDefinition>("CoalNodeResource");var tin=Data<ResourceDefinition>("TinOreNodeResource");
            Assert.That(copper.CanHarvest(Catalog.Item("stone_pickaxe").Tool),Is.False);Assert.That(copper.CanHarvest(Catalog.Item("iron_pickaxe").Tool),Is.True);
            Assert.That(coal.CanHarvest(Catalog.Item("pickaxe").Tool),Is.False);Assert.That(coal.CanHarvest(Catalog.Item("stone_pickaxe").Tool),Is.True);
            foreach(var id in new[]{"pickaxe","stone_pickaxe","iron_pickaxe","steel_pickaxe"})Assert.That(tin.CanHarvest(Catalog.Item(id).Tool),Is.False);
            Assert.That(tin.HarvestMethod,Is.EqualTo(HarvestMethod.Panning));Assert.That(tin.CanHarvest(Catalog.Item("prospecting_pan").Tool),Is.True);Assert.That(copper.CanHarvest(Catalog.Item("prospecting_pan").Tool),Is.False);
        }
        [Test]public void TechnologyUsesSeparateFacilityAndKeepsLegacyMaterials()
        {
            var steel=Catalog.Recipe("steel");Assert.That(steel.RequiredStation,Is.EqualTo(StationKind.BlastFurnace));Assert.That(Data<PlaceableDefinition>("FurnacePlacement").Station==steel.RequiredStation,Is.False);
            Assert.That(Data<PlaceableDefinition>("BlastFurnacePlacement").Station==steel.RequiredStation,Is.True);
            foreach(var name in new[]{"FurnacePlacement","BlastFurnacePlacement"})Assert.That(Data<PlaceableDefinition>(name).Station==Catalog.Recipe("cooked_meat").RequiredStation,Is.False);
            foreach(var id in new[]{"steel_axe","steel_pickaxe","steel_sword","steel_head","steel_body"}){var r=Catalog.Recipe(id);Assert.That(r.RequiredWorkbenchTier,Is.EqualTo(2));Assert.That(r.Result.Tier.Rank,Is.EqualTo(4));Assert.That(r.Ingredients.Any(c=>c.Item.Id.StartsWith("iron_")),Is.True);Assert.That(r.Ingredients.Any(c=>c.Item.Id=="wood" || c.Item.Id=="leather"),Is.True);}
            Assert.That(Catalog.Recipe("reinforced_chest").Ingredients.Any(c=>c.Item.Id=="iron_ingot"),Is.True);Assert.That(Catalog.Item("reinforced_chest").Placement.StorageSlots,Is.EqualTo(36));
        }
        [Test]public void LegacyCargoAndChestMigrationPreserveContentsExactlyOnce()
        {
            var slots=Enumerable.Range(0,32).Select(i=>new SavedStack()).ToArray();slots[31]=new SavedStack{Item="wood",Count=7};
            var game=new SavedGame{Version=5,Player=new PlayerSnapshot{SeenRecipes=new[]{"bronze_chest","axe","steel_wall"}},Placeables=new[]{new PlaceableSnapshot{Item="reinforced_chest",Inventory=slots},new PlaceableSnapshot{Item="furnace",Processing=new ProcessingSnapshot{Sequential=true,Recipe="steel",Batch=1,Remaining=2,Waiting=new[]{new ProcessingEntry{Recipe="steel",Count=2},new ProcessingEntry{Recipe="iron_ingot",Count=1}},Output=new[]{new ProcessingEntry{Recipe="steel",Count=3}}}}}};
            MetalSaveMigration.NormalizeLegacyContent(game,Catalog);Assert.That(game.Placeables[0].Inventory.Length,Is.EqualTo(36));Assert.That(game.Placeables[0].Inventory[31].Count,Is.EqualTo(7));Assert.That(game.Placeables[0].Inventory[35].Count,Is.Zero);Assert.That(game.Player.SeenRecipes,Is.EqualTo(new[]{"axe"}));
            foreach(var id in new[]{"iron_ingot","coal","steel"})Assert.That(game.Pickups.Where(p=>p.Item==id).Sum(p=>p.Count),Is.EqualTo(3));Assert.That(game.Placeables[1].Processing.Recipe,Is.Null);Assert.That(game.Placeables[1].Processing.Waiting.Single().Recipe,Is.EqualTo("iron_ingot"));
            MetalSaveMigration.NormalizeLegacyContent(game,Catalog);Assert.That(game.Pickups.Sum(p=>p.Count),Is.EqualTo(9));
        }
    }
}
