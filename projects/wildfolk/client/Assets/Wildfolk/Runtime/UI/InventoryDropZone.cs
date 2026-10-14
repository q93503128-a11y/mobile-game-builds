using UnityEngine;
using UnityEngine.EventSystems;
namespace Wildfolk
{
    public sealed class InventoryDropZone : MonoBehaviour,IDropHandler
    {
        public InventoryDragController Controller;
        public void OnDrop(PointerEventData data)=>Controller.DropWorld(data);
    }
}
