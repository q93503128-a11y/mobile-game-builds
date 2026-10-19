using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Wildfolk
{
    public sealed class DirectionDrag : MonoBehaviour,IPointerDownHandler,IDragHandler
    {
        public Action<Vector2> Aim;
        public void OnPointerDown(PointerEventData data)=>OnDrag(data);
        public void OnDrag(PointerEventData data)
        {if(RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,data.position,data.pressEventCamera,out var point))Aim?.Invoke(point.normalized);}
    }
}
