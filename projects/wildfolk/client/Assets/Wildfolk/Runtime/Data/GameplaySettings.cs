using UnityEngine;

namespace Wildfolk
{
    [CreateAssetMenu(menuName = "Wildfolk/Gameplay Settings")]
    public sealed class GameplaySettings : ScriptableObject
    {
        [Min(.1f)] public float MoveSpeed;
        [Min(.01f)] public float CameraSmoothTime;
        [Min(1)] public int PunchDamage;
        [Min(.1f)] public float PunchReach;
        [Min(.01f)] public float PunchDuration;
        [Min(.01f)] public float PunchCooldown;
        [Min(.1f)] public float PunchExtension;
        [Min(.1f)] public float EnemySpeed;
        [Min(.1f)] public float EnemyDetectionRange;
        [Min(.1f)] public float EnemyLoseRange;
        [Min(.1f)] public float EnemyStopDistance;
        [Min(.1f)] public float EnemySeparation;
        [Min(.1f)] public float EnemyAttackReach;
        [Min(1)] public int EnemyDamage;
        [Min(.01f)] public float EnemyWindup;
        [Min(.01f)] public float EnemyCooldown;
        [Min(1)] public int InventorySlots;
        [Min(.1f)] public float PickupRadius;
        [Min(.01f)] public float DropScatter;
        [Min(.01f)] public float DropTravelTime;
        [Min(.01f)] public float DropPickupDelay;
    }
}
