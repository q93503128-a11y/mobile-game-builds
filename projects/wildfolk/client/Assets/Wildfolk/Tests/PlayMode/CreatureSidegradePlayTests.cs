using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class CreatureSidegradePlayTests
    {
        private Type Qa=>Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Wildfolk.Editor.CreatureSidegradeQa")!=null).GetType("Wildfolk.Editor.CreatureSidegradeQa");
        private IEnumerator Run(string method)
        {Qa.GetMethod(method).Invoke(null,null);var until=Time.realtimeSinceStartup+180;while(Time.realtimeSinceStartup<until){var phase=(string)Qa.GetProperty("Phase").GetValue(null);if(phase=="complete" || phase=="failed"){Assert.That(phase,Is.EqualTo("complete"),"See Captures/creature-sidegrade-expansion/live.tsv");yield break;}yield return null;}Assert.Fail("Sidegrade flow timed out");}
        [UnityTest]public IEnumerator ActualCraftReachVisualTipWallsSingleHitAndSave11()
        {Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;yield return Run("Begin");}
        [UnityTest]public IEnumerator HostCraftAndHitReplicateExistingToolThenRestoreAcrossReHost()
        {Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;yield return Run("BeginHost");NetworkSession.Instance.Leave();yield return null;}
        [UnityTearDown]public IEnumerator Exit(){Time.timeScale=1;if(NetworkSession.Instance!=null && NetworkSession.Instance.State!=NetworkSessionState.Idle)NetworkSession.Instance.Leave();yield return null;}
    }
}
