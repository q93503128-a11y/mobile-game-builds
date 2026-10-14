using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class GoblinSpawner : MonoBehaviour
    {
        public GoblinDefinition Definition;
        public GoblinController Prefab;
        public WorldGenerator World;
        public WorldClock Clock;
        public GameplaySession Session;
        public Health Player;
        public Camera Camera;
        private readonly List<GoblinController> goblins=new List<GoblinController>();
        private readonly Collider2D[] obstacles=new Collider2D[16];
        private float elapsed;
        public void Register(GoblinController goblin){if(!goblins.Contains(goblin))goblins.Add(goblin);}
        public int ActiveCount {get{for(var i=goblins.Count-1;i>=0;i--)if(goblins[i]==null || !goblins[i].gameObject.activeInHierarchy)goblins.RemoveAt(i);return goblins.Count;}}
        private bool OutsideCamera(Vector2 p){var v=Camera.WorldToViewportPoint(p);return v.x<-.05f || v.x>1.05f || v.y<-.05f || v.y>1.05f;}
        private void Update()
        {
            if(!Session.WorldIsRunning || !Player.IsAlive)return;
            if(Clock.Model.Phase!=DayPhase.Day)
            {elapsed=0;for(var i=goblins.Count-1;i>=0;i--)if(goblins[i]!=null && !goblins[i].InCombat && OutsideCamera(goblins[i].transform.position)){PrefabPool.Release(goblins[i].gameObject);goblins.RemoveAt(i);}return;}
            elapsed+=Time.deltaTime;if(elapsed<Definition.SpawnInterval)return;elapsed=0;
            if(Random.value<Definition.EncounterChance)TrySpawn();
        }
        public bool TrySpawn()
        {
            if(Clock.Model.Phase!=DayPhase.Day || !Session.WorldIsRunning || ActiveCount>=Definition.Population)return false;
            for(var attempt=0;attempt<64;attempt++)
            {
                var road=World.Map.Roads[Random.Range(0,World.Map.Roads.Count)];var segment=Random.Range(1,road.Length);var p=Vector2.Lerp(road[segment-1],road[segment],Random.value);
                if(Vector2.Distance(p,Player.transform.position)<15 || Vector2.Distance(p,Player.transform.position)>45 || !OutsideCamera(p) || !World.Map.IsWalkable(p,1) || Physics2D.OverlapCircle(p,1,new ContactFilter2D{useTriggers=true},obstacles)>0)continue;
                var goblin=PrefabPool.Spawn(Prefab,p,Quaternion.identity);goblin.Session=Session;goblin.PlayerHealth=Player;goblin.GetComponent<LootOnDeath>().Player=Player.GetComponent<PlayerInventory>();Register(goblin);return true;
            }
            return false;
        }
    }
}
