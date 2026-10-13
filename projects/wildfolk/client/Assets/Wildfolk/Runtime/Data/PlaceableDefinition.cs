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
    }
}
