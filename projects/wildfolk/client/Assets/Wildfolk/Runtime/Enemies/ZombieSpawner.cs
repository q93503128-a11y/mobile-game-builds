using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class ZombieSpawner : MonoBehaviour
    {
        public ZombieSpawnDefinition Definition;
        public EnemyController Prefab;
        public WorldClock Clock;
        public GameplaySession Session;
        public Health Player;
        public Camera Camera;
        public WorldSpawnRegion Region;
        public WorldGenerator World;
        private float elapsed;
        private DayPhase previous;
        private int openingRemaining;
        private readonly Collider2D[] obstacles=new Collider2D[64];
        private readonly RaycastHit2D[] barriers=new RaycastHit2D[32];
        private static readonly Vector2[] cardinal={Vector2.up,Vector2.down,Vector2.left,Vector2.right};
        private readonly List<EnemyController> enemies=new List<EnemyController>();
        public int TargetPopulation=>Definition.PopulationFor(Clock.Model.Day);
        private void Start(){previous=Clock.Model.Phase;enemies.AddRange(FindObjectsByType<EnemyController>(FindObjectsSortMode.None));}
        public int ActiveCount {get{for(var i=enemies.Count-1;i>=0;i--)if(enemies[i]==null)enemies.RemoveAt(i);return enemies.Count;}}
        private void Update()
        {
            if(!Session.WorldIsRunning || !Player.IsAlive)return;
            var phase=Clock.Model.Phase;
            if(phase!=previous){previous=phase;elapsed=0;openingRemaining=phase==DayPhase.Night ? Mathf.CeilToInt(TargetPopulation*Definition.InitialFraction) : 0;}
            if(phase!=DayPhase.Night)return;
            elapsed+=Time.deltaTime;
            if(openingRemaining>0)
            {if(elapsed<Definition.InitialInterval)return;elapsed=0;if(TrySpawn())openingRemaining--;return;}
            if(ActiveCount>=TargetPopulation*Definition.RefillThreshold){elapsed=0;return;}
            if(elapsed<Definition.RefillInterval)return;elapsed=0;TrySpawn();
        }
        public bool IsValidPosition(Vector2 position)
        {
            if(!Region.Bounds.Contains(position) || !World.Map.IsWalkable(position,Definition.FootprintRadius) || Vector2.Distance(position,Player.transform.position)<Definition.PlayerDistance || GameplayLight.IsSafe(position))return false;
            var viewport=Camera.WorldToViewportPoint(position);var margin=Definition.CameraMargin;
            if(viewport.z<=0 || viewport.x>=-margin && viewport.x<=1+margin && viewport.y>=-margin && viewport.y<=1+margin)return false;
            if(Physics2D.OverlapCircle(position,Definition.FootprintRadius,new ContactFilter2D{useTriggers=true},obstacles)>0)return false;
            var count=Physics2D.OverlapCircle(position,Definition.BuildingDistance,new ContactFilter2D{useTriggers=true},obstacles);
            for(var i=0;i<count;i++)if(obstacles[i].GetComponentInParent<WorldPlaceable>()!=null)return false;
            return !Enclosed(position);
        }
        private bool Enclosed(Vector2 point)
        {
            foreach(var direction in cardinal)
            {
                var count=Physics2D.Raycast(point,direction,new ContactFilter2D{useTriggers=false},barriers,60);var found=false;
                for(var i=0;i<count;i++){var wall=barriers[i].collider.GetComponentInParent<WorldPlaceable>();if(wall!=null && wall.Definition.IsBarrier){found=true;break;}}
                if(!found)return false;
            }
            return true;
        }
        public bool TrySpawn()
        {
            if(!Session.WorldIsRunning || !Player.IsAlive || Clock.Model.Phase!=DayPhase.Night || ActiveCount>=TargetPopulation)return false;
            for(var i=0;i<Definition.Attempts;i++)
            {
                var angle=Random.value*Mathf.PI*2;var radius=Random.Range(Definition.SpawnNear,Definition.SpawnFar);
                var point=(Vector2)Player.transform.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                if(!IsValidPosition(point))continue;
                var enemy=Instantiate(Prefab,point,Quaternion.identity);enemy.Session=Session;enemy.PlayerHealth=Player;enemy.Clock=Clock;
                var lifecycle=enemy.GetComponent<ZombieLifecycle>();lifecycle.Clock=Clock;lifecycle.Definition=Definition;
                var loot=enemy.GetComponent<LootOnDeath>();loot.Player=Player.GetComponent<PlayerInventory>();enemies.Add(enemy);return true;
            }
            return false;
        }
    }
}
