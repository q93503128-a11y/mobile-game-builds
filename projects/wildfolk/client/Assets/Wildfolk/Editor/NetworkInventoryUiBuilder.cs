using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using static Wildfolk.Editor.GameplayUiFactory;
namespace Wildfolk.Editor
{
    internal static class NetworkInventoryUiBuilder
    {
        internal static NetworkInventoryUi Build(Transform safe)
        {
            var rect=Rect("Inventory UI",safe,Vector2.zero,Vector2.zero,Vector2.zero);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var root=rect.gameObject;root.SetActive(false);var owner=AssetDatabase.LoadAssetAtPath<GameObject>(MultiplayerSceneBuilder.PlayerPrefabPath).GetComponent<PlayerInventory>();var session=owner.GetComponent<GameplaySession>();
            InventoryUiBuilder.Build(root,rect,session,owner,owner.GetComponent<PlayerCrafting>(),owner.Settings.InventorySlots);
            var hotbar=Rect("Hotbar",rect,new Vector2(.5f,0),new Vector2(51,102),new Vector2(918,106));hotbar.SetAsFirstSibling();
            var view=hotbar.gameObject.AddComponent<HotbarView>();view.Owner=owner;view.Session=session;view.Slots=new InventorySlotView[PlayerInventory.HotbarSize];
            for(var i=0;i<view.Slots.Length;i++){var slot=Slot(hotbar,i,94,true);((RectTransform)slot.transform).anchoredPosition=new Vector2((i-4)*102,0);view.Slots[i]=slot.GetComponent<InventorySlotView>();UnityEventTools.AddIntPersistentListener(Button(slot).onClick,view.Select,i);}
            var ui=root.AddComponent<NetworkInventoryUi>();var bag=Image("Inventory",rect,"panel",new Vector2(.5f,0),new Vector2(-459,102),new Vector2(94,94),true);Image("Backpack",bag.transform,"icon_inventory",new Vector2(.5f,.5f),Vector2.zero,new Vector2(62,62));bag.transform.SetAsFirstSibling();ui.Bag=Button(bag);
            ui.Wallet=Text("Session Rewards",rect,"",new Vector2(-580,355),new Vector2(650,44),24,UnityEngine.TextAnchor.MiddleLeft);ui.Wallet.transform.SetAsFirstSibling();
            var feedback=root.AddComponent<PlayerFeedback>();feedback.Label=Text("Message",rect,"",new Vector2(0,432),new Vector2(940,52),26);feedback.Label.raycastTarget=false;
            return ui;
        }
    }
}
