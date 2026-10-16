using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class StabilityModelTests
    {
        private static T Data<T>(string name) where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        [Test]public void EveryCurrentBasicAttackHasOneTargetAndExtensibleShape()
        {
            var catalog=Data<ContentCatalog>("ContentCatalog");Assert.That(catalog.Items.Length,Is.EqualTo(34));Assert.That(catalog.Recipes.Length,Is.EqualTo(25));
            Assert.That(Data<GameplaySettings>("GameplaySettings").PunchAttack.MaxTargets,Is.EqualTo(1));
            foreach(var item in catalog.Items)if(item.Tool!=null){var p=item.Tool.Attack;Assert.That(p.MaxTargets,Is.EqualTo(1));Assert.That(p.Range,Is.GreaterThan(0));Assert.That(p.ArcAngle,Is.InRange(1,360));Assert.That(p.PierceCount,Is.Zero);Assert.That(p.AoERadius,Is.Zero);}
        }
        [Test]public void CupStateChangesAreAtomicAndConserveContainersOnFullBags()
        {
            var cup=Data<WaterContainerDefinition>("WoodenCupContainer");var one=new InventoryModel(1);one.Add(cup.Empty,2);
            var costs=new[]{new ItemAmount{Item=cup.Empty,Count=1}};Assert.That(one.TryExchange(costs,cup.Raw,1),Is.EqualTo(ExchangeResult.NoSpace));Assert.That(one.CountItem(cup.Empty),Is.EqualTo(2));
            var bag=new InventoryModel(2);bag.Add(cup.Raw,1);bag.Add(Data<ItemDefinition>("Wood"),50);
            Assert.That(bag.ReplaceConsumedSlot(0,cup.Raw,cup.Empty),Is.True);Assert.That(bag.GetSlot(0).Item,Is.SameAs(cup.Empty));Assert.That(bag.CountItem(cup.Empty),Is.EqualTo(1));
        }
        [Test]public void RawWaterPenaltyExpiresPreciselyAndSurvivesStateRoundTrip()
        {
            var needs=Data<SurvivalDefinition>("Survival");var raw=Data<ItemDefinition>("RawWaterCup").Consumable;var clean=Data<ItemDefinition>("CleanWaterCup").Consumable;
            var model=new SurvivalModel(needs);model.SetMeters(80,50);model.Restore(raw);Assert.That(model.Thirst,Is.EqualTo(65));Assert.That(model.RecoveryBlockRemaining,Is.EqualTo(60));
            model.Advance(20);var copy=new SurvivalModel(needs);copy.Restore(model.Capture());Assert.That(copy.DrainPenaltyRemaining,Is.EqualTo(40));
            var before=copy.Thirst;copy.Advance(50);Assert.That(copy.Thirst,Is.EqualTo(before-needs.ThirstPerSecond*(40*1.5f+10)).Within(.001f));Assert.That(copy.RecoveryBlockRemaining,Is.Zero);Assert.That(copy.DrainMultiplier,Is.EqualTo(1));
            copy.Restore(clean);Assert.That(copy.RecoveryBlockRemaining,Is.Zero);Assert.That(clean.ThirstRestore,Is.EqualTo(38));
        }
        [Test]public void InventoryRestoreRejectsUnknownAndOversizedStacksWithoutPartialMutation()
        {
            var catalog=Data<ContentCatalog>("ContentCatalog");var bag=new InventoryModel(2);bag.Add(catalog.Item("wood"),7);
            Assert.Throws<InvalidOperationException>(()=>bag.Restore(new[]{new SavedStack{Item="wood",Count=4},new SavedStack{Item="missing",Count=1}},catalog));Assert.That(bag.CountItem(catalog.Item("wood")),Is.EqualTo(7));
            Assert.Throws<ArgumentException>(()=>bag.Restore(new[]{new SavedStack{Item="wood",Count=51},new SavedStack()},catalog));Assert.That(bag.CountItem(catalog.Item("wood")),Is.EqualTo(7));
        }
        [Test]public void AtomicSaveBackupRecoversCorruptionAndIgnoresInterruptedTemporaryFile()
        {
            var root=Path.Combine(Path.GetTempPath(),"wildfolk-save-"+Guid.NewGuid().ToString("N"));var repo=new SaveRepository(Path.Combine(root,"world.json"));
            Action<SavedGame> validate=g=>{if(g.Version!=6)throw new InvalidDataException();};
            try
            {
                repo.Write(new SavedGame{Seed=1},validate);repo.Write(new SavedGame{Seed=2},validate);
                File.WriteAllText(repo.Path+".temporary","interrupted");Assert.That(repo.Read(validate).Seed,Is.EqualTo(2));
                File.WriteAllText(repo.Path,"corrupt");Assert.That(repo.Read(validate).Seed,Is.EqualTo(1));Assert.That(repo.UsedBackup,Is.True);
                repo.Write(new SavedGame{Seed=3},validate);File.WriteAllText(repo.Path,"corrupt again");Assert.That(repo.Read(validate).Seed,Is.EqualTo(1),"A corrupt primary must not overwrite the last valid backup.");
            }
            finally{if(Directory.Exists(root))Directory.Delete(root,true);}
        }
        [Test]public void ChecksumAndFutureVersionAreRejected()
        {
            var data=SaveRepository.Encode(new SavedGame{Seed=17});Assert.Throws<InvalidDataException>(()=>SaveRepository.Decode(data.Replace("17","18")));
            Assert.Throws<InvalidDataException>(()=>SaveRepository.Decode(SaveRepository.Encode(new SavedGame{Version=99})));
        }
        [Test]public void RenewalAndTerrainDataKeepDifferentResourceTimes()
        {
            var world=Data<WorldGenerationDefinition>("WorldGeneration");Assert.That(world.ShallowMoveMultiplier,Is.EqualTo(.7f));Assert.That(world.Resources[0].RespawnSeconds,Is.EqualTo(300));Assert.That(world.Resources[1].RespawnSeconds,Is.EqualTo(600));Assert.That(world.Resources[2].RespawnSeconds,Is.EqualTo(720));Assert.That(world.Resources[4].RespawnSeconds,Is.EqualTo(90));
        }
        [Test]public void HostileRelationIsNotAPlayerTeamDespiteDefaultNumericId()
        {
            var hostile=new GameObject("Hostile relation assertion");var friendly=new GameObject("Player relation assertion");hostile.SetActive(false);friendly.SetActive(false);
            try
            {
                var a=hostile.AddComponent<Health>();var b=friendly.AddComponent<Health>();a.Definition=b.Definition=Data<VitalityDefinition>("PlayerVitality");hostile.AddComponent<CombatAffiliation>().Relation=EntityRelation.Hostile;friendly.AddComponent<CombatAffiliation>().Relation=EntityRelation.PlayerTeam;hostile.SetActive(true);friendly.SetActive(true);a.RestoreHealth(100);b.RestoreHealth(100);
                Assert.That(CombatAffiliation.CanDamage(a,b),Is.True);hostile.GetComponent<CombatAffiliation>().Relation=EntityRelation.PlayerTeam;Assert.That(CombatAffiliation.CanDamage(a,b),Is.False);
            }
            finally{UnityEngine.Object.DestroyImmediate(hostile);UnityEngine.Object.DestroyImmediate(friendly);}
        }
        [Test]public void EditorValidatorAcceptsCurrentDataAndDeveloperToolsStayInEditorAssembly()
        {
            Type validator=null,harness=null;foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()){validator=validator ?? assembly.GetType("Wildfolk.Editor.ProjectDataValidator");harness=harness ?? assembly.GetType("Wildfolk.Editor.DeveloperHarness");}
            Assert.That(validator,Is.Not.Null);Assert.That(harness,Is.Not.Null);Assert.That(harness.Assembly.GetName().Name,Does.Contain("Editor"));var errors=(System.Collections.Generic.List<string>)validator.GetMethod("Validate").Invoke(null,null);Assert.That(errors,Is.Empty,string.Join("\n",errors));
        }
    }
}
