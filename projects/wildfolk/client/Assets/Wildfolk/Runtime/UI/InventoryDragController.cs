using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class InventoryDragController : MonoBehaviour
    {
        public PlayerInventory Owner;
        public GameplaySession Session;
        public PlayerStorage Storage;
        public PlayerItemDrop ItemDrop;
        public GameObject DropZone;
        public Button SplitButton,ChestSplitButton;
        private InventoryModel selectedModel;
        private int selectedIndex=-1;
        public event System.Action SelectionChanged;
        public bool IsSelected(int index,bool chest)=>selectedIndex==index && selectedModel!=null && selectedModel==Model(chest);
        private InventoryModel sourceModel;
        private bool hoveredChest;
        private InventoryModel Model(bool chest) => chest ? Storage.Chest==null ? null : Storage.Chest.Model : Owner.Model;
        public RectTransform CanvasRect;
        public RectTransform Ghost;
        public Image GhostIcon;
        public Text GhostCount;
        public bool IsDragging { get; private set; }
        public int SourceIndex { get; private set; }
        private int pointer;
        private InventorySlotView sourceView;
        private InventorySlotView hoveredView;
        private int hoveredIndex;

        private void Start() { Owner.Model.Changed += Refresh; Session.StateChanged += StateChanged;Storage.Changed+=StorageChanged; }
        private void OnDestroy()
        {
            if (Owner != null && Owner.Model != null) Owner.Model.Changed -= Refresh;
            if (Session != null) Session.StateChanged -= StateChanged;
            if(Storage!=null)Storage.Changed-=StorageChanged;
            if(sourceModel!=null && sourceModel!=Owner.Model)sourceModel.Changed-=Refresh;
        }
        public void Begin(int index, InventorySlotView view, PointerEventData data,bool chest=false)
        {
            var model=Model(chest);
            if (IsDragging || Session.State != SessionState.Inventory || model==null || model.GetSlot(index).IsEmpty) return;
            sourceModel=model;if(model!=Owner.Model)model.Changed+=Refresh;
            IsDragging=true;SourceIndex=index;pointer=data.pointerId;sourceView=view;
            data.eligibleForClick=false;
            sourceView.Icon.color=new Color(1,1,1,.35f);
            var color=sourceView.Count.color;color.a=.35f;sourceView.Count.color=color;
            Ghost.gameObject.SetActive(true);DropZone.SetActive(true);Refresh();Move(data);
        }
        public void Move(PointerEventData data)
        {
            if (!IsDragging || data.pointerId != pointer) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRect,data.position,data.pressEventCamera,out var position))
                Ghost.anchoredPosition=position;
        }
        public void Hover(int index,InventorySlotView view,bool chest=false)
        {
            if (!IsDragging) return;
            ClearHover();hoveredView=view;hoveredIndex=index;hoveredChest=chest;
            view.DropMarker.enabled=true;
            view.DropMarker.color=InventoryModel.CanTransfer(sourceModel,SourceIndex,Model(chest),index) ? new Color(.57f,.75f,.49f,.38f) : new Color(.75f,.36f,.27f,.38f);
        }
        public void Leave(InventorySlotView view) { if (hoveredView==view) ClearHover(); }
        private void ClearHover() { if (hoveredView!=null) hoveredView.DropMarker.enabled=false;hoveredView=null; }
        public void Drop(int index,PointerEventData data,bool chest=false)
        {
            if (!IsDragging || data.pointerId!=pointer) return;
            InventoryModel.Transfer(sourceModel,SourceIndex,Model(chest),index);
            Cancel();
        }
        public void End(PointerEventData data) { if (IsDragging && data.pointerId==pointer) Cancel(); }
        public void Select(int index,bool chest)
        {selectedModel=Model(chest);selectedIndex=index;UpdateSplit();SelectionChanged?.Invoke();}
        public void SplitHalf(){if(Session.State==SessionState.Inventory && selectedModel!=null && selectedIndex>=0)selectedModel.SplitHalf(selectedIndex);UpdateSplit();}
        private void UpdateSplit()
        {
            var valid=Session.State==SessionState.Inventory && selectedModel!=null && selectedIndex>=0 && !selectedModel.GetSlot(selectedIndex).IsEmpty && selectedModel.GetSlot(selectedIndex).Count>1;
            SplitButton.interactable=ChestSplitButton.interactable=valid;
        }
        public void DropWorld(PointerEventData data)
        {if(!IsDragging || data.pointerId!=pointer)return;ItemDrop.Drop(sourceModel,SourceIndex);Cancel();}
        private void Refresh()
        {
            UpdateSplit();
            if (!IsDragging) return;
            var stack=sourceModel.GetSlot(SourceIndex);
            if (stack.IsEmpty) { Cancel();return; }
            GhostIcon.sprite=stack.Item.Icon;GhostCount.text=stack.Count.ToString();
            if (hoveredView!=null) Hover(hoveredIndex,hoveredView,hoveredChest);
        }
        private void StateChanged() { if (Session.State!=SessionState.Inventory){selectedModel=null;selectedIndex=-1;Cancel();}UpdateSplit(); }
        private void StorageChanged(){selectedModel=null;selectedIndex=-1;Cancel();UpdateSplit();SelectionChanged?.Invoke();}
        public void Cancel()
        {
            if(sourceModel!=null && sourceModel!=Owner.Model)sourceModel.Changed-=Refresh;
            sourceModel=null;IsDragging=false;Ghost.gameObject.SetActive(false);DropZone.SetActive(false);ClearHover();
            if (sourceView!=null)
            {
                sourceView.Icon.color=Color.white;
                var color=sourceView.Count.color;color.a=1;sourceView.Count.color=color;
            }
            sourceView=null;
        }
    }
}
