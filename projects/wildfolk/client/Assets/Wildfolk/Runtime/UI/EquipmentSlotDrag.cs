using UnityEngine;
using UnityEngine.EventSystems;
namespace Wildfolk
{
    public sealed class EquipmentSlotDrag : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IDropHandler,IPointerEnterHandler,IPointerExitHandler
    {
        public InventoryDragController Controller;
        public ArmorSlot Slot;
        private UnityEngine.UI.Image surface;
        private Color resting;
        private void Awake(){surface=GetComponent<UnityEngine.UI.Image>();resting=surface.color;}
        public void OnBeginDrag(PointerEventData data)=>Controller.BeginEquipment(Slot,data);
        public void OnDrag(PointerEventData data)=>Controller.Move(data);
        public void OnEndDrag(PointerEventData data)=>Controller.End(data);
        public void OnDrop(PointerEventData data)=>Controller.DropEquipment(Slot,data);
        public void OnPointerEnter(PointerEventData data){if(Controller.IsDragging)surface.color=Controller.CanEquip(Slot) ? new Color(.75f,.95f,.65f,1) : new Color(.85f,.5f,.4f,1);}
        public void OnPointerExit(PointerEventData data)=>surface.color=resting;
        private void LateUpdate(){if(!Controller.IsDragging)surface.color=resting;}
    }
}
