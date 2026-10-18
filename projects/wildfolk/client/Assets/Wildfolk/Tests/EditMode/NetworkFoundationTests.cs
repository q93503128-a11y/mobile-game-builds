using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Unity.Netcode;

namespace Wildfolk.Tests
{
    public sealed class NetworkFoundationTests
    {
        [Test]public void CommandsRejectNonFiniteAndClampUntrustedMagnitude()
        {Assert.That(NetworkInputRules.Sanitize(new Vector2(float.NaN,1)),Is.EqualTo(Vector2.zero));Assert.That(NetworkInputRules.Sanitize(new Vector2(0,float.PositiveInfinity)),Is.EqualTo(Vector2.zero));Assert.That(NetworkInputRules.Sanitize(new Vector2(90,40)).magnitude,Is.EqualTo(1).Within(.0001));}
        [Test]public void PlayerStateIsServerWritableAndRpcInputsAreOwnerOnly()
        {
            var player=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/NetworkPlayer.prefab").GetComponent<NetworkPlayer>();
            Assert.That(player.Status.WritePerm,Is.EqualTo(NetworkVariableWritePermission.Server));Assert.That(player.Motion.WritePerm,Is.EqualTo(NetworkVariableWritePermission.Server));
            foreach(var method in typeof(NetworkPlayer).GetMethods(System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance))
                foreach(var attribute in method.GetCustomAttributesData())if(attribute.AttributeType==typeof(RpcAttribute)){var destination=(SendTo)(int)attribute.ConstructorArguments[0].Value;var rpc=(RpcAttribute)method.GetCustomAttributes(typeof(RpcAttribute),false)[0];Assert.That(rpc.InvokePermission,Is.EqualTo(destination==SendTo.Server ? RpcInvokePermission.Owner : RpcInvokePermission.Server),method.Name);}
        }
        [Test]public void InternetCodeAndLocalAddressAreDifferentRoutes()
        {Assert.That(RelayRoomConnection.ValidCode("AB12CD"),Is.True);Assert.That(RelayRoomConnection.ValidCode("127.0.0.1:7777"),Is.False);Assert.That(RelayRoomConnection.ValidCode("no"),Is.False);Assert.That(LocalRoomConnection.TryAddress("127.0.0.1:7777",out var ip,out var port),Is.True);Assert.That(ip,Is.EqualTo("127.0.0.1"));Assert.That(port,Is.EqualTo(7777));Assert.That(LocalRoomConnection.TryAddress("192.168.1.5:0",out _,out _),Is.False);}
    }
}
