using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Wildfolk.Editor
{
    public static class DeathContentAssets
    {
        internal static void ConfigureMarker(GameObject root)
        {
                if(root.GetComponent<PlayerRescueMarker>()==null)
                {
                    var marker=root.AddComponent<PlayerRescueMarker>();
                    var icon=new GameObject("Rescue Marker",typeof(SpriteRenderer));icon.transform.SetParent(root.transform,false);icon.transform.localPosition=new Vector3(0,1.6f,0);icon.transform.localScale=Vector3.one*.55f;
                    var sprite=icon.GetComponent<SpriteRenderer>();sprite.sprite=VisualArtImporter.Sprite("icon_interact");sprite.sortingOrder=WorldSorting.Overlay;sprite.color=new Color32(241,198,105,255);marker.View=icon;icon.SetActive(false);
                }
        }
        public static void Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Stop Play first.");
            var rules=AssetDatabase.LoadAssetAtPath<ReviveDefinition>("Assets/Wildfolk/Data/Revive.asset");
            foreach(var path in new[]{"Assets/Wildfolk/Prefabs/Player.prefab","Assets/Wildfolk/Prefabs/NetworkPlayer.prefab"})
            {
                var root=PrefabUtility.LoadPrefabContents(path);var death=root.GetComponent<PlayerDeath>()??root.AddComponent<PlayerDeath>();death.Definition=rules;
                ConfigureMarker(root);
                PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
            }
            foreach(var path in new[]{"Assets/Wildfolk/Scenes/Wildfolk.unity","Assets/Wildfolk/Scenes/Multiplayer.unity"})
            {
                var scene=EditorSceneManager.OpenScene(path);var existing=Object.FindFirstObjectByType<DeathHud>();if(existing!=null){Object.DestroyImmediate(existing.DeadView);Object.DestroyImmediate(existing.DownView);Object.DestroyImmediate(existing);}var arena=Object.FindFirstObjectByType<MultiplayerArena>();
                if(arena==null)
                {
                    var hud=Object.FindFirstObjectByType<GameplayHud>();var root=hud.gameObject;var safe=root.transform.Find("Safe Area");var modal=safe.Find("Modal Layer");
                    // Remove the obsolete NewGame-on-death path from the authored scene.
                    var old=modal.Find("Defeat Overlay");if(old!=null)Object.DestroyImmediate(old.gameObject);
                    DeathUiBuilder.Build(root,modal,safe,hud.Session.PlayerHealth.GetComponent<PlayerDeath>());
                }
                else
                {
                    var safe=arena.transform.Find("Safe Area");if(safe==null)safe=Object.FindFirstObjectByType<Canvas>().transform.Find("Safe Area");
                    DeathUiBuilder.Build(arena.gameObject,safe,safe,null);
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Wildfolk/Scenes/MainMenu.unity");
        }
    }
}
