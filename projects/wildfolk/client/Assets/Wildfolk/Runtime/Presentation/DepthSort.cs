using UnityEngine;
using UnityEngine.Rendering;

namespace Wildfolk
{
    public sealed class DepthSort : MonoBehaviour
    {
        public SortingGroup Visual;
        private void LateUpdate() => Visual.sortingOrder = WorldSorting.At(transform.position.y);
    }
}
