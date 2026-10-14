using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Goblin")]
    public sealed class GoblinDefinition : ScriptableObject
    {
        [Min(.1f)]public float Territory=2.5f,Leash=10,Speed=2.4f,WanderSpeed=.65f,AttackReach=1.5f,Windup=.45f,Cooldown=1.3f;
        [Min(1)]public int Damage=6,Population=3;
        [Min(1)]public float SpawnInterval=35;
        [Range(0,1)]public float EncounterChance=.3f;
    }
}
