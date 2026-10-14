using System;
using UnityEngine;

namespace Wildfolk
{
    public enum CraftStatus { Ready, MissingMaterials, NoSpace, StationRequired, Processing, Collect }

    public sealed class PlayerCrafting : MonoBehaviour
    {
        public RecipeCatalog Catalog;
        public PlayerInventory Inventory;
        public GameplaySession Session;
        public GameplaySettings Settings;
        public CraftingStation Station { get; private set; }
        public StationKind AvailableStation => Station != null && InRange(Station) ? Station.Definition.Station : StationKind.None;
        public StationProcessing Processor => Station==null ? null : Station.GetComponent<StationProcessing>();
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
            if(recipe.RequiredStation==StationKind.Workbench && Station.Definition.WorkbenchTier<recipe.RequiredWorkbenchTier)return CraftStatus.StationRequired;
            if(recipe.ProcessingSeconds>0 && Processor!=null && Processor.IsBusy)
            {
                if(Processor.Recipe!=recipe || !Processor.IsReady) return CraftStatus.Processing;
                return Inventory.Model.GetExchangeStatus(Array.Empty<ItemAmount>(),recipe.Result,Processor.OutputCount)==ExchangeResult.Success ? CraftStatus.Collect : CraftStatus.NoSpace;
            }
            var result = Inventory.Model.GetExchangeStatus(recipe.Ingredients, recipe.Result, recipe.ResultCount);
            return result == ExchangeResult.Success ? CraftStatus.Ready :
                result == ExchangeResult.NoSpace ? CraftStatus.NoSpace : CraftStatus.MissingMaterials;
        }
        public CraftStatus Craft(RecipeDefinition recipe)
        {
            if (Session.State != SessionState.Inventory) return CraftStatus.StationRequired;
            var status = GetStatus(recipe);
            if(status==CraftStatus.Collect) { Processor.Collect(Inventory);return status; }
            if (status != CraftStatus.Ready) return status;
            if(recipe.ProcessingSeconds>0)
                return Processor!=null && Processor.Begin(recipe,Inventory) ? CraftStatus.Processing : CraftStatus.StationRequired;
            var result = Inventory.Model.TryExchange(recipe.Ingredients, recipe.Result, recipe.ResultCount);
            return result == ExchangeResult.Success ? CraftStatus.Ready :
                result == ExchangeResult.NoSpace ? CraftStatus.NoSpace : CraftStatus.MissingMaterials;
        }
    }
}
