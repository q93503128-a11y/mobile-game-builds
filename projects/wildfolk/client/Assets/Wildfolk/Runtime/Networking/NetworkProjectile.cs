using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public sealed class NetworkProjectile : NetworkBehaviour
    {
        public readonly NetworkVariable<Vector2> Velocity=new NetworkVariable<Vector2>();
        public readonly NetworkVariable<double> Started=new NetworkVariable<double>();
        public readonly NetworkVariable<Vector2> Origin=new NetworkVariable<Vector2>();
        private StoneProjectile projectile;
        private void Awake(){projectile=GetComponent<StoneProjectile>();projectile.enabled=false;}
        public override void OnNetworkSpawn()
        {projectile.enabled=IsServer;if(IsServer){Velocity.Value=projectile.Velocity;Origin.Value=transform.position;Started.Value=NetworkManager.ServerTime.Time;}}
        private void Update()
        {if(IsSpawned && !IsServer)transform.position=Origin.Value+Velocity.Value*(float)System.Math.Max(0,NetworkManager.ServerTime.Time-Started.Value);}
    }
}
