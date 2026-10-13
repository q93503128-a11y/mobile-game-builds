using UnityEngine;

namespace Wildfolk
{
    [CreateAssetMenu(menuName = "Wildfolk/Resource")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        public VitalityDefinition Vitality;
        public ItemDefinition DropItem;
        [Min(1)] public int DropCount;
    }
}
