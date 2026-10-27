using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Wildfolk.Tests
{
    public sealed class PremiumAdmissionPlayTests
    {
        private string previous,root;private NetworkSession session;
        private sealed class Unavailable:IPremiumAdmissionClient{
            public int Calls;public bool Available=>false;public string PlayerId=>null;
            public Task<PremiumAdmissionSnapshot> Issue(PremiumChallenge c){Calls++;return Task.FromResult<PremiumAdmissionSnapshot>(null);}
            public Task<PremiumAdmissionSnapshot> Verify(PremiumChallenge c,string player,string proof){Calls++;return Task.FromResult<PremiumAdmissionSnapshot>(null);}
        }
        [UnitySetUp]public IEnumerator Setup(){previous=UnityEditor.SessionState.GetString("Wildfolk.QA.Directory","");root=Path.Combine(Path.GetTempPath(),"LowfirePremiumPlay-"+Guid.NewGuid().ToString("N"));UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",root);ClassProfiles.DevelopmentProviders(new LocalAccountProgressionStore(root));SceneFlow.Repository=SaveRepository.Default();yield return SceneManager.LoadSceneAsync(SceneFlow.HomeScene);yield return null;}
        [UnityTest]public IEnumerator OfflineCachedEngineerCannotBecomeHostPremiumButFreeSessionStillStarts(){
            var d=AccountProgression.New();d.Find("engineer").Owned=true;d.Find("engineer").XP=50;d.Selected="engineer";Assert.That(ClassProfiles.Store.Write(ClassProfiles.LocalId,d),Is.True);
            session=NetworkSession.Instance??UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<NetworkSession>("Assets/Wildfolk/Prefabs/NetworkSession.prefab"));session.WorldRepository=new SaveRepository(Path.Combine(root,"qa-host.json"));var unavailable=new Unavailable();session.DevelopmentPremiumClient(unavailable);
            var host=session.Host(ConnectionRoute.LocalNetwork,false);while(!host.IsCompleted)yield return null;Assert.That(host.Result,Is.True);Assert.That(session.PremiumMessageKey,Is.EqualTo("net.premium_unavailable"));Assert.That(unavailable.Calls,Is.Zero);Assert.That(session.EnterGame(),Is.True);
            var end=Time.realtimeSinceStartup+40;while(!NetworkPlayer.Active.Any(p=>p.IsOwner)&&Time.realtimeSinceStartup<end)yield return null;var local=NetworkPlayer.Active.First(p=>p.IsOwner);
            Assert.That(local.GetComponent<PlayerClass>().Definition.Id,Is.EqualTo("survivor"));Assert.That(ClassProfiles.Local.Selected,Is.EqualTo("engineer"),"session fallback never rewrites durable selection");Assert.That(ClassProfiles.Local.Find("engineer").XP,Is.EqualTo(50));Assert.That(session.Manager.NetworkConfig.ProtocolVersion,Is.EqualTo(23));
        }
        [UnityTearDown]public IEnumerator Close(){if(session!=null){session.Leave(false,true);var end=Time.realtimeSinceStartup+20;while(session.State!=NetworkSessionState.Idle&&Time.realtimeSinceStartup<end)yield return null;UnityEngine.Object.Destroy(session.gameObject);yield return null;}yield return SceneManager.LoadSceneAsync(SceneFlow.HomeScene);ClassProfiles.DevelopmentProviders(null);SceneFlow.Repository=null;UnityEditor.SessionState.SetString("Wildfolk.QA.Directory",previous);if(Directory.Exists(root))Directory.Delete(root,true);}
    }
}
