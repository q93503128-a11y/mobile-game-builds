using UnityEngine;

namespace Wildfolk
{
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
        public bool IsBarrier;
        public bool IsDoor;
        [Min(0)] public int StorageSlots;
        public string StationTitle;
        [Min(1)]public int ParallelCapacity=1;
        [Min(.1f)]public float ProcessingSpeed=1;
        [Min(1)]public int WorkbenchTier=1;
    }
}
