using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class StabilityPlayTests
    {
        private WorldSaveService saves;private PlayerInventory player;private PlayerCombat combat;private GameplayHud hud;private PlayPointerDriver input;
        [UnitySetUp]public IEnumerator Load()
        {Time.timeScale=1;input=new PlayPointerDriver();WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;saves=Object.FindFirstObjectByType<WorldSaveService>();saves.AutoSaveEnabled=false;player=saves.Player;combat=player.GetComponent<PlayerCombat>();hud=Object.FindFirstObjectByType<GameplayHud>();saves.Clock.enabled=false;saves.GetComponent<ZombieSpawner>().enabled=false;saves.GetComponent<GoblinSpawner>().enabled=false;saves.World.Renewal.enabled=false;saves.World.Animals.enabled=false;}
        [UnityTearDown]public IEnumerator Release(){input.Dispose();Time.timeScale=1;yield return null;}
        private void Position(Vector2 p,Vector2 facing){player.GetComponent<Rigidbody2D>().position=p;player.transform.position=p;player.GetComponent<PlayerAvatar>().Facing.Face(facing);Physics2D.SyncTransforms();}
        private void ClearStart(){foreach(var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(node.transform.position.sqrMagnitude<144)node.gameObject.SetActive(false);}
        private void EmptyInventory(){var state=new SavedStack[player.Model.Capacity];for(var i=0;i<state.Length;i++)state[i]=new SavedStack();player.Model.Restore(state,saves.Catalog);player.SelectSlot(0);}
        private Health Target(Vector2 p,EntityRelation relation=EntityRelation.Hostile)
        {var go=new GameObject("Combat assertion target");go.SetActive(false);var health=go.AddComponent<Health>();health.Definition=player.GetComponent<Health>().Definition;go.AddComponent<CircleCollider2D>().radius=.5f;go.GetComponent<CircleCollider2D>().isTrigger=true;go.AddComponent<CombatAffiliation>().Relation=relation;go.transform.position=p;go.SetActive(true);return health;}
        private IEnumerator Strike(){Assert.That(combat.TryAttack(),Is.True);yield return new WaitForSeconds(.76f);}
        private int Find(ItemDefinition item){for(var i=0;i<player.Model.Capacity;i++)if(!player.Model.GetSlot(i).IsEmpty && player.Model.GetSlot(i).Item==item)return i;Assert.Fail(item.Id);return -1;}
        [UnityTest]public IEnumerator EveryBasicWeaponHitsExactlyOneForwardTarget()
        {
            ClearStart();Position(Vector2.zero,Vector2.right);var first=Target(new Vector2(1.2f,0));var second=Target(new Vector2(1.2f,.5f));var behind=Target(new Vector2(-.3f,0));
            yield return Strike();Assert.That(first.Current,Is.EqualTo(90));Assert.That(second.Current,Is.EqualTo(100));Assert.That(behind.Current,Is.EqualTo(100));
            foreach(var item in saves.Catalog.Items)if(item.Tool!=null)
            {EmptyInventory();player.Model.Add(item,1);player.SelectSlot(0);first.RestoreHealth(100);second.RestoreHealth(100);Position(Vector2.zero,Vector2.right);yield return null;yield return Strike();Assert.That(first.Current,Is.EqualTo(100-item.Tool.Damage),item.Id);Assert.That(second.Current,Is.EqualTo(100),item.Id);Assert.That(combat.LastHitCount,Is.EqualTo(1));}
        }
        [UnityTest]public IEnumerator PointBlankNormalEdgeAndOutsideRespectRange()
        {
            ClearStart();Position(Vector2.zero,Vector2.right);var target=Target(new Vector2(.6f,0));
            foreach(var distance in new[]{.6f,1.2f,2.09f,2.12f})
            {target.transform.position=new Vector2(distance,0);target.RestoreHealth(100);Physics2D.SyncTransforms();yield return Strike();Assert.That(target.Current,Is.EqualTo(distance<=2.1f ? 90 : 100),"Distance "+distance);}
        }
        [UnityTest]public IEnumerator FriendlyFilteringAndMultiTargetProfileDoNotBlockHostiles()
        {
            ClearStart();Position(Vector2.zero,Vector2.right);var friend=Target(new Vector2(.8f,0),EntityRelation.PlayerTeam);var structure=Target(new Vector2(1.1f,.2f),EntityRelation.TeamStructure);var first=Target(new Vector2(1.3f,0));var second=Target(new Vector2(1.3f,.45f),EntityRelation.NeutralGoblin);
            var child=new GameObject("Second hitbox");child.transform.SetParent(first.transform,false);child.AddComponent<CircleCollider2D>().isTrigger=true;
            var settings=Object.Instantiate(combat.Settings);settings.PunchAttack=new AttackProfile{Range=1.6f,ArcAngle=120,MaxTargets=2};combat.Settings=settings;
            yield return Strike();Assert.That(friend.Current,Is.EqualTo(100));Assert.That(structure.Current,Is.EqualTo(100));Assert.That(first.Current,Is.EqualTo(90));Assert.That(second.Current,Is.EqualTo(90));Assert.That(combat.LastHitCount,Is.EqualTo(2));Object.Destroy(settings);
        }
        [UnityTest]public IEnumerator CupCollectionRawPenaltyBoilingAndReturnedCupWorkTogether()
        {
            var source=Object.FindFirstObjectByType<WaterSource>();Position(source.transform.position,Vector2.up);var cup=source.Definition.Container;
            Assert.That(source.Collect(player),Is.False);Assert.That(player.Model.CountItem(cup.Raw),Is.Zero);
            player.Model.Add(cup.Empty,1);Assert.That(source.Collect(player),Is.True);Assert.That(player.Model.CountItem(cup.Empty),Is.Zero);
            player.SelectSlot(Find(cup.Raw));player.GetComponent<PlayerSurvival>().Model.SetMeters(90,50);Assert.That(player.GetComponent<PlayerSurvival>().TryConsume(),Is.True);Assert.That(player.Model.CountItem(cup.Empty),Is.EqualTo(1));Assert.That(player.GetComponent<PlayerSurvival>().Model.RecoveryBlockRemaining,Is.EqualTo(60));
            yield return new WaitForSeconds(1.1f);Assert.That(source.Collect(player),Is.True);Position(Vector2.zero,Vector2.down);ClearStart();
            var fire=Object.Instantiate(saves.Catalog.Item("campfire").Placement.Prefab,new Vector2(0,-2.1f),Quaternion.identity).GetComponent<CraftingStation>();yield return null;var crafting=player.GetComponent<PlayerCrafting>();Assert.That(crafting.OpenStation(fire),Is.True);Assert.That(crafting.Craft(saves.Catalog.Recipe("clean_water_cup")),Is.EqualTo(CraftStatus.Processing));
            yield return new WaitForSeconds(5.1f);Assert.That(crafting.Craft(saves.Catalog.Recipe("clean_water_cup")),Is.EqualTo(CraftStatus.Collect));hud.Session.CloseInventory();player.GetComponent<PlayerSurvival>().Model.SetMeters(90,50);player.SelectSlot(Find(cup.Clean));Assert.That(player.GetComponent<PlayerSurvival>().TryConsume(),Is.True);Assert.That(player.Model.CountItem(cup.Empty),Is.EqualTo(1));
        }
        [UnityTest]public IEnumerator SaveReloadRestoresPlayerContainersFacilitiesCargoAndResourceDeltas()
        {
            var tree=Object.FindFirstObjectByType<ResourceIdentity>();var treeId=tree.Id;tree.Node.Health.ApplyDamage(tree.Node.Health.Maximum);yield return null;
            Position(Vector2.zero,Vector2.down);player.Model.Add(saves.Catalog.Item("raw_water_cup"),1);player.Model.Add(saves.Catalog.Item("iron_ore"),5);player.Model.Add(saves.Catalog.Item("wood"),17);player.SelectSlot(Find(saves.Catalog.Item("raw_water_cup")));player.GetComponent<PlayerSurvival>().Model.SetMeters(80,50);Assert.That(player.GetComponent<PlayerSurvival>().TryConsume(),Is.True);player.GetComponent<Health>().RestoreHealth(73);player.GetComponent<PlayerProgression>().SetValues(123,17);
            var ids=new[]{"workbench","campfire","furnace","storage_chest","wood_wall","wood_door"};WorldPlaceable furnace=null,chest=null,door=null;
            for(var i=0;i<ids.Length;i++){var placed=Object.Instantiate(saves.Catalog.Item(ids[i]).Placement.Prefab,new Vector2((i%3-1)*2.2f,(i/3*2-1)*2.2f),Quaternion.identity);if(ids[i]=="furnace")furnace=placed;if(ids[i]=="storage_chest")chest=placed;if(ids[i]=="wood_door")door=placed;}
            chest.GetComponent<StorageChest>().Model.Add(saves.Catalog.Item("clean_water_cup"),1);chest.GetComponent<StorageChest>().Model.Add(saves.Catalog.Item("wood"),12);door.GetComponent<WorldDoor>().RestoreOpen(true);furnace.GetComponent<StationProcessing>().Begin(saves.Catalog.Recipe("iron_ingot"),player);yield return new WaitForSeconds(1.2f);
            var snapshot=saves.Capture();Assert.That(saves.Save(),Is.True,saves.LastError);player.GetComponent<PlayerProgression>().SetValues(0,0);player.Model.ConsumeSlot(Find(saves.Catalog.Item("wood")),saves.Catalog.Item("wood"),17);
            Assert.That(saves.Load(),Is.True);yield return null;yield return null;yield return null;saves=Object.FindFirstObjectByType<WorldSaveService>();saves.AutoSaveEnabled=false;player=saves.Player;
            Assert.That(player.GetComponent<Health>().Current,Is.EqualTo(73));Assert.That(player.GetComponent<PlayerProgression>().Experience,Is.EqualTo(123));Assert.That(player.GetComponent<PlayerProgression>().Gold,Is.EqualTo(17));Assert.That(player.Model.CountItem(saves.Catalog.Item("empty_cup")),Is.EqualTo(1));Assert.That(player.GetComponent<PlayerSurvival>().Model.RecoveryBlockRemaining,Is.GreaterThan(57));
            Assert.That(Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None).Length,Is.EqualTo(6));foreach(var identity in Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None))Assert.That(identity.Id,Is.Not.EqualTo(treeId));
            foreach(var placed in Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None)){if(placed.Definition.Item.Id=="storage_chest")Assert.That(placed.GetComponent<StorageChest>().Model.CountItem(saves.Catalog.Item("clean_water_cup")),Is.EqualTo(1));if(placed.Definition.IsDoor)Assert.That(placed.GetComponent<WorldDoor>().IsOpen,Is.True);if(placed.Definition.Station==StationKind.Furnace){Assert.That(placed.GetComponent<StationProcessing>().BatchCount,Is.EqualTo(1));Assert.That(placed.GetComponent<StationProcessing>().Remaining,Is.InRange(.2f,.9f));}}
        }
        [UnityTest]public IEnumerator RegionalRecoveryKeepsDestroyedIdentityAndExclusionsAcrossSave()
        {
            var ores=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Where(x=>saves.World.Definition.Resources[x.Rule].Rarity==ResourceRarity.Rare).Take(2).ToArray();var ids=ores.Select(x=>x.Id).ToArray();var positions=ores.Select(x=>(Vector2)x.transform.position).ToArray();foreach(var ore in ores)ore.Node.Health.ApplyDamage(ore.Node.Health.Maximum);yield return null;
            saves.Clock.Model.Advance(721);Assert.That(saves.World.Renewal.Tick(),Is.EqualTo(1));var states=saves.World.Renewal.Capture();Assert.That(states.Count(x=>x.Destroyed),Is.EqualTo(2));var recovered=states.Single(x=>x.Id.StartsWith("p"));foreach(var point in positions)Assert.That(Vector2.Distance(recovered.Position,point),Is.GreaterThanOrEqualTo(28));Assert.That(saves.GetComponent<WorldNavigation>().OutsideCamera(recovered.Position),Is.True);
            var snapshot=SaveRepository.Decode(SaveRepository.Encode(saves.Capture()));saves.Apply(snapshot);yield return null;Assert.That(saves.World.Renewal.Population.Harvests.Count,Is.EqualTo(2));foreach(var id in ids)Assert.That(Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Any(x=>x.Id==id),Is.False);Assert.That(Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Any(x=>x.Id==recovered.Id),Is.True);
        }
        [UnityTest]public IEnumerator AnimalRecoveryPersistsRegionalTimerAndCreatesNewIdentityOutsideCamera()
        {
            var population=saves.World.Animals;var chosen=Object.FindObjectsByType<AnimalIdentity>(FindObjectsSortMode.None).First();var id=chosen.State.Id;var rule=chosen.State.Rule;var region=chosen.State.Region;
            foreach(var animal in Object.FindObjectsByType<AnimalIdentity>(FindObjectsSortMode.None).Where(a=>a.State.Rule==rule && a.State.Region==region))animal.Animal.Health.ApplyDamage(animal.Animal.Health.Maximum);yield return null;
            Position(new Vector2(-75,-50),Vector2.right);Camera.main.GetComponent<CameraFollow>().enabled=false;Camera.main.transform.position=player.transform.position+Vector3.back*10;
            var before=saves.Capture();saves.Apply(SaveRepository.Decode(SaveRepository.Encode(before)));yield return null;Assert.That(population.Capture().Any(a=>a.Id==id),Is.False);Assert.That(population.Pending.Any(t=>t.Rule==rule && t.Region==region),Is.True);
            saves.Clock.Model.Advance(population.Definition.Rules[rule].RespawnSeconds+1);Assert.That(population.Tick(),Is.GreaterThan(0));Assert.That(population.Capture().Any(a=>a.Id==id),Is.False);
            var recovered=population.Capture().Where(a=>!before.Animals.Any(old=>old.Id==a.Id)).ToArray();Assert.That(recovered.Length,Is.GreaterThan(0));foreach(var animal in recovered)Assert.That(saves.GetComponent<WorldNavigation>().OutsideCamera(animal.Position),Is.True);
        }
        [UnityTest]public IEnumerator LongPressSplitHoldsChosenAmountUntilExplicitPlacementOrCancel()
        {
            ClearStart();player.Model.Add(saves.Catalog.Item("wood"),30);yield return input.Inventory(hud);var panel=Object.FindFirstObjectByType<InventoryPanel>();var drag=Object.FindFirstObjectByType<InventoryDragController>();
            var point=PlayPointerDriver.Point(panel.Slots[0]);input.Pointer(point,true);yield return new WaitForSecondsRealtime(.55f);input.Pointer(point,false);yield return null;Assert.That(drag.SplitPanel.Actions.activeSelf,Is.True);
            yield return input.Click(drag.SplitPanel.ActionSplitButton);Assert.That(drag.SplitPanel.Amount,Is.EqualTo(15));yield return input.Click(drag.SplitPanel.Plus);Assert.That(drag.SplitPanel.Amount,Is.EqualTo(16));yield return input.Click(drag.SplitPanel.ConfirmButton);
            Assert.That(drag.HeldAmount,Is.EqualTo(16));Assert.That(player.Model.GetSlot(0).Count,Is.EqualTo(30));Assert.That(player.Model.GetSlot(1).IsEmpty,Is.True);
            yield return input.Click(panel.Slots[9]);Assert.That(player.Model.GetSlot(0).Count,Is.EqualTo(14));Assert.That(player.Model.GetSlot(9).Count,Is.EqualTo(16));Assert.That(drag.IsDragging,Is.False);
            yield return input.Click(panel.Slots[9]);yield return input.Click(drag.SplitButton);yield return input.Click(drag.SplitPanel.ConfirmButton);yield return input.Click(drag.HeldCancel.transform);Assert.That(player.Model.CountItem(saves.Catalog.Item("wood")),Is.EqualTo(30));Assert.That(drag.IsDragging,Is.False);
        }
        [UnityTest]public IEnumerator PooledPickupsResetCountsAndReuseWithoutDuplicateRewards()
        {
            var first=PrefabPool.Spawn(saves.World.Pickup,new Vector2(10,0),Quaternion.identity);first.Initialize(saves.Catalog.Item("wood"),3,player,saves.World.Settings,true);var id=first.GetInstanceID();PrefabPool.Release(first.gameObject);
            var second=PrefabPool.Spawn(saves.World.Pickup,new Vector2(12,0),Quaternion.identity);Assert.That(second.GetInstanceID(),Is.EqualTo(id));Assert.That(second.Item,Is.Null);second.Initialize(saves.Catalog.Item("stone"),7,player,saves.World.Settings,true);yield return null;Assert.That(second.Count,Is.EqualTo(7));Assert.That(second.Item.Id,Is.EqualTo("stone"));Assert.That(player.Model.CountItem(saves.Catalog.Item("wood")),Is.Zero);
        }
        [UnityTest]public IEnumerator GoblinPersonalSpaceAndSoftSeparationWorkWithoutPlayerPush()
        {
            ClearStart();Position(Vector2.zero,Vector2.right);var spawner=saves.GetComponent<GoblinSpawner>();var goblin=PrefabPool.Spawn(spawner.Prefab,new Vector2(6,0),Quaternion.identity);goblin.Session=hud.Session;goblin.PlayerHealth=player.GetComponent<Health>();goblin.GetComponent<LootOnDeath>().Player=player;yield return null;
            Position(new Vector2(2,0),Vector2.right);yield return new WaitForSeconds(.2f);Assert.That(goblin.State,Is.EqualTo(GoblinState.Watch));
            goblin.GetComponent<Rigidbody2D>().position=new Vector2(10,0);goblin.transform.position=new Vector2(10,0);Position(new Vector2(11.3f,0),Vector2.left);yield return null;Assert.That(goblin.InCombat,Is.True);
            PrefabPool.Release(goblin.gameObject);Position(Vector2.zero,Vector2.right);var first=PrefabPool.Spawn(spawner.Prefab,new Vector2(6,0),Quaternion.identity);first.Session=hud.Session;first.PlayerHealth=player.GetComponent<Health>();var second=PrefabPool.Spawn(spawner.Prefab,new Vector2(6,0),Quaternion.identity);second.Session=hud.Session;second.PlayerHealth=player.GetComponent<Health>();yield return new WaitForSeconds(1);
            Assert.That(Vector2.Distance(first.transform.position,second.transform.position),Is.GreaterThan(.2f));Assert.That(((Vector2)player.transform.position).magnitude,Is.LessThan(.01f));
        }
        [UnityTest]public IEnumerator ShallowTerrainModifierUsesDeviceIndependentMoveCommands()
        {
            ClearStart();var adapter=player.GetComponent<PlayerInputAdapter>();adapter.Source=new MoveSource();Position(Vector2.zero,Vector2.right);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();var land=player.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
            var ford=new Vector2(saves.World.Map.RiverAt(saves.World.Map.FordY),saves.World.Map.FordY);Position(ford,Vector2.right);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.That(player.GetComponent<Rigidbody2D>().linearVelocity.magnitude,Is.EqualTo(land*.7f).Within(.02f));Assert.That(saves.GetComponent<WorldNavigation>().Surface(ford),Is.EqualTo(TerrainSurface.Shallow));Assert.That(saves.GetComponent<WorldNavigation>().Traversable(saves.World.Map.LakeCenter,.5f),Is.False);
        }
        private sealed class MoveSource:IPlayerInputSource{public Vector2 Move=>Vector2.right;public bool Primary=>false;public bool Interact=>false;public bool Inventory=>false;public bool Pause=>false;}
        [UnityTest]public IEnumerator RemoteIdleZombiesReturnSlotsAfterGraceAndThreatMovesToNewRegion()
        {
            ClearStart();var spawner=saves.GetComponent<ZombieSpawner>();var definition=Object.Instantiate(spawner.Definition);definition.LostTargetSeconds=.2f;definition.InactiveSeconds=.2f;definition.InitialFraction=0;definition.RefillInterval=100;spawner.Definition=definition;saves.Clock.Model.SetElapsed(saves.Clock.Definition.DaySeconds+saves.Clock.Definition.EveningSeconds+1);
            Assert.That(spawner.TrySpawn(),Is.True);yield return new WaitForSeconds(1.35f);Assert.That(spawner.ActivePursuerCount,Is.EqualTo(1));Position(new Vector2(-75,-40),Vector2.right);Camera.main.GetComponent<CameraFollow>().enabled=false;Camera.main.transform.position=player.transform.position+Vector3.back*10;spawner.enabled=true;yield return new WaitForSeconds(.1f);Assert.That(spawner.ActivePursuerCount,Is.EqualTo(1));
            yield return new WaitForSeconds(1.6f);spawner.enabled=false;Assert.That(spawner.ActiveCount,Is.Zero);Assert.That(player.GetComponent<PlayerProgression>().Gold,Is.Zero);Assert.That(spawner.TrySpawn(),Is.True);Assert.That(spawner.LocalThreatCount,Is.EqualTo(1));Assert.That(spawner.ActiveCount,Is.LessThanOrEqualTo(definition.HardCap));Object.Destroy(definition);
        }
    }
}
