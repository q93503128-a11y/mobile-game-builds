using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Wildfolk.Tests
{
    public sealed class CreatureLootPlayTests
    {
        private WorldSaveService s;private PlayerInventory bag;private PlayerInputCommands input;
        [UnitySetUp]public IEnumerator Enter()
        {Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;s=Object.FindFirstObjectByType<WorldSaveService>();s.AutoSaveEnabled=false;bag=s.Player;input=bag.GetComponent<PlayerInputCommands>();s.Clock.enabled=s.World.Animals.enabled=false;s.World.GetComponent<GoblinSpawner>().enabled=s.World.GetComponent<ZombieSpawner>().enabled=false;bag.GetComponent<PlayerMotor>().enabled=bag.GetComponent<PlayerInputAdapter>().enabled=false;s.World.Chunks.DrainPending();s.World.Animals.Restore(Array.Empty<AnimalSnapshot>(),new AnimalEcologySnapshot());foreach(var r in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(r.transform.position,Vector2.zero)<20)r.gameObject.SetActive(false);Move(Vector2.zero);Empty();Physics2D.SyncTransforms();}
        private void Empty()=>bag.Model.Restore(new InventoryModel(bag.Model.Capacity).Capture(),s.Catalog);
        private void Move(Vector2 p){input.Session.CloseInventory();bag.transform.position=p;bag.GetComponent<Rigidbody2D>().position=p;Physics2D.SyncTransforms();s.World.Chunks.Reconcile();s.World.Chunks.DrainPending();}
        private void Select(string id,int count=1){bag.Model.Add(s.Catalog.Item(id),count);bag.SelectSlot(Enumerable.Range(0,bag.Model.Capacity).First(i=>bag.Model.GetSlot(i).Item==s.Catalog.Item(id)));}
        private WildlifeActionBrain Actor(string id,Vector2 p,string group="qa")
        {var rule=s.World.Animals.Definition.Rules.Single(r=>r.Prefab.Member.Definition.Id==id);var a=Object.Instantiate(rule.Prefab,p,Quaternion.identity);a.Player=bag.transform;a.Session=input.Session;a.Member.Anchor=p;a.Member.GroupId=group;a.Separation.Player=bag.transform;var brain=a.GetComponent<WildlifeActionBrain>();brain.Bind(s.World.GetComponent<WorldNavigation>());return brain;}
        private GoblinCamp Camp(){var spawner=s.World.GetComponent<GoblinSpawner>();return spawner.SpawnCamp(new Vector2(-2,0),"qa-camp",0,false);}
        [UnityTest]public IEnumerator SalveHealsAtFullHungerAndCooldownAndDeathPreventConsumption()
        {var health=bag.GetComponent<Health>();health.ApplyDamage(40);var hp=health.Current;Select("healing_salve",3);Assert.That(bag.GetComponent<PlayerSurvival>().TryConsume(),Is.True);Assert.That(health.Current,Is.EqualTo(hp+25));Assert.That(bag.Model.CountItem(s.Catalog.Item("healing_salve")),Is.EqualTo(2));Assert.That(bag.GetComponent<PlayerSurvival>().TryConsume(),Is.False);health.ApplyDamage(10000,true);Assert.That(bag.GetComponent<PlayerSurvival>().TryConsume(),Is.False);Assert.That(bag.Model.CountItem(s.Catalog.Item("healing_salve")),Is.EqualTo(2));yield return null;}
        [UnityTest]public IEnumerator RestorativeFoodRestoresStaminaAndVestIsAnEquipmentTradeoff()
        {var stamina=bag.GetComponent<PlayerLocomotion>().Stamina;stamina.Restore(0,1);Select("restorative_broth");Assert.That(bag.GetComponent<PlayerSurvival>().TryConsume(),Is.True);Assert.That(stamina.Current,Is.EqualTo(35));Select("carapace_vest");var stats=bag.GetComponent<CharacterStats>();var before=stats.RecoveryMultiplier;Assert.That(bag.GetComponent<PlayerEquipment>().TryEquip(s.Catalog.Item("carapace_vest")),Is.True);Assert.That(stats.RecoveryMultiplier,Is.EqualTo(before+.15f).Within(.001f));yield return null;}
        [UnityTest]public IEnumerator CampClaimsAreAtomicRangeCheckedAndPersistRemainingLootAcrossSleepAndLoad()
        {var camp=Camp();Move(camp.Location.position+Vector3.down);Assert.That(camp.Take(bag,-1,0),Is.True);var revision=camp.State.LootRevision;var count=camp.State.Loot[0].Count;Assert.That(camp.Take(bag,0,revision),Is.True);Assert.That(camp.Take(bag,0,revision),Is.False);Assert.That(bag.Model.CountItem(s.Catalog.Item("bone_arrow")),Is.EqualTo(count));var remaining=JsonUtility.ToJson(camp.State);Move(new Vector2(900,900));yield return null;s.Rules.Validate(s.Capture());Assert.That(s.Save(),Is.True,s.LastError);Move(new Vector2(0,1));yield return null;camp=s.World.GetComponent<GoblinSpawner>().Camps.Single(c=>c.State.Id=="qa-camp");Assert.That(JsonUtility.ToJson(camp.State),Is.EqualTo(remaining));var game=SaveRepository.Decode(SaveRepository.Encode(s.Capture()));s.Rules.Validate(game);s.Apply(game);yield return null;s.World.Chunks.DrainPending();camp=s.World.GetComponent<GoblinSpawner>().Camps.Single(c=>c.State.Id=="qa-camp");Assert.That(camp.State.Loot[0]==null || camp.State.Loot[0].Count==0,Is.True);Assert.That(camp.State.LootOpened,Is.True);Assert.That(camp.State.LootRevision,Is.EqualTo(revision+1));Assert.That(bag.Model.CountItem(s.Catalog.Item("bone_arrow")),Is.EqualTo(count));Move(new Vector2(40,40));Assert.That(camp.Take(bag,1,camp.State.LootRevision),Is.False);}
        [UnityTest]public IEnumerator FullInventoryAndLivingCampResidentRejectClaimsWithoutChangingEitherSide()
        {var camp=Camp();Move(camp.Location.position+Vector3.down);for(var i=0;i<bag.Model.Capacity;i++)bag.Model.Add(s.Catalog.Item("wood"),50);var original=JsonUtility.ToJson(camp.State);Assert.That(camp.Take(bag,1,0),Is.False);Assert.That(JsonUtility.ToJson(camp.State),Is.EqualTo(original));Empty();var guard=s.World.GetComponent<GoblinSpawner>().SpawnActor(false,new Vector2(5,0),"qa-camp",Vector2.zero,"qa-camp");Assert.That(camp.Secured,Is.False);Assert.That(camp.Take(bag,1,0),Is.False);guard.OwnHealth.ApplyDamage(999,true);Assert.That(camp.Secured,Is.True);Assert.That(camp.Take(bag,1,0),Is.True);yield return null;}
        [UnityTest]public IEnumerator ActualLootModalHasSafeLayoutLocalizedTakeActionsAndClosesWithoutDeposit()
        {var camp=Camp();Move(camp.Location.position+Vector3.down);Assert.That(camp.Use(bag.GetComponent<PlayerInteraction>()),Is.True);yield return null;var panel=Object.FindFirstObjectByType<CampLootPanel>();Assert.That(panel,Is.Not.Null);var fit=panel.GetComponentInChildren<ModalLayout>();Assert.That(fit.SafeArea,Is.Not.Null);Assert.That(((RectTransform)fit.transform).anchoredPosition,Is.EqualTo(Vector2.zero));var revision=camp.State.LootRevision;Assert.That(panel.Take(0),Is.True);Assert.That(camp.State.LootRevision,Is.EqualTo(revision+1));L.SetLanguage("ko");Assert.That(panel.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.text==L.Text("ui.camp.take")),Is.True);Assert.That(CampLootPanel.CloseFocused(),Is.True);Assert.That(input.Session.State,Is.EqualTo(SessionState.Running));yield return null;}
        [UnityTest]public IEnumerator StoneDisguiseWarnsBeforeUnfoldingAndBeforeImpact()
        {var stone=Actor("stone_crawler",new Vector2(1.9f,0));var hp=bag.GetComponent<Health>().Current;yield return new WaitForSeconds(.2f);Assert.That(stone.Phase,Is.EqualTo(WildlifePhase.Warning));Assert.That(stone.Body.sprite,Is.SameAs(stone.RestSprite));Assert.That(stone.Tell.enabled,Is.True);Assert.That(stone.LeftAppendage.gameObject.activeSelf,Is.False);yield return new WaitForSeconds(.9f);Assert.That(stone.Awakened,Is.True);Assert.That(stone.Body.sprite,Is.SameAs(stone.ActiveSprite));Assert.That(stone.LeftAppendage.gameObject.activeSelf,Is.True);Assert.That(bag.GetComponent<Health>().Current,Is.EqualTo(hp));Object.Destroy(stone.gameObject);}
        [UnityTest]public IEnumerator PackAlertJoinsOnlySameNearbyGroupWithoutRelayChains()
        {var first=Actor("wolf",new Vector2(3,0),"packA");var ally=Actor("wolf",new Vector2(8,0),"packA");var other=Actor("wolf",new Vector2(8,2),"packB");yield return new WaitForSeconds(.25f);Assert.That(first.Phase,Is.Not.EqualTo(WildlifePhase.Idle));Assert.That(ally.Phase,Is.Not.EqualTo(WildlifePhase.Idle));Assert.That(other.Phase,Is.EqualTo(WildlifePhase.Idle));Object.Destroy(first.gameObject);Object.Destroy(ally.gameObject);Object.Destroy(other.gameObject);}
        [UnityTest]public IEnumerator ChargeStopsAtSolidObstacleAndFlightRetreatsWithoutInstantDamage()
        {var wall=new GameObject("Charge obstacle");wall.transform.position=new Vector2(1.4f,0);wall.AddComponent<BoxCollider2D>().size=new Vector2(.25f,8);var boar=Actor("giant_boar",new Vector2(3,0));Physics2D.SyncTransforms();var hp=bag.GetComponent<Health>().Current;yield return new WaitForSeconds(2.3f);Assert.That(boar.Phase,Is.EqualTo(WildlifePhase.Recovery));Assert.That(boar.transform.position.x,Is.GreaterThan(1.4f));Assert.That(bag.GetComponent<Health>().Current,Is.EqualTo(hp));Object.Destroy(wall);Object.Destroy(boar.gameObject);yield return null;var bat=Actor("bat",new Vector2(4,0));var start=bat.transform.position;yield return new WaitForSeconds(.2f);Assert.That(bat.Pose.localPosition.y,Is.GreaterThan(.2f));Assert.That(bat.Impacts,Is.Zero);yield return new WaitForSeconds(1.2f);Assert.That(Vector2.Distance(start,bat.transform.position),Is.GreaterThan(1));Assert.That(bat.Phase,Is.EqualTo(WildlifePhase.Recovery));Object.Destroy(bat.gameObject);}
        [UnityTest]public IEnumerator AllHostileActionsDamageSolidPlayerOncePerActionAndPulseDeduplicatesColliders()
        {
            var health=bag.GetComponent<Health>();bag.GetComponent<PlayerSurvival>().enabled=false;
            var extra=new GameObject("Additional player hurtbox");extra.transform.SetParent(bag.transform,false);extra.AddComponent<CircleCollider2D>().isTrigger=true;
            foreach(var id in new[]{"bat","stone_crawler","bog_leech","wolf","giant_boar","crystal_crawler"})
            {
                Move(Vector2.zero);health.RestoreHealth(health.Maximum);var hp=health.Current;
                var actor=Actor(id,new Vector2(id=="stone_crawler" ? 1.6f : id=="giant_boar" ? 3 : 2,0));
                var until=Time.time+5;while(actor.Impacts==0 && Time.time<until)yield return null;
                Assert.That(actor.Impacts,Is.EqualTo(1),id+" must land an actual hit");
                Assert.That(health.Current,Is.EqualTo(hp-actor.Definition.Damage),id+" damage and duplicate collider protection");
                while(actor.Phase==WildlifePhase.Action && Time.time<until)yield return null;
                Assert.That(actor.Impacts,Is.EqualTo(1),id+" hits once per action");
                Object.Destroy(actor.gameObject);yield return null;
            }
            Object.Destroy(extra);
        }
        [UnityTest]public IEnumerator PulseCannotDamageThroughSolidWall()
        {
            var wall=new GameObject("Pulse obstacle");wall.transform.position=new Vector2(1,0);wall.AddComponent<BoxCollider2D>().size=new Vector2(.25f,8);
            var actor=Actor("crystal_crawler",new Vector2(2,0));Physics2D.SyncTransforms();var hp=bag.GetComponent<Health>().Current;
            yield return new WaitForSeconds(3);Assert.That(actor.Impacts,Is.Zero);Assert.That(bag.GetComponent<Health>().Current,Is.EqualTo(hp));
            Object.Destroy(wall);Object.Destroy(actor.gameObject);
        }
        [UnityTest]public IEnumerator Save10MigrationAndCorruptCampStackValidationUseRealWorldSnapshot()
        {var camp=Camp();var game=s.Capture();game.Version=10;var copy=SaveRepository.Decode(SaveRepository.Encode(game));s.Rules.Validate(copy);Assert.That(copy.Version,Is.EqualTo(11));Assert.That(copy.GoblinEcology.Camps.Single(c=>c.Id=="qa-camp").LootInitialized,Is.False);var corrupt=s.Capture();corrupt.GoblinEcology.Camps.Single(c=>c.Id=="qa-camp").Loot[0].Count=1000;Assert.Throws<ArgumentException>(()=>s.Rules.Validate(corrupt));yield return null;}
        [UnityTearDown]public IEnumerator Exit(){CampLootPanel.CloseFocused();foreach(var a in Object.FindObjectsByType<WildlifeActionBrain>(FindObjectsSortMode.None))Object.Destroy(a.gameObject);Time.timeScale=1;yield return null;}
    }
}
