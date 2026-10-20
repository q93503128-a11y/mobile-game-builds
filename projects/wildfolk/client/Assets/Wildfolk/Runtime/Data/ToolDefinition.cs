using System;
using UnityEngine;

namespace Wildfolk
{
    public enum ResourceKind { Wood, Stone, Meat, IronOre, CopperOre, TinOre, Coal, PlantFiber, Berry, Mushroom, Diamond, Crystal, MedicinalHerb }
    public enum ToolKind { Axe, Pickaxe, Sword, Pan, Spear, Bow, Crossbow }
    public enum HarvestMethod { Strike, Panning, Interaction }
    [Serializable] public struct ResourceEfficiency
    {
        public ResourceKind Kind;
        [Min(.01f)] public float Multiplier;
    }

    [CreateAssetMenu(menuName = "Wildfolk/Tool")]
    public sealed class ToolDefinition : ScriptableObject
    {
        public Sprite HeldSprite;
        public EquipmentTier Tier;
        public ToolKind Kind;
        public HarvestMethod HarvestMethod;
        [Min(1)] public int Damage;
        public AttackProfile Attack=new AttackProfile();
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
