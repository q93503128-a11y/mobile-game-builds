using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Goblin Camp")]
    public sealed class GoblinCampDefinition : ScriptableObject
    {
        public GoblinCamp Prefab;
        public int InitialCamps=1,Residents=3,HardCap=24;
        public float RefillSeconds=480,PatrolRadius=4,PlayerDistance=18,BaseDistance=12;
    }
}
