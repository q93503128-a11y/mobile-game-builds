using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public sealed class NetworkProjectile : NetworkBehaviour
    {
        public readonly NetworkVariable<short> Visual=new NetworkVariable<short>(-1);
        public readonly NetworkVariable<Vector2> Velocity=new NetworkVariable<Vector2>();
        public readonly NetworkVariable<double> Started=new NetworkVariable<double>();
        public readonly NetworkVariable<Vector2> Origin=new NetworkVariable<Vector2>();
        private StoneProjectile projectile;private bool heard;
        private void Awake(){projectile=GetComponent<StoneProjectile>();projectile.enabled=false;}
        public override void OnNetworkSpawn()
        {heard=false;projectile.enabled=IsServer;if(IsServer){Visual.Value=projectile.VisualIndex;Velocity.Value=projectile.Velocity;Origin.Value=transform.position;Started.Value=NetworkManager.ServerTime.Time;}}
        private void Update()
        {if(IsSpawned && !IsServer){if(!heard&&Velocity.Value.sqrMagnitude>.001f){heard=true;if(NetworkManager.ServerTime.Time-Started.Value<.7)AudioRuntime.Play(Visual.Value>=0&&NetworkCreatureWorld.Active.Catalog.Items[Visual.Value].Ammo==AmmoKind.Bolt?Sound.Crossbow:Sound.BowRelease,transform.position);}if(Visual.Value>=0){GetComponentInChildren<SpriteRenderer>().sprite=NetworkCreatureWorld.Active.Catalog.Items[Visual.Value].Icon;transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(Velocity.Value.y,Velocity.Value.x)*Mathf.Rad2Deg-90);}transform.position=Origin.Value+Velocity.Value*(float)System.Math.Max(0,NetworkManager.ServerTime.Time-Started.Value);}}
    }
}
