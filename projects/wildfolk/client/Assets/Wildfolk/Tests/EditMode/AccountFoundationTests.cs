using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace Wildfolk.Tests
{
    public sealed class AccountFoundationTests
    {
        private string previous,directory,previousScene;private IAccountProgressionStore previousStore;private SaveRepository previousRepository;
        private sealed class Identity:IAccountIdentityProvider
        {public string PlayerId=>"account-player";public bool Linked=>true;public bool Conflict,Offline,Cleared;public int Connects,Existing;public Task Connect(){Connects++;if(Conflict)throw new AccountConflictException();if(Offline)throw new IOException();return Task.CompletedTask;}public Task UseExisting(){Existing++;Conflict=false;return Task.CompletedTask;}public void SignOut(bool clearSession=false){Cleared=clearSession;}}
        private sealed class Cloud:IAccountCloudTransport
        {public int Calls;public bool PendingDeletion;public Task<AccountReply> Call(string op,Dictionary<string,object> input){Calls++;if(op=="RequestAccountDeletionPreparation")return Task.FromResult(new AccountReply{ok=true,deletionId="isolated-delete"});return Task.FromResult(new AccountReply{ok=true,profile=new CloudAccountProfile{Schema=1,DeletionPending=PendingDeletion,PlayerId="account-player",SupportId="LF-1234-5678-90AB-CDEF-1234",CreatedAt="2026-10-25T00:00:00Z",ProviderLinked=true,ClassProfile=CloudClassProfile.From(AccountProgression.New()),Entitlements=Array.Empty<AccountEntitlement>()}});}}
        [SetUp]public void Setup()
        {previousScene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="MainMenu")UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");previous=UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","");directory=Path.Combine(Path.GetTempPath(),"LowfireAccount-"+Guid.NewGuid().ToString("N"));UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",directory);previousRepository=SceneFlow.Repository;SceneFlow.Repository=SaveRepository.Default();previousStore=ClassProfiles.Store;ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(Path.Combine(directory,"guest")));}
        [TearDown]public void Cleanup()
        {UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",directory);AccountCloudCache.Deselect();ClassProfiles.DevelopmentProviders(previousStore);UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",previous);SceneFlow.Repository=previousRepository;if(Directory.Exists(directory))Directory.Delete(directory,true);if(string.IsNullOrEmpty(previousScene))UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);else UnityEditor.SceneManagement.EditorSceneManager.OpenScene(previousScene);}
        [Test]public void RealMoneyIsAlwaysDisabled()=>Assert.That(BackendPolicy.RealMoneyPurchase,Is.False);
        [Test]public void Profile2CloudMappingExcludesWorldPathAndPending()
        {var p=AccountProgression.New();var wire=JsonUtility.ToJson(CloudClassProfile.From(p));Assert.That(wire,Does.Not.Contain("WorldPath"));Assert.That(wire,Does.Not.Contain("Pending"));Assert.That(CloudClassProfile.From(p).Local().Selected,Is.EqualTo(p.Selected));}
        [Test]public void PendingTransactionCannotBeImported()
        {var p=AccountProgression.New();p.Pending=new ClassPurchaseIntent{Transaction=Guid.NewGuid().ToString("N"),ClassId="hunter",Cost=100,WorldPath="local"};Assert.Throws<InvalidOperationException>(()=>CloudClassProfile.From(p));}
        [Test]public void WrongCloudSchemaOrIdentityRejected()
        {var p=new CloudAccountProfile{Schema=2};Assert.Throws<InvalidOperationException>(()=>p.Validate("a"));}
        [Test]public void AlphaHashBindsAccountEnvironmentOperationBuild()
        {var h=AccountRequestHash.ForAlpha("a","env","op","1");Assert.That(h,Is.Not.EqualTo(AccountRequestHash.ForAlpha("b","env","op","1")));Assert.That(h,Is.Not.EqualTo(AccountRequestHash.ForAlpha("a","env","op2","1")));Assert.That(h.Length,Is.EqualTo(43));}
        [Test]public async Task CachedGuestLinksWithoutCreatingParticipantIdentity()
        {var identity=new Identity();var service=AccountService.InstallDevelopment(identity,new Cloud());var participant=ClassProfiles.LocalId;await service.Connect();Assert.That(identity.Connects,Is.EqualTo(1));Assert.That(service.State,Is.EqualTo(AccountConnection.Connected));Assert.That(ClassProfiles.LocalId,Is.EqualTo(participant));}
        [Test]public async Task ConflictDoesNotSwitchUntilExplicitChoice()
        {var identity=new Identity{Conflict=true};var cloud=new Cloud();var s=AccountService.InstallDevelopment(identity,cloud);await s.Connect();Assert.That(s.State,Is.EqualTo(AccountConnection.AccountConflict));Assert.That(identity.Existing,Is.Zero);Assert.That(cloud.Calls,Is.Zero);await s.Connect(true);Assert.That(identity.Existing,Is.EqualTo(1));Assert.That(s.State,Is.EqualTo(AccountConnection.Connected));}
        [Test]public async Task OfflineKeepsLocalProfileAndWorld()
        {var local=ClassProfiles.LocalId;var s=AccountService.InstallDevelopment(new Identity{Offline=true},new Cloud());await s.Connect();Assert.That(s.State,Is.EqualTo(AccountConnection.Offline));Assert.That(ClassProfiles.LocalId,Is.EqualTo(local));Assert.That(SceneFlow.Repository.Path,Does.StartWith(directory));}
        [Test]public async Task AccountCacheDoesNotOverwriteGuestAndMarksLocalWritesDirty()
        {var guest=ClassProfiles.Store;var id=ClassProfiles.LocalId;Assert.That(guest.Write(id,AccountProgression.New()),Is.True);var path=Path.Combine(directory,"guest","class-"+id+".json");var bytes=File.ReadAllBytes(path);var s=AccountService.InstallDevelopment(new Identity(),new Cloud());await s.Connect();var progress=ClassProfiles.Local;progress.Find("survivor").XP=1800;Assert.That(ClassProfiles.Store.Write(id,progress),Is.True);Assert.That(s.Dirty,Is.True);Assert.That(File.ReadAllBytes(path),Is.EqualTo(bytes));}
        [Test]public async Task FailedAccountCacheWriteReturnsFalseWithoutChangingProgress()
        {var id=ClassProfiles.LocalId;var s=AccountService.InstallDevelopment(new Identity(),new Cloud());await s.Connect();var folder=Directory.GetDirectories(Path.Combine(directory,"CloudAccounts"))[0];Directory.CreateDirectory(Path.Combine(folder,"dirty.temporary"));var progress=ClassProfiles.Local;progress.Find("survivor").XP=1800;Assert.That(ClassProfiles.Store.Write(id,progress),Is.False);Assert.That(ClassProfiles.Local.Find("survivor").XP,Is.Zero);}
        [Test]public async Task AcceptedDeletionClearsCurrentAccountSessionCacheButPreservesGuestAndWorld()
        {var identity=new Identity();var guest=ClassProfiles.Store;var id=ClassProfiles.LocalId;guest.Write(id,AccountProgression.New());var guestPath=Path.Combine(directory,"guest","class-"+id+".json");var bytes=File.ReadAllBytes(guestPath);var world=SceneFlow.Repository.Path;Directory.CreateDirectory(Path.GetDirectoryName(world));File.WriteAllText(world,"isolated-world-marker");var s=AccountService.InstallDevelopment(identity,new Cloud());await s.Connect();var folder=Directory.GetDirectories(Path.Combine(directory,"CloudAccounts"))[0];await s.PrepareDeletion(true);Assert.That(s.State,Is.EqualTo(AccountConnection.DeletionPending));Assert.That(identity.Cleared,Is.True);Assert.That(Directory.Exists(folder),Is.False);Assert.That(AccountCloudCache.Restore(),Is.Null);Assert.That(File.ReadAllBytes(guestPath),Is.EqualTo(bytes));Assert.That(File.ReadAllText(world),Is.EqualTo("isolated-world-marker"));}
        [Test]public async Task ServerPendingDeletionDoesNotRematerializeAnAccountCache()
        {var identity=new Identity();var cloud=new Cloud();var s=AccountService.InstallDevelopment(identity,cloud);await s.Connect();var folder=Directory.GetDirectories(Path.Combine(directory,"CloudAccounts"))[0];cloud.PendingDeletion=true;await s.Sync();Assert.That(s.State,Is.EqualTo(AccountConnection.DeletionPending));Assert.That(identity.Cleared,Is.True);Assert.That(Directory.Exists(folder),Is.False);Assert.That(AccountCloudCache.Restore(),Is.Null);}
        [Test]public void DevelopmentAdapterRequiresExplicitIsolation()
        {UnityEditor.SessionState.EraseString("Wildfolk.QA.Directory");Assert.Throws<InvalidOperationException>(()=>new DevelopmentIdentityProvider("fixture",true));}
        [Test]public void RelayUsesSharedBootstrapAndLanHasNoCloudDependency()
        {var path="Assets/Wildfolk/Runtime/Networking/RoomConnections.cs";var source=File.ReadAllText(path);Assert.That(source,Does.Contain("UgsBootstrap.EnsureAnonymous"));var lan=source.Substring(source.IndexOf("public sealed class LocalRoomConnection",StringComparison.Ordinal));Assert.That(lan,Does.Not.Contain("Authentication"));Assert.That(lan,Does.Not.Contain("Cloud"));}
    }
}
