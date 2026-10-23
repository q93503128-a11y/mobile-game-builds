using UnityEngine;
namespace Wildfolk
{
    public sealed class AccessoryVisual:MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public SpriteRenderer Renderer;
        private ItemDefinition previous;
        private bool initialized;private Vector3 worldPosition;
        private void LateUpdate()
        {
            var item=Equipment.Accessory;var d=item==null?null:item.Accessory;
            if(item!=previous){previous=item;Renderer.sprite=d==null?null:d.Visual;Renderer.enabled=d!=null;initialized=false;}
            if(d==null)return;
            var anchor=d.Anchor==AccessoryAnchor.BodySide?new Vector2(.7f,.15f):d.Anchor==AccessoryAnchor.Waist?new Vector2(.5f,-.3f):d.Anchor==AccessoryAnchor.AroundActor?Vector2.zero:d.Offset;
            var target=Equipment.transform.position+(Vector3)anchor+Vector3.up*(Mathf.Sin(Time.time*2.4f)*d.FloatAmplitude);
            worldPosition=initialized?Vector3.Lerp(worldPosition,target,1-Mathf.Exp(-d.Follow*Time.deltaTime)):target;
            worldPosition=target+Vector3.ClampMagnitude(worldPosition-target,.16f);Renderer.transform.position=worldPosition;
            Renderer.transform.localScale=Vector3.one*d.VisualScale;initialized=true;
        }
    }
}
