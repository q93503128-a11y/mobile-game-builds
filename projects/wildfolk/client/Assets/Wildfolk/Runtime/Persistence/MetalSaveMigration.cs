using System;
using System.Collections.Generic;
using System.IO;
namespace Wildfolk
{
    // Content migration for v1-v5: preserve slots, remove retired discovery IDs and refund
    // only old basic-furnace Steel cargo. No world spawning or user file writes occur here.
    public static class MetalSaveMigration
    {
        public static void NormalizeLegacyContent(SavedGame game,ContentCatalog catalog)
        {
            Seen(game.Player);if(game.IsMultiplayer && game.Multiplayer?.Players!=null)foreach(var p in game.Multiplayer.Players)if(p!=null)Seen(p.State);
            if(game.Placeables==null)return;var drops=new List<PickupSnapshot>(game.Pickups??Array.Empty<PickupSnapshot>());
            foreach(var placed in game.Placeables)
            {
                if(placed==null)continue;
                if(placed.Item=="reinforced_chest" && placed.Inventory!=null && placed.Inventory.Length==32)
                {var next=new SavedStack[36];Array.Copy(placed.Inventory,next,32);for(var i=32;i<next.Length;i++)next[i]=new SavedStack();placed.Inventory=next;}
                var queue=placed.Processing;if(placed.Item!="furnace" || queue==null)continue;
                if(queue.Recipe=="steel")
                {Refund(queue.Batch,false,placed,catalog,drops);queue.Recipe=null;queue.Batch=0;queue.Remaining=0;}
                if(queue.Waiting!=null){var keep=new List<ProcessingEntry>();foreach(var entry in queue.Waiting){if(entry==null)throw new InvalidDataException("Invalid legacy queue");if(entry.Recipe=="steel")Refund(entry.Count,false,placed,catalog,drops);else keep.Add(entry);}queue.Waiting=keep.ToArray();}
                if(queue.Output!=null){var keep=new List<ProcessingEntry>();var steel=false;foreach(var entry in queue.Output){if(entry==null)throw new InvalidDataException("Invalid legacy output");if(entry.Recipe=="steel"){if(steel)throw new InvalidDataException("Duplicate legacy output");steel=true;Refund(entry.Count,true,placed,catalog,drops);}else keep.Add(entry);}queue.Output=keep.ToArray();}
            }
            game.Pickups=drops.ToArray();
        }
        private static void Seen(PlayerSnapshot player)
        {if(player?.SeenRecipes==null)return;var keep=new List<string>();foreach(var id in player.SeenRecipes)if(id!="bronze_chest" && id!="steel_wall")keep.Add(id);player.SeenRecipes=keep.ToArray();}
        private static void Refund(int count,bool complete,PlaceableSnapshot placed,ContentCatalog catalog,List<PickupSnapshot> drops)
        {
            if(count<1 || count>100)throw new InvalidDataException("Invalid legacy Steel cargo");
            if(complete)Drop("steel",count,placed,catalog,drops);else{Drop("iron_ingot",count,placed,catalog,drops);Drop("coal",count,placed,catalog,drops);}
        }
        private static void Drop(string id,int count,PlaceableSnapshot placed,ContentCatalog catalog,List<PickupSnapshot> drops)
        {var max=catalog.Item(id).MaxStack;while(count>0){var part=Math.Min(count,max);drops.Add(new PickupSnapshot{Item=id,Count=part,Position=placed.Position});count-=part;}}
    }
}
