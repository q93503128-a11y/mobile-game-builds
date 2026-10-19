using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class EcologyModelTests
    {
        private static T Data<T>(string name) where T:Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        [Test]public void FamilyAndLightTraitAreIndependentData()
        {var data=ScriptableObject.CreateInstance<CreatureDefinition>();try{data.Family=CreatureFamily.Undead;Assert.That(data.Has(CreatureTrait.LightSensitive),Is.False);data.Family=CreatureFamily.Animal;data.Traits=CreatureTrait.LightSensitive;Assert.That(data.Has(CreatureTrait.LightSensitive),Is.True);}finally{Object.DestroyImmediate(data);}}
        [Test]public void AllFiveSpeciesHaveUsableLootGroupsHabitatsAndBoundedPopulation()
        {var population=Data<AnimalPopulationDefinition>("AnimalPopulation");Assert.That(population.Rules.Length,Is.EqualTo(5));Assert.That(population.Rules.Sum(r=>r.Population),Is.LessThanOrEqualTo(population.HardCap));foreach(var r in population.Rules){Assert.That(r.Prefab.Member.Definition.Family,Is.EqualTo(CreatureFamily.Animal));Assert.That(r.Prefab.Member.Definition.Habitat,Is.Not.EqualTo(CreatureHabitat.None));Assert.That(r.Prefab.Member.Definition.MaximumGroup,Is.GreaterThanOrEqualTo(r.Prefab.Member.Definition.MinimumGroup));Assert.That(r.Prefab.GetComponent<LootOnDeath>().Table.Items.Length,Is.GreaterThan(0));}Assert.That(population.Rules[2].Prefab.Member.Definition.SeparationRadius,Is.LessThan(population.Rules[3].Prefab.Member.Definition.SeparationRadius));}
        [Test]public void V3AnimalMigrationRetainsLivingIdentityPositionHPAndRecoveryDeadline()
        {
            var population=Data<AnimalPopulationDefinition>("AnimalPopulation");var world=Data<WorldGenerationDefinition>("WorldGeneration");var game=new SavedGame{Version=3,Seed=9472026,Elapsed=42,Animals=new[]{new AnimalSnapshot{Id="a0_0",Rule=0,HP=10,Position=new Vector2(-12,-5),Home=new Vector2(-12,-5)},new AnimalSnapshot{Id="a1_0",Rule=1,Missing=true,HP=0,Home=new Vector2(14,-6),RespawnAt=242}}};
            SaveMigration.EcologyFromV3(game,world,population);Assert.That(game.Version,Is.EqualTo(4));Assert.That(game.Animals.Length,Is.EqualTo(1));var alive=game.Animals[0];Assert.That(alive.Id,Is.EqualTo("a0_0"));Assert.That(alive.Position,Is.EqualTo(new Vector2(-12,-5)));Assert.That(alive.HP,Is.EqualTo(10));Assert.That(alive.Species,Is.EqualTo(population.Rules[0].Prefab.Member.Definition.Id));Assert.That(game.AnimalEcology.Pending.Single().Due,Is.EqualTo(242));Assert.That(AnimalPopulation.BuildTargets(new WorldMap(world,game.Seed),population)[1,game.AnimalEcology.Pending.Single().Region],Is.GreaterThan(0));
        }
        [Test]public void RustedSwordIsUsableSingleTargetLocalizedSaveItemAndRareVariantLoot()
        {var catalog=Data<ContentCatalog>("ContentCatalog");var sword=catalog.Item("rusted_sword");Assert.That(sword.Tool,Is.Not.Null);Assert.That(sword.Tool.Kind,Is.EqualTo(ToolKind.Sword));Assert.That(sword.Tool.Attack.MaxTargets,Is.EqualTo(1));Assert.That(L.Has("item.rusted_sword.name"),Is.True);var bag=new InventoryModel(36);bag.Add(sword,1);var next=new InventoryModel(36);next.Restore(bag.Capture(),catalog);Assert.That(next.CountItem(sword),Is.EqualTo(1));Assert.That(Data<CreatureVariantDefinition>("RustedSwordVariant").Loot.RareItems.Single().Item,Is.SameAs(sword));Assert.That(catalog.Items.Any(i=>i.Id.Contains("wool") || i.Id=="gunpowder"),Is.False);}
        [Test]public void SlingshotHasRealDrawReleaseAndRecoveryIntervals()
        {var d=Data<SlingshotDefinition>("SlingshotAttack");Assert.That(d.Draw,Is.GreaterThan(.2f));Assert.That(d.Release,Is.LessThan(d.Draw));Assert.That(d.Recovery,Is.GreaterThan(d.Release));Assert.That(d.MinimumDistance,Is.LessThan(d.PreferredDistance));Assert.That(d.PreferredDistance,Is.LessThan(d.MaximumDistance));}
        [Test]public void FullInventoryReplacementFailureIsAtomic()
        {var catalog=Data<ContentCatalog>("ContentCatalog");var armor=Object.Instantiate(catalog.Item("iron_head"));try{armor.MaxStack=2;var bag=new InventoryModel(2);bag.Add(armor,2);bag.Add(catalog.Item("wood"),50);var before=bag.Capture();Assert.That(bag.ReplaceConsumedSlot(0,armor,catalog.Item("leather_head")),Is.False);Assert.That(bag.GetSlot(0).Count,Is.EqualTo(before[0].Count));Assert.That(bag.GetSlot(1).Count,Is.EqualTo(before[1].Count));}finally{Object.DestroyImmediate(armor);}}
    }
}
