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
        internal static void Build(GameplayAssets assets,GameplaySession session,PlayerMotor motor,PlayerCombat combat,PlayerInventory inventory)
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
            var attackImage=Image("Attack or Use",controls,"attack_button",new Vector2(1,0),new Vector2(-162,150),new Vector2(174,174),true);
            var attackIcon=Image("Item Icon",attackImage.transform,"icon_fist",new Vector2(.5f,.5f),Vector2.zero,new Vector2(92,100));attackIcon.preserveAspect=true;
            var iconView=attackImage.gameObject.AddComponent<AttackIconView>();iconView.Icon=attackIcon;iconView.FistIcon=VisualArtImporter.Sprite("icon_fist");iconView.Equipment=motor.GetComponent<PlayerEquipment>();
            var attack=attackImage.gameObject.AddComponent<HoldAttackInput>();
            attack.Combat=combat;attack.Session=session;attack.Surface=attackImage;
            BuildHotbar(controls,session,inventory);
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
            var surface=Image("Inventory Panel",scrim.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1040,660),true);
            Text("Title",surface.transform,"Inventory",new Vector2(-355,265),new Vector2(240,52),36,TextAnchor.MiddleLeft);
            var close=Image("Close",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(440,265),new Vector2(64,64),true);
            Text("Close Symbol",close.transform,"×",Vector2.zero,new Vector2(58,58),40);
            UnityEventTools.AddPersistentListener(Button(close).onClick,session.CloseInventory);
            Text("Hotbar Label",surface.transform,"Hotbar",new Vector2(-365,194),new Vector2(180,30),22,TextAnchor.MiddleLeft);
            var quick=Rect("Hotbar Slots",surface.transform,new Vector2(.5f,.5f),new Vector2(0,110),new Vector2(918,94));
            var quickLayout=quick.gameObject.AddComponent<GridLayoutGroup>();quickLayout.cellSize=new Vector2(94,94);quickLayout.spacing=new Vector2(8,8);quickLayout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;quickLayout.constraintCount=9;quickLayout.childAlignment=TextAnchor.UpperCenter;
            Text("Storage Label",surface.transform,"Storage",new Vector2(-365,8),new Vector2(180,30),22,TextAnchor.MiddleLeft);
            var grid=Rect("Storage Slots",surface.transform,new Vector2(.5f,.5f),new Vector2(0,-76),new Vector2(918,94));
            var layout=grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize=new Vector2(94,94);layout.spacing=new Vector2(8,8);
            layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=7;
            layout.childAlignment=TextAnchor.UpperLeft;
            panel.Slots=new InventorySlotView[count];
            for(var i=0;i<count;i++)
            {
                var slot=BuildSlot(i<PlayerInventory.HotbarSize ? quick : grid,i,94,i<PlayerInventory.HotbarSize);
                var view=slot.GetComponent<InventorySlotView>();
                UnityEventTools.AddIntPersistentListener(Button(slot).onClick,panel.Select,i);
                panel.Slots[i]=view;
            }
            panel.SelectedIcon=Image("Selected Icon",surface.transform,"white",new Vector2(.5f,.5f),new Vector2(-405,-236),new Vector2(60,70));panel.SelectedIcon.preserveAspect=true;
            panel.SelectedName=Text("Selected Name",surface.transform,string.Empty,new Vector2(-180,-224),new Vector2(320,36),28,TextAnchor.MiddleLeft);
            panel.SelectedCategory=Text("Category",surface.transform,string.Empty,new Vector2(-180,-258),new Vector2(320,28),20,TextAnchor.MiddleLeft);
            scrim.gameObject.SetActive(false);
        }

        private static void BuildHotbar(Transform controls,GameplaySession session,PlayerInventory inventory)
        {
            var holder=Rect("Hotbar",controls,new Vector2(.5f,0),new Vector2(0,102),new Vector2(918,106));
            var view=holder.gameObject.AddComponent<HotbarView>();view.Owner=inventory;view.Session=session;view.Slots=new InventorySlotView[PlayerInventory.HotbarSize];
            for(var i=0;i<view.Slots.Length;i++)
            {
                var slot=BuildSlot(holder,i,94,true);((RectTransform)slot.transform).anchoredPosition=new Vector2((i-4)*102,0);
                view.Slots[i]=slot.GetComponent<InventorySlotView>();
                UnityEventTools.AddIntPersistentListener(Button(slot).onClick,view.Select,i);
            }
        }

        private static UnityEngine.UI.Image BuildSlot(Transform parent,int index,float size,bool quick)
        {
            var slot=Image("Slot "+(index+1),parent,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(size,size),true);
            var view=slot.gameObject.AddComponent<InventorySlotView>();
            var selection=Image("Selection",slot.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(size+6,size+6));
            selection.color=new Color32(210,183,117,255);view.Selection=selection;
            var inset=Image("Inset",selection.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(size-2,size-2));inset.color=new Color32(24,40,33,255);selection.enabled=false;
            view.Icon=Image("Icon",slot.transform,"white",new Vector2(.5f,.5f),new Vector2(0,2),new Vector2(size*.68f,size*.72f));view.Icon.enabled=false;view.Icon.preserveAspect=true;
            view.Count=Text("Stack Count",slot.transform,string.Empty,new Vector2(size*.24f,-size*.31f),new Vector2(42,26),21,TextAnchor.MiddleRight);
            if(quick) Text("Quick Slot",slot.transform,(index+1).ToString(),new Vector2(-size*.31f,size*.32f),new Vector2(24,24),15);
            return slot;
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
