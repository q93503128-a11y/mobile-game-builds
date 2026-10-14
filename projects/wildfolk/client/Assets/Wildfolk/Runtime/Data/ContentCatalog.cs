using System;
using UnityEngine;
namespace Wildfolk
{
    [CreateAssetMenu(menuName="Wildfolk/Content Catalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        public ItemDefinition[] Items=Array.Empty<ItemDefinition>();
        public RecipeDefinition[] Recipes=Array.Empty<RecipeDefinition>();
        public ItemDefinition Item(string id){foreach(var item in Items)if(item.Id==id)return item;throw new InvalidOperationException("Unknown item ID: "+id);}
        public RecipeDefinition Recipe(string id){foreach(var recipe in Recipes)if(recipe.Id==id)return recipe;throw new InvalidOperationException("Unknown recipe ID: "+id);}
    }
}
