using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class StoreSettings
    {
        public int Schema=1;public bool PurchaseEnabled,ProductionLiveEnabled,ExternalSetupReviewed;
        public bool Allows(BuildChannel channel)=>Schema==1&&channel==BuildChannel.Production&&PurchaseEnabled&&ProductionLiveEnabled&&ExternalSetupReviewed;
        public static StoreSettings Load(){var a=Resources.Load<TextAsset>("StoreSettings");return a==null?new StoreSettings():JsonUtility.FromJson<StoreSettings>(a.text);}
    }
    [Serializable]public sealed class StoreItem {public string ProductId;public int GemAmount,SortOrder,VisualTier;public bool Enabled;}
    [Serializable]public sealed class StoreCatalog
    {
        public int Schema,Revision;public bool Proposed;public StoreItem[] Products;
        public static StoreCatalog Load(){var a=Resources.Load<TextAsset>("StoreCatalog");var c=JsonUtility.FromJson<StoreCatalog>(a.text);c.Validate();return c;}
        public void Validate(){if(Schema!=1||Revision<1||Products==null||Products.Length<3||Products.Length>5)throw new InvalidOperationException("Invalid store catalog");var ids=new HashSet<string>();foreach(var p in Products)if(p==null||!System.Text.RegularExpressions.Regex.IsMatch(p.ProductId??"",@"^[a-z][a-z0-9_.]{1,47}$")||!ids.Add(p.ProductId)||p.GemAmount<1||p.GemAmount>144)throw new InvalidOperationException("Invalid store product");}
    }
    [Serializable]public sealed class StorePrice {public string ProductId,FormattedPrice,Currency;public long PriceMicros;public void Validate(){if(string.IsNullOrWhiteSpace(ProductId)||string.IsNullOrWhiteSpace(FormattedPrice)||FormattedPrice.Length>128||!System.Text.RegularExpressions.Regex.IsMatch(Currency??"",@"^[A-Z]{3}$")||PriceMicros<=0)throw new InvalidOperationException("Invalid platform price");}}
    [Serializable]public sealed class StorePurchase {public string ProductId,Token,AccountBinding,ProfileBinding;public int Quantity=1;public bool Pending;}
    public enum BillingOutcome {Purchased,Pending,Cancelled,Unavailable,Failed}
    public sealed class BillingResult {public BillingOutcome Outcome;public StorePurchase Purchase;}
    public interface IStoreBillingClient:IDisposable
    {
        event Action<StorePurchase> Updated;bool Supported{get;}
        Task<StorePrice[]> Fetch(string[] products);Task<StorePurchase[]> Owned();Task<BillingResult> Buy(string product,string accountBinding,string profileBinding);
    }
    public sealed class UnavailableStoreBilling:IStoreBillingClient
    {
        public bool Supported=>false;public event Action<StorePurchase> Updated{add{}remove{}}
        public Task<StorePrice[]> Fetch(string[] products)=>Task.FromResult(Array.Empty<StorePrice>());public Task<StorePurchase[]> Owned()=>Task.FromResult(Array.Empty<StorePurchase>());
        public Task<BillingResult> Buy(string product,string account,string profile)=>Task.FromResult(new BillingResult{Outcome=BillingOutcome.Unavailable});public void Dispose(){}
    }
    [Serializable]public sealed class PurchaseView {public string ProductId,State,Reference,GrantedAt,CompletedAt,RefundedAt;public int GemAmount,Reclaimed,Shortfall;}
    [Serializable]public sealed class StoreState
    {
        public int Schema,CatalogRevision,PendingCount;public string PlayerId,EnvironmentId,AccountBinding;public bool PurchaseEnabled,ReviewRequired;public StoreItem[] Products;public PurchaseView[] Recent;
        public void Validate(string player,string environment,StoreCatalog local){if(Schema!=1||PlayerId!=player||EnvironmentId!=environment||CatalogRevision!=local.Revision||PendingCount<0||PendingCount>64||Products==null||Products.Length!=local.Products.Length||!StoreBinding.Valid(AccountBinding))throw new InvalidOperationException("Invalid store state");foreach(var p in Products){var canonical=Array.Find(local.Products,x=>x.ProductId==p?.ProductId);if(canonical==null||p.GemAmount!=canonical.GemAmount)throw new InvalidOperationException("Store catalog mismatch");}if((Recent?.Length??0)>12)throw new InvalidOperationException("Invalid purchase history");}
    }
    [Serializable]public sealed class PurchaseIntent {public string Id,ProductId,AccountBinding,ProfileBinding;public int GemAmount,CatalogRevision;public void Validate(StoreItem selected,int revision){if(!Guid.TryParseExact(Id,"N",out _)||ProductId!=selected.ProductId||GemAmount!=selected.GemAmount||CatalogRevision<1||CatalogRevision>revision||!StoreBinding.Valid(AccountBinding)||!StoreBinding.Valid(ProfileBinding))throw new InvalidOperationException("Invalid purchase intent");}}
    public static class StoreBinding {public static bool Valid(string value)=>System.Text.RegularExpressions.Regex.IsMatch(value??"",@"^[a-f0-9]{64}$");}
    [Serializable]public sealed class StoreReply {public bool ok,duplicate;public string error,status;public StoreState state;public PurchaseIntent intent;public PurchaseView purchase;public StoreReply[] results;}
    public interface IStoreTransport {Task<StoreReply> Call(string operation,Dictionary<string,object> input);}
    public enum StorePhase {Idle,FetchingProducts,Ready,PurchaseStarting,Pending,Validating,Consuming,Completed,Cancelled,Failed,Recovering,Unavailable,AccountRequired}
}
