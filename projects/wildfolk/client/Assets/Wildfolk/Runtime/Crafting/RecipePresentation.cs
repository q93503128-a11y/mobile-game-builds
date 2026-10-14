using System;
namespace Wildfolk
{
    public static class RecipePresentation
    {
        public static int Compare(RecipeDefinition a,RecipeDefinition b)
        {var category=a.Category.CompareTo(b.Category);if(category!=0)return category;var tier=(a.Tier==null ? 0 : a.Tier.Rank).CompareTo(b.Tier==null ? 0 : b.Tier.Rank);if(tier!=0)return tier;var order=a.SortOrder.CompareTo(b.SortOrder);return order!=0 ? order : StringComparer.Ordinal.Compare(a.Id,b.Id);}
        public static RecipeDefinition[] Sorted(RecipeCatalog catalog){var list=(RecipeDefinition[])catalog.Recipes.Clone();Array.Sort(list,Compare);return list;}
    }
}
