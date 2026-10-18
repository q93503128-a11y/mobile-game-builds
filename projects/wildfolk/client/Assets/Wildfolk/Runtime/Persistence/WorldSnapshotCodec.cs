using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public static class WorldSnapshotCodec
    {
        public static SavedGame Capture(WorldGenerator World,WorldClock Clock,PlayerInventory Player)
            =>Capture(World,Clock,PlayerSnapshotCodec.Capture(Player));
        public static SavedGame Capture(WorldGenerator World,WorldClock Clock,PlayerSnapshot player)
        {
            var game=new SavedGame{Seed=World.Seed,BaseGenerationVersion=World.Map.GenerationVersion,Features=World.Chunks!=null && World.Map.Streamed ? World.Chunks.CaptureFeatures() : System.Array.Empty<WorldFeatureSnapshot>(),Elapsed=Clock.Model.Elapsed,Player=player,Resources=World.Renewal.Capture(),Recovery=World.Renewal.CaptureRecovery(),Animals=World.Animals.Capture()};
            game.AnimalEcology=World.Animals.CaptureEcology();game.GoblinEcology=World.GetComponent<GoblinSpawner>().Capture();
            game.NightCreatures=World.GetComponent<ZombieSpawner>().CaptureWorld();
            var placed=new List<PlaceableSnapshot>();
            foreach(var facility in WorldPlaceable.Active)
            {
                if(!facility.Health.IsAlive)continue;var chest=facility.GetComponent<StorageChest>();var process=facility.GetComponent<StationProcessing>();var door=facility.GetComponent<WorldDoor>();
                placed.Add(new PlaceableSnapshot{Id=facility.SaveId,Item=facility.Definition.Item.Id,Position=facility.transform.position,Rotation=facility.transform.eulerAngles.z,HP=facility.Health.Current,Inventory=chest==null ? null : chest.Model.Capture(),Processing=process==null ? null : process.Capture(),DoorOpen=door!=null && door.IsOpen,Creator=facility.CreatorId,SimulatedAt=Clock.Model.Elapsed});
            }
            if(World.Map.Streamed)placed.AddRange(World.Chunks.DormantPlaces());game.Placeables=placed.ToArray();var pickups=new List<PickupSnapshot>();
            foreach(var pickup in WorldPickup.Active)if(pickup.Item!=null && pickup.Count>0)pickups.Add(new PickupSnapshot{Item=pickup.Item.Id,Count=pickup.Count,Charges=pickup.Charges,Quality=pickup.Quality,Position=pickup.transform.position});
            if(World.Map.Streamed)pickups.AddRange(World.Chunks.DormantPickups());game.Pickups=pickups.ToArray();return game;

        }
        public static void Apply(WorldGenerator World,WorldClock Clock,ContentCatalog Catalog,SavedGame game,PlayerInventory Player)
        {
            if(!SimulationAuthority.Allows(World))throw new System.InvalidOperationException("Only the world authority can restore snapshots.");
            // Remove previous network actors through their real lifecycle, including pending spawns.
            World.Renewal.ReleaseWorld();
            World.GetComponent<ZombieSpawner>().ReleaseWorld();
            for(var i=WorldPickup.Active.Count-1;i>=0;i--)PrefabPool.Release(WorldPickup.Active[i].gameObject);
            for(var i=WorldPlaceable.Active.Count-1;i>=0;i--)PrefabPool.Release(WorldPlaceable.Active[i].gameObject);
            World.Generate(game.Seed,false,game.BaseGenerationVersion);Clock.Model.SetElapsed(game.Elapsed);
            if(World.Map.Streamed){World.Renewal.Restore(game.Resources,game.Recovery);World.Chunks.RestoreDormant(game);World.Animals.Restore(game.Animals,game.AnimalEcology);World.GetComponent<GoblinSpawner>().Restore(game.GoblinEcology);World.GetComponent<ZombieSpawner>().Restore(game.NightCreatures);World.Chunks.Reconcile();return;}
            for(var i=0;i<World.Map.Resources.Count;i++){var p=World.Map.Resources[i];World.CreateResource("r"+i,p.Rule,p.Position,p.Scale);}
            World.Renewal.Restore(game.Resources,game.Recovery);
            foreach(var state in game.Placeables)
            {
                var facility=NetworkSpawnFactory.Spawn(Catalog.Item(state.Item).Placement.Prefab,state.Position,Quaternion.Euler(0,0,state.Rotation));facility.SaveId=state.Id;facility.CreatorId=state.Creator;facility.Health.RestoreHealth(state.HP);
                var chest=facility.GetComponent<StorageChest>();if(chest!=null){chest.BindVisitor(Player);chest.Model.Restore(state.Inventory,Catalog);}
                var process=facility.GetComponent<StationProcessing>();if(process!=null)process.InitializeSnapshot(state.Processing,Catalog,Player);
                var door=facility.GetComponent<WorldDoor>();if(door!=null)door.RestoreOpen(state.DoorOpen);
            }
            World.Animals.Restore(game.Animals,game.AnimalEcology);World.GetComponent<GoblinSpawner>().Restore(game.GoblinEcology);World.GetComponent<ZombieSpawner>().Restore(game.NightCreatures);
            foreach(var state in game.Pickups){var pickup=PrefabPool.Spawn(World.Pickup,state.Position,Quaternion.identity);pickup.Initialize(Catalog.Item(state.Item),state.Count,Player,World.Settings,true,state.Charges,state.Quality);}
            Physics2D.SyncTransforms();WallVisual.RefreshAll();
        }
    }
}
