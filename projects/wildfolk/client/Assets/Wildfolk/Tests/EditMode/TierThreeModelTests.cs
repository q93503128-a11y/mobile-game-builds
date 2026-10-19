using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class TierThreeModelTests
    {
        private static ContentCatalog C=>AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/Wildfolk/Data/ContentCatalog.asset");
        private static WorldGenerationDefinition W=>AssetDatabase.LoadAssetAtPath<WorldGenerationDefinition>("Assets/Wildfolk/Data/WorldGeneration.asset");
        [Test]public void ThirdWorkbenchUsesOnlyMaterialsAndEveryPersonalUpgradeConsumesSteel()
        {
            var two=C.Item("workbench_ii").Placement;Assert.That(WorkbenchProgression.CanUpgrade(two),Is.True);Assert.That(two.UpgradeTo.WorkbenchTier,Is.EqualTo(3));Assert.That(two.UpgradeCosts.Sum(x=>x.Item.Id=="diamond" ? x.Count : 0),Is.EqualTo(1));
            var total=0;foreach(var id in new[]{"axe","pickaxe","sword","spear","head","body"}){var r=C.Recipe("diamond_"+id);Assert.That(r.Ingredients.Any(a=>a.Item.Id=="steel_"+id && a.Count==1),Is.True);Assert.That(r.FutureUnlocks,Is.Empty);total+=r.Ingredients.Where(a=>a.Item.Id=="diamond").Sum(a=>a.Count);}Assert.That(total,Is.EqualTo(12));
        }
        [Test]public void DiamondPowerPreservesWeaponTimingAndArmorGrowthIsLimited()
        {
            foreach(var id in new[]{"axe","pickaxe","sword","spear"}){var old=C.Item("steel_"+id).Tool;var next=C.Item("diamond_"+id).Tool;Assert.That(next.Cooldown,Is.EqualTo(old.Cooldown));Assert.That(next.Attack.Range,Is.EqualTo(old.Attack.Range));Assert.That(next.Damage,Is.LessThanOrEqualTo(Mathf.CeilToInt(old.Damage*1.25f)));}
            foreach(var id in new[]{"head","body"})Assert.That(C.Item("diamond_"+id).Armor.Defense,Is.EqualTo(C.Item("steel_"+id).Armor.Defense+1));
        }
        [Test]public void DeepResourcesHaveCorrectGatingYieldRenewalAndNoSurfaceDependency()
        {
            foreach(var id in new[]{"diamond","crystal"}){var r=W.Resources.Single(x=>x.Prefab.Definition.DropItem.Id==id);var d=r.Prefab.Definition;Assert.That(d.DropCount,Is.EqualTo(id=="diamond" ? 3 : 6));Assert.That(r.RespawnSeconds,Is.EqualTo(id=="diamond" ? 300 : 150));Assert.That(d.CanHarvest(C.Item("iron_pickaxe").Tool),Is.False);Assert.That(d.CanHarvest(C.Item("steel_pickaxe").Tool),Is.True);Assert.That(W.NaturalSources.Where(s=>s.Enabled && s.ResourceRule>=0 && W.Resources[s.ResourceRule].Prefab.Definition.DropItem.Id==id).All(s=>s.Context==ResourceSourceContext.Cave && s.MinimumGenerationVersion==3),Is.True);}
        }
        [TestCase(1)][TestCase(4)]public void PersonalDiamondBudgetAndSharedUpgradeAreCountedOnce(int players)
        {var b=ProgressionEconomy.Calculate(C,players);Assert.That(b.Total[C.Item("diamond")],Is.EqualTo(players*12+1));Assert.That(b.Produced[C.Item("mechanical_part")],Is.GreaterThan(0));Assert.That(C.Recipe("mechanical_part").ResultCount,Is.EqualTo(2));Assert.That(C.Recipe("mechanical_part").Ingredients.Select(a=>a.Item.Id),Is.EquivalentTo(new[]{"bronze","steel"}));}
        [TestCase(9472026)][TestCase(42)][TestCase(1)][TestCase(20261019)]public void NewDeepSupplyIsInitiallySufficientAndEarlierSourcesDoNotMove(int seed)
        {var old=new WorldMap(W,seed,2);var next=new WorldMap(W,seed,3);foreach(var p in old.Resources){var source=next.Resources.Single(x=>x.Id==p.Id);Assert.That(source.Position,Is.EqualTo(p.Position));Assert.That(source.Rule,Is.EqualTo(p.Rule));}Assert.That(old.Resources.Any(p=>W.Resources[p.Rule].Prefab.Definition.DropItem.Id=="diamond"),Is.False);DeepCavernContent.Validate(next.Continental,W);Assert.That(DeepCavernContent.Sites(next.Continental).Select(s=>s.Kind).Distinct().Count(),Is.EqualTo(4));}
        [Test]public void TurretSnapshotRejectsIncompatibleCargoAndWrongModuleRoles()
        {
            var d=C.Item("ballista").Placement;var s=new TurretSnapshot{Ammo=new InventoryModel(3).Capture()};Assert.DoesNotThrow(()=>TurretState.Validate(s,d,C));var bag=new InventoryModel(3);bag.Add(C.Item("crossbow_bolt"),1);s.Ammo=bag.Capture();Assert.Throws<InvalidDataException>(()=>TurretState.Validate(s,d,C));s.Ammo=new InventoryModel(3).Capture();s.Modules[0]="auto_aim";Assert.Throws<InvalidDataException>(()=>TurretState.Validate(s,d,C));s.Modules[0]=null;s.Modules[4]="reinforced_frame";Assert.That(TurretState.MaximumHP(d,s,C),Is.EqualTo(d.Vitality.MaximumHealth*1.5f));
        }
        [Test]public void InlineJsonEmptyTurretStateIsAcceptedOnlyWithoutCargo(){Assert.That(TurretState.IsEmpty(JsonUtility.FromJson<TurretSnapshot>("{}")),Is.True);Assert.That(TurretState.IsEmpty(new TurretSnapshot{Loaded=true}),Is.False);}
        [Test]public void UtilityAndTurretRolesKeepExistingMaterialsRelevant()
        {Assert.That(C.Item("telescope").EquippedLight,Is.Null);Assert.That(C.Item("telescope").Telescope,Is.True);Assert.That(C.Item("crystal_lantern").EquippedLight.Radius,Is.EqualTo(C.Item("lantern").EquippedLight.Radius*1.35f).Within(.001));Assert.That(C.Item("repeater").Placement.Turret.Ammunition,Is.SameAs(C.Item("crossbow_bolt")));Assert.That(C.Item("ballista").Placement.Turret.Ammunition,Is.SameAs(C.Item("ballista_bolt")));Assert.That(C.Items.Where(i=>i.TurretModule!=null).Select(i=>i.TurretModule.Slot).Distinct().Count(),Is.EqualTo(5));}
    }
}
