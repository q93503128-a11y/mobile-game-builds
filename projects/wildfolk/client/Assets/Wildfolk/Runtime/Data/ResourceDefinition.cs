using UnityEngine;

namespace Wildfolk
{
    [CreateAssetMenu(menuName = "Wildfolk/Resource")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        public ResourceKind Kind;
        public HarvestDefinition Harvest=new HarvestDefinition();
        public EquipmentTier MinimumTier {get=>Harvest.MinimumTier;set=>Harvest.MinimumTier=value;}
        public ToolKind RequiredTool {get=>Harvest.RequiredTool;set=>Harvest.RequiredTool=value;}
        public HarvestMethod HarvestMethod {get=>Harvest.Method;set=>Harvest.Method=value;}
        public ItemDefinition RequiredToolItem {get=>Harvest.RequiredToolItem;set=>Harvest.RequiredToolItem=value;}
        public bool CanHarvest(ToolDefinition tool)=>Harvest.Allows(tool);
        public string Requirement=>HarvestMethod==Wildfolk.HarvestMethod.Panning ? L.Text("feedback.requires_pan") : RequiredToolItem!=null ? L.Format("feedback.requires_tool",RequiredToolItem.LocalizedName) : L.Format("feedback.requires_tool",L.Text("tool."+RequiredTool));
        public VitalityDefinition Vitality;
        public ItemDefinition DropItem;
        [Min(1)] public int DropCount;
    }
}
