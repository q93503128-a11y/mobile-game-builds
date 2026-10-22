using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ClassCorrectionTests
    {
        private const string Host="aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",Peer="bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        private sealed class MemoryStore:IAccountProgressionStore
        {
            public string Data=JsonUtility.ToJson(AccountProgression.New());public int Writes;public bool Fail;
            public AccountProgression Read(string id)=>JsonUtility.FromJson<AccountProgression>(Data);
            public bool Write(string id,AccountProgression d){if(Fail)return false;d.Validate();Data=JsonUtility.ToJson(d);Writes++;return true;}
        }
        private MemoryStore store;
        [SetUp]public void Start(){store=new MemoryStore();ClassProfiles.DevelopmentProviders(store);}
        [TearDown]public void Stop()=>ClassProfiles.DevelopmentProviders(null);
        private static ClassAdmission Claim(string id="engineer",int count=6)=>new ClassAdmission{Profile=Peer,Selected=id,Owned=true,XP=999999,Nodes=new[]{"A1","A2","A3","B1","B2","B3","C1","C2","C3"}.Take(count).ToArray()};
        [TestCase("engineer")][TestCase("warrior")][TestCase("medic")]
        public void ReleaseIgnoresForgedPremiumGoldXPAndAllNodes(string id)
        {var locks=new ClassAdmissionLocks();Assert.That(locks.Admit(Host,Claim(id,9),false,new SurvivorClassAdmission(),out var s),Is.True);Assert.That(s.Account.Selected,Is.EqualTo("survivor"));Assert.That(s.Account.Classes.All(p=>p.XP==0 && p.Nodes.Length==0),Is.True);Assert.That(s.Account.Find(id).Owned,Is.False);Assert.That(store.Writes,Is.Zero);Assert.That(s.Persistent,Is.False);}
        [Test]public void HostUsesItsOwnStoreAndRejectsPeerImpersonation()
        {var d=AccountProgression.New();d.Selected="medic";d.Find("medic").Owned=true;d.Find("medic").XP=1800;d.Find("medic").Nodes=new[]{"A1"};store.Write(Host,d);var c=Claim();c.Profile=Host;var locks=new ClassAdmissionLocks();Assert.That(locks.Admit(Host,c,true,new SurvivorClassAdmission(),out var s),Is.True);Assert.That(s.Account.Selected,Is.EqualTo("medic"));Assert.That(s.Account.Find("medic").XP,Is.EqualTo(1800));Assert.That(s.Persistent,Is.True);Assert.That(locks.Admit(Host,c,false,new DevelopmentAlphaClassAdmission(),out _),Is.False);}
        [Test]public void ExplicitDevelopmentAdmissionIsEphemeralAndReconnectLocked()
        {var c=Claim();var locks=new ClassAdmissionLocks();Assert.That(locks.Admit(Host,c,false,new DevelopmentAlphaClassAdmission(),out var s),Is.True);Assert.That(s.Account.Selected,Is.EqualTo("engineer"));Assert.That(s.Grant(Peer,"engineer",8,"death1"),Is.True);c.Selected="medic";c.Nodes=Array.Empty<string>();Assert.That(locks.Admit(Host,c,false,new DevelopmentAlphaClassAdmission(),out var reconnect),Is.True);Assert.That(reconnect,Is.SameAs(s));Assert.That(reconnect.Account.Selected,Is.EqualTo("engineer"));Assert.That(reconnect.Grant(Peer,"engineer",8,"death1"),Is.False);Assert.That(reconnect.Account.Find("engineer").XP,Is.EqualTo(1000007));Assert.That(store.Writes,Is.Zero);locks.Clear();Assert.That(locks.Admit(Host,c,false,new SurvivorClassAdmission(),out var fresh),Is.True);Assert.That(fresh.Account.Selected,Is.EqualTo("survivor"));}
        [Test]public void DevelopmentStillRejectsInvalidAllocation()
        {Assert.That(new DevelopmentAlphaClassAdmission().Verify(Claim(count:9),out _),Is.False);var c=Claim();c.Nodes=new[]{"A2"};Assert.That(new DevelopmentAlphaClassAdmission().Verify(c,out _),Is.False);}
        [Test]public void TenThousandRewardsHaveBoundedRuntimeAndProfileSize()
        {var s=new ClassProgressSession(store.Read(Host),true);var before=Encoding.UTF8.GetByteCount(store.Data);for(var i=0;i<10000;i++)Assert.That(s.Grant(Host,"survivor",1,"death-"+i),Is.True);Assert.That(s.Rewards.Count,Is.EqualTo(RewardReceiptWindow.Capacity));Assert.That(s.Grant(Host,"survivor",1,"death-9999"),Is.False);Assert.That(store.Read(Host).Find("survivor").XP,Is.EqualTo(10000));Assert.That(Encoding.UTF8.GetByteCount(store.Data)-before,Is.LessThan(16));Assert.That(store.Data,Does.Not.Contain("Receipts"));TestContext.WriteLine("10000 rewards; profile bytes "+before+"→"+Encoding.UTF8.GetByteCount(store.Data)+"; runtime receipts "+s.Rewards.Count);}
        [Test]public void FailedDurableWriteCanRetryWithoutLostReceiptOrXP()
        {var s=new ClassProgressSession(store.Read(Host),true);store.Fail=true;Assert.That(s.Grant(Host,"survivor",8,"d"),Is.False);Assert.That(s.Rewards.Count,Is.Zero);Assert.That(s.Account.Find("survivor").XP,Is.Zero);store.Fail=false;Assert.That(s.Grant(Host,"survivor",8,"d"),Is.True);Assert.That(s.Grant(Host,"survivor",8,"d"),Is.False);}
        [Test]public void ProfileOneReceiptMigrationPreservesPermanentProgress()
        {
            var dir=Path.Combine(Path.GetTempPath(),"LowfireMigration-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
            try
            {
                var d=AccountProgression.New();d.Selected="hunter";var p=d.Find("hunter");p.Owned=true;p.XP=1800;p.Nodes=new[]{"A1","A2"};var payload=JsonUtility.ToJson(d);payload=payload.Substring(0,payload.Length-1)+",\"Receipts\":[\"old-kill-1\",\"old-kill-2\"]}";
                string checksum;using(var hash=SHA256.Create())checksum=Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(payload)));
                File.WriteAllText(Path.Combine(dir,"class-"+Host+".json"),JsonUtility.ToJson(new LegacyEnvelope{Payload=payload,Checksum=checksum}));var disk=new LocalAccountProgressionStore(dir);var migrated=disk.Read(Host);
                Assert.That(migrated.Version,Is.EqualTo(1));Assert.That(migrated.Selected,Is.EqualTo("hunter"));Assert.That(migrated.Find("hunter").Nodes,Is.EqualTo(p.Nodes));Assert.That(migrated.Find("hunter").XP,Is.EqualTo(1800));Assert.That(migrated.Find("hunter").Owned,Is.True);Assert.That(disk.Write(Host,migrated),Is.True);Assert.That(File.ReadAllText(Path.Combine(dir,"class-"+Host+".json")),Does.Not.Contain("old-kill"));Assert.That(disk.Read(Host).Selected,Is.EqualTo("hunter"));
            }finally{Directory.Delete(dir,true);}
        }
        [Serializable]private sealed class LegacyEnvelope{public string Payload,Checksum;}
        [Test]public void MedicA2IsGenericCooldownAndKeepsA1A3()
        {var d=ClassCatalog.Current.Find("medic");Assert.That(d.Nodes[0].Effect,Is.EqualTo(ClassEffect.HealingOut));Assert.That(d.Nodes[1].Effect,Is.EqualTo(ClassEffect.MedicineCooldown));Assert.That(4*(1-d.Nodes[1].Amount),Is.EqualTo(3.2f).Within(.00001));Assert.That(d.Nodes[2].Effect,Is.EqualTo(ClassEffect.HotSpeed));}
    }
}
