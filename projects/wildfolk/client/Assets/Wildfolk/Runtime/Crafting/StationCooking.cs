using System;
using UnityEngine;
namespace Wildfolk
{
    public sealed class StationCooking : MonoBehaviour
    {
        public CraftingStation Station;
        public WorldPickup RecoveryPickup;
        public GameplaySettings Settings;
        public RecipeDefinition Recipe { get; private set; }
        public float Remaining { get; private set; }
        public bool IsReady => Recipe!=null && Remaining<=0;
        public bool IsBusy => Recipe!=null;
        public float Progress => Recipe==null ? 0 : 1-Mathf.Clamp01(Remaining/Recipe.CookingSeconds);
        public event Action Changed;
        private PlayerInventory owner;
        private void OnEnable() => Station.Health.Died+=ReleaseCargo;
        private void OnDisable() => Station.Health.Died-=ReleaseCargo;
        private void Update()
        {
            if(Recipe==null || Remaining<=0) return;
            Remaining=Mathf.Max(0,Remaining-Time.deltaTime);
            if(Remaining<=0) Changed?.Invoke();
        }
        public bool Begin(RecipeDefinition recipe,PlayerInventory player)
        {
            if(IsBusy || recipe.CookingSeconds<=0 || recipe.RequiredStation!=Station.Definition.Station || !Station.Health.IsAlive) return false;
            if(!player.Model.TryConsumeCosts(recipe.Ingredients)) return false;
            owner=player;Recipe=recipe;Remaining=recipe.CookingSeconds;Changed?.Invoke();return true;
        }
        public bool Collect(PlayerInventory player)
        {
            if(!IsReady) return false;
            if(player.Model.GetExchangeStatus(Array.Empty<ItemAmount>(),Recipe.Result,Recipe.ResultCount)!=ExchangeResult.Success) return false;
            player.Model.TryExchange(Array.Empty<ItemAmount>(),Recipe.Result,Recipe.ResultCount);
            Recipe=null;owner=null;Changed?.Invoke();return true;
        }
        private void ReleaseCargo()
        {
            if(Recipe==null || owner==null) return;
            if(IsReady) Drop(Recipe.Result,Recipe.ResultCount);
            else foreach(var ingredient in Recipe.Ingredients) Drop(ingredient.Item,ingredient.Count);
            Recipe=null;owner=null;
        }
        private void Drop(ItemDefinition item,int count)
        {
            var pickup=Instantiate(RecoveryPickup,transform.position,Quaternion.identity);
            pickup.Initialize(item,count,owner,Settings);
        }
    }
}
