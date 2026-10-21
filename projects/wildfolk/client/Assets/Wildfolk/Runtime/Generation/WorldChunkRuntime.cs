using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    // One scheduler per world. No per-chunk Update, and no world-wide object search.
    public sealed partial class WorldChunkRuntime : MonoBehaviour
    {
        public WorldGenerator World {get;private set;}
        private readonly Dictionary<WorldChunk,GameObject> active=new Dictionary<WorldChunk,GameObject>();
        private readonly HashSet<WorldChunk> wanted=new HashSet<WorldChunk>();
        private readonly Queue<WorldChunk> pending=new Queue<WorldChunk>();
        private readonly List<WorldChunk> retiring=new List<WorldChunk>();
        private readonly List<Vector2> centers=new List<Vector2>(4);
        private float nextTick;
        private readonly HashSet<WorldChunk> queued=new HashSet<WorldChunk>();
        public int ActiveCount=>active.Count;
        public int PendingCount=>pending.Count;
        public int WantedCount=>wanted.Count;
        public IReadOnlyDictionary<WorldChunk,GameObject> ActiveChunks=>active;
        public bool IsActive(Vector2 p)=>active.ContainsKey(WorldChunk.At(p,World.Definition.ChunkSize));
        public bool Wants(Vector2 p)=>wanted.Contains(WorldChunk.At(p,World.Definition.ChunkSize));
        public void Initialize(WorldGenerator world)
        {
            ClearPoiSites();ClearPresentation();World=world;foreach(var root in active.Values)if(root!=null)Destroy(root);active.Clear();wanted.Clear();pending.Clear();sleepingPlaces.Clear();sleepingPickups.Clear();features.Clear();nextTick=0;enabled=true;
        }
        private void Update()
        {
            if(World==null || World.Map==null || !World.Map.Streamed)return;
            if(Time.unscaledTime>=nextTick){nextTick=Time.unscaledTime+.25f;Reconcile();}

            if(pending.Count>0){var key=pending.Dequeue();if(wanted.Contains(key) && !active.ContainsKey(key))Activate(key);}
            ReconcilePresentation();World.Terrain.ProcessTerrainWork(2.5);
        }
        public void Reconcile()
        {
            centers.Clear();var authority=SimulationAuthority.Allows(World);
            if(NetworkCreatureWorld.Active!=null){foreach(var p in NetworkPlayer.Active)if(p.IsSpawned && p.Health.IsAlive && (authority || p.IsOwner && p.NetworkObject.IsPlayerObject))centers.Add(p.transform.position);}
            if(centers.Count==0 && World.Player!=null)centers.Add(World.Player.transform.position);
            ChunkActivation.Build(World.Map,centers,active.Keys,wanted);
            retiring.Clear();foreach(var pair in active)if(!wanted.Contains(pair.Key))retiring.Add(pair.Key);foreach(var key in retiring)Deactivate(key);
            pending.Clear();queued.Clear();foreach(var center in centers){var key=WorldChunk.At(center,World.Definition.ChunkSize);if(wanted.Contains(key) && !active.ContainsKey(key) && queued.Add(key))pending.Enqueue(key);}foreach(var key in wanted)if(!active.ContainsKey(key) && queued.Add(key))pending.Enqueue(key);
            if(authority){CullDistantObjects();ReconcilePoi();World.Animals.StreamReconcile(this);World.GetComponent<GoblinSpawner>().StreamReconcile(this);World.GetComponent<ZombieSpawner>().StreamReconcile(this);if(NetworkCreatureWorld.Active!=null)WorldChunkInterest.Refresh(NetworkCreatureWorld.Active);}
        }
        public void DrainPending(){var count=pending.Count;for(var i=0;i<count;i++){var key=pending.Dequeue();if(wanted.Contains(key) && !active.ContainsKey(key))Activate(key);}World.Terrain.ProcessTerrainWork(double.PositiveInfinity);}
        private void Activate(WorldChunk key)
        {
            RemovePresentation(key,true);var root=new GameObject("Landscape");root.transform.SetParent(World.Environment,false);active.Add(key,root);World.Terrain.BuildChunk(World.Map,key,root.transform);
            MaterializeFeatures(key,root.transform);MaterializeDeepSites(key,root.transform);MaterializeWater(key,root.transform);
            if(SimulationAuthority.Allows(World))
            {
                var resources=World.Map.Continental.GenerateChunk(key);World.Renewal.PrepareChunk(key,resources);foreach(var p in resources)World.Renewal.Materialize(p,root.transform);
                World.Renewal.MaterializeRecovered(key,root.transform);
                RestoreObjects(key);MaterializePoi(key,root.transform);World.Animals.ActivateChunk(key);World.GetComponent<GoblinSpawner>().ActivateChunk(key);
                MaterializeCaveThreats(key);ReconcilePoi();
            }
            Decorations(key,root.transform);
        }
        private void Deactivate(WorldChunk key)
        {
            if(SimulationAuthority.Allows(World)){UnloadPoiSites(key);World.Renewal.UnloadChunk(key);World.Animals.UnloadChunk(key);World.GetComponent<GoblinSpawner>().UnloadChunk(key);}
            World.Terrain.ReleaseChunk(key);var root=active[key];active.Remove(key);if(root!=null){root.SetActive(false);Destroy(root);}
        }
        private void Decorations(WorldChunk key,Transform parent)
        {
            if(World.Map.Continental.Underground(key.Center(World.Definition.ChunkSize)))return;
            var random=new System.Random(WorldSeeds.Derive(World.Seed,SeedDomain.Vegetation,key.X,key.Y));for(var i=0;i<32;i++)
            {var p=key.Origin(World.Definition.ChunkSize)+new Vector2((float)random.NextDouble(),(float)random.NextDouble())*World.Definition.ChunkSize;if(!World.Map.IsWalkable(p,0) || World.Map.IsWater(p,1))continue;var go=new GameObject("Ground foliage");go.transform.SetParent(parent,false);go.transform.position=p;go.transform.localScale=Vector3.one*Mathf.Lerp(.35f,.7f,(float)random.NextDouble());var sr=go.AddComponent<SpriteRenderer>();sr.sprite=i%3==0 ? World.Fern : World.Grass;sr.sharedMaterial=World.SpriteMaterial;sr.sortingOrder=WorldSorting.GroundDecoration;}
        }
        private void OnDestroy(){ClearPoiSites();ClearPresentation();foreach(var root in active.Values)if(root!=null)Destroy(root);active.Clear();}
    }
}
