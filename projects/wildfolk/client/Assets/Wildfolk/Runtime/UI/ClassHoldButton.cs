using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Wildfolk
{
    public sealed class ClassHoldButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        public event Action<bool> Held;private bool down;private float next;
        public void OnPointerDown(PointerEventData e){down=true;next=0;Held?.Invoke(true);}
        public void OnPointerUp(PointerEventData e)=>Release();
        public void OnPointerExit(PointerEventData e)=>Release();
        private void Release(){if(!down)return;down=false;Held?.Invoke(false);}
        private void OnDisable()=>Release();
        private void Update(){if(down && Time.unscaledTime>=next){next=Time.unscaledTime+.12f;Held?.Invoke(true);}}
    }
}
