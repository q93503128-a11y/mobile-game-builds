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
            var game=new SavedGame{Seed=World.Seed,Elapsed=Clock.Model.Elapsed,Player=player,Resources=World.Renewal.Capture(),Recovery=World.Renewal.CaptureRecovery(),Animals=World.Animals.Capture()};
            game.AnimalEcology=World.Animals.CaptureEcology();game.GoblinEcology=World.GetComponent<GoblinSpawner>().Capture();
            var night=new List<NightCreatureSnapshot>();if(Clock.Model.Phase==DayPhase.Night)foreach(var enemy in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))if(enemy.OwnHealth.IsAlive && enemy.Lifecycle.Phase!=ZombiePhase.Burrowing)night.Add(new NightCreatureSnapshot{Species=enemy.Member.Definition.Id,Variant=enemy.Variant!=null && enemy.Variant.Equipped ? enemy.Variant.Definition.Id : null,Position=enemy.transform.position,HP=enemy.OwnHealth.Current});game.NightCreatures=night.ToArray();
            var placed=new List<PlaceableSnapshot>();
            foreach(var facility in Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None))
            {
                if(!facility.Health.IsAlive)continue;var chest=facility.GetComponent<StorageChest>();var process=facility.GetComponent<StationProcessing>();var door=facility.GetComponent<WorldDoor>();
                placed.Add(new PlaceableSnapshot{Id=facility.SaveId,Item=facility.Definition.Item.Id,Position=facility.transform.position,Rotation=facility.transform.eulerAngles.z,HP=facility.Health.Current,Inventory=chest==null ? null : chest.Model.Capture(),Processing=process==null ? null : process.Capture(),DoorOpen=door!=null && door.IsOpen,Creator=facility.CreatorId});
            }
            game.Placeables=placed.ToArray();var pickups=new List<PickupSnapshot>();
            foreach(var pickup in Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None))if(pickup.Item!=null && pickup.Count>0)pickups.Add(new PickupSnapshot{Item=pickup.Item.Id,Count=pickup.Count,Position=pickup.transform.position});
            game.Pickups=pickups.ToArray();return game;

        }
        public static void Apply(WorldGenerator World,WorldClock Clock,ContentCatalog Catalog,SavedGame game,PlayerInventory Player)
        {
            if(!SimulationAuthority.Allows(World))throw new System.InvalidOperationException("Only the world authority can restore snapshots.");
            // Remove previous network actors through their real lifecycle, including pending spawns.
            foreach(var resource in Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None))PrefabPool.Release(resource.gameObject);
            foreach(var enemy in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None))PrefabPool.Release(enemy.gameObject);
            foreach(var pickup in Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None))PrefabPool.Release(pickup.gameObject);
            foreach(var facility in Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None))PrefabPool.Release(facility.gameObject);
            World.Generate(game.Seed,false);Clock.Model.SetElapsed(game.Elapsed);
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
            foreach(var state in game.Pickups){var pickup=PrefabPool.Spawn(World.Pickup,state.Position,Quaternion.identity);pickup.Initialize(Catalog.Item(state.Item),state.Count,Player,World.Settings,true);}
            Physics2D.SyncTransforms();WallVisual.RefreshAll();
        }
    }
}
