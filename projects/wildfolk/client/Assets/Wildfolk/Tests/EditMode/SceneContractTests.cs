using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wildfolk.Tests
{
    public sealed class SceneContractTests
    {
        [Test] public void SavedSceneRetainsPlayableConnectionsAndNonPushingEnemy()
        {
            const string path="Assets/Wildfolk/Scenes/Wildfolk.unity";
            var scene=SceneManager.GetSceneByPath(path);var wasLoaded=scene.isLoaded;
            if(!wasLoaded) scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                PlayerMotor player=null;EnemyController enemy=null;var nodes=0;
                foreach(var root in scene.GetRootGameObjects())
                {
                    player=player ?? root.GetComponentInChildren<PlayerMotor>();
                    enemy=enemy ?? root.GetComponentInChildren<EnemyController>();
                    nodes+=root.GetComponentsInChildren<ResourceNode>().Length;
                    foreach(var transform in root.GetComponentsInChildren<Transform>(true))
                        Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject),Is.Zero);
                }
                Assert.That(player,Is.Not.Null);Assert.That(player.Session,Is.Not.Null);Assert.That(player.Joystick,Is.Not.Null);
                Assert.That(player.Combat.Session,Is.SameAs(player.Session));
                Assert.That(player.Combat.Equipment,Is.Not.Null);Assert.That(player.Avatar.Facing,Is.Not.Null);
                Assert.That(player.GetComponent<PlayerInventory>().Settings.InventorySlots,Is.EqualTo(36));
                Assert.That(player.GetComponent<PlayerPrimaryAction>().Placement,Is.Not.Null);
                Assert.That(player.GetComponent<PlayerCrafting>().Catalog.Recipes.Length,Is.EqualTo(103));
                Assert.That(player.Session.GetComponent<WorldPauseController>(),Is.Not.Null);
                Assert.That(Object.FindFirstObjectByType<InventoryPanel>().Slots.Length,Is.EqualTo(36));
                Assert.That(enemy,Is.Null,"The saved daytime scene must have no active Zombie.");
                enemy=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Enemy.prefab").GetComponent<EnemyController>();
                Assert.That(enemy.GetComponent<Collider2D>().isTrigger,Is.True);
                Assert.That(enemy.GetComponent<Rigidbody2D>().bodyType,Is.EqualTo(RigidbodyType2D.Kinematic));
                Assert.That(enemy.Facing,Is.Not.Null);
                var hotbar=Object.FindFirstObjectByType<HotbarView>();
                Assert.That(hotbar.Slots.Length,Is.EqualTo(PlayerInventory.HotbarSize));
                Assert.That(hotbar.Owner,Is.SameAs(player.GetComponent<PlayerInventory>()));
                Assert.That(nodes,Is.Zero,"Resources are generated from Seed rather than stored as manual instances.");
                var generator=player.Session.GetComponent<WorldGenerator>();Assert.That(generator,Is.Not.Null);Assert.That(generator.Player,Is.SameAs(player.GetComponent<PlayerInventory>()));
                Assert.That(generator.Definition.Resources.Length,Is.EqualTo(13));Assert.That(generator.Definition.Biome.Id,Is.EqualTo("temperate"));
                Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None),Is.Empty);
            }
            finally { if(!wasLoaded) EditorSceneManager.CloseScene(scene,true); }
        }

        [Test] public void BalanceDataHasValidCombatRangesAndProgressionItems()
        {
            var settings=AssetDatabase.LoadAssetAtPath<GameplaySettings>("Assets/Wildfolk/Data/GameplaySettings.asset");
            Assert.That(settings.PunchCooldown,Is.GreaterThanOrEqualTo(settings.PunchDuration));
            Assert.That(settings.EnemyAttackReach,Is.GreaterThan(settings.EnemyStopDistance));
            Assert.That(settings.EnemyStopDistance,Is.GreaterThan(settings.EnemySeparation));
            Assert.That(settings.DropPickupDelay,Is.GreaterThan(settings.DropTravelTime));
            foreach(var field in typeof(GameplaySettings).GetFields())
            {
                if(field.FieldType==typeof(float)) Assert.That((float)field.GetValue(settings),Is.GreaterThan(0),field.Name);
                if(field.FieldType==typeof(int)) Assert.That((int)field.GetValue(settings),Is.GreaterThan(0),field.Name);
            }
            Assert.That(settings.EnemyLoseRange,Is.GreaterThanOrEqualTo(settings.EnemyDetectionRange));
            Assert.That(settings.InventorySlots,Is.GreaterThanOrEqualTo(PlayerInventory.HotbarSize));
            Assert.That(AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/Wildfolk/Data"}).Length,Is.EqualTo(126));
            var ids=new HashSet<string>();
            foreach(var guid in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(item.MaxStack,Is.GreaterThan(0));Assert.That(item.Icon,Is.Not.Null);Assert.That(item.Id,Is.Not.Empty);
                Assert.That(ids.Add(item.Id),Is.True,"Duplicate item ID: "+item.Id);
                if(item.Category==ItemCategory.Consumable)
                {
                    Assert.That(item.Consumable,Is.Not.Null);
                    Assert.That(item.Consumable.HungerRestore,Is.GreaterThanOrEqualTo(0));Assert.That(item.Consumable.ThirstRestore,Is.GreaterThanOrEqualTo(0));
                    Assert.That(item.Consumable.HungerRestore+item.Consumable.ThirstRestore+item.Consumable.HealthRestore+item.Consumable.GradualHealthRestore+item.Consumable.StaminaRestore,Is.GreaterThan(0));
                }
                if(item.Category==ItemCategory.Placeable) {Assert.That(item.Placement.Prefab,Is.Not.Null);Assert.That(item.Placement.Item,Is.SameAs(item));}
                if(item.NavigationInstrument || item.FishingRod || item.EquippedLight!=null || item.Telescope || item.TurretModule!=null){Assert.That(item.Tool,Is.Null);Assert.That(item.MaxStack,Is.EqualTo(1));}
                else if(item.Category==ItemCategory.Tool || item.Category==ItemCategory.Weapon)
                {
                    Assert.That(item.MaxStack,Is.EqualTo(1));Assert.That(item.Tool,Is.Not.Null);
                    Assert.That(item.Tool.HeldSprite,Is.Not.Null);Assert.That(item.Tool.Damage,Is.GreaterThan(0));
                    Assert.That(item.Tool.Attack.Range,Is.GreaterThan(0));Assert.That(item.Tool.Duration,Is.GreaterThan(0));
                    Assert.That(item.Tool.Cooldown,Is.GreaterThanOrEqualTo(item.Tool.Duration));
                    var kinds=new HashSet<ResourceKind>();
                    foreach(var efficiency in item.Tool.Efficiencies)
                    {Assert.That(kinds.Add(efficiency.Kind),Is.True);Assert.That(efficiency.Multiplier,Is.GreaterThan(0));}
                    if(item.Tool.HarvestMethod==HarvestMethod.Strike){Assert.That(kinds,Does.Contain(ResourceKind.Wood));Assert.That(kinds,Does.Contain(ResourceKind.Stone));}
                    else{Assert.That(item.Tool.HarvestMethod,Is.EqualTo(HarvestMethod.Panning));Assert.That(kinds,Is.Empty);}
                    Assert.That(item.Tool.Tier,Is.Not.Null);
                }
            }
            var recipeIds=new HashSet<string>();
            foreach(var guid in AssetDatabase.FindAssets("t:RecipeDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var recipe=AssetDatabase.LoadAssetAtPath<RecipeDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(recipeIds.Add(recipe.Id),Is.True);Assert.That(recipe.Id,Is.Not.Empty);
                Assert.That(recipe.Result,Is.Not.Null);Assert.That(recipe.ResultCount,Is.GreaterThan(0));
                Assert.That(recipe.ProcessingSeconds,Is.GreaterThanOrEqualTo(0));
                if(recipe.RequiredStation==StationKind.Campfire) Assert.That(recipe.ProcessingSeconds,Is.GreaterThan(0));
                var ingredients=new HashSet<string>();
                foreach(var cost in recipe.Ingredients)
                {Assert.That(cost.Item,Is.Not.Null);Assert.That(cost.Count,Is.GreaterThan(0));Assert.That(ingredients.Add(cost.Item.Id),Is.True);}
            }
            Assert.That(recipeIds.Count,Is.EqualTo(103));
            Assert.That(System.IO.File.Exists("Assets/Wildfolk/Data/StartingEquipment.asset"),Is.False);
            foreach(var guid in AssetDatabase.FindAssets("t:VitalityDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var vitality=AssetDatabase.LoadAssetAtPath<VitalityDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(vitality.MaximumHealth,Is.GreaterThan(0));
            }
            foreach(var guid in AssetDatabase.FindAssets("t:ResourceDefinition",new[]{"Assets/Wildfolk/Data"}))
            {
                var resource=AssetDatabase.LoadAssetAtPath<ResourceDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(resource.Vitality,Is.Not.Null);Assert.That(resource.DropItem,Is.Not.Null);
                Assert.That(resource.DropCount,Is.InRange(1,resource.DropItem.MaxStack));
                if(resource.DropItem.Id=="medicinal_herb"){Assert.That(resource.Kind,Is.EqualTo(ResourceKind.MedicinalHerb));continue;}
                Assert.That(resource.Kind,Is.EqualTo(resource.DropItem.Id=="wood" ? ResourceKind.Wood : resource.DropItem.Id=="stone" ? ResourceKind.Stone : resource.DropItem.Id=="iron_ore" ? ResourceKind.IronOre : resource.DropItem.Id=="copper_ore" ? ResourceKind.CopperOre : resource.DropItem.Id=="tin_ore" ? ResourceKind.TinOre : resource.DropItem.Id=="coal" ? ResourceKind.Coal : resource.DropItem.Id=="plant_fiber" ? ResourceKind.PlantFiber : resource.DropItem.Id=="berry" ? ResourceKind.Berry : resource.DropItem.Id=="mushroom" ? ResourceKind.Mushroom : resource.DropItem.Id=="diamond" ? ResourceKind.Diamond : resource.DropItem.Id=="crystal" ? ResourceKind.Crystal : ResourceKind.Meat));
            }
        }
    }
}
