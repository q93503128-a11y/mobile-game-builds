using UnityEngine;
namespace Wildfolk
{
    [DefaultExecutionOrder(100)]
    public sealed class CharacterVisualLayers : MonoBehaviour
    {
        public Transform BaseSkin,HeadGear,BodyArmor,LeftHandGear,RightHandGear,BackSlot,OptionalFX;
        private CharacterFacing facing;
        private void Awake()=>facing=GetComponent<CharacterFacing>();
        private void LateUpdate(){var rotation=Quaternion.Euler(0,0,Mathf.Atan2(facing.DisplayDirection.y,facing.DisplayDirection.x)*Mathf.Rad2Deg+90);HeadGear.localRotation=BodyArmor.localRotation=rotation;}
        // BackSlot owns a single future attachment. No backpack/cape content is enabled now.
    }
}
