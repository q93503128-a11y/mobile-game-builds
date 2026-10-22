using System;
using System.IO;
using System.Linq;
namespace Wildfolk
{
    public static class ClassSaveValidation
    {
        public static void Migrate(SavedGame game)
        {foreach(var p in game.Placeables)p.PlayerBuilt=(!game.IsMultiplayer || LocalPlayerIdentity.Valid(p.Creator)) && !p.Id.EndsWith(":cargo",StringComparison.Ordinal);Init(game.Player);if(game.Multiplayer?.Players!=null)foreach(var p in game.Multiplayer.Players)Init(p.State);}
        private static void Init(PlayerSnapshot p){p.SessionClass="survivor";p.SurvivalCooldown=0;p.ClassPurchases=Array.Empty<string>();}
        public static void Validate(PlayerSnapshot p)
        {if(p==null || ClassCatalog.Current.Find(p.SessionClass)==null || !float.IsFinite(p.SurvivalCooldown) || p.SurvivalCooldown<0 || p.SurvivalCooldown>360 || p.ClassPurchases==null || p.ClassPurchases.Distinct().Count()!=p.ClassPurchases.Length || p.ClassPurchases.Any(x=>!Guid.TryParseExact(x,"N",out _)))throw new InvalidDataException("Invalid class session state");}
    }
}
