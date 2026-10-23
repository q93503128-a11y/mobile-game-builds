using System;
using Unity.Collections;
using Unity.Netcode;
namespace Wildfolk
{
    public struct BossOfferingState:INetworkSerializable,IEquatable<BossOfferingState>
    {
        public FixedString64Bytes Boss;public long Night;public bool Active;
        public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter{s.SerializeValue(ref Boss);s.SerializeValue(ref Night);s.SerializeValue(ref Active);}
        public bool Equals(BossOfferingState b)=>Boss.Equals(b.Boss) && Night==b.Night && Active==b.Active;
    }
    public sealed class NetworkBossWorld:NetworkBehaviour
    {
        public NetworkList<BossOfferingState> Offerings;private BossWorld world;
        private void Awake(){world=GetComponent<BossWorld>();Offerings=new NetworkList<BossOfferingState>();}
        public override void OnNetworkSpawn(){Offerings.OnListChanged+=Changed;if(IsServer)Publish();else Mirror();}
        public void Publish()
        {if(!IsSpawned || !IsServer)return;for(var i=0;i<world.Definitions.Length;i++){var r=world.Record(world.Definitions[i].Id);var s=new BossOfferingState{Boss=r.Boss,Night=r.LastNight,Active=r.Active};if(i>=Offerings.Count)Offerings.Add(s);else if(!Offerings[i].Equals(s))Offerings[i]=s;}}
        private void Changed(NetworkListEvent<BossOfferingState> e){if(!IsServer)Mirror();}
        private void Mirror(){foreach(var s in Offerings){var r=world.Record(s.Boss.ToString());r.LastNight=s.Night;r.Active=s.Active;}}
        public override void OnNetworkDespawn(){Offerings.OnListChanged-=Changed;}
    }
}
