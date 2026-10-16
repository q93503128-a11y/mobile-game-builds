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
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=9;camera.transform.position=new Vector3(0,0,-10);camera.backgroundColor=new Color32(71,87,53,255);
            var arena=new GameObject("Cooperative Camp").AddComponent<MultiplayerArena>();
            var ecology=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(NetworkEcologyBuilder.PrefabPath));ecology.SetActive(true);
            var world=ecology.GetComponent<NetworkCreatureWorld>();var nav=ecology.GetComponent<WorldNavigation>();nav.Camera=camera;world.Goblins.Camera=world.Night.Camera=camera;
            foreach(var component in ecology.GetComponentsInChildren<Component>(true))PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            arena.Follow=camera.gameObject.AddComponent<CameraFollow>();arena.Follow.Ground=world.World.Terrain.Ground;arena.Follow.Settings=AssetDatabase.LoadAssetAtPath<GameplaySettings>("Assets/Wildfolk/Data/GameplaySettings.asset");
            var canvas=new GameObject("Camp Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceCamera;canvas.GetComponent<Canvas>().worldCamera=camera;canvas.GetComponent<Canvas>().planeDistance=1;canvas.GetComponent<Canvas>().sortingOrder=WorldSorting.Overlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var safe=Rect("Safe Area",canvas.transform,Vector2.zero,Vector2.zero,Vector2.zero);safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<SafeAreaLayout>();
            arena.CampLabel=Text("Camp",safe,"net.camp",new Vector2(-580,470),new Vector2(650,54),30,TextAnchor.MiddleLeft);
            arena.HealthLabel=Text("Health",safe,"",new Vector2(-580,410),new Vector2(650,44),26,TextAnchor.MiddleLeft);
            var joystick=Image("Joystick",safe,"joystick_base",new Vector2(0,0),new Vector2(190,185),new Vector2(236,236),true);
            arena.Joystick=joystick.gameObject.AddComponent<VirtualJoystick>();arena.Joystick.Thumb=(RectTransform)Image("Thumb",joystick.transform,"joystick_thumb",new Vector2(.5f,.5f),Vector2.zero,new Vector2(95,95)).transform;
            var dash=Image("Dash",safe,"attack_button",new Vector2(1,0),new Vector2(-340,280),new Vector2(130,130),true);Image("Icon",dash.transform,"icon_dash",new Vector2(.5f,.5f),Vector2.zero,new Vector2(66,66));
            arena.DashButton=Button(dash);UnityEventTools.AddPersistentListener(arena.DashButton.onClick,arena.Dash);
            var attack=Image("Attack",safe,"attack_button",new Vector2(1,0),new Vector2(-175,175),new Vector2(190,190),true);Image("Fist",attack.transform,"icon_fist",new Vector2(.5f,.5f),Vector2.zero,new Vector2(100,100));attack.gameObject.AddComponent<NetworkAttackInput>().Arena=arena;
            joystick.gameObject.SetActive(false);arena.DashButton.interactable=false;
            var leave=Image("Leave Session",safe,"panel",new Vector2(1,1),new Vector2(-170,-67),new Vector2(250,72),true);Text("Label",leave.transform,"net.leave",Vector2.zero,new Vector2(228,60),26);UnityEventTools.AddPersistentListener(Button(leave).onClick,arena.Leave);
            arena.InventoryUi=NetworkInventoryUiBuilder.Build(safe);arena.InventoryUi.AttackIcon=attack.transform.Find("Fist").GetComponent<Image>();
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<EventSystem>().sendNavigationEvents=false;var module=events.GetComponent<InputSystemUIInputModule>();module.UnassignActions();module.actionsAsset=actions;module.point=point;module.leftClick=click;
            EditorSceneManager.SaveScene(scene,ScenePath);FrontendSceneBuilder.Build();AssetDatabase.SaveAssets();
        }
        public static void CreatePrefabs()
        {
            WorldArt.SpriteMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wildfolk/Art/WorldSprite.mat");
            var ecology=NetworkEcologyBuilder.BuildPrefab();
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Player.prefab");
            var actor=(GameObject)PrefabUtility.InstantiatePrefab(source);actor.name="NetworkPlayer";
            foreach(var component in new Component[]{actor.GetComponent<PlayerPrimaryAction>(),actor.GetComponent<PlayerInteraction>(),actor.GetComponent<PlayerPlacement>(),actor.GetComponent<PlayerCrafting>(),actor.GetComponent<PlayerStorage>()})Object.DestroyImmediate(component);
            var session=actor.AddComponent<GameplaySession>();session.Mode=SessionMode.Multiplayer;session.PlayerHealth=actor.GetComponent<Health>();
            var commands=actor.GetComponent<PlayerInputCommands>();commands.Session=session;commands.Primary=null;commands.Interaction=null;commands.AllowInventory=true;commands.AllowPause=false;
            var motor=actor.GetComponent<PlayerMotor>();motor.Session=session;actor.GetComponent<PlayerCombat>().Session=session;actor.GetComponent<PlayerSurvival>().Session=session;actor.GetComponent<PlayerSurvival>().enabled=false;
            actor.GetComponent<PlayerInputAdapter>().enabled=false;motor.enabled=false;motor.Locomotion.enabled=false;
            actor.AddComponent<NetworkObject>();var transform=actor.AddComponent<NetworkTransform>();transform.SyncPositionZ=false;transform.SyncRotAngleX=transform.SyncRotAngleY=transform.SyncRotAngleZ=false;transform.SyncScaleX=transform.SyncScaleY=transform.SyncScaleZ=false;transform.PositionThreshold=.015f;transform.Interpolate=true;transform.UseUnreliableDeltas=true;
            var drop=actor.GetComponent<PlayerItemDrop>();drop.Session=session;
            actor.AddComponent<NetworkRigidbody2D>();var player=actor.AddComponent<NetworkPlayer>();player.Catalog=AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/Wildfolk/Data/ContentCatalog.asset");
            var marker=WorldArt.Sprite("Local Survivor","joystick_base",actor.transform,Vector2.zero,1.15f,WorldSorting.GroundDecoration);marker.color=new Color32(186,170,115,130);player.LocalMarker=marker.gameObject;marker.gameObject.SetActive(false);actor.AddComponent<NetworkInventory>();
            PrefabUtility.SaveAsPrefabAsset(actor,PlayerPrefabPath);Object.DestroyImmediate(actor);
            var root=new GameObject("Network Session");var transport=root.AddComponent<UnityTransport>();transport.ConnectTimeoutMS=1000;transport.MaxConnectAttempts=5;transport.DisconnectTimeoutMS=4000;
            var manager=root.AddComponent<NetworkManager>();manager.NetworkConfig=new NetworkConfig{NetworkTransport=transport,TickRate=20,ConnectionApproval=true,EnableSceneManagement=true,ProtocolVersion=3,ClientConnectionBufferTimeout=10};
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var service=root.AddComponent<NetworkSession>();service.Manager=manager;service.PlayerPrefab=prefab;service.EcologyPrefab=ecology;
            PrefabUtility.SaveAsPrefabAsset(root,SessionPrefabPath);Object.DestroyImmediate(root);
        }
    }
}
