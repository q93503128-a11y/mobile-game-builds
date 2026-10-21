using System;
using System.Collections.Generic;
using UnityEditor;
namespace Wildfolk.Editor
{
    internal static class FishingDataValidator
    {
        internal static void Validate(List<string> errors)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ContentCatalog>("Assets/Wildfolk/Data/ContentCatalog.asset");var found=AssetDatabase.FindAssets("t:FishingLootTable",new[]{"Assets/Wildfolk/Data"});if(found.Length!=1)errors.Add("Expected one canonical fishing table");
            foreach(var guid in found)
            {
                var table=AssetDatabase.LoadAssetAtPath<FishingLootTable>(AssetDatabase.GUIDToAssetPath(guid));var ids=new HashSet<string>();
                foreach(var e in table.Entries)
                {if(e==null || string.IsNullOrEmpty(e.Id) || !ids.Add(e.Id) || e.Output==null || Array.IndexOf(catalog.Items,e.Output)<0 || e.Amount<1 || e.Amount>e.Output.MaxStack || e.Weight<=0 || float.IsNaN(e.Weight) || float.IsInfinity(e.Weight) || e.MinimumShoreInset<0 || float.IsNaN(e.MinimumShoreInset) || float.IsInfinity(e.MinimumShoreInset) || e.Waters==0 || ((int)e.Waters&~15)!=0 || !Enum.IsDefined(typeof(FishingTime),e.Time) || !L.Has(e.NameKey))errors.Add("Invalid fishing entry: "+e?.Id);}
                foreach(FishingWater water in Enum.GetValues(typeof(FishingWater)))foreach(var night in new[]{false,true})if(table.Roll(new FishingWaterContext(water,night,false,0),.5f)<0)errors.Add("Fishing has no shallow fallback: "+water+"/"+night);
            }
        }
    }
}
