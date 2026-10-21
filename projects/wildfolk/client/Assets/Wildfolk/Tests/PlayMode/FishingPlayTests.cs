using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class FishingPlayTests
    {
        private IEnumerator Run(string method)
        {var qa=Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Wildfolk.Editor.FishingQa")!=null).GetType("Wildfolk.Editor.FishingQa");qa.GetMethod(method).Invoke(null,null);var until=Time.realtimeSinceStartup+200;while(Time.realtimeSinceStartup<until){var phase=(string)qa.GetProperty("Phase").GetValue(null);if(phase=="complete" || phase=="failed"){Assert.That(phase,Is.EqualTo("complete"),"See Captures/fishing-aquatic-expansion/live.tsv");yield break;}yield return null;}Assert.Fail("Fishing flow timed out");}
        [UnityTest]public IEnumerator ActualWaterCatchesCancelFullBagProcessingAndSave11()
        {Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;yield return Run("Begin");}
        [UnityTest]public IEnumerator HostRollAwardReplicatedCatchAndReHostRemainAuthoritative()
        {Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;yield return Run("BeginHost");NetworkSession.Instance.Leave();yield return null;}
        [UnityTearDown]public IEnumerator Exit(){Time.timeScale=1;if(NetworkSession.Instance!=null && NetworkSession.Instance.State!=NetworkSessionState.Idle)NetworkSession.Instance.Leave();yield return null;}
    }
}
