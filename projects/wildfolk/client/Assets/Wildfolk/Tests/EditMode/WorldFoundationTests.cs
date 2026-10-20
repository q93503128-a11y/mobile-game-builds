using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class WorldFoundationTests
    {
        private WorldGenerationDefinition Data=>AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>("Assets/Wildfolk/Data/WorldGeneration.asset");
        [TestCase(9472026)][TestCase(17491)][TestCase(-81027)][TestCase(193080127)]
        public void SeedHasConnectedCavesValidSourcesFourPlayerYieldAndBiomeFamilies(int seed)
        {
            var map=new WorldMap(Data,seed,2);var w=map.Continental;Assert.That(map.Size.x,Is.EqualTo(13120));Assert.That(w.Walkable(Vector2.zero,4),Is.True);
            Assert.That(w.Environment(Vector2.zero).Biome,Is.EqualTo(SurfaceBiome.Grassland));Assert.That(w.Environment(w.ForestAnchor).Forestation,Is.GreaterThan(.75f));Assert.That(w.Environment(w.RockyAnchor).Rockiness,Is.GreaterThan(.8f));
            var families=new System.Collections.Generic.HashSet<SurfaceBiome>();for(var y=-2000;y<=2000;y+=128)for(var x=-2000;x<=2000;x+=128)families.Add(w.Environment(new Vector2(x,y)).Biome);Assert.That(families.Count,Is.EqualTo(4));
            Assert.That(w.Caves.Count,Is.GreaterThanOrEqualTo(2));Assert.That(w.Caves.Any(c=>c.SurfaceEntrances.Count>1),Is.True);
            foreach(var c in w.Caves){Assert.That(c.Rooms.Max(r=>r.Depth),Is.EqualTo(1));Assert.That(c.Rooms.Count(r=>r.Neighbors.Count==1),Is.GreaterThanOrEqualTo(2));var visited=new System.Collections.Generic.HashSet<int>{0};var queue=new System.Collections.Generic.Queue<int>();queue.Enqueue(0);while(queue.Count>0)foreach(var next in c.Rooms[queue.Dequeue()].Neighbors)if(visited.Add(next))queue.Enqueue(next);Assert.That(visited.Count,Is.EqualTo(c.Rooms.Count));}
            foreach(var p in w.GuaranteedResources)Assert.That(w.SourceAllowed(p.Rule,p.Position),Is.True);
            foreach(var cave in w.Caves)foreach(var room in cave.Rooms)foreach(var next in room.Neighbors)for(var i=0;i<=40;i++)Assert.That(w.Walkable(Vector2.Lerp(room.Center,cave.Rooms[next].Center,i/40f),.3f),Is.True,"Cave graph corridor blocked");
            Assert.That(w.GuaranteedResources.Count(p=>p.Rule==2 && p.Position.magnitude<100),Is.GreaterThanOrEqualTo(3));
            var needs=WorldProgressionSupply.RequiredYield(Data);foreach(var n in needs)if(Data.Resources[n.Key].Family==ResourceFamily.Mineral)Assert.That(w.GuaranteedResources.Where(p=>p.Rule==n.Key).Sum(p=>Data.Resources[p.Rule].Prefab.Definition.DropCount),Is.GreaterThanOrEqualTo(n.Value));
        }
        [Test]public void SameSeedReplaysMacroHydrologySourceAnchorsAndChunks()
        {
            var a=new WorldMap(Data,9472026,2);var b=new WorldMap(Data,9472026,2);Assert.That(a.Continental.Anchors.Select(p=>p.Id+":"+p.Position),Is.EqualTo(b.Continental.Anchors.Select(p=>p.Id+":"+p.Position)));
            foreach(var key in new[]{new WorldChunk(0,0),new WorldChunk(-10,0),new WorldChunk(9,2)})Assert.That(a.Continental.GenerateChunk(key).Select(p=>p.Id+":"+p.Position),Is.EqualTo(b.Continental.GenerateChunk(key).Select(p=>p.Id+":"+p.Position)));
            Assert.That(a.Continental.RiverAt(1200),Is.EqualTo(b.Continental.RiverAt(1200)));var different=new WorldMap(Data,17491,2);Assert.That(a.Continental.Anchors[0].Position,Is.Not.EqualTo(different.Continental.Anchors[0].Position));
        }
        [Test]public void SubSeedsAreIsolatedAndLegacyLayoutIsVersionBound()
        {var a=WorldSeeds.Derive(9472026,SeedDomain.Hydrology);Assert.That(a,Is.Not.EqualTo(WorldSeeds.Derive(9472026,SeedDomain.Vegetation)));Assert.That(a,Is.EqualTo(WorldSeeds.Derive(9472026,SeedDomain.Hydrology)));var legacy=new WorldMap(Data,9472026,1);Assert.That(legacy.Size,Is.EqualTo(new Vector2(192,144)));Assert.That(legacy.Resources.Count,Is.EqualTo(Data.Resources.Sum(r=>r.Count)));Assert.That(new WorldMap(Data,9472026,2).Size.x,Is.GreaterThan(legacy.Size.x*50));}
        [Test]public void SourcesUseEnvironmentalTagsAndRegionalRecovery()
        {
            var map=new WorldMap(Data,9472026,2);for(var i=0;i<Data.Resources.Length;i++)Assert.That(map.Renewable(i),Is.EqualTo(i<10 || i==12));
            var tin=Data.NaturalSources.Single(s=>s.Context==ResourceSourceContext.Surface && s.ResourceRule==6);Assert.That(tin.AnyTags,Is.EqualTo(EnvironmentTag.RiverGravel));Assert.That(Data.Resources[6].Prefab.Definition.Harvest.Mode,Is.EqualTo(HarvestMode.Pan));Assert.That(Data.Resources[6].Prefab.Definition.CanHarvest(Data.ProgressionCatalog.Item("steel_pickaxe").Tool),Is.False);
            var deep=Data.NaturalSources.Single(s=>s.SourceId=="diamond_deep");Assert.That(deep.Enabled,Is.True);Assert.That(deep.NaturalRecovery,Is.True);Assert.That(deep.MinimumGenerationVersion,Is.EqualTo(3));Assert.That(deep.DepthRange.x,Is.GreaterThanOrEqualTo(.75));
        }
        [Test]public void VegetationDensityDoesNotMoveOreRiverOrPoi()
        {
            var data=Data;var source=data.NaturalSources.Single(s=>s.Context==ResourceSourceContext.Surface && s.ResourceRule==0);var old=source.CandidatesPerChunk;
            try{var a=new WorldMap(data,9472026,2,false);var key=new WorldChunk(-10,0);var expected=a.Continental.GenerateChunk(key).Where(p=>data.Resources[p.Rule].Family==ResourceFamily.Mineral).Select(p=>p.Id+":"+p.Position).ToArray();source.CandidatesPerChunk+=19;var b=new WorldMap(data,9472026,2,false);Assert.That(b.Continental.GenerateChunk(key).Where(p=>data.Resources[p.Rule].Family==ResourceFamily.Mineral).Select(p=>p.Id+":"+p.Position),Is.EqualTo(expected));Assert.That(b.RiverAt(700),Is.EqualTo(a.RiverAt(700)));Assert.That(b.Continental.Anchors.Select(p=>p.Position),Is.EqualTo(a.Continental.Anchors.Select(p=>p.Position)));}finally{source.CandidatesPerChunk=old;}
        }
        [Test]public void OceanAndHydrologyHaveNaturalBoundariesAndContinuousTransitions()
        {
            var m=new WorldMap(Data,9472026,2);Assert.That(m.IsDeepWater(new Vector2(m.Size.x*.49f,0)),Is.True);Assert.That(m.IsWalkable(new Vector2(m.Size.x*.49f,0)),Is.False);
            var riverBiomes=new System.Collections.Generic.HashSet<SurfaceBiome>();for(var y=-4000;y<4000;y+=128)riverBiomes.Add(m.Continental.Environment(new Vector2(m.RiverAt(y)+40,y)).Biome);Assert.That(riverBiomes.Count,Is.GreaterThan(1));
            var p=new Vector2(64,70);var colorA=WorldTerrainRenderer.Sample(m,p-Vector2.right*.001f);var colorB=WorldTerrainRenderer.Sample(m,p+Vector2.right*.001f);Assert.That(Vector4.Distance(colorA,colorB),Is.LessThan(.02f));
        }
    }
}
