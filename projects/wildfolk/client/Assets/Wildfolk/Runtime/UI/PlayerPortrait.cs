using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class PlayerPortrait : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public Image Head,Body,LeftHand,RightHand,Utility,Accessory;
        private void OnEnable(){if(Equipment!=null){Equipment.Changed+=Refresh;Refresh();}}
        private void OnDisable(){if(Equipment!=null)Equipment.Changed-=Refresh;}
        public void Refresh()
        {
            if(Equipment==null)return;
            Show(Head,Equipment.HeadArmor==null ? null : Equipment.HeadArmor.Armor.WornSprite);
            Show(Body,Equipment.BodyArmor==null ? null : Equipment.BodyArmor.Armor.WornSprite);
            var hand=Equipment.BodyArmor==null ? null : Equipment.BodyArmor.Armor.HandSprite;
            Show(LeftHand,hand);Show(RightHand,hand);
            var scale=WorldToUiScale();
            RefreshUtility(scale);
            RefreshAccessory(scale);
        }
        private float WorldToUiScale()
        {
            var avatar=Equipment.GetComponent<PlayerAvatar>();
            var body=avatar==null||avatar.Body==null ? null : avatar.Body.Find("Body")?.GetComponent<SpriteRenderer>();
            if(body==null||body.sprite==null||Body==null)return 1f;
            var width=body.sprite.bounds.size.x*body.transform.localScale.x;
            return width>.001f ? Body.rectTransform.rect.width/width : 1f;
        }
        private void RefreshUtility(float scale)
        {
            if(Utility==null)return;
            var source=Equipment.UtilityVisual;
            var sprite=Equipment.Utility==null||source==null||!source.enabled ? null : source.sprite;
            Show(Utility,sprite);
            if(sprite==null)return;
            Utility.rectTransform.anchoredPosition=Equipment.transform.InverseTransformPoint(source.transform.position)*scale;
            var rootScale=Equipment.transform.lossyScale;
            var size=new Vector2(source.bounds.size.x/Mathf.Max(Mathf.Abs(rootScale.x),.001f),
                source.bounds.size.y/Mathf.Max(Mathf.Abs(rootScale.y),.001f));
            Utility.rectTransform.sizeDelta=size*scale;
        }
        private void RefreshAccessory(float scale)
        {
            if(Accessory==null)return;
            var definition=Equipment.Accessory==null ? null : Equipment.Accessory.Accessory;
            var sprite=definition==null ? null : definition.Visual;
            Show(Accessory,sprite);
            if(sprite==null)return;
            var rootScale=Equipment.transform.lossyScale;
            var localSize=new Vector2(sprite.bounds.size.x*definition.VisualScale/Mathf.Max(Mathf.Abs(rootScale.x),.001f),
                sprite.bounds.size.y*definition.VisualScale/Mathf.Max(Mathf.Abs(rootScale.y),.001f));
            Accessory.rectTransform.anchoredPosition=AccessoryVisual.AnchorOffset(definition)*scale;
            Accessory.rectTransform.sizeDelta=localSize*scale;
        }
        private static void Show(Image image,Sprite sprite){if(image==null)return;image.sprite=sprite;image.enabled=sprite!=null;}
    }
}
