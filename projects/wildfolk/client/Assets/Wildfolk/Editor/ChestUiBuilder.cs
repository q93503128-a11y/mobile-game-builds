using UnityEditor.Events;
using UnityEngine;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class ChestUiBuilder
    {
        internal static void Build(GameObject root,Transform safe,PlayerInventory owner,PlayerStorage storage,InventoryDragController drag)
        {
            var panel=root.AddComponent<ChestPanel>();panel.Owner=owner;panel.Storage=storage;
            var overlay=Scrim("Chest Overlay",safe);panel.View=overlay.gameObject;
            var surface=Image("Chest Panel",overlay.transform,"panel",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1160,880),true);
            var fit=surface.gameObject.AddComponent<ModalLayout>();fit.SafeArea=(RectTransform)safe;fit.ReferenceSize=new Vector2(1160,880);
            panel.Title=Text("Title",surface.transform,"Storage Chest",new Vector2(-271,378),new Vector2(360,48),32,UnityEngine.TextAnchor.MiddleLeft);
            var close=Image("Close",surface.transform,"panel",new Vector2(.5f,.5f),new Vector2(497,378),new Vector2(64,64),true);
            Text("Symbol",close.transform,"×",Vector2.zero,new Vector2(58,58),40);UnityEventTools.AddPersistentListener(Button(close).onClick,storage.Session.CloseInventory);
            var chest=InventoryUiBuilder.Grid("Chest Slots",surface.transform,new Vector2(0,226),new Vector2(892,192));
            Text("Player Label",surface.transform,"Inventory",new Vector2(-356,104),new Vector2(180,32),23,UnityEngine.TextAnchor.MiddleLeft);
            var inventory=InventoryUiBuilder.Grid("Player Storage",surface.transform,new Vector2(0,-64),new Vector2(892,292));
            Text("Hotbar Label",surface.transform,"Hotbar",new Vector2(-356,-233),new Vector2(180,32),23,UnityEngine.TextAnchor.MiddleLeft);
            var quick=InventoryUiBuilder.Grid("Player Hotbar",surface.transform,new Vector2(0,-305),new Vector2(892,92));
            var definition=UnityEditor.AssetDatabase.LoadAssetAtPath<PlaceableDefinition>("Assets/Wildfolk/Data/StorageChestPlacement.asset");
            panel.ChestSlots=new InventorySlotView[definition.StorageSlots];panel.PlayerSlots=new InventorySlotView[owner.Settings.InventorySlots];
            for(var i=0;i<panel.ChestSlots.Length;i++)panel.ChestSlots[i]=Create(chest,i,false,true,drag,panel);
            for(var i=0;i<panel.PlayerSlots.Length;i++)panel.PlayerSlots[i]=Create(i<PlayerInventory.HotbarSize ? quick : inventory,i,i<PlayerInventory.HotbarSize,false,drag,panel);
            overlay.gameObject.SetActive(false);
        }
        private static InventorySlotView Create(Transform parent,int index,bool quick,bool chest,InventoryDragController drag,ChestPanel panel)
        {
            var slot=Slot(parent,index,92,quick);var view=slot.GetComponent<InventorySlotView>();
            var input=slot.gameObject.AddComponent<InventorySlotDrag>();input.Controller=drag;input.View=view;input.Index=index;input.IsChest=chest;
            if(!chest)UnityEventTools.AddIntPersistentListener(Button(slot).onClick,panel.Select,index);return view;
        }
    }
}
