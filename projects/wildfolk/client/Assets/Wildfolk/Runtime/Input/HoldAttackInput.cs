using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class HoldAttackInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public PlayerPrimaryAction Action;
        public GameplaySession Session;
        public Image Surface;
        private int? pointer;
        private bool repeat;
        public void OnPointerDown(PointerEventData data)
        {
            if(!Session.IsRunning || pointer.HasValue) return;
            pointer=data.pointerId;
            repeat=Action.Kind==PrimaryActionKind.Attack;
            Action.TryUse();
            Surface.color=new Color(.82f,.85f,.75f,1);
        }
        public void OnPointerUp(PointerEventData data) { if(pointer==data.pointerId) ResetInput(); }
        private void Update()
        {
            if(!Session.IsRunning) {ResetInput();return;}
            if(pointer.HasValue && repeat && Action.Kind==PrimaryActionKind.Attack) Action.TryUse();
        }
        public void ResetInput() {pointer=null;repeat=false;Surface.color=Color.white;}
        private void OnDisable()=>ResetInput();
        private void OnApplicationFocus(bool focused) {if(!focused) ResetInput();}
    }
}
