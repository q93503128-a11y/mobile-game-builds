using UnityEngine;

namespace Wildfolk.Presentation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaLayout : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastArea;
        private Vector2Int lastSize;

        private void Awake() => rectTransform = GetComponent<RectTransform>();

        private void LateUpdate()
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            var area = Screen.safeArea;
            if (size.x == 0 || size.y == 0 || (area == lastArea && size == lastSize)) return;
            rectTransform.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
            rectTransform.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            lastArea = area;
            lastSize = size;
        }
    }
}
