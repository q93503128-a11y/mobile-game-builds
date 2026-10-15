using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Wildfolk.Tests
{
    public sealed class FrontendPlayTests
    {
        private PlayPointerDriver pointer;
        private string folder;
        private SaveRepository repository;
        private LocalSettings original;
        private readonly string[] prefKeys={"controls","master","music","sfx","language"};
        private bool[] present;
        private HomeScreen Home=>Object.FindFirstObjectByType<HomeScreen>();
        private WorldSaveService Game=>Object.FindFirstObjectByType<WorldSaveService>();
        [UnitySetUp]public IEnumerator Setup()
        {
            original=LocalSettings.Load();present=prefKeys.Select(k=>PlayerPrefs.HasKey(LocalSettings.Prefix+k)).ToArray();
            pointer=new PlayPointerDriver();Time.timeScale=1;
            folder=Path.Combine(Application.temporaryCachePath,"wildfolk-home-"+System.Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);repository=new SaveRepository(Path.Combine(folder,"world-v1.json"));
            SceneFlow.Home(repository);yield return Ready(false);
        }
        private IEnumerator Ready(bool gameplay)
        {
            for(var i=0;i<120;i++){yield return null;if(gameplay ? Game!=null && Game.World.Map!=null : Home!=null)break;}
            if(gameplay){Assert.That(Game,Is.Not.Null);Game.AutoSaveEnabled=false;Game.Clock.enabled=false;Game.World.Animals.enabled=false;Game.GetComponent<GoblinSpawner>().enabled=false;Game.GetComponent<ZombieSpawner>().enabled=false;Game.Player.GetComponent<PlayerSurvival>().enabled=false;}
            else Assert.That(Home,Is.Not.Null);
            yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            if(Game!=null)Game.AutoSaveEnabled=false;Time.timeScale=1;pointer.Dispose();
            original.Store();for(var i=0;i<prefKeys.Length;i++)if(!present[i])PlayerPrefs.DeleteKey(LocalSettings.Prefix+prefKeys[i]);PlayerPrefs.Save();L.SetLanguage(original.Language);
            SceneFlow.Repository=SaveRepository.Default();if(Directory.Exists(folder))Directory.Delete(folder,true);yield return null;
        }
        [UnityTest]public IEnumerator EmptyAndCorruptSaveDisableContinueAndBackupIsValidated()
        {
            Assert.That(Home.CanContinue,Is.False);Assert.That(Home.ContinueButton.interactable,Is.False);
            File.WriteAllText(repository.Path,"broken");Home.RefreshSave();Assert.That(Home.CanContinue,Is.False);Assert.That(Home.Status.text,Is.EqualTo(L.Text("home.save_unavailable")));
            yield return pointer.Click(Home.SingleButton);Assert.That(Home.Page,Is.EqualTo(FrontendPage.ConfirmNew));yield return pointer.Click(Home.Confirm.GetComponentsInChildren<Button>().Single(b=>b.name=="Cancel"));Assert.That(File.ReadAllText(repository.Path),Is.EqualTo("broken"));
            Home.BeginNew();yield return Ready(true);Assert.That(Game.Save(),Is.True);Assert.That(Game.Save(),Is.True);File.WriteAllText(repository.Path,"broken");SceneFlow.Home(repository);yield return Ready(false);Assert.That(Home.CanContinue,Is.True);Assert.That(repository.UsedBackup,Is.True);
        }
        [UnityTest]public IEnumerator ActualButtonsStartSaveReturnConfirmCancelAndContinueWithoutDuplicateServices()
        {
            yield return pointer.Click(Home.SingleButton);yield return Ready(true);var seed=Game.World.Seed;Game.Player.Model.Add(Game.Catalog.Item("wood"),9);Game.Clock.Model.SetElapsed(43);Assert.That(Game.Save(),Is.True);
            yield return pointer.Click(Object.FindFirstObjectByType<GameplayHud>().PauseButton);Assert.That(Time.timeScale,Is.Zero);var menu=Object.FindFirstObjectByType<PauseMenu>();yield return pointer.Click(menu.Menu.GetComponentsInChildren<Button>().Single(b=>b.name=="Save and Home"));yield return Ready(false);
            Assert.That(Time.timeScale,Is.EqualTo(1));Assert.That(Home.CanContinue,Is.True);var bytes=File.ReadAllText(repository.Path);
            yield return pointer.Click(Home.SingleButton);Assert.That(Home.Page,Is.EqualTo(FrontendPage.ConfirmNew));yield return pointer.Click(Home.Confirm.GetComponentsInChildren<Button>().Single(b=>b.name=="Cancel"));Assert.That(File.ReadAllText(repository.Path),Is.EqualTo(bytes));
            yield return pointer.Click(Home.ContinueButton);yield return Ready(true);Assert.That(Game.World.Seed,Is.EqualTo(seed));Assert.That(Game.Player.Model.CountItem(Game.Catalog.Item("wood")),Is.EqualTo(9));Assert.That(Game.Clock.Model.Elapsed,Is.InRange(43d,44d));
            Assert.That(Object.FindObjectsByType<UserPreferences>(FindObjectsSortMode.None).Length,Is.EqualTo(1));Assert.That(Object.FindObjectsByType<GameAudio>(FindObjectsSortMode.None).Length,Is.EqualTo(1));Assert.That(Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<HomeScreen>(FindObjectsSortMode.None),Is.Empty);
        }
        [UnityTest]public IEnumerator SharedSettingsAndBilingualLabelsSurviveSceneChanges()
        {
            yield return pointer.Click(Home.SettingsButton);var settings=Home.Settings.GetComponent<SettingsPanel>();Assert.That(Home.Page,Is.EqualTo(FrontendPage.Settings));settings.Preferences.SetControl(1.2f);settings.Preferences.SetMaster(.4f);settings.Preferences.SetMusic(.3f);settings.Preferences.SetSfx(.2f);
            settings.Preferences.SetLanguage(System.Array.FindIndex(L.Catalog.Languages,l=>l.Id=="en"));yield return pointer.Click(settings.LanguageLabel.GetComponentInParent<Button>());Assert.That(L.Language,Is.EqualTo("ko"));
            yield return pointer.Click(Home.Settings.GetComponentsInChildren<Button>().Single(b=>b.name=="Back"));Assert.That(Home.SingleButton.GetComponentInChildren<Text>().text,Is.EqualTo("싱글 플레이"));yield return pointer.Click(Home.MultiplayerButton);Assert.That(Home.Page,Is.EqualTo(FrontendPage.Multiplayer));Assert.That(Object.FindObjectsByType<WorldGenerator>(FindObjectsSortMode.None),Is.Empty);
            yield return pointer.Click(Home.Multiplayer.GetComponentsInChildren<Button>().Single(b=>b.name=="Single Player"));yield return Ready(true);
            var prefs=Game.GetComponent<UserPreferences>();Assert.That(prefs.Values.Language,Is.EqualTo("ko"));Assert.That(prefs.Values.ControlSize,Is.EqualTo(1.2f).Within(.001));Assert.That(prefs.Values.Master,Is.EqualTo(.4f).Within(.001));
            yield return pointer.Click(Object.FindFirstObjectByType<GameplayHud>().PauseButton);var pause=Object.FindFirstObjectByType<PauseMenu>();pause.OpenSettings();yield return null;var shared=pause.Settings.GetComponent<SettingsPanel>();Assert.That(shared.Control.value,Is.EqualTo(1.2f).Within(.001));Assert.That(shared.Music.value,Is.EqualTo(.3f).Within(.001));Assert.That(shared.Sfx.value,Is.EqualTo(.2f).Within(.001));
            yield return pointer.Click(shared.LanguageLabel.GetComponentInParent<Button>());Assert.That(L.Language,Is.EqualTo("en"));
        }
        [UnityTest]public IEnumerator HomeIdleKeepsStaticUiAndNoGameplaySimulation()
        {
            yield return new WaitForSecondsRealtime(.4f);var labels=Home.GetComponentsInChildren<Text>(true);var strings=labels.Select(t=>t.text).ToArray();var children=Home.GetComponentsInChildren<Transform>(true).Length;
            yield return new WaitForSecondsRealtime(.5f);Assert.That(labels.Select(t=>t.text),Is.EqualTo(strings));Assert.That(Home.GetComponentsInChildren<Transform>(true).Length,Is.EqualTo(children));
            Assert.That(Object.FindObjectsByType<WorldSaveService>(FindObjectsSortMode.None),Is.Empty);Assert.That(Object.FindObjectsByType<CreatureMember>(FindObjectsSortMode.None),Is.Empty);Assert.That(Object.FindObjectsByType<Health>(FindObjectsSortMode.None),Is.Empty);Assert.That(Object.FindObjectsByType<PrefabPool>(FindObjectsSortMode.None),Is.Empty);
            Assert.That(Object.FindObjectsByType<UserPreferences>(FindObjectsSortMode.None).Length,Is.EqualTo(1));Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo(SceneFlow.HomeScene));
        }
    }
}
