using UnityEngine;
using UnityEngine.EventSystems;

namespace Wildfolk
{
    public sealed class InventorySlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IDropHandler, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
    {
        public InventoryDragController Controller;
        public InventorySlotView View;
        public int Index;
        public bool IsChest;
        public void OnPointerClick(PointerEventData data)=>Controller.Select(Index,IsChest);
        public void OnBeginDrag(PointerEventData data) => Controller.Begin(Index,View,data,IsChest);
        public void OnDrag(PointerEventData data) => Controller.Move(data);
        public void OnEndDrag(PointerEventData data) => Controller.End(data);
        public void OnDrop(PointerEventData data) => Controller.Drop(Index,data,IsChest);
        public void OnPointerEnter(PointerEventData data) => Controller.Hover(Index,View,IsChest);
        public void OnPointerExit(PointerEventData data) => Controller.Leave(View);
    }
}
