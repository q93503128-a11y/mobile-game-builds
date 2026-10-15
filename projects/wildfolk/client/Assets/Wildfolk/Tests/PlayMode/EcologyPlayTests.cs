using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class EcologyPlayTests
    {
        private WorldSaveService save;private PlayerInventory player;private PlayPointerDriver input;
        [UnitySetUp]public IEnumerator Setup()
        {
            Time.timeScale=1;WorldGenerator.SelectedSeed=9472026;input=new PlayPointerDriver();yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;player=save.Player;save.Clock.enabled=false;save.GetComponent<ZombieSpawner>().enabled=false;save.GetComponent<GoblinSpawner>().enabled=false;save.World.Animals.enabled=false;player.GetComponent<PlayerSurvival>().enabled=false;
            foreach(var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(node.transform.position.sqrMagnitude<144)node.gameObject.SetActive(false);
            foreach(var animal in Object.FindObjectsByType<NeutralAnimal>(FindObjectsSortMode.None))animal.enabled=false;
            player.GetComponent<Rigidbody2D>().position=Vector2.zero;player.transform.position=Vector2.zero;yield return new WaitForFixedUpdate();
        }
        [UnityTearDown]public IEnumerator Cleanup(){input.Dispose();Time.timeScale=1;yield return null;}
        private Health Target(Vector2 p,float radius=.4f)
        {var root=new GameObject("Melee fixture");root.SetActive(false);root.transform.position=p;var health=root.AddComponent<Health>();health.Definition=save.GetComponent<ZombieSpawner>().Prefab.OwnHealth.Definition;root.AddComponent<CircleCollider2D>().radius=radius;root.AddComponent<CombatAffiliation>().Relation=EntityRelation.Hostile;root.SetActive(true);return health;}
        [UnityTest]public IEnumerator PointBlankAndFacingMatrixForFistAxePickaxeSword()
        {
            var combat=player.GetComponent<PlayerCombat>();var equipment=player.GetComponent<PlayerEquipment>();var facing=player.GetComponent<PlayerAvatar>().Facing;
            foreach(var id in new[]{"","axe","pickaxe","stone_sword"})
            {
                if(id!=""){var item=save.Catalog.Item(id);player.Model.Add(item,1);for(var i=0;i<36;i++)if(player.Model.GetSlot(i).Item==item){player.SelectSlot(i);break;}}else player.SelectSlot(8);yield return null;
                var reach=equipment.ActiveTool==null ? combat.Settings.PunchAttack.Range : equipment.ActiveTool.Attack.Range;
                var normal=reach*.75f+.4f;var positions=new[]{Vector2.up*.95f,Vector2.right*.95f,Vector2.down*.95f,Vector2.up*normal,Vector2.right*normal,Vector2.up*(reach+.399f),Vector2.up*(reach+.43f)};
                for(var c=0;c<positions.Length;c++)
                {
                    var target=Target(positions[c]);var before=target.Current;facing.Face(Vector2.up);Physics2D.SyncTransforms();yield return new WaitForSeconds(.65f);Assert.That(combat.TryAttack(),Is.True,id+" case "+c);yield return new WaitForSeconds(.4f);
                    Assert.That(target.Current<before,Is.EqualTo(c!=4 && c!=6),id+" case "+c);Assert.That(combat.LastHitCount,Is.LessThanOrEqualTo(1));Object.Destroy(target.gameObject);yield return null;
                }
            }
        }
        [UnityTest]public IEnumerator ActualPointerArmorDragSwapUnequipAndUse()
        {
            var panel=Object.FindFirstObjectByType<InventoryPanel>();var equipment=player.GetComponent<PlayerEquipment>();var ui=Object.FindFirstObjectByType<InventoryEquipment>();var leather=save.Catalog.Item("leather_head");var iron=save.Catalog.Item("iron_head");player.Model.Add(leather,1);player.Model.Add(iron,1);save.World.Session.OpenInventory();yield return null;yield return null;
            yield return input.Drag(panel.Slots[0],PlayPointerDriver.Point(ui.HeadButton));Assert.That(equipment.HeadArmor,Is.SameAs(leather));
            yield return input.Drag(panel.Slots[1],PlayPointerDriver.Point(ui.HeadButton));Assert.That(equipment.HeadArmor,Is.SameAs(iron));Assert.That(player.Model.CountItem(leather),Is.EqualTo(1));
            yield return input.Drag(ui.HeadButton,PlayPointerDriver.Point(panel.Slots[9]));Assert.That(equipment.HeadArmor,Is.Null);Assert.That(player.Model.GetSlot(9).Item,Is.SameAs(iron));
            save.World.Session.CloseInventory();player.SelectSlot(9);yield return null;Assert.That(player.GetComponent<PlayerInteraction>().TryInteract(),Is.True);Assert.That(equipment.HeadArmor,Is.SameAs(iron));
            var body=save.Catalog.Item("leather_body");player.Model.Add(body,1);for(var i=0;i<36;i++)if(player.Model.GetSlot(i).Item==body){player.SelectSlot(i);break;}yield return null;Assert.That(player.GetComponent<PlayerPrimaryAction>().TryUse(),Is.True);Assert.That(equipment.BodyArmor,Is.SameAs(body));
            player.Model.Add(save.Catalog.Item("wood"),1800);var before=player.Model.Capture();Assert.That(equipment.Unequip(ArmorSlot.Head),Is.False);Assert.That(equipment.HeadArmor,Is.SameAs(iron));Assert.That(JsonUtility.ToJson(new StackFixture{Slots=player.Model.Capture()}),Is.EqualTo(JsonUtility.ToJson(new StackFixture{Slots=before})));
        }
        [System.Serializable]private sealed class StackFixture{public SavedStack[] Slots;}
        [UnityTest]public IEnumerator ModalLayerIsCenteredAndBlocksBagAndGameplay()
        {
            var layers=Object.FindFirstObjectByType<GameplayUiLayers>();save.World.Session.OpenCharacter();yield return null;yield return null;var screen=Object.FindFirstObjectByType<CharacterScreen>();var layout=screen.View.GetComponentInChildren<ModalLayout>();Assert.That(((RectTransform)layout.transform).anchoredPosition,Is.EqualTo(Vector2.zero));Assert.That(layers.Gameplay.blocksRaycasts,Is.False);
            var bag=Object.FindFirstObjectByType<GameplayHud>().InventoryButton;yield return input.Click(bag);Assert.That(save.World.Session.State,Is.EqualTo(SessionState.Character));screen.OpenTree();yield return null;Assert.That(Object.FindFirstObjectByType<SkillTreeScreen>().View.activeSelf,Is.True);Assert.That(layers.Gameplay.interactable,Is.False);
        }
        [UnityTest]public IEnumerator DoorHingeEasesAndColliderMatchesPassageAndSave()
        {
            var door=Object.Instantiate(save.Catalog.Item("wood_door").Placement.Prefab,new Vector2(3,0),Quaternion.identity).GetComponent<WorldDoor>();yield return null;var hinge=door.Hinge.position;var frame=door.Placeable.Visual.transform.position;
            Assert.That(door.Toggle(),Is.True);Assert.That(door.OpenAmount,Is.Zero);Assert.That(door.Barrier.enabled,Is.True);yield return new WaitForSeconds(.14f);Assert.That(door.OpenAmount,Is.InRange(.1f,.9f));Assert.That(door.Hinge.position,Is.EqualTo(hinge));Assert.That(door.Placeable.Visual.transform.position,Is.EqualTo(frame));yield return new WaitForSeconds(.25f);Assert.That(door.Barrier.enabled,Is.False);
            var navigation=save.GetComponent<WorldNavigation>();Assert.That(navigation.CanMove(new Vector2(3,-1),Vector2.up,2,.25f),Is.True);var snapshot=save.Capture();save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));yield return null;door=Object.FindFirstObjectByType<WorldDoor>();Assert.That(door.IsOpen,Is.True);Assert.That(door.Barrier.enabled,Is.False);Assert.That(door.Toggle(),Is.True);yield return new WaitForSeconds(.4f);Assert.That(door.Barrier.enabled,Is.True);Assert.That(navigation.CanMove(new Vector2(3,-1),Vector2.up,2,.25f),Is.False);
        }
        [UnityTest]public IEnumerator AnimalsSeparateSoftlyAndHerdAlertStaysLocal()
        {
            var rule=save.World.Animals.Definition.Rules[3];var a=Object.Instantiate(rule.Prefab,Vector2.zero,Quaternion.identity);var b=Object.Instantiate(rule.Prefab,Vector2.zero,Quaternion.identity);
            foreach(var animal in new[]{a,b}){animal.Player=player.transform;animal.Session=save.World.Session;animal.Region=save.World.Region;animal.Member.GroupId="test_herd";animal.Member.Anchor=Vector2.zero;animal.Definition=Object.Instantiate(animal.Definition);animal.Definition.DangerRange=.01f;}
            yield return new WaitForSeconds(.8f);Assert.That(Vector2.Distance(a.transform.position,b.transform.position),Is.GreaterThan(.4f));Assert.That(Vector2.Distance(a.transform.position,player.transform.position),Is.GreaterThan(.3f));Assert.That(player.transform.position.sqrMagnitude,Is.LessThan(.01f));
            a.transform.position=new Vector2(3,0);b.transform.position=new Vector2(4,0);a.FleeFrom(player.transform.position);Assert.That(b.IsFleeing,Is.True);
            var far=Object.Instantiate(rule.Prefab,new Vector2(30,0),Quaternion.identity);far.Player=player.transform;far.Session=save.World.Session;far.Region=save.World.Region;far.Member.GroupId="test_herd";a.Member.Broadcast(player.transform.position,CreatureAlert.Flee);Assert.That(far.IsFleeing,Is.False);
        }
        [UnityTest]public IEnumerator RegionalAnimalRecoveryUsesNewIdsAndRoundTripsGroups()
        {
            var population=save.World.Animals;var initial=population.Capture();Assert.That(initial.Length,Is.GreaterThan(15));Assert.That(initial.Select(a=>a.Species).Distinct().Count(),Is.EqualTo(5));
            var chosen=Object.FindObjectsByType<AnimalIdentity>(FindObjectsSortMode.None).First(a=>a.State.Rule==0);var id=chosen.State.Id;var region=chosen.State.Region;var basePosition=(Vector2)chosen.transform.position;Object.Instantiate(save.Catalog.Item("wood_wall").Placement.Prefab,basePosition,Quaternion.identity);foreach(var actor in Object.FindObjectsByType<AnimalIdentity>(FindObjectsSortMode.None).Where(a=>a.State.Rule==0 && a.State.Region==region))actor.Animal.Health.ApplyDamage(actor.Animal.Health.Maximum);yield return null;Assert.That(population.Capture().Any(a=>a.Id==id),Is.False);Assert.That(population.Pending.Any(t=>t.Rule==0),Is.True);
            var before=save.Capture();save.Apply(SaveRepository.Decode(SaveRepository.Encode(before)));yield return null;Assert.That(population.Capture().Select(a=>a.Id),Is.EquivalentTo(before.Animals.Select(a=>a.Id)));save.Clock.Model.SetElapsed(save.Clock.Model.Elapsed+500);var count=population.Tick();Assert.That(count,Is.GreaterThan(0));Assert.That(population.Capture().Any(a=>a.Id==id),Is.False);Assert.That(population.ActiveCount,Is.LessThanOrEqualTo(population.Definition.HardCap));foreach(var animal in population.Capture().Where(a=>!before.Animals.Any(old=>old.Id==a.Id)))Assert.That(Vector2.Distance(animal.Position,basePosition),Is.GreaterThanOrEqualTo(population.Definition.BaseDistance));save.Validate(save.Capture());
        }
        [UnityTest]public IEnumerator WarbandAssistIsBoundedAndRangedCountVaries()
        {
            var spawner=save.GetComponent<GoblinSpawner>();Random.InitState(715);var observed=new System.Collections.Generic.HashSet<int>();
            for(var i=0;i<6;i++){var actors=spawner.SpawnWarband(new Vector2(6,0),false);Assert.That(actors.Length,Is.InRange(3,5));var ranged=actors.Count(a=>a.Ranged!=null);Assert.That(ranged,Is.InRange(0,2));observed.Add(ranged);foreach(var g in actors)PrefabPool.Release(g.gameObject);yield return null;}
            Assert.That(observed.Count,Is.GreaterThan(1));var near=spawner.SpawnActor(false,new Vector2(4,0),"one",new Vector2(4,0));var ally=spawner.SpawnActor(false,new Vector2(6,0),"one",new Vector2(6,0));var far=spawner.SpawnActor(false,new Vector2(22,0),"two",new Vector2(22,0));near.EnterCombat(player.transform.position);Assert.That(ally.InCombat,Is.True);Assert.That(far.InCombat,Is.False);
        }
        [UnityTest]public IEnumerator SlingshotDrawReleaseProjectileDamageAndBoundedReposition()
        {
            var goblin=save.GetComponent<GoblinSpawner>().SpawnActor(true,new Vector2(0,4),"shot",new Vector2(0,4));goblin.enabled=false;var shot=goblin.Ranged;yield return null;var hp=player.GetComponent<Health>();var before=hp.Current;var rest=shot.Facing.LeftHand.localPosition;Assert.That(shot.Begin(hp),Is.True);yield return new WaitForSeconds(.03f);Assert.That(Vector3.Distance(rest,shot.Facing.LeftHand.localPosition),Is.LessThan(.4f));yield return new WaitForSeconds(.37f);Assert.That(shot.Shots,Is.Zero);Assert.That(shot.DrawAmount(shot.Elapsed),Is.GreaterThan(.5f));var drawnLength=Vector3.Distance(shot.Band.GetPosition(0),shot.Band.GetPosition(1));
            Assert.That(Vector3.Distance(shot.Facing.LeftHand.position,shot.PouchStone.transform.position),Is.LessThan(.12f));yield return new WaitForSeconds(.31f);Assert.That(shot.Shots,Is.EqualTo(1));Assert.That(shot.DrawAmount(shot.Elapsed),Is.LessThan(.5f));Assert.That(Vector3.Distance(shot.Band.GetPosition(0),shot.Band.GetPosition(1)),Is.LessThan(drawnLength));yield return new WaitForSeconds(.19f);Assert.That(shot.Facing.LeftHand.localPosition.x,Is.LessThan(.35f));yield return new WaitForSeconds(.31f);Assert.That(hp.Current,Is.LessThan(before));
            goblin.enabled=true;goblin.transform.position=new Vector2(0,1.8f);goblin.GetComponent<Rigidbody2D>().position=goblin.transform.position;goblin.EnterCombat(player.transform.position);var start=goblin.transform.position;yield return new WaitForSeconds(2.2f);Assert.That(Vector2.Distance(start,goblin.transform.position),Is.LessThan(4));Assert.That(shot.Shots,Is.GreaterThanOrEqualTo(2));
        }
        [UnityTest]public IEnumerator SkeletonUsesNightMeleeVariantAndIndependentLightTrait()
        {
            var spawner=save.GetComponent<ZombieSpawner>();save.Clock.Model.SetElapsed(save.Clock.Definition.DaySeconds+save.Clock.Definition.EveningSeconds+1);var skeleton=Object.Instantiate(spawner.SecondaryPrefab,new Vector2(0,1.3f),Quaternion.identity);skeleton.Session=save.World.Session;skeleton.Clock=save.Clock;skeleton.PlayerHealth=player.GetComponent<Health>();skeleton.Lifecycle.Clock=save.Clock;skeleton.GetComponent<LootOnDeath>().Player=player;skeleton.Variant.SetEquipped(true);
            Assert.That(skeleton.Member.Definition.Family,Is.EqualTo(CreatureFamily.Undead));Assert.That(skeleton.Member.Definition.Has(CreatureTrait.LightSensitive),Is.True);Assert.That(skeleton.Variant.Weapon.enabled,Is.True);var before=skeleton.PlayerHealth.Current;yield return new WaitForSeconds(spawner.Definition.EmergeSeconds+.85f);Assert.That(skeleton.PlayerHealth.Current,Is.LessThan(before));
            var saved=save.Capture();save.Apply(SaveRepository.Decode(SaveRepository.Encode(saved)));yield return null;skeleton=Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Single(e=>e.Member.Definition.Id=="skeleton");Assert.That(skeleton.Variant.Equipped,Is.True);Assert.That(skeleton.OwnHealth.Current,Is.EqualTo(saved.NightCreatures.Single().HP));
            var sword=save.Catalog.Item("rusted_sword");player.Model.Add(sword,1);player.SelectSlot(0);yield return null;var target=Target(new Vector2(1.7f,0));player.GetComponent<PlayerAvatar>().Facing.Face(Vector2.right);Physics2D.SyncTransforms();Assert.That(player.GetComponent<PlayerPrimaryAction>().TryUse(),Is.True);yield return new WaitForSeconds(.35f);Assert.That(target.Current,Is.LessThan(target.Maximum));Object.Destroy(target.gameObject);
            skeleton.transform.position=new Vector2(0,5);skeleton.GetComponent<Rigidbody2D>().position=skeleton.transform.position;Object.Instantiate(save.Catalog.Item("campfire").Placement.Prefab,Vector2.zero,Quaternion.identity);yield return null;skeleton.CancelPursuit();yield return new WaitForSeconds(.15f);Assert.That(skeleton.State,Is.EqualTo(EnemyState.Idle));var copy=Object.Instantiate(skeleton.Member.Definition);copy.Traits&=~CreatureTrait.LightSensitive;skeleton.Member.Definition=copy;yield return new WaitForSeconds(.15f);Assert.That(skeleton.State,Is.EqualTo(EnemyState.Chase));
        }
        [UnityTest]public IEnumerator CampPopulationRefillsNewResidentsAndPersists()
        {
            var spawner=save.GetComponent<GoblinSpawner>();Assert.That(spawner.Camps.Count,Is.EqualTo(1));var camp=spawner.Camps[0];var old=spawner.Capture();Assert.That(old.Actors.Count(a=>a.CampId==camp.State.Id),Is.GreaterThan(0));foreach(var actor in Object.FindObjectsByType<GoblinIdentity>(FindObjectsSortMode.None))if(actor.State.CampId==camp.State.Id)actor.Actor.OwnHealth.ApplyDamage(actor.Actor.OwnHealth.Maximum);yield return null;var killed=old.Actors.Select(a=>a.Id).ToArray();Assert.That(camp.State.RefillAt,Is.GreaterThan(save.Clock.Model.Elapsed));save.Clock.Model.SetElapsed(camp.State.RefillAt+1);Assert.That(spawner.TickCamps(),Is.GreaterThan(0));Assert.That(spawner.Capture().Actors.All(a=>!killed.Contains(a.Id)),Is.True);var snapshot=save.Capture();save.Apply(SaveRepository.Decode(SaveRepository.Encode(snapshot)));yield return null;Assert.That(spawner.Capture().Actors.Select(a=>a.Id),Is.EquivalentTo(snapshot.GoblinEcology.Actors.Select(a=>a.Id)));
        }
        [UnityTest]public IEnumerator V3LoadPreservesOldAnimalsEquipmentDoorAndWorld()
        {
            var armor=save.Catalog.Item("iron_head");player.Model.Add(armor,1);player.GetComponent<PlayerEquipment>().TryEquip(armor);var door=Object.Instantiate(save.Catalog.Item("wood_door").Placement.Prefab,new Vector2(3,0),Quaternion.identity).GetComponent<WorldDoor>();yield return null;door.RestoreOpen(true);
            var old=save.Capture();old.Version=3;old.AnimalEcology=null;old.GoblinEcology=null;old.Animals=new[]{new AnimalSnapshot{Id="a0_0",Rule=0,HP=10,Position=new Vector2(-12,-5),Home=new Vector2(-12,-5)},new AnimalSnapshot{Id="a1_0",Rule=1,HP=20,Position=new Vector2(14,-6),Home=new Vector2(14,-6)}};save.Apply(SaveRepository.Decode(SaveRepository.Encode(old)));yield return null;
            Assert.That(save.Capture().Version,Is.EqualTo(4));Assert.That(save.World.Seed,Is.EqualTo(old.Seed));Assert.That(player.GetComponent<PlayerEquipment>().HeadArmor,Is.SameAs(armor));Assert.That(save.World.Animals.Capture().Single(a=>a.Id=="a0_0").HP,Is.EqualTo(10));Assert.That(Object.FindFirstObjectByType<WorldDoor>().IsOpen,Is.True);Assert.That(save.GetComponent<GoblinSpawner>().Camps.Count,Is.EqualTo(1));save.Validate(save.Capture());
        }
        [UnityTest]public IEnumerator CombatSandboxDoesNotCreateDetachedBrownBodyRenderers()
        {
            var harness=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Wildfolk.Editor.DeveloperHarness")).First(t=>t!=null);harness.GetMethod("OpenCombatSandbox").Invoke(null,null);yield return null;
            Assert.That(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Any(r=>r.name=="Combat Sandbox Target"),Is.False);var equipment=player.GetComponent<PlayerEquipment>();foreach(var renderer in new[]{equipment.HeadVisual,equipment.ArmorVisual,equipment.LeftHandGear,equipment.RightHandGear})Assert.That(renderer.transform.IsChildOf(player.transform),Is.True);
        }
    }
}
