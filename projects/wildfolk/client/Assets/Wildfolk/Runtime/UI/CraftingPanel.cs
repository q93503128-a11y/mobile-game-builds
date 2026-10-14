using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace Wildfolk
{
    public sealed class CraftingPanel : MonoBehaviour
    {
        public PlayerCrafting Owner;
        public Text Title,CategoryLabel,FilterLabel,UpgradeLabel;
        public Button UpgradeButton;
        public GameObject CategoryPopup;
        public Button[] CategoryButtons;
        public RecipeCardView[] Cards;
        public ScrollRect Scroll;
        private StationKind previousStation=(StationKind)(-1);
        private int previousTier=-1,category=-1;
        private bool craftable,previousReady;
        private void Update(){var job=Owner.Processor;var ready=job!=null && job.IsReady;if(ready!=previousReady){previousReady=ready;Refresh();}}
        private void Start(){Owner.Inventory.Model.Changed+=Refresh;Owner.StationChanged+=Refresh;Owner.Session.StateChanged+=Refresh;Refresh();}
        private void OnDestroy(){if(Owner==null)return;if(Owner.Inventory!=null && Owner.Inventory.Model!=null)Owner.Inventory.Model.Changed-=Refresh;Owner.StationChanged-=Refresh;if(Owner.Session!=null)Owner.Session.StateChanged-=Refresh;}
        public void ToggleCategories()=>CategoryPopup.SetActive(!CategoryPopup.activeSelf);
        public void SelectCategory(int index){category=index;CategoryPopup.SetActive(false);Scroll.verticalNormalizedPosition=1;Refresh();}
        public void ToggleFilter(){craftable=!craftable;Refresh();}
        private void Refresh()
        {
            var station=Owner.AvailableStation;var tier=station==StationKind.Workbench ? Owner.Station.Definition.WorkbenchTier : 0;
            if(station!=previousStation){category=-1;craftable=false;CategoryPopup.SetActive(false);}
            for(var i=0;i<CategoryButtons.Length;i++){var available=i==0;foreach(var card in Cards)if(card.Recipe.RequiredStation==station && (int)card.Recipe.Category==i-1)available=true;CategoryButtons[i].gameObject.SetActive(available);}
            Title.text=station==StationKind.None ? "Hand crafting" : Owner.Station.Definition.StationTitle;
            CategoryLabel.text=category<0 ? "All categories  ›" : ((RecipeCategory)category).ToString()+"  ›";FilterLabel.text=craftable ? "Craftable only" : "Show all";
            foreach(var card in Cards)
            {var status=card.Recipe.RequiredStation==station ? Owner.GetStatus(card.Recipe) : CraftStatus.StationRequired;var available=card.Recipe.RequiredStation==station && (category<0 || (int)card.Recipe.Category==category) && (!craftable || status==CraftStatus.Ready || status==CraftStatus.Collect);card.gameObject.SetActive(available);if(available)card.Refresh();}
            var upgrade=station==StationKind.Workbench && Owner.Station.Definition.UpgradeTo!=null;UpgradeButton.gameObject.SetActive(upgrade);
            if(upgrade){var text=new StringBuilder("Upgrade II · ");foreach(var cost in Owner.Station.Definition.UpgradeCosts){if(text.Length>13)text.Append(" + ");text.Append(cost.Item.DisplayName).Append(" ").Append(cost.Count);}UpgradeLabel.text=text.ToString();}
            if(station!=previousStation || tier!=previousTier){previousStation=station;previousTier=tier;Scroll.verticalNormalizedPosition=1;}LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll.content);
        }
    }
}
