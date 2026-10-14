using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class StationProcessing : MonoBehaviour
    {
        public CraftingStation Station;
        public WorldPickup RecoveryPickup;
        public GameplaySettings Settings;
        private RecipeDefinition batchRecipe;
        private float batchRemaining;
        private int batchCount;
        private readonly ProcessingQueue queue=new ProcessingQueue();
        public bool Sequential=>Station.Definition.ProcessingMode==ProcessingMode.Sequential;
        public ProcessingQueue Queue=>queue;
        public RecipeDefinition Recipe {get=>Sequential ? queue.Current : batchRecipe;private set=>batchRecipe=value;}
        public float Remaining {get=>Sequential ? queue.Remaining : batchRemaining;private set=>batchRemaining=value;}
        public int BatchCount {get=>Sequential ? (queue.Current==null ? 0 : 1) : batchCount;private set=>batchCount=value;}
        public int OutputCount=>Sequential ? queue.OutputCount : Recipe==null ? 0 : Recipe.ResultCount*BatchCount;
        public float Duration=>Recipe==null ? 0 : Recipe.ProcessingSeconds/Station.Definition.ProcessingSpeed;
        public bool IsReady => Sequential ? queue.OutputCount>0 : Recipe!=null && Remaining<=0;
        public bool IsBusy => Sequential ? queue.Current!=null || queue.WaitingCount>0 || queue.OutputCount>0 : Recipe!=null;
        public float Progress => Recipe==null ? 0 : 1-Mathf.Clamp01(Remaining/Duration);
        private PlayerInventory owner;
        public ProcessingSnapshot Capture()=>Sequential ? queue.Capture() : Recipe==null ? null : new ProcessingSnapshot{Recipe=Recipe.Id,Batch=BatchCount,Remaining=Remaining};
        public void Restore(ProcessingSnapshot state,ContentCatalog catalog,PlayerInventory player)
        {if(Sequential){queue.Restore(state,catalog,Station.Definition.ProcessingSpeed);owner=player;return;}if(state==null || string.IsNullOrEmpty(state.Recipe)){Recipe=null;owner=null;BatchCount=0;Remaining=0;return;}Recipe=catalog.Recipe(state.Recipe);BatchCount=state.Batch;Remaining=state.Remaining;owner=player;}
        private void OnEnable() => Station.Health.Died+=ReleaseCargo;
        private void OnDisable() => Station.Health.Died-=ReleaseCargo;
        private void Update()
        {
            if(Sequential){queue.Advance(Time.deltaTime,Station.Definition.ProcessingSpeed,Station.Definition.OutputCapacity);return;}
            if(Recipe==null || Remaining<=0) return;
            Remaining=Mathf.Max(0,Remaining-Time.deltaTime);
        }
        public bool Begin(RecipeDefinition recipe,PlayerInventory player)
        {
            if(Sequential)return Add(recipe,player,1);
            if(IsBusy || recipe.ProcessingSeconds<=0 || recipe.RequiredStation!=Station.Definition.Station || !Station.Health.IsAlive) return false;
            var batch=BatchSize(recipe,player.Model);if(batch<=0)return false;
            var costs=ScaledCosts(recipe,batch);if(!player.Model.TryConsumeCosts(costs)) return false;
            owner=player;Recipe=recipe;BatchCount=batch;Remaining=Duration;return true;
        }
        public bool Add(RecipeDefinition recipe,PlayerInventory player,int count)
        {if(!Sequential || recipe.RequiredStation!=Station.Definition.Station || recipe.ProcessingSeconds<=0 || !Station.Health.IsAlive)return false;var accepted=queue.Add(recipe,count,player.Model,Station.Definition.InputCapacity,Station.Definition.ProcessingSpeed);if(accepted)owner=player;return accepted;}
        public int AddAll(RecipeDefinition recipe,PlayerInventory player)
        {var count=BatchSize(recipe,player.Model);return Add(recipe,player,count) ? count : 0;}
        public bool Withdraw(RecipeDefinition recipe,PlayerInventory player)=>Sequential && queue.Withdraw(recipe,player.Model);
        public void Advance(float seconds){if(Sequential)queue.Advance(seconds,Station.Definition.ProcessingSpeed,Station.Definition.OutputCapacity);else Remaining=Mathf.Max(0,Remaining-seconds);}
        public int BatchSize(RecipeDefinition recipe,InventoryModel model)
        {var batch=Sequential ? Station.Definition.InputCapacity-queue.WaitingCount-(queue.Current==null ? 0 : 1) : Station.Definition.ParallelCapacity;foreach(var cost in recipe.Ingredients)batch=(int)Math.Min(batch,model.CountItem(cost.Item)/cost.Count);return batch;}
        private static ItemAmount[] ScaledCosts(RecipeDefinition recipe,int batch)
        {var costs=new ItemAmount[recipe.Ingredients.Length];for(var i=0;i<costs.Length;i++)costs[i]=new ItemAmount{Item=recipe.Ingredients[i].Item,Count=checked(recipe.Ingredients[i].Count*batch)};return costs;}
        public bool Collect(PlayerInventory player)
        {
            if(Sequential){var accepted=queue.Collect(player.Model);if(accepted)queue.Advance(0,Station.Definition.ProcessingSpeed,Station.Definition.OutputCapacity);return accepted;}
            if(!IsReady) return false;
            if(player.Model.GetExchangeStatus(Array.Empty<ItemAmount>(),Recipe.Result,OutputCount)!=ExchangeResult.Success) return false;
            player.Model.TryExchange(Array.Empty<ItemAmount>(),Recipe.Result,OutputCount);
            Recipe=null;owner=null;return true;
        }
        private void ReleaseCargo()
        {
            if(Sequential){if(owner!=null)foreach(var amount in queue.Release())Drop(amount.Item,amount.Count);return;}
            if(Recipe==null || owner==null) return;
            if(IsReady) Drop(Recipe.Result,OutputCount);
            else foreach(var ingredient in Recipe.Ingredients) Drop(ingredient.Item,ingredient.Count*BatchCount);
            Recipe=null;owner=null;
        }
        private void Drop(ItemDefinition item,int count)
        {
            while(count>0){var portion=Mathf.Min(count,item.MaxStack);var pickup=PrefabPool.Spawn(RecoveryPickup,transform.position,Quaternion.identity);pickup.Initialize(item,portion,owner,Settings);count-=portion;}
        }
    }
}
