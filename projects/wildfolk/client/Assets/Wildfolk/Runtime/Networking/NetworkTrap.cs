using Unity.Netcode;
namespace Wildfolk
{
    public sealed class NetworkTrap:NetworkBehaviour
    {
        public readonly NetworkVariable<uint> Impact=new NetworkVariable<uint>();
        public void Trigger(){if(IsSpawned && IsServer)Impact.Value++;}
        public override void OnNetworkSpawn()=>Impact.OnValueChanged+=Changed;
        private void Changed(uint before,uint after){if(!IsServer){var v=GetComponent<WorldPlaceable>().Visual.transform;v.localScale*=1.12f;}}
        public override void OnNetworkDespawn()=>Impact.OnValueChanged-=Changed;
    }
}
