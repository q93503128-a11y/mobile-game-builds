using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Animal Movement")]
    public sealed class AnimalMovementDefinition : ScriptableObject
    {
        [Min(.1f)] public float WanderSpeed=.55f;
        [Min(.1f)] public float FleeSpeed=2.4f;
        [Min(.1f)] public float DangerRange=2.4f;
        [Min(.1f)] public float FleeSeconds=2.2f;
        [Min(.1f)] public float WanderInterval=4;
        [Min(.1f)] public float HomeRadius=4;
    }
}
