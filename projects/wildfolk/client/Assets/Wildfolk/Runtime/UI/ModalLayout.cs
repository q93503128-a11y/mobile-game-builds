using UnityEngine;
namespace Wildfolk
{
    public sealed class ModalLayout : MonoBehaviour
    {
        public RectTransform SafeArea;
        public Vector2 ReferenceSize;
        public float TopReserve=40;
        public float BottomReserve=22;
        public float HorizontalReserve=184;
        private Vector2 previousSize;
        private void OnEnable() => Apply();
        private void LateUpdate() {if(SafeArea.rect.size!=previousSize)Apply();}
        private void Apply()
        {
            previousSize=SafeArea.rect.size;
            var availableHeight=Mathf.Max(1,previousSize.y-TopReserve-BottomReserve);
            var scale=Mathf.Min(1,availableHeight/ReferenceSize.y,Mathf.Max(1,previousSize.x-HorizontalReserve*2)/ReferenceSize.x);
            var rect=(RectTransform)transform;rect.localScale=new Vector3(scale,scale,1);
            rect.anchoredPosition=Vector2.zero;
        }
    }
}
