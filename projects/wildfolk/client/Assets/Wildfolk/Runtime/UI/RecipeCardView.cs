using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class RecipeCardView : MonoBehaviour,UnityEngine.EventSystems.IPointerClickHandler
    {
        public RecipeDefinition Recipe;public PlayerCrafting Crafting;public Image Icon,InputIcon,Progress;public Text Name,Materials,Status,ButtonLabel,NewLabel;public Button CraftButton;
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData data)=>Crafting.SetSelected(Recipe);
        public void Craft(){Crafting.SetSelected(Recipe);Crafting.Craft(Recipe);Refresh();}
        public void Refresh()
        {
            NewLabel.gameObject.SetActive(Crafting.IsNew(Recipe));Icon.sprite=Recipe.Result.Icon;Name.text=Recipe.Result.LocalizedName;
            var requirements=new StringBuilder();foreach(var ingredient in Recipe.Ingredients){var owned=Crafting.Inventory.Model.CountItem(ingredient.Item);if(requirements.Length>0)requirements.Append("  ·  ");requirements.Append("<color=#").Append(owned>=ingredient.Count ? "C5CE9B" : "D99078").Append(">").Append(L.Format("ui.crafting.material",ingredient.Item.LocalizedName,owned,ingredient.Count)).Append("</color>");}Materials.text=requirements.ToString();Materials.color=Color.white;
            var state=Crafting.GetStatus(Recipe);CraftButton.interactable=(state==CraftStatus.Ready || state==CraftStatus.Collect) && Crafting.Session.State==SessionState.Inventory;
            var job=Crafting.Processor;var active=job!=null && job.Recipe==Recipe;Progress.gameObject.SetActive(active);Progress.fillAmount=active ? job.Progress : 0;
            ButtonLabel.text=job!=null && job.Sequential ? "+1" : L.Text(state==CraftStatus.Collect ? "ui.action.take" : Recipe.Processing==ProcessingKind.Smelting ? "ui.action.smelt" : Recipe.ProcessingSeconds>0 ? "ui.action.cook" : "ui.action.craft");
            Status.text=state==CraftStatus.StationRequired ? L.Format("ui.crafting.station",L.Text("item.workbench_ii.name")) : state==CraftStatus.NoSpace ? L.Text("ui.crafting.full") : active ? job.Remaining<=0 ? L.Text("ui.crafting.ready") : L.Format(Recipe.Processing==ProcessingKind.Smelting ? "ui.crafting.smelting" : "ui.crafting.cooking",Mathf.CeilToInt(job.Remaining)) : Recipe.ProcessingSeconds>0 ? L.Format("ui.crafting.seconds",Recipe.ProcessingSeconds) : "";
        }
    }
}