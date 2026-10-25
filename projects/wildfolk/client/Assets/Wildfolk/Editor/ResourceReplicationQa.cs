using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk.Editor
{
    // Explicit two-process regression; snapshots compare actual NGO observers, not simulated peers.
    public static class ResourceReplicationQa
    {
        [Serializable] public sealed class Sample { public ulong NetworkId;public string Id;public int Rule,HP; }
        [Serializable] public sealed class Manifest { public Sample[] Resources; }
        private static void Check(bool ok,string dir,string side,string text)
        {File.AppendAllText(Path.Combine(dir,"resources-"+side+".tsv"),DateTime.UtcNow.ToString("O")+"\t"+(ok?"PASS ":"FAIL ")+text+"\n");if(!ok)throw new InvalidOperationException(text);}
        private static NetworkResource[] Actors()=>UnityEngine.Object.FindObjectsByType<NetworkResource>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(n=>n.IsSpawned).ToArray();
        private static IEnumerator Delay(float seconds){var end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
        private static void Move(NetworkPlayer player,Vector2 point){player.GetComponent<Rigidbody2D>().position=point;player.transform.position=point;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
        private static void Reconcile(NetworkCreatureWorld w){w.World.Chunks.Reconcile();w.World.Chunks.DrainPending();w.FlushPending();}
        public static IEnumerator Probe(string stage,Func<NetworkPlayer> peer,string dir,Action<string> command,Func<string,IEnumerator> ack)
        {
            yield return Delay(.5f);var all=Actors();Check(all.All(n=>n.gameObject.activeInHierarchy),dir,"host","spawned resource roots remain active "+stage);
            var visible=all.Where(n=>n.NetworkObject.IsNetworkVisibleTo(peer().OwnerClientId)).OrderBy(n=>n.NetworkObjectId).ToArray();
            Check(visible.Length>0 && visible.All(n=>n.State.Value.Id.Length>0),dir,"host","initialized visible resource count="+visible.Length+" "+stage);
            var states=visible.Select(n=>new Sample{NetworkId=n.NetworkObjectId,Id=n.State.Value.Id.ToString(),Rule=n.State.Value.Rule,HP=n.State.Value.HP}).ToArray();
            Check(states.Select(s=>s.Id).Distinct().Count()==states.Length,dir,"host","unique logical IDs "+stage);
            File.WriteAllText(Path.Combine(dir,stage+".json"),JsonUtility.ToJson(new Manifest{Resources=states}));command(stage);yield return ack(stage);
        }
        public static IEnumerator Receive(string stage,string dir)
        {
            var expected=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(dir,stage+".json"))).Resources;var end=Time.realtimeSinceStartup+40;bool matches=false;
            while(Time.realtimeSinceStartup<end)
            {
                var actual=Actors();matches=actual.Length==expected.Length && actual.All(n=>n.gameObject.activeInHierarchy) && actual.Select(n=>n.State.Value.Id.ToString()).Distinct().Count()==actual.Length;
                if(matches)foreach(var e in expected){var n=actual.FirstOrDefault(a=>a.NetworkObjectId==e.NetworkId);if(n==null || n.State.Value.Id.ToString()!=e.Id || n.State.Value.Rule!=e.Rule || n.State.Value.HP!=e.HP || n.GetComponent<Health>().Current!=e.HP){matches=false;break;}}
                if(matches)break;yield return null;
            }
            Check(matches,dir,"client","exact observer set / ID / rule / HP; no missing, stale or duplicate replicas; count="+expected.Length+" "+stage);
        }
        public static IEnumerator Exercise(NetworkCreatureWorld w,Func<NetworkPlayer> local,Func<NetworkPlayer> peer,string dir,Action<string> command,Func<string,IEnumerator> ack)
        {
            yield return Probe("resources_initial",peer,dir,command,ack);
            var rich=w.World.Map.Continental.RockyAnchor;Move(local(),rich+Vector2.down*3);Move(peer(),rich+Vector2.down*2);Reconcile(w);yield return Probe("resources_rich",peer,dir,command,ack);
            var tool=w.Catalog.Item("steel_pickaxe").Tool;var nodes=Actors().Where(n=>n.NetworkObject.IsNetworkVisibleTo(peer().OwnerClientId) && !n.State.Value.Id.ToString().StartsWith("poi:") && n.GetComponent<ResourceNode>().Definition.CanHarvest(tool)).OrderBy(n=>Vector2.Distance(n.transform.position,rich)).ToArray();Check(nodes.Length>0,dir,"host","harvestable rich-chunk resource exists");
            var target=nodes[0];var id=target.State.Value.Id.ToString();var node=target.GetComponent<ResourceNode>();Move(local(),(Vector2)target.transform.position+Vector2.down);Move(peer(),(Vector2)target.transform.position+Vector2.down*2);
            var first=node.Harvest(tool,1,local().GetComponent<PlayerInventory>());Check(first==1,dir,"host","real resource Harvest updates HP");yield return Probe("resources_damage",peer,dir,command,ack);
            var replica=target.GetComponent<Health>();Check(node.Harvest(tool,replica.Current,local().GetComponent<PlayerInventory>())>0,dir,"host","real harvest death uses authoritative drop/despawn");File.WriteAllText(Path.Combine(dir,"harvested-resource.txt"),id);yield return Probe("resources_harvest",peer,dir,command,ack);
            for(var cycle=0;cycle<2;cycle++)
            {
                Move(local(),w.World.Map.Spawn);Move(peer(),w.World.Map.Spawn+Vector2.right);Reconcile(w);yield return Probe("resources_leave"+cycle,peer,dir,command,ack);
                Move(local(),rich+Vector2.down*3);Move(peer(),rich+Vector2.down*2);Reconcile(w);yield return Probe("resources_return"+cycle,peer,dir,command,ack);
                Check(!Actors().Any(n=>n.State.Value.Id.ToString()==id),dir,"host","harvest tombstone retained on chunk revisit "+cycle);
            }
        }
    }
}
