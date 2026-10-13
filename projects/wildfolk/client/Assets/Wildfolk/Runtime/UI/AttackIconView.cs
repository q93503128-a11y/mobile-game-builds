using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class AttackIconView : MonoBehaviour
    {
        public PlayerEquipment Equipment;
        public Image Icon;
        public Sprite FistIcon;

        private void Start() { Equipment.Changed += Refresh; Refresh(); }
        private void OnDestroy() { if (Equipment != null) Equipment.Changed -= Refresh; }
        private void Refresh()
        {
            var stack = Equipment.Inventory.SelectedStack;
            Icon.sprite = Equipment.ActiveTool == null ? FistIcon : stack.Item.Icon;
        }
    }
}
