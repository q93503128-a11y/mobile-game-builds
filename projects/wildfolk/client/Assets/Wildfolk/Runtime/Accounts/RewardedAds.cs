using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;
namespace Wildfolk
{
    public enum AdPlacement { Gem,Revive }
    public enum AdOutcome { Earned,Cancelled,Unavailable,Failed }
    [Serializable] public sealed class RewardedSettings
    {
        public int Schema=1;public bool Enabled,PrivacyReviewed,ProductionLiveEnabled,UnderAgeOfConsent,UseRegisteredTestUnits;
        public string[] TestDeviceIds=Array.Empty<string>();
        public string Audience="Unspecified",GemAdUnit="",ReviveAdUnit="";
        public const string TestUnit="ca-app-pub-3940256099942544/5224354917";
        public bool UsesOwnedTestUnits=>BackendPolicy.Channel!=BuildChannel.Production&&UseRegisteredTestUnits;
        public bool ValidOwnedUnits=>ValidUnit(GemAdUnit)&&ValidUnit(ReviveAdUnit)&&GemAdUnit!=ReviveAdUnit;
        public bool ValidTestDevices=>TestDeviceIds!=null&&TestDeviceIds.Length>0&&TestDeviceIds.Length<=16&&Array.TrueForAll(TestDeviceIds,id=>System.Text.RegularExpressions.Regex.IsMatch(id??"",@"^[a-fA-F0-9]{32}$"));
        public bool Allowed=>Schema==1&&Enabled&&PrivacyReviewed&&(Audience=="General"||Audience=="Child")&&(BackendPolicy.Channel==BuildChannel.Production?ProductionLiveEnabled&&ValidOwnedUnits:!UseRegisteredTestUnits||ValidOwnedUnits&&ValidTestDevices);
        public static bool ValidUnit(string x)=>System.Text.RegularExpressions.Regex.IsMatch(x??"",@"^ca-app-pub-\d{16}/\d{10}$")&&!x.StartsWith("ca-app-pub-3940256099942544/");
        public string Unit(AdPlacement p)=>BackendPolicy.Channel==BuildChannel.Production||UsesOwnedTestUnits?(p==AdPlacement.Gem?GemAdUnit:ReviveAdUnit):TestUnit;
        public static RewardedSettings Load(){var a=Resources.Load<TextAsset>("RewardedAdsSettings");return a==null?new RewardedSettings():JsonUtility.FromJson<RewardedSettings>(a.text);}
    }
    public sealed class AdRequestContext
    {
        public AdPlacement Placement;public string Operation,UserId,CustomData;
    }
    public interface IRewardedAdProvider:IDisposable
    {
        event Action Changed;bool Supported{get;}bool Ready(AdPlacement placement);bool PrivacyOptionsRequired{get;}
        Task Prepare(AdPlacement placement);Task<AdOutcome> Show(AdRequestContext context,Action earned);
        void PrivacyOptions();
    }
    public sealed class UnavailableRewardedProvider:IRewardedAdProvider
    {
        public event Action Changed{add{}remove{}}public bool Supported=>false;public bool Ready(AdPlacement p)=>false;public bool PrivacyOptionsRequired=>false;
        public Task Prepare(AdPlacement p)=>Task.CompletedTask;public Task<AdOutcome> Show(AdRequestContext c,Action e)=>Task.FromResult(AdOutcome.Unavailable);public void PrivacyOptions(){}public void Dispose(){}
    }
    [Serializable]public sealed class AdOperation
    {
        public string OperationId,Nonce,Day,State,TransactionId;public int Amount;
        public void Validate(){if(!Guid.TryParseExact(OperationId,"N",out _)||!System.Text.RegularExpressions.Regex.IsMatch(Nonce??"",@"^[a-f0-9]{64}$")||!DateTime.TryParseExact(Day,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _)||Amount<1||Amount>3||(State!="Reserved"&&State!="Verified"&&State!="Granted"))throw new InvalidOperationException("Invalid ad operation");}
    }
    [Serializable]public sealed class RewardedState
    {
        public int Schema,GemPerRewardedAd,DailyRewardedGemLimit,Remaining;public string PlayerId,EnvironmentId,ServerTime,NextReset,CooldownUntil;public bool Eligible;public AdOperation Operation;public AdOperation[] PendingOperations;
        public void Validate(string player,string environment)
        {
            if(Schema!=1||PlayerId!=player||EnvironmentId!=environment||GemPerRewardedAd<1||GemPerRewardedAd>3||DailyRewardedGemLimit<GemPerRewardedAd||DailyRewardedGemLimit>10||Remaining<0||Remaining>DailyRewardedGemLimit/GemPerRewardedAd||Eligible&&Remaining==0||!DateTimeOffset.TryParse(ServerTime,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out var time)||!DateTimeOffset.TryParse(NextReset,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out var reset)||reset<=time||(PendingOperations?.Length??0)>128)throw new InvalidOperationException("Invalid ad state");
            Operation?.Validate();if(PendingOperations!=null)foreach(var op in PendingOperations){if(op==null)throw new InvalidOperationException("Invalid pending ad");op.Validate();}
        }
    }
    [Serializable]public sealed class RewardedReply {public bool ok,duplicate;public string error,transactionId;public RewardedState state;public AdOperation operation;}
    public interface IRewardedTransport {Task<RewardedReply> Call(string op,Dictionary<string,object> input);}
    public sealed class UgsRewardedTransport:IRewardedTransport
    {
        public Task<RewardedReply> Call(string op,Dictionary<string,object> input){UgsBootstrap.VerifyEnvironment();return CloudCodeService.Instance.CallEndpointAsync<RewardedReply>("LowfireRewarded",new Dictionary<string,object>{{"operation",op},{"input",input},{"channel",BackendPolicy.Channel.ToString()},{"buildVersion",Application.version}});}
    }
    [Serializable]public sealed class RewardedJournalState {public int Schema=1;public string Scope,Device=Guid.NewGuid().ToString("N"),ReviveEarned;public AdOperation Pending;public bool Earned;public List<AdOperation> Waiting=new List<AdOperation>();}
    public sealed class RewardedJournal
    {
        [Serializable]private sealed class Envelope{public string Payload,Checksum;}
        public RewardedJournalState State{get;private set;}public string Path{get;}
        public RewardedJournal(string path,string scope){Path=path;State=new RewardedJournalState{Scope=scope};if(!File.Exists(path)&&!File.Exists(path+".backup"))return;foreach(var p in new[]{path,path+".backup"})try{if(!File.Exists(p))continue;var e=JsonUtility.FromJson<Envelope>(File.ReadAllText(p));if(e.Checksum!=EconomyJournal.Hash(e.Payload))continue;var s=JsonUtility.FromJson<RewardedJournalState>(e.Payload);if(s.Schema!=1||s.Scope!=scope||!Guid.TryParseExact(s.Device,"N",out _))continue;if(s.Waiting==null)s.Waiting=new List<AdOperation>();if(s.Waiting.Count>32)throw new IOException("Reward queue full");if(string.IsNullOrEmpty(s.Pending?.OperationId))s.Pending=null;if(string.IsNullOrEmpty(s.ReviveEarned))s.ReviveEarned=null;State=s;return;}catch(Exception){}throw new IOException("Reward journal unavailable");}
        public static RewardedJournal Open(string scope)=>new RewardedJournal(System.IO.Path.Combine(EconomyJournal.Root,"Ads",EconomyJournal.Hash(scope)+".json"),scope);
        public void Save(){if(State.Waiting==null||State.Waiting.Count>32)throw new IOException("Reward queue full");var p=JsonUtility.ToJson(State);EconomyJournal.Atomic(Path,JsonUtility.ToJson(new Envelope{Payload=p,Checksum=EconomyJournal.Hash(p)}));}
        public void Clear(){foreach(var s in new[]{"",".backup",".temporary"})if(File.Exists(Path+s))File.Delete(Path+s);State=new RewardedJournalState{Scope=State.Scope};}
    }
    // The client never supplies a financial proof. SDK completion starts reconciliation;
    // only the authenticated SSV worker can write the server's verified receipt intent.
    public sealed class RewardedAds:IRewardedReviveService,IScopedRewardedRevive,IDisposable
    {
        private static RewardedAds current;public static RewardedAds Current=>current;public static Func<IRewardedAdProvider> ProviderFactory;
        public static RewardedAds Get(){var a=AccountService.Get();if(current!=null&&current.account==a)return current;current?.Dispose();return current=new RewardedAds(a,new UgsRewardedTransport(),ProviderFactory?.Invoke()??new UnavailableRewardedProvider());}
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !LOWFIRE_ALPHA && !LOWFIRE_PRODUCTION)
        public static RewardedAds InstallDevelopment(AccountService account,IRewardedTransport transport,IRewardedAdProvider provider){if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development only");current?.Dispose();return current=new RewardedAds(account,transport,provider);}
#endif
        public event Action Changed;public RewardedState State{get;private set;}public string LastError{get;private set;}public bool Busy{get;private set;}
        private readonly AccountService account;private readonly IRewardedTransport transport;private readonly IRewardedAdProvider provider;
        private RewardedJournal journal,revive;private string scope;private int epoch;private bool preparing;private string preparedTransaction;
        public RewardedAds(AccountService a,IRewardedTransport t,IRewardedAdProvider p){account=a;transport=t;provider=p;account.Changed+=AccountChanged;provider.Changed+=Signal;ReviveServices.InstallRewarded(this);}
        public bool Preparing=>preparing;public bool CanRetry=>provider.Supported&&!preparing&&!Busy;
        public bool Available=>provider.Ready(AdPlacement.Revive);
        public bool PrivacyOptionsRequired=>provider.PrivacyOptionsRequired;public void PrivacyOptions()=>provider.PrivacyOptions();
        public bool CanGem=>!Busy&&account.Linked&&account.CloudAvailable&&!PendingToday&&State?.Eligible==true&&provider.Ready(AdPlacement.Gem);
        public bool Pending=>journal!=null&&(journal.State.Waiting.Count>0||journal.State.Pending!=null&&journal.State.Earned);
        private bool PendingToday=>journal?.State.Pending!=null&&journal.State.Earned&&(string.IsNullOrEmpty(journal.State.Pending.Day)||State?.ServerTime==null||journal.State.Pending.Day==State.ServerTime.Substring(0,10));
        private void Signal()=>Changed?.Invoke();
        private void AccountChanged(){if(account.State==AccountConnection.DeletionPending)try{journal?.Clear();}catch{LastError="local_save_failed";}epoch++;State=null;journal=null;scope=null;LastError=null;Signal();}
        private bool Bind(){if(!account.Linked||!account.CloudAvailable||account.Profile==null){LastError="ad_account_required";return false;}var next=BackendSettings.Current.Environment.Id+":"+account.Profile.PlayerId;try{if(scope!=next){journal=RewardedJournal.Open(next);scope=next;}return true;}catch{LastError="local_save_failed";return false;}}
        private async Task<RewardedReply> Call(string op,AdOperation pending=null)
        {
            var owner=epoch;
            try{
                var input=new Dictionary<string,object>{{"device",journal.State.Device}};
                if(op!="GetRewardedState")input["operationId"]=pending?.OperationId??Guid.NewGuid().ToString("N");
                var r=await transport.Call(op,input);
                if(owner!=epoch)return new RewardedReply{error="account_changed"};
                if(r==null)throw new InvalidOperationException("Missing ad response");
                if(op=="GetRewardedState"&&(!r.ok||r.state==null))State=null;
                if(r.state!=null){r.state.Validate(account.Profile?.PlayerId,BackendSettings.Current.Environment.Id);State=r.state;}
                r.operation?.Validate();LastError=r.ok?null:r.error;return r;
            }catch{if(op=="GetRewardedState")State=null;LastError="cloud_unavailable";return new RewardedReply{error=LastError};}
        }
        public async Task Refresh(){if(Busy||!Bind()) {Signal();return;}Busy=true;var owner=epoch;try{await Call("GetRewardedState");if(owner!=epoch||journal==null)return;if(State?.PendingOperations!=null)foreach(var op in State.PendingOperations)if(op.State=="Verified"&&op.OperationId!=journal.State.Pending?.OperationId&&!journal.State.Waiting.Exists(x=>x.OperationId==op.OperationId)&&journal.State.Waiting.Count<32)journal.State.Waiting.Add(op);if(State?.Operation!=null){KeepPrevious(State.Operation.OperationId);journal.State.Pending=State.Operation;if(State.Operation.State=="Verified")journal.State.Earned=true;journal.Save();}foreach(var op in journal.State.Waiting.ToArray()){if(owner!=epoch)return;await ReconcileCore(op);}if(journal.State.Pending!=null)await ReconcileCore();if(State?.Eligible==true)await provider.Prepare(AdPlacement.Gem);}catch{LastError="local_save_failed";}finally{Busy=false;Signal();}}
        private bool foregroundReward;
        public async Task WatchGem()
        {
            if(Busy||!Bind()||PendingToday)return;foregroundReward=true;Busy=true;var owner=epoch;try{
                var begin=await Call("BeginGemReward");if(!begin.ok||begin.operation==null)return;var o=begin.operation;KeepPrevious(o.OperationId);journal.State.Pending=o;journal.Save(); // durable BEFORE showing
                if(o.State=="Verified"){await ReconcileCore();return;}await provider.Prepare(AdPlacement.Gem);if(owner!=epoch)return;
                var result=await AudioRuntime.DuringOverlay(()=>provider.Show(new AdRequestContext{Placement=AdPlacement.Gem,Operation=o.OperationId,UserId=account.Profile.PlayerId,CustomData="lf3."+BackendSettings.Current.Environment.Id+"."+o.OperationId+"."+o.Nonce+".rewarded_gem"},()=>{if(owner==epoch){journal.State.Earned=true;journal.Save();}}));
                if(owner!=epoch)return;ReportGem(o.OperationId,result);LastError=result==AdOutcome.Earned?"ad_pending":result==AdOutcome.Cancelled?"ad_cancelled":result==AdOutcome.Unavailable?"ad_unavailable":"ad_failed";
                // A callback is deliberately insufficient to increase the wallet.
                if(result==AdOutcome.Earned)for(var n=0;n<10&&owner==epoch;n++){if(await ReconcileCore())break;await Task.Delay(2000);}
                else {await ReconcileCore();LastError=result==AdOutcome.Cancelled?"ad_cancelled":result==AdOutcome.Unavailable?"ad_unavailable":"ad_failed";}
            }catch{LastError="local_save_failed";}finally{foregroundReward=false;Busy=false;Signal();}
        }
        private void KeepPrevious(string next){var old=journal.State.Pending;if(old==null||old.OperationId==next)return;if(journal.State.Earned&&!journal.State.Waiting.Exists(x=>x.OperationId==old.OperationId)){if(journal.State.Waiting.Count>=32)throw new IOException("Reward queue full");journal.State.Waiting.Add(old);}journal.State.Earned=false;}
        private async Task<bool> ReconcileCore(AdOperation queued=null){var o=queued??journal?.State.Pending;if(o==null)return true;var owner=epoch;var r=await Call("RetryGemReward",o);if(owner!=epoch||journal==null)return false;if(r.ok&&r.operation?.State=="Granted"||r.error=="ad_operation_missing"){journal.State.Waiting.RemoveAll(x=>x.OperationId==o.OperationId);if(journal.State.Pending?.OperationId==o.OperationId){journal.State.Pending=null;journal.State.Earned=false;}journal.Save();LastError=null;await Call("GetRewardedState");EconomyService.Get().ObserveServerTransaction(r.transactionId??r.operation?.TransactionId);if(foregroundReward&&r.ok&&r.operation?.State=="Granted")AudioTransactions.Confirmed(scope+":"+(r.transactionId??r.operation.TransactionId),Sound.Gem);await EconomyService.Get().Sync();return true;}return false;}
        private bool ReviveJournal(){try{if(revive==null)revive=RewardedJournal.Open(BackendSettings.Current.Environment.Id+":revive:"+ClassProfiles.LocalId);return true;}catch{LastError="local_save_failed";return false;}}
        public bool AvailableFor(string transaction)=>!Busy&&ReviveJournal()&&(revive.State.ReviveEarned==transaction||Available);
        public async void Prepare(string transaction,bool retry=false){if(preparing||Busy||!retry&&preparedTransaction==transaction||!ReviveJournal()||revive.State.ReviveEarned==transaction)return;preparedTransaction=transaction;preparing=true;try{await provider.Prepare(AdPlacement.Revive);}catch{LastError="ad_unavailable";}finally{preparing=false;Signal();}}
        public async void Show(string transaction,Action<bool> completed)
        {
            if(!AvailableFor(transaction)){completed(false);return;}if(revive.State.ReviveEarned==transaction){completed(true);return;}
            Busy=true;Signal();var durable=false;try{var result=await AudioRuntime.DuringOverlay(()=>provider.Show(new AdRequestContext{Placement=AdPlacement.Revive,Operation=transaction},()=>{var old=revive.State.ReviveEarned;try{revive.State.ReviveEarned=transaction;revive.Save();durable=true;}catch{revive.State.ReviveEarned=old;throw;}}));ReportRevive(transaction,result==AdOutcome.Earned&&durable?"earned":result==AdOutcome.Cancelled?"cancelled":result==AdOutcome.Unavailable?"unavailable":"provider_error");completed(result==AdOutcome.Earned&&durable);}catch{LastError="local_save_failed";completed(false);}finally{preparedTransaction=null;Busy=false;Signal();}
        }
        public void Resolve(string transaction,bool worldSaved){ReportRevive(transaction,worldSaved?"world_saved":"world_save_failed");if(!worldSaved||!ReviveJournal()||revive.State.ReviveEarned!=transaction)return;try{revive.State.ReviveEarned=null;revive.Save();}catch{LastError="local_save_failed";}}
        private async void ReportGem(string operation,AdOutcome result){if(!account.Linked||!account.CloudAvailable||journal==null)return;try{await transport.Call("RecordGemAd",new Dictionary<string,object>{{"operationId",Guid.NewGuid().ToString("N")},{"adOperationId",operation},{"device",journal.State.Device},{"result",result==AdOutcome.Earned?"earned":result==AdOutcome.Cancelled?"cancelled":result==AdOutcome.Unavailable?"unavailable":"provider_error"}});}catch{/* optional support outcome; never financial proof */}}
        private async void ReportRevive(string transaction,string result){if(!account.Linked||!account.CloudAvailable||account.Profile==null)return;try{var j=RewardedJournal.Open(BackendSettings.Current.Environment.Id+":"+account.Profile.PlayerId);await transport.Call("RecordReviveAd",new Dictionary<string,object>{{"operationId",EconomyJournal.Hash(transaction+":"+result).Substring(0,32)},{"device",j.State.Device},{"transactionHash",EconomyJournal.Hash(transaction)},{"result",result}});}catch{/* optional client-reported support evidence; never reward authority */}}
        public void Dispose(){account.Changed-=AccountChanged;provider.Changed-=Signal;provider.Dispose();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset(){current?.Dispose();current=null;ProviderFactory=null;}
    }
}
