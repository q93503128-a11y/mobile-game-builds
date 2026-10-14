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
        public Image InputIcon;
        public Text Name;
        public Text Materials;
        public Text Status;
        public Button CraftButton;
        public Text ButtonLabel;
        public Image Progress;
        private int previousSeconds=-2;
        private void Update()
        {
            if(Recipe.ProcessingSeconds<=0) return;
            var job=Crafting.Processor;
            if(job!=null && job.Recipe==Recipe) Progress.fillAmount=job.Progress;
            var seconds=job!=null && job.Recipe==Recipe ? Mathf.CeilToInt(job.Remaining) : -1;
            if(seconds==previousSeconds) return;
            previousSeconds=seconds;Refresh();
        }

        public void Craft() => Crafting.Craft(Recipe);
        public void Refresh()
        {
            Icon.sprite=Recipe.Result.Icon;
            Name.text=Recipe.Result.DisplayName+(Recipe.ProcessingSeconds>0 ? " ×"+Recipe.ResultCount : string.Empty);
            InputIcon.gameObject.SetActive(Recipe.ProcessingSeconds>0);
            if(Recipe.ProcessingSeconds>0) InputIcon.sprite=Recipe.Ingredients[0].Item.Icon;
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
            CraftButton.interactable=(state==CraftStatus.Ready || state==CraftStatus.Collect) && Crafting.Session.State==SessionState.Inventory;
            var job=Crafting.Processor;
            var active=Recipe.ProcessingSeconds>0 && job!=null && job.Recipe==Recipe;
            var batch=active ? job.BatchCount : Recipe.ProcessingSeconds>0 && job!=null ? job.BatchSize(Recipe,Crafting.Inventory.Model) : 1;
            if(Recipe.ProcessingSeconds>0)Name.text=Recipe.Result.DisplayName+" ×"+(Mathf.Max(1,batch)*Recipe.ResultCount);
            if(active)
            {
                Materials.text=Recipe.Ingredients[0].Item.DisplayName+" ×"+(Recipe.Ingredients[0].Count*batch);
                Materials.fontSize=18;Materials.color=new Color32(197,206,155,255);
            }
            ButtonLabel.text=state==CraftStatus.Collect ? "Take" : Recipe.ProcessingSeconds>0 ? Recipe.Processing==ProcessingKind.Smelting ? "Smelt" : "Cook" : "Craft";
            Progress.gameObject.SetActive(active);Progress.fillAmount=active ? job.Progress : 0;
            Status.text=state==CraftStatus.NoSpace ? "Inventory full" : active ?
                job.IsReady ? "Ready" : (Recipe.Processing==ProcessingKind.Smelting ? "Smelting · " : "Cooking · ")+Mathf.CeilToInt(job.Remaining)+" s" :
                Recipe.ProcessingSeconds>0 ? Recipe.ProcessingSeconds.ToString("0")+" s" : string.Empty;
            if(Recipe.ProcessingSeconds>0 && job!=null)
            {
                var capacity=job.Station.Definition.ParallelCapacity;
                Materials.text=Recipe.Ingredients[0].Item.DisplayName+" "+(active ? "×"+(Recipe.Ingredients[0].Count*batch) : Crafting.Inventory.Model.CountItem(Recipe.Ingredients[0].Item).ToString())+" · Batch "+batch+" / "+capacity;
                if(!active)Status.text+=" · Up to "+capacity;
            }
        }
    }
}
