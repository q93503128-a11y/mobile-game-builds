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
        {var t=Text(name,parent,key,p,size,font,anchor);t.color=JournalStyle.Ink;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
        internal static Image Rule(Transform parent,Vector2 from,Vector2 to,float width=2)
        {var i=QualityUiBuilder.Line(parent,from,to,width);i.color=new Color32(136,124,91,180);return i;}
        internal static Button Action(Transform parent,string name,string key,Vector2 p,Vector2 size,UnityEngine.Events.UnityAction action,out Text label,bool primary=false)
        {
            var image=Image(name,parent,"white",Vector2.one*.5f,p,size,true);var b=Button(image);
            label=Label(image.transform,"Label",key,Vector2.zero,size-new Vector2(28,10),26,TextAnchor.MiddleCenter);
            label.color=primary?JournalStyle.Paper:JournalStyle.Ink;var c=b.colors;c.normalColor=primary?JournalStyle.Ember:new Color32(119,108,77,25);c.highlightedColor=c.selectedColor=primary?new Color32(156,106,71,255):new Color32(119,108,77,50);c.pressedColor=new Color32(110,91,61,180);c.disabledColor=primary?new Color32(146,139,113,255):new Color32(119,108,77,12);b.colors=c;
            Rule(image.transform,new Vector2(-size.x*.5f,-size.y*.5f+1),new Vector2(size.x*.5f,-size.y*.5f+1));
            if(action!=null)UnityEventTools.AddPersistentListener(b.onClick,action);return b;
        }
        internal static JournalSkillNode Node(Transform parent,string name,Vector2 p,Sprite icon,UnityEngine.Events.UnityAction<int> action,int index,out Button button)
        {
            var frame=Image(name,parent,"journal_node",Vector2.one*.5f,p,new Vector2(88,88),true);button=Button(frame);button.transition=Selectable.Transition.None;
            UnityEventTools.AddIntPersistentListener(button.onClick,action,index);var n=frame.gameObject.AddComponent<JournalSkillNode>();n.Frame=frame;
            n.Focus=Image("Selected Ring",frame.transform,"journal_focus",Vector2.one*.5f,Vector2.zero,new Vector2(104,104)).gameObject;
            n.Icon=Image("Icon",frame.transform,"white",Vector2.one*.5f,Vector2.zero,new Vector2(54,54));n.Icon.sprite=icon;n.Icon.preserveAspect=true;
            n.Learned=Image("Learned Seal",frame.transform,"journal_seal",Vector2.one*.5f,new Vector2(34,-28),new Vector2(31,31)).gameObject;
            n.Locked=Image("Lock",frame.transform,"journal_lock",Vector2.one*.5f,new Vector2(34,-28),new Vector2(31,31)).gameObject;return n;
        }
        internal static void Detail(Transform parent,Vector2 p,Vector2 size)
        {Rule(parent,p+new Vector2(-size.x*.5f,size.y*.5f),p+size*.5f);}
    }
}
