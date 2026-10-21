using System;
using UnityEngine;
namespace Wildfolk
{
    [Serializable]public sealed class PoiSupplyVariant {public ItemAmount[] Items=Array.Empty<ItemAmount>();}
    [CreateAssetMenu(menuName="Wildfolk/POI Content")]
    public sealed class PoiContentDefinition:ScriptableObject
    {
        public PoiSite Prefab;
        public PoiSupplyVariant[] Campsite=Array.Empty<PoiSupplyVariant>();
        public ItemAmount[] Graveyard=Array.Empty<ItemAmount>();
        public int[] QuarryRules=Array.Empty<int>();
        public int WellCapacity=3;
        public float WellRefillSeconds=480;
        public static bool Supports(WorldAnchorKind kind)=>kind==WorldAnchorKind.Campsite || kind==WorldAnchorKind.Well || kind==WorldAnchorKind.Quarry || kind==WorldAnchorKind.Graveyard;
        public SavedStack[] Supplies(WorldAnchorKind kind,int seed,string id)
        {
            var slots=new InventoryModel(6).Capture();
            var roll=(uint)WorldSeeds.Derive(seed,SeedDomain.POI,WorldSeeds.StableText(id+":content"));
            var items=kind==WorldAnchorKind.Campsite ? Campsite[roll%(uint)Campsite.Length].Items : kind==WorldAnchorKind.Graveyard ? Graveyard : Array.Empty<ItemAmount>();
            for(var i=0;i<items.Length;i++)slots[i]=new SavedStack{Item=items[i].Item.Id,Count=items[i].Count,Charges=items[i].Charges,Quality=items[i].Quality};
            return slots;
        }
    }
}
