using System.Linq;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class NetworkCreatureContractTests
    {
        [Test]public void RangedReplicaEndsPresentationWithoutLaunchingAnotherStone()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkSlingshotGoblin.prefab");
            var actor=Object.Instantiate(prefab);
            try
            {
                var shot=actor.GetComponent<SlingshotAttack>();shot.PresentReplica(true,shot.Definition.Duration+.1f,Vector2.down);
                Assert.That(shot.IsBusy,Is.False);Assert.That(shot.Band.enabled,Is.False);Assert.That(shot.PouchStone.enabled,Is.False);Assert.That(shot.Shots,Is.Zero);
            }
            finally{Object.DestroyImmediate(actor);}
        }
        [Test]public void EveryEcologySpawnHasRegisteredServerPrefabAndSharedBrain()
        {
            var session=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkSession.prefab").GetComponent<NetworkSession>();
            var entries=session.EcologyPrefab.GetComponent<NetworkCreatureWorld>().Prefabs;
            Assert.That(entries.Length,Is.EqualTo(63));Assert.That(entries.Select(e=>e.NetworkPrefab).Distinct().Count(),Is.EqualTo(63));
            foreach(var e in entries)
            {
                Assert.That(e.Source,Is.Not.Null);Assert.That(e.NetworkPrefab.GetComponent<NetworkObject>(),Is.Not.Null);
                var source=e.Source.GetComponent<CreatureMember>();if(source==null)continue;
                var creature=e.NetworkPrefab.GetComponent<NetworkCreature>();Assert.That(creature,Is.Not.Null);
                Assert.That(e.NetworkPrefab.GetComponent<CreatureMember>().Definition,Is.SameAs(source.Definition));
                Assert.That(creature.Identity.WritePerm,Is.EqualTo(NetworkVariableWritePermission.Server));Assert.That(creature.Frame.WritePerm,Is.EqualTo(NetworkVariableWritePermission.Server));
                var transform=e.NetworkPrefab.GetComponent<NetworkTransform>();Assert.That(transform.Interpolate && transform.UseUnreliableDeltas,Is.True);Assert.That(transform.PositionThreshold,Is.GreaterThan(0));
                Assert.That(creature.Visual,Is.Not.Null);Assert.That(e.NetworkPrefab.GetComponent<LootOnDeath>().Table,Is.Not.Null);
            }
            Assert.That(entries.Count(e=>e.Source.GetComponent<CreatureMember>()!=null),Is.EqualTo(17));
            Assert.That(entries.Single(e=>e.Source.GetComponent<StoneProjectile>()!=null).NetworkPrefab.GetComponent<NetworkProjectile>(),Is.Not.Null);
        }
        [Test]public void PlayerRequestsNeverContainTargetDamageOrClientHitResult()
        {
            var attack=typeof(NetworkPlayer).GetMethod("AttackRpc",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            Assert.That(attack.GetParameters().Single().ParameterType,Is.EqualTo(typeof(RpcParams)));
            var rpc=(RpcAttribute)attack.GetCustomAttributes(typeof(RpcAttribute),false).Single();Assert.That(rpc.InvokePermission,Is.EqualTo(RpcInvokePermission.Owner));
        }
        [Test]public void MultiplayerWorldHasNoPersistenceAndKeepsPopulationDefinitions()
        {
            var world=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkEcology.prefab").GetComponent<NetworkCreatureWorld>();
            Assert.That(world.GetComponent<WorldSaveService>(),Is.Null);Assert.That(world.World.GenerateOnAwake,Is.False);
            Assert.That(world.Animals.Definition.Rules.Length,Is.EqualTo(13));Assert.That(world.Goblins.CampDefinition.Prefab,Is.Not.Null);
            Assert.That(world.World.Water,Is.Not.Null);Assert.That(world.World.Pickup,Is.Not.Null);
        }
    }
}
