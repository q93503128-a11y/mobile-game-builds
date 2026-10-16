using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public struct NetworkSpawnEntry {public GameObject Source,NetworkPrefab;}
    [DefaultExecutionOrder(-150)]
    public sealed class NetworkCreatureWorld : MonoBehaviour
    {
        public static NetworkCreatureWorld Active {get;private set;}
        public NetworkSpawnEntry[] Prefabs;
        public WorldGenerator World;
        public WorldClock Clock;
        public AnimalPopulation Animals;
        public GoblinSpawner Goblins;
        public ZombieSpawner Night;
        public GameplaySession Session;
        public bool IsAuthority=>NetworkManager.Singleton!=null && NetworkManager.Singleton.IsServer;
        public int DeathCount {get;private set;}
        public NetworkDeathResult LastDeath {get;private set;}
        public event Action<NetworkDeathResult> LootGenerated;
        private readonly List<NetworkObject> pending=new List<NetworkObject>();
        private void Awake()
        {Active=this;Animals.enabled=Goblins.enabled=Night.enabled=Clock.enabled=IsAuthority;World.Renewal.enabled=false;}
        private void Start()
        {World.Generate(World.Definition.DefaultSeed);}
        public T Create<T>(T source,Vector3 position,Quaternion rotation,Transform parent=null) where T:Component
        {
            if(!IsAuthority)throw new InvalidOperationException("Only the Host may spawn multiplayer actors.");
            foreach(var entry in Prefabs)if(entry.Source==source.gameObject)
            {
                // Spawn after existing initializers have assigned home/group/variant/target.
                var actor=Instantiate(entry.NetworkPrefab,position,rotation);pending.Add(actor.GetComponent<NetworkObject>());return actor.GetComponent<T>();
            }
            throw new InvalidOperationException("Unregistered multiplayer actor: "+source.name);
        }
        private void LateUpdate()
        {
            if(!IsAuthority)return;
            foreach(var actor in pending)if(actor!=null && !actor.IsSpawned)actor.Spawn(true);pending.Clear();
            // Population uses one live anchor; combat selects among all connected living players.
            var player=SimulationAuthority.TargetForWorld();
            if(player==null)return;
            World.Player=player.GetComponent<PlayerInventory>();Goblins.Player=Night.Player=player;
            var nav=World.GetComponent<WorldNavigation>();nav.Player=player.transform;
        }
        public void RecordDeath(NetworkDeathResult result)
        {if(!IsAuthority)return;DeathCount++;LastDeath=result;LootGenerated?.Invoke(result);}
        private void OnDestroy()
        {foreach(var actor in pending)if(actor!=null && !actor.IsSpawned)Destroy(actor.gameObject);pending.Clear();if(Active==this)Active=null;}
    }
}
