using UnityEngine;
namespace Wildfolk
{
    public enum WildlifeMovement { Walk, Flight, Hop }
    public enum WildlifeAttack { None, Bite, Swoop, Ambush, Charge, Pulse }
    public enum WildlifePhase : byte { Idle, Warning, Windup, Action, Recovery }
    [CreateAssetMenu(menuName="Wildfolk/Wildlife Action")]
    public sealed class WildlifeActionDefinition : ScriptableObject
    {
        public WildlifeMovement Movement;
        public WildlifeAttack Attack;
        public bool Camouflage,Defensive;
        public float Detection=6,Leash=12,WalkSpeed=1.2f,ChaseSpeed=3,ActionSpeed=5;
        public float WarningSeconds=.5f,WindupSeconds=.5f,ActionSeconds=.3f,RecoverySeconds=1.2f;
        public float Reach=1.5f,EngageRange=2.2f,FleeSpeed=4;
        public int Damage=6;
    }
}
