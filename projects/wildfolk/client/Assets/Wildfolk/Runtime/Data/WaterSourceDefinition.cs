using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Water Source")]
    public sealed class WaterSourceDefinition : ScriptableObject
    {
        public WaterContainerDefinition Container;
        [Min(.1f)] public float InteractionRange=3;
        [Min(.1f)] public float CollectionCooldown=1;
    }
}
