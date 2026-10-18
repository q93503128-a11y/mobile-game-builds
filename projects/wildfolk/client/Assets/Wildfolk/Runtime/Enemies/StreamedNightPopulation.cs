using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class ZombieSpawner
    {
        private readonly List<NightCreatureSnapshot> sleepingNight=new List<NightCreatureSnapshot>();
        private readonly Dictionary<EnemyController,string> caveFeatures=new Dictionary<EnemyController,string>();
        private readonly Dictionary<EnemyController,Action> caveDeaths=new Dictionary<EnemyController,Action>();
        private void ClearCavern(EnemyController enemy){if(caveDeaths.TryGetValue(enemy,out var action)){enemy.OwnHealth.Died-=action;caveDeaths.Remove(enemy);}caveFeatures.Remove(enemy);}
        public EnemyController SpawnCavern(Vector2 position,bool armed,string feature=null)
        {
            if(!SimulationAuthority.Allows(this) || ActiveCount>=Definition.HardCap)return null;
            var enemy=PrefabPool.Spawn(SecondaryPrefab??Prefab,position,Quaternion.identity);ConfigureCavern(enemy,feature);if(enemy.Variant!=null)enemy.Variant.SetEquipped(armed);Register(enemy);return enemy;
        }
        private void ConfigureCavern(EnemyController enemy,string feature)
        {ClearCavern(enemy);enemy.Session=Session;enemy.PlayerHealth=Player;enemy.Clock=Clock;enemy.Lifecycle.Clock=Clock;enemy.Lifecycle.Definition=Definition;enemy.Lifecycle.RestoreActive();enemy.Lifecycle.enabled=false;enemy.GetComponent<LootOnDeath>().Player=World.Player;if(feature!=null){caveFeatures[enemy]=feature;Action action=()=>{World.Chunks.MarkFeatureCleared(feature);ClearCavern(enemy);};caveDeaths[enemy]=action;enemy.OwnHealth.Died+=action;}}
        private NightCreatureSnapshot Snapshot(EnemyController e)=>new NightCreatureSnapshot{Species=e.Member.Definition.Id,Variant=e.Variant!=null && e.Variant.Equipped ? e.Variant.Definition.Id : null,Position=e.transform.position,HP=e.OwnHealth.Current,Feature=caveFeatures.TryGetValue(e,out var feature) ? feature : null};
        public NightCreatureSnapshot[] CaptureWorld()
        {var result=new List<NightCreatureSnapshot>(sleepingNight);foreach(var e in enemies)if(e!=null && e.gameObject.activeInHierarchy && e.OwnHealth.IsAlive && e.Lifecycle.Phase!=ZombiePhase.Burrowing && (Clock.Model.Phase==DayPhase.Night || World.Map.Streamed && World.Map.Continental.Underground(e.transform.position)))result.Add(Snapshot(e));return result.ToArray();}
        public void ReleaseWorld()
        {foreach(var e in enemies)if(e!=null){ClearCavern(e);PrefabPool.Release(e.gameObject);}enemies.Clear();sleepingNight.Clear();caveFeatures.Clear();}
        public void StreamReconcile(WorldChunkRuntime runtime)
        {
            for(var i=enemies.Count-1;i>=0;i--){var e=enemies[i];if(e==null || runtime.Wants(e.transform.position))continue;if(e.OwnHealth.IsAlive && e.Lifecycle.Phase!=ZombiePhase.Burrowing)sleepingNight.Add(Snapshot(e));ClearCavern(e);enemies.RemoveAt(i);PrefabPool.Release(e.gameObject);}
            for(var i=sleepingNight.Count-1;i>=0;i--)
            {var s=sleepingNight[i];if(!runtime.IsActive(s.Position) || ActiveCount>=Definition.HardCap)continue;var under=World.Map.Continental.Underground(s.Position);if(!under && Clock.Model.Phase!=DayPhase.Night){sleepingNight.RemoveAt(i);continue;}var e=PrefabPool.Spawn(SpeciesPrefab(s.Species),s.Position,Quaternion.identity);e.Session=Session;e.PlayerHealth=Player;e.Clock=Clock;e.Lifecycle.Clock=Clock;e.Lifecycle.Definition=Definition;e.Lifecycle.enabled=true;e.Lifecycle.RestoreActive();if(under)ConfigureCavern(e,s.Feature);e.OwnHealth.RestoreHealth(s.HP);if(e.Variant!=null)e.Variant.SetEquipped(!string.IsNullOrEmpty(s.Variant));Register(e);sleepingNight.RemoveAt(i);}
        }
    }
}
