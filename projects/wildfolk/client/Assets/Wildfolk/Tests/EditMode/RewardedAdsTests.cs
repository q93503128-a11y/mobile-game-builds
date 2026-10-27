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
    }
}
