using System;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    // Every mutation is serialized on the Host against both independent revisions.
    public sealed class NetworkFacilityInteractor : NetworkBehaviour
    {
        public NetworkInventory Bag {get;private set;}
        public PlayerCrafting Crafting {get;private set;}
        public PlayerStorage Storage {get;private set;}
        public bool IsApplying {get;private set;}
        public bool Pending=>pending!=0;
        private ulong sequence,pending,lastRequest;
        private uint waitBag,waitFacility;
        private NetworkFacility waiting,opened;
        private bool acknowledged;private RecipeDefinition pendingRecipe;
        private uint dragBag,dragFacility;
        public void CaptureTransferState(){dragBag=Bag.Revision.Value;dragFacility=Storage.Chest==null ? 0 : Storage.Chest.GetComponent<NetworkFacility>().Revision.Value;}
        private void Awake(){Bag=GetComponent<NetworkInventory>();Crafting=GetComponent<PlayerCrafting>();Storage=GetComponent<PlayerStorage>();}
        public override void OnNetworkSpawn(){Crafting.Authority=this;Storage.Authority=this;GetComponent<PlayerInteraction>().enabled=IsOwner;Bag.Player.Commands.Session.StateChanged+=SessionChanged;}
        private void SessionChanged(){if(Bag.Player.Commands.Session.State!=SessionState.Inventory)opened=null;}
        public void CloseFacilityView()
        {
            if(!IsSpawned || !IsOwner || IsApplying)return;
            if(IsServer)opened=null;else CloseViewRpc();
        }
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void CloseViewRpc(RpcParams rpc=default)
        {
            if(rpc.Receive.SenderClientId!=OwnerClientId || !Bag.CanOperate)return;
            opened=null;IsApplying=true;try{Crafting.OpenBag();}finally{IsApplying=false;}
        }
        public bool Open(NetworkFacility facility)=>facility!=null && Submit(FacilityOperation.Open,facility);
        public bool Craft(RecipeDefinition recipe)=>Submit(FacilityOperation.Craft,recipe.RequiredStation==StationKind.None ? null : Crafting.Station?.GetComponent<NetworkFacility>(),recipe:recipe);
        public bool Upgrade()=>Submit(FacilityOperation.Upgrade,Crafting.Station?.GetComponent<NetworkFacility>());
        public bool Process(FacilityOperation operation,StationProcessing processor,RecipeDefinition recipe=null)=>Submit(operation,processor.GetComponent<NetworkFacility>(),recipe:recipe);
        public bool Transfer(InventoryModel source,int from,InventoryModel target,int to,int amount)
        {
            var chest=Storage.Chest;return chest!=null && Submit(FacilityOperation.Transfer,chest.GetComponent<NetworkFacility>(),from,to,amount,(byte)((source==chest.Model ? 1 : 0)|(target==chest.Model ? 2 : 0)));
        }
        public bool Drop(int slot,int amount)=>Storage.Chest!=null && Submit(FacilityOperation.Drop,Storage.Chest.GetComponent<NetworkFacility>(),slot,0,amount);
        private bool Submit(FacilityOperation op,NetworkFacility facility,int from=0,int to=0,int amount=0,byte sides=0,RecipeDefinition recipe=null)
        {
            if(!IsSpawned || !IsOwner || !Bag.Player.Health.IsAlive || Pending || Bag.Pending)return false;
            var id=facility==null ? ulong.MaxValue : facility.NetworkObjectId;var revision=facility==null ? 0 : facility.Revision.Value;var index=recipe==null ? -1 : Array.IndexOf(Bag.Player.Catalog.Recipes,recipe);
            var usingDrag=op==FacilityOperation.Transfer || op==FacilityOperation.Drop;
            var bagRevision=usingDrag ? dragBag : Bag.Revision.Value;if(usingDrag)revision=dragFacility;
            if(IsServer){var result=Execute(op,id,bagRevision,revision,index,from,to,amount,sides);PresentResult(op,recipe,result);return result;}
            pendingRecipe=recipe;pending=++sequence;acknowledged=false;waiting=facility;RequestRpc(pending,op,id,bagRevision,revision,index,from,to,amount,sides);return true;
        }
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void RequestRpc(ulong request,FacilityOperation op,ulong id,uint bagRevision,uint facilityRevision,int recipe,int from,int to,int amount,byte sides,RpcParams rpc=default)
        {
            if(rpc.Receive.SenderClientId!=OwnerClientId || request<=lastRequest)return;lastRequest=request;
            var success=Execute(op,id,bagRevision,facilityRevision,recipe,from,to,amount,sides);
            var facility=Resolve(id);ResultRpc(request,op,id,success,Bag.Revision.Value,facility==null ? 0 : facility.Revision.Value);
        }
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]
        private void ResultRpc(ulong request,FacilityOperation op,ulong id,bool success,uint bagRevision,uint facilityRevision)
        {
            if(pending!=request)return;waitBag=bagRevision;waitFacility=facilityRevision;acknowledged=true;
            if(op==FacilityOperation.Open && success){IsApplying=true;try{ApplyOpen(Resolve(id));}finally{IsApplying=false;}}
            if(!success)PlayerFeedback.ShowKey("feedback.slot_space");else PresentResult(op,pendingRecipe,true);FinishPending();
        }
        private static void PresentResult(FacilityOperation op,RecipeDefinition recipe,bool success)
        {
            if(!success)return;
            if(op==FacilityOperation.Upgrade)PlayerFeedback.ShowOperation(PlayerFeedback.Cue.Upgraded);
            else if(op==FacilityOperation.Collect)PlayerFeedback.ShowOperation(PlayerFeedback.Cue.Collected);
            else if(op==FacilityOperation.AddOne || op==FacilityOperation.AddAll)PlayerFeedback.ShowOperation(PlayerFeedback.Cue.Processing);
            else if(op==FacilityOperation.Craft && recipe!=null){if(recipe.ProcessingSeconds>0)PlayerFeedback.ShowOperation(PlayerFeedback.Cue.Processing);else PlayerFeedback.ShowCrafted(recipe);}
        }
        // Only the requesting owner checks receipt; facilities have no network polling loop.
        private void Update(){if(acknowledged)FinishPending();}
        private void FinishPending(){if(Bag.Revision.Value>=waitBag && (waiting==null || !waiting.IsSpawned || waiting.Revision.Value>=waitFacility)){pending=0;acknowledged=false;waiting=null;}}
        private NetworkFacility Resolve(ulong id)=>NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id,out var obj) ? obj.GetComponent<NetworkFacility>() : null;
        private bool ApplyOpen(NetworkFacility facility)
        {
            if(facility==null)return false;
            Bag.Player.Commands.Session.CloseInventory();
            var success=facility.Chest!=null ? Storage.OpenChest(facility.Chest) : facility.Station!=null && Crafting.OpenStation(facility.Station);
            if(success)opened=facility;return success;
        }
        public bool Execute(FacilityOperation op,ulong id,uint bagRevision,uint facilityRevision,int recipe=-1,int from=0,int to=0,int amount=0,byte sides=0)
        {
            if(!Bag.CanOperate || Bag.Player.Commands.IsScouting || bagRevision!=Bag.Revision.Value || amount<0)return false;
            var facility=Resolve(id);var hand=id==ulong.MaxValue && op==FacilityOperation.Craft;
            if(!hand && (facility==null || !facility.InRange(Bag.Player) || facility.Revision.Value!=facilityRevision))return false;
            if(op!=FacilityOperation.Open && Bag.Player.Commands.Session.State!=SessionState.Inventory)return false;
            if(!hand && op!=FacilityOperation.Open && opened!=facility)return false;
            var catalog=Bag.Player.Catalog;var selected=recipe>=0 && recipe<catalog.Recipes.Length ? catalog.Recipes[recipe] : null;
            if(op==FacilityOperation.Craft && (selected==null || hand!=(selected.RequiredStation==StationKind.None)))return false;
            IsApplying=true;
            try
            {
                if(op==FacilityOperation.Open)return ApplyOpen(facility);
                if(op==FacilityOperation.Transfer)
                {
                    if(facility.Chest==null || sides<1 || sides>3)return false;
                    var source=(sides&1)!=0 ? facility.Chest.Model : Bag.Inventory.Model;var target=(sides&2)!=0 ? facility.Chest.Model : Bag.Inventory.Model;
                    return from>=0 && from<source.Capacity && to>=0 && to<target.Capacity && InventoryModel.Transfer(source,from,target,to,amount);
                }
                if(op==FacilityOperation.Drop)
                {
                    if(facility.Chest==null || from<0 || from>=facility.Chest.Model.Capacity)return false;
                    return GetComponent<PlayerItemDrop>().Drop(facility.Chest.Model,from,amount);
                }
                if(op==FacilityOperation.Upgrade)return facility.Upgrade(Bag.Inventory);
                if(op==FacilityOperation.Craft)
                {
                    if(hand && selected.ProcessingSeconds>0)return false;
                    if(!hand && (facility.Station==null || Crafting.Station!=facility.Station || selected.RequiredStation!=facility.Station.Definition.Station))return false;
                    var status=Crafting.GetStatus(selected);if(status!=CraftStatus.Ready && status!=CraftStatus.Collect)return false;
                    var result=Crafting.Craft(selected);return result==CraftStatus.Ready || result==CraftStatus.Processing || result==CraftStatus.Collect;
                }
                var processor=facility.Processor;if(processor==null || Crafting.Station!=facility.Station)return false;
                if(op==FacilityOperation.Collect)return processor.Collect(Bag.Inventory);
                if(selected==null || selected.RequiredStation!=facility.Station.Definition.Station || selected.ProcessingSeconds<=0)return false;
                switch(op){case FacilityOperation.AddOne:return processor.Add(selected,Bag.Inventory,1);case FacilityOperation.AddAll:return processor.AddAll(selected,Bag.Inventory)>0;case FacilityOperation.Withdraw:return processor.Withdraw(selected,Bag.Inventory);default:return false;}
            }
            finally{IsApplying=false;}
        }
        public override void OnNetworkDespawn(){Bag.Player.Commands.Session.StateChanged-=SessionChanged;Crafting.Authority=null;Storage.Authority=null;opened=waiting=null;pending=0;acknowledged=false;Bag.Player.Commands.Session.CloseInventory();}
    }
}
