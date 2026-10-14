using System;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    internal sealed class WorldAssets
    {
        internal WorldGenerationDefinition Generation;
        internal GameplayLightDefinition Light;
        internal GoblinDefinition Goblin;
        internal LootTable ZombieLoot,GoblinLoot,ChickenLoot,BoarLoot;
        internal ResourceDefinition Log;
        internal static WorldAssets Load(GameplayAssets a)
        {
            var w=new WorldAssets();
            var biome=GameplayAssets.Asset<BiomeDefinition>("TemperateBiome",b=>{});
            w.Generation=GameplayAssets.Asset<WorldGenerationDefinition>("WorldGeneration",g=>g.Biome=biome);
            w.Light=GameplayAssets.Asset<GameplayLightDefinition>("FacilityLight",l=>l.Radius=4);
            w.Goblin=GameplayAssets.Asset<GoblinDefinition>("Goblin",g=>{});
            var feather=Material("Feather","feather","Feather");var leather=Material("Leather","leather","Leather");
            w.ChickenLoot=Loot("ChickenLoot",new[]{Entry(a.Survival.Raw,1),Entry(feather,1)},0,0);
            w.BoarLoot=Loot("BoarLoot",new[]{Entry(a.Survival.Raw,2),Entry(leather,1)},0,0);
            w.ZombieLoot=Loot("ZombieLoot",Array.Empty<LootEntry>(),5,1);
            w.GoblinLoot=Loot("GoblinLoot",Array.Empty<LootEntry>(),8,3);
            var logHealth=GameplayAssets.Asset<VitalityDefinition>("FallenLogVitality",v=>v.MaximumHealth=50);
            w.Log=GameplayAssets.Asset<ResourceDefinition>("FallenLogResource",r=>{r.Kind=ResourceKind.Wood;r.DropItem=a.Tree.DropItem;r.DropCount=4;r.Vitality=logHealth;});
            return w;
        }
        private static ItemDefinition Material(string name,string id,string display)=>GameplayAssets.Asset<ItemDefinition>(name,i=>{i.Id=id;i.DisplayName=display;i.Icon=VisualArtImporter.Sprite(id);i.MaxStack=50;i.Category=ItemCategory.Material;});
        private static LootEntry Entry(ItemDefinition item,int count)=>new LootEntry{Item=item,Chance=1,Minimum=count,Maximum=count};
        private static LootTable Loot(string name,LootEntry[] items,int xp,int gold)=>GameplayAssets.Asset<LootTable>(name,l=>{l.Items=items;l.Experience=xp;l.Gold=gold;});
    }
}
