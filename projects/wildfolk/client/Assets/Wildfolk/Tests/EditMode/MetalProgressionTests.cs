using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class MetalProgressionTests
    {
        private static T Data<T>(string name)where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private ContentCatalog Catalog=>Data<ContentCatalog>("ContentCatalog");
        [TestCase(0,false)][TestCase(1,true)][TestCase(2,true)][TestCase(6,true)][TestCase(7,false)][TestCase(-1,false)]
        public void WorkbenchRangeIsOneThroughSix(int tier,bool valid)=>Assert.That(WorkbenchProgression.IsValid(tier),Is.EqualTo(valid));
        [Test]public void OnlyConfiguredNextTierOpensAndLegacyUpgradeCostsRemain()
        {
            var one=Data<PlaceableDefinition>("WorkbenchPlacement");var two=one.UpgradeTo;
            Assert.That(WorkbenchProgression.CanUpgrade(one),Is.True);Assert.That(WorkbenchProgression.CanUpgrade(two),Is.True);Assert.That(two.WorkbenchTier,Is.EqualTo(2));
            Assert.That(one.UpgradeCosts.Select(c=>c.Item.Id+":"+c.Count),Is.EqualTo(new[]{"wood:8","stone:6","iron_ingot:2"}));
            var fake=UnityEngine.Object.Instantiate(two);fake.WorkbenchTier=6;fake.UpgradeTo=one;Assert.That(WorkbenchProgression.CanUpgrade(fake),Is.False);fake.WorkbenchTier=7;Assert.That(WorkbenchProgression.CanUpgrade(fake),Is.False);UnityEngine.Object.DestroyImmediate(fake);
        }
        [Test]public void FourInputsReserveAtomicallyAndRollbackFailureOrException()
        {
            var costs=new[]{new ItemAmount{Item=Catalog.Item("wood"),Count=8},new ItemAmount{Item=Catalog.Item("stone"),Count=6},new ItemAmount{Item=Catalog.Item("iron_ingot"),Count=2},new ItemAmount{Item=Catalog.Item("bronze"),Count=1}};
            var bag=new InventoryModel(8);foreach(var c in costs.Take(3))bag.Add(c.Item,c.Count);
            var before=JsonUtility.ToJson(new PlayerSnapshot{Inventory=bag.Capture()});var commits=0;var events=0;bag.Changed+=()=>events++;
            Assert.That(bag.TryConsumeCosts(costs,()=>{commits++;return true;}),Is.False);Assert.That(commits,Is.Zero);Assert.That(JsonUtility.ToJson(new PlayerSnapshot{Inventory=bag.Capture()}),Is.EqualTo(before));
            bag.Add(costs[3].Item,1);events=0;Assert.That(bag.TryConsumeCosts(costs,()=>false),Is.False);Assert.That(events,Is.Zero);
            Assert.Throws<InvalidOperationException>(()=>bag.TryConsumeCosts(costs,()=>throw new InvalidOperationException()));Assert.That(events,Is.Zero);Assert.That(bag.CountItem(costs[0].Item),Is.EqualTo(8));
            Assert.That(bag.TryConsumeCosts(costs,()=>{commits++;return true;}),Is.True);Assert.That(bag.TryConsumeCosts(costs),Is.False);Assert.That(commits,Is.EqualTo(1));Assert.That(events,Is.EqualTo(1));
        }
        [Test]public void AppendedOresAreDeterministicAndPreserveEveryLegacyPlacementAndId()
        {
            var world=Data<WorldGenerationDefinition>("WorldGeneration");var legacy=UnityEngine.Object.Instantiate(world);legacy.Resources=world.Resources.Take(5).ToArray();
            foreach(var seed in new[]{0,1,-9,42,9472026,int.MinValue,int.MaxValue})
            {
                var old=new WorldMap(legacy,seed);var map=new WorldMap(world,seed);var again=new WorldMap(world,seed);
                for(var i=0;i<old.Resources.Count;i++){Assert.That(map.Resources[i].Position,Is.EqualTo(old.Resources[i].Position),"legacy r"+i);Assert.That(map.Resources[i].Rule,Is.EqualTo(old.Resources[i].Rule));Assert.That(map.Resources[i].Scale,Is.EqualTo(old.Resources[i].Scale));}
                for(var i=old.Resources.Count;i<map.Resources.Count;i++){var p=map.Resources[i];Assert.That(map.ResourceSiteAllowed(p.Rule,p.Position),Is.True);Assert.That(p.Position,Is.EqualTo(again.Resources[i].Position));Assert.That(p.Scale,Is.EqualTo(again.Resources[i].Scale));}
                foreach(var index in new[]{5,6,7})Assert.That(map.Resources.Count(p=>p.Rule==index),Is.EqualTo(world.Resources[index].Count));
            }
            UnityEngine.Object.DestroyImmediate(legacy);
        }
        [Test]public void SupplyContextAndRecoveryAreIndependentOfTechnologyAndRespectMineralFamily()
        {
            var world=Data<WorldGenerationDefinition>("WorldGeneration");var map=new WorldMap(world,9472026);var rule=world.Resources[5];
            Assert.That(WorldMap.Supports(rule,world.Biome,ResourceSourceContext.Surface,0),Is.True);Assert.That(WorldMap.Supports(rule,world.Biome,ResourceSourceContext.Cave,0),Is.False);Assert.That(WorldMap.Supports(rule,world.Biome,ResourceSourceContext.Surface,10),Is.False);
            var population=new ResourcePopulation(map);var p=map.Resources.First(r=>r.Rule==5);population.Remove(5,p.Position,100,true);population.Remove(5,p.Position,100,true);population.Remove(5,p.Position,100,true);
            Assert.That(population.Pending.Count(t=>t.Rule==5),Is.EqualTo(1));Assert.That(population.Excluded(6,p.Position,101),Is.True);Assert.That(population.Excluded(0,p.Position,101),Is.False);
            var copy=new ResourcePopulation(map);copy.Restore(population.Capture());Assert.That(copy.Pending[0].Rule,Is.EqualTo(5));Assert.That(copy.Excluded(7,p.Position,101),Is.True);
            var bench=Data<PlaceableDefinition>("WorkbenchPlacement");var original=bench.WorkbenchTier;
            try{bench.WorkbenchTier=6;var after=new WorldMap(world,9472026);Assert.That(after.Resources.Select(r=>r.Position),Is.EqualTo(map.Resources.Select(r=>r.Position)));}finally{bench.WorkbenchTier=original;}
        }
        [Test]public void BronzeAndSteelReserveEveryIngredientAndPreserveBlockedOutputAndQueueSnapshot()
        {
            foreach(var id in new[]{"bronze","steel"})
            {
                var recipe=Catalog.Recipe(id);var bag=new InventoryModel(8);foreach(var c in recipe.Ingredients.Take(recipe.Ingredients.Length-1))bag.Add(c.Item,c.Count*2);
                var queue=new ProcessingQueue();Assert.That(queue.Add(recipe,1,bag,50,1),Is.False);foreach(var c in recipe.Ingredients.Take(recipe.Ingredients.Length-1))Assert.That(bag.CountItem(c.Item),Is.EqualTo(c.Count*2));
                var last=recipe.Ingredients.Last();bag.Add(last.Item,last.Count*2);Assert.That(queue.Add(recipe,2,bag,50,1),Is.True);queue.Advance(recipe.ProcessingSeconds*2,1,recipe.ResultCount);Assert.That(queue.OutputCount,Is.EqualTo(recipe.ResultCount));Assert.That(queue.Remaining,Is.Zero);
                var restored=new ProcessingQueue();restored.Restore(queue.Capture(),Catalog,1);Assert.That(restored.OutputCount,Is.EqualTo(recipe.ResultCount));Assert.That(restored.Current,Is.SameAs(recipe));Assert.That(restored.Remaining,Is.Zero);
                var full=new InventoryModel(1);full.Add(Catalog.Item("stone"),50);Assert.That(restored.Collect(full),Is.False);Assert.That(restored.OutputCount,Is.EqualTo(recipe.ResultCount));Assert.That(restored.Collect(bag),Is.True);restored.Advance(0,1,100);Assert.That(restored.OutputCount,Is.EqualTo(recipe.ResultCount));
            }
        }
        [Test]public void MiningToolPermissionHasNoFistFallbackAndMaterialKindsRemainDistinct()
        {
            foreach(var name in new[]{"CopperOreNode","TinOreNode","CoalNode"})
            {var r=Data<ResourceDefinition>(name+"Resource");Assert.That(r.CanHarvest(null),Is.False);Assert.That(r.CanHarvest(Data<ToolDefinition>("IronSwordTool")),Is.False);Assert.That(r.CanHarvest(Data<ToolDefinition>(name=="TinOreNode" ? "ProspectingPanTool" : "IronPickaxeTool")),Is.True);Assert.That(r.RequiredToolItem,Is.Not.Null);}
            Assert.That(Data<ResourceDefinition>("TinOreNodeResource").CanHarvest(Data<ToolDefinition>("PickaxeTool")),Is.False);
            Assert.That(Catalog.Items.Length,Is.EqualTo(132));Assert.That(Catalog.Recipes.Length,Is.EqualTo(109));Assert.That(AssetDatabase.FindAssets("t:BiomeDefinition").Length,Is.EqualTo(1));
        }
    }
}
