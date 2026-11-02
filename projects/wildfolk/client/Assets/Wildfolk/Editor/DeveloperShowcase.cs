using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class DeveloperShowcase
    {
        private static readonly List<GameObject> spawned=new List<GameObject>();
        public static string LastStatus {get;private set;}="Choose a creature. Player is the current actor.";
        // Real local prefabs only: no network replicas, facilities, or invented families.
        public static string[] Families=>AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Wildfolk/Prefabs"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>!System.IO.Path.GetFileName(p).StartsWith("Network",StringComparison.Ordinal)).Where(p=>{var g=AssetDatabase.LoadAssetAtPath<GameObject>(p);return g.GetComponent<NeutralAnimal>()!=null||g.GetComponent<EnemyController>()!=null||g.GetComponent<GoblinController>()!=null;}).Select(p=>System.IO.Path.GetFileNameWithoutExtension(p)=="Enemy"?"Zombie":System.IO.Path.GetFileNameWithoutExtension(p)).Concat(new[]{"Skeleton (sword)","Undead King (isolated)"}).OrderBy(n=>n).ToArray();
        public static bool Ground(Vector2 desired,float radius,out Vector2 point)
        {
            var s=DeveloperHarness.Context();Physics2D.SyncTransforms();var map=s.World.Map;var hits=new Collider2D[64];
            for(var i=0;i<96;i++){var r=i==0?0:Mathf.Sqrt(i)*.28f;var p=desired+new Vector2(Mathf.Cos(i*2.399963f),Mathf.Sin(i*2.399963f))*r;if(!map.IsWalkable(p,radius)||map.IsWater(p))continue;
                var count=Physics2D.OverlapCircle(p,radius,new ContactFilter2D{useTriggers=false},hits);if(count==hits.Length)continue;var blocked=false;for(var j=0;j<count;j++)if(hits[j].GetComponentInParent<WorldPlaceable>()!=null||hits[j].GetComponentInParent<ResourceNode>()!=null||hits[j].GetComponentInParent<PlayerInventory>()!=null||hits[j].GetComponentInParent<Health>()!=null){blocked=true;break;}if(blocked)continue;point=p;return true;}
            point=default;return false;
        }
        public static Component NearPlayer(string family)
        {
            var s=DeveloperHarness.Context();if(!DevelopmentProfile.Isolated)throw new InvalidOperationException("Showcase requires an isolated QA profile. Stop Play and choose Start Isolated QA Play.");
            var player=s.Player;var facing=player.GetComponent<CharacterFacing>();var direction=Mathf.Abs(facing.Direction.x)>.6f?new Vector2(Mathf.Sign(facing.Direction.x),0):Vector2.right;var desired=(Vector2)player.transform.position+direction*3.5f;
            if(!Ground(desired,family.StartsWith("Undead King")?1.5f:.7f,out var point))throw new InvalidOperationException("Spawn failed: no valid ground near player.");
            Component actor;
            if(family=="Undead King (isolated)")
            {
                var world=s.World.GetComponent<BossWorld>();if(world.Encounters.Count>0)throw new InvalidOperationException("Clear the existing boss encounter first.");
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/UndeadKing.prefab");var definition=prefab.GetComponent<BossEncounter>().Definition;
                var records=world.Capture().ToList();records.RemoveAll(r=>r.Boss==definition.Id);records.Add(new BossSnapshot{Boss=definition.Id,Home=point,Position=point,HP=definition.Vitality.MaximumHealth,Active=true});
                world.Restore(records.ToArray());actor=world.Encounters.Single();
            }
            else actor=DeveloperHarness.Spawn(family=="Skeleton (sword)"?"Skeleton":family,point,family=="Skeleton (sword)");
            Physics2D.SyncTransforms();var ownership=actor.GetComponent<DeveloperSpawnOwner>()??actor.gameObject.AddComponent<DeveloperSpawnOwner>();ownership.hideFlags=HideFlags.DontSave;ownership.Released=g=>spawned.Remove(g);spawned.Add(actor.gameObject);LastStatus="Spawned "+family+" ×1 at "+point.ToString("F1")+" · QA tracked";return actor;
        }
        public static int Clear()
        {var count=0;foreach(var g in spawned.ToArray())if(g!=null){var boss=g.GetComponent<BossEncounter>();if(boss!=null){boss.Record.Active=false;boss.Record.PendingReward=false;boss.World.Release(boss);}else PrefabPool.Release(g);count++;}spawned.Clear();LastStatus="Cleared "+count+" QA actors; natural population preserved.";return count;}
        public static void Group(string[] families)
        {foreach(var family in families)NearPlayer(family);LastStatus="Spawned QA group ×"+families.Length+" near current player.";}
        public static string Teleport(Vector2 desired)
        {
            var s=DeveloperHarness.Context();if(s.Player.GetComponent<NetworkPlayer>()!=null)throw new InvalidOperationException("Teleport requires the single-player QA world.");
            if(!Ground(desired,.6f,out var p))throw new InvalidOperationException("No valid destination near "+desired);
            var original=(Vector2)s.Player.transform.position;
            DeveloperHarness.Teleport(p);s.World.Chunks?.Reconcile();s.World.Chunks?.DrainPending();
            // Materialized resources may occupy a previously unloaded destination. Search again.
            if(!Ground(p,.6f,out var final)){DeveloperHarness.Teleport(original);s.World.Chunks?.Reconcile();s.World.Chunks?.DrainPending();throw new InvalidOperationException("Destination blocked after chunk activation; original position restored.");}
            DeveloperHarness.Teleport(final);return "Teleport successful: "+final.ToString("F1");
        }
    }
}
