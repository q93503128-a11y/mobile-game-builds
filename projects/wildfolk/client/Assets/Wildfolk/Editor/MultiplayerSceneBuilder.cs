using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wildfolk.Presentation;
using static Wildfolk.Editor.GameplayUiFactory;

namespace Wildfolk.Editor
{
    public static class MultiplayerSceneBuilder
    {
        public const string ScenePath="Assets/Wildfolk/Scenes/Multiplayer.unity";
        public const string PlayerPrefabPath="Assets/Wildfolk/Prefabs/NetworkPlayer.prefab";
        public const string SessionPrefabPath="Assets/Wildfolk/Prefabs/NetworkSession.prefab";
        [MenuItem("Wildfolk/Build Multiplayer Foundation")]
        public static void Build()
        {
            if(EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)throw new System.InvalidOperationException("Stop Play Mode and save the scene first.");
            CreatePrefabs();
            var source=Object.FindFirstObjectByType<InputSystemUIInputModule>();var actions=source.actionsAsset;var point=source.point;var click=source.leftClick;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var prefs=new GameObject("Camp Preferences").AddComponent<UserPreferences>();
            var light=new GameObject("Global Light 2D").AddComponent<Light2D>();light.lightType=Light2D.LightType.Global;light.intensity=.88f;
            WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            var ground=WorldArt.Sprite("Woodland Clearing","ground",null,Vector2.zero,1,WorldSorting.Ground);var size=ground.sprite.bounds.size;ground.transform.localScale=new Vector3(72/size.x,44/size.y,1);
            var environment=new GameObject("Quiet Camp").transform;
            foreach(var p in new[]{new Vector2(-11,6),new Vector2(-8,8),new Vector2(6,7),new Vector2(11,5),new Vector2(-13,-5),new Vector2(12,-6)})
            {var tree=WorldArt.Sprite("Tree","tree_oak",environment,p,1.1f,20-Mathf.RoundToInt(p.y*10));tree.gameObject.AddComponent<CircleCollider2D>().radius=.6f;}
            WorldArt.Sprite("Campfire","campfire",environment,new Vector2(0,4),1.2f,0);WorldArt.Sprite("Fallen Log","fallen_log",environment,new Vector2(-3,3),1.1f,0);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=9;camera.transform.position=new Vector3(0,0,-10);camera.backgroundColor=new Color32(71,87,53,255);
            var arena=new GameObject("Cooperative Camp").AddComponent<MultiplayerArena>();
            var nav=arena.gameObject.AddComponent<WorldNavigation>();nav.StaticAreaSize=new Vector2(72,44);nav.Camera=camera;
            arena.Follow=camera.gameObject.AddComponent<CameraFollow>();arena.Follow.Ground=ground;arena.Follow.Settings=AssetDatabase.LoadAssetAtPath<GameplaySettings>("Assets/Wildfolk/Data/GameplaySettings.asset");
            Boundary(new Vector2(0,22),new Vector2(72,1));Boundary(new Vector2(0,-22),new Vector2(72,1));Boundary(new Vector2(-36,0),new Vector2(1,44));Boundary(new Vector2(36,0),new Vector2(1,44));
            var canvas=new GameObject("Camp Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceCamera;canvas.GetComponent<Canvas>().worldCamera=camera;canvas.GetComponent<Canvas>().planeDistance=1;canvas.GetComponent<Canvas>().sortingOrder=WorldSorting.Overlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var safe=Rect("Safe Area",canvas.transform,Vector2.zero,Vector2.zero,Vector2.zero);safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<SafeAreaLayout>();
            arena.CampLabel=Text("Camp",safe,"net.camp",new Vector2(-580,470),new Vector2(650,54),30,TextAnchor.MiddleLeft);
            arena.HealthLabel=Text("Health",safe,"",new Vector2(-580,410),new Vector2(650,44),26,TextAnchor.MiddleLeft);
            var joystick=Image("Joystick",safe,"joystick_base",new Vector2(0,0),new Vector2(190,185),new Vector2(236,236),true);
            arena.Joystick=joystick.gameObject.AddComponent<VirtualJoystick>();arena.Joystick.Thumb=(RectTransform)Image("Thumb",joystick.transform,"joystick_thumb",new Vector2(.5f,.5f),Vector2.zero,new Vector2(95,95)).transform;
            var dash=Image("Dash",safe,"attack_button",new Vector2(1,0),new Vector2(-180,180),new Vector2(174,174),true);Image("Icon",dash.transform,"icon_dash",new Vector2(.5f,.5f),Vector2.zero,new Vector2(86,86));
            arena.DashButton=Button(dash);UnityEventTools.AddPersistentListener(arena.DashButton.onClick,arena.Dash);
            joystick.gameObject.SetActive(false);arena.DashButton.interactable=false;
            var leave=Image("Leave Session",safe,"panel",new Vector2(1,1),new Vector2(-170,-67),new Vector2(250,72),true);Text("Label",leave.transform,"net.leave",Vector2.zero,new Vector2(228,60),26);UnityEventTools.AddPersistentListener(Button(leave).onClick,arena.Leave);
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<EventSystem>().sendNavigationEvents=false;var module=events.GetComponent<InputSystemUIInputModule>();module.UnassignActions();module.actionsAsset=actions;module.point=point;module.leftClick=click;
            EditorSceneManager.SaveScene(scene,ScenePath);FrontendSceneBuilder.Build();AssetDatabase.SaveAssets();
        }
        private static void Boundary(Vector2 p,Vector2 size){var boundary=new GameObject("Camp Boundary");boundary.transform.position=p;boundary.AddComponent<BoxCollider2D>().size=size;}
        public static void CreatePrefabs()
        {
            WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Player.prefab");
            var actor=(GameObject)PrefabUtility.InstantiatePrefab(source);actor.name="NetworkPlayer";
            foreach(var component in new Component[]{actor.GetComponent<PlayerPrimaryAction>(),actor.GetComponent<PlayerInteraction>(),actor.GetComponent<PlayerPlacement>(),actor.GetComponent<PlayerCrafting>(),actor.GetComponent<PlayerStorage>(),actor.GetComponent<PlayerItemDrop>()})Object.DestroyImmediate(component);
            var session=actor.AddComponent<GameplaySession>();session.Mode=SessionMode.Multiplayer;session.PlayerHealth=actor.GetComponent<Health>();
            var commands=actor.GetComponent<PlayerInputCommands>();commands.Session=session;commands.Primary=null;commands.Interaction=null;commands.AllowInventory=commands.AllowPause=false;
            var motor=actor.GetComponent<PlayerMotor>();motor.Session=session;actor.GetComponent<PlayerCombat>().Session=session;actor.GetComponent<PlayerSurvival>().Session=session;actor.GetComponent<PlayerSurvival>().enabled=false;
            actor.GetComponent<PlayerInputAdapter>().enabled=false;motor.enabled=false;motor.Locomotion.enabled=false;
            actor.AddComponent<NetworkObject>();var transform=actor.AddComponent<NetworkTransform>();transform.SyncPositionZ=false;transform.SyncRotAngleX=transform.SyncRotAngleY=transform.SyncRotAngleZ=false;transform.SyncScaleX=transform.SyncScaleY=transform.SyncScaleZ=false;transform.PositionThreshold=.015f;transform.Interpolate=true;transform.UseUnreliableDeltas=true;
            actor.AddComponent<NetworkRigidbody2D>();var player=actor.AddComponent<NetworkPlayer>();player.Catalog=AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/Wildfolk/Data/ContentCatalog.asset");
            var marker=WorldArt.Sprite("Local Survivor","joystick_base",actor.transform,Vector2.zero,1.15f,WorldSorting.GroundDecoration);marker.color=new Color32(186,170,115,130);player.LocalMarker=marker.gameObject;marker.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(actor,PlayerPrefabPath);Object.DestroyImmediate(actor);
            var root=new GameObject("Network Session");var transport=root.AddComponent<UnityTransport>();transport.ConnectTimeoutMS=1000;transport.MaxConnectAttempts=5;transport.DisconnectTimeoutMS=4000;
            var manager=root.AddComponent<NetworkManager>();manager.NetworkConfig=new NetworkConfig{NetworkTransport=transport,TickRate=20,ConnectionApproval=true,EnableSceneManagement=true,ProtocolVersion=1,ClientConnectionBufferTimeout=10};
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var service=root.AddComponent<NetworkSession>();service.Manager=manager;service.PlayerPrefab=prefab;
            PrefabUtility.SaveAsPrefabAsset(root,SessionPrefabPath);Object.DestroyImmediate(root);
        }
    }
}
