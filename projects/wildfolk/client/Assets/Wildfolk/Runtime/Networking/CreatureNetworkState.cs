using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace Wildfolk
{
    public struct CreatureIdentityState : INetworkSerializable,IEquatable<CreatureIdentityState>
    {
        public FixedString64Bytes Species,Group,Camp;
        public Vector2 Anchor;
        public int Traits;
        public bool Variant;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter
        {s.SerializeValue(ref Species);s.SerializeValue(ref Group);s.SerializeValue(ref Camp);s.SerializeValue(ref Anchor);s.SerializeValue(ref Traits);s.SerializeValue(ref Variant);}
        public bool Equals(CreatureIdentityState o)=>Species.Equals(o.Species)&&Group.Equals(o.Group)&&Camp.Equals(o.Camp)&&Anchor==o.Anchor&&Traits==o.Traits&&Variant==o.Variant;
    }
    // Quantized pose changes only; NGO NetworkTransform carries root XY independently.
    public struct CreatureFrame : INetworkSerializable,IEquatable<CreatureFrame>
    {
        public int HP;
        public ushort Hits;
        public ulong Target;
        public byte Brain,Lifecycle;
        public Vector2 Facing,Left,Right,VisualPosition,VisualScale;
        public float LeftArc,RightArc,Rotation;
        public bool Ranged;
        public double ShotStarted;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter
        {
            s.SerializeValue(ref HP);s.SerializeValue(ref Hits);s.SerializeValue(ref Target);s.SerializeValue(ref Brain);s.SerializeValue(ref Lifecycle);
            s.SerializeValue(ref Facing);s.SerializeValue(ref Left);s.SerializeValue(ref Right);s.SerializeValue(ref VisualPosition);s.SerializeValue(ref VisualScale);
            s.SerializeValue(ref LeftArc);s.SerializeValue(ref RightArc);s.SerializeValue(ref Rotation);s.SerializeValue(ref Ranged);s.SerializeValue(ref ShotStarted);
        }
        public bool Equals(CreatureFrame o)=>HP==o.HP&&Hits==o.Hits&&Target==o.Target&&Brain==o.Brain&&Lifecycle==o.Lifecycle&&Facing==o.Facing&&Left==o.Left&&Right==o.Right&&VisualPosition==o.VisualPosition&&VisualScale==o.VisualScale&&LeftArc==o.LeftArc&&RightArc==o.RightArc&&Rotation==o.Rotation&&Ranged==o.Ranged&&ShotStarted==o.ShotStarted;
    }
}
