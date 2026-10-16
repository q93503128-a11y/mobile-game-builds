using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class StationProcessing : MonoBehaviour
    {
        public event Action Changed;
        public Func<float> ReplicaRemaining {get;set;}
        private void Notify(){Changed?.Invoke();Station.NotifyChanged();}
        internal void InitializeSnapshot(ProcessingSnapshot state,ContentCatalog catalog,PlayerInventory player)=>RestoreCore(state,catalog,player);
        internal void ApplyReplica(ProcessingSnapshot state,ContentCatalog catalog)=>RestoreCore(state,catalog,null);
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
        public float Remaining {get=>!SimulationAuthority.Allows(this) && ReplicaRemaining!=null ? ReplicaRemaining() : Sequential ? queue.Remaining : batchRemaining;private set=>batchRemaining=value;}
        public int BatchCount {get=>Sequential ? (queue.Current==null ? 0 : 1) : batchCount;private set=>batchCount=value;}
        public int OutputCount=>Sequential ? queue.OutputCount : Recipe==null ? 0 : Recipe.ResultCount*BatchCount;
        public float Duration=>Recipe==null ? 0 : Recipe.ProcessingSeconds/Station.Definition.ProcessingSpeed;
        public bool IsReady => Sequential ? queue.OutputCount>0 : Recipe!=null && Remaining<=0;
        public bool IsBusy => Sequential ? queue.Current!=null || queue.WaitingCount>0 || queue.OutputCount>0 : Recipe!=null;
        public float Progress => Recipe==null ? 0 : 1-Mathf.Clamp01(Remaining/Duration);
        private PlayerInventory owner;
        public ProcessingSnapshot Capture()=>Sequential ? queue.Capture() : Recipe==null ? null : new ProcessingSnapshot{Recipe=Recipe.Id,Batch=BatchCount,Remaining=Remaining};
        public void Restore(ProcessingSnapshot state,ContentCatalog catalog,PlayerInventory player)
        {if(!SimulationAuthority.Allows(this))return;RestoreCore(state,catalog,player);Notify();}
        private void RestoreCore(ProcessingSnapshot state,ContentCatalog catalog,PlayerInventory player)
        {if(Sequential){queue.Restore(state,catalog,Station.Definition.ProcessingSpeed);owner=player;return;}if(state==null || string.IsNullOrEmpty(state.Recipe)){Recipe=null;owner=null;BatchCount=0;Remaining=0;return;}Recipe=catalog.Recipe(state.Recipe);BatchCount=state.Batch;Remaining=state.Remaining;owner=player;}
        private void OnEnable() => Station.Health.Died+=ReleaseCargo;
        private void OnDisable() => Station.Health.Died-=ReleaseCargo;
        private void Update()
        {
            Advance(Time.deltaTime);
        }
        public bool Begin(RecipeDefinition recipe,PlayerInventory player)
        {
            var authority=player.GetComponent<NetworkFacilityInteractor>();if(authority!=null && !authority.IsApplying)return authority.Process(FacilityOperation.Craft,this,recipe);
            if(!SimulationAuthority.Allows(this))return false;
            if(Sequential)return Add(recipe,player,1);
            if(!WorkbenchProgression.Allows(Station,recipe) || IsBusy || recipe.ProcessingSeconds<=0 || recipe.RequiredStation!=Station.Definition.Station || !Station.Health.IsAlive) return false;
            var batch=BatchSize(recipe,player.Model);if(batch<=0)return false;
            var costs=ScaledCosts(recipe,batch);if(!player.Model.TryConsumeCosts(costs)) return false;
            owner=player;Recipe=recipe;BatchCount=batch;Remaining=Duration;Notify();return true;
        }
        public bool Add(RecipeDefinition recipe,PlayerInventory player,int count)
        {var authority=player.GetComponent<NetworkFacilityInteractor>();if(authority!=null && !authority.IsApplying)return count==1 && authority.Process(FacilityOperation.AddOne,this,recipe);if(!SimulationAuthority.Allows(this))return false;if(!WorkbenchProgression.Allows(Station,recipe) || !Sequential || recipe.RequiredStation!=Station.Definition.Station || recipe.ProcessingSeconds<=0 || !Station.Health.IsAlive)return false;var accepted=queue.Add(recipe,count,player.Model,Station.Definition.InputCapacity,Station.Definition.ProcessingSpeed);if(accepted){owner=player;Notify();}return accepted;}
        public int AddAll(RecipeDefinition recipe,PlayerInventory player)
        {var authority=player.GetComponent<NetworkFacilityInteractor>();if(authority!=null && !authority.IsApplying)return authority.Process(FacilityOperation.AddAll,this,recipe) ? 1 : 0;var count=BatchSize(recipe,player.Model);return Add(recipe,player,count) ? count : 0;}
        public bool Withdraw(RecipeDefinition recipe,PlayerInventory player)
        {var authority=player.GetComponent<NetworkFacilityInteractor>();if(authority!=null && !authority.IsApplying)return authority.Process(FacilityOperation.Withdraw,this,recipe);if(!SimulationAuthority.Allows(this))return false;var ok=Sequential && queue.Withdraw(recipe,player.Model);if(ok)Notify();return ok;}
        public void Advance(float seconds)
        {if(!SimulationAuthority.Allows(this))return;var before=queue.Version;var previous=Remaining;var wasReady=IsReady;if(Sequential){queue.Advance(seconds,Station.Definition.ProcessingSpeed,Station.Definition.OutputCapacity);if(before!=queue.Version || previous>0 && Remaining<=0)Notify();}else if(Recipe!=null && Remaining>0){Remaining=Mathf.Max(0,Remaining-seconds);if(!wasReady && IsReady)Notify();}}
        public int BatchSize(RecipeDefinition recipe,InventoryModel model)
        {if(!WorkbenchProgression.Allows(Station,recipe))return 0;var batch=Sequential ? Station.Definition.InputCapacity-queue.WaitingCount-(queue.Current==null ? 0 : 1) : Station.Definition.ParallelCapacity;foreach(var cost in recipe.Ingredients)batch=(int)Math.Min(batch,model.CountItem(cost.Item)/cost.Count);return batch;}
        private static ItemAmount[] ScaledCosts(RecipeDefinition recipe,int batch)
        {var costs=new ItemAmount[recipe.Ingredients.Length];for(var i=0;i<costs.Length;i++)costs[i]=new ItemAmount{Item=recipe.Ingredients[i].Item,Count=checked(recipe.Ingredients[i].Count*batch)};return costs;}
        public bool Collect(PlayerInventory player)
        {
            var authority=player.GetComponent<NetworkFacilityInteractor>();if(authority!=null && !authority.IsApplying)return authority.Process(FacilityOperation.Collect,this);
            if(!SimulationAuthority.Allows(this))return false;
            if(Sequential){var accepted=queue.Collect(player.Model);if(accepted){queue.Advance(0,Station.Definition.ProcessingSpeed,Station.Definition.OutputCapacity);Notify();}return accepted;}
            if(!IsReady) return false;
            if(player.Model.GetExchangeStatus(Array.Empty<ItemAmount>(),Recipe.Result,OutputCount)!=ExchangeResult.Success) return false;
            player.Model.TryExchange(Array.Empty<ItemAmount>(),Recipe.Result,OutputCount);
            Recipe=null;owner=null;Notify();return true;
        }
        private void ReleaseCargo()
        {
            if(Sequential){foreach(var amount in queue.Release())Drop(amount.Item,amount.Count);return;}
            if(Recipe==null) return;
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
