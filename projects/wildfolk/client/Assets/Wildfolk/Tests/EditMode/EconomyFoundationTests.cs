using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class EconomyFoundationTests
    {
        private string root;[SetUp]public void Setup(){root=Path.Combine(Path.GetTempPath(),"LowfireEconomyUnit-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);}
        [TearDown]public void Cleanup(){Directory.Delete(root,true);}
        private EconomyJournal Open(string name="account")=>new EconomyJournal(Path.Combine(root,name+".json"),name);
        [Test]public void DurableAggregatesSurviveRestartAndAckOnlyTheirFrozenRange()
        {var j=Open();for(var i=0;i<250;i++)Assert.That(j.Record("harvests",1,"node:"+i),Is.True);var b=j.Freeze();Assert.That(b.Metrics.Single().Value,Is.EqualTo(250));Assert.That(j.State.Outbox.Count,Is.EqualTo(1));j.Record("fish",1,"fish:1");j=Open();Assert.That(j.Freeze().Operation,Is.EqualTo(b.Operation));j.Acknowledge(b.Last);Assert.That(j.Freeze().Metrics.Single().Id,Is.EqualTo("fish"));Assert.That(j.State.Next,Is.EqualTo(252));Assert.That(j.State.Revive,Is.Null);}
        [Test]public void ReHostBossAndRespecSourcesNeverRepeatEvenAfterRecentQueueEviction()
        {var j=Open();Assert.That(j.Record("bosses",1,"world:king:1"),Is.True);Assert.That(j.Record("bosses",1,"world:king:1"),Is.False);Assert.That(j.Record("class_nodes",1,"survivor:A1"),Is.True);Assert.That(Open().Record("class_nodes",1,"survivor:A1"),Is.False);Assert.That(j.Record("bosses",50,"impossible"),Is.False);}
        [Test]public void BiomeMaskIncludesRockyBitAndRepeatedVisitsDoNotGrowTheCount()
        {var j=Open();foreach(var bit in new[]{1,2,4,8})Assert.That(j.Record("biomes",bit,"biome:"+bit),Is.True);Assert.That(j.State.Totals.Single().Value,Is.EqualTo(15));Assert.That(j.Record("biomes",8,"another visit"),Is.False);Assert.That(j.Freeze().Metrics.Single().Value,Is.EqualTo(15));}
        [Test]public void FailedDurableWriteRollsBackMemoryAndDoesNotCreateProgress()
        {var j=Open();Directory.CreateDirectory(j.Path+".temporary");Assert.Throws<UnauthorizedAccessException>(()=>j.Record("fish",1,"f"));Assert.That(j.State.Next,Is.EqualTo(1));Assert.That(j.State.Totals,Is.Empty);}
        [Test]public void GuestAdoptionIsBoundToOneAccountAndPreservesDeviceSequence()
        {var guest=Open("guest");guest.Record("fish",3,"catch");var device=guest.State.Device;var a=Open("a");a.Adopt(guest);var b=Open("b");b.Adopt(Open("guest"));Assert.That(a.State.Device,Is.EqualTo(device));Assert.That(a.Freeze().Metrics.Single().Value,Is.EqualTo(3));Assert.That(b.Freeze(),Is.Null);Assert.That(Open("a").State.AdoptedBy,Is.EqualTo("complete"));}
        [Test]public void CorruptPrimaryUsesVerifiedBackupAndDoubleCorruptionFailsClosed()
        {var j=Open();j.Record("fish",1,"one");j.Record("fish",1,"two");File.WriteAllText(j.Path,"broken");Assert.That(Open().State.Next,Is.EqualTo(2));File.WriteAllText(j.Path+".backup","broken");Assert.Throws<IOException>(()=>Open());}
        [Test]public void ReviveRecoveryUsesPrimaryWorldIdentityAndNeverRefundsUnknownDiskOutcome()
        {var path=Path.Combine(root,"world.json");var i=new GemReviveIntent{WorldPath=path,Run="run",Death=1,Stage="CommitStarted"};Assert.That(CloudGemReviveWallet.DiskOutcome(i),Is.Null);var g=new SavedGame{Player=new PlayerSnapshot{Death=new DeathSnapshot{Run="run",Death=1,Dead=true}}};File.WriteAllText(path,SaveRepository.Encode(g));Assert.That(CloudGemReviveWallet.DiskOutcome(i),Is.EqualTo("failed"));g.Player.Death.Dead=false;File.WriteAllText(path,SaveRepository.Encode(g));Assert.That(CloudGemReviveWallet.DiskOutcome(i),Is.EqualTo("committed"));g.Player.Death.Run="next run";File.WriteAllText(path,SaveRepository.Encode(g));Assert.That(CloudGemReviveWallet.DiskOutcome(i),Is.Null);i.Stage="Committed";Assert.That(CloudGemReviveWallet.DiskOutcome(i),Is.EqualTo("committed"));}
        [Test]public void CataloguePricesAndFiniteRewardBudgetAreConsistent()
        {var c=EconomyCatalog.Current;Assert.That(c.Families.Length,Is.EqualTo(22));Assert.That(c.Families.Sum(f=>f.Tiers.Length),Is.EqualTo(58));Assert.That(c.Families.Sum(f=>f.Tiers.Sum(t=>t.GemReward)),Is.EqualTo(213));Assert.That(c.Newcomer.Sum(),Is.EqualTo(45));Assert.That(c.Monthly.Sum(),Is.EqualTo(40));Assert.That(ClassCatalog.Current.Find("engineer").Price,Is.EqualTo(c.EngineerCost));}
    }
}
