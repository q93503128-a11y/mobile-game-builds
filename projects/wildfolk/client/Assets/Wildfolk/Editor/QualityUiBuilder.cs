using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class QualityUiBuilder
    {
        internal static Button Action(Transform parent,string name,string key,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction callback,out Text label)
        {var image=Image(name,parent,"panel",new Vector2(.5f,.5f),position,size,true);label=Text("Label",image.transform,key,Vector2.zero,size-Vector2.one*12,23);var button=Button(image);UnityEventTools.AddPersistentListener(button.onClick,callback);return button;}
        internal static Image Line(Transform parent,Vector2 a,Vector2 b,float thickness=3)
        {var line=Image("Connection",parent,"white",new Vector2(.5f,.5f),(a+b)*.5f,new Vector2(Vector2.Distance(a,b),thickness));line.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);line.color=new Color32(80,99,76,255);return line;}
        internal static PlayerPortrait Portrait(Transform parent,PlayerInventory player,Vector2 position,float scale)
        {
            var root=Rect("Survivor Preview",parent,new Vector2(.5f,.5f),position,new Vector2(230,250));root.localScale=Vector3.one*scale;
            var portrait=root.gameObject.AddComponent<PlayerPortrait>();portrait.Equipment=player.GetComponent<PlayerEquipment>();
            var ground=Image("Backdrop",root,"joystick_base",new Vector2(.5f,.5f),Vector2.zero,new Vector2(248,248));ground.color=new Color32(88,105,75,255);
            Image("Skin",root,"player_body",new Vector2(.5f,.5f),Vector2.zero,new Vector2(150,150));
            portrait.Body=Image("Body",root,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(150,150));
            Image("Left Hand",root,"player_hand",new Vector2(.5f,.5f),new Vector2(-85,-38),new Vector2(36,36));Image("Right Hand",root,"player_hand",new Vector2(.5f,.5f),new Vector2(85,-38),new Vector2(36,36));
            portrait.LeftHand=Image("Left Hand Gear",root,"white",new Vector2(.5f,.5f),new Vector2(-85,-38),new Vector2(34,38));portrait.RightHand=Image("Right Hand Gear",root,"white",new Vector2(.5f,.5f),new Vector2(85,-38),new Vector2(34,38));
            portrait.Head=Image("Head",root,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(150,150));Image("Left Eye",root,"player_eye",new Vector2(.5f,.5f),new Vector2(-18,-28),new Vector2(10,13));Image("Right Eye",root,"player_eye",new Vector2(.5f,.5f),new Vector2(18,-28),new Vector2(10,13));return portrait;
        }
        internal static void Matte(Transform parent,Vector2 size)
        {var background=Image("Matte Surface",parent,"white",new Vector2(.5f,.5f),Vector2.zero,size-Vector2.one*6);background.color=new Color32(24,40,32,247);background.transform.SetAsFirstSibling();}
        internal static Image Surface(Transform safe,string name,Vector2 size,out GameObject view)
        {var scrim=Scrim(name+" Overlay",safe);view=scrim.gameObject;var surface=Image(name,scrim.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,size,true);Matte(surface.transform,size);var layout=surface.gameObject.AddComponent<ModalLayout>();layout.SafeArea=(RectTransform)safe;layout.ReferenceSize=size;layout.TopReserve=CharacterUiBuilder.ProfileBottomReserve;layout.BottomReserve=40;return surface;}
    }
}
