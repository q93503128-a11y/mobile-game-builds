using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
namespace Wildfolk
{
    public static class AndroidAccountProviders
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]private static void Register()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidAccountBridge.AuthorizationCode=Code;AndroidAccountBridge.IntegrityToken=Integrity;
#endif
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        private static Task<string> Code()
        {
            var source=new TaskCompletionSource<string>();PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(status=>{
                if(status!=SignInStatus.Success){source.TrySetException(new InvalidOperationException("Platform sign-in unavailable"));return;}
                PlayGamesPlatform.Instance.RequestServerSideAccess(false,code=>{if(string.IsNullOrEmpty(code))source.TrySetException(new InvalidOperationException("Platform sign-in unavailable"));else source.TrySetResult(code);});
            });return Timeout(source.Task,60000);
        }
        [UnityEngine.Scripting.Preserve]private sealed class Success:AndroidJavaProxy
        {private readonly Action<AndroidJavaObject> action;public Success(Action<AndroidJavaObject> action):base("com.google.android.gms.tasks.OnSuccessListener"){this.action=action;}[UnityEngine.Scripting.Preserve]public void onSuccess(AndroidJavaObject value)=>action(value);}
        [UnityEngine.Scripting.Preserve]private sealed class Failure:AndroidJavaProxy
        {private readonly Action action;public Failure(Action action):base("com.google.android.gms.tasks.OnFailureListener"){this.action=action;}[UnityEngine.Scripting.Preserve]public void onFailure(AndroidJavaObject value)=>action();}
        private static async Task<T> Timeout<T>(Task<T> task,int milliseconds)
        {if(await Task.WhenAny(task,Task.Delay(milliseconds))!=task)throw new TimeoutException();return await task;}
        private static async Task<string> Integrity(string hash)
        {
            if(!long.TryParse(BackendSettings.Current.GoogleCloudProjectNumber,out var project)||project<=0)throw new InvalidOperationException("Build verification unavailable");
            var source=new TaskCompletionSource<string>();
            using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity=unity.GetStatic<AndroidJavaObject>("currentActivity");activity.Call("runOnUiThread",new AndroidJavaRunnable(()=>{
                    try{
                        var factory=new AndroidJavaClass("com.google.android.play.core.integrity.IntegrityManagerFactory");var manager=factory.CallStatic<AndroidJavaObject>("createStandard",activity);
                        var prepare=new AndroidJavaClass("com.google.android.play.core.integrity.StandardIntegrityManager$PrepareIntegrityTokenRequest").CallStatic<AndroidJavaObject>("builder").Call<AndroidJavaObject>("setCloudProjectNumber",project).Call<AndroidJavaObject>("build");
                        manager.Call<AndroidJavaObject>("prepareIntegrityToken",prepare).Call<AndroidJavaObject>("addOnSuccessListener",new Success(provider=>{
                            var request=new AndroidJavaClass("com.google.android.play.core.integrity.StandardIntegrityManager$StandardIntegrityTokenRequest").CallStatic<AndroidJavaObject>("builder").Call<AndroidJavaObject>("setRequestHash",hash).Call<AndroidJavaObject>("build");
                            provider.Call<AndroidJavaObject>("request",request).Call<AndroidJavaObject>("addOnSuccessListener",new Success(token=>source.TrySetResult(token.Call<string>("token")))).Call<AndroidJavaObject>("addOnFailureListener",new Failure(()=>source.TrySetException(new InvalidOperationException("Build verification unavailable"))));
                        })).Call<AndroidJavaObject>("addOnFailureListener",new Failure(()=>source.TrySetException(new InvalidOperationException("Build verification unavailable"))));
                    }catch{source.TrySetException(new InvalidOperationException("Build verification unavailable"));}
                }));
            }
            return await Timeout(source.Task,60000);
        }
#endif
    }
}
