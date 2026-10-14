using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static Wildfolk.Editor.GameplayUiFactory;

namespace Wildfolk.Editor
{
    internal static class InventoryUiBuilder
    {
        internal static void Build(GameObject root,Transform safe,GameplaySession session,PlayerInventory owner,PlayerCrafting crafting,int count)
        {
            var panel=root.AddComponent<InventoryPanel>();panel.Session=session;panel.Owner=owner;panel.Storage=owner.GetComponent<PlayerStorage>();panel.Crafting=crafting;
            var drag=root.AddComponent<InventoryDragController>();drag.Owner=owner;drag.Session=session;drag.Storage=panel.Storage;drag.CanvasRect=(RectTransform)safe;
            var overlay=Scrim("Inventory Overlay",safe);panel.View=overlay.gameObject;
            var surface=Image("Inventory Panel",overlay.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1520,740),true);
            QualityUiBuilder.Matte(surface.transform,new Vector2(1520,740));var fit=surface.gameObject.AddComponent<ModalLayout>();fit.SafeArea=(RectTransform)safe;fit.TopReserve=CharacterUiBuilder.ProfileBottomReserve;fit.ReferenceSize=new Vector2(1520,740);
            Text("Title",surface.transform,"ui.inventory.title",new Vector2(-556,312),new Vector2(296,52),34,TextAnchor.MiddleLeft);
            var close=Image("Close",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(680,312),new Vector2(64,64),true);
            Text("Close Symbol",close.transform,"×",Vector2.zero,new Vector2(58,58),40);
            UnityEventTools.AddPersistentListener(Button(close).onClick,session.CloseInventory);
            Text("Storage Label",surface.transform,"ui.inventory.storage",new Vector2(-292,245),new Vector2(180,32),23,TextAnchor.MiddleLeft);
            var storage=Grid("Storage Slots",surface.transform,new Vector2(-16,92),new Vector2(752,252));
            Text("Hotbar Label",surface.transform,"ui.inventory.hotbar",new Vector2(-292,-55),new Vector2(180,32),23,TextAnchor.MiddleLeft);
            var quick=Grid("Hotbar Slots",surface.transform,new Vector2(-16,-127),new Vector2(752,78));
            panel.Slots=new InventorySlotView[count];
            for(var i=0;i<count;i++)
            {
                var slot=Slot(i<PlayerInventory.HotbarSize ? quick : storage,i,78,i<PlayerInventory.HotbarSize);
                var view=slot.GetComponent<InventorySlotView>();panel.Slots[i]=view;
                UnityEventTools.AddIntPersistentListener(Button(slot).onClick,panel.Select,i);
                var handler=slot.gameObject.AddComponent<InventorySlotDrag>();handler.Controller=drag;handler.View=view;handler.Index=i;
            }
            storage.GetComponent<GridLayoutGroup>().cellSize=quick.GetComponent<GridLayoutGroup>().cellSize=new Vector2(78,78);storage.GetComponent<GridLayoutGroup>().spacing=quick.GetComponent<GridLayoutGroup>().spacing=new Vector2(6,6);
            panel.SelectedIcon=Image("Selected Icon",surface.transform,"white",new Vector2(.5f,.5f),new Vector2(415,205),new Vector2(58,72));panel.SelectedIcon.preserveAspect=true;
            panel.SelectedName=Text("Selected Name",surface.transform,string.Empty,new Vector2(556,205),new Vector2(222,90),28,TextAnchor.MiddleLeft);
            panel.SelectedCategory=Text("Category",surface.transform,string.Empty,new Vector2(545,115),new Vector2(320,28),20,TextAnchor.MiddleLeft);
            BuildEquipment(root,surface.transform,owner,panel);
            panel.CraftingButton=QualityUiBuilder.Action(surface.transform,"Crafting","ui.crafting.title",new Vector2(479,312),new Vector2(250,64),crafting.OpenHandCrafting,out var craftLabel);
            CraftingUiBuilder.Build(root,safe,crafting);
            overlay.gameObject.SetActive(false);
            var split=Image("Split",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(541,-281),new Vector2(168,58),true);Text("Label",split.transform,"ui.action.split",Vector2.zero,new Vector2(156,50),22);drag.SplitButton=Button(split);UnityEventTools.AddPersistentListener(drag.SplitButton.onClick,drag.OpenSplit);
            drag.ItemDrop=owner.GetComponent<PlayerItemDrop>();
            ChestUiBuilder.Build(root,safe,owner,panel.Storage,drag);
            var dropZone=Image("Drop Zone",safe,"panel",new Vector2(.5f,1),new Vector2(0,-128),new Vector2(320,88),true);dropZone.color=new Color32(155,108,72,255);Text("Label",dropZone.transform,"ui.inventory.drop",Vector2.zero,new Vector2(300,68),27);
            dropZone.gameObject.AddComponent<InventoryDropZone>().Controller=drag;drag.DropZone=dropZone.gameObject;drag.DropZone.SetActive(false);
            var ghost=Rect("Dragged Item",safe,new Vector2(.5f,.5f),Vector2.zero,new Vector2(94,94));
            var group=ghost.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            Image("Frame",ghost,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(94,94));
            drag.Ghost=ghost;
            drag.GhostIcon=Image("Icon",ghost,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(78,78));drag.GhostIcon.preserveAspect=true;
            drag.GhostCount=Text("Stack Count",ghost,string.Empty,new Vector2(25,-32),new Vector2(50,28),23,TextAnchor.MiddleRight);
            ghost.gameObject.SetActive(false);StackSplitUiBuilder.Build(safe,drag);
        }

        internal static RectTransform Grid(string name,Transform parent,Vector2 position,Vector2 size)
        {
            var grid=Rect(name,parent,new Vector2(.5f,.5f),position,size);
            var layout=grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize=new Vector2(92,92);layout.spacing=new Vector2(8,8);
            layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=9;layout.childAlignment=TextAnchor.UpperLeft;
            return grid;
        }

        private static void BuildEquipment(GameObject root,Transform surface,PlayerInventory player,InventoryPanel panel)
        {
            var equipment=root.AddComponent<InventoryEquipment>();equipment.Player=player;equipment.Panel=panel;equipment.Equipment=player.GetComponent<PlayerEquipment>();
            Text("Equipment Title",surface,"ui.inventory.equipment",new Vector2(-554,238),new Vector2(280,38),24);
            QualityUiBuilder.Portrait(surface,player,new Vector2(-554,94),.9f);
            for(var i=0;i<2;i++){var slot=Image("Equipment Slot",surface,"panel",new Vector2(.5f,.5f),new Vector2(-618+i*127,-112),new Vector2(100,100),true);var slotButton=Button(slot);if(i==0)equipment.HeadButton=slotButton;else equipment.BodyButton=slotButton;UnityEventTools.AddIntPersistentListener(slotButton.onClick,equipment.SelectSlot,(int)(i==0 ? ArmorSlot.Head : ArmorSlot.Body));var icon=Image("Equipped Item",slot.transform,"white",new Vector2(.5f,.5f),Vector2.zero,new Vector2(68,72));icon.preserveAspect=true;if(i==0)equipment.HeadIcon=icon;else equipment.BodyIcon=icon;Text("Slot Name",surface,i==0 ? "ui.slot.Head" : "ui.slot.Body",new Vector2(-618+i*127,-186),new Vector2(122,32),21);}
            Text("Slot Hint",surface,"ui.inventory.slot_hint",new Vector2(-554,-262),new Vector2(290,94),20);
            equipment.Details=Text("Armor Details",surface,"",new Vector2(544,23),new Vector2(324,72),23,TextAnchor.MiddleLeft);
            equipment.Comparison=Text("Comparison",surface,"",new Vector2(544,-57),new Vector2(324,74),22,TextAnchor.MiddleLeft);
            equipment.EquipButton=QualityUiBuilder.Action(surface,"Equip","ui.action.equip",new Vector2(541,-144),new Vector2(310,64),equipment.Equip,out equipment.EquipLabel);
            equipment.UnequipButton=QualityUiBuilder.Action(surface,"Unequip","ui.action.unequip",new Vector2(541,-213),new Vector2(310,58),equipment.Unequip,out var unequip);
            QualityUiBuilder.Line(surface,new Vector2(-395,255),new Vector2(-395,-312),2);QualityUiBuilder.Line(surface,new Vector2(366,255),new Vector2(366,-312),2);
        }
    }
}
