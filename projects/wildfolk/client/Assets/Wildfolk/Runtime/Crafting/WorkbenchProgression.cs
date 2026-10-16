using System;
using UnityEngine;
namespace Wildfolk
{
    public enum WorkbenchTier { I=1, II, III, IV, V, VI }
    public static class WorkbenchProgression
    {
        public const int Minimum=1, Maximum=6;
        public static bool IsValid(int tier)=>tier>=Minimum && tier<=Maximum;
        public static string Display(int tier)=>IsValid(tier) ? ((WorkbenchTier)tier).ToString() : throw new ArgumentOutOfRangeException(nameof(tier));
        public static bool CanUpgrade(PlaceableDefinition current)
        {
            if(current==null || current.Station!=StationKind.Workbench || !IsValid(current.WorkbenchTier) || current.WorkbenchTier==Maximum)return false;
            var next=current.UpgradeTo;
            if(next==null || next.Station!=StationKind.Workbench || next.WorkbenchTier!=current.WorkbenchTier+1 || !IsValid(next.WorkbenchTier) || next.Prefab==null || next.Prefab.GetComponent<CraftingStation>()==null || current.UpgradeCosts==null || current.UpgradeCosts.Length==0)return false;
            foreach(var cost in current.UpgradeCosts)if(cost.Item==null || cost.Count<=0)return false;
            return true;
        }
        public static string Name(int tier)=>L.Format("ui.crafting.workbench_tier",Display(tier));
        public static bool Allows(CraftingStation station,RecipeDefinition recipe)
        {
            if(recipe==null || !IsValid(recipe.RequiredWorkbenchTier))return false;
            if(recipe.RequiredStation==StationKind.None)return recipe.RequiredWorkbenchTier==1;
            if(station==null || !station.Health.IsAlive || station.Definition.Station!=recipe.RequiredStation)return false;
            if(recipe.RequiredStation==StationKind.Workbench)return IsValid(station.Definition.WorkbenchTier) && station.Definition.WorkbenchTier>=recipe.RequiredWorkbenchTier;
            return true;
        }
    }
}
