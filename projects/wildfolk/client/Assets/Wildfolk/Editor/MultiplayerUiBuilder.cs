using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;

namespace Wildfolk.Editor
{
    internal static class MultiplayerUiBuilder
    {
        private static readonly Color32 Ink=new Color32(49,63,48,255);
        internal static GameObject Build(Transform safe,HomeScreen home)
        {
            var overlay=Scrim("Multiplayer Overlay",safe);
            var page=Image("Camp Journal",overlay.transform,"lowfire_journal",new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,940),true);
            var ui=overlay.gameObject.AddComponent<MultiplayerMenu>();ui.Home=home;
            ui.SessionPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(MultiplayerSceneBuilder.SessionPrefabPath).GetComponent<NetworkSession>();
            Label(page.transform,"Title","home.multi",new Vector2(30,340),new Vector2(550,56),38);
            var mode=Image("Connection Route",page.transform,"white",new Vector2(.5f,.5f),new Vector2(30,253),new Vector2(530,62),true);mode.color=new Color32(75,85,62,20);
            ui.RouteLabel=Label(mode.transform,"Route","net.internet",Vector2.zero,new Vector2(490,50),24);ui.ModeButton=Button(mode);UnityEventTools.AddPersistentListener(ui.ModeButton.onClick,ui.ToggleRoute);
            ui.HostButton=Action(page.transform,"Host Game","net.host",new Vector2(30,153),new Vector2(530,82),ui.Host,true);
            ui.DestinationLabel=Label(page.transform,"Destination Label","net.room_code",new Vector2(30,57),new Vector2(530,40),22);
            var field=Image("Destination",page.transform,"white",new Vector2(.5f,.5f),new Vector2(-44,-7),new Vector2(380,66),true);field.color=new Color32(232,223,197,255);
            ui.Destination=field.gameObject.AddComponent<InputField>();ui.Destination.targetGraphic=field;ui.Destination.characterLimit=64;
            ui.Destination.textComponent=Label(field.transform,"Value","",Vector2.zero,new Vector2(348,56),25);
            var placeholder=Label(field.transform,"Placeholder","net.code_placeholder",Vector2.zero,new Vector2(348,56),22);placeholder.fontStyle=FontStyle.Italic;placeholder.color=new Color32(100,105,84,255);ui.Destination.placeholder=placeholder;
            ui.JoinButton=Action(page.transform,"Join Game","net.join",new Vector2(235,-7),new Vector2(120,66),ui.Join,false);
            ui.CodeLabel=Label(page.transform,"Session Address","",new Vector2(30,-108),new Vector2(550,74),26);
            ui.EnterButton=Action(page.transform,"Enter Game","net.enter",new Vector2(30,-194),new Vector2(530,72),ui.Enter,true);
            ui.StatusLabel=Label(page.transform,"Session Message","net.intro",new Vector2(30,-283),new Vector2(550,92),21);
            ui.SingleButton=Action(page.transform,"Single Player","home.single",new Vector2(-87,-374),new Vector2(295,62),ui.Single,false);
            ui.BackButton=Action(page.transform,"Back","ui.action.back",new Vector2(210,-374),new Vector2(170,62),ui.Back,false);
            return overlay.gameObject;
        }
        private static Text Label(Transform parent,string name,string key,Vector2 p,Vector2 size,int font)
        {var text=Text(name,parent,key,p,size,font,TextAnchor.MiddleLeft);text.color=Ink;return text;}
        internal static Button Action(Transform parent,string name,string key,Vector2 p,Vector2 size,UnityEngine.Events.UnityAction action,bool primary)
        {
            var image=Image(name,parent,"white",new Vector2(.5f,.5f),p,size,true);
            var label=Label(image.transform,"Label",key,Vector2.zero,size-new Vector2(32,10),26);label.color=primary ? new Color32(248,237,207,255) : Ink;
            var button=Button(image);var colors=button.colors;colors.normalColor=primary ? new Color32(137,88,56,255) : Color.clear;
            colors.highlightedColor=colors.selectedColor=primary ? new Color32(156,106,71,255) : new Color(.28f,.34f,.22f,.055f);
            colors.pressedColor=primary ? new Color32(100,68,46,255) : new Color(.28f,.34f,.22f,.12f);colors.disabledColor=primary ? new Color32(143,140,118,255) : Color.clear;button.colors=colors;
            UnityEventTools.AddPersistentListener(button.onClick,action);return button;
        }
    }
}
