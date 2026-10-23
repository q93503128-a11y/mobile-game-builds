using UnityEngine;
namespace Wildfolk
{
    public enum AccessoryAnchor { AboveHead,BodySide,Waist,AroundActor }
    [CreateAssetMenu(menuName="Wildfolk/Accessory")]
    public sealed class AccessoryDefinition:ScriptableObject
    {
        public AccessoryAnchor Anchor;
        public Sprite Visual;
        public Vector2 Offset=new Vector2(0,1.25f);
        public float VisualScale=.4f,Threshold=.35f,MovementBonus=.08f,DamageReduction=.10f;
        public float FloatAmplitude=.025f,Follow=16;
        public bool Active(Health health)=>health!=null && health.IsAlive && health.Ratio<=Threshold;
    }
}
