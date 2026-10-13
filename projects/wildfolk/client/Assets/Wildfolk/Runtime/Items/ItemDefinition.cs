using UnityEngine;

namespace Wildfolk
{
    public enum ItemCategory { Material, Tool }

    [CreateAssetMenu(menuName = "Wildfolk/Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Icon;
        [Min(1)] public int MaxStack;
        public ItemCategory Category;
        public ToolDefinition Tool;
    }
}
