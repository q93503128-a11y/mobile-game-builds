using Unity.Netcode;
namespace Wildfolk
{
    // Scene NetworkObject: terrain seed arrives with NGO scene synchronization.
    public sealed class NetworkWorldState : NetworkBehaviour
    {
        public readonly NetworkVariable<int> Seed=new NetworkVariable<int>();
        public readonly NetworkVariable<int> BaseGenerationVersion=new NetworkVariable<int>(1);
        public override void OnNetworkSpawn()
        {
            Seed.OnValueChanged+=Changed;
            if(IsServer){BaseGenerationVersion.Value=NetworkSession.Instance.PreparedWorld?.BaseGenerationVersion??NetworkSession.Instance.NewWorldGenerationVersion;Seed.Value=NetworkSession.Instance.PreparedWorld?.Seed??GetComponent<NetworkCreatureWorld>().World.Definition.DefaultSeed;}
            else GetComponent<NetworkCreatureWorld>().InitializeTerrain(Seed.Value,BaseGenerationVersion.Value);
        }
        private void Changed(int old,int value){if(!IsServer)GetComponent<NetworkCreatureWorld>().InitializeTerrain(value,BaseGenerationVersion.Value);}
        public override void OnNetworkDespawn()=>Seed.OnValueChanged-=Changed;
    }
}
