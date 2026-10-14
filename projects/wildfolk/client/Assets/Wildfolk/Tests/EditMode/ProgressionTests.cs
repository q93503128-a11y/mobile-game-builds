using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ProgressionTests
    {
        private static T Data<T>(string name) where T:UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        [Test] public void TiersImproveTheirRolesAndGateOreByToolKindAndRank()
        {
            var ore=Data<ResourceDefinition>("IronOreResource");
            Assert.That(ore.CanHarvest(null),Is.False);Assert.That(ore.CanHarvest(Data<ToolDefinition>("PickaxeTool")),Is.False);
            Assert.That(ore.CanHarvest(Data<ToolDefinition>("StonePickaxeTool")),Is.True);Assert.That(ore.CanHarvest(Data<ToolDefinition>("IronPickaxeTool")),Is.True);
            Assert.That(ore.CanHarvest(Data<ToolDefinition>("StoneSwordTool")),Is.False);
            Assert.That(Data<ToolDefinition>("StoneAxeTool").DamageFor(ResourceKind.Wood),Is.GreaterThan(Data<ToolDefinition>("AxeTool").DamageFor(ResourceKind.Wood)));
            Assert.That(Data<ToolDefinition>("StoneAxeTool").Cooldown,Is.LessThan(Data<ToolDefinition>("AxeTool").Cooldown));
            Assert.That(Data<ToolDefinition>("StonePickaxeTool").DamageFor(ResourceKind.Stone),Is.GreaterThan(Data<ToolDefinition>("PickaxeTool").DamageFor(ResourceKind.Stone)));
            Assert.That(Data<ToolDefinition>("IronPickaxeTool").DamageFor(ResourceKind.IronOre),Is.GreaterThan(Data<ToolDefinition>("StonePickaxeTool").DamageFor(ResourceKind.IronOre)));
            Assert.That(Data<ToolDefinition>("IronSwordTool").Damage,Is.GreaterThan(Data<ToolDefinition>("StoneSwordTool").Damage));
            Assert.That(Data<ToolDefinition>("StoneSwordTool").Damage,Is.GreaterThan(Data<ToolDefinition>("WoodSwordTool").Damage));
            Assert.That(AssetDatabase.FindAssets("t:EquipmentTier",new[]{"Assets/Wildfolk/Data"}).Length,Is.EqualTo(3));
        }
        [Test] public void ChestTransferMergesPartiallyRejectsFullAndSwapsAtomically()
        {
            var wood=Data<ItemDefinition>("Wood");var stone=Data<ItemDefinition>("Stone");var player=new InventoryModel(2);var chest=new InventoryModel(2);
            player.Add(wood,20);chest.Add(wood,45);var changes=0;player.Changed+=()=>{Assert.That(chest.GetSlot(0).Count,Is.EqualTo(50));changes++;};
            Assert.That(InventoryModel.Transfer(player,0,chest,0),Is.True);Assert.That(player.GetSlot(0).Count,Is.EqualTo(15));Assert.That(changes,Is.EqualTo(1));
            Assert.That(InventoryModel.Transfer(player,0,chest,0),Is.False);Assert.That(changes,Is.EqualTo(1));
            chest.Add(stone,3);Assert.That(InventoryModel.Transfer(player,0,chest,1),Is.True);Assert.That(player.GetSlot(0).Item,Is.SameAs(stone));Assert.That(chest.GetSlot(1).Count,Is.EqualTo(15));
        }
        [Test] public void ChestsHaveIndependentStoresAndFailedTransfersPreserveBoth()
        {
            var wood=Data<ItemDefinition>("Wood");var a=new InventoryModel(18);var b=new InventoryModel(18);a.Add(wood,50);b.Add(wood,50);
            Assert.That(InventoryModel.Transfer(a,0,b,0),Is.False);Assert.That(a.GetSlot(0).Count,Is.EqualTo(50));Assert.That(b.GetSlot(0).Count,Is.EqualTo(50));
            Assert.That(InventoryModel.Transfer(a,0,b,1),Is.True);Assert.That(a.GetSlot(0).IsEmpty,Is.True);Assert.That(b.GetSlot(1).Count,Is.EqualTo(50));
        }
        [Test] public void WorldTimeCrossesAllPhasesAndLightingIsContinuousAtBoundaries()
        {
            var definition=Data<DayNightDefinition>("DayNight");var time=new WorldTimeModel(definition);Assert.That(time.Day,Is.EqualTo(1));
            time.Advance(definition.DaySeconds);Assert.That(time.Phase,Is.EqualTo(DayPhase.Evening));
            time.Lighting(out var color,out var intensity);Assert.That(intensity,Is.EqualTo(1));
            time.Advance(definition.EveningSeconds);Assert.That(time.Phase,Is.EqualTo(DayPhase.Night));time.Lighting(out color,out intensity);Assert.That(intensity,Is.EqualTo(definition.NightIntensity));
            time.Advance(definition.NightSeconds);Assert.That(time.Phase,Is.EqualTo(DayPhase.Dawn));time.Lighting(out color,out intensity);Assert.That(intensity,Is.EqualTo(definition.NightIntensity));
            time.Advance(definition.DawnSeconds);Assert.That(time.Day,Is.EqualTo(2));Assert.That(time.Phase,Is.EqualTo(DayPhase.Day));time.Lighting(out color,out intensity);Assert.That(intensity,Is.EqualTo(1));
            Assert.That(definition.NightIntensity,Is.GreaterThanOrEqualTo(.4f));Assert.That(definition.CycleSeconds,Is.EqualTo(480));
        }
        [Test] public void ProcessingRecipesUseSeparateStationsAndNoPlaceholderFuel()
        {
            var cook=Data<RecipeDefinition>("CookedMeatRecipe");var smelt=Data<RecipeDefinition>("IronIngotRecipe");
            Assert.That(cook.Processing,Is.EqualTo(ProcessingKind.Cooking));Assert.That(smelt.Processing,Is.EqualTo(ProcessingKind.Smelting));Assert.That(smelt.RequiredStation,Is.EqualTo(StationKind.Furnace));
            Assert.That(smelt.Ingredients.Length,Is.EqualTo(1));Assert.That(smelt.Ingredients[0].Item.Id,Is.EqualTo("iron_ore"));Assert.That(smelt.ProcessingSeconds,Is.EqualTo(2));
            Assert.That(Data<PlaceableDefinition>("StorageChestPlacement").StorageSlots,Is.EqualTo(24));Assert.That(Data<PlaceableDefinition>("WoodWallPlacement").IsBarrier,Is.True);
            Assert.That(System.IO.File.Exists("Assets/Wildfolk/Audio/action.wav"),Is.False);Assert.That(System.IO.File.Exists("Assets/Wildfolk/Runtime/Crafting/StationCooking.cs"),Is.False);
        }
    }
}
