using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ClassProgressionModelTests
    {
        private string directory;private LocalAccountProgressionStore store;private const string Id="aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private CharacterGrowthDefinition Data=>AssetDatabase.LoadAssetAtPath<CharacterGrowthDefinition>("Assets/Wildfolk/Data/CharacterGrowth.asset");
        [SetUp]public void Setup(){directory=Path.Combine(Path.GetTempPath(),"LowfireClass-"+Guid.NewGuid().ToString("N"));store=new LocalAccountProgressionStore(directory);ClassProfiles.DevelopmentProviders(store);}
        [TearDown]public void Close(){ClassProfiles.DevelopmentProviders(null);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [TestCase(20)][TestCase(99)][TestCase(999)]public void LinearLevelsCarryAcrossOldBoundaries(int before)
        {var data=Data;var xp=checked((long)data.TotalBefore(before+1));var state=GrowthValidation.FromLegacy(xp,data);Assert.That(state.Level,Is.EqualTo(before+1));Assert.That(state.CurrentXP,Is.Zero);Assert.That(state.Points,Is.EqualTo(before));GrowthValidation.Validate(state,data,xp);}
        [Test]public void LongMaximumXPUsesBoundedArithmetic()
        {var data=Data;var state=GrowthValidation.FromLegacy(long.MaxValue,data);Assert.That(state.Level,Is.GreaterThan(1000));Assert.That(state.CurrentXP,Is.LessThan(data.RequiredXP(state.Level)));GrowthValidation.Validate(state,data,long.MaxValue);}
        [Test]public void TotalXPOverflowDoesNotPartiallyMutate()
        {var go=new GameObject();go.SetActive(false);try{var p=go.AddComponent<PlayerProgression>();p.Definition=Data;p.SetValues(long.MaxValue,4);Assert.Throws<OverflowException>(()=>p.Reward(1,1));Assert.That(p.Experience,Is.EqualTo(long.MaxValue));Assert.That(p.Gold,Is.EqualTo(4));}finally{UnityEngine.Object.DestroyImmediate(go);}}
        [Test]public void OldVitalityThreeAndSurplusCapXPArePreserved()
        {
            var data=Data;var old=new GrowthSnapshot{Level=20,CurrentXP=data.RequiredXP(20)+22,Points=16,Skills=new[]{new SavedSkill{Id="vitality",Rank=3},new SavedSkill{Id="endurance"},new SavedSkill{Id="recovery"},new SavedSkill{Id="stride"}}};var xp=(long)data.TotalBefore(20)+old.CurrentXP;
            GrowthValidation.Migrate(old,data,xp);Assert.That(old.Level,Is.EqualTo(21));Assert.That(old.CurrentXP,Is.EqualTo(22));Assert.That(old.Points,Is.EqualTo(17));Assert.That(old.Skills[0].Rank,Is.EqualTo(3));Assert.That(old.Skills.Skip(4).All(s=>s.Rank==0),Is.True);GrowthValidation.Validate(old,data,xp);
        }
        [Test]public void AllSharedSkillRanksAndSurplusPointsValidate()
        {var data=Data;Assert.That(data.Skills.Select(s=>s.MaximumRank),Is.EqualTo(new[]{10,3,3,3,5,3}));var xp=(long)data.TotalBefore(1001);var state=GrowthValidation.FromLegacy(xp,data);foreach(var s in state.Skills){s.Rank=data.Skills.Single(d=>d.Id==s.Id).MaximumRank;state.Points-=s.Rank;}GrowthValidation.Validate(state,data,xp);Assert.That(state.Points,Is.EqualTo(973));Assert.That(GrowthValidation.Bonus(state,data,SkillEffect.Metabolism),Is.EqualTo(.25f).Within(.0001));}
        [Test]public void ClassCatalogHasSixIndependentNineNodeTrees()
        {var c=ClassCatalog.Current;Assert.That(c.Classes.Length,Is.EqualTo(6));foreach(var d in c.Classes){Assert.That(d.Nodes.Length,Is.EqualTo(9));Assert.That(d.Nodes.Select(n=>n.Id).Distinct().Count(),Is.EqualTo(9));Assert.That(d.Nodes.Count(n=>string.IsNullOrEmpty(n.Prerequisite)),Is.EqualTo(3));Assert.That(d.Price,Is.GreaterThanOrEqualTo(0));foreach(var n in d.Nodes)Assert.That(n.Amount,Is.GreaterThan(0));}}
        [Test]public void EarnablePointsComeFromMilestoneEntries()
        {var c=new ClassCatalog{Milestones=new long[]{1,2,3,4,5,6,7,8}};Assert.That(c.Earned(7),Is.EqualTo(7));Assert.That(c.Earned(100),Is.EqualTo(8));Assert.That(ClassCatalog.Current.Earned(1799),Is.EqualTo(5));Assert.That(ClassCatalog.Current.Earned(1800),Is.EqualTo(6));}
        [Test]public void DurableProfileSurvivesReopenAndDoesNotShareClasses()
        {var d=AccountProgression.New();var hunter=d.Find("hunter");hunter.Owned=true;hunter.XP=1800;hunter.Nodes=new[]{"A1","A2","A3","B1","B2","B3"};Assert.That(store.Write(Id,d),Is.True);var restored=new LocalAccountProgressionStore(directory).Read(Id);Assert.That(restored.Find("hunter").XP,Is.EqualTo(1800));Assert.That(restored.Find("medic").XP,Is.Zero);Assert.That(restored.Find("engineer").Owned,Is.False);Assert.That(restored.Find("hunter").Nodes.Length,Is.EqualTo(6));}
        [TestCase("A2")][TestCase("Z1")]
        public void InvalidClassPrerequisiteOrNodeFailsClosed(string node)
        {var d=AccountProgression.New();d.Find("survivor").XP=1800;d.Find("survivor").Nodes=new[]{node};Assert.That(store.Write(Id,d),Is.False);}
        [Test]public void DuplicateAndInsufficientClassPointAllocationsFailClosed()
        {var d=AccountProgression.New();d.Find("survivor").Nodes=new[]{"A1"};Assert.That(store.Write(Id,d),Is.False);d.Find("survivor").XP=1800;d.Find("survivor").Nodes=new[]{"A1","A1"};Assert.That(store.Write(Id,d),Is.False);}
        [Test]public void BrokenPrimaryUsesValidBackupWithoutResettingOwnership()
        {var d=AccountProgression.New();d.Find("medic").Owned=true;Assert.That(store.Write(Id,d),Is.True);d.Find("medic").XP=50;Assert.That(store.Write(Id,d),Is.True);File.WriteAllText(Path.Combine(directory,"class-"+Id+".json"),"broken");Assert.That(store.Read(Id).Find("medic").Owned,Is.True);}
        [Test]public void BrokenProfileWithoutBackupDoesNotGrantDefaultOwnership()
        {Directory.CreateDirectory(directory);File.WriteAllText(Path.Combine(directory,"class-"+Id+".json"),"broken");Assert.Throws<InvalidDataException>(()=>store.Read(Id));}
        [Test]public void PremiumEngineerRequiresProviderAtCanonicalPrice()
        {Assert.That(ClassProfiles.Premium.Available,Is.False);var e=ClassCatalog.Current.Find("engineer");Assert.That(e.Currency,Is.EqualTo("PremiumGem"));Assert.That(e.Price,Is.EqualTo(EconomyCatalog.Current.EngineerCost));Assert.That(AccountProgression.New().Find("engineer").Owned,Is.False);Assert.That(ReviveServices.Wallet.Available,Is.False);Assert.That(ReviveServices.Rewarded.Available,Is.False);}
        [Test]public void MetabolismReducesDrainRatherThanIncreasingNutrition()
        {var definition=AssetDatabase.LoadAssetAtPath<SurvivalDefinition>("Assets/Wildfolk/Data/Survival.asset");var a=new SurvivalModel(definition);var b=new SurvivalModel(definition){MetabolicMultiplier=.75f};a.Advance(60);b.Advance(60);Assert.That(100-b.Hunger,Is.EqualTo((100-a.Hunger)*.75f).Within(.001));Assert.That(100-b.Thirst,Is.EqualTo((100-a.Thirst)*.75f).Within(.001));}
        [Test]public void RepairDataCoversEveryPlaceableAndUpgradeOnce()
        {var c=Resources.Load<SaveGameRules>("SaveGameRules").Catalog;foreach(var i in c.Items.Where(x=>x.Placement!=null && (c.Recipes.Any(r=>r.Result==x) || x.Placement.Station==StationKind.Workbench))){Assert.That(i.Placement.RepairMaterials.Length,Is.GreaterThan(0),i.Id);Assert.That(i.Placement.ConstructionMaterials.Length,Is.GreaterThan(0),i.Id);Assert.That(i.Placement.RepairMaterials.All(x=>!new[]{"diamond","crystal","mechanical_part"}.Contains(x.Item.Id)),Is.True);}
            var wb=c.Item("workbench_iii").Placement;Assert.That(wb.ConstructionMaterials.Single(x=>x.Item.Id=="wood").Count,Is.EqualTo(24));Assert.That(wb.ConstructionMaterials.Single(x=>x.Item.Id=="diamond").Count,Is.EqualTo(1));}
        [Test]public void EveryCurrentMedicineHasInstantApplicationAndGenericClassification()
        {var medicines=Resources.Load<SaveGameRules>("SaveGameRules").Catalog.Items.Where(i=>i.Consumable?.Kind==ConsumableKind.Medicine).ToArray();Assert.That(medicines.Length,Is.EqualTo(2));foreach(var m in medicines)Assert.That(m.Consumable.ApplicationSeconds,Is.Zero);}
        [Test]public void ExplorationCategoriesCoverAllThreeExistingToolKinds()
        {var c=Resources.Load<SaveGameRules>("SaveGameRules").Catalog;Assert.That(c.Items.Count(i=>i.ExplorationCategory==ExplorationToolCategory.Optic),Is.EqualTo(1));Assert.That(c.Items.Count(i=>i.ExplorationCategory==ExplorationToolCategory.Survey),Is.EqualTo(1));Assert.That(c.Items.Count(i=>i.ExplorationCategory==ExplorationToolCategory.Signal),Is.EqualTo(1));}
    }
}
