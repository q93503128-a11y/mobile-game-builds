using UnityEngine;
namespace Wildfolk
{
    public enum ExplorationUtilityKind { Survey,Signal }
    [CreateAssetMenu(menuName="Wildfolk/Exploration Utility")]
    public sealed class ExplorationUtilityDefinition:ScriptableObject
    {
        public ExplorationUtilityKind Kind;
        [Min(1)]public float Range=18;
        [Min(0)]public float ChannelSeconds=1.2f;
        [Min(1)]public float CooldownSeconds=12,LifetimeSeconds=6;
    }
}
