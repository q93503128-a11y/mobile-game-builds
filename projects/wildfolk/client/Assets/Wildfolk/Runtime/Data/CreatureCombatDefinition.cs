using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Creature Combat")]
    public sealed class CreatureCombatDefinition : ScriptableObject
    {
        public int Damage=6;
        public float Speed=2.65f,Detection=30,LoseRange=48,Reach=1.45f,StopDistance=1.25f,Windup=.4f,Swing=.14f,Recovery=1;
        public bool Configured;
    }
}
