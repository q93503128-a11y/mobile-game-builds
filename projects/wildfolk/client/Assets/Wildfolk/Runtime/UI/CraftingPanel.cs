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
        public Text FoodPreview;
        public ScrollRect UpgradeScroll,SummaryScroll;
        public Button UpgradeButton,AddOne,AddAll,Withdraw,Collect;public Button[] CategoryButtons;
        public Button InventoryButton,HomeButton;
        public Text HomeLabel;
        public void SetHome(){if(Owner.GetComponent<PlayerNavigation>().SelectHome(Owner.Station))PlayerFeedback.ShowKey("feedback.home_set");Refresh();}
        public RecipeCardView[] Cards;public ScrollRect Scroll;
        private StationKind previousStation=(StationKind)(-1);private int category=-1;private bool craftable;private float nextRefresh;
        private RecipeDefinition namedRecipe;private string namedLanguage,namedProcessing;
        private RecipeDefinition Selected=>Owner.SelectedRecipe;
        private void Update(){if(View.activeSelf && Owner.Processor!=null && Time.unscaledTime>=nextRefresh){nextRefresh=Time.unscaledTime+.2f;RefreshProgress();}}
        private void Start(){Owner.Inventory.Model.Changed+=Refresh;Owner.StationChanged+=Refresh;Owner.Session.StateChanged+=Refresh;L.Changed+=Refresh;Refresh();}
        private void OnDestroy(){Owner.Inventory.Model.Changed-=Refresh;Owner.StationChanged-=Refresh;Owner.Session.StateChanged-=Refresh;L.Changed-=Refresh;}
        public void SelectCategory(int index){category=index;Scroll.verticalNormalizedPosition=1;Refresh();}
        public void ToggleFilter(){craftable=!craftable;Refresh();}
        public void EnqueueOne(){if(Selected!=null && Owner.Processor.Add(Selected,Owner.Inventory,1) && Owner.Authority==null)PlayerFeedback.ShowOperation(PlayerFeedback.Cue.Processing);Refresh();}
        public void EnqueueAll(){if(Selected!=null && Owner.Processor.AddAll(Selected,Owner.Inventory)>0 && Owner.Authority==null)PlayerFeedback.ShowOperation(PlayerFeedback.Cue.Processing);Refresh();}
        public void WithdrawOne(){if(Selected!=null)Owner.Processor.Withdraw(Selected,Owner.Inventory);Refresh();}
        public void CollectResults(){if(!Owner.Processor.Collect(Owner.Inventory))PlayerFeedback.ShowKey("feedback.bag_full");else if(Owner.Authority==null)PlayerFeedback.ShowOperation(PlayerFeedback.Cue.Collected);Refresh();}
        private void RefreshProgress()
        {
            var job=Owner.Processor;if(job==null)return;
            foreach(var card in Cards)if(card.gameObject.activeSelf && card.Recipe==job.Recipe)card.RefreshProgress();
            if(job.Sequential)CurrentItem.text=job.Recipe==null ? L.Text("ui.crafting.idle") : job.Remaining<=0 ? L.Text("ui.crafting.blocked_output") : L.Format("ui.crafting.current",ProcessingName(job.Recipe),job.Remaining);
        }
        private string ProcessingName(RecipeDefinition recipe){if(namedRecipe==recipe && namedLanguage==L.Language)return namedProcessing;namedRecipe=recipe;namedLanguage=L.Language;var text=new StringBuilder();foreach(var input in recipe.Ingredients){if(text.Length>0)text.Append(" + ");text.Append(input.Item.LocalizedName);}return namedProcessing=L.Format("ui.crafting.input_output",text.ToString(),recipe.Result.LocalizedName);}
        public void Refresh()
        {
            View.SetActive(Owner.ScreenOpen && Owner.Session.State==SessionState.Inventory);if(!View.activeSelf)return;
            var station=Owner.AvailableStation;
            if(HomeButton!=null){HomeButton.gameObject.SetActive(station==StationKind.Workbench);HomeLabel.text=L.Text(Owner.GetComponent<PlayerNavigation>().HomeId==Owner.Station?.GetComponent<WorldPlaceable>().SaveId ? "ui.navigation.home_selected" : "ui.navigation.set_home");}if(station!=previousStation){category=-1;craftable=false;previousStation=station;Scroll.verticalNormalizedPosition=1;}
            Title.text=station==StationKind.None ? L.Text("ui.crafting.hand") : Owner.Station.Definition.Station==StationKind.Workbench ? (string.IsNullOrEmpty(Owner.Station.Definition.TierDisplayKey) ? WorkbenchProgression.Name(Owner.Station.Definition.WorkbenchTier) : L.Text(Owner.Station.Definition.TierDisplayKey)) : Owner.Station.Definition.Item.LocalizedName;
            var tabsChanged=false;for(var i=0;i<CategoryButtons.Length;i++){var available=i==0;foreach(var card in Cards)if(card.Recipe.RequiredStation==station && (int)card.Recipe.Category==i-1)available=true;if(CategoryButtons[i].gameObject.activeSelf!=available){CategoryButtons[i].gameObject.SetActive(available);tabsChanged=true;}CategoryButtons[i].image.color=i-1==category ? new Color32(152,145,96,255) : Color.white;}
            if(tabsChanged)LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)CategoryButtons[0].transform.parent);
            FilterLabel.text=L.Text(craftable ? "ui.crafting.ready_only" : "ui.crafting.show_all");
            var layoutChanged=false;
            foreach(var card in Cards){var status=card.Recipe.RequiredStation==station ? Owner.GetStatus(card.Recipe) : CraftStatus.StationRequired;var show=card.Recipe.RequiredStation==station && (category<0 || (int)card.Recipe.Category==category) && (!craftable || status==CraftStatus.Ready || status==CraftStatus.Collect);if(card.gameObject.activeSelf!=show){card.gameObject.SetActive(show);layoutChanged=true;}if(show)card.Refresh();}
            for(var i=0;i<Grids.Length;i++)
            {
                var visible=false;foreach(var card in Cards)if((int)card.Recipe.Category==i && card.gameObject.activeSelf){visible=true;break;}
                if(Grids[i]!=null && Grids[i].activeSelf!=visible){Grids[i].SetActive(visible);layoutChanged=true;}
                var heading=visible && category<0;if(Sections[i]!=null && Sections[i].activeSelf!=heading){Sections[i].SetActive(heading);layoutChanged=true;}
            }
            var upgrade=station==StationKind.Workbench && WorkbenchProgression.CanUpgrade(Owner.Station.Definition);UpgradeButton.gameObject.SetActive(upgrade);
            UpgradeScroll.gameObject.SetActive(upgrade);
            var largeUpgrade=upgrade && Owner.Station.Definition.UpgradeCosts.Length>3;var ur=(RectTransform)UpgradeScroll.transform;ur.sizeDelta=new Vector2(358,largeUpgrade ? 180 : 104);ur.anchoredPosition=new Vector2(501,largeUpgrade ? -180 : -215);
            if(upgrade)
            {
                var text=new StringBuilder();var ready=true;
                foreach(var cost in Owner.Station.Definition.UpgradeCosts)
                {var owned=Owner.Inventory.Model.CountItem(cost.Item);ready &= owned>=cost.Count;if(text.Length>0)text.Append("\n");text.Append("<color=#").Append(owned>=cost.Count ? "C5CE9B" : "D99078").Append(">").Append(L.Format("ui.crafting.material",cost.Item.LocalizedName,owned,cost.Count)).Append("</color>");}
                UpgradeRequirements.text=text.ToString();UpgradeButton.interactable=ready;
                UpgradeLabel.text=L.Format("ui.crafting.upgrade_to",WorkbenchProgression.Display(Owner.Station.Definition.UpgradeTo.WorkbenchTier));
                if(!ready && !largeUpgrade)UpgradeRequirements.text+="\n"+L.Text("ui.crafting.missing_materials");
            }
            var recipe=Selected;if(recipe==null || recipe.RequiredStation!=station){foreach(var card in Cards)if(card.gameObject.activeSelf){recipe=card.Recipe;Owner.SetSelected(recipe,false);break;}}
            SelectedTitle.text=recipe==null ? "" : string.IsNullOrEmpty(recipe.DisplayKey) ? recipe.Result.LocalizedName : L.Text(recipe.DisplayKey);
            var job=Owner.Processor;var sequential=job!=null && job.Sequential;QueueView.SetActive(sequential);SummaryScroll.gameObject.SetActive(!sequential);MaterialSummary.gameObject.SetActive(!sequential);SelectedIcon.gameObject.SetActive(!sequential);
            if(FoodPreview!=null)
            {
                var food=recipe?.Result.Consumable;FoodPreview.enabled=sequential && food!=null;
                if(FoodPreview.enabled)FoodPreview.text=food.Meal==null ? ItemStats.Food(food) : L.Format("ui.food.stats",food.HungerRestore,food.ThirstRestore)+"\n"+L.Format("ui.meal.preview."+food.Meal.Effect,food.Meal.Effect==MealEffect.StaminaRecovery ? food.Meal.Amount*100 : food.Meal.Amount)+"\n"+L.Format("ui.meal.preview_rule",food.Meal.Duration);
            }
            if(!sequential && recipe!=null){if(SelectedIcon.sprite!=recipe.Result.Icon)SummaryScroll.verticalNormalizedPosition=1;var item=recipe.Result;var exploration=item.FishingRod || item.PortableLight!=null || item.NavigationInstrument || item.Tool!=null || item.Ammo!=AmmoKind.None || item.Consumable!=null || item.Armor!=null || item.WaterCapacity>0 || item.EquippedLight!=null;MaterialSummary.fontSize=exploration ? 20 : 23;SummaryScroll.GetComponent<RectTransform>().sizeDelta=new Vector2(358,upgrade ? largeUpgrade ? 90 : 145 : 240);SummaryScroll.GetComponent<RectTransform>().anchoredPosition=new Vector2(501,upgrade ? largeUpgrade ? -33 : -58 : -105);SelectedIcon.sprite=item.Icon;var summary=recipe.OptionalConversion ? L.Format("ui.crafting.result",item.LocalizedName,recipe.ResultCount) : "";if(item.Tool!=null)summary+="\n\n"+ItemStats.Description(item);if(item.Armor!=null)summary+="\n\n"+ItemStats.Armor(item.Armor);if(item.Placement!=null){summary+="\n\n"+L.Format("ui.crafting.health",item.Placement.Vitality.MaximumHealth);if(item.Placement.StorageSlots>0)summary+="\n"+L.Format("ui.crafting.storage",item.Placement.StorageSlots);}if(item.Ammo!=AmmoKind.None)summary+="\n\n"+L.Format("ui.ammo.info",item.AmmoDamage);if(item.Consumable!=null)summary+="\n\n"+ItemStats.Food(item.Consumable);if(item.FishingRod)summary+="\n\n"+L.Text("ui.fishing.rod");if(item.WaterCapacity>0)summary+="\n\n"+L.Text("ui.waterskin.craft");if(item.EquippedLight!=null)summary+="\n\n"+L.Text("ui.utility.info");if(item.PortableLight!=null)summary+="\n\n"+L.Text("ui.navigation.torch_info");if(item.Telescope || item.TurretModule!=null || item.Placement!=null && (item.Placement.Turret!=null || item.Placement.IsBeacon))summary+="\n\n"+L.Text("item."+item.Id+".description");if(item.NavigationInstrument)summary+="\n\n"+L.Text("ui.navigation.compass_info");MaterialSummary.text=summary.TrimStart('\n')+"\n\n"+L.Text("ui.item_category."+item.Category)+"\n"+L.Format("ui.crafting.owned",Owner.Inventory.Model.CountItem(item));}
            if(sequential)
            {
                QueueState.text=L.Format("ui.crafting.queue",job.Queue.WaitingCount+(job.Recipe==null ? 0 : 1),job.Queue.WaitingCount,job.OutputCount);
                CurrentItem.text=job.Recipe==null ? L.Text("ui.crafting.idle") : job.Remaining<=0 ? L.Text("ui.crafting.blocked_output") : L.Format("ui.crafting.current",ProcessingName(job.Recipe),job.Remaining);
                AddOne.interactable=AddAll.interactable=recipe!=null && job.BatchSize(recipe,Owner.Inventory.Model)>0;Withdraw.interactable=recipe!=null && job.Queue.CountWaiting(recipe)>0;Collect.interactable=job.OutputCount>0;
            }
            if(layoutChanged)LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll.content);
        }
    }
}
