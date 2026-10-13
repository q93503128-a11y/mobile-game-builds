using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class HoldAttackInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public PlayerCombat Combat;
        public GameplaySession Session;
        public Image Surface;
        private int? pointer;

        public void OnPointerDown(PointerEventData data)
        {
            if (!Session.IsRunning || pointer.HasValue) return;
            pointer = data.pointerId;
            Combat.TryAttack();
            Surface.color = new Color(.82f,.85f,.75f,1);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (pointer == data.pointerId) ResetInput();
        }

        private void Update()
        {
            if (!Session.IsRunning) { ResetInput(); return; }
            if (pointer.HasValue || (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)) Combat.TryAttack();
        }

        public void ResetInput() { pointer = null; Surface.color = Color.white; }
        private void OnDisable() => ResetInput();
        private void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
    }
}
