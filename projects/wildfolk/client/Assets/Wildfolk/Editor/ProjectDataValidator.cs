using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class ProjectDataValidator
    {
        private static T[] Assets<T>() where T:UnityEngine.Object
        {var result=new List<T>();foreach(var guid in AssetDatabase.FindAssets("t:"+typeof(T).Name,new[]{"Assets/Wildfolk/Data"}))result.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));return result.ToArray();}
        [MenuItem("Wildfolk/Developer/Validate Data")]
        public static void Run(){var errors=Validate();if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));Debug.Log("Wildfolk data validation passed.");}
        public static List<string> Validate()
        {
            var errors=new List<string>();Action<bool,string> check=(valid,error)=>{if(!valid)errors.Add(error);};
            var items=Assets<ItemDefinition>();var itemSet=new HashSet<ItemDefinition>(items);var ids=new HashSet<string>();
            var tiers=new HashSet<EquipmentTier>(Assets<EquipmentTier>());var ranks=new HashSet<int>();
            foreach(var tier in tiers){check(!string.IsNullOrWhiteSpace(tier.Id) && ids.Add("tier:"+tier.Id),"Duplicate/empty tier ID");check(tier.Rank>0 && ranks.Add(tier.Rank),"Invalid tier rank: "+tier.name);}
            foreach(var item in items)
            {
                check(!string.IsNullOrWhiteSpace(item.Id) && ids.Add("item:"+item.Id),"Duplicate/empty item ID: "+item.name);check(item.MaxStack>0 && item.Icon!=null,"Invalid item: "+item.name);
                if(item.Tool!=null){check(tiers.Contains(item.Tool.Tier),"Invalid tool tier: "+item.Id);Attack(item.Tool.Attack,check,item.Id);check(item.Tool.Damage>0 && item.Tool.Duration>0 && item.Tool.Cooldown>=item.Tool.Duration,"Invalid tool timing: "+item.Id);}
                if(item.Placement!=null)check(item.Placement.Prefab!=null && item.Placement.Item==item && item.Placement.ParallelCapacity>0 && item.Placement.ProcessingSpeed>0,"Invalid placeable: "+item.Id);
                if(item.Container!=null)check(item.Container.Empty==item || item.Container.Raw==item || item.Container.Clean==item,"Unknown container state: "+item.Id);
            }
            var graph=new Dictionary<ItemDefinition,List<ItemDefinition>>();
            foreach(var recipe in Assets<RecipeDefinition>())
            {
                check(!string.IsNullOrWhiteSpace(recipe.Id) && ids.Add("recipe:"+recipe.Id),"Duplicate recipe ID: "+recipe.name);
                check(itemSet.Contains(recipe.Result) && recipe.ResultCount>0,"Unknown result: "+recipe.name);
                check(Enum.IsDefined(typeof(StationKind),recipe.RequiredStation) && recipe.RequiredWorkbenchTier>=1,"Invalid station requirement: "+recipe.name);
                check(recipe.FutureUnlocks!=null && recipe.FutureUnlocks.Length==0,"Additional unlock conditions are reserved, not active: "+recipe.name);
                check(recipe.Tier==null || tiers.Contains(recipe.Tier),"Invalid recipe tier: "+recipe.name);
                check(recipe.Ingredients!=null && recipe.Ingredients.Length>0,"Empty recipe: "+recipe.name);if(recipe.Ingredients==null)continue;
                var costs=new HashSet<ItemDefinition>();
                foreach(var cost in recipe.Ingredients)
                {
                    check(itemSet.Contains(cost.Item) && cost.Count>0 && costs.Add(cost.Item),"Invalid ingredient: "+recipe.name);
                    if(cost.Item==null || recipe.Result==null || cost.Item.Tool==null || recipe.Result.Tool==null)continue;
                    if(!graph.TryGetValue(cost.Item,out var outputs)){outputs=new List<ItemDefinition>();graph.Add(cost.Item,outputs);}outputs.Add(recipe.Result);
                    check(cost.Item.Tier!=null && recipe.Result.Tier!=null && cost.Item.Tier.Rank<recipe.Result.Tier.Rank,"Upgrade tier does not advance: "+recipe.name);
                }
            }
            var visited=new HashSet<ItemDefinition>();var visiting=new HashSet<ItemDefinition>();foreach(var item in graph.Keys)check(!Cycle(item,graph,visited,visiting),"Upgrade chain cycle: "+item.Id);
            foreach(var loot in Assets<LootTable>())
            {
                check(loot.Experience>=0 && loot.Gold>=0,"Negative loot reward: "+loot.name);
                foreach(var drop in loot.Items)check(itemSet.Contains(drop.Item) && drop.Minimum>0 && drop.Maximum>=drop.Minimum && drop.Item!=null && drop.Maximum<=drop.Item.MaxStack && drop.Chance>=0 && drop.Chance<=1,"Invalid drop: "+loot.name);
            }
            foreach(var resource in Assets<ResourceDefinition>())check(resource.DropItem!=null && itemSet.Contains(resource.DropItem) && resource.DropCount>0 && resource.DropCount<=resource.DropItem.MaxStack && (resource.MinimumTier==null || tiers.Contains(resource.MinimumTier)) && Enum.IsDefined(typeof(ToolKind),resource.RequiredTool),"Invalid resource/tier: "+resource.name);
            foreach(var definition in Assets<WorldGenerationDefinition>())
            {
                check(definition.Biome!=null && definition.Size.x>0 && definition.Size.y>0 && definition.ShallowMoveMultiplier>0 && definition.ShallowMoveMultiplier<1,"Invalid world data");
                foreach(var rule in definition.Resources)check(rule.Prefab!=null && rule.Count>0 && rule.StartCount>=0 && rule.StartCount<=rule.Count && rule.Spacing>0 && rule.RespawnSeconds>0 && rule.ScaleRange.x>0 && rule.ScaleRange.y>=rule.ScaleRange.x,"Invalid generation/renewal rule");
            }
            foreach(var spawn in Assets<ZombieSpawnDefinition>())check(spawn.InitialPopulation>0 && spawn.HardCap>=spawn.InitialPopulation && spawn.NightsPerStep>0 && spawn.SpawnNear>=spawn.PlayerDistance && spawn.SpawnFar>spawn.SpawnNear && spawn.InitialInterval>0 && spawn.RefillInterval>0 && spawn.LocalThreatRadius>=spawn.SpawnFar && spawn.InactiveDistance>spawn.LocalThreatRadius && spawn.LoseRange<spawn.InactiveDistance,"Invalid zombie spawn data");
            foreach(var goblin in Assets<GoblinDefinition>())check(goblin.OuterTerritory>goblin.Territory && goblin.PersonalSpace>0 && goblin.Leash>goblin.OuterTerritory && goblin.Population>0 && goblin.EncounterChance>=0 && goblin.EncounterChance<=1,"Invalid Goblin data");
            foreach(var container in Assets<WaterContainerDefinition>())check(!string.IsNullOrEmpty(container.Id) && container.Empty!=null && container.Raw!=null && container.Clean!=null && container.Empty.Liquid==LiquidState.Empty && container.Raw.Liquid==LiquidState.Raw && container.Clean.Liquid==LiquidState.Clean && container.Raw.Consumable!=null && container.Clean.Consumable!=null,"Invalid container state graph");
            var settings=Assets<GameplaySettings>();foreach(var setting in settings)Attack(setting.PunchAttack,check,"fist");
            if(EditorApplication.isPlaying)
            {
                var saveIds=new HashSet<string>();foreach(var placed in UnityEngine.Object.FindObjectsByType<WorldPlaceable>(FindObjectsSortMode.None))check(!string.IsNullOrEmpty(placed.SaveId) && saveIds.Add(placed.SaveId),"Save ID collision");
                foreach(var resource in UnityEngine.Object.FindObjectsByType<ResourceIdentity>(FindObjectsSortMode.None))check(saveIds.Add(resource.Id),"Resource save ID collision");
            }
            return errors;
        }
        private static void Attack(AttackProfile attack,Action<bool,string> check,string id)=>check(attack!=null && attack.MaxTargets>0 && attack.MaxTargets<=32 && attack.Range>0 && attack.ArcAngle>0 && attack.ArcAngle<=360 && attack.PierceCount>=0 && attack.AoERadius>=0,"Invalid attack profile: "+id);
        private static bool Cycle(ItemDefinition node,Dictionary<ItemDefinition,List<ItemDefinition>> graph,HashSet<ItemDefinition> visited,HashSet<ItemDefinition> visiting)
        {if(visiting.Contains(node))return true;if(visited.Contains(node))return false;visiting.Add(node);if(graph.TryGetValue(node,out var edges))foreach(var next in edges)if(Cycle(next,graph,visited,visiting))return true;visiting.Remove(node);visited.Add(node);return false;}
    }
}
