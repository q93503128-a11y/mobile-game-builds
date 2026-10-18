using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ExplorationModelTests
    {
        private static T Data<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        [Test]public void HostClockSampleKeepsSurfaceLightTimeAcrossJoinAndPause()
        {
            var sample=new WorldClockSample{Elapsed=530,SampledAt=100,Advancing=true};Assert.That(sample.Estimate(103),Is.EqualTo(533));Assert.That(sample.Estimate(98),Is.EqualTo(530));sample.Advancing=false;Assert.That(sample.Estimate(200),Is.EqualTo(530));
        }
        [Test]public void EarlyTorchAndCompassUseExistingMaterialsAndOnePlacementItem()
        {
            var catalog=Data<ContentCatalog>("ContentCatalog");var r=catalog.Recipe("torch");
            Assert.That(r.RequiredStation,Is.EqualTo(StationKind.None));Assert.That(r.ResultCount,Is.EqualTo(4));
            Assert.That(r.Ingredients.Select(i=>i.Item.Id),Is.EquivalentTo(new[]{"wood","coal"}));
            var torch=r.Result;Assert.That(torch.PortableLight,Is.Not.Null);Assert.That(torch.HeldSprite,Is.Not.Null);Assert.That(torch.Placement.Item,Is.SameAs(torch));
            Assert.That(torch.Placement.Prefab.GetComponentInChildren<LocalFirelight>().Definition,Is.SameAs(torch.PortableLight));
            Assert.That(torch.PortableLight.DetersEnemies,Is.False);Assert.That(catalog.Recipe("compass").RequiredWorkbenchTier,Is.EqualTo(1));Assert.That(catalog.Item("compass").NavigationInstrument,Is.True);
            Assert.That(catalog.Items.Any(i=>i.Id.Contains("diamond")),Is.False);
        }
        [Test]public void CompassProjectsNorthAndLargeDistanceIndependentlyOfFacing()
        {
            var go=new GameObject();var camera=go.AddComponent<Camera>();
            try{Assert.That(NavigationBearing.Angle(Vector2.up,camera),Is.EqualTo(0).Within(.001));Assert.That(NavigationBearing.Angle(new Vector2(6200,0),camera),Is.EqualTo(-90).Within(.001));camera.transform.rotation=Quaternion.Euler(0,0,90);Assert.That(NavigationBearing.Angle(Vector2.up,camera),Is.EqualTo(-90).Within(.001));}
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
        [Test]public void ChunkUnionDeduplicatesOneTwoAndFourDistantRegions()
        {
            var map=new WorldMap(Data<WorldGenerationDefinition>("WorldGeneration"),9472026,2,false);var result=new HashSet<WorldChunk>();var centers=new List<Vector2>{new Vector2(32,32)};
            ChunkActivation.Build(map,centers,Array.Empty<WorldChunk>(),result);Assert.That(result.Count,Is.EqualTo(9));
            centers.Add(new Vector2(3000,1000));ChunkActivation.Build(map,centers,Array.Empty<WorldChunk>(),result);Assert.That(result.Count,Is.EqualTo(18));
            centers.Add(new Vector2(-3000,1000));centers.Add(new Vector2(0,-3000));ChunkActivation.Build(map,centers,Array.Empty<WorldChunk>(),result);Assert.That(result.Count,Is.EqualTo(36));
            centers[1]=centers[0];ChunkActivation.Build(map,centers,Array.Empty<WorldChunk>(),result);Assert.That(result.Count,Is.EqualTo(27));
        }
        [Test]public void BoundaryHysteresisRetainsThenReleasesWithoutUnboundedHistory()
        {
            var map=new WorldMap(Data<WorldGenerationDefinition>("WorldGeneration"),9472026,2,false);var result=new HashSet<WorldChunk>();var centers=new List<Vector2>{new Vector2(63,32)};
            ChunkActivation.Build(map,centers,Array.Empty<WorldChunk>(),result);var old=result.ToArray();
            centers[0]=new Vector2(65,32);ChunkActivation.Build(map,centers,old,result);Assert.That(result.Contains(new WorldChunk(-1,0)),Is.True);Assert.That(result.Count,Is.EqualTo(12));
            centers[0]=new Vector2(63,32);old=result.ToArray();ChunkActivation.Build(map,centers,old,result);Assert.That(result.Count,Is.EqualTo(12));
            centers[0]=new Vector2(73,32);old=result.ToArray();ChunkActivation.Build(map,centers,old,result);Assert.That(result.Count,Is.EqualTo(9));
        }
        [Test]public void CaveExitGuidanceStaysOnItsChamberGraphAndBrightnessHasAFloor()
        {
            var map=new WorldMap(Data<WorldGenerationDefinition>("WorldGeneration"),9472026,2,false);
            foreach(var cave in map.Continental.Caves)foreach(var room in cave.Rooms)
            {
                var target=NavigationBearing.CaveExit(cave,room.Center);var index=cave.Rooms.IndexOf(room);var destination=cave.Rooms.FindIndex(r=>Vector2.Distance(r.Center,target)<.01f);
                Assert.That(destination==index || room.Neighbors.Contains(destination),Is.True);
                Assert.That(cave.Ambient(room.Center),Is.InRange(.179f,.521f));
            }
        }
        [Test]public void AtomicPlacementReservationRollsBackFailedSpawn()
        {
            var bag=new InventoryModel(2);var torch=Data<ContentCatalog>("ContentCatalog").Item("torch");bag.Add(torch,4);var events=0;bag.Changed+=()=>events++;
            Assert.That(bag.TryDropSlot(0,torch,1,()=>false),Is.False);Assert.That(bag.CountItem(torch),Is.EqualTo(4));Assert.That(events,Is.Zero);
            Assert.Throws<InvalidOperationException>(()=>bag.TryDropSlot(0,torch,1,()=>throw new InvalidOperationException()));Assert.That(bag.CountItem(torch),Is.EqualTo(4));
            Assert.That(bag.TryDropSlot(0,torch,1,()=>true),Is.True);Assert.That(bag.CountItem(torch),Is.EqualTo(3));Assert.That(events,Is.EqualTo(1));
        }
    }
}
