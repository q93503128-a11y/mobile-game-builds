using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class InventorySlotView : MonoBehaviour
    {
        public Image Icon;
        public Text Count;
        public Image Selection;

        public void Show(ItemStack stack,bool selected)
        {
            Icon.enabled=!stack.IsEmpty;
            Icon.sprite=stack.IsEmpty ? null : stack.Item.Icon;
            Count.text=stack.IsEmpty ? string.Empty : stack.Count.ToString();
            Selection.enabled=selected;
        }
    }
}
