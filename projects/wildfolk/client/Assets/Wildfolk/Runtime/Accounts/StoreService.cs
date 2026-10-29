using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using UnityEngine;
namespace Wildfolk
{
    public sealed class UgsStoreTransport:IStoreTransport
    {
        public async Task<StoreReply> Call(string operation,Dictionary<string,object> input){UgsBootstrap.VerifyEnvironment();if(operation=="BeginPurchase"){if(AndroidAccountBridge.IntegrityToken==null)throw new InvalidOperationException("Trusted build unavailable");input["integrityToken"]=await AndroidAccountBridge.IntegrityToken(AccountRequestHash.ForAlpha(AccountService.Get().Profile.PlayerId,BackendSettings.Current.Environment.Id,(string)input["operationId"],Application.version));}return await CloudCodeService.Instance.CallEndpointAsync<StoreReply>("LowfirePurchase",new Dictionary<string,object>{{"operation",operation},{"input",input},{"channel",BackendPolicy.Channel.ToString()},{"buildVersion",Application.version}});}
    }
    public sealed class StoreService:IDisposable
    {
        public static StoreService Current{get;private set;}public static Func<IStoreBillingClient> BillingFactory;
        public static StoreService Get(){var a=AccountService.Get();if(Current!=null&&Current.account==a)return Current;Current?.Dispose();return Current=new StoreService(a,new UgsStoreTransport(),BillingFactory?.Invoke()??new UnavailableStoreBilling(),StoreCatalog.Load());}
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !LOWFIRE_ALPHA && !LOWFIRE_PRODUCTION)
        public static StoreService InstallDevelopment(AccountService account,IStoreTransport transport,IStoreBillingClient billing){if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development required");Current?.Dispose();return Current=new StoreService(account,transport,billing,StoreCatalog.Load(),true);}
#endif
        public StoreState State{get;private set;}public StorePhase Phase{get;private set;}public StorePrice[] Prices{get;private set;}=Array.Empty<StorePrice>();public string LastError{get;private set;}public bool Busy{get;private set;}public event Action Changed;
        public StoreCatalog Catalog{get;}public bool IsDevelopment=>development;public bool AccountReady=>account.State==AccountConnection.Connected&&account.Linked&&account.CloudAvailable&&account.Profile!=null;
        public bool CanBuy=>!Busy&&(journal?.State.Pending.Count??0)==0&&(State?.PendingCount??0)==0&&Phase!=StorePhase.Pending&&Phase!=StorePhase.Consuming&&AccountReady&&Policy&&State?.PurchaseEnabled==true&&!State.ReviewRequired&&billing.Supported&&Prices.Length>0;
        private bool RecoveryAllowed=>development||BackendSettings.Current.Enabled&&BackendPolicy.Channel==BuildChannel.Production&&StoreSettings.Load().ExternalSetupReviewed;
        private bool Policy=>development||BackendSettings.Current.Enabled&&StoreSettings.Load().Allows(BackendPolicy.Channel)&&!Catalog.Proposed;
        private readonly AccountService account;private readonly IStoreTransport transport;private readonly IStoreBillingClient billing;private readonly bool development;private PurchaseJournal journal;private string scope,lastScope;private int epoch;private bool disposed;
        private readonly HashSet<string> deletedScopes=new HashSet<string>();
        private readonly Dictionary<string,PurchaseJournal> owners=new Dictionary<string,PurchaseJournal>();
        public StoreService(AccountService account,IStoreTransport transport,IStoreBillingClient billing,StoreCatalog catalog,bool development=false){if(development&&(!BackendPolicy.Fixtures||!DevelopmentProfile.Isolated))throw new InvalidOperationException("Isolated Development required");this.account=account;this.transport=transport;this.billing=billing;this.development=development;Catalog=catalog;if(account.Profile!=null)lastScope=BackendSettings.Current.Environment.Id+":"+account.Profile.PlayerId;account.Changed+=AccountChanged;billing.Updated+=Updated;}
        private void Signal(StorePhase phase,string error=null){Phase=phase;LastError=error;Changed?.Invoke();}
        private void AccountChanged(){if(account.Profile!=null)lastScope=BackendSettings.Current.Environment.Id+":"+account.Profile.PlayerId;epoch++;State=null;Prices=Array.Empty<StorePrice>();if(account.State==AccountConnection.DeletionPending)try{if(!string.IsNullOrEmpty(lastScope)){deletedScopes.Add(lastScope);PurchaseJournal.Erase(lastScope);}}catch{}journal=null;scope=null;Signal(AccountReady?StorePhase.Idle:StorePhase.AccountRequired);}
        private bool Bind(){if(!AccountReady){Signal(StorePhase.AccountRequired,"purchase_account_required");return false;}try{var next=BackendSettings.Current.Environment.Id+":"+account.Profile.PlayerId;if(next!=scope){journal=PurchaseJournal.Open(next);scope=next;}return true;}catch{Signal(StorePhase.Unavailable,"local_save_failed");return false;}}
        private async Task<StoreReply> Call(string operation,Dictionary<string,object> input,int owner){var r=await transport.Call(operation,input);if(owner!=epoch||disposed)return new StoreReply{error="account_changed"};if(r==null)throw new InvalidOperationException("Missing store reply");if(r.state!=null){r.state.Validate(account.Profile.PlayerId,BackendSettings.Current.Environment.Id,Catalog);State=r.state;owners[State.AccountBinding]=journal;}return r;}
        public async Task Open()
        {
            if(Busy||disposed)return;Prices=Array.Empty<StorePrice>();if(!Bind())return;Busy=true;var owner=epoch;
            try{Signal(StorePhase.FetchingProducts);var state=await Call("GetStoreState",new Dictionary<string,object>(),owner);if(owner!=epoch)return;if(!state.ok||State==null){State=null;Signal(StorePhase.Unavailable,state.error??"store_unavailable");return;}
                if(!Policy||!State.PurchaseEnabled||!billing.Supported){if(RecoveryAllowed&&billing.Supported&&(journal.State.Pending.Count>0||State.PendingCount>0))await RecoverCore(owner);Signal(StorePhase.Unavailable,BackendPolicy.Channel==BuildChannel.Alpha?"alpha_purchase_disabled":"store_unavailable");return;}
                var products=await billing.Fetch(State.Products.Where(p=>p.Enabled).Select(p=>p.ProductId).ToArray());if(owner!=epoch)return;foreach(var p in products){p.Validate();if(!State.Products.Any(x=>x.ProductId==p.ProductId&&x.Enabled))throw new InvalidOperationException("Unknown platform product");}if(products.Select(x=>x.ProductId).Distinct().Count()!=products.Length)throw new InvalidOperationException("Duplicate product");Prices=products;
                await RecoverCore(owner);if(owner==epoch&&Phase!=StorePhase.Completed&&Phase!=StorePhase.Pending&&Phase!=StorePhase.Consuming)Signal(Prices.Length>0?StorePhase.Ready:StorePhase.Unavailable);
            }catch{if(owner==epoch){State=null;Prices=Array.Empty<StorePrice>();Signal(StorePhase.Unavailable,"store_unavailable");}}finally{Busy=false;Changed?.Invoke();}
        }
        public async Task Purchase(string productId)
        {
            if(!CanBuy||!Bind())return;var item=State.Products.FirstOrDefault(p=>p.ProductId==productId&&p.Enabled);if(item==null||!Prices.Any(p=>p.ProductId==productId))return;Busy=true;var owner=epoch;var original=journal;
            try{Signal(StorePhase.PurchaseStarting);var begin=await Call("BeginPurchase",new Dictionary<string,object>{{"operationId",Guid.NewGuid().ToString("N")},{"productId",productId},{"catalogRevision",State.CatalogRevision}},owner);if(owner!=epoch)return;if(!begin.ok||begin.intent==null){Signal(StorePhase.Failed,begin.error??"purchase_unavailable");return;}begin.intent.Validate(item,State.CatalogRevision);if(begin.intent.AccountBinding!=State.AccountBinding)throw new InvalidOperationException("Purchase binding mismatch");owners[begin.intent.AccountBinding]=original;
                var result=await AudioRuntime.DuringOverlay(()=>billing.Buy(productId,begin.intent.AccountBinding,begin.intent.ProfileBinding));
                // Preserve an earned transaction in its ORIGINAL account journal even
                // when the account changes while Google's purchase UI is open.
                if(result.Purchase!=null&&!deletedScopes.Contains(original.State.Scope)){RequirePurchase(result.Purchase,begin.intent.AccountBinding);original.Add(result.Purchase);}
                if(owner!=epoch)return;if(result.Outcome==BillingOutcome.Cancelled){Signal(StorePhase.Cancelled);return;}if(result.Outcome==BillingOutcome.Pending){Signal(StorePhase.Pending);return;}if(result.Outcome!=BillingOutcome.Purchased||result.Purchase==null){Signal(StorePhase.Failed,"purchase_unavailable");return;}await Verify(original.State.Pending.Find(p=>p.Token==result.Purchase.Token),owner,true);if(owner==epoch)await RecoverServer(owner);
            }catch{if(owner==epoch)Signal(StorePhase.Failed,"verification_pending");}finally{Busy=false;Changed?.Invoke();}
        }
        private void RequirePurchase(StorePurchase p,string binding){if(p==null||string.IsNullOrEmpty(p.Token)||p.Token.Length>4096||p.Quantity!=1||p.AccountBinding!=binding||!StoreBinding.Valid(p.ProfileBinding)||!Catalog.Products.Any(x=>x.ProductId==p.ProductId))throw new InvalidOperationException("Invalid platform purchase");}
        private void Updated(StorePurchase purchase){if(disposed)return;if(purchase==null){Prices=Array.Empty<StorePrice>();if(!Busy)Signal(StorePhase.Unavailable,"store_unavailable");return;}try{if(!owners.TryGetValue(purchase.AccountBinding??"",out var owner)||deletedScopes.Contains(owner.State.Scope))return;RequirePurchase(purchase,purchase.AccountBinding);owner.Add(purchase);if(!Busy&&AccountReady&&State?.AccountBinding==purchase.AccountBinding)_=Recover();}catch{Signal(StorePhase.Failed,"local_save_failed");}}
        private async Task Verify(PendingPurchase pending,int owner,bool audible=false){if(pending==null)return;Signal(StorePhase.Validating);var r=await Call("VerifyPurchase",new Dictionary<string,object>{{"operationId",pending.OperationId},{"token",pending.Token}},owner);if(owner!=epoch)return;Apply(r);if(audible&&r.ok&&r.status=="Completed"&&r.purchase!=null&&!string.IsNullOrEmpty(r.purchase.Reference)&&r.purchase.GemAmount>0)AudioTransactions.Confirmed(scope+":"+r.purchase.Reference,Sound.Gem);if(r.ok&&r.status=="Completed"||r.error=="purchase_reversed")journal.Remove(pending.Token);}
        private void Apply(StoreReply r){if(!r.ok){Signal(StorePhase.Failed,r.error??"verification_pending");return;}if(r.purchase?.Reference?.StartsWith("G-",StringComparison.Ordinal)==true)EconomyService.Get().ObserveServerTransaction(r.purchase.Reference);Signal(r.status=="Completed"?StorePhase.Completed:r.status=="Pending"?StorePhase.Pending:r.status=="Consuming"?StorePhase.Consuming:r.status=="Reversed"?StorePhase.Failed:Phase,r.status=="Reversed"?"purchase_reversed":null);}
        public async Task Recover(){if(Busy||disposed||!Bind())return;Busy=true;var owner=epoch;try{Signal(StorePhase.Recovering);if(State==null){var s=await Call("GetStoreState",new Dictionary<string,object>(),owner);if(!s.ok||State==null)return;}await RecoverCore(owner);}catch{if(owner==epoch)Signal(StorePhase.Failed,"verification_pending");}finally{Busy=false;Changed?.Invoke();}}
        private async Task RecoverCore(int owner){if(!RecoveryAllowed)return;var owned=await billing.Owned();if(owner!=epoch)return;foreach(var p in owned){if(p.AccountBinding!=State.AccountBinding)continue;RequirePurchase(p,State.AccountBinding);journal.Add(p);}foreach(var p in journal.State.Pending.Take(4).ToArray()){if(owner!=epoch)return;await Verify(p,owner);}if(owner==epoch)await RecoverServer(owner);}
        private async Task RecoverServer(int owner){for(var n=0;n<3&&owner==epoch;n++){var r=await Call("RecoverPurchases",new Dictionary<string,object>(),owner);if(!r.ok){Signal(StorePhase.Failed,r.error??"verification_pending");return;}if(r.results!=null)foreach(var result in r.results)Apply(result);if(State?.PendingCount==0)break;}if(owner!=epoch)return;
            // Google no longer returns consumed purchases. Server references finish
            // local queued tokens after reinstall/response loss without restoring spent Gem.
            foreach(var pending in journal.State.Pending.ToArray())if(State?.Recent?.Any(r=>r.ProductId==pending.ProductId&&r.State=="Completed")==true){var r=await Call("VerifyPurchase",new Dictionary<string,object>{{"operationId",pending.OperationId},{"token",pending.Token}},owner);if(owner!=epoch)return;if(r.ok&&r.status=="Completed"||r.error=="purchase_reversed")journal.Remove(pending.Token);}
            await EconomyService.Get().Sync();}
        public void Dispose(){if(disposed)return;disposed=true;epoch++;account.Changed-=AccountChanged;billing.Updated-=Updated;billing.Dispose();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]private static void Reset(){Current?.Dispose();Current=null;BillingFactory=null;}
    }
}
