#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !LOWFIRE_ALPHA && !LOWFIRE_PRODUCTION)
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Wildfolk
{
    // Explicit isolated development fixtures. Never Google purchase/receipt proof.
    public sealed class DevelopmentStoreBilling:IStoreBillingClient
    {
        public event Action<StorePurchase> Updated;public bool Supported=>true;public BillingOutcome Next=BillingOutcome.Purchased;public bool FetchFails;public string LongPrice;public int Buys;public TaskCompletionSource<BillingResult> Held;
        public List<StorePurchase> Purchases=new List<StorePurchase>();
        public DevelopmentStoreBilling(){if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development required");}
        public Task<StorePrice[]> Fetch(string[] products){if(FetchFails)throw new InvalidOperationException("Fixture unavailable");return Task.FromResult(products.Select(p=>new StorePrice{ProductId=p,FormattedPrice=LongPrice??(p.EndsWith("_144")?"US$ 5.99":p.EndsWith("_72")?"US$ 2.99":"US$ 0.99"),Currency="USD",PriceMicros=p.EndsWith("_144")?5990000:p.EndsWith("_72")?2990000:990000}).ToArray());}
        public Task<StorePurchase[]> Owned()=>Task.FromResult(Purchases.ToArray());
        public Task<BillingResult> Buy(string product,string account,string profile){Buys++;if(Held!=null)return Held.Task;var result=new BillingResult{Outcome=Next};if(Next==BillingOutcome.Purchased||Next==BillingOutcome.Pending){result.Purchase=new StorePurchase{ProductId=product,Token="fixture-"+product+"-"+Guid.NewGuid().ToString("N"),AccountBinding=account,ProfileBinding=profile,Pending=Next==BillingOutcome.Pending};Purchases.Add(result.Purchase);Updated?.Invoke(result.Purchase);}return Task.FromResult(result);}
        public void Emit(StorePurchase value)=>Updated?.Invoke(value);public void Dispose(){}
    }
    public sealed class DevelopmentStoreTransport:IStoreTransport
    {
        private readonly string player,environment;private readonly HashSet<string> granted=new HashSet<string>();public bool Available=true,FailVerification,Pending,FailStart;public int Grants;public Action<int> Grant;public PurchaseView Last;public int Calls;public TaskCompletionSource<StoreReply> HeldState;
        public string Binding=>EconomyJournal.Hash("fixture-account:"+player+":"+environment);
        public DevelopmentStoreTransport(string player,string environment){if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development required");this.player=player;this.environment=environment;}
        public Task<StoreReply> Call(string operation,Dictionary<string,object> input)
        {
            Calls++;if(operation=="GetStoreState"&&HeldState!=null)return HeldState.Task;var c=StoreCatalog.Load();var state=new StoreState{Schema=1,PlayerId=player,EnvironmentId=environment,AccountBinding=Binding,CatalogRevision=c.Revision,PurchaseEnabled=Available,Products=c.Products,Recent=Last==null?Array.Empty<PurchaseView>():new[]{Last},PendingCount=Pending?1:0};
            if(operation=="GetStoreState")return Task.FromResult(new StoreReply{ok=true,state=state});
            if(operation=="BeginPurchase"){if(FailStart)return Task.FromResult(new StoreReply{error="purchase_unavailable"});var item=Array.Find(c.Products,p=>p.ProductId==(string)input["productId"]);return Task.FromResult(new StoreReply{ok=true,intent=new PurchaseIntent{Id=(string)input["operationId"],ProductId=item.ProductId,GemAmount=item.GemAmount,CatalogRevision=c.Revision,AccountBinding=Binding,ProfileBinding=EconomyJournal.Hash("fixture-profile:"+(string)input["operationId"])}});}
            if(operation=="VerifyPurchase"){
                if(FailVerification)return Task.FromResult(new StoreReply{error="verification_pending"});if(Pending)return Task.FromResult(new StoreReply{ok=true,status="Pending"});var token=(string)input["token"];var item=Array.Find(c.Products,p=>token.StartsWith("fixture-"+p.ProductId+"-",StringComparison.Ordinal));if(item==null)throw new InvalidOperationException("Unknown fixture token");if(granted.Add(token)){Grants++;Grant?.Invoke(item.GemAmount);}Last=new PurchaseView{ProductId=item.ProductId,GemAmount=item.GemAmount,State="Completed",Reference="G-1-111111111111",GrantedAt="2026-10-28T12:00:00Z"};return Task.FromResult(new StoreReply{ok=true,status="Completed",purchase=Last});
            }
            if(operation=="RecoverPurchases")return Task.FromResult(new StoreReply{ok=true,state=state,results=Array.Empty<StoreReply>()});return Task.FromResult(new StoreReply{error="operation_forbidden"});
        }
    }
}
#endif
