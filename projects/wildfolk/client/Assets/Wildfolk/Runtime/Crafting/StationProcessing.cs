using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class StationProcessing : MonoBehaviour
    {
        public CraftingStation Station;
        public WorldPickup RecoveryPickup;
        public GameplaySettings Settings;
        public RecipeDefinition Recipe { get; private set; }
        public float Remaining { get; private set; }
        public int BatchCount { get; private set; }
        public int OutputCount=>Recipe==null ? 0 : Recipe.ResultCount*BatchCount;
        public float Duration=>Recipe==null ? 0 : Recipe.ProcessingSeconds/Station.Definition.ProcessingSpeed;
        public bool IsReady => Recipe!=null && Remaining<=0;
        public bool IsBusy => Recipe!=null;
        public float Progress => Recipe==null ? 0 : 1-Mathf.Clamp01(Remaining/Duration);
        private PlayerInventory owner;
        public ProcessingSnapshot Capture()=>Recipe==null ? null : new ProcessingSnapshot{Recipe=Recipe.Id,Batch=BatchCount,Remaining=Remaining};
        public void Restore(ProcessingSnapshot state,ContentCatalog catalog,PlayerInventory player)
        {if(state==null || string.IsNullOrEmpty(state.Recipe)){Recipe=null;owner=null;BatchCount=0;Remaining=0;return;}Recipe=catalog.Recipe(state.Recipe);BatchCount=state.Batch;Remaining=state.Remaining;owner=player;}
        private void OnEnable() => Station.Health.Died+=ReleaseCargo;
        private void OnDisable() => Station.Health.Died-=ReleaseCargo;
        private void Update()
        {
            if(Recipe==null || Remaining<=0) return;
            Remaining=Mathf.Max(0,Remaining-Time.deltaTime);
        }
        public bool Begin(RecipeDefinition recipe,PlayerInventory player)
        {
            if(IsBusy || recipe.ProcessingSeconds<=0 || recipe.RequiredStation!=Station.Definition.Station || !Station.Health.IsAlive) return false;
            var batch=BatchSize(recipe,player.Model);if(batch<=0)return false;
            var costs=ScaledCosts(recipe,batch);if(!player.Model.TryConsumeCosts(costs)) return false;
            owner=player;Recipe=recipe;BatchCount=batch;Remaining=Duration;return true;
        }
        public int BatchSize(RecipeDefinition recipe,InventoryModel model)
        {var batch=Station.Definition.ParallelCapacity;foreach(var cost in recipe.Ingredients)batch=(int)Math.Min(batch,model.CountItem(cost.Item)/cost.Count);return batch;}
        private static ItemAmount[] ScaledCosts(RecipeDefinition recipe,int batch)
        {var costs=new ItemAmount[recipe.Ingredients.Length];for(var i=0;i<costs.Length;i++)costs[i]=new ItemAmount{Item=recipe.Ingredients[i].Item,Count=checked(recipe.Ingredients[i].Count*batch)};return costs;}
        public bool Collect(PlayerInventory player)
        {
            if(!IsReady) return false;
            if(player.Model.GetExchangeStatus(Array.Empty<ItemAmount>(),Recipe.Result,OutputCount)!=ExchangeResult.Success) return false;
            player.Model.TryExchange(Array.Empty<ItemAmount>(),Recipe.Result,OutputCount);
            Recipe=null;owner=null;return true;
        }
        private void ReleaseCargo()
        {
            if(Recipe==null || owner==null) return;
            if(IsReady) Drop(Recipe.Result,OutputCount);
            else foreach(var ingredient in Recipe.Ingredients) Drop(ingredient.Item,ingredient.Count*BatchCount);
            Recipe=null;owner=null;
        }
        private void Drop(ItemDefinition item,int count)
        {
            var pickup=PrefabPool.Spawn(RecoveryPickup,transform.position,Quaternion.identity);
            pickup.Initialize(item,count,owner,Settings);
        }
    }
}
