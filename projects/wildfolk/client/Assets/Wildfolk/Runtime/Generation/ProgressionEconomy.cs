using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    // Simulates recipe batches, retaining surplus between requests. Shared facilities are paid once.
    public sealed class ProgressionBudget
    {
        public readonly Dictionary<ItemDefinition,int> Shared=new Dictionary<ItemDefinition,int>();
        public readonly Dictionary<ItemDefinition,int> Personal=new Dictionary<ItemDefinition,int>();
        public readonly Dictionary<ItemDefinition,int> Optional=new Dictionary<ItemDefinition,int>();
        public readonly Dictionary<ItemDefinition,int> Total=new Dictionary<ItemDefinition,int>();
        public readonly Dictionary<ItemDefinition,int> Produced=new Dictionary<ItemDefinition,int>();
    }
    public static class ProgressionEconomy
    {
        private sealed class Planner
        {
            public readonly ProgressionBudget Budget=new ProgressionBudget();
            private readonly ContentCatalog catalog;
            private readonly Dictionary<ItemDefinition,RecipeDefinition> recipes=new Dictionary<ItemDefinition,RecipeDefinition>();
            private readonly Dictionary<ItemDefinition,int> surplus=new Dictionary<ItemDefinition,int>();
            private readonly HashSet<ItemDefinition> path=new HashSet<ItemDefinition>();
            public int WorkbenchTier=1;
            public Planner(ContentCatalog value,int maximumTier)
            {catalog=value;foreach(var r in catalog.Recipes){if(r.ResultCharges>0 || r.RequiredWorkbenchTier>maximumTier)continue;if(!recipes.TryAdd(r.Result,r))throw new InvalidOperationException("Ambiguous progression recipe");}}
            private static void Add(Dictionary<ItemDefinition,int> map,ItemDefinition item,int n)
            {map.TryGetValue(item,out var old);map[item]=checked(old+n);}
            public void Take(ItemDefinition item,int count,Dictionary<ItemDefinition,int> group)
            {
                if(count<=0)return;surplus.TryGetValue(item,out var left);var used=Math.Min(left,count);surplus[item]=left-used;count-=used;if(count==0)return;
                if(!recipes.TryGetValue(item,out var r)){Add(group,item,count);Add(Budget.Total,item,count);return;}
                if(!path.Add(item))throw new InvalidOperationException("Cyclic progression recipe");
                if(r.RequiredStation==StationKind.Workbench)WorkbenchTier=Math.Max(WorkbenchTier,r.RequiredWorkbenchTier);
                var batches=Mathf.CeilToInt((float)count/r.ResultCount);var made=checked(batches*r.ResultCount);surplus[item]+=made-count;Add(Budget.Produced,item,made);
                foreach(var a in r.Ingredients)Take(a.Item,checked(a.Count*batches),group);path.Remove(item);
            }
            public void Upgrades()
            {
                // Existing station upgrade data is the sole source of upgrade material costs.
                for(var tier=1;tier<WorkbenchTier;tier++)foreach(var item in catalog.Items)
                {var p=item.Placement;if(p==null || p.Station!=StationKind.Workbench || p.WorkbenchTier!=tier || p.UpgradeTo==null)continue;foreach(var a in p.UpgradeCosts)Take(a.Item,a.Count,Budget.Shared);}
            }
        }
        public static ProgressionBudget Calculate(ContentCatalog catalog,int players,bool optional=true,int maximumTier=6)
        {
            if(catalog==null || players<1 || players>4)throw new ArgumentOutOfRangeException(nameof(players));var p=new Planner(catalog,maximumTier);
            foreach(var r in catalog.Recipes)if(r.RequiredWorkbenchTier<=maximumTier && (r.SupersededBy==null || r.SupersededBy.RequiredWorkbenchTier>maximumTier))p.Take(r.Result,r.EssentialShared,p.Budget.Shared);
            for(var player=0;player<players;player++)foreach(var r in catalog.Recipes)if(r.RequiredWorkbenchTier<=maximumTier && (r.SupersededBy==null || r.SupersededBy.RequiredWorkbenchTier>maximumTier))p.Take(r.Result,r.EssentialPerPlayer,p.Budget.Personal);
            p.Upgrades();
            if(optional)foreach(var r in catalog.Recipes)if(r.RequiredWorkbenchTier<=maximumTier && (r.SupersededBy==null || r.SupersededBy.RequiredWorkbenchTier>maximumTier))p.Take(r.Result,checked(r.ProgressionShared+r.ProgressionPerPlayer*players),p.Budget.Optional);
            return p.Budget;
        }
    }
}
