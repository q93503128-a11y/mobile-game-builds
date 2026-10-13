using UnityEditor;
using UnityEngine;
using static Wildfolk.Editor.WorldArt;
namespace Wildfolk.Editor
{
    internal static class SurvivalWorldBuilder
    {
        internal static void Build(GameplayAssets assets,Transform parent,PlayerInventory player,WorldPickup pickup)
        {
            var pond=Group("Natural Water",parent,-6,5);
            Sprite("Pond","pond",pond,Vector2.zero,1,-950);
            var collider=pond.gameObject.AddComponent<CircleCollider2D>();collider.radius=1.45f;
            pond.gameObject.AddComponent<WaterSource>().Definition=assets.Survival.Source;
            SavePrefab(pond,"WaterSource");
            var boar=Group("Boar",parent,4.3f,-4.3f);Shadow(boar,1.4f,.48f);
            var visual=Sprite("Animal","boar",boar,Vector2.zero,.85f,186);
            boar.gameObject.AddComponent<CircleCollider2D>().radius=.57f;
            var health=boar.gameObject.AddComponent<Health>();health.Definition=assets.Survival.Meat.Vitality;
            var node=boar.gameObject.AddComponent<ResourceNode>();node.Health=health;node.Definition=assets.Survival.Meat;node.PickupPrefab=pickup;node.Settings=assets.Settings;
            Feedback(health,visual.transform,HitReaction.Creature);HealthBar(health,1,false);
            var instance=SavePrefab(boar,"Boar");instance.GetComponent<ResourceNode>().Player=player;
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.GetComponent<ResourceNode>());
        }
    }
}
