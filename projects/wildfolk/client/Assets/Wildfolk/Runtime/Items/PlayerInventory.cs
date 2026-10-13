using UnityEngine;

namespace Wildfolk
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        public GameplaySettings Settings;
        public InventoryModel Model { get; private set; }
        private void Awake() => Model = new InventoryModel(Settings.InventorySlots);
    }
}
