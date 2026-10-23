using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    public static class BossSaveValidation
    {
        public static void MigrateAndValidate(SavedGame game,SaveGameRules rules)
        {
            if(game.Version==13)
            {game.Bosses=Array.Empty<BossSnapshot>();game.Player.Accessory=null;if(game.Multiplayer?.Players!=null)foreach(var p in game.Multiplayer.Players)p.State.Accessory=null;game.Version=14;}
            if(game.Bosses==null || game.Bosses.Length>rules.Bosses.Length)throw new InvalidDataException("Invalid boss encounter list");
            var ids=new HashSet<string>();var time=new WorldTimeModel(rules.Clock);time.SetElapsed(game.Elapsed);var map=rules.ValidationMap(game);
            foreach(var r in game.Bosses)
            {
                var definition=r==null?null:Array.Find(rules.Bosses,d=>d.Id==r.Boss);
                if(definition==null || !ids.Add(r.Boss) || r.LastNight<0 || r.LastNight>time.Day || r.HP<0 || r.HP>definition.Vitality.MaximumHealth || r.RolledLoot==null || r.Eligible==null || r.Eligible.Length>4)throw new InvalidDataException("Invalid boss state");
                if(r.LastNight==0){if(r.Active || r.PendingReward || r.HP!=0 || r.RolledLoot.Length!=0 || r.Eligible.Length!=0 || !string.IsNullOrEmpty(r.Tomb))throw new InvalidDataException("Unfunded encounter");continue;}
                if(!Finite(r.Home) || !Finite(r.Position) || !map.Contains(r.Position) || !map.Contains(r.Home))throw new InvalidDataException("Invalid boss position");
                WorldAnchor anchor=null;if(map.Streamed)foreach(var a in map.Continental.Anchors)if(a.Id==r.Tomb && a.Kind==definition.AnchorKind){anchor=a;break;}
                if(anchor==null || Vector2.Distance(r.Home,anchor.Position+definition.SpawnOffset)>.01f || !Array.Exists(game.Features,f=>f.Id==r.Tomb+":guard" && f.Cleared))throw new InvalidDataException("Locked or unknown royal tomb");
                if(r.PendingReward && (!r.Active || r.HP!=0) || r.Active && r.HP==0 && !r.PendingReward)throw new InvalidDataException("Invalid boss death transaction");
                var people=new HashSet<string>();foreach(var id in r.Eligible)if(!LocalPlayerIdentity.Valid(id) || !people.Add(id))throw new InvalidDataException("Invalid encounter participant");
                if(!r.PendingReward){if(r.RolledLoot.Length!=0 || r.Eligible.Length!=0)throw new InvalidDataException("Unexpected pending rewards");continue;}
                var entries=new List<LootEntry>(definition.Rewards.Items);entries.AddRange(definition.Rewards.RareItems);var items=new HashSet<string>();
                foreach(var s in r.RolledLoot)
                {if(s==null)throw new InvalidDataException("Missing boss stack");var found=entries.Find(e=>e.Item.Id==s.Item);if( found.Item==null || !items.Add(s.Item) || s.Count<found.Minimum || s.Count>found.Maximum || s.Charges!=0 || s.Quality!=LiquidState.Empty)throw new InvalidDataException("Invalid boss reward roll");}
                foreach(var entry in entries)if(entry.Chance>=1 && !items.Contains(entry.Item.Id))throw new InvalidDataException("Missing guaranteed boss reward");
            }
        }
        private static bool Finite(Vector2 p)=>float.IsFinite(p.x) && float.IsFinite(p.y);
    }
}
