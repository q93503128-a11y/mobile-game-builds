using System;
using UnityEngine;

namespace Wildfolk
{
    public enum CraftStatus { Ready, MissingMaterials, NoSpace, StationRequired, Processing, Collect }

    public sealed class PlayerCrafting : MonoBehaviour
    {
        public NetworkFacilityInteractor Authority {get;set;}
        public RecipeCatalog Catalog;
        public PlayerInventory Inventory;
        public GameplaySession Session;
        public GameplaySettings Settings;
        public CraftingStation Station { get; private set; }
        public StationKind AvailableStation => Station != null && InRange(Station) ? Station.Definition.Station : StationKind.None;
        public StationProcessing Processor => Station==null ? null : Station.GetComponent<StationProcessing>();
        public event Action StationChanged;
        private bool stationOpen;
        public bool ScreenOpen {get;private set;}
        public RecipeDefinition SelectedRecipe {get;private set;}
        public void SetSelected(RecipeDefinition recipe,bool notify=true){SelectedRecipe=recipe;if(notify){MarkSeen(recipe);StationChanged?.Invoke();}}
        public void OpenHandCrafting(){if(Session.State!=SessionState.Inventory)return;Authority?.CloseFacilityView();ClearStation();Station=null;stationOpen=false;ScreenOpen=true;StationChanged?.Invoke();}
        public void CloseCrafting(){ScreenOpen=false;ClearStation();Session.CloseInventory();StationChanged?.Invoke();}
        public void OpenBag(){Authority?.CloseFacilityView();ScreenOpen=false;ClearStation();StationChanged?.Invoke();}
        private readonly System.Collections.Generic.HashSet<string> seen=new System.Collections.Generic.HashSet<string>();
        public string[] CaptureSeen(){var a=new string[seen.Count];seen.CopyTo(a);Array.Sort(a,StringComparer.Ordinal);return a;}
        public void RestoreSeen(string[] ids){seen.Clear();foreach(var id in ids)seen.Add(id);StationChanged?.Invoke();}
        public bool IsNew(RecipeDefinition recipe)=>recipe.RequiredWorkbenchTier>1 && AvailableStation==StationKind.Workbench && Station.Definition.WorkbenchTier>=recipe.RequiredWorkbenchTier && !seen.Contains(recipe.Id);
        public void MarkSeen(RecipeDefinition recipe){if(IsNew(recipe) && seen.Add(recipe.Id))StationChanged?.Invoke();}
        public bool UpgradeWorkbench()
        {
            if(Authority!=null && !Authority.IsApplying)return Authority.Upgrade();
            if(Session.State!=SessionState.Inventory || AvailableStation!=StationKind.Workbench || Station.Definition.UpgradeTo==null)return false;
            var old=Station.GetComponent<WorldPlaceable>();var definition=Station.Definition;
            if(!Inventory.Model.TryConsumeCosts(definition.UpgradeCosts))return false;
            Station.Changed-=NotifyStation;var next=Instantiate(definition.UpgradeTo.Prefab,old.transform.position,old.transform.rotation);next.SaveId=old.SaveId;next.Health.RestoreHealth(Mathf.Max(1,Mathf.RoundToInt(old.Health.Ratio*next.Health.Maximum)));old.gameObject.SetActive(false);Destroy(old.gameObject);Station=next.GetComponent<CraftingStation>();Station.Changed+=NotifyStation;StationChanged?.Invoke();return true;
        }
        public void UpgradeFromUI(){if(!UpgradeWorkbench())PlayerFeedback.ShowKey("feedback.upgrade");}

        private void OnEnable() => Session.StateChanged += SessionChanged;
        private void OnDisable() => Session.StateChanged -= SessionChanged;
        private void Update()
        {
            if (stationOpen && (Station == null || !InRange(Station))) ClearStation();
        }
        private bool InRange(CraftingStation station) => station.CanInteract(transform.position,Settings.StationInteractionRange);
        public bool OpenStation(CraftingStation station)
        {
            if(Authority!=null && !Authority.IsApplying)return station!=null && Authority.Open(station.GetComponent<NetworkFacility>());
            if (!Session.IsRunning || station == null || !InRange(station)) return false;
            Station = station;Station.Changed+=NotifyStation; stationOpen = true;ScreenOpen=true;
            Session.OpenInventory();
            StationChanged?.Invoke();
            return true;
        }
        private void SessionChanged()
        {
            if (Session.State != SessionState.Inventory){ScreenOpen=false;ClearStation();StationChanged?.Invoke();}
        }
        private void NotifyStation()=>StationChanged?.Invoke();
        private void ClearStation()
        {
            if (!stationOpen) return;
            if(Station!=null)Station.Changed-=NotifyStation;
            Station = null; stationOpen = false;
            StationChanged?.Invoke();
        }
        public CraftStatus GetStatus(RecipeDefinition recipe)
        {
            if (recipe.RequiredStation != StationKind.None && recipe.RequiredStation != AvailableStation) return CraftStatus.StationRequired;
            if(recipe.RequiredStation==StationKind.Workbench && Station.Definition.WorkbenchTier<recipe.RequiredWorkbenchTier)return CraftStatus.StationRequired;
            if(recipe.ProcessingSeconds>0 && Processor!=null && Processor.Sequential){return Processor.BatchSize(recipe,Inventory.Model)>0 ? CraftStatus.Ready : CraftStatus.MissingMaterials;}
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
            if(Authority!=null && !Authority.IsApplying){var preview=GetStatus(recipe);return Authority.Craft(recipe) ? preview : CraftStatus.StationRequired;}
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
