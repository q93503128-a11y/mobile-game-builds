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
        }
    }
}
