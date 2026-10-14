using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    internal static class QualityAssets
    {
        internal static void Configure(GameplayAssets assets)
        {
            var growth=CharacterPassAssets.Growth();
            if(growth.Skills.Length==4 && growth.Skills[0].NodePosition==Vector2.zero)
            {
                growth.Skills[0].NodePosition=new Vector2(-175,155);growth.Skills[1].NodePosition=new Vector2(-175,-115);
                growth.Skills[2].NodePosition=new Vector2(145,-115);growth.Skills[3].NodePosition=new Vector2(145,155);
                growth.Skills[2].Prerequisite=growth.Skills[1].Id;growth.Skills[3].Prerequisite=growth.Skills[1].Id;EditorUtility.SetDirty(growth);
            }
            var furnace=assets.Progression.Furnace;
            if(furnace.ProcessingMode!=ProcessingMode.Sequential)
            {
                furnace.ProcessingMode=ProcessingMode.Sequential;EditorUtility.SetDirty(furnace);
                foreach(var recipe in assets.Crafting.Catalog.Recipes)if(recipe.RequiredStation==furnace.Station && recipe.ProcessingSeconds>0)
                {recipe.LegacyProcessingSeconds=recipe.ProcessingSeconds;recipe.ProcessingSeconds/=furnace.ParallelCapacity;EditorUtility.SetDirty(recipe);}
            }
            var definition=assets.World.Generation;
            for(var i=0;i<definition.Resources.Length;i++)
            {
                var rule=definition.Resources[i];if(rule.RecoveryConfigured)continue;
                var kind=rule.Prefab.Definition.Kind;
                rule.RecoveryConfigured=rule.NaturalRecovery=true;rule.TargetDensityRatio=.9f;
                rule.Family=kind==ResourceKind.IronOre ? ResourceFamily.Mineral : kind==ResourceKind.Stone ? ResourceFamily.Stone : ResourceFamily.Timber;
                rule.Rarity=rule.Family==ResourceFamily.Mineral ? ResourceRarity.Rare : ResourceRarity.Common;rule.WorldRecoveryScope=rule.Rarity==ResourceRarity.Rare;
                rule.RespawnSeconds=rule.Family==ResourceFamily.Mineral ? 720 : rule.Family==ResourceFamily.Stone ? 600 : rule.Prefab.name.Contains("Log") ? 480 : rule.Prefab.name.Contains("Plant") ? 90 : 300;
                if(rule.Prefab.name.Contains("Plant"))rule.Family=ResourceFamily.Plant;
                rule.HarvestExclusionRadius=rule.Rarity==ResourceRarity.Rare ? 28 : 8;rule.HarvestExclusionSeconds=rule.Rarity==ResourceRarity.Rare ? 1800 : rule.RespawnSeconds*2;rule.ClusterSpacing=rule.Rarity==ResourceRarity.Rare ? 12 : rule.Spacing;
                definition.Resources[i]=rule;EditorUtility.SetDirty(definition);
            }
        }
    }
}
