using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class WorldGenerationTests
    {
        private static T Data<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        [Test]public void SeedRecreatesTerrainRoadsAndResourcesWithoutUsingUnityRandom()
        {
            var d=Data<WorldGenerationDefinition>("WorldGeneration");Random.InitState(101);var before=Random.state;
            var a=new WorldMap(d,12345);Assert.That(Random.state,Is.EqualTo(before));var b=new WorldMap(d,12345);var c=new WorldMap(d,54321);
            Assert.That(a.LakeCenter,Is.EqualTo(b.LakeCenter));Assert.That(a.RiverX,Is.EqualTo(b.RiverX));Assert.That(a.Resources.Count,Is.EqualTo(b.Resources.Count));
            for(var i=0;i<a.Resources.Count;i++){Assert.That(a.Resources[i].Position,Is.EqualTo(b.Resources[i].Position));Assert.That(a.Resources[i].Scale,Is.EqualTo(b.Resources[i].Scale));}
            Assert.That(a.LakeCenter,Is.Not.EqualTo(c.LakeCenter));Assert.That(a.Resources[0].Position,Is.Not.EqualTo(c.Resources[0].Position));
        }
        [Test]public void ManySeedsHaveClearSpawnNearbySuppliesRareOreAndNoOverlaps()
        {
            var d=Data<WorldGenerationDefinition>("WorldGeneration");
            foreach(var seed in new[]{0,1,2,3,9,42,123,9472026,-9,int.MaxValue,int.MinValue,2048})
            {
                var map=new WorldMap(d,seed);Assert.That(map.IsWalkable(map.Spawn,2),Is.True);
                var total=0;for(var rule=0;rule<d.Resources.Length;rule++)
                {
                    var count=0;var near=0;foreach(var p in map.Resources)if(p.Rule==rule){count++;if(p.Position.magnitude<=d.Resources[rule].StartRadius*1.42f)near++;}
                    Assert.That(count,Is.EqualTo(d.Resources[rule].Count));Assert.That(near,Is.GreaterThanOrEqualTo(d.Resources[rule].StartCount));total+=count;
                }
                Assert.That(total,Is.LessThan(300));Assert.That(map.PondCenter.magnitude,Is.LessThan(15));
                for(var i=0;i<map.Resources.Count;i++)
                {
                    var p=map.Resources[i];Assert.That(map.IsWater(p.Position,1.4f),Is.False);Assert.That(map.IsRoad(p.Position),Is.False);
                    Assert.That(p.Position.magnitude,Is.GreaterThanOrEqualTo(d.Resources[p.Rule].SpawnClearance));
                    for(var j=0;j<i;j++){var other=map.Resources[j];Assert.That(Vector2.Distance(p.Position,other.Position),Is.GreaterThanOrEqualTo(Mathf.Max(d.Resources[p.Rule].Spacing,d.Resources[other.Rule].Spacing)));}
                }
            }
            Assert.That(d.Resources[2].Count,Is.LessThan(d.Resources[1].Count));
        }
        [Test]public void RiverHasWalkableCrossingAndLandRemainsConnected()
        {
            var d=Data<WorldGenerationDefinition>("WorldGeneration");var map=new WorldMap(d,9472026);
            var ford=new Vector2(map.RiverAt(map.FordY),map.FordY);Assert.That(map.IsWater(ford),Is.True);Assert.That(map.IsDeepWater(ford),Is.False);Assert.That(map.IsWalkable(ford),Is.True);
            Assert.That(map.IsDeepWater(new Vector2(map.RiverAt(map.FordY+12),map.FordY+12)),Is.True);Assert.That(map.IsDeepWater(map.LakeCenter),Is.True);
            var walkable=new HashSet<Vector2Int>();var w=Mathf.RoundToInt(d.Size.x/2);var h=Mathf.RoundToInt(d.Size.y/2);
            for(var y=1;y<h;y++)for(var x=1;x<w;x++)if(map.IsWalkable(new Vector2(x*2-d.Size.x*.5f,y*2-d.Size.y*.5f),.55f))walkable.Add(new Vector2Int(x,y));
            var start=new Vector2Int(w/2,h/2);var visited=new HashSet<Vector2Int>{start};var queue=new Queue<Vector2Int>();queue.Enqueue(start);
            var directions=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
            while(queue.Count>0){var p=queue.Dequeue();foreach(var direction in directions){var next=p+direction;if(walkable.Contains(next) && visited.Add(next))queue.Enqueue(next);}}
            Assert.That(visited.Count,Is.EqualTo(walkable.Count),"River/lake must not isolate an entire land region.");
        }
        [Test]public void TreeHitCountsUpgradeRecipesAndFurnaceCapacityMatchActualData()
        {
            var hp=Data<VitalityDefinition>("TreeVitality").MaximumHealth;
            Assert.That(Mathf.CeilToInt((float)hp/Data<ToolDefinition>("AxeTool").DamageFor(ResourceKind.Wood)),Is.InRange(5,6));
            Assert.That(Mathf.CeilToInt((float)hp/Data<ToolDefinition>("StoneAxeTool").DamageFor(ResourceKind.Wood)),Is.InRange(3,4));
            Assert.That(Mathf.CeilToInt((float)hp/Data<ToolDefinition>("IronAxeTool").DamageFor(ResourceKind.Wood)),Is.InRange(2,3));
            foreach(var r in Data<RecipeCatalog>("Recipes").Recipes)
            {
                var tool=r.Result.Tool;if(tool==null || tool.Tier.Rank==1)continue;
                Assert.That(r.Ingredients[0].Count,Is.EqualTo(1));Assert.That(r.Ingredients[0].Item.Tool.Kind,Is.EqualTo(tool.Kind));Assert.That(r.Ingredients[0].Item.Tool.Tier.Rank,Is.EqualTo(tool.Tier.Rank-1));
            }
            Assert.That(Data<PlaceableDefinition>("FurnacePlacement").ParallelCapacity,Is.EqualTo(3));Assert.That(Data<PlaceableDefinition>("CampfirePlacement").ParallelCapacity,Is.EqualTo(1));
        }
        [Test]public void GroundActuallyShowsDirtRoadAndFordDiffersFromDeepWater()
        {
            var map=new WorldMap(Data<WorldGenerationDefinition>("WorldGeneration"),9472026);
            var road=WorldTerrainRenderer.Sample(map,map.Roads[0][1]);Assert.That(road.r,Is.GreaterThan(map.Definition.Biome.Dirt.r*.8f));
            var ford=WorldTerrainRenderer.Sample(map,new Vector2(map.RiverAt(map.FordY),map.FordY));
            var deep=WorldTerrainRenderer.Sample(map,new Vector2(map.RiverAt(map.FordY+12),map.FordY+12));
            Assert.That(ford.g,Is.GreaterThan(deep.g+.02f));
        }
        [Test]public void SplitHalfAndPartialTransferPreserveTotalsOnFullOrDifferentTargets()
        {
            var wood=Data<ItemDefinition>("Wood");var stone=Data<ItemDefinition>("Stone");var a=new InventoryModel(3);var b=new InventoryModel(2);a.Add(wood,31);
            Assert.That(a.SplitHalf(0),Is.True);Assert.That(a.GetSlot(0).Count,Is.EqualTo(16));Assert.That(a.GetSlot(1).Count,Is.EqualTo(15));
            b.Add(stone,1);Assert.That(InventoryModel.Transfer(a,0,b,0,4),Is.False);Assert.That(a.GetSlot(0).Count,Is.EqualTo(16));
            Assert.That(InventoryModel.Transfer(a,0,b,1,4),Is.True);Assert.That(a.CountItem(wood)+b.CountItem(wood),Is.EqualTo(31));
            a.Add(stone,1);Assert.That(a.SplitHalf(0),Is.False);
        }
        [Test]public void PopulationAndLootDataAreBoundedAndOnlyOneBiomeExists()
        {
            var d=Data<ZombieSpawnDefinition>("ZombieSpawn");var targets=new[]{6,6,8,8,10,10,12,12,14,14,16,16,18,18};
            for(var i=0;i<targets.Length;i++)Assert.That(d.PopulationFor(i+1),Is.EqualTo(targets[i]));Assert.That(d.PopulationFor(1000),Is.EqualTo(18));
            Assert.That(AssetDatabase.FindAssets("t:BiomeDefinition").Length,Is.EqualTo(1));
            foreach(var guid in AssetDatabase.FindAssets("t:LootTable"))
            {var table=AssetDatabase.LoadAssetAtPath<LootTable>(AssetDatabase.GUIDToAssetPath(guid));Assert.That(table.Experience,Is.GreaterThanOrEqualTo(0));Assert.That(table.Gold,Is.GreaterThanOrEqualTo(0));foreach(var e in table.Items){Assert.That(e.Item,Is.Not.Null);Assert.That(e.Chance,Is.InRange(0,1));Assert.That(e.Minimum,Is.GreaterThan(0));Assert.That(e.Maximum,Is.GreaterThanOrEqualTo(e.Minimum));}}
        }
    }
}
