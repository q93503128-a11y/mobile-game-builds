using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Wildfolk.Editor
{
    public static class ClassFoundationQa
    {
        public static string Phase{get;private set;}="idle";
        private static string Root=>Path.GetFullPath("Captures/class-progression");
        private static WorldSaveService save;private static PlayerInventory bag;private static PlayerClass role;private static Health health;
        private static PlayerDeath death;private static PlayerSurvival survival;
        public static void Abort(){foreach(var runner in UnityEngine.Object.FindObjectsByType<DevelopmentCoroutineHost>(FindObjectsSortMode.None))if(runner.name=="Class objective QA"){runner.StopAllCoroutines();UnityEngine.Object.Destroy(runner.gameObject);}}
        public static void Begin(string method)
        {
            if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated profile required");
            Phase="starting";Directory.CreateDirectory(Root);var runner=new GameObject("Class objective QA").AddComponent<DevelopmentCoroutineHost>();UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
            var flow=(IEnumerator)typeof(ClassFoundationQa).GetMethod(method,BindingFlags.Public|BindingFlags.Static).Invoke(null,null);runner.StartCoroutine(Run(flow));
        }
        private static void Log(string s){Directory.CreateDirectory(Root);File.AppendAllText(Root+"/live.tsv",DateTime.UtcNow.ToString("O")+"\t"+Phase+"\t"+s+"\n");}
        private static void Check(bool value,string s){Log((value?"PASS ":"FAIL ")+s);if(!value)throw new InvalidOperationException(s);}
        private static void Step(string s){Phase=s;Log("begin");}
        private static IEnumerator Run(IEnumerator routine)
        {
            var stack=new Stack<IEnumerator>();stack.Push(routine);
            while(stack.Count>0){bool more;object value;try{more=stack.Peek().MoveNext();value=more?stack.Peek().Current:null;}catch(Exception e){Phase="failed";Log(e.ToString());yield break;}if(!more){stack.Pop();continue;}if(value is IEnumerator child)stack.Push(child);else yield return value;}
            Phase="complete";Log("complete");
        }
        private static IEnumerator Prepare()
        {
            Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;
            yield return SceneManager.LoadSceneAsync(SceneFlow.GameplayScene);yield return null;yield return null;
            ContentExpansionQa.Prepare();save=ContentExpansionQa.S;bag=save.Player;role=bag.GetComponent<PlayerClass>();health=bag.GetComponent<Health>();death=bag.GetComponent<PlayerDeath>();survival=bag.GetComponent<PlayerSurvival>();survival.enabled=false;save.World.Animals.enabled=false;save.World.Renewal.enabled=false;
            ContentExpansionQa.Clear(bag.transform.position,12);ContentExpansionQa.Move(save.World.Map.Spawn);bag.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;death.Persistence=()=>true;L.SetLanguage("ko");
            foreach(var p in WorldPlaceable.Active.ToArray())if(Vector2.Distance(p.transform.position,bag.transform.position)<8)PrefabPool.Release(p.gameObject);
        }
        private static void Role(string id,params string[] nodes)
        {
            var account=AccountProgression.New();foreach(var p in account.Classes){p.Owned=true;p.XP=1800;}account.Selected=id;account.Find(id).Nodes=nodes;
            Check(ClassProfiles.Store.Write(ClassProfiles.LocalId,account),"QA durable profile "+id);
            typeof(PlayerClass).GetProperty("Locked").SetValue(role,false);Check(role.Lock(ClassProfiles.LocalId,account),"QA isolated loadout "+id);role.RestoreCooldown(0);
            health.DamageReduction=0;health.RestoreHealth(health.Maximum);death.OffensiveAction();bag.GetComponent<PlayerCombat>().CancelAttack();bag.GetComponent<PlayerRanged>().Cancel();bag.GetComponent<PlayerInputCommands>().SetMove(Vector2.zero);
        }
        private static IEnumerator Shot(string name)
        {if(Application.isBatchMode)yield break;yield return new WaitForSecondsRealtime(.3f);yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Root+"/"+name+".png");yield return new WaitForEndOfFrame();yield return null;}
        private static Health Source(Vector2 position)
        {var go=new GameObject("QA damage origin");go.SetActive(false);var h=go.AddComponent<Health>();h.Definition=ScriptableObject.CreateInstance<VitalityDefinition>();h.Definition.MaximumHealth=1000;go.transform.position=position;go.SetActive(true);return h;}
        private static WorldPlaceable Place(string id,Vector2 offset)
        {var p=ContentExpansionQa.Place(id,(Vector2)bag.transform.position+offset);p.PlayerBuilt=true;p.Health.RestoreCombatTime(0,0);Physics2D.SyncTransforms();return p;}
        private static void ClearBag(){var a=new SavedStack[bag.Model.Capacity];for(var i=0;i<a.Length;i++)a[i]=new SavedStack();bag.Model.Restore(a,save.Catalog);}
        private static IEnumerator Hold(StructureMaintenance target,BuildingServiceKind kind,float seconds)
        {var service=bag.GetComponent<PlayerBuildingService>();var end=Time.time+seconds;while(Time.time<end){service.Hold(target,kind,true);yield return null;}service.Hold(target,kind,false);}
        public static IEnumerator VisibleSequence(){yield return SharedGrowth();yield return Survivor();yield return Warrior();yield return Hunter();yield return Medicine();yield return Buildings();yield return TurretsAndExploration();yield return Lobby();}
        public static IEnumerator Lobby()
        {
            yield return Prepare();Step("lobby-purchase-tree");Role("warrior");var progression=bag.GetComponent<PlayerProgression>();progression.SetValues(450,240);death.Persistence=null;Check(save.Save(),"isolatedworldGold240durablysaved");var repository=save.Repository;
            Check(ClassProfiles.Store.Write(ClassProfiles.LocalId,AccountProgression.New()),"startdefaultownership");SceneFlow.Home(repository);yield return SceneReady(false);
            L.SetLanguage("ko");var screen=UnityEngine.Object.FindFirstObjectByType<ClassLobbyScreen>();screen.Open();Check(ClassProfiles.Local.Selected=="survivor" && ClassProfiles.Local.Find("survivor").Owned,"defaultSurvivorownedselected");
            screen.SelectCard(1);Check(!screen.SelectButton.interactable && screen.PurchaseButton.interactable && screen.PurchaseLabel.text.Contains("60"),"lockedGoldclassshowspriceandpurchase");
            foreach(var index in new[]{1,2,3,4})
            {screen.SelectCard(index);screen.PurchaseButton.onClick.Invoke();Check(screen.Confirmation.activeSelf,"purchaseconfirmation "+index);screen.ConfirmPurchase();Check(ClassProfiles.Local.Find(ClassCatalog.Current.Classes[index].Id).Owned,"durableGoldunlock "+index);}
            var game=repository.Read(screen.Rules.Validate);Check(game.Player.Gold==0 && game.Player.ClassPurchases.Length==4,"existingworldGoldchargedexactly240with4receipts");Check(!ClassProfiles.Purchase("warrior",screen.Rules),"ownedclassduplicatepurchaserejected");
            screen.SelectCard(5);Check(!screen.PurchaseButton.interactable && !ClassProfiles.Local.Find("engineer").Owned,"productionEngineerunavailablewithoutproviderorprice");yield return Shot("10-lobby-engineer-locked");
            screen.SelectCard(2);screen.SelectClass();Check(ClassProfiles.Local.Selected=="hunter","LobbyselectownedHunter");
            ClassProfiles.Update(ClassProfiles.LocalId,d=>{d.Find("hunter").XP=1800;return true;});Check(!ClassProfiles.Spend("hunter","A2"),"classprerequisiteenforced");screen.OpenTree();
            foreach(var index in new[]{0,1,2,3,4,5}){screen.SelectNode(index);screen.Invest();}Check(ClassProfiles.Local.Find("hunter").Nodes.Length==6 && !ClassProfiles.Spend("hunter","C1"),"sixearnedpointsmustchoosefromnine");
            var after=ClassProfiles.Local.Find("hunter").Nodes.Length;Check(!ClassProfiles.Spend("hunter","A1") && ClassProfiles.Local.Find("hunter").Nodes.Length==after,"duplicatespenddoesnotcharge");Check(ClassProfiles.Local.Find("medic").XP==0,"ClassXPisolatedbyclass");yield return Shot("11-lobby-owned-selected-tree");
            screen.Respec();Check(ClassProfiles.Local.Find("hunter").Nodes.Length==0 && ClassCatalog.Current.Earned(ClassProfiles.Local.Find("hunter").XP)==6,"freeLobbyrespeconlyresetsallocation");
            foreach(var index in Enumerable.Range(0,6)){screen.SelectCard(index);foreach(var n in Enumerable.Range(0,9)){screen.SelectNode(n);Canvas.ForceUpdateCanvases();Check(screen.NodeDetail.preferredHeight<=screen.NodeDetail.rectTransform.rect.height+1,"Koreandetailfits "+index+":"+n);}}
            var pendingReceipt=game.Player.ClassPurchases[0];ClassProfiles.Update(ClassProfiles.LocalId,d=>{d.Find("warrior").Owned=false;d.Pending=new ClassPurchaseIntent{Transaction=pendingReceipt,ClassId="warrior",Cost=60,WorldPath=repository.Path};return true;});Check(ClassProfiles.Reconcile(screen.Rules) && ClassProfiles.Local.Find("warrior").Owned,"committed receipt repairs missing entitlement without second debit");
            ClassProfiles.Update(ClassProfiles.LocalId,d=>{d.Pending=new ClassPurchaseIntent{Transaction=Guid.NewGuid().ToString("N"),ClassId="medic",Cost=60,WorldPath=repository.Path};return true;});Check(ClassProfiles.Reconcile(screen.Rules) && ClassProfiles.Local.Pending==null && repository.Read(screen.Rules.Validate).Player.Gold==0,"uncommitted intent clears without debit or grant");
            var engineer=ClassCatalog.Current.Find("engineer");var profileStore=ClassProfiles.Store;var price=engineer.Price;try{engineer.Price=7;var wallet=new ClassQaWallet();ClassProfiles.DevelopmentProviders(profileStore,wallet);Check(ClassProfiles.Purchase("engineer",screen.Rules) && wallet.Balance==3 && ClassProfiles.Local.Find("engineer").Owned,"QA approved price injected wallet grants and charges once");Check(!ClassProfiles.Purchase("engineer",screen.Rules) && wallet.Balance==3,"QA premium duplicate no second charge");}finally{engineer.Price=price;ClassProfiles.DevelopmentProviders(profileStore);}
            screen.CloseTree();screen.Back();UnityEngine.Object.FindFirstObjectByType<HomeScreen>().TryContinue();yield return SceneReady(true);
            var actor=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>().Player.GetComponent<PlayerClass>();Check(actor.Definition.Id=="hunter" && actor.Locked,"selectedClasslockedonworldentry");Check(!ClassProfiles.Select("warrior") && !ClassProfiles.Respec("hunter") && !actor.Lock(ClassProfiles.LocalId,ClassProfiles.Local),"Gameplayselectionrespeclockrejected");
        }
        private static IEnumerator SceneReady(bool gameplay)
        {
            var until=Time.realtimeSinceStartup+60;while(Time.realtimeSinceStartup<until){yield return null;var ready=gameplay?UnityEngine.Object.FindFirstObjectByType<WorldSaveService>()!=null:UnityEngine.Object.FindFirstObjectByType<HomeScreen>()!=null;if(ready&&!TransitionOverlay.Visible)yield break;}throw new TimeoutException("UI scene transition did not finish");
        }
        private sealed class ClassQaWallet:IClassPurchaseWallet {public int Balance=10;public bool Available=>true;private readonly HashSet<string> done=new HashSet<string>();public bool Commit(string transaction,int price,Func<bool> grant){if(price<=0 || Balance<price || done.Contains(transaction) || !grant())return false;Balance-=price;done.Add(transaction);return true;}}
        public static IEnumerator SharedGrowth()
        {
            yield return Prepare();Step("shared-growth");Role("warrior");var p=bag.GetComponent<PlayerProgression>();
            foreach(var level in new[]{20,99,999}){p.SetValues((long)p.Definition.TotalBefore(level+1)-1,0);p.Reward(1,0);Check(p.Level==level+1 && p.CurrentXP==0,"level boundary "+level);}
            p.SetValues((long)p.Definition.TotalBefore(1001),31);foreach(var i in Enumerable.Range(0,6))for(var n=0;n<p.Definition.Skills[i].MaximumRank;n++)Check(p.Invest(i),"shared rank "+i+":"+n);
            Check(p.SkillPoints==973 && !p.Invest(0) && !p.Invest(-1),"surplus points and invalid/max spend");var hp=health.Current;p.Invest(0);Check(health.Current==hp,"Vitality does not heal");
            var snapshot=save.Capture();save.Validate(snapshot);Check(snapshot.Version==14 && snapshot.Player.Growth.Skills.Length==6,"Save14 common growth validates");save.Apply(snapshot);Check(p.Level==1001 && p.SkillPoints==973 && p.Rank(0)==10,"common growth survives actual save apply");
            p.SetValues(0,0);Check(!p.Invest(2),"prerequisite and insufficient points");p.SetValues(35,0);Check(p.Invest(1) && !p.Invest(2),"point consumption exactly once");
            p.SetValues(450,40);var legacy=save.Capture();legacy.Version=12;legacy.Player.Growth.Skills=legacy.Player.Growth.Skills.Take(4).ToArray();legacy.Player.Growth.Skills[0].Rank=3;legacy.Player.Growth.Points=2;save.Validate(legacy);Check(legacy.Version==14 && legacy.Player.Growth.Skills.Length==6 && legacy.Player.Growth.Skills[0].Rank==3 && legacy.Player.Growth.Points==2,"v12 rank3 and points preserved with two zero nodes");
            p.SetValues((long)p.Definition.TotalBefore(1001),40);var session=bag.GetComponent<PlayerInputCommands>().Session;session.OpenCharacter();yield return null;
            var screen=UnityEngine.Object.FindFirstObjectByType<CharacterScreen>();Check(screen.Level.text.Contains("1001"),"four digit character level UI");screen.OpenTree();yield return null;var tree=UnityEngine.Object.FindFirstObjectByType<SkillTreeScreen>();Check(tree.Nodes.Length==6 && tree.View.activeSelf,"actual six shared nodes");yield return Shot("01-shared-tree");tree.Close();
            var oldXP=ClassProfiles.Local.Find("warrior").XP;p.Reward(10,0);Check(ClassProfiles.Local.Find("warrior").XP==oldXP,"debug reward does not grant Class XP");p.GameplayReward(8,3,"qa-reward-once");p.GameplayReward(8,3,"qa-reward-once");Check(ClassProfiles.Local.Find("warrior").XP==oldXP+8,"duplicate reward receipt awards Class XP once");
        }
        public static IEnumerator Survivor()
        {
            yield return Prepare();Step("survivor");Role("survivor","A1","A2","A3","B1","B2","B3");var stamina=bag.GetComponent<PlayerLocomotion>().Stamina;stamina.Spend(40);health.RestoreHealth(1);health.ApplyDamage(50,true);
            Check(health.Current==10 && death.State==PlayerLife.Alive && role.SurvivalCooldown>269 && role.BlocksDamage,"lethal interception before Death,10percentHP,270s cooldown");Check(stamina.Current==85,"survival restores25 stamina");Check(health.ApplyDamage(20,true)==0,"survival protection blocks damage");death.OffensiveAction();Check(!role.BlocksDamage,"attack intent clears survival protection");
            var snapshot=save.Capture();save.Validate(snapshot);var remaining=snapshot.Player.SurvivalCooldown;save.Apply(snapshot);Check(role.SurvivalCooldown>=remaining-.1,"Save/load preserves cooldown");health.ApplyDamage(1000,true);Check(death.State==PlayerLife.Dead,"second lethal during cooldown reaches normal death");Check(death.HomeRespawn(),"Home respawn succeeds");Check(role.SurvivalCooldown>260,"Home does not reset cooldown");death.OffensiveAction();yield return Shot("02-survivor");
            Role("survivor","C1","C2","C3");Check(Mathf.Approximately(role.NutritionMultiplier,1.1f) && Mathf.Approximately(role.MealDurationMultiplier,1.2f) && Mathf.Approximately(role.NeedsMultiplier,.85f),"survivor fieldcraft modifiers");
        }
        public static IEnumerator Warrior()
        {
            yield return Prepare();Step("warrior");Role("warrior","A1","A2","A3","B1","B2","B3");var source=Source((Vector2)bag.transform.position+Vector2.up*2);bag.GetComponent<PlayerAvatar>().Facing.Face(Vector2.up);Check(role.SetGuard(true),"Guard input held");health.DamageReduction=6;
            Check(health.ApplyDamage(40,false,null,source)==17 && !role.LastHeavy,"armor thenGuard17 actualHP,notheavy");health.RestoreHealth(100);health.DamageReduction=0;role.SetGuard(true);var heavyDamage=health.ApplyDamage(40,false,null,source);Check(heavyDamage==20 && role.LastHeavy,"final20HP qualifies heavy: damage="+heavyDamage+" max="+health.Maximum+" heavy="+role.LastHeavy);
            Check(role.HeavyUntil>role.Now,"heavy defense window");Check(!bag.GetComponent<PlayerLocomotion>().TryDash(),"Guard blocks Dash");Check(Mathf.Approximately(role.AttackInterval,1.35f) && role.GuardMove(Vector2.up)<1,"Guard interval/movement penalty with upgrades");
            Check(bag.GetComponent<PlayerCombat>().TryAttack(),"Guard permits actual attack start");bag.GetComponent<PlayerCombat>().CancelAttack();role.BeginMelee();Check(role.Outgoing(source,100,DamageKind.Melee,false)==100,"first melee attack consumes riposte opportunity at attack start");
            role.SetGuard(false);source.transform.position=(Vector2)bag.transform.position-Vector2.up*2;health.RestoreHealth(100);role.SetGuard(true);Check(health.ApplyDamage(10,false,null,source)==9,"backhit not reduced by frontalGuard,onlyactiveheavywindow");role.SetGuard(false);
            Role("warrior","C1","C2","C3");role.SetGuard(true);Check(role.Frontal((Vector2)bag.transform.position+new Vector2(1,.4f)) && !role.Frontal((Vector2)bag.transform.position-Vector2.up),"150degree guard cannotcoverrear");Check(role.GuardMove(Vector2.up)>role.GuardMove(Vector2.down),"forward mobility specialization");role.SetGuard(false);Check(bag.GetComponent<PlayerLocomotion>().Stamina.RecoveryWait<=.6f,"release shortens recovery wait");yield return Shot("03-warrior-guard");UnityEngine.Object.Destroy(source.gameObject);
        }
        public static IEnumerator Hunter()
        {
            yield return Prepare();Step("hunter");Role("hunter","A1","A2","A3","B1","B2","B3");var beast=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<NeutralAnimal>("Assets/Wildfolk/Prefabs/GiantBoar.prefab"),(Vector2)bag.transform.position+Vector2.right*2,Quaternion.identity);beast.enabled=false;beast.GetComponent<WildlifeActionBrain>().enabled=false;var target=beast.GetComponent<Health>();target.RestoreHealth(target.Maximum);target.ApplyDamage(1,false,bag,health,DamageKind.Ranged,true);
            Check(role.Quarry==target && role.QuarryUntil-role.Now>29,"actual valid hit assigns one30secondQuarry");Check(role.Outgoing(target,100,DamageKind.Ranged,true)==120,"Quarry8plusstationary12 ranged bonus");Check(role.ShotRange((Vector2)(target.transform.position-bag.transform.position))==1.1f,"generic bow/crossbow range towardQuarry");Check(role.SprintMultiplier((Vector2)(target.transform.position-bag.transform.position))<.9f,"tracking sprint efficiency");
            var source=Source((Vector2)bag.transform.position+Vector2.left*2);source.gameObject.AddComponent<CreatureMember>();role.Hit(source,1,DamageKind.Spear);Check(role.Quarry==source,"new validtarget replaces Quarry");UnityEngine.Object.Destroy(source.gameObject);yield return null;yield return null;Check(role.Quarry==null,"destroyedtarget clears Quarry");
            Role("hunter","C1","C2","C3");Check(Mathf.Approximately(role.SpearReach,1.1f),"generic spear reach");role.Hit(target,1,DamageKind.Spear);var collider=target.GetComponent<Collider2D>();if(collider!=null)collider.enabled=false;target.Definition=UnityEngine.Object.Instantiate(target.Definition);target.Definition.MaximumHealth=130;target.RestoreHealth(130);Check(role.Outgoing(target,100,DamageKind.Spear,false)==120,"largeQuarry8plus12 spear bonus");yield return Shot("04-hunter-quarry");
        }
        public static IEnumerator Medicine()
        {
            yield return Prepare();Step("medicine");Role("medic","A1","A2","A3","C1","C2","C3");var growth=bag.GetComponent<PlayerProgression>();growth.SetValues(450,0);for(var i=0;i<3;i++)Check(growth.Invest(5),"FieldCare rank "+i);yield return null;
            ClearBag();var medicine=save.Catalog.Items.Where(i=>i.Consumable?.Kind==ConsumableKind.Medicine).ToArray();foreach(var item in medicine)
            {
                health.RestoreHealth(20);survival.ResetTransient();bag.Model.Add(item,2);ContentExpansionQa.Select(item.Id,false);var before=bag.Model.CountItem(item);Check(bag.GetComponent<PlayerMedicine>().Begin(health),"selfMedicine "+item.Id);Check(Math.Abs(survival.UseReadyAt-Time.time-3.2f)<.02f,"generic Medicine cooldown4 to3.2");Check(bag.Model.CountItem(item)==before-1 && !bag.GetComponent<PlayerMedicine>().Begin(health),"one item and sharedcooldown "+item.Id);
                var expected=Mathf.RoundToInt((item.Consumable.HealthRestore+item.Consumable.GradualHealthRestore)*1.25f*1.3f);
                if(item.Consumable.GradualHealthRestore>0){Check(Mathf.Abs(survival.Treatment.Remaining-item.Consumable.HealingDuration*.75f)<.1f,"HoTdelivery shortened withoutlosingtotal");survival.AdvanceTreatment(20);}
                Check(health.Current==20+expected,"outgoing critical25percent * received30percent "+item.Id);
            }
            // No production medicine gets a new cast; generic non-zero application is supported with a QA clone.
            survival.ResetTransient();health.RestoreHealth(20);ClearBag();var salve=medicine.First(i=>i.Consumable.HealthRestore>0);var clone=UnityEngine.Object.Instantiate(salve);clone.Id=salve.Id;clone.Consumable=UnityEngine.Object.Instantiate(salve.Consumable);clone.Consumable.ApplicationSeconds=1;bag.Model.Add(clone,1);for(var i=0;i<9;i++)if(bag.Model.GetSlot(i).Item==clone)bag.SelectSlot(i);
            Check(bag.GetComponent<PlayerMedicine>().Begin(health),"generic timed medicine begins");Check(Math.Abs(bag.GetComponent<PlayerMedicine>().Until-role.Now-1)<.05,"A2 leaves application unchanged");bag.GetComponent<PlayerMedicine>().Cancel();Check(bag.Model.CountItem(clone)==1,"canceledmedicine notconsumed");yield return Shot("05-medic");
        }
        public static IEnumerator Buildings()
        {
            yield return Prepare();Step("building-services");ClearBag();Role("warrior");var place=Place("wood_wall",Vector2.right*2);var service=place.GetComponent<StructureMaintenance>();place.Health.ApplyDamage(60,true);Check(!service.CanRepair(role),"recentdamage lockout");place.Health.RestoreCombatTime(0,0);bag.Model.Add(save.Catalog.Item("wood"),50);
            yield return Hold(service,BuildingServiceKind.Repair,2.1f);Check(place.Health.Current==place.Health.Maximum && bag.Model.CountItem(save.Catalog.Item("wood"))==49,"normalholdrepair consumescheapmaterialonce");
            place.Health.ApplyDamage(20,true);place.Health.RestoreCombatTime(0,0);Check(bag.GetComponent<PlayerBuildingService>().Hold(service,BuildingServiceKind.Repair,true),"repairhold starts");place.Health.ApplyDamage(1,true);yield return null;Check(!bag.GetComponent<PlayerBuildingService>().Active,"newstructure damageinterruptsrepair");
            Role("engineer","A1","A2","A3","C1","C2","C3");place.Health.RestoreCombatTime(0,0);var wood=bag.Model.CountItem(save.Catalog.Item("wood"));Check(service.Repair(bag),"Engineer zerocostHP repair");Check(bag.Model.CountItem(save.Catalog.Item("wood"))==wood,"Engineer noHPmaterialcharge");Check(service.Repair(bag) && service.Buffer==40 && place.Health.Maximum==100,"30percentbase +10barrierbuffer withoutMaxHPmutation");
            Check(place.Health.ApplyDamage(15,true)==0 && service.Buffer==25 && place.Health.Current==100,"temporarybuffer absorbsfirst");service.gameObject.SetActive(false);service.gameObject.SetActive(true);service.WakeBuffer();Check(service.Buffer==25,"same-sessionstableID buffer sleep/wake");var snapshot=save.Capture();save.Validate(snapshot);Check(!SaveRepository.Encode(snapshot).Contains("Buffer"),"temporarybuffer notserialized");
            place.Health.RestoreHealth(90);Check(service.Refund().Single().Count==2,"90percent fullrefund");place.Health.RestoreHealth(89);Check(service.Refund().Single().Count==1,"below90percent flooredrefund");place.Health.RestoreHealth(1);Check(service.Refund().Length==0,"neardestroyed safezero refund");place.Health.RestoreHealth(100);
            var initial=bag.Model.CountItem(save.Catalog.Item("wood"));for(var i=0;i<100;i++){Check(service.Dismantle(bag),"cycle recovery "+i);Check(bag.Model.TryConsumeCosts(place.Definition.ConstructionMaterials),"cycle rebuildingcost "+i);place=Place("wood_wall",Vector2.right*2);service=place.GetComponent<StructureMaintenance>();}Check(bag.Model.CountItem(save.Catalog.Item("wood"))==initial,"100 build/recover cycles noresourcegrowth");
            var chest=Place("storage_chest",Vector2.left*2);chest.GetComponent<StorageChest>().Model.Add(save.Catalog.Item("wood"),1);Check(!chest.GetComponent<StructureMaintenance>().Dismantle(bag),"chest contents blockrecovery");
            var turret=Place("ballista",Vector2.up*2).GetComponent<WorldTurret>();turret.Ammo.Add(turret.Definition.Ammunition,1);Check(!turret.GetComponent<StructureMaintenance>().Dismantle(bag),"turretammo blocksrecovery");turret.Ammo.ConsumeSlot(0,turret.Definition.Ammunition,1);turret.Modules[0]=save.Catalog.Items.First(i=>i.TurretModule?.Slot==TurretModuleSlot.Loader);Check(!turret.GetComponent<StructureMaintenance>().Dismantle(bag),"turretmodule blocksrecovery");
            var bone=Place("bone_trap",new Vector2(3,2));Check(bone.GetComponent<SpikeTrap>().SingleUse,"BoneTrap stayssingleuse");bone.Health.ApplyDamage(1000,true);Check(!bone.Health.IsAlive && !bone.GetComponent<StructureMaintenance>().CanRepair(role),"consumedBoneTrap cannotberepaired");yield return Shot("06-engineer-repair");
        }
        public static IEnumerator TurretsAndExploration()
        {
            yield return Prepare();Step("turret-overclock");Role("engineer","B1","B2","B3");foreach(var id in new[]{"ballista","repeater"})
            {
                var gun=Place(id,Vector2.right*2).GetComponent<WorldTurret>();var damage=gun.Definition.Damage;var range=gun.Definition.Range;var ammo=gun.AmmoCount();
                Check(role.Overclock(gun),"activeoverclock "+id);Check(Mathf.Approximately(gun.ReloadMultiplier,1.35f) && Mathf.Approximately(gun.RotationMultiplier,1.45f),"overclockspeedwithB3 "+id);Check(gun.OverclockUntil-Time.timeAsDouble>15.8 && role.ActiveReady-role.Now>23.8,"16second duration24second cooldown");Check(!role.Overclock(gun) && gun.AmmoCount()==ammo && gun.Modules.All(m=>m==null) && gun.Definition.Damage==damage && gun.Definition.Range==range,"noammo/module/damage/rangegrant "+id);typeof(WorldTurret).GetField("overclockUntil",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(gun,Time.timeAsDouble-.1);gun.Tick(.01f);Check(gun.ReloadMultiplier==1 && gun.RotationMultiplier==1 && gun.OverclockRemaining==0,"overclock expiry restores module-only speed");PrefabPool.Release(gun.gameObject);typeof(PlayerClass).GetProperty("ActiveReady").SetValue(role,0d);
            }
            yield return Shot("07-engineer-overclock");Step("exploration");ClearBag();ContentExpansionQa.Select("telescope");var scope=bag.GetComponent<PlayerTelescope>();Role("warrior");Check(Mathf.Approximately(scope.EffectiveRange,160),"normaloptic160");Role("explorer","A1","A2","A3","B1","B2","B3");Check(Mathf.Approximately(scope.EffectiveRange,216),"Exploreroptic216");
            var from=(Vector2)bag.transform.position;Vector2 direction=default;var radius=scope.EffectiveRange;
            for(var i=0;i<128 && direction==Vector2.zero;i++){var d=new Vector2(Mathf.Cos(i*Mathf.PI/64),Mathf.Sin(i*Mathf.PI/64));var valid=true;for(var t=1f;t<=radius;t++)if(!save.World.Map.IsWalkable(from+d*t,0)){valid=false;break;}if(valid)direction=d;}
            Check(direction!=Vector2.zero,"validobservedlandcorridor");ContentExpansionQa.Clear(from,radius+1);Check(scope.Use(),"actualopticuse");scope.Aim(direction);yield return null;var marker=bag.GetComponent<PlayerTeamMarker>();var before=save.World.Renewal.Capture().Length;
            Check(marker.Place(scope.ViewPoint,0),"validatedobservedreticlemarker");Check(marker.Until-role.Now>89,"A3 marker90seconds");Check(!marker.Place(scope.ViewPoint+Vector2.one*1000,0),"outofrange markerdenied");Check(!marker.Place(new Vector2(float.NaN,0),0),"nonfinite markerdenied");Check(save.World.Renewal.Capture().Length==before,"optic/marker createsnohiddenresourceactors");yield return Shot("08-explorer-optic-marker");yield return new WaitForSeconds(1.1f);Check(marker.Place(scope.ViewPoint,0),"single marker replace after spam gate");typeof(PlayerTeamMarker).GetProperty("Until").SetValue(marker,role.Now-.1);yield return null;Check(marker.Until==0,"temporary marker expires without save");scope.Exit();Check(!marker.Available,"markerbuttononlyduringobservation");
            Role("explorer","C1","C2","C3");Check(Mathf.Approximately(role.SignalMultiplier,1.4f) && role.Value(ClassEffect.SignalDuration)==.5f && role.Value(ClassEffect.SignalMeaning)==1,"signalbranchgenericmodifiers");marker.Clear();
        }
        public static IEnumerator RewardedContract()
        {
            yield return Prepare();Step("rewarded-completion-contract");Role("warrior");var ad=new DevelopmentRewardedRevive();ReviveServices.SetDevelopmentProviders(ad,new DevelopmentPremiumWallet(0));
            foreach(var outcome in new[]{"Started","Closed","StoreOpened","Skipped","Failed"})
            {
                death.OffensiveAction();health.ApplyDamage(10000,true);Check(death.State==PlayerLife.Dead && death.RewardedRevive(),"pendingad "+outcome);
                if(outcome=="Started" || outcome=="StoreOpened")Check(death.State==PlayerLife.Dead && !death.AdUsed,"nonrewardSDKnotification grantsnothing "+outcome);else ad.Complete(false);
                Check(death.State==PlayerLife.Dead && !death.AdUsed,"noadrevive "+outcome);Check(death.HomeRespawn(),"cancelpending viaHome");
            }
            death.OffensiveAction();health.ApplyDamage(10000,true);Check(death.RewardedRevive(),"completionrequest");ad.Complete(true);ad.Complete(true);Check(death.State==PlayerLife.Alive && death.AdUsed && health.Current==40,"completedexactlyonce");ReviveServices.Reset();Check(!ReviveServices.Rewarded.Available && !ReviveServices.Wallet.Available,"productionprovidersfailclosed");yield return Shot("09-reward-contract");
        }
    }
}
