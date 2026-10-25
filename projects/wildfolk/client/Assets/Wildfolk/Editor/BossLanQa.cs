using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Wildfolk.Editor
{
    [InitializeOnLoad]public static class BossLanQa
    {
        public static string Phase{get;private set;}="idle";
        private const string HostId="cccccccccccccccccccccccccccccccc",ClientId="dddddddddddddddddddddddddddddddd";
        private static string Shared=>Environment.GetEnvironmentVariable("LOWFIRE_BOSS_QA_DIR")??UnityEditor.SessionState.GetString("Lowfire.NetworkQa.Directory",Path.GetFullPath("Captures/undead-king-boss/actual-lan"));
        private static ConnectionRoute Route=>(Environment.GetEnvironmentVariable("LOWFIRE_BOSS_QA_ROUTE")??UnityEditor.SessionState.GetString("Lowfire.NetworkQa.Route","LocalNetwork"))=="Internet"?ConnectionRoute.Internet:ConnectionRoute.LocalNetwork;
        private static string run;private static string Dir=>Path.Combine(Shared,run);
        private static NetworkSession session;private static bool client;
        static BossLanQa(){EditorApplication.playModeStateChanged+=PlayState;}
        private static void PlayState(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode && UnityEditor.SessionState.GetBool("Lowfire.BossLan.Client",false)){UnityEditor.SessionState.SetBool("Lowfire.BossLan.Client",false);Start(true);}}
        public static void Client(){Directory.CreateDirectory(Shared);run=File.ReadAllText(Path.Combine(Shared,"current-run.txt")).Trim();UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",Path.Combine(Dir,"client-profile"));UnityEditor.SessionState.SetBool("Lowfire.BossLan.Client",true);Application.runInBackground=true;EditorApplication.update-=Pump;EditorApplication.update+=Pump;EditorApplication.isPlaying=true;}
        private static void Pump(){if(EditorApplication.isPlaying)EditorApplication.QueuePlayerLoopUpdate();}
        public static void Host(){if(!Application.isPlaying || !DevelopmentProfile.Isolated)throw new InvalidOperationException("Isolated Play required");Application.runInBackground=true;EditorApplication.update-=Pump;EditorApplication.update+=Pump;Directory.CreateDirectory(Shared);run=Guid.NewGuid().ToString("N");Directory.CreateDirectory(Dir);File.WriteAllText(Path.Combine(Shared,"current-run.txt"),run);Start(false);}
        private static void Start(bool remote)
        {client=remote;if(remote)run=File.ReadAllText(Path.Combine(Shared,"current-run.txt")).Trim();if(Route==ConnectionRoute.Internet)RelayRoomConnection.AuthenticationProfile="act-relay-"+Guid.NewGuid().ToString("N").Substring(0,16);ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(Path.Combine(Dir,remote?"client-store":"host-store")));var runner=new GameObject("Boss LAN QA").AddComponent<DevelopmentCoroutineHost>();UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);runner.StartCoroutine(Guard(remote?Remote():Authority()));}
        private static IEnumerator Guard(IEnumerator flow)
        {
            var stack=new System.Collections.Generic.Stack<IEnumerator>();stack.Push(flow);Phase="starting";
            while(stack.Count>0){bool more;object value;try{more=stack.Peek().MoveNext();value=more?stack.Peek().Current:null;}catch(Exception e){Phase="failed";Log(e.ToString());File.WriteAllText(Path.Combine(Dir,client?"client-failed.txt":"host-failed.txt"),e.ToString());if(client)EditorApplication.Exit(1);yield break;}if(!more){stack.Pop();continue;}if(value is IEnumerator sub)stack.Push(sub);else yield return value;}
            Phase="complete";Log("complete");if(client)EditorApplication.Exit(0);
        }
        private static void Log(string text){Directory.CreateDirectory(Dir);File.AppendAllText(Path.Combine(Dir,client?"client.tsv":"host.tsv"),DateTime.UtcNow.ToString("O")+"\t"+Phase+"\t"+text+"\n");}
        private static void Check(bool condition,string text){Log((condition?"PASS ":"FAIL ")+text);if(!condition)throw new InvalidOperationException(text);}
        private static void Command(string command){Phase=command;File.WriteAllText(Path.Combine(Dir,"phase.txt"),command);Log("command");}
        private static string Read(string file){try{using(var stream=new FileStream(Path.Combine(Dir,file),FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))using(var reader=new StreamReader(stream))return reader.ReadToEnd().Trim();}catch(IOException){return "";}}
        private static void Ack(string stage){File.WriteAllText(Path.Combine(Dir,stage+".ack"),"ok");Log("ack "+stage);}
        private static IEnumerator Wait(Func<bool> predicate,string label,float seconds=50)
        {var end=Time.realtimeSinceStartup+(label=="join_first"?180:seconds);while(Time.realtimeSinceStartup<end){if(predicate())yield break;if(!client && File.Exists(Path.Combine(Dir,"client-failed.txt")))throw new InvalidOperationException(Read("client-failed.txt"));yield return null;}throw new TimeoutException(label+"; tomb="+(Tomb!=null)+" unlocked="+(Tomb!=null && Tomb.Unlocked)+" local="+(Local!=null)+" bone="+(Local==null?-1:Bag(Local).Model.CountItem(Local.Catalog.Item("bone"))));}
        private static IEnumerator Acknowledged(string stage){yield return Wait(()=>File.Exists(Path.Combine(Dir,stage+".ack")),stage);}
        private static IEnumerator Shot(string name){if(Application.isBatchMode)yield break;yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(Dir,name+".png"));yield return null;}
        private static void Profile(string profile,string selected,params string[] nodes)
        {var data=AccountProgression.New();foreach(var p in data.Classes){p.Owned=true;p.XP=1800;}data.Selected=selected;data.Find(selected).Nodes=nodes;Check(ClassProfiles.Store.Write(profile,data),"QA admission profile "+selected);}
        private static NetworkPlayer Local=>NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject && p.IsOwner);
        private static NetworkPlayer Other=>NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned && p.NetworkObject.IsPlayerObject && !p.IsOwner);
        private static PlayerInventory Bag(NetworkPlayer p)=>p.GetComponent<PlayerInventory>();
        private static void Freeze()
        {var w=NetworkCreatureWorld.Active;w.Clock.enabled=w.Animals.enabled=w.Goblins.enabled=w.Night.enabled=w.World.Renewal.enabled=false;foreach(var p in NetworkPlayer.Active){p.Motor.enabled=p.GetComponent<PlayerInputAdapter>().enabled=false;p.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;var survival=p.GetComponent<PlayerSurvival>();survival.Definition=UnityEngine.Object.Instantiate(survival.Definition);survival.Definition.RegenPerSecond=0;}}
        private static void Move(NetworkPlayer p,Vector2 at){p.GetComponent<Rigidbody2D>().position=at;p.transform.position=at;p.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
        private static void Give(NetworkPlayer p,string id,int count=1){var item=p.Catalog.Item(id);Bag(p).Model.Add(item,count);for(var i=0;i<9;i++)if(Bag(p).Model.GetSlot(i).Item==item){Bag(p).SelectSlot(i);break;}}
        private static IEnumerator NewHost(bool resume)
        {
            if(session==null)session=NetworkSession.Instance??UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<NetworkSession>(MultiplayerSceneBuilder.SessionPrefabPath));session.ClassVerifier=new DevelopmentAlphaClassAdmission();session.ProfileOverride=HostId;session.WorldRepository=new SaveRepository(Path.Combine(Dir,"world.json"));
            var task=session.Host(Route,resume);yield return Wait(()=>task.IsCompleted,"Host transport");Check(task.Result,"actual "+Route+" Host "+session.LastFailure);if(Route==ConnectionRoute.Internet){File.WriteAllText(Path.Combine(Dir,"room-code.txt"),session.RoomCode);Log("Relay authenticated QA player "+Unity.Services.Authentication.AuthenticationService.Instance.PlayerId);}Check(session.EnterGame(),"Host enters world");yield return Wait(()=>Local!=null && NetworkCreatureWorld.Active?.Ready==true,"Host actor");Freeze();
        }
        private static IEnumerator Join()
        {
            session=NetworkSession.Instance??UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<NetworkSession>(MultiplayerSceneBuilder.SessionPrefabPath));session.ProfileOverride=ClientId;var task=session.Join(Route,Route==ConnectionRoute.Internet?Read("room-code.txt"):"127.0.0.1");yield return Wait(()=>task.IsCompleted,"client transport");Check(task.Result,"actual "+Route+" join "+session.LastFailure);if(Route==ConnectionRoute.Internet)Log("Relay authenticated QA player "+Unity.Services.Authentication.AuthenticationService.Instance.PlayerId);yield return Wait(()=>Local!=null && Other!=null && NetworkCreatureWorld.Active?.Ready==true,"client two actors");yield return Wait(()=>Local.GetComponent<PlayerClass>().ClassXP>=1800,"initial/reconnected transient ClassXP snapshot");Check(Local.GetComponent<PlayerClass>().Definition.Id=="explorer","admitted class snapshot preserved on join");Local.GetComponent<PlayerInputAdapter>().enabled=false;yield return null;
        }
        private static IEnumerator Leave()
        {if(session!=null && session.State!=NetworkSessionState.Idle){session.Leave(true);yield return Wait(()=>session.State==NetworkSessionState.Idle,"leave");}yield return null;yield return null;}

        private static BossWorld World=>NetworkCreatureWorld.Active.World.GetComponent<BossWorld>();
        private static BossEncounter Boss=>World.Encounters.FirstOrDefault();
        private static RoyalTomb Tomb=>UnityEngine.Object.FindFirstObjectByType<RoyalTomb>();
        [Serializable]private sealed class TelegraphSample{public int Attack;public double Started;public Vector2 Direction;}
        private static bool sustain;
        private static IEnumerator Delay(float seconds)
        {var end=Time.time+seconds;while(Time.time<end){if(sustain && !client)foreach(var p in NetworkPlayer.Active)if(p.Health.IsAlive)p.Health.Heal(100);yield return null;}}
        private static IEnumerator Remote()
        {
            yield return SceneManager.LoadSceneAsync(SceneFlow.HomeScene);yield return null;Profile(ClientId,"explorer");string done="";
            var end=Time.realtimeSinceStartup+700;
            while(Time.realtimeSinceStartup<end)
            {
                var stage=Read("phase.txt");if(stage==done || string.IsNullOrEmpty(stage)){yield return null;continue;}done=stage;Phase=stage;
                if(stage.StartsWith("join_")){yield return Join();Ack(stage);}
                else if(stage.StartsWith("resources_")){yield return ResourceReplicationQa.Receive(stage,Dir);Ack(stage);}
                else if(stage=="rollback")
                {yield return Wait(()=>Tomb!=null && Tomb.Unlocked && Bag(Local).Model.CountItem(Local.Catalog.Item("bone"))==30,"rollback inventory replica");yield return Delay(.3f);Check(Boss==null && !World.Record("undead_king").Active && World.Record("undead_king").LastNight==0,"failed offering publishes prior night and inactive mirror");Ack(stage);}
                else if(stage=="summon")
                {
                    yield return Wait(()=>Tomb!=null && Tomb.Unlocked && NetworkCreatureWorld.Active.Clock.Model.Phase==DayPhase.Night,"client sees unlocked night tomb");
                    yield return Wait(()=>Bag(Local).Model.CountItem(Local.Catalog.Item("bone"))==30,"offering bag replica");
                    Check(Tomb.Use(Local.GetComponent<PlayerInteraction>()),"owner sends existing facility summon RPC");
                    yield return Wait(()=>Boss!=null && Boss.Health.Current==9000,"one replicated full HP boss");
                    yield return Wait(()=>Bag(Local).Model.CountItem(Local.Catalog.Item("bone"))==20,"owner Bone10 reply");
                    Check(World.Encounters.Count==1 && Boss.Health.IsReplica,"one boss replica no duplicate");
                    var hp=Boss.Health.Current;Check(Boss.Health.ApplyDamage(500,true)==0,"client damage mutation rejected");Boss.BeginReturn();Check(Boss.State!=BossState.Returning,"client cannot force Returning");Ack(stage);
                }
                else if(stage=="telegraph")
                {var sample=JsonUtility.FromJson<TelegraphSample>(Read("telegraph.json"));yield return Wait(()=>Boss.State==BossState.Windup && Boss.AttackIndex==sample.Attack && Math.Abs(Boss.Started-sample.Started)<.001,"matching server attack and timestamp");Check(Vector2.Distance(Boss.Direction,sample.Direction)<.001f && Boss.GetComponent<BossTelegraph>().Edge.enabled,"replica telegraph geometry uses same direction and authored attack");Ack(stage);}
                else if(stage=="phase_two")
                {
                    yield return Wait(()=>Boss!=null && Boss.Phase==2 && Boss.Marked!=null,"phase and skull replica");
                    yield return Wait(()=>UnityEngine.Object.FindObjectsByType<NetworkCreature>(FindObjectsSortMode.None).Count(c=>c.Member.GroupId=="encounter:undead_king:1" && c.Health.IsAlive)==3,"three actual replicated adds");
                    Check(Boss.Health.Maximum==9000 && Boss.Definition.Speed==2.1f,"2P no HP or speed scaling");
                    File.WriteAllText(Path.Combine(Dir,"marked.txt"),Boss.Marked.GetComponent<NetworkPlayer>().ProfileId);Ack(stage);
                }
                else if(stage=="transfer")
                {
                    yield return Wait(()=>Boss.Marked!=null && Boss.Marked.IsAlive && Boss.Marked.GetComponent<NetworkPlayer>().ProfileId!=Read("marked.txt"),"mark transferred off incapacitated player");Ack(stage);
                }
                else if(stage=="returning")
                {yield return Wait(()=>Boss.State==BossState.Returning && Boss.Marked==null,"party wipe Returning / mark cleanup replica");Ack(stage);}
                else if(stage=="accessory")
                {
                    yield return Wait(()=>Bag(Local).Model.CountItem(Local.Catalog.Item("undead_kings_crown"))==1,"crown inventory replica");
                    Local.Commands.Session.OpenInventory();yield return null;Check(Local.GetComponent<PlayerEquipment>().TryEquip(Local.Catalog.Item("undead_kings_crown")),"owner accessory RPC queued");
                    yield return Wait(()=>Local.GetComponent<PlayerEquipment>().Accessory!=null,"owner accessory equip acknowledged");Local.Commands.Session.CloseInventory();Ack(stage);
                }
                else if(stage=="reward")
                {yield return Wait(()=>Boss==null && Local.GetComponent<PlayerProgression>().Experience>=120 && Local.GetComponent<PlayerProgression>().Gold>=35 && Local.GetComponent<PlayerClass>().ClassXP>=1920,"eligible common reward and transient ClassXP replica once");Ack(stage);}
                else if(stage=="pickup_core")
                {var id=ulong.Parse(Read("core-pickup-id.txt"));yield return Wait(()=>Bag(Local).Model.CountItem(Local.Catalog.Item("boss_core_t1"))==1 || session.Manager.SpawnManager.SpawnedObjects.ContainsKey(id),"core replica or already acknowledged pickup");if(Bag(Local).Model.CountItem(Local.Catalog.Item("boss_core_t1"))==0)Local.GetComponent<NetworkInventory>().Pickup(id);yield return Wait(()=>Bag(Local).Model.CountItem(Local.Catalog.Item("boss_core_t1"))==1,"owner pickup request acknowledged by Host");Ack(stage);}
                else if(stage=="leave" || stage=="resource_disconnect"){yield return Leave();Ack(stage);}
                else if(stage=="resume")
                {yield return Wait(()=>Local.GetComponent<PlayerEquipment>().Accessory!=null,"accessory restored on reHost");Check(Boss==null,"resolved encounter no respawn");Ack(stage);}
                else if(stage=="finish"){Ack(stage);yield return Leave();yield break;}
                yield return null;
            }
            throw new TimeoutException("boss LAN client stages");
        }
        private static long Total(string id)
        {long n=0;foreach(var p in NetworkPlayer.Active)n+=Bag(p).Model.CountItem(p.Catalog.Item(id));foreach(var p in WorldPickup.Active)if(p.Item!=null && p.Item.Id==id)n+=p.Count;foreach(var p in NetworkCreatureWorld.Active.World.Chunks.DormantPickups())if(p.Item==id)n+=p.Count;return n;}
        private static IEnumerator Authority()
        {
            Profile(HostId,"explorer");yield return NewHost(false);Command("join_first");yield return Acknowledged("join_first");yield return Wait(()=>Other!=null,"two actual processes ready");Freeze();
            yield return ResourceReplicationQa.Exercise(NetworkCreatureWorld.Active,()=>Local,()=>Other,Dir,Command,Acknowledged);
            Command("resource_disconnect");yield return Acknowledged("resource_disconnect");yield return Wait(()=>Other==null,"resource peer disconnected");Command("join_resource");yield return Acknowledged("join_resource");yield return Wait(()=>Other!=null,"resource peer rejoined");Freeze();yield return ResourceReplicationQa.Probe("resources_late_join",()=>Other,Dir,Command,Acknowledged);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkUndeadKing.prefab").GetComponent<NetworkBoss>();Check(Math.Abs(prefab.Clock-session.Manager.ServerTime.Time)<.1,"pre-spawn boss clock uses server epoch");
            var w=NetworkCreatureWorld.Active;
            var a=w.World.Map.Continental.Anchors.First(x=>x.Kind==WorldAnchorKind.Graveyard);Move(Local,a.Position+Vector2.down*3);Move(Other,a.Position+Vector2.down*2);
            w.World.Chunks.Reconcile();w.World.Chunks.DrainPending();yield return Delay(.5f);Check(Tomb!=null && !Tomb.Unlocked,"existing guardian seals network tomb");
            var guard=UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Single(e=>e.PersistentGuard);guard.OwnHealth.ApplyDamage(guard.OwnHealth.Current,true,Bag(Local),Local.Health);yield return Delay(.5f);Check(Tomb.Unlocked,"Host guardian unlock persisted flag");
            Give(Other,"bone",30);w.Clock.Model.SetElapsed(301);sustain=true;
            var good=session.WorldRepository;var blocked=Path.Combine(Dir,"blocked");File.WriteAllText(blocked,"QA blocks directory creation");session.WorldRepository=new SaveRepository(Path.Combine(blocked,"world.json"));
            Check(!World.Summon(Tomb,Bag(Other),1) && Boss==null && World.Record("undead_king").LastNight==0 && Bag(Other).Model.CountItem(Other.Catalog.Item("bone"))==30,"failed network offering rolls back actor, Bone and night");session.WorldRepository=good;Command("rollback");yield return Acknowledged("rollback");
            Command("summon");yield return Acknowledged("summon");Check(Boss!=null && World.Encounters.Count==1 && Boss.Health.Current==9000,"one authoritative full HP spawn");Check(!World.Summon(Tomb,Bag(Other),1),"duplicate offering blocked globally");
            yield return Wait(()=>Boss.State!=BossState.Awakening,"network awakening finishes in authored time",6);
            yield return Wait(()=>Boss.State==BossState.Windup && Boss.Age<.25f,"first readable melee windup");File.WriteAllText(Path.Combine(Dir,"telegraph.json"),JsonUtility.ToJson(new TelegraphSample{Attack=Boss.AttackIndex,Started=Boss.Started,Direction=Boss.Direction}));Command("telegraph");yield return Acknowledged("telegraph");
            Boss.Health.ApplyDamage(2700,true,Bag(Local),Local.Health);yield return Delay(2);Check(Boss.Phase==2 && Boss.LivingAdds==3,"2P authored three adds state="+Boss.State+" hp="+Boss.Health.Current+" living="+World.Players.Count);Command("phase_two");yield return Acknowledged("phase_two");
            var marked=Boss.Marked;marked.ApplyDamage(10000,true);yield return Delay(.4f);Check(Boss.Marked!=null && Boss.Marked!=marked && Boss.Marked.IsAlive,"server transfers mark after Downed");Command("transfer");yield return Acknowledged("transfer");
            sustain=false;foreach(var p in NetworkPlayer.Active)if(p.Health.IsAlive)p.Health.ApplyDamage(10000,true);yield return Delay(4.7f);Check(Boss.State==BossState.Returning && Boss.LivingAdds==0 && Boss.Marked==null,"full party wipe retires wave then Returning");
            var prior=Boss.Health.Current;yield return Delay(3);Check(Boss.Health.Current>prior && Boss.Health.Current<9000,"gradual multiplayer return regen");Command("returning");yield return Acknowledged("returning");
            foreach(var p in NetworkPlayer.Active){var d=p.GetComponent<PlayerDeath>();if(d.State==PlayerLife.Downed)d.BecomeDead();Check(d.HomeRespawn(),"existing MP Home respawn");Move(p,Boss.Home+Vector2.down*(p.IsOwner?4:3));}
            sustain=true;yield return Delay(.7f);Check(Boss.State!=BossState.Returning,"living re-entry resumes server fight");
            Give(Other,"undead_kings_crown");Command("accessory");yield return Acknowledged("accessory");Check(Other.GetComponent<PlayerEquipment>().Accessory!=null && Other.GetComponentInChildren<AccessoryVisual>()!=null,"remote accessory state and shared visual anchor");
            var exp=NetworkPlayer.Active.ToDictionary(p=>p.ProfileId,p=>p.GetComponent<PlayerProgression>().Experience);var gold=NetworkPlayer.Active.ToDictionary(p=>p.ProfileId,p=>p.GetComponent<PlayerProgression>().Gold);
            var classXp=NetworkPlayer.Active.ToDictionary(p=>p.ProfileId,p=>p.GetComponent<PlayerClass>().ClassXP);var remoteBytes=File.ReadAllBytes(Path.Combine(Dir,"client-store","class-"+ClientId+".json"));
            var core=Total("boss_core_t1");var crown=Total("undead_kings_crown");Boss.Health.ApplyDamage(Boss.Health.Current,true,Bag(Local),Local.Health);yield return Delay(3.7f);
            Check(Boss==null && Total("boss_core_t1")==core+1,"one shared guaranteed core network set");Check(Total("undead_kings_crown")-crown<=1,"one crown roll shared at most one");
            foreach(var p in NetworkPlayer.Active){Check(p.GetComponent<PlayerProgression>().Experience==exp[p.ProfileId]+120 && p.GetComponent<PlayerProgression>().Gold==gold[p.ProfileId]+35,"living eligible reward exactly once "+p.ProfileId);}
            foreach(var p in NetworkPlayer.Active)Check(p.GetComponent<PlayerClass>().ClassXP==classXp[p.ProfileId]+120,"class XP once in persistent Host or session-only peer "+p.ProfileId);
            Check(ClassProfiles.Store.Read(HostId).PendingXP==null && ClassProfiles.Store.Read(HostId).Find("explorer").XP==classXp[HostId]+120,"Host Profile2 reward committed without pending intent");Check(File.ReadAllBytes(Path.Combine(Dir,"client-store","class-"+ClientId+".json")).SequenceEqual(remoteBytes),"remote verifier progression never writes peer persistent profile");
            Command("reward");yield return Acknowledged("reward");
            var groundCore=WorldPickup.Active.FirstOrDefault(p=>p.Item!=null && p.Item.Id=="boss_core_t1");if(groundCore!=null){var item=groundCore.GetComponent<NetworkGroundItem>();File.WriteAllText(Path.Combine(Dir,"core-pickup-id.txt"),item.NetworkObjectId.ToString());Move(Local,(Vector2)groundCore.transform.position+Vector2.up*6);Move(Other,groundCore.transform.position);Command("pickup_core");yield return Acknowledged("pickup_core");Check(Bag(Other).Model.CountItem(Other.Catalog.Item("boss_core_t1"))==1 && Total("boss_core_t1")==core+1,"authoritative owner pickup keeps exactly one physical core");}
            Check(session.TrySaveWorld(),"Host Save14 boss reward/accessory");yield return Shot("boss-two-process-rewards");
            Command("leave");yield return Acknowledged("leave");yield return Wait(()=>Other==null,"client left");yield return Leave();yield return NewHost(true);Command("join_resumed");yield return Acknowledged("join_resumed");yield return Wait(()=>Other!=null,"reHost actual peer");Freeze();
            Check(Boss==null && World.Record("undead_king").LastNight==1,"reHost retains consumed night and resolved reward");Check(Total("boss_core_t1")==core+1,"reHost no physical loot duplicate");
            Check(NetworkCreatureWorld.Active.World.Renewal.Capture().Any(r=>r.Id==Read("harvested-resource.txt") && r.Destroyed),"harvest tombstone survives save/reHost");yield return ResourceReplicationQa.Probe("resources_rehost",()=>Other,Dir,Command,Acknowledged);
            var oldTomb=World.Record("undead_king").Tomb;var anchor=NetworkCreatureWorld.Active.World.Map.Continental.Anchors.First(x=>x.Kind==WorldAnchorKind.Graveyard && x.Id==oldTomb);Move(Local,anchor.Position+Vector2.down*3);Move(Other,anchor.Position+Vector2.down*2);NetworkCreatureWorld.Active.World.Chunks.Reconcile();NetworkCreatureWorld.Active.World.Chunks.DrainPending();yield return Delay(.5f);
            var bones=Bag(Other).Model.CountItem(Other.Catalog.Item("bone"));Check(Tomb!=null && Tomb.Unlocked && !World.Summon(Tomb,Bag(Other),1) && Bag(Other).Model.CountItem(Other.Catalog.Item("bone"))==bones,"resolved reHost same-night offering rejects without consuming Bone");
            Command("resume");yield return Acknowledged("resume");Command("finish");yield return Acknowledged("finish");yield return Wait(()=>Other==null,"client finished");yield return Leave();File.WriteAllText(Path.Combine(Dir,"complete.txt"),"actual two-process boss LAN complete");
        }
    }
}
