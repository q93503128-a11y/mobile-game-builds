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
        public ScrollRect Scroll;
        public RectTransform SlotsRoot;
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
            bound=Storage.Chest;if(bound!=null)bound.Model.Changed+=Refresh;if(Scroll!=null)Scroll.verticalNormalizedPosition=1;Refresh();
        }
        private void Refresh()
        {
            var visible=Storage.Session.State==SessionState.Inventory && Storage.IsOpen;
            View.SetActive(visible);if(!visible)return;
            Title.text=bound.Placeable.Definition.Item.LocalizedName;
            EnsureSlots(bound.Model.Capacity);
            for(var i=0;i<ChestSlots.Length;i++){ChestSlots[i].gameObject.SetActive(i<bound.Model.Capacity);if(i<bound.Model.Capacity)ChestSlots[i].Show(bound.Model.GetSlot(i),Drag.IsSelected(i,true));}
            for(var i=0;i<PlayerSlots.Length;i++)PlayerSlots[i].Show(Owner.Model.GetSlot(i),Owner.SelectedIndex==i);
        }
        private void EnsureSlots(int capacity)
        {
            if(capacity<=ChestSlots.Length)return;var before=ChestSlots.Length;System.Array.Resize(ref ChestSlots,capacity);
            for(var i=before;i<capacity;i++){var slot=Instantiate(ChestSlots[0],SlotsRoot);slot.name="Slot "+(i+1);var handler=slot.GetComponent<InventorySlotDrag>();handler.Index=i;handler.Controller=Drag;handler.View=slot;ChestSlots[i]=slot;}
        }
        public void Select(int index) => Owner.SelectSlot(index);
    }
}
