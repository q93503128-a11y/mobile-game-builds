using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class MedicinePlayTests
    {
        private Type Qa=>Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Wildfolk.Editor.MedicineQa")!=null).GetType("Wildfolk.Editor.MedicineQa");
        private IEnumerator Run(string method)
        {Qa.GetMethod(method).Invoke(null,null);var until=Time.realtimeSinceStartup+120;while(Time.realtimeSinceStartup<until){var phase=(string)Qa.GetProperty("Phase").GetValue(null);if(phase=="complete" || phase=="failed"){Assert.That(phase,Is.EqualTo("complete"),"See Captures/medicine-expansion/live.tsv");yield break;}yield return null;}Assert.Fail("Medicine flow timed out");}
        [UnityTest]public IEnumerator HerbsCraftingHealingSharedLimitAndSaveLoadUseActualGameplay()
        {Time.timeScale=1;WorldGenerator.SelectedGenerationVersion=3;WorldGenerator.SelectedSeed=9472026;yield return SceneManager.LoadSceneAsync("Wildfolk");yield return null;yield return null;yield return Run("Begin");}
        [UnityTest]public IEnumerator HostHealingReplayProtectionAndReHostUseActualServer()
        {Time.timeScale=1;SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;yield return Run("BeginHost");NetworkSession.Instance.Leave();yield return null;}
    }
}
