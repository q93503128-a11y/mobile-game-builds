using System;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    public static class DeathSaveValidation
    {
        public static void Migrate(SavedGame game,WorldGenerationDefinition definition,WorldMap map=null)
        {
            var spawn=(map??new WorldMap(definition,game.Seed,game.BaseGenerationVersion,false)).Spawn;
            void Convert(PlayerSnapshot p){if(p==null)throw new InvalidDataException("Missing player.");p.Death=new DeathSnapshot{Home=spawn,Run=Guid.NewGuid().ToString("N"),Dead=p.HP==0};}
            Convert(game.Player);
            if(game.IsMultiplayer && game.Multiplayer!=null)
            {
                foreach(var p in game.Multiplayer.Players){if(p.Id==game.Multiplayer.HostId)p.State.Death=game.Player.Death;else Convert(p.State);}
            }
            game.Version=12;
        }
        public static void Validate(PlayerSnapshot p,WorldMap map)
        {
            var d=p?.Death;
            if(d==null || !LocalPlayerIdentity.Valid(d.Run) || d.Death<0 || d.Dead!=(p.HP==0) || float.IsNaN(d.Home.x) || float.IsNaN(d.Home.y) || float.IsInfinity(d.Home.x) || float.IsInfinity(d.Home.y) || !map.Contains(d.Home) || !map.IsWalkable(d.Home,0))throw new InvalidDataException("Invalid player lifecycle/home.");
        }
    }
}
