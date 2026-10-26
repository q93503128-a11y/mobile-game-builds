using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
namespace Wildfolk.Editor
{
    public static class DeathFoundationQa
    {
        public static string Phase {get;private set;}="idle";
        private static string Root=>Path.GetFullPath("Captures/death-revive-respawn");
        private static WorldSaveService save;private static PlayerInventory bag;private static PlayerDeath death;private static WorldGenerator world;
        private static NetworkSession host;private static NetworkPlayer actor;private static ulong sequence=10000;
        private static void Log(string s){Directory.CreateDirectory(Root);File.AppendAllText(Root+"/live.tsv",DateTime.UtcNow.ToString("O")+"\t"+Phase+"\t"+s+"\n");}
        private static void Check(bool b,string s){Log((b?"PASS ":"FAIL ")+s);if(!b)throw new InvalidOperationException(s);}
        private static void Step(string s){Phase=s;Log("begin");}
        private static IEnumerator Guard(IEnumerator flow)
        {var stack=new Stack<IEnumerator>();stack.Push(flow);while(stack.Count>0){bool more;object value;try{more=stack.Peek().MoveNext();value=more?stack.Peek().Current:null;}catch(Exception e){Phase="failed";Log(e.ToString());yield break;}if(!more){stack.Pop();continue;}if(value is IEnumerator child)stack.Push(child);else yield return value;}Phase="complete";Log("complete");}
        private static IEnumerator Shot(string name){if(Application.isBatchMode)yield break;yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Root+"/"+name+".png");yield return new WaitForEndOfFrame();yield return null;}
        private static void Move(PlayerDeath d,Vector2 p){d.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;d.GetComponent<Rigidbody2D>().position=p;d.transform.position=p;Physics2D.SyncTransforms();world.Chunks?.Reconcile();world.Chunks?.DrainPending();NetworkCreatureWorld.Active?.FlushPending();}
        private static void Clear(Vector2 at){foreach(var r in UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))if(Vector2.Distance(at,r.transform.position)<6)r.gameObject.SetActive(false);Physics2D.SyncTransforms();}
        private static void Kill(PlayerDeath d){d.GetComponent<PlayerClass>()?.RestoreCooldown(360);d.OffensiveAction();d.GetComponent<Health>().ApplyDamage(100000,true);}
        public static void BeginSingle()
        {ContentExpansionQa.Prepare();save=ContentExpansionQa.S;bag=save.Player;death=bag.GetComponent<PlayerDeath>();world=save.World;bag.GetComponent<PlayerSurvival>().enabled=false;Application.runInBackground=true;var runner=new GameObject("Isolated lifecycle QA").AddComponent<DevelopmentCoroutineHost>();UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);runner.StartCoroutine(Guard(Single()));}
        private static IEnumerator Single()
        {
            Step("single-natural-death");L.SetLanguage("ko");Clear(death.Home);Move(death,death.Home);save.Clock.Model.SetElapsed(420);
            var enemy=PrefabPool.Spawn(world.GetComponent<ZombieSpawner>().Prefab,(Vector2)bag.transform.position+Vector2.right,Quaternion.identity);enemy.PlayerHealth=bag.GetComponent<Health>();enemy.Session=world.Session;enemy.Clock=save.Clock;enemy.Lifecycle.RestoreActive();enemy.Lifecycle.enabled=false;enemy.enabled=true;enemy.Steering.Navigation=world.GetComponent<WorldNavigation>();
            bag.GetComponent<Health>().RestoreHealth(1);var until=Time.realtimeSinceStartup+10;while(Time.realtimeSinceStartup<until && bag.GetComponent<Health>().IsAlive)yield return null;
            Check(death.State==PlayerLife.Dead,"actual EnemyController swing causes SP Dead");PrefabPool.Release(enemy.gameObject);yield return null;
            Check(Time.timeScale==0 && world.Session.State==SessionState.Defeated,"SP death freezes world and input");
            var hud=UnityEngine.Object.FindFirstObjectByType<DeathHud>();Check(hud.DeadView.activeSelf && hud.Home.interactable && !hud.Ad.interactable && !hud.Gem.interactable,"real recovery UI production-disabled providers and clear free Home");
            Check(save.Repository.Read(save.Validate).Player.HP==0,"lethal autosave persists HP0");yield return Shot("01-single-death-production");
            Check(!death.RewardedRevive() && !death.GemRevive(),"unavailable providers cannot grant free revive");
            var config=UnityEngine.Object.Instantiate(death.Definition);config.ReviveGemCost=7;death.Definition=config;
            var ad=new DevelopmentRewardedRevive();var wallet=new DevelopmentPremiumWallet(100);ReviveServices.SetDevelopmentProviders(ad,wallet);
            Step("single-ad-fail");Check(death.RewardedRevive(),"reward request accepted");Check(!death.RewardedRevive() && !death.GemRevive(),"double click and competing payment rejected while ad pending");ad.Complete(false);Check(death.State==PlayerLife.Dead && !death.AdUsed && !death.PaymentPending,"failed ad grants nothing and preserves opportunity");yield return Shot("02-ad-failure");
            var crystal=save.Catalog.Item("crystal");bag.Model.Add(crystal,3);var before=bag.Model.Capture();var needs=bag.GetComponent<PlayerSurvival>().Model.Capture();var stamina=bag.GetComponent<PlayerLocomotion>().Stamina.Current;var at=(Vector2)bag.transform.position;
            Step("single-unlimited-gems");for(var i=0;i<6;i++)
            {
                Check(death.GemRevive(),"Gem revive "+(i+1));Check(death.State==PlayerLife.Alive && bag.GetComponent<Health>().Current==40 && !death.AdUsed,"partial HP no ad consumption");Check(Vector2.Distance(at,bag.transform.position)<.01f,"same-position field revival");Check(!death.GemRevive(),"duplicate success cannot charge twice");Check(bag.GetComponent<Health>().ApplyDamage(8)==0,"2-second protection rejects incoming damage");
                if(i==0){bag.GetComponent<PlayerCombat>().TryAttack();Check(!death.BlocksDamage,"actual attack start ends protection");bag.GetComponent<PlayerCombat>().CancelAttack();}
                if(i==1){yield return new WaitForSecondsRealtime(2.15f);Check(bag.GetComponent<Health>().ApplyDamage(8)==8,"protection expires naturally after two seconds");}
                Kill(death);
            }
            Check(wallet.Balance==58 && wallet.Commits==6 && bag.Model.CountItem(crystal)==3,"six repeated Gem commits exact balance and Crystal untouched");Check(JsonUtility.ToJson(new Stacks{Values=bag.Model.Capture()})==JsonUtility.ToJson(new Stacks{Values=before}),"no inventory loss");Check(bag.GetComponent<PlayerSurvival>().Model.Capture().Hunger==needs.Hunger && bag.GetComponent<PlayerSurvival>().Model.Capture().Thirst==needs.Thirst && bag.GetComponent<PlayerLocomotion>().Stamina.Current==stamina,"no free hunger/thirst/stamina reset");yield return Shot("03-six-gem-revives");
            Step("single-ad-success-reload");Check(death.RewardedRevive(),"ad opportunity remains after six Gems");ad.Complete(true);ad.Complete(true);Check(death.State==PlayerLife.Alive && death.AdUsed && bag.GetComponent<Health>().Current==40,"reward success commits once despite duplicate callback");Kill(death);Check(!death.RewardedRevive() && death.CanGem,"one successful ad per expedition, Gem remains available");
            var snapshot=save.Repository.Read(save.Validate);Check(snapshot.Version==14 && snapshot.Player.Death.AdUsed && snapshot.Player.Death.Dead,"Save14 persists ad/death");save.Apply(snapshot);Check(death.State==PlayerLife.Dead && death.AdUsed && !death.CanAd,"same-object Load cannot cancel death/refill ad");
            Check(death.GemRevive(),"Gem remains repeatable after ad and reload");Kill(death);
            ReviveServices.SetDevelopmentProviders(ad,new DevelopmentPremiumWallet(0));Check(!death.GemRevive() && death.State==PlayerLife.Dead,"insufficient Premium Gems cannot revive");
            Step("single-rollback");wallet=new DevelopmentPremiumWallet(100);ReviveServices.SetDevelopmentProviders(ad,wallet);death.Persistence=()=>false;Check(!death.GemRevive() && wallet.Balance==100 && wallet.Commits==0 && death.State==PlayerLife.Dead,"failed durable world commit rolls back HP with zero debit");death.Persistence=null;
            Step("single-home");var run=death.Run;hud.Home.onClick.Invoke();yield return null;Check(death.State==PlayerLife.Alive && bag.GetComponent<Health>().Current==100 && !death.AdUsed && run!=death.Run,"real free Home button starts next expedition");Check(Vector2.Distance(bag.transform.position,death.Home)<.01f,"Home initial seed spawn");yield return Shot("04-free-home");
            var nextHome=death.Home+Vector2.right*4;Clear(nextHome);Check(death.AssignHome(nextHome),"authority persistent Home relocation API accepts safe point without Bed gameplay");
            Kill(death);Check(death.RewardedRevive(),"new expedition allows ad again");var callback=ad.Callback;Check(death.HomeRespawn(),"Home can cancel pending ad");callback(true);Check(death.State==PlayerLife.Alive && !death.AdUsed && bag.GetComponent<Health>().Current==100,"late canceled ad callback cannot mutate new expedition");
            Step("single-local-safety");Kill(death);var atBlocked=(Vector2)bag.transform.position;
            var barrier=UnityEngine.Object.Instantiate(save.Catalog.Items.First(i=>i.Placement!=null && i.Placement.IsBarrier).Placement.Prefab,atBlocked,Quaternion.identity);Physics2D.SyncTransforms();
            Check(death.GemRevive(),"solid-blocked death position uses local valid correction");var shift=Vector2.Distance(atBlocked,bag.transform.position);Check(shift>.1f && shift<=3.01f,"field correction stays within3m and never jumps Home");UnityEngine.Object.Destroy(barrier.gameObject);yield return null;
            Step("single-new-scene-load");Kill(death);Check(save.Save(),"death save can be explicitly written");var old=save;Check(save.Load(),"actual SceneFlow Continue boundary");yield return null;yield return null;save=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();bag=save.Player;death=bag.GetComponent<PlayerDeath>();world=save.World;Check(old!=save && death.State==PlayerLife.Dead && !bag.GetComponent<Health>().IsAlive,"new scene reload retains Dead without free revival");yield return Shot("05-loaded-dead");Check(death.HomeRespawn(),"loaded death retains free Home path");ReviveServices.Reset();
        }
        [Serializable]private sealed class Stacks{public SavedStack[] Values;}
        private static void Bind(NetworkPlayer p){actor=p;bag=p.GetComponent<PlayerInventory>();death=p.GetComponent<PlayerDeath>();world=NetworkCreatureWorld.Active.World;p.Motor.enabled=p.GetComponent<PlayerInputAdapter>().enabled=false;var w=NetworkCreatureWorld.Active;w.Clock.enabled=w.Animals.enabled=w.Goblins.enabled=w.Night.enabled=world.Renewal.enabled=false;}
        private static IEnumerator EnterHost(bool resume)
        {var t=host.Host(ConnectionRoute.LocalNetwork,resume);yield return new WaitUntil(()=>t.IsCompleted);Check(t.Result,"LAN Host "+host.LastFailure);Check(host.EnterGame(),"enter multiplayer");var until=Time.realtimeSinceStartup+50;while(Time.realtimeSinceStartup<until){var p=NetworkPlayer.Active.FirstOrDefault(a=>a.IsSpawned && a.NetworkObject.IsPlayerObject && a.IsOwner);if(p!=null && NetworkCreatureWorld.Active!=null && NetworkCreatureWorld.Active.Ready){Bind(p);yield break;}yield return null;}Check(false,"Host spawn timed out");}
        private static NetworkSession NewSession(string label,string profile)
        {var h=UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkSession.prefab")).GetComponent<NetworkSession>();h.ProfileOverride=profile;h.WorldRepository=new SaveRepository(Root+"/"+label+".json");return h;}
        public static void BeginHostSimulation()
        {host=NewSession("simulation-host","dddddddddddddddddddddddddddddddd");host.StartCoroutine(Guard(HostSimulation()));}
        private static IEnumerator Hold(PlayerDeath target,PlayerDeath by,float seconds)
        {var until=target.Now+seconds;while(target.Now<until && target.State!=PlayerLife.Alive && target.State!=PlayerLife.Dead){target.ReceiveHold(by,target.Revision,true);yield return null;}}
        private static IEnumerator HostSimulation()
        {
            Step("host-four-actor-simulation");yield return EnterHost(false);Clear(death.Home);var peers=new List<NetworkPlayer>();
            for(var i=0;i<3;i++){var p=UnityEngine.Object.Instantiate(host.PlayerPrefab).GetComponent<NetworkPlayer>();typeof(NetworkPlayer).GetProperty("ProfileId").SetValue(p,new string((char)('a'+i),32));p.GetComponent<PlayerDeath>().InitializeHome(world.Map.Spawn);p.GetComponent<NetworkObject>().Spawn();p.Motor.enabled=p.GetComponent<PlayerInputAdapter>().enabled=false;peers.Add(p);Move(p.GetComponent<PlayerDeath>(),death.Home+new Vector2(i+1,0));}
            Move(death,death.Home);var target=peers[0].GetComponent<PlayerDeath>();var third=peers[1].GetComponent<PlayerDeath>();
            var helper=peers[2].GetComponent<PlayerDeath>();Move(helper,death.Home+Vector2.left*1.5f);Kill(death);yield return null;Check(death.State==PlayerLife.Downed,"actual Host Game View Downed banner");yield return Shot("06a-host-down-banner");yield return Hold(death,helper,4.3f);Check(death.State==PlayerLife.Alive,"helper restores Host for mobile-control fixture");death.OffensiveAction();
            Kill(target);yield return null;Check(target.State==PlayerLife.Downed && target.GetComponent<Health>().Current==0,"MP HP0 enters Downed keeping actor");Check(!target.GetComponent<PlayerInputCommands>().Attack() && !target.GetComponent<PlayerLocomotion>().TryDash() && !peers[0].GetComponent<NetworkInventory>().CanOperate,"downed input/attack/dash/inventory denied");
            Check(target.ReceiveHold(death,target.Revision,true),"first reviver locks target");Check(!target.ReceiveHold(third,target.Revision,true),"second reviver cannot double lock");death.GetComponent<Health>().ApplyDamage(1,true);Check(target.State==PlayerLife.Downed && death.RescueTarget==null,"damage interrupts and resets hold");
            Check(target.ReceiveHold(death,target.Revision,true),"retry after damage");Move(death,death.Home+Vector2.left*5);yield return null;Check(target.State==PlayerLife.Downed,"distance interrupt");Move(death,death.Home);
            var rev=target.Revision;Check(target.ReceiveHold(death,rev,true),"hold starts");target.ReceiveHold(death,rev,false);Check(!target.ReceiveHold(death,rev,true),"stale hold revision rejected after release");
            yield return Hold(target,death,.5f);yield return new WaitForSecondsRealtime(.65f);Check(target.State==PlayerLife.Downed,"expired input lease cannot complete a revive");
            var interact=UnityEngine.Object.FindFirstObjectByType<InteractButtonView>();yield return new WaitForSecondsRealtime(.25f);Canvas.ForceUpdateCanvases();
            Check(interact.Button.interactable && interact.gameObject.activeInHierarchy,"existing mobile Interact control is available");
            var ev=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,interact.transform.position)};var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(ev,hits);
            Check(hits.Count>0 && hits[0].gameObject.GetComponentInParent<InteractButtonView>()==interact,"mobile Interact is top raycast at its visible control");
            UnityEngine.EventSystems.ExecuteEvents.Execute(interact.gameObject,ev,UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);yield return new WaitForSecondsRealtime(4.4f);UnityEngine.EventSystems.ExecuteEvents.Execute(interact.gameObject,ev,UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
            Check(target.State==PlayerLife.Alive && target.GetComponent<Health>().Current==40,"single teammate commit restores40HP actual="+target.State+" hp="+target.GetComponent<Health>().Current);Check(!target.ReceiveHold(death,rev,true),"stale postcommit revive rejected");yield return Shot("06-simulated-teammate-revive");
            Kill(target);var saved=NetworkWorldPersistence.Active.Capture();var entry=saved.Multiplayer.Players.Single(p=>p.Id==peers[0].ProfileId).State;Check(entry.Death.Dead && entry.HP==0,"capture collapses transient Downed into persistent Dead");PlayerSnapshotCodec.Apply(peers[0].GetComponent<PlayerInventory>(),entry,actor.Catalog);Check(target.State==PlayerLife.Dead,"reconnect restore cannot become Alive");Check(target.HomeRespawn(),"Dead participant returns Home free");
            Kill(target);yield return new WaitForSecondsRealtime(24.2f);Check(target.State==PlayerLife.Dead && target.HomeRespawn(),"timeout then Home available");
            foreach(var p in peers)Kill(p.GetComponent<PlayerDeath>());Kill(death);yield return new WaitForSecondsRealtime(.4f);Check(NetworkPlayer.Active.All(p=>p.GetComponent<PlayerDeath>().State==PlayerLife.Dead),"four simulated actors all-down resolves to Dead, no indefinite wait");yield return Shot("07-simulated-all-down");Check(death.HomeRespawn(),"Host can return Home after party wipe");
            foreach(var p in peers)p.NetworkObject.Despawn();Check(host.TrySaveWorld(),"Host saves lifecycle world");Kill(death);Check(host.TrySaveWorld(),"Host Dead saved");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);yield return null;yield return null;yield return EnterHost(true);Check(death.State==PlayerLife.Dead,"actual Host/reHost restores Dead");Check(death.HomeRespawn(),"reHost free Home");yield return Shot("08-rehost-home");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);
        }

        private static string LanRoot=>Path.Combine(Path.GetTempPath(),"LowfireDeathLan19");
        private static string lanRun;
        private static void Send(string phase){Directory.CreateDirectory(LanRoot);File.WriteAllText(LanRoot+"/phase",lanRun+"|"+phase);Step("lan-"+phase);}
        private static string Read(string name){try{return File.ReadAllText(LanRoot+"/"+name);}catch(IOException){return "";}}
        private static void Ack(string phase)=>File.WriteAllText(LanRoot+"/ack",lanRun+"|"+phase);
        private static IEnumerator WaitAck(string phase,float seconds=30)
        {var until=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<until && Read("ack")!=lanRun+"|"+phase)yield return null;Check(Read("ack")==lanRun+"|"+phase,"real Client acknowledged "+phase);}
        public static void BeginLanHost()
        {lanRun=Guid.NewGuid().ToString("N");host=NewSession("lan-host","dddddddddddddddddddddddddddddddd");host.StartCoroutine(Guard(LanHost()));}
        private static IEnumerator LanHost()
        {
            yield return EnterHost(false);Clear(death.Home);Move(death,death.Home);Send("ready");
            var until=Time.realtimeSinceStartup+200;NetworkPlayer peer=null;
            while(Time.realtimeSinceStartup<until){peer=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject && !p.IsOwner);if(peer!=null)break;yield return null;}
            Check(peer!=null,"actual separate-process LAN Client admitted");peer.Motor.enabled=false;Move(peer.GetComponent<PlayerDeath>(),death.Home+Vector2.right*1.5f);
            Kill(death);Send("revive-host");until=Time.realtimeSinceStartup+20;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Alive)yield return null;
            Check(death.State==PlayerLife.Alive && actor.Health.Current==40,"real Client owner hold RPC revives Host exactly once");yield return WaitAck("revive-host");yield return Shot("09-lan-client-revives-host");
            var pd=peer.GetComponent<PlayerDeath>();Kill(pd);Send("client-down");yield return WaitAck("client-down");yield return Shot("10-lan-client-downed");
            death.OffensiveAction();Check(pd.ReceiveHold(death,pd.Revision,true),"Host starts teammate rescue");actor.Health.ApplyDamage(1,true);Check(pd.State==PlayerLife.Downed,"Host damage interrupts actual Client rescue");
            yield return Hold(pd,death,4.3f);Check(pd.State==PlayerLife.Alive && peer.Health.Current==40,"Host hold revives actual Client");Send("client-revived");yield return WaitAck("client-revived");
            Kill(pd);Send("timeout");yield return new WaitForSecondsRealtime(24.2f);Check(pd.State==PlayerLife.Dead,"real Client timeout to Dead authoritative");Send("home");yield return WaitAck("home");Check(pd.State==PlayerLife.Alive && Vector2.Distance(pd.transform.position,pd.Home)<.05f,"actual Client free Home and duplicate stale request once");
            Kill(death);Kill(pd);yield return new WaitForSecondsRealtime(.4f);Check(death.State==PlayerLife.Dead && pd.State==PlayerLife.Dead,"actual two-process all-down Dead");yield return Shot("11-lan-all-down");Check(death.HomeRespawn(),"Host Home after actual party wipe");Send("all-down-home");yield return WaitAck("all-down-home");
            Kill(pd);Send("disconnect-down");yield return WaitAck("disconnect-down");until=Time.realtimeSinceStartup+15;while(Time.realtimeSinceStartup<until && peer!=null && peer.IsSpawned)yield return null;Check(peer==null || !peer.IsSpawned,"actual downed Client disconnect");
            var entry=NetworkWorldPersistence.Active.Capture().Multiplayer.Players.Single(p=>p.Id=="bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");Check(entry.State.Death.Dead && entry.State.HP==0,"disconnect record cannot restore Alive");Send("rejoin");
            until=Time.realtimeSinceStartup+45;peer=null;while(Time.realtimeSinceStartup<until){peer=NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject && !p.IsOwner);if(peer!=null)break;yield return null;}
            Check(peer!=null && peer.GetComponent<PlayerDeath>().State==PlayerLife.Dead && !peer.Health.IsAlive,"same-profile actual reconnect remains Dead");yield return WaitAck("rejoin");yield return Shot("12-lan-reconnect-dead");Send("leave");yield return WaitAck("leave");
            until=Time.realtimeSinceStartup+15;while(Time.realtimeSinceStartup<until && peer!=null && peer.IsSpawned)yield return null;
            Kill(death);yield return new WaitForSecondsRealtime(.3f);Check(death.State==PlayerLife.Dead,"solo Host all-down ends");Check(host.TrySaveWorld(),"actual LAN death records durable");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);yield return null;yield return null;yield return EnterHost(true);Check(death.State==PlayerLife.Dead,"actual LAN reHost keeps Host Dead");Check(death.HomeRespawn(),"reHost Home path");yield return Shot("13-lan-rehost-home");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);
        }
        public static void BeginLanClient()
        {var phase=Read("phase");if(!phase.EndsWith("|ready"))throw new InvalidOperationException("Start ready Host first.");lanRun=phase.Split('|')[0];host=NewSession("lan-client","bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");host.StartCoroutine(Guard(LanClient()));}
        private static IEnumerator WaitPhase(string phase,float seconds=50)
        {var until=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<until && Read("phase")!=lanRun+"|"+phase)yield return null;Check(Read("phase")==lanRun+"|"+phase,"Host phase "+phase);}
        private static IEnumerator JoinClient()
        {var task=host.Join(ConnectionRoute.LocalNetwork,"127.0.0.1");yield return new WaitUntil(()=>task.IsCompleted);Check(task.Result,"Client transport startup");var until=Time.realtimeSinceStartup+45;while(Time.realtimeSinceStartup<until){var p=NetworkPlayer.Active.FirstOrDefault(a=>a.IsSpawned && a.NetworkObject.IsPlayerObject && a.IsOwner);if(p!=null && NetworkCreatureWorld.Active!=null){Bind(p);yield break;}yield return null;}Check(false,"Client owner spawn timeout");}
        private static IEnumerator LanClient()
        {
            Application.runInBackground=true;yield return JoinClient();yield return WaitPhase("revive-host");var until=Time.realtimeSinceStartup+20;PlayerDeath target=null;
            while(Time.realtimeSinceStartup<until){target=death.NearbyDowned();if(target!=null)break;yield return null;}Check(target!=null,"Client sees Host Downed replica and interaction priority");
            while(Time.realtimeSinceStartup<until && target.State!=PlayerLife.Alive){actor.HoldRevive(true);yield return null;}actor.HoldRevive(false);Check(target.State==PlayerLife.Alive && target.GetComponent<Health>().Current==40,"Client sees committed Host40HP replica");Ack("revive-host");
            yield return WaitPhase("client-down");until=Time.realtimeSinceStartup+8;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Downed)yield return null;
            Check(death.State==PlayerLife.Downed && actor.Commands.Session.State==SessionState.Downed && !actor.Commands.Attack(),"Client Downed presentation and input lock");Check(!actor.ExecuteLife(1,0,0,false,true),"Client cannot call authoritative life executor");Ack("client-down");
            yield return WaitPhase("client-revived");until=Time.realtimeSinceStartup+8;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Alive)yield return null;Check(death.State==PlayerLife.Alive && actor.Health.Current==40 && actor.Commands.Session.IsRunning,"Client Alive controls restored");Ack("client-revived");
            yield return WaitPhase("home");until=Time.realtimeSinceStartup+8;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Dead)yield return null;Check(death.State==PlayerLife.Dead,"Client Dead replica after timeout");var ui=UnityEngine.Object.FindFirstObjectByType<DeathHud>();Check(ui.DeadView.activeSelf && !ui.Ad.gameObject.activeSelf && !ui.Gem.gameObject.activeSelf,"MP UI only free Home, no paid options");ui.Home.onClick.Invoke();actor.RequestHome();until=Time.realtimeSinceStartup+10;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Alive)yield return null;Check(death.State==PlayerLife.Alive && actor.Health.Current==100,"real Home button/RPC double request commits once");Ack("home");
            yield return WaitPhase("all-down-home");until=Time.realtimeSinceStartup+8;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Dead)yield return null;Check(death.State==PlayerLife.Dead,"Client sees actual party wipe");actor.RequestHome();until=Time.realtimeSinceStartup+8;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Alive)yield return null;Check(death.State==PlayerLife.Alive,"Client Home after all-down");Ack("all-down-home");
            yield return WaitPhase("disconnect-down");until=Time.realtimeSinceStartup+8;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Downed)yield return null;Check(death.State==PlayerLife.Downed,"Client disconnects from actual Downed");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);Ack("disconnect-down");yield return WaitPhase("rejoin");yield return null;yield return null;yield return JoinClient();until=Time.realtimeSinceStartup+8;while(Time.realtimeSinceStartup<until && death.State!=PlayerLife.Dead)yield return null;Check(death.State==PlayerLife.Dead && actor.Health.Current==0,"real reconnect cannot cancel death");Ack("rejoin");
            yield return WaitPhase("leave");host.Leave();yield return new WaitUntil(()=>host.State==NetworkSessionState.Idle);Ack("leave");
        }
    }
}
