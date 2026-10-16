using UnityEngine;

namespace Wildfolk
{
    [CreateAssetMenu(menuName = "Wildfolk/Resource")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        public ResourceKind Kind;
        public EquipmentTier MinimumTier;
        public ToolKind RequiredTool;
        public HarvestMethod HarvestMethod;
        public bool CanHarvest(ToolDefinition tool) => (tool==null ? HarvestMethod==Wildfolk.HarvestMethod.Strike : tool.HarvestMethod==HarvestMethod) &&
            (MinimumTier==null || tool!=null && tool.Tier!=null && tool.Tier.Rank>=MinimumTier.Rank && tool.Kind==RequiredTool);
        public ItemDefinition RequiredToolItem;
        public string Requirement=>HarvestMethod==Wildfolk.HarvestMethod.Panning ? L.Text("feedback.requires_pan") : RequiredToolItem!=null ? L.Format("feedback.requires_tool",RequiredToolItem.LocalizedName) : L.Format("feedback.requires_tool",L.Text("tool."+RequiredTool));
        public VitalityDefinition Vitality;
        public ItemDefinition DropItem;
        [Min(1)] public int DropCount;
    }
}
