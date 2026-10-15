using UnityEditor;
using UnityEngine;
using static Wildfolk.Editor.WorldArt;
namespace Wildfolk.Editor
{
    internal static class EcologyContentBuilder
    {
        internal static void RangedGoblin(GameplayAssets a,WorldPickup pickup)
        {
            var pebble=Group("Pebble",null);var image=Sprite("Stone","pebble",pebble,Vector2.zero,.6f,WorldSorting.Drop);var projectile=pebble.gameObject.AddComponent<StoneProjectile>();
            var projectilePrefab=PrefabUtility.SaveAsPrefabAsset(pebble.gameObject,"Assets/Wildfolk/Prefabs/Pebble.prefab").GetComponent<StoneProjectile>();Object.DestroyImmediate(pebble.gameObject);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Goblin.prefab");var root=Object.Instantiate(source);root.name="Slingshot Goblin";
            var goblin=root.GetComponent<GoblinController>();goblin.Member.Definition=EcologyAssets.Species("SlingshotGoblin");
            var attack=root.AddComponent<SlingshotAttack>();goblin.Ranged=attack;attack.Definition=EcologyAssets.Data<SlingshotDefinition>("SlingshotAttack");attack.Facing=goblin.Facing;attack.Pose=goblin.Pose;attack.Owner=goblin.OwnHealth;attack.Projectile=projectilePrefab;
            attack.Frame=Sprite("Slingshot Frame","slingshot",goblin.Facing.RightHand,new Vector2(0,.2f),.65f,3).transform;
            attack.PouchStone=Sprite("Loaded Pebble","pebble",goblin.Pose,Vector2.zero,.45f,4);attack.PouchStone.enabled=false;
            attack.Band=Group("Band",root.transform).gameObject.AddComponent<LineRenderer>();attack.Band.useWorldSpace=true;attack.Band.positionCount=3;attack.Band.widthMultiplier=.035f;attack.Band.numCapVertices=2;attack.Band.sharedMaterial=SpriteMaterial;attack.Band.startColor=attack.Band.endColor=new Color32(189,174,127,255);attack.Band.sortingOrder=4;attack.Band.enabled=false;
            // Band belongs to the character sorting group, rather than a separate world-depth layer.
            attack.Band.transform.SetParent(goblin.Pose,false);
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Wildfolk/Prefabs/SlingshotGoblin.prefab");Object.DestroyImmediate(root);
        }
        internal static void Skeleton()
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/Enemy.prefab");var root=Object.Instantiate(source);root.name="Skeleton";var enemy=root.GetComponent<EnemyController>();
            enemy.Member.Definition=EcologyAssets.Species("Skeleton");enemy.Combat=EcologyAssets.Data<CreatureCombatDefinition>("SkeletonCombat");enemy.OwnHealth.Definition=EcologyAssets.Data<VitalityDefinition>("SkeletonVitality");
            foreach(var renderer in enemy.AttackPose.GetComponentsInChildren<SpriteRenderer>())
            {if(renderer.name=="Body")renderer.sprite=VisualArtImporter.Sprite("skeleton");if(renderer.name.Contains("Hand")){renderer.sprite=VisualArtImporter.Sprite("bone_hand");renderer.transform.localScale=Vector3.one*.65f;}if(renderer.name.Contains("Eye"))renderer.enabled=false;}
            enemy.Facing.HandSeparation=.65f;enemy.Facing.HandForward=.28f;
            var loot=root.GetComponent<LootOnDeath>();loot.Table=EcologyAssets.Data<LootTable>("SkeletonLoot");
            var variant=root.AddComponent<CreatureVariant>();enemy.Variant=variant;variant.Definition=EcologyAssets.Data<CreatureVariantDefinition>("RustedSwordVariant");variant.BaseLoot=loot.Table;
            variant.Weapon=Sprite("Rusted Blade","rusted_sword",enemy.Facing.RightHand,new Vector2(0,.45f),.6f,3);variant.Weapon.enabled=false;
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Wildfolk/Prefabs/Skeleton.prefab");Object.DestroyImmediate(root);
        }
        internal static void Camp()
        {
            var root=Group("Goblin Camp",null);var camp=root.gameObject.AddComponent<GoblinCamp>();camp.Definition=EcologyAssets.Data<GoblinCampDefinition>("GoblinCamp");
            var shelter=Group("Shelter",root,-2,1);Shadow(shelter,3.2f,.85f);var image=Sprite("Tent","goblin_tent",shelter,Vector2.zero,1,0);var sort=shelter.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();shelter.gameObject.AddComponent<DepthSort>().Visual=sort;shelter.gameObject.AddComponent<BoxCollider2D>().size=new Vector2(2.1f,1.2f);
            var fire=Group("Camp Fire",root,1,-1);Shadow(fire,1.5f,.4f);Sprite("Fire","campfire",fire,Vector2.zero,.7f,0);var fireSort=fire.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();fire.gameObject.AddComponent<DepthSort>().Visual=fireSort;
            var box=Group("Camp Supplies",root,2,1.2f);Shadow(box,1.1f,.35f);Sprite("Box","camp_box",box,Vector2.zero,.8f,0);var boxSort=box.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();box.gameObject.AddComponent<DepthSort>().Visual=boxSort;box.gameObject.AddComponent<BoxCollider2D>().size=new Vector2(.8f,.55f);
            camp.Definition.Prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/GoblinCamp.prefab").GetComponent<GoblinCamp>();EditorUtility.SetDirty(camp.Definition);Object.DestroyImmediate(root.gameObject);
        }
    }
}
