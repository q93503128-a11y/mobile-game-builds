using Unity.Collections;
using Unity.Netcode;
namespace Wildfolk
{
    public sealed class NetworkCamp : NetworkBehaviour
    {
        public readonly NetworkVariable<FixedString64Bytes> Id=new NetworkVariable<FixedString64Bytes>();
        public readonly NetworkVariable<double> RefillAt=new NetworkVariable<double>();
        private GoblinCamp camp;
        private void Awake()=>camp=GetComponent<GoblinCamp>();
        public override void OnNetworkSpawn()
        {
            if(IsServer){Publish();NetworkManager.NetworkTickSystem.Tick+=Publish;}
            else camp.State=new CampSnapshot{Id=Id.Value.ToString(),Position=transform.position,RefillAt=RefillAt.Value};
            RefillAt.OnValueChanged+=RefillChanged;
        }
        private void Publish()
        {if(!IsSpawned || !IsServer || camp.State==null)return;var id=new FixedString64Bytes(camp.State.Id);if(!Id.Value.Equals(id))Id.Value=id;if(RefillAt.Value!=camp.State.RefillAt)RefillAt.Value=camp.State.RefillAt;}
        private void RefillChanged(double old,double value){if(!IsServer && camp.State!=null)camp.State.RefillAt=value;}
        public override void OnNetworkDespawn()
        {if(NetworkManager!=null && NetworkManager.NetworkTickSystem!=null)NetworkManager.NetworkTickSystem.Tick-=Publish;RefillAt.OnValueChanged-=RefillChanged;}
    }
}
