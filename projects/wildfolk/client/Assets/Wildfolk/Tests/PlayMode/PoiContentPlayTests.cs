using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class PoiContentPlayTests
    {
        private IEnumerator Run(string method)
        {var qa=Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Wildfolk.Editor.PoiContentQa")!=null).GetType("Wildfolk.Editor.PoiContentQa");qa.GetMethod(method).Invoke(null,null);var until=Time.realtimeSinceStartup+420;while(Time.realtimeSinceStartup<until){var phase=(string)qa.GetProperty("Phase").GetValue(null);if(phase=="complete" || phase=="failed"){Assert.That(phase,Is.EqualTo("complete"),"See Captures/base-defense-poi-expansion/live.tsv");yield break;}yield return null;}Assert.Fail("POI flow timed out");}
        [UnityTest]public IEnumerator AllEightPoiFiniteSuppliesWellQuarryGuardLegacySaveAndTrapRoles()
        {Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;yield return Run("Begin");}
        [UnityTest]public IEnumerator HostSharedRequestsFourSimulatedClaimantsGuardSleepAndTwoReHosts()
        {Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;yield return Run("BeginHost");}

        [UnityTest,Explicit("Requires an actual separate isolated LAN Host running BeginCoopHost")]
        public IEnumerator RealClientSuppliesWellAndGuardedClear()
        {Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;yield return Run("BeginCoopClient");}

        [UnityTest]public IEnumerator GraveyardGuardKeepsLocalBoundaryWithoutResettingHealthOrReward()
        {Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;yield return Run("BeginGuardBoundary");}
        [UnityTearDown]public IEnumerator Close(){CampLootPanel.CloseFocused();Time.timeScale=1;if(NetworkSession.Instance!=null && NetworkSession.Instance.State!=NetworkSessionState.Idle){NetworkSession.Instance.Leave(true,true);yield return new WaitUntil(()=>NetworkSession.Instance.State==NetworkSessionState.Idle);}}
    }
}
