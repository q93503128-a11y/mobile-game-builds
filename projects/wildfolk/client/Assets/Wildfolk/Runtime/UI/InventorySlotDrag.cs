using UnityEngine;
using UnityEngine.EventSystems;

namespace Wildfolk
{
    public sealed class InventorySlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IDropHandler, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler,IPointerDownHandler,IPointerUpHandler
    {
        public InventoryDragController Controller;
        public InventorySlotView View;
        public int Index;
        public bool IsChest;
        private bool pressed,longPressed;private float pressedAt;private Vector2 pressedPosition;private PointerEventData press;
        private UnityEngine.UI.ScrollRect scrolling;
        public void OnPointerDown(PointerEventData data){pressed=true;longPressed=false;pressedAt=Time.unscaledTime;pressedPosition=data.position;press=data;}
        public void OnPointerUp(PointerEventData data)=>pressed=false;
        private void Update()
        {
            if(!pressed || longPressed || Controller.IsDragging || Time.unscaledTime-pressedAt<.45f || Vector2.Distance(press.position,pressedPosition)>16)return;
            longPressed=true;Controller.Select(Index,IsChest);Controller.OpenItemActions();
        }
        public void OnPointerClick(PointerEventData data){if(longPressed || scrolling!=null)return;if(Controller.IsHoldingPartial)Controller.PlaceHeld(Index,IsChest);else Controller.Select(Index,IsChest);}
        public void OnBeginDrag(PointerEventData data)
        {
            pressed=false;if(longPressed)return;var scroll=IsChest ? GetComponentInParent<UnityEngine.UI.ScrollRect>() : null;
            var extended=data as UnityEngine.InputSystem.UI.ExtendedPointerEventData;
            var touch=extended!=null ? extended.pointerType==UnityEngine.InputSystem.UI.UIPointerType.Touch : data.pointerId>=0;
            var delta=data.position-pressedPosition;var empty=IsChest && Controller.Storage.Chest.Model.GetSlot(Index).IsEmpty;
            if(scroll!=null && !Controller.IsHoldingPartial && (empty || touch && Time.unscaledTime-pressedAt<.18f && Mathf.Abs(delta.y)>Mathf.Abs(delta.x)))
            {scrolling=scroll;data.eligibleForClick=false;scroll.OnInitializePotentialDrag(data);scroll.OnBeginDrag(data);return;}
            if(Controller.IsHoldingPartial)Controller.BeginHeld(data);else Controller.Begin(Index,View,data,IsChest);
        }
        public void OnDrag(PointerEventData data){if(scrolling!=null)scrolling.OnDrag(data);else Controller.Move(data);}
        public void OnEndDrag(PointerEventData data){if(scrolling!=null){scrolling.OnEndDrag(data);scrolling=null;}else Controller.End(data);}
        public void OnDrop(PointerEventData data) => Controller.Drop(Index,data,IsChest);
        public void OnPointerEnter(PointerEventData data) => Controller.Hover(Index,View,IsChest);
        public void OnPointerExit(PointerEventData data) => Controller.Leave(View);
    }
}
