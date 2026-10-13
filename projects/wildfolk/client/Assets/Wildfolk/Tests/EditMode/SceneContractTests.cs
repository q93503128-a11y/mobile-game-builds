using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wildfolk.Tests
{
    public sealed class SceneContractTests
    {
        [Test] public void SavedSceneRetainsPlayableConnectionsAndNonPushingEnemy()
        {
            const string path="Assets/Wildfolk/Scenes/Wildfolk.unity";
            var scene=SceneManager.GetSceneByPath(path);var wasLoaded=scene.isLoaded;
            if(!wasLoaded) scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                PlayerMotor player=null;EnemyController enemy=null;var nodes=0;
                foreach(var root in scene.GetRootGameObjects())
                {
                    player=player ?? root.GetComponentInChildren<PlayerMotor>();
                    enemy=enemy ?? root.GetComponentInChildren<EnemyController>();
                    nodes+=root.GetComponentsInChildren<ResourceNode>().Length;
                    foreach(var transform in root.GetComponentsInChildren<Transform>(true))
                        Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject),Is.Zero);
                }
                Assert.That(player,Is.Not.Null);Assert.That(player.Session,Is.Not.Null);Assert.That(player.Joystick,Is.Not.Null);
                Assert.That(player.Combat.Session,Is.SameAs(player.Session));
                Assert.That(enemy,Is.Not.Null);Assert.That(enemy.Session,Is.SameAs(player.Session));
                Assert.That(enemy.PlayerHealth,Is.SameAs(player.GetComponent<Health>()));
                Assert.That(enemy.GetComponent<Collider2D>().isTrigger,Is.True);
                Assert.That(enemy.GetComponent<Rigidbody2D>().bodyType,Is.EqualTo(RigidbodyType2D.Kinematic));
                Assert.That(nodes,Is.GreaterThan(0));
                Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None),Is.Empty);
            }
            finally { if(!wasLoaded) EditorSceneManager.CloseScene(scene,true); }
        }

        [Test] public void BalanceDataHasValidCombatRangesAndOnlyTwoActualItems()
        {
            var settings=AssetDatabase.LoadAssetAtPath<GameplaySettings>("Assets/Wildfolk/Data/GameplaySettings.asset");
            Assert.That(settings.PunchCooldown,Is.GreaterThanOrEqualTo(settings.PunchDuration));
            Assert.That(settings.EnemyAttackReach,Is.GreaterThan(settings.EnemyStopDistance));
            Assert.That(settings.EnemyStopDistance,Is.GreaterThan(settings.EnemySeparation));
            Assert.That(settings.DropPickupDelay,Is.GreaterThan(settings.DropTravelTime));
            foreach(var field in typeof(GameplaySettings).GetFields())
            {
                if(field.FieldType==typeof(float)) Assert.That((float)field.GetValue(settings),Is.GreaterThan(0),field.Name);
                if(field.FieldType==typeof(int)) Assert.That((int)field.GetValue(settings),Is.GreaterThan(0),field.Name);
            }
            Assert.That(settings.EnemyLoseRange,Is.GreaterThanOrEqualTo(settings.EnemyDetectionRange));
            Assert.That(AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/Wildfolk/Data"}).Length,Is.EqualTo(2));
            var ids=new HashSet<string>();
            foreach(var guid in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(item.MaxStack,Is.GreaterThan(0));Assert.That(item.Icon,Is.Not.Null);Assert.That(item.Id,Is.Not.Empty);
                Assert.That(ids.Add(item.Id),Is.True,"Duplicate item ID: "+item.Id);
            }
            foreach(var guid in AssetDatabase.FindAssets("t:VitalityDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var vitality=AssetDatabase.LoadAssetAtPath<VitalityDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(vitality.MaximumHealth,Is.GreaterThan(0));
            }
            foreach(var guid in AssetDatabase.FindAssets("t:ResourceDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var resource=AssetDatabase.LoadAssetAtPath<ResourceDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(resource.Vitality,Is.Not.Null);Assert.That(resource.DropItem,Is.Not.Null);
                Assert.That(resource.DropCount,Is.InRange(1,resource.DropItem.MaxStack));
            }
        }
    }
}
