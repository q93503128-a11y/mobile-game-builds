using UnityEngine;

namespace Wildfolk
{
    [CreateAssetMenu(menuName = "Wildfolk/Resource")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        public ResourceKind Kind;
        public VitalityDefinition Vitality;
        public ItemDefinition DropItem;
        [Min(1)] public int DropCount;
    }
}
