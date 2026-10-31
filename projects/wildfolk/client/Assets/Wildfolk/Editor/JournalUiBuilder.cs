using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    // Small authoring vocabulary for existing scenes. Authored UI is serialized, not rebuilt at runtime.
    internal static class JournalUiBuilder
    {
        internal static Transform Book(Transform safe,string name,out GameObject view)
        {
            var overlay=Scrim(name+" Overlay",safe);overlay.color=new Color(.05f,.07f,.04f,.48f);view=overlay.gameObject;
            var spread=Rect(name,overlay.transform,Vector2.one*.5f,Vector2.zero,new Vector2(1720,1000));
            var fit=spread.gameObject.AddComponent<ModalLayout>();fit.SafeArea=(RectTransform)safe;fit.ReferenceSize=spread.sizeDelta;fit.HorizontalReserve=40;fit.TopReserve=fit.BottomReserve=30;
            var spine=Image("Cloth Spine",spread,"white",Vector2.one*.5f,Vector2.zero,new Vector2(95,950),true);spine.color=new Color32(100,87,58,255);
            var left=Image("Left Page",spread,"lowfire_journal",Vector2.one*.5f,new Vector2(-420,0),new Vector2(850,980),true);left.transform.localScale=new Vector3(-1,1,1);
            Image("Right Page",spread,"lowfire_journal",Vector2.one*.5f,new Vector2(420,0),new Vector2(850,980),true);
            return spread;
        }
        internal static Transform Sheet(Transform safe,string name,out GameObject view)
        {
            var overlay=Scrim(name+" Overlay",safe);overlay.color=new Color(.05f,.07f,.04f,.48f);view=overlay.gameObject;
            var sheet=Rect(name,overlay.transform,Vector2.one*.5f,Vector2.zero,new Vector2(1720,1000));
            var fit=sheet.gameObject.AddComponent<ModalLayout>();fit.SafeArea=(RectTransform)safe;fit.ReferenceSize=sheet.sizeDelta;fit.HorizontalReserve=40;fit.TopReserve=fit.BottomReserve=20;
            Image("Field paper",sheet,"lowfire_journal",Vector2.one*.5f,Vector2.zero,new Vector2(1720,980),true);
            return sheet;
        }
        internal static Text Label(Transform parent,string name,string key,Vector2 p,Vector2 size,int font=26,TextAnchor anchor=TextAnchor.MiddleLeft)
        {if(name=="Title")font=Mathf.Max(44,font);size.y=Mathf.Max(size.y,font*1.6f);var t=Text(name,parent,key,p,size,font,anchor);t.color=JournalStyle.Ink;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
        internal static Image Rule(Transform parent,Vector2 from,Vector2 to,float width=2)
        {var i=QualityUiBuilder.Line(parent,from,to,width);i.color=new Color32(136,124,91,180);return i;}
        internal static Button Action(Transform parent,string name,string key,Vector2 p,Vector2 size,UnityEngine.Events.UnityAction action,out Text label,bool primary=false)
        {
            var image=Image(name,parent,"white",Vector2.one*.5f,p,size,true);var b=Button(image);
            label=Label(image.transform,"Label",key,Vector2.zero,size-new Vector2(32,16),32,TextAnchor.MiddleCenter);
            JournalStyle.Action(b,primary);
            Rule(image.transform,new Vector2(-size.x*.5f,-size.y*.5f+1),new Vector2(size.x*.5f,-size.y*.5f+1));
            if(action!=null)UnityEventTools.AddPersistentListener(b.onClick,action);return b;
        }
        internal static JournalSkillNode Node(Transform parent,string name,Vector2 p,Sprite icon,UnityEngine.Events.UnityAction<int> action,int index,out Button button)
        {
            var frame=Image(name,parent,"journal_node",Vector2.one*.5f,p,new Vector2(120,120),true);button=Button(frame);button.transition=Selectable.Transition.None;
            UnityEventTools.AddIntPersistentListener(button.onClick,action,index);var n=frame.gameObject.AddComponent<JournalSkillNode>();n.Frame=frame;
            n.Focus=Image("Selected Ring",frame.transform,"journal_focus",Vector2.one*.5f,Vector2.zero,new Vector2(136,136)).gameObject;
            n.Icon=Image("Icon",frame.transform,"white",Vector2.one*.5f,Vector2.zero,new Vector2(54,54));n.Icon.sprite=icon;n.Icon.preserveAspect=true;
            n.Learned=Image("Learned Seal",frame.transform,"journal_seal",Vector2.one*.5f,new Vector2(34,-28),new Vector2(31,31)).gameObject;
            n.Locked=Image("Lock",frame.transform,"journal_lock",Vector2.one*.5f,new Vector2(34,-28),new Vector2(31,31)).gameObject;return n;
        }
        internal static Button Link(Transform parent,string name,string key,Vector2 p,Vector2 size,UnityEngine.Events.UnityAction action,out Text label)
        {
            var image=Image(name,parent,"white",Vector2.one*.5f,p,size,true);var button=Button(image);var colors=button.colors;colors.normalColor=colors.disabledColor=Color.clear;colors.highlightedColor=colors.selectedColor=new Color(0,0,0,.04f);colors.pressedColor=new Color(0,0,0,.09f);button.colors=colors;
            label=Label(image.transform,"Label",key,Vector2.zero,size-new Vector2(24,16),30,TextAnchor.MiddleCenter);Rule(image.transform,new Vector2(-size.x*.32f,-38),new Vector2(size.x*.32f,-38));if(action!=null)UnityEventTools.AddPersistentListener(button.onClick,action);return button;
        }
        internal static void Detail(Transform parent,Vector2 p,Vector2 size)
        {Rule(parent,p+new Vector2(-size.x*.5f,size.y*.5f),p+size*.5f);}
    }
}
