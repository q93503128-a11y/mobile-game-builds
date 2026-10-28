using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
namespace Wildfolk.AndroidBilling
{
    public sealed class PlayBillingClient:IStoreBillingClient
    {
        private AndroidJavaObject bridge;private Callback callback;private BillingHost host;private bool disposed;private TaskCompletionSource<BillingResult> purchase;
        private readonly Dictionary<string,TaskCompletionSource<string>> requests=new Dictionary<string,TaskCompletionSource<string>>();
        public event Action<StorePurchase> Updated;
        public bool Supported=>!disposed&&Application.platform==RuntimePlatform.Android&&BackendPolicy.Channel==BuildChannel.Production&&BackendSettings.Current.Enabled&&StoreSettings.Load().ExternalSetupReviewed;
        [Serializable]private sealed class Message {public string Type,Request;public int Code;public StorePurchase[] Items;}
        [Serializable]private sealed class Products {public StorePrice[] Items;}
        private sealed class Callback:AndroidJavaProxy
        {
            private readonly ConcurrentQueue<string> queue;
            public Callback(ConcurrentQueue<string> queue):base("com.gltam.lowfire.billing.PlayBilling$Callback"){this.queue=queue;}
            [UnityEngine.Scripting.Preserve]public void onResult(string json){if(json!=null&&json.Length<=300000)queue.Enqueue(json);}
        }
        private void Initialize(){if(bridge!=null)return;if(!Supported)throw new InvalidOperationException("Billing unavailable");host=new GameObject("Billing callbacks").AddComponent<BillingHost>();UnityEngine.Object.DontDestroyOnLoad(host.gameObject);host.Receive=Receive;host.Resumed=Resume;callback=new Callback(host.Queue);using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer")){bridge=new AndroidJavaObject("com.gltam.lowfire.billing.PlayBilling",player.GetStatic<AndroidJavaObject>("currentActivity"),callback);}}
        private async Task<string> Request(string method,string arg=null){Initialize();var id=Guid.NewGuid().ToString("N");var t=new TaskCompletionSource<string>();requests.Add(id,t);try{if(arg==null)bridge.Call(method,id);else bridge.Call(method,id,arg);if(await Task.WhenAny(t.Task,Task.Delay(15000))!=t.Task)throw new TimeoutException("Billing unavailable");return await t.Task;}finally{requests.Remove(id);}}
        private async void Resume(){if(disposed)return;try{await Owned();}catch{}}
        private void Receive(string json)
        {
            if(disposed)return;Message m;try{m=JsonUtility.FromJson<Message>(json);}catch{return;}if(m==null)return;
            if(m.Type=="products"||m.Type=="owned"){if(requests.TryGetValue(m.Request??"",out var pending)){if(m.Code==0)pending.TrySetResult(json);else pending.TrySetException(new InvalidOperationException("Billing unavailable"));}if(m.Type=="owned"&&m.Code==0&&m.Items!=null)foreach(var p in m.Items)Updated?.Invoke(p);return;}
            if(m.Type=="disconnected"){Updated?.Invoke(null);return;}if(m.Type!="purchase")return;var result=new BillingResult{Outcome=m.Code==1?BillingOutcome.Cancelled:m.Code==3||m.Code==-1||m.Code==2?BillingOutcome.Unavailable:BillingOutcome.Failed};
            if(m.Code==0&&m.Items!=null){foreach(var p in m.Items)Updated?.Invoke(p);if(m.Items.Length>0){result.Purchase=m.Items[0];result.Outcome=result.Purchase.Pending?BillingOutcome.Pending:BillingOutcome.Purchased;}}
            var active=purchase;purchase=null;active?.TrySetResult(result);
        }
        public async Task<StorePrice[]> Fetch(string[] products){var json=await Request("fetch",SerializeIds(products));return JsonUtility.FromJson<Products>(json).Items??Array.Empty<StorePrice>();}
        private static string SerializeIds(string[] ids){return "["+string.Join(",",Array.ConvertAll(ids,id=>"\""+id+"\""))+"]";}
        public async Task<StorePurchase[]> Owned(){var json=await Request("owned");return JsonUtility.FromJson<Message>(json).Items??Array.Empty<StorePurchase>();}
        public Task<BillingResult> Buy(string product,string account,string profile){if(!Supported||!StoreSettings.Load().Allows(BackendPolicy.Channel)||purchase!=null)return Task.FromResult(new BillingResult{Outcome=BillingOutcome.Unavailable});Initialize();purchase=new TaskCompletionSource<BillingResult>();var task=purchase.Task;try{bridge.Call("buy",product,account,profile);}catch{purchase.TrySetResult(new BillingResult{Outcome=BillingOutcome.Failed});purchase=null;}return task;}
        public void Dispose(){if(disposed)return;disposed=true;foreach(var r in requests.Values)r.TrySetException(new InvalidOperationException("Billing unavailable"));requests.Clear();purchase?.TrySetResult(new BillingResult{Outcome=BillingOutcome.Unavailable});purchase=null;if(bridge!=null){bridge.Call("dispose");bridge.Dispose();bridge=null;}if(host!=null)UnityEngine.Object.Destroy(host.gameObject);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]private static void Register()=>StoreService.BillingFactory=()=>new PlayBillingClient();
    }
    // Main-thread dispatch only. An empty queue allocates/rebuilds nothing.
    public sealed class BillingHost:MonoBehaviour
    {
        public readonly ConcurrentQueue<string> Queue=new ConcurrentQueue<string>();public Action<string> Receive;public Action Resumed;
        private void Update(){while(Queue.TryDequeue(out var json))Receive?.Invoke(json);}
        private void OnApplicationPause(bool paused){if(!paused)Resumed?.Invoke();}
    }
}
