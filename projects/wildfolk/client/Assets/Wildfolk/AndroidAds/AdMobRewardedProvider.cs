using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
namespace Wildfolk.AndroidAds
{
    public sealed class AdMobRewardedProvider:IRewardedAdProvider
    {
        private const int LoadTimeoutMilliseconds=20000;
        private readonly RewardedSettings settings;
        private readonly RewardedAd[] ads=new RewardedAd[2];
        private readonly Task[] loading=new Task[2];
        private Task initialization;private TaskCompletionSource<bool> initializing;
        private static bool fullscreen;
        private Action finishActive;
        private bool initialized,showing,disposed;
        private int generation,initGeneration;
        public event Action Changed;
        public AdMobRewardedProvider(RewardedSettings settings)=>this.settings=settings;
        public bool Supported=>settings.Allowed&&Application.platform==RuntimePlatform.Android;
        public bool Ready(AdPlacement placement)=>initialized&&!fullscreen&&!showing&&!disposed&&ads[(int)placement]?.CanShowAd()==true;
        public bool PrivacyOptionsRequired=>Supported&&ConsentInformation.PrivacyOptionsRequirementStatus==PrivacyOptionsRequirementStatus.Required;
        private static void Dispatch(Action action){if(MobileAdsEventExecutor.IsOnMainThread())action();else MobileAdsEventExecutor.ExecuteInUpdate(action);}
        private Task Initialize()
        {
            if(initialization!=null)return initialization;
            if(!Supported||disposed)return Task.CompletedTask;
            MobileAdsEventExecutor.Initialize();
            var source=new TaskCompletionSource<bool>();initializing=source;
            var token=++initGeneration;initialization=FinishInitialization(source,token);
            ConsentInformation.Update(new ConsentRequestParameters{TagForUnderAgeOfConsent=settings.UnderAgeOfConsent||settings.Audience=="Child"},error=>Dispatch(()=>{
                if(disposed||token!=initGeneration||error!=null){source.TrySetResult(false);return;}
                ConsentForm.LoadAndShowConsentFormIfRequired(formError=>Dispatch(()=>{
                    if(disposed||token!=initGeneration||formError!=null||!ConsentInformation.CanRequestAds()){source.TrySetResult(false);return;}
                    MobileAds.SetRequestConfiguration(new RequestConfiguration{
                        TagForChildDirectedTreatment=settings.Audience=="Child"?TagForChildDirectedTreatment.True:TagForChildDirectedTreatment.False,
                        TagForUnderAgeOfConsent=settings.UnderAgeOfConsent||settings.Audience=="Child"?TagForUnderAgeOfConsent.True:TagForUnderAgeOfConsent.False,
                        MaxAdContentRating=settings.Audience=="Child"?MaxAdContentRating.G:MaxAdContentRating.T,
                        TestDeviceIds=settings.UsesOwnedTestUnits?new List<string>(settings.TestDeviceIds):new List<string>()});
                    MobileAds.Initialize(_=>Dispatch(()=>source.TrySetResult(!disposed&&token==initGeneration)));
                }));
            }));
            return initialization;
        }
        private async Task FinishInitialization(TaskCompletionSource<bool> source,int token)
        {
            await Task.WhenAny(source.Task,Task.Delay(LoadTimeoutMilliseconds));
            if(disposed||token!=initGeneration)return;
            initialized=source.Task.IsCompleted&&source.Task.Result;
            if(!initialized){initGeneration++;initialization=null;}
            initializing=null;Changed?.Invoke();
        }
        public async Task Prepare(AdPlacement placement)
        {
            var index=(int)placement;
            if(disposed||showing||fullscreen||Ready(placement))return;
            if(loading[index]!=null){await loading[index];return;}
            var pending=new TaskCompletionSource<bool>();loading[index]=pending.Task;
            try{
                await Initialize();
                if(!initialized||disposed||showing||fullscreen||Ready(placement)||!TestRequestAllowed())return;
                var source=new TaskCompletionSource<bool>();var token=generation;
                RewardedAd.Load(settings.Unit(placement),new AdRequest(),(ad,error)=>Dispatch(()=>{
                    if(disposed||token!=generation||error!=null||source.Task.IsCompleted){ad?.Destroy();source.TrySetResult(false);return;}
                    ads[index]?.Destroy();ads[index]=ad;source.TrySetResult(true);Changed?.Invoke();
                }));
                await Task.WhenAny(source.Task,Task.Delay(LoadTimeoutMilliseconds));
                source.TrySetResult(false); // a late load cannot resurrect an expired request
            }finally{loading[index]=null;pending.TrySetResult(true);}
        }
        public Task<AdOutcome> Show(AdRequestContext context,Action earned)
        {
            if(!Ready(context.Placement)||string.IsNullOrEmpty(context.Operation)||!TestRequestAllowed())return Task.FromResult(AdOutcome.Unavailable);
            var ad=ads[(int)context.Placement];ads[(int)context.Placement]=null;
            showing=fullscreen=true;var token=++generation;var source=new TaskCompletionSource<AdOutcome>();
            var completion=new RewardedCompletion(earned,result=>{
                showing=fullscreen=false;finishActive=null;ad.Destroy();source.TrySetResult(result);Changed?.Invoke();});
            finishActive=completion.Fail;
            ad.OnAdFullScreenContentClosed+=()=>Dispatch(completion.Close);
            ad.OnAdFullScreenContentFailed+=_=>Dispatch(completion.Fail);
            try{
                if(context.Placement==AdPlacement.Gem){
                    if(string.IsNullOrEmpty(context.UserId)||string.IsNullOrEmpty(context.CustomData)){completion.Fail();return source.Task;}
                    ad.SetServerSideVerificationOptions(new ServerSideVerificationOptions{UserId=context.UserId,CustomData=context.CustomData});
                }
                ad.Show(_=>Dispatch(()=>{if(disposed||token!=generation)completion.Fail();else completion.Earn();}));
            }catch{completion.Fail();}
            return source.Task;
        }
        private bool TestRequestAllowed()
        {
            if(!settings.UsesOwnedTestUnits)return true;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Public AdRequest.isTestDevice(Context), verified against Google's
            // pinned play-services-ads-api25.5.0 AAR. Building this probe sends no ad request.
            try{using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=unity.GetStatic<AndroidJavaObject>("currentActivity"))
                using(var builder=new AndroidJavaObject("com.google.android.gms.ads.AdRequest$Builder"))
                using(var request=builder.Call<AndroidJavaObject>("build"))return request.Call<bool>("isTestDevice",activity);
            }catch{return false;}
#else
            return false;
#endif
        }
        public void PrivacyOptions()
        {
            if(!PrivacyOptionsRequired||showing||fullscreen)return;
            ConsentForm.ShowPrivacyOptionsForm(_=>Dispatch(()=>{
                initGeneration++;initializing?.TrySetResult(false);initializing=null;initialized=false;initialization=null;
                foreach(var ad in ads)ad?.Destroy();Array.Clear(ads,0,ads.Length);Changed?.Invoke();}));
        }
        public void Dispose(){disposed=true;generation++;initGeneration++;initializing?.TrySetResult(false);finishActive?.Invoke();foreach(var ad in ads)ad?.Destroy();Array.Clear(ads,0,ads.Length);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]private static void Register(){RewardedAds.ProviderFactory=()=>new AdMobRewardedProvider(RewardedSettings.Load());}
    }
}
