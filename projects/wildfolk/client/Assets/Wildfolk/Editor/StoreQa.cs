using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk.Editor
{
    [InitializeOnLoad]public static class StoreQa
    {
        private static string Root=>Environment.GetEnvironmentVariable("LOWFIRE_ECONOMY_RESULTS")??throw new InvalidOperationException("Explicit QA output required");private static readonly List<string> Checks=new List<string>();
        static StoreQa(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&UnityEditor.SessionState.GetBool("Lowfire.StoreQa",false)){UnityEditor.SessionState.SetBool("Lowfire.StoreQa",false);var host=new GameObject("Store isolated QA").AddComponent<DevelopmentCoroutineHost>();host.StartCoroutine(Guard(UnityEditor.SessionState.GetBool("Lowfire.StoreActualQa",false)?Actual():Visual()));}};}
        public static void Validate(){if(!Application.isBatchMode)throw new InvalidOperationException("Batch only");var errors=ProjectDataValidator.Validate();StoreBuildValidator.Validate(errors);var s=StoreSettings.Load();if(s.PurchaseEnabled||s.ProductionLiveEnabled||s.ExternalSetupReviewed||BackendPolicy.RealMoneyPurchase)errors.Add("Committed purchase policy must remain OFF");var output=Environment.GetEnvironmentVariable("LOWFIRE_BACKEND_RESULTS");Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"data-validator.json"),JsonUtility.ToJson(new Result{Passed=errors.Count==0,Checks=errors.ToArray()},true));if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));}
        public static void VisualBatch(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");Directory.CreateDirectory(Root);UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",Path.Combine(Path.GetTempPath(),"LowfireStoreQA",Guid.NewGuid().ToString("N")));UnityEditor.SessionState.SetBool("Lowfire.StoreQa",true);EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);Application.runInBackground=true;EditorApplication.update+=()=>{if(EditorApplication.isPlaying)EditorApplication.QueuePlayerLoopUpdate();};EditorApplication.isPlaying=true;}
        public static void ActualBatch(){UnityEditor.SessionState.SetBool("Lowfire.StoreActualQa",true);VisualBatch();}
        private static IEnumerator Actual()
        {
            UnityEditor.SessionState.SetBool("Lowfire.StoreActualQa",false);if(!DevelopmentProfile.Isolated||!BackendPolicy.Fixtures)throw new InvalidOperationException("Isolated Development required");
            yield return Await(UgsBootstrap.EnsureAnonymous("iap-"+Guid.NewGuid().ToString("N").Substring(0,20)));File.WriteAllText(Path.Combine(Root,"player-id.txt"),Unity.Services.Authentication.AuthenticationService.Instance.PlayerId);
            foreach(var operation in new[]{"GetStoreState","BeginPurchase","VerifyPurchase","RecoverPurchases","Consume"})
            {
                var endpoint=operation=="Consume"?"LowfirePlayVerify":"LowfirePurchase";
                var task=Unity.Services.CloudCode.CloudCodeService.Instance.CallEndpointAsync<StoreReply>(endpoint,new Dictionary<string,object>{{"operation",operation},{"channel","Development"},{"buildVersion",Application.version},{"input",new Dictionary<string,object>{{"operationId",Guid.NewGuid().ToString("N")},{"productId","lowfire_gems_24"},{"catalogRevision",1},{"token","isolated-non-google-proof"},{"verified",true}}}});
                yield return Await(task);Check(!task.Result.ok&&task.Result.error=="purchase_account_required","actual UGS anonymous request blocked before Google: "+operation+" / "+task.Result.error);
            }
            File.WriteAllText(Path.Combine(Root,"actual-iap.json"),"{\"ActualUgs\":true,\"ActualGooglePurchase\":false,\"AnonymousNegativeOnly\":true}");Unity.Services.Authentication.AuthenticationService.Instance.SignOut(true);
        }
        [Serializable]private sealed class Result{public bool Passed;public string[] Checks;}
        private static void Check(bool ok,string label){Checks.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(Path.Combine(Root,"store-checks.txt"),Checks);if(!ok)throw new InvalidOperationException(label);}
        private static IEnumerator Guard(IEnumerator routine){var stack=new Stack<IEnumerator>();stack.Push(routine);while(stack.Count>0){bool more;object current;try{more=stack.Peek().MoveNext();current=more?stack.Peek().Current:null;}catch(Exception e){File.WriteAllText(Path.Combine(Root,"failed.txt"),e.ToString());EditorApplication.Exit(1);yield break;}if(!more){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;}File.WriteAllText(Path.Combine(Root,"checks.json"),JsonUtility.ToJson(new Result{Passed=true,Checks=Checks.ToArray()},true));EditorApplication.Exit(0);}
        private static IEnumerator Await(Task task){var end=Time.realtimeSinceStartup+60;while(!task.IsCompleted){if(Time.realtimeSinceStartup>end)throw new TimeoutException("Fixture timeout");yield return null;}if(task.IsFaulted)throw task.Exception;}
        private static IEnumerator Frames(GameObject view,string name){foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1280,720),new Vector2Int(844,390)})foreach(var language in new[]{"ko","en"}){L.SetLanguage(language);yield return Frame(view,size,name+"-"+language);var screen=UnityEngine.Object.FindFirstObjectByType<StoreScreen>();if(view==screen.View){var local=screen.Status.rectTransform.anchoredPosition;Check(local.y>-350,"status stays inside readable paper area "+name);Check(screen.Purchase.interactable==false||screen.Cards[0].gameObject.activeInHierarchy,"purchase requires visible cards "+name);}}}
        private static IEnumerator Frame(GameObject view,Vector2Int size,string name)=>(IEnumerator)typeof(EconomyQa).GetMethod("Frame",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{view,size,name});
        private static IEnumerator Visual()
        {
            foreach(var input in UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.BaseInputModule>(FindObjectsSortMode.None))input.enabled=false;
            var identity=(IAccountIdentityProvider)Activator.CreateInstance(typeof(EconomyQa).GetNestedType("Identity",BindingFlags.NonPublic),new object[]{false});var cloud=Activator.CreateInstance(typeof(EconomyQa).GetNestedType("VisualCloud",BindingFlags.NonPublic),true);var account=AccountService.InstallDevelopment(identity,(IAccountCloudTransport)cloud);var economy=EconomyService.InstallDevelopment(account,(IEconomyTransport)cloud);yield return Await(account.Connect());yield return Await(economy.Sync());Check(account.State==AccountConnection.Connected,"isolated fixture account connected");
            var billing=new DevelopmentStoreBilling();var transport=new DevelopmentStoreTransport(account.Profile.PlayerId,BackendSettings.Current.Environment.Id);transport.Grant=amount=>{var field=cloud.GetType().GetField("AdGem");field.SetValue(cloud,(int)field.GetValue(cloud)+amount);};var service=StoreService.InstallDevelopment(account,transport,billing);var screen=UnityEngine.Object.FindFirstObjectByType<StoreScreen>();Check(screen!=null&&screen.Entry.onClick.GetPersistentEventCount()==1,"authored Store entry uses existing Profile hierarchy");
            screen.Meta.Home.ShowHome();yield return Frames(screen.Meta.Home.Menu,"main-menu");screen.Meta.OpenHub();yield return Frames(screen.Meta.HubView,"profile-store-entry");
            transport.HeldState=new TaskCompletionSource<StoreReply>();screen.Open();yield return Frames(screen.View,"store-loading");var hold=transport.HeldState;transport.HeldState=null;hold.SetResult(transport.Call("GetStoreState",new Dictionary<string,object>()).Result);while(service.Busy)yield return null;Check(service.CanBuy,"fixture platform metadata enables selected purchase only");yield return Frames(screen.View,"store-ready");screen.Select(0);yield return Frames(screen.View,"store-selection");
            billing.Next=BillingOutcome.Pending;transport.Pending=true;yield return Await(service.Purchase("lowfire_gems_24"));Check(service.Phase==StorePhase.Pending&&transport.Grants==0,"pending payment grants nothing");yield return Frames(screen.View,"store-pending");transport.Pending=false;yield return Await(service.Recover());Check(transport.Grants==1&&economy.Snapshot.Balance==164,"fixture verification refreshes canonical wallet transport once");yield return Frames(screen.View,"store-success-fixture");
            billing.Purchases.Clear();billing.Next=BillingOutcome.Cancelled;yield return Await(service.Purchase("lowfire_gems_24"));Check(service.Phase==StorePhase.Cancelled&&!service.Busy,"cancellation is recoverable normal state");yield return Frames(screen.View,"store-cancelled");billing.Next=BillingOutcome.Failed;yield return Await(service.Purchase("lowfire_gems_24"));Check(!service.Busy,"failed billing releases single purchase gate");yield return Frames(screen.View,"store-failed");
            transport.Available=false;yield return Await(service.Open());Check(service.Prices.Length==0&&!service.CanBuy,"unavailable state removes stale prices");yield return Frames(screen.View,"store-unavailable");transport.Available=true;billing.LongPrice="US$ 1,234,567.89";yield return Await(service.Open());yield return Frames(screen.View,"store-long-price");screen.ToggleHistory();yield return Frames(screen.View,"store-history");screen.ToggleHistory();
            account.KeepLocal();yield return Await(service.Open());Check(!service.CanBuy,"offline account cannot purchase");yield return Frames(screen.View,"store-account-required");Check(BackendPolicy.Channel==BuildChannel.Development,"visual run is Development only");Check(transport.Grants==1,"opening/recovering never restores consumed Gem a second time");
        }
    }
}
