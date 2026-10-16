using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public sealed class NetworkInventory : NetworkBehaviour,IInventoryAuthority
    {
        public NetworkList<NetworkStack> Slots;
        public readonly NetworkVariable<uint> Revision=new NetworkVariable<uint>(0,NetworkVariableReadPermission.Owner);
        public readonly NetworkVariable<int> Selected=new NetworkVariable<int>(0,NetworkVariableReadPermission.Owner);
        public readonly NetworkVariable<SessionRewardState> Rewards=new NetworkVariable<SessionRewardState>(default,NetworkVariableReadPermission.Owner);
        public NetworkPlayer Player {get;private set;}
        public PlayerInventory Inventory {get;private set;}
        public bool IsApplying {get;private set;}
        public bool CanOperate=>IsSpawned && IsServer && Player.Health.IsAlive && NetworkManager.IsListening && NetworkManager.ConnectedClients.ContainsKey(OwnerClientId);
        public bool Pending=>pending!=0;
        private ulong sequence,pending,lastRequest;
        private uint waitingRevision;
        private PlayerProgression progression;
        private readonly List<NetworkGroundItem> nearby=new List<NetworkGroundItem>();
        private readonly Dictionary<ulong,uint> attempts=new Dictionary<ulong,uint>();
        private void Awake(){Slots=new NetworkList<NetworkStack>(null,NetworkVariableReadPermission.Owner,NetworkVariableWritePermission.Server);Player=GetComponent<NetworkPlayer>();Inventory=GetComponent<PlayerInventory>();progression=GetComponent<PlayerProgression>();}
        public override void OnNetworkSpawn()
        {
            Inventory.Authority=this;Inventory.Model.IsReadOnly=!IsServer;
            GetComponent<PlayerItemDrop>().BindWorld(NetworkCreatureWorld.Active.World);
            Slots.OnListChanged+=ListChanged;Selected.OnValueChanged+=SelectionChanged;Revision.OnValueChanged+=RevisionChanged;Rewards.OnValueChanged+=RewardsChanged;
            Player.Commands.Session.StateChanged+=ViewChanged;
            if(IsServer){Inventory.Model.Changed+=Publish;Inventory.SelectionChanged+=PublishSelection;progression.Changed+=PublishRewards;Publish();PublishSelection();PublishRewards();}
            else if(IsOwner){MirrorAll();Inventory.ApplySelection(Selected.Value);ApplyRewards(Rewards.Value);}
        }
        private short Index(ItemDefinition item)=>item==null ? (short)-1 : (short)System.Array.IndexOf(Player.Catalog.Items,item);
        private ItemDefinition Item(short index)=>index<0 || index>=Player.Catalog.Items.Length ? null : Player.Catalog.Items[index];
        private void Publish()
        {
            if(!IsSpawned || !IsServer)return;
            for(var i=0;i<Inventory.Model.Capacity;i++)
            {var stack=Inventory.Model.GetSlot(i);var value=new NetworkStack{Item=Index(stack.IsEmpty ? null : stack.Item),Count=stack.Count};if(i>=Slots.Count)Slots.Add(value);else if(!Slots[i].Equals(value))Slots[i]=value;}
            Revision.Value++;
        }
        private void PublishSelection(){if(!IsServer || !IsSpawned)return;Selected.Value=Inventory.SelectedIndex;Revision.Value++;}
        private void PublishRewards(){if(IsServer && IsSpawned)Rewards.Value=new SessionRewardState{Experience=progression.Experience,Gold=progression.Gold};}
        private void MirrorAll(){var values=new ItemStack[Inventory.Model.Capacity];for(var i=0;i<Slots.Count && i<values.Length;i++){var stack=Slots[i];values[i]=stack.Count==0 ? default : new ItemStack(Item(stack.Item),stack.Count);}Inventory.Model.ApplyReplicaSlots(values);}
        private void ListChanged(NetworkListEvent<NetworkStack> change)
        {if(IsServer || !IsOwner)return;if(change.Type==NetworkListEvent<NetworkStack>.EventType.Value && change.Index>=0 && change.Index<Inventory.Model.Capacity){var value=change.Value;Inventory.Model.ApplyReplicaSlot(change.Index,value.Count==0 ? default : new ItemStack(Item(value.Item),value.Count));}else MirrorAll();}
        private void SelectionChanged(int old,int value){if(!IsServer && IsOwner)Inventory.ApplySelection(value);}
        private void RewardsChanged(SessionRewardState old,SessionRewardState value){if(!IsServer && IsOwner)ApplyRewards(value);}
        private void ApplyRewards(SessionRewardState value)=>progression.SetValues(value.Experience,value.Gold);
        private void RevisionChanged(uint old,uint value){if(pending!=0 && waitingRevision!=0 && value>=waitingRevision){pending=0;waitingRevision=0;}}
        private void ViewChanged(){if(IsOwner && !IsServer && !GetComponent<NetworkFacilityInteractor>().IsApplying)ViewRpc(Player.Commands.Session.State==SessionState.Inventory);}
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void ViewRpc(bool open,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId!=OwnerClientId || !CanOperate)return;if(open)Player.Commands.Session.OpenInventory();else Player.Commands.Session.CloseInventory();}
        public bool Select(int slot)=>Submit(InventoryOperation.Select,slot,0,0,0);
        public bool Transfer(int from,int to,int amount)=>Submit(InventoryOperation.Transfer,from,to,amount,0);
        public bool Drop(int slot,int amount)=>Submit(InventoryOperation.Drop,slot,0,amount,0);
        public bool Equip(int slot)=>Submit(InventoryOperation.Equip,slot,0,0,0);
        public bool Unequip(ArmorSlot slot,int destination)=>Submit(InventoryOperation.Unequip,(int)slot,destination,0,0);
        public bool Pickup(ulong objectId)=>Submit(InventoryOperation.Pickup,0,0,0,objectId);
        private bool Submit(InventoryOperation operation,int from,int to,int amount,ulong target)
        {
            if(!IsSpawned || !IsOwner || !Player.Health.IsAlive || Pending)return false;
            if(IsServer)return Execute(operation,from,to,amount,target,Revision.Value);
            pending=++sequence;waitingRevision=0;OperationRpc(pending,Revision.Value,operation,from,to,amount,target);return true;
        }
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void OperationRpc(ulong request,uint revision,InventoryOperation operation,int from,int to,int amount,ulong target,RpcParams rpc=default)
        {
            if(rpc.Receive.SenderClientId!=OwnerClientId || request<=lastRequest)return;lastRequest=request;
            Execute(operation,from,to,amount,target,revision);FinishRpc(request,Revision.Value);
        }
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]
        private void FinishRpc(ulong request,uint revision)
        {if(pending!=request)return;waitingRevision=revision;if(Revision.Value>=revision){pending=0;waitingRevision=0;}}
        public bool Execute(InventoryOperation operation,int from,int to,int amount,ulong target,uint expectedRevision)
        {
            if(!CanOperate || expectedRevision!=Revision.Value || amount<0)return false;
            var capacity=Inventory.Model.Capacity;
            if(operation!=InventoryOperation.Pickup && operation!=InventoryOperation.Unequip && (from<0 || from>=capacity))return false;
            if(operation!=InventoryOperation.Select && operation!=InventoryOperation.Pickup && Player.Commands.Session.State!=SessionState.Inventory)return false;
            IsApplying=true;
            try
            {
                switch(operation)
                {
                    case InventoryOperation.Select:Inventory.ApplySelection(from);return true;
                    case InventoryOperation.Transfer:return to>=0 && to<capacity && Inventory.Model.MoveStackAmount(from,to,amount);
                    case InventoryOperation.Equip:return Player.Equipment.TryEquipFromSlot(from);
                    case InventoryOperation.Unequip:return from>=0 && from<=1 && to>=-1 && to<capacity && Player.Equipment.Unequip((ArmorSlot)from,to);
                    case InventoryOperation.Drop:return GetComponent<PlayerItemDrop>().Drop(Inventory.Model,from,amount);
                    case InventoryOperation.Pickup:return NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(target,out var obj) && obj.TryGetComponent<NetworkGroundItem>(out var item) && item.TryPickup(this);
                    default:return false;
                }
            }
            finally{IsApplying=false;}
        }
        public void Track(NetworkGroundItem item){if(!nearby.Contains(item))nearby.Add(item);}
        public void Untrack(NetworkGroundItem item){nearby.Remove(item);if(item!=null)attempts.Remove(item.NetworkObjectId);}
        public void InvalidatePickup(NetworkGroundItem item){if(item!=null)attempts.Remove(item.NetworkObjectId);}
        private void FixedUpdate()
        {
            if(!IsSpawned || !IsOwner || Pending || !Player.Health.IsAlive)return;
            for(var i=nearby.Count-1;i>=0;i--)
            {
                if(i>=nearby.Count)continue;var item=nearby[i];if(item==null || !item.IsSpawned){nearby.RemoveAt(i);continue;}
                var id=item.NetworkObjectId;if(!item.Ready(Player)){attempts.Remove(id);continue;}if(attempts.TryGetValue(id,out var revision) && revision==Revision.Value)continue;
                attempts[id]=Revision.Value;Pickup(id);break;
            }
        }
        public override void OnNetworkDespawn()
        {
            Inventory.Model.Changed-=Publish;Inventory.SelectionChanged-=PublishSelection;progression.Changed-=PublishRewards;Player.Commands.Session.StateChanged-=ViewChanged;
            Slots.OnListChanged-=ListChanged;Selected.OnValueChanged-=SelectionChanged;Revision.OnValueChanged-=RevisionChanged;Rewards.OnValueChanged-=RewardsChanged;
            Inventory.Authority=null;nearby.Clear();attempts.Clear();
        }
    }
}
