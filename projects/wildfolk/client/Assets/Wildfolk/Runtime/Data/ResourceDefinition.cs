using UnityEngine;

namespace Wildfolk
{
    [CreateAssetMenu(menuName = "Wildfolk/Resource")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        public ResourceKind Kind;
        public EquipmentTier MinimumTier;
        public ToolKind RequiredTool;
        public bool CanHarvest(ToolDefinition tool) => MinimumTier==null ||
            (tool!=null && tool.Tier!=null && tool.Tier.Rank>=MinimumTier.Rank && tool.Kind==RequiredTool);
        public VitalityDefinition Vitality;
        public ItemDefinition DropItem;
        [Min(1)] public int DropCount;
    }
}
