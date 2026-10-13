using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Wildfolk.Editor.WorldArt;
namespace Wildfolk.Editor
{
    internal static class ProgressionWorldBuilder
    {
        internal static void Resource(Transform parent,string name,string art,Vector2 position,ResourceDefinition definition,PlayerInventory player,WorldPickup pickup,GameplaySettings settings)
        {
            var root=Group(name,parent,position.x,position.y);Shadow(root,2.1f,.62f);
            var visual=Sprite("Node",art,root,Vector2.zero,1,100-Mathf.RoundToInt(position.y*20));root.gameObject.AddComponent<CircleCollider2D>().radius=.72f;
            var health=root.gameObject.AddComponent<Health>();health.Definition=definition.Vitality;
            var node=root.gameObject.AddComponent<ResourceNode>();node.Health=health;node.Definition=definition;node.PickupPrefab=pickup;node.Settings=settings;
            Feedback(health,visual.transform,HitReaction.Stone);HealthBar(health,1.16f,false);
            var instance=SavePrefab(root,name.Replace(" ",""));instance.GetComponent<ResourceNode>().Player=player;PrefabUtility.RecordPrefabInstancePropertyModifications(instance.GetComponent<ResourceNode>());
        }
        internal static void Animal(Transform parent,string name,string art,Vector2 position,ResourceDefinition definition,GameplayAssets assets,PlayerInventory player,WorldPickup pickup,WorldSpawnRegion region,GameplaySession session)
        {
            var root=Group(name,parent,position.x,position.y);Shadow(root,1.4f,.48f);
            var pose=Group("Visual",root);var group=pose.gameObject.AddComponent<SortingGroup>();root.gameObject.AddComponent<DepthSort>().Visual=group;
            Sprite("Body",art,pose,Vector2.zero,.85f,0);
            var body=root.gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;body.constraints=RigidbodyConstraints2D.FreezeRotation;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            var collider=root.gameObject.AddComponent<CircleCollider2D>();collider.radius=.57f;collider.isTrigger=true;
            var health=root.gameObject.AddComponent<Health>();health.Definition=definition.Vitality;
            var node=root.gameObject.AddComponent<ResourceNode>();node.Health=health;node.Definition=definition;node.PickupPrefab=pickup;node.Settings=assets.Settings;
            var animal=root.gameObject.AddComponent<NeutralAnimal>();animal.Definition=assets.Progression.Animal;animal.Health=health;animal.Visual=pose;
            Feedback(health,pose,HitReaction.Creature);HealthBar(health,1,false);
            var instance=SavePrefab(root,name);var mover=instance.GetComponent<NeutralAnimal>();mover.Player=player.transform;mover.Session=session;mover.Region=region;instance.GetComponent<ResourceNode>().Player=player;
            PrefabUtility.RecordPrefabInstancePropertyModifications(mover);PrefabUtility.RecordPrefabInstancePropertyModifications(instance.GetComponent<ResourceNode>());
        }
    }
}
