using System;
using System.Linq;
using NUnit.Framework;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Wildfolk.Tests
{
    public sealed class CreatureLootModelTests
    {
        private static T D<T>(string name)where T:Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static ContentCatalog C=>D<ContentCatalog>("ContentCatalog");
        private static string[] ValidateData(){var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Wildfolk.Editor.ProjectDataValidator")).First(t=>t!=null);return ((System.Collections.Generic.List<string>)type.GetMethod("Validate").Invoke(null,null)).ToArray();}
        [TestCase("bone",4)][TestCase("chitin",3)][TestCase("medicinal_gland",2)]
        public void RawMaterialsHaveDistinctImplementedSinks(string id,int sinks)
        {var recipes=C.Recipes.Where(r=>r.Ingredients.Any(a=>a.Item.Id==id)).ToArray();Assert.That(recipes.Length,Is.EqualTo(sinks));Assert.That(recipes.Select(r=>r.Result.Id).Distinct().Count(),Is.EqualTo(sinks));Assert.That(recipes.Any(r=>r.ResultCount>1 || r.Result.Consumable!=null),Is.True);Assert.That(recipes.All(r=>r.FutureUnlocks.Length==0),Is.True);}
        [Test]public void SidegradesPreserveDiamondAndNaturalCrystalRoles()
        {Assert.That(C.Item("bone_arrow").AmmoDamage,Is.LessThan(C.Item("bronze_arrow").AmmoDamage));Assert.That(C.Item("barbed_bolt").AmmoDamage,Is.LessThan(C.Item("steel_bolt").AmmoDamage));Assert.That(C.Item("carapace_vest").Armor.Defense,Is.LessThan(C.Item("iron_body").Armor.Defense));Assert.That(C.Item("carapace_vest").Armor.StaminaRecoveryBonus,Is.GreaterThan(0));var crystal=D<LootTable>("CrystalCrawlerLoot").Items.Single(a=>a.Item.Id=="crystal");Assert.That(crystal.Maximum,Is.EqualTo(1));Assert.That(crystal.Chance,Is.LessThanOrEqualTo(.15f));Assert.That(D<AnimalPopulationDefinition>("AnimalPopulation").Rules.All(r=>r.Prefab.GetComponent<LootOnDeath>().Table.Items.All(a=>a.Item.Id!="diamond" && a.Item.Id!="mechanical_part")),Is.True);}
        [Test]public void StationRolesAndNewFoodEffectsAreReal()
        {Assert.That(C.Recipes.Count(r=>r.RequiredStation==StationKind.Furnace),Is.EqualTo(4));Assert.That(C.Recipes.Single(r=>r.RequiredStation==StationKind.BlastFurnace).Id,Is.EqualTo("steel"));foreach(var id in new[]{"bone_broth","restorative_broth"}){var r=C.Recipe(id);Assert.That(r.RequiredStation,Is.EqualTo(StationKind.CookingPot));Assert.That(r.ProcessingSeconds,Is.GreaterThan(0));}Assert.That(C.Item("bone_broth").Consumable.HealthRestore,Is.GreaterThan(0));Assert.That(C.Item("restorative_broth").Consumable.StaminaRestore,Is.GreaterThan(0));Assert.That(C.Item("healing_salve").Consumable.HungerRestore,Is.Zero);Assert.That(C.Recipe("healing_salve").RequiredStation,Is.EqualTo(StationKind.None));}
        [Test]public void OriginalFiveRuleIndicesAreStableAndNewActionsHaveReadableWindows()
        {var p=D<AnimalPopulationDefinition>("AnimalPopulation");Assert.That(p.Rules.Take(5).Select(r=>r.Prefab.Member.Definition.Id),Is.EqualTo(new[]{"chicken","boar","rabbit","deer","sheep"}));Assert.That(p.HardCap,Is.EqualTo(80));foreach(var r in p.Rules.Skip(5)){var d=r.Prefab.Member.Definition;var a=d.Actions;Assert.That(a,Is.Not.Null);Assert.That(r.Prefab.GetComponent<WildlifeActionBrain>(),Is.Not.Null);Assert.That(r.ChunkChance,Is.InRange(.01f,1f));Assert.That(r.RegionalTarget,Is.GreaterThan(0));if(a.Attack!=WildlifeAttack.None){Assert.That(a.WarningSeconds+a.WindupSeconds,Is.GreaterThanOrEqualTo(.6f));Assert.That(a.RecoverySeconds,Is.GreaterThan(.5f));Assert.That(a.Leash,Is.GreaterThan(a.Detection));}}}
        [Test]public void CamouflageFlightAndCrystalHabitatUseDifferentAuthoredShapes()
        {var stone=D<CreatureDefinition>("StoneCrawlerCreature");Assert.That(stone.Actions.Camouflage,Is.True);Assert.That(D<CreatureDefinition>("CrystalCrawlerCreature").Actions.Camouflage,Is.False);Assert.That(D<CreatureDefinition>("CrystalCrawlerCreature").Habitat,Is.EqualTo(CreatureHabitat.CrystalPocket));foreach(var name in new[]{"Bat","Crow"})Assert.That(D<CreatureDefinition>(name+"Creature").Actions.Movement,Is.EqualTo(WildlifeMovement.Flight));Assert.That(D<CreatureDefinition>("MountainGoatCreature").Actions.Movement,Is.EqualTo(WildlifeMovement.Hop));}
        [Test]public void CreatureDropsAvoidUnrelatedGatheringAndPreserveNaturalFeatherSources()
        {
            foreach(var guid in AssetDatabase.FindAssets("t:LootTable",new[]{"Assets/Wildfolk/Data"}))
            {
                var table=AssetDatabase.LoadAssetAtPath<LootTable>(AssetDatabase.GUIDToAssetPath(guid));
                var entries=table.Items.Concat(table.RareItems).ToArray();
                Assert.That(entries.Any(x=>x.Item.Id=="plant_fiber"),Is.False,table.name+" is not a plant source");
                if(entries.Any(x=>x.Item.Id=="feather"))Assert.That(table.name,Is.EqualTo("ChickenLoot").Or.EqualTo("CrowLoot"));
            }
            foreach(var name in new[]{"ZombieLoot","GoblinLoot","SlingshotGoblinLoot","SkeletonLoot","SkeletonSwordLoot"})
                Assert.That(D<LootTable>(name).Items.All(x=>x.Item.Id!="wood" && x.Item.Id!="stone" && x.Item.Id!="iron_ore" && x.Item.Id!="crossbow_bolt"),Is.True,name);
            Assert.That(D<LootTable>("SkeletonLoot").Items.Any(x=>x.Item.Id=="bone" && x.Chance==1),Is.True);
            Assert.That(D<LootTable>("ChickenLoot").Items.Single(x=>x.Item.Id=="feather").Chance,Is.EqualTo(1));
            Assert.That(D<LootTable>("CrowLoot").Items.Single(x=>x.Item.Id=="feather").Chance,Is.LessThan(1));
        }
        [Test]public void MigrationPreservesOldLivingIdsAndDoesNotPretendToHaveRolledCampLoot()
        {var a=new AnimalSnapshot{Id="old",Species="deer",HP=9,GroupId="herd"};var c=new CampSnapshot{Id="oldcamp",RefillAt=42};var g=new SavedGame{Version=10,Animals=new[]{a},AnimalEcology=new AnimalEcologySnapshot{InitializedRegions=new[]{7}},GoblinEcology=new GoblinEcologySnapshot{Camps=new[]{c}}};SaveMigration.CreatureLootFromEarlier(g);Assert.That(g.Version,Is.EqualTo(11));Assert.That(g.Animals.Single(),Is.SameAs(a));Assert.That(a.HP,Is.EqualTo(9));Assert.That(g.AnimalEcology.InitializedRegions,Is.EqualTo(new[]{7}));Assert.That(g.AnimalEcology.ExpandedRegions,Is.Empty);Assert.That(c.LootInitialized,Is.False);Assert.That(c.Loot,Is.Empty);Assert.That(c.RefillAt,Is.EqualTo(42));}
        [Test]public void CampRollIsStableByPersistentIdAndNeverRefillsAnOpenedContainer()
        {var go=new GameObject("Camp roll test");try{var c=go.AddComponent<GoblinCamp>();c.State=new CampSnapshot{Id="campA"};c.InitializeLoot(null,C,42);var first=JsonUtility.ToJson(c.State);c.State=new CampSnapshot{Id="campA"};c.InitializeLoot(null,C,42);Assert.That(JsonUtility.ToJson(c.State),Is.EqualTo(first));Assert.That(c.LootId,Is.EqualTo("campA:supplies"));Assert.That(c.State.Loot.All(s=>s!=null),Is.True);c.State.Loot[0]=new SavedStack();c.State.LootOpened=true;c.State.LootRevision=2;var spent=JsonUtility.ToJson(c.State);c.InitializeLoot(null,C,42);Assert.That(JsonUtility.ToJson(c.State),Is.EqualTo(spent));Assert.That(c.State.Loot.Take(4).Skip(1).All(s=>s!=null && s.Count>0),Is.True);}finally{Object.DestroyImmediate(go);}}
        [Test]public void LootNetworkWritesBelongToServerAndRequestsContainNoClientRewards()
        {var camp=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkGoblinCamp.prefab").GetComponent<NetworkCamp>();Assert.That(camp.LootRevision.WritePerm,Is.EqualTo(NetworkVariableWritePermission.Server));Assert.That(camp.Opened.WritePerm,Is.EqualTo(NetworkVariableWritePermission.Server));var rpc=typeof(NetworkFacilityInteractor).GetMethod("CampLootRpc",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);Assert.That(rpc.GetParameters().Select(p=>p.ParameterType),Is.EqualTo(new[]{typeof(ulong),typeof(ulong),typeof(int),typeof(uint),typeof(uint),typeof(RpcParams)}));Assert.That(((RpcAttribute)rpc.GetCustomAttributes(typeof(RpcAttribute),false).Single()).InvokePermission,Is.EqualTo(RpcInvokePermission.Owner));}
        [Test]public void DataValidatorRejectsNonfiniteRecoveryWithoutChangingAuthoredAssets()
        {var food=C.Item("healing_salve").Consumable;var armor=C.Item("carapace_vest").Armor;var stamina=food.StaminaRestore;var bonus=armor.StaminaRecoveryBonus;try{food.StaminaRestore=float.NaN;armor.StaminaRecoveryBonus=-.1f;var errors=ValidateData();Assert.That(errors.Any(x=>x=="Invalid consumable recovery: "+food.name),Is.True);Assert.That(errors.Any(x=>x=="Invalid armor recovery: "+armor.name),Is.True);}finally{food.StaminaRestore=stamina;armor.StaminaRecoveryBonus=bonus;}Assert.That(ValidateData(),Is.Empty);}
    }
}
