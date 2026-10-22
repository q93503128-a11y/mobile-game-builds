using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class DeathFoundationPlayTests
    {
        private IEnumerator Run(string method)
        {var type=Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Wildfolk.Editor.DeathFoundationQa")!=null).GetType("Wildfolk.Editor.DeathFoundationQa");type.GetMethod(method).Invoke(null,null);var until=Time.realtimeSinceStartup+360;while(Time.realtimeSinceStartup<until){var phase=(string)type.GetProperty("Phase").GetValue(null);if(phase=="complete" || phase=="failed"){Assert.That(phase,Is.EqualTo("complete"),"See Captures/death-revive-respawn/live.tsv");yield break;}yield return null;}Assert.Fail("Death flow timed out");}
        [UnityTest]public IEnumerator SingleDeathAdsRepeatedGemTransactionsHomeAndRealReload()
        {Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;yield return Run("BeginSingle");}
        [UnityTest]public IEnumerator FourActorDownReviveInterruptTimeoutWipePersistenceReHost()
        {Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;yield return Run("BeginHostSimulation");}
        [UnityTest,Explicit("Requires ready separate LAN Host")]
        public IEnumerator ActualSeparateLanClientLifecycle()
        {Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;yield return Run("BeginLanClient");}
        [UnityTearDown]public IEnumerator Close(){Time.timeScale=1;ReviveServices.Reset();if(NetworkSession.Instance!=null && NetworkSession.Instance.State!=NetworkSessionState.Idle){NetworkSession.Instance.Leave(true,true);yield return new WaitUntil(()=>NetworkSession.Instance.State==NetworkSessionState.Idle);}}
    }
}
