using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ResourceTempoTests
    {
        private WorldGenerationDefinition Data=>AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>("Assets/Wildfolk/Data/WorldGeneration.asset");
        [TestCase(1,6,75)][TestCase(2,6,120)][TestCase(5,6,150)][TestCase(6,5,165)][TestCase(7,10,105)]
        public void SourcesPayOnCompletionAndRecoverWithinRegionalTempo(int index,int expectedYield,float seconds)
        {var d=Data;var r=d.Resources[index];Assert.That(r.Prefab.Definition.DropCount,Is.EqualTo(expectedYield));Assert.That(r.RespawnSeconds,Is.EqualTo(seconds));Assert.That(new WorldMap(d,9472026,2,false).Renewable(index),Is.True);Assert.That(r.TargetDensityRatio,Is.LessThanOrEqualTo(1));}
        [TestCase(1)][TestCase(4)]public void RecipeEconomyIncludesSharedFacilitiesAllUpgradeChainsAndBatchedFuel(int players)
        {
            var d=Data;var b=ProgressionEconomy.Calculate(d.ProgressionCatalog,players);var c=d.ProgressionCatalog;
            Assert.That(b.Total[c.Item("iron_ore")],Is.EqualTo(players==1 ? 83 : 197));
            Assert.That(b.Shared[c.Item("iron_ore")],Is.EqualTo(14));Assert.That(b.Personal[c.Item("iron_ore")],Is.EqualTo(36*players));
            Assert.That(b.Total.ContainsKey(c.Item("empty_cup")),Is.False);Assert.That(b.Total[c.Item("wood")],Is.EqualTo(players==1 ? 188 : 332));
            var old=players==1 ? 121 : 226;Assert.That(b.Total[c.Item("coal")],Is.EqualTo(old));
            foreach(var item in b.Total.Keys)Assert.That(b.Total[item],Is.EqualTo((b.Shared.TryGetValue(item,out var s) ? s : 0)+(b.Personal.TryGetValue(item,out var p) ? p : 0)+(b.Optional.TryGetValue(item,out var o) ? o : 0)));
            Assert.That(c.Recipe("copper_ingot").ResultCount,Is.EqualTo(1));Assert.That(c.Recipe("tin_ingot").ResultCount,Is.EqualTo(1));Assert.That(c.Recipe("steel").ResultCount,Is.EqualTo(1));
        }
        [Test]public void RecoveryDebtIsOneTicketPerChunkAndCandidatesStayInsideThatRegion()
        {
            var d=Data;var map=new WorldMap(d,9472026,2,false);var pop=new ResourcePopulation(map);var p=map.Resources.First(x=>x.Rule==2);var key=WorldChunk.At(p.Position,64);var region=WorldMap.Encode(key);var sources=map.Continental.GenerateChunk(key);
            pop.InitializeRegion(region,sources,null,0);foreach(var s in sources.Where(x=>x.Rule==2))pop.Remove(2,s.Position,0,true);
            Assert.That(pop.Pending.Count(x=>x.Rule==2 && x.Region==region),Is.EqualTo(1));Assert.That(pop.Pending.Single(x=>x.Rule==2).Due,Is.EqualTo(120));
            for(var i=0;i<100;i++)Assert.That(WorldChunk.At(pop.Candidate(2,region),64),Is.EqualTo(key));
            var restored=new ResourcePopulation(map);restored.Restore(pop.Capture());restored.InitializeRegion(region,sources,sources.Where(x=>x.Rule==2).Select(x=>new ResourceSnapshot{Id=x.Id,Rule=2,Position=x.Position,Destroyed=true}),90);
            Assert.That(restored.Pending.Count(x=>x.Rule==2),Is.EqualTo(1));Assert.That(restored.Pending.Single(x=>x.Rule==2).Due,Is.EqualTo(120));Assert.That(restored.Count(2,region),Is.Zero);
        }
    }
}
