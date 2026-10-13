using UnityEngine;
using UnityEngine.Rendering;

namespace Wildfolk
{
    public sealed class DepthSort : MonoBehaviour
    {
        public SortingGroup Visual;
        private void LateUpdate() => Visual.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 20);
    }
}
