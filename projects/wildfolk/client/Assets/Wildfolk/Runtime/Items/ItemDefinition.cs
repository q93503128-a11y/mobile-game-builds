using UnityEngine;

namespace Wildfolk
{
    public enum ItemCategory { Material, Tool, Placeable, Weapon, Consumable, Armor, Container }

    [CreateAssetMenu(menuName = "Wildfolk/Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public string LocalizedName=>L.Text("item."+Id+".name");
        public Sprite Icon;
        [Min(1)] public int MaxStack;
        public ItemCategory Category;
        public ToolDefinition Tool;
        public PlaceableDefinition Placement;
        public ConsumableDefinition Consumable;
        public ArmorDefinition Armor;
        public WaterContainerDefinition Container;
        public GameplayLightDefinition PortableLight;
        public Sprite HeldSprite;
        public bool NavigationInstrument;
        public LiquidState Liquid;
        public EquipmentTier Tier => Tool!=null ? Tool.Tier : Placement!=null ? Placement.Tier : Armor!=null ? Armor.Tier : null;
    }
}
