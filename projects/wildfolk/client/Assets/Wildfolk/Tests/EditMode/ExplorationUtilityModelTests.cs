using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ExplorationUtilityModelTests
    {
        [Test]public void SceneInitializationHasStandardTransportInactivityBudget()
        {var p=UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkSession.prefab");var transport=p.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();Assert.That(transport.DisconnectTimeoutMS,Is.EqualTo(30000));Assert.That(transport.MaxPacketQueueSize,Is.EqualTo(128));}
        private ContentCatalog C=>Resources.Load<SaveGameRules>("SaveGameRules").Catalog;
        [Test]public void TwoDistinctOptionalToolsHaveBoundedBudgetsAndBothLanguages()
        {
            Assert.That(C.Items.Length,Is.EqualTo(134));Assert.That(C.Recipes.Length,Is.EqualTo(111));Assert.That(C.Items[126].Id,Is.EqualTo("hunters_spear"));Assert.That(C.Items[131].Id,Is.EqualTo("fish_portions"));
            var survey=C.Item("ore_surveyor");var whistle=C.Item("signal_whistle");Assert.That(survey.ExplorationUtility.Kind,Is.EqualTo(ExplorationUtilityKind.Survey));Assert.That(whistle.ExplorationUtility.Kind,Is.EqualTo(ExplorationUtilityKind.Signal));Assert.That(survey.ExplorationUtility.Range,Is.EqualTo(18));Assert.That(whistle.ExplorationUtility.LifetimeSeconds,Is.EqualTo(20));
            Assert.That(C.Recipe("ore_surveyor").RequiredWorkbenchTier,Is.EqualTo(2));Assert.That(C.Recipe("signal_whistle").RequiredWorkbenchTier,Is.EqualTo(1));
            foreach(var item in new[]{survey,whistle}){Assert.That(item.MaxStack,Is.EqualTo(1));Assert.That(item.Tool,Is.Null);Assert.That(item.Icon,Is.Not.Null);Assert.That(item.ExplorationUtility.CooldownSeconds,Is.GreaterThan(item.ExplorationUtility.LifetimeSeconds));foreach(var language in new[]{"en","ko"}){L.SetLanguage(language);Assert.That(L.Text("item."+item.Id+".description"),Is.Not.Empty);Assert.That(item.LocalizedName,Is.Not.Empty);}}
            var bag=new InventoryModel(36);bag.Add(survey,1);bag.Add(whistle,1);var restored=new InventoryModel(36);restored.Restore(bag.Capture(),C);Assert.That(restored.CountItem(survey),Is.EqualTo(1));Assert.That(restored.CountItem(whistle),Is.EqualTo(1));
            Assert.That(C.Recipe("ore_surveyor").Ingredients.Select(i=>i.Item.Id+":"+i.Count),Is.EquivalentTo(new[]{"wood:2","bronze:2","leather:1"}));Assert.That(C.Recipe("signal_whistle").Ingredients.Select(i=>i.Item.Id+":"+i.Count),Is.EquivalentTo(new[]{"wood:2","plant_fiber:2"}));
        }
        [Test]public void BearingOnlyHasEightSectorsAndNoSurveyPositionIsSerialized()
        {
            for(byte i=0;i<8;i++)Assert.That(PlayerExplorationUtility.Bearing(PlayerExplorationUtility.Direction(i)),Is.EqualTo(i));
            Assert.That(typeof(SurveyState).GetFields().Any(f=>f.FieldType==typeof(Vector2)),Is.False);
            var a=new SurveyState{Phase=SurveyPhase.Found,Item=23,Bearing=7,Until=42.5,Ready=48.5};using(var w=new FastBufferWriter(128,Allocator.Temp)){w.WriteNetworkSerializable(a);using(var r=new FastBufferReader(w,Allocator.Temp)){r.ReadNetworkSerializable(out SurveyState b);Assert.That(b.Equals(a),Is.True);b.Bearing=0;Assert.That(b.Equals(a),Is.False);}}
            var s=new ExplorationSignal{Point=new Vector2(15,-8),Started=54,Until=74,Range=60};using(var w=new FastBufferWriter(128,Allocator.Temp)){w.WriteNetworkSerializable(s);using(var r=new FastBufferReader(w,Allocator.Temp)){r.ReadNetworkSerializable(out ExplorationSignal b);Assert.That(b.Equals(s),Is.True);}}
            Assert.That(NetworkProtocol.Current,Is.EqualTo(19));
        }
        [Test]public void RegionsDoNotMixUndergroundAndSurfaceOrSeparateCaves()
        {var rules=Resources.Load<SaveGameRules>("SaveGameRules");var map=new WorldMap(rules.World,9472026,3,false);Assert.That(PlayerExplorationUtility.SameRegion(map,Vector2.zero,Vector2.one),Is.True);var a=map.Continental.Caves[0].Origin;Assert.That(PlayerExplorationUtility.SameRegion(map,Vector2.zero,a),Is.False);Assert.That(PlayerExplorationUtility.SameRegion(map,a,a+Vector2.one),Is.True);}
    }
}
