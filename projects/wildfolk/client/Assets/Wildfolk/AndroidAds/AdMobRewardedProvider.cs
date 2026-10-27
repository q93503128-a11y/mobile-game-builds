using System;
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
namespace Wildfolk.AndroidAds
{
    public sealed class AdMobRewardedProvider:IRewardedAdProvider
    {
        private readonly RewardedSettings settings;private readonly RewardedAd[] ads=new RewardedAd[2];private Task initialization;private static bool fullscreen;private Action<AdOutcome> finishActive;private bool initialized,showing,disposed;private int generation;
        public event Action Changed;public AdMobRewardedProvider(RewardedSettings s)=>settings=s;
        public bool Supported=>settings.Allowed&&Application.platform==RuntimePlatform.Android;
        public bool Ready(AdPlacement p)=>initialized&&!fullscreen&&!showing&&!disposed&&ads[(int)p]?.CanShowAd()==true;
        public bool PrivacyOptionsRequired=>settings.Allowed&&Application.platform==RuntimePlatform.Android&&ConsentInformation.PrivacyOptionsRequirementStatus==PrivacyOptionsRequirementStatus.Required;
        private Task Initialize()
        {
            if(initialization!=null)return initialization;if(!settings.Allowed||Application.platform!=RuntimePlatform.Android)return Task.CompletedTask;
            var t=new TaskCompletionSource<bool>();initialization=t.Task;MobileAds.RaiseAdEventsOnUnityMainThread=true;
            ConsentInformation.Update(new ConsentRequestParameters{TagForUnderAgeOfConsent=settings.UnderAgeOfConsent||settings.Audience=="Child"},error=>{
                if(disposed||error!=null){initialization=null;t.TrySetResult(false);Changed?.Invoke();return;}
                ConsentForm.LoadAndShowConsentFormIfRequired(formError=>{
                    if(disposed||formError!=null||!ConsentInformation.CanRequestAds()){initialization=null;t.TrySetResult(false);Changed?.Invoke();return;}
                    MobileAds.SetRequestConfiguration(new RequestConfiguration{TagForChildDirectedTreatment=settings.Audience=="Child"?TagForChildDirectedTreatment.True:TagForChildDirectedTreatment.False,TagForUnderAgeOfConsent=settings.UnderAgeOfConsent?TagForUnderAgeOfConsent.True:TagForUnderAgeOfConsent.False,MaxAdContentRating=settings.Audience=="Child"?MaxAdContentRating.G:MaxAdContentRating.T});
                    MobileAds.Initialize(_=>{initialized=!disposed;t.TrySetResult(initialized);Changed?.Invoke();});
                });
            });return initialization;
        }
        public async Task Prepare(AdPlacement p){if(disposed||showing||Ready(p))return;await Initialize();if(!initialized||disposed||showing||Ready(p))return;var t=new TaskCompletionSource<bool>();var token=generation;RewardedAd.Load(settings.Unit(p),new AdRequest(),(ad,error)=>{if(disposed||token!=generation||error!=null){ad?.Destroy();t.TrySetResult(false);return;}ads[(int)p]?.Destroy();ads[(int)p]=ad;t.TrySetResult(true);Changed?.Invoke();});await t.Task;}
        public Task<AdOutcome> Show(AdRequestContext context,Action earned)
        {
            if(!Ready(context.Placement)||string.IsNullOrEmpty(context.Operation))return Task.FromResult(AdOutcome.Unavailable);
            var ad=ads[(int)context.Placement];ads[(int)context.Placement]=null;showing=fullscreen=true;var token=++generation;var t=new TaskCompletionSource<AdOutcome>();var rewarded=false;var finished=false;
            void Finish(AdOutcome result){if(finished)return;finished=true;showing=fullscreen=false;finishActive=null;ad.Destroy();t.TrySetResult(result);Changed?.Invoke();}
            finishActive=Finish;
            ad.OnAdFullScreenContentClosed+=()=>Finish(rewarded?AdOutcome.Earned:AdOutcome.Cancelled);
            ad.OnAdFullScreenContentFailed+=_=>Finish(AdOutcome.Failed);
            if(context.Placement==AdPlacement.Gem){if(string.IsNullOrEmpty(context.UserId)||string.IsNullOrEmpty(context.CustomData)){Finish(AdOutcome.Failed);return t.Task;}ad.SetServerSideVerificationOptions(new ServerSideVerificationOptions{UserId=context.UserId,CustomData=context.CustomData});}
            try{ad.Show(reward=>{if(finished||disposed||token!=generation||rewarded)return;try{earned();rewarded=true;}catch{Finish(AdOutcome.Failed);}});}catch{Finish(AdOutcome.Failed);}return t.Task;
        }
        public void PrivacyOptions(){if(PrivacyOptionsRequired)ConsentForm.ShowPrivacyOptionsForm(_=>{initialized=false;initialization=null;foreach(var ad in ads)ad?.Destroy();Array.Clear(ads,0,ads.Length);Changed?.Invoke();});}
        public void Dispose(){disposed=true;generation++;finishActive?.Invoke(AdOutcome.Failed);foreach(var ad in ads)ad?.Destroy();Array.Clear(ads,0,ads.Length);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]private static void Register(){RewardedAds.ProviderFactory=()=>new AdMobRewardedProvider(RewardedSettings.Load());}
    }
}
