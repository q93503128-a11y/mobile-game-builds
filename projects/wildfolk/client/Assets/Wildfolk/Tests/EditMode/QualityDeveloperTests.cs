using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class QualityDeveloperTests
    {
        [Test]public void OverviewKeepsActualMapMarkersAndDeterministicExport()
        {
            var definition=AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>("Assets/Wildfolk/Data/WorldGeneration.asset");var map=new WorldMap(definition,9472026,3);
            var type=Type.GetType("Wildfolk.Editor.WorldOverview, Assembly-CSharp-Editor",true);var tool=Activator.CreateInstance(type,new object[]{map,128});var root=Path.GetFullPath("Captures/Codex_QA_QualityOverviewTest");
            try
            {
                Assert.That(type.GetField("Map").GetValue(tool),Is.SameAs(map));var targets=(Dictionary<string,Vector2>)type.GetField("Targets").GetValue(tool);Assert.That(targets["Spawn"],Is.EqualTo(map.Spawn));
                foreach(var anchor in map.Continental.Anchors)Assert.That(targets[anchor.Kind+" / "+anchor.Id],Is.EqualTo(anchor.Position));
                type.GetMethod("Export").Invoke(tool,new object[]{root});var image=File.ReadAllBytes(Path.Combine(root,"world-overview-seed-9472026.png"));type.GetMethod("Export").Invoke(tool,new object[]{root});Assert.That(File.ReadAllBytes(Path.Combine(root,"world-overview-seed-9472026.png")),Is.EqualTo(image));
                var svg=File.ReadAllText(Path.Combine(root,"world-overview-seed-9472026.svg"));Assert.That(svg,Does.Contain("9472026"));Assert.That(svg,Does.Contain("Cave entrance"));Assert.That(svg,Does.Contain("Road / trail"));
            }
            finally{((IDisposable)tool).Dispose();}
        }
    }
}
