using System;
using UnityEngine;
namespace Wildfolk
{
    public enum TargetSelectionRule { ClosestToAim, Nearest }
    [Serializable] public sealed class AttackProfile
    {
        [Min(1)] public int MaxTargets=1;
        [Range(1,360)] public float ArcAngle=120;
        [Min(.1f)] public float Range=1.6f;
        [Min(0)] public int PierceCount;
        [Min(0)] public float AoERadius;
        public TargetSelectionRule SelectionRule;
    }
}
