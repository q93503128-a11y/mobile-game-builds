using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class InventoryDragController : MonoBehaviour
    {
        public PlayerInventory Owner;
        public GameplaySession Session;
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

        private void Start() { Owner.Model.Changed += Refresh; Session.StateChanged += StateChanged; }
        private void OnDestroy()
        {
            if (Owner != null && Owner.Model != null) Owner.Model.Changed -= Refresh;
            if (Session != null) Session.StateChanged -= StateChanged;
        }
        public void Begin(int index, InventorySlotView view, PointerEventData data)
        {
            if (IsDragging || Session.State != SessionState.Inventory || Owner.Model.GetSlot(index).IsEmpty) return;
            IsDragging=true;SourceIndex=index;pointer=data.pointerId;sourceView=view;
            data.eligibleForClick=false;
            sourceView.Icon.color=new Color(1,1,1,.35f);
            var color=sourceView.Count.color;color.a=.35f;sourceView.Count.color=color;
            Ghost.gameObject.SetActive(true);Refresh();Move(data);
        }
        public void Move(PointerEventData data)
        {
            if (!IsDragging || data.pointerId != pointer) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRect,data.position,data.pressEventCamera,out var position))
                Ghost.anchoredPosition=position;
        }
        public void Hover(int index,InventorySlotView view)
        {
            if (!IsDragging) return;
            ClearHover();hoveredView=view;hoveredIndex=index;
            view.DropMarker.enabled=true;
            view.DropMarker.color=Owner.Model.CanMoveStack(SourceIndex,index) ? new Color(.57f,.75f,.49f,.38f) : new Color(.75f,.36f,.27f,.38f);
        }
        public void Leave(InventorySlotView view) { if (hoveredView==view) ClearHover(); }
        private void ClearHover() { if (hoveredView!=null) hoveredView.DropMarker.enabled=false;hoveredView=null; }
        public void Drop(int index,PointerEventData data)
        {
            if (!IsDragging || data.pointerId!=pointer) return;
            Owner.Model.MoveStack(SourceIndex,index);
            Cancel();
        }
        public void End(PointerEventData data) { if (IsDragging && data.pointerId==pointer) Cancel(); }
        private void Refresh()
        {
            if (!IsDragging) return;
            var stack=Owner.Model.GetSlot(SourceIndex);
            if (stack.IsEmpty) { Cancel();return; }
            GhostIcon.sprite=stack.Item.Icon;GhostCount.text=stack.Count.ToString();
            if (hoveredView!=null) Hover(hoveredIndex,hoveredView);
        }
        private void StateChanged() { if (Session.State!=SessionState.Inventory) Cancel(); }
        public void Cancel()
        {
            IsDragging=false;Ghost.gameObject.SetActive(false);ClearHover();
            if (sourceView!=null)
            {
                sourceView.Icon.color=Color.white;
                var color=sourceView.Count.color;color.a=1;sourceView.Count.color=color;
            }
            sourceView=null;
        }
    }
}
