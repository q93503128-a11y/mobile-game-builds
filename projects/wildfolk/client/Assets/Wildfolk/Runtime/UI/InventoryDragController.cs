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
        public PlayerEquipment Equipment;
        private ArmorSlot? equipmentSource;
        public GameObject DropZone;
        public Button SplitButton,ChestSplitButton;
        public StackSplitPanel SplitPanel;
        public GameObject HeldCancel;
        public int HeldAmount {get;private set;}
        public bool IsHoldingPartial {get;private set;}
        private InventoryModel selectedModel;
        private int selectedIndex=-1;
        public event System.Action SelectionChanged;
        public bool IsSelected(int index,bool chest)=>selectedIndex==index && selectedModel!=null && selectedModel==Model(chest);
        private InventoryModel sourceModel;
        private bool hoveredChest;
        private InventoryModel Model(bool chest) => chest ? Storage==null || Storage.Chest==null ? null : Storage.Chest.Model : Owner.Model;
        public RectTransform CanvasRect;
        public RectTransform Ghost;
        public Image GhostIcon;
        public Text GhostCount;
        public bool IsDragging { get; private set; }
        public int SourceIndex { get; private set; }
        private int pointer;
        private string remainder;
        private void LateUpdate(){if(IsHoldingPartial && sourceView!=null && sourceView.Count.text!=remainder)sourceView.Count.text=remainder;}
        private InventorySlotView sourceView;
        private InventorySlotView hoveredView;
        private int hoveredIndex;

        private void Start() { Owner.Model.Changed += Refresh; Session.StateChanged += StateChanged;if(Storage!=null)Storage.Changed+=StorageChanged; }
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
            Storage?.Authority?.CaptureTransferState();sourceModel=model;if(model!=Owner.Model)model.Changed+=Refresh;
            IsDragging=true;SourceIndex=index;pointer=data.pointerId;sourceView=view;
            HeldAmount=model.GetSlot(index).Count;IsHoldingPartial=false;
            data.eligibleForClick=false;
            sourceView.Icon.color=new Color(1,1,1,.35f);
            var color=sourceView.Count.color;color.a=.35f;sourceView.Count.color=color;
            DropZone.transform.SetAsLastSibling();Ghost.SetAsLastSibling();Ghost.gameObject.SetActive(true);DropZone.SetActive(true);Refresh();Move(data);
        }
        public void BeginEquipment(ArmorSlot slot,PointerEventData data)
        {
            if(IsDragging || Session.State!=SessionState.Inventory)return;
            Equipment=Owner.GetComponent<PlayerEquipment>();
            var item=slot==ArmorSlot.Head ? Equipment.HeadArmor : Equipment.BodyArmor;if(item==null)return;
            equipmentSource=slot;sourceModel=Owner.Model;IsDragging=true;HeldAmount=1;pointer=data.pointerId;
            data.eligibleForClick=false;Ghost.SetAsLastSibling();Ghost.gameObject.SetActive(true);DropZone.SetActive(false);Refresh();Move(data);
        }
        public void DropEquipment(ArmorSlot slot,PointerEventData data)
        {
            if(!IsDragging || equipmentSource.HasValue || data.pointerId!=pointer || sourceModel!=Owner.Model)return;
            var stack=sourceModel.GetSlot(SourceIndex);
            if(!stack.IsEmpty && stack.Item.Armor!=null && stack.Item.Armor.Slot==slot)Owner.GetComponent<PlayerEquipment>().TryEquipFromSlot(SourceIndex);
            else PlayerFeedback.ShowKey("feedback.armor_slot");Cancel();
        }
        public bool CanEquip(ArmorSlot slot)
        {if(!IsDragging || equipmentSource.HasValue || sourceModel!=Owner.Model)return false;var stack=sourceModel.GetSlot(SourceIndex);return !stack.IsEmpty && stack.Item.Armor!=null && stack.Item.Armor.Slot==slot;}
        public void Move(PointerEventData data)
        {
            if (!IsDragging || data.pointerId != pointer) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRect,data.position,data.pressEventCamera!=null ? data.pressEventCamera : CanvasRect.GetComponentInParent<Canvas>().worldCamera,out var position))
                Ghost.anchoredPosition=position;
        }
        public void Hover(int index,InventorySlotView view,bool chest=false)
        {
            if (!IsDragging) return;
            ClearHover();hoveredView=view;hoveredIndex=index;hoveredChest=chest;
            view.DropMarker.enabled=true;
            var destination=Model(chest);var compatible=equipmentSource.HasValue ? !chest && destination.GetSlot(index).IsEmpty : InventoryModel.CanTransfer(sourceModel,SourceIndex,destination,index,HeldAmount);
            view.DropMarker.color=compatible ? new Color(.57f,.75f,.49f,.38f) : new Color(.75f,.36f,.27f,.38f);
        }
        public void Leave(InventorySlotView view) { if (hoveredView==view) ClearHover(); }
        private void ClearHover() { if (hoveredView!=null) hoveredView.DropMarker.enabled=false;hoveredView=null; }
        public void Drop(int index,PointerEventData data,bool chest=false)
        {
            if (!IsDragging || data.pointerId!=pointer) return;
            if(equipmentSource.HasValue){if(!chest)Equipment.Unequip(equipmentSource.Value,index);else PlayerFeedback.ShowKey("feedback.armor_bag");}
            else Transfer(sourceModel,SourceIndex,Model(chest),index,HeldAmount);
            Cancel();
        }
        public void End(PointerEventData data) { if (IsDragging && data.pointerId==pointer && !IsHoldingPartial) Cancel(); }
        public void Select(int index,bool chest)
        {selectedModel=Model(chest);selectedIndex=index;UpdateSplit();SelectionChanged?.Invoke();}
        public void OpenSplit()=>OpenSelected(false);
        public void OpenItemActions()=>OpenSelected(true);
        private void OpenSelected(bool actions)
        {
            if(Session.State!=SessionState.Inventory || IsDragging || selectedModel==null || selectedIndex<0)return;
            var chest=Storage!=null && Storage.Chest!=null && selectedModel==Storage.Chest.Model;
            var view=chest ? GetComponent<ChestPanel>().ChestSlots[selectedIndex] : Storage!=null && Storage.IsOpen ? GetComponent<ChestPanel>().PlayerSlots[selectedIndex] : GetComponent<InventoryPanel>().Slots[selectedIndex];
            var stack=selectedModel.GetSlot(selectedIndex);if(stack.IsEmpty)return;
            if(actions)SplitPanel.OpenActions(selectedIndex,view,chest,stack.Count,stack.Item);else SplitPanel.Open(selectedIndex,view,chest,stack.Count,stack.Item);
        }
        public void HoldPartial(int index,InventorySlotView view,bool chest,int amount)
        {
            if(Session.State!=SessionState.Inventory)return;
            var model=Model(chest);if(model==null || model.GetSlot(index).Count<=amount || amount<1)return;
            var canvas=CanvasRect.GetComponentInParent<Canvas>();var scale=canvas.scaleFactor;
            var data=new PointerEventData(EventSystem.current){pointerId=-734,position=RectTransformUtility.WorldToScreenPoint(canvas.worldCamera,view.transform.position)+new Vector2(95,95)*scale};
            Begin(index,view,data,chest);IsHoldingPartial=true;HeldAmount=amount;HeldCancel.transform.SetAsLastSibling();HeldCancel.SetActive(true);Refresh();
        }
        public void BeginHeld(PointerEventData data){if(IsHoldingPartial){pointer=data.pointerId;Move(data);}}
        public bool PlaceHeld(int index,bool chest)
        {if(!IsHoldingPartial)return false;if(!Transfer(sourceModel,SourceIndex,Model(chest),index,HeldAmount)){PlayerFeedback.ShowKey("feedback.slot_space");return false;}Cancel();return true;}
        public void DropHeldWorld(){if(IsHoldingPartial && ItemDrop.Drop(sourceModel,SourceIndex,HeldAmount))Cancel();}
        private bool Transfer(InventoryModel source,int from,InventoryModel target,int to,int amount)
        {return source==Owner.Model && target==Owner.Model ? Owner.TransferSlots(from,to,amount) : Storage!=null && Storage.Authority!=null ? Storage.Authority.Transfer(source,from,target,to,amount) : InventoryModel.Transfer(source,from,target,to,amount);}
        private void UpdateSplit()
        {
            var valid=Session.State==SessionState.Inventory && !IsDragging && selectedModel!=null && selectedIndex>=0 && !selectedModel.GetSlot(selectedIndex).IsEmpty && selectedModel.GetSlot(selectedIndex).Count>1;
            SplitButton.interactable=valid;if(ChestSplitButton!=null)ChestSplitButton.interactable=valid;
        }
        public void DropWorld(PointerEventData data)
        {if(!IsDragging || data.pointerId!=pointer)return;if(!equipmentSource.HasValue)ItemDrop.Drop(sourceModel,SourceIndex,HeldAmount);Cancel();}
        private void Refresh()
        {
            UpdateSplit();
            if (!IsDragging) return;
            if(equipmentSource.HasValue){var item=equipmentSource==ArmorSlot.Head ? Equipment.HeadArmor : Equipment.BodyArmor;if(item==null){Cancel();return;}GhostIcon.sprite=item.Icon;GhostCount.text="1";return;}
            var stack=sourceModel.GetSlot(SourceIndex);
            if (stack.IsEmpty || stack.Count<HeldAmount) { Cancel();return; }
            GhostIcon.sprite=stack.Item.Icon;GhostCount.text=HeldAmount.ToString();remainder=(stack.Count-HeldAmount).ToString();
            if (hoveredView!=null) Hover(hoveredIndex,hoveredView,hoveredChest);
        }
        private void StateChanged() { if (Session.State!=SessionState.Inventory){selectedModel=null;selectedIndex=-1;Cancel();SplitPanel.Cancel();}UpdateSplit(); }
        private void StorageChanged(){selectedModel=null;selectedIndex=-1;Cancel();UpdateSplit();SelectionChanged?.Invoke();}
        public void Cancel()
        {
            if(sourceView!=null && sourceModel!=null){var stack=sourceModel.GetSlot(SourceIndex);sourceView.Count.text=stack.IsEmpty ? string.Empty : stack.Count.ToString();}
            if(sourceModel!=null && sourceModel!=Owner.Model)sourceModel.Changed-=Refresh;
            sourceModel=null;equipmentSource=null;IsDragging=IsHoldingPartial=false;HeldAmount=0;Ghost.gameObject.SetActive(false);DropZone.SetActive(false);if(HeldCancel!=null)HeldCancel.SetActive(false);ClearHover();
            if (sourceView!=null)
            {
                sourceView.Icon.color=Color.white;
                var color=sourceView.Count.color;color.a=1;sourceView.Count.color=color;
            }
            sourceView=null;
            UpdateSplit();
        }
    }
}
