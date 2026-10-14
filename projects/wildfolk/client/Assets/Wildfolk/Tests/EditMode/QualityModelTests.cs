using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class QualityModelTests
    {
        private static T Data<T>(string name) where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        [Test]public void QueueReservesInputProcessesSingleUnitsAndWithdrawsOnlyWaiting()
        {
            var recipe=Data<ContentCatalog>("ContentCatalog").Recipe("iron_ingot");var bag=new InventoryModel(3);bag.Add(recipe.Ingredients[0].Item,10);var queue=new ProcessingQueue();
            Assert.That(queue.Add(recipe,1,bag,50,1),Is.True);Assert.That(queue.WaitingCount,Is.Zero);Assert.That(queue.Withdraw(recipe,bag),Is.False);
            Assert.That(queue.Add(recipe,9,bag,50,1),Is.True);Assert.That(bag.CountItem(recipe.Ingredients[0].Item),Is.Zero);Assert.That(queue.Withdraw(recipe,bag),Is.True);
            queue.Advance(recipe.ProcessingSeconds+.5f,1,100);Assert.That(queue.OutputCount,Is.EqualTo(1));Assert.That(queue.Current,Is.SameAs(recipe));Assert.That(queue.Remaining,Is.EqualTo(recipe.ProcessingSeconds-.5f).Within(.001));Assert.That(queue.WaitingCount,Is.EqualTo(7));
            Assert.That(queue.Collect(bag),Is.True);Assert.That(bag.CountItem(recipe.Result),Is.EqualTo(1));Assert.That(bag.CountItem(recipe.Ingredients[0].Item),Is.EqualTo(1));
        }
        [Test]public void OutputCapacityAndInventorySpacePreserveReservedCargo()
        {
            var catalog=Data<ContentCatalog>("ContentCatalog");var recipe=catalog.Recipe("iron_ingot");var bag=new InventoryModel(1);bag.Add(recipe.Ingredients[0].Item,3);var queue=new ProcessingQueue();queue.Add(recipe,3,bag,50,1);queue.Advance(100,1,1);
            Assert.That(queue.OutputCount,Is.EqualTo(1));Assert.That(queue.Remaining,Is.Zero);Assert.That(queue.WaitingCount,Is.EqualTo(1));bag.Add(catalog.Item("wood"),catalog.Item("wood").MaxStack);Assert.That(queue.Collect(bag),Is.False);Assert.That(queue.OutputCount,Is.EqualTo(1));Assert.That(queue.Withdraw(recipe,bag),Is.False);Assert.That(queue.WaitingCount,Is.EqualTo(1));
            bag.ConsumeSlot(0,catalog.Item("wood"),catalog.Item("wood").MaxStack);Assert.That(queue.Collect(bag),Is.True);queue.Advance(0,1,1);Assert.That(queue.OutputCount,Is.EqualTo(1));
        }
        [Test]public void QueueAndLegacyBatchRestoreWithoutMaterialDuplication()
        {
            var catalog=Data<ContentCatalog>("ContentCatalog");var recipe=catalog.Recipe("iron_ingot");var queue=new ProcessingQueue();queue.Restore(new ProcessingSnapshot{Recipe=recipe.Id,Batch=3,Remaining=4},catalog,1);Assert.That(queue.OutputCount,Is.EqualTo(1));Assert.That(queue.WaitingCount,Is.EqualTo(1));Assert.That(queue.Remaining,Is.EqualTo(2).Within(.001));
            var next=new ProcessingQueue();next.Restore(queue.Capture(),catalog,1);next.Advance(100,1,100);Assert.That(next.OutputCount,Is.EqualTo(3));Assert.That(next.Current,Is.Null);Assert.That(next.Release().Sum(x=>x.Count),Is.EqualTo(3));
        }
        [Test]public void QueueSaveRetainsAlternatingRecipeOrder()
        {
            var catalog=Data<ContentCatalog>("ContentCatalog");var a=catalog.Recipe("iron_ingot");var b=catalog.Recipe("cooked_meat");var bag=new InventoryModel(5);bag.Add(a.Ingredients[0].Item,3);bag.Add(b.Ingredients[0].Item,1);var queue=new ProcessingQueue();queue.Add(a,1,bag,10,1);queue.Add(b,1,bag,10,1);queue.Add(a,1,bag,10,1);
            var restored=new ProcessingQueue();restored.Restore(queue.Capture(),catalog,1);restored.Advance(a.ProcessingSeconds,1,100);Assert.That(restored.Current,Is.SameAs(b));restored.Advance(b.ProcessingSeconds,1,100);Assert.That(restored.Current,Is.SameAs(a));
        }
        [Test]public void SkillPrerequisiteAndLegacyLearnedRankAreBothRespected()
        {
            var obj=new GameObject();obj.SetActive(false);try{var p=obj.AddComponent<PlayerProgression>();p.Definition=Data<CharacterGrowthDefinition>("CharacterGrowth");p.SetValues(450,0);Assert.That(p.Invest(2),Is.False);Assert.That(p.Invest(1),Is.True);Assert.That(p.Invest(2),Is.True);var old=GrowthValidation.FromLegacy(450,p.Definition);old.Skills[2].Rank=1;old.Points--;p.Restore(old,450,0);Assert.That(p.Unlocked(2),Is.True);Assert.That(p.Invest(2),Is.True);}finally{UnityEngine.Object.DestroyImmediate(obj);}
        }
        [Test]public void PopulationUsesRegionalDeficitsAndWorldScopeForRareRules()
        {
            var definition=Data<WorldGenerationDefinition>("WorldGeneration");var map=new WorldMap(definition,definition.DefaultSeed);var population=new ResourcePopulation(map);var rare=Array.FindIndex(definition.Resources,r=>r.Rarity==ResourceRarity.Rare);Assert.That(rare,Is.GreaterThanOrEqualTo(0));var ores=map.Resources.Where(p=>p.Rule==rare).ToArray();
            Assert.That(population.Region(rare,ores[0].Position),Is.EqualTo(population.Region(rare,ores.Last().Position)));var target=population.Target(rare,0);Assert.That(target,Is.LessThan(ores.Length));population.Remove(rare,ores[0].Position,100,true);Assert.That(population.Pending,Is.Empty);population.Remove(rare,ores[1].Position,100,true);Assert.That(population.Pending.Count,Is.EqualTo(1));Assert.That(population.Excluded(rare,ores[0].Position,101),Is.True);
            var distances=Enumerable.Range(0,40).Select(_=>Vector2.Distance(population.Candidate(rare,0),ores[0].Position));Assert.That(distances.Max(),Is.GreaterThan(50));
        }
        [Test]public void RecoverySaveKeepsExclusionsAndCandidateSequence()
        {
            var definition=Data<WorldGenerationDefinition>("WorldGeneration");var map=new WorldMap(definition,definition.DefaultSeed);var a=new ResourcePopulation(map);var p=map.Resources[0];a.Remove(p.Rule,p.Position,0,true);a.Candidate(p.Rule,0);a.Serial=7;var b=new ResourcePopulation(map);b.Restore(a.Capture());Assert.That(b.Serial,Is.EqualTo(7));Assert.That(b.Excluded(p.Rule,p.Position,1),Is.True);Assert.That(b.Candidate(p.Rule,0),Is.EqualTo(a.Candidate(p.Rule,0)));
        }
        [Test]public void LocalizationHasTwoCompleteLanguagesAndEquivalentPlaceholders()
        {
            var catalog=L.Catalog;Assert.That(catalog.Languages.Select(x=>x.Id),Is.EqualTo(new[]{"en","ko"}));Assert.That(catalog.Entries.Select(x=>x.Key).Distinct().Count(),Is.EqualTo(catalog.Entries.Length));foreach(var entry in catalog.Entries){Assert.That(entry.Values.Length,Is.EqualTo(2),entry.Key);Assert.That(entry.Values.All(x=>!string.IsNullOrWhiteSpace(x)),Is.True,entry.Key);var a=Regex.Matches(entry.Values[0],@"\{\d+(?::[^}]+)?\}").Cast<Match>().Select(x=>x.Value).OrderBy(x=>x).ToArray();var b=Regex.Matches(entry.Values[1],@"\{\d+(?::[^}]+)?\}").Cast<Match>().Select(x=>x.Value).OrderBy(x=>x).ToArray();Assert.That(b,Is.EqualTo(a),entry.Key);}
            foreach(var item in Data<ContentCatalog>("ContentCatalog").Items)Assert.That(L.Has("item."+item.Id+".name"),Is.True,item.Id);
        }
        [Test]public void LanguagePersistsAsLocalPreferenceAndRefreshesCatalog()
        {
            const string prefix="wildfolk.quality.language.";var previous=L.Language;try{var settings=new LocalSettings();Assert.That(settings.Language,Is.EqualTo("en"));settings.SetLanguage("ko");settings.Store(prefix);Assert.That(LocalSettings.Load(prefix).Language,Is.EqualTo("ko"));L.SetLanguage("ko");Assert.That(L.Text("ui.inventory.title"),Is.EqualTo("인벤토리"));L.SetLanguage("en");Assert.That(L.Text("ui.inventory.title"),Is.EqualTo("Inventory"));}finally{L.SetLanguage(previous);foreach(var key in new[]{"language","controls","master","music","sfx"})PlayerPrefs.DeleteKey(prefix+key);}
        }
    }
}
