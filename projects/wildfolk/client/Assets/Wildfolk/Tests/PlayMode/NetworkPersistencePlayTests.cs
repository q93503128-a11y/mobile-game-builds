using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Wildfolk.Tests
{
    public sealed class NetworkPersistencePlayTests
    {
        private NetworkSession session;private NetworkPlayer player,other;private NetworkCreatureWorld world;private SaveRepository repository;
        private NetworkInventory Bag(NetworkPlayer p)=>p.GetComponent<NetworkInventory>();
        private NetworkConstruction Build(NetworkPlayer p)=>p.GetComponent<NetworkConstruction>();
        [UnitySetUp]public IEnumerator Host()
        {
            Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;Object.FindFirstObjectByType<HomeScreen>().OpenMultiplayer();session=NetworkSession.Instance;
            repository=new SaveRepository(Path.Combine(Application.temporaryCachePath,"lowfire-world-"+Guid.NewGuid().ToString("N"),"world.json"));session.NewWorldGenerationVersion=1;session.WorldRepository=repository;session.ProfileOverride=Guid.NewGuid().ToString("N");yield return Enter();
        }
        private IEnumerator Enter()
        {
            var task=session.Host(ConnectionRoute.LocalNetwork);for(var i=0;i<300 && !task.IsCompleted;i++)yield return null;Assert.That(task.IsCompleted && task.Result,Is.True,session.LastFailure);Assert.That(session.EnterGame(),Is.True);
            player=null;var deadline=Time.realtimeSinceStartup+10;while(Time.realtimeSinceStartup<deadline){yield return null;player=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject);if(player!=null)break;}Assert.That(player,Is.Not.Null,"state="+session.State+", world="+(NetworkCreatureWorld.Active!=null)+", ready="+(NetworkCreatureWorld.Active!=null && NetworkCreatureWorld.Active.Ready)+", clients="+session.Manager.ConnectedClients.Count);world=NetworkCreatureWorld.Active;Freeze();yield return null;
        }
        private void Freeze()
        {
            world.Clock.enabled=world.Animals.enabled=world.Goblins.enabled=world.Night.enabled=world.World.Renewal.enabled=false;
            foreach(var actor in NetworkPlayer.Active){actor.GetComponent<PlayerInputAdapter>().enabled=false;actor.GetComponent<NetworkInventory>().enabled=false;actor.Motor.enabled=false;actor.GetComponent<PlayerInteraction>().enabled=false;actor.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;}
            foreach(var process in Object.FindObjectsByType<StationProcessing>(FindObjectsSortMode.None))process.enabled=false;
            foreach(var animal in Object.FindObjectsByType<NeutralAnimal>(FindObjectsSortMode.None))animal.enabled=false;foreach(var goblin in Object.FindObjectsByType<GoblinController>(FindObjectsSortMode.None))goblin.enabled=false;foreach(var enemy in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))enemy.enabled=false;
        }
        private void Position(NetworkPlayer actor,Vector2 p){actor.GetComponent<Rigidbody2D>().position=p;actor.transform.position=p;actor.Motor.Avatar.Facing.Face(Vector2.up);Physics2D.SyncTransforms();}
        private Vector2 Spot()
        {
            Physics2D.SyncTransforms();
            for(var x=-65;x<65;x+=4)for(var y=-45;y<45;y+=4){var p=new Vector2(x,y);if(world.World.Map.IsWalkable(p,3) && !world.World.Map.IsWater(p,3) && Physics2D.OverlapBox(p,new Vector2(7,7),0)==null)return p;}
            throw new InvalidOperationException("No clear fixture location.");
        }
        private void Select(NetworkPlayer actor,ItemDefinition item,int count=1){actor.Commands.Session.CloseInventory();Bag(actor).Inventory.Model.Add(item,count);var slot=Enumerable.Range(0,36).First(i=>!Bag(actor).Inventory.Model.GetSlot(i).IsEmpty && Bag(actor).Inventory.Model.GetSlot(i).Item==item);Bag(actor).Inventory.SelectSlot(slot);}
        private NetworkPlayer Other(Vector2 p){var actor=Object.Instantiate(session.PlayerPrefab,p,Quaternion.identity).GetComponent<NetworkPlayer>();actor.NetworkObject.Spawn();other=actor;Freeze();return actor;}
        private NetworkFacility Facility(StationKind kind)=>Object.FindObjectsByType<NetworkFacility>(FindObjectsSortMode.None).Single(f=>f.Station!=null && f.Station.Definition.Station==kind);
        private NetworkFacilityInteractor Open(NetworkFacility f){player.Commands.Session.CloseInventory();Position(player,(Vector2)f.transform.position+Vector2.down*2);var actor=player.GetComponent<NetworkFacilityInteractor>();Assert.That(actor.Open(f),Is.True);return actor;}
        [UnityTest]public IEnumerator PlacementRaceRollbackAndEveryCurrentBuildingUseExistingDefinitions()
        {
            var spot=Spot();other=Other(spot+new Vector2(.2f,-2.1f));Position(player,spot+Vector2.down*2.1f);var wood=world.Catalog.Item("wood_wall");Select(player,wood,2);Select(other,wood,2);
            var revision=Bag(player).Revision.Value;Assert.That(Build(player).ExecutePlace(revision,Bag(player).Inventory.SelectedIndex,spot),Is.True);Assert.That(Build(other).ExecutePlace(Bag(other).Revision.Value,Bag(other).Inventory.SelectedIndex,spot),Is.False);Assert.That(Build(player).ExecutePlace(revision,Bag(player).Inventory.SelectedIndex,spot),Is.False);Assert.That(Bag(player).Inventory.Model.CountItem(wood),Is.EqualTo(1));Assert.That(Bag(other).Inventory.Model.CountItem(wood),Is.EqualTo(2));
            var first=Object.FindObjectsByType<NetworkBuilding>(FindObjectsSortMode.None).Single(b=>b.Placeable.Definition.Item==wood);Assert.That(first.State.Value.Creator.ToString(),Is.EqualTo(session.LocalProfile));
            Assert.That(Build(player).ExecutePlace(Bag(player).Revision.Value,Bag(player).Inventory.SelectedIndex,spot+Vector2.right*40),Is.False);
            var entries=world.Prefabs;var clear=Spot();Position(player,clear+Vector2.down*2.1f);world.Prefabs=entries.Where(e=>e.Source!=wood.Placement.Prefab.gameObject).ToArray();LogAssert.Expect(LogType.Error,"Building transaction failed: InvalidOperationException");Assert.That(Build(player).ExecutePlace(Bag(player).Revision.Value,Bag(player).Inventory.SelectedIndex,clear),Is.False);world.Prefabs=entries;Assert.That(Bag(player).Inventory.Model.CountItem(wood),Is.EqualTo(1));
            foreach(var item in world.Catalog.Items.Where(i=>i.Placement!=null && i!=wood))
            {clear=Spot();Position(player,clear+Vector2.down*2.1f);Select(player,item);yield return null;var preview=player.GetComponent<PlayerPlacement>();Assert.That(preview.Preview.activeSelf && preview.PreviewValid,Is.True,item.Id);Assert.That(Object.FindFirstObjectByType<NetworkInventoryUi>().AttackIcon.sprite,Is.SameAs(item.Icon));Assert.That(player.Commands.Attack(),Is.True,item.Id);Assert.That(Bag(player).Inventory.Model.CountItem(item),Is.Zero,item.Id);}
            Assert.That(Object.FindObjectsByType<NetworkBuilding>(FindObjectsSortMode.None).All(b=>b.IsSpawned),Is.True);yield return null;
        }
        [UnityTest]public IEnumerator WallAdjacencyAndDoorDesiredStateRejectCompetingToggle()
        {
            var spot=Spot();Position(player,spot+Vector2.down*2.1f);var wall=world.Catalog.Item("wood_wall");Select(player,wall,2);Assert.That(Build(player).ExecutePlace(Bag(player).Revision.Value,Bag(player).Inventory.SelectedIndex,spot),Is.True);var first=Object.FindObjectsByType<WallVisual>(FindObjectsSortMode.None).Single();
            Position(player,spot+new Vector2(1.9f,-2.1f));Assert.That(Build(player).ExecutePlace(Bag(player).Revision.Value,Bag(player).Inventory.SelectedIndex,spot+Vector2.right*1.9f),Is.True);var second=Object.FindObjectsByType<WallVisual>(FindObjectsSortMode.None).Single(w=>w!=first);Assert.That(first.RightEnd.activeSelf,Is.False);Assert.That(second.LeftEnd.activeSelf,Is.False);
            var death=0;second.Placeable.Health.Died+=()=>death++;Assert.That(second.Placeable.Health.ApplyDamage(second.Placeable.Health.Maximum),Is.GreaterThan(0));Assert.That(second.Placeable.Health.ApplyDamage(1),Is.Zero);Assert.That(death,Is.EqualTo(1));yield return null;Assert.That(first.RightEnd.activeSelf,Is.True);
            spot=Spot();Position(player,spot+Vector2.down*2.1f);Select(player,world.Catalog.Item("wood_door"));Assert.That(Build(player).ExecutePlace(Bag(player).Revision.Value,Bag(player).Inventory.SelectedIndex,spot),Is.True);var door=Object.FindFirstObjectByType<WorldDoor>();var network=door.GetComponent<NetworkBuilding>();other=Other(spot+new Vector2(.2f,-2.1f));var revision=network.Revision.Value;
            Assert.That(Build(player).ExecuteDoor(network.NetworkObjectId,revision,true),Is.True);Assert.That(Build(other).ExecuteDoor(network.NetworkObjectId,revision,true),Is.False);yield return new WaitForSeconds(.4f);Assert.That(door.IsOpen,Is.True);Assert.That(door.Barrier.enabled,Is.False);Assert.That(door.OpenAmount,Is.EqualTo(1));
            Position(other,spot);Assert.That(Build(player).ExecuteDoor(network.NetworkObjectId,network.Revision.Value,false),Is.False);Position(other,spot+Vector2.down*2.1f);Assert.That(Build(player).ExecuteDoor(network.NetworkObjectId,network.Revision.Value,false),Is.True);yield return new WaitForSeconds(.4f);Assert.That(door.Barrier.enabled,Is.True);Assert.That(network.State.Value.Open,Is.False);
        }
        [UnityTest]public IEnumerator HostSaveRehostRestoresWorldCargoGroundEcologyAndPlayerWithoutDuplicates()
        {
            world.Clock.Model.SetElapsed(world.Clock.Definition.DaySeconds+world.Clock.Definition.EveningSeconds+10);var ecology=world.Animals.CaptureEcology();foreach(var ticket in ecology.Pending)ticket.Due=10000;world.Animals.Restore(world.Animals.Capture(),ecology);world.FlushPending();Freeze();
            var savedDoor=world.Create(world.Catalog.Item("wood_door").Placement.Prefab,Spot(),Quaternion.Euler(0,0,90));savedDoor.GetComponent<WorldDoor>().RestoreOpen(true);var doorId=savedDoor.SaveId;
            var savedWall=world.Create(world.Catalog.Item("stone_wall").Placement.Prefab,Spot(),Quaternion.Euler(0,0,90));var wallId=savedWall.SaveId;
            world.Night.Restore(new[]{new NightCreatureSnapshot{Species=world.Night.SecondaryPrefab.Member.Definition.Id,Variant=world.Night.SecondaryPrefab.Variant.Definition.Id,Position=Spot(),HP=world.Night.SecondaryPrefab.OwnHealth.Maximum}});world.Goblins.Camps.First().State.RefillAt=1234;world.FlushPending();Freeze();
            var chest=Object.FindObjectsByType<NetworkFacility>(FindObjectsSortMode.None).First(f=>f.Chest!=null);chest.Chest.Model.Add(world.Catalog.Item("wood"),7);
            var bench=Facility(StationKind.Workbench);Open(bench);foreach(var cost in bench.Station.Definition.UpgradeCosts)Bag(player).Inventory.Model.Add(cost.Item,cost.Count);Assert.That(player.GetComponent<PlayerCrafting>().UpgradeWorkbench(),Is.True);
            var furnace=Facility(StationKind.Furnace);Open(furnace);var iron=world.Catalog.Recipe("iron_ingot");Bag(player).Inventory.Model.Add(iron.Ingredients[0].Item,3);Assert.That(furnace.Processor.AddAll(iron,Bag(player).Inventory),Is.GreaterThan(0));furnace.Processor.Advance(2.5f);
            var advanced=world.Create(world.Catalog.Item("blast_furnace").Placement.Prefab,Spot(),Quaternion.identity).GetComponent<NetworkFacility>();advanced.Processor.enabled=false;world.FlushPending();Open(advanced);var steel=world.Catalog.Recipe("steel");foreach(var cost in steel.Ingredients)Bag(player).Inventory.Model.Add(cost.Item,cost.Count);Assert.That(advanced.Processor.Add(steel,Bag(player).Inventory,1),Is.True);Bag(player).Inventory.Model.Add(world.Catalog.Item("bronze"),3);Bag(player).Inventory.Model.Add(world.Catalog.Item("tin_ore"),2);
            var fire=Facility(StationKind.Campfire);Open(fire);var meat=world.Catalog.Recipe("cooked_meat");Bag(player).Inventory.Model.Add(meat.Ingredients[0].Item,1);Assert.That(fire.Processor.Begin(meat,Bag(player).Inventory),Is.True);fire.Processor.Advance(1);
            var head=world.Catalog.Items.First(i=>i.Armor!=null && i.Armor.Slot==ArmorSlot.Head);Bag(player).Inventory.Model.Add(head,1);Assert.That(player.Equipment.TryEquip(head),Is.True);player.GetComponent<PlayerProgression>().SetValues(123,9);player.GetComponent<PlayerSurvival>().Model.SetMeters(77,62);player.Health.ApplyDamage(7);Bag(player).Inventory.Model.Add(world.Catalog.Item("stone"),11);
            var resource=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).First(r=>r.Rule==5);resource.Node.Health.ApplyDamage(1);var id=resource.Id;var hp=resource.Node.Health.Current;
            var destroyed=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).First(r=>r.Rule==6);destroyed.Node.Health.ApplyDamage(destroyed.Node.Health.Maximum);var destroyedId=destroyed.Id;var ground=NetworkGroundItem.Drop(world.Catalog.Item("wood"),4,Spot(),null,true);yield return null;yield return null;Freeze();
            player.Commands.Session.CloseInventory();Position(player,world.World.Map.Spawn);var snapshot=NetworkWorldPersistence.Active.Capture();var encoded=SaveRepository.Encode(snapshot);var savedId=snapshot.Multiplayer.WorldId;var worldCount=snapshot.Placeables.Length;Assert.That(SaveRepository.Decode(encoded).Version,Is.EqualTo(14));Assert.That(session.TrySaveWorld(),Is.True);
            Assert.That(snapshot.Placeables.Any(p=>p.Item=="workbench_ii"),Is.True);Assert.That(snapshot.Pickups.Any(p=>p.Item=="wood" && p.Count==4),Is.True);var resourceCount=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Length;
            session.Leave();for(var i=0;i<300 && (session.State!=NetworkSessionState.Idle || Object.FindFirstObjectByType<HomeScreen>()==null);i++)yield return null;Assert.That(session.State,Is.EqualTo(NetworkSessionState.Idle));yield return new WaitForSeconds(1);yield return Enter();
            Assert.That(NetworkWorldPersistence.Active.WorldId,Is.EqualTo(savedId));Assert.That(world.World.Seed,Is.EqualTo(snapshot.Seed));Assert.That(world.Clock.Model.Elapsed,Is.InRange(snapshot.Elapsed,snapshot.Elapsed+1));Assert.That(Object.FindObjectsByType<NetworkBuilding>(FindObjectsSortMode.None).Length,Is.EqualTo(worldCount));Assert.That(Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None).Select(p=>p.SaveId).Distinct().Count(),Is.EqualTo(worldCount));
            var identities=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None);Assert.That(identities.Length,Is.EqualTo(resourceCount));Assert.That(identities.Select(r=>r.Id).Distinct().Count(),Is.EqualTo(resourceCount));Assert.That(identities.Single(r=>r.Id==id).Node.Health.Current,Is.EqualTo(hp));Assert.That(identities.Any(r=>r.Id==destroyedId),Is.False);
            Assert.That(Facility(StationKind.Workbench).Station.Definition.WorkbenchTier,Is.EqualTo(2));var loaded=Facility(StationKind.Furnace).Processor;Assert.That(loaded.OutputCount,Is.EqualTo(1));Assert.That(loaded.Queue.WaitingCount,Is.EqualTo(1));Assert.That(loaded.Queue.CountWaiting(iron),Is.EqualTo(1));Assert.That(Facility(StationKind.BlastFurnace).Processor.Recipe.Id,Is.EqualTo("steel"));Assert.That(loaded.Remaining,Is.InRange(.5f,1.5f));Assert.That(Facility(StationKind.Campfire).Processor.Remaining,Is.InRange(3,4));
            Assert.That(Object.FindObjectsByType<StorageChest>(FindObjectsSortMode.None).Sum(c=>c.Model.CountItem(world.Catalog.Item("wood"))),Is.EqualTo(7));Assert.That(Bag(player).Inventory.Model.CountItem(world.Catalog.Item("stone")),Is.EqualTo(11));Assert.That(player.Equipment.HeadArmor.Id,Is.EqualTo(head.Id));Assert.That(player.GetComponent<PlayerProgression>().Gold,Is.EqualTo(9));Assert.That(player.GetComponent<PlayerProgression>().Experience,Is.EqualTo(123));Assert.That(player.GetComponent<PlayerSurvival>().Model.Thirst,Is.EqualTo(62));Assert.That(player.Health.Current,Is.EqualTo(snapshot.Player.HP));
            var recaptured=NetworkWorldPersistence.Active.Capture();Assert.That(recaptured.Animals.Select(a=>a.Id).OrderBy(x=>x),Is.EqualTo(snapshot.Animals.Select(a=>a.Id).OrderBy(x=>x)));Assert.That(recaptured.GoblinEcology.Camps.Select(c=>c.Id),Is.EquivalentTo(snapshot.GoblinEcology.Camps.Select(c=>c.Id)));Assert.That(recaptured.GoblinEcology.Camps.Select(c=>c.RefillAt),Is.EquivalentTo(snapshot.GoblinEcology.Camps.Select(c=>c.RefillAt)));Assert.That(recaptured.Recovery.Pending.Length,Is.EqualTo(snapshot.Recovery.Pending.Length));Assert.That(NetworkPlayer.Active.Count(p=>p.NetworkObject.IsPlayerObject),Is.EqualTo(1));
            Assert.That(recaptured.Placeables.Single(p=>p.Item=="blast_furnace").Processing.Recipe=="steel",Is.True);Assert.That(Bag(player).Inventory.Model.CountItem(world.Catalog.Item("bronze")),Is.EqualTo(3));Assert.That(Bag(player).Inventory.Model.CountItem(world.Catalog.Item("tin_ore")),Is.EqualTo(2));
            Assert.That(recaptured.Pickups.Select(p=>p.Item+":"+p.Count),Is.EquivalentTo(snapshot.Pickups.Select(p=>p.Item+":"+p.Count)));Assert.That(recaptured.NightCreatures.Length,Is.EqualTo(1));Assert.That(recaptured.NightCreatures[0].Variant,Is.EqualTo(snapshot.NightCreatures[0].Variant));Assert.That(recaptured.Animals.Select(a=>a.GroupId),Is.EquivalentTo(snapshot.Animals.Select(a=>a.GroupId)));Assert.That(recaptured.Placeables.Single(p=>p.Id==doorId).DoorOpen,Is.True);Assert.That(recaptured.Placeables.Single(p=>p.Id==wallId).Rotation,Is.EqualTo(90).Within(.01));Assert.That(recaptured.GoblinEcology.Camps.Any(c=>c.RefillAt==1234),Is.True);
        }
        [UnityTest]public IEnumerator RetireAndRejoinSameIdentityRestoreOnceAndRejectConcurrentIdentity()
        {
            var profile=player.ProfileId;Bag(player).Inventory.Model.Add(world.Catalog.Item("wood"),13);player.GetComponent<PlayerProgression>().SetValues(20,5);Assert.That(session.CanApprove(profile,91),Is.False);Assert.That(session.TryRegisterParticipant(profile,91),Is.False);
            var old=player;var oldId=player.NetworkObjectId;session.RetireParticipant(0);Assert.That(old.IsSpawned,Is.False);Assert.That(session.Manager.SpawnManager.SpawnedObjects.ContainsKey(oldId),Is.False);Assert.That(session.TryRegisterParticipant(profile,0),Is.True);session.SpawnParticipant(0);session.SpawnParticipant(0);yield return null;
            player=NetworkPlayer.Active.Single(p=>p.NetworkObject.IsPlayerObject);Freeze();Assert.That(player.ProfileId,Is.EqualTo(profile));Assert.That(Bag(player).Inventory.Model.CountItem(world.Catalog.Item("wood")),Is.EqualTo(13));Assert.That(player.GetComponent<PlayerProgression>().Gold,Is.EqualTo(5));Assert.That(player.NetworkObjectId,Is.Not.EqualTo(oldId));Assert.That(NetworkPlayer.Active.Count(p=>p.NetworkObject.IsPlayerObject),Is.EqualTo(1));
        }
        [UnityTest]public IEnumerator SaveFailureKeepsHostSessionAndCorruptLoadCannotCreateReplacementWorld()
        {
            session.NewWorldGenerationVersion=1;session.WorldRepository=new SaveRepository(Path.Combine(repository.Path,"child.json"));Directory.CreateDirectory(Path.GetDirectoryName(repository.Path));File.WriteAllText(repository.Path,"a file blocks the save directory");LogAssert.Expect(LogType.Error,new Regex("Cooperative world save failed:"));session.Leave();yield return null;Assert.That(session.State,Is.EqualTo(NetworkSessionState.Playing));Assert.That(session.Manager.IsListening,Is.True);Assert.That(session.MessageKey,Is.EqualTo("net.save_failed"));session.NewWorldGenerationVersion=1;session.WorldRepository=repository;
            Assert.That(session.TrySaveWorld(),Is.True);session.Leave();for(var i=0;i<300 && (session.State!=NetworkSessionState.Idle || Object.FindFirstObjectByType<HomeScreen>()==null);i++)yield return null;
            File.WriteAllText(repository.Path,"corrupt");if(File.Exists(repository.Path+".backup"))File.WriteAllText(repository.Path+".backup","corrupt");var task=session.Host(ConnectionRoute.LocalNetwork);for(var i=0;i<300 && !task.IsCompleted;i++)yield return null;Assert.That(task.Result,Is.False);Assert.That(session.State,Is.EqualTo(NetworkSessionState.Idle));Assert.That(session.Manager.IsListening,Is.False);Assert.That(session.MessageKey,Is.EqualTo("net.load_failed"));Assert.That(File.ReadAllText(repository.Path),Is.EqualTo("corrupt"));
        }
        [UnityTearDown]public IEnumerator Cleanup(){if(session!=null && session.State!=NetworkSessionState.Idle){session.NewWorldGenerationVersion=1;session.WorldRepository=repository;session.Leave(true,true);for(var i=0;i<300 && session.State!=NetworkSessionState.Idle;i++)yield return null;}Time.timeScale=1;}
    }
}
