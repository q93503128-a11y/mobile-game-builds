using System;
using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerEquipment : MonoBehaviour
    {
        public PlayerInventory Inventory;
        public PlayerCombat Combat;
        public SpriteRenderer HeldVisual;
        public ToolDefinition ActiveTool { get; private set; }
        public ItemDefinition SelectedItem { get; private set; }
        public PlaceableDefinition ActivePlaceable => SelectedItem == null ? null : SelectedItem.Placement;
        public event Action Changed;

        private void Start()
        {
            Inventory.Model.Changed += Refresh;
            Inventory.SelectionChanged += Refresh;
            Refresh();
        }
        private void OnDestroy()
        {
            if (Inventory != null && Inventory.Model != null) Inventory.Model.Changed -= Refresh;
            if (Inventory != null) Inventory.SelectionChanged -= Refresh;
        }
        private void Refresh()
        {
            var stack = Inventory.SelectedStack;
            var item = stack.IsEmpty ? null : stack.Item;
            var next = stack.IsEmpty ? null : stack.Item.Tool;
            if (item == SelectedItem && next == ActiveTool && HeldVisual.enabled == (next != null)) return;
            Combat.CancelAttack();
            SelectedItem = item;
            ActiveTool = next;
            HeldVisual.sprite = ActiveTool == null ? null : ActiveTool.HeldSprite;
            HeldVisual.enabled = ActiveTool != null;
            Changed?.Invoke();
        }
    }
}
