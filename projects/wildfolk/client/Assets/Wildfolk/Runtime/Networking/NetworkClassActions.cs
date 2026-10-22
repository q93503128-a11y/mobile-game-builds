using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public struct ClassActionState:INetworkSerializable,IEquatable<ClassActionState>
    {
        public byte ClassIndex;public ulong Nodes,VisibleQuarry;public bool Guarding;
        public byte Bearing,Distance,Condition,Meaning;public Vector2 Marker;
        public double QuarryUntil,MarkerUntil,ActiveReady,Protection,Heavy;
        public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter
        {s.SerializeValue(ref ClassIndex);s.SerializeValue(ref Nodes);s.SerializeValue(ref VisibleQuarry);s.SerializeValue(ref Guarding);s.SerializeValue(ref Bearing);s.SerializeValue(ref Distance);s.SerializeValue(ref Condition);s.SerializeValue(ref Meaning);s.SerializeValue(ref Marker);s.SerializeValue(ref QuarryUntil);s.SerializeValue(ref MarkerUntil);s.SerializeValue(ref ActiveReady);s.SerializeValue(ref Protection);s.SerializeValue(ref Heavy);}
        public bool Equals(ClassActionState b)=>ClassIndex==b.ClassIndex && Nodes==b.Nodes && VisibleQuarry==b.VisibleQuarry && Guarding==b.Guarding && Bearing==b.Bearing && Distance==b.Distance && Condition==b.Condition && Meaning==b.Meaning && Marker==b.Marker && QuarryUntil==b.QuarryUntil && MarkerUntil==b.MarkerUntil && ActiveReady==b.ActiveReady && Protection==b.Protection && Heavy==b.Heavy;
    }
    public enum ClassAction:byte { Guard,Medicine,Mark,Building,Overclock }
    public sealed class NetworkClassActions:NetworkBehaviour
    {
        public readonly NetworkVariable<ClassActionState> State=new NetworkVariable<ClassActionState>();
        private ulong sequence,lastRequest;private PlayerClass role;private PlayerInventory bag;private NetworkInventory inventory;private float nextTick;
        private void Awake(){role=GetComponent<PlayerClass>();bag=GetComponent<PlayerInventory>();inventory=GetComponent<NetworkInventory>();}
        public override void OnNetworkSpawn(){State.OnValueChanged+=Changed;if(IsServer){role.Changed+=Publish;Publish();}else Apply(State.Value);}
        public override void OnNetworkDespawn(){State.OnValueChanged-=Changed;role.Changed-=Publish;GetComponent<PlayerBuildingService>().Cancel();GetComponent<PlayerMedicine>().Cancel();GetComponent<PlayerTeamMarker>().Clear();}
        private void Changed(ClassActionState old,ClassActionState next){if(!IsServer)Apply(next);}
        private void Apply(ClassActionState value)
        {if(value.ClassIndex>=ClassCatalog.Current.Classes.Length)return;role.Replica(ClassCatalog.Current.Classes[value.ClassIndex].Id,value.Nodes);}
        private void Update(){if(!IsSpawned || !IsServer || Time.unscaledTime<nextTick)return;nextTick=Time.unscaledTime+.2f;Publish();}
        public void Publish()
        {
            if(!IsSpawned || !IsServer || role.Definition==null)return;
            var marker=GetComponent<PlayerTeamMarker>();var quarry=role.Quarry;var delta=quarry==null?Vector2.zero:(Vector2)(quarry.transform.position-transform.position);
            var detail=role.Value(ClassEffect.QuarryDetail)>0;
            var n=new ClassActionState{ClassIndex=(byte)Array.IndexOf(ClassCatalog.Current.Classes,role.Definition),Nodes=role.AllocationBits(),Guarding=role.Guarding,QuarryUntil=quarry==null?0:role.QuarryUntil,VisibleQuarry=quarry!=null && ClassActionRules.Clear(GetComponent<Health>(),quarry)?quarry.GetComponent<NetworkObject>()?.NetworkObjectId??ulong.MaxValue:ulong.MaxValue,Bearing=PlayerExplorationUtility.Bearing(delta),Distance=(byte)(detail?delta.magnitude<8?0:delta.magnitude<20?1:delta.magnitude<40?2:3:delta.magnitude<15?0:delta.magnitude<40?1:2),Condition=(byte)(quarry==null?0:detail?quarry.Ratio>.75f?0:quarry.Ratio>.5f?1:quarry.Ratio>.25f?2:3:quarry.Ratio>.6f?0:quarry.Ratio>.25f?1:2),Marker=marker.Point,MarkerUntil=marker.Until,Meaning=marker.Meaning,ActiveReady=role.ActiveReady,Protection=role.ProtectionUntil,Heavy=role.HeavyUntil};
            if(!State.Value.Equals(n))State.Value=n;
        }
        public bool Request(ClassAction action,bool held=false,UnityEngine.Object target=null,Vector2 point=default,byte mode=0)
        {
            if(!IsSpawned || !IsOwner)return false;var component=target as Component;var net=component!=null?component.GetComponent<NetworkObject>():null;var id=net==null?ulong.MaxValue:net.NetworkObjectId;
            var revision=component!=null?component.GetComponent<NetworkBuilding>()?.Revision.Value??0:0;
            if(IsServer)return Execute(++sequence,action,held,id,point,mode,inventory.Revision.Value,revision);
            RequestRpc(++sequence,action,held,id,point,mode,inventory.Revision.Value,revision);return true;
        }
        [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
        private void RequestRpc(ulong request,ClassAction action,bool held,ulong target,Vector2 point,byte mode,uint bagRevision,uint targetRevision,RpcParams rpc=default)
        {if(rpc.Receive.SenderClientId==OwnerClientId)Execute(request,action,held,target,point,mode,bagRevision,targetRevision);}
        public bool Execute(ulong request,ClassAction action,bool held,ulong target,Vector2 point,byte mode,uint bagRevision,uint targetRevision)
        {
            if(!IsSpawned || !IsServer || request==0 || request<=lastRequest)return false;lastRequest=request;
            // Release always works, even after the actor becomes incapacitated or its bag revision changes.
            if(action==ClassAction.Guard && !held)return role.SetGuard(false);
            if(action==ClassAction.Building && !held)return GetComponent<PlayerBuildingService>().Hold(null,BuildingServiceKind.Repair,false);
            if(!inventory.CanOperate || !GetComponent<PlayerInputCommands>().Session.IsRunning)return false;
            if(action==ClassAction.Guard)return role.SetGuard(held);
            if(action==ClassAction.Mark)return GetComponent<PlayerTeamMarker>().Place(point,mode);
            if(bagRevision!=inventory.Revision.Value || !NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(target,out var obj))return false;
            if(action==ClassAction.Medicine)return GetComponent<PlayerMedicine>().Begin(obj.GetComponent<Health>());
            if(action==ClassAction.Overclock)return role.Overclock(obj.GetComponent<WorldTurret>());
            if(action==ClassAction.Building && obj.TryGetComponent<NetworkBuilding>(out var building) && targetRevision==building.Revision.Value)return GetComponent<PlayerBuildingService>().Hold(obj.GetComponent<StructureMaintenance>(),(BuildingServiceKind)mode,held);
            return false;
        }
        public void SendProfile()
        {if(!IsSpawned || !IsServer || role.Definition==null)return;ProgressRpc(role.Definition.Id,role.ClassXP);}
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]private void ProgressRpc(FixedString64Bytes id,long total)
        {if(IsServer)return;ClassProfiles.Update(ClassProfiles.LocalId,d=>{var p=d.Find(id.ToString());if(p?.Owned!=true || total<p.XP)return false;p.XP=total;return true;});}
        public void MedicineFeedback(Health target)
        {if(IsSpawned && IsServer)MedicineFeedbackRpc();}
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]private void MedicineFeedbackRpc()=>PlayerFeedback.ShowKey("class.medicine_done");
    }
}
