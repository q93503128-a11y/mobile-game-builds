using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class RecipeCardView : MonoBehaviour
    {
        public RecipeDefinition Recipe;
        public PlayerCrafting Crafting;
        public Image Icon;
        public Text Name;
        public Text Materials;
        public Text Status;
        public Button CraftButton;

        public void Craft() => Crafting.Craft(Recipe);
        public void Refresh()
        {
            Icon.sprite=Recipe.Result.Icon;
            Name.text=Recipe.Result.DisplayName;
            var requirements=new StringBuilder();
            var enough=true;
            foreach(var ingredient in Recipe.Ingredients)
            {
                var owned=Crafting.Inventory.Model.CountItem(ingredient.Item);
                if(requirements.Length>0) requirements.Append("  ·  ");
                requirements.Append(ingredient.Item.DisplayName).Append(" ").Append(owned).Append(" / ").Append(ingredient.Count);
                enough &= owned>=ingredient.Count;
            }
            Materials.text=requirements.ToString();
            Materials.color=enough ? new Color32(197,206,155,255) : new Color32(197,135,106,255);
            var state=Crafting.GetStatus(Recipe);
            CraftButton.interactable=state==CraftStatus.Ready && Crafting.Session.State==SessionState.Inventory;
            Status.text=state==CraftStatus.NoSpace ? "Inventory full" : string.Empty;
        }
    }
}
