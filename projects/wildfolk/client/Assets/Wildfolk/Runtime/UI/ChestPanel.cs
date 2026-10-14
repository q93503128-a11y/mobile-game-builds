using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class ChestPanel : MonoBehaviour
    {
        public PlayerStorage Storage;
        public PlayerInventory Owner;
        public GameObject View;
        public InventorySlotView[] ChestSlots,PlayerSlots;
        public Text Title;
        public InventoryDragController Drag;
        private StorageChest bound;
        private void Start() {Storage.Changed+=Bind;L.Changed+=Refresh;Storage.Session.StateChanged+=Refresh;Owner.Model.Changed+=Refresh;Owner.SelectionChanged+=Refresh;Drag.SelectionChanged+=Refresh;Bind();}
        private void OnDestroy()
        {
            L.Changed-=Refresh;
            if(Storage!=null){Storage.Changed-=Bind;if(Storage.Session!=null)Storage.Session.StateChanged-=Refresh;}
            if(Owner!=null){if(Owner.Model!=null)Owner.Model.Changed-=Refresh;Owner.SelectionChanged-=Refresh;}
            if(bound!=null)bound.Model.Changed-=Refresh;
            if(Drag!=null)Drag.SelectionChanged-=Refresh;
        }
        private void Bind()
        {
            if(bound!=null)bound.Model.Changed-=Refresh;
            bound=Storage.Chest;if(bound!=null)bound.Model.Changed+=Refresh;Refresh();
        }
        private void Refresh()
        {
            var visible=Storage.Session.State==SessionState.Inventory && Storage.IsOpen;
            View.SetActive(visible);if(!visible)return;
            Title.text=bound.Placeable.Definition.Item.LocalizedName;
            for(var i=0;i<ChestSlots.Length;i++){ChestSlots[i].gameObject.SetActive(i<bound.Model.Capacity);if(i<bound.Model.Capacity)ChestSlots[i].Show(bound.Model.GetSlot(i),Drag.IsSelected(i,true));}
            for(var i=0;i<PlayerSlots.Length;i++)PlayerSlots[i].Show(Owner.Model.GetSlot(i),Owner.SelectedIndex==i);
        }
        public void Select(int index) => Owner.SelectSlot(index);
    }
}
