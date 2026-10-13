using System;
using UnityEngine;

namespace Wildfolk
{
    public enum ResourceKind { Wood, Stone, Meat }
    [Serializable] public struct ResourceEfficiency
    {
        public ResourceKind Kind;
        [Min(.01f)] public float Multiplier;
    }

    [CreateAssetMenu(menuName = "Wildfolk/Tool")]
    public sealed class ToolDefinition : ScriptableObject
    {
        public Sprite HeldSprite;
        [Min(1)] public int Damage;
        [Min(.1f)] public float Reach;
        [Min(.01f)] public float Duration;
        [Min(.01f)] public float Cooldown;
        [Min(0)] public float HandExtension;
        [Range(1,180)] public float SwingArc;
        public ResourceEfficiency[] Efficiencies;

        public int DamageFor(ResourceKind kind)
        {
            foreach (var entry in Efficiencies)
                if (entry.Kind == kind) return Mathf.Max(1, Mathf.RoundToInt(Damage * entry.Multiplier));
            return Damage;
        }
    }
}
