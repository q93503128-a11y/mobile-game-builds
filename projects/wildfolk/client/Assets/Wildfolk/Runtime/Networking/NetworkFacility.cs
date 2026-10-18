using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    // Shared, server-owned facility state. Opening a screen never reserves or locks cargo.
    public sealed class NetworkFacility : NetworkBehaviour
    {
        public NetworkList<NetworkStack> Slots;
        public NetworkList<FacilityProcessEntry> Cargo;
        public readonly NetworkVariable<uint> Revision=new NetworkVariable<uint>();
        public readonly NetworkVariable<int> Tier=new NetworkVariable<int>(1);
        public readonly NetworkVariable<FacilityProcessState> Processing=new NetworkVariable<FacilityProcessState>(new FacilityProcessState{Recipe=-1});
        public StorageChest Chest {get;private set;}
        public CraftingStation Station {get;private set;}
        public StationProcessing Processor {get;private set;}
        private WorldPlaceable placeable;
        private ContentCatalog Catalog=>NetworkCreatureWorld.Active.Catalog;
        private void Awake(){Slots=new NetworkList<NetworkStack>();Cargo=new NetworkList<FacilityProcessEntry>();Chest=GetComponent<StorageChest>();Station=GetComponent<CraftingStation>();Processor=GetComponent<StationProcessing>();placeable=GetComponent<WorldPlaceable>();placeable.Health.IsReplica=true;}
        public override void OnNetworkSpawn()
        {
            if(Chest!=null){Chest.Model.IsReadOnly=!IsServer;if(IsServer)Chest.Model.Changed+=Publish;}
            if(Processor!=null){Processor.ReplicaRemaining=()=>Processing.Value.Running ? Mathf.Max(0,(float)(Processing.Value.Ends-NetworkManager.ServerTime.Time)) : 0;if(IsServer)Processor.Changed+=Publish;}
            Slots.OnListChanged+=SlotsChanged;Cargo.OnListChanged+=CargoChanged;Processing.OnValueChanged+=ProcessChanged;Tier.OnValueChanged+=TierChanged;Revision.OnValueChanged+=RevisionChanged;
            if(IsServer){if(Station!=null && Station.Definition.Station==StationKind.Workbench)Tier.Value=Station.Definition.WorkbenchTier;Publish();}else{ApplyTier();MirrorSlots();MirrorProcessing();}
        }
        public bool InRange(NetworkPlayer player)=>IsSpawned && placeable.Health.IsAlive && Vector2.Distance(transform.position,player.transform.position)<=player.GetComponent<PlayerInventory>().Settings.StationInteractionRange;
        public bool Upgrade(PlayerInventory player)
        {
            if(!IsServer || !IsSpawned || Station==null || !WorkbenchProgression.CanUpgrade(Station.Definition))return false;
            var old=Station.Definition;var hp=placeable.Health.Current;
            try
            {
                if(!player.Model.TryConsumeCosts(old.UpgradeCosts,()=>{SetDefinition(old.UpgradeTo);Tier.Value=Station.Definition.WorkbenchTier;return true;}))return false;
            }
            catch(Exception){SetDefinition(old);placeable.Health.RestoreHealth(hp);Tier.Value=old.WorkbenchTier;return false;}
            Publish();return true;
        }
        private void ApplyTier()
        {
            if(Station==null || Station.Definition.Station!=StationKind.Workbench || !WorkbenchProgression.IsValid(Tier.Value))return;
            foreach(var item in Catalog.Items)if(item.Placement!=null && item.Placement.Station==StationKind.Workbench && item.Placement.WorkbenchTier==Tier.Value){SetDefinition(item.Placement);return;}
        }
        private void SetDefinition(PlaceableDefinition next)
        {
            var ratio=placeable.Health.Ratio;
            placeable.Definition=Station.Definition=next;placeable.Visual.sprite=next.TierVisual!=null ? next.TierVisual : next.Prefab.Visual.sprite;placeable.Visual.transform.localScale=next.Prefab.Visual.transform.localScale;placeable.Health.Definition=next.Vitality;
            if(IsServer)placeable.Health.RestoreHealth(Mathf.Max(1,Mathf.RoundToInt(ratio*placeable.Health.Maximum)));Station.NotifyChanged();
        }
        private void Publish()
        {
            if(!IsServer || !IsSpawned)return;
            if(Chest!=null)for(var i=0;i<Chest.Model.Capacity;i++){var slot=Chest.Model.GetSlot(i);var value=new NetworkStack{Item=slot.IsEmpty ? (short)-1 : (short)Array.IndexOf(Catalog.Items,slot.Item),Count=slot.Count,Charges=slot.Charges,Quality=slot.Quality};if(i>=Slots.Count)Slots.Add(value);else if(!Slots[i].Equals(value))Slots[i]=value;}
            if(Processor!=null)
            {
                var snapshot=Processor.Capture();var current=Processor.Recipe;
                var entries=new List<FacilityProcessEntry>();
                if(snapshot?.Waiting!=null)foreach(var e in snapshot.Waiting)entries.Add(new FacilityProcessEntry{Recipe=(short)Array.IndexOf(Catalog.Recipes,Catalog.Recipe(e.Recipe)),Waiting=e.Count});
                if(snapshot?.Output!=null)foreach(var e in snapshot.Output)entries.Add(new FacilityProcessEntry{Recipe=(short)Array.IndexOf(Catalog.Recipes,Catalog.Recipe(e.Recipe)),Output=e.Count});
                for(var i=0;i<entries.Count;i++){if(i>=Cargo.Count)Cargo.Add(entries[i]);else if(!Cargo[i].Equals(entries[i]))Cargo[i]=entries[i];}
                while(Cargo.Count>entries.Count)Cargo.RemoveAt(Cargo.Count-1);
                Processing.Value=new FacilityProcessState{Recipe=current==null ? (short)-1 : (short)Array.IndexOf(Catalog.Recipes,current),Batch=Processor.BatchCount,Running=current!=null && Processor.Remaining>0,Ends=current==null ? 0 : NetworkManager.ServerTime.Time+Processor.Remaining};
            }
            Revision.Value++;
        }
        private void MirrorSlots(){if(Chest==null || IsServer)return;var values=new ItemStack[Chest.Model.Capacity];for(var i=0;i<Slots.Count && i<values.Length;i++){var s=Slots[i];values[i]=s.Count==0 ? default : new ItemStack(Catalog.Items[s.Item],s.Count,s.Charges,s.Quality);}Chest.Model.ApplyReplicaSlots(values);}
        private void SlotsChanged(NetworkListEvent<NetworkStack> change){if(IsServer || Chest==null)return;if(change.Type==NetworkListEvent<NetworkStack>.EventType.Value){var s=change.Value;Chest.Model.ApplyReplicaSlot(change.Index,s.Count==0 ? default : new ItemStack(Catalog.Items[s.Item],s.Count,s.Charges,s.Quality));}else MirrorSlots();}
        private void MirrorProcessing()
        {
            if(IsServer || Processor==null)return;var value=Processing.Value;var waiting=new List<ProcessingEntry>();var output=new List<ProcessingEntry>();
            foreach(var entry in Cargo){var id=Catalog.Recipes[entry.Recipe].Id;if(entry.Waiting>0)waiting.Add(new ProcessingEntry{Recipe=id,Count=entry.Waiting});if(entry.Output>0)output.Add(new ProcessingEntry{Recipe=id,Count=entry.Output});}
            Processor.ApplyReplica(new ProcessingSnapshot{Sequential=Processor.Sequential,Recipe=value.Recipe<0 ? null : Catalog.Recipes[value.Recipe].Id,Batch=value.Batch,Remaining=value.Running ? Mathf.Max(0,(float)(value.Ends-NetworkManager.ServerTime.Time)) : 0,Waiting=waiting.ToArray(),Output=output.ToArray()},Catalog);Station.NotifyChanged();
        }
        private void CargoChanged(NetworkListEvent<FacilityProcessEntry> change)=>MirrorProcessing();
        private void ProcessChanged(FacilityProcessState old,FacilityProcessState value)=>MirrorProcessing();
        private void TierChanged(int old,int value){if(!IsServer)ApplyTier();}
        private void RevisionChanged(uint old,uint value){if(!IsServer){MirrorProcessing();Station?.NotifyChanged();}}
        public override void OnNetworkDespawn()
        {
            if(Chest!=null)Chest.Model.Changed-=Publish;if(Processor!=null)Processor.Changed-=Publish;
            Slots.OnListChanged-=SlotsChanged;Cargo.OnListChanged-=CargoChanged;Processing.OnValueChanged-=ProcessChanged;Tier.OnValueChanged-=TierChanged;Revision.OnValueChanged-=RevisionChanged;
        }
    }
}
