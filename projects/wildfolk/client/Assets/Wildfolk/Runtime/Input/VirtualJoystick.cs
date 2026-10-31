using UnityEngine;
using UnityEngine.EventSystems;
namespace Wildfolk
{
    // The input zone and transient visual have independent bounds and pointer ownership.
    public sealed class VirtualJoystick : MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        public RectTransform Thumb,Visual;
        public CanvasGroup Feedback;
        public GameplaySession Session;
        [Range(0,.5f)]public float DeadZone=.12f;
        public Vector2 Value=>Session!=null&&Session.IsRunning?value:Vector2.zero;
        private Vector2 value,origin;private RectTransform area;private int? pointer;private GameplaySession observed;
        private void Awake(){area=(RectTransform)transform;}
        private void OnEnable(){BindSession(Session);}
        public void BindSession(GameplaySession session)
        {
            if(observed!=null)observed.StateChanged-=ResetInput;
            Session=observed=session;if(observed!=null)observed.StateChanged+=ResetInput;ResetInput();
        }
        public void OnPointerDown(PointerEventData data)
        {
            if(Session==null||!Session.IsRunning||pointer.HasValue)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(area,data.position,data.pressEventCamera,out origin)||!area.rect.Contains(origin))return;
            pointer=data.pointerId;Visual.anchoredPosition=origin;Feedback.alpha=.65f;OnDrag(data);
        }
        public void OnDrag(PointerEventData data)
        {
            if(pointer!=data.pointerId)return;if(Session==null||!Session.IsRunning){ResetInput();return;}
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(area,data.position,data.pressEventCamera,out var local))return;
            var radius=Mathf.Max(1,Visual.rect.width*.36f*Visual.localScale.x);
            var raw=Vector2.ClampMagnitude((local-origin)/radius,1);value=raw.magnitude<DeadZone?Vector2.zero:raw;
            Thumb.anchoredPosition=raw*(Visual.rect.width*.36f);
        }
        public void OnPointerUp(PointerEventData data){if(pointer==data.pointerId)ResetInput();}
        public void ResetInput(){value=Vector2.zero;pointer=null;if(Thumb!=null)Thumb.anchoredPosition=Vector2.zero;if(Feedback!=null)Feedback.alpha=0;}
        private void OnDisable(){if(observed!=null)observed.StateChanged-=ResetInput;observed=null;ResetInput();}
        private void OnApplicationFocus(bool focused){if(!focused)ResetInput();}
        private void OnApplicationPause(bool paused){if(paused)ResetInput();}
    }
}
