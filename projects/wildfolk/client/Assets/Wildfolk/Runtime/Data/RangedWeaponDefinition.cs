using UnityEngine;
namespace Wildfolk
{
    public enum AmmoKind { None,Arrow,Bolt }
    [CreateAssetMenu(menuName="Wildfolk/Ranged Weapon")]
    public sealed class RangedWeaponDefinition : ScriptableObject
    {
        public AmmoKind Ammunition;
        public float DrawSeconds=.35f,RecoverySeconds=.4f,Speed=18,Range=14;
        public int Damage=12;
        public bool Crossbow;
    }
}
