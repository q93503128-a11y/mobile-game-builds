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
        private float elapsed;
        private DayPhase previous;
        private readonly Collider2D[] obstacles=new Collider2D[32];
        private readonly List<EnemyController> enemies=new List<EnemyController>();
        private void Start() {enemies.AddRange(FindObjectsByType<EnemyController>(FindObjectsSortMode.None));previous=Clock.Model.Phase;}
        public int ActiveCount { get {for(var i=enemies.Count-1;i>=0;i--)if(enemies[i]==null)enemies.RemoveAt(i);return enemies.Count;} }
        private void Update()
        {
            if(!Session.WorldIsRunning || !Player.IsAlive)return;
            var phase=Clock.Model.Phase;
            if(phase!=previous){previous=phase;elapsed=0;}
            elapsed+=Time.deltaTime;
            var interval=phase==DayPhase.Night ? Definition.NightInterval : Definition.DayInterval;
            if(elapsed<interval)return;elapsed=0;TrySpawn();
        }
        public bool IsValidPosition(Vector2 position)
        {
            if(!Region.Bounds.Contains(position) || Vector2.Distance(position,Player.transform.position)<Definition.PlayerDistance)return false;
            var viewport=Camera.WorldToViewportPoint(position);
            var margin=Definition.CameraMargin;
            if(viewport.z<=0 || (viewport.x>=-margin && viewport.x<=1+margin && viewport.y>=-margin && viewport.y<=1+margin))return false;
            var count=Physics2D.OverlapCircle(position,Definition.FootprintRadius,new ContactFilter2D{useTriggers=false},obstacles);
            if(count>0)return false;
            count=Physics2D.OverlapCircle(position,Definition.BuildingDistance,new ContactFilter2D{useTriggers=true},obstacles);
            for(var i=0;i<count;i++)if(obstacles[i].GetComponentInParent<WorldPlaceable>()!=null)return false;
            return true;
        }
        public bool TrySpawn()
        {
            if(!Session.WorldIsRunning || !Player.IsAlive)return false;
            var limit=Clock.Model.Phase==DayPhase.Night ? Definition.NightLimit : Definition.DayLimit;
            if(ActiveCount>=limit)return false;
            for(var i=0;i<Definition.Attempts;i++)
            {
                var point=Region.PerimeterPoint(Random.Range(0,4),Random.value);
                if(!IsValidPosition(point))continue;
                var enemy=Instantiate(Prefab,point,Quaternion.identity);
                enemy.Session=Session;enemy.PlayerHealth=Player;enemy.Clock=Clock;enemies.Add(enemy);return true;
            }
            return false;
        }
    }
}
