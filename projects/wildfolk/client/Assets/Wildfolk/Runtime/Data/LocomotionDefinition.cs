using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Locomotion")]
    public sealed class LocomotionDefinition : ScriptableObject
    {
        [Min(1)]public float MaximumStamina=100;
        [Min(0)]public float Recovery=16,RecoveryDelay=1.2f,SprintDrain=14,DashCost=24;
        [Range(0,1)]public float SprintEnter=.9f,SprintMaintain=.72f;
        [Min(0)]public float SprintHold=.4f;
        [Min(1)]public float SprintSpeed=1.55f;
        [Min(.01f)]public float DashDistance=2.6f,DashDuration=.18f,DashCooldown=.55f;
        [Min(0)]public float ExhaustedRestart=20,FullHideDelay=.7f;
        [Range(0,1)]public float PoorRecoveryMultiplier=.45f;
    }
}
