using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class AccountFoundationPlayTests
    {
        private string directory,previous;private AccountService service;private Identity identity;
        private sealed class Identity:IAccountIdentityProvider
        {public string PlayerId=>"isolated-play-account";public bool Linked=>!Offline;public bool Offline,Cleared;public Task Connect(){if(Offline)throw new IOException();return Task.CompletedTask;}public Task UseExisting()=>Connect();public void SignOut(bool clearSession=false){Cleared=clearSession;}}
        private sealed class Cloud:IAccountCloudTransport
        {public Task<AccountReply> Call(string operation,Dictionary<string,object> input)=>Task.FromResult(new AccountReply{ok=true,profile=new CloudAccountProfile{Schema=1,PlayerId="isolated-play-account",SupportId="LF-1234-5678-90AB-CDEF-1234",CreatedAt="2026-10-25T00:00:00Z",ProviderLinked=true,ClassProfile=CloudClassProfile.From(AccountProgression.New()),Entitlements=Array.Empty<AccountEntitlement>()}});}
        [UnitySetUp]public IEnumerator Setup()
        {
#if UNITY_EDITOR
            previous=UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","");directory=Path.Combine(Path.GetTempPath(),"LowfireAccountPlay-"+Guid.NewGuid().ToString("N"));UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",directory);
            ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(Path.Combine(directory,"Guest")));identity=new Identity();service=AccountService.InstallDevelopment(identity,new Cloud());
#endif
            SceneFlow.Repository=SaveRepository.Default();yield return SceneManager.LoadSceneAsync(SceneFlow.HomeScene);yield return null;
        }
        [UnityTest]public IEnumerator AccountJournalAndFeedbackUseRealAuthoredControls()
        {
            var screen=UnityEngine.Object.FindFirstObjectByType<AccountProfileScreen>();screen.Open();var task=service.Connect();while(!task.IsCompleted)yield return null;Assert.That(task.IsFaulted,Is.False);Assert.That(service.State,Is.EqualTo(AccountConnection.Connected));Assert.That(screen.Support.text,Is.EqualTo(service.Profile.SupportId));
            var old=GUIUtility.systemCopyBuffer;try{screen.CopySupport();Assert.That(GUIUtility.systemCopyBuffer,Is.EqualTo(service.Profile.SupportId));}finally{GUIUtility.systemCopyBuffer=old;}
            screen.Help();yield return null;Assert.That(screen.View.activeSelf,Is.False);Assert.That(screen.FeedbackView.activeSelf,Is.True);Assert.That(screen.Consent.isOn,Is.False);Assert.That(screen.FeedbackText.characterLimit,Is.EqualTo(1500));Assert.That(screen.Category.options.Count,Is.EqualTo(6));
            screen.CloseHelp();screen.Delete();screen.ConfirmDelete();Assert.That(service.State,Is.EqualTo(AccountConnection.Connected));screen.CancelDelete();Assert.That(screen.View.activeSelf,Is.True);screen.Back();Assert.That(screen.GetComponent<EconomyScreen>().HubView.activeSelf,Is.True);Assert.That(screen.Home.Menu.activeSelf,Is.False);
        }
        [UnityTest]public IEnumerator OfflineAuthenticationDoesNotBlockSingleplayerSaveAndHome()
        {
            identity.Offline=true;var task=service.Connect();while(!task.IsCompleted)yield return null;Assert.That(service.State,Is.EqualTo(AccountConnection.Offline));
            var home=UnityEngine.Object.FindFirstObjectByType<HomeScreen>();Assert.That(home.SingleButton.interactable,Is.True);home.BeginNew();yield return null;yield return null;
            var save=UnityEngine.Object.FindFirstObjectByType<WorldSaveService>();Assert.That(save,Is.Not.Null);Assert.That(save.Repository.Path,Does.StartWith(directory));Assert.That(save.Save(),Is.True);Assert.That(save.Repository.Read(save.Rules.Validate).Version,Is.EqualTo(14));
            save.SaveAndHome();yield return null;Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo(SceneFlow.HomeScene));
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            Time.timeScale=1;if(SceneManager.GetActiveScene().name!=SceneFlow.HomeScene)yield return SceneManager.LoadSceneAsync(SceneFlow.HomeScene);
#if UNITY_EDITOR
            UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",directory);AccountCloudCache.Deselect();ClassProfiles.DevelopmentProviders(null);UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",previous);SceneFlow.Repository=SaveRepository.Default();
            if(directory!=null&&Path.GetFullPath(directory).StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase)&&Directory.Exists(directory))Directory.Delete(directory,true);
#endif
        }
    }
}
