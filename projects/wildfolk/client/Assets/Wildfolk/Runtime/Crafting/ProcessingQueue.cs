using System;
using System.Collections.Generic;
using UnityEngine;
namespace Wildfolk
{
    public sealed class ProcessingQueue
    {
        private readonly List<RecipeDefinition> waiting=new List<RecipeDefinition>();
        private readonly Dictionary<RecipeDefinition,int> output=new Dictionary<RecipeDefinition,int>();
        public uint Version {get;private set;}
        public RecipeDefinition Current {get;private set;}
        public float Remaining {get;private set;}
        public int WaitingCount=>waiting.Count;
        public int OutputCount {get{var n=0;foreach(var pair in output)n+=pair.Value*pair.Key.ResultCount;return n;}}
        public int CountWaiting(RecipeDefinition recipe){var count=0;foreach(var entry in waiting)if(entry==recipe)count++;return count;}
        public int CountOutput(RecipeDefinition recipe)=>output.TryGetValue(recipe,out var n) ? n*recipe.ResultCount : 0;
        public bool Add(RecipeDefinition recipe,int count,InventoryModel bag,int capacity,float speed)
        {
            if(count<=0 || waiting.Count+(Current==null ? 0 : 1)+count>capacity)return false;
            if(!bag.TryConsumeCosts(Costs(recipe,count)))return false;
            for(var i=0;i<count;i++)waiting.Add(recipe);StartNext(speed);return true;
        }
        public bool Withdraw(RecipeDefinition recipe,InventoryModel bag)
        {var index=waiting.FindLastIndex(r=>r==recipe);if(index<0 || !bag.TryAddAmounts(Costs(recipe,1)))return false;waiting.RemoveAt(index);return true;}
        public void Advance(float seconds,float speed,int capacity)
        {
            if(seconds<0 || speed<=0)throw new ArgumentOutOfRangeException();StartNext(speed);
            while(Current!=null)
            {
                var consumed=Mathf.Min(seconds,Remaining);Remaining-=consumed;seconds-=consumed;
                if(Remaining>0 || OutputCount+Current.ResultCount>capacity)return;
                Version++;output[Current]=output.TryGetValue(Current,out var n) ? n+1 : 1;Current=null;StartNext(speed);if(seconds<=0)return;
            }
        }
        private void StartNext(float speed)
        {if(Current!=null || waiting.Count==0)return;Current=waiting[0];waiting.RemoveAt(0);Remaining=Current.ProcessingSeconds/speed;}
        public bool Collect(InventoryModel bag)
        {
            var amounts=new List<ItemAmount>();foreach(var pair in output)amounts.Add(new ItemAmount{Item=pair.Key.Result,Count=pair.Value*pair.Key.ResultCount,Charges=pair.Key.ResultCharges,Quality=pair.Key.ResultQuality});
            if(amounts.Count==0 || !bag.TryAddAmounts(amounts.ToArray()))return false;output.Clear();return true;
        }
        public ProcessingSnapshot Capture()
        {
            var reserved=new List<ProcessingEntry>();foreach(var recipe in waiting){var e=reserved.Count>0 && reserved[reserved.Count-1].Recipe==recipe.Id ? reserved[reserved.Count-1] : null;if(e==null)reserved.Add(new ProcessingEntry{Recipe=recipe.Id,Count=1});else e.Count++;}
            var completed=new List<ProcessingEntry>();foreach(var pair in output)completed.Add(new ProcessingEntry{Recipe=pair.Key.Id,Count=pair.Value});
            return new ProcessingSnapshot{Sequential=true,Recipe=Current==null ? null : Current.Id,Batch=Current==null ? 0 : 1,Remaining=Remaining,Waiting=reserved.ToArray(),Output=completed.ToArray()};
        }
        public void Restore(ProcessingSnapshot state,ContentCatalog catalog,float speed)
        {
            waiting.Clear();output.Clear();Current=null;Remaining=0;if(state==null)return;
            if(state.Sequential)
            {
                if(!string.IsNullOrEmpty(state.Recipe)){Current=catalog.Recipe(state.Recipe);Remaining=state.Remaining;}
                foreach(var e in state.Waiting)for(var i=0;i<e.Count;i++)waiting.Add(catalog.Recipe(e.Recipe));foreach(var e in state.Output)output[catalog.Recipe(e.Recipe)]=e.Count;
            }
            else if(!string.IsNullOrEmpty(state.Recipe))
            {
                var recipe=catalog.Recipe(state.Recipe);var unit=recipe.ProcessingSeconds/speed;
                var legacy=recipe.LegacyProcessingSeconds>0 ? recipe.LegacyProcessingSeconds/speed : unit;
                var done=Mathf.Clamp(Mathf.FloorToInt((1-state.Remaining/legacy)*state.Batch),0,state.Batch);
                if(done>0)output[recipe]=done;
                if(done<state.Batch){Current=recipe;Remaining=unit*Mathf.Clamp01(state.Remaining/legacy*state.Batch-(state.Batch-done-1));for(var i=done+1;i<state.Batch;i++)waiting.Add(recipe);}
            }
            StartNext(speed);
        }
        public ItemAmount[] Release()
        {
            var amounts=new List<ItemAmount>();if(Current!=null)amounts.AddRange(Costs(Current,1));foreach(var recipe in waiting)amounts.AddRange(Costs(recipe,1));foreach(var pair in output)amounts.Add(new ItemAmount{Item=pair.Key.Result,Count=pair.Value*pair.Key.ResultCount,Charges=pair.Key.ResultCharges,Quality=pair.Key.ResultQuality});
            Current=null;Remaining=0;waiting.Clear();output.Clear();return amounts.ToArray();
        }
        public static ItemAmount[] Costs(RecipeDefinition recipe,int count)
        {var result=new ItemAmount[recipe.Ingredients.Length];for(var i=0;i<result.Length;i++)result[i]=new ItemAmount{Item=recipe.Ingredients[i].Item,Count=checked(recipe.Ingredients[i].Count*count),Charges=recipe.Ingredients[i].Charges,Quality=recipe.Ingredients[i].Quality};return result;}
    }
}
