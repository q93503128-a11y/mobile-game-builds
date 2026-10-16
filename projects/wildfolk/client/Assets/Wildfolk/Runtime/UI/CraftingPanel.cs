using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class CraftingPanel : MonoBehaviour
    {
        public PlayerCrafting Owner;public GameObject View,QueueView;public GameObject[] Sections,Grids;
        public Image SelectedIcon;
        public Text Title,FilterLabel,UpgradeLabel,UpgradeRequirements,SelectedTitle,MaterialSummary,QueueState,CurrentItem;
        public ScrollRect UpgradeScroll;
        public Button UpgradeButton,AddOne,AddAll,Withdraw,Collect;public Button[] CategoryButtons;
        public Button InventoryButton;
        public RecipeCardView[] Cards;public ScrollRect Scroll;
        private StationKind previousStation=(StationKind)(-1);private int category=-1;private bool craftable;private float nextRefresh;
        private RecipeDefinition Selected=>Owner.SelectedRecipe;
        private void Update(){if(View.activeSelf && Owner.Processor!=null && Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.2f;RefreshProgress();}}
        private void Start(){Owner.Inventory.Model.Changed+=Refresh;Owner.StationChanged+=Refresh;Owner.Session.StateChanged+=Refresh;L.Changed+=Refresh;Refresh();}
        private void OnDestroy(){Owner.Inventory.Model.Changed-=Refresh;Owner.StationChanged-=Refresh;Owner.Session.StateChanged-=Refresh;L.Changed-=Refresh;}
        public void SelectCategory(int index){category=index;Scroll.verticalNormalizedPosition=1;Refresh();}
        public void ToggleFilter(){craftable=!craftable;Refresh();}
        public void EnqueueOne(){if(Selected!=null)Owner.Processor.Add(Selected,Owner.Inventory,1);Refresh();}
        public void EnqueueAll(){if(Selected!=null)Owner.Processor.AddAll(Selected,Owner.Inventory);Refresh();}
        public void WithdrawOne(){if(Selected!=null)Owner.Processor.Withdraw(Selected,Owner.Inventory);Refresh();}
        public void CollectResults(){if(!Owner.Processor.Collect(Owner.Inventory))PlayerFeedback.ShowKey("feedback.bag_full");Refresh();}
        private void RefreshProgress()
        {
            var job=Owner.Processor;if(job==null)return;
            foreach(var card in Cards)if(card.gameObject.activeSelf && card.Recipe==job.Recipe)card.RefreshProgress();
            if(job.Sequential)CurrentItem.text=job.Recipe==null ? L.Text("ui.crafting.idle") : job.Remaining<=0 ? L.Text("ui.crafting.blocked_output") : L.Format("ui.crafting.current",ProcessingName(job.Recipe),job.Remaining);
        }
        private static string ProcessingName(RecipeDefinition recipe){var text=new StringBuilder();foreach(var input in recipe.Ingredients){if(text.Length>0)text.Append(" + ");text.Append(input.Item.LocalizedName);}return L.Format("ui.crafting.input_output",text.ToString(),recipe.Result.LocalizedName);}
        public void Refresh()
        {
            View.SetActive(Owner.ScreenOpen && Owner.Session.State==SessionState.Inventory);if(!View.activeSelf)return;
            var station=Owner.AvailableStation;if(station!=previousStation){category=-1;craftable=false;previousStation=station;Scroll.verticalNormalizedPosition=1;}
            Title.text=station==StationKind.None ? L.Text("ui.crafting.hand") : Owner.Station.Definition.Station==StationKind.Workbench ? (string.IsNullOrEmpty(Owner.Station.Definition.TierDisplayKey) ? WorkbenchProgression.Name(Owner.Station.Definition.WorkbenchTier) : L.Text(Owner.Station.Definition.TierDisplayKey)) : Owner.Station.Definition.Item.LocalizedName;
            for(var i=0;i<CategoryButtons.Length;i++){var available=i==0;foreach(var card in Cards)if(card.Recipe.RequiredStation==station && (int)card.Recipe.Category==i-1)available=true;CategoryButtons[i].gameObject.SetActive(available);CategoryButtons[i].image.color=i-1==category ? new Color32(152,145,96,255) : Color.white;}
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)CategoryButtons[0].transform.parent);
            FilterLabel.text=L.Text(craftable ? "ui.crafting.ready_only" : "ui.crafting.show_all");
            foreach(var section in Sections)if(section!=null)section.SetActive(false);foreach(var grid in Grids)if(grid!=null)grid.SetActive(false);
            foreach(var card in Cards){var status=card.Recipe.RequiredStation==station ? Owner.GetStatus(card.Recipe) : CraftStatus.StationRequired;var show=card.Recipe.RequiredStation==station && (category<0 || (int)card.Recipe.Category==category) && (!craftable || status==CraftStatus.Ready || status==CraftStatus.Collect);card.gameObject.SetActive(show);if(show){card.Refresh();Grids[(int)card.Recipe.Category].SetActive(true);if(category<0)Sections[(int)card.Recipe.Category].SetActive(true);}}
            var upgrade=station==StationKind.Workbench && WorkbenchProgression.CanUpgrade(Owner.Station.Definition);UpgradeButton.gameObject.SetActive(upgrade);
            UpgradeScroll.gameObject.SetActive(upgrade);
            if(upgrade)
            {
                var text=new StringBuilder();var ready=true;
                foreach(var cost in Owner.Station.Definition.UpgradeCosts)
                {var owned=Owner.Inventory.Model.CountItem(cost.Item);ready &= owned>=cost.Count;if(text.Length>0)text.Append("\n");text.Append("<color=#").Append(owned>=cost.Count ? "C5CE9B" : "D99078").Append(">").Append(L.Format("ui.crafting.material",cost.Item.LocalizedName,owned,cost.Count)).Append("</color>");}
                UpgradeRequirements.text=text.ToString();UpgradeButton.interactable=ready;
                UpgradeLabel.text=L.Format("ui.crafting.upgrade_to",WorkbenchProgression.Display(Owner.Station.Definition.UpgradeTo.WorkbenchTier));
                if(!ready)UpgradeRequirements.text+="\n"+L.Text("ui.crafting.missing_materials");
            }
            var recipe=Selected;if(recipe==null || recipe.RequiredStation!=station){foreach(var card in Cards)if(card.gameObject.activeSelf){recipe=card.Recipe;Owner.SetSelected(recipe,false);break;}}
            SelectedTitle.text=recipe==null ? "" : string.IsNullOrEmpty(recipe.DisplayKey) ? recipe.Result.LocalizedName : L.Text(recipe.DisplayKey);
            var job=Owner.Processor;var sequential=job!=null && job.Sequential;QueueView.SetActive(sequential);MaterialSummary.gameObject.SetActive(!sequential);SelectedIcon.gameObject.SetActive(!sequential);
            if(!sequential && recipe!=null){var item=recipe.Result;SelectedIcon.sprite=item.Icon;var summary=L.Text("ui.item_category."+item.Category)+"\n"+L.Format("ui.crafting.owned",Owner.Inventory.Model.CountItem(item));if(item.Tool!=null)summary+="\n\n"+L.Format("ui.crafting.damage",item.Tool.Damage);if(item.Armor!=null)summary+="\n\n"+L.Format("ui.inventory.armor_detail",L.Text("ui.slot."+item.Armor.Slot),item.Armor.Defense);if(item.Placement!=null){summary+="\n\n"+L.Format("ui.crafting.health",item.Placement.Vitality.MaximumHealth);if(item.Placement.StorageSlots>0)summary+="\n"+L.Format("ui.crafting.storage",item.Placement.StorageSlots);}MaterialSummary.text=summary;}
            if(sequential)
            {
                QueueState.text=L.Format("ui.crafting.queue",job.Queue.WaitingCount+(job.Recipe==null ? 0 : 1),job.Queue.WaitingCount,job.OutputCount);
                CurrentItem.text=job.Recipe==null ? L.Text("ui.crafting.idle") : job.Remaining<=0 ? L.Text("ui.crafting.blocked_output") : L.Format("ui.crafting.current",ProcessingName(job.Recipe),job.Remaining);
                AddOne.interactable=AddAll.interactable=recipe!=null && job.BatchSize(recipe,Owner.Inventory.Model)>0;Withdraw.interactable=recipe!=null && job.Queue.CountWaiting(recipe)>0;Collect.interactable=job.OutputCount>0;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll.content);
        }
    }
}
