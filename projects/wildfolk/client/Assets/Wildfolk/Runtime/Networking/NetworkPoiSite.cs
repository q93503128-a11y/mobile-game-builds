using System;
using Unity.Collections;
using Unity.Netcode;
namespace Wildfolk
{
    public struct PoiNetworkState:INetworkSerializable,IEquatable<PoiNetworkState>
    {
        public FixedString64Bytes Id;public WorldAnchorKind Kind;public bool Opened,Secured,TombUnlocked;public uint Revision;public int Water;
        public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter{s.SerializeValue(ref Id);s.SerializeValue(ref Kind);s.SerializeValue(ref Opened);s.SerializeValue(ref Secured);s.SerializeValue(ref TombUnlocked);s.SerializeValue(ref Revision);s.SerializeValue(ref Water);}
        public bool Equals(PoiNetworkState b)=>Id.Equals(b.Id) && Kind==b.Kind && Opened==b.Opened && Secured==b.Secured && TombUnlocked==b.TombUnlocked && Revision==b.Revision && Water==b.Water;
    }
    public sealed class NetworkPoiSite:NetworkBehaviour
    {
        public readonly NetworkVariable<PoiNetworkState> State=new NetworkVariable<PoiNetworkState>();
        public NetworkList<NetworkStack> Loot;private PoiSite site;
        private void Awake(){site=GetComponent<PoiSite>();Loot=new NetworkList<NetworkStack>();}
        public override void OnNetworkSpawn(){State.OnValueChanged+=Changed;Loot.OnListChanged+=LootChanged;if(IsServer)Publish();else Mirror();}
        public void Publish()
        {
            if(!IsSpawned || !IsServer || site.State==null)return;
            var cat=site.Catalog;
            for(var i=0;i<6;i++){var s=site.State.Loot[i];var value=s.Count==0 ? new NetworkStack{Item=-1} : new NetworkStack{Item=(short)Array.IndexOf(cat.Items,cat.Item(s.Item)),Count=s.Count,Charges=s.Charges,Quality=s.Quality};if(i>=Loot.Count)Loot.Add(value);else if(!Loot[i].Equals(value))Loot[i]=value;}
            var state=new PoiNetworkState{Id=site.Id,Kind=site.Kind,Opened=site.State.Opened,Secured=site.Secured,TombUnlocked=site.World.Chunks.FeatureCleared(site.GuardId),Revision=site.Revision,Water=site.State.Water};if(!state.Equals(State.Value))State.Value=state;
        }
        private void Changed(PoiNetworkState a,PoiNetworkState b)=>Mirror();private void LootChanged(NetworkListEvent<NetworkStack> e)=>Mirror();
        private void Mirror()
        {
            if(IsServer)return;var cat=NetworkCreatureWorld.Active.Catalog;var state=State.Value;var loot=new InventoryModel(6).Capture();
            for(var i=0;i<Loot.Count && i<6;i++){var s=Loot[i];if(s.Count>0 && s.Item>=0 && s.Item<cat.Items.Length)loot[i]=new SavedStack{Item=cat.Items[s.Item].Id,Count=s.Count,Charges=s.Charges,Quality=s.Quality};}
            site.ReplicaTombUnlocked=state.TombUnlocked;site.ApplyReplica(state.Id.ToString(),state.Kind,new PoiSiteSnapshot{Schema=1,Loot=loot,Opened=state.Opened,Revision=state.Revision,Water=state.Water},state.Secured,cat);
        }
        public override void OnNetworkDespawn(){State.OnValueChanged-=Changed;Loot.OnListChanged-=LootChanged;}
    }
}
