using System;
using Unity.Netcode;
namespace Wildfolk
{
    public struct NetworkStack : INetworkSerializable,IEquatable<NetworkStack>
    {
        public short Item;
        public int Count;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter{s.SerializeValue(ref Item);s.SerializeValue(ref Count);}
        public bool Equals(NetworkStack other)=>Item==other.Item&&Count==other.Count;
    }
    public struct SessionRewardState : INetworkSerializable,IEquatable<SessionRewardState>
    {
        public long Experience,Gold;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter{s.SerializeValue(ref Experience);s.SerializeValue(ref Gold);}
        public bool Equals(SessionRewardState other)=>Experience==other.Experience&&Gold==other.Gold;
    }
    public enum InventoryOperation : byte { Select,Transfer,Drop,Equip,Unequip,Pickup }
    public interface IInventoryAuthority
    {
        bool IsApplying {get;}
        bool Select(int slot);
        bool Transfer(int from,int to,int amount);
        bool Drop(int slot,int amount);
        bool Equip(int slot);
        bool Unequip(ArmorSlot slot,int destination);
    }
}
