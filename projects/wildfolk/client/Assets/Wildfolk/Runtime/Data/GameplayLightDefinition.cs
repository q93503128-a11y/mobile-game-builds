using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Gameplay Light")]
    public sealed class GameplayLightDefinition : ScriptableObject
    {
        public Color Color=new Color(1,.68f,.32f);
        public bool DetersEnemies=true;
        [Min(.1f)]public float Radius=5;
        [Range(0,1)]public float Strength=1;
        [Range(0,1)]public float SafeThreshold=.65f;
        [Min(0)]public float PursuitGrace=2;
    }
}
