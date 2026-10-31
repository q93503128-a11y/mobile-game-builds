using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class MultiplayerUiBuilder
    {
        internal static GameObject Build(Transform safe,HomeScreen home)
        {
            var page=JournalUiBuilder.Sheet(safe,"Camp Journal",out var view);var ui=view.AddComponent<MultiplayerMenu>();ui.Home=home;
            ui.SessionPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(MultiplayerSceneBuilder.SessionPrefabPath).GetComponent<NetworkSession>();
            JournalUiBuilder.Label(page,"Title","home.multi",new Vector2(-440,400),new Vector2(630,80),44);
            ui.BackButton=JournalUiBuilder.Action(page,"Back","ui.action.back",new Vector2(690,400),new Vector2(180,120),ui.Back,out _);
            ui.ModeButton=JournalUiBuilder.Action(page,"Connection Route","",new Vector2(50,290),new Vector2(1400,120),ui.ToggleRoute,out ui.RouteLabel);
            var setup=Rect("Create or Join",page,Vector2.one*.5f,Vector2.zero,new Vector2(1720,1000));ui.SetupView=setup.gameObject;
            JournalUiBuilder.Label(setup,"Create heading","net.create_title",new Vector2(-410,145),new Vector2(650,80),40);
            JournalUiBuilder.Label(setup,"Create hint","net.create_hint",new Vector2(-410,30),new Vector2(650,130),32);
            ui.HostButton=JournalUiBuilder.Action(setup,"Host Game","net.host",new Vector2(-410,-130),new Vector2(650,132),ui.Host,out _,true);
            JournalUiBuilder.Rule(setup,new Vector2(0,180),new Vector2(0,-200));
            ui.DestinationLabel=JournalUiBuilder.Label(setup,"Destination Label","net.room_code",new Vector2(410,145),new Vector2(650,70),36);
            var field=Image("Destination",setup,"white",Vector2.one*.5f,new Vector2(410,25),new Vector2(650,120),true);field.color=new Color32(240,230,201,255);
            ui.Destination=field.gameObject.AddComponent<InputField>();ui.Destination.targetGraphic=field;ui.Destination.characterLimit=64;
            ui.Destination.textComponent=JournalUiBuilder.Label(field.transform,"Value","",Vector2.zero,new Vector2(600,95),36);
            var placeholder=JournalUiBuilder.Label(field.transform,"Placeholder","net.code_placeholder",Vector2.zero,new Vector2(600,95),32);placeholder.color=JournalStyle.Muted;ui.Destination.placeholder=placeholder;
            ui.JoinButton=JournalUiBuilder.Action(setup,"Join Game","net.join",new Vector2(410,-130),new Vector2(650,132),ui.Join,out _,true);
            var room=Rect("Room",page,Vector2.one*.5f,Vector2.zero,new Vector2(1720,1000));ui.RoomView=room.gameObject;
            ui.CodeLabel=JournalUiBuilder.Label(room,"Session Address","",new Vector2(0,130),new Vector2(1400,110),48,TextAnchor.MiddleCenter);
            ui.PlayersLabel=JournalUiBuilder.Label(room,"Players","",new Vector2(0,20),new Vector2(1400,80),36,TextAnchor.MiddleCenter);
            ui.CopyCodeButton=JournalUiBuilder.Action(room,"Copy code","net.copy_code",new Vector2(-360,-130),new Vector2(620,126),ui.CopyCode,out _);
            ui.EnterButton=JournalUiBuilder.Action(room,"Enter Game","net.enter",new Vector2(360,-130),new Vector2(620,126),ui.Enter,out _,true);
            ui.StatusLabel=JournalUiBuilder.Label(page,"Session Message","net.intro",new Vector2(0,-270),new Vector2(1400,115),32,TextAnchor.MiddleCenter);
            ui.SingleButton=JournalUiBuilder.Action(page,"Single Player","home.single",new Vector2(0,-405),new Vector2(650,120),ui.Single,out _);
            return view;
        }
    }
}
