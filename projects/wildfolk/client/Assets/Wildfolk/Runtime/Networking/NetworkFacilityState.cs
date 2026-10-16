using System;
using Unity.Netcode;
namespace Wildfolk
{
    public enum FacilityOperation : byte { Open, Transfer, Craft, Upgrade, AddOne, AddAll, Withdraw, Collect, Drop }
    public struct FacilityProcessEntry : INetworkSerializable,IEquatable<FacilityProcessEntry>
    {
        public short Recipe; public int Waiting,Output;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref Recipe);s.SerializeValue(ref Waiting);s.SerializeValue(ref Output);}
        public bool Equals(FacilityProcessEntry other)=>Recipe==other.Recipe && Waiting==other.Waiting && Output==other.Output;
    }
    public struct FacilityProcessState : INetworkSerializable,IEquatable<FacilityProcessState>
    {
        public short Recipe; public int Batch; public double Ends; public bool Running;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref Recipe);s.SerializeValue(ref Batch);s.SerializeValue(ref Ends);s.SerializeValue(ref Running);}
        public bool Equals(FacilityProcessState other)=>Recipe==other.Recipe && Batch==other.Batch && Ends==other.Ends && Running==other.Running;
    }
}
