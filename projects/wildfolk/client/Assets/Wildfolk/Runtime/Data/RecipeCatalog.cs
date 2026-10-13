using UnityEngine;

namespace Wildfolk
{
    [CreateAssetMenu(menuName = "Wildfolk/Recipe Catalog")]
    public sealed class RecipeCatalog : ScriptableObject
    {
        public RecipeDefinition[] Recipes;
    }
}
