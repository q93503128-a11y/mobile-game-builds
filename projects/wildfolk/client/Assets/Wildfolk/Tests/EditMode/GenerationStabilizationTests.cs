using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class GenerationStabilizationTests
    {
        public static IEnumerable<int> Seeds()
        {var fixedSeeds=new[]{9472026,-324239329,int.MinValue,int.MaxValue,0,1,-1,42,12345,54321};foreach(var n in fixedSeeds)yield return n;var rng=new System.Random(25102026);for(var i=0;i<26;i++)yield return (int)((long)rng.Next()*(i%2==0?1:-1));}
        private static string Point(Vector2 p)=>p.x.ToString("R",CultureInfo.InvariantCulture)+","+p.y.ToString("R",CultureInfo.InvariantCulture);
        public static string Fingerprint(WorldMap map)
        {
            var w=map.Continental;var b=new StringBuilder();foreach(var a in w.Anchors)b.Append(a.Id).Append(':').Append((int)a.Kind).Append(':').Append(Point(a.Position)).Append(';');
            foreach(var c in w.Caves){b.Append(c.Id).Append(':').Append(Point(c.Origin));foreach(var r in c.Rooms)b.Append(Point(r.Center)).Append(':').Append(Point(r.Radius)).Append(':').Append(r.Depth.ToString("R",CultureInfo.InvariantCulture)).Append(':').Append(string.Join(",",r.Neighbors));}
            foreach(var road in w.Roads)foreach(var p in road)b.Append(Point(p));
            foreach(var r in w.GuaranteedResources)b.Append(r.Id).Append(':').Append(r.Rule).Append(':').Append(Point(r.Position)).Append(';');
            foreach(var point in new[]{map.Spawn,w.ForestAnchor,w.RockyAnchor,w.SedimentAnchor})foreach(var r in w.GenerateChunk(WorldChunk.At(point,map.Definition.ChunkSize)))b.Append(r.Id).Append(':').Append(Point(r.Position));
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(b.ToString()))).Replace("-","").ToLowerInvariant();
        }
        [TestCaseSource(nameof(Seeds))][Timeout(120000)]
        public void GenerationThreeIsValidAndDeterministic(int seed)
        {
            var d=AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>("Assets/Wildfolk/Data/WorldGeneration.asset");var before=UnityEngine.Random.state;var a=new WorldMap(d,seed,3);var hash=Fingerprint(a);var b=new WorldMap(d,seed,3);
            Assert.That(UnityEngine.Random.state,Is.EqualTo(before));Assert.That(Fingerprint(b),Is.EqualTo(hash));Assert.That(a.IsWalkable(a.Spawn,4),Is.True);var world=a.Continental;
            foreach(var kind in new[]{WorldAnchorKind.Campsite,WorldAnchorKind.Well,WorldAnchorKind.Quarry,WorldAnchorKind.Graveyard,WorldAnchorKind.CaveEntrance})Assert.That(world.Anchors.Any(x=>x.Kind==kind),Is.True,kind.ToString());
            var anchors=world.Anchors;foreach(var anchor in anchors){Assert.That(float.IsFinite(anchor.Position.x) && float.IsFinite(anchor.Position.y) && a.Contains(anchor.Position),Is.True);}
            for(var i=0;i<anchors.Count;i++)for(var j=i+1;j<anchors.Count;j++)Assert.That(Vector2.Distance(anchors[i].Position,anchors[j].Position),Is.GreaterThan(1),"overlapping POI anchors");
            var biomes=new HashSet<SurfaceBiome>();for(var y=-7;y<=7;y++)for(var x=-7;x<=7;x++){var p=new Vector2(x,y)*a.Size.x/18;if(world.CoastDistance(p)>0)biomes.Add(world.Environment(p).Biome);}Assert.That(biomes.Count,Is.EqualTo(4));
            foreach(var r in world.GuaranteedResources)Assert.That(a.Contains(r.Position) && float.IsFinite(r.Position.x) && float.IsFinite(r.Position.y),Is.True);
            Directory.CreateDirectory("Captures/post-boss-generation");File.AppendAllText("Captures/post-boss-generation/fingerprints.tsv",seed+"\t"+hash+"\n");
        }
    }
}
