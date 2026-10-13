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
        internal static void Build(GameplayAssets assets,GameplaySession session,PlayerMotor motor,FistCombat combat,PlayerInventory inventory)
        {
            var root=new GameObject("Mobile HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            scaler.matchWidthOrHeight=.5f;
            var safe=Rect("Safe Area",root.transform,Vector2.zero,Vector2.zero,Vector2.zero);
            safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;
            safe.gameObject.AddComponent<SafeAreaLayout>();
            var controls=Rect("Controls",safe,Vector2.zero,Vector2.zero,Vector2.zero);
            controls.anchorMax=Vector2.one;controls.offsetMin=controls.offsetMax=Vector2.zero;
            var controlsGroup=controls.gameObject.AddComponent<CanvasGroup>();
            var joystickImage=Image("Joystick",controls,"joystick_base",Vector2.zero,new Vector2(166,160),new Vector2(222,222),true);
            var thumb=Image("Thumb",joystickImage.transform,"joystick_thumb",new Vector2(.5f,.5f),Vector2.zero,new Vector2(95,95));
            var joystick=joystickImage.gameObject.AddComponent<VirtualJoystick>();
            joystick.Thumb=(RectTransform)thumb.transform;joystick.Session=session;motor.Joystick=joystick;
            var attackImage=Image("Bare Hand Attack",controls,"attack_button",new Vector2(1,0),new Vector2(-162,150),new Vector2(174,174),true);
            Image("Fist",attackImage.transform,"icon_fist",new Vector2(.5f,.5f),Vector2.zero,new Vector2(86,86));
            var attack=attackImage.gameObject.AddComponent<HoldAttackInput>();
            attack.Combat=combat;attack.Session=session;attack.Surface=attackImage;
            BuildBoss(safe);
            BuildInventory(root, safe,session,inventory,assets.Settings.InventorySlots);
            var defeat=BuildDefeat(safe,session);
            var backpack=Image("Inventory",safe,"panel",new Vector2(1,1),new Vector2(-90,-82),new Vector2(86,86),true);
            Image("Backpack",backpack.transform,"icon_inventory",new Vector2(.5f,.5f),Vector2.zero,new Vector2(57,57));
            var inventoryButton=Button(backpack);
            UnityEventTools.AddPersistentListener(inventoryButton.onClick,session.ToggleInventory);
            var hud=root.AddComponent<GameplayHud>();
            hud.Session=session;hud.Joystick=joystick;hud.Attack=attack;hud.Controls=controlsGroup;
            hud.InventoryButton=inventoryButton;hud.DefeatPanel=defeat;
            var eventSystem=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<EventSystem>().sendNavigationEvents=false;
            var module=eventSystem.GetComponent<InputSystemUIInputModule>();
            module.UnassignActions();
            module.actionsAsset=assets.UIActions;module.point=assets.Point;module.leftClick=assets.Click;
        }

        private static void BuildInventory(GameObject root,Transform safe,GameplaySession session,PlayerInventory inventory,int count)
        {
            var panel=root.AddComponent<InventoryPanel>();
            panel.Session=session;panel.Owner=inventory;
            var scrim=Scrim("Inventory Overlay",safe);panel.View=scrim.gameObject;
            var surface=Image("Inventory Panel",scrim.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(720,760),true);
            Text("Title",surface.transform,"Inventory",new Vector2(-220,324),new Vector2(220,52),36,TextAnchor.MiddleLeft);
            var close=Image("Close",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(296,324),new Vector2(64,64),true);
            Text("Close Symbol",close.transform,"×",Vector2.zero,new Vector2(58,58),40);
            UnityEventTools.AddPersistentListener(Button(close).onClick,session.CloseInventory);
            var grid=Rect("Slots",surface.transform,new Vector2(.5f,.5f),new Vector2(0,10),new Vector2(550,550));
            var layout=grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize=new Vector2(125,125);layout.spacing=new Vector2(15,15);
            layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=4;
            layout.childAlignment=TextAnchor.UpperCenter;
            panel.Slots=new InventorySlotView[count];
            for(var i=0;i<count;i++)
            {
                var slot=Image("Slot "+(i+1),grid,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(125,125),true);
                var view=slot.gameObject.AddComponent<InventorySlotView>();
                var selection=Image("Selection",slot.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(123,123));
                selection.color=new Color32(164,167,103,150);selection.enabled=false;view.Selection=selection;
                view.Icon=Image("Icon",slot.transform,"white",new Vector2(.5f,.5f),new Vector2(0,8),new Vector2(73,73));
                view.Icon.enabled=false;
                view.Count=Text("Stack Count",slot.transform,string.Empty,new Vector2(28,-37),new Vector2(64,30),25,TextAnchor.MiddleRight);
                UnityEventTools.AddIntPersistentListener(Button(slot).onClick,panel.Select,i);
                panel.Slots[i]=view;
            }
            panel.SelectedIcon=Image("Selected Icon",surface.transform,"white",new Vector2(.5f,.5f),new Vector2(-260,-316),new Vector2(58,58));
            panel.SelectedName=Text("Selected Name",surface.transform,string.Empty,new Vector2(-60,-307),new Vector2(320,36),28,TextAnchor.MiddleLeft);
            panel.SelectedCategory=Text("Category",surface.transform,string.Empty,new Vector2(-60,-337),new Vector2(320,28),20,TextAnchor.MiddleLeft);
            scrim.gameObject.SetActive(false);
        }

        private static void BuildBoss(Transform safe)
        {
            var holder=Rect("Boss HUD",safe,new Vector2(.5f,1),new Vector2(0,-88),new Vector2(640,90));
            var boss=holder.gameObject.AddComponent<BossHud>();
            var view=Rect("View",holder,new Vector2(.5f,.5f),Vector2.zero,new Vector2(640,90));boss.View=view.gameObject;
            boss.Name=Text("Name",view,string.Empty,new Vector2(0,27),new Vector2(620,40),28);
            var track=Image("Track",view,"panel",new Vector2(.5f,.5f),new Vector2(0,-14),new Vector2(620,26));
            boss.Fill=Image("Health",track.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(604,12));
            boss.Fill.type=UnityEngine.UI.Image.Type.Filled;boss.Fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
            boss.Fill.color=new Color32(169,105,68,255);view.gameObject.SetActive(false);
        }

        private static GameObject BuildDefeat(Transform safe,GameplaySession session)
        {
            var scrim=Scrim("Defeat Overlay",safe);
            var panel=Image("Defeat Panel",scrim.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(500,280),true);
            Text("Title",panel.transform,"You have fallen",new Vector2(0,60),new Vector2(430,60),36);
            var restart=Image("Restart",panel.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,-55),new Vector2(240,72),true);
            Text("Label",restart.transform,"Try again",Vector2.zero,new Vector2(220,62),27);
            UnityEventTools.AddPersistentListener(Button(restart).onClick,session.Restart);
            scrim.gameObject.SetActive(false);
            return scrim.gameObject;
        }
    }
}
