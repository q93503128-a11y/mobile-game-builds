using UnityEngine;
namespace Wildfolk
{
    public enum TurretModuleSlot { Loader,Aim,Ammo,Rotation,Frame }
    [CreateAssetMenu(menuName="Wildfolk/Turret Module")]
    public sealed class TurretModuleDefinition : ScriptableObject
    {
        public TurretModuleSlot Slot;
        public float Multiplier=1;
        public int ExtraAmmoSlots;
    }
}
