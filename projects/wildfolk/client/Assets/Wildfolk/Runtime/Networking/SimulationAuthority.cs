using UnityEngine;
namespace Wildfolk
{
    // Absence of a network actor preserves the existing singleplayer simulation.
    public static class SimulationAuthority
    {
        public static bool Allows(Component actor)
        {
            var boss=actor.GetComponent<NetworkBoss>();if(boss!=null)return boss.IsSpawned && boss.IsServer;
            var resource=actor.GetComponent<NetworkResource>();if(resource!=null)return resource.IsSpawned && resource.IsServer;
            var building=actor.GetComponent<NetworkBuilding>();if(building!=null)return building.IsSpawned && building.IsServer;
            var facility=actor.GetComponent<NetworkFacility>();
            if(facility!=null)return facility.IsSpawned && facility.IsServer;
            var creature=actor.GetComponent<NetworkCreature>();
            if(creature!=null)return creature.IsSpawned && creature.IsServer;
            var player=actor.GetComponent<NetworkPlayer>();
            if(player!=null)return player.IsSpawned && player.IsServer;
            var world=actor.GetComponent<NetworkCreatureWorld>();
            return world==null || world.IsAuthority;
        }
        public static Health TargetForWorld()
        {
            var manager=Unity.Netcode.NetworkManager.Singleton;
            if(manager==null || !manager.IsServer)return null;
            foreach(var player in NetworkPlayer.Active)if(player.Health.IsAlive)return player.Health;
            return null;
        }
        public static Health Target(Component actor,Health fallback)
        {
            if(actor.GetComponent<NetworkCreature>()==null)return fallback;
            var manager=Unity.Netcode.NetworkManager.Singleton;
            if(manager==null || !manager.IsServer)return fallback;
            Health nearest=null;var distance=float.MaxValue;
            foreach(var player in NetworkPlayer.Active)
            {
                var health=player.Health;if(!health.IsAlive)continue;
                var d=((Vector2)(player.transform.position-actor.transform.position)).sqrMagnitude;
                if(d>=distance)continue;nearest=health;distance=d;
            }
            return nearest;
        }
    }
}
