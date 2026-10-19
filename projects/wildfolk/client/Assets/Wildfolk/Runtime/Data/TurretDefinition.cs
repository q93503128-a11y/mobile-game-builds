using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Turret")]
    public sealed class TurretDefinition : ScriptableObject
    {
        public ItemDefinition Ammunition;
        public StoneProjectile Projectile;
        public int Damage;
        public float Range,ProjectileSpeed,ReloadSeconds,RotationSpeed;
        public Sprite HeadSprite;
    }
}
