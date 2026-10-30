using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Wildfolk.Tests
{
    public sealed class ReplicaFeedbackPlayTests
    {
        private string previous;
        private GameObject actor;
        private Health health;
        private int hits;
        private Coroutine Reaction=>typeof(HitFeedback).GetField("reaction",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(actor.GetComponent<HitFeedback>()) as Coroutine;
        [UnitySetUp] public IEnumerator Enter()
        {
            previous=UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","");
            UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",Path.Combine(Path.GetTempPath(),"LowfireReplicaFeedback",Guid.NewGuid().ToString("N")));
            yield return SceneManager.LoadSceneAsync("MainMenu");
            actor=Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkTree.prefab"));
            health=actor.GetComponent<Health>();hits=0;health.Damaged+=Hit;
            yield return null;
        }
        [UnityTearDown] public IEnumerator Release()
        {
            if(health!=null)health.Damaged-=Hit;
            if(actor!=null)Object.Destroy(actor);
            yield return null;
            UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",previous);
        }
        private void Hit()=>hits++;
        [UnityTest] public IEnumerator InitialDamagedSnapshotUpdatesHealthWithoutPlayingAnImpact()
        {
            var hp=health.Maximum-1;health.ApplyReplica(hp);
            Assert.That(health.Current,Is.EqualTo(hp));Assert.That(hits,Is.Zero);
            Assert.That(Reaction,Is.Null,"Initial HP is a baseline, not a new harvest hit");
            yield return null;
        }
        [UnityTest] public IEnumerator NewDamageAndExplicitBufferedImpactEachReactOnce()
        {
            health.ApplyReplica(health.Maximum);health.ApplyReplica(health.Maximum-1,true);
            Assert.That(hits,Is.EqualTo(1),"HP drop plus hit receipt must not double the effect");Assert.That(Reaction,Is.Not.Null);
            health.ApplyReplica(health.Current);health.ApplyReplica(health.Maximum);
            Assert.That(hits,Is.EqualTo(1),"Unchanged and healing snapshots are silent");
            health.ApplyReplica(health.Current,true);Assert.That(hits,Is.EqualTo(2),"A new absorbed impact still has feedback");
            yield return new WaitForSeconds(.2f);Assert.That(Reaction,Is.Null);
        }
        [UnityTest] public IEnumerator ReusedActorPrimesItsNewSnapshotSilently()
        {
            health.ApplyReplica(health.Maximum);health.ApplyReplica(health.Maximum-1);
            Assert.That(hits,Is.EqualTo(1));
            actor.SetActive(false);health.ResetForSpawn();actor.GetComponent<HitFeedback>().ResetForSpawn();actor.SetActive(true);
            health.ApplyReplica(health.Maximum-1);Assert.That(hits,Is.EqualTo(1));Assert.That(Reaction,Is.Null);
            health.ApplyReplica(health.Current,true);Assert.That(hits,Is.EqualTo(2));
            yield return null;
        }
    }
}
