using UnityEngine;
using UnityEngine.UI;

namespace Wildfolk
{
    public sealed class CraftingPanel : MonoBehaviour
    {
        public PlayerCrafting Owner;
        public Text Title;
        public RecipeCardView[] Cards;
        public ScrollRect Scroll;
        private StationKind previousStation=(StationKind)(-1);
        private void Start()
        {
            Owner.Inventory.Model.Changed+=Refresh;
            Owner.StationChanged+=Refresh;
            Owner.Session.StateChanged+=Refresh;
            Refresh();
        }
        private void OnDestroy()
        {
            if(Owner!=null)
            {
                if(Owner.Inventory!=null && Owner.Inventory.Model!=null) Owner.Inventory.Model.Changed-=Refresh;
                Owner.StationChanged-=Refresh;
                if(Owner.Session!=null) Owner.Session.StateChanged-=Refresh;
            }
        }
        private void Refresh()
        {
            var station=Owner.AvailableStation;
            Title.text=station==StationKind.None ? "Hand crafting" : Owner.Station.Definition.StationTitle;
            foreach(var card in Cards)
            {
                var available=card.Recipe.RequiredStation==station;
                card.gameObject.SetActive(available);
                if(available) card.Refresh();
            }
            if(station!=previousStation){previousStation=station;LayoutRebuilder.ForceRebuildLayoutImmediate(Scroll.content);Scroll.verticalNormalizedPosition=1;}
        }
    }
}
