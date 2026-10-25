using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Tests
{
    public sealed class ResourceReplicationRegressionTests
    {
        [TestCase("")][TestCase("r0")][TestCase("c:-27:53:6:11")][TestCase("poi:graveyard:12:3")]
        public void ResourceWireRoundTripsWithExactLength(string id)
        {
            var state=new ResourceNetworkState{Id=id,Rule=6,HP=73,Hits=42};
            using(var writer=new FastBufferWriter(128,Allocator.Temp))
            {writer.WriteNetworkSerializable(state);Assert.That(writer.Length,Is.EqualTo(14+System.Text.Encoding.UTF8.GetByteCount(id)));using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out ResourceNetworkState value);Assert.That(value.Equals(state),Is.True);Assert.That(reader.Position,Is.EqualTo(writer.Length));}}
        }
        [Test]public void InactiveRootCannotResolveNgoBehaviourOwner()
        {
            var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkStone.prefab"));
            try{var state=go.GetComponent<NetworkResource>();Assert.That(state.GetComponentInParent<NetworkObject>(),Is.SameAs(go.GetComponent<NetworkObject>()));go.SetActive(false);Assert.That(state.GetComponentInParent<NetworkObject>(),Is.Null,"NGO uses this lookup in InitializeChildNetworkBehaviours; a disabled pending actor loses its wire fields");Assert.That(go.GetComponentsInChildren<NetworkBehaviour>(true).Length,Is.EqualTo(1));}
            finally{Object.DestroyImmediate(go);}
        }
    }
}
