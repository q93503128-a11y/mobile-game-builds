using System;
using UnityEngine;

namespace Wildfolk
{
    public enum CraftStatus { Ready, MissingMaterials, NoSpace, StationRequired, Cooking, Collect }

    public sealed class PlayerCrafting : MonoBehaviour
    {
        public RecipeCatalog Catalog;
        public PlayerInventory Inventory;
        public GameplaySession Session;
        public GameplaySettings Settings;
        public CraftingStation Station { get; private set; }
        public StationKind AvailableStation => Station != null && InRange(Station) ? Station.Definition.Station : StationKind.None;
        public StationCooking Cooking => Station==null ? null : Station.GetComponent<StationCooking>();
        public event Action StationChanged;
        private bool stationOpen;

        private void OnEnable() => Session.StateChanged += SessionChanged;
        private void OnDisable() => Session.StateChanged -= SessionChanged;
        private void Update()
        {
            if (stationOpen && (Station == null || !InRange(Station))) ClearStation();
        }
        private bool InRange(CraftingStation station) => station.CanInteract(transform.position,Settings.StationInteractionRange);
        public bool OpenStation(CraftingStation station)
        {
            if (!Session.IsRunning || station == null || !InRange(station)) return false;
            Station = station; stationOpen = true;
            Session.OpenInventory();
            StationChanged?.Invoke();
            return true;
        }
        private void SessionChanged()
        {
            if (Session.State != SessionState.Inventory) ClearStation();
        }
        private void ClearStation()
        {
            if (!stationOpen) return;
            Station = null; stationOpen = false;
            StationChanged?.Invoke();
        }
        public CraftStatus GetStatus(RecipeDefinition recipe)
        {
            if (recipe.RequiredStation != StationKind.None && recipe.RequiredStation != AvailableStation) return CraftStatus.StationRequired;
            if(recipe.CookingSeconds>0 && Cooking!=null && Cooking.IsBusy)
            {
                if(Cooking.Recipe!=recipe || !Cooking.IsReady) return CraftStatus.Cooking;
                return Inventory.Model.GetExchangeStatus(Array.Empty<ItemAmount>(),recipe.Result,recipe.ResultCount)==ExchangeResult.Success ? CraftStatus.Collect : CraftStatus.NoSpace;
            }
            var result = Inventory.Model.GetExchangeStatus(recipe.Ingredients, recipe.Result, recipe.ResultCount);
            return result == ExchangeResult.Success ? CraftStatus.Ready :
                result == ExchangeResult.NoSpace ? CraftStatus.NoSpace : CraftStatus.MissingMaterials;
        }
        public CraftStatus Craft(RecipeDefinition recipe)
        {
            if (Session.State != SessionState.Inventory) return CraftStatus.StationRequired;
            var status = GetStatus(recipe);
            if(status==CraftStatus.Collect) { Cooking.Collect(Inventory);return status; }
            if (status != CraftStatus.Ready) return status;
            if(recipe.CookingSeconds>0)
                return Cooking!=null && Cooking.Begin(recipe,Inventory) ? CraftStatus.Cooking : CraftStatus.StationRequired;
            var result = Inventory.Model.TryExchange(recipe.Ingredients, recipe.Result, recipe.ResultCount);
            return result == ExchangeResult.Success ? CraftStatus.Ready :
                result == ExchangeResult.NoSpace ? CraftStatus.NoSpace : CraftStatus.MissingMaterials;
        }
    }
}
