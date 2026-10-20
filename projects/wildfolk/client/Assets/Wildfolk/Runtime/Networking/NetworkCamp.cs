using Unity.Collections;
using Unity.Netcode;
namespace Wildfolk
{
    public sealed class NetworkCamp : NetworkBehaviour
    {
        public readonly NetworkVariable<FixedString64Bytes> Id=new NetworkVariable<FixedString64Bytes>();
        public readonly NetworkVariable<double> RefillAt=new NetworkVariable<double>();
        public readonly NetworkVariable<bool> Secured=new NetworkVariable<bool>(),Opened=new NetworkVariable<bool>();
        public readonly NetworkVariable<uint> LootRevision=new NetworkVariable<uint>();
        public NetworkList<NetworkStack> Loot;
        private GoblinCamp camp;
        private bool lootPublished;private uint publishedRevision;
        private void Awake(){camp=GetComponent<GoblinCamp>();Loot=new NetworkList<NetworkStack>();}
        public override void OnNetworkSpawn()
        {
            if(IsServer){Publish();PublishLoot();NetworkManager.NetworkTickSystem.Tick+=Publish;}
            else camp.State=new CampSnapshot{Id=Id.Value.ToString(),Position=transform.position,RefillAt=RefillAt.Value};
            RefillAt.OnValueChanged+=RefillChanged;
            Loot.OnListChanged+=LootChanged;LootRevision.OnValueChanged+=RevisionChanged;Opened.OnValueChanged+=OpenedChanged;Secured.OnValueChanged+=SecurityChanged;
            if(!IsServer)MirrorLoot();
        }
        private void Publish()
        {if(!IsSpawned || !IsServer || camp.State==null)return;var id=new FixedString64Bytes(camp.State.Id);if(!Id.Value.Equals(id))Id.Value=id;if(RefillAt.Value!=camp.State.RefillAt)RefillAt.Value=camp.State.RefillAt;}
        private void RefillChanged(double old,double value){if(!IsServer && camp.State!=null)camp.State.RefillAt=value;}
        public void PublishLoot()
        {
            if(!IsServer || !IsSpawned || camp.State==null)return;Secured.Value=camp.Secured;
            if(lootPublished && publishedRevision==camp.State.LootRevision)return;lootPublished=true;publishedRevision=camp.State.LootRevision;
            var catalog=NetworkCreatureWorld.Active.Catalog;
            for(var i=0;i<6;i++){var s=i<camp.State.Loot.Length ? camp.State.Loot[i] : null;var value=s==null || s.Count==0 ? new NetworkStack{Item=-1} : new NetworkStack{Item=(short)System.Array.IndexOf(catalog.Items,catalog.Item(s.Item)),Count=s.Count};if(i>=Loot.Count)Loot.Add(value);else if(!Loot[i].Equals(value))Loot[i]=value;}
            Opened.Value=camp.State.LootOpened;LootRevision.Value=camp.State.LootRevision;
        }
        private void MirrorLoot(){if(IsServer)return;var slots=new InventoryModel(6).Capture();var catalog=NetworkCreatureWorld.Active.Catalog;for(var i=0;i<Loot.Count && i<6;i++)if(Loot[i].Count>0)slots[i]=new SavedStack{Item=catalog.Items[Loot[i].Item].Id,Count=Loot[i].Count};camp.ApplyReplica(catalog,slots,Opened.Value,LootRevision.Value);}
        private void LootChanged(NetworkListEvent<NetworkStack> e)=>MirrorLoot();
        private void RevisionChanged(uint old,uint value)=>MirrorLoot();
        private void OpenedChanged(bool old,bool value)=>MirrorLoot();
        private void SecurityChanged(bool old,bool value)=>camp.Notify();
        public override void OnNetworkDespawn()
        {if(NetworkManager!=null && NetworkManager.NetworkTickSystem!=null)NetworkManager.NetworkTickSystem.Tick-=Publish;RefillAt.OnValueChanged-=RefillChanged;Loot.OnListChanged-=LootChanged;LootRevision.OnValueChanged-=RevisionChanged;Opened.OnValueChanged-=OpenedChanged;Secured.OnValueChanged-=SecurityChanged;}
    }
}
