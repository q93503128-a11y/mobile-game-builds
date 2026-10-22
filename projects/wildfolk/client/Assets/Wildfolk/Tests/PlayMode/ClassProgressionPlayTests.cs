using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class ClassProgressionPlayTests
    {
        private IEnumerator Run(string method)
        {var t=Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Wildfolk.Editor.ClassFoundationQa")!=null).GetType("Wildfolk.Editor.ClassFoundationQa");t.GetMethod("Begin").Invoke(null,new object[]{method});var end=Time.realtimeSinceStartup+240;while(Time.realtimeSinceStartup<end){var phase=(string)t.GetProperty("Phase").GetValue(null);if(phase=="complete" || phase=="failed"){Assert.That(phase,Is.EqualTo("complete"),"Captures/class-progression/live.tsv");yield break;}yield return null;}Assert.Fail("Class QA timed out");}
        [UnityTest]public IEnumerator LobbyOwnershipPurchaseTreeAndSessionLock()=>Run("Lobby");
        [UnityTest]public IEnumerator SharedGrowthMigrationAndProfileXP()=>Run("SharedGrowth");
        [UnityTest]public IEnumerator SurvivorLethalCooldownAndHome()=>Run("Survivor");
        [UnityTest]public IEnumerator WarriorFrontalGuardAndActualHPHeavy()=>Run("Warrior");
        [UnityTest]public IEnumerator HunterQuarryAndWeaponCategories()=>Run("Hunter");
        [UnityTest]public IEnumerator MedicinePipelineAndGenericDelivery()=>Run("Medicine");
        [UnityTest]public IEnumerator RepairBufferRecoveryAndHundredCycles()=>Run("Buildings");
        [UnityTest]public IEnumerator OverclockAndExplorationMarker()=>Run("TurretsAndExploration");
        [UnityTest]public IEnumerator AdOfficialCompletionOnly()=>Run("RewardedContract");
        [UnityTearDown]public IEnumerator Close(){Time.timeScale=1;ReviveServices.Reset();var t=Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Wildfolk.Editor.ClassFoundationQa")!=null).GetType("Wildfolk.Editor.ClassFoundationQa");t.GetMethod("Abort").Invoke(null,null);yield return null;}
    }
}
