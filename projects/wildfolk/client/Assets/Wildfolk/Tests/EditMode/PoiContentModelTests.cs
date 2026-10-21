using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class PoiContentModelTests
    {
        private static SaveGameRules Rules=>Resources.Load<SaveGameRules>("SaveGameRules");
        private static WorldGenerationDefinition World=>Rules.World;
        private static PoiContentDefinition Content=>World.PoiContent;
        [Test]public void FourSitesUseExistingBoundedRewardsAndNoNewItemsRecipesOrGeneration()
        {
            Assert.That(Rules.Catalog.Items.Length,Is.EqualTo(134));Assert.That(Rules.Catalog.Recipes.Length,Is.EqualTo(111));Assert.That(WorldGenerationVersion.Current,Is.EqualTo(3));Assert.That(Content.Campsite.Length,Is.EqualTo(3));
            var signatures=Enumerable.Range(0,80).Select(seed=>string.Join(";",Content.Supplies(WorldAnchorKind.Campsite,seed,"poi1:2").Select(s=>s.Item+"="+s.Count))).Distinct().ToArray();Assert.That(signatures.Length,Is.EqualTo(3));
            foreach(var kind in new[]{WorldAnchorKind.Campsite,WorldAnchorKind.Graveyard})for(var seed=0;seed<30;seed++){var a=Content.Supplies(kind,seed,"poi3:4");Assert.That(JsonUtility.ToJson(new PoiSiteSnapshot{Loot=a}),Is.EqualTo(JsonUtility.ToJson(new PoiSiteSnapshot{Loot=Content.Supplies(kind,seed,"poi3:4")})));Assert.That(a.Count(s=>s.Count>0),Is.EqualTo(3));Assert.That(a.Any(s=>s.Item=="steel" || s.Item=="diamond" || s.Item=="crystal" || s.Item=="mechanical_part" || s.Item=="bronze"),Is.False);}
            Assert.That(Enum.GetValues(typeof(WorldAnchorKind)).Cast<WorldAnchorKind>().Count(PoiContentDefinition.Supports),Is.EqualTo(4));
        }
        [Test]public void WellIsCappedLazyAndOnlyGameTimeRecoversOneServingPerDay()
        {
            var s=new PoiSiteSnapshot{Water=0,RefillAt=480};PoiSite.AdvanceWater(s,479,3,480);Assert.That(s.Water,Is.Zero);PoiSite.AdvanceWater(s,480,3,480);Assert.That(s.Water,Is.EqualTo(1));Assert.That(s.RefillAt,Is.EqualTo(960));PoiSite.AdvanceWater(s,10000,3,480);Assert.That(s.Water,Is.EqualTo(3));Assert.That(s.RefillAt,Is.Zero);
            var clean=Rules.Catalog.Item("clean_water_cup").Consumable.ThirstRestore;Assert.That(clean/Content.WellRefillSeconds,Is.LessThan(Rules.Player.GetComponent<PlayerSurvival>().Definition.ThirstPerSecond));
        }
        [Test]public void OptionalSave11StateRejectsFabricatedLootBadWaterAndDuplicateGuard()
        {
            var map=new WorldMap(World,9472026,3,false);var anchor=map.Continental.Anchors.First(a=>a.Kind==WorldAnchorKind.Campsite);var state=new PoiSiteSnapshot{Schema=1,Loot=Content.Supplies(anchor.Kind,map.Seed,anchor.Id)};
            var game=new SavedGame{BaseGenerationVersion=3,Seed=map.Seed,Features=new[]{new WorldFeatureSnapshot{Id=anchor.Id,Visited=true,Site=state}}};Assert.DoesNotThrow(()=>PoiSaveValidation.Validate(game,map,World));
            var clone=SaveRepository.Decode(SaveRepository.Encode(game));Assert.That(clone.Version,Is.EqualTo(11));Assert.That(clone.Features[0].Site.Schema,Is.EqualTo(1));
            state.Loot[0].Count++;Assert.Throws<InvalidDataException>(()=>PoiSaveValidation.Validate(game,map,World));state.Loot[0].Count--;state.Water=1;Assert.Throws<InvalidDataException>(()=>PoiSaveValidation.Validate(game,map,World));state.Water=0;state.Schema=2;Assert.Throws<InvalidDataException>(()=>PoiSaveValidation.Validate(game,map,World));state.Schema=0;Assert.DoesNotThrow(()=>PoiSaveValidation.Validate(game,map,World));
            var old=JsonUtility.FromJson<WorldFeatureSnapshot>("{\"Id\":\"old\",\"Visited\":true}");Assert.That(old.Site==null || old.Site.Schema==0,Is.True);
            var grave=map.Continental.Anchors.First(a=>a.Kind==WorldAnchorKind.Graveyard);var guard=grave.Id+":guard";game.Features=new[]{new WorldFeatureSnapshot{Id=grave.Id,Site=new PoiSiteSnapshot{Schema=1,Loot=Content.Supplies(grave.Kind,map.Seed,grave.Id)}},new WorldFeatureSnapshot{Id=guard}};game.NightCreatures=new[]{new NightCreatureSnapshot{Species="skeleton",Feature=guard,HP=32,Position=grave.Position}};Assert.DoesNotThrow(()=>PoiSaveValidation.Validate(game,map,World));game.NightCreatures=new[]{game.NightCreatures[0],game.NightCreatures[0]};Assert.Throws<InvalidDataException>(()=>PoiSaveValidation.Validate(game,map,World));
        }
        [Test]public void QuarryTombstonesAreSeparateFromNaturalRecoveryAndToolGatesRemain()
        {
            var map=new WorldMap(World,9472026,3,false);var a=map.Continental.Anchors.First(x=>x.Kind==WorldAnchorKind.Quarry);var rule=Content.QuarryRules[0];var s=new PoiSiteSnapshot{Schema=1,Loot=new InventoryModel(6).Capture(),Nodes=new[]{new ResourceSnapshot{Id="poi:"+a.Id+":0",Rule=rule,Position=a.Position,Scale=1,Destroyed=true}}};var game=new SavedGame{Features=new[]{new WorldFeatureSnapshot{Id=a.Id,Site=s}}};Assert.DoesNotThrow(()=>PoiSaveValidation.Validate(game,map,World));s.Nodes[0].Rule=Content.QuarryRules[3];Assert.Throws<InvalidDataException>(()=>PoiSaveValidation.Validate(game,map,World));
            Assert.That(World.Resources[Content.QuarryRules[2]].Prefab.Definition.CanHarvest(Rules.Catalog.Item("pickaxe").Tool),Is.False);Assert.That(World.Resources[Content.QuarryRules[2]].Prefab.Definition.CanHarvest(Rules.Catalog.Item("iron_pickaxe").Tool),Is.True);
        }
        [Test]public void TrapsHaveDifferentReuseRolesWallsAndTurretsKeepTheirBudgets()
        {
            var c=Rules.Catalog;var bone=c.Item("bone_trap").Placement.Prefab.GetComponent<SpikeTrap>();var spike=c.Item("spike_trap").Placement.Prefab.GetComponent<SpikeTrap>();Assert.That(bone.SingleUse,Is.True);Assert.That(bone.Damage,Is.EqualTo(32));Assert.That(spike.SingleUse,Is.False);Assert.That(spike.Damage,Is.EqualTo(16));
            Assert.That(new[]{"wood_wall","stone_wall","reinforced_wall"}.Select(id=>c.Item(id).Placement.Vitality.MaximumHealth),Is.EqualTo(new[]{100,220,360}));Assert.That(c.Item("ballista").Placement.Turret.Damage,Is.EqualTo(62));Assert.That(c.Item("repeater").Placement.Turret.ReloadSeconds,Is.EqualTo(.55f));
            foreach(var lang in new[]{"en","ko"}){L.SetLanguage(lang);foreach(var key in new[]{"ui.poi.supplies","ui.poi.well_container","ui.poi.guarded","poi.Well"})Assert.That(L.Has(key),Is.True);}
        }
        [Test]public void Protocol18RoundTripsSiteIdentitySecurityRevisionAndSharedWater()
        {
            var a=new PoiNetworkState{Id="poi4:5",Kind=WorldAnchorKind.Well,Opened=true,Secured=true,Revision=54,Water=2};using(var w=new FastBufferWriter(256,Allocator.Temp)){w.WriteNetworkSerializable(a);using(var r=new FastBufferReader(w,Allocator.Temp)){r.ReadNetworkSerializable(out PoiNetworkState b);Assert.That(b.Equals(a),Is.True);b.Water--;Assert.That(b.Equals(a),Is.False);}}
            Assert.That(NetworkProtocol.Current,Is.EqualTo(18));var e=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkEcology.prefab").GetComponent<NetworkCreatureWorld>();Assert.That(e.Prefabs.Count(p=>p.Source.GetComponent<PoiSite>()!=null && p.NetworkPrefab.GetComponent<NetworkPoiSite>()!=null),Is.EqualTo(1));
        }
    }
}
