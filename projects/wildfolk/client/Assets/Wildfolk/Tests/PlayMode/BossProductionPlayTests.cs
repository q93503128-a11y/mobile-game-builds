using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class BossProductionPlayTests
    {
        [UnityTest] public IEnumerator LifecycleSaveRewardsAndNightGate()=>Run("Lifecycle");
        [UnityTest] public IEnumerator AccessoryExchangeThresholdAndPersistence()=>Run("Equipment");
        [UnityTest] public IEnumerator PendingReloadAndFailedWritesAreAtomic()=>Run("Transactions");
        private static IEnumerator Run(string scenario)
        {
#if UNITY_EDITOR
            if(scenario=="Transactions")LogAssert.Expect(UnityEngine.LogType.Error,"Boss reward commit deferred: InvalidOperationException");
            var state=Type.GetType("UnityEditor.SessionState, UnityEditor");state.GetMethod("SetString").Invoke(null,new object[]{"Wildfolk.QA.Directory",System.IO.Path.GetFullPath("Captures/boss-play-tests/"+scenario)});
            var t=Type.GetType("Wildfolk.Editor.BossProductionQa, Assembly-CSharp-Editor");Assert.That(t,Is.Not.Null);
            yield return (IEnumerator)t.GetMethod("Run",BindingFlags.Public|BindingFlags.Static).Invoke(null,new object[]{scenario});
#else
            yield break;
#endif
        }
    }
}
