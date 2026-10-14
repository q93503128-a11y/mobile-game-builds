using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class PlayerPortrait : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public Image Head,Body,LeftHand,RightHand;
        private void OnEnable(){Equipment.Changed+=Refresh;Refresh();}
        private void OnDisable(){if(Equipment!=null)Equipment.Changed-=Refresh;}
        public void Refresh()
        {Show(Head,Equipment.HeadArmor==null ? null : Equipment.HeadArmor.Armor.WornSprite);Show(Body,Equipment.BodyArmor==null ? null : Equipment.BodyArmor.Armor.WornSprite);var hand=Equipment.BodyArmor==null ? null : Equipment.BodyArmor.Armor.HandSprite;Show(LeftHand,hand);Show(RightHand,hand);}
        private static void Show(Image image,Sprite sprite){image.sprite=sprite;image.enabled=sprite!=null;}
    }
}
