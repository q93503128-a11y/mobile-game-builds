using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class NetworkInventoryUi : MonoBehaviour
    {
        public Text Wallet;
        public Button Bag;
        public Image AttackIcon;
        public InteractButtonView Interact;
        private Sprite fist;
        private NetworkPlayer owner;
        public void Bind(NetworkPlayer player)
        {
            owner=player;fist=AttackIcon.sprite;var inventory=player.GetComponent<PlayerInventory>();var session=player.Commands.Session;
            var panel=GetComponent<InventoryPanel>();panel.Owner=inventory;panel.Session=session;panel.Crafting=player.GetComponent<PlayerCrafting>();panel.Storage=player.GetComponent<PlayerStorage>();
            var crafting=GetComponent<CraftingPanel>();crafting.Owner=panel.Crafting;foreach(var card in crafting.Cards)card.Crafting=panel.Crafting;
            var chest=GetComponent<ChestPanel>();chest.Owner=inventory;chest.Storage=panel.Storage;
            var drag=GetComponent<InventoryDragController>();drag.Owner=inventory;drag.Session=session;drag.Storage=panel.Storage;drag.Equipment=player.Equipment;drag.ItemDrop=player.GetComponent<PlayerItemDrop>();
            var equipment=GetComponent<InventoryEquipment>();equipment.Player=inventory;equipment.Equipment=player.Equipment;
            foreach(var portrait in GetComponentsInChildren<PlayerPortrait>(true))portrait.Equipment=player.Equipment;
            var hotbar=GetComponentInChildren<HotbarView>(true);hotbar.Owner=inventory;hotbar.Session=session;
            // Scene-authored close actions are bound to the local actor at runtime.
            foreach(var button in GetComponentsInChildren<Button>(true))
            {
                if(button.name=="Close"){button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(panel.Crafting.CloseCrafting);}
                if(button==panel.CraftingButton){button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(panel.Crafting.OpenHandCrafting);}
                if(button==crafting.InventoryButton){button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(panel.Crafting.OpenBag);}
                if(button==crafting.UpgradeButton){button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(panel.Crafting.UpgradeFromUI);}
            }
            Interact.Interaction=player.GetComponent<PlayerInteraction>();
            Bag.onClick.AddListener(session.ToggleInventory);player.GetComponent<PlayerProgression>().Changed+=Refresh;player.Equipment.Changed+=Refresh;L.Changed+=Refresh;
            gameObject.SetActive(true);Refresh();
        }
        private void Refresh(){if(owner==null)return;var p=owner.GetComponent<PlayerProgression>();Wallet.text=L.Format("net.wallet",p.Level,p.Experience,p.Gold);AttackIcon.sprite=owner.Equipment.ActiveTool==null ? fist : owner.Equipment.SelectedItem.Icon;}
        private void OnDestroy(){if(owner!=null){owner.GetComponent<PlayerProgression>().Changed-=Refresh;owner.Equipment.Changed-=Refresh;}L.Changed-=Refresh;}
    }
}
