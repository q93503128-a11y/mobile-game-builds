using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
namespace Wildfolk.Tests
{
    public sealed class MedicineModelTests
    {
        private ContentCatalog Catalog=>Resources.Load<SaveGameRules>("SaveGameRules").Catalog;
        private WorldGenerationDefinition Data=>Resources.Load<SaveGameRules>("SaveGameRules").World;
        [Test]public void TreatmentIsGradualExactSingleAndTransient()
        {var t=new TreatmentModel();Assert.That(t.Begin(20,8),Is.True);Assert.That(t.Advance(0),Is.Zero);Assert.That(t.Advance(.3f),Is.Zero);Assert.That(t.Begin(20,8),Is.False);var sum=t.Advance(1.7f);Assert.That(sum,Is.EqualTo(5));sum+=t.Advance(.01f);sum+=t.Advance(2.99f);sum+=t.Advance(30);Assert.That(sum,Is.EqualTo(20));Assert.That(t.Active,Is.False);Assert.That(t.Advance(30),Is.Zero);Assert.That(t.Begin(20,8),Is.True);t.Clear();Assert.That(t.Advance(30),Is.Zero);Assert.Throws<ArgumentOutOfRangeException>(()=>t.Advance(float.NaN));Assert.Throws<ArgumentOutOfRangeException>(()=>t.Begin(20,float.PositiveInfinity));}
        [Test]public void TwoMedicinesUseOneHerbAndFiberSinkWithoutNewStationOrFoodChanges()
        {var d=Catalog.Item("field_dressing").Consumable;var s=Catalog.Item("healing_salve").Consumable;Assert.That(d.Kind,Is.EqualTo(ConsumableKind.Medicine));Assert.That(d.HealthRestore,Is.Zero);Assert.That(d.GradualHealthRestore,Is.EqualTo(20));Assert.That(d.HealingDuration,Is.EqualTo(8));Assert.That(s.Kind,Is.EqualTo(ConsumableKind.Medicine));Assert.That(s.HealthRestore,Is.EqualTo(30));Assert.That(s.GradualHealthRestore,Is.Zero);Assert.That(d.UseCooldownSeconds,Is.EqualTo(4));Assert.That(s.UseCooldownSeconds,Is.EqualTo(4));foreach(var id in new[]{"field_dressing","healing_salve"}){var r=Catalog.Recipe(id);Assert.That(r.RequiredStation,Is.EqualTo(StationKind.None));Assert.That(r.Ingredients.Single(c=>c.Item.Id=="plant_fiber").Count,Is.EqualTo(2));Assert.That(r.Ingredients.Single(c=>c.Item.Id=="medicinal_herb").Count,Is.EqualTo(1));}Assert.That(Catalog.Recipes.Count(r=>r.Ingredients.Any(i=>i.Item.Id=="medicinal_herb")),Is.EqualTo(4));Assert.That(Catalog.Recipes.Count(r=>r.Ingredients.Any(i=>i.Item.Id=="medicinal_gland")),Is.EqualTo(2));Assert.That(Catalog.Items.Where(i=>i.Consumable!=null && i.Consumable.Kind==ConsumableKind.Food).Max(i=>i.Consumable.HealthRestore),Is.EqualTo(18));}
        [Test]public void HerbIsUnarmedSurfacePlantWithRegionalRecoveryAndNoStarterSupply()
        {var rule=Data.Resources[12];var source=Data.NaturalSources.Single(s=>s.SourceId=="medicinal_herb_surface");Assert.That(source.ResourceRule,Is.EqualTo(12));Assert.That(source.StarterSupply,Is.Zero);Assert.That(source.SurfaceBiomes,Is.EquivalentTo(new[]{SurfaceBiome.Forest,SurfaceBiome.Wetlands}));Assert.That(source.CandidatesPerChunk,Is.EqualTo(6));Assert.That(source.Probability,Is.EqualTo(.6f));Assert.That(rule.Prefab.Definition.DropCount,Is.EqualTo(2));Assert.That(rule.Prefab.Definition.CanHarvest(null),Is.True);Assert.That(rule.NaturalRecovery,Is.True);Assert.That(rule.RespawnSeconds,Is.EqualTo(240));Assert.That(rule.HarvestExclusionSeconds,Is.EqualTo(480));Assert.That(rule.IndependentGeneration,Is.True);}
        [TestCase(1)][TestCase(2)][TestCase(3)]public void AppendingHerbsPreservesEveryPreviousSourceIdAndPosition(int generation)
        {
            var old=UnityEngine.Object.Instantiate(Data);old.Resources=old.Resources.Take(12).ToArray();old.NaturalSources=old.NaturalSources.Where(s=>s.ResourceRule!=12).ToArray();
            try{var a=new WorldMap(old,9472026,generation);var b=new WorldMap(Data,9472026,generation);if(generation==1){Assert.That(b.Resources.Where(p=>p.Rule<12).Select(Key),Is.EqualTo(a.Resources.Select(Key)));Assert.That(b.Resources.Count(p=>p.Rule==12),Is.EqualTo(12));}else{Assert.That(b.Continental.GuaranteedResources.Where(p=>p.Rule<12).Select(Key),Is.EqualTo(a.Continental.GuaranteedResources.Select(Key)));for(var y=-20;y<=20;y+=2)for(var x=-20;x<=20;x+=2){var key=new WorldChunk(x,y);Assert.That(b.Continental.GenerateChunk(key).Where(p=>p.Rule<12).Select(Key),Is.EqualTo(a.Continental.GenerateChunk(key).Select(Key)),key.ToString());}}}finally{UnityEngine.Object.DestroyImmediate(old);}
        }
        private static string Key(ResourcePlacement p)=>p.Id+":"+p.Rule+":"+p.Position+":"+p.Scale;
        [Test]public void HealingStatusRoundTripsThroughNetworkSerializationAndUsesProtocol16()
        {var expected=new PlayerStatusState{HP=37,MaximumHP=100,Head=-1,Body=4,Held=122,Utility=-1,TreatmentUntil=812.25};using(var writer=new FastBufferWriter(128,Allocator.Temp)){writer.WriteNetworkSerializable(expected);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out PlayerStatusState actual);Assert.That(actual.Equals(expected),Is.True);actual.TreatmentUntil=0;Assert.That(actual.Equals(expected),Is.False);}}Assert.That(NetworkProtocol.Current,Is.EqualTo(16));}
    }
}
