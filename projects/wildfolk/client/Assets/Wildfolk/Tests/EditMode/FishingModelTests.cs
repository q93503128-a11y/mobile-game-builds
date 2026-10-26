using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class FishingModelTests
    {
        private ContentCatalog C=>Resources.Load<SaveGameRules>("SaveGameRules").Catalog;
        private FishingLootTable Table=>AssetDatabase.LoadAssetAtPath<FishingLootTable>("Assets/Wildfolk/Data/FishingCatches.asset");
        [Test]public void ConditionsWeightsAndYieldDoNotDependOnClientOrSpeciesName()
        {
            var contexts=new[]{new FishingWaterContext(FishingWater.Inland,false,false,0),new FishingWaterContext(FishingWater.Inland,false,true,3),new FishingWaterContext(FishingWater.Wetlands,true,true,3),new FishingWaterContext(FishingWater.Ocean,true,true,3),new FishingWaterContext(FishingWater.Cavern,true,false,0)};
            var expected=new[]{new[]{"raw_fish"},new[]{"raw_fish","perch"},new[]{"raw_fish","crucian_carp","perch","eel"},new[]{"mackerel"},new[]{"raw_fish"}};
            for(var c=0;c<contexts.Length;c++){var outputs=Enumerable.Range(0,10000).Select(i=>Table.Entries[Table.Roll(contexts[c],(i+.5f)/10000)].Output.Id).Distinct();Assert.That(outputs,Is.EquivalentTo(expected[c]));}
            var deep=contexts[1];var perch=Enumerable.Range(0,10000).Count(i=>Table.Entries[Table.Roll(deep,(i+.5f)/10000)].Output.Id=="perch");Assert.That(perch,Is.EqualTo(2000));
            Assert.That(Table.Entries.Single(e=>e.Id=="perch").Matches(new FishingWaterContext(FishingWater.Inland,false,false,3)),Is.False,"Ford must not count as deep");Assert.That(Table.Roll(deep,float.NaN),Is.EqualTo(-1));
        }
        [TestCase(9472026)][TestCase(17491)][TestCase(-81027)]public void ActualGenerationSupportsReachableInlandWetlandsSeaAndDeep(int seed)
        {
            var rules=Resources.Load<SaveGameRules>("SaveGameRules");var map=new WorldMap(rules.World,seed,3,false);var wet=false;var deep=false;
            for(var y=-2200;y<=2200;y+=17){var p=new Vector2(map.RiverAt(y)+7,y);if(!FishingWaterContext.TryCast(map,p,Vector2.left,out var point) || !FishingWaterContext.TrySample(map,point,false,out var c))continue;wet|=c.Water==FishingWater.Wetlands;deep|=c.Deep && c.ShoreInset>=2.25f;}
            Assert.That(wet && deep,Is.True,"Actual shore positions must support table conditions");var ocean=new Vector2(map.Size.x*.49f,0);Assert.That(FishingWaterContext.TrySample(map,ocean,false,out var sea),Is.True);Assert.That(sea.Water,Is.EqualTo(FishingWater.Ocean));
            Assert.That(FishingWaterContext.TrySample(map,Vector2.zero,false,out _),Is.False);Assert.That(FishingWaterContext.TryCast(map,Vector2.zero,Vector2.right,out _),Is.False);
            var legacy=new WorldMap(rules.World,seed,1,false);var center=legacy.PondCenter;Assert.That(FishingWaterContext.TrySample(legacy,center,true,out var old),Is.True);Assert.That(old.Water,Is.EqualTo(FishingWater.Inland));
            var cave=map.Continental.Caves.First();var pool=cave.Rooms.FirstOrDefault(r=>r.Kind==CavernRoomKind.WaterChamber);if(pool!=null){var found=false;for(var x=-30;x<30;x++)for(var y=-30;y<30;y++){var p=pool.Center+new Vector2(x,y);if(FishingWaterContext.TrySample(map,p,true,out var water) && water.Water==FishingWater.Cavern){Assert.That(water.ShoreInset,Is.Zero);Assert.That(water.Deep,Is.False);found=true;}}Assert.That(found,Is.True);}
        }
        [Test]public void ProcessingPreservesExistingFoodAndInventoryCodecs()
        {
            Assert.That(C.Items.Length,Is.EqualTo(136));Assert.That(C.Recipes.Length,Is.EqualTo(111));Assert.That(C.Items[126].Id,Is.EqualTo("hunters_spear"));var bag=new InventoryModel(36);bag.Add(C.Item("raw_fish"),7);
            foreach(var id in new[]{"crucian_carp","perch","mackerel","eel"}){bag.Add(C.Item(id),1);var r=C.Recipe("fillet_"+id);Assert.That(r.RequiredStation,Is.EqualTo(StationKind.None));Assert.That(bag.TryExchange(r.Ingredients,r.Result,r.ResultCount),Is.EqualTo(ExchangeResult.Success));Assert.That(C.Item(id).Consumable.Meal,Is.Null);}
            Assert.That(bag.CountItem(C.Item("raw_fish")),Is.EqualTo(13));var restored=new InventoryModel(36);restored.Restore(bag.Capture(),C);Assert.That(restored.CountItem(C.Item("raw_fish")),Is.EqualTo(13));foreach(var id in new[]{"cooked_fish","fish_stew","mixed_stew","trail_broth"})Assert.That(C.Recipe(id).Ingredients.Any(i=>i.Item.Id=="raw_fish"),Is.True);
            Assert.That(C.Recipes.Count(r=>r.RequiredStation==StationKind.None),Is.EqualTo(10));Assert.That(C.Recipes.Count(r=>r.RequiredStation==StationKind.Campfire),Is.EqualTo(7));Assert.That(C.Recipes.Count(r=>r.RequiredStation==StationKind.CookingPot),Is.EqualTo(9));Assert.That(C.Recipe("fish_portions").ResultCount,Is.EqualTo(2));Assert.That(C.Item("fish_portions").Consumable.Meal,Is.Null);
        }
        [Test]public void OptionalConversionsDoNotMoveExistingProgressionSupplyBudgets()
        {
            var previous=ScriptableObject.CreateInstance<ContentCatalog>();previous.Items=C.Items.Take(127).ToArray();previous.Recipes=C.Recipes.Take(104).ToArray();
            try{foreach(var players in new[]{1,4})foreach(var tier in new[]{2,3}){var a=ProgressionEconomy.Calculate(previous,players,maximumTier:tier);var b=ProgressionEconomy.Calculate(C,players,maximumTier:tier);Assert.That(b.Total.Select(x=>x.Key.Id+":"+x.Value),Is.EquivalentTo(a.Total.Select(x=>x.Key.Id+":"+x.Value)));}Assert.That(C.Recipes.Count(r=>r.OptionalConversion),Is.EqualTo(4));}finally{UnityEngine.Object.DestroyImmediate(previous);}
        }
        [Test]public void CatchStateRoundTripsWithCurrentProtocolAndBothLanguages()
        {
            var a=new ActivityState{Sequence=51,Kind=ActivityKind.FishingCaught,Started=442.5,Duration=.6f,Point=new Vector2(18,-3),CatchIndex=6,CatchCount=1};using(var writer=new FastBufferWriter(128,Allocator.Temp)){writer.WriteNetworkSerializable(a);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out ActivityState b);Assert.That(b.Equals(a),Is.True);b.CatchIndex--;Assert.That(b.Equals(a),Is.False);b=a;b.CatchCount++;Assert.That(b.Equals(a),Is.False);}}
            Assert.That(NetworkProtocol.Current,Is.EqualTo(22));var language=L.Language;try{foreach(var lang in new[]{"en","ko"}){L.SetLanguage(lang);foreach(var e in Table.Entries){Assert.That(L.Text(e.NameKey),Is.Not.Empty);Assert.That(e.Output.Icon,Is.Not.Null);}foreach(var r in C.Recipes.Where(r=>r.Id.StartsWith("fillet_")))Assert.That(L.Text(r.DisplayKey),Is.Not.Empty);}}finally{L.SetLanguage(language);}
        }
    }
}
