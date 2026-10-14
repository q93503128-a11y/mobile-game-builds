using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class CharacterPassModelTests
    {
        private static T Data<T>(string name) where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        [Test]public void LevelCurveCarriesRemainderAndAwardsPoints()
        {var obj=new GameObject();obj.SetActive(false);try{var p=obj.AddComponent<PlayerProgression>();p.Definition=Data<CharacterGrowthDefinition>("CharacterGrowth");p.SetValues(0,0);p.Reward(34,1);Assert.That(p.Level,Is.EqualTo(1));p.Reward(8,3);Assert.That(p.Level,Is.EqualTo(2));Assert.That(p.CurrentXP,Is.EqualTo(7));Assert.That(p.SkillPoints,Is.EqualTo(1));Assert.That(p.Experience,Is.EqualTo(42));Assert.That(p.Gold,Is.EqualTo(4));p.Reward(55,0);Assert.That(p.Level,Is.EqualTo(3));Assert.That(p.CurrentXP,Is.EqualTo(7));Assert.That(p.SkillPoints,Is.EqualTo(2));}finally{UnityEngine.Object.DestroyImmediate(obj);}}
        [Test]public void SkillsSpendPointsRespectRanksAndRestoreEffects()
        {var obj=new GameObject();obj.SetActive(false);try{var p=obj.AddComponent<PlayerProgression>();p.Definition=Data<CharacterGrowthDefinition>("CharacterGrowth");p.SetValues(0,0);Assert.That(p.Invest(0),Is.False);p.SetValues(225,0);Assert.That(p.Level,Is.EqualTo(4));for(var i=0;i<3;i++)Assert.That(p.Invest(0),Is.True);Assert.That(p.Invest(0),Is.False);Assert.That(p.SkillPoints,Is.Zero);Assert.That(p.Bonus(SkillEffect.MaximumHealth),Is.EqualTo(30));var s=p.Capture();p.SetValues(0,0);p.Restore(s,225,0);Assert.That(p.Rank(0),Is.EqualTo(3));Assert.That(p.Bonus(SkillEffect.MaximumHealth),Is.EqualTo(30));}finally{UnityEngine.Object.DestroyImmediate(obj);}}
        [Test]public void InvalidPointsRanksAndXPAreRejected()
        {var data=Data<CharacterGrowthDefinition>("CharacterGrowth");var s=GrowthValidation.FromLegacy(42,data);GrowthValidation.Validate(s,data,42);s.Points++;Assert.Throws<InvalidDataException>(()=>GrowthValidation.Validate(s,data,42));s.Points--;s.Skills[0].Rank=9;Assert.Throws<InvalidDataException>(()=>GrowthValidation.Validate(s,data,42));s.Skills[0].Rank=0;s.CurrentXP++;Assert.Throws<InvalidDataException>(()=>GrowthValidation.Validate(s,data,42));}
        [Test]public void SprintUsesHoldHysteresisAndExhaustionRecovery()
        {var d=Data<LocomotionDefinition>("Locomotion");var m=new StaminaModel(d);m.Advance(.39f,1,true,1,1,1);Assert.That(m.IsSprinting,Is.False);m.Advance(.02f,1,true,1,1,1);Assert.That(m.IsSprinting,Is.True);m.Advance(1,.8f,true,1,1,1);Assert.That(m.IsSprinting,Is.True);m.Advance(20,1,true,1,1,1);Assert.That(m.Current,Is.Zero);Assert.That(m.IsSprinting,Is.False);m.Advance(.2f,1,true,1,1,1);Assert.That(m.IsSprinting,Is.False);m.Advance(3,0,true,1,1,1);Assert.That(m.Current,Is.GreaterThan(d.ExhaustedRestart));m.Advance(.41f,1,true,1,1,1);Assert.That(m.IsSprinting,Is.True);}
        [Test]public void DashSpendingIsAtomicAndRecoveryHonorsDelayAndCondition()
        {var d=Data<LocomotionDefinition>("Locomotion");var m=new StaminaModel(d);Assert.That(m.Spend(24),Is.True);Assert.That(m.Current,Is.EqualTo(76));Assert.That(m.Spend(90),Is.False);Assert.That(m.Current,Is.EqualTo(76));m.Advance(d.RecoveryDelay,0,false,0,1,1);Assert.That(m.Current,Is.EqualTo(76));m.Advance(1,0,false,0,1,.45f);Assert.That(m.Current,Is.EqualTo(83.2f).Within(.01f));}
        [Test]public void SurvivalPreventsCriticalSprintAndChangesEfficiency()
        {var needs=new SurvivalModel(Data<SurvivalDefinition>("Survival"));needs.SetMeters(29,80);Assert.That(needs.CanSprint,Is.False);needs.SetMeters(50,80);Assert.That(needs.CanSprint,Is.True);Assert.That(needs.SprintEfficiency,Is.LessThan(1));var d=Data<LocomotionDefinition>("Locomotion");var healthy=new StaminaModel(d);var hungry=new StaminaModel(d);healthy.Advance(1,1,true,1,1,1);hungry.Advance(1,1,true,needs.SprintEfficiency,1,1);Assert.That(hungry.Current,Is.LessThan(healthy.Current));}
        [Test]public void ArmorCostsSlotsAndSilhouettesAreActualData()
        {var catalog=Data<ContentCatalog>("ContentCatalog");foreach(var part in new[]{"head","body"}){var leather=catalog.Item("leather_"+part);var iron=catalog.Item("iron_"+part);Assert.That(iron.Armor.Defense,Is.GreaterThan(leather.Armor.Defense));Assert.That(iron.Armor.Slot,Is.EqualTo(leather.Armor.Slot));Assert.That(iron.Armor.WornSprite,Is.Not.Null);Assert.That(catalog.Recipe(leather.Id).RequiredStation,Is.EqualTo(StationKind.Workbench));Assert.That(catalog.Recipe(iron.Id).RequiredWorkbenchTier,Is.EqualTo(1));}}
        [Test]public void CategoriesAndStableTierSortDoNotDependOnNewFlags()
        {var catalog=Data<RecipeCatalog>("Recipes");var sorted=RecipePresentation.Sorted(catalog);for(var i=1;i<sorted.Length;i++)Assert.That(RecipePresentation.Compare(sorted[i-1],sorted[i]),Is.LessThanOrEqualTo(0));var again=RecipePresentation.Sorted(catalog);Assert.That(again,Is.EqualTo(sorted));Assert.That(Array.Exists(sorted,r=>r.Category==RecipeCategory.Armor),Is.True);Assert.That(Array.Exists(sorted,r=>r.RequiredWorkbenchTier==2),Is.True);Assert.That(Data<PlaceableDefinition>("WorkbenchPlacement").UpgradeTo,Is.SameAs(Data<PlaceableDefinition>("WorkbenchIIPlacement")));Assert.That(Data<PlaceableDefinition>("ReinforcedChestPlacement").StorageSlots,Is.EqualTo(32));}
        [Test]public void V1EnvelopeStillDecodesAndFutureVersionsFail()
        {var game=new SavedGame{Version=1,Player=new PlayerSnapshot{XP=123}};Assert.That(SaveRepository.Decode(SaveRepository.Encode(game)).Version,Is.EqualTo(1));Assert.Throws<InvalidDataException>(()=>SaveRepository.Decode(SaveRepository.Encode(new SavedGame{Version=99})));}
    }
}
