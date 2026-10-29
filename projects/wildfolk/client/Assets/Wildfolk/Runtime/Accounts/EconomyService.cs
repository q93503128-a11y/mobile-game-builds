using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;
namespace Wildfolk
{
    public interface IEconomyTransport {Task<EconomyReply> Call(string operation,Dictionary<string,object> input);}
    public sealed class UgsEconomyTransport:IEconomyTransport
    {
        public async Task<EconomyReply> Call(string operation,Dictionary<string,object> input)
        {
            UgsBootstrap.VerifyEnvironment();
            for(var attempt=0;attempt<3;attempt++)try{
                if(operation=="SubmitProgress"&&BackendPolicy.Channel!=BuildChannel.Development){if(AndroidAccountBridge.IntegrityToken==null)throw new BackendSetupException("integrity_required");input["integrityToken"]=await AndroidAccountBridge.IntegrityToken(AccountRequestHash.ForAlpha(AccountService.Get().Profile.PlayerId,BackendSettings.Current.Environment.Id,(string)input["operationId"],Application.version));}
                var reply=await CloudCodeService.Instance.CallEndpointAsync<EconomyReply>("LowfireEconomy",new Dictionary<string,object>{{"operation",operation},{"channel",BackendPolicy.Channel.ToString()},{"buildVersion",Application.version},{"input",input}});
                if(attempt<2&&(reply.error=="busy"||reply.error=="cloud_unavailable")){await Task.Delay(250*(1<<attempt));continue;}return reply;
            }catch{if(attempt==2)throw;await Task.Delay(250*(1<<attempt));}throw new InvalidOperationException("Cloud unavailable");
        }
    }
    public sealed class EconomyService
    {
        public static EconomyService Current{get;private set;}
        public static EconomyService Get(){var a=AccountService.Get();if(Current!=null&&Current.account==a)return Current;if(Current!=null)Current.account.Changed-=Current.AccountChanged;return Current=new EconomyService(a,new UgsEconomyTransport());}
        public event Action Changed;public EconomySnapshot Snapshot=>journal?.State.Cached;public string LastError{get;private set;}public string LastTransaction{get;private set;}public string LastAchievementClaim{get;private set;}public string LastAttendanceClaim{get;private set;}
        public bool Busy{get;private set;}public bool Online=>account.CloudAvailable&&account.Profile!=null&&(BackendSettings.Current.Enabled||account.IsDevelopment)&&journal!=null&&!blocked;
        public bool CanSpend=>Online&&!Busy&&journal.State.Revive==null&&(Snapshot?.PendingRevives.Length??0)==0;public bool Badge=>Snapshot!=null&&(Snapshot.Claimable||Array.Exists(Snapshot.Boards,b=>b.Available));
        public bool JournalPending=>journal?.State.Revive!=null;
        public bool PendingProgress=>journal!=null&&(journal.State.Mutable.First>0||journal.State.Outbox.Count>0);public int LocalValue(string id)=>journal?.State.Totals.Find(m=>m.Id==id)?.Value??0;
        public double ResetSeconds=>Snapshot==null?0:Math.Max(0,(DateTimeOffset.Parse(Snapshot.NextReset)-DateTimeOffset.Parse(Snapshot.ServerTime)).TotalSeconds-(Time.realtimeSinceStartupAsDouble-anchor));
        private AccountService account;private IEconomyTransport transport;private EconomyJournal journal;private string scope;private int epoch;private bool blocked;private double anchor,nextFlush;private bool syncing;
        public EconomyService(AccountService a,IEconomyTransport t){account=a;transport=t;account.Changed+=AccountChanged;Bind();ReviveServices.InstallCloud(new CloudGemReviveWallet(this));}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static EconomyService InstallDevelopment(AccountService a,IEconomyTransport t){if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures||!a.IsDevelopment)throw new InvalidOperationException("Isolated Development only");if(Current!=null)Current.account.Changed-=Current.AccountChanged;return Current=new EconomyService(a,t);}
#endif
        private void Bind()
        {
            var player=account.Profile?.PlayerId;var next=BackendSettings.Current.Environment.Id+":"+(player??("guest:"+ClassProfiles.LocalId));if(next==scope)return;scope=next;epoch++;journal=null;blocked=false;
            try{journal=EconomyJournal.Open(scope);if(player!=null)journal.Adopt(EconomyJournal.Open(BackendSettings.Current.Environment.Id+":guest:"+ClassProfiles.LocalId));if(journal.State.Cached!=null)journal.State.Cached.Validate(player,BackendSettings.Current.Environment.Id);anchor=Time.realtimeSinceStartupAsDouble;}
            catch(Exception){journal=null;blocked=true;LastError="local_save_failed";}
        }
        private void AccountChanged(){if(account.State==AccountConnection.DeletionPending){try{var guest=EconomyJournal.Open(BackendSettings.Current.Environment.Id+":guest:"+ClassProfiles.LocalId);if(guest.State.AdoptedBy==scope)guest.Clear();journal?.Clear();}catch{}journal=null;scope=null;epoch++;}else Bind();Changed?.Invoke();}
        public void Record(string metric,int amount,string source){try{Bind();if(journal?.Record(metric,amount,source)==true)Changed?.Invoke();}catch(Exception){blocked=true;LastError="local_save_failed";Changed?.Invoke();}}
        public async Task Pulse(){if(Time.realtimeSinceStartupAsDouble<nextFlush||syncing)return;nextFlush=Time.realtimeSinceStartupAsDouble+300;if(Online)await Sync();}
        private async Task<EconomyReply> Call(string operation,Dictionary<string,object> input,bool mutation=true)
        {
            if(!Online||Busy)return new EconomyReply{error="online_required"};Busy=true;Changed?.Invoke();var owner=epoch;
            try{if(mutation&&!input.ContainsKey("operationId"))input["operationId"]=Guid.NewGuid().ToString("N");var beforeCompleted=Snapshot?.completed??0;var r=await transport.Call(operation,input);if(owner!=epoch)return new EconomyReply{error="account_changed"};
                if(r.snapshot!=null){r.snapshot.Validate(account.Profile.PlayerId,BackendSettings.Current.Environment.Id);journal.Cache(r.snapshot);anchor=Time.realtimeSinceStartupAsDouble;}
                if(r.ok&&operation=="SubmitProgress"&&r.snapshot!=null&&r.snapshot.completed>beforeCompleted)AudioTransactions.Confirmed(scope+":achievement:"+(string)input["operationId"],Sound.Achievement);if(!r.ok&&(operation=="ClaimAll"||operation=="ClaimAchievement"||operation=="ClaimAttendance"||operation=="UnlockEngineer"))AudioRuntime.Play(Sound.UiError);LastError=r.ok?null:r.error;LastTransaction=r.transactionId??r.revive?.TransactionId??LastTransaction;if(r.ok&&!r.alreadyClaimed){if(operation=="ClaimAttendance"&&r.claimedDay>0)AudioTransactions.Confirmed(scope+":"+(r.transactionId??(string)input["operationId"]),r.granted>0?Sound.Gem:Sound.Attendance);else if((operation=="ClaimAchievement"||operation=="ClaimAll")&&r.claimedCount>0)AudioTransactions.Confirmed(scope+":"+(r.transactionId??(string)input["operationId"]),r.granted>0?Sound.Gem:Sound.Achievement);}return r;
            }catch(Exception){LastError="cloud_unavailable";return new EconomyReply{error=LastError};}
            finally{Busy=false;Changed?.Invoke();}
        }
        public async Task Sync()
        {
            if(syncing||!Online||Busy)return;syncing=true;var owner=epoch;
            try{journal.Freeze();var queued=journal.State.Outbox.Count;for(var index=0;index<queued&&owner==epoch;index++){var b=journal.State.Outbox[0];var r=await Call("SubmitProgress",new Dictionary<string,object>{{"operationId",b.Operation},{"device",journal.State.Device},{"first",b.First},{"last",b.Last},{"metrics",b.Metrics}});if(!r.ok||owner!=epoch)return;if(r.acknowledged<b.Last){LastError="invalid_progress_ack";return;}journal.Acknowledge(r.acknowledged);}if(owner==epoch)await Call("GetEconomy",new Dictionary<string,object>(),false);}
            catch(Exception){LastError="local_save_failed";blocked=true;}finally{syncing=false;Changed?.Invoke();}
        }
        public async Task<EconomyReply> Claim(string family=null){await Sync();var r=await Call(family==null?"ClaimAll":"ClaimAchievement",family==null?new Dictionary<string,object>():new Dictionary<string,object>{{"family",family}});if(r.ok)LastAchievementClaim=family??"all";return r;}
        public async Task<EconomyReply> Attend(string board){var r=await Call("ClaimAttendance",new Dictionary<string,object>{{"board",board}});if(r.ok)LastAttendanceClaim=board;return r;}
        public async Task<bool> UnlockEngineer(){if(!CanSpend)return false;var r=await Call("UnlockEngineer",new Dictionary<string,object>{{"expectedCost",EconomyCatalog.Current.EngineerCost}});if(!r.ok)return false;await account.Sync();var owned=account.Profile?.ClassProfile?.Local().Find("engineer")?.Owned==true;if(owned&&!r.alreadyOwned)AudioTransactions.Confirmed("engineer:"+account.Profile.PlayerId,Sound.Engineer);return owned;}
        public Task<EconomyReply> History(int offset=0)=>Call("TransactionHistory",new Dictionary<string,object>{{"offset",offset},{"limit",20}},false);
        internal void ObserveServerTransaction(string transaction){if(!string.IsNullOrEmpty(transaction))LastTransaction=transaction;}
        internal void PendingChanged()=>Changed?.Invoke();
        internal EconomyJournal Journal=>journal;
        internal Task<EconomyReply> ReviveCall(string op,Dictionary<string,object> input)=>Call(op,input);
        public static void ClearDevelopment(){if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development only");Reset();ReviveServices.Reset();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset(){if(Current!=null)Current.account.Changed-=Current.AccountChanged;Current=null;}
    }
}
