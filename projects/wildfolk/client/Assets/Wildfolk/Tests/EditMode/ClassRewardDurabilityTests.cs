using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ClassRewardDurabilityTests
    {
        private const string Profile="eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
        private string directory;private IAccountProgressionStore previous;private LocalAccountProgressionStore disk;private SaveRepository world;
        private SavedGame before,after;private SaveGameRules rules;
        private sealed class FailingStore:IAccountProgressionStore
        {public IAccountProgressionStore Inner;public bool Fail;public AccountProgression Read(string p)=>Inner.Read(p);public bool Write(string p,AccountProgression d)=>!Fail && Inner.Write(p,d);}
        [SetUp]public void Prepare()
        {
            rules=Resources.Load<SaveGameRules>("SaveGameRules");directory=Path.Combine(Path.GetTempPath(),"LowfireBossDurability-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);previous=ClassProfiles.Store;
            disk=new LocalAccountProgressionStore(Path.Combine(directory,"profiles"));ClassProfiles.DevelopmentProviders(disk);var account=AccountProgression.New();account.Find("survivor").XP=1800;account.Find("survivor").Nodes=new[]{"A1"};Assert.That(disk.Write(Profile,account),Is.True);
            var map=new WorldMap(rules.World,rules.World.DefaultSeed,3,false);var player=rules.Player;var growth=player.GetComponent<PlayerProgression>().Definition;
            before=new SavedGame{Version=11,Seed=map.Seed,BaseGenerationVersion=3,Elapsed=301,Player=new PlayerSnapshot{Position=map.Spawn,HP=100,Inventory=Enumerable.Range(0,player.Settings.InventorySlots).Select(_=>new SavedStack()).ToArray(),Survival=new SurvivalModel(player.GetComponent<PlayerSurvival>().Definition).Capture(),Growth=GrowthValidation.FromLegacy(0,growth),Stamina=player.GetComponent<PlayerLocomotion>().Definition.MaximumStamina},Recovery=new ResourcePopulation(map).Capture(),AnimalEcology=new AnimalEcologySnapshot(),GoblinEcology=new GoblinEcologySnapshot()};rules.Validate(before);
            var tomb=map.Continental.Anchors.First(a=>a.Kind==WorldAnchorKind.Graveyard);var home=tomb.Position+rules.Bosses[0].SpawnOffset;before.Features=new[]{new WorldFeatureSnapshot{Id=tomb.Id+":guard",Visited=true,Cleared=true}};before.Bosses=new[]{new BossSnapshot{Boss="undead_king",Tomb=tomb.Id,LastNight=1,Active=true,HP=9000,Home=home,Position=home}};rules.Validate(before);
            after=SaveRepository.Decode(SaveRepository.Encode(before));after.Bosses[0].Active=false;after.Bosses[0].HP=0;after.Player.XP=120;after.Player.Gold=35;after.Player.Growth=GrowthValidation.FromLegacy(120,growth);rules.Validate(after);world=new SaveRepository(Path.Combine(directory,"world.json"));world.Write(before,rules.Validate);
        }
        [TearDown]public void Cleanup()
        {ClassProfiles.DevelopmentProviders(previous);if(directory!=null && Path.GetFullPath(directory).StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) && Directory.Exists(directory))Directory.Delete(directory,true);}
        private ClassXpIntent Intent()=>new ClassXpIntent{Receipt="undead_king:"+before.Bosses[0].Tomb+":1",ClassId="survivor",WorldPath=world.Path,Boss="undead_king",Tomb=before.Bosses[0].Tomb,Night=1,Seed=before.Seed,Generation=3,Experience=120};
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)]
        public void RestartAtEachPairCommitBoundaryRecoversExactlyOnce(int boundary)
        {
            var fail=new FailingStore{Inner=disk};if(boundary==3)ClassProfiles.DevelopmentProviders(fail);
            ClassRewardPersistence.ClassXpCommit ticket=null;
            if(boundary>0){ticket=ClassRewardPersistence.Begin(Profile,Intent());Assert.That(ticket,Is.Not.Null);}
            if(boundary>=2)world.Write(after,rules.Validate);
            if(boundary==3)
            {
                fail.Fail=true;Assert.That(ticket.Complete(out _),Is.False);Assert.That(disk.Read(Profile).PendingXP,Is.Not.Null);
            }
            if(boundary==4){Assert.That(ticket.Complete(out var total),Is.True);Assert.That(total,Is.EqualTo(1920));Assert.That(ticket.Complete(out total),Is.True);Assert.That(total,Is.EqualTo(1920));}
            ticket?.Dispose();var restarted=new LocalAccountProgressionStore(disk.Directory);var expected=boundary>=2?1920:1800;
            for(var i=0;i<3;i++){var loaded=restarted.Read(Profile);Assert.That(loaded.Find("survivor").XP,Is.EqualTo(expected));Assert.That(loaded.PendingXP,Is.Null);Assert.That(loaded.Selected,Is.EqualTo("survivor"));Assert.That(loaded.Find("survivor").Nodes,Is.EqualTo(new[]{"A1"}));Assert.That(loaded.Version,Is.EqualTo(2));}
            Assert.That(world.Read(rules.Validate).Player.XP,Is.EqualTo(boundary>=2?120:0));
        }
        [Test]public void ProfileWriteFailureRetainsRecoverableIntentAndCannotDoubleCredit()
        {
            var fail=new FailingStore{Inner=disk};ClassProfiles.DevelopmentProviders(fail);var ticket=ClassRewardPersistence.Begin(Profile,Intent());Assert.That(ticket,Is.Not.Null);world.Write(after,rules.Validate);fail.Fail=true;
            Assert.That(ticket.Complete(out _),Is.False);Assert.That(disk.Read(Profile).PendingXP,Is.Not.Null);ticket.Dispose();ClassProfiles.DevelopmentProviders(disk);
            Assert.That(disk.Read(Profile).Find("survivor").XP,Is.EqualTo(1920));Assert.That(disk.Read(Profile).Find("survivor").XP,Is.EqualTo(1920));
        }
        [Test]public void FailedWorldCommitRollsBackIntentAndLeavesBothXpValuesUnchanged()
        {var ticket=ClassRewardPersistence.Begin(Profile,Intent());ticket.Rollback();ticket.Dispose();Assert.That(disk.Read(Profile).PendingXP,Is.Null);Assert.That(disk.Read(Profile).Find("survivor").XP,Is.EqualTo(1800));Assert.That(world.Read(rules.Validate).Player.XP,Is.Zero);}
        [Test]public void UnavailableIntentStoreCannotStartWorldRewardCommit()
        {ClassProfiles.DevelopmentProviders(new FailingStore{Inner=disk,Fail=true});Assert.That(ClassRewardPersistence.Begin(Profile,Intent()),Is.Null);Assert.That(ClassRewardPersistence.Active(Profile),Is.False);Assert.That(disk.Read(Profile).PendingXP,Is.Null);Assert.That(world.Read(rules.Validate).Player.XP,Is.Zero);}
        [Test]public void ValidBackupPendingIntentRecoversWithoutDuplicatingAlreadyCommittedXp()
        {var ticket=ClassRewardPersistence.Begin(Profile,Intent());world.Write(after,rules.Validate);Assert.That(ticket.Complete(out _),Is.True);ticket.Dispose();File.WriteAllText(Path.Combine(disk.Directory,"class-"+Profile+".json"),"invalid primary");Assert.That(disk.Read(Profile).Find("survivor").XP,Is.EqualTo(1920));Assert.That(disk.Read(Profile).Find("survivor").XP,Is.EqualTo(1920));}
        [Test]public void SessionOnlyRewardsDoNotWriteOrPromoteRemoteProfiles()
        {var path=Path.Combine(disk.Directory,"class-"+Profile+".json");var bytes=File.ReadAllBytes(path);var session=new ClassProgressSession(disk.Read(Profile),false);Assert.That(session.Grant(Profile,"survivor",120,"session"),Is.True);Assert.That(session.Grant(Profile,"survivor",120,"session"),Is.False);Assert.That(File.ReadAllBytes(path),Is.EqualTo(bytes));Assert.That(disk.Read(Profile).Find("survivor").XP,Is.EqualTo(1800));}
    }
}
