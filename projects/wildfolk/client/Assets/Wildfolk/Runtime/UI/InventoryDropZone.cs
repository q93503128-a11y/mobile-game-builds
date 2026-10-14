using UnityEngine;
using UnityEngine.EventSystems;
namespace Wildfolk
{
    public sealed class InventoryDropZone : MonoBehaviour,IDropHandler,IPointerClickHandler
    {
        public InventoryDragController Controller;
        public void OnDrop(PointerEventData data)=>Controller.DropWorld(data);
        public void OnPointerClick(PointerEventData data)=>Controller.DropHeldWorld();
    }
}
