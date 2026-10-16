using UnityEngine;
using UnityEngine.EventSystems;
namespace Wildfolk
{
    public sealed class NetworkAttackInput : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
    {
        public MultiplayerArena Arena;
        private int? pointer;
        private float next;
        public void OnPointerDown(PointerEventData e){if(pointer.HasValue)return;pointer=e.pointerId;Fire();}
        public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)pointer=null;}
        private void Fire(){Arena.LocalPlayer?.Commands.Attack();next=Time.unscaledTime+.12f;}
        private void Update(){if(pointer.HasValue && Time.unscaledTime>=next)Fire();}
        private void OnDisable()=>pointer=null;
        private void OnApplicationFocus(bool focus){if(!focus)pointer=null;}
    }
}
