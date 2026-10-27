using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
namespace Wildfolk.Editor
{
    // Real Development Auth/Cloud Code and real NGO transport, always in isolated batch projects.
    [InitializeOnLoad]public static class PremiumMultiplayerQa
    {
        private static string Dir=>Environment.GetEnvironmentVariable("LOWFIRE_PREMIUM_QA_DIR")??throw new InvalidOperationException("Explicit isolated QA directory required");
        private static string role;private static AccountService account;private static EconomyService economy;private static NetworkSession session;private static CountingClient proofs;
        private static ConnectionRoute Route=>Environment.GetEnvironmentVariable("LOWFIRE_PREMIUM_ROUTE")=="Internet"?ConnectionRoute.Internet:ConnectionRoute.LocalNetwork;
        static PremiumMultiplayerQa(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode){var r=UnityEditor.SessionState.GetString("Lowfire.PremiumQA.Role","");if(r!=""){role=r;UnityEditor.SessionState.EraseString("Lowfire.PremiumQA.Role");UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",UnityEditor.SessionState.GetString("Lowfire.PremiumQA.Profile",""));var runner=new GameObject("Premium isolated QA").AddComponent<DevelopmentCoroutineHost>();UnityEngine.Object.DontDestroyOnLoad(runner);runner.StartCoroutine(Guard(Run()));}}};}
        public static void HostBatch()=>Bootstrap("host");public static void ClientBatch()=>Bootstrap("client");
        private static void Bootstrap(string r){if(!Application.isBatchMode||BackendPolicy.Channel!=BuildChannel.Development)throw new InvalidOperationException("Development batch only");Directory.CreateDirectory(Dir);UnityEditor.SessionState.SetString("Lowfire.PremiumQA.Role",r);UnityEditor.SessionState.SetString("Lowfire.PremiumQA.Profile",Path.Combine(Path.GetTempPath(),"LowfirePremiumQA",r+Guid.NewGuid().ToString("N").Substring(0,8)));UnityEditor.SceneManagement.EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);Application.runInBackground=true;EditorApplication.update+=()=>{if(EditorApplication.isPlaying)EditorApplication.QueuePlayerLoopUpdate();};EditorApplication.isPlaying=true;}
        private sealed class Identity:IAccountIdentityProvider{public string PlayerId=>AuthenticationService.Instance.PlayerId;public bool Linked=>false;public Task Connect()=>Task.CompletedTask;public Task UseExisting()=>Connect();public void SignOut(bool clearSession=false)=>AuthenticationService.Instance.SignOut(clearSession);}
        private sealed class CountingClient:IPremiumAdmissionClient{
            private readonly PremiumAdmissionClient real=new PremiumAdmissionClient();public int Issues,Verifications;public PremiumAdmissionSnapshot Last;
            public bool Available=>real.Available;public string PlayerId=>real.PlayerId;
            public async Task<PremiumAdmissionSnapshot> Issue(PremiumChallenge c){Issues++;return await real.Issue(c);}
            public async Task<PremiumAdmissionSnapshot> Verify(PremiumChallenge c,string player,string proof){Verifications++;Last=await real.Verify(c,player,proof);return Last;}
        }
        private static IEnumerator Guard(IEnumerator flow){var stack=new Stack<IEnumerator>();stack.Push(flow);while(stack.Count>0){bool more;object current;try{more=stack.Peek().MoveNext();current=more?stack.Peek().Current:null;}catch(Exception e){File.WriteAllText(Path.Combine(Dir,role+"-failed.txt"),e.GetType().Name+": "+e.Message+"\n"+e.StackTrace);try{AuthenticationService.Instance.SignOut(true);}catch{}EditorApplication.Exit(1);yield break;}if(!more){stack.Pop();continue;}if(current is IEnumerator sub)stack.Push(sub);else yield return current;}AuthenticationService.Instance.SignOut(true);File.WriteAllText(Path.Combine(Dir,role+"-complete.txt"),"ok");EditorApplication.Exit(0);}
        private static void Check(bool value,string label){File.AppendAllText(Path.Combine(Dir,role+"-checks.txt"),(value?"PASS ":"FAIL ")+label+"\n");if(!value)throw new InvalidOperationException(label);}
        private static IEnumerator Wait(Func<bool> done,string label,float seconds=100){var end=Time.realtimeSinceStartupAsDouble+seconds;while(!done()){if(Time.realtimeSinceStartupAsDouble>end)throw new TimeoutException(label);var other=Path.Combine(Dir,role=="host"?"client-failed.txt":"host-failed.txt");if(File.Exists(other))throw new InvalidOperationException("peer failed: "+File.ReadAllText(other));yield return null;}}
        private static IEnumerator Await(Task t){yield return Wait(()=>t.IsCompleted,"cloud task");if(t.IsFaulted)throw new InvalidOperationException("Cloud QA task failed");}
        private static string Read(string name){var file=Path.Combine(Dir,name);try{return File.Exists(file)?SharedRead(file):"";}catch(IOException){return "";}}
        private static string SharedRead(string file){using(var input=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))using(var reader=new StreamReader(input))return reader.ReadToEnd().Trim();}
        private static void Command(string name){var file=Path.Combine(Dir,"phase.txt");var temporary=file+".next";File.WriteAllText(temporary,name);if(File.Exists(file))File.Replace(temporary,file,null);else File.Move(temporary,file);}
        private static void Ack(string name)=>File.WriteAllText(Path.Combine(Dir,name+".ack"),"ok");
        private static NetworkPlayer Local=>NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned&&p.IsOwner);
        private static NetworkPlayer Remote=>NetworkPlayer.Active.FirstOrDefault(p=>p.IsSpawned&&!p.IsOwner);
        private static IEnumerator PrepareAccount(bool unowned=false){
            if(unowned){AuthenticationService.Instance.SignOut(true);AuthenticationService.Instance.SwitchProfile("premium-unowned-"+Guid.NewGuid().ToString("N").Substring(0,12));yield return Await(AuthenticationService.Instance.SignInAnonymouslyAsync());}
            else yield return Await(UgsBootstrap.EnsureAnonymous("premium-qa-"+Guid.NewGuid().ToString("N").Substring(0,12)));
            File.WriteAllText(Path.Combine(Dir,role+(unowned?"-unowned":"")+"-player-id.txt"),AuthenticationService.Instance.PlayerId);
            ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(Path.Combine(DevelopmentProfile.Directory,"guest-"+Guid.NewGuid().ToString("N"))));account=AccountService.InstallDevelopment(new Identity(),new UgsAccountCloudTransport());economy=EconomyService.InstallDevelopment(account,new UgsEconomyTransport());yield return Await(account.Connect());Check(account.CloudAvailable,"actual isolated Development account connected" );yield return Await(economy.Sync());Check(economy.Snapshot?.Balance==0,"fresh actual wallet0");
        }
        private static void Freeze(){var w=NetworkCreatureWorld.Active;if(w!=null){w.Clock.enabled=w.Animals.enabled=w.Goblins.enabled=w.Night.enabled=w.World.Renewal.enabled=false;}foreach(var p in NetworkPlayer.Active){p.GetComponent<PlayerInputAdapter>().enabled=false;p.GetComponent<PlayerSurvival>().enabled=false;}}
        private static IEnumerator Leave(){if(session!=null&&session.State!=NetworkSessionState.Idle){session.Leave(true);yield return Wait(()=>session.State==NetworkSessionState.Idle,"leave");}yield return Wait(()=>SceneManager.GetActiveScene().name==SceneFlow.HomeScene,"home scene");yield return null;}
        private static IEnumerator Host(bool resume){var t=session.Host(Route,resume);yield return Await(t);Check(t.Result,"actual "+Route+" Host");File.WriteAllText(Path.Combine(Dir,"destination.txt"),Route==ConnectionRoute.Internet?session.RoomCode:"127.0.0.1");Check(session.EnterGame(),"Host enters isolated world");yield return Wait(()=>Local!=null&&NetworkCreatureWorld.Active?.Ready==true,"host actor");Freeze();}
        private static IEnumerator Join(){var t=session.Join(Route,Read("destination.txt"));yield return Await(t);Check(t.Result,"actual "+Route+" Join");yield return Wait(()=>Local!=null&&Remote!=null&&NetworkCreatureWorld.Active?.Ready==true,"client actors");Freeze();yield return null;}
        private static IEnumerator Run(){
            yield return null;Check(DevelopmentProfile.Isolated,"isolated QA save path");yield return PrepareAccount();
            yield return Wait(()=>File.Exists(Path.Combine(Dir,"funded.txt")),"QA grant preparation",180);yield return Await(economy.Sync());
            if(role=="client"){var unlock=economy.UnlockEngineer();yield return Await(unlock);Check(unlock.Result&&economy.Snapshot.Balance==0,"actual Engineer120 debit+account ownership");Check(ClassProfiles.Select("engineer"),"select owned Engineer before join");}
            session=NetworkSession.Instance??UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<NetworkSession>(MultiplayerSceneBuilder.SessionPrefabPath));session.ProfileOverride=role=="host"?"cccccccccccccccccccccccccccccccc":"dddddddddddddddddddddddddddddddd";
            // Isolated default routing GUID is deliberately shared by the legacy harness.
            // Copy only this QA account's loadout into its distinct participant route.
            if(role=="client")Check(ClassProfiles.Store.Write(session.ProfileOverride,ClassProfiles.Local),"isolated distinct client participant route");
            session.WorldRepository=new SaveRepository(Path.Combine(DevelopmentProfile.Directory,"qa-world.json"));proofs=new CountingClient();session.DevelopmentPremiumClient(proofs);File.WriteAllText(Path.Combine(Dir,role+"-prepared.txt"),"ok");
            if(role=="host")yield return Authority();else yield return Client();
        }
        private static IEnumerator Authority(){
            yield return Wait(()=>File.Exists(Path.Combine(Dir,"client-prepared.txt")),"client preparation");yield return Host(false);Command("join-owned");yield return Wait(()=>Read("join-owned.ack")=="ok"&&Remote!=null,"owned join");Freeze();
            Check(Remote.GetComponent<PlayerClass>().Definition.Id=="engineer"&&!Remote.GetComponent<PlayerClass>().PersistentProgression,"remote Engineer admitted by actual Cloud Code proof");var first=proofs.Last;Check(first!=null&&proofs.Verifications==1,"one backend verification on admission");File.WriteAllText(Path.Combine(Dir,"first-admission.json"),JsonUtility.ToJson(first,true));
            Check(Remote.GetComponent<PlayerClass>().GrantXP(8,"qa-session-only"),"remote session ClassXP grant");Command("leave-owned");yield return Wait(()=>Read("leave-owned.ack")=="ok"&&Remote==null,"client left");Command("rejoin-owned");yield return Wait(()=>Read("rejoin-owned.ack")=="ok"&&Remote!=null,"client rejoin");Freeze();
            Check(Remote.GetComponent<PlayerClass>().ClassXP==8&&proofs.Verifications==2&&proofs.Last.AdmissionId!=first.AdmissionId&&proofs.Last.SessionId==first.SessionId,"same-session reconnect reverified with new nonce and preserved XP");
            yield return Leave();yield return Wait(()=>Read("host-closed.ack")=="ok","peer sees host closure");yield return Host(true);Command("fresh-session");yield return Wait(()=>Read("fresh-session.ack")=="ok"&&Remote!=null,"fresh session join");Freeze();
            Check(Remote.GetComponent<PlayerClass>().Definition.Id=="engineer"&&Remote.GetComponent<PlayerClass>().ClassXP==0&&proofs.Last.SessionId!=first.SessionId,"Continue/reHost uses fresh session proof and remote XP remains ephemeral");
            Command("offline-rejoin");yield return Wait(()=>Read("offline-rejoin.ack")=="ok"&&Remote!=null,"offline fallback");Freeze();Check(Remote.GetComponent<PlayerClass>().Definition.Id=="survivor","offline premium fallback keeps join available");
            Command("unowned-rejoin");yield return Wait(()=>Read("unowned-rejoin.ack")=="ok"&&Remote!=null,"unowned fallback");Freeze();Check(Remote.GetComponent<PlayerClass>().Definition.Id=="survivor","new unowned backend account cannot inherit previous Engineer");
            Command("finish-client");yield return Wait(()=>Read("finish-client.ack")=="ok"&&Remote==null,"client final leave");yield return Leave();
            var unlock=economy.UnlockEngineer();yield return Await(unlock);Check(unlock.Result,"Host actual Engineer120 unlock");Check(ClassProfiles.Select("engineer"),"Host selects Engineer");yield return Host(true);Check(Local.GetComponent<PlayerClass>().Definition.Id=="engineer"&&Local.GetComponent<PlayerClass>().PersistentProgression,"Host own Engineer uses same actual issuer/verifier boundary");yield return Leave();
            File.WriteAllText(Path.Combine(Dir,"host-summary.json"),JsonUtility.ToJson(new Summary{ActualBackend=true,ActualGoogle=false,FixtureOwnership=false,Protocol=NetworkProtocol.Current,Issues=proofs.Issues,Verifications=proofs.Verifications},true));Command("done");
        }
        [Serializable]private sealed class Summary{public bool ActualBackend,ActualGoogle,FixtureOwnership;public int Protocol,Issues,Verifications;}
        private static IEnumerator Client(){
            string previous="";var end=Time.realtimeSinceStartupAsDouble+600;
            while(Time.realtimeSinceStartupAsDouble<end){var phase=Read("phase.txt");if(phase==previous||phase==""){if(session.State==NetworkSessionState.Idle&&SceneManager.GetActiveScene().name==SceneFlow.HomeScene&&Read("rejoin-owned.ack")=="ok")Ack("host-closed");yield return null;continue;}previous=phase;
                if(phase=="join-owned"||phase=="rejoin-owned"||phase=="fresh-session"){yield return Join();Check(Local.GetComponent<PlayerClass>().Definition.Id=="engineer","owner replica sees verified Engineer "+phase);if(phase=="rejoin-owned")yield return Wait(()=>Local.GetComponent<PlayerClass>().ClassXP==8,"reconnected XP snapshot");Check(ClassProfiles.Local.Find("engineer").XP==0,"remote XP did not write durable account cache");Ack(phase);}
                else if(phase=="leave-owned"){yield return Leave();Ack(phase);}
                else if(phase=="offline-rejoin"){yield return Leave();account.KeepLocal();yield return Join();Check(Local.GetComponent<PlayerClass>().Definition.Id=="survivor"&&session.PremiumMessageKey=="net.premium_unavailable","offline fallback localized and join succeeds");Ack(phase);}
                else if(phase=="unowned-rejoin"){yield return Leave();yield return PrepareAccount(true);ClassProfiles.Update(ClassProfiles.LocalId,p=>{p.Find("engineer").Owned=true;p.Selected="engineer";return true;});yield return Join();Check(Local.GetComponent<PlayerClass>().Definition.Id=="survivor","forged local Engineer flag cannot replace actual server ownership");Ack(phase);}
                else if(phase=="finish-client"){yield return Leave();Ack(phase);}
                else if(phase=="done")yield break;
            }throw new TimeoutException("premium QA flow");
        }
    }
}
