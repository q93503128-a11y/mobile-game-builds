using System;
using Unity.Netcode;
using UnityEngine;

namespace Wildfolk
{
    public struct PlayerMotionState : INetworkSerializable,IEquatable<PlayerMotionState>
    {
        public Vector2 Facing;
        public byte Flags;
        public bool Moving=>(Flags&1)!=0;
        public bool Sprinting=>(Flags&2)!=0;
        public bool Dashing=>(Flags&4)!=0;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T:IReaderWriter
        {serializer.SerializeValue(ref Facing);serializer.SerializeValue(ref Flags);}
        public bool Equals(PlayerMotionState other)=>Facing==other.Facing && Flags==other.Flags;
    }
    public struct PlayerStatusState : INetworkSerializable,IEquatable<PlayerStatusState>
    {
        public int HP,MaximumHP;
        public double TreatmentUntil;
        public short MealItem;
        public double MealUntil;
        public short Head,Body,Held,Utility;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T:IReaderWriter
        {serializer.SerializeValue(ref HP);serializer.SerializeValue(ref MaximumHP);serializer.SerializeValue(ref Head);serializer.SerializeValue(ref Body);serializer.SerializeValue(ref Held);serializer.SerializeValue(ref Utility);serializer.SerializeValue(ref TreatmentUntil);serializer.SerializeValue(ref MealItem);serializer.SerializeValue(ref MealUntil);}
        public bool Equals(PlayerStatusState other)=>HP==other.HP && MaximumHP==other.MaximumHP && Head==other.Head && Body==other.Body && Held==other.Held && Utility==other.Utility && TreatmentUntil==other.TreatmentUntil && MealItem==other.MealItem && MealUntil==other.MealUntil;
    }
    public struct PlayerAttackState : INetworkSerializable,IEquatable<PlayerAttackState>
    {
        public ushort Sequence;
        public byte Hand;
        public short Tool;
        public double Started;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter
        {s.SerializeValue(ref Sequence);s.SerializeValue(ref Hand);s.SerializeValue(ref Tool);s.SerializeValue(ref Started);}
        public bool Equals(PlayerAttackState o)=>Sequence==o.Sequence&&Hand==o.Hand&&Tool==o.Tool&&Started==o.Started;
    }
    public static class NetworkInputRules
    {
        public static Vector2 Sanitize(Vector2 value)=>float.IsNaN(value.x)||float.IsNaN(value.y)||float.IsInfinity(value.x)||float.IsInfinity(value.y) ? Vector2.zero : Vector2.ClampMagnitude(value,1);
    }
}
