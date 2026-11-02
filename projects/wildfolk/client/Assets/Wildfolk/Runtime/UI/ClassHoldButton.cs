using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class ClassHoldButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
    {
        public event Action<bool> Held;private bool down;private float next;private Button surface;
        private void Awake()=>surface=GetComponent<Button>();
        public void OnPointerDown(PointerEventData e){if(!isActiveAndEnabled||surface==null||!surface.IsActive()||!surface.IsInteractable())return;down=true;next=0;Held?.Invoke(true);}
        public void OnPointerUp(PointerEventData e)=>Release();
        public void OnPointerExit(PointerEventData e)=>Release();
        private void Release(){if(!down)return;down=false;Held?.Invoke(false);}
        private void OnDisable()=>Release();
        private void Update(){if(!down)return;if(surface==null||!surface.IsActive()||!surface.IsInteractable()){Release();return;}if(Time.unscaledTime>=next){next=Time.unscaledTime+.12f;Held?.Invoke(true);}}
    }
}
