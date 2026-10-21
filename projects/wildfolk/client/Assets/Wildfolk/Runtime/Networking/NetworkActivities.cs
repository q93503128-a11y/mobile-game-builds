using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public enum ActivityKind:byte { None,Ranged,Fishing,FishingCaught }
    public struct ActivityState:INetworkSerializable,System.IEquatable<ActivityState>
    {
        public uint Sequence;public ActivityKind Kind;public double Started;public float Duration;public Vector2 Point;
        public int CatchIndex,CatchCount;
        public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter{s.SerializeValue(ref Sequence);s.SerializeValue(ref Kind);s.SerializeValue(ref Started);s.SerializeValue(ref Duration);s.SerializeValue(ref Point);s.SerializeValue(ref CatchIndex);s.SerializeValue(ref CatchCount);}
        public bool Equals(ActivityState o)=>Sequence==o.Sequence&&Kind==o.Kind&&Started==o.Started&&Duration==o.Duration&&Point==o.Point&&CatchIndex==o.CatchIndex&&CatchCount==o.CatchCount;
    }
    public sealed class NetworkActivities:NetworkBehaviour
    {
        public readonly NetworkVariable<ActivityState> Activity=new NetworkVariable<ActivityState>();
        public void Publish(ActivityKind kind,Vector2 point,float duration)
        {if(IsSpawned && IsServer)Activity.Value=new ActivityState{Sequence=Activity.Value.Sequence+1,Kind=kind,Point=point,Started=NetworkManager.ServerTime.Time,Duration=duration};}
        public void PublishCatch(int entry,int count,Vector2 point)
        {if(IsSpawned && IsServer)Activity.Value=new ActivityState{Sequence=Activity.Value.Sequence+1,Kind=ActivityKind.FishingCaught,Point=point,Started=NetworkManager.ServerTime.Time,Duration=.6f,CatchIndex=entry,CatchCount=count};}
        public void PublishContext(FishingWaterContext context){if(IsSpawned && IsServer)WaterFeedbackRpc((int)context.Water,context.Night,context.Deep && context.ShoreInset>=2.25f);}
        public void Feedback(bool full){if(IsSpawned && IsServer)FishingFeedbackRpc(full);}
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]private void FishingFeedbackRpc(bool full)=>PlayerFeedback.ShowKey(full ? "feedback.bag_full" : "feedback.fishing_water");
        [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]private void WaterFeedbackRpc(int water,bool night,bool deep)=>PlayerFeedback.ShowWater(new FishingWaterContext((FishingWater)water,night,deep,deep ? 2.25f : 0));
        private PlayerEquipment equipment;private PlayerRanged ranged;
        private void Awake(){equipment=GetComponent<PlayerEquipment>();ranged=GetComponent<PlayerRanged>();}
        private void LateUpdate(){if(!IsSpawned || IsServer || Activity.Value.Kind!=ActivityKind.Ranged)return;var s=Activity.Value;var elapsed=(float)(NetworkManager.ServerTime.Time-s.Started);var item=equipment.SelectedItem;if(item!=null && item.Ranged!=null && elapsed>=0 && elapsed<s.Duration)ranged.Present(elapsed,item.Ranged);}
    }
}
