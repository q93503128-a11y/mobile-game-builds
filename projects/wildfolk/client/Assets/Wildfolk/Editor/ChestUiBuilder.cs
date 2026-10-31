using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class ChestUiBuilder
    {
        internal static void Build(GameObject root,Transform safe,PlayerInventory owner,PlayerStorage storage,InventoryDragController drag)
        {
            var panel=root.AddComponent<ChestPanel>();panel.Owner=owner;panel.Storage=storage;panel.Drag=drag;
            var overlay=Scrim("Chest Overlay",safe);panel.View=overlay.gameObject;
            var surface=Image("Chest Panel",overlay.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1160,980),true);
            var fit=surface.gameObject.AddComponent<ModalLayout>();fit.SafeArea=(RectTransform)safe;fit.TopReserve=fit.BottomReserve=40;fit.ReferenceSize=new Vector2(1160,980);
            panel.Title=Text("Title",surface.transform,"item.storage_chest.name",new Vector2(-271,428),new Vector2(360,48),32,UnityEngine.TextAnchor.MiddleLeft);
            var close=Image("Close",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(497,428),new Vector2(64,64),true);
            Text("Symbol",close.transform,"×",Vector2.zero,new Vector2(58,58),40);UnityEventTools.AddPersistentListener(Button(close).onClick,storage.Session.CloseInventory);
            var scrollArea=Rect("Chest Scroll",surface.transform,new Vector2(.5f,.5f),new Vector2(0,240),new Vector2(820,300));
            var viewport=Image("Viewport",scrollArea,"white",new Vector2(.5f,.5f),new Vector2(-12,0),new Vector2(792,300),true);viewport.color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            var chest=InventoryUiBuilder.Grid("Chest Slots",viewport.transform,Vector2.zero,new Vector2(792,300));chest.anchorMin=chest.anchorMax=new Vector2(.5f,1);chest.pivot=new Vector2(.5f,1);chest.anchoredPosition=Vector2.zero;
            chest.GetComponent<UnityEngine.UI.GridLayoutGroup>().constraintCount=8;chest.GetComponent<UnityEngine.UI.GridLayoutGroup>().cellSize=new Vector2(80,80);
            chest.GetComponent<GridLayoutGroup>().padding=new RectOffset(48,48,0,0);chest.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            panel.Scroll=scrollArea.gameObject.AddComponent<ScrollRect>();panel.Scroll.content=panel.SlotsRoot=chest;panel.Scroll.viewport=(RectTransform)viewport.transform;panel.Scroll.horizontal=false;panel.Scroll.inertia=false;panel.Scroll.scrollSensitivity=50;panel.Scroll.movementType=ScrollRect.MovementType.Clamped;
            var track=Image("Scrollbar",scrollArea,"panel",new Vector2(.5f,.5f),new Vector2(402,0),new Vector2(16,300),true);track.color=new Color32(43,61,47,255);var handle=Image("Handle",track.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(14,0),true);handle.color=new Color32(142,151,111,255);
            var bar=track.gameObject.AddComponent<Scrollbar>();bar.direction=Scrollbar.Direction.BottomToTop;bar.handleRect=(RectTransform)handle.transform;bar.targetGraphic=handle;panel.Scroll.verticalScrollbar=bar;panel.Scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
            Text("Player Label",surface.transform,"ui.inventory.title",new Vector2(-356,54),new Vector2(180,32),23,UnityEngine.TextAnchor.MiddleLeft);
            var inventory=InventoryUiBuilder.Grid("Player Storage",surface.transform,new Vector2(0,-114),new Vector2(892,292));
            Text("Hotbar Label",surface.transform,"ui.inventory.hotbar",new Vector2(-356,-283),new Vector2(180,32),23,UnityEngine.TextAnchor.MiddleLeft);
            var quick=InventoryUiBuilder.Grid("Player Hotbar",surface.transform,new Vector2(0,-355),new Vector2(892,92));
            panel.ChestSlots=new InventorySlotView[36];panel.PlayerSlots=new InventorySlotView[owner.Settings.InventorySlots];
            for(var i=0;i<panel.ChestSlots.Length;i++)panel.ChestSlots[i]=Create(chest,i,false,true,drag,panel);
            for(var i=0;i<panel.PlayerSlots.Length;i++)panel.PlayerSlots[i]=Create(i<PlayerInventory.HotbarSize ? quick : inventory,i,i<PlayerInventory.HotbarSize,false,drag,panel);
            var split=Image("Split",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(0,-446),new Vector2(168,58),true);Text("Label",split.transform,"ui.action.split",Vector2.zero,new Vector2(156,50),22);drag.ChestSplitButton=Button(split);UnityEventTools.AddPersistentListener(drag.ChestSplitButton.onClick,drag.OpenSplit);
            Arrange(panel);overlay.gameObject.SetActive(false);
        }
        internal static void Arrange(ChestPanel panel)
        {
            var surface=panel.View.transform.Find("Chest Panel");var size=new Vector2(1900,980);((RectTransform)surface).sizeDelta=size;
            var fit=surface.GetComponent<ModalLayout>();fit.ReferenceSize=size;fit.HorizontalReserve=20;fit.TopReserve=fit.BottomReserve=20;
            InventoryUiBuilder.Caption(panel.Title,new Vector2(-480,410),new Vector2(800,80),40);InventoryUiBuilder.ActionSize(surface.Find("Close").GetComponent<Button>(),new Vector2(865,410),new Vector2(120,120));
            InventoryUiBuilder.Caption(surface.Find("Player Label").GetComponent<Text>(),new Vector2(475,320),new Vector2(800,55),32);surface.Find("Hotbar Label").gameObject.SetActive(false);
            InventoryUiBuilder.Position(panel.Scroll,new Vector2(-475,-10),new Vector2(850,700));InventoryUiBuilder.Position(panel.Scroll.viewport,new Vector2(-12,0),new Vector2(820,700));
            panel.SlotsRoot.sizeDelta=new Vector2(808,panel.SlotsRoot.sizeDelta.y);var chestGrid=panel.SlotsRoot.GetComponent<GridLayoutGroup>();chestGrid.constraintCount=6;chestGrid.cellSize=Vector2.one*128;chestGrid.spacing=Vector2.one*8;chestGrid.padding=new RectOffset();
            var track=panel.Scroll.transform.Find("Scrollbar");InventoryUiBuilder.Position(track,new Vector2(415,0),new Vector2(24,700));
            var inventory=surface.Find("Player Storage") as RectTransform;
            var playerScroll=surface.Find("Player Scroll");
            if(playerScroll==null)
            {
                var viewport=Image("Player Scroll",surface,"white",Vector2.one*.5f,new Vector2(475,-30),new Vector2(820,650),true);viewport.color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();playerScroll=viewport.transform;
                var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.content=inventory;scroll.viewport=viewport.rectTransform;scroll.horizontal=false;scroll.inertia=false;scroll.movementType=ScrollRect.MovementType.Clamped;
                inventory.SetParent(playerScroll,false);inventory.anchorMin=inventory.anchorMax=new Vector2(.5f,1);inventory.pivot=new Vector2(.5f,1);inventory.anchoredPosition=Vector2.zero;inventory.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            }
            else inventory=playerScroll.GetComponent<ScrollRect>().content;
            inventory.sizeDelta=new Vector2(808,808);var playerGrid=inventory.GetComponent<GridLayoutGroup>();playerGrid.constraintCount=6;playerGrid.cellSize=Vector2.one*128;playerGrid.spacing=Vector2.one*8;
            foreach(var slot in panel.PlayerSlots){slot.transform.SetParent(inventory,false);slot.transform.SetAsLastSibling();InventoryUiBuilder.SlotSize(slot);}
            foreach(var slot in panel.ChestSlots)InventoryUiBuilder.SlotSize(slot);
            surface.Find("Player Hotbar").gameObject.SetActive(false);InventoryUiBuilder.ActionSize(panel.Drag.ChestSplitButton,new Vector2(0,-430),new Vector2(500,120));
        }
        private static InventorySlotView Create(Transform parent,int index,bool quick,bool chest,InventoryDragController drag,ChestPanel panel)
        {
            var slot=Slot(parent,index,chest ? 80 : 92,quick);var view=slot.GetComponent<InventorySlotView>();
            var input=slot.gameObject.AddComponent<InventorySlotDrag>();input.Controller=drag;input.View=view;input.Index=index;input.IsChest=chest;
            if(!chest)UnityEventTools.AddIntPersistentListener(Button(slot).onClick,panel.Select,index);return view;
        }
    }
}
