using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Wildfolk.Editor.WorldArt;
namespace Wildfolk.Editor
{
    internal static class WorldContentBuilder
    {
        internal static void Build(GameplayAssets a,WorldPickup pickup)
        {
            var tree=Resource("Tree","tree_oak",a.Tree,new Vector2(0,.82f),.66f,3,HitReaction.Tree,pickup,a.Settings);
            var stone=Resource("Stone","rock_wide",a.Stone,Vector2.zero,.72f,1.16f,HitReaction.Stone,pickup,a.Settings);
            var ore=Resource("IronOreNode","iron_ore_node",a.Progression.Ore,Vector2.zero,.72f,1.16f,HitReaction.Stone,pickup,a.Settings);
            var log=Resource("FallenLog","fallen_log",a.World.Log,Vector2.zero,.75f,.9f,HitReaction.Tree,pickup,a.Settings,new Vector2(2.5f,.9f));
            if(a.World.Generation.Resources.Length==0)a.World.Generation.Resources=new[]{Rule(tree,140,4,8,3.5f,4,.85f,0),Rule(stone,55,3,9,3.5f,3.4f,0,.85f),Rule(ore,16,0,0,18,4,0,.9f),Rule(log,12,3,8,3.5f,3.2f,.4f,0)};
            a.World.Generation.Resources[0].AlternateSprite=VisualArtImporter.Sprite("tree_olive");a.World.Generation.Resources[1].AlternateSprite=VisualArtImporter.Sprite("rock_split");
            EditorUtility.SetDirty(a.World.Generation);
            Animal("Chicken","chicken",a.Progression.Chicken,a.World.ChickenLoot,a,pickup);
            Animal("Boar","boar",a.Survival.Boar,a.World.BoarLoot,a,pickup);
            Goblin(a,pickup);
        }
        private static ResourceGenerationRule Rule(ResourceNode prefab,int count,int start,float radius,float clearance,float spacing,float forest,float rock)=>new ResourceGenerationRule{Prefab=prefab,Count=count,StartCount=start,StartRadius=radius,SpawnClearance=clearance,Spacing=spacing,ForestPreference=forest,RockPreference=rock,ScaleRange=new Vector2(.9f,1.15f)};
        private static ResourceNode Resource(string name,string art,ResourceDefinition definition,Vector2 offset,float radius,float barHeight,HitReaction reaction,WorldPickup pickup,GameplaySettings settings,Vector2 box=default)
        {
            var root=Group(name,null);Shadow(root,3,.7f);
            var group=root.gameObject.AddComponent<SortingGroup>();root.gameObject.AddComponent<DepthSort>().Visual=group;
            var visual=Sprite("Resource",art,root,offset,1,0);
            if(box.sqrMagnitude>0)root.gameObject.AddComponent<BoxCollider2D>().size=box;else root.gameObject.AddComponent<CircleCollider2D>().radius=radius;
            var health=root.gameObject.AddComponent<Health>();health.Definition=definition.Vitality;
            var node=root.gameObject.AddComponent<ResourceNode>();node.Definition=definition;node.Health=health;node.PickupPrefab=pickup;node.Settings=settings;node.Visual=visual;
            Feedback(health,visual.transform,reaction);HealthBar(health,barHeight,false);
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/"+name+".prefab");Object.DestroyImmediate(root.gameObject);return prefab.GetComponent<ResourceNode>();
        }
        private static void Animal(string name,string art,VitalityDefinition vitality,LootTable table,GameplayAssets a,WorldPickup pickup)
        {
            var root=Group(name,null);Shadow(root,1.4f,.48f);var pose=Group("Visual",root);var group=pose.gameObject.AddComponent<SortingGroup>();root.gameObject.AddComponent<DepthSort>().Visual=group;
            Sprite("Body",art,pose,Vector2.zero,.85f,0);Body(root,.57f);
            var health=root.gameObject.AddComponent<Health>();health.Definition=vitality;
            var animal=root.gameObject.AddComponent<NeutralAnimal>();animal.Definition=a.Progression.Animal;animal.Health=health;animal.Visual=pose;
            Loot(root.gameObject,health,table,a,pickup);Feedback(health,pose,HitReaction.Creature);HealthBar(health,1,false);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/"+name+".prefab");Object.DestroyImmediate(root.gameObject);
        }
        private static void Body(Transform root,float radius)
        {var body=root.gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;body.constraints=RigidbodyConstraints2D.FreezeRotation;body.interpolation=RigidbodyInterpolation2D.Interpolate;var collider=root.gameObject.AddComponent<CircleCollider2D>();collider.radius=radius;collider.isTrigger=true;}
        private static void Goblin(GameplayAssets a,WorldPickup pickup)
        {
            var root=Group("Goblin",null);Shadow(root,1.7f,.55f);Body(root,.53f);
            var health=root.gameObject.AddComponent<Health>();health.Definition=GameplayAssets.Asset<VitalityDefinition>("GoblinVitality",v=>v.MaximumHealth=50);
            var visual=Group("Visual",root);var group=visual.gameObject.AddComponent<SortingGroup>();root.gameObject.AddComponent<DepthSort>().Visual=group;
            Sprite("Body","goblin",visual,Vector2.zero,1,0);var facing=root.gameObject.AddComponent<CharacterFacing>();
            facing.LeftHand=Sprite("Left Hand","zombie_hand",visual,new Vector2(-.8f,-.42f),.72f,1).transform;facing.RightHand=Sprite("Right Hand","zombie_hand",visual,new Vector2(.8f,-.42f),.72f,1).transform;
            facing.LeftEye=Sprite("Left Eye","player_eye",visual,new Vector2(-.18f,-.28f),.8f,2).transform;facing.RightEye=Sprite("Right Eye","player_eye",visual,new Vector2(.18f,-.28f),.8f,2).transform;
            var goblin=root.gameObject.AddComponent<GoblinController>();goblin.Definition=a.World.Goblin;goblin.OwnHealth=health;goblin.Pose=visual;goblin.Facing=facing;
            Loot(root.gameObject,health,a.World.GoblinLoot,a,pickup);Feedback(health,visual,HitReaction.Creature);HealthBar(health,1.2f,false);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/Goblin.prefab");Object.DestroyImmediate(root.gameObject);
        }
        internal static void Loot(GameObject root,Health health,LootTable table,GameplayAssets a,WorldPickup pickup)
        {var loot=root.AddComponent<LootOnDeath>();loot.Health=health;loot.Table=table;loot.Pickup=pickup;loot.Settings=a.Settings;}
    }
}
