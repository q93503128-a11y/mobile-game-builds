using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Wildfolk.Presentation;
using static Wildfolk.Editor.GameplayUiFactory;

namespace Wildfolk.Editor
{
    internal static class GameplayHudBuilder
    {
        internal static void Build(GameplayAssets assets,GameplaySession session,PlayerMotor motor,PlayerInventory inventory)
        {
            var root=new GameObject("Mobile HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var safe=Rect("Safe Area",root.transform,Vector2.zero,Vector2.zero,Vector2.zero);safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<SafeAreaLayout>();
            var controls=Rect("Controls",safe,Vector2.zero,Vector2.zero,Vector2.zero);controls.anchorMax=Vector2.one;controls.offsetMin=controls.offsetMax=Vector2.zero;
            var controlsGroup=controls.gameObject.AddComponent<CanvasGroup>();
            var joystick=BuildJoystick(controls,session);motor.Joystick=joystick;motor.GetComponent<PlayerInputAdapter>().Joystick=joystick;
            var action=motor.GetComponent<PlayerPrimaryAction>();
            var attackImage=Image("Primary Action",controls,"attack_button",new Vector2(1,0),new Vector2(-162,150),new Vector2(204,204),true);
            var attackIcon=Image("Item Icon",attackImage.transform,"icon_fist",new Vector2(.5f,.5f),Vector2.zero,new Vector2(92,100));attackIcon.preserveAspect=true;
            var icon=attackImage.gameObject.AddComponent<AttackIconView>();icon.Icon=attackIcon;icon.FistIcon=VisualArtImporter.Sprite("icon_fist");icon.Action=action;
            var attack=attackImage.gameObject.AddComponent<HoldAttackInput>();attack.Action=action;attack.Session=session;attack.Surface=attackImage;
            var interactSurface=Image("Interact",controls,"attack_button",new Vector2(1,0),new Vector2(-162,310),new Vector2(150,150),true);
            var interact=interactSurface.gameObject.AddComponent<InteractButtonView>();interact.Interaction=motor.GetComponent<PlayerInteraction>();interact.Surface=interactSurface;interact.Button=Button(interactSurface);interact.DefaultIcon=VisualArtImporter.Sprite("icon_interact");
            interact.Icon=Image("Icon",interactSurface.transform,"icon_interact",new Vector2(.5f,.5f),Vector2.zero,new Vector2(64,64));interact.Icon.preserveAspect=true;
            var feedback=root.AddComponent<PlayerFeedback>();feedback.Label=Text("Message",safe,string.Empty,new Vector2(0,432),new Vector2(940,52),26);feedback.Label.raycastTarget=false;
            BuildHotbar(controls,session,inventory);
            BuildBoss(safe);
            var time=Image("World Time",safe,"panel",new Vector2(0,1),new Vector2(148,-229),new Vector2(232,44));
            var timeHud=time.gameObject.AddComponent<TimeHud>();timeHud.Clock=session.GetComponent<WorldClock>();
            timeHud.Icon=Image("Time Icon",time.transform,"sun",new Vector2(.5f,.5f),new Vector2(-92,0),new Vector2(30,30));
            timeHud.Label=Text("Time",time.transform,"",new Vector2(19,0),new Vector2(172,36),20);
            InventoryUiBuilder.Build(root,safe,session,inventory,motor.GetComponent<PlayerCrafting>(),assets.Settings.InventorySlots);
            DeathUiBuilder.Build(root,safe,safe,inventory.GetComponent<PlayerDeath>());
            var pause=SurvivalUiBuilder.Pause(safe,session);
            CharacterUiBuilder.Build(root,safe,session,inventory);
            var backpack=Image("Inventory",safe,"panel",new Vector2(.5f,0),new Vector2(-549,102),new Vector2(112,112),true);
            Image("Backpack",backpack.transform,"icon_inventory",new Vector2(.5f,.5f),Vector2.zero,new Vector2(62,62));
            var inventoryButton=Button(backpack);UnityEventTools.AddPersistentListener(inventoryButton.onClick,session.ToggleInventory);
            var pauseImage=Image("Pause",safe,"panel",new Vector2(1,1),new Vector2(-72,-66),new Vector2(76,76),true);
            Image("Icon",pauseImage.transform,"icon_pause",new Vector2(.5f,.5f),Vector2.zero,new Vector2(38,42));
            var pauseButton=Button(pauseImage);UnityEventTools.AddPersistentListener(pauseButton.onClick,session.OpenPause);
            var sizes=root.AddComponent<ControlSizeLayout>();sizes.Preferences=session.GetComponent<UserPreferences>();sizes.Session=session;
            sizes.Joystick=joystick.Visual;sizes.Action=(RectTransform)attackImage.transform;sizes.Interact=(RectTransform)interactSurface.transform;
            var dash=Image("Dash",controls,"attack_button",new Vector2(1,0),new Vector2(-335,265),new Vector2(138,138),true);
            Image("Icon",dash.transform,"icon_dash",new Vector2(.5f,.5f),Vector2.zero,new Vector2(70,70));UnityEventTools.AddPersistentListener(Button(dash).onClick,motor.Locomotion.DashFromUI);sizes.Dash=(RectTransform)dash.transform;
            sizes.Hotbar=(RectTransform)controls.Find("Hotbar");sizes.InventoryButton=(RectTransform)backpack.transform;
            var hud=root.AddComponent<GameplayHud>();hud.Session=session;hud.Joystick=joystick;hud.Attack=attack;hud.Interact=interact;hud.Controls=controlsGroup;
            hud.InventoryButton=inventoryButton;hud.PauseButton=pauseButton;hud.PausePanel=pause;
            OrganizeLayers(root,safe,session);
            ConfigureTouchLayout();
            var eventSystem=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));eventSystem.GetComponent<EventSystem>().sendNavigationEvents=false;
            var module=eventSystem.GetComponent<InputSystemUIInputModule>();module.UnassignActions();module.actionsAsset=assets.UIActions;module.point=assets.Point;module.leftClick=assets.Click;
        }

        internal static VirtualJoystick BuildJoystick(Transform parent,GameplaySession session)
        {
            var zone=Image("Movement Input",parent,"white",Vector2.zero,Vector2.zero,Vector2.zero,true);zone.color=Color.clear;
            var rect=zone.rectTransform;rect.anchorMax=new Vector2(.42f,.72f);rect.offsetMin=new Vector2(12,150);rect.offsetMax=new Vector2(-12,0);zone.transform.SetAsFirstSibling();
            var visual=Image("Joystick",zone.transform,"joystick_base",Vector2.one*.5f,Vector2.zero,new Vector2(240,240));
            var j=zone.gameObject.AddComponent<VirtualJoystick>();j.Session=session;j.Visual=visual.rectTransform;j.Feedback=visual.gameObject.AddComponent<CanvasGroup>();j.Feedback.alpha=0;j.Feedback.blocksRaycasts=false;j.Feedback.interactable=false;
            j.Thumb=Image("Thumb",visual.transform,"joystick_thumb",Vector2.one*.5f,Vector2.zero,new Vector2(96,96)).rectTransform;return j;
        }
        internal static void ConfigureTouchLayout()
        {
            foreach(var controls in Object.FindObjectsByType<ClassHud>(FindObjectsInactive.Include,FindObjectsSortMode.None))ClassContentAssets.ArrangeHud(controls);
            foreach(var layout in Object.FindObjectsByType<ControlSizeLayout>(FindObjectsInactive.Include,FindObjectsSortMode.None))layout.ClassControls=layout.GetComponentInParent<ClassHud>(true);
            foreach(var hud in Object.FindObjectsByType<GameplayHud>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                hud.Interact.Surface.rectTransform.sizeDelta=new Vector2(150,150);((RectTransform)hud.InventoryButton.transform).sizeDelta=new Vector2(112,112);
                ((RectTransform)hud.PauseButton.transform).sizeDelta=new Vector2(112,112);
                var profile=hud.GetComponent<ProfileHud>();if(profile!=null){profile.HealthValue.fontSize=32;profile.HealthValue.rectTransform.sizeDelta=new Vector2(330,52);profile.HealthValue.rectTransform.anchoredPosition=new Vector2(0,40);}
            }
            foreach(var arena in Object.FindObjectsByType<MultiplayerArena>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var ui=arena.InventoryUi;var attack=Object.FindObjectsByType<NetworkAttackInput>(FindObjectsInactive.Include,FindObjectsSortMode.None);NetworkAttackInput primary=null;foreach(var a in attack)if(a.Arena==arena)primary=a;if(primary==null)continue;
                var layout=ui.GetComponent<ControlSizeLayout>()??ui.gameObject.AddComponent<ControlSizeLayout>();layout.Preferences=Object.FindFirstObjectByType<UserPreferences>();layout.Session=null;layout.Joystick=arena.Joystick.Visual;layout.Action=(RectTransform)primary.transform;layout.Interact=ui.Interact.Surface.rectTransform;layout.Dash=(RectTransform)arena.DashButton.transform;layout.Hotbar=(RectTransform)ui.GetComponentInChildren<HotbarView>(true).transform;layout.InventoryButton=(RectTransform)ui.Bag.transform;
                layout.Action.sizeDelta=new Vector2(204,204);layout.Interact.sizeDelta=new Vector2(150,150);layout.InventoryButton.sizeDelta=new Vector2(112,112);arena.Layout=layout;
                layout.ClassControls=ui.GetComponentInParent<ClassHud>(true);
                var oldHealth=arena.CampLabel.transform.parent.Find("Health");if(oldHealth!=null&&oldHealth.GetComponent<Text>()!=null)Object.DestroyImmediate(oldHealth.gameObject);
                var oldCampText=arena.CampLabel.GetComponent<LocalizedLabel>();if(oldCampText!=null)Object.DestroyImmediate(oldCampText);
                var camp=arena.CampLabel.rectTransform;camp.anchorMin=camp.anchorMax=new Vector2(.5f,1);InventoryUiBuilder.Caption(arena.CampLabel,new Vector2(0,-170),new Vector2(1000,90),30);arena.CampLabel.alignment=TextAnchor.MiddleCenter;arena.CampLabel.gameObject.SetActive(false);
                var leave=arena.CampLabel.transform.parent.Find("Leave Session").GetComponent<Button>();InventoryUiBuilder.ActionSize(leave,new Vector2(-200,-75),new Vector2(360,120));
                ui.Wallet.rectTransform.anchorMin=ui.Wallet.rectTransform.anchorMax=Vector2.one;InventoryUiBuilder.Caption(ui.Wallet,new Vector2(-220,-150),new Vector2(400,60),28);ui.Wallet.alignment=TextAnchor.MiddleRight;
                var profile=ui.GetComponent<ProfileHud>();if(profile!=null){profile.HealthValue.fontSize=32;profile.HealthValue.rectTransform.sizeDelta=new Vector2(330,52);profile.HealthValue.rectTransform.anchoredPosition=new Vector2(0,40);}
                if(layout.ClassControls!=null)InventoryUiBuilder.Position(layout.ClassControls.ClassName,new Vector2(-220,-220),new Vector2(400,60));
            }
        }

        private static void OrganizeLayers(GameObject root,RectTransform safe,GameplaySession session)
        {
            var children=new System.Collections.Generic.List<Transform>();foreach(Transform child in safe)children.Add(child);
            var gameplay=Rect("Gameplay Layer",safe,Vector2.zero,Vector2.zero,Vector2.zero);gameplay.anchorMax=Vector2.one;gameplay.offsetMin=gameplay.offsetMax=Vector2.zero;
            var modal=Rect("Modal Layer",safe,Vector2.zero,Vector2.zero,Vector2.zero);modal.anchorMax=Vector2.one;modal.offsetMin=modal.offsetMax=Vector2.zero;
            var drag=root.GetComponent<InventoryDragController>();var split=drag.SplitPanel;var hud=root.GetComponent<GameplayHud>();
            var modalRoots=new System.Collections.Generic.HashSet<Transform>{root.GetComponent<InventoryPanel>().View.transform,root.GetComponent<CraftingPanel>().View.transform,root.GetComponent<ChestPanel>().View.transform,root.GetComponent<CharacterScreen>().View.transform,root.GetComponent<SkillTreeScreen>().View.transform,hud.PausePanel.transform,root.GetComponent<DeathHud>().DeadView.transform,drag.DropZone.transform,drag.Ghost,split.View.transform,split.Actions.transform,drag.HeldCancel.transform};
            foreach(var child in children)child.SetParent(modalRoots.Contains(child) ? modal : gameplay,false);
            root.GetComponent<PlayerFeedback>().Label.transform.SetParent(safe,false);root.GetComponent<PlayerFeedback>().Label.transform.SetAsLastSibling();
            var layers=root.AddComponent<GameplayUiLayers>();layers.Session=session;layers.Gameplay=gameplay.gameObject.AddComponent<CanvasGroup>();
            // One ordered canvas hierarchy: gameplay, blocking modal surfaces, then their item drag overlay.
            drag.Ghost.SetAsLastSibling();
        }

        internal static HotbarView BuildHotbar(Transform controls,GameplaySession session,PlayerInventory inventory)
        {
            var holder=Rect("Hotbar",controls,new Vector2(.5f,0),new Vector2(51,102),new Vector2(1080,120));
            var view=holder.gameObject.AddComponent<HotbarView>();view.Owner=inventory;view.Session=session;view.Slots=new InventorySlotView[PlayerInventory.HotbarSize];
            for(var i=0;i<view.Slots.Length;i++)
            {
                var slot=Slot(holder,i,112,true);((RectTransform)slot.transform).anchoredPosition=new Vector2((i-4)*120,0);
                view.Slots[i]=slot.GetComponent<InventorySlotView>();UnityEventTools.AddIntPersistentListener(Button(slot).onClick,view.Select,i);
            }
            return view;
        }
        internal static void BuildBoss(Transform safe)
        {
            var holder=Rect("Boss HUD",safe,new Vector2(.5f,1),new Vector2(0,-88),new Vector2(640,90));var boss=holder.gameObject.AddComponent<BossHud>();
            var view=Rect("View",holder,new Vector2(.5f,.5f),Vector2.zero,new Vector2(640,90));boss.View=view.gameObject;
            boss.Name=Text("Name",view,string.Empty,new Vector2(0,27),new Vector2(620,40),28);
            var track=Image("Track",view,"panel",new Vector2(.5f,.5f),new Vector2(0,-14),new Vector2(620,26));
            boss.Fill=Image("Health",track.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(604,12));boss.Fill.type=UnityEngine.UI.Image.Type.Filled;boss.Fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;boss.Fill.color=new Color32(169,105,68,255);view.gameObject.SetActive(false);
        }
    }
}
