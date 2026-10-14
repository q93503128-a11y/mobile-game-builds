using UnityEngine;
namespace Wildfolk
{
    public enum LiquidState { Empty, Raw, Clean }
    [CreateAssetMenu(menuName="Wildfolk/Water Container")]
    public sealed class WaterContainerDefinition : ScriptableObject
    {
        public string Id;
        public ItemDefinition Empty,Raw,Clean;
    }
}
