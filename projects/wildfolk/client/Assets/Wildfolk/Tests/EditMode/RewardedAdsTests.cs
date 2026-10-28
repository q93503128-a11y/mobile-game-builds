using System;
using System.IO;
using NUnit.Framework;
namespace Wildfolk.Tests
{
    public sealed class RewardedAdsTests
    {
        private string root;[SetUp]public void Setup(){root=Path.Combine(Path.GetTempPath(),"LowfireAds-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);}
        [TearDown]public void Cleanup(){Directory.Delete(root,true);}
        private RewardedJournal Open(string scope="a")=>new RewardedJournal(Path.Combine(root,"ads.json"),scope);
        [Test]public void PendingRewardAndScopedEarnedReviveSurviveProcessRestart(){var j=Open();j.State.Pending=new AdOperation{OperationId="op",Nonce="nonce",State="Reserved"};j.State.ReviveEarned="run:1:ad";j.Save();var again=Open();Assert.That(again.State.Pending.OperationId,Is.EqualTo("op"));Assert.That(again.State.ReviveEarned,Is.EqualTo("run:1:ad"));Assert.That(again.State.Device,Is.EqualTo(j.State.Device));Assert.Throws<IOException>(()=>Open("other"));}
        [Test]public void CorruptPrimaryFallsBackAndUnrecoverableJournalFailsClosed(){var j=Open();j.Save();j.State.ReviveEarned="run:2:ad";j.Save();File.WriteAllText(j.Path,"bad");Assert.That(Open().State.ReviveEarned,Is.Null);File.WriteAllText(j.Path+".backup","bad");Assert.Throws<IOException>(()=>Open());}
        [Test]public void DefaultAndMissingLegalConfigurationNeverAllowAnAdRequest(){var s=RewardedSettings.Load();Assert.That(s.Enabled,Is.False);Assert.That(s.ProductionLiveEnabled,Is.False);Assert.That(s.Allowed,Is.False);s=new RewardedSettings{Enabled=true};Assert.That(s.Allowed,Is.False);Assert.That(RewardedSettings.ValidUnit(RewardedSettings.TestUnit),Is.False);}
        [Test]public void DisabledProviderNeverCallsEarnedAndHasNoImplicitFixture(){var p=new UnavailableRewardedProvider();var called=false;Assert.That(p.Show(new AdRequestContext{Placement=AdPlacement.Gem},()=>called=true).Result,Is.EqualTo(AdOutcome.Unavailable));Assert.That(called,Is.False);Assert.That(p.Ready(AdPlacement.Revive),Is.False);}

        [Test]public void NativeCompletionIgnoresDuplicateAndLateEvents(){int count=0,done=0;var outcome=AdOutcome.Failed;var gate=new RewardedCompletion(()=>count++,r=>{done++;outcome=r;});gate.Earn();gate.Earn();gate.Close();gate.Close();gate.Earn();gate.Fail();Assert.That(count,Is.EqualTo(1));Assert.That(done,Is.EqualTo(1));Assert.That(outcome,Is.EqualTo(AdOutcome.Earned));}
        [Test]public void ClosedBeforeEarnedAndFailedDurabilityNeverCountAsReward(){var count=0;var result=AdOutcome.Earned;var gate=new RewardedCompletion(()=>count++,r=>result=r);gate.Close();gate.Earn();Assert.That(count,Is.Zero);Assert.That(result,Is.EqualTo(AdOutcome.Cancelled));gate=new RewardedCompletion(()=>throw new IOException(),r=>result=r);gate.Earn();gate.Close();Assert.That(result,Is.EqualTo(AdOutcome.Failed));}
        [Test]public void ReplyRejectsWrongAccountInvalidQuotaAndMalformedServerTime(){var s=new RewardedState{Schema=1,PlayerId="p",EnvironmentId="e",GemPerRewardedAd=1,DailyRewardedGemLimit=1,Remaining=1,Eligible=true,ServerTime="2026-10-28T12:00:00Z",NextReset="2026-10-29T00:00:00Z"};Assert.DoesNotThrow(()=>s.Validate("p","e"));Assert.Throws<InvalidOperationException>(()=>s.Validate("other","e"));s.Remaining=2;Assert.Throws<InvalidOperationException>(()=>s.Validate("p","e"));s.Remaining=1;s.ServerTime="bad";Assert.Throws<InvalidOperationException>(()=>s.Validate("p","e"));}
        [Test]public void OversizedRetryQueueCannotBecomeAnUnreadableSavedJournal(){var j=Open();for(var i=0;i<33;i++)j.State.Waiting.Add(new AdOperation{OperationId=i.ToString()});Assert.Throws<IOException>(()=>j.Save());Assert.That(File.Exists(j.Path),Is.False);}
        [Test]public void OwnedTestPathIsOptInAndRequiresUnitsAndRegisteredDevices(){var s=RewardedSettings.Load();Assert.That(s.UseRegisteredTestUnits,Is.False);Assert.That(s.Unit(AdPlacement.Gem),Is.EqualTo(RewardedSettings.TestUnit));s=new RewardedSettings{Enabled=true,PrivacyReviewed=true,Audience="General",UseRegisteredTestUnits=true};Assert.That(s.Allowed,Is.False);s.GemAdUnit=s.ReviveAdUnit=RewardedSettings.TestUnit;s.TestDeviceIds=new[]{"B3EEABB8EE11C2BE770B684D95219ECB"};Assert.That(s.Allowed,Is.False,"official sample ID is not an owned-unit configuration");Assert.That(s.ProductionLiveEnabled,Is.False);}
    }
}
