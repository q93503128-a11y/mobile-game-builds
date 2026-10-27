using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class RewardedAdsPlayTests
    {
        private string root,previous;private AccountService account;private Cloud cloud;private RewardedAds ads;private DevelopmentRewardedAdProvider provider;
        private sealed class Identity:IAccountIdentityProvider{public string PlayerId=>"ads-isolated-player";public bool Linked=>true;public Task Connect()=>Task.CompletedTask;public Task UseExisting()=>Task.CompletedTask;public void SignOut(bool clearSession=false){}}
        private sealed class Cloud:IAccountCloudTransport,IEconomyTransport,IRewardedTransport
        {
            public bool Verified;public int Calls;public TaskCompletionSource<RewardedReply> Delay;
            Task<AccountReply> IAccountCloudTransport.Call(string op,Dictionary<string,object> input)=>Task.FromResult(new AccountReply{ok=true,profile=new CloudAccountProfile{Schema=1,PlayerId="ads-isolated-player",SupportId="LF-1234-5678-90AB-CDEF-1234",CreatedAt="2026-10-27T00:00:00Z",ProviderLinked=true,ClassProfile=CloudClassProfile.From(AccountProgression.New()),Entitlements=Array.Empty<AccountEntitlement>()}});
            Task<EconomyReply> IEconomyTransport.Call(string op,Dictionary<string,object> input)=>Task.FromResult(new EconomyReply{ok=true});
            public Task<RewardedReply> Call(string op,Dictionary<string,object> input){Calls++;if(Delay!=null)return Delay.Task;var o=new AdOperation{OperationId="11111111111111111111111111111111",Nonce=new string('a',64),State=Verified?"Granted":"Reserved",Amount=1};return Task.FromResult(new RewardedReply{ok=true,operation=op=="GetRewardedState"?null:o,state=op=="GetRewardedState"?new RewardedState{Schema=1,PlayerId="ads-isolated-player",EnvironmentId=BackendSettings.Current.Environment.Id,Remaining=Verified?0:1,Eligible=!Verified,GemPerRewardedAd=1}:null});}
        }
        [SetUp]public void Setup(){previous=UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","");root=Path.Combine(Path.GetTempPath(),"LowfireRewardedPlay-"+Guid.NewGuid().ToString("N"));UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",root);ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(Path.Combine(root,"guest")));cloud=new Cloud();account=AccountService.InstallDevelopment(new Identity(),cloud);EconomyService.InstallDevelopment(account,cloud);account.Connect().GetAwaiter().GetResult();provider=new DevelopmentRewardedAdProvider();ads=new RewardedAds(account,cloud,provider);}
        [TearDown]public void Cleanup(){ads.Dispose();EconomyService.ClearDevelopment();ClassProfiles.AttachCloud(null);UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",previous);if(Directory.Exists(root))Directory.Delete(root,true);}
        [UnityTest]public IEnumerator CancelAndNoFillConsumeNoRewardAndCanRetry(){foreach(var result in new[]{AdOutcome.Cancelled,AdOutcome.Unavailable,AdOutcome.Failed}){provider.Outcomes.Enqueue(result);var t=ads.WatchGem();while(!t.IsCompleted)yield return null;Assert.That(ads.Pending,Is.False);Assert.That(cloud.Verified,Is.False);}Assert.That(provider.Shows,Is.EqualTo(3));Assert.That(ads.LastError,Is.EqualTo("ad_failed"));}
        [UnityTest]public IEnumerator EarnedCallbackWaitsForServerAndParallelShowIsRejected(){provider.Outcomes.Enqueue(AdOutcome.Cancelled);cloud.Delay=new TaskCompletionSource<RewardedReply>();var t=ads.WatchGem();var other=ads.WatchGem();Assert.That(other.IsCompleted,Is.True);Assert.That(provider.Shows,Is.Zero);account.SignOut();cloud.Delay.SetResult(new RewardedReply{ok=true});while(!t.IsCompleted)yield return null;Assert.That(provider.Shows,Is.Zero);Assert.That(ads.State,Is.Null);}
        [UnityTest]public IEnumerator OnlyVerifiedServerStateReconcilesAndDisablesFurtherDailyShows(){provider.Outcomes.Enqueue(AdOutcome.Earned);cloud.Verified=true;var t=ads.WatchGem();while(!t.IsCompleted)yield return null;Assert.That(ads.Pending,Is.False);Assert.That(ads.State.Remaining,Is.Zero);Assert.That(ads.CanGem,Is.False);}
        [UnityTest]public IEnumerator EarnedReviveRetriesWithoutAnotherAdUntilWorldSaveSucceeds(){provider.Outcomes.Enqueue(AdOutcome.Cancelled);var success=true;ads.Show("run:1:ad",x=>success=x);yield return null;Assert.That(success,Is.False);provider.Outcomes.Enqueue(AdOutcome.Earned);ads.Show("run:1:ad",x=>success=x);yield return null;Assert.That(success,Is.True);ads.Resolve("run:1:ad",false);var shows=provider.Shows;provider.Available=false;ads.Show("run:1:ad",x=>success=x);Assert.That(success,Is.True);Assert.That(provider.Shows,Is.EqualTo(shows));Assert.That(ads.AvailableFor("new-run:1:ad"),Is.False);ads.Resolve("run:1:ad",true);Assert.That(ads.AvailableFor("run:1:ad"),Is.False);}
    }
}
