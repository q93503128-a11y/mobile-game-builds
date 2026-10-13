using UnityEngine;
using UnityEngine.EventSystems;

namespace Wildfolk
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Thumb;
        public GameplaySession Session;
        public Vector2 Value { get; private set; }
        private RectTransform area;
        private int? pointer;

        private void Awake() => area = (RectTransform)transform;

        public void OnPointerDown(PointerEventData data)
        {
            if (!Session.IsRunning || pointer.HasValue) return;
            pointer = data.pointerId;
            OnDrag(data);
        }

        public void OnDrag(PointerEventData data)
        {
            if (pointer != data.pointerId || !Session.IsRunning) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, data.position, data.pressEventCamera, out var local)) return;
            var radius = area.rect.width * .36f;
            Value = Vector2.ClampMagnitude(local / radius, 1);
            Thumb.anchoredPosition = Value * radius;
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (pointer == data.pointerId) ResetInput();
        }

        public void ResetInput()
        {
            Value = Vector2.zero;
            pointer = null;
            Thumb.anchoredPosition = Vector2.zero;
        }

        private void OnDisable() => ResetInput();
        private void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
    }
}
