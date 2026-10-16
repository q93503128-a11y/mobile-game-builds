using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Wildfolk.Tests
{
    public sealed class NetworkFoundationPlayTests
    {
        [UnityTest]public IEnumerator LocalHostSpawnsAuthoritativePlayerAndHostEcologyWithoutSave()
        {
            SceneFlow.Home(SaveRepository.Default());yield return null;yield return null;
            var home=Object.FindFirstObjectByType<HomeScreen>();home.OpenMultiplayer();var menu=home.Multiplayer.GetComponent<MultiplayerMenu>();
            var task=menu.Session.Host(ConnectionRoute.LocalNetwork);
            for(var i=0;i<300 && !task.IsCompleted;i++)yield return null;
            Assert.That(task.IsCompleted && task.Result,Is.True,menu.Session.LastFailure);
            Assert.That(menu.Session.Manager.NetworkConfig.Prefabs.Prefabs.Count,Is.EqualTo(12));
            Assert.That(menu.Session.EnterGame(),Is.True);
            NetworkPlayer player=null;
            for(var i=0;i<300;i++){yield return null;player=Object.FindFirstObjectByType<NetworkPlayer>();if(player!=null && player.IsSpawned)break;}
            Assert.That(player,Is.Not.Null);Assert.That(player.IsServer && player.IsOwner,Is.True);
            Assert.That(Object.FindObjectsByType<WorldGenerator>(FindObjectsSortMode.None).Length,Is.EqualTo(1));Assert.That(NetworkCreatureWorld.Active.IsAuthority,Is.True);Assert.That(Object.FindObjectsByType<WorldSaveService>(FindObjectsSortMode.None),Is.Empty);
            player.GetComponent<PlayerInputAdapter>().enabled=false;var before=player.transform.position;
            player.Commands.SetMove(Vector2.up);yield return new WaitForSeconds(.2f);Assert.That(player.transform.position.y,Is.GreaterThan(before.y+.1f));
            Assert.That(player.Commands.Dash(),Is.True);player.Commands.SetMove(Vector2.zero);yield return new WaitForSeconds(.25f);Assert.That(player.Motor.Locomotion.Stamina.Current,Is.LessThan(player.Motor.Locomotion.Stamina.Maximum));
            NetworkSession.Instance.Leave();for(var i=0;i<180 && Object.FindFirstObjectByType<HomeScreen>()==null;i++)yield return null;
            Assert.That(Object.FindFirstObjectByType<HomeScreen>(),Is.Not.Null);
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {if(NetworkSession.Instance!=null && NetworkSession.Instance.State!=NetworkSessionState.Idle){NetworkSession.Instance.Leave();for(var i=0;i<180 && NetworkSession.Instance.State!=NetworkSessionState.Idle;i++)yield return null;}Time.timeScale=1;}
    }
}
