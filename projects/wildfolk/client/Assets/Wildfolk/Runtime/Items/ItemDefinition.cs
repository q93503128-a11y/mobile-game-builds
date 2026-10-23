using UnityEngine;

namespace Wildfolk
{
    public enum ItemCategory { Material, Tool, Placeable, Weapon, Consumable, Armor, Container, Accessory }

    [CreateAssetMenu(menuName = "Wildfolk/Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        public ExplorationToolCategory ExplorationCategory;
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
        public AccessoryDefinition Accessory;
        public bool Equippable=>Armor!=null || EquippedLight!=null || Accessory!=null;
        public WaterContainerDefinition Container;
        public GameplayLightDefinition PortableLight;
        public Sprite HeldSprite;
        public bool NavigationInstrument;
        public int WaterCapacity;
        public ConsumableDefinition RawDrink,CleanDrink;
        public bool FishingRod;
        public bool Telescope;
        public ExplorationUtilityDefinition ExplorationUtility;
        public TurretModuleDefinition TurretModule;
        public GameplayLightDefinition EquippedLight;
        public RangedWeaponDefinition Ranged;
        public AmmoKind Ammo;
        public int AmmoDamage;
        public LiquidState Liquid;
        public EquipmentTier Tier => Tool!=null ? Tool.Tier : Placement!=null ? Placement.Tier : Armor!=null ? Armor.Tier : null;
    }
}
