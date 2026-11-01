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
        internal static Button IconAction(Transform parent,string name,string key,Vector2 p,Vector2 size,UnityEngine.Events.UnityAction action,Sprite icon,out Text label,bool primary=false)
        {
            var button=Action(parent,name,key,p,size,action,out label,primary);IconLayout(button,icon);return button;
        }
        internal static void IconLayout(Button button,Sprite sprite,float extent=64)
        {
            var size=((RectTransform)button.transform).sizeDelta;
            var previous=button.transform.Find("Action Icon");var icon=previous==null?Image("Action Icon",button.transform,"white",Vector2.one*.5f,Vector2.zero,Vector2.one):previous.GetComponent<Image>();icon.rectTransform.anchoredPosition=new Vector2(-size.x*.5f+32+extent*.5f,0);icon.rectTransform.sizeDelta=new Vector2(extent,extent);icon.sprite=sprite;icon.preserveAspect=true;icon.color=JournalStyle.Ink;
            var label=button.GetComponentInChildren<Text>(true);label.alignment=TextAnchor.MiddleLeft;label.rectTransform.anchoredPosition=new Vector2((extent+32)*.5f,0);label.rectTransform.sizeDelta=new Vector2(size.x-extent-80,size.y-16);
        }
        internal static Button RowAction(Transform parent,string name,string key,Vector2 p,Vector2 size,UnityEngine.Events.UnityAction action,string icon,out Text label)
        {
            var button=IconAction(parent,name,key,p,size,action,VisualArtImporter.Sprite(icon),out label);ClearSurface(button);
            var arrow=Image("Open",button.transform,"ui_next",Vector2.one*.5f,new Vector2(size.x*.5f-28,0),new Vector2(32,32));arrow.color=JournalStyle.Muted;
            return button;
        }
        internal static Transform CampNavigation(HomeScreen home)
        {
            var existing=home.Menu.transform.Find("Camp Records");if(existing!=null)return existing;
            var plate=Image("Camp Records",home.Menu.transform,"panel",Vector2.one*.5f,new Vector2(-400,-360),new Vector2(960,180));return plate.transform;
        }
        internal static Button CampEntry(HomeScreen home,string name,string key,float x,UnityEngine.Events.UnityAction action,string icon,out Text label)
        {
            var parent=CampNavigation(home);var button=Action(parent,name,key,new Vector2(x,0),new Vector2(300,160),action,out label);ClearSurface(button);
            var rule=button.transform.Find("Connection");if(rule!=null)Object.DestroyImmediate(rule.gameObject);
            var art=Image("Navigation Symbol",button.transform,icon,Vector2.one*.5f,new Vector2(0,29),new Vector2(64,64));art.preserveAspect=true;art.color=icon=="journal_gem"?Color.white:JournalStyle.Paper;
            label.rectTransform.anchoredPosition=new Vector2(0,-45);label.rectTransform.sizeDelta=new Vector2(292,62);label.color=JournalStyle.Paper;label.fontSize=32;return button;
        }
        private static void ClearSurface(Button button)
        {var colors=button.colors;colors.normalColor=colors.disabledColor=Color.clear;colors.highlightedColor=colors.selectedColor=new Color(1,1,1,.08f);colors.pressedColor=new Color(0,0,0,.12f);button.colors=colors;}
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
