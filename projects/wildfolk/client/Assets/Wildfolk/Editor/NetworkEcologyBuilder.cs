using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using static Wildfolk.Editor.WorldArt;
namespace Wildfolk.Editor
{
    public static class NetworkEcologyBuilder
    {
        public const string PrefabPath="Assets/Wildfolk/Prefabs/NetworkEcology.prefab";
        private static T Data<T>(string name) where T:Object=>AssetDatabase.LoadAssetAtPath<T>("Assets/Wildfolk/Data/"+name+".asset");
        private static T Actor<T>(string name) where T:Component=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/"+name+".prefab").GetComponent<T>();
        public static GameObject BuildPrefab()
        {
            var entries=new List<NetworkSpawnEntry>();
            foreach(var name in new[]{"Chicken","Boar","Rabbit","Deer","Sheep","Enemy","Goblin","SlingshotGoblin","Skeleton","Pebble","GoblinCamp","Tree","Stone","IronOreNode","FallenLog","SmallPlant","WorldPickup","StorageChest","ReinforcedChest","Workbench","WorkbenchII","Furnace","Campfire","WoodWall","StoneWall","WoodDoor"})
            {
                var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/"+name+".prefab");
                var root=(GameObject)PrefabUtility.InstantiatePrefab(source);root.name="Network "+name;root.AddComponent<NetworkObject>();
                if(root.GetComponent<CreatureMember>()!=null)
                {
                    var transform=root.AddComponent<NetworkTransform>();transform.SyncPositionZ=false;transform.SyncRotAngleX=transform.SyncRotAngleY=transform.SyncRotAngleZ=false;transform.SyncScaleX=transform.SyncScaleY=transform.SyncScaleZ=false;transform.PositionThreshold=.04f;transform.Interpolate=true;transform.UseUnreliableDeltas=true;
                    root.AddComponent<NetworkRigidbody2D>();var creature=root.AddComponent<NetworkCreature>();
                    var animal=root.GetComponent<NeutralAnimal>();var enemy=root.GetComponent<EnemyController>();var goblin=root.GetComponent<GoblinController>();creature.Visual=animal!=null ? animal.Visual : enemy!=null ? enemy.AttackPose : goblin.Pose;
                }
                if(root.GetComponent<WorldPlaceable>()!=null){root.AddComponent<NetworkBuilding>();if(root.GetComponent<CraftingStation>()!=null || root.GetComponent<StorageChest>()!=null)root.AddComponent<NetworkFacility>();}
                if(root.GetComponent<ResourceNode>()!=null)root.AddComponent<NetworkResource>();
                if(root.GetComponent<WorldPickup>()!=null){root.AddComponent<NetworkGroundItem>();var trigger=root.AddComponent<CircleCollider2D>();trigger.isTrigger=true;trigger.radius=Data<GameplaySettings>("GameplaySettings").PickupRadius;var body=root.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;}
                if(root.GetComponent<StoneProjectile>()!=null)root.AddComponent<NetworkProjectile>();
                if(root.GetComponent<GoblinCamp>()!=null)root.AddComponent<NetworkCamp>();
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Wildfolk/Prefabs/Network"+name+".prefab");Object.DestroyImmediate(root);
                entries.Add(new NetworkSpawnEntry{Source=source,NetworkPrefab=prefab});
            }
            var go=new GameObject("Host Creature Ecology");go.SetActive(false);
            var anchor=new GameObject("Ecology Anchor");anchor.transform.SetParent(go.transform);var health=anchor.AddComponent<Health>();health.Definition=Data<VitalityDefinition>("PlayerVitality");var inventory=anchor.AddComponent<PlayerInventory>();inventory.Settings=Data<GameplaySettings>("GameplaySettings");
            var session=go.AddComponent<GameplaySession>();session.Mode=SessionMode.Multiplayer;session.PlayerHealth=health;
            var clock=go.AddComponent<WorldClock>();clock.Definition=Data<DayNightDefinition>("DayNight");clock.Session=session;
            var region=go.AddComponent<WorldSpawnRegion>();
            var ground=Sprite("Forest Floor","ground",go.transform,Vector2.zero,1,WorldSorting.Ground);
            var terrain=go.AddComponent<WorldTerrainRenderer>();terrain.Ground=ground;
            var world=go.AddComponent<WorldGenerator>();world.GenerateOnAwake=false;world.Definition=Data<WorldGenerationDefinition>("WorldGeneration");world.Region=region;world.Session=session;world.Player=inventory;world.Pickup=Actor<WorldPickup>("WorldPickup");world.Settings=Data<GameplaySettings>("GameplaySettings");world.Terrain=terrain;world.SpriteMaterial=SpriteMaterial;world.Grass=VisualArtImporter.Sprite("grass");world.Fern=VisualArtImporter.Sprite("fern");world.Water=Data<WaterSourceDefinition>("NaturalWater");
            world.Environment=new GameObject("Generated Environment").transform;world.Environment.SetParent(go.transform);
            var nav=go.AddComponent<WorldNavigation>();nav.World=world;nav.Player=anchor.transform;
            var renewal=go.AddComponent<ResourceRegeneration>();renewal.World=world;renewal.Clock=clock;renewal.Navigation=nav;renewal.enabled=false;world.Renewal=renewal;
            var animals=go.AddComponent<AnimalPopulation>();animals.Definition=Data<AnimalPopulationDefinition>("AnimalPopulation");animals.World=world;animals.Clock=clock;animals.Navigation=nav;world.Animals=animals;
            var night=go.AddComponent<ZombieSpawner>();night.World=world;night.Definition=Data<ZombieSpawnDefinition>("ZombieSpawn");night.Prefab=Actor<EnemyController>("Enemy");night.SecondaryPrefab=Actor<EnemyController>("Skeleton");night.Clock=clock;night.Session=session;night.Player=health;night.Region=region;
            var goblins=go.AddComponent<GoblinSpawner>();goblins.World=world;goblins.Definition=Data<GoblinDefinition>("Goblin");goblins.CampDefinition=Data<GoblinCampDefinition>("GoblinCamp");goblins.Prefab=Actor<GoblinController>("Goblin");goblins.RangedPrefab=Actor<GoblinController>("SlingshotGoblin");goblins.Clock=clock;goblins.Session=session;goblins.Player=health;
            go.AddComponent<NetworkObject>();go.AddComponent<NetworkWorldState>();var persistence=go.AddComponent<NetworkWorldPersistence>();persistence.Rules=AssetDatabase.LoadAssetAtPath<SaveGameRules>("Assets/Wildfolk/Resources/SaveGameRules.asset");
            var service=go.AddComponent<NetworkCreatureWorld>();service.World=world;service.Catalog=Data<ContentCatalog>("ContentCatalog");service.Session=session;service.Clock=clock;service.Animals=animals;service.Goblins=goblins;service.Night=night;service.Prefabs=entries.ToArray();
            var result=PrefabUtility.SaveAsPrefabAsset(go,PrefabPath);Object.DestroyImmediate(go);return result;
        }
        // One hidden batch compile/asset build; no window activation or multi-peer Play.
        public static void BuildBatch()
        {UnityEditor.SceneManagement.EditorSceneManager.OpenScene(FrontendSceneBuilder.ScenePath);MultiplayerSceneBuilder.Build();}
    }
}
