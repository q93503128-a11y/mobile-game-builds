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
            var facing=Group("Facing",root);
            var group=facing.gameObject.AddComponent<SortingGroup>();
            var depth=root.gameObject.AddComponent<DepthSort>();depth.Visual=group;
            var reaction=Group("Reaction",facing);
            var body=Sprite("Body","player_body",reaction,Vector2.zero,1.1f,0);
            var left=Sprite("Left Hand","player_hand",reaction,new Vector2(-.91f,-.4f),.94f,1);
            var right=Sprite("Right Hand","player_hand",reaction,new Vector2(.91f,-.4f),.94f,1);
            Group("Left Equipment Socket",left.transform);
            Group("Right Equipment Socket",right.transform);
            Group("Headwear Socket",reaction,0,.48f);
            var avatar=root.gameObject.AddComponent<PlayerAvatar>();
            avatar.FacingRoot=facing;avatar.Body=body.transform;avatar.LeftHand=left.transform;avatar.RightHand=right.transform;
            var combat=root.gameObject.AddComponent<FistCombat>();combat.Settings=assets.Settings;combat.Avatar=avatar;combat.OwnHealth=health;
            var motor=root.gameObject.AddComponent<PlayerMotor>();motor.Settings=assets.Settings;motor.Avatar=avatar;motor.Combat=combat;avatar.Motor=motor;
            root.gameObject.AddComponent<PlayerInventory>().Settings=assets.Settings;
            HealthBar(health,1.35f,true,1.65f);
            Feedback(health,reaction,HitReaction.Creature);
            return SavePrefab(root,"Player");
        }

        internal static GameObject Enemy(Transform parent,GameplayAssets assets)
        {
            var root=Group("Feral Shade",parent,6.6f,-.2f);
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
            Sprite("Body","enemy",reaction,Vector2.zero,.9f,0);
            var controller=root.gameObject.AddComponent<EnemyController>();controller.Settings=assets.Settings;controller.OwnHealth=health;controller.AttackPose=pose;
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
            var pickup=root.gameObject.AddComponent<WorldPickup>();pickup.Icon=icon;pickup.FloatingVisual=floating;
            var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Wildfolk/Prefabs/WorldPickup.prefab");
            Object.DestroyImmediate(root.gameObject);
            return prefab.GetComponent<WorldPickup>();
        }
    }
}
