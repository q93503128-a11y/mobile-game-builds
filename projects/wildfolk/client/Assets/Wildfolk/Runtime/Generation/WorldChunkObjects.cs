using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed partial class WorldChunkRuntime
    {
        private readonly Dictionary<string,PlaceableSnapshot> sleepingPlaces=new Dictionary<string,PlaceableSnapshot>();
        private readonly List<PickupSnapshot> sleepingPickups=new List<PickupSnapshot>();
        private readonly Dictionary<string,WorldFeatureSnapshot> features=new Dictionary<string,WorldFeatureSnapshot>();
        private readonly List<string> objectKeys=new List<string>();
        public bool TryHome(string id,out Vector2 position)
        {position=default;if(!sleepingPlaces.TryGetValue(id,out var state) || World.Definition.ProgressionCatalog.Item(state.Item).Placement.Station!=StationKind.Workbench || state.HP<=0)return false;position=state.Position;return true;}
        public PlaceableSnapshot[] DormantPlaces(){var result=new List<PlaceableSnapshot>();foreach(var s in sleepingPlaces.Values){AdvanceCargo(s,World.Renewal.Clock.Model.Elapsed);result.Add(s);}return result.ToArray();}
        public PickupSnapshot[] DormantPickups()=>sleepingPickups.ToArray();
        public WorldFeatureSnapshot[] CaptureFeatures()=>new List<WorldFeatureSnapshot>(features.Values).ToArray();
        public void RestoreDormant(SavedGame game)
        {
            sleepingPlaces.Clear();foreach(var s in game.Placeables)sleepingPlaces.Add(s.Id,s);sleepingPickups.Clear();sleepingPickups.AddRange(game.Pickups);features.Clear();foreach(var f in game.Features??Array.Empty<WorldFeatureSnapshot>())features.Add(f.Id,f);
        }
        public static PlaceableSnapshot CapturePlace(WorldPlaceable f,double now)
        {var chest=f.GetComponent<StorageChest>();var process=f.GetComponent<StationProcessing>();var door=f.GetComponent<WorldDoor>();return new PlaceableSnapshot{Id=f.SaveId,Item=f.Definition.Item.Id,Creator=f.CreatorId,Position=f.transform.position,Rotation=f.transform.eulerAngles.z,HP=f.Health.Current,Inventory=chest==null ? null : chest.Model.Capture(),Processing=process==null ? null : process.Capture(),DoorOpen=door!=null && door.IsOpen,SimulatedAt=now};}
        private void CullDistantObjects()
        {
            var now=World.Renewal.Clock.Model.Elapsed;for(var i=WorldPlaceable.Active.Count-1;i>=0;i--){var f=WorldPlaceable.Active[i];if(!f.Health.IsAlive || Wants(f.transform.position))continue;sleepingPlaces[f.SaveId]=CapturePlace(f,now);PrefabPool.Release(f.gameObject);}
            for(var i=WorldPickup.Active.Count-1;i>=0;i--){var p=WorldPickup.Active[i];if(p.Item==null || p.Count<=0 || Wants(p.transform.position))continue;sleepingPickups.Add(new PickupSnapshot{Item=p.Item.Id,Count=p.Count,Position=p.transform.position});PrefabPool.Release(p.gameObject);}
        }
        private void AdvanceCargo(PlaceableSnapshot s,double now)
        {
            if(s.Processing==null){s.SimulatedAt=now;return;}var elapsed=Mathf.Max(0,(float)(now-s.SimulatedAt));var definition=World.Definition.ProgressionCatalog.Item(s.Item).Placement;
            if(s.Processing.Sequential){var queue=new ProcessingQueue();queue.Restore(s.Processing,World.Definition.ProgressionCatalog,definition.ProcessingSpeed);queue.Advance(elapsed,definition.ProcessingSpeed,definition.OutputCapacity);s.Processing=queue.Capture();}
            else s.Processing.Remaining=Mathf.Max(0,s.Processing.Remaining-elapsed);s.SimulatedAt=now;
        }
        private void RestoreObjects(WorldChunk key)
        {
            objectKeys.Clear();foreach(var pair in sleepingPlaces)if(WorldChunk.At(pair.Value.Position,World.Definition.ChunkSize).Equals(key))objectKeys.Add(pair.Key);
            foreach(var id in objectKeys)
            {
                var s=sleepingPlaces[id];AdvanceCargo(s,World.Renewal.Clock.Model.Elapsed);var f=NetworkSpawnFactory.Spawn(World.Definition.ProgressionCatalog.Item(s.Item).Placement.Prefab,s.Position,Quaternion.Euler(0,0,s.Rotation));f.SaveId=s.Id;f.CreatorId=s.Creator;f.Health.RestoreHealth(s.HP);
                var chest=f.GetComponent<StorageChest>();if(chest!=null){chest.BindVisitor(World.Player);chest.Model.Restore(s.Inventory,World.Definition.ProgressionCatalog);}var process=f.GetComponent<StationProcessing>();if(process!=null)process.InitializeSnapshot(s.Processing,World.Definition.ProgressionCatalog,World.Player);var door=f.GetComponent<WorldDoor>();if(door!=null)door.RestoreOpen(s.DoorOpen);sleepingPlaces.Remove(id);
            }
            for(var i=sleepingPickups.Count-1;i>=0;i--){var s=sleepingPickups[i];if(!WorldChunk.At(s.Position,World.Definition.ChunkSize).Equals(key))continue;var pickup=PrefabPool.Spawn(World.Pickup,s.Position,Quaternion.identity);pickup.Initialize(World.Definition.ProgressionCatalog.Item(s.Item),s.Count,World.Player,World.Settings,true);sleepingPickups.RemoveAt(i);}
        }
        private void MaterializeFeatures(WorldChunk key,Transform parent)
        {
            foreach(var a in World.Map.Continental.Anchors)if(WorldChunk.At(a.Position,World.Definition.ChunkSize).Equals(key))
            {
                if(SimulationAuthority.Allows(World) && !features.ContainsKey(a.Id))features[a.Id]=new WorldFeatureSnapshot{Id=a.Id,Visited=true};
                if(a.Kind==WorldAnchorKind.GoblinCamp)continue;
                var go=Landmark(parent,a.Kind,a.Position);if(a.Kind==WorldAnchorKind.CaveEntrance){var portal=go.AddComponent<CavernPortal>();portal.World=World;portal.AnchorId=a.Id;var collider=go.AddComponent<CircleCollider2D>();collider.radius=1.4f;collider.isTrigger=true;}
            }
            foreach(var cave in World.Map.Continental.Caves)for(var i=0;i<cave.SurfaceEntrances.Count;i++)if(WorldChunk.At(cave.Entry(i),World.Definition.ChunkSize).Equals(key))
            {var go=Landmark(parent,WorldAnchorKind.CaveEntrance,cave.Entry(i));var portal=go.AddComponent<CavernPortal>();portal.World=World;portal.AnchorId="cave"+cave.Id+":"+i;portal.ReturnToSurface=true;go.AddComponent<CircleCollider2D>().isTrigger=true;}
        }
        private GameObject Landmark(Transform parent,WorldAnchorKind kind,Vector2 position)
        {var go=new GameObject(kind==WorldAnchorKind.CaveEntrance ? "Caverns" : "Weathered landmark");go.transform.SetParent(parent,false);go.transform.position=position;var sr=go.AddComponent<SpriteRenderer>();sr.sprite=World.PoiSprites[(int)kind];sr.sharedMaterial=World.SpriteMaterial;sr.sortingOrder=WorldSorting.GroundDecoration+1;return go;}
        public void MarkFeatureCleared(string id){if(features.TryGetValue(id,out var f))f.Cleared=true;}
        private void MaterializeCaveThreats(WorldChunk key)
        {
            foreach(var cave in World.Map.Continental.Caves)foreach(var room in cave.Rooms)
            {
                if(room.Depth<.6f || !WorldChunk.At(room.Center,World.Definition.ChunkSize).Equals(key))continue;var id="encounter"+cave.Id+":"+cave.Rooms.IndexOf(room);
                if(features.ContainsKey(id))continue;var enemy=World.GetComponent<ZombieSpawner>().SpawnCavern(room.Center+Vector2.up*3,room.Depth>=.8f,id);if(enemy!=null)features[id]=new WorldFeatureSnapshot{Id=id,Visited=true};
            }
        }
    }
}
