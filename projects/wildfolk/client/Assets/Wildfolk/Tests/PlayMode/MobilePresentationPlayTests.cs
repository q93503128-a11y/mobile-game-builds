using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TouchPhase=UnityEngine.InputSystem.TouchPhase;
using Object=UnityEngine.Object;
namespace Wildfolk.Tests
{
    public sealed class MobilePresentationPlayTests
    {
        private PlayPointerDriver pointer;private Touchscreen touch;private WorldSaveService save;private GameplayHud hud;
        private int generation;private int? seed;
        [UnitySetUp]public IEnumerator Setup()
        {
            if(!DevelopmentProfile.Isolated)Assert.Ignore("Requires an isolated QA profile");
            generation=WorldGenerator.SelectedGenerationVersion;seed=WorldGenerator.SelectedSeed;pointer=new PlayPointerDriver();touch=InputSystem.AddDevice<Touchscreen>();Time.timeScale=1;
            WorldGenerator.SelectedGenerationVersion=1;WorldGenerator.SelectedSeed=9472026;
            yield return SceneManager.LoadSceneAsync(SceneFlow.GameplayScene);yield return null;yield return null;
            save=Object.FindFirstObjectByType<WorldSaveService>();save.AutoSaveEnabled=false;save.Clock.enabled=false;save.World.Animals.enabled=false;save.GetComponent<ZombieSpawner>().enabled=false;save.GetComponent<GoblinSpawner>().enabled=false;save.Player.GetComponent<PlayerSurvival>().enabled=false;hud=Object.FindFirstObjectByType<GameplayHud>();
        }
        private void Touch(int id,TouchPhase phase,Vector2 position)=>InputSystem.QueueStateEvent(touch,new TouchState{touchId=id,phase=phase,position=position});
        private Vector2 Center=>RectTransformUtility.WorldToScreenPoint(null,hud.Joystick.transform.position);
        [UnityTest]public IEnumerator FloatingDeadzoneOwnershipAndConcurrentAttackUseActualUiInputModule()
        {
            var origin=Center;Touch(1,TouchPhase.Began,origin);yield return null;yield return null;
            Assert.That(hud.Joystick.Feedback.alpha,Is.GreaterThan(0));Assert.That(hud.Joystick.Value,Is.EqualTo(Vector2.zero));
            Touch(1,TouchPhase.Moved,origin+Vector2.right);yield return null;Assert.That(hud.Joystick.Value,Is.EqualTo(Vector2.zero));
            Touch(1,TouchPhase.Moved,origin+Vector2.right*80);yield return null;yield return null;Assert.That(hud.Joystick.Value.x,Is.GreaterThan(.5f));
            var attacks=0;save.Player.GetComponent<PlayerCombat>().AttackStarted+=()=>attacks++;
            Touch(2,TouchPhase.Began,PlayPointerDriver.Point(hud.Attack));yield return null;yield return null;
            Assert.That(attacks,Is.GreaterThan(0));Assert.That(hud.Joystick.Value.x,Is.GreaterThan(.5f),"Right action must not steal movement pointer");
            Touch(2,TouchPhase.Ended,PlayPointerDriver.Point(hud.Attack));yield return null;Assert.That(hud.Joystick.Value.x,Is.GreaterThan(.5f));
            Touch(1,TouchPhase.Canceled,origin+Vector2.right*80);yield return null;yield return null;Assert.That(hud.Joystick.Value,Is.EqualTo(Vector2.zero));Assert.That(hud.Joystick.Feedback.alpha,Is.Zero);
        }
        [UnityTest]public IEnumerator HotbarAndModalDoNotStartOrRetainMovement()
        {
            var bar=Object.FindFirstObjectByType<HotbarView>();var slot=bar.Slots[2];Touch(1,TouchPhase.Began,PlayPointerDriver.Point(slot));yield return null;
            Assert.That(hud.Joystick.Feedback.alpha,Is.Zero);Touch(1,TouchPhase.Ended,PlayPointerDriver.Point(slot));yield return null;
            var origin=Center;Touch(2,TouchPhase.Began,origin);yield return null;Touch(2,TouchPhase.Moved,origin+Vector2.right*80);yield return null;
            Assert.That(hud.Joystick.Value.sqrMagnitude,Is.GreaterThan(0));hud.Session.OpenInventory();yield return null;
            Assert.That(hud.Joystick.Value,Is.EqualTo(Vector2.zero));Assert.That(hud.Joystick.Feedback.alpha,Is.Zero);
            Touch(3,TouchPhase.Began,origin);yield return null;Assert.That(hud.Joystick.Feedback.alpha,Is.Zero);
            Touch(2,TouchPhase.Ended,origin);Touch(3,TouchPhase.Ended,origin);yield return null;
        }
        [UnityTest]public IEnumerator CommonRootsStayUnlockedWithoutPointsButPrerequisitesStayLocked()
        {
            hud.Session.OpenCharacter();hud.Session.OpenSkillTree();yield return null;var tree=Object.FindFirstObjectByType<SkillTreeScreen>();
            Assert.That(tree.Growth.SkillPoints,Is.Zero);
            for(var i=0;i<tree.NodeViews.Length;i++)Assert.That(tree.NodeViews[i].Locked.activeSelf,Is.EqualTo(!tree.Growth.Unlocked(i)));
            Assert.That(tree.InvestButton.interactable,Is.False);
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            if(save!=null)save.AutoSaveEnabled=false;if(touch!=null&&touch.added)InputSystem.RemoveDevice(touch);pointer?.Dispose();Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=generation;WorldGenerator.SelectedSeed=seed;
            yield return SceneManager.LoadSceneAsync(SceneFlow.HomeScene);yield return null;
        }
    }
}
