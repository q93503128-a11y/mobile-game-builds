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
        public ContentCatalog Catalog;
        public const float RewardRadius=24;
        public WorldGenerator World;
        public WorldClock Clock;
        public AnimalPopulation Animals;
        public GoblinSpawner Goblins;
        public ZombieSpawner Night;
        public GameplaySession Session;
        public bool Ready {get;private set;}
        public bool IsAuthority=>NetworkManager.Singleton!=null && NetworkManager.Singleton.IsServer;
        public int DeathCount {get;private set;}
        public NetworkDeathResult LastDeath {get;private set;}
        public event Action<NetworkDeathResult> LootGenerated;
        private readonly List<NetworkObject> pending=new List<NetworkObject>();
        private void Awake()
        {Active=this;Animals.enabled=Goblins.enabled=Night.enabled=Clock.enabled=IsAuthority;World.Renewal.enabled=IsAuthority;}
        private void Start()
        {if(IsAuthority){GetComponent<NetworkWorldPersistence>().InitializeWorld();Ready=true;}}
        public void InitializeTerrain(int seed,int generationVersion=2){if(IsAuthority || Ready && World.Seed==seed && World.Map.GenerationVersion==generationVersion)return;World.Generate(seed,false,generationVersion);Ready=true;}
        public void CreateFacilities()
        {
            var offset=0;foreach(var entry in Prefabs)if(entry.Source.GetComponent<WorldPlaceable>() is WorldPlaceable placed && (placed.Definition.StorageSlots>0 || placed.Definition.Station!=StationKind.None) && placed.Definition.WorkbenchTier<=1 && placed.Definition.Station!=StationKind.BlastFurnace && placed.Definition.Station!=StationKind.CookingPot)
            {
                var center=World.Map.Spawn+new Vector2((offset-2)*3,4);offset++;
                var found=false;var position=center;
                for(var radius=0;radius<20 && !found;radius++)for(var x=-radius;x<=radius && !found;x++)for(var y=-radius;y<=radius;y++)
                {var candidate=center+new Vector2(x,y);if(!World.Map.IsWalkable(candidate,1.2f) || Physics2D.OverlapCircle(candidate,1.2f)!=null)continue;position=candidate;found=true;break;}
                if(found)Create(entry.Source.GetComponent<WorldPlaceable>(),position,Quaternion.identity);
            }
        }
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
        public void CancelPending(GameObject actor){var obj=actor.GetComponent<NetworkObject>();pending.Remove(obj);actor.SetActive(false);Destroy(actor);}
        public bool SpawnPendingNow(NetworkObject actor)
        {
            if(!IsAuthority || !NetworkManager.Singleton.IsListening || actor==null || actor.IsSpawned)return false;
            pending.Remove(actor);actor.CheckObjectVisibility=client=>WorldChunkInterest.Visible(this,client,actor.transform.position);actor.Spawn(true);return actor.IsSpawned;
        }
        public void FlushPending(){if(!IsAuthority)return;foreach(var actor in pending)if(actor!=null && !actor.IsSpawned){actor.CheckObjectVisibility=client=>WorldChunkInterest.Visible(this,client,actor.transform.position);actor.Spawn(true);}pending.Clear();}
        private void LateUpdate()
        {
            if(!IsAuthority)return;
            FlushPending();
            // Population uses one live anchor; combat selects among all connected living players.
            var player=SimulationAuthority.TargetForWorld();
            if(player==null)return;
            World.Player=player.GetComponent<PlayerInventory>();Goblins.Player=Night.Player=player;
            var nav=World.GetComponent<WorldNavigation>();nav.Player=player.transform;
        }
        private readonly string rewardRun=System.Guid.NewGuid().ToString("N");
        public void RecordDeath(NetworkDeathResult result)
        {
            if(!IsAuthority || !result.TryCommit())return;DeathCount++;LastDeath=result;
            var manager=NetworkManager.Singleton;if(!manager.SpawnManager.SpawnedObjects.TryGetValue(result.CreatureId,out var creature))return;
            foreach(var item in result.Items)NetworkGroundItem.Drop(item.Item,item.Count,creature.transform.position,null,false);
            if(manager.SpawnManager.SpawnedObjects.TryGetValue(result.AttackerId,out var attacker) && attacker.GetComponent<NetworkPlayer>()!=null)
                foreach(var player in NetworkPlayer.Active)if(player.Health.IsAlive && manager.ConnectedClients.ContainsKey(player.OwnerClientId) && Vector2.Distance(player.transform.position,creature.transform.position)<=RewardRadius){player.GetComponent<PlayerProgression>().GameplayReward(result.Experience,result.Gold,rewardRun+":"+DeathCount);EconomyTelemetry.Killed(player,creature.gameObject,rewardRun+":"+DeathCount);}
            LootGenerated?.Invoke(result);
        }
        private void OnDestroy()
        {foreach(var actor in pending)if(actor!=null && !actor.IsSpawned)Destroy(actor.gameObject);pending.Clear();if(Active==this)Active=null;}
    }
}
