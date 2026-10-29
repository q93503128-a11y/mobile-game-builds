using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Wildfolk.Tests
{
 public sealed class AudioPlayTests
 {
  private readonly List<Sound> heard=new List<Sound>();private string previous;
  [UnitySetUp]public IEnumerator SetUp(){heard.Clear();previous=UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","");UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",Path.Combine(Path.GetTempPath(),"LowfireAudioTests",Guid.NewGuid().ToString("N")));SceneFlow.Repository=SaveRepository.Default();if(AudioRuntime.Current!=null)Object.Destroy(AudioRuntime.Current.gameObject);yield return null;yield return SceneManager.LoadSceneAsync("MainMenu");yield return null;AudioRuntime.Played+=Hear;}
  [UnityTearDown]public IEnumerator TearDown(){AudioRuntime.Played-=Hear;if(AudioRuntime.Current!=null)Object.Destroy(AudioRuntime.Current.gameObject);yield return null;SceneFlow.Repository=null;UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",previous);}
  private void Hear(Sound cue)=>heard.Add(cue);
  [UnityTest]public IEnumerator AuthoredCategoryButtonUsesPageAndDisabledButtonIsSilent()
  {var go=new GameObject("Category",typeof(RectTransform),typeof(UnityEngine.UI.Button));go.AddComponent<AudioButton>();var button=go.GetComponent<UnityEngine.UI.Button>();button.onClick.Invoke();Assert.That(heard.Count(c=>c==Sound.Page),Is.EqualTo(1));button.interactable=false;button.onClick.Invoke();Assert.That(heard.Count(c=>c==Sound.Page),Is.EqualTo(1));Object.Destroy(go);yield return null;}
  [UnityTest]public IEnumerator MuteAndSettingsDriveRealMixerAndAllSources()
  {var prefs=Object.FindFirstObjectByType<UserPreferences>();var r=AudioRuntime.Current;prefs.SetMaster(0);yield return null;Assert.That(r.Bank.Mixer.GetFloat("MasterVolume",out var db),Is.True);Assert.That(db,Is.EqualTo(-80));Assert.That(r.GetComponentsInChildren<AudioSource>().All(s=>s.mute),Is.True);Assert.That(AudioRuntime.Play(Sound.UiConfirm),Is.False);prefs.SetMaster(.5f);prefs.SetMusic(.5f);prefs.SetSfx(.25f);yield return null;r.Bank.Mixer.GetFloat("SfxVolume",out db);Assert.That(db,Is.EqualTo(-12.0412f).Within(.01));Assert.That(LocalSettings.Load().Sfx,Is.EqualTo(.25f));}
  [UnityTest]public IEnumerator SceneReloadKeepsOneOwnerAndBoundedSources()
  {var owner=AudioRuntime.Current;var transitions=owner.Landscape.Transitions;for(var i=0;i<3;i++){yield return SceneManager.LoadSceneAsync("MainMenu");yield return null;Assert.That(AudioRuntime.Current,Is.SameAs(owner));Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length,Is.EqualTo(1));Assert.That(owner.GetComponentsInChildren<AudioSource>().Length,Is.EqualTo(30));}Assert.That(owner.Landscape.Transitions,Is.EqualTo(transitions));}
  [UnityTest]public IEnumerator ConfirmedReceiptCannotRepeatAndMuteDoesNotReplay()
  {var receipt=Guid.NewGuid().ToString("N");Assert.That(AudioTransactions.Confirmed(receipt,Sound.Gem),Is.True);Assert.That(AudioTransactions.Confirmed(receipt,Sound.Gem),Is.False);yield return null;Assert.That(heard.Count(c=>c==Sound.Gem),Is.EqualTo(1));}
  [UnityTest]public IEnumerator NativeOverlaySuspendsAndFinallyRestoresAudioEvenOnFailure()
  {var held=new System.Threading.Tasks.TaskCompletionSource<bool>();var operation=AudioRuntime.DuringOverlay(()=>held.Task);yield return null;Assert.That(AudioListener.pause,Is.True);Assert.That(AudioRuntime.Play(Sound.UiConfirm),Is.False);held.SetException(new InvalidOperationException("isolated provider failure"));while(!operation.IsCompleted)yield return null;Assert.That(operation.IsFaulted,Is.True);Assert.That(AudioListener.pause,Is.False);Assert.That(AudioRuntime.Play(Sound.UiConfirm),Is.True);}
  [UnityTest]public IEnumerator HarvestDamageIsOneMaterialEventAndColdSceneIsSilent()
  {heard.Clear();yield return SceneManager.LoadSceneAsync(SceneFlow.GameplayScene);yield return null;yield return null;var node=Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).First(n=>n.Definition.Kind==ResourceKind.Wood);var camera=Camera.main;camera.transform.position=new Vector3(node.transform.position.x,node.transform.position.y,-10);yield return null;heard.Clear();node.Health.ApplyDamage(1,true);yield return null;Assert.That(heard.Count(c=>c==Sound.WoodHit),Is.EqualTo(1));Assert.That(heard.Contains(Sound.BodyFall),Is.False);}
  [UnityTest]public IEnumerator ExistingOwnerStateReceiptsDoNotReplayOnLoadOrSameSequence()
  {yield return SceneManager.LoadSceneAsync(SceneFlow.GameplayScene);yield return null;yield return null;var player=Object.FindFirstObjectByType<WorldSaveService>().Player;var progress=player.GetComponent<PlayerProgression>();var net=player.gameObject.AddComponent<NetworkCommonGrowth>();var apply=typeof(NetworkCommonGrowth).GetMethod("Apply",BindingFlags.Instance|BindingFlags.NonPublic);var state=new NetworkGrowthSnapshot{Growth=progress.Capture(),XP=progress.Experience,Gold=progress.Gold,ActionSequence=4,Action=ConfirmedPlayerAction.Food};heard.Clear();apply.Invoke(net,new object[]{new FixedString4096Bytes(JsonUtility.ToJson(state))});Assert.That(heard,Is.Empty);state.ActionSequence++;apply.Invoke(net,new object[]{new FixedString4096Bytes(JsonUtility.ToJson(state))});apply.Invoke(net,new object[]{new FixedString4096Bytes(JsonUtility.ToJson(state))});yield return null;Assert.That(heard.Count(c=>c==Sound.Eat),Is.EqualTo(1));}
 }
}
