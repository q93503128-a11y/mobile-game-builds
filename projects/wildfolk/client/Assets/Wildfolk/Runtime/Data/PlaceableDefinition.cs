using UnityEngine;

namespace Wildfolk
{
    public enum ProcessingMode { Batch, Sequential }
    [CreateAssetMenu(menuName = "Wildfolk/Placeable")]
    public sealed class PlaceableDefinition : ScriptableObject
    {
        public ItemDefinition Item;
        public WorldPlaceable Prefab;
        public VitalityDefinition Vitality;
        public Vector2 Footprint;
        [Min(.1f)] public float MaximumDistance;
        public StationKind Station;
        public EquipmentTier Tier;
        public bool IsBeacon;
        public TurretDefinition Turret;
        public bool IsBarrier;
        public bool IsDoor;
        [Min(0)] public int StorageSlots;
        [Min(1)]public int ParallelCapacity=1;
        public ProcessingMode ProcessingMode;
        [Min(1)]public int InputCapacity=50,OutputCapacity=100;
        [Min(.1f)]public float ProcessingSpeed=1;
        [Range(1,6)]public int WorkbenchTier=1;
        public Sprite TierVisual;
        public string TierDisplayKey;
        public PlaceableDefinition UpgradeTo;
        public ItemAmount[] UpgradeCosts=System.Array.Empty<ItemAmount>();
    }
}
