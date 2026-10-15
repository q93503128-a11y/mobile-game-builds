using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Wildfolk.Editor.WorldArt;

namespace Wildfolk.Editor
{
    internal static class GameplayActorBuilder
    {
        internal static GameObject Player(Transform parent,GameplayAssets assets)
        {
            var root=Group("Player",parent,0,-.35f);
            Shadow(root,1.8f,.58f);
            var rigidbody=root.gameObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale=0;rigidbody.constraints=RigidbodyConstraints2D.FreezeRotation;
            rigidbody.interpolation=RigidbodyInterpolation2D.Interpolate;
            rigidbody.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            root.gameObject.AddComponent<CircleCollider2D>().radius=.55f;
            var health=root.gameObject.AddComponent<Health>();health.Definition=assets.PlayerVitality;
            var visual=Group("Character Visual",root);
            var group=visual.gameObject.AddComponent<SortingGroup>();
            var depth=root.gameObject.AddComponent<DepthSort>();depth.Visual=group;
            var reaction=Group("Reaction",visual);
            var motion=Group("Motion Root",reaction);
            var body=Sprite("Body","player_body",motion,Vector2.zero,1.1f,0);
            var left=Sprite("Left Hand","player_hand",motion,new Vector2(-.85f,-.38f),.82f,1);
            var right=Sprite("Right Hand","player_hand",motion,new Vector2(.85f,-.38f),.82f,1);
            var facing=root.gameObject.AddComponent<CharacterFacing>();
            facing.LeftHand=left.transform;facing.RightHand=right.transform;
            facing.LeftEye=Sprite("Left Eye","player_eye",motion,new Vector2(-.18f,-.28f),.85f,2).transform;
            facing.RightEye=Sprite("Right Eye","player_eye",motion,new Vector2(.18f,-.28f),.85f,2).transform;
            Group("Left Equipment Socket",left.transform);
            var equipmentSocket=Group("Right Equipment Socket",right.transform);
            var held=Sprite("Held Tool","axe",equipmentSocket,new Vector2(0,.55f),.72f,3);held.enabled=false;
            var layers=root.gameObject.AddComponent<CharacterVisualLayers>();layers.BaseSkin=body.transform;layers.BackSlot=Group("Back Slot",motion);layers.OptionalFX=Group("Optional FX",motion);
            var avatar=root.gameObject.AddComponent<PlayerAvatar>();
            avatar.Facing=facing;avatar.Body=motion;
            var combat=root.gameObject.AddComponent<PlayerCombat>();combat.Settings=assets.Settings;combat.Avatar=avatar;combat.OwnHealth=health;
            var motor=root.gameObject.AddComponent<PlayerMotor>();motor.Settings=assets.Settings;motor.Avatar=avatar;motor.Combat=combat;avatar.Motor=motor;
            var inventory=root.gameObject.AddComponent<PlayerInventory>();inventory.Settings=assets.Settings;
            var needs=root.gameObject.AddComponent<PlayerSurvival>();needs.Definition=assets.Survival.Needs;needs.Health=health;needs.Inventory=inventory;
            var equipment=root.gameObject.AddComponent<PlayerEquipment>();equipment.Inventory=inventory;equipment.Combat=combat;equipment.HeldVisual=held;combat.Equipment=equipment;
            var armor=Group("Armor Socket",motion).gameObject.AddComponent<SpriteRenderer>();armor.sharedMaterial=SpriteMaterial;armor.sortingOrder=1;armor.transform.localScale=Vector3.one*1.1f;armor.enabled=false;equipment.ArmorVisual=armor;layers.BodyArmor=armor.transform;
            equipment.HeadVisual=Gear("Head Gear",motion,1.1f,1);layers.HeadGear=equipment.HeadVisual.transform;
            equipment.LeftHandGear=Gear("Left Hand Gear",left.transform,1,2);equipment.RightHandGear=Gear("Right Hand Gear",right.transform,1,2);layers.LeftHandGear=equipment.LeftHandGear.transform;layers.RightHandGear=equipment.RightHandGear.transform;
            var storage=root.gameObject.AddComponent<PlayerStorage>();storage.Settings=assets.Settings;
            var crafting=root.gameObject.AddComponent<PlayerCrafting>();crafting.Inventory=inventory;crafting.Catalog=assets.Crafting.Catalog;crafting.Settings=assets.Settings;
            var placement=root.gameObject.AddComponent<PlayerPlacement>();placement.Equipment=equipment;placement.Inventory=inventory;placement.Settings=assets.Settings;placement.Facing=facing;
            var action=root.gameObject.AddComponent<PlayerPrimaryAction>();action.Equipment=equipment;action.Combat=combat;action.Placement=placement;action.Survival=needs;
            var interaction=root.gameObject.AddComponent<PlayerInteraction>();interaction.Equipment=equipment;interaction.Crafting=crafting;interaction.Settings=assets.Settings;interaction.Facing=facing;interaction.Storage=storage;
            var growth=root.gameObject.AddComponent<PlayerProgression>();growth.Definition=CharacterPassAssets.Growth();
            var stats=root.gameObject.AddComponent<CharacterStats>();stats.Progression=growth;stats.Health=health;
            var locomotion=root.gameObject.AddComponent<PlayerLocomotion>();locomotion.Definition=CharacterPassAssets.Locomotion();locomotion.Stats=stats;locomotion.Motor=motor;motor.Locomotion=locomotion;motor.Survival=needs;
            root.gameObject.AddComponent<CombatAffiliation>().Relation=EntityRelation.PlayerTeam;
            var commands=root.gameObject.AddComponent<PlayerInputCommands>();commands.Primary=action;commands.Interaction=interaction;motor.Input=commands;commands.Locomotion=locomotion;
            root.gameObject.AddComponent<PlayerInputAdapter>().Commands=commands;
            var itemDrop=root.gameObject.AddComponent<PlayerItemDrop>();itemDrop.Inventory=inventory;itemDrop.Settings=assets.Settings;
            HealthBar(health,1.35f,true,1.65f);
            Feedback(health,reaction,HitReaction.Creature);
            return SavePrefab(root,"Player");
        }

        private static SpriteRenderer Gear(string name,Transform parent,float scale,int order){var visual=Group(name,parent).gameObject.AddComponent<SpriteRenderer>();visual.sharedMaterial=SpriteMaterial;visual.sortingOrder=order;visual.transform.localScale=Vector3.one*scale;visual.enabled=false;return visual;}

        internal static void Facility(PlaceableDefinition definition,WorldPickup pickup,GameplaySettings settings)
        {
            var name=definition.Item.DisplayName;
            var root=Group(name,null);
            Shadow(root,2.1f,.62f);
            var visual=Sprite("Facility",definition.Item.Id,root,Vector2.zero,.8f,0);
            var group=root.gameObject.AddComponent<SortingGroup>();root.gameObject.AddComponent<DepthSort>().Visual=group;
            var collider=root.gameObject.AddComponent<BoxCollider2D>();collider.size=definition.Footprint;
            var health=root.gameObject.AddComponent<Health>();health.Definition=definition.Vitality;
            root.gameObject.AddComponent<CombatAffiliation>().Relation=EntityRelation.TeamStructure;
            var placeable=root.gameObject.AddComponent<WorldPlaceable>();placeable.Definition=definition;placeable.Health=health;placeable.Visual=visual;
            if(definition.Station!=StationKind.None)
            {
                var station=root.gameObject.AddComponent<CraftingStation>();station.Definition=definition;station.Health=health;
                if(definition.Station==StationKind.Campfire || definition.Station==StationKind.Furnace)
                {
                    var process=root.gameObject.AddComponent<StationProcessing>();process.Station=station;process.RecoveryPickup=pickup;process.Settings=settings;
                    var glow=Group("Firelight",root).gameObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();glow.lightType=UnityEngine.Rendering.Universal.Light2D.LightType.Point;glow.pointLightOuterRadius=5.5f;glow.pointLightInnerRadius=3;glow.color=new Color(1,.73f,.4f);glow.intensity=0;
                    root.gameObject.AddComponent<FacilityGlow>().Light=glow;root.gameObject.AddComponent<GameplayLight>().Definition=AssetDatabase.LoadAssetAtPath<GameplayLightDefinition>("Assets/Wildfolk/Data/FacilityLight.asset");
                }
            }
            if(definition.IsDoor)
            {
                var interaction=root.gameObject.AddComponent<CircleCollider2D>();interaction.isTrigger=true;interaction.radius=.25f;
                var door=root.gameObject.AddComponent<WorldDoor>();door.Placeable=placeable;door.Barrier=collider;door.Interaction=interaction;
                visual.sprite=VisualArtImporter.Sprite("door_frame");visual.transform.localScale=Vector3.one*.9f;
                door.Hinge=Group("Left Hinge",root,-.85f,0);
                var leaf=Sprite("Door Leaf","door_leaf",door.Hinge,new Vector2(.85f,0),.84f,1);
                // The frame remains fixed. Only the leaf rotates around its left edge.
                leaf.transform.localPosition=new Vector3(.85f,0,0);
            }
            else if(definition.IsBarrier)
            {
                visual.transform.localScale=new Vector3(definition.Footprint.x/2.2f,.9f,1);
                var wall=root.gameObject.AddComponent<WallVisual>();wall.Placeable=placeable;
                if(definition.Item.Id=="wood_wall"){wall.LeftEnd=Sprite("Left End","wall_post",root,new Vector2(-definition.Footprint.x*.5f,0),.85f,1).gameObject;wall.RightEnd=Sprite("Right End","wall_post",root,new Vector2(definition.Footprint.x*.5f,0),.85f,1).gameObject;}
            }
            if(definition.StorageSlots>0){var chest=root.gameObject.AddComponent<StorageChest>();chest.Placeable=placeable;chest.RecoveryPickup=pickup;chest.Settings=settings;}
            Feedback(health,visual.transform,HitReaction.Tree);HealthBar(health,1.0f,false);
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/"+name.Replace(" ","")+".prefab");
            definition.Prefab=prefab.GetComponent<WorldPlaceable>();EditorUtility.SetDirty(definition);
            Object.DestroyImmediate(root.gameObject);
        }

        internal static void PlacementPreview(PlayerPlacement placement,Transform parent,SpriteRenderer ground)
        {
            var root=Group("Placement Preview",parent);
            placement.Preview=root.gameObject;placement.Ground=ground;
            placement.PreviewFootprint=Sprite("Footprint","white",root,Vector2.zero,1,WorldSorting.Overlay-2);
            placement.PreviewVisual=Sprite("Ghost","workbench",root,Vector2.zero,.8f,WorldSorting.Overlay-1);
            root.gameObject.SetActive(false);
        }

        internal static GameObject Enemy(Transform parent,GameplayAssets assets)
        {
            var root=Group("Zombie",parent,6.6f,-.2f);
            Shadow(root,1.7f,.6f);
            var rigidbody=root.gameObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType=RigidbodyType2D.Kinematic;rigidbody.gravityScale=0;
            rigidbody.constraints=RigidbodyConstraints2D.FreezeRotation;rigidbody.interpolation=RigidbodyInterpolation2D.Interpolate;
            var collider=root.gameObject.AddComponent<CircleCollider2D>();collider.radius=.53f;collider.isTrigger=true;
            var health=root.gameObject.AddComponent<Health>();health.Definition=assets.EnemyVitality;
            var visual=Group("Visual",root);
            var group=visual.gameObject.AddComponent<SortingGroup>();
            root.gameObject.AddComponent<DepthSort>().Visual=group;
            var pose=Group("Attack Pose",visual);var reaction=Group("Reaction",pose);
            Sprite("Body","enemy",reaction,Vector2.zero,1.1f,0);
            var facing=root.gameObject.AddComponent<CharacterFacing>();facing.HandSeparation=.8f;facing.HandForward=.42f;
            facing.LeftHand=Sprite("Left Hand","zombie_hand",reaction,new Vector2(-.8f,-.42f),.78f,1).transform;
            facing.RightHand=Sprite("Right Hand","zombie_hand",reaction,new Vector2(.8f,-.42f),.78f,1).transform;
            facing.LeftEye=Sprite("Left Eye","zombie_eye",reaction,new Vector2(-.18f,-.28f),.8f,2).transform;
            facing.RightEye=Sprite("Right Eye","zombie_eye",reaction,new Vector2(.18f,-.28f),.8f,2).transform;
            var controller=root.gameObject.AddComponent<EnemyController>();controller.Settings=assets.Settings;controller.OwnHealth=health;controller.AttackPose=pose;controller.Facing=facing;
            controller.Member=root.gameObject.AddComponent<CreatureMember>();controller.Member.Definition=EcologyAssets.Species("Zombie");
            controller.Steering=root.gameObject.AddComponent<EnemySteering>();controller.Steering.Definition=StabilityAssets.Steering();root.gameObject.AddComponent<CombatAffiliation>().Relation=EntityRelation.Hostile;
            var lifecycle=root.gameObject.AddComponent<ZombieLifecycle>();lifecycle.Controller=controller;lifecycle.Visual=visual;lifecycle.Hitbox=collider;lifecycle.Definition=assets.Progression.Spawn;controller.Lifecycle=lifecycle;
            WorldContentBuilder.Loot(root.gameObject,health,assets.World.ZombieLoot,assets,pickup:AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Wildfolk/Prefabs/WorldPickup.prefab").GetComponent<WorldPickup>());
            Feedback(health,reaction,HitReaction.Creature);
            HealthBar(health,1.18f,false);
            return SavePrefab(root,"Enemy");
        }

        internal static WorldPickup Pickup()
        {
            var root=Group("World Pickup",null);
            Shadow(root,.63f,.23f);
            var floating=Group("Floating Visual",root,0,.08f);
            var icon=Sprite("Item","icon_wood",floating,Vector2.zero,.55f,150);
            var pickup=root.gameObject.AddComponent<WorldPickup>();pickup.Icon=icon;pickup.FloatingVisual=floating;icon.sortingOrder=WorldSorting.Drop;
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/WorldPickup.prefab");
            Object.DestroyImmediate(root.gameObject);
            return prefab.GetComponent<WorldPickup>();
        }
    }
}
