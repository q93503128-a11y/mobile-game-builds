using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class TierThreePlayTests
    {
        private WorldSaveService save;private PlayerInventory player;private PlayerInputCommands commands;
        [UnitySetUp]public IEnumerator Enter()
        {
            Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;
            save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;player=save.Player;commands=player.GetComponent<PlayerInputCommands>();save.Clock.enabled=save.World.Animals.enabled=false;save.World.GetComponent<ZombieSpawner>().enabled=save.World.GetComponent<GoblinSpawner>().enabled=false;player.GetComponent<PlayerMotor>().enabled=player.GetComponent<PlayerInputAdapter>().enabled=false;save.World.Chunks.DrainPending();Clear(Vector2.zero,18);Move(Vector2.zero);commands.SetMove(Vector2.zero);player.Model.Restore(new InventoryModel(player.Model.Capacity).Capture(),save.Catalog);
        }
        private void Clear(Vector2 center,float radius){foreach(var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(node.transform.position,center)<radius)node.gameObject.SetActive(false);Physics2D.SyncTransforms();}
        private WorldPlaceable Place(string id,Vector2 position)=>Object.Instantiate(save.Catalog.Item(id).Placement.Prefab,position,Quaternion.identity);
        private void Give(string id,int amount=1)=>player.Model.Add(save.Catalog.Item(id),amount);
        private int Slot(string id)=>Enumerable.Range(0,player.Model.Capacity).First(i=>player.Model.GetSlot(i).Item==save.Catalog.Item(id));
        private void Move(Vector2 p){player.transform.position=p;player.GetComponent<Rigidbody2D>().position=p;save.World.Chunks.Reconcile();save.World.Chunks.DrainPending();}
        private void Module(WorldTurret turret,string id){Give(id);Assert.That(turret.Execute(player,TurretOperation.Install,Slot(id)),Is.True);}
        private Health Enemy(Vector2 p,EntityRelation relation=EntityRelation.Hostile)
        {var go=new GameObject("Isolated turret target");go.SetActive(false);var h=go.AddComponent<Health>();h.Definition=ScriptableObject.CreateInstance<VitalityDefinition>();h.Definition.MaximumHealth=1000;go.AddComponent<CircleCollider2D>().isTrigger=true;go.AddComponent<CombatAffiliation>().Relation=relation;go.transform.position=p;go.SetActive(true);return h;}
        [UnityTest]public IEnumerator WorkbenchUpgradeConsumesMaterialsAndDiamondUsesSteelIngredient()
        {
            var bench=Place("workbench_ii",new Vector2(2,0)).GetComponent<CraftingStation>();var crafting=player.GetComponent<PlayerCrafting>();Assert.That(crafting.OpenStation(bench),Is.True,"II station must open in range");Assert.That(crafting.UpgradeWorkbench(),Is.False);
            foreach(var cost in bench.Definition.UpgradeCosts)player.Model.Add(cost.Item,cost.Count);Assert.That(crafting.UpgradeWorkbench(),Is.True,"All six upgrade materials should be available: "+string.Join(",",bench.Definition.UpgradeCosts.Select(c=>c.Item.Id+":"+player.Model.CountItem(c.Item)+"/"+c.Count)));yield return null;
            var third=WorldPlaceable.Active.Single(p=>p.Definition.Station==StationKind.Workbench && p.Definition.WorkbenchTier==3);Assert.That(crafting.Station,Is.SameAs(third.GetComponent<CraftingStation>()),"Upgrade keeps the already open UI bound to the new station");var r=save.Catalog.Recipe("diamond_pickaxe");foreach(var c in r.Ingredients)player.Model.Add(c.Item,c.Count);Assert.That(crafting.Craft(r),Is.EqualTo(CraftStatus.Ready));Assert.That(player.Model.CountItem(save.Catalog.Item("steel_pickaxe")),Is.Zero);Assert.That(player.Model.CountItem(r.Result),Is.EqualTo(1));
            var panel=Object.FindFirstObjectByType<CraftingPanel>();Assert.That(panel.Cards.Any(c=>c.Recipe.Id=="telescope"),Is.True);commands.Session.CloseInventory();
        }
        [UnityTest]public IEnumerator ScopeKeepsSimulationChunksUnchangedAndPresentationExpires()
        {
            Give("telescope");player.SelectSlot(Slot("telescope"));var scope=player.GetComponent<PlayerTelescope>();var count=save.World.Chunks.WantedCount;Assert.That(scope.Use(),Is.True);commands.SetMove(Vector2.right);yield return new WaitForSeconds(.5f);Assert.That(commands.Move,Is.EqualTo(Vector2.zero));Assert.That(commands.Interact(),Is.False);Assert.That(commands.Dash(),Is.False);Assert.That(save.World.Chunks.WantedCount,Is.EqualTo(count));Assert.That(save.World.Chunks.PresentationOnlyCount,Is.GreaterThan(0));
            player.GetComponent<Health>().ApplyDamage(1);Assert.That(scope.IsViewing,Is.False);Assert.That(save.World.Chunks.PresentationOnlyCount,Is.GreaterThan(0));yield return new WaitForSecondsRealtime(3.2f);Assert.That(save.World.Chunks.PresentationOnlyCount,Is.Zero);
        }
        [UnityTest]public IEnumerator BeaconsTrackOnlyOneAndSnapshotRestoresNavigationAndUtility()
        {
            Give("compass");var a=Place("signal_beacon",new Vector2(2,0));var b=Place("signal_beacon",new Vector2(-2,0));var nav=player.GetComponent<PlayerNavigation>();Assert.That(nav.Mode,Is.EqualTo(NavigationMode.Automatic));Assert.That(nav.BeaconId,Is.Null.Or.Empty);nav.SelectNavigation(NavigationMode.Beacon,b.SaveId);Assert.That(nav.TryBeacon(out var p,out _),Is.True);Assert.That(p,Is.EqualTo((Vector2)b.transform.position));
            Give("crystal_lantern");Assert.That(player.GetComponent<PlayerEquipment>().TryEquip(save.Catalog.Item("crystal_lantern")),Is.True);var game=save.Capture();save.Rules.Validate(game);Assert.That(game.Player.NavigationMode,Is.EqualTo(NavigationMode.Beacon));Assert.That(game.Player.Utility,Is.EqualTo("crystal_lantern"));nav.SelectNavigation(NavigationMode.Off);Assert.That(nav.Mode,Is.EqualTo(NavigationMode.Off));save.Apply(game);yield return null;save.World.Chunks.DrainPending();Assert.That(nav.BeaconId,Is.EqualTo(b.SaveId));Assert.That(BeaconDirectory.Points.Count,Is.EqualTo(2));
        }
        [UnityTest]public IEnumerator ManualTurretRequiresRealCompatibleAmmoReloadAndKeepsModules()
        {
            var t=Place("ballista",new Vector2(2,0)).GetComponent<WorldTurret>();Give("crossbow_bolt",4);Assert.That(t.Execute(player,TurretOperation.Deposit,Slot("crossbow_bolt")),Is.False);Give("ballista_bolt",12);Assert.That(t.Execute(player,TurretOperation.Deposit,Slot("ballista_bolt")),Is.True);Assert.That(t.Fire(),Is.False);Assert.That(t.Execute(player,TurretOperation.Reload),Is.True);var enemy=Enemy(new Vector2(2,8));Physics2D.SyncTransforms();Assert.That(t.Fire(),Is.True);Assert.That(t.Fire(),Is.False);yield return new WaitForSeconds(.5f);Assert.That(enemy.Current,Is.EqualTo(938));Assert.That(t.AmmoCount(),Is.EqualTo(11));
            Module(t,"ammo_box");Module(t,"rotation_gear");Module(t,"reinforced_frame");Assert.That(t.AmmoSlots,Is.EqualTo(3));Assert.That(t.GetComponent<Health>().Maximum,Is.EqualTo(270));var state=t.Capture();TurretState.Validate(state,t.GetComponent<WorldPlaceable>().Definition,save.Catalog);var second=Place("ballista",new Vector2(-2,0)).GetComponent<WorldTurret>();second.Restore(state,save.Catalog);Assert.That(second.AmmoCount(),Is.EqualTo(11));Assert.That(second.Modules[3].Id,Is.EqualTo("rotation_gear"));Object.Destroy(enemy.gameObject);
        }
        [UnityTest]public IEnumerator FullAutomationUsesLosIgnoresFriendliesAndStopsWhenEmpty()
        {
            var t=Place("repeater",new Vector2(2,0)).GetComponent<WorldTurret>();Give("crossbow_bolt",3);t.Execute(player,TurretOperation.Deposit,Slot("crossbow_bolt"));Module(t,"auto_loader");Module(t,"auto_aim");var foe=Enemy(new Vector2(2,8));var friend=Enemy(new Vector2(2,4),EntityRelation.PlayerTeam);var wall=new GameObject("Isolated line of sight wall");wall.transform.position=new Vector2(2,6);wall.AddComponent<BoxCollider2D>().size=new Vector2(8,1);Physics2D.SyncTransforms();yield return new WaitForSeconds(.8f);Assert.That(t.Shots,Is.Zero);Object.Destroy(wall);yield return new WaitForSeconds(2.4f);Assert.That(t.Shots,Is.EqualTo(3));Assert.That(t.AmmoCount(),Is.Zero);Assert.That(foe.Current,Is.EqualTo(952));Assert.That(friend.Current,Is.EqualTo(1000));yield return new WaitForSeconds(.7f);Assert.That(t.Shots,Is.EqualTo(3));Object.Destroy(foe.gameObject);Object.Destroy(friend.gameObject);
        }
        [UnityTest]public IEnumerator SleepingTurretHasNoFarCombatAndReturnsWithCargo()
        {
            var t=Place("ballista",new Vector2(2,0)).GetComponent<WorldTurret>();var id=t.GetComponent<WorldPlaceable>().SaveId;Give("ballista_bolt",9);t.Execute(player,TurretOperation.Deposit,Slot("ballista_bolt"));Module(t,"auto_loader");Module(t,"auto_aim");Move(new Vector2(1000,1000));yield return null;Assert.That(WorldPlaceable.Active.Any(p=>p.SaveId==id),Is.False);var before=save.World.Chunks.DormantPlaces().Single(p=>p.Id==id);Assert.That(before.Turret.Ammo.Sum(a=>a.Count),Is.EqualTo(9));save.Clock.Model.Advance(100);var after=save.World.Chunks.DormantPlaces().Single(p=>p.Id==id);Assert.That(after.Turret.Ammo.Sum(a=>a.Count),Is.EqualTo(9));Move(Vector2.zero);yield return null;var back=WorldPlaceable.Active.Single(p=>p.SaveId==id).GetComponent<WorldTurret>();Assert.That(back.AmmoCount(),Is.EqualTo(9));Assert.That(back.Modules[0].Id,Is.EqualTo("auto_loader"));
        }
        [UnityTest]public IEnumerator TurretProjectileStopsAtInactiveSimulationChunk()
        {
            var turret=Place("ballista",new Vector2(2,0)).GetComponent<WorldTurret>();Give("ballista_bolt");
            Assert.That(turret.Execute(player,TurretOperation.Deposit,Slot("ballista_bolt")),Is.True);
            Assert.That(turret.Execute(player,TurretOperation.Reload),Is.True);Assert.That(turret.Fire(),Is.True);
            var shot=Object.FindObjectsByType<StoneProjectile>(FindObjectsSortMode.None).Single();shot.transform.position=new Vector2(900,900);
            Assert.That(save.World.Chunks.Wants(shot.transform.position),Is.False);yield return null;
            Assert.That(shot==null || !shot.gameObject.activeInHierarchy,Is.True);
        }
        [UnityTest]public IEnumerator WithdrawingLastLoadedRoundClearsReadyState()
        {
            var turret=Place("ballista",new Vector2(2,0)).GetComponent<WorldTurret>();Give("ballista_bolt");
            Assert.That(turret.Execute(player,TurretOperation.Deposit,Slot("ballista_bolt")),Is.True);
            Assert.That(turret.Execute(player,TurretOperation.Reload),Is.True);Assert.That(turret.Loaded,Is.True);
            Assert.That(turret.Execute(player,TurretOperation.Withdraw,0),Is.True);
            Assert.That(turret.AmmoCount(),Is.Zero);Assert.That(turret.Loaded,Is.False);Assert.That(turret.Fire(),Is.False);
            Assert.That(player.Model.CountItem(save.Catalog.Item("ballista_bolt")),Is.EqualTo(1));yield return null;
        }
        [UnityTest]public IEnumerator DeepGemRecoveryFindsNewLocationWithoutExpandingActiveChunks()
        {
            foreach(var id in new[]{"diamond","crystal"})
            {
                var rule=System.Array.FindIndex(save.World.Definition.Resources,r=>r.Prefab.Definition.DropItem.Id==id);
                var source=save.World.Map.Resources.First(p=>p.Rule==rule);Move(source.Position);yield return null;
                var renewal=save.World.Renewal;var region=renewal.Population.Region(rule,source.Position);
                foreach(var node in Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None))
                    if(node.Rule==rule && renewal.Population.Region(rule,node.transform.position)==region)node.Node.Health.ApplyDamage(node.Node.Health.Maximum);
                var room=save.World.Map.Continental.CaveAt(source.Position).Rooms.Where(r=>Vector2.Distance(r.Center,source.Position)>35 && Vector2.Distance(r.Center,source.Position)<90).OrderBy(r=>Vector2.Distance(r.Center,source.Position)).First();
                Move(room.Center);yield return new WaitForSeconds(.3f);Assert.That(save.World.Chunks.IsActive(source.Position),Is.True);
                var active=save.World.Chunks.WantedCount;save.Clock.Model.Advance(301);
                for(var attempt=0;attempt<300;attempt++)renewal.Tick();
                var replacements=Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None).Where(n=>n.Rule==rule && n.Id.StartsWith("p") && renewal.Population.Region(rule,n.transform.position)==region).ToArray();
                Assert.That(replacements,Is.Not.Empty,id+" must recover inside the existing active cave chunk");
                Assert.That(replacements.All(n=>Vector2.Distance(n.transform.position,source.Position)>.1f),Is.True);
                Assert.That(save.World.Chunks.WantedCount,Is.EqualTo(active));
            }
        }
        [UnityTearDown]public IEnumerator Leave(){Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=WorldGenerationVersion.Current;yield return null;}
    }
}
