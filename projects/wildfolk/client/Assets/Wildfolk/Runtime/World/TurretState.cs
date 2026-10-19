using System;
using System.IO;
using UnityEngine;
namespace Wildfolk
{
    [Serializable] public sealed class TurretSnapshot
    {
        public SavedStack[] Ammo;
        public string[] Modules=new string[5];
        public float Heading,ReloadRemaining;
        public bool Loaded;
    }
    public static class TurretState
    {
        // Unity's inline JSON serializer may round-trip a null nested class as an empty object.
        public static bool IsEmpty(TurretSnapshot s)
        {
            if(s==null)return true;if(s.Loaded || s.Heading!=0 || s.ReloadRemaining!=0)return false;
            if(s.Ammo!=null)foreach(var a in s.Ammo)if(a!=null && (!string.IsNullOrEmpty(a.Item) || a.Count!=0 || a.Charges!=0 || a.Quality!=LiquidState.Empty))return false;
            if(s.Modules!=null)foreach(var id in s.Modules)if(!string.IsNullOrEmpty(id))return false;return true;
        }
        public static void Validate(TurretSnapshot s,PlaceableDefinition definition,ContentCatalog catalog)
        {
            if(s==null || definition.Turret==null || s.Modules==null || s.Modules.Length!=5 || !float.IsFinite(s.Heading) || !float.IsFinite(s.ReloadRemaining) || s.ReloadRemaining<0 || s.ReloadRemaining>definition.Turret.ReloadSeconds+.1f)throw new InvalidDataException("Invalid turret state");
            var slots=1;
            for(var i=0;i<5;i++)if(!string.IsNullOrEmpty(s.Modules[i])){var m=catalog.Item(s.Modules[i]).TurretModule;if(m==null || (int)m.Slot!=i)throw new InvalidDataException("Invalid turret module role");if(i==(int)TurretModuleSlot.Ammo)slots+=m.ExtraAmmoSlots;}
            var model=new InventoryModel(3);model.Restore(s.Ammo,catalog);
            for(var i=0;i<3;i++){var stack=model.GetSlot(i);if(!stack.IsEmpty && (i>=slots || stack.Item!=definition.Turret.Ammunition))throw new InvalidDataException("Incompatible turret ammunition");}
        }
        public static int MaximumHP(PlaceableDefinition d,TurretSnapshot s,ContentCatalog catalog)
        {if(s?.Modules==null || s.Modules.Length!=5 || string.IsNullOrEmpty(s.Modules[4]))return d.Vitality.MaximumHealth;var m=catalog.Item(s.Modules[4]).TurretModule;return Mathf.RoundToInt(d.Vitality.MaximumHealth*(m==null ? 1 : m.Multiplier));}
    }
}
