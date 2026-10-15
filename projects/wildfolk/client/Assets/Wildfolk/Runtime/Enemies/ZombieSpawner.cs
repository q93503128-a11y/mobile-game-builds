using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class ZombieSpawner : MonoBehaviour
    {
        public ZombieSpawnDefinition Definition;
        public EnemyController Prefab;
        public EnemyController SecondaryPrefab;
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
        private readonly Dictionary<EnemyController,float> inactive=new Dictionary<EnemyController,float>();
        private readonly List<EnemyController> enemies=new List<EnemyController>();
        public int TargetPopulation=>Definition.PopulationFor(Clock.Model.Day);
        public EnemyController SpeciesPrefab(string id)
        {foreach(var prefab in new[]{Prefab,SecondaryPrefab})if(prefab!=null && prefab.Member.Definition.Id==id)return prefab;throw new System.IO.InvalidDataException("Unknown nocturnal species");}
        public void Restore(NightCreatureSnapshot[] states)
        {
            foreach(var state in states)
            {
                var enemy=PrefabPool.Spawn(SpeciesPrefab(state.Species),state.Position,Quaternion.identity);enemy.Session=Session;enemy.PlayerHealth=Player;enemy.Clock=Clock;enemy.Lifecycle.Clock=Clock;enemy.Lifecycle.Definition=Definition;enemy.Lifecycle.RestoreActive();enemy.OwnHealth.RestoreHealth(state.HP);enemy.GetComponent<LootOnDeath>().Player=World.Player;if(enemy.Variant!=null)enemy.Variant.SetEquipped(!string.IsNullOrEmpty(state.Variant));Register(enemy);
            }
            previous=Clock.Model.Phase;openingRemaining=0;elapsed=0;
        }
        public void Register(EnemyController enemy){if(!enemies.Contains(enemy))enemies.Add(enemy);}
        private void Start(){previous=Clock.Model.Phase;enemies.AddRange(FindObjectsByType<EnemyController>(FindObjectsSortMode.None));openingRemaining=previous==DayPhase.Night ? Mathf.CeilToInt(TargetPopulation*Definition.InitialFraction) : 0;}
        private void Prune()
        {for(var i=enemies.Count-1;i>=0;i--)if(enemies[i]==null || !enemies[i].gameObject.activeInHierarchy){if(!object.ReferenceEquals(enemies[i],null))inactive.Remove(enemies[i]);enemies.RemoveAt(i);}}
        public int ActiveCount {get{Prune();return enemies.Count;}}
        public int LocalThreatCount {get{Prune();var count=0;foreach(var enemy in enemies)if(Vector2.Distance(enemy.transform.position,Player.transform.position)<=Definition.LocalThreatRadius)count++;return count;}}
        public int ActivePursuerCount {get{var count=0;foreach(var enemy in enemies)if(enemy!=null && enemy.gameObject.activeInHierarchy && enemy.State!=EnemyState.Idle)count++;return count;}}
        private void RetireInactive()
        {
            var navigation=World.GetComponent<WorldNavigation>();
            foreach(var enemy in enemies)
            {
                if(enemy==null || !enemy.gameObject.activeInHierarchy || enemy.Lifecycle.Phase!=ZombiePhase.Active)continue;
                var eligible=enemy.State==EnemyState.Idle && Vector2.Distance(enemy.transform.position,Player.transform.position)>Definition.InactiveDistance && navigation.OutsideCamera(enemy.transform.position);
                if(!eligible){inactive.Remove(enemy);continue;}
                if(!inactive.TryGetValue(enemy,out var since))inactive[enemy]=Time.time;
                else if(Time.time-since>=Definition.InactiveSeconds){enemy.Lifecycle.Burrow();inactive.Remove(enemy);}
            }
        }
        private void Update()
        {
            if(!Session.WorldIsRunning || !Player.IsAlive)return;
            Prune();RetireInactive();
            var phase=Clock.Model.Phase;
            if(phase!=previous){previous=phase;elapsed=0;openingRemaining=phase==DayPhase.Night ? Mathf.CeilToInt(TargetPopulation*Definition.InitialFraction) : 0;}
            if(phase!=DayPhase.Night)return;
            elapsed+=Time.deltaTime;
            if(openingRemaining>0)
            {if(elapsed<Definition.InitialInterval)return;elapsed=0;if(TrySpawn())openingRemaining--;return;}
            if(LocalThreatCount>=TargetPopulation*Definition.RefillThreshold){elapsed=0;return;}
            if(elapsed<Definition.RefillInterval)return;elapsed=0;TrySpawn();
        }
        public bool IsValidPosition(Vector2 position)
        {
            if(!Region.Bounds.Contains(position) || !World.GetComponent<WorldNavigation>().CanSpawn(position,Definition.FootprintRadius,Definition.PlayerDistance,false) || Vector2.Distance(position,Player.transform.position)<Definition.PlayerDistance || GameplayLight.IsSafe(position))return false;
            var viewport=Camera.WorldToViewportPoint(position);var margin=Definition.CameraMargin;
            if(viewport.z<=0 || viewport.x>=-margin && viewport.x<=1+margin && viewport.y>=-margin && viewport.y<=1+margin)return false;
            if(Physics2D.OverlapCircle(position,Definition.FootprintRadius,new ContactFilter2D{useTriggers=true},obstacles)>0)return false;
            var count=Physics2D.OverlapCircle(position,Definition.BuildingDistance,new ContactFilter2D{useTriggers=true},obstacles);
            for(var i=0;i<count;i++)if(obstacles[i].GetComponentInParent<WorldPlaceable>()!=null)return false;
            return true;
        }
        public bool TrySpawn()
        {
            if(!Session.WorldIsRunning || !Player.IsAlive || Clock.Model.Phase!=DayPhase.Night || ActiveCount>=Definition.HardCap || LocalThreatCount>=TargetPopulation)return false;
            for(var i=0;i<Definition.Attempts;i++)
            {
                var angle=Random.value*Mathf.PI*2;var radius=Random.Range(Definition.SpawnNear,Definition.SpawnFar);
                var point=(Vector2)Player.transform.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                if(!IsValidPosition(point))continue;
                var enemy=PrefabPool.Spawn(SecondaryPrefab!=null && Random.value<Definition.SecondarySpeciesChance ? SecondaryPrefab : Prefab,point,Quaternion.identity);enemy.Session=Session;enemy.PlayerHealth=Player;enemy.Clock=Clock;
                if(enemy.Variant!=null)enemy.Variant.SetEquipped(Random.value<Definition.SwordVariantChance);
                var lifecycle=enemy.GetComponent<ZombieLifecycle>();lifecycle.Clock=Clock;lifecycle.Definition=Definition;
                var loot=enemy.GetComponent<LootOnDeath>();loot.Player=Player.GetComponent<PlayerInventory>();Register(enemy);return true;
            }
            return false;
        }
    }
}
